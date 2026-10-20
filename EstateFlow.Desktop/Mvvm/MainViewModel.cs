using System.Collections.ObjectModel;
using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class MainViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    public ObservableCollection<EstateSummaryDto> Estates { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand LoadEstatesCommand { get; }

    public MainViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadEstatesCommand = new AsyncRelayCommand(async _ => await LoadEstatesAsync());
    }

    private async Task LoadEstatesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading estates...";
        try
        {
            var estates = await _apiClient.GetEstatesAsync();
            Estates.Clear();
            foreach (var estate in estates)
                Estates.Add(estate);

            StatusMessage = $"Loaded {Estates.Count} estate(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}