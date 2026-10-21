using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Validation;

namespace EstateFlow.Desktop.Mvvm;

// Editing support for the records on the estate screen. The "Add" form is reused: pressing Edit
// on a row loads that record into the form and the button becomes "Save Changes".
public partial class EstateDetailViewModel
{
    // ─────────────────────────────────────────────
    // Shared helpers
    // ─────────────────────────────────────────────

    private static string FormatMoney(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal? value) =>
        value.HasValue ? FormatMoney(value.Value) : string.Empty;

    private static void SelectHeirs(IEnumerable<HeirOption> options, string? ids)
    {
        var wanted =
            (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .ToHashSet();

        foreach (var option in options)
            option.IsSelected = wanted.Contains(option.BeneficiaryId);
    }

    private static void ClearHeirSelection(IEnumerable<HeirOption> options)
    {
        foreach (var option in options)
            option.IsSelected = false;
    }

    private static void SplitName(string? fullName, out string first, out string middle, out string surname)
    {
        var parts = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        first = parts.Length > 0 ? parts[0] : string.Empty;
        surname = parts.Length > 1 ? parts[^1] : string.Empty;
        middle = parts.Length > 2 ? string.Join(" ", parts[1..^1]) : string.Empty;
    }

    // ─────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────

    public ICommand EditBeneficiaryCommand { get; private set; } = null!;
    public ICommand CancelBeneficiaryEditCommand { get; private set; } = null!;
    public ICommand EditMovableAssetCommand { get; private set; } = null!;
    public ICommand CancelMovableEditCommand { get; private set; } = null!;
    public ICommand EditImmovablePropertyCommand { get; private set; } = null!;
    public ICommand CancelImmovableEditCommand { get; private set; } = null!;
    public ICommand EditFinancialClaimCommand { get; private set; } = null!;
    public ICommand CancelClaimEditCommand { get; private set; } = null!;
    public ICommand EditLiabilityCommand { get; private set; } = null!;
    public ICommand CancelLiabilityEditCommand { get; private set; } = null!;
    public ICommand EditDiaryEntryCommand { get; private set; } = null!;
    public ICommand CancelDiaryEditCommand { get; private set; } = null!;
    public ICommand GenerateSection29Command { get; private set; } = null!;

    private void InitEditCommands()
    {
        EditBeneficiaryCommand = new RelayCommand(p => { if (p is BeneficiaryDto b) BeginEditBeneficiary(b); });
        CancelBeneficiaryEditCommand = new RelayCommand(_ => ResetBeneficiaryForm());

        EditMovableAssetCommand = new RelayCommand(p => { if (p is MovableAssetDto a) BeginEditMovable(a); });
        CancelMovableEditCommand = new RelayCommand(_ => ResetMovableForm());

        EditImmovablePropertyCommand = new RelayCommand(p => { if (p is ImmovablePropertyDto x) BeginEditImmovable(x); });
        CancelImmovableEditCommand = new RelayCommand(_ => ResetImmovableForm());

        EditFinancialClaimCommand = new RelayCommand(p => { if (p is FinancialClaimDto c) BeginEditClaim(c); });
        CancelClaimEditCommand = new RelayCommand(_ => ResetClaimForm());

        EditLiabilityCommand = new RelayCommand(p => { if (p is LiabilityDto l) BeginEditLiability(l); });
        CancelLiabilityEditCommand = new RelayCommand(_ => ResetLiabilityForm());

        EditDiaryEntryCommand = new RelayCommand(p => { if (p is DiaryEntryDto d) BeginEditDiaryEntry(d); });
        CancelDiaryEditCommand = new RelayCommand(_ => ResetDiaryForm());

        GenerateSection29Command = new RelayCommand(_ => ExecuteGenerateSection29Notice());
    }

    // ─────────────────────────────────────────────
    // Executor
    // ─────────────────────────────────────────────

    public string ExecutorSaveLabel => IsEditingExecutor ? "Save Changes" : "Add Executor";

    // ─────────────────────────────────────────────
    // Heirs
    // ─────────────────────────────────────────────

    private Guid? _editingBeneficiaryId;
    private int _editingBeneficiaryOrder;

    public bool IsEditingBeneficiary => _editingBeneficiaryId.HasValue;

    public string BeneficiaryFormTitle =>
        IsEditingBeneficiary ? "EDIT HEIR / BENEFICIARY" : "ADD HEIR / BENEFICIARY";

    public string BeneficiarySaveLabel =>
        IsEditingBeneficiary ? "Save Changes" : "+ Add Beneficiary";

    private void SetEditingBeneficiary(Guid? id, int order = 0)
    {
        _editingBeneficiaryId = id;
        _editingBeneficiaryOrder = order;
        OnPropertyChanged(nameof(IsEditingBeneficiary));
        OnPropertyChanged(nameof(BeneficiaryFormTitle));
        OnPropertyChanged(nameof(BeneficiarySaveLabel));
    }

    private Task SaveBeneficiaryAsync(CreateBeneficiaryRequest request)
    {
        if (_editingBeneficiaryId is Guid id)
        {
            request.DisplayOrder = _editingBeneficiaryOrder;
            return _apiClient.UpdateBeneficiaryAsync(_estateId, id, request);
        }

        return _apiClient.AddBeneficiaryAsync(_estateId, request);
    }

    // The ID number holds the date of birth, so fill it in when it has not been entered yet.
    private void FillBeneficiaryDateOfBirthFromId()
    {
        if (NewBeneficiaryDateOfBirth is null &&
            SaIdNumber.TryGetDateOfBirth(NewBeneficiaryIdNumber, DateTime.Today, out var dob))
        {
            NewBeneficiaryDateOfBirth = dob;
        }
    }

    private void ResetBeneficiaryForm()
    {
        SetEditingBeneficiary(null);

        NewBeneficiaryFirstName = string.Empty;
        NewBeneficiaryMiddleName = string.Empty;
        NewBeneficiarySurname = string.Empty;
        NewBeneficiaryDateOfBirth = null;
        NewBeneficiaryIdNumber = string.Empty;
        NewBeneficiaryEmail = string.Empty;
        NewBeneficiaryRelationship = string.Empty;
        NewBeneficiaryHeirType = "Natural Person";
        NewBeneficiaryRelationshipDescription = string.Empty;
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
    }

    private void BeginEditBeneficiary(BeneficiaryDto b)
    {
        ResetBeneficiaryForm();
        SetEditingBeneficiary(b.BeneficiaryId, b.DisplayOrder);

        var first = b.FirstName ?? string.Empty;
        var middle = b.MiddleName ?? string.Empty;
        var surname = b.Surname ?? string.Empty;

        // Older records only have a full name.
        if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(surname))
            SplitName(b.FullName, out first, out middle, out surname);

        NewBeneficiaryFirstName = first;
        NewBeneficiaryMiddleName = middle;
        NewBeneficiarySurname = surname;
        NewBeneficiaryDateOfBirth = b.DateOfBirth;
        NewBeneficiaryIdNumber = b.IdNumber ?? string.Empty;
        NewBeneficiaryEmail = b.Email ?? string.Empty;
        NewBeneficiaryRelationship = b.Relationship ?? string.Empty;
        NewBeneficiaryHeirType = string.IsNullOrWhiteSpace(b.HeirType) ? "Natural Person" : b.HeirType;
        NewBeneficiaryRelationshipDescription = b.RelationshipDescription ?? string.Empty;
        NewBeneficiaryGender = b.Gender ?? string.Empty;
        NewBeneficiaryMaritalStatus = b.MaritalStatus ?? string.Empty;
        NewBeneficiaryWillClause = b.WillClause ?? string.Empty;
        NewBeneficiaryCashLegacyAmount = FormatMoney(b.CashLegacyAmount);
        NewBeneficiaryResidueNumerator = b.ResidueNumerator?.ToString() ?? string.Empty;
        NewBeneficiaryResidueDenominator = b.ResidueDenominator?.ToString() ?? string.Empty;
        NewBeneficiaryAdvancesPaid = FormatMoney(b.AdvancesPaid);
        NewBeneficiaryIsSurvivingSpouse = b.IsSurvivingSpouse;
        NewBeneficiaryIsMinor = b.IsMinor;
        NewBeneficiaryGuardianOrTrustee = b.GuardianOrTrustee ?? string.Empty;
        NewBeneficiaryIsPredeceased = b.IsPredeceased;
        NewBeneficiaryIsUltimateBeneficiary = b.IsUltimateBeneficiaryPreviousEstate;
        NewBeneficiaryBareDominiumFraction = b.BareDominiumFraction ?? string.Empty;

        StatusMessage = $"Editing {b.FullName}. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Movable assets
    // ─────────────────────────────────────────────

    private Guid? _editingMovableId;

    public bool IsEditingMovable => _editingMovableId.HasValue;
    public string MovableFormTitle => IsEditingMovable ? "EDIT MOVABLE ASSET" : "ADD MOVABLE ASSET";
    public string MovableSaveLabel => IsEditingMovable ? "Save Changes" : "+ Add Asset";

    private void SetEditingMovable(Guid? id)
    {
        _editingMovableId = id;
        OnPropertyChanged(nameof(IsEditingMovable));
        OnPropertyChanged(nameof(MovableFormTitle));
        OnPropertyChanged(nameof(MovableSaveLabel));
    }

    private Task SaveMovableAsync(CreateMovableAssetRequest request) =>
        _editingMovableId is Guid id
            ? _apiClient.UpdateMovableAssetAsync(_estateId, id, request)
            : _apiClient.AddMovableAssetAsync(_estateId, request);

    private void ResetMovableForm()
    {
        SetEditingMovable(null);

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
        ClearHeirSelection(MovableHeirOptions);
    }

    private void BeginEditMovable(MovableAssetDto a)
    {
        ResetMovableForm();
        SetEditingMovable(a.AssetId);

        NewAssetCategory = string.IsNullOrWhiteSpace(a.AssetCategory) ? "Movable" : a.AssetCategory;
        NewAssetDescription = a.Description;
        NewAssetType = a.AssetType;
        NewAssetValue = FormatMoney(a.Value);
        NewAssetIsVehicle = a.IsVehicle;
        NewAssetRegistrationNumber = a.RegistrationNumber ?? string.Empty;
        NewAssetVinNumber = a.VinNumber ?? string.Empty;
        NewAssetMake = a.Make ?? string.Empty;
        NewAssetValuerName = a.ValuerName ?? string.Empty;
        NewAssetValuationDate = a.ValuationDate;
        NewAssetIsSpecialBequest = a.IsSpecialBequest;
        NewAssetIsSoldOrReducedToCash = a.IsSoldOrReducedToCash;
        NewAssetSaleProceeds = FormatMoney(a.SaleProceeds);
        NewAssetSaleDate = a.SaleDate;
        NewAssetBuyerName = a.BuyerName ?? string.Empty;
        NewAssetShareCompany = a.ShareCompany ?? string.Empty;
        NewAssetNumberOfShares = a.NumberOfShares?.ToString() ?? string.Empty;
        NewAssetShareClass = a.ShareClass ?? string.Empty;
        NewAssetPriceCents = FormatMoney(a.PriceCentsPerShare);
        NewAssetCumulativeDividend = FormatMoney(a.CumulativeDividend);
        SelectHeirs(MovableHeirOptions, a.AwardedToBeneficiaryIds);

        StatusMessage = $"Editing {a.Description}. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Immovable property
    // ─────────────────────────────────────────────

    private Guid? _editingImmovableId;

    public bool IsEditingImmovable => _editingImmovableId.HasValue;
    public string ImmovableFormTitle => IsEditingImmovable ? "EDIT IMMOVABLE PROPERTY" : "ADD IMMOVABLE PROPERTY";
    public string ImmovableSaveLabel => IsEditingImmovable ? "Save Changes" : "+ Add Property";

    private void SetEditingImmovable(Guid? id)
    {
        _editingImmovableId = id;
        OnPropertyChanged(nameof(IsEditingImmovable));
        OnPropertyChanged(nameof(ImmovableFormTitle));
        OnPropertyChanged(nameof(ImmovableSaveLabel));
    }

    private Task SaveImmovableAsync(CreateImmovablePropertyRequest request) =>
        _editingImmovableId is Guid id
            ? _apiClient.UpdateImmovablePropertyAsync(_estateId, id, request)
            : _apiClient.AddImmovablePropertyAsync(_estateId, request);

    private void ResetImmovableForm()
    {
        SetEditingImmovable(null);

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
        ClearHeirSelection(ImmovableHeirOptions);
    }

    private void BeginEditImmovable(ImmovablePropertyDto p)
    {
        ResetImmovableForm();
        SetEditingImmovable(p.AssetId);

        NewPropertyDescription = p.Description;
        NewPropertyTitleDeedNumber = p.TitleDeedNumber ?? string.Empty;
        NewPropertyMunicipalValue = FormatMoney(p.MunicipalValue);
        NewPropertyType = string.IsNullOrWhiteSpace(p.PropertyType) ? "Conventional" : p.PropertyType;
        NewPropertyErfNumber = p.ErfNumber ?? string.Empty;
        NewPropertyTownship = p.Township ?? string.Empty;
        NewPropertyDivision = p.Division ?? string.Empty;
        NewPropertyProvince = p.Province ?? string.Empty;
        NewPropertyExtent = FormatMoney(p.ExtentSquareMetres);
        NewPropertyStreetAddress = p.StreetAddress ?? string.Empty;
        NewPropertySchemeName = p.SchemeName ?? string.Empty;
        NewPropertySectionNumber = p.SectionNumber ?? string.Empty;
        NewPropertyExclusiveUseAreas = p.ExclusiveUseAreas ?? string.Empty;
        NewPropertyShareNumerator = p.ShareNumerator?.ToString() ?? string.Empty;
        NewPropertyShareDenominator = p.ShareDenominator?.ToString() ?? string.Empty;
        NewPropertyMarketValue = FormatMoney(p.MarketValue);
        NewPropertyValuerName = p.ValuerName ?? string.Empty;
        NewPropertyValuationDate = p.ValuationDate;
        NewPropertyIsSpecialBequest = p.IsSpecialBequest;
        NewPropertyIsSold = p.IsSoldByEstate;
        NewPropertySalePrice = p.SalePrice.HasValue ? FormatMoney(p.SalePrice) : "0";
        NewPropertySaleDate = p.SaleDate;
        NewPropertyPurchaserName = p.PurchaserName ?? string.Empty;
        SelectHeirs(ImmovableHeirOptions, p.AwardedToBeneficiaryIds);

        StatusMessage = $"Editing {p.Description}. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Financial claims
    // ─────────────────────────────────────────────

    private Guid? _editingClaimId;

    public bool IsEditingClaim => _editingClaimId.HasValue;
    public string ClaimFormTitle => IsEditingClaim ? "EDIT FINANCIAL CLAIM" : "ADD FINANCIAL CLAIM";
    public string ClaimSaveLabel => IsEditingClaim ? "Save Changes" : "+ Add Claim";

    private void SetEditingClaim(Guid? id)
    {
        _editingClaimId = id;
        OnPropertyChanged(nameof(IsEditingClaim));
        OnPropertyChanged(nameof(ClaimFormTitle));
        OnPropertyChanged(nameof(ClaimSaveLabel));
    }

    private Task SaveClaimAsync(CreateFinancialClaimRequest request) =>
        _editingClaimId is Guid id
            ? _apiClient.UpdateFinancialClaimAsync(_estateId, id, request)
            : _apiClient.AddFinancialClaimAsync(_estateId, request);

    private void ResetClaimForm()
    {
        SetEditingClaim(null);

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
        ClearHeirSelection(ClaimHeirOptions);
    }

    private void BeginEditClaim(FinancialClaimDto c)
    {
        ResetClaimForm();
        SetEditingClaim(c.ClaimId);

        NewClaimInstitutionName = c.InstitutionName;
        NewClaimInstitutionAddress = c.InstitutionAddress;
        NewClaimAccountReference = c.AccountReference;
        NewClaimValue = FormatMoney(c.Value);
        NewClaimType = c.ClaimType ?? string.Empty;
        NewClaimInFavour = c.IsInFavourOfEstate;
        NewClaimAccruedInterest = FormatMoney(c.AccruedInterest);
        NewClaimValuationDate = c.ValuationDate;
        NewClaimIsSpecialBequest = c.IsSpecialBequest;
        NewClaimIsCollected = c.IsCollectedAsCash;
        NewClaimCollectedAmount = FormatMoney(c.CollectedAmount);
        NewClaimCollectedDate = c.CollectedDate;
        NewClaimIsCreditLife = c.IsCreditLifeInsurance;
        NewClaimLinkedLiabilityNote = c.LinkedLiabilityNote ?? string.Empty;
        SelectHeirs(ClaimHeirOptions, c.AwardedToBeneficiaryIds);

        StatusMessage = $"Editing {c.InstitutionName}. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Liabilities
    // ─────────────────────────────────────────────

    private Guid? _editingLiabilityId;

    public bool IsEditingLiability => _editingLiabilityId.HasValue;
    public string LiabilityFormTitle => IsEditingLiability ? "EDIT LIABILITY" : "ADD LIABILITY";
    public string LiabilitySaveLabel => IsEditingLiability ? "Save Changes" : "+ Add Liability";

    private void SetEditingLiability(Guid? id)
    {
        _editingLiabilityId = id;
        OnPropertyChanged(nameof(IsEditingLiability));
        OnPropertyChanged(nameof(LiabilityFormTitle));
        OnPropertyChanged(nameof(LiabilitySaveLabel));
    }

    private Task SaveLiabilityAsync(CreateLiabilityRequest request) =>
        _editingLiabilityId is Guid id
            ? _apiClient.UpdateLiabilityAsync(_estateId, id, request)
            : _apiClient.AddLiabilityAsync(_estateId, request);

    private void ResetLiabilityForm()
    {
        SetEditingLiability(null);

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
    }

    private void BeginEditLiability(LiabilityDto l)
    {
        ResetLiabilityForm();
        SetEditingLiability(l.LiabilityId);

        NewLiabilityCreditorName = l.CreditorName;
        NewLiabilityDescription = l.Description ?? string.Empty;
        NewLiabilityAmount = FormatMoney(l.Amount);
        NewLiabilityCategory = l.Category ?? string.Empty;
        NewLiabilityAccountReference = l.AccountReference ?? string.Empty;
        NewLiabilityInterestAfterDeath = FormatMoney(l.InterestAfterDeath);
        NewLiabilityStatus = string.IsNullOrWhiteSpace(l.Status) ? "Admitted" : l.Status;
        NewLiabilityDateLodged = l.DateLodged;
        NewLiabilitySecuredAgainst = l.SecuredAgainst ?? string.Empty;
        NewLiabilityTakenOverByHeir = l.IsTakenOverByHeir;
        NewLiabilityIsPaid = l.IsPaid;
        NewLiabilityAmountPaid = FormatMoney(l.AmountPaid);
        NewLiabilityDatePaid = l.DatePaid;
        NewLiabilityCreditLife = l.IsCoveredByCreditLife;

        StatusMessage = $"Editing {l.CreditorName}. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Diary entries
    // ─────────────────────────────────────────────

    private Guid? _editingDiaryId;

    public bool IsEditingDiary => _editingDiaryId.HasValue;
    public string DiaryFormTitle => IsEditingDiary ? "EDIT DIARY ENTRY" : "ADD DIARY ENTRY";
    public string DiarySaveLabel => IsEditingDiary ? "Save Changes" : "+ Add Diary Entry";

    private void SetEditingDiary(Guid? id)
    {
        _editingDiaryId = id;
        OnPropertyChanged(nameof(IsEditingDiary));
        OnPropertyChanged(nameof(DiaryFormTitle));
        OnPropertyChanged(nameof(DiarySaveLabel));
    }

    private Task SaveDiaryEntryAsync(CreateDiaryEntryRequest request) =>
        _editingDiaryId is Guid id
            ? _apiClient.UpdateDiaryEntryAsync(_estateId, id, request)
            : _apiClient.AddDiaryEntryAsync(_estateId, request);

    private void ResetDiaryForm()
    {
        SetEditingDiary(null);

        NewDiaryNote = string.Empty;
        NewDiaryDueDate = DateTime.Today.AddDays(7);
        NewDiaryNotifyBy = "Email";
    }

    private void BeginEditDiaryEntry(DiaryEntryDto d)
    {
        ResetDiaryForm();
        SetEditingDiary(d.DiaryId);

        NewDiaryNote = d.Note;
        NewDiaryDueDate = d.DueDate;
        NewDiaryNotifyBy = string.IsNullOrWhiteSpace(d.NotifyBy) ? "Email" : d.NotifyBy;

        StatusMessage = "Editing the diary entry. Change the details in the form at the top, then press Save Changes.";
    }

    // ─────────────────────────────────────────────
    // Section 29 / 35 advertisements
    // ─────────────────────────────────────────────

    // Fills the advertisement forms from what is saved on the estate.
    private void LoadAdvertisementsFromEstate()
    {
        foreach (var ad in Estate?.Advertisements ?? new List<AdvertisementDto>())
        {
            if (ad.AdvertisementType == "Section29")
            {
                Section29GazetteNumber = ad.GazetteNumber ?? string.Empty;
                Section29GazetteDate = ad.GazetteDate;
                Section29NewspaperName = ad.NewspaperName ?? string.Empty;
                Section29NewspaperDate = ad.NewspaperDate;
                Section29ClaimDeadline = ad.ClaimDeadline;
                Section29ObjectionsReceived = ad.ObjectionsReceived;
                Section29Notes = ad.Notes ?? string.Empty;
            }
            else if (ad.AdvertisementType == "Section35")
            {
                Section35GazetteNumber = ad.GazetteNumber ?? string.Empty;
                Section35GazetteDate = ad.GazetteDate;
                Section35NewspaperName = ad.NewspaperName ?? string.Empty;
                Section35NewspaperDate = ad.NewspaperDate;
                Section35ClaimDeadline = ad.ClaimDeadline;
                Section35ObjectionsReceived = ad.ObjectionsReceived;
                Section35Notes = ad.Notes ?? string.Empty;
            }
        }

        Section29Complete = Section29GazetteDate.HasValue && Section29ClaimDeadline.HasValue;
        Section29Overdue =
            Section29ClaimDeadline.HasValue &&
            Section29ClaimDeadline.Value < DateTime.Today &&
            !Section29ObjectionsReceived;
    }

    // Writes a draft of the Section 29 notice to a text file and opens it.
    private void ExecuteGenerateSection29Notice()
    {
        if (Estate is null)
        {
            StatusMessage = "No estate is currently open.";
            return;
        }

        var deceased = Estate.Deceased;
        var name = string.IsNullOrWhiteSpace(deceased?.FullName) ? "(name of the deceased)" : deceased!.FullName;
        var published = Section29GazetteDate ?? Section29NewspaperDate;
        var deadline = Section29ClaimDeadline ?? published?.AddDays(30);

        var sb = new StringBuilder();
        sb.AppendLine("NOTICE TO CREDITORS AND DEBTORS IN A DECEASED ESTATE");
        sb.AppendLine("(Section 29 of the Administration of Estates Act 66 of 1965)");
        sb.AppendLine();
        sb.AppendLine($"Estate late: {name}");

        if (!string.IsNullOrWhiteSpace(deceased?.IdNumber))
            sb.AppendLine($"Identity number: {deceased!.IdNumber}");

        sb.AppendLine($"Date of death: {Estate.DateOfDeath:dd MMMM yyyy}");

        if (!string.IsNullOrWhiteSpace(deceased?.PlaceOfDeath))
            sb.AppendLine($"Place of death: {deceased!.PlaceOfDeath}");

        if (!string.IsNullOrWhiteSpace(Estate.MastersReference))
            sb.AppendLine($"Master's reference: {Estate.MastersReference}");

        if (!string.IsNullOrWhiteSpace(Estate.MastersOffice))
            sb.AppendLine($"Master's office: {Estate.MastersOffice}");

        sb.AppendLine($"File number: {Estate.FileNumber}");
        sb.AppendLine();
        sb.AppendLine(
            "All persons who have claims against this estate are called upon to lodge their claims " +
            "in writing with the undersigned within 30 days of the date of publication of this notice. " +
            "All persons who owe money to the estate are called upon to pay their debts to the " +
            "undersigned within the same period.");

        if (deadline.HasValue)
        {
            sb.AppendLine();
            sb.AppendLine($"Last day to lodge claims: {deadline.Value:dd MMMM yyyy}");
        }

        if (!string.IsNullOrWhiteSpace(Section29GazetteNumber) || Section29GazetteDate.HasValue)
            sb.AppendLine($"Government Gazette: {Section29GazetteNumber} {Section29GazetteDate:dd MMMM yyyy}".TrimEnd());

        if (!string.IsNullOrWhiteSpace(Section29NewspaperName) || Section29NewspaperDate.HasValue)
            sb.AppendLine($"Newspaper: {Section29NewspaperName} {Section29NewspaperDate:dd MMMM yyyy}".TrimEnd());

        sb.AppendLine();

        if (Estate.FirmActsAsAgent && !string.IsNullOrWhiteSpace(Estate.AgentName))
        {
            sb.AppendLine("Agent for the executor:");
            sb.AppendLine(Estate.AgentName);

            if (!string.IsNullOrWhiteSpace(Estate.AgentReference))
                sb.AppendLine($"Reference: {Estate.AgentReference}");
            if (!string.IsNullOrWhiteSpace(Estate.AgentEmail))
                sb.AppendLine($"Email: {Estate.AgentEmail}");
            if (!string.IsNullOrWhiteSpace(Estate.AgentPhone))
                sb.AppendLine($"Telephone: {Estate.AgentPhone}");

            sb.AppendLine();
        }

        sb.AppendLine(Estate.Executors.Count > 1 ? "Executors:" : "Executor:");

        if (Estate.Executors.Count == 0)
            sb.AppendLine("(no executor captured yet)");

        foreach (var executor in Estate.Executors)
        {
            sb.AppendLine(string.IsNullOrWhiteSpace(executor.Role)
                ? executor.FullName
                : $"{executor.FullName} ({executor.Role})");

            if (!string.IsNullOrWhiteSpace(executor.Address))
                sb.AppendLine(executor.Address);
            if (!string.IsNullOrWhiteSpace(executor.Email))
                sb.AppendLine($"Email: {executor.Email}");
            if (!string.IsNullOrWhiteSpace(executor.Phone))
                sb.AppendLine($"Telephone: {executor.Phone}");

            sb.AppendLine();
        }

        sb.AppendLine($"Dated: {DateTime.Today:dd MMMM yyyy}");

        try
        {
            var safeFile = string.Concat(
                Estate.FileNumber.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_'));

            var path = Path.Combine(Path.GetTempPath(), $"Section29_Notice_{safeFile}.txt");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

            StatusMessage = "Section 29 notice draft created.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not create the notice draft: {ex.Message}";
        }
    }
}
