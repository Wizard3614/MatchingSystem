namespace MatchingSystem.Models.tables
{
    public class Roles
    {
        public string Id { get; set; } // 角色唯一标识
        public string Name { get; set; } = string.Empty;// 角色名称
        public List<string> Permissions { get; set; } = new List<string>();//角色职责
    }
}
