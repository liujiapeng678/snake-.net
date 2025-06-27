using Microsoft.AspNetCore.Mvc;
using SnakeMatchingSystem.Utils;

namespace SnakeMatchingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchingController : ControllerBase
    {
        private static MatchingPool _pool;

        public MatchingController(MatchingPool pool)
        {
            _pool = pool;
        }

        public class Player
        {
            public int UserId { get; set; }
            public int Rating { get; set; }
            public int BotId { get; set; }

        }

        [HttpPost("add")]
        public IActionResult AddPlayer([FromBody] Player player)
        {
            _pool.AddPlayer(player.UserId, player.Rating, player.BotId);
            return Ok();
        }


        [HttpPost("remove")]
        public IActionResult RemovePlayer([FromBody] Player player)
        {
            _pool.RemovePlayer(player.UserId);
            return Ok();
        }
    }
}
