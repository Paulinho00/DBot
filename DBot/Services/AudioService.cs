﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace DBot.Services
{
    public class AudioService
    {
        private string[] supportedFormats = { "mp3", "wav" };
        private LavaNode _lavaNode;
        private string _path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\Resources\";

        public AudioService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStarted += OnTrackStarted;
        }

        /// <summary>
        /// Joins channel where caller is 
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>message with channel's name or cause why it didn't join to channel</returns>
        public string JoinChannel(SocketCommandContext context)
        {
            //Check if bot is connected to channel
            if (_lavaNode.HasPlayer(context.Guild))
            {
                return "Jestem już połączony do kanału";
            }

            var channel = context.User as IVoiceState;

            //Check if caller is connected to channel
            if (channel?.VoiceChannel == null)
            {
                return "Musisz być na kanale głosowym";

            }

            try
            {
                 _lavaNode.JoinAsync(channel.VoiceChannel, context.Channel as ITextChannel);
                return $"Dołączyłem do {channel.VoiceChannel.Name}";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Plays track searched by given string
        /// </summary>
        /// <param name="searchQuery">query to search song</param>
        /// <param name="context">Context of command</param>
        /// <returns>message with name of track or number of songs or cause why it didn't play/returns>
        public async Task<string> PlayFromInternetAsync(string searchQuery, SocketCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return "Nie podałeś nazwy do wyszukania";

            }

            if (!_lavaNode.HasPlayer(context.Guild))
            {
                JoinChannel(context);
            }

            //Send request to lavalink to find song
            var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
            {
                return "Nic takiego nie znalazłem";
            }

            var player = _lavaNode.GetPlayer(context.Guild);
            await player.UpdateVolumeAsync(100);
            string message = null;

            //Check if found playlist
            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                player.Queue.Enqueue(searchResponse.Tracks);
                message = $"Dodano {searchResponse.Tracks.Count} utworów do kolejki";
            }
            else
            {
                var track = searchResponse.Tracks.FirstOrDefault();
                player.Queue.Enqueue(track);

                message = $"Dodano do kolejki {track?.Title}";
            }

            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            {
                return message;
            }


            player.Queue.TryDequeue(out var lavaTrack);
            await player.PlayAsync(x =>
            {
                x.Track = lavaTrack;
                x.ShouldPause = false;
            });
            return message;
        }

        /// <summary>
        /// Pauses player
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>message on succesful pausing or cause of not pausing</returns>
        public async Task<string> PausePlayerAsync(SocketCommandContext context)
        {
            if(!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            if(player.PlayerState != PlayerState.Playing)
            {
                return "Nie gram nic to co mam zapauzować";
            }

            try
            {
                await player.PauseAsync();
                return "Zapauzowane";
            }
            catch(Exception e)
            {
                return e.Message;
            }

        }

        /// <summary>
        /// Resumes player
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>message on succesful resume or cause of not resuming</returns>
        public async Task<string> ResumePlayerAsync(SocketCommandContext context)
        {
            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                return "Nie gram nic to co mam wznowić";
            }

            try
            {
                await player.ResumeAsync();
                return "Wznowione";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Skip current track
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>Name of skipped track and name of next track or cause of not skipping</returns>
        public async Task<string> SkipCurrentTrackAsync(SocketCommandContext context)
        {
            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return "Nie mam nic do skipowania";
            }

            try
            {
                var (oldTrack, currentTrack) = await player.SkipAsync();
                return $"Pominięto {oldTrack.Title}, teraz gra: {currentTrack.Title}";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Stops player
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>Message of succesful player stop or cause of not stopping</returns>
        public async Task<string> StopPlayerAsync(SocketCommandContext context)
        {
            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                return "Nie mam jak zastopować";
            }

            try
            {
                await player.StopAsync();
                return "Zastopowane";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Clears player's track queue
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>Message of succesful clear or cause of not clearing</returns>
        public string ClearPlayerQueue(SocketCommandContext context)
        {
            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            if(player.Queue.Count == 0)
            {
                return "Kolejka jest pusta";
            }

            player.Queue.Clear();
            return "Kolejka wyczyszczona";

        }

        /// <summary>
        /// Leaves channel which bot is connected to
        /// </summary>
        /// <param name="context">Context of command</param>
        /// <returns>Message of succesful leave or cause of not leaving</returns>
        public async Task<string> LeaveChannelAsync(SocketCommandContext context)
        {
            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            var voiceChannel = (context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
            if(voiceChannel == null)
            {
                return "Nie wiem skąd mam się odłączyć";
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                return $"Odłączyłem się od {voiceChannel.Name}";
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if(args.Reason != TrackEndReason.Finished)
            {
                return;
            }
            var player = args.Player;
            var track = args.Track;
            var test = track.Url.Substring(track.Url.Length - 3);

            //If local sound was played, don't send messages about queue
            if (supportedFormats.Contains(track.Url.Substring(track.Url.Length - 3))){
                return;
            }

            if (!player.Queue.TryDequeue(out var lavaTrack))
            {
                await player.TextChannel.SendMessageAsync("Zagrano wszystko z kolejki, dawaj kolejne");
                return;
            }

            if (lavaTrack is null) {
                await player.TextChannel.SendMessageAsync("Następny element nie jest utworem");
                return;
            }

            await args.Player.PlayAsync(lavaTrack);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nTeraz gra: {lavaTrack.Title}");
        }

        private async Task OnTrackStarted(TrackStartEventArgs arg)
        {
            var track = arg.Track;
            if (supportedFormats.Contains(track.Url.Substring(track.Url.Length - 3)))
            {
                return;
            }

            await arg.Player.VoiceChannel.SendMessageAsync($"Teraz grane: {arg.Track.Title}");
        }

        /// <summary>
        /// Plays track from local file
        /// </summary>
        /// <param name="fileName">file to be played</param>
        /// <param name="context">Context of command</param>
        /// <returns>Message of succesful play or cause of not playing</returns>
        public async Task<string> PlayLocalFile(string filename, SocketCommandContext context)
        {
            if (!_lavaNode.HasPlayer(context.Guild))
            {
                JoinChannel(context);
            }

            if (!_lavaNode.TryGetPlayer(context.Guild, out var player))
            {
                return "Nie jestem na żadnym kanale";
            }

            await player.UpdateVolumeAsync(200);

            if(player.PlayerState == PlayerState.Playing)
            {
                return "Ale widzisz że gram?";
            }

            if (player.PlayerState == PlayerState.Paused)
            {
                return "Zapauzowany jestem";
            }

            //Send request to lavalink to find song
            var soundFilePath = Directory.GetFiles(_path, filename + ".mp3", SearchOption.AllDirectories);

            if (soundFilePath.Length != 0)
            {
                var searchResponse = await _lavaNode.SearchAsync(SearchType.Direct, soundFilePath[0]);

                if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
                {
                    return "Co ty wymyślasz, nie ma nic takiego";
                }

                await player.PlayAsync(x =>
                {
                    x.Track = searchResponse.Tracks.FirstOrDefault();
                    x.ShouldPause = false;
                });

                return "Puszczone";

            }
            else
                return "Co ty wymyślasz, nie ma nic takiego";
        }

        /// <summary>
        /// Returns dictionary with category names as keys and string with all sounds in corresponding category as value
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAllSoundsFromLocalFiles()
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
        /// Returns dictionary with all sounds filenames assigned to category, with category as key or category parts as key when filenames list exceed 1024 characters 
        /// </summary>
        /// <param name="categoryPath"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllSoundsFromCategory(string categoryPath)
        {
            Dictionary<string, string> categorySounds = new Dictionary<string, string>();

            var filenames = Directory.GetFiles(categoryPath).Select(f => Path.GetFileName(f));
            filenames.OrderBy(filename => filename);
            StringBuilder filenamesFormated = new StringBuilder();
            string categoryName = Path.GetFileName(categoryPath);

            if (String.IsNullOrWhiteSpace(categoryName))
                categoryName = "Inne";

            int categoryPartCounter = 1;


            foreach (string filename in filenames)
            {
                var formatedFilename = filename.Substring(0, filename.Length - 4);

                //Checks if filenames list exceed permitted 1024 characters after insertion of new filename
                if (filenamesFormated.Length + formatedFilename.Length > 1024)
                {
                    //Add filenames in parts
                    categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());
                    filenamesFormated.Clear();
                    filenamesFormated.Append("`" + formatedFilename + "` ");
                    categoryPartCounter++;
                }
                else
                    filenamesFormated.Append("`" + formatedFilename + "` ");
            }


            //Checks if there is more than one part, if no then add single part with all filenames
            if (categoryPartCounter == 1)
            {
                categorySounds.Add(categoryName, filenamesFormated.ToString());
            }
            else
            {
                categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());
            }

            return categorySounds;
        }
    }


}
