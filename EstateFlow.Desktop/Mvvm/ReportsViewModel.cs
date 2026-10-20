using System.Collections.ObjectModel;
using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class ReportsViewModel : BaseViewModel
{
    public int ActiveCount => Settings.Count(s => !s.IsPaused);
    public int PausedCount => Settings.Count(s => s.IsPaused);
    private readonly IApiClient _apiClient;

    public ObservableCollection<ReportSettingDto> Settings { get; } = new();

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ToggleCommand { get; }

    public ReportsViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        ToggleCommand = new AsyncRelayCommand(async param => await ExecuteToggleAsync(param));
    }

    public async Task LoadAsync()
    {
        StatusMessage = "Loading report settings...";
        try
        {
            var settings = await _apiClient.GetReportSettingsAsync();
            Settings.Clear();
            foreach (var s in settings)
                Settings.Add(s);

            StatusMessage = Settings.Count == 0 ? "No estates found." : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load report settings: {ex.Message}";
        }
    }

    private async Task ExecuteToggleAsync(object? parameter)
    {
        if (parameter is not ReportSettingDto setting) return;

        var newValue = !setting.IsPaused;
        try
        {
            await _apiClient.ToggleReportPauseAsync(setting.EstateId, newValue);
            setting.IsPaused = newValue;
            // Force the ItemsControl to re-render this row
            var index = Settings.IndexOf(setting);
            if (index >= 0)
            {
                Settings.RemoveAt(index);
                Settings.Insert(index, setting);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not update report setting: {ex.Message}";
        }
    }
}