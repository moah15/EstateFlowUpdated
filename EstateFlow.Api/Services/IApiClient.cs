using EstateFlow.Core.Dtos;

namespace EstateFlow.Desktop.Services;

public interface IApiClient
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<LoginResponse> VerifyTwoFactorAsync(string email, string code);
    Task<List<EstateSummaryDto>> GetEstatesAsync();
    Task<EstateDto?> GetEstateAsync(Guid estateId);
    Task<Guid> CreateEstateAsync(CreateEstateRequest request);
    Task UpdateEstateAsync(Guid estateId, UpdateEstateRequest request);
    Task DeleteEstateAsync(Guid estateId);
    Task AddExecutorAsync(Guid estateId, CreateExecutorRequest request);
    Task DeleteExecutorAsync(Guid estateId, Guid executorId);
    Task UpdateExecutorAsync(Guid estateId, Guid executorId, UpdateExecutorRequest request);
    Task<string> UploadExecutorDocumentAsync(Guid estateId, Guid executorId, string filePath);
    Task UpsertAdvertisementAsync(Guid estateId, UpsertAdvertisementRequest request);
    Task AddBeneficiaryAsync(Guid estateId, CreateBeneficiaryRequest request);
    Task DeleteBeneficiaryAsync(Guid estateId, Guid beneficiaryId);
    Task AddMovableAssetAsync(Guid estateId, CreateMovableAssetRequest request);
    Task DeleteMovableAssetAsync(Guid estateId, Guid assetId);
    Task<string> GenerateVehicleLetterToTempFileAsync(Guid estateId, Guid assetId);
    Task AddImmovablePropertyAsync(Guid estateId, CreateImmovablePropertyRequest request);
    Task DeleteImmovablePropertyAsync(Guid estateId, Guid assetId);
    Task AddFinancialClaimAsync(Guid estateId, CreateFinancialClaimRequest request);
    Task DeleteFinancialClaimAsync(Guid estateId, Guid claimId);
    Task<string> GenerateInstitutionLetterToTempFileAsync(Guid estateId, Guid claimId);
    Task<string> DownloadLiquidationAccountToTempFileAsync(Guid estateId);
    Task AddLiabilityAsync(Guid estateId, CreateLiabilityRequest request);
    Task DeleteLiabilityAsync(Guid estateId, Guid liabilityId);
    Task<LiquidationDistributionDraftDto?> GetLiquidationDistributionDraftAsync(Guid estateId);
    Task AddDiaryEntryAsync(Guid estateId, CreateDiaryEntryRequest request);
    Task UpdateDiaryEntryStatusAsync(Guid estateId, Guid diaryId, bool isCompleted);
    Task DeleteDiaryEntryAsync(Guid estateId, Guid diaryId);
    Task AddProgressStageAsync(Guid estateId, CreateProgressStageRequest request);
    Task UpdateProgressStageAsync(Guid estateId, UpdateProgressStageRequest request);
    Task DeleteProgressStageAsync(Guid estateId, Guid stageId);
    Task<List<DiaryEntryWithEstateDto>> GetAllDiaryEntriesAsync();
    Task<List<ReportSettingDto>> GetReportSettingsAsync();
    Task ToggleReportPauseAsync(Guid estateId, bool isPaused);
    Task UploadDocumentAsync(Guid estateId, string filePath, string documentType);
    Task<string> GetDocumentDownloadUrlAsync(Guid estateId, Guid documentId);
}