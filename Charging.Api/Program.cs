using Charging.Api.Data;
using Charging.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Criar o banco de dados automaticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao criar o banco de dados");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comentado para funcionar no Docker sem HTTPS
// app.UseHttpsRedirection();

app.MapGet("/api/usuarios/{id}",
        async ([FromRoute] int id, [FromServices] ApplicationDbContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id); // is { } usuario
            return usuario != null
                ? Results.Ok(usuario)
                : Results.NotFound();
        })
    .WithName("GetUsuarioById")
    .WithOpenApi();

app.MapGet("/api/usuarios",
        async (ApplicationDbContext db) => await db.Usuarios.ToListAsync())
    .WithName("GetUsuarios")
    .WithOpenApi();

app.MapPost("/api/usuarios", async (Usuario usuario, ApplicationDbContext db) =>
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
    })
    .WithName("CreateUsuario")
    .WithOpenApi();

await app.RunAsync();

public partial class Program
{
}