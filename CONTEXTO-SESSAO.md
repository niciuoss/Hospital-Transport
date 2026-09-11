# Contexto de Sessão — Sistema de Transporte Hospitalar

> **Instrução para o Claude:** Leia este arquivo no início de cada sessão para entender o projeto, o estado atual e o que foi feito anteriormente. Após cada sessão produtiva, o usuário atualiza as seções marcadas com `[ATUALIZAR]`.

---

## Sobre o Sistema

Sistema de gerenciamento de passagens para o **ônibus hospitalar do Hospital Municipal de Parambu**. Pacientes vêm ao hospital, têm seus dados cadastrados e saem com a passagem impressa (PDF). Funcionalidades principais:

- Cadastro de pacientes (com dados pessoais, CPF, CNS, RG, data de nascimento)
- Agendamento de viagens com seleção visual de poltrona
- Suporte a acompanhante e crianças de 0 a 7 anos (colo ou cadeirinha)
- Poltronas prioritárias (19 e 20, com elevador) no ônibus de Fortaleza
- Dois ônibus: **Ônibus Fortaleza** (47 poltronas, layout Standard) e **Microônibus Quixeramobim** (31 poltronas, layout Microbus)
- Impressão de passagem em PDF (A4 paisagem, 2 vias)
- Relatórios em PDF: Diário, Mensal, Anual, Pacientes, Acompanhantes, Cadastros, Destinos
- Sistema de usuários com roles: Admin e Assistente Social
- Modo de manutenção (habilitar/desabilitar sistema com mensagem)

---

## Stack Técnica

| Camada | Tecnologia |
|--------|-----------|
| **Backend** | ASP.NET Core 8, Clean Architecture (4 camadas) |
| **Banco de dados** | PostgreSQL 15 |
| **ORM** | Entity Framework Core + Npgsql |
| **PDF** | QuestPDF (Community License) |
| **Frontend** | Next.js 15 + React 19 + TypeScript |
| **UI** | shadcn/ui + Tailwind CSS v4 |
| **Forms** | React Hook Form + Zod |
| **HTTP Client** | Axios |
| **Infraestrutura** | Docker + Docker Compose |

---

## Repositório GitHub

```
https://github.com/niciuoss/Hospital-Transport.git
Branch principal: main
```

**Para voltar a uma versão anterior:**
```bash
git log --oneline          # ver commits
git checkout <hash>        # ir para versão específica
```

---

## Estrutura de Pastas

```
Hospital-Transport/
├── back-end/HospitalTransport/
│   ├── HospitalTransport.API/           ← Controllers, Program.cs, Dockerfile, docker-compose.yml
│   ├── HospitalTransport.Application/   ← Services, DTOs, Validators, Interfaces
│   ├── HospitalTransport.Domain/        ← Entities, Enums, Interfaces de repositório
│   └── HospitalTransport.Infrastructure/← Repositórios, DbContext, Migrations, PdfService
├── front-end/hospital-transport-web/
│   ├── src/app/(dashboard)/
│   │   ├── appointments/                ← Lista e Novo agendamento
│   │   ├── patients/                    ← Lista, Novo e Editar paciente
│   │   ├── reports/                     ← Layout com sub-abas
│   │   │   ├── date/                    ← Diário, Mensal, Anual
│   │   │   ├── patients/                ← Pacientes, Acompanhantes, Cadastros
│   │   │   └── destinations/            ← Relatório de destinos
│   │   ├── users/                       ← Gerenciamento de usuários
│   │   └── admin/                       ← Administração do sistema
│   ├── src/components/
│   │   ├── appointments/                ← AppointmentCard, SeatSelector, SeatSelectorMicrobus
│   │   ├── patients/                    ← PatientForm, PatientCard
│   │   └── layout/                      ← Sidebar, Header
│   └── src/hooks/                       ← useAppointments, usePatients, useBuses, useAuth
└── CONTEXTO-SESSAO.md                   ← Este arquivo
```

---

## Docker — Como rodar

O docker-compose fica em:
```
back-end/HospitalTransport/HospitalTransport.API/docker-compose.yml
```

**Comandos principais:**
```bash
# Subir o sistema
docker compose up -d

# Parar
docker compose down

# Rebuild completo (após alterações de código)
docker compose build --no-cache
docker compose up -d

# Ver logs
docker compose logs -f api
docker compose logs -f frontend
```

**Portas:**
- Frontend: `3000`
- API: `8088`
- PostgreSQL: `5439`

**Acesso na rede:**
- `http://192.168.0.252:3000` — Sistema (qualquer PC da rede)
- `http://192.168.0.252:8088/api` — API

