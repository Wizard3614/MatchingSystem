
namespace MatchingSystem.Models.Requests
{
    public class UpdateRoleRequest
    {
        public string RoleId { get; set; }
        public string NewRoleName { get; set; }  // 新的角色名称
        public List<string> NewPermissions { get; set; }  // 新的权限列表
    }
}
