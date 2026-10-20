using System;
using System.Windows.Input;
using EstateFlow.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EstateFlow.Desktop.Mvvm;

public class ShellViewModel : BaseViewModel, INavigationService, IShellState
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthState _authState;

    // ==========================================
    // CURRENT VIEW MODEL
    // ==========================================

    private object? _currentViewModel;

    public object? CurrentViewModel
    {
        get => _currentViewModel;

        set
        {
            if (SetProperty(ref _currentViewModel, value))
            {
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }
    }

    // ==========================================
    // LOGIN STATE
    // ==========================================

    public bool IsLoggedIn =>
        CurrentViewModel is not null &&
        CurrentViewModel is not LoginViewModel;

    // ==========================================
    // CURRENT SCREEN
    // ==========================================

    private string _currentScreen = string.Empty;

    public string CurrentScreen
    {
        get => _currentScreen;
        set => SetProperty(ref _currentScreen, value);
    }

    // ==========================================
    // LAST OPENED ESTATE
    // ==========================================

    public Guid? LastOpenedEstateId { get; private set; }

    // ==========================================
    // COMMANDS
    // ==========================================

    public ICommand NavigateCommand { get; }

    public ICommand LogoutCommand { get; }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public ShellViewModel(
        IServiceProvider serviceProvider,
        IAuthState authState)
    {
        _serviceProvider = serviceProvider;
        _authState = authState;

        NavigateCommand =
            new RelayCommand(ExecuteNavigate);

        LogoutCommand =
            new RelayCommand(_ => ExecuteLogout());

        // IMPORTANT:
        // DO NOT call NavigateToLogin() here.
        //
        // App.xaml.cs performs initial navigation
        // after ShellViewModel is fully constructed.
    }

    // ==========================================
    // LOGOUT
    // ==========================================

    private void ExecuteLogout()
    {
        _authState.Token = null;
        _authState.FullName = null;
        _authState.Role = null;

        NavigateToLogin();
    }

    // ==========================================
    // NAVIGATION COMMAND
    // ==========================================

    private void ExecuteNavigate(object? parameter)
    {
        var screen =
            parameter as string ?? string.Empty;

        switch (screen)
        {
            case "dashboard":
                NavigateToDashboard();
                break;

            case "estates":
                NavigateToEstates();
                break;

            case "diary":
                NavigateToDiary();
                break;

            case "reports":
                NavigateToReports();
                break;

            case "document-scan":
                NavigateToDocumentScan();
                break;

            case "new-estate":
                NavigateToNewEstate();
                break;
        }
    }

    // ==========================================
    // LOGIN
    // ==========================================

    public void NavigateToLogin()
    {
        CurrentScreen = string.Empty;

        CurrentViewModel =
            _serviceProvider
                .GetRequiredService<LoginViewModel>();
    }

    // ==========================================
    // DASHBOARD
    // ==========================================

    public string UserName =>
        string.IsNullOrWhiteSpace(_authState.FullName)
            ? "Signed in"
            : _authState.FullName!;

    public string UserInitials
    {
        get
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "?"
                : parts.Length == 1 ? parts[0][..1].ToUpperInvariant()
                : (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }

    public void NavigateToEstates()
    {
        CurrentScreen = "estates";

        CurrentViewModel =
            _serviceProvider
                .GetRequiredService<EstatesViewModel>();
    }

    public void NavigateToDashboard()
    {
        CurrentScreen = "dashboard";
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(UserInitials));

        CurrentViewModel =
            _serviceProvider
                .GetRequiredService<DashboardViewModel>();
    }

    // ==========================================
    // ESTATE DETAIL
    // ==========================================

    public void NavigateToEstateDetail(Guid estateId)
    {
        CurrentScreen = "estate-detail";

        LastOpenedEstateId = estateId;

        var detailViewModel =
            _serviceProvider
                .GetRequiredService<EstateDetailViewModel>();

        CurrentViewModel = detailViewModel;

        _ = detailViewModel.LoadAsync(estateId);
    }

    // ==========================================
    // DIARY
    // ==========================================

    public void NavigateToDiary()
    {
        CurrentScreen = "diary";

        var diaryViewModel =
            _serviceProvider
                .GetRequiredService<DiaryViewModel>();

        CurrentViewModel = diaryViewModel;

        _ = diaryViewModel.LoadAsync();
    }

    // ==========================================
    // REPORTS
    // ==========================================

    public void NavigateToReports()
    {
        CurrentScreen = "reports";

        var reportsViewModel =
            _serviceProvider
                .GetRequiredService<ReportsViewModel>();

        CurrentViewModel = reportsViewModel;

        _ = reportsViewModel.LoadAsync();
    }

    // ==========================================
    // DOCUMENT SCAN
    // ==========================================

    public void NavigateToDocumentScan()
    {
        CurrentScreen = "document-scan";

        var docViewModel =
            _serviceProvider
                .GetRequiredService<DocumentScanViewModel>();

        CurrentViewModel = docViewModel;

        if (LastOpenedEstateId.HasValue)
        {
            _ = docViewModel.LoadAsync(
                LastOpenedEstateId.Value);
        }
        else
        {
            docViewModel.ShowNoEstateSelected();
        }
    }

    // ==========================================
    // CREATE NEW ESTATE
    // ==========================================

    public void NavigateToNewEstate()
    {
        CurrentScreen = "new-estate";

        var formViewModel =
            _serviceProvider
                .GetRequiredService<EstateFormViewModel>();

        formViewModel.InitializeForCreate();

        CurrentViewModel = formViewModel;
    }

    // ==========================================
    // EDIT ESTATE
    // ==========================================

    public void NavigateToEditEstate(Guid estateId)
    {
        CurrentScreen = "edit-estate";

        LastOpenedEstateId = estateId;

        var formViewModel =
            _serviceProvider
                .GetRequiredService<EstateFormViewModel>();

        CurrentViewModel = formViewModel;

        _ = formViewModel.InitializeForEditAsync(estateId);
    }
}