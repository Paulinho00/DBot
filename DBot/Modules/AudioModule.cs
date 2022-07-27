using DBot.Managers;
using DBot.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace DBot.Modules
{
    /// <summary>
    /// Module for audio related commands
    /// </summary>
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;

        public AudioModule(LavaNode lavaNode, AudioService audioService)
        {
            _lavaNode = lavaNode;
            _audioService = audioService;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("j")]
        [Summary("Dołącza bota do kanału głosowego")]
        public async Task JoinChannelAsync()
        {
            await ReplyAsync(_audioService.JoinChannel(Context));
        }

        [Command("Play")]
        [Alias("p")]
        [Summary("gra coś z internetu")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            await ReplyAsync(await _audioService.PlayFromInternetAsync(searchQuery, Context));
        }

        [Command("Pause")]
        [Summary("pauza")]
        public async Task PauseAsync()
        {
            await ReplyAsync(await _audioService.PausePlayerAsync(Context));
        }

        [Command("Resume")]
        [Alias("r")]
        [Summary("wznawia granie")]
        public async Task ResumeAsync()
        {
            await ReplyAsync(await _audioService.ResumePlayerAsync(Context));
        }

        [Command("Skip")]
        [Summary("pomija aktualny utwór")]
        public async Task SkipAsync()
        {
            await ReplyAsync(await _audioService.SkipCurrentTrackAsync(Context));
        }

        [Command("Stop")]
        [Summary("pomija utwór i pauzuje granie")]
        public async Task StopAsync()
        {
            await ReplyAsync(await _audioService.StopPlayerAsync(Context));
        }

        [Command("clear")]
        [Alias("clr")]
        [Summary("czyści kolejkę utworów")]
        public async Task ClearQueue()
        {
            await ReplyAsync( _audioService.ClearPlayerQueue(Context));
        }

        [Command("leave")]
        [Alias("l")]
        [Summary("opuszcza kanał")]
        public async Task LeaveChannelAsync()
        {
            await ReplyAsync(await _audioService.LeaveChannelAsync(Context));
        }


        [Command("kubica")]
        [Alias("k")]
        [Summary("gra zdefiniowane dźwięki ")]
        public async Task PlayLocalTracks([Remainder]string filename)
        {
            await ReplyAsync(await _audioService.PlayLocalFile(filename, Context));
        }

        /// <summary>
        /// Displays all possible sounds from local files
        /// </summary>
        /// <returns></returns>
        [Command("sounds")]
        [Alias("s")]
        [Summary("wyświetla wszystkie możliwe dźwięki")]
        public async Task DisplayLocalFilesSounds()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Blue);

            //Field with all possible sounds from local files
            embed.AddField("Dźwięki", _audioService.GetAllSoundsFromLocalFiles());

            await ReplyAsync(embed: embed.Build());
        }

    }
}
