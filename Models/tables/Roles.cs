namespace MatchingSystem.Models.tables
{
    public class Roles
    {
        public string Id { get; set; } // 角色唯一标识
        public string Name { get; set; } // 角色名称
        public List<string> Permissions { get; set; }//角色职责
    }
}
