using DisCatSharp.ApplicationCommands.Context;

namespace DBot.Services;

public interface IMessageService
{
    Task SendMessageAsync(InteractionContext ctx, string message);
}