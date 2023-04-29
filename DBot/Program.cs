using DBot;
using DisCatSharp;
using DisCatSharp.Enums;

public class Program
{
    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = File.ReadAllText("config.txt").Trim(),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.Guilds |
                      DiscordIntents.GuildMembers |
                      DiscordIntents.GuildMessageReactions |
                      DiscordIntents.GuildMessages |
                      DiscordIntents.GuildVoiceStates |
                      DiscordIntents.GuildPresences
        });

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}
