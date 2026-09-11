@echo off
chcp 65001 >nul
title Hospital Transport - Fixar IP desta máquina
color 0E

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║   FIXAR IP DESTA MÁQUINA EM 192.168.0.252 (IP ESTÁTICO)     ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo Isso vai configurar a placa de rede "Ethernet" com:
echo    IP:      192.168.0.252
echo    Máscara: 255.255.255.0
echo    Gateway: 192.168.0.1
echo    DNS:     192.168.0.1 (e 8.8.8.8 como reserva)
echo.
echo ⚠️  IMPORTANTE: confirme com quem administra a rede/roteador do hospital
echo    que o IP 192.168.0.252 NÃO está reservado para outra máquina, e que
echo    o roteador não vai tentar entregar esse mesmo IP por DHCP para outro
echo    aparelho (o ideal é excluir 192.168.0.252 da faixa de DHCP do roteador).
echo.
echo Este script precisa ser executado como ADMINISTRADOR.
echo.
pause

:: Verifica se está rodando como administrador
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    color 0C
    echo.
    echo ❌ ERRO: Este script precisa ser executado como Administrador.
    echo.
    echo Clique com o botão direito neste arquivo e escolha
    echo "Executar como administrador".
    echo.
    pause
    exit /b 1
)

echo.
echo Aplicando configuração...
netsh interface ip set address name="Ethernet" static 192.168.0.252 255.255.255.0 192.168.0.1 1
netsh interface ip set dns name="Ethernet" static 192.168.0.1 primary
netsh interface ip add dns name="Ethernet" 8.8.8.8 index=2

if errorlevel 1 (
    color 0C
    echo.
    echo ❌ Algo deu errado. Verifique se o nome da placa de rede ainda é
    echo    "Ethernet" ^(rode "ipconfig" para conferir^) e ajuste este arquivo
    echo    se o nome for diferente.
    echo.
    pause
    exit /b 1
)

color 0A
echo.
echo ✓ IP fixado com sucesso!
echo.
echo Conferindo:
ipconfig | findstr /C:"IPv4" /C:"Gateway"
echo.
echo A partir de agora, o sistema sempre estará em:
echo    http://192.168.0.252:3000
echo.
pause
