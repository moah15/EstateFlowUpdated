using EstateFlow.Core.Dtos;
using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Reports;

public class LdLine
{
    public string Description { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class LdHeirRow
{
    public string Name { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string HeirType { get; set; } = string.Empty;
    public string ResidueFraction { get; set; } = string.Empty;
    public decimal CashLegacy { get; set; }
    public decimal SpecialBequests { get; set; }
    public List<string> SpecialItems { get; set; } = new();
    public decimal ResidueShare { get; set; }
    public decimal AdvancesPaid { get; set; }
    public decimal Total => CashLegacy + SpecialBequests + ResidueShare - AdvancesPaid;
}

public class LdAccount
{
    // Header
    public string FileNumber { get; set; } = string.Empty;
    public string MastersReference { get; set; } = string.Empty;
    public string MastersOffice { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string DeceasedName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string MatrimonialRegime { get; set; } = string.Empty;
    public string SpouseName { get; set; } = string.Empty;
    public DateTime DateOfDeath { get; set; }
    public string PlaceOfDeath { get; set; } = string.Empty;
    public DateTime? DateOfAccount { get; set; }
    public string PlaceOfAccount { get; set; } = string.Empty;
    public bool IsTestate { get; set; }
    public DateTime? WillDate { get; set; }
    public DateTime? LettersDate { get; set; }
    public List<string> Executors { get; set; } = new();

    // Estate bank account
    public string BankName { get; set; } = string.Empty;
    public string BankBranchCode { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;

    // Assets
    public List<LdLine> Immovable { get; set; } = new();
    public List<LdLine> Movable { get; set; } = new();
    public List<LdLine> Claims { get; set; } = new();
    public decimal ImmovableTotal { get; set; }
    public decimal MovableTotal { get; set; }
    public decimal ClaimsTotal { get; set; }
    public decimal GrossTotal { get; set; }

    // Liabilities
    public List<LdLine> Liabilities { get; set; } = new();
    public decimal LiabilitiesTotal { get; set; }

    // Community of property
    public bool InCommunity { get; set; }
    public decimal SpouseHalf { get; set; }
    public decimal DeceasedGross { get; set; }
    public decimal DeceasedLiabilities { get; set; }

    // Income received after death (from the cashbook)
    public List<LdLine> Income { get; set; } = new();
    public decimal IncomeTotal { get; set; }

    // Costs
    public List<LdLine> Costs { get; set; } = new();
    public decimal CostsTotal { get; set; }
    public decimal CapitalCostsTotal { get; set; }
    public decimal IncomeCostsTotal { get; set; }
    public decimal MastersFee { get; set; }
    public bool MastersFeeFromScale { get; set; }

    // Estate duty
    public decimal DeemedProperty { get; set; }
    public decimal NetValueForDuty { get; set; }
    public decimal SpouseDeduction { get; set; }
    public decimal CharityDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal Abatement { get; set; }
    public decimal DutiableAmount { get; set; }
    public decimal DutyLowRate { get; set; }
    public decimal DutyHighRate { get; set; }
    public decimal DutyThreshold { get; set; }
    public decimal DutyOnLowBand { get; set; }
    public decimal DutyOnHighBand { get; set; }
    public decimal EstateDuty { get; set; }

    // Distribution
    public decimal NetEstate { get; set; }              // distributable value before duty and legacies
    public decimal CashLegaciesTotal { get; set; }
    public decimal SpecialBequestsTotal { get; set; }
    public decimal Residue { get; set; }
    public List<LdHeirRow> Heirs { get; set; } = new();
    public decimal FractionsTotal { get; set; }

    public List<string> Notes { get; set; } = new();
    // Information that is missing or looks wrong – shown prominently so it is fixed before lodging.
    public List<string> Checks { get; set; } = new();
}

public static class LiquidationAccountBuilder
{
    private const string CommunityRegime = "In community of property";

    // Statutory figures (see the SARS "Other taxes" page and Schedule 2 of the Regulations to the
    // Administration of Estates Act). They are defaults – estate-level overrides win where they exist.
    public const decimal DefaultHigherDutyRate = 25m;
    public const decimal DefaultDutyThreshold = 30_000_000m;
    public const decimal IncomeFeePercentage = 6m;          // executor's fee on income accrued after death
    public const decimal MaxCapitalFeePercentage = 3.5m;    // prescribed maximum on gross value of assets
    public const decimal SmallEstateLimit = 250_000m;       // below this: no Master's fee / s18(3) procedure

    // ─────────────────────────────────────────────────────────────
    // Public helpers (unit-tested)
    // ─────────────────────────────────────────────────────────────

    // Master's fee: nil below R250 000; R600 from R250 000 up to R400 000; thereafter R200 for
    // each complete further R100 000; capped at R7 000. Assessed on the gross value of the
    // whole (joint) estate.
    public static decimal MastersFeeScale(decimal grossValue)
    {
        if (grossValue < SmallEstateLimit) return 0m;
        if (grossValue < 400_000m) return 600m;

        var steps = Math.Floor((grossValue - 400_000m) / 100_000m);
        return Math.Min(7_000m, 600m + 200m * steps);
    }

    // Two-band estate duty on the dutiable amount (after the abatement).
    public static decimal EstateDutyFor(decimal dutiable, decimal lowRate, decimal highRate, decimal threshold)
    {
        if (dutiable <= 0m) return 0m;
        if (dutiable <= threshold) return dutiable * lowRate / 100m;
        return threshold * lowRate / 100m + (dutiable - threshold) * highRate / 100m;
    }

    // 13 digits, plausible month/day, Luhn check digit.
    public static bool IsValidSaIdNumber(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        var s = id.Trim();
        if (s.Length != 13 || !s.All(char.IsDigit)) return false;

        var month = int.Parse(s.Substring(2, 2));
        var day = int.Parse(s.Substring(4, 2));
        if (month < 1 || month > 12 || day < 1 || day > 31) return false;

        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = s[12 - i] - '0';
            if (i % 2 == 1)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }
        return sum % 10 == 0;
    }

    // ─────────────────────────────────────────────────────────────
    // Build
    // ─────────────────────────────────────────────────────────────

    public static LdAccount Build(Estate estate)
    {
        var acc = new LdAccount
        {
            FileNumber = estate.FileNumber,
            MastersReference = estate.MastersReference ?? string.Empty,
            MastersOffice = estate.MastersOffice ?? string.Empty,
            AccountType = estate.AccountType,
            DeceasedName = estate.Deceased?.FullName ?? string.Empty,
            IdNumber = estate.Deceased?.IdNumber ?? string.Empty,
            MaritalStatus = estate.Deceased?.MaritalStatus ?? string.Empty,
            MatrimonialRegime = estate.Deceased?.MatrimonialRegime ?? string.Empty,
            SpouseName = estate.Deceased?.SpouseFullName ?? string.Empty,
            DateOfDeath = estate.DateOfDeath,
            PlaceOfDeath = estate.Deceased?.PlaceOfDeath ?? string.Empty,
            DateOfAccount = estate.DateOfAccount,
            PlaceOfAccount = estate.PlaceOfAccount ?? string.Empty,
            IsTestate = estate.IsTestate,
            WillDate = estate.Deceased?.WillDate,
            LettersDate = estate.LettersOfExecutorshipDate,
            BankName = estate.EstateBankName ?? string.Empty,
            BankBranchCode = estate.EstateBankBranchCode ?? string.Empty,
            BankAccountNumber = estate.EstateBankAccountNumber ?? string.Empty
        };

        acc.Executors = estate.Executors
            .Select(e => string.IsNullOrWhiteSpace(e.Role) ? e.FullName : $"{e.FullName} ({e.Role})")
            .ToList();

        var heirs = estate.Beneficiaries.OrderBy(b => b.DisplayOrder).ToList();
        var specialByHeir = new Dictionary<Guid, decimal>();
        var specialItemsByHeir = new Dictionary<Guid, List<string>>();

        acc.InCommunity = acc.MatrimonialRegime == CommunityRegime;

        // In community of property the deceased can only bequeath their half of a joint-estate asset.
        var bequestFactor = acc.InCommunity ? 0.5m : 1m;
        var specialBequestsWithoutHeir = new List<string>();

        void AllocateSpecial(string description, string? ids, bool isSpecial, decimal value)
        {
            if (!isSpecial) return;

            var wanted = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();
            var valid = wanted.Where(w => heirs.Any(h => h.BeneficiaryId == w)).ToList();

            if (valid.Count == 0)
            {
                specialBequestsWithoutHeir.Add(description);
                return;
            }

            var bequeathed = value * bequestFactor;
            foreach (var id in valid)
            {
                specialByHeir[id] = specialByHeir.GetValueOrDefault(id) + bequeathed / valid.Count;
                if (!specialItemsByHeir.TryGetValue(id, out var items))
                    specialItemsByHeir[id] = items = new List<string>();
                items.Add(valid.Count > 1 ? $"{description} (1/{valid.Count} share)" : description);
            }
        }

        // ── Assets ──────────────────────────────────────────────
        foreach (var p in estate.ImmovableProperties)
        {
            var baseValue = p.MarketValue ?? p.MunicipalValue;
            var share = (p.ShareNumerator.HasValue && p.ShareDenominator is > 0)
                ? (decimal)p.ShareNumerator.Value / p.ShareDenominator.Value
                : 1m;
            var value = Math.Round(baseValue * share, 2);

            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(p.ErfNumber)) notes.Add($"Erf {p.ErfNumber}");
            if (!string.IsNullOrWhiteSpace(p.SchemeName)) notes.Add($"{p.SchemeName}, Section {p.SectionNumber}");
            if (!string.IsNullOrWhiteSpace(p.Township)) notes.Add(p.Township!);
            if (!string.IsNullOrWhiteSpace(p.TitleDeedNumber)) notes.Add($"Title deed {p.TitleDeedNumber}");
            if (share < 1m) notes.Add($"{p.ShareNumerator}/{p.ShareDenominator} share");
            if (!p.MarketValue.HasValue) notes.Add("municipal value used (no market value captured)");
            if (p.IsSoldByEstate)
                notes.Add($"sold by estate{(p.SalePrice.HasValue ? $" for R {p.SalePrice:N2}" : "")}");

            acc.Immovable.Add(new LdLine
            {
                Description = p.Description,
                Note = string.Join("; ", notes),
                Amount = value
            });
            AllocateSpecial(p.Description, p.AwardedToBeneficiaryIds, p.IsSpecialBequest, value);

            if (value <= 0m)
                acc.Checks.Add($"Immovable property \"{p.Description}\" has no value captured.");
            if (!p.MarketValue.HasValue && p.MunicipalValue > 0m)
                acc.Checks.Add($"Immovable property \"{p.Description}\": no market value captured – the municipal value is used. The Master expects a market value at date of death.");
        }

        foreach (var m in estate.MovableAssets)
        {
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(m.ShareCompany))
                notes.Add($"{m.NumberOfShares} {m.ShareClass} shares in {m.ShareCompany}");
            if (m.IsSoldOrReducedToCash)
                notes.Add($"sold by estate{(m.SaleProceeds.HasValue ? $" for R {m.SaleProceeds:N2}" : "")}");

            acc.Movable.Add(new LdLine { Description = m.Description, Note = string.Join("; ", notes), Amount = m.Value });
            AllocateSpecial(m.Description, m.AwardedToBeneficiaryIds, m.IsSpecialBequest, m.Value);

            if (m.Value <= 0m)
                acc.Checks.Add($"Movable asset \"{m.Description}\" has no value captured.");
        }

        foreach (var c in estate.FinancialClaims.Where(fc => fc.IsInFavourOfEstate))
        {
            var value = c.Value + c.AccruedInterest;
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(c.ClaimType)) notes.Add(c.ClaimType!);
            if (!string.IsNullOrWhiteSpace(c.AccountReference)) notes.Add($"ref {c.AccountReference}");
            if (c.AccruedInterest > 0) notes.Add($"incl. interest R {c.AccruedInterest:N2}");
            if (c.IsCollectedAsCash) notes.Add($"collected R {c.CollectedAmount:N2}");

            acc.Claims.Add(new LdLine { Description = c.InstitutionName, Note = string.Join("; ", notes), Amount = value });
            AllocateSpecial(c.InstitutionName, c.AwardedToBeneficiaryIds, c.IsSpecialBequest, value);
        }

        foreach (var d in specialBequestsWithoutHeir)
            acc.Checks.Add($"\"{d}\" is marked as a special bequest but is not allocated to any heir – it is left in the residue.");

        acc.ImmovableTotal = acc.Immovable.Sum(x => x.Amount);
        acc.MovableTotal = acc.Movable.Sum(x => x.Amount);
        acc.ClaimsTotal = acc.Claims.Sum(x => x.Amount);
        acc.GrossTotal = acc.ImmovableTotal + acc.MovableTotal + acc.ClaimsTotal;

        // ── Liabilities (claims against the estate count as liabilities) ──
        foreach (var l in estate.Liabilities.Where(x => !string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)))
        {
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(l.Category)) notes.Add(l.Category!);
            if (!string.IsNullOrWhiteSpace(l.AccountReference)) notes.Add($"ref {l.AccountReference}");
            if (l.InterestAfterDeath > 0) notes.Add($"incl. interest after death R {l.InterestAfterDeath:N2}");
            if (!string.IsNullOrWhiteSpace(l.Status) && l.Status != "Admitted") notes.Add(l.Status!);
            if (l.IsTakenOverByHeir) notes.Add("taken over by heir");

            acc.Liabilities.Add(new LdLine
            {
                Description = string.IsNullOrWhiteSpace(l.Description) ? l.CreditorName : $"{l.CreditorName} – {l.Description}",
                Note = string.Join("; ", notes),
                Amount = l.Amount + l.InterestAfterDeath
            });
        }

