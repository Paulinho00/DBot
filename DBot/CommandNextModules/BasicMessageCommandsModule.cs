using DBot.Services;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;

namespace DBot.CommandNextModules;

public class BasicMessageCommandsModule(IMessageService _messageService) : ApplicationCommandsModule
{
    /// <summary>
    /// `pingpong Bob -> ping ten times @Bob
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [SlashCommand("pingpong", "pinguje dziesięć razy wybraną osobę")]
    public async Task PingUserTenTimesAsync(InteractionContext ctx, [Option("nazwaUzytkownika", "Nazwa użytkownika do spingowania")] DiscordUser? user = null)
    {
        if (user == null) await _messageService.SendMessageAsync(ctx, "Nie ma takiego użytkownika");
        else
        {
            await _messageService.SendMessageAsync(ctx, "HALO");
            for (int i = 0; i < 10; i++)
            {
                await ctx.Channel.SendMessageAsync($"<@{user.Id}>");
            }
        }
    }
    
    /// <summary>
    /// `draw wt wot ws -> Gra: wt or Gra: wot or Gra: ws
    /// </summary>
    /// <param name="gamesName"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [SlashCommand("draw", "losuje gre")]
    public async Task DrawGameAsync(InteractionContext ctx, [Option("listGier","lista gier, oddzielona spacjami")] string gamesName)
    {
        Random random = new Random();
        var games = gamesName.Split(" ");
        int indexOfGame = random.Next(games.Length);
        await _messageService.SendMessageAsync(ctx, $"Gra: {games[indexOfGame]}");
    }
}