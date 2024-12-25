namespace MatchingSystem.Models.tables
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string MobilePhone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; } = null;
        public string HashedPassword { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public string Salt { get; set; } = string.Empty;
        public List<string> RoleList { get; set; } = new List<string>();
        public User()
        {
        }

        public User(
            string username,
            string email,
            string realName,
            string mobilePhone,
            string code,
            string avatar,
            string gender,
            DateTime? birthday,
            string hashedPassword,
            bool isDeleted,
            string salt,
            List<string> roleList)
        {
            (Username, Email, RealName, MobilePhone, Code, Avatar, Gender, Birthday, HashedPassword, IsDeleted, Salt, RoleList) =
                (username, email, realName, mobilePhone, code, avatar, gender, birthday, hashedPassword, isDeleted, salt, roleList);
        }

        public override string ToString()
        {
            return $"UserId: {UserId}, Username: {Username}, Email: {Email}, RealName: {RealName}, MobilePhone: {MobilePhone}, Code: {Code}, Avatar: {Avatar}, Gender: {Gender}, Birthday: {Birthday?.ToString("yyyy-MM-dd")}, IsDeleted: {IsDeleted}";
        }
    }
}
