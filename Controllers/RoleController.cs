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
        private readonly UserService _userService;

        public RoleController(IRoleService roleService,UserService userService)
        {
            _roleService = roleService;
            _userService = userService;
    }

        // 创建新角色
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromHeader] string access_token, [FromBody] CreateRoleRequest request)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (success, message) = await _roleService.CreateRoleAsync(request);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        //更新角色
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateRole([FromHeader] string access_token, [FromBody] UpdateRoleRequest request)
        {

            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

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
                return NotFound(ex.Message);  
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //删除角色
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteRole([FromHeader] string access_token, [FromBody] DeleteRoleRequest request)
        {

            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

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
        public async Task<IActionResult> GetRoles([FromHeader] string access_token)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var roles = await _roleService.GetRolesAsync();
            if (roles == null || !roles.Any())
            {
                return NotFound("No roles found.");
            }

            return Ok(roles); // 返回角色列表
        }

        //查看角色
        //获取角色通过 ID
        [HttpPost("getRoleById/{roleId}")]
        public async Task<IActionResult> GetRoleById([FromHeader] string access_token, string roleId)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var result = await _roleService.GetRoleByIdAsync(roleId);

            if (result.success)
            {
                return Ok(new { role = result.result });
            }

            return NotFound(new { message = result.result });
        }

        // 获取角色通过名称
        //[HttpPost("getRoleByName/{roleName}")]
        //public async Task<IActionResult> GetRoleByName(string roleName)
        //{
        //    var result = await _roleService.GetRoleByNameAsync(roleName);

        //    if (result.success)
        //    {
        //        return Ok(new { role = result.result });
        //    }

        //    return NotFound(new { message = result.result });
        //}

        //分配角色
        [HttpPost("{adminUserId}/assignroles/{userId}")]
        public async Task<IActionResult> AssignRolesToUser(int adminUserId, int userId, [FromBody] AssignrolesRequest request)
        {

            if (request.RoleIds == null || !request.RoleIds.Any())
            {
                return BadRequest(new { message = "Role IDs cannot be empty or null." });
            }

            var result = await _userService.AssignRolesToUserAsync(adminUserId, userId, request.RoleIds);

            if (result.success)
            {
                return Ok(new { message = result.message });
            }

            return BadRequest(new { message = "Failed to assign roles", error = result.message });
        }



    }


}