using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace zen_demo_dotnet.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Use a default connection string for migrations
            var connectionString = "Host=localhost;Database=zen_demo;Username=postgres;Password=postgres";
            
            // Check if DATABASE_URL environment variable is set
            var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                // Convert from URL format to connection string if needed
                if (envConnectionString.StartsWith("postgres://"))
                {
                    // Parse the URL format
                    var uri = new Uri(envConnectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    var username = userInfo[0];
                    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
                    var host = uri.Host;
                    var port = uri.Port;
                    var database = uri.AbsolutePath.TrimStart('/');
                    var sslMode = uri.Query.Contains("sslmode=disable") ? "Disable" : "Prefer";

                    // Build the connection string
                    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode}";
                }
                else
                {
                    connectionString = envConnectionString;
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
