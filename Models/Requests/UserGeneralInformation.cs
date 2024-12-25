namespace MatchingSystem.Models.Requests
{
    public class UserGeneralInformation
    {
        public string Username { get; set; }
        public string Avator { get; set; }
        public string Gender { get; set; }
        public List<string> RoleList { get; set; } = new List<string>();
    }
}
