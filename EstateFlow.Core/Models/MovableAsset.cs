namespace EstateFlow.Core.Models;

public class MovableAsset
{
    public Guid AssetId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string Description { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public decimal Value { get; set; }

    public bool IsVehicle { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? VinNumber { get; set; }
    public string? Make { get; set; }
    // Movable | Registered Share
    public string? AssetCategory { get; set; }
    public string? ValuerName { get; set; }
    public DateTime? ValuationDate { get; set; }

    // Comma-separated BeneficiaryIds the asset is awarded to
    public string? AwardedToBeneficiaryIds { get; set; }
    public bool IsSpecialBequest { get; set; }

    public bool IsSoldOrReducedToCash { get; set; }
    public decimal? SaleProceeds { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? BuyerName { get; set; }

    // Registered shares
    public string? ShareCompany { get; set; }
    public int? NumberOfShares { get; set; }
    public string? ShareClass { get; set; }
    public decimal? PriceCentsPerShare { get; set; }
    public decimal CumulativeDividend { get; set; }

    public bool RequiresTrafficDeptLetter() => IsVehicle;
}