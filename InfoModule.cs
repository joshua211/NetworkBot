using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace NetworkBot
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private NetworkListener _listener;

        public InfoModule(NetworkListener l)
        {
            _listener = l;
        }


        [Command("home")]
        public async Task WhoIsHome()
        {
            var active = _listener.Devices.Where(d => !d.LastRequest.OlderThan(NetworkListener.DhcpTimeout));
            if (!active.Any())
            {
                await ReplyAsync("No one seems to be home!");
                return;
            }

            string s = "";
            foreach (var a in active)
                s += "- " + a.Name + "\n";

            await ReplyAsync("These devices are at home: \n" + s);
        }
    }
}