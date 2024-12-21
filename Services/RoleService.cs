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
            var role = new Roles
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            return (true, "Role created successfully");
        }

        // 其他方法如获取角色、删除角色等
    }

}
