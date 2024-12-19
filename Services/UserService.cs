using Microsoft.Extensions.Configuration;
using MatchingSystem.Dbcontext;
using MatchingSystem.Models;
using MatchingSystem.Models.Requests;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MatchingSystem.Models.tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Reflection.Metadata.Ecma335;

namespace MatchingSystem.Services
{
    public class UserService
    {
        private readonly UserDbContext _ctx;
        private readonly IConnectionMultiplexer _redis;
        private readonly JwtService _jwtService;

        public UserService(UserDbContext ctx, IConnectionMultiplexer redis, JwtService jwtService)
        {
            _ctx = ctx;
            _redis = redis;
            _jwtService = jwtService;
        }

        // 获取用户信息
        public async Task<(Object userDetails, List<int> roleList, string message, bool success)> GetUserInfoAsync(string accessToken, string code)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return (null, null, "Access token is missing.", false);
            }

            // 查找用户
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Code == code);
            if (user == null)
            {
                return (null, null, "User not found.", false);
            }

            // 获取角色 ID 列表
            var roleListIds = user.RoleList;

            // 查询角色名称列表//

            // 构造用户详细信息
            var userDetails = new UserDetails
            {
                Username = user.Username,
                Email = user.Email,
                Realname = user.RealName,
                MobilePhone = user.MobilePhone,
                Code = user.Code,
                Avator = user.Avatar,
                Gender = user.Gender,
                Birthday = user.Birthday
            };

            return (userDetails, roleListIds, "User information retrieved successfully.", true);
        }


        public async Task<(bool success, string message)> TokenExistsAsync(string token)
        {

            if (string.IsNullOrEmpty(token))
            {
                return (false, "Token is missing");
            }

            var jwtBody = _jwtService.ParseJwtToken(token);
            string key = $"user:{jwtBody.Code}";

            var db = _redis.GetDatabase();
            bool tokenExists = await db.KeyExistsAsync(key);
            if (!tokenExists)
            {
                return (false, "Token is invalid or expired");
            }

            return (true, "");
        }
    }
}
