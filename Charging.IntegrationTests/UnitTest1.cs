using System.Net;
using System.Net.Http.Json;
using Charging.Api.Data;
using Charging.Api.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;

// using VerifyTests;
// using VerifyXunit;

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
        { Email = "email@email.com", Nome = "Nome usuario", DataCriacao = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteNaoExistir_DeveRetornarNotFound()
    {
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestesComResultado()
    {
        var result = await _client.GetAsync("/api/usuarios");
        result.EnsureSuccessStatusCode();
        var response = await result.Content
            .ReadFromJsonAsync<List<Usuario>>();
        response.ShouldNotBeEmpty();
        response.Count.ShouldBe(2);
    }
}

// [UsesVerify]

/*
public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    public UnitTest1(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestes()
    {
        //Arrange - Criar entidade no banco de dados
        var entidadeTeste = new Teste("Teste");
        await entidadeRepository.AddAsync(entidadeTeste);
        //Act - Realizar o request
        var response = await client.GetAsync("/teste");
        response.EnsureSuccessStatusCode();
        //Arrange - Retornar o valor criado na base
        var content = await response.Content.ReadAsJson<IList<Teste>>();
        content.Should().Contains(entidadeTeste);
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteNaoExistir_DeveRetornarNotFound()
    {
        //Act - Realizar o request
        var response = await client.GetAsync("/teste");
        //Arrange - Retornar Status Code NotFound
        response.StatusCode.ShouldBe(NotFound);
    }

    [Fact]
    public async Task BuscarTeste_QuandoHouverTesteEmBase_DeveRetornarTestes()
    {
        //Arrange - Criar entidade no banco de dados
        var entidadeTeste = new Teste("Teste");
        await entidadeRepository.AddAsync(entidadeTeste);
        //Act - Realizar o request
        var response = await client.GetAsync($"/teste/{entidadeTeste.Id]");
        response.EnsureSuccessStatusCode();
        //Arrange - Retornar o valor criado na base
        var content = await response.Content.ReadAsJson<Teste>();
        content.Should().Be(entidadeTeste);
    }
}*/