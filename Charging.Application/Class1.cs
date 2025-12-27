using Charging.Domain.Entities;
using Charging.Domain.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace Charging.Application;

public record ListagemUsuarioQuery();

public record BuscaUsuarioQuery(int Id);

public record InclusaoUsuarioCommand(string Nome, string Email);
public record InclusaoUsuarioResponse(int Id, string Nome, string Email, DateTime DataCriacao);

public interface IInclusaoUsuarioHandler
{
    Task<InclusaoUsuarioResponse> Handle(InclusaoUsuarioCommand command, CancellationToken cancellationToken);
}

public class InclusaoUsuarioHandler(IUsuarioRepository db) : IInclusaoUsuarioHandler
{
    public async Task<InclusaoUsuarioResponse> Handle(InclusaoUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = new Usuario { Nome = command.Nome, Email = command.Email, DataCriacao = DateTime.UtcNow };
        await db.AddAsync(usuario);
        return new InclusaoUsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.DataCriacao);
    }
}

public static class Dependencies
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IInclusaoUsuarioHandler, InclusaoUsuarioHandler>();
        return services;
    }
}