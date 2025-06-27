using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SnakeServer.Models;
using SnakeServer.Utils;

namespace SnakeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IConfiguration _config;
        public BotController(IConfiguration config)
        {
            _config = config; // 注入配置以获取连接字符串
        }
        private class Bot
        {
            public required int Id { get; set; }
            public required string BotName { get; set; }
            public required string BotDescription { get; set; }
            public required string Code { get; set; }
            public required int Rating { get; set; }
        }
        private class BotRank
        {
            public required string BotName { get; set; }
            public required string BotOwner { get; set; }
            public required string BotOwnerPhoto { get; set; }
            public required int Rating { get; set; }
        }

        [HttpGet("getrank")] // 获取bot排行榜
        public IActionResult Getrank()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("MySQL"));
            conn.Open();

            var getrankSql = $"SELECT b.BotName, u.Username, u.Photo, b.Rating FROM Bots b INNER JOIN Users u ON b.UserId = u.Id WHERE b.Rating IS NOT NULL ORDER BY b.Rating DESC;";

            var bots = new List<BotRank>(); // 存储结果的列表

            using (var getCmd = new MySqlCommand(getrankSql, conn))
            {
                using var reader = getCmd.ExecuteReader();
                while (reader.Read()) // 遍历所有行
                {
                    bots.Add(new BotRank
                    {
                        BotName = reader.GetString("BotName"),
                        BotOwner = reader.GetString("Username"),
                        BotOwnerPhoto = reader.GetString("Photo"),
                        Rating = reader.GetInt32("Rating"),
                    });
                }
            }
            return Ok(bots);
        }

        [HttpGet("getlist/{id}")] // 获取bot列表
        public IActionResult Getlist(int id)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("MySQL"));
            conn.Open();

            var getlistSql = $"SELECT Id, BotName, BotDescription, Code, Rating FROM Bots WHERE UserId = {id}";

            var bots = new List<Bot>(); // 存储结果的列表

            using (var getlistCmd = new MySqlCommand(getlistSql, conn))
            {
                using var reader = getlistCmd.ExecuteReader();
                while (reader.Read()) // 遍历所有行
                {
                    bots.Add(new Bot
                    {
                        Id = reader.GetInt32("id"),
                        BotName = reader.GetString("BotName"),
                        BotDescription = reader.GetString("BotDescription"),
                        Code = reader.GetString("Code"),
                        Rating = reader.GetInt32("Rating"),
                    });
                }
            }
            return Ok(bots);
        }

        [HttpPost("add")]     //添加bot
        public IActionResult Add([FromBody] Bots bot)
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();

                var addBotSql = $"INSERT INTO Bots (UserId, BotName, BotDescription, Code, Rating) VALUES ({bot.UserId}, '{bot.BotName}', '{bot.BotDescription}', '{bot.Code}', {bot.Rating});";
                using (var addCmd = new MySqlCommand(addBotSql, conn))
                {
                    addCmd.ExecuteNonQuery();
                    return Ok();
                }

            }
        }
        [HttpPost("remove")]  //删除bot
        public IActionResult Remove([FromBody] Bots bot)
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();

                var removeBotSql = $"DELETE FROM Bots WHERE Id = {bot.Id}";
                using (var removeCmd = new MySqlCommand(removeBotSql, conn))
                {
                    removeCmd.ExecuteNonQuery();
                    return Ok();
                }
            }
        }
        [HttpPost("update")]  //更新bot
        public IActionResult Update([FromBody] Bots bot)
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();

                var updateBotSql = $"UPDATE Bots SET BotName = '{bot.BotName}', BotDescription = '{bot.BotDescription}', Code = '{bot.Code}' WHERE Id = {bot.Id};";
                using (var updateCmd = new MySqlCommand(updateBotSql, conn))
                {
                    updateCmd.ExecuteNonQuery();
                    return Ok();
                }
            }
        }
    }
}
