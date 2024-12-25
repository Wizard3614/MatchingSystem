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
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
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
        //查询用户列表
        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers([FromHeader] string access_token, [FromQuery] int page = 1, [FromQuery] int page_size = 10)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }
            var (users,message,success) = await _userService.GetUsersAsync(access_token, page, page_size);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true,message, data = users });
        }
        //保存用户信息（个人中心，增和改）
        [HttpPost("saveUser")]
        public async Task<IActionResult> SaveUser([FromHeader] string access_token, [FromBody] User request)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (message,success) = await _userService.SaveUserInfoAsync(request);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        //保存用户信息（管理员）
        [HttpPost("saveUserAdmin")]
        public async Task<IActionResult> SaveUserAdmin([FromHeader] string access_token, [FromBody] User request)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (message, success) = await _userService.SaveUserInfoAdminAsync(request);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        //删除用户
        [HttpPut("delete/{userCode}")]
        public async Task<IActionResult> DeleteUser([FromHeader] string access_token, string userCode)
        {
            var (flag, msg) = await _userService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (message, success) = await _userService.DeleteUserAsync(userCode);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
    }
}
