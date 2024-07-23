using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;

namespace DBot.Entities
{
    public class TrackQueueEntry(DiscordChannel channel, LavalinkTrack track) : IQueueEntry
    {
        public LavalinkTrack Track { get; set; } = track;

        public async Task AfterPlayingAsync(LavalinkGuildPlayer player)
        {
            if(player.QueueEntries.Count == 0) 
            {
                await channel.SendMessageAsync("Zagrano wszystko z kolejki, dawaj kolejne");
            }
        }

        public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
        {
            await channel.SendMessageAsync($"Teraz grane: {Track.Info.Title} - {Track.Info.Author}");
            return true;
        }
    }
}
