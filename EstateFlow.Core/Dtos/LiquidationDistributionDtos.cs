namespace EstateFlow.Core.Dtos;

public class LiabilityDto
{
    public Guid LiabilityId { get; set; }
    public string CreditorName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    public string? Category { get; set; }
    public string? AccountReference { get; set; }
    public decimal InterestAfterDeath { get; set; }
    public string? Status { get; set; } = "Admitted";
    public DateTime? DateLodged { get; set; }
    public string? SecuredAgainst { get; set; }
    public bool IsTakenOverByHeir { get; set; }
    public bool IsPaid { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? DatePaid { get; set; }
    public bool IsCoveredByCreditLife { get; set; }

    public string StatusSummary =>
        (IsPaid ? $"Paid R {AmountPaid:N2}" : (Status ?? "Admitted"))
        + (IsTakenOverByHeir ? " · taken over by heir" : "")
        + (IsCoveredByCreditLife ? " · credit life" : "");
}

public class CreateLiabilityRequest
{
    public string CreditorName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    public string? Category { get; set; }
    public string? AccountReference { get; set; }
    public decimal InterestAfterDeath { get; set; }
    public string? Status { get; set; } = "Admitted";
    public DateTime? DateLodged { get; set; }
    public string? SecuredAgainst { get; set; }
    public bool IsTakenOverByHeir { get; set; }
    public bool IsPaid { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? DatePaid { get; set; }
    public bool IsCoveredByCreditLife { get; set; }
}

public class LiquidationDistributionDraftDto
{
    public Guid EstateId { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string DeceasedName { get; set; } = string.Empty;
    public string? MastersReference { get; set; }

    // Schedule totals (full joint estate where married in community of property)
    public decimal MovableAssetsTotal { get; set; }
    public decimal ImmovableAssetsTotal { get; set; }
    public decimal FinancialClaimsTotal { get; set; }
    public decimal GrossEstateTotal { get; set; }
    public decimal LiabilitiesTotal { get; set; }

    // Community of property
    public bool InCommunity { get; set; }
    public decimal SpouseHalf { get; set; }
    public decimal DeceasedGrossTotal { get; set; }

    // Costs. AdministrationExpenses now carries the TOTAL costs (fees, VAT, Master's fee, expenses).
    public decimal AdministrationExpenses { get; set; }
    public decimal MastersFee { get; set; }
    public decimal IncomeAfterDeath { get; set; }
    public decimal TotalDeductions { get; set; }

    // Estate duty
    public decimal DeemedProperty { get; set; }
    public decimal NetValueForDuty { get; set; }
    public decimal DutiableAmount { get; set; }
    public decimal EstateDuty { get; set; }

    // Distribution
    public decimal NetEstateBeforeDuty { get; set; }
    public decimal CashLegaciesTotal { get; set; }
    public decimal SpecialBequestsTotal { get; set; }
    public decimal ResidueForDistribution { get; set; }

    public int BeneficiaryCount { get; set; }
    public decimal EqualIndicativeShare { get; set; }

    // Added so the screen shows exactly what the PDF shows (both come from LiquidationAccountBuilder).
    public decimal CapitalCostsTotal { get; set; }
    public decimal IncomeCostsTotal { get; set; }
    public bool MastersFeeFromScale { get; set; }
    public decimal SpouseDeduction { get; set; }
    public decimal CharityDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal Abatement { get; set; }
    public decimal DutyLowRate { get; set; }
    public decimal DutyHighRate { get; set; }
    public decimal DutyThreshold { get; set; }
    public decimal DutyOnLowBand { get; set; }
    public decimal DutyOnHighBand { get; set; }
    public decimal DeceasedLiabilitiesTotal { get; set; }

    public List<LiquidationDistributionLineDto> Income { get; set; } = new();
    // Missing or doubtful information – fix before lodging with the Master.
    public List<string> Checks { get; set; } = new();
    public bool HasChecks => Checks.Count > 0;
    public string ChecksHeading =>
        Checks.Count == 1 ? "1 item to resolve before lodging" : $"{Checks.Count} items to resolve before lodging";

    public string DutyBreakdownText =>
        EstateDuty <= 0m
            ? "No estate duty payable on these figures."
            : DutyOnHighBand > 0m
                ? $"{DutyLowRate:0.##}% on the first R {DutyThreshold:N0} (R {DutyOnLowBand:N2}) + {DutyHighRate:0.##}% on the balance (R {DutyOnHighBand:N2})"
                : $"{DutyLowRate:0.##}% of the dutiable amount (R {DutyOnLowBand:N2})";

    public List<LiquidationDistributionLineDto> Assets { get; set; } = new();
    public List<LiquidationDistributionLineDto> Deductions { get; set; } = new();
    public List<LiquidationDistributionLineDto> Costs { get; set; } = new();
    public List<LiquidationDistributionBeneficiaryDto> Beneficiaries { get; set; } = new();
    public List<string> Notes { get; set; } = new();

    public bool HasNotes => Notes.Count > 0;
}

public class LiquidationDistributionLineDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class LiquidationDistributionBeneficiaryDto
{
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string HeirType { get; set; } = string.Empty;
    public string ResidueFraction { get; set; } = string.Empty;
    public decimal CashLegacy { get; set; }
    public decimal SpecialBequests { get; set; }
    public decimal ResidueShare { get; set; }
    public decimal AdvancesPaid { get; set; }
    public decimal TotalDistribution { get; set; }
    public decimal IndicativeEqualShare { get; set; }
}