**⚠️ Os dados do banco ficam no volume Docker `postgres_data`. Nunca fazer `docker volume rm postgres_data` a não ser que queira apagar tudo.**

### IP configurável via `.env` (desde 11/09/2026)

O IP da máquina não está mais hardcoded no `docker-compose.yml` nem no `Dockerfile` do
front-end. Existe um arquivo `.env` na mesma pasta do `docker-compose.yml`
(`back-end/HospitalTransport/HospitalTransport.API/.env`) com:

```
SERVER_IP=192.168.0.252
FRONTEND_PORT=3000
API_PORT=8088
POSTGRES_PORT=5439
```

**Se o IP da máquina mudar de novo**, edite só a linha `SERVER_IP` nesse `.env` e rode:

```bash
docker compose build --no-cache frontend
docker compose up -d
```

Não precisa mais editar `docker-compose.yml` nem `Dockerfile` na mão. (O `NEXT_PUBLIC_API_URL`
do Next.js é embutido no build da imagem do front-end, então só reiniciar não basta — é
preciso rebuildar a imagem do frontend depois de mudar o IP.)

⚠️ **O `.env` está no `.gitignore`** (não vai pro GitHub, porque o IP é específico de cada
máquina). Existe um `.env.example` versionado como modelo. Em um clone novo do repositório,
o primeiro passo é: `copy .env.example .env` (dentro de
`back-end/HospitalTransport/HospitalTransport.API/`) e ajustar o `SERVER_IP`.

### Como manter o IP desta máquina fixo (não mudar mais sozinho)

O IP mudou de `192.168.0.249` para `192.168.0.252` porque a máquina pega IP por DHCP (o
roteador decide) e não tinha IP fixo configurado. Duas formas de resolver, dos dados que
levantei nesta máquina (Ethernet, placa Realtek PCIe GbE, MAC `00-E0-20-03-B1-C8`, gateway
`192.168.0.1`, máscara `255.255.255.0`):

1. **Reserva de IP no roteador (o ideal, mas exige acesso ao roteador)** — no painel de
   administração do roteador (normalmente `http://192.168.0.1`), procurar por "DHCP" /
   "Reserva de IP" / "Static Lease" e vincular o MAC `00-E0-20-03-B1-C8` ao IP
   `192.168.0.252`. Assim o roteador sempre entrega o mesmo IP pra essa máquina, sem mexer
   em nada no Windows.

2. **IP estático direto no Windows** — criei o script
   `back-end/HospitalTransport/HospitalTransport.API/fixar-ip-estatico.bat`. Rodar como
   **Administrador** (clique direito → "Executar como administrador"). Ele fixa
   `192.168.0.252` na placa "Ethernet" com máscara `255.255.255.0`, gateway `192.168.0.1` e
   DNS `192.168.0.1`/`8.8.8.8`.
   ⚠️ **Risco:** se o roteador continuar distribuindo esse mesmo IP por DHCP pra outro
   aparelho, pode dar conflito de IP. O ideal é, além de rodar o script, pedir pra quem
   administra o roteador excluir `192.168.0.252` da faixa de DHCP (ou fazer a reserva do
   item 1, que é mais segura).

---

## Banco de Dados

**Tabelas principais:**
- `Users` — Usuários do sistema (admin / assistente social)
- `Patients` — Pacientes cadastrados
- `Buses` — Ônibus (2 registros fixos com IDs predefinidos)
- `Appointments` — Agendamentos/passagens
- `SystemControl` — Liga/desliga o sistema

**IDs fixos dos ônibus:**
```
Ônibus Fortaleza:       11111111-1111-1111-1111-111111111111
Microônibus Quixeramobim: 22222222-2222-2222-2222-222222222222
```

**Soft delete:** Nenhum registro é apagado fisicamente. Todos têm `IsActive` (bool). Exclusões apenas setam `IsActive = false`.

---

## Regras de Negócio Importantes

- **Poltrona 4 não existe** no ônibus de Fortaleza (numeração pula de 3 para 5)
- **Poltronas 19 e 20** são exclusivas para pacientes prioritários (com elevador)
- **Crianças de 0 a 7 anos** obrigatoriamente têm acompanhante. Podem ir no colo (SeatNumber = 0) ou em cadeirinha (SeatNumber > 0)
- **Cada ônibus tem poltronas independentes** — marcar poltrona 1 no ônibus de Fortaleza não afeta o de Quixeramobim
- **Idade calculada dinamicamente** via BirthDate — não é mais valor estático do banco
- **CPF, CNS e RG** aceitam somente números (sem formatação)

---

## API — Endpoints principais

