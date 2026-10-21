using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using EstateFlow.Core.Dtos;
using Microsoft.Win32;

namespace EstateFlow.Desktop.Mvvm;

// One box on a Master's Office form, as shown in the editor.
public class FormFieldItem : BaseViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = "Text";
    public bool Multiline { get; init; }
    public int Page { get; init; }
    public int MaxLength { get; init; }
    public bool AutoFilled { get; init; }

    // What the estate records suggested (empty when nothing was suggested).
    public string Original { get; init; } = string.Empty;

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(IsEdited));
                OnPropertyChanged(nameof(IsChecked));
                OnPropertyChanged(nameof(Hint));
            }
        }
    }

    private bool _showPageHeader;
    public bool ShowPageHeader
    {
        get => _showPageHeader;
        set => SetProperty(ref _showPageHeader, value);
    }

    public bool IsCheckbox => Type == "Checkbox";
    public bool IsText => !IsCheckbox;
    public bool IsMultiline => Multiline && !IsCheckbox;
    public string PageText => $"PAGE {Page}";

    public bool IsChecked
    {
        get => Value.Trim().ToLowerInvariant() is "true" or "yes" or "1" or "x" or "on";
        set => Value = value ? "true" : "false";
    }

    public bool IsEdited => !string.Equals(Value, Original, StringComparison.Ordinal);

    public string Hint =>
        AutoFilled && !IsEdited ? "from estate"
        : IsEdited ? "edited"
        : string.Empty;
}

// One heading on the forms list, for example "Deceased Estates".
public class FormGroupItem : BaseViewModel
{
    public string Heading { get; init; } = string.Empty;
    public ObservableCollection<FormSummaryDto> Forms { get; } = new();

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}

// Estate Documents: pick a Master's Office form, review what was filled in from the estate,
// change anything, then save it to the estate, open it or download it.
public partial class DocumentScanViewModel
{
    private List<FormGroupDto> _catalog = new();
    private int _formLoadVersion;

    public ObservableCollection<FormGroupItem> FormGroups { get; } = new();
    public ObservableCollection<FormFieldItem> FormFields { get; } = new();
    public ObservableCollection<FormFieldItem> VisibleFormFields { get; } = new();

    // ─── mode switch ───

    private bool _isEstateDocumentsMode;
    public bool IsEstateDocumentsMode
    {
        get => _isEstateDocumentsMode;
        set
        {
            if (!SetProperty(ref _isEstateDocumentsMode, value))
                return;

            OnPropertyChanged(nameof(IsScannedMode));

            if (value)
                _ = EnsureFormCatalogAsync();
        }
    }

    public bool IsScannedMode => !IsEstateDocumentsMode;

    // ─── list of forms ───

    private string _formSearch = string.Empty;
    public string FormSearch
    {
        get => _formSearch;
        set
        {
            if (SetProperty(ref _formSearch, value))
                BuildFormGroups();
        }
    }

    private FormSummaryDto? _selectedForm;
    public FormSummaryDto? SelectedForm
    {
        get => _selectedForm;
        private set
        {
            if (!SetProperty(ref _selectedForm, value))
                return;

            OnPropertyChanged(nameof(HasSelectedForm));
            OnPropertyChanged(nameof(SelectedFormTitle));
        }
    }

    public bool HasSelectedForm => SelectedForm is not null;
    public string SelectedFormTitle => SelectedForm?.DisplayName ?? string.Empty;

    // The list folds away once a form is chosen, so the form itself is right below.
    private bool _isFormPickerOpen = true;
    public bool IsFormPickerOpen
    {
        get => _isFormPickerOpen;
        private set
        {
            if (SetProperty(ref _isFormPickerOpen, value))
                OnPropertyChanged(nameof(IsFormPickerClosed));
        }
    }

    public bool IsFormPickerClosed => !IsFormPickerOpen;

    // ─── the form being edited ───

    private bool _canFillSelectedForm;
    public bool CanFillSelectedForm
    {
        get => _canFillSelectedForm;
        private set => SetProperty(ref _canFillSelectedForm, value);
    }

    private string _formMessage = string.Empty;
    public string FormMessage
    {
        get => _formMessage;
        private set => SetProperty(ref _formMessage, value);
    }

    private bool _isFormBusy;
    public bool IsFormBusy
    {
        get => _isFormBusy;
        private set => SetProperty(ref _isFormBusy, value);
    }

    private bool _showEmptyFields = true;
    public bool ShowEmptyFields
    {
        get => _showEmptyFields;
        set
        {
            if (SetProperty(ref _showEmptyFields, value))
                ApplyFieldFilter();
        }
    }

    // ─── commands ───

