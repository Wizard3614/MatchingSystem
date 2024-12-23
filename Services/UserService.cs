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
        private readonly RoleDbContext _roleDbContext;
        private readonly IConnectionMultiplexer _redis;
        private readonly JwtService _jwtService;

        public UserService(UserDbContext ctx, IConnectionMultiplexer redis, JwtService jwtService, RoleDbContext roleDbContext)
        {
            _ctx = ctx;
            _roleDbContext = roleDbContext;
            _redis = redis;
            _jwtService = jwtService;
        }

        // 获取用户信息
        public async Task<(Object userDetails, List<string> roleList, string message, bool success)> GetUserInfoAsync(string accessToken, string code)
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


        // 分配角色
        public async Task<(bool success, string message)> AssignRolesToUserAsync(int adminUserId, int targetUserId, List<string> roleIds)
        {
            // 获取管理员用户
            var adminUser = await _ctx.Users.FindAsync(adminUserId);
            if (adminUser == null)
            {
                return (false, "Admin user can't be found");
            }

            // 获取目标用户
            var user = await _ctx.Users.FindAsync(targetUserId);
            if (user == null)
            {
                return (false, "Target user can't be found");
            }

            // 获取管理员角色列表
            var adminRoles = await _roleDbContext.Roles
                                                  .Where(r => adminUser.RoleList.Contains(r.Id))
                                                  .ToListAsync();

            // 检查管理员是否有分配角色的权限 ps:只能判定存在对应权限，若所需权限不存在，会输出：Cannot get the value of a token type 'Number' as a string.
            var hasPermissionToAssignRoles = adminRoles.Any(role => role.Permissions.Contains("AssignRoles"));
            if (!hasPermissionToAssignRoles)
            {
                return (false, "Admin does not have permission to assign roles.");
            }

            var existingRoles = await _roleDbContext.Roles
                                                    .Where(r => roleIds.Contains(r.Id))
                                                    .ToListAsync();

            if (existingRoles.Count != roleIds.Count)
            {
                return (false, "Some roles can't be found.");
            }

            var rolesToAdd = roleIds.Except(user.RoleList).ToList();

            if (!rolesToAdd.Any())
            {
                return (false, "No new roles to add.");
            }

            user.RoleList.AddRange(rolesToAdd);

            await _ctx.SaveChangesAsync();

            return (true, "Successfully added role");
        }



        // 获取用户的角色详细信息
        public async Task<(bool,object)> GetRolesForUserAsync(int userId)
        {
            var user = await _ctx.Users
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, "No user exits");
            }


            var roles = await _roleDbContext.Roles
                                     .Where(r => user.RoleList.Contains(r.Id))
                                     .ToListAsync();

            if (roles.Count == 0)
            {
                return (false, "No roles assigned to this user.");
            }

            return (true,roles);
        }
    }
}
