
using DBot.CommandNextModules;
using DBot.Services;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using Microsoft.Extensions.DependencyInjection;
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
                          DiscordIntents.GuildWebhooks |
                          DiscordIntents.All,
                MinimumLogLevel = LogLevel.Debug,
                ServiceProvider = new ServiceCollection()
                    .AddTransient<AudioService>()
                    .BuildServiceProvider()
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new List<string>() { "`" }
            });

            commands.RegisterCommands<AudioCommandsModule>();
            commands.RegisterCommands<BasicMessageCommandsModule>();

            var lavalinkConfig = GetLavalinkConfiguration();
            
            var lavalink = discord.UseLavalink();
            
            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }


        private static LavalinkConfiguration GetLavalinkConfiguration()
        {
            var endpoint = new ConnectionEndpoint()
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration()
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            return lavalinkConfig;
        }
    }
}
