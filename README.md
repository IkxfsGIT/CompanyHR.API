# CompanyHR.API
API для учёта сотрудников компании. Создаётся при помощи Visual Studio Code, терминал GitBash, на Windows 10/11. Backend: C# (возможна смена на другой язык, например, Node.js + Express). Frontend: Flutter. Дизайн делается в Figma.

## Запуск проекта

1. Восстановление зависимостей, сборка и запуск API:
   ```bash
   dotnet run --project Tools/CompanyHR.Tools -- start-dev
   
---

## 3. Удалите скрипты из системы контроля версий (если они были закоммичены)

Если папка `Scripts` уже была в репозитории, нужно удалить её из Git, чтобы она не мешала:

```bash
git rm -r Scripts
git commit -m "Удалить устаревшие bash-скрипты, заменить на C#-утилиту Tools"