
using DBot.CommandNextModules;
using DBot.Services;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using Microsoft.Extensions.Configuration;
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
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var client = new DiscordClient(new DiscordConfiguration()
            {
                Token = config.GetRequiredSection("token").Get<string>(),
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
                MinimumLogLevel = LogLevel.Debug
            });

            var services = new ServiceCollection()
                    .AddSingleton<IAudioService, AudioService>()
                    .BuildServiceProvider();

            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new List<string>() { "`" },
                ServiceProvider = services
            });

            var appCommands = client.UseApplicationCommands(new ApplicationCommandsConfiguration()
            {
                ServiceProvider = services
            });

            var guildId = config.GetRequiredSection("guildId").Get<ulong>();
            appCommands.RegisterGuildCommands<AudioCommandsModule>(guildId);
            
            commands.RegisterCommands<BasicMessageCommandsModule>();

            var lavalinkConfig = GetLavalinkConfiguration();
            
            var lavalink = client.UseLavalink();
            
            await client.ConnectAsync();
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
                SocketEndpoint = endpoint,
                EnableBuiltInQueueSystem = true
            };

            return lavalinkConfig;
        }
    }
}
