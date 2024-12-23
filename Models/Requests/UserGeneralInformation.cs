namespace MatchingSystem.Models.Requests
{
    public class UserGeneralInformation
    {
        public string Username { get; set; }
        public string Avator { get; set; }
        public string Gender { get; set; }
        public List<int> RoleList { get; set; } = new List<int>();
    }
}
