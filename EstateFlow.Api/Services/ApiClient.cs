using EstateFlow.Core.Dtos;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        await EnsureSuccessAsync(response);

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

        await EnsureSuccessAsync(response);

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

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response);

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

        await EnsureSuccessAsync(response);

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

        await EnsureSuccessAsync(response);
    }
    public async Task DeleteEstateAsync(Guid estateId)
    {
        var response = await _httpClient.DeleteAsync($"api/estates/{estateId}");
        await EnsureSuccessAsync(response);
    }

    public async Task<string> GenerateVehicleLetterToTempFileAsync(
        Guid estateId, Guid assetId)
    {
        var response = await _httpClient.GetAsync(
            $"api/estates/{estateId}/movable-assets/{assetId}/vehicle-letter");
        await EnsureSuccessAsync(response);

        var tempFolder = Path.Combine(Path.GetTempPath(), "EstateFlow", "Letters");
        Directory.CreateDirectory(tempFolder);
        var localPath = Path.Combine(tempFolder, $"VehicleLetter_{assetId}.pdf");

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(localPath);
        await source.CopyToAsync(destination);
        return localPath;
    }

    public async Task<string> GenerateInstitutionLetterToTempFileAsync(
        Guid estateId, Guid claimId)
    {
        var response = await _httpClient.GetAsync(
            $"api/estates/{estateId}/financial-claims/{claimId}/institution-letter");
        await EnsureSuccessAsync(response);

        var tempFolder = Path.Combine(Path.GetTempPath(), "EstateFlow", "Letters");
        Directory.CreateDirectory(tempFolder);
        var localPath = Path.Combine(tempFolder, $"InstitutionLetter_{claimId}.pdf");

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(localPath);
        await source.CopyToAsync(destination);
        return localPath;
    }

    public async Task<string> DownloadLiquidationAccountToTempFileAsync(Guid estateId)
    {
        var response = await _httpClient.GetAsync(
            $"api/estates/{estateId}/liquidation-account");
        await EnsureSuccessAsync(response);

        var tempFolder = Path.Combine(Path.GetTempPath(), "EstateFlow", "Accounts");
        Directory.CreateDirectory(tempFolder);
        var localPath = Path.Combine(tempFolder, $"LiquidationAccount_{estateId}.pdf");

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(localPath);
        await source.CopyToAsync(destination);
        return localPath;
    }

    public async Task AddLiabilityAsync(Guid estateId, CreateLiabilityRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/estates/{estateId}/liabilities", request);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteLiabilityAsync(Guid estateId, Guid liabilityId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/estates/{estateId}/liabilities/{liabilityId}");
        await EnsureSuccessAsync(response);
    }

    public async Task<LiquidationDistributionDraftDto?> GetLiquidationDistributionDraftAsync(
        Guid estateId)
    {
        var response = await _httpClient.GetAsync(
            $"api/estates/{estateId}/liquidation-distribution");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<LiquidationDistributionDraftDto>();
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

        await EnsureSuccessAsync(response);
    }

    public async Task UpdateProgressStageAsync(
        Guid estateId,
        UpdateProgressStageRequest request)
    {
        var response =
            await _httpClient.PatchAsJsonAsync(
                $"api/estates/{estateId}/progress-stage",
                request);

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteProgressStageAsync(
        Guid estateId,
        Guid stageId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/progress-stages/{stageId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteDiaryEntryAsync(
        Guid estateId,
        Guid diaryId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/diary/estate/{estateId}/{diaryId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    // ─────────────────────────────────────────────
    // Executors
    // ─────────────────────────────────────────────

    public async Task AddExecutorAsync(
        Guid estateId,
        CreateExecutorRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/estates/{estateId}/executors",
                request);

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteExecutorAsync(
        Guid estateId,
        Guid executorId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/executors/{executorId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task<string> UploadExecutorDocumentAsync(
        Guid estateId,
        Guid executorId,
        string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "The selected file could not be found.", filePath);

        await using var fileStream = File.OpenRead(filePath);
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(GetContentType(filePath));
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        var response =
            await _httpClient.PostAsync(
                $"api/estates/{estateId}/executors/{executorId}/letters",
                form);

        await EnsureSuccessAsync(response);

        var result =
            await response.Content
                .ReadFromJsonAsync<Dictionary<string, string>>();

        return result! ["url"];
    }

    public async Task UpsertAdvertisementAsync(
        Guid estateId,
        UpsertAdvertisementRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/estates/{estateId}/advertisements",
                request);

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/beneficiaries/{beneficiaryId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteMovableAssetAsync(
        Guid estateId,
        Guid assetId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/movable-assets/{assetId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteImmovablePropertyAsync(
        Guid estateId,
        Guid assetId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/immovable-properties/{assetId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteFinancialClaimAsync(
        Guid estateId,
        Guid claimId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/estates/{estateId}/financial-claims/{claimId}");

        await EnsureSuccessAsync(response);
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

        await EnsureSuccessAsync(response);
    }

    public async Task<string> GetDocumentDownloadUrlAsync(
        Guid estateId,
        Guid documentId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/estates/{estateId}/documents/{documentId}/download");

        await EnsureSuccessAsync(response);

        var extension =
            response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? $"EstateFlow_{documentId}";

        extension = extension.Trim('"');

        var tempFolder = Path.Combine(
            Path.GetTempPath(),
            "EstateFlow",
            "Documents");

        Directory.CreateDirectory(tempFolder);

        var localPath = Path.Combine(
            tempFolder,
            $"{documentId}_{Path.GetFileName(extension)}");

        await using var source =
            await response.Content.ReadAsStreamAsync();

        await using var destination =
            File.Create(localPath);

        await source.CopyToAsync(destination);

        return localPath;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body =
            await response.Content.ReadAsStringAsync();

        var message = body;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var json =
                    System.Text.Json.JsonDocument.Parse(body);

                if (json.RootElement.TryGetProperty(
                        "message",
                        out var messageElement))
                {
                    message =
                        messageElement.GetString() ?? body;
                }
                else if (json.RootElement.TryGetProperty(
                             "detail",
                             out var detailElement))
                {
                    message =
                        detailElement.GetString() ?? body;
                }
                else if (json.RootElement.TryGetProperty(
                             "title",
                             out var titleElement))
                {
                    message =
                        titleElement.GetString() ?? body;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // The API returned plain text rather than JSON.
            }
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message =
                response.ReasonPhrase
                ?? "The EstateFlow API request failed.";
        }

        throw new HttpRequestException(
            $"EstateFlow API returned {(int)response.StatusCode} " +
            $"({response.StatusCode}): {message}");
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