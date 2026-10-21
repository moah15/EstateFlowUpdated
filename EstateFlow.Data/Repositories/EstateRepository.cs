using EstateFlow.Core.Interfaces;
using EstateFlow.Core.Models;
using EstateFlow.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EstateFlow.Data.Repositories;

public class EstateRepository : IEstateRepository
{
    private readonly EstateFlowDbContext _context;

    public EstateRepository(
        EstateFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Estate?> GetByIdAsync(
        Guid estateId)
    {
        return await _context.Estates
            .Include(e => e.Deceased)
            .Include(e => e.Executors)
            .Include(e => e.Beneficiaries)
            .Include(e => e.ImmovableProperties)
            .Include(e => e.MovableAssets)
            .Include(e => e.FinancialClaims)
            .Include(e => e.Liabilities)
            .Include(e => e.DiaryEntries)
            .Include(e => e.ProgressStages)
            .Include(e => e.ScannedDocuments)
            .Include(e => e.AutomatedReports)
            .Include(e => e.CashbookEntries)
            .Include(e => e.ChecklistItems)
            .Include(e => e.Controller)
            .Include(e => e.Controller)
.Include(e => e.Advertisements)   // ← add this
.AsSplitQuery()
            // Many collections are loaded at once; without this EF builds one huge
            // cross-joined query that explodes once the cashbook has a few hundred rows.
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                e => e.EstateId == estateId);
    }
    public async Task UpdateExecutorAsync(Executor executor)
    {
        _context.Executors.Update(executor);
        await _context.SaveChangesAsync();
    }

    public async Task UpsertAdvertisementAsync(EstateAdvertisement advertisement)
    {
        var existing = await _context.Advertisements
            .FirstOrDefaultAsync(a =>
                a.EstateId == advertisement.EstateId &&
                a.AdvertisementType == advertisement.AdvertisementType);

        if (existing is null)
        {
            advertisement.AdvertisementId = Guid.NewGuid();
            _context.Advertisements.Add(advertisement);
        }
        else
        {
            existing.GazetteNumber = advertisement.GazetteNumber;
            existing.GazetteDate = advertisement.GazetteDate;
            existing.NewspaperName = advertisement.NewspaperName;
            existing.NewspaperDate = advertisement.NewspaperDate;
            existing.ClaimDeadline = advertisement.ClaimDeadline;
            existing.ObjectionsReceived = advertisement.ObjectionsReceived;
            existing.Notes = advertisement.Notes;
        }

        await _context.SaveChangesAsync();
    }



