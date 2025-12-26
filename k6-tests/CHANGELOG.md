# 📝 Changelog - Testes de Performance k6

## [2.0.0] - Configurações Centralizadas e Thresholds Aprimorados

### ✨ Novas Funcionalidades

#### 🎛️ Configurações Constantes

Todos os testes agora possuem um objeto `CONFIG` no início do arquivo com todas as configurações em um único lugar:

**Antes:**
```javascript
export const options = {
  vus: 5,
  duration: '1s',
  thresholds: {
    http_req_duration: ['p(95)<100'],
    http_req_failed: ['rate<0.01'],
  },
};
```

**Depois:**
```javascript
const CONFIG = {
  VUS: 5,
  DURATION: '1s',
  THRESHOLD_P95_MS: 100,
  THRESHOLD_P99_MS: 150,
  THRESHOLD_ERROR_RATE: 0.0,
  THRESHOLD_ERROR_TOLERANCE: 0.005,
};

export const options = {
  vus: CONFIG.VUS,
  duration: CONFIG.DURATION,
  thresholds: {
    http_req_duration: [
      `p(95)<${CONFIG.THRESHOLD_P95_MS}`,
      `p(99)<${CONFIG.THRESHOLD_P99_MS}`,
    ],
    http_req_failed: [`rate<=${CONFIG.THRESHOLD_ERROR_RATE}`],
  },
};
```

### 🎯 Novos Thresholds

#### Smoke Test

| Métrica | Valor Anterior | Novo Valor | Mudança |
|---------|----------------|------------|---------|
| P95 | < 100ms | < 100ms | Mantido |
| **P99** | ❌ Não existia | **< 150ms** | ✨ Novo |
| Taxa de Erro P95 | < 1% | **≤ 0%** | 🔒 Mais rigoroso |
| Taxa de Erro P99 | ❌ Não existia | **≤ 0.5%** | ✨ Novo |
| Checks | ❌ Não existia | **≥ 99%** | ✨ Novo |

#### Load Test

| Métrica | Valor Anterior | Novo Valor | Mudança |
|---------|----------------|------------|---------|
| P95 | < 300ms | < 300ms | Mantido |
| **P99** | ❌ Não existia | **< 500ms** | ✨ Novo |
| **Média** | ❌ Não existia | **< 200ms** | ✨ Novo |
| Taxa de Erro P95 | < 5% | **≤ 0%** | 🔒 Mais rigoroso |
| Taxa de Erro P99 | < 5% | **≤ 0.5%** | 🔒 Mais rigoroso |
| Checks | ≥ 95% | **≥ 99.5%** | 🔒 Mais rigoroso |

#### Stress Test

| Métrica | Valor Anterior | Novo Valor | Mudança |
|---------|----------------|------------|---------|
| P95 | < 500ms | < 500ms | Mantido |
| **P99** | ❌ Não existia | **< 1000ms** | ✨ Novo |
| Taxa de Erro P95 | < 10% | **≤ 5%** | 🔒 Mais rigoroso |
| Taxa de Erro P99 | < 10% | **≤ 10%** | Mantido |

### 📦 Arquivos Modificados

- ✅ `smoke-test.js` - Adicionado objeto CONFIG
- ✅ `load-test.js` - Adicionado objeto CONFIG
- ✅ `stress-test.js` - Adicionado objeto CONFIG
- ✨ `config.md` - Novo documento de configuração
- ✨ `CHANGELOG.md` - Este arquivo

### 🎨 Melhorias de Código

#### Distribuição de Cenários Configurável

**Antes (hardcoded):**
```javascript
if (Math.random() < 0.4) {
  // POST
} else if (Math.random() < 0.75) {
  // GET ALL
}
```

**Depois (configurável):**
```javascript
const CONFIG = {
  SCENARIO_POST_PERCENTAGE: 0.4,
  SCENARIO_GET_ALL_PERCENTAGE: 0.3,
};

if (Math.random() < CONFIG.SCENARIO_POST_PERCENTAGE) {
  // POST
} else if (Math.random() < (CONFIG.SCENARIO_POST_PERCENTAGE + CONFIG.SCENARIO_GET_ALL_PERCENTAGE)) {
  // GET ALL
}
```

