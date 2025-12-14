var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/teste/{id}", (int id) =>
    {
        return Results.Ok(new Usuario
        {
            Id = id,
            Nome = "João Silva",
            Email = "joao@email.com",
            DataCriacao = DateTime.Now
        });
    })
    .WithName("GetTesteById")
    .WithOpenApi();

app.MapGet($"/api/teste", () =>
    {
        return Results.Ok(new List<Usuario>
        {
            new() { Id = 1, Nome = "João Silva", Email = "joao@email.com", DataCriacao = DateTime.Now },
            new() { Id = 2, Nome = "Maria Santos", Email = "maria@email.com", DataCriacao = DateTime.Now }
        });
    })
    .WithName("GetTeste")
    .WithOpenApi();

await app.RunAsync();

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}

public partial class Program { }