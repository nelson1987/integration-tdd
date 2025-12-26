# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos do projeto
COPY ["Charging.Api/Charging.Api.csproj", "Charging.Api/"]
RUN dotnet restore "Charging.Api/Charging.Api.csproj"

# Copiar todo o código fonte
COPY . .
WORKDIR "/src/Charging.Api"

# Build da aplicação
RUN dotnet build "Charging.Api.csproj" -c Release -o /app/build

# Estágio 2: Publish
FROM build AS publish
RUN dotnet publish "Charging.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar curl para healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 80

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Charging.Api.dll"]

