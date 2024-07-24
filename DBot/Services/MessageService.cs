using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DBot.Services;

public class MessageService : IMessageService
{
    public async Task SendMessageAsync(InteractionContext ctx, string message)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = message
            });
    }
}