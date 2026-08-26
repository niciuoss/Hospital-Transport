# Post LinkedIn — Hospital Transport System

> Copie apenas o texto abaixo desta linha e cole no LinkedIn.
> No LinkedIn, você pode selecionar partes do texto e usar o botão de negrito do editor se quiser destacar algo.

---

Desenvolvi um sistema que está em produção em um hospital municipal real — e ele faz a diferença na vida de pacientes todo dia. 🚌🏥

O problema: o Hospital Municipal de Parambu precisava organizar o transporte de pacientes que viajam de ônibus até a capital do estado para realizar tratamentos de saúde. Antes do sistema, o processo era manual, sujeito a erros e desorganização. Qualquer pessoa podia embarcar sem comprovante. Poltronas eram alocadas sem critério. Não havia registro confiável de quem viajou.

A solução que eu construí: um sistema completo de gestão de transporte hospitalar, do back-end ao front-end, containerizado e rodando em produção.

O que o sistema faz:

✅ Cadastro completo de pacientes (CPF, RG, Cartão SUS, nome da mãe, endereço, data de nascimento)

✅ Geração de passagens numeradas em PDF — cada paciente viaja com seu comprovante oficial com número de poltrona

✅ Seletor visual de assentos com dois layouts reais de veículo: ônibus padrão de 48 poltronas e micro-ônibus de 31 poltronas

✅ Lógica de assentos prioritários reservados para pacientes com necessidades especiais

✅ Suporte a crianças de colo com acompanhante obrigatório — a passagem da criança é vinculada à poltrona do responsável

✅ Relatórios mensais e anuais em PDF para prestação de contas — com total de passageiros, acompanhantes, pacientes prioritários e destinos

✅ Dashboard com gráficos em tempo real: agendamentos por tipo de tratamento, top destinos, evolução mensal

✅ Controle de usuários com dois perfis: Administrador e Assistente Social

✅ Modo de manutenção: permite desligar o sistema com mensagem customizada sem derrubar o servidor

Stack técnica:

Back-end: ASP.NET Core 8 · Clean Architecture (Domain / Application / Infrastructure / API) · Entity Framework Core 8 · PostgreSQL 15 · FluentValidation · QuestPDF · Docker multi-stage build

Front-end: Next.js 15 (App Router) · React 19 · TypeScript 5 · Tailwind CSS v4 · shadcn/ui · React Hook Form · Zod · Axios · Recharts

Infra: Docker · Docker Compose · deploy containerizado

Por que me orgulho desse projeto:

Não é um CRUD simples. A lógica de negócio é especializada: assentos prioritários que só pacientes elegíveis podem selecionar, crianças que não ocupam poltrona própria mas precisam de assento vinculado ao acompanhante, passagens geradas com layout de impressão dupla em A4 para economizar papel, relatório mensal com listagem completa para auditoria pública.

Tudo isso resolve um problema real, em um ambiente onde a confiabilidade importa de verdade — porque do outro lado da tela estão pacientes dependendo do ônibus para chegar ao tratamento.

Esse projeto está em produção. Pacientes reais embarcam com as passagens que esse sistema gera.

Se você trabalha com saúde pública, gestão hospitalar ou tem interesse em como tecnologia resolve problemas reais no SUS, bora conversar. 💬

#dotnet #aspnetcore #nextjs #react #typescript #postgresql #docker #cleanarchitecture #saude #healthtech #desenvolvimento #csharp #tailwindcss #shadcnui #softwareengineering #sistemadesaude
