using SnakeClient.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SnakeClient.Services
{
    public class WebSocketService : IWebSocketService, IDisposable
    {
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;

        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        public event Action<string>? OnMessageReceived;
        public event Action<bool>? OnConnectionStateChanged;

        public async Task<bool> ConnectAsync(string uri)
        {
            try
            {
                await DisconnectAsync(); // 确保之前的连接已关闭

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);

                Console.WriteLine("WebSocket连接成功");
                OnConnectionStateChanged?.Invoke(true);

                // 开始监听消息
                _receiveTask = Task.Run(StartListening);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket连接失败: {ex.Message}");
                OnConnectionStateChanged?.Invoke(false);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
                }
                _webSocket.Dispose();
                _webSocket = null;
            }

            if (_receiveTask != null)
            {
                await _receiveTask;
                _receiveTask = null;
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            OnConnectionStateChanged?.Invoke(false);
        }

        public async Task SendAsync(object message)
        {
            if (!IsConnected)
            {
                Console.WriteLine("WebSocket未连接");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                var buffer = new ArraySegment<byte>(bytes);

                await _webSocket!.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
                Console.WriteLine($"发送消息: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息失败: {ex.Message}");
            }
        }
        private async Task StartListening()
        {
            var buffer = new byte[1024 * 16];

            try
            {
                while (_webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource!.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"收到消息: {message}");
                        OnMessageReceived?.Invoke(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("服务器关闭了WebSocket连接");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("WebSocket监听已取消");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket监听错误: {ex.Message}");
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}
