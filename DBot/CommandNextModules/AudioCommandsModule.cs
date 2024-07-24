
using DBot.Entities;
using DBot.Services;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.CommandsNext.Attributes;


namespace DBot.CommandNextModules;

//TODO: Add volume Command
//TODO: Add other sources option 
public class AudioCommandsModule(IAudioService audioService) : ApplicationCommandsModule
{
    [SlashCommand("join", "Dołącza bota do kanału głosowego")]
    [Aliases("j")]
    public async Task Join(InteractionContext ctx)
    {
        await audioService.ConnectToVoiceChannelAsync(ctx);
    }

    /// <summary>
    /// Leaves voice channel
    /// </summary>
    /// <param name="ctx"></param>
    [SlashCommand("leave", "opuszcza kanał")]
    public async Task Leave(InteractionContext ctx)
    {
        await audioService.LeaveAsync(ctx);
    }

    /// <summary>
    /// Displays names of all local sounds
    /// </summary>
    /// <param name="ctx"></param>
    [SlashCommand("sounds", "wyświetla wszystkie możliwe dźwięki")]
    public async Task DisplayLocalFileSounds(InteractionContext ctx)
    {
        await audioService.DisplayLocalSoundsAsync(ctx);
    }

    /// <summary>
    /// Plays track searched by given string
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="soundName">query to search song</param>
    [SlashCommand("play", "puszcza coś z internetu")]
    public async Task PlaySoundFromInternet(InteractionContext ctx,
        [Option("nazwaPiosenki", "Nazwa piosenki do zagrania")] string soundName)
    {
        await audioService.PlaySoundFromInternetAsync(ctx, soundName);
    }


    /// <summary>
    /// Plays track from local file
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="fileName"></param>
    [SlashCommand("kubica", "gra lokalne dźwięki")]
    public async Task PlayLocalSound(InteractionContext ctx, 
        [Option("NazwaDzwieku", "Nazwa dźwięku")] string fileName)
    {
        await audioService.PlayLocalSoundAsync(ctx, fileName);
    }
    
    [SlashCommand("pause", "pauzuje odtwarzacz")]
    public async Task Pause(InteractionContext ctx)
    {
        await audioService.PauseAsync(ctx);
    }

    [SlashCommand("Resume", "wznawia granie")]
    public async Task Resume(InteractionContext ctx)
    {
        await audioService.ResumeAsync(ctx);
    }
    
    [SlashCommand("stop", "pomija obecny utwór i zatrzymuje odtwarzacz")]
    public async Task Stop(InteractionContext ctx)
    {
        await audioService.StopAsync(ctx);
    }
    
    [SlashCommand("skip", "pomija obecny utwór")]
    public async Task Skip(InteractionContext ctx)
    {
        await audioService.SkipAsync(ctx);
    }

    [SlashCommand("clear", "czyści kolejkę utworów")]
    public async Task ClearQueue(InteractionContext ctx)
    {
        await audioService.ClearQueueAsync(ctx);
    }
    
}