using MatchingSystem.Models.tables;
using Microsoft.EntityFrameworkCore;

namespace MatchingSystem.Dbcontext
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
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
