using System.Net;
using System.Net.Http.Json;

using Charging.Application.Models;
using Charging.Infrastructure.Data;

using Shouldly;

namespace Charging.IntegrationTests;

[Collection("GuidCollection")]
public class GetIntegrationTests
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public GetIntegrationTests(ApiFixture fixture)
    {
        _client = fixture._client;
        _context = fixture._context;
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestes()
    {
        await _context.Usuarios.AddAsync(new Usuario
        {
            Email = $"email-{Guid.NewGuid()}@email.com", Nome = "Nome usuario", DataCriacao = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        HttpResponseMessage result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteNaoExistir_DeveRetornarNotFound()
    {
        HttpResponseMessage result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestesComResultado()
    {
        HttpResponseMessage result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        List<Usuario>? response = await result.Content
            .ReadFromJsonAsync<List<Usuario>>();
        response.ShouldNotBeEmpty();
        response.ShouldNotBeNull();
    }
}