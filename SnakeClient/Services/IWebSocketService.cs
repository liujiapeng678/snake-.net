using System.Text.Json;

namespace SnakeClient.Services
{
    public interface IWebSocketService
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync(string uri);
        Task DisconnectAsync();
        Task SendAsync(object message);
        event Action<string>? OnMessageReceived;
        event Action<bool>? OnConnectionStateChanged;
    }
}