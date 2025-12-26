#!/bin/bash

# Script Bash para gerenciar o Docker Compose
# Uso: ./docker-run.sh [comando]

COMMAND=${1:-up}

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}"
cat << "EOF"
╔═══════════════════════════════════════════════════════════════╗
║                                                                ║
║     Docker Compose - Charging API                             ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝
EOF
echo -e "${NC}"

case $COMMAND in
    up)
        echo -e "${CYAN}\n🚀 Iniciando containers (API + SQL Server)...\n${NC}"
        docker-compose up -d
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}\n✓ Containers iniciados com sucesso!\n${NC}"
            echo -e "${CYAN}📊 Serviços disponíveis:${NC}"
            echo -e "${YELLOW}   API:        http://localhost:5254${NC}"
            echo -e "${YELLOW}   Swagger:    http://localhost:5254/swagger${NC}"
            echo -e "${YELLOW}   SQL Server: localhost:1433${NC}"
            echo -e "${CYAN}\n💡 Use './docker-run.sh logs' para ver os logs\n${NC}"
        else
            echo -e "${RED}\n✗ Erro ao iniciar containers!\n${NC}"
        fi
        ;;
        
    down)
        echo -e "${CYAN}\n🛑 Parando e removendo containers...\n${NC}"
        docker-compose down
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}\n✓ Containers removidos com sucesso!\n${NC}"
        fi
        ;;
        
    test)
        echo -e "${CYAN}\n🧪 Executando TODOS os testes k6...\n${NC}"
        docker-compose --profile test up --abort-on-container-exit
        
        echo -e "${CYAN}\n📊 Resultados salvos em: k6-results/\n${NC}"
        ;;
        
    test-smoke)
        echo -e "${CYAN}\n🧪 Executando Smoke Test...\n${NC}"
        docker-compose run --rm k6 run /scripts/smoke-test.js
        
        echo -e "${GREEN}\n✓ Smoke test concluído!\n${NC}"
        ;;
        
    test-load)
        echo -e "${CYAN}\n🧪 Executando Load Test...\n${NC}"
        docker-compose run --rm k6-load run /scripts/load-test.js
        
        echo -e "${GREEN}\n✓ Load test concluído!\n${NC}"
        ;;
        
    test-stress)
        echo -e "${CYAN}\n🧪 Executando Stress Test...\n${NC}"
        docker-compose run --rm k6-stress run /scripts/stress-test.js
        
        echo -e "${GREEN}\n✓ Stress test concluído!\n${NC}"
        ;;
        
    logs)
        echo -e "${CYAN}\n📋 Exibindo logs (Ctrl+C para sair)...\n${NC}"
        docker-compose logs -f
        ;;
        
    build)
        echo -e "${CYAN}\n🔨 Rebuilding containers...\n${NC}"
        docker-compose build --no-cache
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}\n✓ Build concluído com sucesso!\n${NC}"
        fi
        ;;
        
    restart)
        echo -e "${CYAN}\n🔄 Reiniciando containers...\n${NC}"
        docker-compose restart
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}\n✓ Containers reiniciados!\n${NC}"
        fi
        ;;
        
    ps)
        echo -e "${CYAN}\n📊 Status dos containers:\n${NC}"
        docker-compose ps
        ;;
        
    *)
        echo -e "${RED}\n❌ Comando inválido!\n${NC}"
        echo -e "${CYAN}Comandos disponíveis:${NC}"
        echo -e "${YELLOW}  up           - Iniciar API e SQL Server${NC}"
        echo -e "${YELLOW}  down         - Parar e remover containers${NC}"
        echo -e "${YELLOW}  test         - Executar todos os testes k6${NC}"
        echo -e "${YELLOW}  test-smoke   - Executar smoke test${NC}"
        echo -e "${YELLOW}  test-load    - Executar load test${NC}"
        echo -e "${YELLOW}  test-stress  - Executar stress test${NC}"
        echo -e "${YELLOW}  logs         - Ver logs dos containers${NC}"
        echo -e "${YELLOW}  build        - Rebuild dos containers${NC}"
        echo -e "${YELLOW}  restart      - Reiniciar containers${NC}"
        echo -e "${YELLOW}  ps           - Ver status dos containers\n${NC}"
        ;;
esac

