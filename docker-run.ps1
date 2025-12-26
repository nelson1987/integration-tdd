# Script PowerShell para gerenciar o Docker Compose
# Uso: .\docker-run.ps1 [comando]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("up", "down", "test", "test-smoke", "test-load", "test-stress", "logs", "build", "restart", "ps")]
    [string]$Command = "up"
)

$ColorInfo = "Cyan"
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorWarning = "Yellow"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput @"

╔═══════════════════════════════════════════════════════════════╗
║                                                                ║
║     Docker Compose - Charging API                             ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝

"@ $ColorInfo

switch ($Command) {
    "up" {
        Write-ColorOutput "`n🚀 Iniciando containers (API + SQL Server)...`n" $ColorInfo
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "`n✓ Containers iniciados com sucesso!`n" $ColorSuccess
            Write-ColorOutput "📊 Serviços disponíveis:" $ColorInfo
            Write-ColorOutput "   API:        http://localhost:5254" $ColorWarning
            Write-ColorOutput "   Swagger:    http://localhost:5254/swagger" $ColorWarning
            Write-ColorOutput "   SQL Server: localhost:1433" $ColorWarning
            Write-ColorOutput "`n💡 Use '.\docker-run.ps1 logs' para ver os logs`n" $ColorInfo
        } else {
            Write-ColorOutput "`n✗ Erro ao iniciar containers!`n" $ColorError
        }
    }
    
    "down" {
        Write-ColorOutput "`n🛑 Parando e removendo containers...`n" $ColorInfo
        docker-compose down
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "`n✓ Containers removidos com sucesso!`n" $ColorSuccess
        }
    }
    
    "test" {
        Write-ColorOutput "`n🧪 Executando TODOS os testes k6...`n" $ColorInfo
        docker-compose --profile test up --abort-on-container-exit
        
        Write-ColorOutput "`n📊 Resultados salvos em: k6-results/`n" $ColorInfo
    }
    
    "test-smoke" {
        Write-ColorOutput "`n🧪 Executando Smoke Test...`n" $ColorInfo
        docker-compose run --rm k6 run /scripts/smoke-test.js
        
        Write-ColorOutput "`n✓ Smoke test concluído!`n" $ColorSuccess
    }
    
    "test-load" {
        Write-ColorOutput "`n🧪 Executando Load Test...`n" $ColorInfo
        docker-compose run --rm k6-load run /scripts/load-test.js
        
        Write-ColorOutput "`n✓ Load test concluído!`n" $ColorSuccess
    }
    
    "test-stress" {
        Write-ColorOutput "`n🧪 Executando Stress Test...`n" $ColorInfo
        docker-compose run --rm k6-stress run /scripts/stress-test.js
        
        Write-ColorOutput "`n✓ Stress test concluído!`n" $ColorSuccess
    }
    
    "logs" {
        Write-ColorOutput "`n📋 Exibindo logs (Ctrl+C para sair)...`n" $ColorInfo
        docker-compose logs -f
    }
    
    "build" {
        Write-ColorOutput "`n🔨 Rebuilding containers...`n" $ColorInfo
        docker-compose build --no-cache
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "`n✓ Build concluído com sucesso!`n" $ColorSuccess
        }
    }
    
    "restart" {
        Write-ColorOutput "`n🔄 Reiniciando containers...`n" $ColorInfo
        docker-compose restart
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "`n✓ Containers reiniciados!`n" $ColorSuccess
        }
    }
    
    "ps" {
        Write-ColorOutput "`n📊 Status dos containers:`n" $ColorInfo
        docker-compose ps
    }
    
    default {
        Write-ColorOutput "`n❌ Comando inválido!`n" $ColorError
        Write-ColorOutput "Comandos disponíveis:" $ColorInfo
        Write-ColorOutput "  up           - Iniciar API e SQL Server" $ColorWarning
        Write-ColorOutput "  down         - Parar e remover containers" $ColorWarning
        Write-ColorOutput "  test         - Executar todos os testes k6" $ColorWarning
        Write-ColorOutput "  test-smoke   - Executar smoke test" $ColorWarning
        Write-ColorOutput "  test-load    - Executar load test" $ColorWarning
        Write-ColorOutput "  test-stress  - Executar stress test" $ColorWarning
        Write-ColorOutput "  logs         - Ver logs dos containers" $ColorWarning
        Write-ColorOutput "  build        - Rebuild dos containers" $ColorWarning
        Write-ColorOutput "  restart      - Reiniciar containers" $ColorWarning
        Write-ColorOutput "  ps           - Ver status dos containers`n" $ColorWarning
    }
}

