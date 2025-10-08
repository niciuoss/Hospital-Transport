using HospitalTransport.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace HospitalTransport.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateAppointmentTicket(Appointment appointment)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.MarginVertical(0.5f, Unit.Centimetre);
                    page.MarginHorizontal(1, Unit.Centimetre);

                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Content().Row(row =>
                    {
                        row.RelativeItem().Element(container => ComposeSingleTicket(container, appointment));
                        row.ConstantItem(20).AlignCenter().LineVertical(1).LineColor(Colors.Grey.Lighten1);
                        row.RelativeItem().Element(container => ComposeSingleTicket(container, appointment));
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            var imagePath = "capa.png";
            if (File.Exists(imagePath))
            {
                container.Image(imagePath, ImageScaling.FitWidth);
            }
        }

        private void ComposeSingleTicket(IContainer container, Appointment appointment)
        {
            container.Column(column =>
            {
                column.Spacing(6);

                column.Item().Element(ComposeHeader);
                
                ComposeDataSection(column, "DADOS DO PACIENTE", new List<(string, string)>
                {
                    ("Nome Completo:", appointment.Patient.FullName),
                    ("CPF:", FormatCPF(appointment.Patient.CPF)),
                    ("Cartão SUS:", appointment.Patient.SusCardNumber),
                    ("Data de Nascimento:", appointment.Patient.BirthDate.ToString("dd/MM/yyyy")),
                    ("Telefone:", appointment.Patient.PhoneNumber)
                });
                
                ComposeDataSection(column, "DADOS DO AGENDAMENTO", new List<(string, string)>
                {
                    ("Prontuário:", appointment.MedicalRecordNumber),
                    ("Hospital de Destino:", appointment.DestinationHospital),
                    ("Tipo de Tratamento:", GetTreatmentTypeDescription(appointment)),
                    ("Data da Viagem:", appointment.AppointmentDate.ToString("dd/MM/yyyy")),
                    ("Horário:", appointment.AppointmentDate.ToString("HH:mm")),
                    ("Poltrona:", appointment.SeatNumber.ToString("D2"))
                });
                
                column.Item().PaddingVertical(4, Unit.Millimetre)
                    .Border(1)
                    .BorderColor(Colors.Blue.Medium)
                    .Background(Colors.Blue.Lighten4)
                    .PaddingVertical(4)
                    .Column(col =>
                    {
                        col.Spacing(2);
                        col.Item().AlignCenter().Text("SUA POLTRONA").FontSize(12).SemiBold();
                        col.Item().AlignCenter().Text(appointment.SeatNumber.ToString("D2"))
                            .FontSize(42).Bold().FontColor(Colors.Blue.Darken2);
                    });
                
                if (appointment.Companion != null)
                {
                    ComposeDataSection(column, "DADOS DO ACOMPANHANTE", new List<(string, string)>
                    {
                        ("Nome Completo:", appointment.Companion.FullName),
                        ("CPF:", FormatCPF(appointment.Companion.CPF)),
                        ("Cartão SUS:", appointment.Companion.SusCardNumber),
                        ("Poltrona:", appointment.CompanionSeatNumber?.ToString("D2") ?? "N/A")
                    });
                }
                
                column.Item().Element(ComposeInstructions);

                //column.Item().Element(container => ComposeInstructions(container));

                column.Item().PaddingTop(5).Element(c => ComposeTicketFooter(c, appointment));
            });
        }

        private void ComposeTicketFooter(IContainer container, Appointment appointment)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(2, Unit.Millimetre).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken2));
                        text.Span("Emitido por: ").SemiBold();
                        text.Span(appointment.CreatedByUser.FullName);
                    });

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken2));
                        text.Span("Data/Hora: ").SemiBold();
                        text.Span($"{appointment.CreatedAt:dd/MM/yyyy HH:mm}");
                    });
                });
            });
        }

        private void ComposeDataSection(ColumnDescriptor column, string title, List<(string label, string value)> items)
        {
            column.Item().Text(title).SemiBold().FontSize(11);
            column.Item().Grid(grid =>
            {
                grid.Columns(12); 

                foreach (var item in items)
                {
                    grid.Item(4).Text(item.label).SemiBold();
                    grid.Item(8).Text(item.value);
                }
            });
        }

        private void ComposeInstructions(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten1)
                .Padding(8)
                .Column(col =>
                {
                    col.Item().Text("INSTRUÇÕES IMPORTANTES:").FontSize(10).Bold();
                    col.Item().PaddingTop(2).Text("• Apresente este comprovante no dia da viagem;").FontSize(9);
                    col.Item().Text("• Chegue com 30 minutos de antecedência;").FontSize(9);
                    col.Item().Text("• Traga documento de identificação com foto;").FontSize(9);
                    col.Item().Text("• Em caso de imprevistos, entre em contato com o hospital pelo telefone (88) 9 8193-9906.").FontSize(9);
                });
        }

        #region MÉTODOS INTOCADOS (PARA GeneratePassengerListPdf)
        
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
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Total de Passageiros: {CountTotalPassengers(appointments)}")
                                .FontSize(12).Bold();

                            row.RelativeItem().AlignRight().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

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

        public byte[] GenerateAnnualReportPdf(List<Appointment> appointments, int year)
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

                        column.Item().AlignCenter().Text($"Relatório Anual - {year}")
                            .FontSize(16).FontColor(Colors.Grey.Darken1);

                        column.Item().AlignCenter().Text("Sistema de Transporte de Pacientes")
                            .FontSize(12).FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // Estatísticas Gerais
                        column.Item().Element(container => DrawSection(container, "ESTATÍSTICAS GERAIS", () =>
                        {
                            var totalPassengers = appointments.Count + appointments.Count(a => a.Companion != null);
                            return new List<(string label, string value)>
                    {
                        ("Total de Agendamentos:", appointments.Count.ToString()),
                        ("Total de Passageiros:", totalPassengers.ToString()),
                        ("Pacientes Prioritários:", appointments.Count(a => a.IsPriority).ToString()),
                        ("Viagens com Acompanhante:", appointments.Count(a => a.Companion != null).ToString()),
                        ("Destinos Únicos:", appointments.Select(a => a.DestinationHospital).Distinct().Count().ToString())
                    };
                        }));

                        // Por Mês
                        column.Item().Element(container =>
                        {
                            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("VIAGENS POR MÊS").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(10);

                                var monthlyData = appointments
                                    .GroupBy(a => a.AppointmentDate.Month)
                                    .OrderBy(g => g.Key)
                                    .Select(g => new { Month = g.Key, Count = g.Count() });

                                foreach (var data in monthlyData)
                                {
                                    var monthName = new DateTime(year, data.Month, 1).ToString("MMMM", new System.Globalization.CultureInfo("pt-BR"));
                                    col.Item().PaddingVertical(3).Row(row =>
                                    {
                                        row.ConstantItem(120).Text(monthName.ToUpper()).FontSize(10).Bold();
                                        row.RelativeItem().Column(innerCol =>
                                        {
                                            innerCol.Item().Background(Colors.Blue.Lighten3).Height(8)
                                                .Width((float)(data.Count * 200.0 / appointments.Count));
                                        });
                                        row.ConstantItem(60).AlignRight().Text(data.Count.ToString()).FontSize(10).Bold();
                                    });
                                }
                            });
                        });

                        // Por Tipo de Tratamento
                        column.Item().Element(container =>
                        {
                            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("POR TIPO DE TRATAMENTO").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(10);

                                var treatmentData = appointments
                                    .GroupBy(a => a.TreatmentType)
                                    .OrderByDescending(g => g.Count())
                                    .Select(g => new { Type = g.Key.ToString(), Count = g.Count() });

                                foreach (var data in treatmentData)
                                {
                                    col.Item().PaddingVertical(3).Row(row =>
                                    {
                                        row.ConstantItem(120).Text(data.Type).FontSize(10).Bold();
                                        row.RelativeItem().Column(innerCol =>
                                        {
                                            innerCol.Item().Background(Colors.Green.Lighten3).Height(8)
                                                .Width((float)(data.Count * 200.0 / appointments.Count));
                                        });
                                        row.ConstantItem(60).AlignRight().Text(data.Count.ToString()).FontSize(10).Bold();
                                    });
                                }
                            });
                        });

                        // Top 10 Destinos
                        column.Item().Element(container =>
                        {
                            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("TOP 10 DESTINOS MAIS FREQUENTES").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(10);

                                var destinationData = appointments
                                    .GroupBy(a => a.DestinationHospital)
                                    .OrderByDescending(g => g.Count())
                                    .Take(10)
                                    .Select((g, index) => new { Rank = index + 1, Hospital = g.Key, Count = g.Count() });

                                foreach (var data in destinationData)
                                {
                                    col.Item().PaddingVertical(3).Row(row =>
                                    {
                                        row.ConstantItem(30).Text($"#{data.Rank}").FontSize(10).Bold().FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().Text(data.Hospital).FontSize(10);
                                        row.ConstantItem(60).AlignRight().Text(data.Count.ToString()).FontSize(10).Bold();
                                    });
                                }
                            });
                        });
                    });

                    // Footer
                    page.Footer().Column(column =>
                    {
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        column.Item().PaddingTop(5).AlignCenter().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private int CountTotalPassengers(List<Appointment> appointments)
        {
            return appointments.Count + appointments.Count(a => a.Companion != null);
        }

        private string FormatCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
                return cpf;
            
            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
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
        
        #endregion
    }
}