        foreach (var c in estate.FinancialClaims.Where(fc => !fc.IsInFavourOfEstate))
        {
            acc.Liabilities.Add(new LdLine
            {
                Description = $"{c.InstitutionName} – claim against the estate",
                Note = string.IsNullOrWhiteSpace(c.ClaimType) ? string.Empty : c.ClaimType!,
                Amount = c.Value + c.AccruedInterest
            });
        }

        acc.LiabilitiesTotal = acc.Liabilities.Sum(x => x.Amount);

        var notAdmitted = estate.Liabilities.Count(x =>
            string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Status, "Disputed", StringComparison.OrdinalIgnoreCase));
        if (notAdmitted > 0)
            acc.Checks.Add($"{notAdmitted} liability(ies) are still Pending/Disputed but are deducted. Admit or reject them before lodging.");

        // ── Community of property: deceased's half ──────────────
        var deceasedGross = acc.GrossTotal;
        var deceasedLiabilities = acc.LiabilitiesTotal;
        if (acc.InCommunity)
        {
            deceasedGross = Math.Round(acc.GrossTotal / 2m, 2);
            deceasedLiabilities = Math.Round(acc.LiabilitiesTotal / 2m, 2);
            acc.SpouseHalf = Math.Round((acc.GrossTotal - acc.LiabilitiesTotal) / 2m, 2);
            acc.Notes.Add("Married in community of property: the deceased's estate is one half of the joint estate (assets, liabilities and executor's fee base). Special bequests of joint-estate assets are taken at the deceased's half share. Assets that are the deceased's separate property (e.g. excluded by a will or an inheritance condition) must be adjusted manually.");
        }
        acc.DeceasedGross = deceasedGross;
        acc.DeceasedLiabilities = deceasedLiabilities;

        // ── Income received after death (cashbook) ─────────────
        var cashbook = estate.CashbookEntries ?? new List<CashbookEntry>();

        foreach (var g in cashbook
                     .Where(e => e.EntryType == CashbookTypes.IncomeAfterDeath)
                     .GroupBy(e => string.IsNullOrWhiteSpace(e.Category) ? "Other income" : e.Category.Trim())
                     .OrderBy(grp => grp.Key))
        {
            acc.Income.Add(new LdLine
            {
                Description = g.Key,
                Note = g.Count() > 1 ? $"{g.Count()} receipts" : g.First().Description,
                Amount = g.Sum(e => e.Amount)
            });
        }
        acc.IncomeTotal = acc.Income.Sum(x => x.Amount);

        // ── Costs ───────────────────────────────────────────────
        var capitalFee = Math.Round(deceasedGross * estate.ExecutorFeePercentage / 100m, 2);
        var incomeFee = Math.Round(acc.IncomeTotal * IncomeFeePercentage / 100m, 2);
        var vatRate = estate.IsVatRegistered ? estate.VatRate : 0m;
        var capitalVat = Math.Round(capitalFee * vatRate / 100m, 2);
        var incomeVat = Math.Round(incomeFee * vatRate / 100m, 2);

        acc.Costs.Add(new LdLine
        {
            Description = $"Executor's fee ({estate.ExecutorFeePercentage:0.##}% of gross value of assets)",
            Note = $"on R {deceasedGross:N2}",
            Amount = capitalFee
        });
        if (estate.IsVatRegistered)
            acc.Costs.Add(new LdLine { Description = $"VAT on executor's fee ({estate.VatRate:0.##}%)", Amount = capitalVat });

        if (acc.IncomeTotal > 0m)
        {
            acc.Costs.Add(new LdLine
            {
                Description = $"Executor's fee on income after death ({IncomeFeePercentage:0.##}%)",
                Note = $"on R {acc.IncomeTotal:N2}",
                Amount = incomeFee
            });
            if (estate.IsVatRegistered)
                acc.Costs.Add(new LdLine { Description = $"VAT on fee on income ({estate.VatRate:0.##}%)", Amount = incomeVat });
        }

        var mastersFee = estate.MastersFee;
        if (mastersFee <= 0m)
        {
            mastersFee = MastersFeeScale(acc.GrossTotal);
            acc.MastersFeeFromScale = true;
        }
        acc.MastersFee = mastersFee;
        if (mastersFee > 0m)
        {
            acc.Costs.Add(new LdLine
            {
                Description = "Master's fee",
                Note = acc.MastersFeeFromScale ? "per Schedule 2 scale on the gross value of the estate" : "as captured",
                Amount = mastersFee
            });
        }

        if (estate.AdministrationExpenses > 0m)
            acc.Costs.Add(new LdLine { Description = "Other administration expenses (lump sum on General tab)", Amount = estate.AdministrationExpenses });

        var adminFromCashbook = 0m;
        foreach (var g in cashbook
                     .Where(e => e.EntryType == CashbookTypes.AdministrationCost)
                     .GroupBy(e => string.IsNullOrWhiteSpace(e.Category) ? "Administration costs" : e.Category.Trim())
                     .OrderBy(grp => grp.Key))
        {
            var total = g.Sum(e => e.Amount);
            adminFromCashbook += total;
            acc.Costs.Add(new LdLine
            {
                Description = g.Key,
                Note = g.Count() > 1 ? $"{g.Count()} payments" : g.First().Description,
                Amount = total
            });
        }

        acc.IncomeCostsTotal = incomeFee + incomeVat;
        acc.CapitalCostsTotal = capitalFee + capitalVat + mastersFee + estate.AdministrationExpenses + adminFromCashbook;
        acc.CostsTotal = acc.CapitalCostsTotal + acc.IncomeCostsTotal;

        // ── Legacies ────────────────────────────────────────────
        var activeHeirs = heirs.Where(h => !h.IsPredeceased).ToList();
        acc.CashLegaciesTotal = activeHeirs.Sum(h => h.CashLegacyAmount);
        acc.SpecialBequestsTotal = Math.Round(activeHeirs.Sum(h => specialByHeir.GetValueOrDefault(h.BeneficiaryId)), 2);
        acc.FractionsTotal = activeHeirs
            .Where(h => h.ResidueNumerator.HasValue && h.ResidueDenominator is > 0)
            .Sum(h => (decimal)h.ResidueNumerator!.Value / h.ResidueDenominator!.Value);

        decimal Fraction(Beneficiary h) =>
            (h.ResidueNumerator.HasValue && h.ResidueDenominator is > 0)
                ? (decimal)h.ResidueNumerator.Value / h.ResidueDenominator.Value
                : 0m;

        // Capital residue = what is left of the deceased's estate after liabilities, costs, duty and legacies.
        decimal CapitalResidue(decimal duty) =>
            deceasedGross - deceasedLiabilities - acc.CapitalCostsTotal - duty
            - acc.CashLegaciesTotal - acc.SpecialBequestsTotal;

        var incomeResidue = acc.IncomeTotal - acc.IncomeCostsTotal;

        decimal HeirTotalBeforeAdvances(Beneficiary h, decimal capitalResidue) =>
            h.CashLegacyAmount
            + specialByHeir.GetValueOrDefault(h.BeneficiaryId)
            + Math.Max(0m, capitalResidue) * Fraction(h);

        acc.NetEstate = deceasedGross - deceasedLiabilities - acc.CapitalCostsTotal + incomeResidue;

        // ── Estate duty ─────────────────────────────────────────
        acc.DutyLowRate = estate.EstateDutyRate;
        acc.DutyHighRate = estate.EstateDutyHigherRate ? DefaultHigherDutyRate : acc.DutyLowRate;
        acc.DutyThreshold = estate.EstateDutyThreshold > 0m ? estate.EstateDutyThreshold : DefaultDutyThreshold;

        var abatement = estate.EstateDutyRebate;
        if (estate.PreviousSpousesCount > 0)
            abatement += Math.Max(0m, estate.EstateDutyRebate - estate.PreviousSpouseRebateUsed);
        acc.Abatement = abatement;

        acc.DeemedProperty = estate.DeemedPropertyForDuty;
        acc.OtherDeductions = estate.OtherDutyDeductions;

        // Post-death income is not part of the dutiable estate; deemed property (s3(3)) is.
        acc.NetValueForDuty = deceasedGross + estate.DeemedPropertyForDuty - deceasedLiabilities - acc.CapitalCostsTotal;

        decimal spouseDed = 0m, charityDed = 0m, dutiable = 0m, duty = 0m;

        // Spouse (s4(q)) and charity (s4(h)) bequests depend on the residue, which depends on the duty –
        // iterate to a fixed point (converges quickly because each pass scales by at most 25%).
        for (var i = 0; i < 80; i++)
        {
            var capRes = CapitalResidue(duty);
            spouseDed = activeHeirs.Where(h => h.IsSurvivingSpouse).Sum(h => HeirTotalBeforeAdvances(h, capRes));
            charityDed = activeHeirs
                .Where(h => !h.IsSurvivingSpouse && IsCharity(h))
                .Sum(h => HeirTotalBeforeAdvances(h, capRes));
            dutiable = Math.Max(0m, acc.NetValueForDuty - spouseDed - charityDed - acc.OtherDeductions - abatement);
            var next = EstateDutyFor(dutiable, acc.DutyLowRate, acc.DutyHighRate, acc.DutyThreshold);
            if (Math.Abs(next - duty) < 0.0000001m) { duty = next; break; }
            duty = next;
        }

        duty = Math.Round(duty, 2);
        {
            var capRes = CapitalResidue(duty);
            spouseDed = activeHeirs.Where(h => h.IsSurvivingSpouse).Sum(h => HeirTotalBeforeAdvances(h, capRes));
            charityDed = activeHeirs
                .Where(h => !h.IsSurvivingSpouse && IsCharity(h))
                .Sum(h => HeirTotalBeforeAdvances(h, capRes));
            dutiable = Math.Max(0m, acc.NetValueForDuty - spouseDed - charityDed - acc.OtherDeductions - abatement);
        }

        acc.SpouseDeduction = Math.Round(spouseDed, 2);
        acc.CharityDeduction = Math.Round(charityDed, 2);
        acc.DutiableAmount = Math.Round(dutiable, 2);
        acc.EstateDuty = duty;
        acc.DutyOnLowBand = Math.Round(Math.Min(dutiable, acc.DutyThreshold) * acc.DutyLowRate / 100m, 2);
        acc.DutyOnHighBand = Math.Round(Math.Max(0m, dutiable - acc.DutyThreshold) * acc.DutyHighRate / 100m, 2);

        // ── Distribution ────────────────────────────────────────
        var totalResidue = CapitalResidue(duty) + incomeResidue;
        acc.Residue = Math.Round(totalResidue, 2);

        foreach (var h in activeHeirs)
        {
            var frac = Fraction(h);
            acc.Heirs.Add(new LdHeirRow
            {
                Name = h.FullName,
                IdNumber = h.IdNumber ?? string.Empty,
                Relationship = string.IsNullOrWhiteSpace(h.RelationshipDescription) ? h.Relationship : h.RelationshipDescription!,
                HeirType = h.HeirType ?? string.Empty,
                ResidueFraction = h.ResidueNumerator.HasValue && h.ResidueDenominator.HasValue
                    ? $"{h.ResidueNumerator}/{h.ResidueDenominator}" : "–",
                CashLegacy = h.CashLegacyAmount,
                SpecialBequests = Math.Round(specialByHeir.GetValueOrDefault(h.BeneficiaryId), 2),
                SpecialItems = specialItemsByHeir.TryGetValue(h.BeneficiaryId, out var items) ? items : new List<string>(),
                ResidueShare = Math.Round(totalResidue * frac, 2),
                AdvancesPaid = h.AdvancesPaid
            });
        }

        // Each heir's share is rounded to the cent; put any 1–5 cent rounding difference on the
        // largest share so the distribution account balances exactly.
        if (acc.Heirs.Count > 0 && Math.Abs(acc.FractionsTotal - 1m) <= 0.0001m)
        {
            var diff = acc.Residue - acc.Heirs.Sum(x => x.ResidueShare);
            if (diff != 0m && Math.Abs(diff) <= 0.05m)
            {
                var largest = acc.Heirs.OrderByDescending(x => x.ResidueShare).First();
                largest.ResidueShare += diff;
            }
        }

        BuildNotes(estate, acc, heirs, activeHeirs, cashbook);
        BuildChecks(estate, acc, heirs, activeHeirs, cashbook);

        return acc;
    }

    private static bool IsCharity(Beneficiary h) =>
        !string.IsNullOrWhiteSpace(h.HeirType) &&
        h.HeirType.StartsWith("Charity", StringComparison.OrdinalIgnoreCase);

    private static void BuildNotes(
        Estate estate, LdAccount acc, List<Beneficiary> heirs, List<Beneficiary> activeHeirs, ICollection<CashbookEntry> cashbook)
    {
        if (acc.MastersFeeFromScale && acc.MastersFee > 0m)
            acc.Notes.Add("Master's fee was calculated from the prescribed scale because no amount was captured. Enter an amount on the General tab to override it.");

        if (acc.GrossTotal > 0m && acc.GrossTotal < SmallEstateLimit)
            acc.Notes.Add($"The gross value of the estate (R {acc.GrossTotal:N2}) is below R {SmallEstateLimit:N0}. Small estates are generally dealt with under s18(3) without Letters of Executorship and without a Master's fee – confirm the correct procedure with the Master.");

        if (estate.ExecutorFeePercentage > MaxCapitalFeePercentage)
            acc.Notes.Add($"The executor's fee percentage ({estate.ExecutorFeePercentage:0.##}%) is above the prescribed maximum of {MaxCapitalFeePercentage:0.##}% (plus VAT). The Master will not approve more than the tariff.");
        else
            acc.Notes.Add($"Executor's fee at {estate.ExecutorFeePercentage:0.##}% of the gross value of assets plus {IncomeFeePercentage:0.##}% on income after death (tariff maximums; the Master approves the final amount).");

        if (acc.IncomeTotal > 0m)
            acc.Notes.Add("Income received after death is added to the residue but is not part of the dutiable estate; the executor's fee on that income is not deducted for estate duty.");

        if (acc.CapitalCostsTotal > 0m)
            acc.Notes.Add("Administration costs are deducted in arriving at the dutiable amount. Costs that relate only to post-death income may not qualify – confirm with SARS practice.");

        if (acc.DeemedProperty > 0m)
            acc.Notes.Add("Deemed property is included for estate duty only; it is not distributed through the estate. Estate duty attributable to it may be recoverable from the recipients (s11 of the Estate Duty Act).");

        if (estate.PreviousSpousesCount > 0)
            acc.Notes.Add("The abatement includes the unused portion of the previously deceased spouse's abatement (s4A).");

        if (acc.DutiableAmount > acc.DutyThreshold)
            acc.Notes.Add($"Estate duty is {acc.DutyLowRate:0.##}% on the first R {acc.DutyThreshold:N0} of the dutiable amount and {acc.DutyHighRate:0.##}% on the balance.");

        acc.Notes.Add("Estate duty rates, the abatement, the Master's fee scale and the executor's tariff are the current statutory figures. Confirm them against SARS and the Master before lodging.");

        if (!estate.IsTestate)
            acc.Notes.Add("Intestate estate: the shares captured on the Heirs tab must follow the Intestate Succession Act – check them before lodging.");

        if (heirs.Any(h => h.IsPredeceased))
            acc.Notes.Add("Predeceased heirs are excluded from the distribution. If their shares pass to descendants or accrue to other heirs, capture that on the Heirs tab.");

        if (activeHeirs.Any(h => h.AdvancesPaid > 0m))
            acc.Notes.Add("Advances already paid to heirs are shown as deductions from the amount still payable to them.");

        if (acc.Residue < 0m)
            acc.Notes.Add("The estate is insolvent on these figures (negative residue). Consider the sequestration / insolvent-estate procedures before distributing.");
    }

    private static void BuildChecks(
        Estate estate, LdAccount acc, List<Beneficiary> heirs, List<Beneficiary> activeHeirs, ICollection<CashbookEntry> cashbook)
    {
        if (string.IsNullOrWhiteSpace(acc.DeceasedName))
            acc.Checks.Add("The deceased's full name is missing.");

        if (string.IsNullOrWhiteSpace(acc.IdNumber))
            acc.Checks.Add("The deceased's identity number is missing.");
        else if (!IsValidSaIdNumber(acc.IdNumber))
            acc.Checks.Add($"The deceased's identity number \"{acc.IdNumber}\" is not a valid South African ID number (13 digits with a correct check digit). Ignore this only for a foreign passport number.");

        if (string.IsNullOrWhiteSpace(acc.MastersReference))
            acc.Checks.Add("The Master's reference number is missing.");
        if (string.IsNullOrWhiteSpace(acc.MastersOffice))
            acc.Checks.Add("The Master's office is missing.");
        if (!acc.LettersDate.HasValue)
            acc.Checks.Add("The date of the Letters of Executorship is missing.");
        if (!acc.DateOfAccount.HasValue)
            acc.Checks.Add("The date of the account is missing.");
        if (string.IsNullOrWhiteSpace(acc.PlaceOfAccount))
            acc.Checks.Add("The place where the account is signed is missing.");
        if (acc.Executors.Count == 0 && !estate.FirmActsAsAgent)
            acc.Checks.Add("No executor has been captured.");
        if (acc.IsTestate && !acc.WillDate.HasValue)
            acc.Checks.Add("The estate is testate but the date of the will is missing.");

        var marital = acc.MaritalStatus ?? string.Empty;
        if (marital.StartsWith("Married", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(acc.MatrimonialRegime))
                acc.Checks.Add("The deceased was married but the matrimonial regime is missing.");
            if (string.IsNullOrWhiteSpace(acc.SpouseName))
                acc.Checks.Add("The deceased was married but the surviving spouse's name is missing.");
        }

        if (acc.DateOfDeath > DateTime.Today)
            acc.Checks.Add("The date of death is in the future.");
        if (acc.LettersDate.HasValue && acc.LettersDate.Value.Date < acc.DateOfDeath.Date)
            acc.Checks.Add("The Letters of Executorship are dated before the date of death.");

        if (acc.LettersDate.HasValue)
        {
            var due = acc.LettersDate.Value.AddMonths(6);
            if (acc.DateOfAccount.HasValue && acc.DateOfAccount.Value.Date > due.Date)
                acc.Checks.Add($"The account is dated more than six months after the Letters ({due:dd MMM yyyy}). An extension from the Master is required (s35(1)).");
            else
                acc.Notes.Add($"The L&D account is due within six months of the Letters, i.e. by {due:dd MMM yyyy} (s35(1)).");
        }

        if (heirs.Count == 0)
            acc.Checks.Add("No heirs have been captured.");
        else if (activeHeirs.Count > 0 && Math.Abs(acc.FractionsTotal - 1m) > 0.0001m &&
                 (acc.FractionsTotal > 0m || acc.Residue != 0m))
        {
            var allocated = acc.Heirs.Sum(x => x.ResidueShare);
            acc.Checks.Add($"The residue fractions add up to {acc.FractionsTotal:0.####}, not 1. Unallocated residue: R {(acc.Residue - allocated):N2}.");
        }

        foreach (var h in heirs)
        {
            if (!string.IsNullOrWhiteSpace(h.IdNumber) && !IsValidSaIdNumber(h.IdNumber))
                acc.Checks.Add($"Heir {h.FullName}: the ID number \"{h.IdNumber}\" is not a valid South African ID number (ignore for foreign passports).");
        }

        if (acc.DeemedProperty > 0m && string.IsNullOrWhiteSpace(estate.DeemedPropertyNote))
            acc.Checks.Add("Deemed property for estate duty is entered but has no description (e.g. which policies).");
        if (acc.OtherDeductions > 0m && string.IsNullOrWhiteSpace(estate.OtherDutyDeductionsNote))
            acc.Checks.Add("Other estate duty deductions are entered but have no description.");

        var unreconciled = cashbook.Count(e => !e.IsReconciled);
        if (cashbook.Count > 0 && unreconciled > 0)
            acc.Checks.Add($"{unreconciled} cashbook entr{(unreconciled == 1 ? "y is" : "ies are")} not yet reconciled to the estate bank statement.");

        if (cashbook.Count > 0 &&
            (string.IsNullOrWhiteSpace(acc.BankName) || string.IsNullOrWhiteSpace(acc.BankAccountNumber)))
            acc.Checks.Add("Cashbook entries exist but the estate bank account details are missing on the General tab.");
    }
}
