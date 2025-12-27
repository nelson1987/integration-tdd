using Charging.Application.Models;

namespace Charging.Domain;

public interface IUsuarioRepository
{
    Task<Usuario?> FindAsync(int id);
    Task<List<Usuario>> ListAsync();
    Task AddAsync(Usuario usuario);
}