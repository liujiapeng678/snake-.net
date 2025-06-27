using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using SnakeServer.Middleware;
using SnakeServer.Models;
using static Mysqlx.Crud.Order.Types;

namespace SnakeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GameController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }
        public class Players
        {
            public int aId { get; set; }
            public int bId { get; set; }
            public int aBotId { get; set; }
            public int bBotId { get; set; }
            public int aRating { get; set; }
            public int bRating { get; set; }

        }
        public class Bot
        {
            public int userId { get; set; }
            public int botId { get; set; }
            public int direction { get; set; }
        }

        [HttpPost("start")]
        public IActionResult Start([FromBody] Players players)
        {
            WebSocketMiddleware.StartGame(players.aId, players.bId, players.aBotId, players.bBotId, players.aRating, players.bRating, _config, _httpClient);
            return Ok();
        }

        [HttpPost("receive")]
        public IActionResult Receive([FromBody] Bot bot)
        {
            Console.WriteLine("receive bot move success");
            if (WebSocketMiddleware._connections[bot.userId] != null)
            {
                Game game = WebSocketMiddleware.game;
                if (game != null)
                {
                    if (game.PlayerA.Id == bot.userId)
                    {
                        game.SetNextStepA(bot.direction);
                    }
                    else if (game.PlayerB.Id == bot.userId)
                    {
                        game.SetNextStepB(bot.direction);
                    }
                }
            }
            return Ok();
        }
    }
}