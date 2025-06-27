using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SnakeServer.Utils
{
    
    public class JwtGenerator
    {
        public static string CreateToken(string username)
        {
            // 1. 设置密钥（至少16字符）
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("liujiapeng666dashuaige666liujiaming666kanhailaoshi666"));

            // 2. 创建签名凭证
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. 构建JWT
            var token = new JwtSecurityToken(
                issuer: "snake-server",
                audience: "snake-client",
                claims: new[] { new Claim(ClaimTypes.Name, username) },
                expires: DateTime.Now.AddHours(1),  // 有效期
                signingCredentials: creds);

            // 4. 返回字符串形式的Token
            return new JwtSecurityTokenHandler().WriteToken(token);
            //string token = JwtTokenGenerator.CreateToken(username);
            //return token;
        }
    }
}
