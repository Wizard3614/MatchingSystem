namespace MatchingSystem.Models.Requests
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
        public string VerificationCode { get; set; }
        public string HashedPassword { get; set; }
    }
}
