using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        //~echo hello -> hello
        [Command("echo")]
        [Summary("Echoes a message.")]
        public Task EchoAsync([Remainder][Summary("The text to echo")] string echo)
        {
            return ReplyAsync(echo);
        }

        [Command("userinfo")]
        [Summary("Return a information about chosen user.")]
        public async Task UserInfoAsync([Summary("User to get info from")] SocketUser user = null)
        {
            if (user == null) await ReplyAsync("Nie ma takiego użytkownika");
            else await ReplyAsync($"{user.Username}, {user.ActiveClients.ToString}");
        }

        [Command("pingpong")]
        [Summary("Pings 10 time chosen user.")]
        public async Task PingTenTimesUser([Summary("User to be pinged")] SocketUser user = null)
        {
            if (user == null) await ReplyAsync("Nie ma takiego użytkownika");
            else
            {
                List<Task> pingTasks = new List<Task>();
                for (int i = 0; i < 10; i++)
                {
                    pingTasks.Add(ReplyAsync($"<@{user.Id}>"));
                }
                await Task.WhenAny(pingTasks);
            }
        }
    }
}
