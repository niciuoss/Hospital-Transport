@echo off
chcp 65001 >nul
title Hospital Transport - Logs do Sistema
color 0B

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          LOGS DO SISTEMA                                   ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo Pressione Ctrl+C para sair
echo.

cd /d "%~dp0"

docker-compose logs -f