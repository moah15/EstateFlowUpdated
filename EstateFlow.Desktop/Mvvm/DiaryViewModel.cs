using System.Collections.ObjectModel;
using System.Net.Mail;
using EstateFlow.Core.Dtos;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class DiaryViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    public ObservableCollection<DiaryEntryWithEstateDto>
        Entries { get; } = new();

    public ObservableCollection<DiaryRecipientOption>
        RecipientOptions { get; } = new();

    private DiaryEntryWithEstateDto? _selectedEntry;
    public DiaryEntryWithEstateDto? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (!SetProperty(
                    ref _selectedEntry,
                    value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(PreviewEstateName));
            OnPropertyChanged(nameof(PreviewFileNumber));
            OnPropertyChanged(nameof(PreviewDueDate));
            OnPropertyChanged(nameof(PreviewStatus));
            OnPropertyChanged(nameof(PreviewBody));
            OnPropertyChanged(nameof(PreviewNotificationMethod));

            _ = LoadSelectedEntryAsync();
        }
    }

    private DiaryRecipientOption? _selectedRecipient;
    public DiaryRecipientOption? SelectedRecipient
    {
        get => _selectedRecipient;
        set
        {
            if (!SetProperty(
                    ref _selectedRecipient,
                    value))
            {
                return;
            }

            if (value is null)
                return;

            RecipientName = value.Name;
            RecipientEmail = value.Email;
        }
    }

    private string _recipientName = string.Empty;
    public string RecipientName
    {
        get => _recipientName;
        set => SetProperty(
            ref _recipientName,
            value);
    }

    private string _recipientEmail = string.Empty;
    public string RecipientEmail
    {
        get => _recipientEmail;
        set => SetProperty(
            ref _recipientEmail,
            value);
    }

    private string _emailSubject = string.Empty;
    public string EmailSubject
    {
        get => _emailSubject;
        set => SetProperty(
            ref _emailSubject,
            value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(
            ref _statusMessage,
            value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(
            ref _isBusy,
            value);
    }

    public bool HasSelection =>
        SelectedEntry is not null;

    public string PreviewEstateName =>
        string.IsNullOrWhiteSpace(
            SelectedEntry?.DeceasedFullName)
            ? "Estate matter"
            : $"Estate Late {SelectedEntry.DeceasedFullName}";

    public string PreviewFileNumber =>
        SelectedEntry?.FileNumber
        ?? "—";

    public string PreviewDueDate =>
        SelectedEntry is null
            ? "—"
            : SelectedEntry.DueDate
                .ToString("dd MMMM yyyy");

    public string PreviewStatus =>
        SelectedEntry is null
            ? "—"
            : SelectedEntry.IsCompleted
                ? "Completed"
                : SelectedEntry.Urgency switch
                {
                    "overdue" => "Overdue",
                    "due-soon" => "Due soon",
                    _ => "Open"
                };

    public string PreviewNotificationMethod =>
        string.IsNullOrWhiteSpace(
            SelectedEntry?.NotifyBy)
            ? "—"
            : SelectedEntry.NotifyBy;

    public string PreviewBody =>
        string.IsNullOrWhiteSpace(
            SelectedEntry?.Note)
            ? "Select a diary entry to preview its correspondence."
            : SelectedEntry.Note;

    public AsyncRelayCommand SendEmailCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }

    public DiaryViewModel(
        IApiClient apiClient)
    {
        _apiClient = apiClient;

        SendEmailCommand =
            new AsyncRelayCommand(
                _ => SendEmailAsync());

        RefreshCommand =
            new AsyncRelayCommand(
                _ => LoadAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage =
            "Loading diary...";

        try
        {
            var selectedDiaryId =
                SelectedEntry?.DiaryId;

            var entries =
                await _apiClient
                    .GetAllDiaryEntriesAsync();

            Entries.Clear();

            foreach (var entry in entries)
                Entries.Add(entry);

            if (Entries.Count == 0)
            {
                SelectedEntry = null;

                RecipientOptions.Clear();
                SelectedRecipient = null;
                RecipientName = string.Empty;
                RecipientEmail = string.Empty;
                EmailSubject = string.Empty;

                StatusMessage =
                    "No diary entries.";
            }
            else
            {
                SelectedEntry =
                    selectedDiaryId.HasValue
                        ? Entries.FirstOrDefault(
                              x =>
                                  x.DiaryId ==
                                  selectedDiaryId.Value)
                          ?? Entries[0]
                        : Entries[0];

                StatusMessage =
                    string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not load diary: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSelectedEntryAsync()
    {
        RecipientOptions.Clear();
        SelectedRecipient = null;
        RecipientName = string.Empty;
        RecipientEmail = string.Empty;

        if (SelectedEntry is null)
        {
            EmailSubject = string.Empty;
            return;
        }

        EmailSubject =
            $"Estate Diary Update — File {SelectedEntry.FileNumber}";

        try
        {
            var estate =
                await _apiClient.GetEstateAsync(
                    SelectedEntry.EstateId);

            if (estate is null)
            {
                StatusMessage =
                    "The selected estate could not be loaded.";
                return;
            }

            AddRecipient(
                estate.Controller?.StaffName,
                estate.Controller?.Email,
                "Estate Controller");

            foreach (var executor
                     in estate.Executors)
            {
                AddRecipient(
                    executor.FullName,
                    executor.Email,
                    "Executor");
            }

            foreach (var beneficiary
                     in estate.Beneficiaries)
            {
                AddRecipient(
                    beneficiary.FullName,
                    beneficiary.Email,
                    "Beneficiary / Heir");
            }

            SelectedRecipient =
                RecipientOptions.FirstOrDefault();

            if (RecipientOptions.Count == 0)
            {
                StatusMessage =
                    "No saved email recipients for this estate. " +
                    "You can type any recipient name and email address manually.";
            }
            else
            {
                StatusMessage =
                    "Choose a saved recipient or type a different email address.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage =
                "Could not load saved recipients. " +
                $"You can still type an email manually. {ex.Message}";
        }
    }

    private void AddRecipient(
        string? name,
        string? email,
        string role)
    {
        if (string.IsNullOrWhiteSpace(
                email))
        {
            return;
        }

        var cleanEmail =
            email.Trim();

        if (RecipientOptions.Any(
                x => string.Equals(
                    x.Email,
                    cleanEmail,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        RecipientOptions.Add(
            new DiaryRecipientOption
            {
                Name =
                    string.IsNullOrWhiteSpace(name)
                        ? cleanEmail
                        : name.Trim(),
                Email = cleanEmail,
                Role = role
            });
    }

    private async Task SendEmailAsync()
    {
        if (SelectedEntry is null)
        {
            StatusMessage =
                "Select a diary entry first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(
                RecipientEmail))
        {
            StatusMessage =
                "Enter a recipient email address.";
            return;
        }

        if (!IsValidEmail(
                RecipientEmail))
        {
            StatusMessage =
                "Enter a valid recipient email address.";
            return;
        }

        if (string.IsNullOrWhiteSpace(
                EmailSubject))
        {
            StatusMessage =
                "Enter an email subject.";
            return;
        }

        IsBusy = true;
        StatusMessage =
            "Sending diary email...";

        try
        {
            await _apiClient.SendDiaryEmailAsync(
                SelectedEntry.EstateId,
                SelectedEntry.DiaryId,
                new SendDiaryEmailRequest
                {
                    RecipientName =
                        RecipientName.Trim(),
                    RecipientEmail =
                        RecipientEmail.Trim(),
                    Subject =
                        EmailSubject.Trim()
                });

            StatusMessage =
                $"Diary email sent to {RecipientEmail.Trim()}.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not send diary email: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool IsValidEmail(
        string email)
    {
        try
        {
            _ =
                new MailAddress(
                    email.Trim());

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class DiaryRecipientOption
{
    public string Name { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string Role { get; set; } =
        string.Empty;

    public string DisplayText =>
        $"{Name} — {Role} — {Email}";
}
