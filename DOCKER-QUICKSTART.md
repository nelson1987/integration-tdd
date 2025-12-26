# 🚀 Docker Quick Start

Guia rápido de 5 minutos para começar com Docker.

## ⚡ Início Rápido

### 0️⃣ Configurar Variáveis de Ambiente (primeira vez)

```powershell
# Windows
.\setup-env.ps1

# Linux/Mac
chmod +x setup-env.sh
./setup-env.sh
```

Isso cria o arquivo `.env` com as configurações padrão.

### 1️⃣ Iniciar Ambiente

```powershell
# Windows
.\docker-run.ps1 up

# Linux/Mac  
./docker-run.sh up
```

**Aguarde ver:**
```
✓ Containers iniciados com sucesso!

📊 Serviços disponíveis:
   API:        http://localhost:5254
   Swagger:    http://localhost:5254/swagger
   SQL Server: localhost:1433
```

### 2️⃣ Testar API

Abra no navegador: http://localhost:5254/swagger

Ou via curl:
```bash
curl http://localhost:5254/api/usuarios
```

### 3️⃣ Executar Testes k6

```powershell
# Smoke test (rápido - 1 segundo)
.\docker-run.ps1 test-smoke

# Load test (completo - 2 minutos)
.\docker-run.ps1 test-load
```

### 4️⃣ Ver Resultados

```bash
# Ver resultados em JSON
cat k6-results/smoke-results.json
cat k6-results/load-results.json

# Ver relatório HTML (se gerado)
k6-tests/report-load.html
```

### 5️⃣ Parar Ambiente

```powershell
.\docker-run.ps1 down
```

---

## 📊 Comandos Essenciais

| Comando | Descrição |
|---------|-----------|
| `up` | Iniciar ambiente |
| `down` | Parar ambiente |
| `logs` | Ver logs |
| `ps` | Status dos containers |
| `test-smoke` | Teste rápido |
| `test-load` | Teste de carga |
| `restart` | Reiniciar |

---

## 🐛 Problemas Comuns

### API não responde

```bash
# Ver logs
docker-compose logs api

# Restart
docker-compose restart api
```

### Porta já em uso

Edite `docker-compose.yml`:
```yaml
api:
  ports:
    - "5255:80"  # Mude para outra porta
```

### Limpar tudo e começar do zero

```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

---

## 📚 Mais Informações

- Documentação completa: [DOCKER.md](DOCKER.md)
- Testes k6: [k6-tests/README.md](k6-tests/README.md)
- README principal: [README.md](README.md)

---

**Tempo estimado:** 5 minutos ⏱️

