using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Enums;
using Microsoft.Extensions.Logging;

namespace DBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = File.ReadAllText("config.txt").Trim(),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.Guilds |
                          DiscordIntents.GuildMembers |
                          DiscordIntents.GuildMessageReactions |
                          DiscordIntents.GuildMessages |
                          DiscordIntents.GuildVoiceStates |
                          DiscordIntents.GuildPresences |
                          DiscordIntents.MessageContent |
                          DiscordIntents.AllUnprivileged,
                MinimumLogLevel = LogLevel.Debug
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new List<string>() { "`" }
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
