using System.Net.Http.Json;

using Charging.Application.Models;

namespace Charging.IntegrationTests;

[Collection("GuidCollection")]
public class UsuariosIntegrationTests
{
    private readonly HttpClient _client;

    public UsuariosIntegrationTests(ApiFixture fixture)
    {
        _client = fixture._client;
    }

    [Fact]
    public async Task Post_DeveCriarUmNovoUsuario()
    {
        // Arrange
        Usuario novoUsuario = new() { Nome = "José Carlos", Email = $"jose-{Guid.NewGuid()}@email.com" };

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
        Usuario novoUsuario = new() { Nome = "José Carlos", Email = $"jose-{Guid.NewGuid()}@email.com" };
        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        Usuario? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<Usuario>();

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
        Usuario novoUsuario = new() { Nome = "José Carlos", Email = $"jose-{Guid.NewGuid()}@email.com" };
        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        Usuario? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<Usuario>();

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
        Usuario novoUsuario = new() { Nome = "José Carlos", Email = $"jose-{Guid.NewGuid()}@email.com" };
        HttpResponseMessage responsePost = await _client.PostAsJsonAsync("/api/usuarios", novoUsuario);
        Usuario? usuarioCriado = await responsePost.Content.ReadFromJsonAsync<Usuario>();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/usuarios/{usuarioCriado!.Id}");

        // Assert - verifica toda a resposta HTTP
        await Verify(response)
            .ScrubMember("dataCriacao")
            .ScrubMember("id")
            .ScrubMember("email");
    }
}