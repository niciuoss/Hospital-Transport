# Como Deletar um Paciente via API

## Importante

O delete feito pela API é um **soft delete** — o paciente não é removido fisicamente do banco, apenas marcado como inativo (`IsActive = false`). Isso preserva o histórico de agendamentos vinculados a ele.

> **Nunca delete um paciente diretamente pelo banco de dados (ex: Beekeeper)**, pois isso viola a constraint de chave estrangeira `FK_Appointments_Patients_CompanionId` e causará erro caso o paciente esteja registrado como acompanhante em algum agendamento.

---

## Passo 1 — Encontrar o ID do paciente

Faça uma busca pelo nome, CPF ou cartão SUS:

```
GET http://localhost:8088/api/patients/search?searchTerm=<nome ou CPF>
```

Ou liste todos os pacientes:

```
GET http://localhost:8088/api/patients
```

Na resposta, copie o valor do campo `id` do paciente desejado. Exemplo:

```json
{
  "id": "226ca2d5-daa3-4ccf-abcc-d1796df68526",
  "fullName": "João da Silva",
  ...
}
```

---

## Passo 2 — Executar o DELETE

### Via Insomnia (ou Postman)

| Campo   | Valor                                                                 |
|---------|-----------------------------------------------------------------------|
| Método  | `DELETE`                                                              |
| URL     | `http://localhost:8088/api/patients/{id_paciente}`                    |
| Body    | nenhum                                                                |

Exemplo com o ID real:

```
DELETE http://localhost:8088/api/patients/226ca2d5-daa3-4ccf-abcc-d1796df68526
```

### Resposta de sucesso

```json
{
  "success": true,
  "message": "Paciente removido com sucesso",
  "data": true
}
```

---

## Portas da API

| Modo de execução         | Porta                    |
|--------------------------|--------------------------|
| Servico Windows (padrao) | http://localhost:8088    |
| Desenvolvimento          | http://localhost:5200    |
| Docker                   | http://localhost:8080    |
