@echo off
REM XOR Encryptor Run Script
REM Запускает GUI или Console версию

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║        XOR Encryptor - Выбор версии для запуска                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

echo Выберите версию:
echo   1. GUI (Graphical User Interface)
echo   2. Console (Консольная версия)
echo   3. Выход
echo.

set /p choice="Ваш выбор (1-3): "

if "%choice%"=="1" goto gui
if "%choice%"=="2" goto console
if "%choice%"=="3" goto exit
if "%choice%"=="" goto menu
goto invalid

:gui
echo.
echo 🚀 Запуск GUI версии...
echo.

if exist "bin\Debug\net8.0\XorEncryptor.exe" (
    start bin\Debug\net8.0\XorEncryptor.exe
    timeout /t 2 /nobreak
    echo ✅ GUI приложение запущено
) else (
    echo ❌ GUI исполняемый файл не найден!
    echo 💡 Пожалуйста, сначала соберите проект: build.bat
    pause
)
goto end

:console
echo.
echo 🚀 Запуск Console версии...
echo.

if exist "bin\Debug\net8.0\XorEncryptor.Console.exe" (
    bin\Debug\net8.0\XorEncryptor.Console.exe
    goto end
) else (
    echo ❌ Console исполняемый файл не найден!
    echo 💡 Пожалуйста, сначала соберите проект: build.bat
    pause
    goto menu
)

:invalid
echo ❌ Неверный выбор. Пожалуйста, выберите 1, 2 или 3.
timeout /t 2 /nobreak
goto menu

:menu
cls
goto start

:end
endlocal
