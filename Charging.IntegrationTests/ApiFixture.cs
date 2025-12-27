using Charging.Api;
using Charging.Infrastructure.Data;

using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Testcontainers.MsSql;

namespace Charging.IntegrationTests;

public class ApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithPassword("yourStrong(!)Password123")
        .Build();

    public HttpClient _client = null!;
    public ApplicationDbContext _context = null!;
    private IServiceScope _scope = null!;

    public async Task ResetDatabaseAsync()
    {
        // Limpa todos os dados das tabelas
        _context.Usuarios.RemoveRange(_context.Usuarios);
        await _context.SaveChangesAsync();

        // Reseta a identidade da tabela para começar do 1
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Usuarios', RESEED, 0)");
    }

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
        ExecResult execResult = await _msSqlContainer.ExecAsync(new[]
        {
            "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "yourStrong(!)Password123", "-Q",
            "SELECT 1"
        });

        if (execResult.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"SQL Server is not ready. sqlcmd check failed with exit code {execResult.ExitCode}. Stderr: {execResult.Stderr}. Stdout: {execResult.Stdout}");
        }

        string? connectionString = _msSqlContainer.GetConnectionString();

        // On some Windows/Docker setups, localhost can be problematic.
        // Let's try forcing the IP address to see if it resolves the connection refusal.
        connectionString = connectionString.Replace("localhost", "127.0.0.1");

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using ApplicationDbContext dbContext = new(options);

        int retries = 3;
        while (retries > 0)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync();
                break; // Success
            }
            catch (SqlException ex)
            {
                retries--;
                if (retries == 0)
                {
                    throw new Exception("Database creation failed after multiple retries.", ex);
                }

                await Task.Delay(3000);
            }
        }

        WebApplicationFactory<Program> factory = new ApiFactory().WithWebHostBuilder(builder =>
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