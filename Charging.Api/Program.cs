using Charging.Application;
using Charging.Domain;
using Charging.Domain.Entities;
using Charging.Domain.Repositories;
using Charging.Infrastructure;

using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Criar o banco de dados automaticamente
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<ApplicationDbContext>();
//     try
//     {
//         context.Database.EnsureCreated();
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Charging.Api.Program>>();
//         logger.LogError(ex, "Erro ao criar o banco de dados");
//     }
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comentado para funcionar no Docker sem HTTPS
// app.UseHttpsRedirection();

app.MapGet("/api/usuarios/{id}",
        async ([FromRoute] int id, [FromServices] IUsuarioRepository usuarioRepository) =>
        {
            //var query = new BuscaUsuarioQuery(id);
            Usuario? usuario = await usuarioRepository.FindAsync(id);
            return usuario != null
                ? Results.Ok(usuario)
                : Results.NotFound();
        })
    .WithName("GetUsuarioById")
    .WithOpenApi();

app.MapGet("/api/usuarios",
        async (IUsuarioRepository db) => await db.ListAsync())
    .WithName("GetUsuarios")
    .WithOpenApi();

app.MapPost("/api/usuarios", async (
        [FromBody] InclusaoUsuarioCommand command,
        [FromServices] IInclusaoUsuarioHandler db,
        CancellationToken cancellationToken) =>
    {
        var usuario = await db.Handle(command, cancellationToken);
        return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
    })
    .WithName("CreateUsuario")
    .WithOpenApi();

await app.RunAsync();

namespace Charging.Api
{
    public class Program
    {
    }
}