    public ICommand ShowScannedDocumentsCommand { get; private set; } = null!;
    public ICommand ShowEstateDocumentsCommand { get; private set; } = null!;
    public ICommand SelectFormCommand { get; private set; } = null!;
    public ICommand ChangeFormCommand { get; private set; } = null!;
    public ICommand ResetFormCommand { get; private set; } = null!;
    public ICommand OpenFilledFormCommand { get; private set; } = null!;
    public ICommand SaveFormToEstateCommand { get; private set; } = null!;
    public ICommand DownloadFilledFormCommand { get; private set; } = null!;
    public ICommand OpenBlankFormCommand { get; private set; } = null!;
    public ICommand DownloadBlankFormCommand { get; private set; } = null!;

    private void InitFormCommands()
    {
        ShowScannedDocumentsCommand = new RelayCommand(_ => IsEstateDocumentsMode = false);
        ShowEstateDocumentsCommand = new RelayCommand(_ => IsEstateDocumentsMode = true);

        SelectFormCommand = new AsyncRelayCommand(p => SelectFormAsync(p as FormSummaryDto));
        ChangeFormCommand = new RelayCommand(_ => IsFormPickerOpen = true);
        ResetFormCommand = new RelayCommand(_ => ResetFormValues());

        OpenFilledFormCommand = new AsyncRelayCommand(_ => OpenFilledFormAsync());
        SaveFormToEstateCommand = new AsyncRelayCommand(_ => SaveFormToEstateAsync());
        DownloadFilledFormCommand = new AsyncRelayCommand(_ => DownloadFilledFormAsync());
        OpenBlankFormCommand = new AsyncRelayCommand(_ => OpenBlankFormAsync());
        DownloadBlankFormCommand = new AsyncRelayCommand(_ => DownloadBlankFormAsync());
    }

    // The estate selector changed: refill the open form from the newly chosen estate.
    private void EstateChangedForForms()
    {
        if (IsEstateDocumentsMode && SelectedForm is not null)
            _ = LoadSelectedFormAsync();
    }

    // ─── catalog ───

    private async Task EnsureFormCatalogAsync()
    {
        if (_catalog.Count > 0)
            return;

        try
        {
            FormMessage = "Loading forms...";
            _catalog = await _apiClient.GetFormCatalogAsync();
            BuildFormGroups();
            FormMessage = string.Empty;
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not load the forms: {ex.Message}";
        }
    }

