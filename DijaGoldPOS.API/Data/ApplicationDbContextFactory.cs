using DijaGoldPOS.API.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DijaGoldPOS.API.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext to support EF Core migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // Create a mock current user service for design-time
        var currentUserService = new MockCurrentUserService();

        return new ApplicationDbContext(optionsBuilder.Options, currentUserService);
    }
}

/// <summary>
/// Mock implementation of ICurrentUserService for design-time operations
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    public string UserId => "system";

    public string UserName => "System";

    public int? BranchId => null;

    public string? BranchName => null;
}
