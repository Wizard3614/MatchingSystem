namespace MatchingSystem.Models.Requests
{
    public class RegisterUserRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string HassedPassword { get; set; } = string.Empty;
    }
}
