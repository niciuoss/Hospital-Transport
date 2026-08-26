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

        private static int CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age))
                age--;
            return age < 0 ? 0 : age;
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

                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Liberation Sans"));

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
                    ("Poltrona:", GetDisplaySeatNumber(appointment))
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
                        col.Item().AlignCenter().Text(GetDisplaySeatNumber(appointment))
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

        public byte[] GenerateMonthlyReportPdf(List<Appointment> appointments, int year, int month)
        {
            //VERIFICAÇÃO: Lista vazia ou null
            if (appointments == null || !appointments.Any())
            {
                throw new InvalidOperationException("Nenhum agendamento encontrado para o período selecionado.");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    //PAISAGEM (Landscape)
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Element(ComposeMonthlyHeader);
                    page.Content().Element(ComposeMonthlyContent);
                    page.Footer().Element(ComposeFooter);

                    void ComposeMonthlyHeader(IContainer container)
                    {
                        container.Column(column =>
                        {
                            column.Spacing(10);

                            // Logo e Título
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("HOSPITAL MUNICIPAL DE PARAMBU")
                                        .FontSize(14).Bold();
                                    col.Item().Text("SECRETARIA MUNICIPAL DE SAÚDE")
                                        .FontSize(12).SemiBold();
                                    col.Item().Text("Sistema de Transporte Hospitalar")
                                        .FontSize(10);
                                });

                                row.ConstantItem(150).AlignRight().Column(col =>
                                {
                                    col.Item().Text($"RELATÓRIO MENSAL")
                                        .FontSize(12).Bold();
                                    col.Item().Text($"{GetMonthName(month)}/{year}")
                                        .FontSize(11);
                                    col.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                        .FontSize(8);
                                });
                            });

                            // Separator
                            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        });
                    }

                    void ComposeMonthlyContent(IContainer container)
                    {
                        container.Column(column =>
                        {
                            column.Spacing(15);

                            // Resumo no topo
                            column.Item().Element(ComposeResumo);

                            // Título da tabela
                            column.Item().PaddingTop(10).Text("LISTA COMPLETA DE PACIENTES E ACOMPANHANTES")
                                .FontSize(11).Bold().FontColor(Colors.Blue.Darken2);

                            // Tabela de pacientes e acompanhantes
                            column.Item().Table(table =>
                            {
                                // Definir colunas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);  // Nº
                                    columns.ConstantColumn(50);  // Data
                                    columns.ConstantColumn(35);  // Tipo
                                    columns.RelativeColumn(2);   // Nome
                                    columns.ConstantColumn(55);  // RG
                                    columns.ConstantColumn(55);  // CPF
                                    columns.ConstantColumn(30);  // Idade
                                    columns.ConstantColumn(55);  // D.Nascimento
                                    columns.RelativeColumn(2);   // Nome da Mãe
                                    columns.ConstantColumn(55);  // SUS
                                    columns.RelativeColumn(2);   // Endereço
                                    columns.ConstantColumn(60);  // Hospital
                                });

                                // Cabeçalho
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderStyle).Text("Nº");
                                    header.Cell().Element(HeaderStyle).Text("Data");
                                    header.Cell().Element(HeaderStyle).Text("Tipo");
                                    header.Cell().Element(HeaderStyle).Text("Nome Completo");
                                    header.Cell().Element(HeaderStyle).Text("RG");
                                    header.Cell().Element(HeaderStyle).Text("CPF");
                                    header.Cell().Element(HeaderStyle).Text("Idade");
                                    header.Cell().Element(HeaderStyle).Text("Data Nascimento");
                                    header.Cell().Element(HeaderStyle).Text("Nome da Mãe");
                                    header.Cell().Element(HeaderStyle).Text("Cartão SUS");
                                    header.Cell().Element(HeaderStyle).Text("Endereço");
                                    header.Cell().Element(HeaderStyle).Text("Hospital");

                                    static IContainer HeaderStyle(IContainer container) =>
                                        container.Border(1)
                                            .Background(Colors.Blue.Lighten3)
                                            .Padding(5)
                                            .AlignCenter()
                                            .AlignMiddle();
                                });

                                // Dados
                                int rowNumber = 0;
                                var sortedAppointments = appointments.OrderBy(a => a.AppointmentDate).ToList();

                                foreach (var appointment in sortedAppointments)
                                {
                                    // Appointment não pode ser null
                                    if (appointment == null) continue;

                                    // Patient não pode ser null
                                    if (appointment.Patient == null) continue;

                                    // LINHA DO PACIENTE
                                    rowNumber++;
                                    var patient = appointment.Patient;
                                    var isEvenRow = rowNumber % 2 == 0;

                                    table.Cell().Element(c => CellStyle(c, isEvenRow)).Text(rowNumber.ToString());
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(appointment.AppointmentDate.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text("PACIENTE").FontColor(Colors.Green.Darken2).Bold();
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(patient.FullName ?? "-");
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(patient.RG ?? "-");
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(FormatCPFReport(patient.CPF));
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(CalculateAge(patient.BirthDate).ToString());
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(patient.BirthDate.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(patient.MotherName ?? "-");
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(patient.SusCardNumber ?? "-");
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(FormatAddressReport(patient));
                                    table.Cell().Element(c => CellStyle(c, isEvenRow))
                                        .Text(appointment.DestinationHospital ?? "-");

                                    // LINHA DO ACOMPANHANTE (se existir)
                                    if (appointment.Companion != null)
                                    {
                                        rowNumber++;
                                        var companion = appointment.Companion;
                                        isEvenRow = rowNumber % 2 == 0;

                                        table.Cell().Element(c => CellStyle(c, isEvenRow)).Text(rowNumber.ToString());
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(appointment.AppointmentDate.ToString("dd/MM/yyyy"));
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text("ACOMP.").FontColor(Colors.Blue.Darken2).Bold();
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(companion.FullName ?? "-");
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(companion.RG ?? "-");
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(FormatCPFReport(companion.CPF));
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(CalculateAge(companion.BirthDate).ToString());
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(FormatPhoneReport(companion.PhoneNumber));
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(companion.MotherName ?? "-");
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(companion.SusCardNumber ?? "-");
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(FormatAddressReport(companion));
                                        table.Cell().Element(c => CellStyle(c, isEvenRow))
                                            .Text(appointment.DestinationHospital ?? "-");
                                    }
                                }

                                static IContainer CellStyle(IContainer container, bool isEvenRow) =>
                                    container.Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Background(isEvenRow ? Colors.Grey.Lighten4 : Colors.White)
                                        .Padding(4);
                            });
                        });
                    }

                    void ComposeResumo(IContainer container)
                    {
                        var totalPatients = appointments.Count();
                        var totalWithCompanions = appointments.Count(a => a.CompanionId.HasValue);
                        var totalPeople = totalPatients + totalWithCompanions;
                        var totalPriority = appointments.Count(a => a.IsPriority);

                        container.Background(Colors.Blue.Lighten4)
                            .Padding(10)
                            .Column(column =>
                            {
                                column.Spacing(5);

                                column.Item().Text("RESUMO DO MÊS")
                                    .FontSize(11).Bold().FontColor(Colors.Blue.Darken3);

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Total de Pacientes: {totalPatients}")
                                        .FontSize(10).SemiBold();
                                    row.RelativeItem().Text($"Com Acompanhantes: {totalWithCompanions}")
                                        .FontSize(10).SemiBold();
                                    row.RelativeItem().Text($"Pacientes Prioritários: {totalPriority}")
                                        .FontSize(10).SemiBold();
                                    row.RelativeItem().Text($"Total de Pessoas Transportadas: {totalPeople}")
                                        .FontSize(10).SemiBold().FontColor(Colors.Green.Darken2);
                                });
                            });
                    }

                    void ComposeFooter(IContainer container)
                    {
                        container.AlignCenter().Text(text =>
                        {
                            text.Span("Página ").FontSize(8);
                            text.CurrentPageNumber().FontSize(8);
                            text.Span(" de ").FontSize(8);
                            text.TotalPages().FontSize(8);
                        });
                    }
                });
            });

            return document.GeneratePdf();
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janeiro",
                2 => "Fevereiro",
                3 => "Março",
                4 => "Abril",
                5 => "Maio",
                6 => "Junho",
                7 => "Julho",
                8 => "Agosto",
                9 => "Setembro",
                10 => "Outubro",
                11 => "Novembro",
                12 => "Dezembro",
                _ => "Mês Inválido"
            };
        }

        private string FormatCPFReport(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return "-";
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) return cpf;
            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        private string FormatPhoneReport(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "-";
            phone = new string(phone.Where(char.IsDigit).ToArray());
            if (phone.Length == 11)
                return $"({phone.Substring(0, 2)}) {phone.Substring(2, 5)}-{phone.Substring(7, 4)}";
            if (phone.Length == 10)
                return $"({phone.Substring(0, 2)}) {phone.Substring(2, 4)}-{phone.Substring(6, 4)}";
            return phone;
        }

        private string FormatAddressReport(Patient patient)
        {
            if (!string.IsNullOrWhiteSpace(patient.Address))
                return patient.Address;
            return "-";
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
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Liberation Sans"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("HOSPITAL MUNICIPAL DE PARAMBU")
                            .FontSize(18).Bold().FontColor(Colors.Green.Darken2);

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
                                columns.ConstantColumn(60); // Poltrona 
                                columns.RelativeColumn(4); // Nome 
                                columns.RelativeColumn(2); // CPF 
                                columns.ConstantColumn(90); // Tipo 
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
                                    .AlignCenter().Text(GetDisplaySeatNumber(appointment)).FontSize(10).Bold(); 

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(appointment.Patient.FullName).FontSize(9); 

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .Text(FormatCPF(appointment.Patient.CPF)).FontSize(9); 

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                    .AlignCenter() 
                                    .Text(appointment.IsPriority ? "Prioritário" : "Paciente").FontSize(9)
                                    .FontColor(appointment.IsPriority ? Colors.Red.Darken1 : Colors.Blue.Darken1);

                                // Linha do acompanhante (se houver)
                                if (appointment.Companion != null && appointment.CompanionSeatNumber.HasValue)
                                {
                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .AlignCenter().Text(appointment.CompanionSeatNumber.Value.ToString("D2")).FontSize(10).Bold();

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text(appointment.Companion.FullName).FontSize(9);

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .Text(FormatCPF(appointment.Companion.CPF)).FontSize(9);

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                        .AlignCenter()
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
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Liberation Sans"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("HOSPITAL MUNICIPAL DE PARAMBU")
                            .FontSize(18).Bold().FontColor(Colors.Green.Darken2);

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

        /// <summary>
        /// Retorna o número da poltrona formatado para exibição no PDF.
        /// Para crianças de colo (IsInfant = true e SeatNumber = 0), exibe o número
        /// da poltrona do acompanhante com asterisco (ex: "26*") para indicar que
        /// a criança está na mesma poltrona que o responsável.
        /// </summary>
        private string GetDisplaySeatNumber(Appointment appointment)
        {
            // Criança de colo no colo do acompanhante (não usa cadeirinha)
            if (appointment.IsInfant && appointment.SeatNumber == 0 && appointment.CompanionSeatNumber.HasValue)
            {
                return $"{appointment.CompanionSeatNumber.Value.ToString("D2")}*";
            }

            // Caso normal: exibe o número da poltrona padrão
            return appointment.SeatNumber.ToString("D2");
        }

        #endregion

        public byte[] GeneratePatientsInPeriodPdf(List<Appointment> appointments, DateTime from, DateTime to)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("HOSPITAL MUNICIPAL DE PARAMBU").FontSize(14).Bold();
                        col.Item().Text("Relatório de Pacientes por Período").FontSize(12);
                        col.Item().Text($"Período: {from:dd/MM/yyyy} a {to:dd/MM/yyyy}").FontSize(10);
                        col.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        var totalPatients = appointments.Count;
                        col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().Text($"Total de Pacientes Transportados: {totalPatients}").FontSize(14).Bold();
                        });

                        col.Item().PaddingTop(10).Text("Detalhamento por Data").FontSize(11).Bold();

                        var byDate = appointments.GroupBy(a => a.AppointmentDate.Date).OrderBy(g => g.Key);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(100);
                                cols.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Data").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Pacientes").Bold();
                            });
                            foreach (var group in byDate)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Key.ToString("dd/MM/yyyy"));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Count().ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
            return document.GeneratePdf();
        }

        public byte[] GenerateCompanionsInPeriodPdf(List<Appointment> appointments, DateTime from, DateTime to)
        {
            var withCompanion = appointments.Where(a => a.CompanionId.HasValue).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("HOSPITAL MUNICIPAL DE PARAMBU").FontSize(14).Bold();
                        col.Item().Text("Relatório de Acompanhantes por Período").FontSize(12);
                        col.Item().Text($"Período: {from:dd/MM/yyyy} a {to:dd/MM/yyyy}").FontSize(10);
                        col.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().Text($"Total de Acompanhantes: {withCompanion.Count}").FontSize(14).Bold();
                            inner.Item().Text($"Total de Agendamentos no Período: {appointments.Count}").FontSize(11);
                        });

                        col.Item().PaddingTop(10).Text("Detalhamento por Data").FontSize(11).Bold();

                        var byDate = appointments.GroupBy(a => a.AppointmentDate.Date).OrderBy(g => g.Key);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(100);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Data").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Agendamentos").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Acompanhantes").Bold();
                            });
                            foreach (var group in byDate)
                            {
                                var companions = group.Count(a => a.CompanionId.HasValue);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Key.ToString("dd/MM/yyyy"));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Count().ToString());
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(companions.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
            return document.GeneratePdf();
        }

        public byte[] GenerateRegistrationsPdf(List<Patient> patients, DateTime from, DateTime to)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("HOSPITAL MUNICIPAL DE PARAMBU").FontSize(14).Bold();
                        col.Item().Text("Relatório de Cadastros Realizados").FontSize(12);
                        col.Item().Text($"Período: {from:dd/MM/yyyy} a {to:dd/MM/yyyy}").FontSize(10);
                        col.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().Text($"Total de Cadastros Realizados: {patients.Count}").FontSize(14).Bold();
                        });

                        col.Item().PaddingTop(10).Text("Lista de Pacientes Cadastrados").FontSize(11).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(25);
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(90);
                                cols.ConstantColumn(90);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Nº").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Nome Completo").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("CPF").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Cadastrado em").Bold();
                            });
                            int row = 0;
                            foreach (var p in patients.OrderBy(p => p.FullName))
                            {
                                row++;
                                bool even = row % 2 == 0;
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.ToString());
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.FullName);
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(FormatCPFReport(p.CPF));
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.CreatedAt.ToString("dd/MM/yyyy"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
            return document.GeneratePdf();
        }

        public byte[] GenerateDestinationsReportPdf(List<Appointment> appointments, DateTime from, DateTime to)
        {
            var destinations = appointments
                .GroupBy(a => a.DestinationHospital)
                .OrderBy(g => g.Key)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("HOSPITAL MUNICIPAL DE PARAMBU").FontSize(14).Bold();
                        col.Item().Text("Relatório de Destinos").FontSize(12);
                        col.Item().Text($"Período: {from:dd/MM/yyyy} a {to:dd/MM/yyyy}").FontSize(10);
                        col.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().Text($"Total de Destinos Únicos: {destinations.Count}").FontSize(12).Bold();
                            inner.Item().Text($"Total de Viagens no Período: {appointments.Count}").FontSize(11);
                        });

                        col.Item().PaddingTop(10).Text("Destinos por Ordem Alfabética").FontSize(11).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(25);
                                cols.RelativeColumn(3);
                                cols.ConstantColumn(80);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Nº").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Destino").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Viagens").Bold();
                            });
                            int row = 0;
                            foreach (var dest in destinations)
                            {
                                row++;
                                bool even = row % 2 == 0;
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.ToString());
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(dest.Name ?? "-");
                                table.Cell().Background(even ? Colors.Grey.Lighten4 : Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(dest.Count.ToString()).Bold();
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}