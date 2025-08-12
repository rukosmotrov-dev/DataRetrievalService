using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataRetrievalService.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                     ?? "Server=(localdb)\\MSSQLLocalDB;Database=DataRetrievalService;Trusted_Connection=True;TrustServerCertificate=True;";

            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs);

            return new AppDbContext(builder.Options);
        }
    }
}
