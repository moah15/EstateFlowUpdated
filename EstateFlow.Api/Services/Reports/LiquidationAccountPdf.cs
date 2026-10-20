using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EstateFlow.Api.Services.Reports;

public static class LiquidationAccountPdf
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static string R(decimal v) =>
        (v < 0 ? "-R " : "R ") + Math.Abs(v).ToString("N2", Inv);

    private static string D(DateTime? d) =>
        d.HasValue ? d.Value.ToString("dd MMMM yyyy", Inv) : "–";

    public static byte[] Generate(LdAccount a)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().AlignCenter().Text("LIQUIDATION AND DISTRIBUTION ACCOUNT").Bold().FontSize(14);
                    h.Item().AlignCenter().Text($"in the estate of the late {a.DeceasedName}").FontSize(11);
                    h.Item().AlignCenter().Text($"{a.AccountType} account  ·  DRAFT generated from captured data – for review by the attorney")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    h.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(10);

                    // ── Particulars ─────────────────────────────
                    col.Item().Element(c => Particulars(c, a));

                    // ── Assets ──────────────────────────────────
                    col.Item().Element(c => Section(c, "SCHEDULE A – ASSETS", s =>
                    {
                        Group(s, "Immovable property", a.Immovable, a.ImmovableTotal);
                        Group(s, "Movable assets", a.Movable, a.MovableTotal);
                        Group(s, "Claims in favour of the estate", a.Claims, a.ClaimsTotal);
                        Total(s, a.InCommunity ? "Gross value of the joint estate" : "Gross value of assets", a.GrossTotal);
                    }));

                    // ── Liabilities ─────────────────────────────
                    col.Item().Element(c => Section(c, "SCHEDULE B – LIABILITIES", s =>
                    {
                        if (a.Liabilities.Count == 0) Line(s, "None captured", "", null);
                        foreach (var l in a.Liabilities) Line(s, l.Description, l.Note, l.Amount);
                        Total(s, a.InCommunity ? "Total liabilities of the joint estate" : "Total liabilities", a.LiabilitiesTotal);
                    }));

                    if (a.InCommunity)
                    {
                        col.Item().Element(c => Section(c, "COMMUNITY OF PROPERTY", s =>
                        {
                            Line(s, "Joint estate: assets less liabilities", "", a.GrossTotal - a.LiabilitiesTotal);
                            Line(s, "Less: surviving spouse's half share", a.SpouseName, -a.SpouseHalf);
                            Line(s, "Deceased's half of the gross assets", "", a.DeceasedGross);
                            Line(s, "Deceased's half of the liabilities", "", a.DeceasedLiabilities);
                        }));
                    }

                    // ── Income after death ──────────────────────
                    if (a.Income.Count > 0)
                    {
                        col.Item().Element(c => Section(c, "SCHEDULE C – INCOME RECEIVED AFTER DEATH", s =>
                        {
                            foreach (var l in a.Income) Line(s, l.Description, l.Note, l.Amount);
                            Total(s, "Total income received after death", a.IncomeTotal);
                        }));
                    }

                    // ── Costs ───────────────────────────────────
                    col.Item().Element(c => Section(c, "SCHEDULE D – ADMINISTRATION COSTS", s =>
                    {
                        foreach (var l in a.Costs) Line(s, l.Description, l.Note, l.Amount);
                        Total(s, "Total administration costs", a.CostsTotal);
                    }));

                    // ── Estate duty ─────────────────────────────
                    col.Item().Element(c => Section(c, "ESTATE DUTY", s =>
                    {
                        Line(s, "Deceased's share of the gross value of assets", "", a.DeceasedGross);
                        if (a.DeemedProperty > 0m)
                            Line(s, "Add: deemed property (s3(3))", "", a.DeemedProperty);
                        Line(s, "Less: liabilities", "", -a.DeceasedLiabilities);
                        Line(s, "Less: administration costs", "excluding the fee on post-death income", -a.CapitalCostsTotal);
                        Line(s, "Net value of the estate for duty purposes", "", a.NetValueForDuty);
                        Line(s, "Less: bequests to the surviving spouse (s4(q))", "", -a.SpouseDeduction);
                        Line(s, "Less: bequests to public benefit organisations (s4(h))", "", -a.CharityDeduction);
                        if (a.OtherDeductions > 0m)
                            Line(s, "Less: other deductions", "", -a.OtherDeductions);
                        Line(s, "Less: abatement (s4A)", "", -a.Abatement);
                        Line(s, "Dutiable amount", "", a.DutiableAmount);
                        if (a.EstateDuty > 0m)
                        {
                            Line(s, $"Duty at {a.DutyLowRate:0.##}% on the first R {a.DutyThreshold:N0}", "", a.DutyOnLowBand);
                            if (a.DutyOnHighBand > 0m)
                                Line(s, $"Duty at {a.DutyHighRate:0.##}% on the balance", "", a.DutyOnHighBand);
                        }
                        Total(s, "Estate duty payable", a.EstateDuty);
                    }));

                    // ── Distribution ────────────────────────────
                    col.Item().Element(c => Section(c, "DISTRIBUTION ACCOUNT", s =>
                    {
                        Line(s, "Deceased's share of the gross value of assets", "", a.DeceasedGross);
                        Line(s, "Less: liabilities", "", -a.DeceasedLiabilities);
                        Line(s, "Less: administration costs", "", -a.CapitalCostsTotal);
                        if (a.IncomeTotal > 0m)
                        {
                            Line(s, "Add: income received after death", "", a.IncomeTotal);
                            Line(s, "Less: executor's fee and VAT on that income", "", -a.IncomeCostsTotal);
                        }
                        Line(s, "Net distributable estate", "", a.NetEstate);
                        Line(s, "Less: estate duty", "", -a.EstateDuty);
                        Line(s, "Less: cash legacies", "", -a.CashLegaciesTotal);
                        Line(s, "Less: special bequests (assets awarded in kind)", "", -a.SpecialBequestsTotal);
                        Total(s, "Residue for distribution", a.Residue);
                    }));

                    col.Item().Element(c => HeirsTable(c, a));

                    // ── Notes ───────────────────────────────────
                    if (a.Notes.Count > 0)
                    {
                        col.Item().Column(n =>
                        {
                            n.Item().Text("NOTES").Bold().FontSize(9);
                            foreach (var note in a.Notes)
                                n.Item().Text("•  " + note).FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                    }

                    // ── Signature ───────────────────────────────
                    col.Item().PaddingTop(24).Column(sig =>
                    {
                        sig.Item().Text("Dated at " + (string.IsNullOrWhiteSpace(a.PlaceOfAccount) ? "____________________" : a.PlaceOfAccount)
                            + " on " + (a.DateOfAccount.HasValue ? D(a.DateOfAccount) : "____________________"));
                        sig.Item().PaddingTop(28).Width(220).LineHorizontal(0.7f);
                        sig.Item().Text(a.Executors.Count > 0 ? string.Join(", ", a.Executors) : "Executor");
                    });

                    // ── Internal checks (not part of the account) ──
                    if (a.Checks.Count > 0)
                    {
                        col.Item().PageBreak();
                        col.Item().Column(k =>
                        {
                            k.Spacing(4);
                            k.Item().Text("INTERNAL – ITEMS TO RESOLVE BEFORE LODGING").Bold().FontSize(11).FontColor(Colors.Red.Darken2);
                            k.Item().Text("This page is a working checklist generated from the captured data. It is not part of the account – remove it before the account is lodged.")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                            foreach (var chk in a.Checks)
                                k.Item().Text("[   ]  " + chk).FontSize(9);
                        });
                    }
                });

                page.Footer().Row(f =>
                {
                    f.RelativeItem().Text($"File {a.FileNumber}  ·  Master's ref {a.MastersReference}").FontSize(7).FontColor(Colors.Grey.Darken1);
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

    // ───────────────────────── helpers ─────────────────────────

    private static void Particulars(IContainer c, LdAccount a)
    {
        void Pair(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(120).Text(label).FontColor(Colors.Grey.Darken1);
                r.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "–" : value);
            });
        }

        c.Row(row =>
        {
            row.RelativeItem().Column(l =>
            {
                Pair(l, "Deceased", a.DeceasedName);
                Pair(l, "Identity number", a.IdNumber);
                Pair(l, "Date of death", D(a.DateOfDeath));
                Pair(l, "Place of death", a.PlaceOfDeath);
                Pair(l, "Marital status", a.MaritalStatus);
                Pair(l, "Matrimonial regime", a.MatrimonialRegime);
                Pair(l, "Surviving spouse", a.SpouseName);
            });
            row.ConstantItem(16);
            row.RelativeItem().Column(r2 =>
            {
                Pair(r2, "File number", a.FileNumber);
                Pair(r2, "Master's reference", a.MastersReference);
                Pair(r2, "Master's office", a.MastersOffice);
                Pair(r2, "Testate / intestate", a.IsTestate ? "Testate" : "Intestate");
                Pair(r2, "Date of will", D(a.WillDate));
                Pair(r2, "Letters of executorship", D(a.LettersDate));
                Pair(r2, "Executor(s)", string.Join(", ", a.Executors));
                Pair(r2, "Estate bank account",
                    string.Join(" · ", new[] { a.BankName, a.BankBranchCode, a.BankAccountNumber }
                        .Where(x => !string.IsNullOrWhiteSpace(x))));
            });
        });
    }

    private static void Section(IContainer c, string title, Action<ColumnDescriptor> body)
    {
        c.Column(col =>
        {
            col.Item().Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(4)
                .Text(title).Bold().FontSize(9);
            col.Item().Column(body);
        });
    }

    private static void Group(ColumnDescriptor s, string heading, List<LdLine> lines, decimal total)
    {
        s.Item().PaddingTop(4).Text(heading).SemiBold().Italic();
        if (lines.Count == 0)
            Line(s, "None captured", "", null);
        foreach (var l in lines)
            Line(s, l.Description, l.Note, l.Amount);
        s.Item().Row(r =>
        {
            r.RelativeItem().AlignRight().Text("Subtotal").FontColor(Colors.Grey.Darken1);
            r.ConstantItem(100).AlignRight().Text(R(total)).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void Line(ColumnDescriptor s, string description, string note, decimal? amount)
    {
        s.Item().BorderBottom(0.4f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Row(r =>
        {
            r.RelativeItem().Column(d =>
            {
                d.Item().Text(description);
                if (!string.IsNullOrWhiteSpace(note))
                    d.Item().Text(note).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
            });
            if (amount.HasValue)
                r.ConstantItem(100).AlignRight().Text(R(amount.Value));
            else
                r.ConstantItem(100);
        });
    }

    private static void Total(ColumnDescriptor s, string label, decimal amount)
    {
        s.Item().PaddingTop(3).BorderTop(0.8f).Row(r =>
        {
            r.RelativeItem().Text(label).Bold();
            r.ConstantItem(100).AlignRight().Text(R(amount)).Bold();
        });
    }

    private static IContainer Cell(IContainer c) =>
        c.BorderBottom(0.4f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(2);

    private static void HeirsTable(IContainer c, LdAccount a)
    {
        c.Table(t =>
        {
            t.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(3);   // name
                cols.RelativeColumn(1);   // fraction
                cols.RelativeColumn(2);   // cash legacy
                cols.RelativeColumn(2);   // special bequests
                cols.RelativeColumn(2);   // residue
                cols.RelativeColumn(2);   // advances
                cols.RelativeColumn(2);   // total
            });

            t.Header(h =>
            {
                h.Cell().Element(Cell).Text("Heir").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Share").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Cash legacy").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Special bequests").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Residue").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Advances").Bold();
                h.Cell().Element(Cell).AlignRight().Text("Total").Bold();
            });

            foreach (var h in a.Heirs)
            {
                t.Cell().Element(Cell).Column(n =>
                {
                    n.Item().Text(h.Name);
                    if (!string.IsNullOrWhiteSpace(h.Relationship))
                        n.Item().Text(h.Relationship).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                });
                t.Cell().Element(Cell).AlignRight().Text(h.ResidueFraction);
                t.Cell().Element(Cell).AlignRight().Text(R(h.CashLegacy));
                t.Cell().Element(Cell).AlignRight().Text(R(h.SpecialBequests));
                t.Cell().Element(Cell).AlignRight().Text(R(h.ResidueShare));
                t.Cell().Element(Cell).AlignRight().Text(R(-h.AdvancesPaid));
                t.Cell().Element(Cell).AlignRight().Text(R(h.Total)).Bold();
            }

            t.Cell().ColumnSpan(6).PaddingTop(3).Text("Total payable to heirs (after advances already paid)").Bold();
            t.Cell().PaddingTop(3).AlignRight().Text(R(a.Heirs.Sum(x => x.Total))).Bold();
        });
    }
}
