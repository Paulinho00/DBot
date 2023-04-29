using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;

namespace DBot.CommandNextModules;

public class BasicMessageCommandsModule : BaseCommandModule
{
    /// <summary>
    /// `pingpong Bob -> ping ten times @Bob
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [Command("pingpong")]
    [Description("pinguje dziesięć razy wybraną osobę")]
    public async Task PingUserTenTimesAsync(CommandContext ctx, [RemainingText][Description("użytkownik")] DiscordMember? user = null)
    {
        if (user == null) await ctx.Channel.SendMessageAsync("Nie ma takiego użytkownika");
        else
        {
            List<Task> pingTasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                pingTasks.Add(ctx.Channel.SendMessageAsync($"<@{user.Id}>"));
            }
            await Task.WhenAll(pingTasks);
        }
    }
    
    /// <summary>
    /// `draw wt wot ws -> Gra: wt or Gra: wot or Gra: ws
    /// </summary>
    /// <param name="gamesName"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [Command("draw")]
    [Description("losuje gre")]
    public async Task DrawGameAsync(CommandContext ctx, [RemainingText][Description("lista gier")] params string[] gamesName)
    {
        Random random = new Random();
        int indexOfGame = random.Next(gamesName.Length);
        await ctx.RespondAsync($"Gra: {gamesName[indexOfGame]}");
    }
}