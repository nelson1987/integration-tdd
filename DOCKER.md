# 🐳 Docker - Guia Completo

Este guia explica como usar Docker e Docker Compose para executar a Charging API, SQL Server e testes k6.

## 📋 Índice

- [Pré-requisitos](#pré-requisitos)
- [Configuração Inicial](#configuração-inicial)
- [Arquitetura](#arquitetura)
- [Quick Start](#quick-start)
- [Comandos Disponíveis](#comandos-disponíveis)
- [Containers](#containers)
- [Volumes e Persistência](#volumes-e-persistência)
- [Troubleshooting](#troubleshooting)

---

## 🔧 Pré-requisitos

- **Docker Desktop** instalado e rodando
- **Docker Compose** v3.8 ou superior
- **WSL 2** (recomendado para Windows)

---

## ⚙️ Configuração Inicial

### 1. Criar arquivo .env

Antes de iniciar, configure as variáveis de ambiente:

**Windows (PowerShell):**
```powershell
.\setup-env.ps1
```

**Linux/Mac:**
```bash
chmod +x setup-env.sh
./setup-env.sh
```

**Ou manualmente:**
```bash
# Windows
Copy-Item .env.example .env

# Linux/Mac
cp .env.example .env
```

📖 **Documentação completa:** [ENV-SETUP.md](ENV-SETUP.md)

---

### Instalação do Docker

**Windows:**
```powershell
winget install Docker.DockerDesktop
```

**Linux:**
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

**Verificar instalação:**
```bash
docker --version
docker-compose --version
```

---

## 🏗️ Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Network                          │
│  (charging-network)                                         │
│                                                             │
│  ┌──────────────┐     ┌──────────────┐     ┌───────────┐  │
│  │              │     │              │     │           │  │
│  │  SQL Server  │────▶│  Charging    │────▶│    k6     │  │
│  │  :1433       │     │  API :80     │     │  Tests    │  │
│  │              │     │              │     │           │  │
│  └──────────────┘     └──────────────┘     └───────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
         │                      │
         │                      │
    Port 1433             Port 5254
    (host)                (host)
```

### Containers

| Container | Imagem | Porta | Descrição |
|-----------|--------|-------|-----------|
| **sqlserver** | mcr.microsoft.com/mssql/server:2019 | 1433 | Banco de dados |
| **api** | charging-api:latest | 5254 | API REST |
| **k6** | grafana/k6:latest | - | Testes smoke |
| **k6-load** | grafana/k6:latest | - | Testes load |
| **k6-stress** | grafana/k6:latest | - | Testes stress |

---

## 🚀 Quick Start

### Método 1: Script Automatizado (Recomendado)

**Windows (PowerShell):**
```powershell
.\docker-run.ps1 up
```

**Linux/Mac:**
```bash
chmod +x docker-run.sh
./docker-run.sh up
```

### Método 2: Docker Compose Direto

```bash
# Iniciar API e SQL Server
docker-compose up -d

# Ver logs
docker-compose logs -f

# Executar testes
docker-compose --profile test up
```

### Acessando a Aplicação

Após iniciar:
- **API:** http://localhost:5254
- **Swagger UI:** http://localhost:5254/swagger
- **SQL Server:** localhost:1433
  - **User:** sa
  - **Password:** yourStrong(!)Password123

---

## 📜 Comandos Disponíveis

### Scripts Auxiliares

#### Windows (PowerShell)

```powershell
# Iniciar
.\docker-run.ps1 up

# Parar
.\docker-run.ps1 down

# Ver logs
.\docker-run.ps1 logs

# Status
.\docker-run.ps1 ps

# Rebuild
.\docker-run.ps1 build

# Testes
.\docker-run.ps1 test-smoke
.\docker-run.ps1 test-load
.\docker-run.ps1 test-stress
.\docker-run.ps1 test  # Todos
```

#### Linux/Mac (Bash)

```bash
# Iniciar
./docker-run.sh up

# Parar
./docker-run.sh down

# Ver logs
./docker-run.sh logs

# Status
./docker-run.sh ps

# Rebuild
./docker-run.sh build

# Testes
./docker-run.sh test-smoke
./docker-run.sh test-load
./docker-run.sh test-stress
./docker-run.sh test  # Todos
```

### Docker Compose Direto

```bash
# Gerenciamento de Containers
docker-compose up -d                    # Iniciar em background
docker-compose down                     # Parar e remover
docker-compose restart                  # Reiniciar
docker-compose stop                     # Apenas parar
docker-compose start                    # Iniciar containers parados

# Logs e Monitoramento
docker-compose logs -f                  # Ver logs (follow)
docker-compose logs api                 # Logs apenas da API
docker-compose ps                       # Status dos containers
docker-compose top                      # Processos rodando

# Build
docker-compose build                    # Build de todos
docker-compose build api                # Build apenas da API
docker-compose build --no-cache         # Build sem cache

# Testes k6
docker-compose run --rm k6              # Smoke test
docker-compose run --rm k6-load         # Load test
docker-compose run --rm k6-stress       # Stress test
docker-compose --profile test up        # Todos os testes
```

---

## 📦 Containers Detalhados

### 1. SQL Server

**Configuração:**
- Imagem: `mcr.microsoft.com/mssql/server:2019-latest`
- Porta: `1433:1433`
- Senha SA: `yourStrong(!)Password123`
- Volume: `sqlserver-data`

**Healthcheck:**
```bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'yourStrong(!)Password123' -Q 'SELECT 1'
```

**Conectar manualmente:**
```bash
docker exec -it charging-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'yourStrong(!)Password123'
```

**Backup do banco:**
```bash
docker exec charging-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'yourStrong(!)Password123' \
  -Q "BACKUP DATABASE Charging TO DISK='/var/opt/mssql/backup/charging.bak'"
```

### 2. API (Charging.Api)

**Configuração:**
- Build: Multi-stage Dockerfile
- Porta: `5254:80`
- Environment: Development
- Connection String: Aponta para container `sqlserver`

**Healthcheck:**
```bash
curl -f http://localhost:80/api/usuarios
```

**Ver logs:**
```bash
docker-compose logs -f api
```

**Entrar no container:**
```bash
docker exec -it charging-api bash
```

### 3. k6 Tests

**Smoke Test:**
```bash
docker-compose run --rm k6 run /scripts/smoke-test.js
```

**Load Test:**
```bash
docker-compose run --rm k6-load run /scripts/load-test.js
```

**Stress Test:**
```bash
docker-compose run --rm k6-stress run /scripts/stress-test.js
```

**Resultados:**
Os resultados são salvos em `./k6-results/` no host.

---

## 💾 Volumes e Persistência

### Volumes Definidos

```yaml
volumes:
  sqlserver-data:    # Dados do SQL Server
```

### Gerenciar Volumes

```bash
# Listar volumes
docker volume ls

# Inspecionar volume
docker volume inspect integration-tdd_sqlserver-data

# Remover volume (perde dados!)
docker volume rm integration-tdd_sqlserver-data

# Remover tudo (containers + volumes)
docker-compose down -v
```

### Backup do Volume

```bash
# Backup
docker run --rm \
  -v integration-tdd_sqlserver-data:/data \
  -v $(pwd):/backup \
  ubuntu tar czf /backup/sqlserver-backup.tar.gz /data

# Restore
docker run --rm \
  -v integration-tdd_sqlserver-data:/data \
  -v $(pwd):/backup \
  ubuntu tar xzf /backup/sqlserver-backup.tar.gz -C /
```

---

## 🐛 Troubleshooting

### Problema: Container não inicia

**Solução:**
```bash
# Ver logs
docker-compose logs api
docker-compose logs sqlserver

# Verificar status
docker-compose ps

# Reiniciar container específico
docker-compose restart api
```

### Problema: Porta já em uso

```
Error: bind: address already in use
```

**Solução:**
```bash
# Windows
netstat -ano | findstr :5254
taskkill /PID <PID> /F

# Linux
sudo lsof -i :5254
kill -9 <PID>

# Ou alterar porta no docker-compose.yml
ports:
  - "5255:80"  # Usar porta 5255 ao invés de 5254
```

### Problema: SQL Server não responde

**Solução:**
```bash
# Aguardar healthcheck
docker-compose ps

# Verificar logs
docker-compose logs sqlserver

# Reiniciar
docker-compose restart sqlserver
```

### Problema: Testes k6 falham

**Solução:**
```bash
# Verificar se API está saudável
curl http://localhost:5254/api/usuarios

# Ver logs da API
docker-compose logs api

# Executar teste manualmente
docker-compose run --rm k6 run /scripts/smoke-test.js --verbose
```

### Problema: Build falha

**Solução:**
```bash
# Limpar cache
docker-compose build --no-cache

# Remover imagens antigas
docker image prune -a

# Build verbose
docker-compose build --progress=plain
```

### Problema: Banco de dados não criado

**Solução:**
```bash
# Entrar na API e verificar
docker exec -it charging-api bash
cat /app/init-db.sh

# Ver logs de inicialização
docker-compose logs api | grep -i database

# Criar manualmente
docker exec charging-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'yourStrong(!)Password123' \
  -Q "CREATE DATABASE Charging"
```

---

## 🔄 Workflows Comuns

### Desenvolvimento Local

```bash
# 1. Iniciar ambiente
docker-compose up -d

# 2. Ver logs em tempo real
docker-compose logs -f api

# 3. Fazer mudanças no código
# (rebuild necessário)

# 4. Rebuild e restart
docker-compose build api
docker-compose restart api

# 5. Executar testes
docker-compose run --rm k6 run /scripts/smoke-test.js

# 6. Parar quando terminar
docker-compose down
```

### Testes de Performance

```bash
# 1. Ambiente limpo
docker-compose down -v
docker-compose up -d

# 2. Aguardar healthcheck
docker-compose ps

# 3. Executar smoke test primeiro
docker-compose run --rm k6 run /scripts/smoke-test.js

# 4. Se passar, executar load test
docker-compose run --rm k6-load run /scripts/load-test.js

# 5. Ver resultados
cat k6-results/load-results.json
```

### CI/CD Pipeline

```bash
# Build
docker-compose build

# Start services
docker-compose up -d

# Wait for health
timeout 60 bash -c 'until docker-compose ps | grep healthy; do sleep 2; done'

# Run tests
docker-compose --profile test up --abort-on-container-exit

# Cleanup
docker-compose down -v
```

---

## 📊 Monitoramento

### Docker Stats

```bash
# CPU/Memória em tempo real
docker stats

# Apenas containers da aplicação
docker stats charging-api charging-sqlserver
```

### Health Status

```bash
# Ver health de todos containers
docker-compose ps

# Health check manual
docker inspect --format='{{.State.Health.Status}}' charging-api
```

---

## 🔐 Segurança

### Senhas

⚠️ **IMPORTANTE:** A senha `yourStrong(!)Password123` é para **desenvolvimento apenas**.

Para produção:
1. Use variáveis de ambiente
2. Use Docker Secrets
3. Use Azure Key Vault ou similar

### Exemplo com .env

```bash
# Criar .env
cat > .env << EOF
SA_PASSWORD=Sua_Senha_Super_Forte_Aqui!
EOF

# Atualizar docker-compose.yml
environment:
  - SA_PASSWORD=${SA_PASSWORD}
```

---

## 📚 Referências

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [SQL Server Container](https://hub.docker.com/_/microsoft-mssql-server)
- [Grafana k6](https://k6.io/docs/)

---

## 🆘 Suporte

Se encontrar problemas:

1. Verifique os logs: `docker-compose logs`
2. Verifique o status: `docker-compose ps`
3. Limpe tudo: `docker-compose down -v`
4. Rebuild: `docker-compose build --no-cache`
5. Tente novamente: `docker-compose up -d`

---

**Última atualização:** 2024-12-26

