using Microsoft.AspNetCore.Mvc;
using MatchingSystem.Models.Requests;
using MatchingSystem.Services;

namespace MatchingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 用户注册接口
        /// </summary>
        /// <param name="request">用户注册请求数据</param>
        /// <returns>注册成功与否的结果</returns>
        [HttpPost("register")]
        public IActionResult RegisterUser([FromBody] RegisterUserRequest request)
        {

            var (success, message) = _authService.RegisterUser(request);

            if (!success)
            {
                // 如果注册失败，返回 Conflict 状态码
                return Conflict(new { success = false, message });
            }

            // 如果注册成功，返回 OK 状态码
            return Ok(new { success = true, message });
        }

        /// <summary>
        /// 用户登录接口，返回JWT令牌
        /// </summary>
        /// <param name="request">登录请求数据</param>
        /// <returns>JWT令牌</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            var (success, message, token) = await _authService.LoginAsync(request);

            if (!success)
            {
                return Unauthorized(new { success, message });
            }

            return Ok(new { success, message, token });
        }

        // 用户登出
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader] string access_token)
        {

            var (flag, msg) = await _authService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (success, message) = await _authService.LogoutAsync(access_token);

            if (!success)
            {
                return Unauthorized(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        [HttpPost("validate-login")]
        public async Task<IActionResult> ValidateLogin([FromHeader] string access_token, [FromBody] Validate_login request)
        {

            var (flag, msg) = await _authService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            string tokenToValidate = request.Token ?? access_token;

            var (success, message) = await _authService.ValidateLoginAsync(tokenToValidate);

            if (!success)
            {
                return Ok(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        // 用户踢出
        [HttpPost("kick-user")]
        public async Task<IActionResult> KickUser([FromHeader] string access_token, [FromBody] string code)
        {

            var (flag, msg) = await _authService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (success, message) = await _authService.KickUserAsync(access_token, code);

            if (!success)
            {
                return Unauthorized(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        // 忘记密码
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromHeader] string access_token, [FromBody] ForgotPasswordRequest request)
        {
            var (flag, msg) = await _authService.TokenExistsAsync(access_token);
            if (!flag)
            {
                return BadRequest(new { success = false, msg });
            }

            var (success, message) = await _authService.ForgotPasswordAsync(access_token, request);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
    }
}
