using System.Runtime.CompilerServices;
using Charging.Api.Models;

namespace Charging.IntegrationTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Configurações globais do Verify
        // VerifyHttp.Enable();

        // Serialização mais limpa
        VerifierSettings.SortPropertiesAlphabetically();
        VerifierSettings.SortJsonObjects();

        // Remove propriedades dinâmicas globalmente
        VerifierSettings.IgnoreMember<Usuario>(u => u.DataCriacao);
    }
}