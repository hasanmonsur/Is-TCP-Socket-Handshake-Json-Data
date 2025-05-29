// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TcpSocketServer;

Console.WriteLine("Hello, World!");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(logging => logging.AddConsole());
        services.AddSingleton<TcpServer>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting TCP Server...");

var tcpServer = host.Services.GetRequiredService<TcpServer>();
await tcpServer.StartAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

await host.RunAsync();