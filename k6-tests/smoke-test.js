// Smoke Test - Teste de fumaça rápido
// 5 requests em 1 segundo com máximo de 100ms de resposta

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// ═══════════════════════════════════════════════════════════════
// CONFIGURAÇÕES - Altere aqui para ajustar os thresholds
// ═══════════════════════════════════════════════════════════════
const CONFIG = {
  VUS: 5,                           // Usuários virtuais simultâneos
  DURATION: '1s',                   // Duração do teste
  THRESHOLD_P95_MS: 100,            // P95 deve ser menor que 100ms
  THRESHOLD_P99_MS: 150,            // P99 deve ser menor que 150ms
  THRESHOLD_ERROR_RATE: 0.0,        // Taxa de erro deve ser 0% (praticamente zero)
  THRESHOLD_ERROR_TOLERANCE: 0.005, // Tolerância de erro P99: 0.5%
  BASE_URL: __ENV.BASE_URL || 'http://localhost:5000',
};

// Métricas customizadas
const errorRate = new Rate('errors');

// Configuração do teste
export const options = {
  vus: CONFIG.VUS,
  duration: CONFIG.DURATION,
  thresholds: {
    http_req_duration: [
      `p(95)<${CONFIG.THRESHOLD_P95_MS}`,  // P95 das requisições
      `p(99)<${CONFIG.THRESHOLD_P99_MS}`,  // P99 das requisições
    ],
    http_req_failed: [`rate<=${CONFIG.THRESHOLD_ERROR_RATE}`], // Taxa de falha HTTP
    errors: [`rate<=${CONFIG.THRESHOLD_ERROR_RATE}`],          // Taxa de erro geral no P95
    checks: ['rate>0.99'],                                      // 99% dos checks devem passar
  },
};

const BASE_URL = CONFIG.BASE_URL;

export default function () {
  // Dados para criar usuário
  const payload = JSON.stringify({
    nome: `Usuario Teste ${Date.now()}-${Math.random()}`,
    email: `user-${Date.now()}-${Math.random()}@email.com`,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // POST - Criar usuário
  let response = http.post(`${BASE_URL}/api/usuarios`, payload, params);
  
  const checkPost = check(response, {
    'POST status é 201': (r) => r.status === 201,
    'POST response tem ID': (r) => r.json('id') !== null,
    'POST response tem Nome': (r) => r.json('nome') !== null,
    'POST response tem Email': (r) => r.json('email') !== null,
    'POST tempo < 100ms': (r) => r.timings.duration < 100,
  });
  
  errorRate.add(!checkPost);

  if (response.status === 201) {
    const usuarioId = response.json('id');

    // GET - Buscar usuário por ID
    response = http.get(`${BASE_URL}/api/usuarios/${usuarioId}`);
    
    const checkGet = check(response, {
      'GET status é 200': (r) => r.status === 200,
      'GET retorna usuário correto': (r) => r.json('id') === usuarioId,
      'GET tempo < 100ms': (r) => r.timings.duration < 100,
    });
    
    errorRate.add(!checkGet);
  }

  // GET ALL - Listar todos usuários
  response = http.get(`${BASE_URL}/api/usuarios`);
  
  const checkGetAll = check(response, {
    'GET ALL status é 200': (r) => r.status === 200,
    'GET ALL retorna array': (r) => Array.isArray(r.json()),
    'GET ALL tempo < 100ms': (r) => r.timings.duration < 100,
  });
  
  errorRate.add(!checkGetAll);
}

export function handleSummary(data) {
  return {
    'summary-smoke.json': JSON.stringify(data),
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
  };
}

function textSummary(data, options) {
  const indent = options.indent || '';
  const enableColors = options.enableColors || false;
  
  let summary = '\n';
  summary += `${indent}Smoke Test - Resumo\n`;
  summary += `${indent}${'='.repeat(50)}\n\n`;
  
  summary += `${indent}Requisições:\n`;
  summary += `${indent}  Total: ${data.metrics.http_reqs.values.count}\n`;
  summary += `${indent}  Taxa: ${data.metrics.http_reqs.values.rate.toFixed(2)}/s\n\n`;
  
  summary += `${indent}Duração das Requisições:\n`;
  summary += `${indent}  Média: ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms\n`;
  summary += `${indent}  Mínima: ${data.metrics.http_req_duration.values.min.toFixed(2)}ms\n`;
  summary += `${indent}  Máxima: ${data.metrics.http_req_duration.values.max.toFixed(2)}ms\n`;
  summary += `${indent}  P95: ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms\n\n`;
  
  summary += `${indent}Taxa de Erros: ${(data.metrics.errors?.values.rate * 100 || 0).toFixed(2)}%\n`;
  
  return summary;
}

