using DBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using Microsoft.Extensions.DependencyInjection;
using DBot.Managers;

namespace DBot
{
    public class Startup
    {
        private readonly CommandHandler _commandHandler;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClientManager _clientManager;

        public Startup()
        {
            _commandService = new CommandService();
            _clientManager = new DiscordSocketClientManager();
            _serviceProvider = BuildServiceProvider();
            _commandHandler = new CommandHandler(_clientManager.SocketClient, _commandService, _serviceProvider);            
        }

        /// <summary>
        /// Async context of application
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            await _commandHandler.InitCommandsAsync();

            //Change token source dependent on your way of storing
            await _clientManager.SocketClient.LoginAsync(TokenType.Bot, File.ReadAllText("../../../config.txt").Trim());
            await _clientManager.SocketClient.StartAsync();

            //Block using task until the program is closed
            await Task.Delay(-1);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_clientManager.SocketClient)
            .AddSingleton(_commandService)
            .AddSingleton(_clientManager.LavaConfig)
            .AddSingleton(_clientManager.InstanceOfLavaNode)
            .AddSingleton<AudioService>()
            .BuildServiceProvider();
    }
}
