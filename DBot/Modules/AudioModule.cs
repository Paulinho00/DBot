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
        [Summary("Join voice channel")]
        public async Task JoinChannelAsync()
        {
            await ReplyAsync(_audioService.JoinChannel(Context));
        }

        [Command("Play")]
        [Alias("p")]
        [Summary("Play sounds from internet")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            await ReplyAsync(await _audioService.PlayFromInternetAsync(searchQuery, Context));
        }

        [Command("Pause")]
        [Summary("Pauses player")]
        public async Task PauseAsync()
        {
            await ReplyAsync(await _audioService.PausePlayerAsync(Context));
        }

        [Command("Resume")]
        [Alias("r")]
        [Summary("Resume player")]
        public async Task ResumeAsync()
        {
            await ReplyAsync(await _audioService.ResumePlayerAsync(Context));
        }

        [Command("Skip")]
        [Summary("Skips current track")]
        public async Task SkipAsync()
        {
            await ReplyAsync(await _audioService.SkipCurrentTrackAsync(Context));
        }

        [Command("Stop")]
        [Summary("Stop player")]
        public async Task StopAsync()
        {
            await ReplyAsync(await _audioService.StopPlayerAsync(Context));
        }

        [Command("clear")]
        [Alias("clr")]
        [Summary("Clear player's tracks queue")]
        public async Task ClearQueue()
        {
            await ReplyAsync( _audioService.ClearPlayerQueue(Context));
        }

        [Command("leave")]
        [Alias("l")]
        [Summary("Leave voice channel which bot is connected to")]
        public async Task LeaveChannelAsync()
        {
            await ReplyAsync(await _audioService.LeaveChannelAsync(Context));
        }


        [Command("kubica")]
        [Summary("Play track from local file")]
        public async Task PlayKubicaTracks([Remainder]string filename)
        {
            await ReplyAsync(await _audioService.PlayLocalFile(filename, Context));
        }
       
    }
}
