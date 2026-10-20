namespace EstateFlow.Core.Dtos;

public class CreateMovableAssetRequest
{
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
}

public class CreateImmovablePropertyRequest
{
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
    public bool IsSpecialBequest { get; set; }
    public bool IsSoldByEstate { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? PurchaserName { get; set; }
}

public class CreateFinancialClaimRequest
{
    public string InstitutionName { get; set; } = string.Empty;
    public string InstitutionAddress { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public decimal Value { get; set; }

    public string? ClaimType { get; set; }
    public bool IsInFavourOfEstate { get; set; } = true;
    public decimal AccruedInterest { get; set; }
    public DateTime? ValuationDate { get; set; }
    public string? AwardedToBeneficiaryIds { get; set; }
    public bool IsSpecialBequest { get; set; }
    public bool IsCollectedAsCash { get; set; }
    public decimal CollectedAmount { get; set; }
    public DateTime? CollectedDate { get; set; }
    public bool IsCreditLifeInsurance { get; set; }
    public string? LinkedLiabilityNote { get; set; }
}