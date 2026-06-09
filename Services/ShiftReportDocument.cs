using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using shift_handover_page.Models;

namespace shift_handover_page.Services;

public static class ShiftReportDocument
{
    public static byte[] Generate(Shift shift)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10.5f));

                page.Header().Column(column =>
                {
                    column.Item().Text("Shift Handover Report").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken3);
                    column.Item().Text($"{shift.Title} • {shift.ScheduledFor:dd MMM yyyy}").FontSize(12).FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Element(CellStyle).Text("Status").SemiBold();
                        table.Cell().Element(CellStyle).Text(shift.Status.ToString());
                        table.Cell().Element(CellStyle).Text("Claimed By").SemiBold();
                        table.Cell().Element(CellStyle).Text(shift.ClaimedByUserName ?? "-");

                        table.Cell().Element(CellStyle).Text("Claimed At").SemiBold();
                        table.Cell().Element(CellStyle).Text(shift.ClaimedAt?.ToString("dd MMM yyyy HH:mm") ?? "-");
                        table.Cell().Element(CellStyle).Text("Closed At").SemiBold();
                        table.Cell().Element(CellStyle).Text(shift.ClosedAt?.ToString("dd MMM yyyy HH:mm") ?? "-");
                    });

                    column.Item().Text("Closure Summary").SemiBold();
                    column.Item().Text(shift.ClosureSummary ?? "No closure summary was recorded.");

                    column.Item().PaddingTop(10).Text("Logged Events").FontSize(14).SemiBold();

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Type");
                            header.Cell().Element(HeaderStyle).Text("Time");
                            header.Cell().Element(HeaderStyle).Text("Title");
                            header.Cell().Element(HeaderStyle).Text("Description");
                        });

                        foreach (var log in shift.Logs.OrderBy(log => log.OccurredAt))
                        {
                            table.Cell().Element(CellStyle).Text(log.LogType.ToString());
                            table.Cell().Element(CellStyle).Text(log.OccurredAt.ToString("dd MMM yyyy HH:mm"));
                            table.Cell().Element(CellStyle).Text(log.Title);
                            table.Cell().Element(CellStyle).Text(BuildLogDescription(log));
                        }

                        if (!shift.Logs.Any())
                        {
                            table.Cell().ColumnSpan(4).Element(CellStyle).Text("No logs were recorded for this shift.");
                        }
                    });
                });

                page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:dd MMM yyyy HH:mm}");
            });
        }).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingRight(4);
    }

    private static IContainer HeaderStyle(IContainer container)
    {
        return container.Background(Colors.Blue.Darken3).PaddingVertical(6).PaddingRight(4);
    }

    private static string BuildLogDescription(ShiftLog log)
    {
        if (log.LogType != ShiftLogType.Manpower || !log.ManpowerCount.HasValue)
        {
            return log.Description;
        }

        return $"{log.Description} (Headcount: {log.ManpowerCount.Value})";
    }
}