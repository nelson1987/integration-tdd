// Load Test - Teste de carga
// 1000 usuários por minuto (aproximadamente 16-17 usuários simultâneos)
// Requisições devem ser < 300ms

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// ═══════════════════════════════════════════════════════════════
// CONFIGURAÇÕES - Altere aqui para ajustar os thresholds
// ═══════════════════════════════════════════════════════════════
const CONFIG = {
  // Estágios do teste
  RAMP_UP_DURATION: '30s',          // Tempo para aumentar carga
  SUSTAIN_DURATION: '1m',           // Tempo mantendo carga
  RAMP_DOWN_DURATION: '30s',        // Tempo para diminuir carga
  TARGET_VUS: 17,                   // Usuários virtuais alvo (~1000 req/min)
  
  // Thresholds de performance
  THRESHOLD_P95_MS: 300,            // P95 deve ser menor que 300ms
  THRESHOLD_P99_MS: 500,            // P99 deve ser menor que 500ms
  THRESHOLD_AVG_MS: 200,            // Média deve ser menor que 200ms
  
  // Thresholds de erro
  THRESHOLD_ERROR_RATE_P95: 0.0,    // Taxa de erro P95: 0% (praticamente zero)
  THRESHOLD_ERROR_RATE_P99: 0.005,  // Taxa de erro P99 tolerável: 0.5%
  THRESHOLD_CHECKS_PASS: 0.995,     // 99.5% dos checks devem passar
  
  // Distribuição de cenários
  SCENARIO_POST_PERCENTAGE: 0.4,     // 40% POST
  SCENARIO_GET_ALL_PERCENTAGE: 0.3,  // 30% GET ALL
  // 30% restante é GET BY ID
  
  BASE_URL: __ENV.BASE_URL || 'http://localhost:5000',
};

// Métricas customizadas
const errorRate = new Rate('errors');
const successfulRequests = new Counter('successful_requests');
const requestDuration = new Trend('custom_request_duration');

// Configuração do teste
export const options = {
  stages: [
    { duration: CONFIG.RAMP_UP_DURATION, target: CONFIG.TARGET_VUS },
    { duration: CONFIG.SUSTAIN_DURATION, target: CONFIG.TARGET_VUS },
    { duration: CONFIG.RAMP_DOWN_DURATION, target: 0 },
  ],
  thresholds: {
    http_req_duration: [
      `p(95)<${CONFIG.THRESHOLD_P95_MS}`,
      `p(99)<${CONFIG.THRESHOLD_P99_MS}`,
      `avg<${CONFIG.THRESHOLD_AVG_MS}`,
    ],
    http_req_failed: [`rate<=${CONFIG.THRESHOLD_ERROR_RATE_P99}`],
    errors: [
      `rate<=${CONFIG.THRESHOLD_ERROR_RATE_P95}`,  // P95 praticamente zero
      `rate<=${CONFIG.THRESHOLD_ERROR_RATE_P99}`,  // P99 tolerável 0.5%
    ],
    checks: [`rate>=${CONFIG.THRESHOLD_CHECKS_PASS}`],
  },
};

const BASE_URL = CONFIG.BASE_URL;

// Função auxiliar para gerar dados aleatórios
function generateRandomUser() {
  const timestamp = Date.now();
  const random = Math.floor(Math.random() * 1000000);
  
  return {
    nome: `Usuario Load Test ${timestamp}-${random}`,
    email: `loadtest-${timestamp}-${random}@email.com`,
  };
}

export default function () {
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // Cenário 1: Criar usuário (configurável via CONFIG.SCENARIO_POST_PERCENTAGE)
  if (Math.random() < CONFIG.SCENARIO_POST_PERCENTAGE) {
    const payload = JSON.stringify(generateRandomUser());
    const startTime = Date.now();
    
    const response = http.post(`${BASE_URL}/api/usuarios`, payload, params);
    requestDuration.add(Date.now() - startTime);
    
    const success = check(response, {
      'POST status é 201': (r) => r.status === 201,
      'POST tempo < 300ms': (r) => r.timings.duration < 300,
      'POST tem resposta válida': (r) => {
        const body = r.json();
        return body && body.id && body.nome && body.email;
      },
    });
    
    if (success) successfulRequests.add(1);
    errorRate.add(!success);
    
    sleep(0.1); // Pequena pausa entre requisições
  }
  
  // Cenário 2: Listar todos usuários (configurável via CONFIG.SCENARIO_GET_ALL_PERCENTAGE)
  else if (Math.random() < (CONFIG.SCENARIO_POST_PERCENTAGE + CONFIG.SCENARIO_GET_ALL_PERCENTAGE)) {
    const startTime = Date.now();
    
    const response = http.get(`${BASE_URL}/api/usuarios`);
    requestDuration.add(Date.now() - startTime);
    
    const success = check(response, {
      'GET ALL status é 200': (r) => r.status === 200,
      'GET ALL tempo < 300ms': (r) => r.timings.duration < 300,
      'GET ALL retorna array': (r) => Array.isArray(r.json()),
    });
    
    if (success) successfulRequests.add(1);
    errorRate.add(!success);
    
    sleep(0.1);
  }
  
  // Cenário 3: Buscar usuário por ID (30% das requisições)
  else {
    // Criar um usuário primeiro para garantir que existe
    const payload = JSON.stringify(generateRandomUser());
    const createResponse = http.post(`${BASE_URL}/api/usuarios`, payload, params);
    
    if (createResponse.status === 201) {
      const usuarioId = createResponse.json('id');
      
      const startTime = Date.now();
      const response = http.get(`${BASE_URL}/api/usuarios/${usuarioId}`);
      requestDuration.add(Date.now() - startTime);
      
      const success = check(response, {
        'GET BY ID status é 200': (r) => r.status === 200,
        'GET BY ID tempo < 300ms': (r) => r.timings.duration < 300,
        'GET BY ID retorna usuário correto': (r) => {
          const body = r.json();
          return body && body.id === usuarioId;
        },
      });
      
      if (success) successfulRequests.add(1);
      errorRate.add(!success);
    }
    
    sleep(0.1);
  }
}

