using EstateFlow.Core.Dtos;

namespace EstateFlow.Desktop.Services;

public interface IApiClient
{
    // ─────────────────────────────────────────────
    // Authentication
    // ─────────────────────────────────────────────

    Task<LoginResponse> LoginAsync(
        string email,
        string password);

    Task<LoginResponse> VerifyTwoFactorAsync(
        string email,
        string code);

    // ─────────────────────────────────────────────
    // Estates
    // ─────────────────────────────────────────────

    Task<List<EstateSummaryDto>> GetEstatesAsync();

    Task<EstateDto?> GetEstateAsync(
        Guid estateId);

    Task<Guid> CreateEstateAsync(
        CreateEstateRequest request);

    Task UpdateEstateAsync(
        Guid estateId,
        UpdateEstateRequest request);

    Task DeleteEstateAsync(
        Guid estateId);

    // ─────────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────────

    Task AddProgressStageAsync(
        Guid estateId,
        CreateProgressStageRequest request);

    Task UpdateProgressStageAsync(
        Guid estateId,
        UpdateProgressStageRequest request);

    Task DeleteProgressStageAsync(
        Guid estateId,
        Guid stageId);

    // ─────────────────────────────────────────────
    // Diary
    // ─────────────────────────────────────────────

    Task<List<DiaryEntryWithEstateDto>>
        GetAllDiaryEntriesAsync();

    Task AddDiaryEntryAsync(
        Guid estateId,
        CreateDiaryEntryRequest request);

    Task UpdateDiaryEntryStatusAsync(
        Guid estateId,
        Guid diaryId,
        bool isCompleted);

    Task DeleteDiaryEntryAsync(
        Guid estateId,
        Guid diaryId);

    Task SendDiaryEmailAsync(
        Guid estateId,
        Guid diaryId,
        SendDiaryEmailRequest request);

    // ─────────────────────────────────────────────
    // Reports
    // ─────────────────────────────────────────────

    Task<List<ReportSettingDto>>
        GetReportSettingsAsync();

    Task ToggleReportPauseAsync(
        Guid estateId,
        bool isPaused);

    // ─────────────────────────────────────────────
    // Executors
    // ─────────────────────────────────────────────

    Task AddExecutorAsync(
        Guid estateId,
        CreateExecutorRequest request);

    Task DeleteExecutorAsync(
        Guid estateId,
        Guid executorId);

    // ─────────────────────────────────────────────
    // Beneficiaries
    // ─────────────────────────────────────────────

    Task AddBeneficiaryAsync(
        Guid estateId,
        CreateBeneficiaryRequest request);

    Task DeleteBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId);

    // ─────────────────────────────────────────────
    // Movable Assets
    // ─────────────────────────────────────────────

    Task AddMovableAssetAsync(
        Guid estateId,
        CreateMovableAssetRequest request);

    Task DeleteMovableAssetAsync(
        Guid estateId,
        Guid assetId);

    // ─────────────────────────────────────────────
    // Immovable Properties
    // ─────────────────────────────────────────────

    Task AddImmovablePropertyAsync(
        Guid estateId,
        CreateImmovablePropertyRequest request);

    Task DeleteImmovablePropertyAsync(
        Guid estateId,
        Guid assetId);

    // ─────────────────────────────────────────────
    // Financial Claims
    // ─────────────────────────────────────────────

    Task AddFinancialClaimAsync(
        Guid estateId,
        CreateFinancialClaimRequest request);

    Task DeleteFinancialClaimAsync(
        Guid estateId,
        Guid claimId);

    // ─────────────────────────────────────────────
    // Liabilities / L&D
    // ─────────────────────────────────────────────

    Task AddLiabilityAsync(
        Guid estateId,
        CreateLiabilityRequest request);

    Task DeleteLiabilityAsync(
        Guid estateId,
        Guid liabilityId);

    Task<LiquidationDistributionDraftDto?>
        GetLiquidationDistributionDraftAsync(
            Guid estateId);

    // ─────────────────────────────────────────────
    // Letters
    // ─────────────────────────────────────────────

    Task<string> GenerateVehicleLetterToTempFileAsync(
        Guid estateId,
        Guid assetId);

    Task<string> GenerateInstitutionLetterToTempFileAsync(
        Guid estateId,
        Guid claimId);

    Task<string> DownloadLiquidationAccountToTempFileAsync(
        Guid estateId);

    // ─────────────────────────────────────────────
    // Cashbook / Checklist / Audit trail
    // ─────────────────────────────────────────────

    Task AddCashbookEntryAsync(
        Guid estateId,
        CreateCashbookEntryRequest request);

    Task DeleteCashbookEntryAsync(
        Guid estateId,
        Guid entryId);

    Task SetCashbookReconciledAsync(
        Guid estateId,
        Guid entryId,
        bool isReconciled);

    Task<string> DownloadCashbookToTempFileAsync(
        Guid estateId);

    Task AddChecklistItemAsync(
        Guid estateId,
        CreateChecklistItemRequest request);

    Task<int> SeedChecklistAsync(
        Guid estateId);

    Task UpdateChecklistStatusAsync(
        Guid estateId,
        Guid itemId,
        string status);

    Task DeleteChecklistItemAsync(
        Guid estateId,
        Guid itemId);

    Task<List<AuditEntryDto>> GetAuditTrailAsync(
        Guid estateId,
        int take);

    // ─────────────────────────────────────────────
    // Scanned Documents
    // ─────────────────────────────────────────────

    Task UploadDocumentAsync(
        Guid estateId,
        string filePath,
        string documentType);

    Task<string> DownloadDocumentToTempFileAsync(
        Guid estateId,
        Guid documentId,
        string suggestedFileName);

    Task DeleteDocumentAsync(
        Guid estateId,
        Guid documentId);
    Task UpdateExecutorAsync(Guid estateId, Guid executorDetailId, UpdateExecutorRequest updateExecutorRequest);
    Task<string> UploadExecutorDocumentAsync(Guid estateId, Guid executorDetailId, string fileName);
    Task UpsertAdvertisementAsync(Guid estateId, UpsertAdvertisementRequest upsertAdvertisementRequest);
}
