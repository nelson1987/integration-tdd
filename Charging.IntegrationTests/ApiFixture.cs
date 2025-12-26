using System;
using System.Threading.Tasks;
using Charging.Api.Data;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace Charging.IntegrationTests;

public class ApiFixture : IAsyncLifetime
{
    public HttpClient _client = null!;
    public ApplicationDbContext _context = null!;
    private IServiceScope _scope = null!;

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithPassword("yourStrong(!)Password123")
        .Build();

    public async Task InitializeAsync()
    {
        try
        {
            await _msSqlContainer.StartAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to start the MS SQL Server container. Please ensure Docker is running and properly configured. Inner exception: " +
                ex.Message,
                ex);
        }

        // Additional health check using sqlcmd inside the container
        var execResult = await _msSqlContainer.ExecAsync(new[]
        {
            "/opt/mssql-tools/bin/sqlcmd",
            "-S", "localhost",
            "-U", "sa",
            "-P", "yourStrong(!)Password123",
            "-Q", "SELECT 1"
        });

        if (execResult.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"SQL Server is not ready. sqlcmd check failed with exit code {execResult.ExitCode}. Stderr: {execResult.Stderr}. Stdout: {execResult.Stdout}");
        }

        var connectionString = _msSqlContainer.GetConnectionString();

        // On some Windows/Docker setups, localhost can be problematic.
        // Let's try forcing the IP address to see if it resolves the connection refusal.
        connectionString = connectionString.Replace("localhost", "127.0.0.1");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var dbContext = new ApplicationDbContext(options);

        var retries = 3;
        while (retries > 0)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                break; // Success
            }
            catch (SqlException ex)
            {
                retries--;
                if (retries == 0) throw new Exception("Migration failed after multiple retries.", ex);
                await Task.Delay(3000);
            }
        }

        var factory = new ApiFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(connectionString));
            });
        });
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        await _msSqlContainer.StopAsync();
    }
}