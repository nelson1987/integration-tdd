using Charging.Application.Models;
using Charging.Domain;
using Charging.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Charging.Infrastructure;

public interface IDbContext
{
}

public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        return services;
    }
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext db;

    public UsuarioRepository(IDbContext context)
    {
        db = (ApplicationDbContext)context;
    }

    public async Task<Usuario?> FindAsync(int id)
    {
        return await db.Usuarios.FindAsync(id);
    }

    public async Task<List<Usuario>> ListAsync()
    {
        return await db.Usuarios.ToListAsync();
    }

    public async Task AddAsync(Usuario usuario)
    {
        await db.Usuarios.AddAsync(usuario);
        await db.SaveChangesAsync();
    }
}