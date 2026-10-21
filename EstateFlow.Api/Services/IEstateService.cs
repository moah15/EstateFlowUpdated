using EstateFlow.Core.Dtos;

namespace EstateFlow.Api.Services;

public interface IEstateService
{
    Task<EstateDto?> GetEstateAsync(
        Guid estateId);

    Task<List<EstateSummaryDto>>
        GetAllEstatesAsync();

    Task<List<EstateSummaryDto>>
        GetEstatesForControllerAsync(
            Guid controllerId);

    // ─────────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────────

    Task<Guid> AddProgressStageAsync(
        Guid estateId,
        CreateProgressStageRequest request);

    Task<Guid> UpdateProgressStageAsync(
        Guid estateId,
        UpdateProgressStageRequest request);

    Task DeleteProgressStageAsync(
        Guid estateId,
        Guid stageId);

    // ─────────────────────────────────────────────
    // Diary
    // ─────────────────────────────────────────────

    Task<List<DiaryEntryWithEstateDto>>
        GetAllDiaryEntriesAsync(
            Guid controllerId);

    Task<Guid> AddDiaryEntryAsync(
        Guid estateId,
        CreateDiaryEntryRequest request);

    Task UpdateDiaryEntryStatusAsync(
        Guid estateId,
        Guid diaryId,
        bool isCompleted);

    Task DeleteDiaryEntryAsync(
        Guid estateId,
        Guid diaryId);

    // ─────────────────────────────────────────────
    // Reports
    // ─────────────────────────────────────────────

    Task<List<ReportSettingDto>>
        GetReportSettingsAsync(
            Guid controllerId);

    Task ToggleReportPauseAsync(
        Guid estateId,
        bool isPaused);

    // ─────────────────────────────────────────────
    // Estate
    // ─────────────────────────────────────────────

    Task<Guid> CreateEstateAsync(
        CreateEstateRequest request,
        Guid controllerId);

    Task UpdateEstateAsync(
        Guid estateId,
        UpdateEstateRequest request);
    Task DeleteEstateAsync(Guid estateId);

    // ─────────────────────────────────────────────
    // Executor
    // ─────────────────────────────────────────────

    Task<Guid> AddExecutorAsync(
        Guid estateId,
        CreateExecutorRequest request);

    Task DeleteExecutorAsync(
        Guid estateId,
        Guid executorId);

    Task UpdateExecutorAsync(
    Guid estateId,
    Guid executorId,
    UpdateExecutorRequest request);

    Task<string> UploadExecutorLettersAsync(
        Guid estateId,
        Guid executorId,
        IFormFile file);

    Task UpsertAdvertisementAsync(
        Guid estateId,
        UpsertAdvertisementRequest request);

    // ─────────────────────────────────────────────
    // Beneficiary
    // ─────────────────────────────────────────────

    Task<Guid> AddBeneficiaryAsync(
        Guid estateId,
        CreateBeneficiaryRequest request);

    Task DeleteBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId);

    // ─────────────────────────────────────────────
    // Movable Asset
    // ─────────────────────────────────────────────

    Task<Guid> AddMovableAssetAsync(
        Guid estateId,
        CreateMovableAssetRequest request);

    Task DeleteMovableAssetAsync(
        Guid estateId,
        Guid assetId);

    // ─────────────────────────────────────────────
    // Immovable Property
    // ─────────────────────────────────────────────

    Task<Guid> AddImmovablePropertyAsync(
        Guid estateId,
        CreateImmovablePropertyRequest request);

    Task DeleteImmovablePropertyAsync(
        Guid estateId,
        Guid assetId);

    // ─────────────────────────────────────────────
    // Financial Claim
    // ─────────────────────────────────────────────

    Task<Guid> AddFinancialClaimAsync(
        Guid estateId,
        CreateFinancialClaimRequest request);

    Task DeleteFinancialClaimAsync(
        Guid estateId,
        Guid claimId);

    // ─────────────────────────────────────────────
    // Liabilities + Liquidation & Distribution
    // ─────────────────────────────────────────────

    Task<Guid> AddLiabilityAsync(
        Guid estateId,
        CreateLiabilityRequest request);

    Task DeleteLiabilityAsync(
        Guid estateId,
        Guid liabilityId);

    Task<LiquidationDistributionDraftDto>
        GetLiquidationDistributionDraftAsync(
            Guid estateId);

    Task<(byte[] Content, string FileName)>
        GetLiquidationAccountPdfAsync(
            Guid estateId);

    // ─────────────────────────────────────────────
    // Cashbook
    // ─────────────────────────────────────────────

    Task<Guid> AddCashbookEntryAsync(
        Guid estateId,
        CreateCashbookEntryRequest request);

    Task DeleteCashbookEntryAsync(
        Guid estateId,
        Guid entryId);

    Task SetCashbookReconciledAsync(
        Guid estateId,
        Guid entryId,
        bool isReconciled);

    Task<(byte[] Content, string FileName)>
        GetCashbookPdfAsync(
            Guid estateId);

    // ─────────────────────────────────────────────
    // Checklist
    // ─────────────────────────────────────────────

    Task<Guid> AddChecklistItemAsync(
        Guid estateId,
        CreateChecklistItemRequest request);

    Task UpdateChecklistStatusAsync(
        Guid estateId,
        Guid itemId,
        string status);

    Task DeleteChecklistItemAsync(
        Guid estateId,
        Guid itemId);

    Task<int> SeedChecklistAsync(
        Guid estateId);

    // ─────────────────────────────────────────────
    // Audit trail
    // ─────────────────────────────────────────────

    Task<List<AuditEntryDto>> GetAuditTrailAsync(
        Guid estateId,
        int take);

   

}
