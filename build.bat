@echo off
REM XOR Encryptor Build Script
REM Собирает GUI и Console версии приложения

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║          XOR Encryptor - Скрипт сборки (Build Script)          ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

set "CONFIG=Release"
if "%1"=="" set "CONFIG=Debug"
if "%1"=="debug" set "CONFIG=Debug"
if "%1"=="release" set "CONFIG=Release"
if "%1"=="--debug" set "CONFIG=Debug"
if "%1"=="--release" set "CONFIG=Release"

echo Режим сборки: %CONFIG%
echo.

REM Clean previous builds
echo 🧹 Очистка предыдущих сборок...
rmdir /s /q bin obj >nul 2>&1

echo.
echo ┌────────────────────────────────────────────────────────────────┐
echo │ Сборка GUI версии (Avalonia)...                                │
echo └────────────────────────────────────────────────────────────────┘

dotnet build XorEncryptor.csproj -c %CONFIG%
if errorlevel 1 (
    echo.
    echo ❌ Ошибка при сборке GUI версии!
    exit /b 1
)
echo ✅ GUI версия собрана успешно!

echo.
echo ┌────────────────────────────────────────────────────────────────┐
echo │ Сборка Console версии...                                       │
echo └────────────────────────────────────────────────────────────────┘

dotnet build XorEncryptor.Console.csproj -c %CONFIG%
if errorlevel 1 (
    echo.
    echo ❌ Ошибка при сборке Console версии!
    exit /b 1
)
echo ✅ Console версия собрана успешно!

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                   ✅ Все версии собраны успешно!               ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

echo 📁 Выходные файлы:
echo    GUI:     bin\%CONFIG%\net8.0\XorEncryptor.exe
echo    Console: bin\%CONFIG%\net8.0\XorEncryptor.Console.exe
echo.

if "%CONFIG%"=="Debug" (
    echo 💡 Совет: Для сборки Release версии выполните:
    echo    build.bat release
)

endlocal
