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
            //Console.WriteLine("将要加入bot");
            try
            {
                Console.WriteLine("将要加入bot");
                _pool.AddBot(bot.userId, bot.botId, bot.botCode, bot.input);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加入bot时出错: {ex.Message}");
                return BadRequest(ex.Message);
            }
            //_pool.AddBot(bot.userId, bot.botId, bot.botCode, bot.input);
            //return Ok();
        }
    }
}