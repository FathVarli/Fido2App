using Domain.Entity.Concrete;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
     
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        
        public DbSet<StoredCredentialDetail> Fido2StoredCredential { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=fido;Password=test123")
                .EnableSensitiveDataLogging();

            base.OnConfiguring(optionsBuilder);
        }
        
        
    }
}