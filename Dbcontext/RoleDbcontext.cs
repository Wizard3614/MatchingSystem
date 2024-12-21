using Microsoft.EntityFrameworkCore;
using MatchingSystem.Models;

namespace MatchingSystem.Dbcontext
{
    public class RoleDbContext : DbContext
    {
        public DbSet<Roles> Roles { get; set; }

        public RoleDbContext(DbContextOptions<RoleDbContext> options)
            : base(options)
        {
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

