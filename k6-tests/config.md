# 🎛️ Guia de Configuração - Testes de Performance k6

Este guia explica como ajustar os thresholds e configurações dos testes de performance.

## 📋 Índice

- [Configurações por Teste](#configurações-por-teste)
- [Thresholds Explicados](#thresholds-explicados)
- [Como Alterar Configurações](#como-alterar-configurações)
- [Exemplos de Ajustes](#exemplos-de-ajustes)

---

## 🎯 Configurações por Teste

### **Smoke Test** (`smoke-test.js`)

**Localização:** Início do arquivo, objeto `CONFIG`

```javascript
const CONFIG = {
  VUS: 5,                           // Usuários virtuais simultâneos
  DURATION: '1s',                   // Duração do teste
  THRESHOLD_P95_MS: 100,            // P95 deve ser < 100ms
  THRESHOLD_P99_MS: 150,            // P99 deve ser < 150ms
  THRESHOLD_ERROR_RATE: 0.0,        // Taxa de erro P95: 0% (praticamente zero)
  THRESHOLD_ERROR_TOLERANCE: 0.005, // Taxa de erro P99: 0.5%
  BASE_URL: 'http://localhost:5000',
};
```

**Objetivo:** Validação rápida de funcionalidade básica

---

### **Load Test** (`load-test.js`)

**Localização:** Início do arquivo, objeto `CONFIG`

```javascript
const CONFIG = {
  // Estágios do teste
  RAMP_UP_DURATION: '30s',          // Tempo para aumentar carga
  SUSTAIN_DURATION: '1m',           // Tempo mantendo carga
  RAMP_DOWN_DURATION: '30s',        // Tempo para diminuir carga
  TARGET_VUS: 17,                   // Usuários virtuais (~1000 req/min)
  
  // Thresholds de performance
  THRESHOLD_P95_MS: 300,            // P95 deve ser < 300ms
  THRESHOLD_P99_MS: 500,            // P99 deve ser < 500ms
  THRESHOLD_AVG_MS: 200,            // Média deve ser < 200ms
  
  // Thresholds de erro
  THRESHOLD_ERROR_RATE_P95: 0.0,    // Taxa de erro P95: 0% (praticamente zero)
  THRESHOLD_ERROR_RATE_P99: 0.005,  // Taxa de erro P99 tolerável: 0.5%
  THRESHOLD_CHECKS_PASS: 0.995,     // 99.5% dos checks devem passar
  
  // Distribuição de cenários
  SCENARIO_POST_PERCENTAGE: 0.4,     // 40% POST
  SCENARIO_GET_ALL_PERCENTAGE: 0.3,  // 30% GET ALL
  // 30% restante é GET BY ID
};
```

**Objetivo:** Simular carga de produção (1000 usuários/minuto)

---

### **Stress Test** (`stress-test.js`)

**Localização:** Início do arquivo, objeto `CONFIG`

```javascript
const CONFIG = {
  // Estágios progressivos
  STAGE_1_DURATION: '1m',
  STAGE_1_TARGET_VUS: 10,
  
  STAGE_2_DURATION: '2m',
  STAGE_2_TARGET_VUS: 20,
  
  STAGE_3_DURATION: '2m',
  STAGE_3_TARGET_VUS: 50,
  
  STAGE_4_DURATION: '2m',
  STAGE_4_TARGET_VUS: 100,
  
  STAGE_5_DURATION: '2m',
  STAGE_5_TARGET_VUS: 200,
  
  RAMP_DOWN_DURATION: '1m',
  
  // Thresholds (mais permissivos)
  THRESHOLD_P95_MS: 500,            // P95 pode chegar a 500ms
  THRESHOLD_P99_MS: 1000,           // P99 pode chegar a 1000ms
  THRESHOLD_ERROR_RATE_P95: 0.05,   // Aceita até 5% de erro no P95
  THRESHOLD_ERROR_RATE_P99: 0.1,    // Aceita até 10% de erro no P99
  
  // Distribuição
  SCENARIO_POST_PERCENTAGE: 0.5,    // 50% POST
  SCENARIO_GET_ALL_PERCENTAGE: 0.3, // 30% GET ALL
};
```

**Objetivo:** Encontrar ponto de quebra do sistema

---

## 📊 Thresholds Explicados

### **Percentis (P95, P99)**

- **P50 (Mediana):** 50% das requisições são mais rápidas que este valor
- **P95:** 95% das requisições são mais rápidas que este valor
- **P99:** 99% das requisições são mais rápidas que este valor

**Exemplo:**
- P95 = 100ms significa que 95% das requisições levaram menos de 100ms
- P99 = 150ms significa que 99% das requisições levaram menos de 150ms

### **Taxa de Erro**

- **0.0** = 0% de erro (praticamente zero erros)
- **0.005** = 0.5% de erro tolerável (5 erros a cada 1000 requisições)
- **0.01** = 1% de erro
- **0.05** = 5% de erro
- **0.1** = 10% de erro

### **Checks**

- **0.99** = 99% dos checks devem passar
- **0.995** = 99.5% dos checks devem passar
- **0.999** = 99.9% dos checks devem passar

---

## 🔧 Como Alterar Configurações

### Método 1: Editar diretamente o arquivo

1. Abra o arquivo de teste desejado (ex: `load-test.js`)
2. Localize o objeto `CONFIG` no início do arquivo
3. Altere os valores desejados
4. Salve o arquivo

**Exemplo:**

```javascript
const CONFIG = {
  THRESHOLD_P95_MS: 300,  // Era 300ms, mudar para 200ms
  // ↓
  THRESHOLD_P95_MS: 200,  // Agora é mais rigoroso
};
```

### Método 2: Via variáveis de ambiente (futura implementação)

```powershell
$env:THRESHOLD_P95_MS=200; k6 run load-test.js
```

---

## 💡 Exemplos de Ajustes

### Cenário 1: API mais rápida - Tornar thresholds mais rigorosos

**Load Test:**
```javascript
const CONFIG = {
  THRESHOLD_P95_MS: 150,            // Era 300ms → agora 150ms
  THRESHOLD_P99_MS: 250,            // Era 500ms → agora 250ms
  THRESHOLD_AVG_MS: 100,            // Era 200ms → agora 100ms
  THRESHOLD_ERROR_RATE_P95: 0.0,    // Mantém 0%
  THRESHOLD_ERROR_RATE_P99: 0.001,  // Era 0.5% → agora 0.1%
};
```

### Cenário 2: API mais lenta - Relaxar thresholds

**Load Test:**
```javascript
const CONFIG = {
  THRESHOLD_P95_MS: 500,            // Era 300ms → agora 500ms
  THRESHOLD_P99_MS: 800,            // Era 500ms → agora 800ms
  THRESHOLD_AVG_MS: 350,            // Era 200ms → agora 350ms
  THRESHOLD_ERROR_RATE_P99: 0.01,   // Era 0.5% → agora 1%
};
```

### Cenário 3: Aumentar carga de teste

**Load Test:**
```javascript
const CONFIG = {
  TARGET_VUS: 34,                   // Era 17 → agora 34 (dobra a carga)
  SUSTAIN_DURATION: '2m',           // Era 1min → agora 2min
  // Ajustar thresholds proporcionalmente
  THRESHOLD_P95_MS: 400,
  THRESHOLD_P99_MS: 700,
};
```

### Cenário 4: Teste ultra-rígido (Zero tolerância)

**Smoke Test:**
```javascript
const CONFIG = {
  VUS: 5,
  DURATION: '1s',
  THRESHOLD_P95_MS: 50,             // Muito rigoroso: 50ms
  THRESHOLD_P99_MS: 100,            // Muito rigoroso: 100ms
  THRESHOLD_ERROR_RATE: 0.0,        // Zero erros
  THRESHOLD_ERROR_TOLERANCE: 0.0,   // Zero tolerância
};
```

### Cenário 5: Distribuição personalizada de cenários

**Load Test - Mais foco em consultas:**
```javascript
const CONFIG = {
  SCENARIO_POST_PERCENTAGE: 0.2,     // 20% POST (era 40%)
  SCENARIO_GET_ALL_PERCENTAGE: 0.5,  // 50% GET ALL (era 30%)
  // 30% GET BY ID (restante)
};
```

**Load Test - Mais foco em criação:**
```javascript
const CONFIG = {
  SCENARIO_POST_PERCENTAGE: 0.7,     // 70% POST (era 40%)
  SCENARIO_GET_ALL_PERCENTAGE: 0.2,  // 20% GET ALL (era 30%)
  // 10% GET BY ID (restante)
};
```

---

## 📈 Valores Recomendados por Ambiente

### **Desenvolvimento Local**

```javascript
// Mais permissivo, foco em funcionalidade
const CONFIG = {
  THRESHOLD_P95_MS: 500,
  THRESHOLD_P99_MS: 1000,
  THRESHOLD_ERROR_RATE_P99: 0.01,   // 1%
};
```

### **Staging/QA**

```javascript
// Próximo de produção, mas com margem
const CONFIG = {
  THRESHOLD_P95_MS: 300,
  THRESHOLD_P99_MS: 500,
  THRESHOLD_ERROR_RATE_P95: 0.0,
  THRESHOLD_ERROR_RATE_P99: 0.005,  // 0.5%
};
```

### **Produção (Validação de SLA)**

```javascript
// Rigoroso, baseado em SLA real
const CONFIG = {
  THRESHOLD_P95_MS: 200,
  THRESHOLD_P99_MS: 400,
  THRESHOLD_ERROR_RATE_P95: 0.0,    // Zero no P95
  THRESHOLD_ERROR_RATE_P99: 0.001,  // 0.1% no P99
  THRESHOLD_CHECKS_PASS: 0.999,     // 99.9%
};
```

---

## 🎓 Boas Práticas

### ✅ **DO's**

- ✅ Ajuste thresholds baseado em dados reais de produção
- ✅ Documente mudanças nos thresholds
- ✅ Comece com valores permissivos e aumente rigor gradualmente
- ✅ Teste localmente antes de commitar mudanças
- ✅ Mantenha thresholds alinhados com SLAs do negócio

### ❌ **DON'Ts**

- ❌ Não defina thresholds impossíveis (ex: P95 < 10ms para DB queries)
- ❌ Não ignore falhas nos testes sem investigar
- ❌ Não use os mesmos thresholds para dev e produção
- ❌ Não altere múltiplos valores sem entender o impacto

---

## 🔍 Interpretando Resultados

### Teste Passou ✅

```
✓ P95 < 300ms (actual: 245ms)
✓ Taxa de erro <= 0% (actual: 0%)
✓ Checks >= 99.5% (actual: 100%)
```

**Ação:** API está performando bem!

### Teste Falhou ❌

```
✗ P95 < 300ms (actual: 425ms)
✓ Taxa de erro <= 0.5% (actual: 0.2%)
✗ Checks >= 99.5% (actual: 98.1%)
```

**Possíveis ações:**
1. **Investigar:** Por que P95 está alto? Queries lentas? Lock de banco?
2. **Otimizar:** Melhorar código/queries
3. **Ajustar threshold:** Se 425ms é aceitável para o negócio

---

## 📚 Referências

- [k6 Thresholds Documentation](https://k6.io/docs/using-k6/thresholds/)
- [Understanding Percentiles](https://www.dynatrace.com/news/blog/why-percentiles-dont-work-the-way-you-think/)
- [SLA Best Practices](https://www.atlassian.com/incident-management/kpis/sla-vs-slo-vs-sli)

---

## 🆘 Troubleshooting

### Problema: Todos os testes estão falhando

**Solução:** Verifique se a API está rodando e acessível:
```powershell
curl http://localhost:5000/api/usuarios
```

### Problema: P95/P99 muito altos

**Possíveis causas:**
- Queries de banco lentas
- Falta de índices
- Recursos insuficientes (CPU/RAM)
- Conexões de banco limitadas

### Problema: Taxa de erro alta

**Possíveis causas:**
- Limite de conexões do banco atingido
- Timeout de requisições
- Erros de validação na API
- Concorrência causando deadlocks

---

**Última atualização:** $(Get-Date -Format "yyyy-MM-dd")

**Dúvidas?** Consulte [README.md](README.md) ou [QUICKSTART.md](QUICKSTART.md)

