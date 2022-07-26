using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DBot.Modules
{
    public class BasicCommandsModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _commandService;


        public BasicCommandsModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        /// <summary>
        /// `eee hello -> hello
        /// </summary>
        /// <param name="echo"></param>
        /// <returns></returns>
        [Command("eee")]
        [Summary("echo")]
        public Task EchoAsync([Remainder] string echo)
        {
            return ReplyAsync(echo);
        }

        /// <summary>
        /// `userinfo Bob -> Bob#6681, Created: 09.12.2018 17:19:04 +00:00, Status: Online
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Command("userinfo")]
        [Summary("zwraca informacje o wybranej osobie")]
        public async Task UserInfoAsync([Remainder] SocketUser user = null)
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
        [Summary("pinguje dziesięć razy wybraną osobę")]
        public async Task PingTenTimesUser([Remainder] SocketUser user = null)
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
        [Command("draw")]
        [Summary("losuje gre")]
        public async Task DrawGame([Remainder][Summary("lista gier")] string gamesName)
        {
            string[] games = gamesName.Split(" ");
            Random random = new Random();
            int indexOfGame = random.Next(games.Length);
            await ReplyAsync($"Gra: {games[indexOfGame]}");
        }


        /// <summary>
        /// commands -> display all commands with parameter, aliases and desc
        /// </summary>
        /// <returns></returns>
        [Command("help")]
        [Summary("lista komend i możliwych dźwieków")]
        [Alias("h")]
        public async Task DisplayAllCommands()
        {
            List<CommandInfo> commandInfos = _commandService.Commands.ToList();
            var embed = new EmbedBuilder();
            embed.WithTitle("Komendy");
            embed.WithColor(Color.Red);
            foreach (CommandInfo command in commandInfos) {

                //Field title with command name and parameters
                StringBuilder commandTitle = new StringBuilder(command.Name);

                for (int i = 1; i < command.Aliases.Count; i++)
                {
                    commandTitle.Append(" / " + command.Aliases[i].ToString());
                }

                foreach (var parameter in command.Parameters) {
                    if (parameter.IsOptional)
                    {
                        commandTitle.Append($" `[{parameter.Name}]`");
                    }
                    else
                    {
                        commandTitle.Append($" `{parameter.Name}`");
                    }
                   
                }

                //Field desc with aliases and description
                StringBuilder commandDesc = new StringBuilder();
                commandDesc.AppendLine("\n" + command.Summary ?? "Nie ma opisu");
                embed.AddField(commandTitle.ToString() , commandDesc.ToString());
            }

            //Creates list of all possible sounds from local files
            var filesName = Directory.GetFiles(@"C:\Users\Paweł\source\repos\DBot\DBot\Resources\").Select(f => Path.GetFileName(f));
            StringBuilder filesNameFormated = new StringBuilder();
            foreach (string filename in filesName)
            {
                var formatedFilename = filename.Substring(0, filename.Length - 4);
                filesNameFormated.Append("`" + formatedFilename + "` ");
            }
            //Field with all possible sounds from local files
            embed.AddField("Dźwięki", filesNameFormated.ToString());

            await ReplyAsync(embed: embed.Build());
        }
    }
}
