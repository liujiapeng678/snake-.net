using MySql.Data.MySqlClient;
using SnakeServer.Middleware;
using SnakeServer.Models;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static SnakeServer.Controllers.GameController;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SnakeServer.Models
{
    public class Game
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly int rows;
        private readonly int cols;
        private readonly int innerWallsCount;
        public int[,] GameMap { get; set; }
        private static readonly int[] dx = { -1, 0, 1, 0 };
        private static readonly int[] dy = { 0, 1, 0, -1 };
        public Player PlayerA { get; set; }
        public Player PlayerB { get; set; }
        private int? nextStepA = null;
        private int? nextStepB = null;
        private readonly object lockObject = new object();
        private string status = "playing";  // playing finished
        private string loser = ""; // all, A, B

        public Game(int rows, int cols, int innerWallsCount, int aId, Bots? aBot, int bId, Bots? bBot, int aRating, int bRating, HttpClient httpClient, IConfiguration config)
        {
            this.rows = rows;
            this.cols = cols;
            this.innerWallsCount = innerWallsCount;
            GameMap = new int[rows, cols];

            int aBotId = -1, bBotId = -1;
            string aBotCode = "", bBotCode = "";
            if (aBot != null)
            {
                aBotId = aBot.Id;
                aBotCode = aBot.Code;
            }
            if (bBot != null)
            {
                bBotId = bBot.Id;
                bBotCode = bBot.Code;
            }

            PlayerA = new Player(aId, aBotId, aRating, aBotCode, rows - 2, 1, new List<int>());
            PlayerB = new Player(bId, bBotId, bRating, bBotCode, 1, cols - 2, new List<int>());
            _httpClient = httpClient;
            _config = config;
        }

        private string MapToString()
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    res.Append(GameMap[i, j]);
                }
            }
            return res.ToString();
        }

        public void SetNextStepA(int nextStepA)
        {
            lock (lockObject)
            {
                Console.WriteLine("SetNextStepA");
                Console.WriteLine(nextStepA);
                Console.WriteLine(this.nextStepA);
                this.nextStepA = nextStepA;
            }
        }

        public void SetNextStepB(int nextStepB)
        {
            lock (lockObject)
            {
                Console.WriteLine("SetNextStepB");
                Console.WriteLine(nextStepB);
                Console.WriteLine(this.nextStepB);
                this.nextStepB = nextStepB;
            }
        }

        private bool CheckConnect(int sx, int sy, int tx, int ty)
        {
            if (sx == tx && sy == ty) return true;
            GameMap[sx, sy] = 1;
            for (int i = 0; i < 4; i++)
            {
                int x = sx + dx[i], y = sy + dy[i];
                if (x >= 0 && x < rows && y >= 0 && y < cols && GameMap[x, y] == 0)
                {
                    if (CheckConnect(x, y, tx, ty))
                    {
                        GameMap[sx, sy] = 0;
                        return true;
                    }
                }
            }
            GameMap[sx, sy] = 0;
            return false;
        }

        private bool CreateWalls()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    GameMap[i, j] = 0;
                }
            }
            for (int r = 0; r < rows; r++)
            {
                GameMap[r, 0] = GameMap[r, cols - 1] = 1;
            }
            for (int c = 0; c < cols; c++)
            {
                GameMap[0, c] = GameMap[rows - 1, c] = 1;
            }

            Random rand = new Random();
            for (int i = 0; i < innerWallsCount / 2; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    int r = rand.Next(rows);
                    int c = rand.Next(cols);
                    if (GameMap[r, c] == 1 || GameMap[rows - 1 - r, cols - 1 - c] == 1)
                    {
                        continue;
                    }
                    if ((r == rows - 2 && c == 1) || (r == 1 && c == cols - 2))
                    {
                        continue;
                    }
                    GameMap[r, c] = GameMap[rows - 1 - r, cols - 1 - c] = 1;
                    break;
                }
            }
            return CheckConnect(rows - 2, 1, 1, cols - 2);
        }

        public void CreateGameMap()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (CreateWalls())
                {
                    break;
                }
            }
        }

        private string GetInput(Player player)
        {
            Player me, you;
            if (player.Id == PlayerA.Id)
            {
                me = PlayerA;
                you = PlayerB;
            }
            else
            {
                me = PlayerB;
                you = PlayerA;
            }
            return MapToString() + '#' +
                   me.Sx.ToString() + '#' +
                   me.Sy.ToString() + "#(" +
                   me.StepsToString() + ")#" +
                   you.Sx.ToString() + '#' +
                   you.Sy.ToString() + "#(" +
                   you.StepsToString() + ")#";
        }

        private async Task SendBotCodeAsync(Player player)
        {
            if (player.BotId <= -1) return;

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5273/api/bot/add",
                new
                {
                    userId = player.Id,
                    botId = player.BotId,
                    botCode = player.BotCode,
                    input = GetInput(player)
                }
            );

        }
        private async Task SendAllMessage(object message)
        {

            if (WebSocketMiddleware._connections[PlayerA.Id] != null)
            {
                await WebSocketMiddleware.SendMessageToUser(PlayerA.Id, message, WebSocketMiddleware._connections[PlayerA.Id]);
            }
            if (WebSocketMiddleware._connections[PlayerB.Id] != null)
            {
                await WebSocketMiddleware.SendMessageToUser(PlayerB.Id, message, WebSocketMiddleware._connections[PlayerB.Id]);
            }
        }

        
        private void SendMove()
        {
            lock (lockObject)
            {
                var resp = new
                {
                    @event = "move",
                    aMove = nextStepA,
                    bMove = nextStepB
                };
                nextStepA = null;
                nextStepB = null;
                SendAllMessage(resp);
            }
        }

        private bool CheckValid(List<Cell> cellsA, List<Cell> cellsB)
        {
            int n = cellsA.Count;
            Cell cell = cellsA[n - 1];
            if (GameMap[cell.X, cell.Y] == 1)
            {
                return false;
            }
            for (int i = 0; i < n - 1; i++)
            {
                if (cellsA[i].X == cell.X && cellsA[i].Y == cell.Y)
                {
                    return false;
                }
            }
            for (int i = 0; i < n - 1; i++)
            {
                if (cellsB[i].X == cell.X && cellsB[i].Y == cell.Y)
                {
                    return false;
                }
            }
            return true;
        }

        private void Judge()
        {
            List<Cell> cellsA = PlayerA.GetCells();
            List<Cell> cellsB = PlayerB.GetCells();

            bool validA = CheckValid(cellsA, cellsB);
            bool validB = CheckValid(cellsB, cellsA);
            if (!validA || !validB)
            {
                status = "finished";
                if (!validA && !validB)
                {
                    loser = "all";
                }
                else if (!validA)
                {
                    loser = "A";
                }
                else if (!validB)
                {
                    loser = "B";
                }
            }
        }
        private void SendResult()
        {
            var resp = new
            {
                @event = "result",
                loser = loser
            };
            SaveRecordToDatabase();
            SendAllMessage(resp);
        }

        private void SaveRecordToDatabase()
        {
            int aRating = PlayerA.Rating;
            int aBotId = PlayerA.BotId.Value;
            int bRating = PlayerB.Rating;
            int bBotId = PlayerB.BotId.Value;
            if ("A".Equals(loser))
            {
                aRating -= 30;
                bRating += 40;
            }
            else if ("B".Equals(loser))
            {
                aRating += 40;
                bRating -= 30;
            }
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update Bot A's rating
                        string updateBotASql = $"UPDATE Bots SET Rating = {aRating} WHERE Id = {aBotId}";
                        using (var cmdA = new MySqlCommand(updateBotASql, conn, transaction))
                        {
                            cmdA.ExecuteNonQuery();
                        }
                        // Update Bot B's rating
                        string updateBotBSql = $"UPDATE Bots SET Rating = {bRating} WHERE Id = {bBotId}";
                        using (var cmdB = new MySqlCommand(updateBotBSql, conn, transaction))
                        {
                            cmdB.ExecuteNonQuery();
                        }
                        // Insert new record into Records table
                        string insertRecordSql = $"INSERT INTO Records (AId, ASx, ASy, BId, BSx, BSy, ASteps, BSteps, Map, Loser, CreateTime) VALUES ({PlayerA.Id}, {PlayerA.Sx}, {PlayerA.Sy}, {PlayerB.Id}, {PlayerB.Sx}, {PlayerB.Sy}, '{PlayerA.StepsToString()}', '{PlayerB.StepsToString()}', '{MapToString()}', '{loser}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";
                        using (var cmdRecord = new MySqlCommand(insertRecordSql, conn, transaction))
                        {
                            cmdRecord.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction on error
                        transaction.Rollback();
                        throw; // Re-throw the exception to handle it at a higher level
                    }
                }
            }
        }
        private async Task<bool> NextStepAsync()
        {
            try
            {
                await Task.Delay(200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await SendBotCodeAsync(PlayerA);
            await SendBotCodeAsync(PlayerB);

            for (int i = 0; i < 5000; i++)
            {
                try
                {
                    await Task.Delay(100);
                    lock (lockObject)
                    {
                        if (nextStepA != null && nextStepB != null)
                        {
                            PlayerA.Steps.Add(nextStepA.Value);
                            PlayerB.Steps.Add(nextStepB.Value);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }

        public async Task RunAsync()
        {
            try
            {
                await Task.Delay(2000);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"第{i}次调用NextStepAsync()");
                if (await NextStepAsync())
                {
                    Console.WriteLine("+++++++++++++++++++++++++++++++++");
                    Console.WriteLine(nextStepA);
                    Console.WriteLine(nextStepB);
                    Console.WriteLine("+++++++++++++++++++++++++++++++++++++");
                    Console.WriteLine($"第{i}次调用Judge()");
                    Judge();
                    if (status == "playing")
                    {
                        Console.WriteLine($"第{i}次调用SendMove()");
                        SendMove();
                    }
                    else
                    {
                        Console.WriteLine($"第{i}次调用SendResult()");
                        SendResult();
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("经过一段时间没输入，应该走到这");
                    status = "finished";
                    lock (lockObject)
                    {
                        if (nextStepA == null && nextStepB == null)
                        {
                            loser = "all";
                        }
                        else if (nextStepA == null)
                        {
                            loser = "A";
                        }
                        else
                        {
                            loser = "B";
                        }
                    }
                    try
                    {
                        SendResult();
                        Console.WriteLine("已经发送结束结果");
                        Console.WriteLine(loser);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SendResult()发生异常: {ex.Message}");
                        Console.WriteLine($"异常堆栈: {ex.StackTrace}");
                    }

                    break;
                }
            }
        }
    }
}