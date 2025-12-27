using Charging.Domain.Entities;

namespace Charging.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> FindAsync(int id);
    Task<List<Usuario>> ListAsync();
    Task AddAsync(Usuario usuario);
}