namespace EstateFlow.Core.Models;

public class ImmovableProperty
{
    public Guid AssetId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? TitleDeedNumber { get; set; }
    // Municipal value is used for the Inventory; MarketValue for the L&D account.
    public decimal MunicipalValue { get; set; }

    // Conventional | Sectional Title
    public string? PropertyType { get; set; }
    public string? ErfNumber { get; set; }
    public string? Township { get; set; }
    public string? Division { get; set; }
    public string? Province { get; set; }
    public decimal? ExtentSquareMetres { get; set; }
    public string? StreetAddress { get; set; }

    // Sectional title
    public string? SchemeName { get; set; }
    public string? SectionNumber { get; set; }
    public string? ExclusiveUseAreas { get; set; }

    // Ownership share – only when the deceased held less than 100% (blank if married in community)
    public int? ShareNumerator { get; set; }
    public int? ShareDenominator { get; set; }

    public decimal? MarketValue { get; set; }
    public string? ValuerName { get; set; }
    public DateTime? ValuationDate { get; set; }

    // Comma-separated BeneficiaryIds the property is awarded to
    public string? AwardedToBeneficiaryIds { get; set; }
    public bool IsSpecialBequest { get; set; }

    public bool IsSoldByEstate { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? PurchaserName { get; set; }
}
