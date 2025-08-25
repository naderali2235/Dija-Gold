using DijaGoldPOS.API.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Custom health check service for DijaGold POS system
/// </summary>
public class DijaGoldHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<DijaGoldHealthCheck> _logger;

    public DijaGoldHealthCheck(string connectionString, ILogger<DijaGoldHealthCheck> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting comprehensive health check");

            var healthChecks = new List<Task<HealthCheckResult>>
            {
                CheckDatabaseConnectionAsync(),
                CheckDatabaseMigrationsAsync(),
                CheckSystemResourcesAsync(),
                CheckBusinessLogicAsync()
            };

            var results = await Task.WhenAll(healthChecks);

            // If any check failed, return the first failure
            var failedCheck = results.FirstOrDefault(r => r.Status != HealthStatus.Healthy);
            if (failedCheck.Status != HealthStatus.Healthy)
            {
                _logger.LogWarning("Health check failed: {Description}", failedCheck.Description);
                return failedCheck;
            }

            var data = new Dictionary<string, object>
            {
                ["database"] = "Healthy",
                ["migrations"] = "Applied",
                ["system_resources"] = "OK",
                ["business_logic"] = "Operational",
                ["timestamp"] = DateTime.UtcNow
            };

            _logger.LogInformation("All health checks passed successfully");
            return HealthCheckResult.Healthy("All systems operational", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }

    private async Task<HealthCheckResult> CheckDatabaseConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();

            await connection.CloseAsync();

            _logger.LogDebug("Database connection check passed");
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }

    private async Task<HealthCheckResult> CheckDatabaseMigrationsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if migrations table exists and has recent migrations
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NOT NULL
                BEGIN
                    SELECT COUNT(*) FROM __EFMigrationsHistory
                END
                ELSE
                BEGIN
                    SELECT 0
                END";

            var result = await command.ExecuteScalarAsync();
            var migrationCount = result != null ? (int)result : 0;

            await connection.CloseAsync();

            if (migrationCount > 0)
            {
                _logger.LogDebug("Database migrations check passed: {Count} migrations applied", migrationCount);
                return HealthCheckResult.Healthy($"Database migrations applied: {migrationCount}");
            }
            else
            {
                _logger.LogWarning("No database migrations found");
                return HealthCheckResult.Degraded("No database migrations found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migrations check failed");
            return HealthCheckResult.Unhealthy("Database migrations check failed", ex);
        }
    }

    private async Task<HealthCheckResult> CheckSystemResourcesAsync()
    {
        try
        {
            await Task.Yield(); // Make this method truly async
            var issues = new List<string>();

            // Check available memory
            var totalMemory = GC.GetTotalMemory(false);
            if (totalMemory > 1_000_000_000) // 1GB
            {
                issues.Add($"High memory usage: {totalMemory:N0} bytes");
            }

            // Check processor count
            var processorCount = Environment.ProcessorCount;
            if (processorCount < 2)
            {
                issues.Add($"Low processor count: {processorCount}");
            }

            // Check disk space (simplified)
            var tempPath = Path.GetTempPath();
            var drive = new DriveInfo(Path.GetPathRoot(tempPath)!);
            if (drive.IsReady)
            {
                var availableSpaceGB = drive.AvailableFreeSpace / 1_000_000_000.0;
                if (availableSpaceGB < 1)
                {
                    issues.Add($"Low disk space: {availableSpaceGB:F2} GB available");
                }
            }

            if (issues.Any())
            {
                _logger.LogWarning("System resources check found issues: {Issues}", string.Join(", ", issues));
                return HealthCheckResult.Degraded($"System resource issues: {string.Join(", ", issues)}");
            }

            _logger.LogDebug("System resources check passed");
            return HealthCheckResult.Healthy("System resources OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources check failed");
            return HealthCheckResult.Unhealthy("System resources check failed", ex);
        }
    }

    private async Task<HealthCheckResult> CheckBusinessLogicAsync()
    {
        try
        {
            // Check if business-critical tables have data
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var checks = new Dictionary<string, string>
            {
                ["Users"] = "SELECT COUNT(*) FROM AspNetUsers",
                ["Products"] = "SELECT COUNT(*) FROM Products",
                ["Suppliers"] = "SELECT COUNT(*) FROM Suppliers",
                ["Branches"] = "SELECT COUNT(*) FROM Branches"
            };

            var issues = new List<string>();
            var data = new Dictionary<string, object>();

            foreach (var check in checks)
            {
                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = check.Value;
                    var countResult = await command.ExecuteScalarAsync();
                    var count = countResult != null ? (int)countResult : 0;
                    data[check.Key] = count;

                    // Warn if critical tables are empty
                    if (count == 0 && (check.Key == "Users" || check.Key == "Branches"))
                    {
                        issues.Add($"{check.Key} table is empty");
                    }
                }
                catch (Exception ex)
                {
                    issues.Add($"{check.Key} check failed: {ex.Message}");
                }
            }

            await connection.CloseAsync();

            if (issues.Any())
            {
                _logger.LogWarning("Business logic check found issues: {Issues}", string.Join(", ", issues));
                return HealthCheckResult.Degraded($"Business logic issues: {string.Join(", ", issues)}", null, data);
            }

            _logger.LogDebug("Business logic check passed");
            return HealthCheckResult.Healthy("Business logic operational", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Business logic check failed");
            return HealthCheckResult.Unhealthy("Business logic check failed", ex);
        }
    }
}
