using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Context
{
    public class AppDbContextFactory :  IDesignTimeDbContextFactory<AppDbContext>
    {
        
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=fido;Password=test123");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}