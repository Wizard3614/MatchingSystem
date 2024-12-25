using Microsoft.EntityFrameworkCore;
using MatchingSystem.Models.tables;

namespace MatchingSystem.Dbcontext
{
    public class RoleDbContext : DbContext
    {
        public DbSet<Roles> Roles { get; set; }

        public RoleDbContext(DbContextOptions<RoleDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 设置 Permissions 列，使用 ValueConverter 将 List<string> 转换为逗号分隔的字符串
            modelBuilder.Entity<Roles>()
                .Property(r => r.Permissions)
                .HasConversion(
                    v => string.Join(',', v),  // 将 List<string> 转换为字符串（逗号分隔）
                    v => v.Split(',', StringSplitOptions.None).ToList()  // 将字符串转换回 List<string>
                );


        }

        public void EnsureCreated()
        {
            if (!Database.CanConnect())
            {
                Database.EnsureCreated();
            }
        }
    }
}

