using Charging.Domain;
using Charging.Domain.Repositories;
using Charging.Infrastructure.Data;
using Charging.Infrastructure.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Charging.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        return services;
    }
}