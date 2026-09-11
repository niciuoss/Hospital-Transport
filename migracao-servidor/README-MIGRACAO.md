# Migração do Hospital Transport para o servidor Ubuntu (192.168.0.72)

> Este guia foi escrito para você (ou o TI) executar **diretamente no servidor Ubuntu**.
> Eu (Claude) não tenho acesso a essa máquina, então preparei tudo pronto: as imagens
> Docker já buildadas (.tar), o backup do banco, e o docker-compose com portas que não
> conflitam com o sistema que já roda lá na porta 3000.

## Resumo do plano

- O sistema já existente no Ubuntu continua rodando normalmente na porta **3000**.
- O Hospital Transport passa a rodar nas portas **3001** (site), **8089** (API) e **5440** (Postgres) — nenhuma dessas conflita com o sistema atual.
- O banco é migrado por **pg_dump** (backup lógico, mesmo formato entre servidores, seguro e testado).
- Nada é apagado da máquina Windows atual — ela continua funcionando em paralelo até vocês confirmarem que tudo está OK no Ubuntu.

## O que está nesta pasta

```
migracao-servidor/
├── README-MIGRACAO.md          ← este guia
├── .env.ubuntu                  ← portas/IP para o Ubuntu (3001/8089/5440)
├── docker-compose.ubuntu.yml    ← compose usando as imagens .tar (não builda nada)
└── backups/                     ← NÃO vai pro GitHub (arquivos grandes)
    ├── hospital_transport_api.tar
    ├── hospital_transport_frontend_ubuntu.tar   (já com o IP 192.168.0.72 embutido)
    ├── postgres_15.tar
    └── hospital_transport_db_<data>.sql          (backup do banco atual)
```

**Antes de transferir:** confira que a pasta `backups/` está atualizada (os .tar são gerados
a partir do estado do sistema no momento em que foram exportados). Se o sistema mudou desde
então, peça para o Claude gerar um backup novo antes de migrar de verdade.

## Passo 0 — Levar os arquivos até o servidor Ubuntu

Copie a pasta `migracao-servidor/` inteira (com a subpasta `backups/`) para o servidor Ubuntu,
por pendrive, rede (`scp`/compartilhamento) ou como preferir. Ela tem ~380 MB por causa das
imagens Docker.

```bash
# Exemplo via scp, rodando no Windows (Git Bash) — ajuste o usuário/caminho:
scp -r migracao-servidor usuario@192.168.0.72:/home/usuario/hospital-transport-migracao
```

## Passo 1 — Checar que não há conflito de nomes no Ubuntu

No servidor Ubuntu, antes de tudo:

```bash
docker ps -a
docker volume ls
docker network ls
```

Confirme que não existem contêiner/volume/rede chamados `hospital_transport_*` já em uso
pelo sistema existente. Se o Docker não estiver instalado no Ubuntu, instale primeiro
(https://docs.docker.com/engine/install/ubuntu/) e garanta que o Docker Compose v2 está
disponível (`docker compose version`).

## Passo 2 — Carregar as imagens Docker (.tar)

Dentro da pasta copiada no Ubuntu:

```bash
cd hospital-transport-migracao/backups
docker load -i hospital_transport_api.tar
docker load -i hospital_transport_frontend_ubuntu.tar
docker load -i postgres_15.tar
docker images | grep hospitaltransportapi
```

Você deve ver `hospitaltransportapi-api:latest` e `hospitaltransportapi-frontend:ubuntu`.

## Passo 3 — Subir o banco vazio e restaurar o backup

```bash
cd hospital-transport-migracao
docker compose --env-file .env.ubuntu -f docker-compose.ubuntu.yml up -d postgres

# Espera o banco ficar "healthy" (uns 10-15s), depois confirme:
docker ps   # STATUS deve mostrar "healthy" para hospital_transport_db

# Restaura o backup (ajuste o nome do arquivo .sql pela data mais recente em backups/):
cat backups/hospital_transport_db_<DATA>.sql | docker exec -i hospital_transport_db psql -U postgres -d HospitalTransportDB
```

## Passo 4 — Conferir que os dados vieram certo (NENHUM dado pode faltar)

```bash
docker exec hospital_transport_db psql -U postgres -d HospitalTransportDB -t -c "
SELECT 'Users', count(*) FROM \"Users\"
UNION ALL SELECT 'Patients', count(*) FROM \"Patients\"
UNION ALL SELECT 'Buses', count(*) FROM \"Buses\"
UNION ALL SELECT 'Appointments', count(*) FROM \"Appointments\";"
```

Compare com os números registrados no backup do dia da migração (anotados no
`CONTEXTO-SESSAO.md`, seção "Migração para Ubuntu"). Os números devem bater exatamente.

## Passo 5 — Subir a API e o front-end

```bash
docker compose --env-file .env.ubuntu -f docker-compose.ubuntu.yml up -d
```

## Passo 6 — Testar

```bash
curl -s http://localhost:8089/api/buses
```

No navegador de qualquer PC da rede: **http://192.168.0.72:3001**

Confira: login, lista de pacientes (deve mostrar os mesmos ~1.335 cadastrados), lista de
agendamentos (deve mostrar os mesmos ~2.095), e tente imprimir uma passagem de teste.

## Passo 7 — Quando tudo estiver validado

Só depois de usar o sistema no Ubuntu por alguns dias e confirmar que está tudo certo:

- Pode desligar o sistema na máquina Windows (`parar-sistema.bat`), mas **não apague** o
  volume `postgres_data` nem os containers de lá até ter certeza absoluta que o Ubuntu está
  estável — mantenha como backup "a frio" por um tempo.
- Se quiser trocar as portas do Ubuntu para as "padrão" (3000/8088/5439) no futuro, dá pra
  fazer, mas só depois de desligar/remover o sistema antigo que ocupa a porta 3000 lá, ou
  negociar outra porta com quem administra aquele sistema.

## Se algo der errado no meio do caminho

- O `docker-compose.ubuntu.yml` não mexe no sistema existente do Ubuntu (portas e nomes de
  container diferentes).
- Se a restauração do backup falhar na metade, é seguro repetir: pare os containers
  (`docker compose --env-file .env.ubuntu -f docker-compose.ubuntu.yml down`), remova o
  volume (`docker volume rm hospital-transport-migracao_postgres_data` — **cuidado, isso
  apaga só o volume novo e vazio do Ubuntu, não mexe no Windows**) e repita a partir do Passo 3.
- A máquina Windows continua com os dados originais intactos o tempo todo — a pior coisa que
  pode acontecer é ter que repetir a cópia, nunca perder dado.
