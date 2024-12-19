namespace MatchingSystem.Models.Requests
{
    public class LoginUserRequest
    {
        public string Identifier { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Nounce {  get; set; } = string.Empty;

        public string Timestamp {  get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;
    }
}
