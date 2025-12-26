# 🔐 Configuração de Variáveis de Ambiente

Este documento explica como configurar as variáveis de ambiente para o projeto.

## 📋 Quick Start

### 1️⃣ Criar arquivo .env

**Windows (PowerShell):**
```powershell
Copy-Item .env.example .env
```

**Linux/Mac:**
```bash
cp .env.example .env
```

### 2️⃣ Editar valores (se necessário)

Abra o arquivo `.env` e ajuste os valores conforme sua necessidade:

```bash
code .env
# ou
notepad .env
# ou
vim .env
```

### 3️⃣ Pronto!

O Docker Compose irá carregar automaticamente as variáveis do arquivo `.env`.

---

## 📚 Variáveis Disponíveis

### 🗄️ SQL Server

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `MSSQL_SA_PASSWORD` | `yourStrong(!)Password123` | Senha do usuário SA |
| `MSSQL_PID` | `Express` | Edição do SQL Server |
| `MSSQL_PORT` | `1433` | Porta exposta no host |

**⚠️ IMPORTANTE:** Mude a senha para produção!

### 🗃️ Database

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `DB_SERVER` | `sqlserver` | Nome do container/servidor |
| `DB_PORT` | `1433` | Porta do SQL Server |
| `DB_NAME` | `Charging` | Nome do banco de dados |
| `DB_USER` | `sa` | Usuário do banco |
| `DB_TRUST_CERTIFICATE` | `True` | Trust server certificate |

### 🚀 API

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `API_PORT` | `5254` | Porta da API no host |
| `API_INTERNAL_PORT` | `80` | Porta interna no container |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente da aplicação |
| `ASPNETCORE_URLS` | `http://+:80` | URLs que a API escuta |

### 🧪 K6 Tests

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `K6_BASE_URL` | `http://api:80` | URL da API para testes |
| `K6_RESULTS_PATH` | `/results` | Pasta de resultados no container |

### 🐳 Docker

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `COMPOSE_PROJECT_NAME` | `charging-api` | Nome do projeto Docker |
| `DOCKER_BUILDKIT` | `1` | Habilita BuildKit |
| `COMPOSE_DOCKER_CLI_BUILD` | `1` | Build via Docker CLI |

### 💚 Healthcheck

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `HEALTHCHECK_INTERVAL` | `10s` | Intervalo entre checks |
| `HEALTHCHECK_TIMEOUT` | `5s` | Timeout do check |
| `HEALTHCHECK_RETRIES` | `5` | Tentativas antes de falhar |
| `HEALTHCHECK_START_PERIOD` | `30s` | Período de aquecimento |

### 💾 Volumes

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `SQLSERVER_VOLUME` | `sqlserver-data` | Nome do volume do SQL Server |

---

## 🎯 Exemplos de Configuração

### Desenvolvimento Local

```bash
# .env
MSSQL_SA_PASSWORD=Dev123!Strong
API_PORT=5254
ASPNETCORE_ENVIRONMENT=Development
```

### Staging

```bash
# .env
MSSQL_SA_PASSWORD=Staging_SuperStrong_Pass!2024
API_PORT=8080
ASPNETCORE_ENVIRONMENT=Staging
DB_NAME=ChargingStaging
COMPOSE_PROJECT_NAME=charging-staging
```

### Produção

```bash
# .env
MSSQL_SA_PASSWORD=${VAULT_SQL_PASSWORD}  # De um secret vault
API_PORT=443
ASPNETCORE_ENVIRONMENT=Production
DB_NAME=ChargingProd
COMPOSE_PROJECT_NAME=charging-prod
HEALTHCHECK_INTERVAL=30s
```

---

## 🔒 Segurança

### ⚠️ NUNCA Commite o arquivo .env

O arquivo `.env` está no `.gitignore` e **não deve** ser commitado ao Git porque contém informações sensíveis.

```bash
# Verificar se .env está ignorado
git status

# .env não deve aparecer na lista de arquivos
```

### ✅ Use .env.example como Template

O arquivo `.env.example` é um template **sem valores sensíveis** e deve ser commitado:

