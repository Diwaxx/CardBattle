using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CardGame.Server.Data;

namespace CardGame.Server
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
            var connectionString = configuration.GetConnectionString("PostgreSQL");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'PostgreSQL' not found. " +
                    "Please check your appsettings.Development.json file.");
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new GameDbContext(optionsBuilder.Options);
        }
    }
}