```
POST   /api/auth/login
GET    /api/buses
GET    /api/patients
GET    /api/patients/search?searchTerm=
GET    /api/patients/registrations-report-pdf?dateFrom=&dateTo=
GET    /api/appointments
GET    /api/appointments/seat-availability?date=&busId=&isPriority=
POST   /api/appointments
DELETE /api/appointments/{id}
GET    /api/appointments/{id}/ticket
GET    /api/appointments/passenger-list-pdf?date=
GET    /api/appointments/monthly-report-pdf?year=&month=
GET    /api/appointments/annual-report-pdf?year=
GET    /api/appointments/patients-report-pdf?dateFrom=&dateTo=
GET    /api/appointments/companions-report-pdf?dateFrom=&dateTo=
GET    /api/appointments/destinations-report-pdf?dateFrom=&dateTo=
```

---

## Histórico de Versões

| Versão | Commit | Descrição |
|--------|--------|-----------|
| v1.0 | `694a3de` | Versão inicial funcional |
| v1.1 | `14d5938` | v1.1 funcional |
| v1.2 | `d586e12` | v1.2 |
| v1.3 | `3320fea` | v1.3 |
| back atualizado | `a7d5537` | Atualização do backend |
| v1.4 | `1308ff5` | 10 correções |
| **v1.5** | *(sessão atual)* | **IP configurável via .env, IP fixo 192.168.0.252, preparação da migração para o servidor Ubuntu** |

---

## O que foi feito na sessão v1.5 (11/09/2026)

O IP da máquina mudou sozinho de `192.168.0.249` para `192.168.0.252` (DHCP dinâmico, sem
reserva). Nesta sessão:

1. **IP corrigido e sistema voltou a funcionar** em `http://192.168.0.252:3000` (era a
   prioridade nº 1, pedida no meio da sessão, antes de qualquer outra coisa — o hospital
   precisava tirar passagens).

2. **IP não é mais hardcoded** — criado `.env` em
   `back-end/HospitalTransport/HospitalTransport.API/.env` com `SERVER_IP`, `FRONTEND_PORT`,
   `API_PORT`, `POSTGRES_PORT`. O `docker-compose.yml` agora lê essas variáveis
   (`${SERVER_IP}`, `${API_PORT:-8088}` etc.) em vez de ter o IP escrito direto no arquivo.
   Ver seção "IP configurável via .env" acima.

3. **Script `fixar-ip-estatico.bat`** criado para fixar o IP `192.168.0.252` direto no
   Windows via `netsh` (precisa rodar como Administrador). Não foi executado nesta sessão —
   fica disponível para quando vocês decidirem aplicar (ver seção sobre IP fixo acima).

4. **Preparação da migração para o servidor Ubuntu (192.168.0.72)** — pasta
   `migracao-servidor/` criada na raiz do projeto com tudo pronto para migrar (ver seção
   "Migração para o servidor Ubuntu" abaixo). A migração em si **não foi executada** — falta
   alguém com acesso à máquina Ubuntu rodar o passo a passo do
   `migracao-servidor/README-MIGRACAO.md`.

## O que foi feito na sessão v1.4 (26/08/2026)

**Data:** 26/08/2026

### Correções aplicadas:

1. **Poltronas 47/48** — Validator limitava poltrona do acompanhante a 46. Corrigido para 48.

2. **Ônibus independentes** — `IsSeatAvailableAsync` no repositório não filtrava por `BusId`. Corrigido na interface, repositório e serviço.

3. **CPF/CNS/RG somente números** — Adicionado filtro `value.replace(/\D/g, "")` no `handleChange` dos formulários de criação e edição de paciente.

4. **Idade automática** — Adicionado método `CalculateAge(DateOnly birthDate)` em `PatientService`, `AppointmentService` e `PdfService`. A coluna `Age` no banco continua existindo mas é ignorada nas respostas.

5. **Botão "Próximo" bloqueado** — Adicionado ao `disabled`: `!formData.medicalRecordNumber`, `!formData.destinationHospital`, `!formData.appointmentTime`, e validação do campo "Outro" do tratamento.

6. **Bug seletor de poltrona (criança)** — O seletor do paciente agora exclui `formData.companionSeatNumber` das poltronas disponíveis, evitando conflito quando acompanhante é selecionado primeiro.

7. **Relatórios e passagens excluídas** — Confirmado que todos os relatórios já filtram por `IsActive`. Código correto.

8. **Ordenação + busca** — `GetAllAppointmentsAsync` ordenado por `CreatedAt DESC`. Busca na página de agendamentos inclui CPF e CNS. Placeholder atualizado.

