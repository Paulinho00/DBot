using Discord;
using Discord.WebSocket;

public class Program
{
    /// <summary>
    /// Bot client
    /// </summary>
    private readonly DiscordSocketClient _client;

    public static Task Main(string[] args)
    {
       return new Program().MainAsync();
    }

    private Program()
    {
        _client = new DiscordSocketClient();


        //Assign _client's Log event to Log handler implementation
        _client.Log += Log;

    }

    /// <summary>
    /// Async context of application
    /// </summary>
    /// <returns></returns>
    public async Task MainAsync()
    {
       
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
