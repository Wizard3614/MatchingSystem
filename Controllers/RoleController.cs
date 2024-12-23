using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MatchingSystem.Dbcontext;
using MatchingSystem.Services;
using MatchingSystem.Models.Requests;
using MatchingSystem.Models.tables;
using Microsoft.AspNetCore.Identity.Data;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;         


namespace MatchingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // 创建新角色
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var (success, message) = await _roleService.CreateRoleAsync(request);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        //更新角色
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request body.");
            }

            try
            {
                var (success, message) = await _roleService.UpdateRoleAsync(request.RoleId, request.NewRoleName, request.NewPermissions);
                return Ok(new { success = true, message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);  // 如果角色不存在，返回 404
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //删除角色
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleRequest request)
        {
            var (success, message) = await _roleService.DeleteRoleAsync(request.RoleId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message });
        }

        //查看角色列表
        [HttpPost]
        [Route("getRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            if (roles == null || !roles.Any())
            {
                return NotFound("No roles found.");
            }

            return Ok(roles); // 返回角色列表
        }


    }


}