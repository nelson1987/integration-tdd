using Charging.Domain.Entities;
using Charging.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Charging.Infrastructure.Data.Implementations;

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