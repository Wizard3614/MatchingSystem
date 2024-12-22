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
using System;

namespace MatchingSystem.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleDbContext _dbContext;

        public RoleService(RoleDbContext dbContext)
        {
            _dbContext = dbContext;

        }



        public async Task<(bool success, string message)> CreateRoleAsync(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return (false, "Role ID cannot be empty.");
            }

            if (_dbContext.Roles.Any(u => u.Id == request.Id))
            {
                return (false, "role id already exists.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return (false, "Name cannot be empty ");
            }

            if (request.Permissions == null || !request.Permissions.Any())
            {
                return (false, "Permission cannot be empty");
            }

            var role = new Roles
            {
                Id = request.Id,
                Name = request.Name,
                Permissions = request.Permissions ?? new List<string>()
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            return (true, "Role created successfully");
        }


        public async Task<(bool success, string message)> UpdateRoleAsync(string roleId, string newRoleName, List<string> newPermissions)
        {
            var roledata = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);  // 通过 roleId 查找角色

            if (roledata == null)
            {
                throw new ArgumentException("Role not found."); // 如果找不到角色，抛出异常
            }

            // 更新角色的名称和权限
            roledata.Name = newRoleName;
            roledata.Permissions = newPermissions ?? new List<string>();  // 如果传入的权限为 null，则使用空列表

            // 保存更改到数据库
            _dbContext.Roles.Update(roledata);
            await _dbContext.SaveChangesAsync();

            return (true, "Role Updata successfully");
        }

        public async Task<(bool success, string message)> DeleteRoleAsync(string roleId)
        {
            // 查找角色
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);  // 通过 roleId 查找角色

            if (role == null)
            {
                return (true, "Role Delete fault"); // 如果找不到角色，返回 false 表示删除失败
            }

            // 删除角色
            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();  // 保存更改
            return (true, "Role Delete successfully"); //删除成功
        }

        // 查看所有角色
        public async Task<List<Roles>> GetRolesAsync()
        {
            return await _dbContext.Roles.ToListAsync();
        }






    }

}
