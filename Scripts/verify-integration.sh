cat > Scripts/verify-integration.sh << 'EOF'
#!/bin/bash

echo "Проверка целостности проекта..."

cd Backend/CompanyHR.API

# Проверка наличия обязательных пакетов
REQUIRED_PACKAGES=("AutoMapper.Extensions.Microsoft.DependencyInjection" "FluentValidation.AspNetCore" "Serilog.AspNetCore")
for pkg in "${REQUIRED_PACKAGES[@]}"; do
    if ! dotnet list package | grep -q "$pkg"; then
        echo "Отсутствует пакет: $pkg"
        dotnet add package "$pkg"
    else
        echo "Пакет $pkg установлен"
    fi
done

# Проверка сборки
echo "Выполняется сборка..."
dotnet build --no-restore
if [ $? -eq 0 ]; then
    echo "Сборка успешна"
else
    echo "Ошибка сборки"
    exit 1
fi

echo "Проект готов!"
EOF
