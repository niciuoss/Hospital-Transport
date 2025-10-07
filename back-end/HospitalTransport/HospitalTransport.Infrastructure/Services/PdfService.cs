using HospitalTransport.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HospitalTransport.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // Licença do QuestPDF (Community License) - Lembrar de mudar!
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateAppointmentTicket(Appointment appointment)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("HOSPITAL MUNICIPAL")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().AlignCenter().Text("Sistema de Transporte de Pacientes")
                            .FontSize(12).FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // Título
                        column.Item().AlignCenter().Text("COMPROVANTE DE AGENDAMENTO")
                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken1);

                        // Dados do Paciente
                        column.Item().Element(container => DrawSection(container, "DADOS DO PACIENTE", () =>
                        {
                            return new List<(string label, string value)>
                            {
                                ("Nome Completo:", appointment.Patient.FullName),
                                ("CPF:", FormatCPF(appointment.Patient.CPF)),
                                ("Cartão SUS:", appointment.Patient.SusCardNumber),
                                ("Data de Nascimento:", appointment.Patient.BirthDate.ToString("dd/MM/yyyy")),
                                ("Telefone:", appointment.Patient.PhoneNumber)
                            };
                        }));

                        // Dados do Agendamento
                        column.Item().Element(container => DrawSection(container, "DADOS DO AGENDAMENTO", () =>
                        {
                            var items = new List<(string label, string value)>
                            {
                                ("Prontuário:", appointment.MedicalRecordNumber),
                                ("Hospital de Destino:", appointment.DestinationHospital),
                                ("Tipo de Tratamento:", GetTreatmentTypeDescription(appointment)),
                                ("Data da Viagem:", appointment.AppointmentDate.ToString("dd/MM/yyyy")),
                                ("Horário:", appointment.AppointmentDate.ToString("HH:mm")),
                                ("Poltrona:", appointment.SeatNumber.ToString("D2"))
                            };

                            if (appointment.IsPriority)
                            {
                                items.Add(("Status:", "PACIENTE PRIORITÁRIO"));
                            }

                            return items;
                        }));

                        // Destaque para a poltrona
                        column.Item().PaddingVertical(10).Border(2).BorderColor(Colors.Blue.Darken2)
                            .Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                            {
                                col.Item().AlignCenter().Text("SUA POLTRONA")
                                    .FontSize(14).Bold();
                                col.Item().AlignCenter().Text(appointment.SeatNumber.ToString("D2"))
                                    .FontSize(48).Bold().FontColor(Colors.Blue.Darken3);
                            });

                        // Dados do Acompanhante (se houver)
                        if (appointment.Companion != null)
                        {
                            column.Item().Element(container => DrawSection(container, "DADOS DO ACOMPANHANTE", () =>
                            {
                                return new List<(string label, string value)>
                                {
                                    ("Nome Completo:", appointment.Companion.FullName),
                                    ("CPF:", FormatCPF(appointment.Companion.CPF)),
                                    ("Cartão SUS:", appointment.Companion.SusCardNumber),
                                    ("Poltrona:", appointment.CompanionSeatNumber?.ToString("D2") ?? "N/A")
                                };
                            }));
                        }

                        // Instruções
                        column.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Medium)
                            .Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                            {
                                col.Item().Text("INSTRUÇÕES IMPORTANTES:").FontSize(12).Bold();
                                col.Item().PaddingTop(5).Text("• Apresente este comprovante no dia da viagem;").FontSize(10);
                                col.Item().Text("• Chegue com 30 minutos de antecedência;").FontSize(10);
                                col.Item().Text("• Traga documento de identificação com foto;").FontSize(10);
                                col.Item().Text("• Em caso de imprevistos, entre em contato com o hospital.").FontSize(10);
                            });
                    });

                    // Footer
                    page.Footer().Column(column =>
                    {
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"Emitido por: {appointment.CreatedByUser.FullName}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);

                            row.RelativeItem().AlignRight().Text($"Data/Hora: {appointment.CreatedAt:dd/MM/yyyy HH:mm}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        column.Item().AlignCenter().Text($"Código do Agendamento: {appointment.Id}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GeneratePassengerListPdf(List<Appointment> appointments, DateTime date)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("HOSPITAL MUNICIPAL")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().AlignCenter().Text("Lista de Passageiros")
                            .FontSize(14).FontColor(Colors.Grey.Darken1);

                        column.Item().AlignCenter().Text($"Data da Viagem: {date:dd/MM/yyyy}")
                            .FontSize(12).FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Informações gerais
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Total de Passageiros: {CountTotalPassengers(appointments)}")
                                .FontSize(12).Bold();

                            row.RelativeItem().AlignRight().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Tabela de passageiros
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50); // Poltrona
                                columns.RelativeColumn(3); // Nome
                                columns.RelativeColumn(2); // CPF
                                columns.ConstantColumn(80); // Tipo
                            });

                            // Header da tabela
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Poltrona").FontColor(Colors.White).Bold().FontSize(10);

                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Nome Completo").FontColor(Colors.White).Bold().FontSize(10);

                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("CPF").FontColor(Colors.White).Bold().FontSize(10);

                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Tipo").FontColor(Colors.White).Bold().FontSize(10);
                            });

                            // Ordenar por poltrona
                            var sortedAppointments = appointments.OrderBy(a => a.SeatNumber).ToList();

                            foreach (var appointment in sortedAppointments)
                            {
                                // Linha do paciente
                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(appointment.SeatNumber.ToString("D2")).FontSize(10).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(appointment.Patient.FullName).FontSize(10);

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(FormatCPF(appointment.Patient.CPF)).FontSize(10);

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(appointment.IsPriority ? "Prioritário" : "Paciente").FontSize(9)
                                    .FontColor(appointment.IsPriority ? Colors.Red.Darken1 : Colors.Blue.Darken1);

                                // Linha do acompanhante (se houver)
                                if (appointment.Companion != null && appointment.CompanionSeatNumber.HasValue)
                                {
                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text(appointment.CompanionSeatNumber.Value.ToString("D2")).FontSize(10).Bold();

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text(appointment.Companion.FullName).FontSize(10);

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text(FormatCPF(appointment.Companion.CPF)).FontSize(10);

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text("Acompanhante").FontSize(9).FontColor(Colors.Green.Darken1);
                                }
                            }
                        });

                        // Resumo
                        column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Medium)
                            .Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                            {
                                col.Item().Text("RESUMO").FontSize(12).Bold();
                                col.Item().PaddingTop(5).Text($"Pacientes: {appointments.Count}").FontSize(10);
                                col.Item().Text($"Acompanhantes: {appointments.Count(a => a.Companion != null)}").FontSize(10);
                                col.Item().Text($"Total de Passageiros: {CountTotalPassengers(appointments)}").FontSize(10).Bold();
                                col.Item().Text($"Poltronas Prioritárias Ocupadas: {appointments.Count(a => a.IsPriority && a.SeatNumber <= 3)}").FontSize(10);
                            });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Documento gerado pelo Sistema de Transporte Hospitalar - ")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span($"Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private int CountTotalPassengers(List<Appointment> appointments)
        {
            return appointments.Count + appointments.Count(a => a.Companion != null);
        }

        private void DrawSection(IContainer container, string title, Func<List<(string label, string value)>> getItems)
        {
            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text(title).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(10);

                foreach (var item in getItems())
                {
                    column.Item().PaddingVertical(3).Row(row =>
                    {
                        row.ConstantItem(150).Text(item.label).FontSize(10).Bold();
                        row.RelativeItem().Text(item.value).FontSize(10);
                    });
                }
            });
        }

        private string FormatCPF(string cpf)
        {
            if (cpf.Length == 11)
            {
                return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
            }
            return cpf;
        }

        private string GetTreatmentTypeDescription(Appointment appointment)
        {
            var description = appointment.TreatmentType.ToString();
            if (appointment.TreatmentType == Domain.Enums.TreatmentType.Outro &&
                !string.IsNullOrEmpty(appointment.TreatmentTypeOther))
            {
                description += $" - {appointment.TreatmentTypeOther}";
            }
            return description;
        }
    }
}
