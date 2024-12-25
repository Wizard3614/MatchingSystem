using MatchingSystem.Models.tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MatchingSystem.Dbcontext
{
    public class MDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Roles> Roles { get; set; }

        public MDbContext(DbContextOptions<MDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Roles>()
                .Property(r => r.Permissions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.None).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), 
                    c => c.ToList()
                ));

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_User_Code_Numeric", "Code GLOB '[0-9]*'");
                });

                entity.Property(u => u.Code)
                    .IsRequired()
                    .HasMaxLength(20);
            });
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
