using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace DBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;

       public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
        {
            _client = client;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        public async Task InitCommandsAsync()
        {
           
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            //Check if message is not system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int prefixEndIndex = 0;

            //Check if command has prefix and is not send by bot
            if (!(message.HasCharPrefix('`', ref prefixEndIndex) && !message.HasMentionPrefix(_client.CurrentUser, ref prefixEndIndex)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, prefixEndIndex, _serviceProvider);

            //Sends error message when command is not properly executed
            if(!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            await messageParam.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
