
namespace DBot.Services
{
    public interface IAudioService
    {
        Dictionary<string, string> GetAllSoundsFromLocalFiles();
        FileInfo? GetFilePath(string fileName);
    }
}
