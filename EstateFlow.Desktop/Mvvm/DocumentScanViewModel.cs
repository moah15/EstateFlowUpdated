using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using EstateFlow.Core.Dtos;
using EstateFlow.Desktop.Services;
using Microsoft.Win32;

namespace EstateFlow.Desktop.Mvvm;

public class DocumentScanViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    private Guid? _currentEstateId;
    private string? _selectedFilePath;
    private bool _suppressEstateSelectionLoad;

    public ObservableCollection<EstateSummaryDto> Estates { get; } = new();
    public ObservableCollection<ScannedDocumentDto> Documents { get; } = new();

    private EstateSummaryDto? _selectedEstate;
    public EstateSummaryDto? SelectedEstate
    {
        get => _selectedEstate;
        set
        {
            if (!SetProperty(ref _selectedEstate, value))
                return;

            _currentEstateId = value?.EstateId;

            _selectedFilePath = null;
            SelectedFileName = "No file selected";

            OnPropertyChanged(nameof(CanUpload));

            if (!_suppressEstateSelectionLoad)
            {
                _ = LoadDocumentsForSelectedEstateAsync();
            }
        }
    }

    private string _headerText = "Document Registry";
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

    private string _newDocumentType = "Document";
    public string NewDocumentType
    {
        get => _newDocumentType;
        set
        {
            if (SetProperty(ref _newDocumentType, value))
            {
                OnPropertyChanged(nameof(CanUpload));
            }
        }
    }

    private string _selectedFileName = "No file selected";
    public string SelectedFileName
    {
        get => _selectedFileName;
        set => SetProperty(ref _selectedFileName, value);
    }

    private bool _isUploading;
    public bool IsUploading
    {
        get => _isUploading;
        set
        {
            if (SetProperty(ref _isUploading, value))
            {
                OnPropertyChanged(nameof(CanUpload));
            }
        }
    }

    public bool CanUpload =>
        !IsUploading &&
        _currentEstateId.HasValue &&
        !string.IsNullOrWhiteSpace(_selectedFilePath) &&
        !string.IsNullOrWhiteSpace(NewDocumentType);

    public ICommand ChooseFileCommand { get; }
    public ICommand UploadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ViewDocumentCommand { get; }
    public ICommand DeleteDocumentCommand { get; }

    public DocumentScanViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;

        ChooseFileCommand = new RelayCommand(_ => ChooseFile());
        UploadCommand = new AsyncRelayCommand(UploadAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        ViewDocumentCommand = new AsyncRelayCommand(ViewDocumentAsync);
        DeleteDocumentCommand = new AsyncRelayCommand(DeleteDocumentAsync);

        // The document screen can now be opened directly from the sidebar.
        // It loads the available estates so the user can choose one.
        _ = LoadEstateChoicesAsync(null);
    }

    public void ShowNoEstateSelected()
    {
        _currentEstateId = null;
        _selectedFilePath = null;

        Documents.Clear();

        HeaderText = "Document Registry";
        SelectedFileName = "No file selected";
        NewDocumentType = "Document";

        StatusMessage = "Choose an estate below to view or upload documents.";

        OnPropertyChanged(nameof(CanUpload));

        _ = LoadEstateChoicesAsync(null);
    }

    public async Task LoadAsync(Guid estateId)
    {
        await LoadEstateChoicesAsync(estateId);
    }

    private async Task LoadEstateChoicesAsync(Guid? preferredEstateId)
    {
        var previouslySelectedId =
            preferredEstateId ?? _currentEstateId;

        try
        {
            StatusMessage = "Loading estates...";

            var estates =
                await _apiClient.GetEstatesAsync();

            _suppressEstateSelectionLoad = true;

            Estates.Clear();

            foreach (var estate in estates
                         .OrderBy(e => e.FileNumber))
            {
                Estates.Add(estate);
            }

            SelectedEstate =
                previouslySelectedId.HasValue
                    ? Estates.FirstOrDefault(
                        e => e.EstateId == previouslySelectedId.Value)
                    : null;

            _suppressEstateSelectionLoad = false;

            if (SelectedEstate is not null)
            {
                _currentEstateId = SelectedEstate.EstateId;
                await LoadDocumentsAsync();
            }
            else
            {
                _currentEstateId = null;
                Documents.Clear();
                HeaderText = "Document Registry";

                StatusMessage = Estates.Count == 0
                    ? "No estates are available."
                    : "Choose an estate to view or upload scanned documents.";

                OnPropertyChanged(nameof(CanUpload));
            }
        }
        catch (Exception ex)
        {
            _suppressEstateSelectionLoad = false;

            Estates.Clear();
            Documents.Clear();

            StatusMessage =
                $"Could not load estates: {ex.Message}";
        }
    }

    private async Task LoadDocumentsForSelectedEstateAsync()
    {
        if (SelectedEstate is null)
        {
            _currentEstateId = null;
            Documents.Clear();
            HeaderText = "Document Registry";
            StatusMessage = "Choose an estate first.";
            OnPropertyChanged(nameof(CanUpload));
            return;
        }

        _currentEstateId = SelectedEstate.EstateId;

        await LoadDocumentsAsync();
    }

    private async Task LoadDocumentsAsync()
    {
        if (!_currentEstateId.HasValue)
        {
            Documents.Clear();
            HeaderText = "Document Registry";
            StatusMessage = "Choose an estate first.";
            return;
        }

        StatusMessage = "Loading documents...";

        try
        {
            var estate =
                await _apiClient.GetEstateAsync(
                    _currentEstateId.Value);

            if (estate is null)
            {
                Documents.Clear();
                StatusMessage = "Estate not found.";
                return;
            }

            HeaderText =
                $"File {estate.FileNumber} · {estate.Deceased?.FullName}";

            Documents.Clear();

            if (estate.ScannedDocuments is not null)
            {
                foreach (var document in estate.ScannedDocuments)
                {
                    Documents.Add(document);
                }
            }

            StatusMessage = Documents.Count == 0
                ? "No scanned documents have been uploaded for this estate yet."
                : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not load documents: {ex.Message}";
        }
    }

    private void ChooseFile()
    {
        if (!_currentEstateId.HasValue)
        {
            StatusMessage =
                "Choose an estate before selecting a document.";
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Choose document to upload",

            Filter =
                "Supported Documents (*.pdf;*.jpg;*.jpeg;*.png;*.docx)|*.pdf;*.jpg;*.jpeg;*.png;*.docx|" +
                "PDF Documents (*.pdf)|*.pdf|" +
                "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|" +
                "Word Documents (*.docx)|*.docx|" +
                "All Files (*.*)|*.*",

            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        var result = dialog.ShowDialog();

        if (result != true)
            return;

        _selectedFilePath = dialog.FileName;
        SelectedFileName = Path.GetFileName(dialog.FileName);

        StatusMessage = $"Selected: {SelectedFileName}";

        OnPropertyChanged(nameof(CanUpload));
    }

    private async Task UploadAsync(object? parameter)
    {
        if (!_currentEstateId.HasValue)
        {
            StatusMessage =
                "Choose an estate before uploading a document.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_selectedFilePath))
        {
            StatusMessage = "Choose a file first.";
            return;
        }

        if (!File.Exists(_selectedFilePath))
        {
            StatusMessage =
                "The selected file could not be found. Choose the file again.";

            _selectedFilePath = null;
            SelectedFileName = "No file selected";

            OnPropertyChanged(nameof(CanUpload));
            return;
        }

        if (string.IsNullOrWhiteSpace(NewDocumentType))
        {
            StatusMessage =
                "Enter a document type before uploading.";
            return;
        }

        try
        {
            IsUploading = true;

            StatusMessage =
                $"Uploading {SelectedFileName} to file {SelectedEstate?.FileNumber}...";

            await _apiClient.UploadDocumentAsync(
                _currentEstateId.Value,
                _selectedFilePath,
                NewDocumentType.Trim());

            _selectedFilePath = null;

            SelectedFileName = "No file selected";
            NewDocumentType = "Document";

            await LoadDocumentsAsync();

            StatusMessage =
                "Document uploaded successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Upload failed: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Document Upload Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsUploading = false;
            OnPropertyChanged(nameof(CanUpload));
        }
    }

    private async Task RefreshAsync(object? parameter)
    {
        if (!_currentEstateId.HasValue)
        {
            await LoadEstateChoicesAsync(null);
            return;
        }

        await LoadEstateChoicesAsync(
            _currentEstateId.Value);
    }

    private async Task ViewDocumentAsync(object? parameter)
    {
        if (!_currentEstateId.HasValue)
        {
            StatusMessage =
                "Choose an estate first.";
            return;
        }

        if (parameter is not ScannedDocumentDto document)
        {
            StatusMessage =
                "Could not determine which document to open.";
            return;
        }

        try
        {
            StatusMessage =
                $"Opening {document.FileName}...";

            var localPath =
                await _apiClient.DownloadDocumentToTempFileAsync(
                    _currentEstateId.Value,
                    document.DocumentId,
                    document.FileName);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = localPath,
                    UseShellExecute = true
                });

            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not open document: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Open Document Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task DeleteDocumentAsync(object? parameter)
    {
        if (!_currentEstateId.HasValue)
        {
            StatusMessage =
                "Choose an estate first.";
            return;
        }

        if (parameter is not ScannedDocumentDto document)
        {
            StatusMessage =
                "Could not determine which document to delete.";
            return;
        }

        var result =
            System.Windows.MessageBox.Show(
                $"Are you sure you want to delete '{document.FileName}'?\n\n" +
                "This action cannot be undone.",
                "Delete Document",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

        if (result !=
            System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            StatusMessage =
                $"Deleting {document.FileName}...";

            await _apiClient.DeleteDocumentAsync(
                _currentEstateId.Value,
                document.DocumentId);

            await LoadDocumentsAsync();

            StatusMessage =
                "Document deleted successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not delete document: {ex.Message}";

            System.Windows.MessageBox.Show(
                StatusMessage,
                "Delete Document Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}
