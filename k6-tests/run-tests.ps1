# Script PowerShell para executar testes de performance k6
# Uso: .\run-tests.ps1 [-TestType <smoke|load|stress|all>] [-BaseUrl <url>]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("smoke", "load", "stress", "all")]
    [string]$TestType = "all",
    
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5000"
)

# Cores para output
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorWarning = "Yellow"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Test-K6Installed {
    try {
        $null = Get-Command k6 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

function Test-ApiAvailable {
    param([string]$Url)
    
    try {
        $response = Invoke-WebRequest -Uri "$Url/api/usuarios" -Method Get -TimeoutSec 5 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

function Run-K6Test {
    param(
        [string]$TestFile,
        [string]$TestName,
        [string]$Url
    )
    
    Write-ColorOutput "`n╔════════════════════════════════════════════════════════════════╗" $ColorInfo
    Write-ColorOutput "║  Executando: $TestName" $ColorInfo
    Write-ColorOutput "╚════════════════════════════════════════════════════════════════╝`n" $ColorInfo
    
    $env:BASE_URL = $Url
    
    $startTime = Get-Date
    k6 run $TestFile
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "`n✓ $TestName concluído com sucesso em $($duration.TotalSeconds.ToString('0.00'))s" $ColorSuccess
    }
    else {
        Write-ColorOutput "`n✗ $TestName falhou!" $ColorError
    }
    
    return $LASTEXITCODE
}

# Banner
Write-ColorOutput @"

╔═══════════════════════════════════════════════════════════════╗
║                                                                ║
║     K6 Performance Tests - Charging API                       ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝

"@ $ColorInfo

# Verificar se k6 está instalado
Write-ColorOutput "🔍 Verificando dependências..." $ColorInfo

if (-not (Test-K6Installed)) {
    Write-ColorOutput "`n✗ k6 não está instalado!" $ColorError
    Write-ColorOutput "`nPara instalar o k6, execute:" $ColorWarning
    Write-ColorOutput "  winget install k6" $ColorWarning
    Write-ColorOutput "  ou visite: https://k6.io/docs/get-started/installation/`n" $ColorWarning
    exit 1
}

Write-ColorOutput "✓ k6 está instalado" $ColorSuccess

# Verificar se a API está disponível
Write-ColorOutput "`n🔍 Verificando disponibilidade da API em $BaseUrl..." $ColorInfo

if (-not (Test-ApiAvailable -Url $BaseUrl)) {
    Write-ColorOutput "`n✗ API não está disponível em $BaseUrl" $ColorError
    Write-ColorOutput "`nCertifique-se de que a API está rodando antes de executar os testes." $ColorWarning
    Write-ColorOutput "Execute: dotnet run --project ..\Charging.Api`n" $ColorWarning
    exit 1
}

Write-ColorOutput "✓ API está disponível" $ColorSuccess

# Criar diretório de resultados se não existir
$resultsDir = "results"
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

# Executar testes
$exitCode = 0

switch ($TestType) {
    "smoke" {
        $exitCode = Run-K6Test -TestFile "smoke-test.js" -TestName "Smoke Test" -Url $BaseUrl
    }
    "load" {
        $exitCode = Run-K6Test -TestFile "load-test.js" -TestName "Load Test" -Url $BaseUrl
    }
    "stress" {
        $exitCode = Run-K6Test -TestFile "stress-test.js" -TestName "Stress Test" -Url $BaseUrl
    }
    "all" {
        Write-ColorOutput "`n📋 Executando todos os testes...`n" $ColorInfo
        
        $smokeResult = Run-K6Test -TestFile "smoke-test.js" -TestName "Smoke Test" -Url $BaseUrl
        $loadResult = Run-K6Test -TestFile "load-test.js" -TestName "Load Test" -Url $BaseUrl
        $stressResult = Run-K6Test -TestFile "stress-test.js" -TestName "Stress Test" -Url $BaseUrl
        
        $exitCode = [Math]::Max([Math]::Max($smokeResult, $loadResult), $stressResult)
    }
}

# Resumo final
Write-ColorOutput "`n╔════════════════════════════════════════════════════════════════╗" $ColorInfo
Write-ColorOutput "║  Resumo da Execução" $ColorInfo
Write-ColorOutput "╚════════════════════════════════════════════════════════════════╝`n" $ColorInfo

if ($exitCode -eq 0) {
    Write-ColorOutput "✓ Todos os testes foram executados com sucesso!" $ColorSuccess
}
else {
    Write-ColorOutput "✗ Alguns testes falharam. Verifique os logs acima." $ColorError
}

Write-ColorOutput "`n📊 Relatórios salvos em:" $ColorInfo
Get-ChildItem -Filter "*.json" | ForEach-Object { Write-ColorOutput "   - $($_.Name)" $ColorWarning }
Get-ChildItem -Filter "*.html" | ForEach-Object { Write-ColorOutput "   - $($_.Name)" $ColorWarning }

Write-ColorOutput ""

exit $exitCode

