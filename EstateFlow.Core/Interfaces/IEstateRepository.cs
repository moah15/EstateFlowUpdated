using EstateFlow.Core.Models;

namespace EstateFlow.Core.Interfaces;

public interface IEstateRepository
{
    Task<Estate?> GetByIdAsync(Guid estateId);

    Task<List<Estate>> GetAllAsync();

    Task<List<Estate>> GetByControllerIdAsync(
        Guid controllerId);

    Task<Estate> AddAsync(
        Estate estate);

    Task SaveAsync(
        Estate estate);

    Task DeleteAsync(
        Guid estateId);

    Task<bool> ExistsAsync(
        Guid estateId);

    // Explicit child inserts
    Task AddExecutorAsync(Executor executor);

    Task AddBeneficiaryAsync(
        Beneficiary beneficiary);

    Task AddMovableAssetAsync(
        MovableAsset asset);

    Task AddImmovablePropertyAsync(
        ImmovableProperty property);

    Task AddFinancialClaimAsync(
        FinancialClaim claim);

    Task AddLiabilityAsync(
        Liability liability);

    Task AddDiaryEntryAsync(
        DiaryEntry diaryEntry);

    Task AddProgressStageAsync(
        ProgressStage progressStage);

    Task AddAutomatedReportAsync(
        AutomatedReport report);

    // Pause / resume the monthly report for one estate without loading the whole estate graph.
    Task SetReportPausedAsync(
        Guid estateId,
        bool isPaused);

    Task AddCashbookEntryAsync(
        CashbookEntry entry);

    Task AddChecklistItemsAsync(
        IEnumerable<ChecklistItem> items);

    Task<bool> DeleteCashbookEntryAsync(
        Guid estateId,
        Guid entryId);

    Task<bool> SetCashbookReconciledAsync(
        Guid estateId,
        Guid entryId,
        bool isReconciled);

    Task<bool> DeleteChecklistItemAsync(
        Guid estateId,
        Guid itemId);

    Task<bool> SetChecklistStatusAsync(
        Guid estateId,
        Guid itemId,
        string status,
        DateTime? completedDate);

    Task<int> NextChecklistSortOrderAsync(
        Guid estateId);

    Task<List<AuditLog>> GetAuditLogsAsync(
        Guid estateId,
        int take);
    Task UpdateExecutorAsync(Executor executor);
    Task UpsertAdvertisementAsync(EstateAdvertisement advertisement);
}