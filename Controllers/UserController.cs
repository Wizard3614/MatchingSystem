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

    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // 获取用户信息
        [HttpPost("getuserinfo")]
        public async Task<IActionResult> GetUserInfo([FromHeader] string access_token, [FromBody] string Code)
        {
            var (flag,msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (userDetails, roleList, message, success) = await _userService.GetUserInfoAsync(access_token, Code);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }


            var response = new
            {
                userDetails,
                roleList
            };

            return Ok(new { success = true, message, data = response });
        }


        //方便获取user属性
        // 分配角色给用户
        [HttpPost("{adminUserId}/assignroles/{userId}")]
        public async Task<IActionResult> AssignRolesToUser(int adminUserId, int userId, [FromBody] AssignrolesRequest request)
        {
            var result = await _userService.AssignRolesToUserAsync(adminUserId, userId, request.RoleIds);
            if (result.success)
            {
                return Ok(new { message = result.message }); 
            }

            return BadRequest(new { message = "false", error = result });
        }


        //获取用户角色和职责
        [HttpPost("getuserrole")]
        public async Task<IActionResult> GetRolesForUser(int userId)
        {
            var (success, result) = await _userService.GetRolesForUserAsync(userId);

            if (success)
            {
                return Ok(result);
            }
            return BadRequest(new { message = "false", error = result }); 
        }

    }
}
