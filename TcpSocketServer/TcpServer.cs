using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TcpSocketServer
{
    public class TcpServer
    {
        private readonly TcpListener _listener;
        private readonly ILogger<TcpServer> _logger;
        private bool _isRunning;

        public TcpServer(ILogger<TcpServer> logger)
        {
            _listener = new TcpListener(IPAddress.Any, 12345);
            _logger = logger;
            _isRunning = false;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _listener.Start();
                _isRunning = true;
                _logger.LogInformation("TCP Server started on port 12345.");

                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    _logger.LogInformation("Client connected: {ClientEndpoint}", client.Client.RemoteEndPoint);
                    _ = HandleClientAsync(client, cancellationToken); // Fire-and-forget client handling
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TCP Server.");
            }
            finally
            {
                Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    var jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    try
                    {
                        var message = JsonSerializer.Deserialize<Message>(jsonData);
                        _logger.LogInformation("Received message: {MessageId}, Content: {Content}", message?.Id, message?.Content);

                        // Echo back the same message with a modified content
                        var response = new Message
                        {
                            Id = message?.Id ?? Guid.NewGuid().ToString(),
                            Content = $"Server received: {message?.Content}",
                            Timestamp = DateTime.UtcNow
                        };
                        var responseJson = JsonSerializer.Serialize(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                        await stream.WriteAsync(responseBytes, cancellationToken);
                        _logger.LogInformation("Sent response to client: {ClientEndpoint}", client.Client.RemoteEndPoint);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Invalid JSON received from client: {ClientEndpoint}", client.Client.RemoteEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client: {ClientEndpoint}", client.Client.RemoteEndPoint);
            }
            finally
            {
                client.Close();
                _logger.LogInformation("Client disconnected: {ClientEndpoint}", client.Client.RemoteEndPoint);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            _logger.LogInformation("TCP Server stopped.");
        }
    }
}
