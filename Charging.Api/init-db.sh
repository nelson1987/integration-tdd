#!/bin/bash

echo "Aguardando SQL Server ficar pronto..."
sleep 10

echo "Criando banco de dados se não existir..."

# Verificar se o banco existe e criar se necessário
# Isso será feito através do EF Core na primeira execução da API
echo "Banco será criado automaticamente pelo EF Core"

