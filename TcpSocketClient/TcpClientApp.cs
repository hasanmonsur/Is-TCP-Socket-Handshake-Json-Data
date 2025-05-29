using System;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace TcpSocketClient
{
    public class TcpClientApp
    {
        private readonly ILogger<TcpClientApp> _logger;

        public TcpClientApp(ILogger<TcpClientApp> logger)
        {
            _logger = logger;
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync("localhost", 12345, cancellationToken);
                _logger.LogInformation("Connected to TCP server.");

                using var stream = client.GetStream();
                var jsonData = JsonSerializer.Serialize(message);
                var data = Encoding.UTF8.GetBytes(jsonData);
                await stream.WriteAsync(data, cancellationToken);
                _logger.LogInformation("Sent message: {MessageId}, Content: {Content}", message.Id, message.Content);

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = JsonSerializer.Deserialize<Message>(responseJson);
                _logger.LogInformation("Received response: {MessageId}, Content: {Content}", response?.Id, response?.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to TCP server.");
                throw;
            }
        }
    }
}
