using DBot.Services;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace DBot.CommandNextModules;

public class AudioCommandsModule : BaseCommandModule
{

    [Command("join")]
    [Aliases("j")]
    [Description("Dołącza bota do kanału głosowego")]
    public async Task Join(CommandContext ctx)
    {
        await ConnectToVoiceChannel(ctx);
    }

    [Command("leave")]
    [Aliases("l")]
    [Description("opuszcza kanał")]
    public async Task Leave(CommandContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }
        
        var node = lava.ConnectedNodes.Values.First();
        if (!node.ConnectedGuilds.Any())
        {
            await ctx.RespondAsync("Nie jestem podłączony do kanału");
            return;
        }
        
        foreach (var lavalinkGuildConnection in node.ConnectedGuilds)
        {
           await lavalinkGuildConnection.Value.DisconnectAsync();
           await ctx.RespondAsync($"Rozłączyłem się z {lavalinkGuildConnection.Value.Channel.Name}");
        }
    }

        
    [Command("sounds")]
    [Aliases("s")]
    [Description("wyświetla wszystkie możliwe dźwięki")]
    public async Task DisplayLocalFileSounds(CommandContext ctx, [RemainingText][Description("lista gier")] params string[] gamesName)
    {
        Random random = new Random();
        int indexOfGame = random.Next(gamesName.Length);
        await ctx.RespondAsync($"Gra: {gamesName[indexOfGame]}");
    }
    
    private async Task ConnectToVoiceChannel(CommandContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        { 
            await ctx.RespondAsync( "Nie ma połączenia z Lavalink");
            return;
        }
        var node = lava.ConnectedNodes.Values.First();
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel.Type != ChannelType.Voice)
        {
            await ctx.RespondAsync("Musisz być na kanale głosowym");
            return;
        }
        
        await node.ConnectAsync(ctx.Member.VoiceState.Channel);
        await ctx.RespondAsync($"Dołączyłem do {ctx.Member.VoiceState.Channel.Name}");
    }
    
    

}