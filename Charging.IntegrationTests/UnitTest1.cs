using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Shouldly;

// using VerifyTests;
// using VerifyXunit;

namespace Charging.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(_ =>
        {
            // _.AddTransient<ICreateProductCommandHandler, CreateProductHandler>();
            // _.AddTransient<IProductRepository, ProductRepository>();
            // _.AddTransient<IDbContext, DbContext>();
            // var descriptorType = typeof(DbContextOptions<ApplicationDbContext>);
            //
            // var descriptor = Enumerable
            //     .SingleOrDefault<ServiceDescriptor>(services, s => s.ServiceType == descriptorType);
            //
            // if (descriptor is not null) services.Remove(descriptor);
            //
            // EntityFrameworkServiceCollectionExtensions.AddDbContext<ApplicationDbContext>(services, options =>
            //     SqlServerDbContextOptionsExtensions.UseSqlServer(options, _dbContainer.GetConnectionString()));
        });
    }
}

public class UnitTest1 : IClassFixture<ApiFactory>
{
    // private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UnitTest1(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestes()
    {
        var result = await _client.GetAsync("/api/teste");
        result.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task ListarTestes_QuandoTesteExistir_DeveRetornarListaTestesComResultado()
    {
        var result = await _client.GetAsync("/api/teste");
        result.EnsureSuccessStatusCode();
        var response = await result.Content
            .ReadFromJsonAsync<List<Usuario>>();
        response.ShouldNotBeEmpty();
        response.Count.ShouldBe(2);
    }
}

// public static class ModuleInitializer
// {
//     [ModuleInitializer]
//     public static void Init()
//     {
//         // Configurações globais do Verify
//         VerifyHttp.Enable();
//
//         // Serialização mais limpa
//         VerifierSettings.SortPropertiesAlphabetically();
//         VerifierSettings.SortJsonObjects();
//
//         // Remove propriedades dinâmicas globalmente
//         VerifierSettings.IgnoreMember<DateTime>();
//     }
// }

// [UsesVerify]
public class UsuariosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsuariosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_DeveRetornarUsuarioPorId()
    {
        // Act
        var response = await _client.GetAsync("/api/usuarios/1");
        var usuario = await response.Content.ReadFromJsonAsync<Usuario>();

        // Assert
        await Verify(usuario)
            .IgnoreMember<Usuario>(u => u.DataCriacao); // Ignora campo dinâmico
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaDeUsuarios()
    {
        // Act
        var response = await _client.GetAsync("/api/usuarios");
        //var usuarios = await response.Content.ReadFromJsonAsync<List<Usuario>>();

        // Assert
        await Verify(response)
            .IgnoreMember<Usuario>(u => u.DataCriacao);
    }

    [Fact]
    public async Task Get_DeveRetornarJsonCompleto()
    {
        // Act
        var response = await _client.GetAsync("/api/usuarios/1");
        var json = await response.Content.ReadAsStringAsync();

        // Assert - verifica o JSON bruto
        await VerifyJson(json)
            .ScrubMember("dataCriacao"); // Remove campo do snapshot
    }

    [Fact]
    public async Task Get_DeveRetornarStatusCodeCorreto()
    {
        // Act
        var response = await _client.GetAsync("/api/usuarios/1");

        // Assert - verifica toda a resposta HTTP
        await Verify(response)
            .ScrubMember("dataCriacao");
    }
}

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