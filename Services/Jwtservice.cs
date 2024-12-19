using StackExchange.Redis;
using MatchingSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MatchingSystem.Services
{
    public class JwtService
    {
        private readonly IConnectionMultiplexer _redis;

        public JwtService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// 解析 JWT Token 并返回一个 JwtBody 对象
        /// </summary>
        /// <param name="jwtToken">JWT Token 字符串</param>
        /// <returns>解析后的 JwtBody 对象</returns>
        public JwtBody ParseJwtToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var tokenS = handler.ReadJwtToken(jwtToken);

                var userIdClaim = tokenS.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var codeClaim = tokenS.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PostalCode)?.Value;
                var expirationClaim = tokenS.ValidTo;

                var jwtBody = new JwtBody
                {
                    UserId = userIdClaim,
                    Code = codeClaim,
                    Expiration = expirationClaim
                };

                return jwtBody;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid JWT token", ex);
            }
        }

        /// <summary>
        /// 验证 JWT Token 是否在 Redis 中存在
        /// </summary>
        /// <param name="accessToken">要验证的 access_token</param>
        /// <returns>如果存在返回 true, 否则返回 false</returns>
        public async Task<bool> ValidateTokenExistence(string accessToken)
        {
            try
            {
                var jwtBody = ParseJwtToken(accessToken);
                string key = $"user:{jwtBody.Code}:token"; 

                var db = _redis.GetDatabase();
                bool tokenExists = await db.KeyExistsAsync(key);  

                return tokenExists;
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}
