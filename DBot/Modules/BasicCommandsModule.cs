using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBot.Services;
using Discord.Commands;
using Discord.WebSocket;

namespace DBot.Modules
{
    public class BasicCommandsModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// `eee hello -> hello
        /// </summary>
        /// <param name="echo"></param>
        /// <returns></returns>
        [Command("eee")]
        [Summary("Echoes a message.")]
        public Task EchoAsync([Remainder][Summary("The text to echo")] string echo)
        {
            return ReplyAsync(echo);
        }

        /// <summary>
        /// `userinfo Bob -> Bob#6681, Created: 09.12.2018 17:19:04 +00:00, Status: Online
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Command("userinfo")]
        [Summary("Return a information about chosen user.")]
        public async Task UserInfoAsync([Remainder][Summary("User to get info from")] SocketUser user = null)
        {
            if (user == null) await ReplyAsync("Nie ma takiego użytkownika");
            else await ReplyAsync($"{user.Username}#{user.Discriminator}, Created: {user.CreatedAt}, Status: {user.Status}");
        }

        /// <summary>
        /// `pingpong Bob -> ping ten times @Bob
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Command("pingpong", RunMode = RunMode.Async)]
        [Summary("Pings 10 time chosen user.")]
        public async Task PingTenTimesUser([Remainder][Summary("User to be pinged")] SocketUser user = null)
        {
            if (user == null) await ReplyAsync("Nie ma takiego użytkownika");
            else
            {
                List<Task> pingTasks = new List<Task>();
                for (int i = 0; i < 10; i++)
                {
                    pingTasks.Add(ReplyAsync($"<@{user.Id}>"));
                }
                await Task.WhenAll(pingTasks);
            }
        }

        /// <summary>
        /// `gameDraw wt wot ws -> Gra: wt or Gra: wot or Gra: ws
        /// </summary>
        /// <param name="gamesName"></param>
        /// <returns></returns>
        public async Task drawGame([Remainder][Summary("List of games name")] string gamesName)
        {
            string[] games = gamesName.Split(" ");
            Random random = new Random();
            int indexOfGame = random.Next(games.Length);
            await ReplyAsync($"Gra: {games[indexOfGame]}");
        }
    }
}
