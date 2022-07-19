using Discord;
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
        private readonly LavaNode _lavaNode;

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
                return "Nie jestem połączony do kanału";
            }

            //Send request to lavalink to find song
            var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
            {
                return "Nic takiego nie znalazłem";
            }

            var player = _lavaNode.GetPlayer(context.Guild);
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

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if(args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
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
            await arg.Player.TextChannel.SendMessageAsync($"Teraz grane: {arg.Track.Title}");
        }
    }
}
