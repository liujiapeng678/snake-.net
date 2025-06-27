using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using SnakeServer.Models;

namespace SnakeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly IConfiguration _config;
        public RecordController(IConfiguration config)
        {
            _config = config; 
        }
        private class Record
        {
            public required string AUsername { get; set; }
            public required string APhoto { get; set; }
            public required string BUsername { get; set; }
            public required string BPhoto { get; set; }
            public required string Winner { get; set; }
            public required DateTime Time { get; set; }
        }

        [HttpGet("getlist")] 
        public IActionResult Getlist()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("MySQL"));
            conn.Open();

            var getlistSql = @"
        SELECT 
            r.AId,
            r.BId,
            r.Loser,
            r.CreateTime,
            ua.Username AS AUsername,
            ua.Photo AS APhoto,
            ub.Username AS BUsername,
            ub.Photo AS BPhoto
        FROM Records r
        INNER JOIN Users ua ON r.AId = ua.Id
        INNER JOIN Users ub ON r.BId = ub.Id
        ORDER BY r.CreateTime DESC";

            var records = new List<Record>(); // 存储结果的列表

            using (var getlistCmd = new MySqlCommand(getlistSql, conn))
            {
                using var reader = getlistCmd.ExecuteReader();
                while (reader.Read()) // 遍历所有行
                {
                    string loser = reader.GetString("Loser");
                    string aUsername = reader.GetString("AUsername");
                    string bUsername = reader.GetString("BUsername");

                    string winner = loser switch
                    {
                        "A" => bUsername,  // A败，B胜
                        "B" => aUsername,  // B败，A胜
                        "all" => "平局",   // 平局
                        _ => "未知"
                    };

                    records.Add(new Record
                    {
                        AUsername = aUsername,
                        APhoto = reader.GetString("APhoto"),
                        BUsername = bUsername,
                        BPhoto = reader.GetString("BPhoto"),
                        Winner = winner,
                        Time = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(records);
            }
        }
    }
}
