# Testes de Performance com k6 - Charging API

Este diretório contém testes de performance automatizados para a Charging API usando [k6](https://k6.io/).

## 📋 Índice

- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Testes Disponíveis](#testes-disponíveis)
- [Como Executar](#como-executar)
- [Métricas e Thresholds](#métricas-e-thresholds)
- [Interpretação dos Resultados](#interpretação-dos-resultados)

## 🔧 Pré-requisitos

- **k6** instalado
- **API Charging** rodando (porta 5000)
- **SQL Server** configurado e acessível

## 📥 Instalação

### Instalar k6

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
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

## 🧪 Testes Disponíveis

### 1. Smoke Test (`smoke-test.js`)

**Objetivo:** Verificação rápida de funcionalidade básica

**Critérios:**
- ✅ 5 requisições em 1 segundo
- ✅ 95% das requisições < 100ms
- ✅ Taxa de erro < 1%

**Quando executar:**
- Após cada deploy
- Antes de executar testes mais pesados
- Para validação rápida

```bash
k6 run smoke-test.js
```

### 2. Load Test (`load-test.js`)

**Objetivo:** Simular carga de produção esperada

**Critérios:**
- ✅ ~1000 usuários por minuto (17 VUs simultâneos)
- ✅ 95% das requisições < 300ms
- ✅ Taxa de erro < 5%
- ✅ Mix realista de operações CRUD (40% POST, 30% GET ALL, 30% GET BY ID)

**Duração:** ~2 minutos

**Quando executar:**
- Antes de releases
- Após mudanças significativas no código
- Para validar SLAs

```bash
k6 run load-test.js
```

**Relatórios gerados:**
- `summary-load.json` - Dados brutos em JSON
- `report-load.html` - Relatório visual em HTML

### 3. Stress Test (`stress-test.js`)

**Objetivo:** Encontrar o ponto de quebra da aplicação

**Estratégia:**
- Aumenta gradualmente de 10 até 200 usuários virtuais
- Monitora degradação de performance
- Identifica limites do sistema

**Duração:** ~10 minutos

**Quando executar:**
- Para capacidade planning
- Para identificar gargalos
- Antes de eventos de alto tráfego

```bash
k6 run stress-test.js
```

## 🚀 Como Executar

### Método 1: Script PowerShell (Recomendado)

```powershell
# Executar todos os testes
.\run-tests.ps1

# Executar teste específico
.\run-tests.ps1 -TestType smoke
.\run-tests.ps1 -TestType load
.\run-tests.ps1 -TestType stress

# Com URL customizada
.\run-tests.ps1 -TestType load -BaseUrl "http://localhost:5000"
```

### Método 2: Executar testes individuais

```bash
# Smoke Test
k6 run smoke-test.js

# Load Test
k6 run load-test.js

# Stress Test
k6 run stress-test.js

# Com URL customizada
k6 run load-test.js -e BASE_URL=http://localhost:5000
```

### Método 3: Com saída em JSON

```bash
k6 run --out json=results.json load-test.js
```

## 📊 Métricas e Thresholds

### Métricas Principais

| Métrica | Descrição |
|---------|-----------|
| `http_req_duration` | Tempo total de resposta da requisição |
| `http_req_waiting` | Tempo aguardando resposta do servidor |
| `http_req_sending` | Tempo enviando dados |
| `http_req_receiving` | Tempo recebendo dados |
| `http_reqs` | Total de requisições por segundo |
| `http_req_failed` | Percentual de requisições falhadas |
| `checks` | Percentual de validações que passaram |

### Thresholds Configurados

#### Smoke Test
```javascript
{
  http_req_duration: ['p(95)<100'],  // 95% < 100ms
  http_req_failed: ['rate<0.01'],    // Erro < 1%
}
```

#### Load Test
```javascript
{
  http_req_duration: ['p(95)<300'],  // 95% < 300ms
  http_req_failed: ['rate<0.05'],    // Erro < 5%
  checks: ['rate>0.95'],             // Checks > 95%
}
```

#### Stress Test
```javascript
{
  http_req_duration: ['p(95)<500'],  // 95% < 500ms
  http_req_failed: ['rate<0.1'],     // Erro < 10%
}
```

## 📈 Interpretação dos Resultados

### Métricas de Sucesso

**✅ Bom:**
- P95 < 200ms
- P99 < 500ms
- Taxa de erro < 1%
- Throughput > 100 req/s

**⚠️ Atenção:**
- P95 entre 200-400ms
- P99 entre 500-1000ms
- Taxa de erro entre 1-5%
- Throughput entre 50-100 req/s

**❌ Crítico:**
- P95 > 400ms
- P99 > 1000ms
- Taxa de erro > 5%
- Throughput < 50 req/s

### Exemplo de Output

```
✓ POST status é 201
✓ POST tempo < 300ms
✓ GET ALL status é 200

checks.........................: 98.50% ✓ 1970    ✗ 30
http_req_duration..............: avg=142.5ms  min=45ms   med=130ms   max=450ms   p(95)=280ms  p(99)=380ms
http_reqs......................: 2000   33.33/s
```

### Análise de Percentis

- **P50 (Mediana):** Experiência típica do usuário
- **P95:** 95% dos usuários têm essa experiência ou melhor
- **P99:** Casos extremos, mas importantes para SLA

## 🔍 Troubleshooting

### API não está respondendo

```bash
# Verificar se a API está rodando
curl http://localhost:5000/api/usuarios

# Iniciar a API
cd ../Charging.Api
dotnet run
```

### k6 não encontrado

```powershell
# Windows
winget install k6

# Verificar instalação
k6 version
```

### Erros de conexão

- Verifique se o SQL Server está rodando
- Verifique a string de conexão
- Verifique firewall/portas

### Performance ruim

- Verifique recursos do sistema (CPU, RAM)
- Verifique índices do banco de dados
- Analise logs da aplicação
- Considere conexões pool do EF Core

## 📁 Estrutura de Arquivos

```
k6-tests/
├── smoke-test.js           # Teste de fumaça
├── load-test.js            # Teste de carga
├── stress-test.js          # Teste de estresse
├── run-tests.ps1           # Script de automação PowerShell
├── README.md               # Esta documentação
├── summary-*.json          # Resultados em JSON (gerado)
└── report-*.html           # Relatórios HTML (gerado)
```

## 🎯 Objetivos de Performance

### Requisitos Atuais

1. **Smoke Test:**
   - ✅ 5 requests/segundo
   - ✅ P95 < 100ms

2. **Load Test:**
   - ✅ 1000 usuários/minuto (~17 req/s)
   - ✅ P95 < 300ms

### Metas Futuras

- [ ] 5000 usuários/minuto
- [ ] P95 < 150ms em load test
- [ ] Suporte a 500 VUs simultâneos

## 📚 Recursos Adicionais

- [Documentação oficial k6](https://k6.io/docs/)
- [k6 Cloud](https://k6.io/cloud/) - Para testes distribuídos
- [Grafana k6](https://grafana.com/docs/k6/latest/) - Visualização avançada
- [k6 Extensions](https://k6.io/docs/extensions/) - Extensões e plugins

## 🤝 Contribuindo

Para adicionar novos testes:

1. Crie um novo arquivo `.js` seguindo a estrutura dos existentes
2. Defina thresholds apropriados
3. Documente o objetivo e critérios do teste
4. Adicione ao `run-tests.ps1` se necessário

