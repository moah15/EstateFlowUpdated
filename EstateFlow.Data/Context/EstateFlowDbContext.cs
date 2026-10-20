using EstateFlow.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EstateFlow.Data.Context;

public class EstateFlowDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<CashbookEntry> CashbookEntries => Set<CashbookEntry>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public EstateFlowDbContext(DbContextOptions<EstateFlowDbContext> options)
        : base(options) { }

    public DbSet<Estate> Estates => Set<Estate>();
    public DbSet<EstateController> Controllers => Set<EstateController>();
    public DbSet<Deceased> Deceased => Set<Deceased>();
    public DbSet<Executor> Executors => Set<Executor>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<ImmovableProperty> ImmovableProperties => Set<ImmovableProperty>();
    public DbSet<MovableAsset> MovableAssets => Set<MovableAsset>();
    public DbSet<FinancialClaim> FinancialClaims => Set<FinancialClaim>();
    public DbSet<Liability> Liabilities => Set<Liability>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<ProgressStage> ProgressStages => Set<ProgressStage>();
    public DbSet<ScannedDocument> ScannedDocuments => Set<ScannedDocument>();
    public DbSet<AutomatedReport> AutomatedReports => Set<AutomatedReport>();
    public DbSet<EstateAdvertisement> EstateAdvertisements => Set<EstateAdvertisement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Estate>().HasKey(e => e.EstateId);
        modelBuilder.Entity<Estate>().Property(e => e.MastersFee).HasPrecision(18, 2);
        modelBuilder.Entity<Estate>().Property(e => e.EstateDutyRebate).HasPrecision(18, 2);
        modelBuilder.Entity<Estate>().Property(e => e.ExecutorFeePercentage).HasPrecision(5, 2);
        modelBuilder.Entity<Estate>().Property(e => e.VatRate).HasPrecision(5, 2);
        modelBuilder.Entity<Estate>().Property(e => e.EstateDutyRate).HasPrecision(5, 2);
        modelBuilder.Entity<EstateController>().HasKey(e => e.ControllerId);
        modelBuilder.Entity<Deceased>().HasKey(e => e.DeceasedId);
        modelBuilder.Entity<Executor>().HasKey(e => e.ExecutorId);
        modelBuilder.Entity<Beneficiary>().HasKey(e => e.BeneficiaryId);
        modelBuilder.Entity<Beneficiary>().Property(e => e.CashLegacyAmount).HasPrecision(18, 2);

        modelBuilder.Entity<ImmovableProperty>().HasKey(e => e.AssetId);
        modelBuilder.Entity<MovableAsset>().HasKey(e => e.AssetId);
        modelBuilder.Entity<MovableAsset>().Property(e => e.SaleProceeds).HasPrecision(18, 2);
        modelBuilder.Entity<MovableAsset>().Property(e => e.PriceCentsPerShare).HasPrecision(18, 2);
        modelBuilder.Entity<MovableAsset>().Property(e => e.CumulativeDividend).HasPrecision(18, 2);
        modelBuilder.Entity<ImmovableProperty>().Property(e => e.ExtentSquareMetres).HasPrecision(18, 2);
        modelBuilder.Entity<ImmovableProperty>().Property(e => e.MarketValue).HasPrecision(18, 2);
        modelBuilder.Entity<ImmovableProperty>().Property(e => e.SalePrice).HasPrecision(18, 2);
        modelBuilder.Entity<FinancialClaim>().HasKey(e => e.ClaimId);
        modelBuilder.Entity<FinancialClaim>().Property(e => e.AccruedInterest).HasPrecision(18, 2);
        modelBuilder.Entity<FinancialClaim>().Property(e => e.CollectedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Liability>().HasKey(e => e.LiabilityId);
        modelBuilder.Entity<Liability>().Property(e => e.InterestAfterDeath).HasPrecision(18, 2);
        modelBuilder.Entity<Liability>().Property(e => e.AmountPaid).HasPrecision(18, 2);
        modelBuilder.Entity<DiaryEntry>().HasKey(e => e.DiaryId);
        modelBuilder.Entity<ProgressStage>().HasKey(e => e.StageId);
        modelBuilder.Entity<ScannedDocument>().HasKey(e => e.DocumentId);
        modelBuilder.Entity<AutomatedReport>().HasKey(e => e.ReportId);
        modelBuilder.Entity<EstateAdvertisement>().HasKey(e => e.AdvertisementId);

        // Relationships
        modelBuilder.Entity<Estate>()
            .HasOne(e => e.Controller)
            .WithMany(c => c.AssignedEstates)
            .HasForeignKey(e => e.ControllerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Deceased>()
            .HasOne(d => d.Estate)
            .WithOne(e => e.Deceased)
            .HasForeignKey<Deceased>(d => d.EstateId);

        modelBuilder.Entity<Executor>()
            .HasOne(x => x.Estate).WithMany(e => e.Executors)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Beneficiary>()
            .HasOne(x => x.Estate).WithMany(e => e.Beneficiaries)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ImmovableProperty>()
            .HasOne(x => x.Estate).WithMany(e => e.ImmovableProperties)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MovableAsset>()
            .HasOne(x => x.Estate).WithMany(e => e.MovableAssets)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FinancialClaim>()
            .HasOne(x => x.Estate).WithMany(e => e.FinancialClaims)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Liability>()
            .HasOne(x => x.Estate).WithMany(e => e.Liabilities)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiaryEntry>()
            .HasOne(x => x.Estate).WithMany(e => e.DiaryEntries)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProgressStage>()
            .HasOne(x => x.Estate).WithMany(e => e.ProgressStages)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScannedDocument>()
            .HasOne(x => x.Estate).WithMany(e => e.ScannedDocuments)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AutomatedReport>()
            .HasOne(x => x.Estate).WithMany(e => e.AutomatedReports)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EstateAdvertisement>()
            .HasOne(x => x.Estate).WithMany(e => e.Advertisements)
            .HasForeignKey(x => x.EstateId).OnDelete(DeleteBehavior.Cascade);

        foreach (var prop in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal)))
        {
            prop.SetColumnType("decimal(18,2)");
        }
    }
}