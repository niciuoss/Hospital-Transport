batch@echo off
chcp 65001 >nul
title Hospital Transport - Atualizando Sistema
color 0E

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          ATUALIZANDO SISTEMA...                            ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

echo [1/4] Parando sistema...
docker-compose down
echo ✓ Sistema parado

echo.
echo [2/4] Removendo imagens antigas...
docker-compose down --rmi all
echo ✓ Imagens removidas

echo.
echo [3/4] Reconstruindo imagens...
docker-compose build --no-cache
echo ✓ Imagens reconstruídas

echo.
echo [4/4] Iniciando sistema atualizado...
docker-compose up -d
echo ✓ Sistema iniciado

echo.
echo Aguardando 25 segundos...
timeout /t 25 /nobreak >nul

color 0A
echo.
echo ✓ Sistema atualizado e iniciado com sucesso!
echo.
echo 🌐 Front-end: http://localhost:3000
echo.
pause