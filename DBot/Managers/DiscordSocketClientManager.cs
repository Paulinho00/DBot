using DBot.Services;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace DBot.Managers
{
    /// <summary>
    /// Manager for DiscordSocketClient
    /// </summary>
    public class DiscordSocketClientManager
    {
        public DiscordSocketClient SocketClient { get; }
        public LavaNode InstanceOfLavaNode { get; }
        public LavaConfig LavaConfig { get; }

        public DiscordSocketClientManager()
        {
            SocketClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildVoiceStates |
                GatewayIntents.GuildPresences
            });

            LavaConfig = new LavaConfig();
            InstanceOfLavaNode = new LavaNode(SocketClient, LavaConfig);

            SocketClient.Log += Log;
            SocketClient.Ready += OnReadyAsync;
        }

        /// <summary>
        /// Return log messages on console
        /// </summary>
        /// <param name="msg">LogMessage object provided by log source</param>
        /// <returns></returns>
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Connects to LavaLink server
        /// </summary>
        /// <returns></returns>
        private async Task OnReadyAsync()
        {
            if (!InstanceOfLavaNode.IsConnected)
            {
               InstanceOfLavaNode.ConnectAsync();
            }
        }
    }
}
