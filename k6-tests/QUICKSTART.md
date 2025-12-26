# 🚀 Quick Start - Testes de Performance k6

Guia rápido para começar a usar os testes de performance.

## ⚡ Início Rápido (5 minutos)

### 1️⃣ Instalar k6

**Windows:**
```powershell
winget install k6
```

**macOS:**
```bash
brew install k6
```

**Linux:**
```bash
snap install k6
```

### 2️⃣ Iniciar a API

```powershell
# Na raiz do projeto
cd Charging.Api
dotnet run
```

Aguarde até ver:
```
Now listening on: http://localhost:5000
```

### 3️⃣ Executar Testes

**Opção A - Script Automático (Recomendado):**
```powershell
cd k6-tests
.\run-tests.ps1
```

**Opção B - Teste Individual:**
```powershell
cd k6-tests

# Smoke test (rápido - 1 segundo)
k6 run smoke-test.js

# Load test (completo - 2 minutos)
k6 run load-test.js

# Stress test (intenso - 10 minutos)
k6 run stress-test.js
```

## 📊 Entendendo os Resultados

### ✅ Teste Passou
```
✓ POST status é 201
✓ POST tempo < 100ms

checks.........................: 100.00% ✓ 50    ✗ 0
http_req_duration..............: avg=85ms    p(95)=95ms
```

### ❌ Teste Falhou
```
✗ POST tempo < 100ms

checks.........................: 80.00%  ✓ 40    ✗ 10
http_req_duration..............: avg=150ms   p(95)=320ms
```

## 🎯 Requisitos dos Testes

| Teste | Duração | Objetivo | Threshold |
|-------|---------|----------|-----------|
| **Smoke** | 1s | 5 req/s | P95 < 100ms |
| **Load** | 2min | 1000 usuários/min | P95 < 300ms |
| **Stress** | 10min | Encontrar limite | P95 < 500ms |

## 📈 Visualizando Relatórios

Após executar o **Load Test**, abra:
```
k6-tests/report-load.html
```

No navegador para ver relatório visual completo.

## 🔧 Troubleshooting Rápido

### Problema: "k6 não é reconhecido"
**Solução:** Reinicie o terminal após instalar o k6

### Problema: "Cannot GET /api/usuarios"
**Solução:** Certifique-se que a API está rodando na porta 5000

### Problema: Testes muito lentos
**Solução:** Verifique se o SQL Server está rodando e acessível

### Problema: Taxa de erro alta
**Solução:** 
1. Verifique logs da API
2. Verifique conexão com banco de dados
3. Reduza a carga (edite o arquivo .js)

## 📝 Exemplos de Comandos

```powershell
# Executar com URL customizada
$env:BASE_URL="http://localhost:5000"; k6 run load-test.js

# Salvar resultados em JSON
k6 run --out json=results.json load-test.js

# Executar com mais detalhes
k6 run --verbose load-test.js

# Executar apenas o que está em cada cenário
k6 run --iterations 10 smoke-test.js
```

## 🎓 Próximos Passos

1. ✅ Execute o smoke test
2. ✅ Analise os resultados
3. ✅ Execute o load test
4. ✅ Abra o relatório HTML
5. 📚 Leia o [README.md](README.md) completo
6. 🔧 Customize os testes para suas necessidades

## 💡 Dicas

- **Sempre** execute o smoke test antes do load test
- **Monitore** recursos do sistema durante os testes
- **Compare** resultados entre diferentes versões
- **Documente** mudanças de performance

## 🆘 Precisa de Ajuda?

Leia a documentação completa: [README.md](README.md)

---

**Tempo estimado para primeiro teste:** 5 minutos ⏱️