    public async Task<List<Estate>> GetAllAsync()
    {
        return await _context.Estates
            .Include(e => e.Deceased)
            .Include(e => e.Controller)
            .Include(e => e.DiaryEntries)
            .Include(e => e.AutomatedReports)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Estate>>
        GetByControllerIdAsync(
            Guid controllerId)
    {
        return await _context.Estates
            .Where(e =>
                e.ControllerId == controllerId)
            .Include(e => e.Deceased)
            .Include(e => e.Controller)
            .Include(e => e.DiaryEntries)
            .Include(e => e.AutomatedReports)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estate> AddAsync(
        Estate estate)
    {
        await _context.Estates.AddAsync(estate);

        await _context.SaveChangesAsync();

        return estate;
    }

    public async Task SaveAsync(
        Estate estate)
    {
        // Estate returned by GetByIdAsync is tracked.
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        Guid estateId)
    {
        var estate =
            await _context.Estates
                .FirstOrDefaultAsync(
                    e => e.EstateId == estateId);

        if (estate is null)
            return;

        _context.Estates.Remove(estate);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(
        Guid estateId)
    {
        return await _context.Estates
            .AsNoTracking()
            .AnyAsync(
                e => e.EstateId == estateId);
    }

    // =====================================================
    // EXPLICIT CHILD INSERTS
    // =====================================================

    public async Task AddExecutorAsync(
        Executor executor)
    {
        _context.Entry(executor).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddBeneficiaryAsync(
        Beneficiary beneficiary)
    {
        _context.Entry(beneficiary).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddMovableAssetAsync(
        MovableAsset asset)
    {
        _context.Entry(asset).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddImmovablePropertyAsync(
        ImmovableProperty property)
    {
        _context.Entry(property).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddFinancialClaimAsync(
        FinancialClaim claim)
    {
        _context.Entry(claim).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddLiabilityAsync(
        Liability liability)
    {
        _context.Entry(liability).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }


    public async Task AddDiaryEntryAsync(
        DiaryEntry diaryEntry)
    {
        _context.Entry(diaryEntry).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddProgressStageAsync(
        ProgressStage progressStage)
    {
        _context.Entry(progressStage).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddAutomatedReportAsync(
        AutomatedReport report)
    {
        _context.Entry(report).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task SetReportPausedAsync(
        Guid estateId,
        bool isPaused)
    {
        var latest =
            await _context.AutomatedReports
                .Where(r => r.EstateId == estateId)
                .OrderByDescending(r => r.ReportPeriod)
                .FirstOrDefaultAsync();

        if (latest is not null)
        {
            latest.IsPaused = isPaused;
            await _context.SaveChangesAsync();
            return;
        }

        var estateExists =
            await _context.Estates
                .AsNoTracking()
                .AnyAsync(e => e.EstateId == estateId);

        if (!estateExists)
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        // No report has ever been recorded for this estate: insert a marker row that
        // carries the paused flag. SentDate stays null so the monthly function still
        // treats the estate as "never sent".
        _context.AutomatedReports.Add(
            new AutomatedReport
            {
                EstateId = estateId,
                ReportPeriod = DateTime.UtcNow,
                RecipientType = "Executor",
                SentDate = null,
                IsPaused = isPaused
            });

        await _context.SaveChangesAsync();
    }

    public async Task AddCashbookEntryAsync(
        CashbookEntry entry)
    {
        _context.Entry(entry).State =
            EntityState.Added;

        await _context.SaveChangesAsync();
    }

    public async Task AddChecklistItemsAsync(
        IEnumerable<ChecklistItem> items)
    {
        foreach (var item in items)
        {
            _context.Entry(item).State =
                EntityState.Added;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCashbookEntryAsync(
        Guid estateId,
        Guid entryId)
    {
        var entry =
            await _context.CashbookEntries
                .FirstOrDefaultAsync(e =>
                    e.EntryId == entryId && e.EstateId == estateId);

        if (entry is null)
            return false;

        _context.CashbookEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCashbookReconciledAsync(
        Guid estateId,
        Guid entryId,
        bool isReconciled)
    {
        var entry =
            await _context.CashbookEntries
                .FirstOrDefaultAsync(e =>
                    e.EntryId == entryId && e.EstateId == estateId);

        if (entry is null)
            return false;

        entry.IsReconciled = isReconciled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteChecklistItemAsync(
        Guid estateId,
        Guid itemId)
    {
        var item =
            await _context.ChecklistItems
                .FirstOrDefaultAsync(i =>
                    i.ItemId == itemId && i.EstateId == estateId);

        if (item is null)
            return false;

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetChecklistStatusAsync(
        Guid estateId,
        Guid itemId,
        string status,
        DateTime? completedDate)
    {
        var item =
            await _context.ChecklistItems
                .FirstOrDefaultAsync(i =>
                    i.ItemId == itemId && i.EstateId == estateId);

        if (item is null)
            return false;

        item.Status = status;
        item.CompletedDate = completedDate;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> NextChecklistSortOrderAsync(
        Guid estateId)
    {
        var max =
            await _context.ChecklistItems
                .Where(i => i.EstateId == estateId)
                .Select(i => (int?)i.SortOrder)
                .MaxAsync();

        return (max ?? 0) + 10;
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(
        Guid estateId,
        int take)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EstateId == estateId)
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .ToListAsync();
    }
}