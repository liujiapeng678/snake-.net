using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SnakeServer.Models;
using SnakeServer.Utils;

namespace SnakeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config; // 注入配置以获取连接字符串
        }
        
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users user)
        {
            // 1. 检查用户名是否已存在
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();

                // 检查用户名重复性
                var checkSql = $"SELECT COUNT(*) FROM Users WHERE Username = '{user.Username}'";
                using (var checkCmd = new MySqlCommand(checkSql, conn))
                {
                    var count = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        return Conflict(); // 直接返回错误消息
                    }
                }

                // 2. 无重复则插入新用户
                var insertSql = @$"
                    INSERT INTO Users (Username, Password, Photo) 
                    VALUES ('{user.Username}', '{user.Password}', '{user.Photo}')";

                using (var insertCmd = new MySqlCommand(insertSql, conn))
                {
                    insertCmd.ExecuteNonQuery();
                    return Ok(); // 返回成功消息
                }
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Users user)
        {
            // 1. 检查用户名是否已存在若存在直接返回错误消息 2. 检查用户名与明文存储的密码是否匹配若不匹配返回错误信息 3. 返回jwt和用户信息（string的photo）
            using (var conn = new MySqlConnection(_config.GetConnectionString("MySQL")))
            {
                conn.Open();

                // 1. 检查用户名是否存在
                var checkUserSql = $"SELECT COUNT(*) FROM Users WHERE Username = '{user.Username}'";
                using (var checkCmd = new MySqlCommand(checkUserSql, conn))
                {
                    var count = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        return Conflict();
                    }
                }

                // 2. 检查密码是否匹配（明文比较）
                var passwordCheckSql = $"SELECT Password, Photo, Id FROM Users WHERE Username = '{user.Username}'";
                using (var passwordCmd = new MySqlCommand(passwordCheckSql, conn))
                {
                    using (var reader = passwordCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 获取数据库中的密码和头像
                            var storedPassword = reader["Password"].ToString();
                            var userPhoto = reader["Photo"].ToString();
                            var id = reader["Id"].ToString();

                            // 比较密码（明文）
                            if (user.Password != storedPassword)
                            {
                                return Conflict();
                            }

                            // 3. 生成并返回JWT和用户信息
                            reader.Close(); // 必须先关闭reader才能继续操作连接
                            var token = JwtGenerator.CreateToken(user.Username);
                            return Ok(new
                            {
                                Id = id,
                                Token = token,
                                Photo = userPhoto
                            });
                        }
                        return Conflict();
                    }
                }
            }
        }
    }
}
