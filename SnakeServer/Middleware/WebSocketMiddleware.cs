using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using SnakeServer.Models;
using System.Collections.Concurrent;
using SnakeServer.Constants;
using System.Data;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;

namespace SnakeServer.Middleware
{

    public class WebSocketMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly RequestDelegate _next;
        public static Game? game { get; set; } = null;
        public static readonly ConcurrentDictionary<int, WebSocket> _connections = new(); // 保存 WebSocket 连接

        public WebSocketMiddleware(RequestDelegate next, HttpClient httpClient)
        {
            _next = next;
            _httpClient = httpClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 检查是否是 WebSocket 请求，并且路径匹配 /ws/{userId} 格式
            if (context.Request.Path.StartsWithSegments("/ws") && context.WebSockets.IsWebSocketRequest)
            {
                int userId = ExtractUserIdFromRequest(context);
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                // 将连接保存到字典
                _connections[userId] = webSocket;
                // 处理 WebSocket 连接
                await HandleWebSocketConnection(userId, webSocket);
            }
            else
            {
                await _next(context);
            }
        }
        private int ExtractUserIdFromRequest(HttpContext context)
        {
            // 从 URL 路径提取 userId (路径格式为 /ws/{userId})
            var path = context.Request.Path.Value;

            if (string.IsNullOrEmpty(path))
                return -1;

            // 分割路径，期望格式: /ws/123
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 验证路径格式：应该有两个部分 ["ws", "userId"]
            if (segments.Length == 2 && segments[0].Equals("ws", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(segments[1], out var userId) && userId > 0)
                {
                    return userId;
                }
            }

            return -1;
        }
        private async Task HandleWebSocketConnection(int userId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 16]; // 缓冲区

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessMessage(userId, message, webSocket);
                        Console.WriteLine(userId);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error for user {userId}: {ex.Message}");
            }
            finally
            {
                // 清理连接
                _connections.TryRemove(userId, out _);
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", CancellationToken.None);
                }
            }
        }
        public static void RemoveConnection(int userId)
        {
            _connections.TryRemove(userId, out _);
        }
        public static async Task SendMessageToUser(int userId, object message, WebSocket webSocket)
        {
            if (_connections[userId] != null && _connections[userId].State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        private class WebsocketFormat
        {
            public string? @event { get; set; }
            public int? botId { get; set; }
            public int? rating { get; set; }
            public int? d { get; set; }
        }
        private void ProcessMessage(int userId, string message, WebSocket webSocket)
        {
            var messageObj = JsonSerializer.Deserialize<WebsocketFormat>(message);

            switch (messageObj?.@event)
            {
                case "start-match":
                    StartMatch(userId, messageObj.botId, messageObj.rating);
                    break;

                case "stop-match":
                    StopMatch(userId);
                    break;

                case "move":
                    Move(messageObj.d.Value, userId);
                    break;
            }
        }
        private async void StartMatch(int userId, int? botId, int? rating)
        {
            Console.WriteLine(botId);
            Console.WriteLine("开始匹配");
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5001/api/matching/add",
                new
                {
                    userId = userId,
                    rating = rating,
                    botId = botId
                }
            );
        }
        private async void StopMatch(int userId)
        {
            Console.WriteLine("停止匹配");
            var response = await _httpClient.PostAsJsonAsync(
               "http://localhost:5001/api/matching/remove",
               new
               {
                   userId = userId
               }
           );
        }
        private void Move(int d, int userId)
        {
            if (WebSocketMiddleware.game.PlayerA.Id == userId)
            {
                if (WebSocketMiddleware.game.PlayerA.BotId <= -1) WebSocketMiddleware.game.SetNextStepA(d);
            }
            else if (WebSocketMiddleware.game.PlayerB.Id == userId)
            {
                if (WebSocketMiddleware.game.PlayerB.BotId <= -1) WebSocketMiddleware.game.SetNextStepB(d);
            }
        }
        public static async Task StartGame(int aId, int bId, int aBotId, int bBotId, int aRating, int bRating, IConfiguration _config, HttpClient _httpClient)
        {
            
            Users? a = null;
            Users? b = null;
            Bots? aBot = null;
            Bots? bBot = null;
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();
                // 查询用户A
                var userASql = $"SELECT * FROM Users WHERE Id = {aId}";
                using (var userACmd = new MySqlCommand(userASql, conn))
                {
                    using (var reader = userACmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            a = new Users
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Photo = reader.GetString("Photo"),
                            };
                        }
                    }
                }
                // 查询用户B
                var userBSql = $"SELECT * FROM Users WHERE Id = {bId}";
                using (var userBCmd = new MySqlCommand(userBSql, conn))
                { 
                    using (var reader = userBCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            b = new Users
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Photo = reader.GetString("Photo"),
                            };
                        }
                    }
                }
                // 查询机器人A (如果aBotId > 0)
                if (aBotId > 0)
                {
                    var botASql = $"SELECT * FROM Bots WHERE Id = {aBotId}";
                    using (var botACmd = new MySqlCommand(botASql, conn))
                    {
                        using (var reader = botACmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                aBot = new Bots
                                {
                                    Id = reader.GetInt32("Id"),
                                    UserId = reader.GetInt32("UserId"),
                                    BotName = reader.GetString("BotName"),
                                    BotDescription = reader.GetString("BotDescription"),
                                    Code = reader.GetString("Code"),
                                    Rating = reader.GetInt32("Rating"),
                                };
                            }
                        }
                    }
                }
                // 查询机器人B (如果bBotId > 0)
                if (bBotId > 0)
                {
                    var botBSql = $"SELECT * FROM Bots WHERE Id = {bBotId}";
                    using (var botBCmd = new MySqlCommand(botBSql, conn))
                    {
                        using (var reader = botBCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bBot = new Bots
                                {
                                    Id = reader.GetInt32("Id"),
                                    UserId = reader.GetInt32("UserId"),
                                    BotName = reader.GetString("BotName"),
                                    BotDescription = reader.GetString("BotDescription"),
                                    Code = reader.GetString("Code"),
                                    Rating = reader.GetInt32("Rating"),
                                };
                            }
                        }
                    }
                }
            }
            // 创建游戏实例
            Game g = new Game(Constants.Constants.ROWS, Constants.Constants.COLS, Constants.Constants.INNER_WALLS_COUNT, aId, aBot, bId, bBot, aRating, bRating, _httpClient, _config); // 根据实际需要调整参数
            g.CreateGameMap();
            game = g;
            //if (_connections[aId] != null)
            //{     //在玩家匹配时意外断开，但是玩家仍在匹配池中的情况，此时WebSocketServer是空，会空指针
            //    _connections[aId].game = game;
            //}
            //if (_connections[bId] != null)
            //{
            //    _connections[bId].game = game;
            //}
            // 启动游戏
            Task.Run(async () => await game.RunAsync());
            if (_connections[aId] != null)
            {
                await SendMessageToUser(aId, new
                {
                    opponentName = b.Username,
                    opponentPhoto = b.Photo,
                    @event = "match-found",
                    me = "A",
                    aId = game.PlayerA.Id,
                    aSx = game.PlayerA.Sx,
                    aSy = game.PlayerA.Sy,
                    bId = game.PlayerB.Id,
                    bSx = game.PlayerB.Sx,
                    bSy = game.PlayerB.Sy,
                    map = ConvertTo2DArray(game.GameMap)
                }, _connections[aId]);
            }
            if (_connections[bId] != null)
            {
                await SendMessageToUser(bId, new
                {
                    opponentName = a.Username,
                    opponentPhoto = a.Photo,
                    @event = "match-found",
                    me = "B",
                    aId = game.PlayerA.Id,
                    aSx = game.PlayerA.Sx,
                    aSy = game.PlayerA.Sy,
                    bId = game.PlayerB.Id,
                    bSx = game.PlayerB.Sx,
                    bSy = game.PlayerB.Sy,
                    map = ConvertTo2DArray(game.GameMap)
                }, _connections[bId]);
            }


        }
        public static int[][] ConvertTo2DArray(int[,] matrix)
        {
            if (matrix == null) return null;

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int[][] jaggedArray = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = matrix[i, j];
                }
            }

            return jaggedArray;
        }
    }
}


