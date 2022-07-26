﻿using System;
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
        public async Task PingTenTimesUserAsync([Remainder] SocketUser user = null)
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
        public async Task DrawGameAsync([Remainder][Summary("lista gier")] string gamesName)
        {
            string[] games = gamesName.Split(" ");
            Random random = new Random();
            int indexOfGame = random.Next(games.Length);
            await ReplyAsync($"Gra: {games[indexOfGame]}");
        }


        /// <summary>
        /// all -> display all commands with parameter, aliases and desc
        /// </summary>
        /// <returns></returns>
        [Command("all")]
        [Summary("lista komend i możliwych dźwieków")]
        public async Task DisplayAllCommandsAsync()
        {
            List<CommandInfo> commandInfos = _commandService.Commands.ToList();
            var embed = new EmbedBuilder();
            embed.WithTitle("Komendy");
            embed.WithColor(Color.Red);

            foreach (CommandInfo command in commandInfos) {
                //Field title with command name, aliases and parameters
                var commandTitle = GetCommandNameAliasesAndParameters(command);

                //Field desc with description
                StringBuilder commandDesc = new StringBuilder();
                commandDesc.AppendLine("\n" + command.Summary ?? "Nie ma opisu");
                embed.AddField(commandTitle, commandDesc.ToString());
            }

            //Field with all possible sounds from local files
            embed.AddField("Dźwięki", GetAllSoundsFromLocalFiles());

            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Display details of given command
        /// </summary>
        /// <param name="command">displays details of this command</param>
        /// <returns></returns>
        [Command("help")]
        [Alias("h")]
        [Summary("Pokazuje szczegóły konkretnej komendy")]
        public async Task DisplayCommandDetailsAsync([Remainder] string command)
        {
            CommandInfo? commandInfo = _commandService.Commands.FirstOrDefault(c => c.Name == command);

            if (commandInfo == null)
            {
                await ReplyAsync("Nie ma takiej komendy");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Red);

            //Embed's field title with command name, aliases and parameters
            string title = GetCommandNameAliasesAndParameters(commandInfo);

            //Embed's field title with description
            embed.AddField(title, "\n" + commandInfo.Summary ?? "Nie ma opisu");
            await ReplyAsync(embed: embed.Build());


        }

        /// <summary>
        /// Displays all possible sounds from local files
        /// </summary>
        /// <returns></returns>
        [Command("sounds")]
        [Alias("s")]
        [Summary("wyświetla wszystkie możliwe dźwięki")]
        public async Task DisplayLocalFilesSounds()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Red);

            //Field with all possible sounds from local files
            embed.AddField("Dźwięki", GetAllSoundsFromLocalFiles());

            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Returns formated string with command name, aliases and parameters
        /// </summary>
        /// <param name="command">commandInfo for string</param>
        /// <returns>formated string with command name, aliases and parameters</returns>
        private string GetCommandNameAliasesAndParameters(CommandInfo command)
        {
            StringBuilder commandTitle = new StringBuilder(command.Name);

            for (int i = 1; i < command.Aliases.Count; i++)
            {
                commandTitle.Append(" / " + command.Aliases[i].ToString());
            }

            foreach (var parameter in command.Parameters)
            {
                if (parameter.IsOptional)
                {
                    commandTitle.Append($" `[{parameter.Name}]`");
                }
                else
                {
                    commandTitle.Append($" `{parameter.Name}`");
                }

            }

            return commandTitle.ToString();
        }

        /// <summary>
        /// Returns string with names of all possible sounds
        /// </summary>
        /// <returns></returns>
        private string GetAllSoundsFromLocalFiles()
        {
            //Creates list of all possible sounds from local files
            var filesName = Directory.GetFiles(@"C:\Users\Paweł\source\repos\DBot\DBot\Resources\").Select(f => Path.GetFileName(f));

            StringBuilder filesNameFormated = new StringBuilder();
            foreach (string filename in filesName)
            {
                var formatedFilename = filename.Substring(0, filename.Length - 4);
                filesNameFormated.Append("`" + formatedFilename + "` ");
            }
            return filesNameFormated.ToString();
        }
    }
}
