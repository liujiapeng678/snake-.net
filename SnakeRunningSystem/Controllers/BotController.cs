using Microsoft.AspNetCore.Mvc;
using SnakeRunningSystem.Utils;

namespace SnakeRunningSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private static BotPool _pool;

        public BotController(BotPool pool)
        {
            _pool = pool;
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] Bot bot)
        {
            _pool.AddBot(bot.userId, bot.botId, bot.botCode, bot.input);
            return Ok();
        }
    }
}