
using DBot.Entities;
using DBot.Services;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace DBot.CommandNextModules;

public class AudioCommandsModule(IAudioService audioService) : BaseCommandModule
{
    private LavalinkGuildPlayer? _player;
    private LavalinkSession? _session;

    [Command("join")]
    [Aliases("j")]
    [Description("Dołącza bota do kanału głosowego")]
    public async Task Join(CommandContext ctx)
    {
        await ConnectToVoiceChannel(ctx);
    }

    /// <summary>
    /// Leaves voice channel
    /// </summary>
    /// <param name="ctx"></param>
    [Command("leave")]
    [Aliases("l")]
    [Description("opuszcza kanał")]
    public async Task Leave(CommandContext ctx)
    {
        if (_session == null || !_session.IsConnected)
        {
            await ctx.RespondAsync("Nie ma połączenia do Lavalink");
            return;
        }

        if (_player == null)
        {
            await ctx.RespondAsync("Nie jestem podłączony do kanału");
            return;
        }

        await _player.DisconnectAsync();
        _player = null;
    }

    /// <summary>
    /// Displays names of all local sounds
    /// </summary>
    /// <param name="ctx"></param>
    [Command("sounds")]
    [Aliases("s")]
    [Description("wyświetla wszystkie możliwe dźwięki")]
    public async Task DisplayLocalFileSounds(CommandContext ctx)
    {
        Dictionary<string, string> fieldsValues = audioService.GetAllSoundsFromLocalFiles();
        var embed = new DiscordEmbedBuilder();
        embed.Color = DiscordColor.IndianRed;

        //Add each category and its sounds to fields 
        foreach (string category in fieldsValues.Keys)
        {
            var field = new DiscordEmbedField(category, fieldsValues[category]);
            embed.AddField(field);
        }

        await ctx.Channel.SendMessageAsync(embed: embed.Build());
    }

    /// <summary>
    /// Plays track searched by given string
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="soundName">query to search song</param>
    [Command("play")]
    [Aliases("p")]
    [Description("puszcza coś z internetu")]
    public async Task PlaySoundFromInternet(CommandContext ctx,
        [RemainingText] [Description("nazwa piosenki")]
        string soundName)
    {
        if (ctx.Member.VoiceState == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }

        if (_session == null)
        {
            if (!await ConnectToVoiceChannel(ctx))
                return;
        }
        
        //Checks if search query is a URI
        LavalinkTrackLoadingResult loadResult;
        if (soundName.Contains("https"))
        {
            loadResult = await _player!.LoadTracksAsync(LavalinkSearchType.Plain, soundName);
        }
        else
        {
            loadResult = await _player!.LoadTracksAsync(LavalinkSearchType.Youtube, soundName);
        }

        if (loadResult.LoadType == LavalinkLoadResultType.Empty
            || loadResult.LoadType == LavalinkLoadResultType.Error)
        {
            await ctx.RespondAsync("Nie ma nic takiego");
            return;
        }

        //Checks if loaded result is a playlist, if yes then it adds all songs to queue
        if (loadResult.LoadType == LavalinkLoadResultType.Playlist)
        {
            var result = (LavalinkPlaylist) loadResult.Result;
            foreach (var loadResultTrack in result.Tracks)
            {
                _player.AddToQueue(new TrackQueueEntry(ctx.Channel, loadResultTrack), loadResultTrack);
                _player.PlayQueueAsync();
            }

            await ctx.RespondAsync($"Zagrane");
        }
        else
        {
            LavalinkTrack track;
            if(loadResult.LoadType == LavalinkLoadResultType.Track)
            {
                track = (LavalinkTrack) loadResult.Result;
            }
            else
            {
                track = ((List<LavalinkTrack>) loadResult.Result).First();
            }

            _player.AddToQueue(new TrackQueueEntry(ctx.Channel, track), track);
            _player.PlayQueueAsync();
            await ctx.RespondAsync($"Dodane do kolejki: {track.Info.Title} - {track.Info.Author}");
        }
    }


