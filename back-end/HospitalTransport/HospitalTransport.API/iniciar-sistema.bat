@echo off
chcp 65001 >nul
title Hospital Transport - Iniciando Sistema
color 0A

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          HOSPITAL TRANSPORT - SISTEMA                      ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

:: Verifica se o Docker está instalado
echo [1/5] Verificando Docker...
docker --version >nul 2>&1
if errorlevel 1 (
    color 0C
    echo.
    echo ❌ ERRO: Docker não está instalado!
    echo.
    pause
    exit /b 1
)
echo ✓ Docker encontrado

:: Aguarda o Docker estar completamente pronto
echo.
echo [2/5] Aguardando Docker iniciar completamente...
set MAX_ATTEMPTS=30
set ATTEMPT=0

:wait_docker
set /a ATTEMPT+=1
docker ps >nul 2>&1
if errorlevel 1 (
    if %ATTEMPT% GEQ %MAX_ATTEMPTS% (
        color 0C
        echo.
        echo ❌ ERRO: Docker não iniciou após %MAX_ATTEMPTS% tentativas
        echo.
        echo Por favor:
        echo 1. Abra o Docker Desktop manualmente
        echo 2. Aguarde aparecer "Engine running"
        echo 3. Execute este script novamente
        echo.
        pause
        exit /b 1
    )
    echo Tentativa %ATTEMPT%/%MAX_ATTEMPTS%... aguardando 2 segundos
    timeout /t 2 /nobreak >nul
    goto wait_docker
)
echo ✓ Docker está pronto!

:: Navega para a pasta do docker-compose
cd /d "%~dp0"

:: Para containers antigos (se existirem)
echo.
echo [3/5] Parando containers antigos...
docker-compose down >nul 2>&1
echo ✓ Containers antigos parados

:: Inicia o sistema
echo.
echo [4/5] Iniciando sistema...
docker-compose up -d

if errorlevel 1 (
    color 0C
    echo.
    echo ❌ ERRO ao iniciar o sistema!
    echo.
    echo Verifique se o Docker Desktop está aberto e em execução.
    echo.
    pause
    exit /b 1
)

echo ✓ Containers iniciados

:: Aguarda os serviços ficarem prontos
echo.
echo [5/5] Aguardando serviços iniciarem...
timeout /t 25 /nobreak >nul

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║          SISTEMA INICIADO COM SUCESSO!                     ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo 🌐 Front-end:  http://localhost:3000
echo 🔌 Back-end:   http://localhost:8088
echo 📊 Swagger:    http://localhost:8088/swagger
echo.
echo ⚠️  IMPORTANTE: NÃO FECHE ESTA JANELA!
echo     O sistema está rodando em segundo plano.
echo.
echo Para parar o sistema, execute: parar-sistema.bat
echo.
pause