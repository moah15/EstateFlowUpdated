using EstateFlow.Core.Dtos;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EstateFlow.Desktop.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ─────────────────────────────────────────────
    // Authentication
    // ─────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(
        string email,
        string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<LoginResponse>())!;
    }

    public async Task<LoginResponse> VerifyTwoFactorAsync(
        string email,
        string code)
    {
        var request = new VerifyTwoFactorRequest
        {
            Email = email,
            Code = code
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login/verify-2fa",
                request);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<LoginResponse>())!;
    }

    // ─────────────────────────────────────────────
    // Estates
    // ─────────────────────────────────────────────

    public async Task<List<EstateSummaryDto>>
        GetEstatesAsync()
    {
        var result =
            await _httpClient
                .GetFromJsonAsync<List<EstateSummaryDto>>(
                    "api/estates");

        return result ??
               new List<EstateSummaryDto>();
    }

    public async Task<EstateDto?> GetEstateAsync(
        Guid estateId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/estates/{estateId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<EstateDto>();
    }

    public async Task<Guid> CreateEstateAsync(
        CreateEstateRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/estates",
                request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<Dictionary<string, Guid>>();

        return result!["estateId"];
    }

    public async Task UpdateEstateAsync(
        Guid estateId,
        UpdateEstateRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/estates/{estateId}",
                request);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEstateAsync(
        Guid estateId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────────

    public async Task AddProgressStageAsync(
        Guid estateId,
        CreateProgressStageRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/progress-stages",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task UpdateProgressStageAsync(
        Guid estateId,
        UpdateProgressStageRequest request)
    {
        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/estates/{estateId}/progress-stage",
                request);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProgressStageAsync(
        Guid estateId,
        Guid stageId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/progress-stages/{stageId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Diary
    // ─────────────────────────────────────────────

    public async Task<List<DiaryEntryWithEstateDto>>
        GetAllDiaryEntriesAsync()
    {
        var result =
            await _httpClient
                .GetFromJsonAsync<List<DiaryEntryWithEstateDto>>(
                    "api/diary");

        return result ??
               new List<DiaryEntryWithEstateDto>();
    }

    public async Task AddDiaryEntryAsync(
        Guid estateId,
        CreateDiaryEntryRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/diary/estate/{estateId}",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task UpdateDiaryEntryStatusAsync(
        Guid estateId,
        Guid diaryId,
        bool isCompleted)
    {
        var request =
            new UpdateDiaryEntryStatusRequest
            {
                IsCompleted = isCompleted
            };

        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/diary/estate/{estateId}/{diaryId}/status",
                request);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteDiaryEntryAsync(
        Guid estateId,
        Guid diaryId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/diary/estate/{estateId}/{diaryId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task SendDiaryEmailAsync(
        Guid estateId,
        Guid diaryId,
        SendDiaryEmailRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/diary/estate/{estateId}/{diaryId}/send-email",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Diary email could not be sent");
    }

    // ─────────────────────────────────────────────
    // Reports
    // ─────────────────────────────────────────────

    public async Task<List<ReportSettingDto>>
        GetReportSettingsAsync()
    {
        var result =
            await _httpClient
                .GetFromJsonAsync<List<ReportSettingDto>>(
                    "api/reports/settings");

        return result ??
               new List<ReportSettingDto>();
    }

    public async Task ToggleReportPauseAsync(
        Guid estateId,
        bool isPaused)
    {
        var request =
            new ToggleReportPauseRequest
            {
                IsPaused = isPaused
            };

        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/reports/settings/{estateId}",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The report setting could not be changed");
    }

    // ─────────────────────────────────────────────
    // Executors
    // ─────────────────────────────────────────────

    public async Task<Guid> AddExecutorAsync(
        Guid estateId,
        CreateExecutorRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/executors",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The executor could not be saved");

        var result =
            await response.Content
                .ReadFromJsonAsync<Dictionary<string, Guid>>();

        return result!["executorId"];
    }

    public async Task DeleteExecutorAsync(
        Guid estateId,
        Guid executorId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/executors/{executorId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateExecutorAsync(
        Guid estateId,
        Guid executorId,
        UpdateExecutorRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/estates/{estateId}/executors/{executorId}",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Executor details could not be saved");
    }

    public async Task<string> UploadExecutorDocumentAsync(
        Guid estateId,
        Guid executorId,
        string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "The selected file could not be found.",
                filePath);

        await using var fileStream =
            File.OpenRead(filePath);

        using var form =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                GetContentType(filePath));

        form.Add(
            fileContent,
            "file",
            Path.GetFileName(filePath));

        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/executors/{executorId}/letters",
                form);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Letters of executorship upload failed");

        using var json =
            JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

        return json.RootElement
            .TryGetProperty("url", out var urlProp)
            ? urlProp.GetString() ?? string.Empty
            : string.Empty;
    }

    public async Task UpsertAdvertisementAsync(
        Guid estateId,
        UpsertAdvertisementRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/estates/{estateId}/advertisements",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Advertisement details could not be saved");
    }

    // ─────────────────────────────────────────────
    // Beneficiaries
    // ─────────────────────────────────────────────

    public async Task AddBeneficiaryAsync(
        Guid estateId,
        CreateBeneficiaryRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/beneficiaries",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task DeleteBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/beneficiaries/{beneficiaryId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Movable Assets
    // ─────────────────────────────────────────────

    public async Task AddMovableAssetAsync(
        Guid estateId,
        CreateMovableAssetRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/movable-assets",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task DeleteMovableAssetAsync(
        Guid estateId,
        Guid assetId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/movable-assets/{assetId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Immovable Properties
    // ─────────────────────────────────────────────

    public async Task AddImmovablePropertyAsync(
        Guid estateId,
        CreateImmovablePropertyRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/immovable-properties",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task DeleteImmovablePropertyAsync(
        Guid estateId,
        Guid assetId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/immovable-properties/{assetId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Financial Claims
    // ─────────────────────────────────────────────

    public async Task AddFinancialClaimAsync(
        Guid estateId,
        CreateFinancialClaimRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/financial-claims",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task DeleteFinancialClaimAsync(
        Guid estateId,
        Guid claimId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/financial-claims/{claimId}");

        response.EnsureSuccessStatusCode();
    }

    // ─────────────────────────────────────────────
    // Liabilities / Liquidation & Distribution
    // ─────────────────────────────────────────────

    public async Task AddLiabilityAsync(
        Guid estateId,
        CreateLiabilityRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/liabilities",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The record could not be saved");
    }

    public async Task DeleteLiabilityAsync(
        Guid estateId,
        Guid liabilityId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/liabilities/{liabilityId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<LiquidationDistributionDraftDto?>
        GetLiquidationDistributionDraftAsync(
            Guid estateId)
    {
        return await _httpClient
            .GetFromJsonAsync<LiquidationDistributionDraftDto>(
                $"api/estates/{estateId}/liquidation-distribution");
    }

    // ─────────────────────────────────────────────
    // Letters
    // ─────────────────────────────────────────────

    public async Task<string> GenerateVehicleLetterToTempFileAsync(
        Guid estateId,
        Guid assetId)
    {
        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/letters/vehicle/{assetId}",
                null);

        response.EnsureSuccessStatusCode();

        return await SaveResponseToTempFileAsync(
            response,
            $"Vehicle_Letter_{assetId}.txt");
    }

    public async Task<string> GenerateInstitutionLetterToTempFileAsync(
        Guid estateId,
        Guid claimId)
    {
        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/letters/institution/{claimId}",
                null);

        response.EnsureSuccessStatusCode();

        return await SaveResponseToTempFileAsync(
            response,
            $"Institution_Letter_{claimId}.txt");
    }

    public async Task<string> DownloadLiquidationAccountToTempFileAsync(
        Guid estateId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/estates/{estateId}/liquidation-distribution/pdf");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The L&D account could not be generated");

        return await SaveResponseToTempFileAsync(
            response,
            $"LD_Account_{estateId}.pdf");
    }

    // ─────────────────────────────────────────────
    // Cashbook
    // ─────────────────────────────────────────────

    public async Task AddCashbookEntryAsync(
        Guid estateId,
        CreateCashbookEntryRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/cashbook",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The cashbook entry could not be saved");
    }

    public async Task DeleteCashbookEntryAsync(
        Guid estateId,
        Guid entryId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/cashbook/{entryId}");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The cashbook entry could not be deleted");
    }

    public async Task SetCashbookReconciledAsync(
        Guid estateId,
        Guid entryId,
        bool isReconciled)
    {
        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/estates/{estateId}/cashbook/{entryId}/reconciled",
                new SetReconciledRequest { IsReconciled = isReconciled });

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The reconciliation could not be saved");
    }

    public async Task<string> DownloadCashbookToTempFileAsync(
        Guid estateId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/estates/{estateId}/cashbook/pdf");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The cashbook could not be generated");

        return await SaveResponseToTempFileAsync(
            response,
            $"Cashbook_{estateId}.pdf");
    }

    // ─────────────────────────────────────────────
    // Checklist
    // ─────────────────────────────────────────────

    public async Task AddChecklistItemAsync(
        Guid estateId,
        CreateChecklistItemRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/checklist",
                request);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The task could not be saved");
    }

    public async Task<int> SeedChecklistAsync(
        Guid estateId)
    {
        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/checklist/seed",
                null);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The standard checklist could not be added");

        using var json =
            JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

        return json.RootElement.TryGetProperty("added", out var added)
            ? added.GetInt32()
            : 0;
    }

    public async Task UpdateChecklistStatusAsync(
        Guid estateId,
        Guid itemId,
        string status)
    {
        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/estates/{estateId}/checklist/{itemId}/status",
                new UpdateChecklistStatusRequest { Status = status });

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The task status could not be changed");
    }

    public async Task DeleteChecklistItemAsync(
        Guid estateId,
        Guid itemId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/checklist/{itemId}");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The task could not be deleted");
    }

    // ─────────────────────────────────────────────
    // Scanned Documents
    // ─────────────────────────────────────────────

    public async Task UploadDocumentAsync(
        Guid estateId,
        string filePath,
        string documentType)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "The selected document could not be found.",
                filePath);

        await using var fileStream =
            File.OpenRead(filePath);

        using var form =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                GetContentType(filePath));

        form.Add(
            fileContent,
            "file",
            Path.GetFileName(filePath));

        form.Add(
            new StringContent(documentType),
            "documentType");

        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/documents",
                form);

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Document upload failed");
    }

    public async Task<string> DownloadDocumentToTempFileAsync(
        Guid estateId,
        Guid documentId,
        string suggestedFileName)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/estates/{estateId}/documents/{documentId}/download");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Document download failed");

        var safeFileName =
            string.IsNullOrWhiteSpace(suggestedFileName)
                ? $"{documentId}.tmp"
                : suggestedFileName;

        var tempPath =
            Path.Combine(
                Path.GetTempPath(),
                safeFileName);

        await using (var fileStream =
                     File.Create(tempPath))
        {
            await response.Content
                .CopyToAsync(fileStream);
        }

        return tempPath;
    }

    public async Task DeleteDocumentAsync(
        Guid estateId,
        Guid documentId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/documents/{documentId}");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "Document deletion failed");
    }

    // ─────────────────────────────────────────────
    // Edit existing records
    // ─────────────────────────────────────────────

    public Task UpdateBeneficiaryAsync(Guid estateId, Guid beneficiaryId, CreateBeneficiaryRequest request) =>
        PutAsync($"api/estates/{estateId}/beneficiaries/{beneficiaryId}", request,
            "The heir could not be saved");

    public Task UpdateMovableAssetAsync(Guid estateId, Guid assetId, CreateMovableAssetRequest request) =>
        PutAsync($"api/estates/{estateId}/movable-assets/{assetId}", request,
            "The asset could not be saved");

    public Task UpdateImmovablePropertyAsync(Guid estateId, Guid assetId, CreateImmovablePropertyRequest request) =>
        PutAsync($"api/estates/{estateId}/immovable-properties/{assetId}", request,
            "The property could not be saved");

    public Task UpdateFinancialClaimAsync(Guid estateId, Guid claimId, CreateFinancialClaimRequest request) =>
        PutAsync($"api/estates/{estateId}/financial-claims/{claimId}", request,
            "The claim could not be saved");

    public Task UpdateLiabilityAsync(Guid estateId, Guid liabilityId, CreateLiabilityRequest request) =>
        PutAsync($"api/estates/{estateId}/liabilities/{liabilityId}", request,
            "The liability could not be saved");

    public Task UpdateCashbookEntryAsync(Guid estateId, Guid entryId, CreateCashbookEntryRequest request) =>
        PutAsync($"api/estates/{estateId}/cashbook/{entryId}", request,
            "The cashbook entry could not be saved");

    public Task UpdateChecklistItemAsync(Guid estateId, Guid itemId, CreateChecklistItemRequest request) =>
        PutAsync($"api/estates/{estateId}/checklist/{itemId}", request,
            "The task could not be saved");

    public Task UpdateDiaryEntryAsync(Guid estateId, Guid diaryId, CreateDiaryEntryRequest request) =>
        PutAsync($"api/diary/estate/{estateId}/{diaryId}", request,
            "The diary entry could not be saved");

    public async Task<List<FormGroupDto>> GetFormCatalogAsync()
    {
        var response = await _httpClient.GetAsync("api/forms");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The list of forms could not be loaded");

        return await response.Content.ReadFromJsonAsync<List<FormGroupDto>>()
               ?? new List<FormGroupDto>();
    }

    public async Task<EstateFormDto> GetEstateFormAsync(Guid estateId, string formId)
    {
        var response = await _httpClient.GetAsync(
            $"api/estates/{estateId}/forms/{Uri.EscapeDataString(formId)}");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The form could not be loaded");

        return (await response.Content.ReadFromJsonAsync<EstateFormDto>())!;
    }

    public async Task<string> GenerateEstateFormToTempFileAsync(
        Guid estateId,
        string formId,
        Dictionary<string, string> values)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/estates/{estateId}/forms/{Uri.EscapeDataString(formId)}/pdf",
            new GenerateFormRequest { Values = values });

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The form could not be created");

        return await SaveResponseToTempFileAsync(response, $"{formId}.pdf");
    }

    public async Task<string> DownloadBlankFormToTempFileAsync(string formId)
    {
        var response = await _httpClient.GetAsync(
            $"api/forms/{Uri.EscapeDataString(formId)}/blank");

        await EnsureSuccessWithApiMessageAsync(
            response,
            "The blank form could not be downloaded");

        return await SaveResponseToTempFileAsync(response, $"{formId}_blank.pdf");
    }

    private async Task PutAsync<T>(string url, T request, string fallbackMessage)
    {
        var response = await _httpClient.PutAsJsonAsync(url, request);
        await EnsureSuccessWithApiMessageAsync(response, fallbackMessage);
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────

    private static async Task EnsureSuccessWithApiMessageAsync(
        HttpResponseMessage response,
        string fallbackMessage)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body =
            await response.Content.ReadAsStringAsync();

        var message = string.Empty;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var json =
                    JsonDocument.Parse(body);

                if (json.RootElement.TryGetProperty(
                        "message",
                        out var messageElement))
                {
                    message =
                        messageElement.GetString()
                        ?? string.Empty;
                }
                else if (json.RootElement.TryGetProperty(
                             "errors",
                             out var errorsElement))
                {
                    var details = new List<string>();

                    var items =
                        errorsElement.ValueKind == JsonValueKind.Object
                            ? errorsElement.EnumerateObject().Select(p => p.Value)
                            : errorsElement.ValueKind == JsonValueKind.Array
                                ? errorsElement.EnumerateArray()
                                : Enumerable.Empty<JsonElement>();

                    foreach (var item in items)
                    {
                        if (item.ValueKind == JsonValueKind.Array)
                            details.AddRange(item.EnumerateArray().Select(e => e.ToString()));
                        else
                            details.Add(item.ToString());
                    }

                    message = string.Join(" ", details.Where(d => !string.IsNullOrWhiteSpace(d)));
                }
            }
            catch (JsonException)
            {
                message = body;
            }
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message =
                $"{fallbackMessage}. " +
                $"Server returned {(int)response.StatusCode} ({response.StatusCode}).";
        }

        throw new HttpRequestException(message);
    }

    private static async Task<string> SaveResponseToTempFileAsync(
        HttpResponseMessage response,
        string fallbackFileName)
    {
        var headerFileName =
            response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName;

        var fileName =
            string.IsNullOrWhiteSpace(headerFileName)
                ? fallbackFileName
                : headerFileName.Trim('"');

        fileName = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = fallbackFileName;

        var tempPath =
            Path.Combine(
                Path.GetTempPath(),
                fileName);

        await using var fileStream =
            File.Create(tempPath);

        await response.Content.CopyToAsync(
            fileStream);

        return tempPath;
    }

    private static string GetContentType(
        string filePath)
    {
        return Path.GetExtension(filePath)
            .ToLowerInvariant() switch
        {
            ".pdf" =>
                "application/pdf",

            ".jpg" or ".jpeg" =>
                "image/jpeg",

            ".png" =>
                "image/png",

            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            _ =>
                "application/octet-stream"
        };
    }
}
