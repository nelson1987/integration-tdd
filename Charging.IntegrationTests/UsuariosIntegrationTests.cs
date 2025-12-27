using System.Net.Http.Json;

using Charging.Application;
using Charging.Domain.Entities;

namespace Charging.IntegrationTests;

[Collection("GuidCollection")]
public class UsuariosIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly ApiFixture _fixture;

    public UsuariosIntegrationTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture._client;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Post_DeveCriarUmNovoUsuario()
    {
        // Arrange
        InclusaoUsuarioCommand novoUsuario = new(Nome: "José Carlos", Email: $"jose-{Guid.NewGuid()}@email.com");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        Usuario? usuarioCriado = await response.Content.ReadFromJsonAsync<Usuario>();

        // Assert
        await Verify(usuarioCriado)
            .IgnoreMember<Usuario>(u => u.Id)
            .IgnoreMember<Usuario>(u => u.DataCriacao)
            .IgnoreMember<Usuario>(u => u.Email);
    }

    [Fact]
    public async Task Get_DeveRetornarUsuarioPorId()
    {
        // Arrange
        InclusaoUsuarioCommand novoUsuario =
            new InclusaoUsuarioCommand("José Carlos", $"jose-{Guid.NewGuid()}@email.com");

        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        InclusaoUsuarioResponse? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<InclusaoUsuarioResponse>();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/usuarios/{usuarioCriado!.Id}");
        Usuario? usuario = await response.Content.ReadFromJsonAsync<Usuario>();

        // Assert
        await Verify(usuario)
            .IgnoreMember<Usuario>(u => u.DataCriacao)
            .IgnoreMember<Usuario>(u => u.Email); // Ignora campo dinâmico
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaDeUsuarios()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        Usuario novoUsuario1 = new() { Nome = "José Carlos", Email = $"jose-{guid}@email.com" };
        Usuario novoUsuario2 = new() { Nome = "Maria Eduarda", Email = $"maria-{guid}@email.com" };
        await _client.PostAsJsonAsync("/api/usuarios", novoUsuario1);
        await _client.PostAsJsonAsync("/api/usuarios", novoUsuario2);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/usuarios");
        List<Usuario>? usuarios = await response.Content.ReadFromJsonAsync<List<Usuario>>();

        // Assert
        await Verify(usuarios)
            .IgnoreMember<Usuario>(u => u.DataCriacao)
            .IgnoreMember<Usuario>(u => u.Id)
            .IgnoreMember<Usuario>(u => u.Email);
    }

    [Fact]
    public async Task Get_DeveRetornarJsonCompleto()
    {
        // Arrange
        InclusaoUsuarioCommand novoUsuario = new("José Carlos", Email: $"jose-{Guid.NewGuid()}@email.com");
        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        InclusaoUsuarioResponse? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<InclusaoUsuarioResponse>();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/usuarios/{usuarioCriado!.Id}");
        string json = await response.Content.ReadAsStringAsync();

        // Assert - verifica o JSON bruto
        await VerifyJson(json)
            .ScrubMember("dataCriacao") // Remove campo do snapshot
            .ScrubMember("id")
            .ScrubMember("email");
    }

    [Fact]
    public async Task Get_DeveRetornarStatusCodeCorreto()
    {
        // Arrange
        InclusaoUsuarioCommand novoUsuario = new(Nome: "José Carlos", Email: $"jose-{Guid.NewGuid()}@email.com");
        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        InclusaoUsuarioResponse? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<InclusaoUsuarioResponse>();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/usuarios/{usuarioCriado!.Id}");

        // Assert - verifica toda a resposta HTTP
        await Verify(response)
            .ScrubMember("dataCriacao")
            .ScrubMember("id")
            .ScrubMember("email");
    }
}