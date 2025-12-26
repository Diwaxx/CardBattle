@echo off
chcp 65001 >nul
echo.
echo ╔══════════════════════════════════════╗
echo ║        🎴 CARDGAME SERVER            ║
echo ║          Запуск сервера              ║
echo ╚══════════════════════════════════════╝
echo.

:: Проверка .NET
echo [1/4] Проверяю установку .NET...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ ОШИБКА: .NET 8 не установлен!
    echo.
    echo Установите .NET 8 SDK отсюда:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo После установки перезапустите этот скрипт.
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET %DOTNET_VERSION% установлен

:: Создание конфигурации
echo.
echo [2/4] Настраиваю конфигурацию...
if not exist "appsettings.Development.json" (
    if exist "appsettings.Example.json" (
        copy "appsettings.Example.json" "appsettings.Development.json" >nul
        echo ✅ Создал файл настроек
    ) else (
        echo ❌ Файл appsettings.Example.json не найден!
        pause
        exit /b 1
    )
)

:: Восстановление пакетов
echo.
echo [3/4] Восстанавливаю пакеты NuGet...
dotnet restore >nul 2>&1
if errorlevel 1 (
    echo ⚠️ Предупреждение: Проблемы с восстановлением пакетов
    echo Продолжаю запуск...
)

:: Запуск сервера
echo.
echo [4/4] Запускаю сервер...
echo.
echo ════════════════════════════════════════════
echo 🌐 СЕРВЕР ЗАПУЩЕН НА: http://localhost:5000
echo 📚 Документация API:    http://localhost:5000/swagger
echo ❤️  Проверка здоровья:  http://localhost:5000/health
echo ════════════════════════════════════════════
echo.
echo 📝 Для остановки сервера нажмите: Ctrl+C
echo 📁 База данных создана: cardgame.db
echo.
echo ⏳ Запуск... (это может занять несколько секунд)
echo.

:: Запуск сервера
dotnet run --urls "http://localhost:5000"