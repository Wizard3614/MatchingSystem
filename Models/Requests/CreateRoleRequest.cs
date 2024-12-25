namespace MatchingSystem.Models.Requests
{
    public class CreateRoleRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; }
        public List<string> Permissions { get; set; } // 角色拥有的权限
    }
}
