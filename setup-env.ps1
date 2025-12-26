# Script PowerShell para configurar arquivo .env
# Uso: .\setup-env.ps1

$ColorInfo = "Cyan"
$ColorSuccess = "Green"
$ColorWarning = "Yellow"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput @"

╔═══════════════════════════════════════════════════════════════╗
║                                                                ║
║     Setup Variáveis de Ambiente - Charging API                ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝

"@ $ColorInfo

# Verificar se .env já existe
if (Test-Path ".env") {
    Write-ColorOutput "⚠️  Arquivo .env já existe!" $ColorWarning
    $response = Read-Host "Deseja sobrescrever? (s/N)"
    
    if ($response -ne "s" -and $response -ne "S") {
        Write-ColorOutput "`n✓ Operação cancelada. Arquivo .env mantido.`n" $ColorInfo
        exit 0
    }
}

# Criar .env a partir do .env.example
Write-ColorOutput "`n📝 Criando arquivo .env...`n" $ColorInfo

$envContent = @"
# ═══════════════════════════════════════════════════════════════
# VARIÁVEIS DE AMBIENTE - Charging API
# ═══════════════════════════════════════════════════════════════
# 
# ATENÇÃO: Este arquivo contém informações sensíveis!
# NÃO commitar este arquivo ao Git
# ═══════════════════════════════════════════════════════════════

# ───────────────────────────────────────────────────────────────
# SQL SERVER
# ───────────────────────────────────────────────────────────────
MSSQL_SA_PASSWORD=yourStrong(!)Password123
MSSQL_PID=Express
MSSQL_PORT=1433

# ───────────────────────────────────────────────────────────────
# DATABASE
# ───────────────────────────────────────────────────────────────
DB_SERVER=sqlserver
DB_PORT=1433
DB_NAME=Charging
DB_USER=sa
DB_TRUST_CERTIFICATE=True

# ───────────────────────────────────────────────────────────────
# API
# ───────────────────────────────────────────────────────────────
API_PORT=5254
API_INTERNAL_PORT=80
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80

# ───────────────────────────────────────────────────────────────
# K6 TESTS
# ───────────────────────────────────────────────────────────────
K6_BASE_URL=http://api:80
K6_RESULTS_PATH=/results

# ───────────────────────────────────────────────────────────────
# DOCKER
# ───────────────────────────────────────────────────────────────
COMPOSE_PROJECT_NAME=charging-api
DOCKER_BUILDKIT=1
COMPOSE_DOCKER_CLI_BUILD=1

# ───────────────────────────────────────────────────────────────
# HEALTHCHECK
# ───────────────────────────────────────────────────────────────
HEALTHCHECK_INTERVAL=10s
HEALTHCHECK_TIMEOUT=5s
HEALTHCHECK_RETRIES=5
HEALTHCHECK_START_PERIOD=30s

# ───────────────────────────────────────────────────────────────
# VOLUMES
# ───────────────────────────────────────────────────────────────
SQLSERVER_VOLUME=sqlserver-data
"@

# Escrever arquivo
$envContent | Out-File -FilePath ".env" -Encoding UTF8

Write-ColorOutput "✓ Arquivo .env criado com sucesso!" $ColorSuccess
Write-ColorOutput "`n📋 Configurações:`n" $ColorInfo
Write-ColorOutput "   SQL Server: localhost:1433" $ColorWarning
Write-ColorOutput "   SA Password: yourStrong(!)Password123" $ColorWarning
Write-ColorOutput "   API: http://localhost:5254" $ColorWarning
Write-ColorOutput "   Swagger: http://localhost:5254/swagger" $ColorWarning

Write-ColorOutput "`n⚠️  IMPORTANTE:" $ColorWarning
Write-ColorOutput "   - Altere a senha do SQL Server para produção!" $ColorWarning
Write-ColorOutput "   - NÃO commite o arquivo .env ao Git!" $ColorWarning
Write-ColorOutput "   - O arquivo já está no .gitignore`n" $ColorWarning

Write-ColorOutput "🚀 Próximos passos:" $ColorInfo
Write-ColorOutput "   1. (Opcional) Edite .env: code .env" $ColorInfo
Write-ColorOutput "   2. Inicie o ambiente: .\docker-run.ps1 up" $ColorInfo
Write-ColorOutput "   3. Execute testes: .\docker-run.ps1 test-smoke`n" $ColorInfo

Write-ColorOutput "📖 Documentação: ENV-SETUP.md`n" $ColorInfo