export function handleSummary(data) {
  const report = generateReport(data);
  
  return {
    'summary-load.json': JSON.stringify(data),
    'report-load.html': generateHTMLReport(data),
    stdout: report,
  };
}

function generateReport(data) {
  let report = '\n';
  report += '╔═══════════════════════════════════════════════════════════════╗\n';
  report += '║          LOAD TEST - Relatório de Performance                ║\n';
  report += '╚═══════════════════════════════════════════════════════════════╝\n\n';
  
  report += '📊 MÉTRICAS GERAIS\n';
  report += '─────────────────────────────────────────────────────────────────\n';
  report += `  Total de Requisições: ${data.metrics.http_reqs.values.count}\n`;
  report += `  Taxa de Requisições: ${data.metrics.http_reqs.values.rate.toFixed(2)}/s\n`;
  report += `  Requisições bem-sucedidas: ${data.metrics.successful_requests?.values.count || 0}\n`;
  report += `  Taxa de Sucesso: ${((1 - (data.metrics.errors?.values.rate || 0)) * 100).toFixed(2)}%\n\n`;
  
  report += '⏱️  TEMPO DE RESPOSTA\n';
  report += '─────────────────────────────────────────────────────────────────\n';
  report += `  Média: ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms\n`;
  report += `  Mínimo: ${data.metrics.http_req_duration.values.min.toFixed(2)}ms\n`;
  report += `  Máximo: ${data.metrics.http_req_duration.values.max.toFixed(2)}ms\n`;
  report += `  Mediana (P50): ${data.metrics.http_req_duration.values.med.toFixed(2)}ms\n`;
  report += `  P90: ${data.metrics.http_req_duration.values['p(90)'].toFixed(2)}ms\n`;
  report += `  P95: ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms\n`;
  report += `  P99: ${data.metrics.http_req_duration.values['p(99)'].toFixed(2)}ms\n\n`;
  
  report += '✅ VALIDAÇÃO DE THRESHOLDS\n';
  report += '─────────────────────────────────────────────────────────────────\n';
  const p95 = data.metrics.http_req_duration.values['p(95)'];
  report += `  P95 < 300ms: ${p95 < 300 ? '✓ PASS' : '✗ FAIL'} (${p95.toFixed(2)}ms)\n`;
  
  const errorRateValue = (data.metrics.errors?.values.rate || 0) * 100;
  report += `  Taxa de Erro < 5%: ${errorRateValue < 5 ? '✓ PASS' : '✗ FAIL'} (${errorRateValue.toFixed(2)}%)\n`;
  
  const checkRate = (data.metrics.checks?.values.rate || 0) * 100;
  report += `  Checks > 95%: ${checkRate > 95 ? '✓ PASS' : '✗ FAIL'} (${checkRate.toFixed(2)}%)\n\n`;
  
  report += '🎯 OBJETIVO: 1000 usuários/minuto\n';
  report += '─────────────────────────────────────────────────────────────────\n';
  const requestsPerMinute = data.metrics.http_reqs.values.rate * 60;
  report += `  Taxa Alcançada: ${requestsPerMinute.toFixed(0)} requisições/minuto\n`;
  report += `  Status: ${requestsPerMinute >= 1000 ? '✓ OBJETIVO ATINGIDO' : '⚠ ABAIXO DO OBJETIVO'}\n\n`;
  
  return report;
}

