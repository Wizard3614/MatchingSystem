﻿using Microsoft.Extensions.Configuration;
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
    public interface IRoleService
    {
        //创建角色
        Task<(bool success, string message)> CreateRoleAsync(CreateRoleRequest request);
        // 添加其他接口方法签名

        //更新角色
        Task<(bool success, string message)> UpdateRoleAsync(string roleId, string newRoleName, List<string> newPermissions);

        //删除角色
        Task<(bool success, string message)> DeleteRoleAsync(string roleId);

        ////查看角色列表
        Task<List<Roles>> GetRolesAsync();

        // 获取角色通过 ID
        Task<(bool success, object result)> GetRoleByIdAsync(string roleId);

        // 获取角色通过名称
        //Task<(bool success, object result)> GetRoleByNameAsync(string roleName);
    }
}
