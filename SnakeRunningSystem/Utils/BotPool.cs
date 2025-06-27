
using SnakeRunningSystem.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace SnakeRunningSystem.Utils
{
    public class BotPool
    {
        private readonly object _lock = new object();
        private readonly Queue<Bot> _bots = new Queue<Bot>();
        private readonly Thread _workerThread;
        private readonly HttpClient _httpClient;
        private volatile bool _isRunning = true;

        public BotPool(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _workerThread = new Thread(Run)
            {
                IsBackground = true,
                Name = "BotPool-Worker"
            };
            _workerThread.Start();
        }

        public void AddBot(int userId, int botId, string botCode, string input)
        {
            lock (_lock)
            {
                _bots.Enqueue(new Bot(userId, botId, botCode, input));
                Monitor.PulseAll(_lock); // 唤醒等待的线程
            }
        }

        private void Consume(Bot bot)
        {
            var consumer = new Consumer(_httpClient);
            // 启动一个新线程来执行bot，带超时控制
            var consumerThread = new Thread(() => consumer.RunBotCode(bot))
            {
                IsBackground = true,
                Name = $"Consumer-{bot.userId}-{bot.botId}"
            };

            consumerThread.Start();

            // 等待最多2秒
            if (!consumerThread.Join(2000))
            {
                // 超时了，中断线程
                consumerThread.Interrupt();
                Console.WriteLine($"Bot {bot.botId} execution timed out");
            }
        }

        private void Run()
        {
            while (_isRunning)
            {
                Bot bot = null;

                lock (_lock)
                {
                    while (_bots.Count == 0 && _isRunning)
                    {
                        Monitor.Wait(_lock); // 等待新的bot被添加
                    }

                    if (_bots.Count > 0)
                    {
                        bot = _bots.Dequeue();
                    }
                }

                if (bot != null)
                {
                    try
                    {
                        Consume(bot);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing bot: {ex.Message}");
                    }
                }
            }
        }
    }
}