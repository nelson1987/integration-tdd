using System.Net;
using System.Net.Http.Json;
using Charging.Api.Data;
using Charging.Api.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Charging.IntegrationTests;

[Collection("GuidCollection")]
public class UnitTest1
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public UnitTest1(ApiFixture fixture)
    {
        _client = fixture._client;
        _context = fixture._context;
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestes()
    {
        await _context.Usuarios.AddAsync(new Usuario()
        { Email = $"email-{Guid.NewGuid()}@email.com", Nome = "Nome usuario", 
            DataCriacao = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteNaoExistir_DeveRetornarNotFound()
    {
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestesComResultado()
    {
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        var response = await result.Content
            .ReadFromJsonAsync<List<Usuario>>();
        response.ShouldNotBeEmpty();
        response.ShouldNotBeNull();
    }
}