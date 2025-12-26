// Stress Test - Teste de estresse
// Aumenta gradualmente a carga até encontrar o ponto de quebra

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

// ═══════════════════════════════════════════════════════════════
// CONFIGURAÇÕES - Altere aqui para ajustar os thresholds
// ═══════════════════════════════════════════════════════════════
const CONFIG = {
  // Estágios progressivos de estresse
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
  
  // Thresholds de estresse (mais permissivos)
  THRESHOLD_P95_MS: 500,            // P95 pode chegar a 500ms
  THRESHOLD_P99_MS: 1000,           // P99 pode chegar a 1000ms
  THRESHOLD_ERROR_RATE_P95: 0.05,   // Aceita até 5% de erro no P95
  THRESHOLD_ERROR_RATE_P99: 0.1,    // Aceita até 10% de erro no P99
  
  // Distribuição de cenários
  SCENARIO_POST_PERCENTAGE: 0.5,    // 50% POST
  SCENARIO_GET_ALL_PERCENTAGE: 0.3, // 30% GET ALL
  // 20% restante é GET BY ID
  
  BASE_URL: __ENV.BASE_URL || 'http://localhost:5000',
};

const errorRate = new Rate('errors');
const successfulRequests = new Counter('successful_requests');

export const options = {
  stages: [
    { duration: CONFIG.STAGE_1_DURATION, target: CONFIG.STAGE_1_TARGET_VUS },
    { duration: CONFIG.STAGE_2_DURATION, target: CONFIG.STAGE_2_TARGET_VUS },
    { duration: CONFIG.STAGE_3_DURATION, target: CONFIG.STAGE_3_TARGET_VUS },
    { duration: CONFIG.STAGE_4_DURATION, target: CONFIG.STAGE_4_TARGET_VUS },
    { duration: CONFIG.STAGE_5_DURATION, target: CONFIG.STAGE_5_TARGET_VUS },
    { duration: CONFIG.RAMP_DOWN_DURATION, target: 0 },
  ],
  thresholds: {
    http_req_duration: [
      `p(95)<${CONFIG.THRESHOLD_P95_MS}`,
      `p(99)<${CONFIG.THRESHOLD_P99_MS}`,
    ],
    http_req_failed: [`rate<=${CONFIG.THRESHOLD_ERROR_RATE_P99}`],
    errors: [
      `rate<=${CONFIG.THRESHOLD_ERROR_RATE_P95}`,
      `rate<=${CONFIG.THRESHOLD_ERROR_RATE_P99}`,
    ],
  },
};

const BASE_URL = CONFIG.BASE_URL;

export default function () {
  const payload = JSON.stringify({
    nome: `Stress User ${Date.now()}-${Math.random()}`,
    email: `stress-${Date.now()}-${Math.random()}@email.com`,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // Mix de operações CRUD (configurável via CONFIG)
  const scenario = Math.random();
  
  if (scenario < CONFIG.SCENARIO_POST_PERCENTAGE) {
    // POST
    const response = http.post(`${BASE_URL}/api/usuarios`, payload, params);
    const success = check(response, {
      'status é 201': (r) => r.status === 201,
    });
    if (success) successfulRequests.add(1);
    errorRate.add(!success);
  } else if (scenario < (CONFIG.SCENARIO_POST_PERCENTAGE + CONFIG.SCENARIO_GET_ALL_PERCENTAGE)) {
    // GET ALL
    const response = http.get(`${BASE_URL}/api/usuarios`);
    const success = check(response, {
      'status é 200': (r) => r.status === 200,
    });
    if (success) successfulRequests.add(1);
    errorRate.add(!success);
  } else {
    // GET BY ID
    const createResponse = http.post(`${BASE_URL}/api/usuarios`, payload, params);
    if (createResponse.status === 201) {
      const id = createResponse.json('id');
      const response = http.get(`${BASE_URL}/api/usuarios/${id}`);
      const success = check(response, {
        'status é 200': (r) => r.status === 200,
      });
      if (success) successfulRequests.add(1);
      errorRate.add(!success);
    }
  }

  sleep(0.1);
}

export function handleSummary(data) {
  return {
    'summary-stress.json': JSON.stringify(data),
    stdout: generateStressReport(data),
  };
}

function generateStressReport(data) {
  let report = '\n';
  report += '╔═══════════════════════════════════════════════════════════════╗\n';
  report += '║          STRESS TEST - Análise de Quebra                     ║\n';
  report += '╚═══════════════════════════════════════════════════════════════╝\n\n';
  
  report += `Total de Requisições: ${data.metrics.http_reqs.values.count}\n`;
  report += `Taxa Máxima: ${data.metrics.http_reqs.values.rate.toFixed(2)}/s\n`;
  report += `P95 Latência: ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms\n`;
  report += `P99 Latência: ${data.metrics.http_req_duration.values['p(99)'].toFixed(2)}ms\n`;
  report += `Taxa de Erro: ${((data.metrics.errors?.values.rate || 0) * 100).toFixed(2)}%\n\n`;
  
  return report;
}