function generateHTMLReport(data) {
  const p95 = data.metrics.http_req_duration.values['p(95)'];
  const errorRateValue = (data.metrics.errors?.values.rate || 0) * 100;
  const checkRate = (data.metrics.checks?.values.rate || 0) * 100;
  const requestsPerMinute = data.metrics.http_reqs.values.rate * 60;
  
  return `
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Load Test Report - Charging API</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f5f5f5; padding: 20px; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #333; margin-bottom: 10px; }
        .date { color: #666; margin-bottom: 30px; }
        .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .metric-card { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; }
        .metric-card.success { background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); }
        .metric-card.warning { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }
        .metric-card.info { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); }
        .metric-label { font-size: 14px; opacity: 0.9; margin-bottom: 5px; }
        .metric-value { font-size: 32px; font-weight: bold; }
        .metric-unit { font-size: 16px; opacity: 0.8; }
        .section { margin-bottom: 30px; }
        .section-title { font-size: 24px; color: #333; margin-bottom: 15px; border-bottom: 2px solid #667eea; padding-bottom: 10px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f8f9fa; font-weight: 600; color: #333; }
        .pass { color: #38ef7d; font-weight: bold; }
        .fail { color: #f5576c; font-weight: bold; }
        .footer { text-align: center; color: #666; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🚀 Load Test Report - Charging API</h1>
        <p class="date">Data: ${new Date().toLocaleString('pt-BR')}</p>
        
        <div class="metrics-grid">
            <div class="metric-card">
                <div class="metric-label">Total de Requisições</div>
                <div class="metric-value">${data.metrics.http_reqs.values.count}</div>
            </div>
            <div class="metric-card success">
                <div class="metric-label">Taxa de Requisições</div>
                <div class="metric-value">${data.metrics.http_reqs.values.rate.toFixed(2)}<span class="metric-unit">/s</span></div>
            </div>
            <div class="metric-card info">
                <div class="metric-label">Tempo Médio</div>
                <div class="metric-value">${data.metrics.http_req_duration.values.avg.toFixed(2)}<span class="metric-unit">ms</span></div>
            </div>
            <div class="metric-card ${errorRateValue < 5 ? 'success' : 'warning'}">
                <div class="metric-label">Taxa de Erro</div>
                <div class="metric-value">${errorRateValue.toFixed(2)}<span class="metric-unit">%</span></div>
            </div>
        </div>
        
        <div class="section">
            <h2 class="section-title">⏱️ Tempo de Resposta</h2>
            <table>
                <tr><th>Métrica</th><th>Valor</th></tr>
                <tr><td>Mínimo</td><td>${data.metrics.http_req_duration.values.min.toFixed(2)}ms</td></tr>
                <tr><td>Máximo</td><td>${data.metrics.http_req_duration.values.max.toFixed(2)}ms</td></tr>
                <tr><td>Média</td><td>${data.metrics.http_req_duration.values.avg.toFixed(2)}ms</td></tr>
                <tr><td>Mediana (P50)</td><td>${data.metrics.http_req_duration.values.med.toFixed(2)}ms</td></tr>
                <tr><td>P90</td><td>${data.metrics.http_req_duration.values['p(90)'].toFixed(2)}ms</td></tr>
                <tr><td>P95</td><td>${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms</td></tr>
                <tr><td>P99</td><td>${data.metrics.http_req_duration.values['p(99)'].toFixed(2)}ms</td></tr>
            </table>
        </div>
        
        <div class="section">
            <h2 class="section-title">✅ Validação de Thresholds</h2>
            <table>
                <tr><th>Threshold</th><th>Esperado</th><th>Atual</th><th>Status</th></tr>
                <tr>
                    <td>P95 Tempo de Resposta</td>
                    <td>< 300ms</td>
                    <td>${p95.toFixed(2)}ms</td>
                    <td class="${p95 < 300 ? 'pass' : 'fail'}">${p95 < 300 ? '✓ PASS' : '✗ FAIL'}</td>
                </tr>
                <tr>
                    <td>Taxa de Erro</td>
                    <td>< 5%</td>
                    <td>${errorRateValue.toFixed(2)}%</td>
                    <td class="${errorRateValue < 5 ? 'pass' : 'fail'}">${errorRateValue < 5 ? '✓ PASS' : '✗ FAIL'}</td>
                </tr>
                <tr>
                    <td>Taxa de Checks</td>
                    <td>> 95%</td>
                    <td>${checkRate.toFixed(2)}%</td>
                    <td class="${checkRate > 95 ? 'pass' : 'fail'}">${checkRate > 95 ? '✓ PASS' : '✗ FAIL'}</td>
                </tr>
                <tr>
                    <td>Requisições por Minuto</td>
                    <td>> 1000/min</td>
                    <td>${requestsPerMinute.toFixed(0)}/min</td>
                    <td class="${requestsPerMinute >= 1000 ? 'pass' : 'fail'}">${requestsPerMinute >= 1000 ? '✓ PASS' : '⚠ ABAIXO'}</td>
                </tr>
            </table>
        </div>
        
        <div class="footer">
            <p>Relatório gerado por k6 - Performance Testing Tool</p>
        </div>
    </div>
</body>
</html>
`;
}

