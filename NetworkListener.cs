using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace NetworkBot
{
    public class NetworkListener
    {
        public static int DhcpTimeout = 30;

        public event Action<DhcpEventArgs> NewDhcpConnection;
        public UdpClient Host { get; private set; }
        public List<DhcpDevice> Devices { get; private set; }
        private List<string> IngoreNames;
        public bool IsListening { get; private set; }
        private Thread ListenerThread;
        private IPEndPoint EndPoint;


        public NetworkListener()
        {
            EndPoint = new IPEndPoint(IPAddress.Any, 0);
            IsListening = false;
            Host = new UdpClient();
            Devices = new List<DhcpDevice>();
            Host.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ListenerThread = new Thread(new ThreadStart(Listen));
            Host.Client.Bind(new IPEndPoint(IPAddress.Any, 67));

            IngoreNames = File.Exists("./ignore.json")
                ? JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("./ignore.json"))
                : new List<string>();
        }

        public void Start()
        {
            IsListening = true;
            ListenerThread.Start();
        }

        public void Stop() => IsListening = false;

        private void Listen()
        {
            while (IsListening)
            {
                if (Host.Available > 0)
                {
                    //12 is the statuscode for the name and is 14 bytes long, with 2 weird bytes at the start
                    byte[] Data = Host.Receive(ref EndPoint);
                    Data = Data.Skip(236)
                                .SkipWhile(b => b != 12)
                                .Skip(2).Take(12)
                                .ToArray();
                    string name = System.Text.Encoding.UTF8.GetString(Data)
                                    .Split('\n')[0]
                                    .SkipLast(1)
                                    .AsString();
                    if (IngoreNames.Exists(s => s == name))
                        continue;

                    var existingDevice = Devices.FirstOrDefault(d => d.Name == name);
                    if (existingDevice == null || existingDevice.LastRequest.OlderThan(DhcpTimeout))
                    {
                        if (existingDevice == null)
                        {
                            existingDevice = new DhcpDevice() { Name = name, LastRequest = DateTime.Now };
                            Devices.Add(existingDevice);
                        }
                        NewDhcpConnection?.Invoke(new DhcpEventArgs(this, existingDevice));
                    }
                    else
                        existingDevice.LastRequest = DateTime.Now;

                }
                else
                    Thread.Sleep(100);
            }
        }

        public class DhcpEventArgs
        {
            public DhcpEventArgs(object sender, DhcpDevice device)
            {
                this.Sender = sender;
                this.Device = device;

            }
            public object Sender { get; set; }
            public DhcpDevice Device { get; set; }


        }
    }
}