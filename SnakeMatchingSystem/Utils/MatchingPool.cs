using SnakeMatchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeMatchingSystem.Utils
{
    public class MatchingPool
    {
        private static readonly List<Player> _players = new List<Player>();
        private readonly object _lock = new object();
        private readonly HttpClient _httpClient;
        private Thread _matchingThread;

        public MatchingPool(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _matchingThread = new Thread(Run);
            _matchingThread.Start();
        }

        public void AddPlayer(int? userId, int? rating, int? botId)
        {
            lock (_lock)
            {
                Console.WriteLine(userId);
                _players.Add(new Player(userId, rating, botId, 0));
            }
        }

        public void RemovePlayer(int? userId)
        {
            lock (_lock)
            {
                var newPlayers = new List<Player>();
                foreach (var player in _players)
                {
                    if (!player.UserId.Equals(userId))
                    {
                        newPlayers.Add(player);
                    }
                }
                _players.Clear();
                _players.AddRange(newPlayers);
            }
        }

        private void IncreaseWaitingTime()
        {
            lock (_lock)
            {
                foreach (var player in _players)
                {
                    player.WaitingTime = (player.WaitingTime ?? 0) + 1;
                }
            }
        }

        private void MatchPlayers()
        {
            Console.WriteLine($"Players count: {_players.Count}");
            lock (_lock)
            {
                var used = new bool[_players.Count];
                for (int i = 0; i < _players.Count; i++)
                {
                    for (int j = i + 1; j < _players.Count; j++)
                    {
                        var playerA = _players[i];
                        var playerB = _players[j];
                        if (CheckMatched(playerA, playerB))
                        {
                            used[i] = used[j] = true;
                            SendResult(playerA, playerB);
                            break;
                        }
                    }
                }
                var newPlayers = new List<Player>();
                for (int i = 0; i < _players.Count; i++)
                {
                    if (!used[i])
                    {
                        newPlayers.Add(_players[i]);
                    }
                }
                _players.Clear();
                _players.AddRange(newPlayers);
            }
        }

        private bool CheckMatched(Player a, Player b)
        {
            int ratingDelta = Math.Abs((a.Rating ?? 0) - (b.Rating ?? 0));
            int waitingTime = Math.Min(a.WaitingTime ?? 0, b.WaitingTime ?? 0);
            return waitingTime * 10 >= ratingDelta;
        }

        private async void SendResult(Player a, Player b)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5081/api/game/start",
                new
                {
                    aId = a.UserId,
                    aBotId = a.BotId,
                    aRating = a.Rating,
                    bId = b.UserId,
                    bBotId = b.BotId,
                    bRating = b.Rating,
                }
            );
        }

        private void Run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                IncreaseWaitingTime();
                MatchPlayers();
            }
        }
    }
}