using EstateFlow.Core.Models;

namespace EstateFlow.Core.Dtos;

// comment test
public class EstateDto
{
    public Guid EstateId { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? MastersReference { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string EstateStatus { get; set; } = string.Empty;
    public bool IsDutiable { get; set; }
    public bool FirmActsAsAgent { get; set; }
    public string? AgentName { get; set; }
    public string? AgentReference { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }
    public decimal AdministrationExpenses { get; set; }
    public DateTime? DateOfAccount { get; set; }
    public string? PlaceOfAccount { get; set; }
    public string AccountType { get; set; } = "First and Final";
    public bool IsTestate { get; set; } = true;
    public string? MastersOffice { get; set; }
    public DateTime? LettersOfExecutorshipDate { get; set; }
    public decimal ExecutorFeePercentage { get; set; } = 3.5m;
    public bool IsVatRegistered { get; set; } = true;
    public decimal VatRate { get; set; } = 15m;
    public decimal MastersFee { get; set; }
    public decimal EstateDutyRebate { get; set; } = 3500000m;
    public decimal EstateDutyRate { get; set; } = 20m;
    public string? SourceOfEstate { get; set; }
    public string? EstateBankName { get; set; }
    public string? EstateBankBranchCode { get; set; }
    public string? EstateBankAccountNumber { get; set; }
    public bool HasMultipleLdAccounts { get; set; }
    public int PreviousSpousesCount { get; set; }
    public decimal PreviousSpouseRebateUsed { get; set; }
    public bool EstateDutyHigherRate { get; set; }
    public decimal EstateDutyThreshold { get; set; } = 3_500_000m;
    public decimal DeemedPropertyForDuty { get; set; }
    public string? DeemedPropertyNote { get; set; }
    public decimal OtherDutyDeductions { get; set; }
    public string? OtherDutyDeductionsNote { get; set; }
    public Guid ControllerId { get; set; }

    public EstateControllerDto? Controller { get; set; }
    public DeceasedDto? Deceased { get; set; }
    public List<ExecutorDto> Executors { get; set; } = new();
    public List<BeneficiaryDto> Beneficiaries { get; set; } = new();
    public List<MovableAssetDto> MovableAssets { get; set; } = new();
    public List<ImmovablePropertyDto> ImmovableProperties { get; set; } = new();
    public List<FinancialClaimDto> FinancialClaims { get; set; } = new();
    public List<LiabilityDto> Liabilities { get; set; } = new();
    public List<DiaryEntryDto> DiaryEntries { get; set; } = new();
    public List<ProgressStageDto> ProgressStages { get; set; } = new();
    public List<ScannedDocumentDto> ScannedDocuments { get; set; } = new();
    public List<CashbookEntryDto> CashbookEntries { get; set; } = new();
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
    
    
}

public class EstateControllerDto
{
    public Guid ControllerId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class DeceasedDto
{
    public Guid DeceasedId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string? MaritalStatus { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Surname { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? MatrimonialRegime { get; set; }
    public string? SpouseFullName { get; set; }
    public string? SpouseIdNumber { get; set; }
    public DateTime? WillDate { get; set; }
    public string? WillLocation { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool LivedInSouthAfrica { get; set; } = true;
    public DateTime? CodicilDate { get; set; }
}

public class ExecutorDto
{
    public Guid ExecutorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? MastersOffice { get; set; }
    
    public DateTime? LettersOfExecutorshipDate { get; set; }
    
    public string? LettersDocumentUrl { get; set; }
    public string? LettersDocumentName { get; set; }
    
   
    public string? LettersOfExecutorshipRef { get; set; }
   


}

public class BeneficiaryDto
{
    public Guid BeneficiaryId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string HeirType { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Surname { get; set; }
    public string? RelationshipDescription { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? WillClause { get; set; }
    public decimal CashLegacyAmount { get; set; }
    public int? ResidueNumerator { get; set; }
    public int? ResidueDenominator { get; set; }
    public decimal AdvancesPaid { get; set; }
    public bool IsSurvivingSpouse { get; set; }
    public bool IsMinor { get; set; }
    public string? GuardianOrTrustee { get; set; }
    public bool IsPredeceased { get; set; }
    public bool IsUltimateBeneficiaryPreviousEstate { get; set; }
    public string? BareDominiumFraction { get; set; }
    public int DisplayOrder { get; set; }

    public string ShareSummary =>
        string.Join(" · ",
            new[]
            {
                ResidueNumerator.HasValue && ResidueDenominator.HasValue
                    ? $"Residue {ResidueNumerator}/{ResidueDenominator}"
                    : "Special bequests only",
                CashLegacyAmount > 0 ? $"Cash legacy R {CashLegacyAmount:N2}" : null,
                string.IsNullOrWhiteSpace(WillClause) ? null : $"Clause {WillClause}",
                IsSurvivingSpouse ? "Surviving spouse" : null,
                IsMinor ? "Minor" : null,
                IsPredeceased ? "Predeceased" : null,
                IsUltimateBeneficiaryPreviousEstate ? $"Bare dominium {BareDominiumFraction}" : null
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class MovableAssetDto
{
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public bool IsVehicle { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? VinNumber { get; set; }
    public string? Make { get; set; }
    public string? AssetCategory { get; set; }
    public string? ValuerName { get; set; }
    public DateTime? ValuationDate { get; set; }
    public string? AwardedToBeneficiaryIds { get; set; }
    public string AwardedToNames { get; set; } = string.Empty;
    public bool IsSpecialBequest { get; set; }
    public bool IsSoldOrReducedToCash { get; set; }
    public decimal? SaleProceeds { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? BuyerName { get; set; }
    public string? ShareCompany { get; set; }
    public int? NumberOfShares { get; set; }
    public string? ShareClass { get; set; }
    public decimal? PriceCentsPerShare { get; set; }
    public decimal CumulativeDividend { get; set; }

    public string ValuationSummary =>
        string.Join(", ",
            new[] { ValuerName, ValuationDate?.ToString("dd MMM yyyy") }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

    public string ShareDetails =>
        AssetCategory == "Registered Share"
            ? string.Join(" ",
                new[]
                {
                    NumberOfShares?.ToString(),
                    ShareClass,
                    ShareCompany,
                    PriceCentsPerShare.HasValue ? $"@ {PriceCentsPerShare:0.##} cps" : null
                }.Where(s => !string.IsNullOrWhiteSpace(s)))
            : string.Empty;

    public string DispositionSummary
    {
        get
        {
            var parts = new List<string>();
            if (IsSoldOrReducedToCash)
                parts.Add(SaleProceeds.HasValue
                    ? $"Sold / reduced to cash (R {SaleProceeds:N2})"
                    : "Sold / reduced to cash");
            if (!string.IsNullOrWhiteSpace(AwardedToNames))
                parts.Add($"Awarded to {AwardedToNames}");
            if (IsSpecialBequest)
                parts.Add("Special bequest");
            return parts.Count == 0 ? "Not yet awarded" : string.Join(" · ", parts);
        }
    }
}

public class ImmovablePropertyDto
{
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TitleDeedNumber { get; set; }
    public decimal MunicipalValue { get; set; }
    public string? PropertyType { get; set; }
    public string? ErfNumber { get; set; }
    public string? Township { get; set; }
    public string? Division { get; set; }
    public string? Province { get; set; }
    public decimal? ExtentSquareMetres { get; set; }
    public string? StreetAddress { get; set; }
    public string? SchemeName { get; set; }
    public string? SectionNumber { get; set; }
    public string? ExclusiveUseAreas { get; set; }
    public int? ShareNumerator { get; set; }
    public int? ShareDenominator { get; set; }
    public decimal? MarketValue { get; set; }
    public string? ValuerName { get; set; }
    public DateTime? ValuationDate { get; set; }
    public string? AwardedToBeneficiaryIds { get; set; }
    public string AwardedToNames { get; set; } = string.Empty;
    public bool IsSpecialBequest { get; set; }
    public bool IsSoldByEstate { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? PurchaserName { get; set; }

    public string ShareText =>
        ShareNumerator.HasValue && ShareDenominator.HasValue
            ? $"{ShareNumerator}/{ShareDenominator}"
            : "Full ownership";

    public string LocationSummary
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(ErfNumber)) parts.Add($"Erf {ErfNumber}");
            if (!string.IsNullOrWhiteSpace(SchemeName)) parts.Add($"{SchemeName} Section {SectionNumber}");
            if (!string.IsNullOrWhiteSpace(Township)) parts.Add(Township!);
            if (!string.IsNullOrWhiteSpace(Division)) parts.Add(Division!);
            if (!string.IsNullOrWhiteSpace(Province)) parts.Add(Province!);
            return string.Join(", ", parts);
        }
    }

    public string ValuationSummary
    {
        get
        {
            if (!MarketValue.HasValue) return "Market value not captured";
            var s = $"Market R {MarketValue:N2}";
            var extra = new List<string>();
            if (!string.IsNullOrWhiteSpace(ValuerName)) extra.Add(ValuerName!);
            if (ValuationDate.HasValue) extra.Add(ValuationDate.Value.ToString("dd MMM yyyy"));
            if (extra.Count > 0) s += $" ({string.Join(", ", extra)})";
            return s;
        }
    }

    public string DispositionSummary
    {
        get
        {
            if (IsSoldByEstate)
                return "Sold by estate"
                    + (SalePrice.HasValue ? $" for R {SalePrice:N2}" : "")
                    + (string.IsNullOrWhiteSpace(PurchaserName) ? "" : $" to {PurchaserName}");
            if (!string.IsNullOrWhiteSpace(AwardedToNames))
                return (IsSpecialBequest ? "Special bequest to " : "Awarded to ") + AwardedToNames;
            return "Not yet awarded";
        }
    }
}

public class FinancialClaimDto
{
    public bool IsCreditLifeInsurance { get; set; }
    public Guid ClaimId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string InstitutionAddress { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? ClaimType { get; set; }
    public bool IsInFavourOfEstate { get; set; } = true;
    public decimal AccruedInterest { get; set; }
    public DateTime? ValuationDate { get; set; }
    public string? AwardedToBeneficiaryIds { get; set; }
    public string AwardedToNames { get; set; } = string.Empty;
    public bool IsSpecialBequest { get; set; }
    public bool IsCollectedAsCash { get; set; }
    public decimal CollectedAmount { get; set; }
    public DateTime? CollectedDate { get; set; }
    public bool IsEstateditLifeInsurance { get; set; }
    public string? LinkedLiabilityNote { get; set; }

    public decimal TotalAtDeath => Value + AccruedInterest;

    public string DispositionSummary =>
        !IsInFavourOfEstate ? "Claim against the estate – counts as a liability"
        : IsCollectedAsCash ? $"Collected R {CollectedAmount:N2}"
        : !string.IsNullOrWhiteSpace(AwardedToNames)
            ? (IsSpecialBequest ? "Special bequest to " : "Awarded to ") + AwardedToNames
        : "Residue";
}

public class DiaryEntryDto
{
    public Guid DiaryId { get; set; }
    public DateTime DueDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string NotifyBy { get; set; } = string.Empty;
}

public class ProgressStageDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScannedDocumentDto
{
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}