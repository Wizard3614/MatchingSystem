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

namespace MatchingSystem.Services
{
    public class AuthService
    {
        private readonly UserDbContext _ctx;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly JwtService _jwtService;

        public AuthService(UserDbContext ctx, IConfiguration configuration, IConnectionMultiplexer redis, JwtService jwtService)
        {
            _ctx = ctx;
            _configuration = configuration;
            _redis = redis;
            _jwtService = jwtService;
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

        // 用户注册
        public (bool success, string message) RegisterUser(RegisterUserRequest request)
        {
            if (_ctx.Users.Any(u => u.Code == request.Code))
            {
                return (false, "User code already exists.");
            }

            if (_ctx.Users.Any(u => u.Email == request.Email))
            {
                return (false, "User email already exists.");
            }

            if (_ctx.Users.Any(u => u.Username == request.Username))
            {
                return (false, "User name already exists.");
            }

            var newUser = new User
            {
                Code = request.Code,
                Email = request.Email,
                Username = request.Username,
                HashedPassword = request.HassedPassword
            };

            _ctx.Users.Add(newUser);
            _ctx.SaveChanges();

            return (true, "User registered successfully.");
        }

        // 用户登录并生成 JWT 令牌
        public async Task<(bool success, string message, string token)> LoginAsync(LoginUserRequest request)
        {
            var user = _ctx.Users.FirstOrDefault(u => u.Code == request.Identifier || u.Email == request.Identifier);
            if (user == null)
            {
                return (false, "Invalid code or email.", null);
            }

            var method = "POST";
            var path = "/api/authorize/login";
            var timestamp = request.Timestamp;
            var nonce = request.Nounce;
            var loginToken = request.Token;
            var sha1Password = user.HashedPassword;

            if (DateTimeOffset.TryParse(timestamp, out DateTimeOffset dateTime2))
            {
                DateTimeOffset dateTime1 = DateTimeOffset.UtcNow;
                TimeSpan difference = dateTime2 - dateTime1;

                if (difference > TimeSpan.FromMinutes(2))
                {
                    return (false, "Time out.", null);
                }
            }

            var signatureString = $"{method}\n{path}\n{timestamp}\n{nonce}\n{loginToken}\n{sha1Password}";
            var computedSignature = SHA.ComputeSha256Hash(signatureString);

            if (computedSignature != request.Signature)
            {
                return (false, "Invalid signature.", null);
            }

            var jwtToken = GenerateJwtToken(user);

            string key = $"user:{user.Code}";
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, jwtToken, TimeSpan.FromHours(0.15));

            return (true, "Login successful", jwtToken);
        }

        // 验证 JWT 是否有效
        public async Task<(bool success, string message)> ValidateLoginAsync(string tokenToValidate)
        {
            if (string.IsNullOrEmpty(tokenToValidate))
            {
                return (false, "Token is missing");
            }

            var jwtBody = _jwtService.ParseJwtToken(tokenToValidate);
            string key = $"user:{jwtBody.Code}";

            var db = _redis.GetDatabase() ;
            bool tokenExists = await db.KeyExistsAsync(key);
            if (!tokenExists)
            {
                return (false, "Token is invalid or expired");
            }

            return (true, "Token is valid");
        }

        // 用户登出
        public async Task<(bool success, string message)> LogoutAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return (false, "Access token is missing");
            }

            var jwtBody = _jwtService.ParseJwtToken(accessToken);
            string key = $"user:{jwtBody.Code}";

            var db = _redis.GetDatabase();
            bool isRemoved = await db.KeyDeleteAsync(key);
            if (!isRemoved)
            {
                return (false, "Failed to logout or token not found");
            }

            return (true, "Successfully logged out");
        }

        // 用户踢出
        public async Task<(bool success, string message)> KickUserAsync(string accessToken, string code)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return (false, "Access token is missing.");
            }

            var jwtBody = _jwtService.ParseJwtToken(accessToken);
            string key = $"user:{jwtBody.Code}";

            var db = _redis.GetDatabase();
            bool tokenExists = await db.KeyExistsAsync(key);
            if (!tokenExists)
            {
                return (false, "Invalid or expired access token.");
            }

            var targetUser = await _ctx.Users.FirstOrDefaultAsync(u => u.Code == code);
            if (targetUser == null)
            {
                return (false, "User not found.");
            }

            string key2 = $"user:{code}";
            bool tokenRemoved = await db.KeyDeleteAsync(key2);

            if (!tokenRemoved)
            {
                return (false, "Failed to invalidate the token.");
            }

            return (true, "User has been logged out successfully.");
        }

        // 忘记密码
        public async Task<(bool success, string message)> ForgotPasswordAsync(string accessToken, ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return (false, "Access token is missing.");
            }

            var jwtBody = _jwtService.ParseJwtToken(accessToken);
            string key = $"user:{jwtBody.Code}";

            var db = _redis.GetDatabase();
            bool tokenExists = await db.KeyExistsAsync(key);
            if (!tokenExists)
            {
                return (false, "Invalid or expired access token.");
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.VerificationCode) || string.IsNullOrEmpty(request.HashedPassword))
            {
                return (false, "Required fields are missing.");
            }

            /////////////
            //验证码处理/
            /////////////

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return (false, "Email mismatch.");
            }

            var hashedPassword = SHA.ComputeSha1Hash(request.HashedPassword);
            user.HashedPassword = hashedPassword;
            await _ctx.SaveChangesAsync();

            return (true, "Password successfully updated.");
        }

        // 生成 JWT
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.PostalCode, user.Code),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(0.15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
