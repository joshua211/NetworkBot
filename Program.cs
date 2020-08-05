using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static NetworkBot.NetworkListener;

namespace NetworkBot
{
    class Program
    {
        private string token;
        private ulong channelId;

        private DiscordSocketClient _client;
        private CommandService _commands;
        private NetworkListener _listener;

        private IMessageChannel channel;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            await InitConfig();
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _listener = new NetworkListener();

            _listener.NewDhcpConnection += HandleDhcpConnection;
            _client.Log += Log;
            _client.Ready += () =>
            {
                var c = _client.GetChannel(channelId);
                channel = (IMessageChannel)_client.GetChannel(channelId);
                _listener.Start();
                return Task.CompletedTask;
            };
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            var commandHandler = new CommandHandler(BuildServiceProvider(), _client, _commands);
            await commandHandler.InstallCommandsAsync();

            await Task.Delay(-1);
        }

        public async void HandleDhcpConnection(DhcpEventArgs args) => await channel.SendMessageAsync($"{args.Device.Name} is now at home!");



        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_listener)
            .BuildServiceProvider();


        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task InitConfig()
        {
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync("./config.json"));
            token = config["token"];
            channelId = Convert.ToUInt64(config["channelId"]);
        }

        public static void Log(string msg, Exception e = null, LogSeverity level = LogSeverity.Debug)
                    => Log(new LogMessage(level, "Code", msg, e));
    }
}
