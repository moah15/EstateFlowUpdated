using EstateFlow.Desktop.Services;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Input;

namespace EstateFlow.Desktop.Mvvm;

public class LoginViewModel : BaseViewModel
{
    private readonly INavigationService _navigation;
    private readonly IApiClient _apiClient;
    private readonly IAuthState _authState;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _requiresTwoFactor;
    public bool RequiresTwoFactor
    {
        get => _requiresTwoFactor;
        set => SetProperty(ref _requiresTwoFactor, value);
    }

    private string _twoFactorCode = string.Empty;
    public string TwoFactorCode
    {
        get => _twoFactorCode;
        set => SetProperty(ref _twoFactorCode, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand VerifyTwoFactorCommand { get; }

    public LoginViewModel(INavigationService navigation, IApiClient apiClient, IAuthState authState)
    {
        _navigation = navigation;
        _apiClient = apiClient;
        _authState = authState;

        LoginCommand = new AsyncRelayCommand(async param => await ExecuteLoginAsync(param));
        VerifyTwoFactorCommand = new AsyncRelayCommand(async _ => await ExecuteVerifyTwoFactorAsync());
    }

    private async Task ExecuteLoginAsync(object? parameter)
    {
        var passwordBox = parameter as PasswordBox;
        var password = passwordBox?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter both an email address and password.";
            return;
        }

        try
        {
            var result = await _apiClient.LoginAsync(Email, password);

            if (result.RequiresTwoFactor)
            {
                RequiresTwoFactor = true;
                ErrorMessage = string.Empty;
                return;
            }

            ErrorMessage = string.Empty;
            _authState.Token = result.Token;
            _authState.FullName = result.FullName;
            _authState.Role = result.Role;
            _navigation.NavigateToDashboard();
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Invalid email or password.";
        }
    }

    private async Task ExecuteVerifyTwoFactorAsync()
    {
        if (string.IsNullOrWhiteSpace(TwoFactorCode))
        {
            ErrorMessage = "Please enter your authentication code.";
            return;
        }

        try
        {
            var result = await _apiClient.VerifyTwoFactorAsync(Email, TwoFactorCode);
            ErrorMessage = string.Empty;
            _authState.Token = result.Token;
            _authState.FullName = result.FullName;
            _authState.Role = result.Role;
            _navigation.NavigateToDashboard();
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Invalid authentication code.";
        }
    }
}