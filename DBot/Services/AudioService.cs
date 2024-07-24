using System.Text;
using System.Text.RegularExpressions;
using DBot.Entities;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace DBot.Services;

public class AudioService : IAudioService
{
    private readonly string[] _supportedFormats = { ".mp3", ".wav" };
    private readonly string _path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Resources\";
    private LavalinkSession? _session;
    private LavalinkGuildPlayer? _player;

    public async Task LeaveAsync(InteractionContext ctx)
    {
        if (_session == null || !_session.IsConnected)
        {
            await SendReplyMessage(ctx, "Nie ma połączenia do Lavalink");
            return;
        }

        if (_player == null)
        {
            await SendReplyMessage(ctx, "Nie jestem podłączony do kanału");
            return;
        }

        await _player.DisconnectAsync();
        await SendReplyMessage(ctx,"Uciekam");
        _player = null;
        _session = null;
    }
    
    public async Task DisplayLocalSoundsAsync(InteractionContext ctx)
    {
        Dictionary<string, string> fieldsValues = GetAllSoundsFromLocalFiles();
        var embed = new DiscordEmbedBuilder();
        embed.Color = DiscordColor.IndianRed;

        //Add each category and its sounds to fields 
        foreach (string category in fieldsValues.Keys)
        {
            var field = new DiscordEmbedField(category, fieldsValues[category]);
            embed.AddField(field);
        }

        var embeds = new List<DiscordEmbed> { embed.Build() }.AsReadOnly();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder(new DiscordMessageBuilder().WithEmbed(embed)));
    }
    
    public async Task PlaySoundFromInternetAsync(InteractionContext ctx, string query)
    {
        if (ctx.Member!.VoiceState == null)
        {
            await SendReplyMessage(ctx, "Musisz być na kanale głosowym");
            return;
        }

        if (_session == null)
        {
            if (!await ConnectToVoiceChannelAsync(ctx))
                return;
        }
        
        //Checks if search query is a URI
        LavalinkTrackLoadingResult loadResult;
        if (query.Contains("https"))
        {
            loadResult = await _player!.LoadTracksAsync(LavalinkSearchType.Plain, query);
        }
        else
        {
            loadResult = await _player!.LoadTracksAsync(LavalinkSearchType.Youtube, query);
        }

        if (loadResult.LoadType == LavalinkLoadResultType.Empty
            || loadResult.LoadType == LavalinkLoadResultType.Error)
        {
            await SendReplyMessage(ctx, "Nie ma nic takiego");
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

            await SendReplyMessage(ctx, "Zagrane");
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
            await SendReplyMessage(ctx, $"Dodane do kolejki: {track.Info.Title} - {track.Info.Author}");
        }
    }
    
    public async Task PlayLocalSoundAsync(InteractionContext ctx, string fileName)
    {
        if (ctx.Member!.VoiceState == null)
        {
            await SendReplyMessage(ctx, "Musisz być na kanale głosowym");
            return;
        }

        if (_session == null)
        {
            if (!await ConnectToVoiceChannelAsync(ctx))
                return;
        }

        var filepath = GetFilePath(fileName);
        if (filepath == null)
        {
            await SendReplyMessage(ctx, "Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }

        var loadResult = await _player!.LoadTracksAsync(LavalinkSearchType.Plain, filepath.FullName);

        if (loadResult.LoadType == LavalinkLoadResultType.Empty
            || loadResult.LoadType == LavalinkLoadResultType.Error)
        {
            await SendReplyMessage(ctx, "Co ty wymyślasz, nie ma takiego dźwięku");
            return;
        }
        
        await _player.PlayAsync(filepath.FullName);
        if (_player.QueueEntries.Count != 0)
        {
            _player.PlayQueueAsync();
        }

        await SendReplyMessage(ctx,$"Zagrane");
    }
    
    public async Task PauseAsync(InteractionContext ctx)
    {
        if (_session == null)
        {
            await SendReplyMessage(ctx, "Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await SendReplyMessage(ctx, "Ale co ja mam wznawiać?");
            return;
        }
        
        await _player.PauseAsync();
        await SendReplyMessage(ctx, "Zapauzowane");
    }
    
    public async Task ResumeAsync(InteractionContext ctx)
    {
        if (_session == null)
        {
            await SendReplyMessage(ctx, "Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await SendReplyMessage(ctx, "Ale co ja mam wznawiać?");
            return;
        }
        await _player.ResumeAsync();
        await SendReplyMessage(ctx, "Jedziemy");
    }
    
    public async Task StopAsync(InteractionContext ctx)
    {
        if (_session == null)
        {
            await SendReplyMessage(ctx,"Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.CurrentTrack == null)
        {
            await SendReplyMessage(ctx, "Ale co ja mam Stopować?");
            return;
        }
        
        await _player.StopAsync();
        await SendReplyMessage(ctx, "Zastopowane");
    }
    
    public async Task SkipAsync(InteractionContext ctx)
    {
        if (_session == null)
        {
            await SendReplyMessage(ctx, "Nie ma mnie na żadnym kanale");
            return;
        }

        if (_player!.QueueEntries.Count == 0)
        {
            await SendReplyMessage(ctx, "Ale co ja mam Stopować?");
            return;
        }

        //_shouldPauseAfterStop = true;
        var firstTrack = _player.QueueEntries[0];
        _player.RemoveFromQueue(firstTrack);
        await SendReplyMessage(ctx, $"Pominięto {firstTrack.Track.Info.Title} - {firstTrack.Track.Info.Author}");
    }

    public async Task ClearQueueAsync(InteractionContext ctx)
    {
        if (_player == null)
        {
            await SendReplyMessage(ctx, "Nic nie ma w kolejce");
            return;
        }

        foreach (var queueEntry in _player.QueueEntries)
        {
            _player.RemoveFromQueue(queueEntry);
        }
        await SendReplyMessage(ctx, "Wyczyszono kolejkę");
    }
    
    /// <summary>
    /// Returns all sounds from Resources directory
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> GetAllSoundsFromLocalFiles()
    {
        Dictionary<string, string> categoriesWithSounds = new Dictionary<string, string>();

        string[] categories = Directory.GetDirectories(_path);

        //Adds each category with its sounds to dictionary
        foreach (string category in categories)
        {
            var allSoundsFromCategory = GetAllSoundsFromCategory(category);
            foreach (string parts in allSoundsFromCategory.Keys)
                categoriesWithSounds.Add(parts, allSoundsFromCategory[parts]);


        }
        
        //Checks if there are sounds without category and adds them to dictionary
        var filenamesWithoutCategory = GetAllSoundsFromCategory(_path);
        if (filenamesWithoutCategory[filenamesWithoutCategory.Keys.First()].Length != 0)
        {
            foreach(string parts in filenamesWithoutCategory.Keys)
                categoriesWithSounds.Add(parts, filenamesWithoutCategory[parts]);
        }
        return categoriesWithSounds;
    }

    /// <summary>
    /// Get path to a filename within a resources directory
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>Full, absolute path</returns>
    private FileInfo? GetFilePath(String fileName)
    {
        foreach (var format in _supportedFormats)
        {
            try
            {
                var soundFilePath = Directory.GetFiles(_path, fileName + format, SearchOption.AllDirectories);
                if (soundFilePath.Any())
                {
                    FileInfo fileInfo = new FileInfo(soundFilePath[0]);
                    return fileInfo;
                }
            }
            catch (IOException)
            {
                return null;
            }
        }
        return null;

    }
    
    
     /// <summary>
     /// Returns dictionary with all sounds filenames assigned to category, with category as key or category parts as key when filenames list exceed 1024 characters 
     /// </summary>
     /// <param name="categoryPath"></param>
     /// <returns></returns>
     private Dictionary<string, string> GetAllSoundsFromCategory(string categoryPath)
     {
         var categorySounds = new Dictionary<string, string>();

         var filenames = Directory.GetFiles(categoryPath).Select(f => Path.GetFileName(f))
             .Where(x => _supportedFormats.Any(format => x.EndsWith(format))).ToList();
         filenames = filenames.OrderBy(filename => filename).ToList();
         var filenamesFormated = new StringBuilder();
         var categoryName = Path.GetFileName(categoryPath);

         if (string.IsNullOrWhiteSpace(categoryName))
             categoryName = "Inne";

         var categoryPartCounter = 1;


         foreach (var filename in filenames)
         {
             //replace normal space with hard space to avoid breaking line in the middle of a phrase
             var formatedFilename = Regex.Replace(filename.Substring(0, filename.Length - 4), " ", "⠀");

             //Checks if filenames list exceed permitted 1024 characters after insertion of new filename
             if (filenamesFormated.Length + formatedFilename.Length > 1021)
             {
                 //Add filenames in parts
                 categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());
                 filenamesFormated.Clear();
                 filenamesFormated.Append("`" + formatedFilename + "` ");
                 categoryPartCounter++;
             }
             else
             {
                 filenamesFormated.Append("`" + formatedFilename + "` ");
             }
         }


         //Checks if there is more than one part, if no then add single part with all filenames
         if (categoryPartCounter == 1)
             categorySounds.Add(categoryName, filenamesFormated.ToString());
         else
             categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());

         return categorySounds;
     }
     
     private async Task SendReplyMessage(InteractionContext ctx, string message)
     {
         await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
             new DiscordInteractionResponseBuilder()
             {
                 Content = message
             });
     }
     
     /// <summary>
     /// Tries to connect to voice channel
     /// </summary>
     /// <param name="ctx"></param>
     /// <returns>true if it connected, false otherwise</returns>
     public async Task<bool> ConnectToVoiceChannelAsync(InteractionContext ctx)
     {
         var lava = ctx.Client.GetLavalink();

         if (!lava.ConnectedSessions.Any())
         {
             await SendReplyMessage(ctx, "Nie ma połączenia z Lavalink");
             return false;
         }

         var lavalinkConfig = new LavalinkConfiguration();
         lavalinkConfig.DefaultVolume = 500;
         lavalinkConfig.EnableBuiltInQueueSystem = true;


         _session = await lava.ConnectAsync(lavalinkConfig);
         if (ctx.Member!.VoiceState == null ||  ctx.Member.VoiceState.Channel!.Type != ChannelType.Voice)
         {
             await SendReplyMessage(ctx, "Musisz być na kanale głosowym");
             return false;
         }
        
         _player = await _session.ConnectAsync(ctx.Member.VoiceState.Channel);
         await SendReplyMessage(ctx, $"Dołączyłem do {ctx.Member.VoiceState.Channel.Name}");
         return true;
     }
}

    
    
