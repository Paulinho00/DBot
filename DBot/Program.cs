using DBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

public class Program
{
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _commandHandler;
    private readonly CommandService _commandService;

    public static Task Main(string[] args)
    {
       return new Program().MainAsync();
    }

    private Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig()
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
        _commandService = new CommandService();
        _commandHandler = new CommandHandler(_client, _commandService);



        //Assign _client's Log event to Log handler implementation
        _client.Log += Log;

    }

    /// <summary>
    /// Async context of application
    /// </summary>
    /// <returns></returns>
    public async Task MainAsync()
    {
        await _commandHandler.InitCommandsAsync();

        //Change token source dependent on your way of storing
        await _client.LoginAsync(TokenType.Bot, File.ReadAllText("../../../config.txt").Trim());
        await _client.StartAsync();

        //Block using task until the program is closed
        await Task.Delay(-1);

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
}
