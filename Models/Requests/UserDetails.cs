namespace MatchingSystem.Models.Requests
{
    public class UserDetails
    {
        public string Username { get; set; } 
        public string Email { get; set; }     
        public string Realname { get; set; } 
        public string MobilePhone { get; set; }
        public string Code { get; set; }      
        public string Avator { get; set; }   
        public string Gender { get; set; }   
        public DateTime? Birthday { get; set; }
    }
}