    /// <summary>
    /// Plays track from local file
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="fileName"></param>
    [Command("kubica")]
    [Aliases("k")]
    [Description("gra lokalne dźwięki ")]
    public async Task PlayLocalSoundAsync(CommandContext ctx,
        [RemainingText] [Description("nazwa pliku")]
        string fileName)
    {
        if (ctx.Member.VoiceState == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }

        if (_session == null)
        {
            if (!await ConnectToVoiceChannel(ctx))
                return;
        }

        var filepath = audioService.GetFilePath(fileName);
        if (filepath == null)
        {
            await ctx.RespondAsync("Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }

        var loadResult = await _player.LoadTracksAsync(LavalinkSearchType.Plain, filepath.FullName);

        if (loadResult.LoadType == LavalinkLoadResultType.Empty
            || loadResult.LoadType == LavalinkLoadResultType.Error)
        {
            await ctx.RespondAsync("Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }
        
        await _player.PlayAsync(filepath.FullName);
        if (_player.QueueEntries.Count != 0)
        {
            _player.PlayQueueAsync();
        }

        await ctx.RespondAsync($"Zagrane");
    }
    
    [Command("pause")]
    [Description("pauzuje odtwarzacz")]
    public async Task Pause(CommandContext ctx)
    {
        if (_session == null)
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam pauzować?");
            return;
        }

        await _player.PauseAsync();
    }

    [Command("Resume")]
    [Aliases("r")]
    [Description("wznawia granie")]
    public async Task ResumeAsync(CommandContext ctx)
    {
        if (_session == null)
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam wznawiać?");
            return;
        }
        await _player.ResumeAsync();
    }
    
    [Command("stop")]
    [Description("pomija obecny utwór i zatrzymuje odtwarzacz")]
    public async Task StopAsync(CommandContext ctx) 
    { 
        if (_session == null)
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam Stopować?");
            return;
        }
        
        await _player.StopAsync();

    }
    
    [Command("skip")]
    [Description("pomija obecny utwór")]
    public async Task SkipAsync(CommandContext ctx)
    {
        if (_session == null)
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.QueueEntries.Count == 0)
        {
            await ctx.RespondAsync("Ale co ja mam Stopować?");
            return;
        }

        //_shouldPauseAfterStop = true;
        var firstTrack = _player.QueueEntries[0];
        _player.RemoveFromQueue(firstTrack);
        await ctx.RespondAsync($"Pominięto {firstTrack.Track.Info.Title} - {firstTrack.Track.Info.Author}");
    }

    [Command("clear")]
    [Description("czyści kolejkę utworów")]
    public async Task ClearQueue(CommandContext ctx)
    {
        if (_player == null)
        {
            await ctx.RespondAsync("Nic nie ma w kolejce");
            return;
        }

        foreach (var queueEntry in _player.QueueEntries)
        {
            _player.RemoveFromQueue(queueEntry);
        }
        await ctx.RespondAsync("Wyczyszono kolejkę");
    }
    
    /// <summary>
    /// Tries to connect to voice channel
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns>true if it connected, false otherwise</returns>
    private async Task<bool> ConnectToVoiceChannel(CommandContext ctx)
    {
        var lava = ctx.Client.GetLavalink();

        if (!lava.ConnectedSessions.Any())
        { 
            await ctx.RespondAsync( "Nie ma połączenia z Lavalink");
            return false;
        }

        var lavalinkConfig = new LavalinkConfiguration();
        lavalinkConfig.DefaultVolume = 100;
        lavalinkConfig.EnableBuiltInQueueSystem = true;


        _session = await lava.ConnectAsync(lavalinkConfig);
        if (ctx.Member.VoiceState == null ||  ctx.Member.VoiceState.Channel!.Type != ChannelType.Voice)
        {
            await ctx.RespondAsync("Musisz być na kanale głosowym");
            return false;
        }
        
        _player = await _session.ConnectAsync(ctx.Member.VoiceState.Channel);
        await ctx.RespondAsync($"Dołączyłem do {ctx.Member.VoiceState.Channel.Name}");
        return true;
    }
    
}