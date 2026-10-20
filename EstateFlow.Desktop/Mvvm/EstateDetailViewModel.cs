using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Desktop.Services;
using System.Collections.ObjectModel;

namespace EstateFlow.Desktop.Mvvm;

public partial class EstateDetailViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigation;
    private Guid _estateId;

    private EstateDto? _estate;
    public EstateDto? Estate
    {
        get => _estate;
        set
        {
            if (SetProperty(ref _estate, value))
            {
                OnPropertyChanged(nameof(CurrentProgressTitle));
                OnPropertyChanged(nameof(CurrentProgressDate));
                OnPropertyChanged(nameof(CurrentProgressStatus));
                OnPropertyChanged(nameof(CurrentProgressSummary)); RebuildHeirOptions();
                OnPropertyChanged(nameof(PropertyShareAllowed));
                RefreshLedger();

            }
        }
    }

    private ProgressStageDto? CurrentProgressItem =>
        Estate?.ProgressStages?
            .OrderByDescending(x => x.TargetDate ?? DateTime.MinValue)
            .FirstOrDefault();

    public string CurrentProgressTitle =>
        CurrentProgressItem?.StageName
        ?? "No progress update recorded";

    public string CurrentProgressDate =>
        CurrentProgressItem?.TargetDate is DateTime date
            ? date.ToString("dd MMM yyyy")
            : "—";

    public string CurrentProgressStatus =>
        CurrentProgressItem?.Status switch
        {
            "InProgress" => "In Progress",
            "Complete" => "Complete",
            "Pending" => "Pending",
            _ => CurrentProgressItem?.Status ?? "Not Started"
        };

    public string CurrentProgressSummary =>
        CurrentProgressItem is null
            ? "Add the first progress update from the Progress tab."
            : $"{CurrentProgressDate} • {CurrentProgressStatus}";

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // ─────────────────────────────────────────────
    // Quick-add: Executor
    // ─────────────────────────────────────────────

    private string _newExecutorFullName = string.Empty;
    public string NewExecutorFullName
    {
        get => _newExecutorFullName;
        set => SetProperty(ref _newExecutorFullName, value);
    }

    private string _newExecutorIdNumber = string.Empty;
    public string NewExecutorIdNumber
    {
        get => _newExecutorIdNumber;
        set => SetProperty(ref _newExecutorIdNumber, value);
    }


    private string _newExecutorEmail = string.Empty;
    public string NewExecutorEmail
    {
        get => _newExecutorEmail;
        set => SetProperty(ref _newExecutorEmail, value);
    }

    private string _newExecutorRole = "Nominated Executor";
    public string NewExecutorRole
    {
        get => _newExecutorRole;
        set => SetProperty(ref _newExecutorRole, value);
    }

    // ─────────────────────────────────────────────
    // Quick-add: Beneficiary
    // ─────────────────────────────────────────────

    private string _newBeneficiaryFirstName = string.Empty;
    public string NewBeneficiaryFirstName
    {
        get => _newBeneficiaryFirstName;
        set { if (SetProperty(ref _newBeneficiaryFirstName, value)) OnPropertyChanged(nameof(NewBeneficiaryFullName)); }
    }

    private string _newBeneficiaryMiddleName = string.Empty;
    public string NewBeneficiaryMiddleName
    {
        get => _newBeneficiaryMiddleName;
        set { if (SetProperty(ref _newBeneficiaryMiddleName, value)) OnPropertyChanged(nameof(NewBeneficiaryFullName)); }
    }

    private string _newBeneficiarySurname = string.Empty;
    public string NewBeneficiarySurname
    {
        get => _newBeneficiarySurname;
        set { if (SetProperty(ref _newBeneficiarySurname, value)) OnPropertyChanged(nameof(NewBeneficiaryFullName)); }
    }

    public string NewBeneficiaryFullName =>
        string.Join(" ",
            new[] { NewBeneficiaryFirstName, NewBeneficiaryMiddleName, NewBeneficiarySurname }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim()));

    private string _newBeneficiaryShareType = "Residue";
    public string NewBeneficiaryShareType { get => _newBeneficiaryShareType; set => SetProperty(ref _newBeneficiaryShareType, value); }

    private string _newBeneficiarySharePercentage = string.Empty;
    public string NewBeneficiarySharePercentage { get => _newBeneficiarySharePercentage; set => SetProperty(ref _newBeneficiarySharePercentage, value); }

    private string _newBeneficiaryShareFraction = string.Empty;
    public string NewBeneficiaryShareFraction { get => _newBeneficiaryShareFraction; set => SetProperty(ref _newBeneficiaryShareFraction, value); }

    private string _newBeneficiaryWillClause = string.Empty;
    public string NewBeneficiaryWillClause { get => _newBeneficiaryWillClause; set => SetProperty(ref _newBeneficiaryWillClause, value); }

    private string _newBeneficiaryBequestDescription = string.Empty;
    public string NewBeneficiaryBequestDescription { get => _newBeneficiaryBequestDescription; set => SetProperty(ref _newBeneficiaryBequestDescription, value); }

    private string _newBeneficiaryCashLegacyAmount = "0";
    public string NewBeneficiaryCashLegacyAmount { get => _newBeneficiaryCashLegacyAmount; set => SetProperty(ref _newBeneficiaryCashLegacyAmount, value); }

    private bool _newBeneficiaryIsSurvivingSpouse;
    public bool NewBeneficiaryIsSurvivingSpouse { get => _newBeneficiaryIsSurvivingSpouse; set => SetProperty(ref _newBeneficiaryIsSurvivingSpouse, value); }

    private bool _newBeneficiaryIsMinor;
    public bool NewBeneficiaryIsMinor { get => _newBeneficiaryIsMinor; set => SetProperty(ref _newBeneficiaryIsMinor, value); }

    private string _newBeneficiaryGuardianOrTrustee = string.Empty;
    public string NewBeneficiaryGuardianOrTrustee { get => _newBeneficiaryGuardianOrTrustee; set => SetProperty(ref _newBeneficiaryGuardianOrTrustee, value); }

    private bool _newBeneficiaryIsPredeceased;
    public bool NewBeneficiaryIsPredeceased { get => _newBeneficiaryIsPredeceased; set => SetProperty(ref _newBeneficiaryIsPredeceased, value); }
    private string _newBeneficiaryRelationshipDescription = string.Empty;
    public string NewBeneficiaryRelationshipDescription { get => _newBeneficiaryRelationshipDescription; set => SetProperty(ref _newBeneficiaryRelationshipDescription, value); }

    private DateTime? _newBeneficiaryDateOfBirth;
    public DateTime? NewBeneficiaryDateOfBirth { get => _newBeneficiaryDateOfBirth; set => SetProperty(ref _newBeneficiaryDateOfBirth, value); }

    private string _newBeneficiaryGender = string.Empty;
    public string NewBeneficiaryGender { get => _newBeneficiaryGender; set => SetProperty(ref _newBeneficiaryGender, value); }

    private string _newBeneficiaryMaritalStatus = string.Empty;
    public string NewBeneficiaryMaritalStatus { get => _newBeneficiaryMaritalStatus; set => SetProperty(ref _newBeneficiaryMaritalStatus, value); }

    private string _newBeneficiaryResidueNumerator = string.Empty;
    public string NewBeneficiaryResidueNumerator { get => _newBeneficiaryResidueNumerator; set => SetProperty(ref _newBeneficiaryResidueNumerator, value); }

    private string _newBeneficiaryResidueDenominator = string.Empty;
    public string NewBeneficiaryResidueDenominator { get => _newBeneficiaryResidueDenominator; set => SetProperty(ref _newBeneficiaryResidueDenominator, value); }

    private string _newBeneficiaryAdvancesPaid = "0";
    public string NewBeneficiaryAdvancesPaid { get => _newBeneficiaryAdvancesPaid; set => SetProperty(ref _newBeneficiaryAdvancesPaid, value); }

    private bool _newBeneficiaryIsUltimateBeneficiary;
    public bool NewBeneficiaryIsUltimateBeneficiary { get => _newBeneficiaryIsUltimateBeneficiary; set => SetProperty(ref _newBeneficiaryIsUltimateBeneficiary, value); }

    private string _newBeneficiaryBareDominiumFraction = string.Empty;
    public string NewBeneficiaryBareDominiumFraction { get => _newBeneficiaryBareDominiumFraction; set => SetProperty(ref _newBeneficiaryBareDominiumFraction, value); }

    private string _newBeneficiaryIdNumber = string.Empty;
    public string NewBeneficiaryIdNumber
    {
        get => _newBeneficiaryIdNumber;
        set => SetProperty(ref _newBeneficiaryIdNumber, value);
    }


    private string _newBeneficiaryEmail = string.Empty;
    public string NewBeneficiaryEmail
    {
        get => _newBeneficiaryEmail;
        set => SetProperty(ref _newBeneficiaryEmail, value);
    }

    private string _newBeneficiaryRelationship = string.Empty;
    public string NewBeneficiaryRelationship
    {
        get => _newBeneficiaryRelationship;
        set => SetProperty(ref _newBeneficiaryRelationship, value);
    }

    private string _newBeneficiaryHeirType = "Heir";
    public string NewBeneficiaryHeirType
    {
        get => _newBeneficiaryHeirType;
        set => SetProperty(ref _newBeneficiaryHeirType, value);
    }

    // ─────────────────────────────────────────────
    // Quick-add: Movable Asset
    // ─────────────────────────────────────────────

    private string _newAssetDescription = string.Empty;
    public string NewAssetDescription
    {
        get => _newAssetDescription;
        set => SetProperty(ref _newAssetDescription, value);
    }

    private string _newAssetType = string.Empty;
    public string NewAssetType
    {
        get => _newAssetType;
        set => SetProperty(ref _newAssetType, value);
    }

    private string _newAssetValue = "0";
    public string NewAssetValue
    {
        get => _newAssetValue;
        set => SetProperty(ref _newAssetValue, value);
    }

    private bool _newAssetIsVehicle;
    public bool NewAssetIsVehicle
    {
        get => _newAssetIsVehicle;
        set => SetProperty(ref _newAssetIsVehicle, value);
    }

    private string _newAssetRegistrationNumber = string.Empty;
    public string NewAssetRegistrationNumber
    {
        get => _newAssetRegistrationNumber;
        set => SetProperty(ref _newAssetRegistrationNumber, value);
    }

    private string _newAssetVinNumber = string.Empty;
    public string NewAssetVinNumber
    {
        get => _newAssetVinNumber;
        set => SetProperty(ref _newAssetVinNumber, value);
    }

    private string _newAssetMake = string.Empty;
    public string NewAssetMake
    {
        get => _newAssetMake;
        set => SetProperty(ref _newAssetMake, value);
    }
    public ObservableCollection<HeirOption> MovableHeirOptions { get; } = new();

    public ObservableCollection<HeirOption> ImmovableHeirOptions { get; } = new();
    public ObservableCollection<HeirOption> ClaimHeirOptions { get; } = new();

    private void RebuildHeirOptions()
    {
        MovableHeirOptions.Clear();
        ImmovableHeirOptions.Clear();
        ClaimHeirOptions.Clear();

        foreach (var heir in Estate?.Beneficiaries ?? new List<BeneficiaryDto>())
        {
            MovableHeirOptions.Add(new HeirOption { BeneficiaryId = heir.BeneficiaryId, FullName = heir.FullName });
            ImmovableHeirOptions.Add(new HeirOption { BeneficiaryId = heir.BeneficiaryId, FullName = heir.FullName });
            ClaimHeirOptions.Add(new HeirOption { BeneficiaryId = heir.BeneficiaryId, FullName = heir.FullName });
        }
    }

    // Shared award rules. Returns an error message, or null when valid.
    private string? ValidateAward(IEnumerable<HeirOption> options, bool isSpecialBequest, bool isSoldOrCollected)
    {
        var selected = options.Where(h => h.IsSelected).ToList();
        if (selected.Count == 0) return null;

        if (isSoldOrCollected && !isSpecialBequest)
            return "An item cannot be both awarded to heirs and sold / collected as cash unless it is a special bequest.";

        if (!isSpecialBequest)
        {
            var noResidue =
                (Estate?.Beneficiaries ?? new List<BeneficiaryDto>())
                    .Where(b => selected.Any(s => s.BeneficiaryId == b.BeneficiaryId) &&
                                (!b.ResidueNumerator.HasValue || !b.ResidueDenominator.HasValue))
                    .Select(b => b.FullName)
                    .ToList();

            if (noResidue.Count > 0)
                return $"{string.Join(", ", noResidue)} has no residue portion. " +
                       "Tick 'special bequest' or give the heir a residue portion first.";
        }

        return null;
    }

    private static string JoinSelected(IEnumerable<HeirOption> options) =>
        string.Join(",", options.Where(h => h.IsSelected).Select(h => h.BeneficiaryId));

    private string _newAssetCategory = "Movable";
    public string NewAssetCategory
    {
        get => _newAssetCategory;
        set
        {
            if (SetProperty(ref _newAssetCategory, value))
                OnPropertyChanged(nameof(NewAssetIsRegisteredShare));
        }
    }

    public bool NewAssetIsRegisteredShare => NewAssetCategory == "Registered Share";

    private string _newAssetValuerName = string.Empty;
    public string NewAssetValuerName { get => _newAssetValuerName; set => SetProperty(ref _newAssetValuerName, value); }

    private DateTime? _newAssetValuationDate;
    public DateTime? NewAssetValuationDate { get => _newAssetValuationDate; set => SetProperty(ref _newAssetValuationDate, value); }

    private bool _newAssetIsSpecialBequest;
    public bool NewAssetIsSpecialBequest { get => _newAssetIsSpecialBequest; set => SetProperty(ref _newAssetIsSpecialBequest, value); }

    private bool _newAssetIsSoldOrReducedToCash;
    public bool NewAssetIsSoldOrReducedToCash { get => _newAssetIsSoldOrReducedToCash; set => SetProperty(ref _newAssetIsSoldOrReducedToCash, value); }

    private string _newAssetSaleProceeds = string.Empty;
    public string NewAssetSaleProceeds { get => _newAssetSaleProceeds; set => SetProperty(ref _newAssetSaleProceeds, value); }

    private DateTime? _newAssetSaleDate;
    public DateTime? NewAssetSaleDate { get => _newAssetSaleDate; set => SetProperty(ref _newAssetSaleDate, value); }

    private string _newAssetBuyerName = string.Empty;
    public string NewAssetBuyerName { get => _newAssetBuyerName; set => SetProperty(ref _newAssetBuyerName, value); }

    private string _newAssetShareCompany = string.Empty;
    public string NewAssetShareCompany { get => _newAssetShareCompany; set => SetProperty(ref _newAssetShareCompany, value); }

    private string _newAssetNumberOfShares = string.Empty;
    public string NewAssetNumberOfShares { get => _newAssetNumberOfShares; set => SetProperty(ref _newAssetNumberOfShares, value); }

    private string _newAssetShareClass = string.Empty;
    public string NewAssetShareClass { get => _newAssetShareClass; set => SetProperty(ref _newAssetShareClass, value); }

    private string _newAssetPriceCents = string.Empty;
    public string NewAssetPriceCents { get => _newAssetPriceCents; set => SetProperty(ref _newAssetPriceCents, value); }

    private string _newAssetCumulativeDividend = "0";
    public string NewAssetCumulativeDividend { get => _newAssetCumulativeDividend; set => SetProperty(ref _newAssetCumulativeDividend, value); }
    // ─────────────────────────────────────────────
    // Quick-add: Immovable Property
    // ─────────────────────────────────────────────

    private string _newPropertyDescription = string.Empty;
    public string NewPropertyDescription { get => _newPropertyDescription; set => SetProperty(ref _newPropertyDescription, value); }

    private string _newPropertyTitleDeedNumber = string.Empty;
    public string NewPropertyTitleDeedNumber { get => _newPropertyTitleDeedNumber; set => SetProperty(ref _newPropertyTitleDeedNumber, value); }

    private string _newPropertyMunicipalValue = "0";
    public string NewPropertyMunicipalValue { get => _newPropertyMunicipalValue; set => SetProperty(ref _newPropertyMunicipalValue, value); }

    private string _newPropertyType = "Conventional";
    public string NewPropertyType
    {
        get => _newPropertyType;
        set
        {
            if (SetProperty(ref _newPropertyType, value))
            {
                OnPropertyChanged(nameof(NewPropertyIsSectional));
                OnPropertyChanged(nameof(NewPropertyIsConventional));
            }
        }
    }
    public bool NewPropertyIsSectional => NewPropertyType == "Sectional Title";
    public bool NewPropertyIsConventional => !NewPropertyIsSectional;

    private string _newPropertyErfNumber = string.Empty;
    public string NewPropertyErfNumber { get => _newPropertyErfNumber; set => SetProperty(ref _newPropertyErfNumber, value); }

    private string _newPropertyTownship = string.Empty;
    public string NewPropertyTownship { get => _newPropertyTownship; set => SetProperty(ref _newPropertyTownship, value); }

    private string _newPropertyDivision = string.Empty;
    public string NewPropertyDivision { get => _newPropertyDivision; set => SetProperty(ref _newPropertyDivision, value); }

    private string _newPropertyProvince = string.Empty;
    public string NewPropertyProvince { get => _newPropertyProvince; set => SetProperty(ref _newPropertyProvince, value); }

    private string _newPropertyExtent = string.Empty;
    public string NewPropertyExtent { get => _newPropertyExtent; set => SetProperty(ref _newPropertyExtent, value); }

    private string _newPropertyStreetAddress = string.Empty;
    public string NewPropertyStreetAddress { get => _newPropertyStreetAddress; set => SetProperty(ref _newPropertyStreetAddress, value); }

    private string _newPropertySchemeName = string.Empty;
    public string NewPropertySchemeName { get => _newPropertySchemeName; set => SetProperty(ref _newPropertySchemeName, value); }

    private string _newPropertySectionNumber = string.Empty;
    public string NewPropertySectionNumber { get => _newPropertySectionNumber; set => SetProperty(ref _newPropertySectionNumber, value); }

    private string _newPropertyExclusiveUseAreas = string.Empty;
    public string NewPropertyExclusiveUseAreas { get => _newPropertyExclusiveUseAreas; set => SetProperty(ref _newPropertyExclusiveUseAreas, value); }

    // Ownership share is only captured when the deceased held less than 100%
    // and was NOT married in community of property.
    public bool PropertyShareAllowed =>
        Estate?.Deceased?.MatrimonialRegime != "In community of property";

    private string _newPropertyShareNumerator = string.Empty;
    public string NewPropertyShareNumerator { get => _newPropertyShareNumerator; set => SetProperty(ref _newPropertyShareNumerator, value); }

    private string _newPropertyShareDenominator = string.Empty;
    public string NewPropertyShareDenominator { get => _newPropertyShareDenominator; set => SetProperty(ref _newPropertyShareDenominator, value); }

    private string _newPropertyMarketValue = string.Empty;
    public string NewPropertyMarketValue { get => _newPropertyMarketValue; set => SetProperty(ref _newPropertyMarketValue, value); }

    private string _newPropertyValuerName = string.Empty;
    public string NewPropertyValuerName { get => _newPropertyValuerName; set => SetProperty(ref _newPropertyValuerName, value); }

    private DateTime? _newPropertyValuationDate;
    public DateTime? NewPropertyValuationDate { get => _newPropertyValuationDate; set => SetProperty(ref _newPropertyValuationDate, value); }

    private bool _newPropertyIsSpecialBequest;
    public bool NewPropertyIsSpecialBequest { get => _newPropertyIsSpecialBequest; set => SetProperty(ref _newPropertyIsSpecialBequest, value); }

    private bool _newPropertyIsSold;
    public bool NewPropertyIsSold { get => _newPropertyIsSold; set => SetProperty(ref _newPropertyIsSold, value); }

    private string _newPropertySalePrice = "0";
    public string NewPropertySalePrice { get => _newPropertySalePrice; set => SetProperty(ref _newPropertySalePrice, value); }

    private DateTime? _newPropertySaleDate;
    public DateTime? NewPropertySaleDate { get => _newPropertySaleDate; set => SetProperty(ref _newPropertySaleDate, value); }

    private string _newPropertyPurchaserName = string.Empty;
    public string NewPropertyPurchaserName { get => _newPropertyPurchaserName; set => SetProperty(ref _newPropertyPurchaserName, value); }

    // ─────────────────────────────────────────────
    // Quick-add: Financial Claim
    // ─────────────────────────────────────────────

    private string _newClaimInstitutionName = string.Empty;
    public string NewClaimInstitutionName
    {
        get => _newClaimInstitutionName;
        set => SetProperty(ref _newClaimInstitutionName, value);
    }

    private string _newClaimInstitutionAddress = string.Empty;
    public string NewClaimInstitutionAddress
    {
        get => _newClaimInstitutionAddress;
        set => SetProperty(ref _newClaimInstitutionAddress, value);
    }

    private string _newClaimAccountReference = string.Empty;
    public string NewClaimAccountReference
    {
        get => _newClaimAccountReference;
        set => SetProperty(ref _newClaimAccountReference, value);
    }

    private string _newClaimValue = "0";
    public string NewClaimValue
    {
        get => _newClaimValue;
        set => SetProperty(ref _newClaimValue, value);
    }

    private string _newClaimType = string.Empty;
    public string NewClaimType { get => _newClaimType; set => SetProperty(ref _newClaimType, value); }

    private bool _newClaimInFavour = true;
    public bool NewClaimInFavour { get => _newClaimInFavour; set => SetProperty(ref _newClaimInFavour, value); }

    private string _newClaimAccruedInterest = "0";
    public string NewClaimAccruedInterest { get => _newClaimAccruedInterest; set => SetProperty(ref _newClaimAccruedInterest, value); }

    private DateTime? _newClaimValuationDate;
    public DateTime? NewClaimValuationDate { get => _newClaimValuationDate; set => SetProperty(ref _newClaimValuationDate, value); }

    private bool _newClaimIsSpecialBequest;
    public bool NewClaimIsSpecialBequest { get => _newClaimIsSpecialBequest; set => SetProperty(ref _newClaimIsSpecialBequest, value); }

    private bool _newClaimIsCollected;
    public bool NewClaimIsCollected { get => _newClaimIsCollected; set => SetProperty(ref _newClaimIsCollected, value); }

    private string _newClaimCollectedAmount = "0";
    public string NewClaimCollectedAmount { get => _newClaimCollectedAmount; set => SetProperty(ref _newClaimCollectedAmount, value); }

    private DateTime? _newClaimCollectedDate;
    public DateTime? NewClaimCollectedDate { get => _newClaimCollectedDate; set => SetProperty(ref _newClaimCollectedDate, value); }

    private bool _newClaimIsCreditLife;
    public bool NewClaimIsCreditLife { get => _newClaimIsCreditLife; set => SetProperty(ref _newClaimIsCreditLife, value); }

    private string _newClaimLinkedLiabilityNote = string.Empty;
    public string NewClaimLinkedLiabilityNote { get => _newClaimLinkedLiabilityNote; set => SetProperty(ref _newClaimLinkedLiabilityNote, value); }

    // ─────────────────────────────────────────────
    // Quick-add: Diary Entry
    // ─────────────────────────────────────────────

    private string _newDiaryNote = string.Empty;
    public string NewDiaryNote
    {
        get => _newDiaryNote;
        set => SetProperty(ref _newDiaryNote, value);
    }

    private DateTime? _newDiaryDueDate = DateTime.Today.AddDays(7);
    public DateTime? NewDiaryDueDate
    {
        get => _newDiaryDueDate;
        set => SetProperty(ref _newDiaryDueDate, value);
    }

    private string _newDiaryNotifyBy = "Email";
    public string NewDiaryNotifyBy
    {
        get => _newDiaryNotifyBy;
        set => SetProperty(ref _newDiaryNotifyBy, value);
    }

    // ─────────────────────────────────────────────
    // Quick-add: Progress Stage
    // ─────────────────────────────────────────────

    private string _newStageName = string.Empty;
    public string NewStageName
    {
        get => _newStageName;
        set => SetProperty(ref _newStageName, value);
    }

    private DateTime? _newStageTargetDate = DateTime.Today;
    public DateTime? NewStageTargetDate
    {
        get => _newStageTargetDate;
        set => SetProperty(ref _newStageTargetDate, value);
    }

    private string _newStageStatus = "InProgress";
    public string NewStageStatus
    {
        get => _newStageStatus;
        set => SetProperty(ref _newStageStatus, value);
    }

    private bool _isEditingProgress;
    public bool IsEditingProgress
    {
        get => _isEditingProgress;
        set => SetProperty(ref _isEditingProgress, value);
    }

    private Guid _editingProgressStageId;
    private string _editingProgressTitle = string.Empty;
    public string EditingProgressTitle
    {
        get => _editingProgressTitle;
        set => SetProperty(ref _editingProgressTitle, value);
    }

    private DateTime? _editingProgressDate = DateTime.Today;
    public DateTime? EditingProgressDate
    {
        get => _editingProgressDate;
        set => SetProperty(ref _editingProgressDate, value);
    }

    private string _editingProgressStatus = "InProgress";
    public string EditingProgressStatus
    {
        get => _editingProgressStatus;
        set => SetProperty(ref _editingProgressStatus, value);
    }


    // ─────────────────────────────────────────────
    // Part 3: Liabilities + L&D draft
    // ─────────────────────────────────────────────

    private string _newLiabilityCreditorName = string.Empty;
    public string NewLiabilityCreditorName
    {
        get => _newLiabilityCreditorName;
        set => SetProperty(ref _newLiabilityCreditorName, value);
    }

    private string _newLiabilityDescription = string.Empty;
    public string NewLiabilityDescription
    {
        get => _newLiabilityDescription;
        set => SetProperty(ref _newLiabilityDescription, value);
    }

    private string _newLiabilityAmount = "0";
    public string NewLiabilityAmount
    {
        get => _newLiabilityAmount;
        set => SetProperty(ref _newLiabilityAmount, value);
    }

    private string _newLiabilityCategory = string.Empty;
    public string NewLiabilityCategory { get => _newLiabilityCategory; set => SetProperty(ref _newLiabilityCategory, value); }

    private string _newLiabilityAccountReference = string.Empty;
    public string NewLiabilityAccountReference { get => _newLiabilityAccountReference; set => SetProperty(ref _newLiabilityAccountReference, value); }

    private string _newLiabilityInterestAfterDeath = "0";
    public string NewLiabilityInterestAfterDeath { get => _newLiabilityInterestAfterDeath; set => SetProperty(ref _newLiabilityInterestAfterDeath, value); }

    private string _newLiabilityStatus = "Admitted";
    public string NewLiabilityStatus { get => _newLiabilityStatus; set => SetProperty(ref _newLiabilityStatus, value); }

    private DateTime? _newLiabilityDateLodged;
    public DateTime? NewLiabilityDateLodged { get => _newLiabilityDateLodged; set => SetProperty(ref _newLiabilityDateLodged, value); }

    private string _newLiabilitySecuredAgainst = string.Empty;
    public string NewLiabilitySecuredAgainst { get => _newLiabilitySecuredAgainst; set => SetProperty(ref _newLiabilitySecuredAgainst, value); }

    private bool _newLiabilityTakenOverByHeir;
    public bool NewLiabilityTakenOverByHeir { get => _newLiabilityTakenOverByHeir; set => SetProperty(ref _newLiabilityTakenOverByHeir, value); }

    private bool _newLiabilityIsPaid;
    public bool NewLiabilityIsPaid { get => _newLiabilityIsPaid; set => SetProperty(ref _newLiabilityIsPaid, value); }

    private string _newLiabilityAmountPaid = "0";
    public string NewLiabilityAmountPaid { get => _newLiabilityAmountPaid; set => SetProperty(ref _newLiabilityAmountPaid, value); }

    private DateTime? _newLiabilityDatePaid;
    public DateTime? NewLiabilityDatePaid { get => _newLiabilityDatePaid; set => SetProperty(ref _newLiabilityDatePaid, value); }

    private bool _newLiabilityCreditLife;
    public bool NewLiabilityCreditLife { get => _newLiabilityCreditLife; set => SetProperty(ref _newLiabilityCreditLife, value); }

    private LiquidationDistributionDraftDto? _liquidationDistributionDraft;
    public LiquidationDistributionDraftDto? LiquidationDistributionDraft
    {
        get => _liquidationDistributionDraft;
        set => SetProperty(ref _liquidationDistributionDraft, value);
    }    // ─────────────────────────────────────────────
    // Executor Tab – Detail fields
    // ─────────────────────────────────────────────

    private Guid _executorDetailId;

    private string _executorDetailFullName = string.Empty;
    public string ExecutorDetailFullName
    {
        get => _executorDetailFullName;
        set => SetProperty(ref _executorDetailFullName, value);
    }

    private string _executorDetailIdNumber = string.Empty;
    public string ExecutorDetailIdNumber
    {
        get => _executorDetailIdNumber;
        set => SetProperty(ref _executorDetailIdNumber, value);
    }

    private string _executorDetailEmail = string.Empty;
    public string ExecutorDetailEmail
    {
        get => _executorDetailEmail;
        set => SetProperty(ref _executorDetailEmail, value);
    }

    private string _executorDetailPhone = string.Empty;
    public string ExecutorDetailPhone
    {
        get => _executorDetailPhone;
        set => SetProperty(ref _executorDetailPhone, value);
    }

    private string _executorDetailAddress = string.Empty;
    public string ExecutorDetailAddress
    {
        get => _executorDetailAddress;
        set => SetProperty(ref _executorDetailAddress, value);
    }

    private string _executorDetailRole = "Nominated Executor";
    public string ExecutorDetailRole
    {
        get => _executorDetailRole;
        set => SetProperty(ref _executorDetailRole, value);
    }

    private string _executorDetailMastersOffice = string.Empty;
    public string ExecutorDetailMastersOffice
    {
        get => _executorDetailMastersOffice;
        set => SetProperty(ref _executorDetailMastersOffice, value);
    }

    private string _executorDetailLettersRef = string.Empty;
    public string ExecutorDetailLettersRef
    {
        get => _executorDetailLettersRef;
        set => SetProperty(ref _executorDetailLettersRef, value);
    }

    private DateTime? _executorDetailLettersDate;
    public DateTime? ExecutorDetailLettersDate
    {
        get => _executorDetailLettersDate;
        set => SetProperty(ref _executorDetailLettersDate, value);
    }

    private string? _lettersDocumentName;
    public string? LettersDocumentName
    {
        get => _lettersDocumentName;
        set => SetProperty(ref _lettersDocumentName, value);
    }

    private bool _hasLettersDocument;
    public bool HasLettersDocument
    {
        get => _hasLettersDocument;
        set => SetProperty(ref _hasLettersDocument, value);
    }

    // ─────────────────────────────────────────────
    // Executor Tab – Section 29 Advertisement
    // ─────────────────────────────────────────────

    private string _section29GazetteNumber = string.Empty;
    public string Section29GazetteNumber
    {
        get => _section29GazetteNumber;
        set => SetProperty(ref _section29GazetteNumber, value);
    }

    private DateTime? _section29GazetteDate;
    public DateTime? Section29GazetteDate
    {
        get => _section29GazetteDate;
        set => SetProperty(ref _section29GazetteDate, value);
    }

    private string _section29NewspaperName = string.Empty;
    public string Section29NewspaperName
    {
        get => _section29NewspaperName;
        set => SetProperty(ref _section29NewspaperName, value);
    }

    private DateTime? _section29NewspaperDate;
    public DateTime? Section29NewspaperDate
    {
        get => _section29NewspaperDate;
        set => SetProperty(ref _section29NewspaperDate, value);
    }

    private DateTime? _section29ClaimDeadline;
    public DateTime? Section29ClaimDeadline
    {
        get => _section29ClaimDeadline;
        set => SetProperty(ref _section29ClaimDeadline, value);
    }

    private bool _section29ObjectionsReceived;
    public bool Section29ObjectionsReceived
    {
        get => _section29ObjectionsReceived;
        set => SetProperty(ref _section29ObjectionsReceived, value);
    }

    private string _section29Notes = string.Empty;
    public string Section29Notes
    {
        get => _section29Notes;
        set => SetProperty(ref _section29Notes, value);
    }

    private bool _section29Complete;
    public bool Section29Complete
    {
        get => _section29Complete;
        set => SetProperty(ref _section29Complete, value);
    }

    private bool _section29Overdue;
    public bool Section29Overdue
    {
        get => _section29Overdue;
        set => SetProperty(ref _section29Overdue, value);
    }

    // ─────────────────────────────────────────────
    // Executor Tab – Section 35 Advertisement
    // ─────────────────────────────────────────────

    private string _section35GazetteNumber = string.Empty;
    public string Section35GazetteNumber
    {
        get => _section35GazetteNumber;
        set => SetProperty(ref _section35GazetteNumber, value);
    }

    private DateTime? _section35GazetteDate;
    public DateTime? Section35GazetteDate
    {
        get => _section35GazetteDate;
        set => SetProperty(ref _section35GazetteDate, value);
    }

    private string _section35NewspaperName = string.Empty;
    public string Section35NewspaperName
    {
        get => _section35NewspaperName;
        set => SetProperty(ref _section35NewspaperName, value);
    }

    private DateTime? _section35NewspaperDate;
    public DateTime? Section35NewspaperDate
    {
        get => _section35NewspaperDate;
        set => SetProperty(ref _section35NewspaperDate, value);
    }

    private DateTime? _section35ClaimDeadline;
    public DateTime? Section35ClaimDeadline
    {
        get => _section35ClaimDeadline;
        set => SetProperty(ref _section35ClaimDeadline, value);
    }

    private bool _section35ObjectionsReceived;
    public bool Section35ObjectionsReceived
    {
        get => _section35ObjectionsReceived;
        set => SetProperty(ref _section35ObjectionsReceived, value);
    }

    private string _section35Notes = string.Empty;
    public string Section35Notes
    {
        get => _section35Notes;
        set => SetProperty(ref _section35Notes, value);
    }

    // ─────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────

    public ICommand BackCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteEstateCommand { get; }

    public ICommand AddExecutorCommand { get; }
    public ICommand DeleteExecutorCommand { get; }
    public ICommand SaveExecutorDetailCommand { get; }
    public ICommand SelectExecutorCommand { get; }
    public ICommand UploadLettersCommand { get; }
    public ICommand ViewLettersCommand { get; }
    public ICommand SaveSection29Command { get; }
    public ICommand SaveSection35Command { get; }

    public ICommand AddBeneficiaryCommand { get; }
    public ICommand DeleteBeneficiaryCommand { get; }

    public ICommand AddMovableAssetCommand { get; }
    public ICommand DeleteMovableAssetCommand { get; }
    public ICommand GenerateVehicleLetterCommand { get; }

    public ICommand AddImmovablePropertyCommand { get; }
    public ICommand DeleteImmovablePropertyCommand { get; }

    public ICommand AddFinancialClaimCommand { get; }
    public ICommand DeleteFinancialClaimCommand { get; }
    public ICommand GenerateInstitutionLetterCommand { get; }
    public ICommand DownloadLiquidationAccountCommand { get; }

    public ICommand AddLiabilityCommand { get; }
    public ICommand DeleteLiabilityCommand { get; }
    public ICommand RefreshLiquidationDistributionCommand { get; }

    public ICommand AddDiaryEntryCommand { get; }
    public ICommand OpenDiaryEntryCommand { get; }
    public ICommand ToggleDiaryCompleteCommand { get; }
    public ICommand DeleteDiaryEntryCommand { get; }

    public ICommand AddProgressStageCommand { get; }
    public ICommand StartStbbWorkflowCommand { get; }
    public ICommand EditProgressStageCommand { get; }
    public ICommand SaveProgressStageCommand { get; }
    public ICommand CancelProgressEditCommand { get; }
    public ICommand DeleteProgressStageCommand { get; }

    public EstateDetailViewModel(
        IApiClient apiClient,
        INavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;

        InitLedgerCommands();

        BackCommand = new RelayCommand(
            _ => _navigation.NavigateToDashboard());

        EditCommand = new RelayCommand(_ =>
        {
            if (Estate is not null)
                _navigation.NavigateToEditEstate(Estate.EstateId);
        });

        DeleteEstateCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteDeleteEstateAsync());

        AddExecutorCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddExecutorAsync());

        DeleteExecutorCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteExecutorAsync(parameter));
        SaveExecutorDetailCommand =
    new AsyncRelayCommand(
        async _ => await ExecuteSaveExecutorDetailAsync());

        SelectExecutorCommand = new RelayCommand(parameter =>
        {
            if (parameter is ExecutorDto executor)
                LoadExecutorDetail(executor);
        });

        UploadLettersCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteUploadLettersAsync());

        ViewLettersCommand =
            new RelayCommand(
                _ => ExecuteViewLetters());

        SaveSection29Command =
            new AsyncRelayCommand(
                async _ => await ExecuteSaveSection29Async());

        SaveSection35Command =
            new AsyncRelayCommand(
                async _ => await ExecuteSaveSection35Async());


        AddBeneficiaryCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddBeneficiaryAsync());

        DeleteBeneficiaryCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteBeneficiaryAsync(parameter));

        AddMovableAssetCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddMovableAssetAsync());

        DeleteMovableAssetCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteMovableAssetAsync(parameter));

        GenerateVehicleLetterCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteGenerateVehicleLetterAsync(parameter));

        AddImmovablePropertyCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddImmovablePropertyAsync());

        DeleteImmovablePropertyCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteImmovablePropertyAsync(parameter));

        AddFinancialClaimCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddFinancialClaimAsync());

        DeleteFinancialClaimCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteFinancialClaimAsync(parameter));

        GenerateInstitutionLetterCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteGenerateInstitutionLetterAsync(parameter));

        DownloadLiquidationAccountCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteDownloadLiquidationAccountAsync());

        AddLiabilityCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddLiabilityAsync());

        DeleteLiabilityCommand =
            new AsyncRelayCommand(
                async parameter => await ExecuteDeleteLiabilityAsync(parameter));

        RefreshLiquidationDistributionCommand =
            new AsyncRelayCommand(
                async _ => await LoadLiquidationDistributionAsync());

        AddDiaryEntryCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddDiaryEntryAsync());

        OpenDiaryEntryCommand =
            new RelayCommand(
                parameter => ExecuteOpenDiaryEntry(parameter));

        ToggleDiaryCompleteCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteToggleDiaryCompleteAsync(parameter));

        DeleteDiaryEntryCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteDiaryEntryAsync(parameter));

        StartStbbWorkflowCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteStartStbbWorkflowAsync());

        AddProgressStageCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteAddProgressStageAsync());

        EditProgressStageCommand =
            new RelayCommand(
                parameter => ExecuteBeginEditProgressStage(parameter));

        SaveProgressStageCommand =
            new AsyncRelayCommand(
                async _ => await ExecuteSaveProgressStageAsync());

        CancelProgressEditCommand =
            new RelayCommand(
                _ => CancelProgressEdit());

        DeleteProgressStageCommand =
            new AsyncRelayCommand(
                async parameter =>
                    await ExecuteDeleteProgressStageAsync(parameter));
    }

    // ─────────────────────────────────────────────
    // Load / Refresh Estate
    // ─────────────────────────────────────────────

    public async Task LoadAsync(Guid estateId)
    {
        _estateId = estateId;
        IsLoading = true;
        StatusMessage = "Loading estate...";


        try
        {
            Estate = await _apiClient.GetEstateAsync(estateId);

            if (Estate is not null)
            {
                await LoadLiquidationDistributionAsync();
                await LoadAuditAsync();
            }

            StatusMessage =
                Estate is null
                    ? "Estate not found."
                    : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not load estate: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─────────────────────────────────────────────
    // Delete Estate
    // ─────────────────────────────────────────────

    private async Task ExecuteDeleteEstateAsync()
    {
        if (_estateId == Guid.Empty || Estate is null)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to permanently delete estate {Estate.FileNumber}?\n\n" +
            "This will remove the estate and its associated records. " +
            "This action cannot be undone.",
            "Delete Estate",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Deleting estate...";

            await _apiClient.DeleteEstateAsync(_estateId);

            System.Windows.MessageBox.Show(
                "Estate deleted successfully.",
                "EstateFlow",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            _navigation.NavigateToDashboard();
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not delete estate: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Delete Estate Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─────────────────────────────────────────────
    // Executor
    // ─────────────────────────────────────────────

    private async Task ExecuteAddExecutorAsync()
    {
        if (_estateId == Guid.Empty)
        {
            StatusMessage =
                "No estate is currently open.";
            return;
        }

        if (string.IsNullOrWhiteSpace(
                NewExecutorFullName))
        {
            StatusMessage =
                "Executor full name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(
                NewExecutorEmail))
        {
            StatusMessage =
                "Executor email is required.";
            return;
        }

        try
        {
            IsLoading = true;

            StatusMessage =
                "Saving executor...";

            var request =
                new CreateExecutorRequest
                {
                    FullName =
                        NewExecutorFullName.Trim(),

                    IdNumber =
                        NewExecutorIdNumber?.Trim()
                        ?? string.Empty,

                    Email =
                        NewExecutorEmail.Trim(),

                    Role =
                        string.IsNullOrWhiteSpace(
                            NewExecutorRole)
                            ? "Nominated Executor"
                            : NewExecutorRole.Trim()
                };

            await _apiClient.AddExecutorAsync(
                _estateId,
                request);

            // Reload the estate from the API/SQL.
            // This proves we are displaying persisted data,
            // rather than merely adding something locally.
            var refreshedEstate =
                await _apiClient.GetEstateAsync(
                    _estateId);

            if (refreshedEstate is null)
            {
                StatusMessage =
                    "Executor was saved, but the estate could not be refreshed.";

                return;
            }

            Estate = refreshedEstate;

            NewExecutorFullName =
                string.Empty;

            NewExecutorIdNumber =
                string.Empty;

            NewExecutorEmail =
                string.Empty;

            NewExecutorRole =
                "Nominated Executor";

            StatusMessage =
                "Executor added successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not add executor: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteDeleteExecutorAsync(
        object? parameter)
    {
        if (parameter is not Guid executorId)
        {
            StatusMessage =
                "Could not determine which executor to remove.";

            return;
        }

        if (_estateId == Guid.Empty)
        {
            StatusMessage =
                "No estate is currently open.";

            return;
        }

        try
        {
            IsLoading = true;

            StatusMessage =
                "Removing executor...";

            await _apiClient.DeleteExecutorAsync(
                _estateId,
                executorId);

            var refreshedEstate =
                await _apiClient.GetEstateAsync(
                    _estateId);

            if (refreshedEstate is null)
            {
                StatusMessage =
                    "Executor was removed, but the estate could not be refreshed.";

                return;
            }

            Estate = refreshedEstate;

            StatusMessage =
                "Executor removed successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove executor: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    // ─────────────────────────────────────────────
    // Beneficiary
    // ─────────────────────────────────────────────

    private async Task ExecuteAddBeneficiaryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBeneficiaryFullName))
        {
            StatusMessage = "Beneficiary full name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewBeneficiaryEmail))
        {
            StatusMessage = "Beneficiary email is required.";
            return;
        }

        if (NewBeneficiaryHeirType == "Natural Person" &&
            (NewBeneficiaryDateOfBirth is null ||
             string.IsNullOrWhiteSpace(NewBeneficiaryMaritalStatus)))
        {
            StatusMessage = "Date of birth and marital status are required for a natural person.";
            return;
        }

        int? residueNumerator = null;
        int? residueDenominator = null;

        if (!string.IsNullOrWhiteSpace(NewBeneficiaryResidueNumerator) ||
            !string.IsNullOrWhiteSpace(NewBeneficiaryResidueDenominator))
        {
            if (!int.TryParse(NewBeneficiaryResidueNumerator, out var n) ||
                !int.TryParse(NewBeneficiaryResidueDenominator, out var d) ||
                n <= 0 || d <= 0 || n > d)
            {
                StatusMessage = "Residue portion must be a valid fraction, e.g. 1 / 1 or 1 / 2.";
                return;
            }

            residueNumerator = n;
            residueDenominator = d;
        }

        Money.TryParse(NewBeneficiaryCashLegacyAmount, out var cashLegacyAmount);
        Money.TryParse(NewBeneficiaryAdvancesPaid, out var advancesPaid);

        try
        {
            await _apiClient.AddBeneficiaryAsync(
                _estateId,
                new CreateBeneficiaryRequest
                {
                    FullName = NewBeneficiaryFullName,
                    IdNumber = NewBeneficiaryIdNumber,
                    Email = NewBeneficiaryEmail.Trim(),
                    Relationship = NewBeneficiaryRelationship,
                    HeirType = NewBeneficiaryHeirType,
                    FirstName = NewBeneficiaryFirstName,
                    MiddleName = NewBeneficiaryMiddleName,
                    Surname = NewBeneficiarySurname,
                    RelationshipDescription = NewBeneficiaryRelationshipDescription,
                    DateOfBirth = NewBeneficiaryDateOfBirth,
                    Gender = NewBeneficiaryGender,
                    MaritalStatus = NewBeneficiaryMaritalStatus,
                    WillClause = NewBeneficiaryWillClause,
                    CashLegacyAmount = cashLegacyAmount,
                    ResidueNumerator = residueNumerator,
                    ResidueDenominator = residueDenominator,
                    AdvancesPaid = advancesPaid,
                    IsSurvivingSpouse = NewBeneficiaryIsSurvivingSpouse,
                    IsMinor = NewBeneficiaryIsMinor,
                    GuardianOrTrustee = NewBeneficiaryGuardianOrTrustee,
                    IsPredeceased = NewBeneficiaryIsPredeceased,
                    IsUltimateBeneficiaryPreviousEstate = NewBeneficiaryIsUltimateBeneficiary,
                    BareDominiumFraction = NewBeneficiaryBareDominiumFraction,
                    DisplayOrder = (Estate?.Beneficiaries.Count ?? 0) + 1
                });

            NewBeneficiaryFirstName = string.Empty;
            NewBeneficiaryMiddleName = string.Empty;
            NewBeneficiarySurname = string.Empty;
            NewBeneficiaryIdNumber = string.Empty;
            NewBeneficiaryEmail = string.Empty;
            NewBeneficiaryRelationship = string.Empty;
            NewBeneficiaryHeirType = "Natural Person";
            NewBeneficiaryRelationshipDescription = string.Empty;
            NewBeneficiaryDateOfBirth = null;
            NewBeneficiaryGender = string.Empty;
            NewBeneficiaryMaritalStatus = string.Empty;
            NewBeneficiaryWillClause = string.Empty;
            NewBeneficiaryCashLegacyAmount = "0";
            NewBeneficiaryResidueNumerator = string.Empty;
            NewBeneficiaryResidueDenominator = string.Empty;
            NewBeneficiaryAdvancesPaid = "0";
            NewBeneficiaryIsSurvivingSpouse = false;
            NewBeneficiaryIsMinor = false;
            NewBeneficiaryGuardianOrTrustee = string.Empty;
            NewBeneficiaryIsPredeceased = false;
            NewBeneficiaryIsUltimateBeneficiary = false;
            NewBeneficiaryBareDominiumFraction = string.Empty;

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not add beneficiary: {ex.Message}";
        }
    }

    private async Task ExecuteDeleteBeneficiaryAsync(
        object? parameter)
    {
        if (parameter is not Guid beneficiaryId)
            return;

        try
        {
            await _apiClient.DeleteBeneficiaryAsync(
                _estateId,
                beneficiaryId);

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove beneficiary: {ex.Message}";
        }
    }

    // ─────────────────────────────────────────────
    // Movable Asset
    // ─────────────────────────────────────────────

    private async Task ExecuteAddMovableAssetAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAssetDescription))
        {
            StatusMessage = "Asset description is required.";
            return;
        }

        var selectedHeirs = MovableHeirOptions.Where(h => h.IsSelected).ToList();

        if (NewAssetIsSoldOrReducedToCash && selectedHeirs.Count > 0 && !NewAssetIsSpecialBequest)
        {
            StatusMessage =
                "An asset cannot be both awarded to heirs and sold / reduced to cash unless it is a special bequest.";
            return;
        }

        if (!NewAssetIsSpecialBequest && selectedHeirs.Count > 0)
        {
            var noResidue =
                (Estate?.Beneficiaries ?? new List<BeneficiaryDto>())
                    .Where(b => selectedHeirs.Any(s => s.BeneficiaryId == b.BeneficiaryId) &&
                                (!b.ResidueNumerator.HasValue || !b.ResidueDenominator.HasValue))
                    .Select(b => b.FullName)
                    .ToList();

            if (noResidue.Count > 0)
            {
                StatusMessage =
                    $"{string.Join(", ", noResidue)} has no residue portion. " +
                    "Tick 'special bequest' or give the heir a residue portion first.";
                return;
            }
        }

        Money.TryParse(NewAssetValue, out var value);
        Money.TryParse(NewAssetCumulativeDividend, out var cumulativeDividend);

        decimal? saleProceeds =
            Money.TryParse(NewAssetSaleProceeds, out var sp) ? sp : null;

        int? numberOfShares =
            int.TryParse(NewAssetNumberOfShares, out var ns) ? ns : null;

        decimal? priceCents =
            Money.TryParse(NewAssetPriceCents, out var pc) ? pc : null;

        try
        {
            await _apiClient.AddMovableAssetAsync(
                _estateId,
                new CreateMovableAssetRequest
                {
                    Description = NewAssetDescription,
                    AssetType = NewAssetType,
                    Value = value,
                    IsVehicle = NewAssetIsVehicle,
                    RegistrationNumber = NewAssetRegistrationNumber,
                    VinNumber = NewAssetVinNumber,
                    Make = NewAssetMake,
                    AssetCategory = NewAssetCategory,
                    ValuerName = NewAssetValuerName,
                    ValuationDate = NewAssetValuationDate,
                    AwardedToBeneficiaryIds =
                        selectedHeirs.Count == 0
                            ? null
                            : string.Join(",", selectedHeirs.Select(h => h.BeneficiaryId)),
                    IsSpecialBequest = NewAssetIsSpecialBequest,
                    IsSoldOrReducedToCash = NewAssetIsSoldOrReducedToCash,
                    SaleProceeds = NewAssetIsSoldOrReducedToCash ? saleProceeds : null,
                    SaleDate = NewAssetIsSoldOrReducedToCash ? NewAssetSaleDate : null,
                    BuyerName = NewAssetIsSoldOrReducedToCash ? NewAssetBuyerName : null,
                    ShareCompany = NewAssetIsRegisteredShare ? NewAssetShareCompany : null,
                    NumberOfShares = NewAssetIsRegisteredShare ? numberOfShares : null,
                    ShareClass = NewAssetIsRegisteredShare ? NewAssetShareClass : null,
                    PriceCentsPerShare = NewAssetIsRegisteredShare ? priceCents : null,
                    CumulativeDividend = NewAssetIsRegisteredShare ? cumulativeDividend : 0
                });

            NewAssetDescription = string.Empty;
            NewAssetType = string.Empty;
            NewAssetValue = "0";
            NewAssetIsVehicle = false;
            NewAssetRegistrationNumber = string.Empty;
            NewAssetVinNumber = string.Empty;
            NewAssetMake = string.Empty;
            NewAssetCategory = "Movable";
            NewAssetValuerName = string.Empty;
            NewAssetValuationDate = null;
            NewAssetIsSpecialBequest = false;
            NewAssetIsSoldOrReducedToCash = false;
            NewAssetSaleProceeds = string.Empty;
            NewAssetSaleDate = null;
            NewAssetBuyerName = string.Empty;
            NewAssetShareCompany = string.Empty;
            NewAssetNumberOfShares = string.Empty;
            NewAssetShareClass = string.Empty;
            NewAssetPriceCents = string.Empty;
            NewAssetCumulativeDividend = "0";

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not add asset: {ex.Message}";
        }
    }

    private async Task ExecuteDeleteMovableAssetAsync(
        object? parameter)
    {
        if (parameter is not Guid assetId)
            return;

        try
        {
            await _apiClient.DeleteMovableAssetAsync(
                _estateId,
                assetId);

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove asset: {ex.Message}";
        }
    }

    // ─────────────────────────────────────────────
    // Immovable Property
    // ─────────────────────────────────────────────

    private async Task ExecuteAddImmovablePropertyAsync()
    {
        if (_estateId == Guid.Empty)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPropertyDescription))
        {
            StatusMessage = "Property description is required.";
            return;
        }

        var awardError = ValidateAward(ImmovableHeirOptions, NewPropertyIsSpecialBequest, NewPropertyIsSold);
        if (awardError != null)
        {
            StatusMessage = awardError;
            return;
        }

        Money.TryParse(NewPropertyMunicipalValue, out var municipalValue);

        decimal? marketValue = Money.TryParse(NewPropertyMarketValue, out var mv) ? mv : null;
        decimal? extent = Money.TryParse(NewPropertyExtent, out var ex) ? ex : null;
        decimal? salePrice = NewPropertyIsSold && Money.TryParse(NewPropertySalePrice, out var sp) ? sp : null;

        int? shareN = null, shareD = null;
        if (PropertyShareAllowed &&
            int.TryParse(NewPropertyShareNumerator, out var n) &&
            int.TryParse(NewPropertyShareDenominator, out var d))
        {
            if (d <= 0 || n <= 0 || n >= d)
            {
                StatusMessage = "Ownership share must be a fraction below 1 (e.g. 1/2). Leave blank for full ownership.";
                return;
            }
            shareN = n;
            shareD = d;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Saving immovable property...";

            await _apiClient.AddImmovablePropertyAsync(
                _estateId,
                new CreateImmovablePropertyRequest
                {
                    Description = NewPropertyDescription.Trim(),
                    TitleDeedNumber = NewPropertyTitleDeedNumber?.Trim() ?? string.Empty,
                    MunicipalValue = municipalValue,
                    PropertyType = NewPropertyType,
                    ErfNumber = NewPropertyIsConventional ? NewPropertyErfNumber : null,
                    Township = NewPropertyTownship,
                    Division = NewPropertyDivision,
                    Province = NewPropertyProvince,
                    ExtentSquareMetres = extent,
                    StreetAddress = NewPropertyStreetAddress,
                    SchemeName = NewPropertyIsSectional ? NewPropertySchemeName : null,
                    SectionNumber = NewPropertyIsSectional ? NewPropertySectionNumber : null,
                    ExclusiveUseAreas = NewPropertyIsSectional ? NewPropertyExclusiveUseAreas : null,
                    ShareNumerator = shareN,
                    ShareDenominator = shareD,
                    MarketValue = marketValue,
                    ValuerName = NewPropertyValuerName,
                    ValuationDate = NewPropertyValuationDate,
                    AwardedToBeneficiaryIds = JoinSelected(ImmovableHeirOptions),
                    IsSpecialBequest = NewPropertyIsSpecialBequest,
                    IsSoldByEstate = NewPropertyIsSold,
                    SalePrice = salePrice,
                    SaleDate = NewPropertyIsSold ? NewPropertySaleDate : null,
                    PurchaserName = NewPropertyIsSold ? NewPropertyPurchaserName : null
                });

            NewPropertyDescription = string.Empty;
            NewPropertyTitleDeedNumber = string.Empty;
            NewPropertyMunicipalValue = "0";
            NewPropertyType = "Conventional";
            NewPropertyErfNumber = string.Empty;
            NewPropertyTownship = string.Empty;
            NewPropertyDivision = string.Empty;
            NewPropertyProvince = string.Empty;
            NewPropertyExtent = string.Empty;
            NewPropertyStreetAddress = string.Empty;
            NewPropertySchemeName = string.Empty;
            NewPropertySectionNumber = string.Empty;
            NewPropertyExclusiveUseAreas = string.Empty;
            NewPropertyShareNumerator = string.Empty;
            NewPropertyShareDenominator = string.Empty;
            NewPropertyMarketValue = string.Empty;
            NewPropertyValuerName = string.Empty;
            NewPropertyValuationDate = null;
            NewPropertyIsSpecialBequest = false;
            NewPropertyIsSold = false;
            NewPropertySalePrice = "0";
            NewPropertySaleDate = null;
            NewPropertyPurchaserName = string.Empty;

            var refreshedEstate = await _apiClient.GetEstateAsync(_estateId);
            if (refreshedEstate is not null)
                Estate = refreshedEstate;

            StatusMessage = "Immovable property added successfully.";
        }
        catch (Exception ex2)
        {
            StatusMessage = $"Could not add immovable property: {ex2.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteDeleteImmovablePropertyAsync(
        object? parameter)
    {
        if (parameter is not Guid assetId)
        {
            StatusMessage =
                "Could not determine which property to remove.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Removing immovable property...";

            await _apiClient.DeleteImmovablePropertyAsync(
                _estateId,
                assetId);

            var refreshedEstate =
                await _apiClient.GetEstateAsync(_estateId);

            if (refreshedEstate is not null)
                Estate = refreshedEstate;

            StatusMessage =
                "Immovable property removed successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove immovable property: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─────────────────────────────────────────────
    // Letter Generation
    // ─────────────────────────────────────────────

    private async Task ExecuteGenerateVehicleLetterAsync(
        object? parameter)
    {
        if (parameter is not Guid assetId)
        {
            StatusMessage =
                "Could not determine which vehicle to use.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generating traffic letter...";

            var filePath =
                await _apiClient
                    .GenerateVehicleLetterToTempFileAsync(
                        _estateId,
                        assetId);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

            StatusMessage =
                "Traffic letter generated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not generate traffic letter: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Letter Generation Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteDownloadLiquidationAccountAsync()
    {
        if (_estateId == Guid.Empty)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generating liquidation and distribution account...";

            var tempPath =
                await _apiClient.DownloadLiquidationAccountToTempFileAsync(_estateId);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Liquidation and Distribution Account",
                FileName = System.IO.Path.GetFileName(tempPath),
                DefaultExt = ".pdf",
                Filter = "PDF document (*.pdf)|*.pdf"
            };

            var finalPath = tempPath;
            if (dialog.ShowDialog() == true)
            {
                System.IO.File.Copy(tempPath, dialog.FileName, true);
                finalPath = dialog.FileName;
            }

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = finalPath,
                    UseShellExecute = true
                });

            StatusMessage = "Liquidation and distribution account generated.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not generate the account: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "L&D Account Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteGenerateInstitutionLetterAsync(
        object? parameter)
    {
        if (parameter is not Guid claimId)
        {
            StatusMessage =
                "Could not determine which financial claim to use.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generating institution letter...";

            var filePath =
                await _apiClient
                    .GenerateInstitutionLetterToTempFileAsync(
                        _estateId,
                        claimId);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

            StatusMessage =
                "Institution letter generated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not generate institution letter: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Letter Generation Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─────────────────────────────────────────────
    // Financial Claim
    // ─────────────────────────────────────────────

    private async Task ExecuteAddFinancialClaimAsync()
    {
        if (string.IsNullOrWhiteSpace(NewClaimInstitutionName))
        {
            StatusMessage = "Institution name is required.";
            return;
        }

        var awardError = ValidateAward(ClaimHeirOptions, NewClaimIsSpecialBequest, NewClaimIsCollected);
        if (awardError != null)
        {
            StatusMessage = awardError;
            return;
        }

        Money.TryParse(NewClaimValue, out var value);
        Money.TryParse(NewClaimAccruedInterest, out var interest);
        Money.TryParse(NewClaimCollectedAmount, out var collected);

        try
        {
            await _apiClient.AddFinancialClaimAsync(
                _estateId,
                new CreateFinancialClaimRequest
                {
                    InstitutionName = NewClaimInstitutionName,
                    InstitutionAddress = NewClaimInstitutionAddress,
                    AccountReference = NewClaimAccountReference,
                    Value = value,
                    ClaimType = NewClaimType,
                    IsInFavourOfEstate = NewClaimInFavour,
                    AccruedInterest = interest,
                    ValuationDate = NewClaimValuationDate,
                    AwardedToBeneficiaryIds = JoinSelected(ClaimHeirOptions),
                    IsSpecialBequest = NewClaimIsSpecialBequest,
                    IsCollectedAsCash = NewClaimIsCollected,
                    CollectedAmount = NewClaimIsCollected ? collected : 0,
                    CollectedDate = NewClaimIsCollected ? NewClaimCollectedDate : null,
                    IsCreditLifeInsurance = NewClaimIsCreditLife,
                    LinkedLiabilityNote = NewClaimLinkedLiabilityNote
                });

            NewClaimInstitutionName = string.Empty;
            NewClaimInstitutionAddress = string.Empty;
            NewClaimAccountReference = string.Empty;
            NewClaimValue = "0";
            NewClaimType = string.Empty;
            NewClaimInFavour = true;
            NewClaimAccruedInterest = "0";
            NewClaimValuationDate = null;
            NewClaimIsSpecialBequest = false;
            NewClaimIsCollected = false;
            NewClaimCollectedAmount = "0";
            NewClaimCollectedDate = null;
            NewClaimIsCreditLife = false;
            NewClaimLinkedLiabilityNote = string.Empty;

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not add claim: {ex.Message}";
        }
    }

    private async Task ExecuteDeleteFinancialClaimAsync(
        object? parameter)
    {
        if (parameter is not Guid claimId)
            return;

        try
        {
            await _apiClient.DeleteFinancialClaimAsync(
                _estateId,
                claimId);

            await LoadAsync(_estateId);
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove claim: {ex.Message}";
        }
    }


    // ─────────────────────────────────────────────
    // Liabilities + Liquidation & Distribution
    // ─────────────────────────────────────────────

    private async Task ExecuteAddLiabilityAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLiabilityCreditorName))
        {
            StatusMessage = "Creditor name is required.";
            return;
        }

        if (!Money.TryParse(NewLiabilityAmount, out var amount) ||
            amount < 0)
        {
            StatusMessage = "Enter a valid liability amount.";
            return;
        }

        Money.TryParse(NewLiabilityInterestAfterDeath, out var interestAfter);
        Money.TryParse(NewLiabilityAmountPaid, out var amountPaid);

        try
        {
            await _apiClient.AddLiabilityAsync(
                _estateId,
                new CreateLiabilityRequest
                {
                    CreditorName = NewLiabilityCreditorName.Trim(),
                    Description = NewLiabilityDescription.Trim(),
                    Amount = amount,
                    Category = NewLiabilityCategory,
                    AccountReference = NewLiabilityAccountReference,
                    InterestAfterDeath = interestAfter,
                    Status = NewLiabilityStatus,
                    DateLodged = NewLiabilityDateLodged,
                    SecuredAgainst = NewLiabilitySecuredAgainst,
                    IsTakenOverByHeir = NewLiabilityTakenOverByHeir,
                    IsPaid = NewLiabilityIsPaid,
                    AmountPaid = amountPaid,
                    DatePaid = NewLiabilityDatePaid,
                    IsCoveredByCreditLife = NewLiabilityCreditLife
                });

            NewLiabilityCreditorName = string.Empty;
            NewLiabilityDescription = string.Empty;
            NewLiabilityAmount = "0";
            NewLiabilityCategory = string.Empty;
            NewLiabilityAccountReference = string.Empty;
            NewLiabilityInterestAfterDeath = "0";
            NewLiabilityStatus = "Admitted";
            NewLiabilityDateLodged = null;
            NewLiabilitySecuredAgainst = string.Empty;
            NewLiabilityTakenOverByHeir = false;
            NewLiabilityIsPaid = false;
            NewLiabilityAmountPaid = "0";
            NewLiabilityDatePaid = null;
            NewLiabilityCreditLife = false;

            await LoadAsync(_estateId);
            StatusMessage = "Liability added.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not add liability: {ex.Message}";
        }
    }

    private async Task ExecuteDeleteLiabilityAsync(
        object? parameter)
    {
        if (parameter is not Guid liabilityId)
            return;

        try
        {
            await _apiClient.DeleteLiabilityAsync(
                _estateId,
                liabilityId);

            await LoadAsync(_estateId);
            StatusMessage = "Liability removed.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove liability: {ex.Message}";
        }
    }

    private async Task LoadLiquidationDistributionAsync()
    {
        if (_estateId == Guid.Empty)
            return;

        try
        {
            LiquidationDistributionDraft =
                await _apiClient
                    .GetLiquidationDistributionDraftAsync(
                        _estateId);
        }
        catch (Exception ex)
        {
            LiquidationDistributionDraft = null;
            StatusMessage =
                $"Could not load L&D draft: {ex.Message}";
        }
    }

    // ─────────────────────────────────────────────
    // Diary
    // ─────────────────────────────────────────────

    private async Task ExecuteAddDiaryEntryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDiaryNote))
        {
            StatusMessage =
                "Diary note is required.";
            return;
        }

        if (!NewDiaryDueDate.HasValue)
        {
            StatusMessage =
                "Diary due date is required.";
            return;
        }

        try
        {
            StatusMessage =
                "Adding diary entry...";

            await _apiClient.AddDiaryEntryAsync(
                _estateId,
                new CreateDiaryEntryRequest
                {
                    Note = NewDiaryNote.Trim(),
                    DueDate = NewDiaryDueDate.Value,
                    NotifyBy = NewDiaryNotifyBy
                });

            NewDiaryNote = string.Empty;
            NewDiaryDueDate =
                DateTime.Today.AddDays(7);
            NewDiaryNotifyBy = "Email";

            await LoadAsync(_estateId);

            StatusMessage =
                "Diary entry added.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not add diary entry: {ex.Message}";
        }
    }

    private void ExecuteOpenDiaryEntry(
        object? parameter)
    {
        if (parameter is not DiaryEntryDto diaryEntry)
        {
            StatusMessage =
                "Could not open the diary entry.";
            return;
        }

        var status =
            diaryEntry.IsCompleted
                ? "Completed"
                : "Open";

        var message =
            $"{diaryEntry.Note}\n\n" +
            $"Due date: {diaryEntry.DueDate:dd MMMM yyyy}\n" +
            $"Notify by: {diaryEntry.NotifyBy}\n" +
            $"Status: {status}";

        System.Windows.MessageBox.Show(
            message,
            "Diary Entry",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private async Task ExecuteToggleDiaryCompleteAsync(
        object? parameter)
    {
        if (parameter is not DiaryEntryDto diaryEntry)
            return;

        try
        {
            var newStatus =
                !diaryEntry.IsCompleted;

            await _apiClient.UpdateDiaryEntryStatusAsync(
                _estateId,
                diaryEntry.DiaryId,
                newStatus);

            await LoadAsync(_estateId);

            StatusMessage =
                newStatus
                    ? "Diary entry marked complete."
                    : "Diary entry reopened.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not update diary entry: {ex.Message}";
        }
    }

    private async Task ExecuteDeleteDiaryEntryAsync(
        object? parameter)
    {
        if (parameter is not Guid diaryId)
            return;

        try
        {
            await _apiClient.DeleteDiaryEntryAsync(
                _estateId,
                diaryId);

            await LoadAsync(_estateId);

            StatusMessage =
                "Diary entry removed.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove diary entry: {ex.Message}";
        }
    }

    // ─────────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────────

    // Four-stage administration workflow (Report & Letters -> Administration ->
    // L&D account -> Inspection & distribution). Dates are templates counted from
    // today and are meant to be adjusted by the controller.
    private static readonly (string Name, int Days, string Status)[] StbbStages =
    {
        ("Stage 1 – Report estate to the Master and obtain Letters of Executorship", 42, "InProgress"),
        ("Stage 2 – Administration: advert, valuations, creditors, SARS, estate bank account", 225, "Pending"),
        ("Stage 3 – Liquidation & Distribution Account lodged with the Master", 255, "Pending"),
        ("Stage 4 – Inspection (21 days), distribution and final cash statement", 360, "Pending")
    };

    private static readonly (string Note, int Days)[] StbbDiary =
    {
        ("Report estate to the Master; send checklist and beneficiary questionnaires", 7),
        ("Follow up Letters of Executorship (usually 2–6 weeks)", 42),
        ("Notify SARS of the death; collect information for pre- and post-death returns", 56),
        ("Open estate bank account", 56),
        ("Place Section 29 advertisement (Government Gazette and local newspaper)", 49),
        ("Obtain valuations of immovable property, vehicles and other assets", 70),
        ("Assess creditors' claims – accept or reject", 100),
        ("Confirm income tax returns lodged / request SARS tax clearance", 180),
        ("Stage 2 groundwork complete (target: 6 months from Letters)", 225),
        ("Lodge Liquidation & Distribution Account with the Master", 255),
        ("Respond to the Master's query sheet", 285),
        ("Advertise the L&D Account for inspection (21 days)", 300),
        ("Inspection period ends – confirm with Master and Magistrate that no objections were lodged", 321),
        ("Distribute to heirs; prepare final cash statement; close estate bank account", 340),
        ("Obtain final SARS tax clearance", 360)
    };

    private async Task ExecuteStartStbbWorkflowAsync()
    {
        if (_estateId == Guid.Empty)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        var existing = Estate?.ProgressStages?.Any(s => s.StageName.StartsWith("Stage 1")) == true;
        if (existing)
        {
            var answer = System.Windows.MessageBox.Show(
                "A four-stage workflow already exists on this estate. Add it again?",
                "Four-stage workflow",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (answer != System.Windows.MessageBoxResult.Yes)
                return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Creating workflow stages and diary reminders...";

            var today = DateTime.Today;

            foreach (var (name, days, status) in StbbStages)
            {
                await _apiClient.AddProgressStageAsync(
                    _estateId,
                    new CreateProgressStageRequest
                    {
                        StageName = name,
                        TargetDate = today.AddDays(days),
                        Status = status
                    });
            }

            foreach (var (note, days) in StbbDiary)
            {
                await _apiClient.AddDiaryEntryAsync(
                    _estateId,
                    new CreateDiaryEntryRequest
                    {
                        Note = "[Workflow] " + note,
                        DueDate = today.AddDays(days),
                        NotifyBy = "InApp"
                    });
            }

            await LoadAsync(_estateId);

            StatusMessage =
                $"Workflow created: {StbbStages.Length} stages and {StbbDiary.Length} diary reminders. Adjust the dates to this estate.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not create the workflow: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteAddProgressStageAsync()
    {
        if (_estateId == Guid.Empty)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewStageName))
        {
            StatusMessage = "Progress update is required.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Adding progress update...";

            await _apiClient.AddProgressStageAsync(
                _estateId,
                new CreateProgressStageRequest
                {
                    StageName = NewStageName.Trim(),
                    TargetDate = NewStageTargetDate ?? DateTime.Today,
                    Status = NewStageStatus
                });

            NewStageName = string.Empty;
            NewStageTargetDate = DateTime.Today;
            NewStageStatus = "InProgress";

            await RefreshEstateAsync();

            StatusMessage =
                "Progress update added.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not add progress update: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteBeginEditProgressStage(
        object? parameter)
    {
        if (parameter is not ProgressStageDto stage)
        {
            StatusMessage =
                "Could not determine which progress update to edit.";
            return;
        }

        _editingProgressStageId = stage.StageId;
        EditingProgressTitle = stage.StageName;
        EditingProgressDate = stage.TargetDate ?? DateTime.Today;
        EditingProgressStatus =
            string.IsNullOrWhiteSpace(stage.Status)
                ? "InProgress"
                : stage.Status;

        IsEditingProgress = true;
        StatusMessage = string.Empty;
    }

    private async Task ExecuteSaveProgressStageAsync()
    {
        if (_editingProgressStageId == Guid.Empty)
        {
            StatusMessage =
                "Choose a progress update to edit first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingProgressTitle))
        {
            StatusMessage =
                "Progress update is required.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Saving progress update...";

            await _apiClient.UpdateProgressStageAsync(
                _estateId,
                new UpdateProgressStageRequest
                {
                    StageId = _editingProgressStageId,
                    StageName = EditingProgressTitle.Trim(),
                    TargetDate = EditingProgressDate ?? DateTime.Today,
                    Status = EditingProgressStatus
                });

            CancelProgressEdit();
            await RefreshEstateAsync();

            StatusMessage =
                "Progress update saved.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not save progress update: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelProgressEdit()
    {
        _editingProgressStageId = Guid.Empty;
        EditingProgressTitle = string.Empty;
        EditingProgressDate = DateTime.Today;
        EditingProgressStatus = "InProgress";
        IsEditingProgress = false;
    }

    private async Task ExecuteDeleteProgressStageAsync(
        object? parameter)
    {
        if (parameter is not Guid stageId)
        {
            StatusMessage =
                "Could not determine which progress update to remove.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Removing progress update...";

            await _apiClient.DeleteProgressStageAsync(
                _estateId,
                stageId);

            if (_editingProgressStageId == stageId)
                CancelProgressEdit();

            await RefreshEstateAsync();

            StatusMessage =
                "Progress update removed.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not remove progress update: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshEstateAsync()
    {
        var refreshedEstate =
            await _apiClient.GetEstateAsync(_estateId);

        if (refreshedEstate is not null)
            Estate = refreshedEstate;
    }

    // ─────────────────────────────────────────────
    // Executor Tab – command implementations
    // ─────────────────────────────────────────────

    public void LoadExecutorDetail(ExecutorDto executor)
    {
        _executorDetailId = executor.ExecutorId;
        ExecutorDetailFullName = executor.FullName;
        ExecutorDetailIdNumber = executor.IdNumber ?? string.Empty;
        ExecutorDetailEmail = executor.Email ?? string.Empty;
        ExecutorDetailPhone = executor.Phone ?? string.Empty;
        ExecutorDetailAddress = executor.Address ?? string.Empty;
        ExecutorDetailRole = executor.Role ?? "Nominated Executor";
        ExecutorDetailMastersOffice = executor.MastersOffice ?? string.Empty;
        ExecutorDetailLettersRef = executor.LettersOfExecutorshipRef ?? string.Empty;
        ExecutorDetailLettersDate = executor.LettersOfExecutorshipDate;
        HasLettersDocument = !string.IsNullOrWhiteSpace(executor.LettersOfExecutorshipRef);
        LettersDocumentName = executor.LettersOfExecutorshipRef;
    }

    private async Task ExecuteSaveExecutorDetailAsync()
    {
        if (_executorDetailId == Guid.Empty)
        {
            StatusMessage = "Select an executor first.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Saving executor details...";

            await _apiClient.UpdateExecutorAsync(
                _estateId,
                _executorDetailId,
                new UpdateExecutorRequest
                {
                    FullName = ExecutorDetailFullName.Trim(),
                    IdNumber = ExecutorDetailIdNumber.Trim(),
                    Email = ExecutorDetailEmail.Trim(),
                    Phone = ExecutorDetailPhone.Trim(),
                    Address = ExecutorDetailAddress.Trim(),
                    Role = ExecutorDetailRole.Trim(),
                    MastersOffice = ExecutorDetailMastersOffice.Trim(),
                    LettersOfExecutorshipRef = ExecutorDetailLettersRef.Trim(),
                    LettersOfExecutorshipDate = ExecutorDetailLettersDate
                });

            await RefreshEstateAsync();
            StatusMessage = "Executor details saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save executor: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteUploadLettersAsync()
    {
        if (_executorDetailId == Guid.Empty)
        {
            StatusMessage = "Select an executor first.";
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Letters of Executorship",
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Uploading Letters of Executorship...";

            var blobRef = await _apiClient.UploadExecutorDocumentAsync(
                _estateId,
                _executorDetailId,
                dialog.FileName);

            ExecutorDetailLettersRef = blobRef;
            HasLettersDocument = true;
            LettersDocumentName = System.IO.Path.GetFileName(dialog.FileName);

            await _apiClient.UpdateExecutorAsync(
                _estateId,
                _executorDetailId,
                new UpdateExecutorRequest
                {
                    FullName = ExecutorDetailFullName,
                    IdNumber = ExecutorDetailIdNumber,
                    Email = ExecutorDetailEmail,
                    Phone = ExecutorDetailPhone,
                    Address = ExecutorDetailAddress,
                    Role = ExecutorDetailRole,
                    MastersOffice = ExecutorDetailMastersOffice,
                    LettersOfExecutorshipRef = blobRef,
                    LettersOfExecutorshipDate = ExecutorDetailLettersDate
                });

            await RefreshEstateAsync();
            StatusMessage = "Letters of Executorship uploaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not upload document: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteViewLetters()
    {
        if (string.IsNullOrWhiteSpace(ExecutorDetailLettersRef))
        {
            StatusMessage = "No Letters of Executorship document on file.";
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ExecutorDetailLettersRef,
                    UseShellExecute = true
                });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not open document: {ex.Message}";
        }
    }

    private async Task ExecuteSaveSection29Async()
    {
        if (_estateId == Guid.Empty) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Saving Section 29 advertisement...";

            await _apiClient.UpsertAdvertisementAsync(
                _estateId,
                new UpsertAdvertisementRequest
                {
                    AdvertisementType = "Section29",
                    GazetteDate = Section29GazetteDate,
                    GazetteNumber = Section29GazetteNumber,
                    NewspaperName = Section29NewspaperName,
                    NewspaperDate = Section29NewspaperDate,
                    ClaimDeadline = Section29ClaimDeadline,
                    ObjectionsReceived = Section29ObjectionsReceived,
                    Notes = Section29Notes
                });

            Section29Complete = Section29GazetteDate.HasValue && Section29ClaimDeadline.HasValue;
            Section29Overdue = Section29ClaimDeadline.HasValue && Section29ClaimDeadline.Value < DateTime.Today && !Section29ObjectionsReceived;

            StatusMessage = "Section 29 advertisement saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save Section 29: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteSaveSection35Async()
    {
        if (_estateId == Guid.Empty) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Saving Section 35 advertisement...";

            await _apiClient.UpsertAdvertisementAsync(
                _estateId,
                new UpsertAdvertisementRequest
                {
                    AdvertisementType = "Section35",
                    GazetteDate = Section35GazetteDate,
                    GazetteNumber = Section35GazetteNumber,
                    NewspaperName = Section35NewspaperName,
                    NewspaperDate = Section35NewspaperDate,
                    ClaimDeadline = Section35ClaimDeadline,
                    ObjectionsReceived = Section35ObjectionsReceived,
                    Notes = Section35Notes
                });

            StatusMessage = "Section 35 advertisement saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save Section 35: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}