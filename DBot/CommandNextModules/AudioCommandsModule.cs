
using DBot.Services;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.EventArgs;

namespace DBot.CommandNextModules;

public class AudioCommandsModule : BaseCommandModule
{
    private readonly AudioService _audioService;
    private readonly Queue<LavalinkTrack> _songsQueue;
    private DiscordChannel? _textChannel;
    private LavalinkGuildConnection? _connection;
    private bool _shouldPauseAfterStop;

    public AudioCommandsModule(AudioService audioService)
    {
        _audioService = audioService;
        _songsQueue = new Queue<LavalinkTrack>();
    }
    
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

        _connection = null;
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
        Dictionary<string, string> fieldsValues = _audioService.GetAllSoundsFromLocalFiles();
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
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();

        if (!node.ConnectedGuilds.Any())
        {
            if (!await ConnectToVoiceChannel(ctx))
                return;
        }

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }
        
        
        //Checks if search query is a URI
        LavalinkLoadResult loadResult;
        if (soundName.Contains("https"))
        {
            loadResult = await node.Rest.GetTracksAsync(new Uri(soundName));
        }
        else
        {
            loadResult = await node.Rest.GetTracksAsync(soundName);
        }

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
            || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.RespondAsync("Nie ma nic takiego");
            return;
        }

        //Checks if loaded result is a playlist, if yes then it adds all songs to queue
        if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
        {
            foreach (var loadResultTrack in loadResult.Tracks)
            {
                _songsQueue.Enqueue(loadResultTrack);
            }

            var track = _songsQueue.Dequeue();
            await _connection.PlayAsync(track);
            await ctx.RespondAsync($"Zagrane");
        }
        else
        {

            var track = loadResult.Tracks.First();

            if (_connection.CurrentState.CurrentTrack != null)
            {
                _songsQueue.Enqueue(track);
                await ctx.RespondAsync($"Dodane do kolejki: {track.Title}");
                return;
            }


            await _connection.PlayAsync(track);
            await ctx.RespondAsync($"Zagrane");
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
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();

        if (!node.ConnectedGuilds.Any())
        {
            if (!await ConnectToVoiceChannel(ctx))
                return;
        }
        

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }

        var filepath = _audioService.GetFilePath(fileName);
        if (filepath == null)
        {
            await ctx.RespondAsync("Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }

        var loadResult = await node.Rest.GetTracksAsync(filepath);

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
            || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.RespondAsync("Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }

        var track = loadResult.Tracks.First();
        if (_connection.CurrentState.CurrentTrack != null && !_connection.CurrentState.CurrentTrack.SourceName.Equals("local"))
        {
            await ctx.RespondAsync("Ale widzisz że gram?");
            return;
        }
        
        await _connection.PlayAsync(track);

        await ctx.RespondAsync($"Zagrane");
    }
    
    [Command("pause")]
    [Description("pauzuje odtwarzacz")]
    public async Task Pause(CommandContext ctx)
    {
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }
        
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        if (!node.ConnectedGuilds.Any())
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }
        

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }
        

        if (_connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam pauzować?");
            return;
        }

        await _connection.PauseAsync();
    }

    [Command("Resume")]
    [Aliases("r")]
    [Description("wznawia granie")]
    public async Task ResumeAsync(CommandContext ctx)
    {
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }
        
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        if (!node.ConnectedGuilds.Any())
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }

        if (_connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam wznawiać?");
            return;
        }
        await _connection.ResumeAsync();
    }
    
    [Command("stop")]
    [Description("pomija obecny utwór i zatrzymuje odtwarzacz")]
    public async Task StopAsync(CommandContext ctx)
    {
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }
        
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        if (!node.ConnectedGuilds.Any())
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }

        if (_connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam stopować?");
            return;
        }

        _shouldPauseAfterStop = true;
        await _connection.StopAsync();

    }
    
    [Command("skip")]
    [Description("pomija obecny utwór")]
    public async Task SkipAsync(CommandContext ctx)
    {
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.Channel.SendMessageAsync("Musisz być na kanale głosowym");
            return;
        }
        
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        if (!node.ConnectedGuilds.Any())
        {
            await ctx.RespondAsync("Nie ma mnie na żadnym kanale");
            return;
        }

        if (_connection == null)
        {
            await ctx.RespondAsync("Nie ma połączenia z Lavalink");
            return;
        }

        if (_connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync("Ale co ja mam stopować?");
            return;
        }
        await _connection.StopAsync();

    }

    [Command("clear")]
    [Description("czyści kolejkę utworów")]
    public async Task ClearQueue(CommandContext ctx)
    {
        _songsQueue.Clear();
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
        if (!lava.ConnectedNodes.Any())
        { 
            await ctx.RespondAsync( "Nie ma połączenia z Lavalink");
            return false;
        }
        var node = lava.ConnectedNodes.Values.First();
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel.Type != ChannelType.Voice)
        {
            await ctx.RespondAsync("Musisz być na kanale głosowym");
            return false;
        }
        
        _connection = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
        _connection.PlaybackStarted += OnTrackStarted;
        _connection.PlaybackFinished += OnTrackFinished;
        _textChannel = ctx.Channel;
        await ctx.RespondAsync($"Dołączyłem do {ctx.Member.VoiceState.Channel.Name}");
        return true;
    }
    
    
    
    private async Task OnTrackFinished(LavalinkGuildConnection lavalinkGuildConnection, TrackFinishEventArgs args)
    {
        if (!_audioService.IsTrackIndetifierLocal(args.Track.Identifier) && _songsQueue.Count == 0)
        {
            await _textChannel.SendMessageAsync("Zagrano wszystko z kolejki, dawaj kolejne");
            return;
        }

        if (args.Reason == TrackEndReason.Stopped)
        {
            await _connection.PlayAsync(_songsQueue.Dequeue());
            if (_shouldPauseAfterStop)
            {
                await _connection.PauseAsync();
                _shouldPauseAfterStop = false;
            }

            return;
        }
            if (_songsQueue.Count > 0)
        {
            await _connection.PlayAsync(_songsQueue.Dequeue());
        }

    }
    
    private async Task OnTrackStarted(LavalinkGuildConnection sender, TrackStartEventArgs args)
    {
        if (!_audioService.IsTrackIndetifierLocal(args.Track.Identifier))
        {
            await _textChannel.SendMessageAsync($"Teraz grane: {args.Track.Title}");
        }
    }
}