    private void BuildFormGroups()
    {
        var search = FormSearch.Trim();
        var searching = search.Length > 0;

        // remember which headings are open
        var open = FormGroups.Where(g => g.IsExpanded).Select(g => g.Heading).ToHashSet();

        FormGroups.Clear();

        foreach (var group in _catalog)
        {
            var forms = group.Forms
                .Where(f => !searching ||
                            f.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            f.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (forms.Count == 0)
                continue;

            var item = new FormGroupItem
            {
                Heading = $"{group.Heading}  ({forms.Count})",
                IsExpanded = searching || open.Contains($"{group.Heading}  ({forms.Count})") ||
                             (FormGroups.Count == 0 && open.Count == 0)
            };

            foreach (var form in forms)
                item.Forms.Add(form);

            FormGroups.Add(item);
        }
    }

    // ─── picking and loading a form ───

    private async Task SelectFormAsync(FormSummaryDto? form)
    {
        if (form is null)
            return;

        SelectedForm = form;
        IsFormPickerOpen = false;
        await LoadSelectedFormAsync();
    }

    private async Task LoadSelectedFormAsync()
    {
        var version = ++_formLoadVersion;

        FormFields.Clear();
        VisibleFormFields.Clear();
        CanFillSelectedForm = false;

        var form = SelectedForm;
        if (form is null)
            return;

        if (!form.CanFill)
        {
            FormMessage = form.Note ?? "This form cannot be filled in here.";
            return;
        }

        if (!_currentEstateId.HasValue)
        {
            FormMessage = "Choose an estate above. The form is filled in from that estate's details.";
            return;
        }

        try
        {
            IsFormBusy = true;
            FormMessage = "Filling the form in from the estate...";

            var loaded = await _apiClient.GetEstateFormAsync(_currentEstateId.Value, form.Id);

            if (version != _formLoadVersion)
                return;

            foreach (var field in loaded.Fields)
            {
                FormFields.Add(new FormFieldItem
                {
                    Id = field.Id,
                    Label = field.Label,
                    Type = field.Type,
                    Multiline = field.Multiline,
                    Page = field.Page,
                    MaxLength = field.MaxLength,
                    AutoFilled = field.AutoFilled,
                    Original = field.Value,
                    Value = field.Value
                });
            }

            CanFillSelectedForm = loaded.CanFill;
            ApplyFieldFilter();

            var filled = loaded.Fields.Count(f => f.AutoFilled);

            FormMessage = loaded.CanFill
                ? $"{filled} of {loaded.Fields.Count} boxes were filled in from the estate. " +
                  "Check every box, change anything that is wrong and complete the rest, then save."
                : loaded.Message ?? string.Empty;
        }
        catch (Exception ex)
        {
            if (version == _formLoadVersion)
                FormMessage = $"Could not load the form: {ex.Message}";
        }
        finally
        {
            if (version == _formLoadVersion)
                IsFormBusy = false;
        }
    }

    private void ApplyFieldFilter()
    {
        VisibleFormFields.Clear();

        var lastPage = 0;

        foreach (var field in FormFields)
        {
            if (!ShowEmptyFields && string.IsNullOrWhiteSpace(field.Value))
                continue;

            field.ShowPageHeader = field.Page != lastPage;
            lastPage = field.Page;

            VisibleFormFields.Add(field);
        }
    }

    private void ResetFormValues()
    {
        foreach (var field in FormFields)
            field.Value = field.Original;

        FormMessage = "The boxes were put back to what the estate records suggested.";
    }

    // ─── output ───

    private Dictionary<string, string> CurrentFormValues() =>
        FormFields
            .Where(f => !string.IsNullOrWhiteSpace(f.Value))
            .ToDictionary(f => f.Id, f => f.Value, StringComparer.Ordinal);

    private async Task<string?> BuildFilledFormAsync()
    {
        if (SelectedForm is null || !CanFillSelectedForm)
        {
            FormMessage = "Choose a form that can be filled in first.";
            return null;
        }

        if (!_currentEstateId.HasValue)
        {
            FormMessage = "Choose an estate first.";
            return null;
        }

        return await _apiClient.GenerateEstateFormToTempFileAsync(
            _currentEstateId.Value,
            SelectedForm.Id,
            CurrentFormValues());
    }

    private static void OpenFile(string path) =>
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });

    private async Task OpenFilledFormAsync()
    {
        try
        {
            IsFormBusy = true;
            FormMessage = "Creating the PDF...";

            var path = await BuildFilledFormAsync();
            if (path is null)
                return;

            OpenFile(path);
            FormMessage = "The form was opened. Use Save to estate or Download to keep it.";
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not open the form: {ex.Message}";
        }
        finally
        {
            IsFormBusy = false;
        }
    }

    // Keeps the finished form with the estate, in its document registry.
    private async Task SaveFormToEstateAsync()
    {
        try
        {
            IsFormBusy = true;
            FormMessage = "Saving the form to the estate...";

            var path = await BuildFilledFormAsync();
            if (path is null || SelectedForm is null || !_currentEstateId.HasValue)
                return;

            var type = string.IsNullOrWhiteSpace(SelectedForm.Code)
                ? SelectedForm.Title
                : $"{SelectedForm.Code} {SelectedForm.Title}";

            if (type.Length > 70)
                type = type[..70].TrimEnd();

            await _apiClient.UploadDocumentAsync(_currentEstateId.Value, path, type);
            await LoadDocumentsAsync();

            FormMessage = "Saved to this estate's documents. You can open it again from the scanned documents list.";
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not save the form: {ex.Message}";
        }
        finally
        {
            IsFormBusy = false;
        }
    }

    private async Task DownloadFilledFormAsync()
    {
        try
        {
            IsFormBusy = true;
            FormMessage = "Creating the PDF...";

            var path = await BuildFilledFormAsync();
            if (path is null)
                return;

            var saved = AskWhereToSave(Path.GetFileName(path));
            if (saved is null)
            {
                FormMessage = "Download cancelled.";
                return;
            }

            File.Copy(path, saved, true);
            FormMessage = $"Saved to {saved}";
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not download the form: {ex.Message}";
        }
        finally
        {
            IsFormBusy = false;
        }
    }

    private async Task OpenBlankFormAsync()
    {
        if (SelectedForm is null)
            return;

        try
        {
            IsFormBusy = true;
            OpenFile(await _apiClient.DownloadBlankFormToTempFileAsync(SelectedForm.Id));
            FormMessage = "The blank form was opened.";
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not open the blank form: {ex.Message}";
        }
        finally
        {
            IsFormBusy = false;
        }
    }

    private async Task DownloadBlankFormAsync()
    {
        if (SelectedForm is null)
            return;

        try
        {
            IsFormBusy = true;

            var path = await _apiClient.DownloadBlankFormToTempFileAsync(SelectedForm.Id);
            var saved = AskWhereToSave(Path.GetFileName(path));

            if (saved is null)
                return;

            File.Copy(path, saved, true);
            FormMessage = $"Saved to {saved}";
        }
        catch (Exception ex)
        {
            FormMessage = $"Could not download the blank form: {ex.Message}";
        }
        finally
        {
            IsFormBusy = false;
        }
    }

    private static string? AskWhereToSave(string suggestedName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save form",
            FileName = suggestedName,
            DefaultExt = ".pdf",
            Filter = "PDF document (*.pdf)|*.pdf"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
