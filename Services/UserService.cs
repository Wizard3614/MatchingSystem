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
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        //查询用户列表
        public async Task<(Object userDetails,string message,bool success)> GetUsersAsync(string access_token,int page, int pageSize)
        {
            var users = await _ctx.Users
            .Where(u => !u.IsDeleted)  // 不查询已删除的用户
                .Skip((page - 1) * pageSize)  // 计算跳过的记录数
                .Take(pageSize)  // 获取当前页的数据
                .Select(u => new UserGeneralInformation
                {
                    Username = u.Username,
                    Avator = u.Avatar,
                    Gender = u.Gender,
                    RoleList = u.RoleList
                })
                .ToListAsync();

            return (users, "User information retrieved successfully.",true);
        }
        // 保存用户信息（个人中心）
        public async Task<(string message,bool success)> SaveUserInfoAsync(User request)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Code == request.Code);
            if (user == null)
            {
                return ("User not found.",false);
            }

            user.Email = request.Email;
            user.Username = request.Username;
            user.HashedPassword = request.HashedPassword;
            user.Avatar = request.Avatar;
            user.MobilePhone = request.MobilePhone;
            await _ctx.SaveChangesAsync();

            return ("User info saved successfully.",true);
        }
        // 保存用户信息（管理员）
        public async Task<(string message, bool success)> SaveUserInfoAdminAsync(User request)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Code == request.Code);
            if (user == null)
            {
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    HashedPassword = request.HashedPassword,
                    Avatar = request.Avatar,
                    MobilePhone = request.MobilePhone,
                    Code = request.Code, // 新增用户时需要有 Code
                    Gender = request.Gender,
                    Birthday = request.Birthday,
                    Salt = request.Salt,
                    RoleList = request.RoleList, // 如果有角色信息也保存
                    IsDeleted = false // 新用户默认未删除
                };

                _ctx.Users.Add(newUser);
                await _ctx.SaveChangesAsync();

                return ("User created successfully.", true);
            }
            else
            {
                user.Username = request.Username;
                user.Email = request.Email;
                user.RealName = request.RealName;
                user.Avatar = request.Avatar;
                user.MobilePhone = request.MobilePhone;
                user.Gender = request.Gender;
                user.Birthday = request.Birthday;
                user.RoleList = request.RoleList; 
                user.IsDeleted = request.IsDeleted; 
                await _ctx.SaveChangesAsync();

                return ("User info saved successfully.", true);
            }
        }
        //删除用户
        public async Task<(string message, bool success)> DeleteUserAsync(string userCode)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Code == userCode);
            if (user == null)
            {
                return ("User not found.", false);
            }

            user.IsDeleted = true;
            await _ctx.SaveChangesAsync();

            return ( "User deleted successfully.",true);
        }
        //Token验证
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