### 📊 Comparação Visual

#### Taxa de Erro - Antes vs Depois

```
ANTES:
┌─────────────┬──────────┐
│ Teste       │ Erro Max │
├─────────────┼──────────┤
│ Smoke       │   1.0%   │
│ Load        │   5.0%   │
│ Stress      │  10.0%   │
└─────────────┴──────────┘

DEPOIS:
┌─────────────┬──────────┬──────────┐
│ Teste       │ P95 Erro │ P99 Erro │
├─────────────┼──────────┼──────────┤
│ Smoke       │   0.0%   │   0.5%   │ ← Muito mais rigoroso!
│ Load        │   0.0%   │   0.5%   │ ← Muito mais rigoroso!
│ Stress      │   5.0%   │  10.0%   │ ← Mais rigoroso!
└─────────────┴──────────┴──────────┘
```

### 🎯 Benefícios

#### ✅ Facilidade de Manutenção

- Todos os valores em um único lugar
- Não precisa procurar pelo código
- Comentários descritivos
- Nomes autoexplicativos

#### ✅ Thresholds Mais Rigorosos

- **P95 erro = 0%** significa que 95% das execuções devem ter ZERO erros
- **P99 erro = 0.5%** permite apenas 0.5% de erro nos piores casos
- Garante qualidade muito superior

#### ✅ Maior Visibilidade

- P99 agora é monitorado em todos os testes
- Detecta outliers que antes passavam despercebidos
- Média adicionada ao load test para visão geral

### 📖 Documentação Adicionada

- ✨ **config.md** - Guia completo de configuração
  - Explicação de cada parâmetro
  - Exemplos de ajustes
  - Valores recomendados por ambiente
  - Boas práticas

### 🔄 Migração

Para migrar testes customizados:

1. Abra o arquivo de teste
2. Localize o objeto `CONFIG` no início
3. Ajuste os valores conforme necessário
4. Execute e valide

**Exemplo:**
```javascript
// Ajustar para API mais rápida
const CONFIG = {
  THRESHOLD_P95_MS: 150,  // Era 300ms
  THRESHOLD_P99_MS: 250,  // Era 500ms
};
```

### 🚀 Próximos Passos

- [ ] Adicionar suporte a variáveis de ambiente
- [ ] Criar presets (dev, staging, production)
- [ ] Dashboard Grafana para visualização
- [ ] Alertas automáticos quando thresholds falharem

### 📝 Notas de Breaking Changes

⚠️ **NENHUM BREAKING CHANGE** - Os testes continuam funcionando exatamente como antes, mas com thresholds mais rigorosos.

Se seus testes estavam **passando antes** e **falharem agora**, isso significa que:
1. A API não estava cumprindo os novos critérios mais rigorosos
2. Você pode ajustar os thresholds conforme necessário
3. Ou otimizar a API para cumprir os novos critérios

### 🎓 Exemplos de Uso

#### Cenário 1: Ambiente de Desenvolvimento

```javascript
// Relaxar thresholds para desenvolvimento local
const CONFIG = {
  THRESHOLD_P95_MS: 500,
  THRESHOLD_ERROR_RATE_P99: 0.02,  // 2%
};
```

#### Cenário 2: Produção com SLA rigoroso

```javascript
// SLA: 99.9% de disponibilidade, P95 < 150ms
const CONFIG = {
  THRESHOLD_P95_MS: 150,
  THRESHOLD_ERROR_RATE_P95: 0.0,
  THRESHOLD_ERROR_RATE_P99: 0.001,  // 0.1%
  THRESHOLD_CHECKS_PASS: 0.999,     // 99.9%
};
```

---

## [1.0.0] - Release Inicial

### ✨ Funcionalidades

- ✅ Smoke Test implementado
- ✅ Load Test implementado
- ✅ Stress Test implementado
- ✅ Script PowerShell de automação
- ✅ Documentação completa
- ✅ Relatórios HTML

---

**Legenda:**
- ✨ Novo
- 🔒 Mais rigoroso
- 🔄 Modificado
- 📚 Documentação
- 🐛 Bug fix
- ⚡ Performance

