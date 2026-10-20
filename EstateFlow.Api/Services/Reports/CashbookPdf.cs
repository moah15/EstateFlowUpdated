using System.Globalization;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EstateFlow.Api.Services.Reports;

// Estate bank account cashbook: every receipt and payment with a running balance. Used to support
// the L&D account and, at the end of the administration, the final cash statement.
public static class CashbookPdf
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static string R(decimal v) =>
        (v < 0 ? "-R " : "R ") + Math.Abs(v).ToString("N2", Inv);

    private static string Money(decimal v) => v == 0m ? "" : v.ToString("N2", Inv);

    public static byte[] Generate(Estate estate)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var entries = estate.CashbookEntries
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => CashbookTypes.IsReceipt(e.EntryType) ? 0 : 1)
            .ThenBy(e => e.EntryId)
            .ToList();

        var receipts = entries.Where(e => CashbookTypes.IsReceipt(e.EntryType)).Sum(e => e.Amount);
        var payments = entries.Where(e => !CashbookTypes.IsReceipt(e.EntryType)).Sum(e => e.Amount);
        var closing = receipts - payments;
        var unreconciled = entries.Count(e => !e.IsReconciled);

        var bank = string.Join(" · ",
            new[] { estate.EstateBankName, estate.EstateBankBranchCode, estate.EstateBankAccountNumber }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().AlignCenter().Text("ESTATE BANK ACCOUNT – CASHBOOK").Bold().FontSize(14);
                    h.Item().AlignCenter().Text($"Estate late {estate.Deceased?.FullName}  ·  File {estate.FileNumber}").FontSize(10);
                    h.Item().AlignCenter().Text($"Bank: {(string.IsNullOrWhiteSpace(bank) ? "not captured" : bank)}  ·  Printed {DateTime.Today:dd MMMM yyyy}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    h.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(58);   // date
                            c.RelativeColumn(2.2f); // type / category
                            c.RelativeColumn(3.2f); // description / payee
                            c.ConstantColumn(68);   // receipts
                            c.ConstantColumn(68);   // payments
                            c.ConstantColumn(74);   // balance
                            c.ConstantColumn(16);   // reconciled
                        });

                        t.Header(h =>
                        {
                            h.Cell().Element(Cell).Text("Date").Bold();
                            h.Cell().Element(Cell).Text("Type / category").Bold();
                            h.Cell().Element(Cell).Text("Description / payee").Bold();
                            h.Cell().Element(Cell).AlignRight().Text("Receipts").Bold();
                            h.Cell().Element(Cell).AlignRight().Text("Payments").Bold();
                            h.Cell().Element(Cell).AlignRight().Text("Balance").Bold();
                            h.Cell().Element(Cell).AlignCenter().Text("R").Bold();
                        });

                        decimal running = 0m;
                        foreach (var e in entries)
                        {
                            var isReceipt = CashbookTypes.IsReceipt(e.EntryType);
                            running += isReceipt ? e.Amount : -e.Amount;

                            t.Cell().Element(Cell).Text(e.EntryDate.ToString("dd MMM yy", Inv));
                            t.Cell().Element(Cell).Column(c =>
                            {
                                c.Item().Text(e.EntryType);
                                if (!string.IsNullOrWhiteSpace(e.Category))
                                    c.Item().Text(e.Category).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });
                            t.Cell().Element(Cell).Column(c =>
                            {
                                c.Item().Text(e.Description);
                                var who = string.Join(" · ", new[] { e.PayerOrPayee, e.Reference }.Where(x => !string.IsNullOrWhiteSpace(x)));
                                if (who.Length > 0)
                                    c.Item().Text(who).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });
                            t.Cell().Element(Cell).AlignRight().Text(isReceipt ? Money(e.Amount) : "");
                            t.Cell().Element(Cell).AlignRight().Text(isReceipt ? "" : Money(e.Amount));
                            t.Cell().Element(Cell).AlignRight().Text(R(running));
                            t.Cell().Element(Cell).AlignCenter().Text(e.IsReconciled ? "Y" : "");
                        }

                        t.Cell().ColumnSpan(3).PaddingTop(4).Text("Totals").Bold();
                        t.Cell().PaddingTop(4).AlignRight().Text(Money(receipts)).Bold();
                        t.Cell().PaddingTop(4).AlignRight().Text(Money(payments)).Bold();
                        t.Cell().PaddingTop(4).AlignRight().Text(R(closing)).Bold();
                        t.Cell().PaddingTop(4).Text("");
                    });

                    // Summary by type
                    col.Item().Column(s =>
                    {
                        s.Item().Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(4)
                            .Text("SUMMARY BY TYPE").Bold();
                        foreach (var type in CashbookTypes.All)
                        {
                            var total = entries.Where(e => e.EntryType == type).Sum(e => e.Amount);
                            if (total == 0m) continue;
                            s.Item().BorderBottom(0.4f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Row(r =>
                            {
                                r.RelativeItem().Text(type);
                                r.ConstantItem(100).AlignRight().Text(R(CashbookTypes.IsReceipt(type) ? total : -total));
                            });
                        }
                        s.Item().PaddingTop(3).BorderTop(0.8f).Row(r =>
                        {
                            r.RelativeItem().Text("Balance in the estate bank account per cashbook").Bold();
                            r.ConstantItem(100).AlignRight().Text(R(closing)).Bold();
                        });
                    });

                    col.Item().Text(unreconciled == 0
                            ? "All entries are marked as reconciled to the bank statement."
                            : $"{unreconciled} of {entries.Count} entries are not yet reconciled to the bank statement (Y = reconciled).")
                        .FontSize(8).FontColor(unreconciled == 0 ? Colors.Green.Darken2 : Colors.Orange.Darken3);
                });

                page.Footer().Row(f =>
                {
                    f.RelativeItem().Text($"File {estate.FileNumber}").FontSize(7).FontColor(Colors.Grey.Darken1);
                    f.RelativeItem().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" of ");
                        t.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static IContainer Cell(IContainer c) =>
        c.BorderBottom(0.4f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(2);
}
