using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Validation;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class EstateFormViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigation;

    private Guid? _editingEstateId;

    private string _fileNumber = string.Empty;
    public string FileNumber
    {
        get => _fileNumber;
        set => SetProperty(ref _fileNumber, value);
    }

    private string _mastersReference = string.Empty;
    public string MastersReference
    {
        get => _mastersReference;
        set => SetProperty(ref _mastersReference, value);
    }

    private DateTime _dateOfDeath = DateTime.Today;
    public DateTime DateOfDeath
    {
        get => _dateOfDeath;
        set => SetProperty(ref _dateOfDeath, value);
    }

    private bool _isDutiable;
    public bool IsDutiable
    {
        get => _isDutiable;
        set => SetProperty(ref _isDutiable, value);
    }

    private string _estateStatus = "Reported";
    public string EstateStatus
    {
        get => _estateStatus;
        set => SetProperty(ref _estateStatus, value);
    }

    private bool _firmActsAsAgent;
    public bool FirmActsAsAgent
    {
        get => _firmActsAsAgent;
        set => SetProperty(ref _firmActsAsAgent, value);
    }

    private string _agentName = string.Empty;
    public string AgentName
    {
        get => _agentName;
        set => SetProperty(ref _agentName, value);
    }

    private string _agentReference = string.Empty;
    public string AgentReference
    {
        get => _agentReference;
        set => SetProperty(ref _agentReference, value);
    }

    private string _agentEmail = string.Empty;
    public string AgentEmail
    {
        get => _agentEmail;
        set => SetProperty(ref _agentEmail, value);
    }

    private string _agentPhone = string.Empty;
    public string AgentPhone
    {
        get => _agentPhone;
        set => SetProperty(ref _agentPhone, value);
    }

    private string _administrationExpenses = "0";
    public string AdministrationExpenses
    {
        get => _administrationExpenses;
        set => SetProperty(ref _administrationExpenses, value);
    }
    private DateTime? _dateOfAccount;
    public DateTime? DateOfAccount { get => _dateOfAccount; set => SetProperty(ref _dateOfAccount, value); }

    private string _placeOfAccount = string.Empty;
    public string PlaceOfAccount { get => _placeOfAccount; set => SetProperty(ref _placeOfAccount, value); }

    private string _accountType = "First and Final";
    public string AccountType { get => _accountType; set => SetProperty(ref _accountType, value); }

    private bool _isTestate = true;
    public bool IsTestate { get => _isTestate; set => SetProperty(ref _isTestate, value); }

    private string _mastersOffice = string.Empty;
    public string MastersOffice { get => _mastersOffice; set => SetProperty(ref _mastersOffice, value); }

    private DateTime? _lettersOfExecutorshipDate;
    public DateTime? LettersOfExecutorshipDate { get => _lettersOfExecutorshipDate; set => SetProperty(ref _lettersOfExecutorshipDate, value); }

    private string _executorFeePercentage = "3.5";
    public string ExecutorFeePercentage { get => _executorFeePercentage; set => SetProperty(ref _executorFeePercentage, value); }

    private bool _isVatRegistered = true;
    public bool IsVatRegistered { get => _isVatRegistered; set => SetProperty(ref _isVatRegistered, value); }

    private string _vatRate = "15";
    public string VatRate { get => _vatRate; set => SetProperty(ref _vatRate, value); }

    private string _mastersFee = "0";
    public string MastersFee { get => _mastersFee; set => SetProperty(ref _mastersFee, value); }

    private string _estateDutyRebate = "3500000";
    public string EstateDutyRebate { get => _estateDutyRebate; set => SetProperty(ref _estateDutyRebate, value); }

    private string _estateDutyRate = "20";
    public string EstateDutyRate { get => _estateDutyRate; set => SetProperty(ref _estateDutyRate, value); }
    private string _sourceOfEstate = string.Empty;
    public string SourceOfEstate { get => _sourceOfEstate; set => SetProperty(ref _sourceOfEstate, value); }

    private string _estateBankName = string.Empty;
    public string EstateBankName { get => _estateBankName; set => SetProperty(ref _estateBankName, value); }

    private string _estateBankBranchCode = string.Empty;
    public string EstateBankBranchCode { get => _estateBankBranchCode; set => SetProperty(ref _estateBankBranchCode, value); }

    private string _estateBankAccountNumber = string.Empty;
    public string EstateBankAccountNumber { get => _estateBankAccountNumber; set => SetProperty(ref _estateBankAccountNumber, value); }

    private bool _hasMultipleLdAccounts;
    public bool HasMultipleLdAccounts { get => _hasMultipleLdAccounts; set => SetProperty(ref _hasMultipleLdAccounts, value); }

    private string _previousSpousesCount = "0";
    public string PreviousSpousesCount { get => _previousSpousesCount; set => SetProperty(ref _previousSpousesCount, value); }

    private string _previousSpouseRebateUsed = "0";
    public string PreviousSpouseRebateUsed { get => _previousSpouseRebateUsed; set => SetProperty(ref _previousSpouseRebateUsed, value); }

    private string _deceasedFirstName = string.Empty;
    public string DeceasedFirstName
    {
        get => _deceasedFirstName;
        set { if (SetProperty(ref _deceasedFirstName, value)) OnPropertyChanged(nameof(DeceasedFullName)); }
    }

    private string _deceasedMiddleName = string.Empty;
    public string DeceasedMiddleName
    {
        get => _deceasedMiddleName;
        set { if (SetProperty(ref _deceasedMiddleName, value)) OnPropertyChanged(nameof(DeceasedFullName)); }
    }

    private string _deceasedSurname = string.Empty;
    public string DeceasedSurname
    {
        get => _deceasedSurname;
        set { if (SetProperty(ref _deceasedSurname, value)) OnPropertyChanged(nameof(DeceasedFullName)); }
    }

    public string DeceasedFullName =>
        string.Join(" ",
            new[] { DeceasedFirstName, DeceasedMiddleName, DeceasedSurname }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim()));

    private string _deceasedPlaceOfDeath = string.Empty;
    public string DeceasedPlaceOfDeath { get => _deceasedPlaceOfDeath; set => SetProperty(ref _deceasedPlaceOfDeath, value); }

    private string _deceasedMatrimonialRegime = string.Empty;
    public string DeceasedMatrimonialRegime { get => _deceasedMatrimonialRegime; set => SetProperty(ref _deceasedMatrimonialRegime, value); }

    private string _deceasedSpouseFullName = string.Empty;
    public string DeceasedSpouseFullName { get => _deceasedSpouseFullName; set => SetProperty(ref _deceasedSpouseFullName, value); }

    private string _deceasedSpouseIdNumber = string.Empty;
    public string DeceasedSpouseIdNumber { get => _deceasedSpouseIdNumber; set => SetProperty(ref _deceasedSpouseIdNumber, value); }

    private DateTime? _deceasedWillDate;
    public DateTime? DeceasedWillDate { get => _deceasedWillDate; set => SetProperty(ref _deceasedWillDate, value); }

    private string _deceasedWillLocation = string.Empty;
    public string DeceasedWillLocation { get => _deceasedWillLocation; set => SetProperty(ref _deceasedWillLocation, value); }
    private string _deceasedGender = string.Empty;
    public string DeceasedGender { get => _deceasedGender; set => SetProperty(ref _deceasedGender, value); }

    private DateTime? _deceasedDateOfBirth;
    public DateTime? DeceasedDateOfBirth { get => _deceasedDateOfBirth; set => SetProperty(ref _deceasedDateOfBirth, value); }

    private bool _deceasedLivedInSouthAfrica = true;
    public bool DeceasedLivedInSouthAfrica { get => _deceasedLivedInSouthAfrica; set => SetProperty(ref _deceasedLivedInSouthAfrica, value); }

    private DateTime? _deceasedCodicilDate;
    public DateTime? DeceasedCodicilDate { get => _deceasedCodicilDate; set => SetProperty(ref _deceasedCodicilDate, value); }

    private string _deceasedIdNumber = string.Empty;
    public string DeceasedIdNumber
    {
        get => _deceasedIdNumber;
        set
        {
            if (SetProperty(ref _deceasedIdNumber, value))
                FillDeceasedDateOfBirthFromId();
        }
    }

    // The ID number holds the date of birth, so fill it in when it has not been entered yet.
    private void FillDeceasedDateOfBirthFromId()
    {
        if (DeceasedDateOfBirth is null &&
            SaIdNumber.TryGetDateOfBirth(DeceasedIdNumber, DateOfDeath, out var dob))
        {
            DeceasedDateOfBirth = dob;
        }
    }

    private string _deceasedMaritalStatus = string.Empty;
    public string DeceasedMaritalStatus
    {
        get => _deceasedMaritalStatus;
        set => SetProperty(ref _deceasedMaritalStatus, value);
    }

    private string _headerText = "New Estate";
    public string HeaderText
    {
        get => _headerText;
        set => SetProperty(ref _headerText, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsEditMode => _editingEstateId.HasValue;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public EstateFormViewModel(IApiClient apiClient, INavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        SaveCommand = new AsyncRelayCommand(async _ => await ExecuteSaveAsync());
        CancelCommand = new RelayCommand(_ => ExecuteCancel());
    }

    public void InitializeForCreate()
    {
        _editingEstateId = null;
        HeaderText = "New Estate";
        FileNumber = string.Empty;
        MastersReference = string.Empty;
        DateOfDeath = DateTime.Today;
        IsDutiable = false;
        EstateStatus = "Reported";
        FirmActsAsAgent = false;
        AgentName = string.Empty;
        AgentReference = string.Empty;
        AgentEmail = string.Empty;
        AgentPhone = string.Empty;
        AdministrationExpenses = "0";
        DateOfAccount = null;
        PlaceOfAccount = string.Empty;
        AccountType = "First and Final";
        IsTestate = true;
        MastersOffice = string.Empty;
        LettersOfExecutorshipDate = null;
        ExecutorFeePercentage = "3.5";
        IsVatRegistered = true;
        VatRate = "15";
        MastersFee = "0";
        EstateDutyRebate = "3500000";
        EstateDutyRate = "20";
        SourceOfEstate = string.Empty;
        EstateBankName = string.Empty;
        EstateBankBranchCode = string.Empty;
        EstateBankAccountNumber = string.Empty;
        HasMultipleLdAccounts = false;
        PreviousSpousesCount = "0";
        PreviousSpouseRebateUsed = "0";
        DeceasedFirstName = string.Empty;
        DeceasedMiddleName = string.Empty;
        DeceasedSurname = string.Empty;
        DeceasedPlaceOfDeath = string.Empty;
        DeceasedMatrimonialRegime = string.Empty;
        DeceasedSpouseFullName = string.Empty;
        DeceasedSpouseIdNumber = string.Empty;
        DeceasedWillDate = null;
        DeceasedWillLocation = string.Empty;
        DeceasedGender = string.Empty;
        DeceasedDateOfBirth = null;
        DeceasedLivedInSouthAfrica = true;
        DeceasedCodicilDate = null;
        DeceasedIdNumber = string.Empty;
        DeceasedMaritalStatus = string.Empty;
        StatusMessage = string.Empty;
    }

    public async Task InitializeForEditAsync(Guid estateId)
    {
        _editingEstateId = estateId;
        HeaderText = "Edit Estate";
        StatusMessage = "Loading...";

        var estate = await _apiClient.GetEstateAsync(estateId);
        if (estate is null)
        {
            StatusMessage = "Estate not found.";
            return;
        }

        FileNumber = estate.FileNumber;
        MastersReference = estate.MastersReference ?? string.Empty;
        DateOfDeath = estate.DateOfDeath;
        IsDutiable = estate.IsDutiable;
        EstateStatus = estate.EstateStatus;
        FirmActsAsAgent = estate.FirmActsAsAgent;
        AgentName = estate.AgentName ?? string.Empty;
        AgentReference = estate.AgentReference ?? string.Empty;
        AgentEmail = estate.AgentEmail ?? string.Empty;
        AgentPhone = estate.AgentPhone ?? string.Empty;
        AdministrationExpenses = estate.AdministrationExpenses.ToString("0.00");
        DateOfAccount = estate.DateOfAccount;
        PlaceOfAccount = estate.PlaceOfAccount ?? string.Empty;
        AccountType = estate.AccountType;
        IsTestate = estate.IsTestate;
        MastersOffice = estate.MastersOffice ?? string.Empty;
        LettersOfExecutorshipDate = estate.LettersOfExecutorshipDate;
        ExecutorFeePercentage = estate.ExecutorFeePercentage.ToString("0.##");
        IsVatRegistered = estate.IsVatRegistered;
        VatRate = estate.VatRate.ToString("0.##");
        MastersFee = estate.MastersFee.ToString("0.00");
        EstateDutyRebate = estate.EstateDutyRebate.ToString("0.00");
        EstateDutyRate = estate.EstateDutyRate.ToString("0.##");
        SourceOfEstate = estate.SourceOfEstate ?? string.Empty;
        EstateBankName = estate.EstateBankName ?? string.Empty;
        EstateBankBranchCode = estate.EstateBankBranchCode ?? string.Empty;
        EstateBankAccountNumber = estate.EstateBankAccountNumber ?? string.Empty;
        HasMultipleLdAccounts = estate.HasMultipleLdAccounts;
        PreviousSpousesCount = estate.PreviousSpousesCount.ToString();
        PreviousSpouseRebateUsed = estate.PreviousSpouseRebateUsed.ToString("0.00");
        DeceasedFirstName = estate.Deceased?.FirstName ?? string.Empty;
        DeceasedMiddleName = estate.Deceased?.MiddleName ?? string.Empty;
        DeceasedSurname = estate.Deceased?.Surname ?? string.Empty;

        // Older estates only have FullName — split it so the fields aren't blank.
        if (string.IsNullOrWhiteSpace(DeceasedFirstName) &&
            string.IsNullOrWhiteSpace(DeceasedSurname) &&
            !string.IsNullOrWhiteSpace(estate.Deceased?.FullName))
        {
            var parts = estate.Deceased!.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            DeceasedFirstName = parts[0];
            if (parts.Length > 1)
            {
                DeceasedSurname = parts[^1];
                DeceasedMiddleName = string.Join(" ", parts[1..^1]);
            }
        }

        DeceasedPlaceOfDeath = estate.Deceased?.PlaceOfDeath ?? string.Empty;
        DeceasedMatrimonialRegime = estate.Deceased?.MatrimonialRegime ?? string.Empty;
        DeceasedSpouseFullName = estate.Deceased?.SpouseFullName ?? string.Empty;
        DeceasedSpouseIdNumber = estate.Deceased?.SpouseIdNumber ?? string.Empty;
        DeceasedWillDate = estate.Deceased?.WillDate;
        DeceasedWillLocation = estate.Deceased?.WillLocation ?? string.Empty;
        DeceasedGender = estate.Deceased?.Gender ?? string.Empty;
        DeceasedDateOfBirth = estate.Deceased?.DateOfBirth;
        DeceasedLivedInSouthAfrica = estate.Deceased?.LivedInSouthAfrica ?? true;
        DeceasedCodicilDate = estate.Deceased?.CodicilDate;
        DeceasedIdNumber = estate.Deceased?.IdNumber ?? string.Empty;
        DeceasedMaritalStatus = estate.Deceased?.MaritalStatus ?? string.Empty;
        StatusMessage = string.Empty;
    }

    private async Task ExecuteSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FileNumber) || string.IsNullOrWhiteSpace(DeceasedFullName))
        {
            StatusMessage = "File number and deceased full name are required.";
            return;
        }

        var idError = SaIdNumber.Validate(DeceasedIdNumber, DeceasedDateOfBirth, "Deceased");
        if (idError is not null)
        {
            StatusMessage = idError;
            return;
        }

        Money.TryParse(
            AdministrationExpenses,
            out var administrationExpenses);
        Money.TryParse(ExecutorFeePercentage, out var executorFeePercentage);
        Money.TryParse(VatRate, out var vatRate);
        Money.TryParse(MastersFee, out var mastersFee);
        Money.TryParse(EstateDutyRebate, out var estateDutyRebate);
        Money.TryParse(EstateDutyRate, out var estateDutyRate);
        int.TryParse(PreviousSpousesCount, out var previousSpousesCount);
        Money.TryParse(PreviousSpouseRebateUsed, out var previousSpouseRebateUsed);

        try
        {
            if (IsEditMode)
            {
                var updateRequest = new UpdateEstateRequest
                {
                    FileNumber = FileNumber,
                    MastersReference = MastersReference,
                    DateOfDeath = DateOfDeath,
                    IsDutiable = IsDutiable,
                    FirmActsAsAgent = FirmActsAsAgent,
                    AgentName = AgentName,
                    AgentReference = AgentReference,
                    AgentEmail = AgentEmail,
                    AgentPhone = AgentPhone,
                    AdministrationExpenses = administrationExpenses,
                    DateOfAccount = DateOfAccount,
                    PlaceOfAccount = PlaceOfAccount,
                    AccountType = AccountType,
                    IsTestate = IsTestate,
                    MastersOffice = MastersOffice,
                    LettersOfExecutorshipDate = LettersOfExecutorshipDate,
                    ExecutorFeePercentage = executorFeePercentage,
                    IsVatRegistered = IsVatRegistered,
                    VatRate = vatRate,
                    MastersFee = mastersFee,
                    EstateDutyRebate = estateDutyRebate,
                    EstateDutyRate = estateDutyRate,
                    SourceOfEstate = SourceOfEstate,
                    EstateBankName = EstateBankName,
                    EstateBankBranchCode = EstateBankBranchCode,
                    EstateBankAccountNumber = EstateBankAccountNumber,
                    HasMultipleLdAccounts = HasMultipleLdAccounts,
                    PreviousSpousesCount = previousSpousesCount,
                    PreviousSpouseRebateUsed = previousSpouseRebateUsed,
                    EstateStatus = EstateStatus,
                    DeceasedFullName = DeceasedFullName,
                    DeceasedIdNumber = DeceasedIdNumber,
                    DeceasedMaritalStatus = DeceasedMaritalStatus,
                    DeceasedFirstName = DeceasedFirstName,
                    DeceasedMiddleName = DeceasedMiddleName,
                    DeceasedSurname = DeceasedSurname,
                    DeceasedPlaceOfDeath = DeceasedPlaceOfDeath,
                    DeceasedMatrimonialRegime = DeceasedMatrimonialRegime,
                    DeceasedSpouseFullName = DeceasedSpouseFullName,
                    DeceasedSpouseIdNumber = DeceasedSpouseIdNumber,
                    DeceasedWillDate = DeceasedWillDate,
                    DeceasedWillLocation = DeceasedWillLocation,
                    DeceasedGender = DeceasedGender,
                    DeceasedDateOfBirth = DeceasedDateOfBirth,
                    DeceasedLivedInSouthAfrica = DeceasedLivedInSouthAfrica,
                    DeceasedCodicilDate = DeceasedCodicilDate
                };
                await _apiClient.UpdateEstateAsync(_editingEstateId!.Value, updateRequest);
                _navigation.NavigateToEstateDetail(_editingEstateId.Value);
            }
            else
            {
                var createRequest = new CreateEstateRequest
                {
                    FileNumber = FileNumber,
                    MastersReference = MastersReference,
                    DateOfDeath = DateOfDeath,
                    IsDutiable = IsDutiable,
                    FirmActsAsAgent = FirmActsAsAgent,
                    AgentName = AgentName,
                    AgentReference = AgentReference,
                    AgentEmail = AgentEmail,
                    AgentPhone = AgentPhone,
                    AdministrationExpenses = administrationExpenses,
                    DateOfAccount = DateOfAccount,
                    PlaceOfAccount = PlaceOfAccount,
                    AccountType = AccountType,
                    IsTestate = IsTestate,
                    MastersOffice = MastersOffice,
                    LettersOfExecutorshipDate = LettersOfExecutorshipDate,
                    ExecutorFeePercentage = executorFeePercentage,
                    IsVatRegistered = IsVatRegistered,
                    VatRate = vatRate,
                    MastersFee = mastersFee,
                    EstateDutyRebate = estateDutyRebate,
                    EstateDutyRate = estateDutyRate,
                    SourceOfEstate = SourceOfEstate,
                    EstateBankName = EstateBankName,
                    EstateBankBranchCode = EstateBankBranchCode,
                    EstateBankAccountNumber = EstateBankAccountNumber,
                    HasMultipleLdAccounts = HasMultipleLdAccounts,
                    PreviousSpousesCount = previousSpousesCount,
                    PreviousSpouseRebateUsed = previousSpouseRebateUsed,
                    DeceasedFullName = DeceasedFullName,
                    DeceasedIdNumber = DeceasedIdNumber,
                    DeceasedMaritalStatus = DeceasedMaritalStatus,
                    DeceasedFirstName = DeceasedFirstName,
                    DeceasedMiddleName = DeceasedMiddleName,
                    DeceasedSurname = DeceasedSurname,
                    DeceasedPlaceOfDeath = DeceasedPlaceOfDeath,
                    DeceasedMatrimonialRegime = DeceasedMatrimonialRegime,
                    DeceasedSpouseFullName = DeceasedSpouseFullName,
                    DeceasedSpouseIdNumber = DeceasedSpouseIdNumber,
                    DeceasedWillDate = DeceasedWillDate,
                    DeceasedWillLocation = DeceasedWillLocation,
                    DeceasedGender = DeceasedGender,
                    DeceasedDateOfBirth = DeceasedDateOfBirth,
                    DeceasedLivedInSouthAfrica = DeceasedLivedInSouthAfrica,
                    DeceasedCodicilDate = DeceasedCodicilDate
                };
                var newEstateId = await _apiClient.CreateEstateAsync(createRequest);
                _navigation.NavigateToEstateDetail(newEstateId);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save estate: {ex.Message}";
        }
    }

    private void ExecuteCancel()
    {
        if (IsEditMode)
            _navigation.NavigateToEstateDetail(_editingEstateId!.Value);
        else
            _navigation.NavigateToDashboard();
    }
}