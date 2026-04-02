cat > Scripts/run-migrations.sh << 'EOF'
#!/bin/bash

echo "Запуск миграций базы данных..."

cd Backend/CompanyHR.API

dotnet ef database update

echo "Миграции выполнены успешно!"
EOF