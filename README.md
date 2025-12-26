# Charging API - Projeto de Integração com TDD

## 📋 Sobre o Projeto

Este é um projeto template em .NET 8.0 que demonstra a implementação de uma API RESTful com foco em **Testes de Integração** utilizando **Test-Driven Development (TDD)**. O projeto implementa um CRUD básico de usuários com persistência em SQL Server.

## 🏗️ Arquitetura

O projeto está dividido em duas partes principais:

### 1. **Charging.Api** - API Principal
- API RESTful implementada com **ASP.NET Core 8.0**
- Utiliza **Minimal APIs** para definição de endpoints
- **Entity Framework Core 7.0** para acesso a dados
- **SQL Server** como banco de dados
- **Swagger/OpenAPI** para documentação

### 2. **Charging.IntegrationTests** - Testes de Integração
- Framework de testes: **xUnit**
- **TestContainers** para isolamento de banco de dados em testes
- **Verify** para snapshot testing
- **Shouldly** para assertions mais legíveis
- **WebApplicationFactory** para testes de integração da API

## 🛠️ Tecnologias Utilizadas

### API
- .NET 8.0
- ASP.NET Core 8.0
- Entity Framework Core 7.0.19
- SQL Server
- Swagger/OpenAPI

### Testes
- xUnit 2.9.3
- Microsoft.AspNetCore.Mvc.Testing 8.0.22
- Testcontainers.MsSql 3.10.0
- Verify 31.8.0
- Shouldly 4.3.0
- Coverlet (Code Coverage)

## 📦 Estrutura do Projeto

```
integration-tdd/
├── Charging.Api/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs          # Contexto do Entity Framework
│   │   └── Configurations/
│   │       └── UsuarioConfiguration.cs      # Configuração da entidade Usuario
│   ├── Models/
│   │   └── Usuario.cs                       # Modelo de domínio
│   ├── Program.cs                           # Configuração e endpoints da API
│   ├── appsettings.json                     # Configurações da aplicação
│   └── Charging.Api.csproj
│
├── Charging.IntegrationTests/
│   ├── ApiFactory.cs                        # Factory para WebApplicationFactory
│   ├── ApiFixture.cs                        # Fixture com TestContainers
│   ├── GuidCollectionFixture.cs             # Collection fixture do xUnit
│   ├── GuidFixture.cs                       # Fixture auxiliar
│   ├── UsuariosIntegrationTests.cs          # Testes de integração principais
│   └── Charging.IntegrationTests.csproj
│
└── Charging.sln
```

## 📊 Modelo de Dados

### Usuario
```csharp
{
    Id: int,
    Nome: string (obrigatório, máx. 255 caracteres),
    Email: string (obrigatório, máx. 255 caracteres, único),
    DataCriacao: DateTime (obrigatório)
}
```

## 🔌 Endpoints da API

### GET /api/usuarios
Retorna todos os usuários cadastrados.

**Resposta:** `200 OK`
```json
[
  {
    "id": 1,
    "nome": "José Carlos",
    "email": "jose@email.com",
    "dataCriacao": "2024-01-01T10:00:00"
  }
]
```

### GET /api/usuarios/{id}
Retorna um usuário específico por ID.

**Parâmetros:**
- `id` (int) - ID do usuário

**Respostas:**
- `200 OK` - Usuário encontrado
- `404 Not Found` - Usuário não encontrado

### POST /api/usuarios
Cria um novo usuário.

**Body:**
```json
{
  "nome": "José Carlos",
  "email": "jose@email.com"
}
```

**Resposta:** `201 Created`
```json
{
  "id": 1,
  "nome": "José Carlos",
  "email": "jose@email.com",
  "dataCriacao": "2024-01-01T10:00:00"
}
```

## 🧪 Testes de Integração

### Estratégia de Testes

Os testes de integração utilizam:
- **TestContainers** para criar instâncias isoladas do SQL Server em containers Docker
- **WebApplicationFactory** para criar uma instância da API em memória
- **Verify** para snapshot testing, garantindo que as respostas da API permaneçam consistentes

### Cobertura de Testes

1. **Post_DeveCriarUmNovoUsuario** - Valida criação de usuário
2. **Get_DeveRetornarUsuarioPorId** - Valida busca por ID
3. **GetAll_DeveRetornarListaDeUsuarios** - Valida listagem de usuários
4. **Get_DeveRetornarJsonCompleto** - Valida formato JSON da resposta
5. **Get_DeveRetornarStatusCodeCorreto** - Valida códigos de status HTTP

### Arquitetura de Testes

- **ApiFixture**: Gerencia o ciclo de vida do container SQL Server e da aplicação de teste
- **GuidCollectionFixture**: Compartilha fixtures entre testes usando xUnit Collection
- **Snapshot Testing**: Utiliza arquivos `.verified.txt` para validar respostas

## 🚀 Como Executar

### Pré-requisitos

- **.NET 8.0 SDK** instalado
- **Docker Desktop** instalado e em execução (para testes de integração)
- **SQL Server** (para execução local da API)

### Executar a API

```powershell
cd Charging.Api
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Executar os Testes de Integração

```powershell
cd Charging.IntegrationTests
dotnet test
```

### Executar Testes com Cobertura de Código

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Restaurar Dependências

```powershell
dotnet restore
```

### Build da Solução

```powershell
dotnet build
```

## 🔧 Configuração

### String de Conexão

A string de conexão padrão está configurada em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Charging;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Testes de Integração

Os testes criam automaticamente um container SQL Server com:
- **Senha**: `yourStrong(!)Password123`
- **Porta**: Dinâmica (gerenciada pelo TestContainers)
- **Banco de dados**: Criado e migrado automaticamente

## 📝 Padrões e Boas Práticas

### Testes
- ✅ Isolamento total entre testes usando containers descartáveis
- ✅ Snapshot testing para validação de contratos
- ✅ Nomenclatura clara seguindo padrão: `Method_Should_ExpectedBehavior`
- ✅ Arrange-Act-Assert pattern

### Código
- ✅ Minimal APIs para simplicidade
- ✅ Entity Framework com Fluent API para configuração de entidades
- ✅ Dependency Injection nativo do ASP.NET Core
- ✅ Configuração por ambiente (Development/Production)

## 🐛 Troubleshooting

### Problema: Testes falham com erro de conexão ao SQL Server

**Solução**: Certifique-se de que o Docker Desktop está em execução:
```powershell
docker ps
```

### Problema: Porta já em uso ao executar a API

**Solução**: Altere as portas em `Properties/launchSettings.json` ou encerre o processo que está usando a porta.

### Problema: Migration não executada

**Solução**: Execute as migrations manualmente:
```powershell
cd Charging.Api
dotnet ef database update
```

## 📚 Recursos Adicionais

- [Documentação ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [xUnit](https://xunit.net/)
- [TestContainers](https://dotnet.testcontainers.org/)
- [Verify](https://github.com/VerifyTests/Verify)

## 👥 Contribuindo

Este é um projeto template para demonstração de boas práticas em testes de integração. Sinta-se livre para usar como base para seus projetos!

## 📄 Licença

Este projeto é um template de código aberto para fins educacionais.
