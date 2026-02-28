@echo off
chcp 65001 >nul
title Hospital Transport - Parando Sistema
color 0E

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          PARANDO SISTEMA...                                ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

docker-compose down

if errorlevel 1 (
    color 0C
    echo.
    echo ❌ ERRO ao parar o sistema!
    echo.
) else (
    color 0A
    echo.
    echo ✓ Sistema parado com sucesso!
    echo.
)

pause