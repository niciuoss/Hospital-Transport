@echo off
chcp 65001 >nul
title Hospital Transport - Reiniciando Sistema
color 0E

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          REINICIANDO SISTEMA...                            ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

echo [1/2] Parando sistema...
docker-compose down
echo ✓ Sistema parado

echo.
echo [2/2] Iniciando sistema...
docker-compose up -d
echo ✓ Sistema iniciado

echo.
echo Aguardando 20 segundos...
timeout /t 20 /nobreak >nul

color 0A
echo.
echo ✓ Sistema reiniciado com sucesso!
echo.
echo 🌐 Front-end: http://localhost:3000
echo.
pause