```bash
git add .env.example
git commit -m "Add environment variables template"
```

### 🔐 Produção: Use Secrets

Para produção, use gerenciadores de secrets:

- **Azure Key Vault**
- **AWS Secrets Manager**
- **HashiCorp Vault**
- **Docker Secrets**

**Exemplo com Docker Secrets:**

```yaml
# docker-compose.prod.yml
services:
  api:
    secrets:
      - db_password
    environment:
      - ConnectionStrings__DefaultConnection=Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=/run/secrets/db_password;

secrets:
  db_password:
    external: true
```

---

## 🔄 Sobrescrever Variáveis

### Método 1: Arquivo .env.local

Crie um arquivo `.env.local` (também ignorado pelo Git):

```bash
# .env.local
API_PORT=9000
MSSQL_SA_PASSWORD=MinhaOutraSenha123!
```

Docker Compose lê ambos os arquivos (`.env` e `.env.local`).

### Método 2: Variáveis de Ambiente do Sistema

```bash
# Windows PowerShell
$env:API_PORT="9000"
docker-compose up -d

# Linux/Mac
export API_PORT=9000
docker-compose up -d

# Inline
API_PORT=9000 docker-compose up -d
```

### Método 3: Flag -e no docker-compose

```bash
docker-compose run -e API_PORT=9000 api
```

---

## 🐛 Troubleshooting

### Problema: Variáveis não são carregadas

**Solução:**
```bash
# Verificar se .env existe
ls -la .env

# Ver variáveis carregadas
docker-compose config

# Recriar containers
docker-compose down
docker-compose up -d
```

### Problema: Senha do SQL Server inválida

```
Error: Password validation failed
```

**Solução:**
A senha deve ter:
- Mínimo 8 caracteres
- Letras maiúsculas e minúsculas
- Números
- Caracteres especiais

```bash
# Boas senhas:
MSSQL_SA_PASSWORD=Strong@Pass123
MSSQL_SA_PASSWORD=MyS3cur3P@ssw0rd!
MSSQL_SA_PASSWORD=yourStrong(!)Password123

# Más senhas:
MSSQL_SA_PASSWORD=123456        # Muito simples
MSSQL_SA_PASSWORD=password      # Sem números/especiais
```

### Problema: Porta já em uso

```
Error: port is already allocated
```

**Solução:**
```bash
# Alterar porta no .env
API_PORT=5255  # Trocar de 5254 para 5255
MSSQL_PORT=1434  # Trocar de 1433 para 1434

# Ou parar o serviço que está usando a porta
# Windows
netstat -ano | findstr :5254
taskkill /PID <PID> /F

# Linux
sudo lsof -i :5254
kill -9 <PID>
```

### Problema: Arquivo .env não é lido

**Solução:**

1. Verificar se o arquivo está na raiz do projeto
2. Verificar se não tem espaços no nome (deve ser `.env`, não `.env ` ou ` .env`)
3. Verificar encoding (deve ser UTF-8, sem BOM)

```bash
# Verificar localização
pwd
ls -la .env

# Verificar sintaxe
cat .env

# Recriar do template
cp .env.example .env
```

---

## 📖 Referências

### Docker Compose e Variáveis

- [Docker Compose Environment Variables](https://docs.docker.com/compose/environment-variables/)
- [Environment File](https://docs.docker.com/compose/env-file/)
- [Variable Substitution](https://docs.docker.com/compose/compose-file/#variable-substitution)

### Segurança

- [Docker Secrets](https://docs.docker.com/engine/swarm/secrets/)
- [OWASP - Storing Secrets](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)

---

## ✅ Checklist de Configuração

- [ ] Criar arquivo `.env` a partir do `.env.example`
- [ ] Alterar senha padrão do SQL Server
- [ ] Verificar portas disponíveis
- [ ] Ajustar environment conforme necessidade
- [ ] **NÃO** commitar arquivo `.env`
- [ ] Documentar qualquer variável adicional
- [ ] Testar com `docker-compose config`
- [ ] Iniciar ambiente com `docker-compose up -d`

---

**Última atualização:** 2024-12-26

