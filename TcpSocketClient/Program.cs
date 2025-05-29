// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TcpSocketClient;



Console.WriteLine("Hello, World!");
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(logging => logging.AddConsole());
        services.AddSingleton<TcpClientApp>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var client = host.Services.GetRequiredService<TcpClientApp>();
var cts = new CancellationTokenSource();

logger.LogInformation("TCP Client started. Type a message to send (or 'exit' to quit):");

while (true)
{
    Console.Write("Enter message: ");
    var input = Console.ReadLine();
    if (input?.ToLower() == "exit")
        break;

    try
    {
        var message = new Message { Content = input ?? "Empty message" };
        await client.SendMessageAsync(message, cts.Token);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send message.");
    }
}

logger.LogInformation("TCP Client stopped.");
await host.StopAsync();