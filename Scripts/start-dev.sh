cat > Scripts/start-dev.sh << 'EOF'
#!/bin/bash

echo "Запуск среды разработки Company HR..."

export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1

mkdir -p logs

echo "Восстановление зависимостей .NET..."
dotnet restore

echo "Сборка проекта..."
dotnet build

echo "Запуск API..."
dotnet run --project Backend/CompanyHR.API/CompanyHR.API.csproj
EOF