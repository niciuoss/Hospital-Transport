#!/usr/bin/env bash
# Restaura o backup do banco no servidor Ubuntu, sem depender de copiar/colar
# comandos com aspas escapadas (isso é o que causou o erro "USERS" antes).
#
# Uso: rode este script DENTRO da pasta migracao-servidor/ (a que tem o
# docker-compose.ubuntu.yml, o .env.ubuntu e a subpasta backups/), no
# servidor Ubuntu:
#
#   cd hospital-transport-migracao
#   bash restaurar-backup.sh
#
# O que ele faz, na ordem certa:
#   1. Sobe SÓ o postgres (não sobe api/frontend ainda).
#   2. Espera o banco ficar "healthy".
#   3. Restaura o .sql mais recente da pasta backups/.
#   4. Confere as contagens de Users/Patients/Buses/Appointments.
#
# Só depois de ver as contagens aqui é que se deve rodar o Passo 5 do
# README-MIGRACAO.md (subir api + frontend).

set -euo pipefail
cd "$(dirname "$0")"

COMPOSE="docker compose --env-file .env.ubuntu -f docker-compose.ubuntu.yml"

echo "==> Subindo apenas o container do Postgres..."
$COMPOSE up -d postgres

echo "==> Esperando o Postgres ficar 'healthy'..."
for i in $(seq 1 30); do
  status=$(docker inspect --format='{{.State.Health.Status}}' hospital_transport_db 2>/dev/null || echo "starting")
  if [ "$status" = "healthy" ]; then
    echo "    Postgres pronto."
    break
  fi
  sleep 2
  if [ "$i" -eq 30 ]; then
    echo "!! Postgres não ficou 'healthy' a tempo. Rode 'docker ps' e 'docker logs hospital_transport_db' para investigar."
    exit 1
  fi
done

# Pega o .sql mais recente da pasta backups/ automaticamente
SQL_FILE=$(ls -t backups/*.sql 2>/dev/null | head -n 1 || true)
if [ -z "$SQL_FILE" ]; then
  echo "!! Nenhum arquivo .sql encontrado em backups/. Abortando."
  exit 1
fi
echo "==> Restaurando: $SQL_FILE"

# Checa se já existem tabelas (ex.: a API já subiu antes e criou o schema vazio
# via EF Core migrations). Se existir, avisa e para -- restaurar por cima de um
# schema já criado causa erros de "already exists" e dados incompletos.
existing_tables=$(docker exec hospital_transport_db psql -U postgres -d HospitalTransportDB -t -c \
  "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';" | tr -d '[:space:]')

if [ "${existing_tables:-0}" != "0" ]; then
  echo "!! O banco já tem $existing_tables tabela(s). Isso normalmente acontece quando a API"
  echo "   já subiu antes e criou o schema vazio sozinha (EF Core migrations)."
  echo "   Para restaurar com segurança, é preciso zerar esse volume vazio primeiro:"
  echo ""
  echo "     $COMPOSE down"
  echo "     docker volume rm hospital-transport-migracao_postgres_data"
  echo ""
  echo "   (isso NÃO mexe na máquina Windows, só apaga o volume novo e vazio daqui do Ubuntu)"
  echo "   Depois rode este script de novo."
  exit 1
fi

cat "$SQL_FILE" | docker exec -i hospital_transport_db psql -U postgres -d HospitalTransportDB

echo ""
echo "==> Conferindo as contagens:"
docker exec hospital_transport_db psql -U postgres -d HospitalTransportDB -t <<'SQL'
SELECT 'Users', count(*) FROM "Users"
UNION ALL SELECT 'Patients', count(*) FROM "Patients"
UNION ALL SELECT 'Buses', count(*) FROM "Buses"
UNION ALL SELECT 'Appointments', count(*) FROM "Appointments";
SQL

echo ""
echo "==> Se os números acima baterem com os do CONTEXTO-SESSAO.md, pode seguir para o Passo 5:"
echo "    $COMPOSE up -d"
