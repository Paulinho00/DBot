
using DisCatSharp.ApplicationCommands.Context;

namespace DBot.Services
{
    public interface IAudioService
    {
        Task<bool> ConnectToVoiceChannelAsync(InteractionContext ctx);
        Task LeaveAsync(InteractionContext ctx);
        Task DisplayLocalSoundsAsync(InteractionContext ctx);
        Task PlaySoundFromInternetAsync(InteractionContext ctx, string query);
        Task PlayLocalSoundAsync(InteractionContext ctx, string fileName);
        Task PauseAsync(InteractionContext ctx);
        Task ResumeAsync(InteractionContext ctx);
        Task StopAsync(InteractionContext ctx);
        Task SkipAsync(InteractionContext ctx);
        Task ClearQueueAsync(InteractionContext ctx);
    }
}
