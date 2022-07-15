using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Modules
{
    /// <summary>
    /// Module for audio related commands
    /// </summary>
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins voice channel")]
        public async Task JoinChannelAsync([Summary("Channel to which bot will join")]IVoiceChannel channel = null)
        {
            //Assings voice channel of command caller if there is no specific voice channel given
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            if (channel == null) 
            { 
                await Context.Channel.SendMessageAsync("Musisz być na kanale głosowym"); 
                return; 
            }

            var audioClient = await channel.ConnectAsync();
        }
       
    }
}
