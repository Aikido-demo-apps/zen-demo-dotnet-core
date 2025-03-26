using Microsoft.EntityFrameworkCore;
using zen_demo_dotnet.Models;

namespace zen_demo_dotnet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Pet entity
            modelBuilder.Entity<Pet>()
                .HasKey(p => p.Id);
            
            modelBuilder.Entity<Pet>()
                .Property(p => p.Name)
                .IsRequired();
        }
    }
}