9. **Sub-abas Relatórios** — Criado layout com sub-navegação lateral em `/reports/layout.tsx`. Três sub-abas:
   - `/reports/date` — Relatório Diário (antes "Período"), Mensal, Anual
   - `/reports/patients` — Pacientes por período, Acompanhantes, Cadastros
   - `/reports/destinations` — Destinos por ordem alfabética
   - Novos endpoints no backend + métodos PDF no `PdfService`

10. **Acesso pela rede** — `docker-compose.yml` e `Dockerfile` do frontend configurados com `NEXT_PUBLIC_API_URL=http://192.168.0.249:8088/api`. Sistema acessível em `192.168.0.249:3000`.

### Correção no Dockerfile do backend:
Downloads do SourceForge (fontes MS) substituídos por `fonts-liberation` + `fonts-freefont-ttf` via apt (mais confiável e sem dependência externa).

---

## Migração para o servidor Ubuntu (192.168.0.72)

**Status: preparado, mas ainda NÃO executado.** O objetivo é levar o sistema da máquina
Windows atual (192.168.0.252) para um servidor Ubuntu do hospital que já roda outro sistema
na porta 3000 — os dois devem rodar na mesma máquina sem conflito.

Todo o material está em `migracao-servidor/` na raiz do repo — **leia
`migracao-servidor/README-MIGRACAO.md` antes de migrar**. Resumo:

- Hospital Transport passa a rodar em **192.168.0.72:3001** (site), porta **8089** (API) e
  **5440** (Postgres) — não conflita com o sistema existente na porta 3000.
- Migração do banco por **pg_dump** (backup lógico), sem perda de dados.
- Imagens Docker já buildadas em `.tar` (API e front-end, este já com o IP `192.168.0.72`
  embutido) — no Ubuntu só precisa `docker load`, não precisa buildar nada lá.
- `migracao-servidor/backups/` **não vai pro GitHub** (arquivos grandes/binários — está no
  `.gitignore`). Fica só nesta máquina. Se for migrar depois de um tempo, gere backups novos
  antes (os dados mudam todo dia com novos agendamentos).

**Números de referência do banco no momento em que o backup foi gerado (11/09/2026,
16:51h)** — usar para conferir que nada se perdeu após restaurar no Ubuntu:

| Tabela | Registros |
|---|---|
| Users | 7 |
| Patients | 1.335 |
| Buses | 2 |
| Appointments | 2.095 |

**Arquivo do backup usado:** `hospital_transport_db_20260911_165142.sql`

---

## [ATUALIZAR] Próximas sessões

> *Edite esta seção no início de cada nova sessão para registrar o que foi feito e o que está pendente.*

### Pendências / ideias futuras:
- Autenticação real com JWT + BCrypt (atualmente usa Base64 simples)
- Adicionar `[Authorize]` nos controllers
- Testes automatizados
- Validação de CPF (dígito verificador)
- **Executar a migração para o servidor Ubuntu** — ver seção "Migração para o servidor
  Ubuntu" acima e `migracao-servidor/README-MIGRACAO.md`. Precisa de alguém com acesso à
  máquina 192.168.0.72 pra rodar os comandos lá.
- **Decidir e aplicar o IP fixo** desta máquina Windows — reserva no roteador (recomendado)
  ou rodar `fixar-ip-estatico.bat` como Administrador. Ver seção "Como manter o IP desta
  máquina fixo" acima.

### Problemas conhecidos:
- *(nenhum no momento)*

---

## Dicas para o Claude na próxima sessão

- O sistema roda em Docker. Para testar, usar `curl http://localhost:8088/api/...`
- O banco **nunca** deve ser resetado — volume `postgres_data` tem dados reais de produção
- Antes de qualquer mudança grande, verificar se está no GitHub: `git log --oneline -5`
- O `docker-compose.yml` fica em `back-end/HospitalTransport/HospitalTransport.API/`, não na raiz
- Migrations do EF Core são aplicadas automaticamente no startup do container da API
- O frontend usa `NEXT_PUBLIC_*` embutido no build — mudanças de URL exigem rebuild da imagem
- **O IP não é mais hardcoded**: está no `.env` ao lado do `docker-compose.yml`. Se o IP
  mudar de novo, editar só o `.env` e rodar `docker compose build --no-cache frontend &&
  docker compose up -d` — não precisa mais editar `docker-compose.yml`/`Dockerfile`.
- Se for gerar um novo pacote de migração pro Ubuntu (dados mudam todo dia), repetir:
  `docker exec hospital_transport_db pg_dump -U postgres -F p -d HospitalTransportDB > migracao-servidor/backups/hospital_transport_db_<data>.sql`
  e rebuildar a imagem do frontend com `NEXT_PUBLIC_API_URL=http://192.168.0.72:8089/api`
  antes de exportar o `.tar` (o IP do Ubuntu fica embutido no build do Next.js).
