using EstateFlow.Api.Services.Reports;
using EstateFlow.Api.Services.Storage;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Interfaces;
using EstateFlow.Core.Models;
using EstateFlow.Core.Validation;
using EstateFlow.Data.Repositories;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;

namespace EstateFlow.Api.Services;

public class EstateService : IEstateService
{
    private readonly IEstateRepository _repository;
    private readonly IBlobStorageService _blobStorage;

    public EstateService(
        IEstateRepository repository,
        IBlobStorageService blobStorage)
    {
        _repository = repository;
        _blobStorage = blobStorage;
    }

    // =====================================================
    // GET ESTATE
    // =====================================================

    public async Task<EstateDto?> GetEstateAsync(
        Guid estateId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId);

        if (estate is null)
            return null;

        return new EstateDto
        {
            EstateId = estate.EstateId,
            FileNumber = estate.FileNumber,
            MastersReference = estate.MastersReference,
            DateOfDeath = estate.DateOfDeath,
            EstateStatus = estate.EstateStatus,
            IsDutiable = estate.IsDutiable,
            FirmActsAsAgent = estate.FirmActsAsAgent,
            AgentName = estate.AgentName,
            AgentReference = estate.AgentReference,
            AgentEmail = estate.AgentEmail,
            AgentPhone = estate.AgentPhone,
            AdministrationExpenses = estate.AdministrationExpenses,
            DateOfAccount = estate.DateOfAccount,
            PlaceOfAccount = estate.PlaceOfAccount,
            AccountType = estate.AccountType,
            IsTestate = estate.IsTestate,
            MastersOffice = estate.MastersOffice,
            LettersOfExecutorshipDate = estate.LettersOfExecutorshipDate,
            ExecutorFeePercentage = estate.ExecutorFeePercentage,
            IsVatRegistered = estate.IsVatRegistered,
            VatRate = estate.VatRate,
            MastersFee = estate.MastersFee,
            EstateDutyRebate = estate.EstateDutyRebate,
            EstateDutyRate = estate.EstateDutyRate,
                        SourceOfEstate = estate.SourceOfEstate,
            EstateBankName = estate.EstateBankName,
            EstateBankBranchCode = estate.EstateBankBranchCode,
            EstateBankAccountNumber = estate.EstateBankAccountNumber,
            HasMultipleLdAccounts = estate.HasMultipleLdAccounts,
            PreviousSpousesCount = estate.PreviousSpousesCount,
            PreviousSpouseRebateUsed = estate.PreviousSpouseRebateUsed,
            EstateDutyHigherRate = estate.EstateDutyHigherRate,
            EstateDutyThreshold = estate.EstateDutyThreshold,
            DeemedPropertyForDuty = estate.DeemedPropertyForDuty,
            DeemedPropertyNote = estate.DeemedPropertyNote,
            OtherDutyDeductions = estate.OtherDutyDeductions,
            OtherDutyDeductionsNote = estate.OtherDutyDeductionsNote,
            ControllerId = estate.ControllerId,

            Controller =
                estate.Controller is null
                    ? null
                    : new EstateControllerDto
                    {
                        ControllerId = estate.Controller.ControllerId,
                        StaffName = estate.Controller.StaffName,
                        Role = estate.Controller.Role,
                        Email = estate.Controller.Email
                    },

            Deceased =
                estate.Deceased is null
                    ? null
                    : new DeceasedDto
                    {
                        DeceasedId =
                            estate.Deceased.DeceasedId,

                        FullName =
                            estate.Deceased.FullName,

                        IdNumber =
                            estate.Deceased.IdNumber,

                        MaritalStatus =
                            estate.Deceased.MaritalStatus,
                        FirstName = estate.Deceased.FirstName,
                        MiddleName = estate.Deceased.MiddleName,
                        Surname = estate.Deceased.Surname,
                        PlaceOfDeath = estate.Deceased.PlaceOfDeath,
                        MatrimonialRegime = estate.Deceased.MatrimonialRegime,
                        SpouseFullName = estate.Deceased.SpouseFullName,
                        SpouseIdNumber = estate.Deceased.SpouseIdNumber,
                        WillDate = estate.Deceased.WillDate,
                        WillLocation = estate.Deceased.WillLocation,
                        Gender = estate.Deceased.Gender,
                        DateOfBirth = estate.Deceased.DateOfBirth,
                        LivedInSouthAfrica = estate.Deceased.LivedInSouthAfrica,
                        CodicilDate = estate.Deceased.CodicilDate

                    },

            Executors =
                estate.Executors
                    .Select(x => new ExecutorDto
                    {
                        ExecutorId = x.ExecutorId,
                        FullName = x.FullName,
                        IdNumber = x.IdNumber,
                        Role = x.Role,
                        Email = x.Email,
                        Phone = x.Phone,
                        Address = x.Address,
                        MastersOffice = x.MastersOffice,
                        LettersOfExecutorshipRef = x.LettersOfExecutorshipRef,
                        LettersOfExecutorshipDate = x.LettersOfExecutorshipDate
                    })
                    .ToList(),

            Beneficiaries =
                estate.Beneficiaries
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new BeneficiaryDto
                    {
                        BeneficiaryId = x.BeneficiaryId,
                        FullName = x.FullName,
                        IdNumber = x.IdNumber,
                        Relationship = x.Relationship,
                        HeirType = x.HeirType,
                        Email = x.Email,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        Surname = x.Surname,
                        RelationshipDescription = x.RelationshipDescription,
                        DateOfBirth = x.DateOfBirth,
                        Gender = x.Gender,
                        MaritalStatus = x.MaritalStatus,
                        WillClause = x.WillClause,
                        CashLegacyAmount = x.CashLegacyAmount,
                        ResidueNumerator = x.ResidueNumerator,
                        ResidueDenominator = x.ResidueDenominator,
                        AdvancesPaid = x.AdvancesPaid,
                        IsSurvivingSpouse = x.IsSurvivingSpouse,
                        IsMinor = x.IsMinor,
                        GuardianOrTrustee = x.GuardianOrTrustee,
                        IsPredeceased = x.IsPredeceased,
                        IsUltimateBeneficiaryPreviousEstate = x.IsUltimateBeneficiaryPreviousEstate,
                        BareDominiumFraction = x.BareDominiumFraction,
                        DisplayOrder = x.DisplayOrder
                    })
                    .ToList(),



            MovableAssets =
                estate.MovableAssets
                    .Select(x => new MovableAssetDto
                    {
                        AssetId = x.AssetId,
                        Description = x.Description,
                        AssetType = x.AssetType,
                        Value = x.Value,
                        IsVehicle = x.IsVehicle,
                        RegistrationNumber = x.RegistrationNumber,
                        VinNumber = x.VinNumber,
                        Make = x.Make,
                        AssetCategory = x.AssetCategory,
                        ValuerName = x.ValuerName,
                        ValuationDate = x.ValuationDate,
                        AwardedToBeneficiaryIds = x.AwardedToBeneficiaryIds,
                        AwardedToNames = ResolveHeirNames(x.AwardedToBeneficiaryIds, estate.Beneficiaries),
                        IsSpecialBequest = x.IsSpecialBequest,
                        IsSoldOrReducedToCash = x.IsSoldOrReducedToCash,
                        SaleProceeds = x.SaleProceeds,
                        SaleDate = x.SaleDate,
                        BuyerName = x.BuyerName,
                        ShareCompany = x.ShareCompany,
                        NumberOfShares = x.NumberOfShares,
                        ShareClass = x.ShareClass,
                        PriceCentsPerShare = x.PriceCentsPerShare,
                        CumulativeDividend = x.CumulativeDividend
                    })
                    .ToList(),

            ImmovableProperties =
                estate.ImmovableProperties
                    .Select(x =>
                        new ImmovablePropertyDto
                        {
                            AssetId = x.AssetId,

                            Description =
                                x.Description,

                            TitleDeedNumber =
                                x.TitleDeedNumber,

                            MunicipalValue =
                                x.MunicipalValue,

                            PropertyType = x.PropertyType,
                            ErfNumber = x.ErfNumber,
                            Township = x.Township,
                            Division = x.Division,
                            Province = x.Province,
                            ExtentSquareMetres = x.ExtentSquareMetres,
                            StreetAddress = x.StreetAddress,
                            SchemeName = x.SchemeName,
                            SectionNumber = x.SectionNumber,
                            ExclusiveUseAreas = x.ExclusiveUseAreas,
                            ShareNumerator = x.ShareNumerator,
                            ShareDenominator = x.ShareDenominator,
                            MarketValue = x.MarketValue,
                            ValuerName = x.ValuerName,
                            ValuationDate = x.ValuationDate,
                            AwardedToBeneficiaryIds = x.AwardedToBeneficiaryIds,
                            AwardedToNames = ResolveHeirNames(x.AwardedToBeneficiaryIds, estate.Beneficiaries),
                            IsSpecialBequest = x.IsSpecialBequest,
                            IsSoldByEstate = x.IsSoldByEstate,
                            SalePrice = x.SalePrice,
                            SaleDate = x.SaleDate,
                            PurchaserName = x.PurchaserName
                        })
                    .ToList(),

            FinancialClaims =
                estate.FinancialClaims
                    .Select(x =>
                        new FinancialClaimDto
                        {
                            ClaimId = x.ClaimId,
                            InstitutionName = x.InstitutionName,
                            InstitutionAddress = x.InstitutionAddress,
                            AccountReference = x.AccountReference,
                            Value = x.Value,
                            ClaimType = x.ClaimType,
                            IsInFavourOfEstate = x.IsInFavourOfEstate,
                            AccruedInterest = x.AccruedInterest,
                            ValuationDate = x.ValuationDate,
                            AwardedToBeneficiaryIds = x.AwardedToBeneficiaryIds,
                            AwardedToNames = ResolveHeirNames(x.AwardedToBeneficiaryIds, estate.Beneficiaries),
                            IsSpecialBequest = x.IsSpecialBequest,
                            IsCollectedAsCash = x.IsCollectedAsCash,
                            CollectedAmount = x.CollectedAmount,
                            CollectedDate = x.CollectedDate,
                            IsCreditLifeInsurance = x.IsCreditLifeInsurance,
                            LinkedLiabilityNote = x.LinkedLiabilityNote
                        })
                    .ToList(),

            Liabilities =
                estate.Liabilities
                    .Select(x => new LiabilityDto
                    {
                        LiabilityId = x.LiabilityId,
                        CreditorName = x.CreditorName,
                        Description = x.Description,
                        Amount = x.Amount,
                        Category = x.Category,
                        AccountReference = x.AccountReference,
                        InterestAfterDeath = x.InterestAfterDeath,
                        Status = x.Status,
                        DateLodged = x.DateLodged,
                        SecuredAgainst = x.SecuredAgainst,
                        IsTakenOverByHeir = x.IsTakenOverByHeir,
                        IsPaid = x.IsPaid,
                        AmountPaid = x.AmountPaid,
                        DatePaid = x.DatePaid,
                        IsCoveredByCreditLife = x.IsCoveredByCreditLife
                    })
                    .ToList(),

            DiaryEntries =
                estate.DiaryEntries
                    .OrderBy(x => x.IsCompleted)
                    .ThenBy(x => x.DueDate)
                    .Select(x =>
                        new DiaryEntryDto
                        {
                            DiaryId = x.DiaryId,
                            DueDate = x.DueDate,
                            Note = x.Note,

                            IsCompleted =
                                x.IsCompleted,

                            NotifyBy = x.NotifyBy
                        })
                    .ToList(),

            ProgressStages =
                estate.ProgressStages
                    .OrderByDescending(x => x.TargetDate)
                    .ThenByDescending(x => x.StageId)
                    .Select(x =>
                        new ProgressStageDto
                        {
                            StageId = x.StageId,

                            StageName =
                                x.StageName,

                            TargetDate =
                                x.TargetDate,

                            Status = x.Status
                        })
                    .ToList(),

            ScannedDocuments =
                estate.ScannedDocuments
                    .Select(x =>
                        new ScannedDocumentDto
                        {
                            DocumentId =
                                x.DocumentId,

                            DocumentType =
                                x.DocumentType,

                            FileName =
                                x.FileName,

                            UploadDate =
                                x.UploadDate
                        })
                    .ToList(),

            CashbookEntries =
                estate.CashbookEntries
                    .OrderBy(x => x.EntryDate)
                    .ThenBy(x => x.EntryId)
                    .Select(MapCashbook)
                    .ToList(),

            ChecklistItems =
                estate.ChecklistItems
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Title)
                    .Select(MapChecklist)
                    .ToList(),

            Advertisements =
                estate.Advertisements
                    .Select(MapAdvertisement)
                    .ToList()
        };
    }

    private static AdvertisementDto MapAdvertisement(EstateAdvertisement x) =>
        new()
        {
            AdvertisementId = x.AdvertisementId,
            AdvertisementType = x.AdvertisementType,
            GazetteNumber = x.GazetteNumber,
            GazetteDate = x.GazetteDate,
            NewspaperName = x.NewspaperName,
            NewspaperDate = x.NewspaperDate,
            ClaimDeadline = x.ClaimDeadline,
            ObjectionsReceived = x.ObjectionsReceived,
            Notes = x.Notes
        };

    // =====================================================
    // GET ESTATES
    // =====================================================

    public async Task<List<EstateSummaryDto>>
        GetAllEstatesAsync()
    {
        var estates =
            await _repository.GetAllAsync();

        return estates
            .Select(MapToSummary)
            .ToList();
    }

    public async Task<List<EstateSummaryDto>>
        GetEstatesForControllerAsync(
            Guid controllerId)
    {
        var estates =
            await _repository
                .GetByControllerIdAsync(
                    controllerId);

        return estates
            .Select(MapToSummary)
            .ToList();
    }

    // =====================================================
    // PROGRESS
    // =====================================================

    public async Task<Guid> AddProgressStageAsync(
        Guid estateId,
        CreateProgressStageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StageName))
        {
            throw new ArgumentException(
                "Progress update is required.");
        }

        var validStatuses =
            new[]
            {
                "Pending",
                "InProgress",
                "Complete"
            };

        var normalizedStatus =
            validStatuses.FirstOrDefault(
                status => string.Equals(
                    status,
                    request.Status,
                    StringComparison.OrdinalIgnoreCase))
            ?? "InProgress";

        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var stage =
            new ProgressStage
            {
                StageId = Guid.NewGuid(),
                EstateId = estateId,
                StageName = request.StageName.Trim(),
                TargetDate = request.TargetDate ?? DateTime.Today,
                Status = normalizedStatus
            };

        await _repository.AddProgressStageAsync(stage);

        return stage.StageId;
    }

    public async Task<Guid> UpdateProgressStageAsync(
        Guid estateId,
        UpdateProgressStageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StageName))
        {
            throw new ArgumentException(
                "Progress update is required.");
        }

        var validStatuses =
            new[]
            {
                "Pending",
                "InProgress",
                "Complete"
            };

        var normalizedStatus =
            validStatuses.FirstOrDefault(
                status => string.Equals(
                    status,
                    request.Status,
                    StringComparison.OrdinalIgnoreCase));

        if (normalizedStatus is null)
        {
            throw new ArgumentException(
                "Progress status must be Pending, InProgress or Complete.");
        }

        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var stage =
            estate.ProgressStages
                .FirstOrDefault(
                    x => x.StageId == request.StageId)
            ?? throw new InvalidOperationException(
                $"Progress update {request.StageId} not found on estate {estateId}.");

        stage.StageName = request.StageName.Trim();
        stage.TargetDate = request.TargetDate ?? DateTime.Today;
        stage.Status = normalizedStatus;

        await _repository.SaveAsync(estate);

        return stage.StageId;
    }

    public async Task DeleteProgressStageAsync(
        Guid estateId,
        Guid stageId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var stage =
            estate.ProgressStages
                .FirstOrDefault(
                    x => x.StageId == stageId)
            ?? throw new InvalidOperationException(
                $"Progress stage {stageId} not found.");

        estate.ProgressStages.Remove(stage);

        await _repository.SaveAsync(estate);
    }

    // =====================================================
    // DIARY
    // =====================================================

    public async Task<List<DiaryEntryWithEstateDto>>
        GetAllDiaryEntriesAsync(
            Guid controllerId)
    {
        var estates =
            await _repository
                .GetByControllerIdAsync(controllerId);

        return estates
            .SelectMany(
                e => e.DiaryEntries.Select(
                    d => new DiaryEntryWithEstateDto
                    {
                        DiaryId = d.DiaryId,
                        EstateId = e.EstateId,
                        FileNumber = e.FileNumber,

                        DeceasedFullName =
                            e.Deceased?.FullName,

                        Note = d.Note,
                        DueDate = d.DueDate,

                        IsCompleted =
                            d.IsCompleted,

                        NotifyBy = d.NotifyBy
                    }))
            .OrderBy(d => d.IsCompleted)
            .ThenBy(d => d.DueDate)
            .ToList();
    }

    public async Task<Guid> AddDiaryEntryAsync(
        Guid estateId,
        CreateDiaryEntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.Note))
        {
            throw new ArgumentException(
                "Diary note is required.");
        }

        if (request.DueDate == default)
        {
            throw new ArgumentException(
                "Diary due date is required.");
        }

        var validNotificationMethods =
            new[]
            {
                "Email",
                "InApp",
                "Both"
            };

        var notifyBy =
            validNotificationMethods
                .FirstOrDefault(
                    x => string.Equals(
                        x,
                        request.NotifyBy,
                        StringComparison.OrdinalIgnoreCase));

        if (notifyBy is null)
        {
            throw new ArgumentException(
                "Notification method must be Email, InApp or Both.");
        }

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var diaryEntry =
            new DiaryEntry
            {
                DiaryId = Guid.NewGuid(),
                EstateId = estateId,
                Note = request.Note.Trim(),
                DueDate = request.DueDate,
                NotifyBy = notifyBy,
                IsCompleted = false
            };

        await _repository.AddDiaryEntryAsync(
            diaryEntry);

        return diaryEntry.DiaryId;
    }

    public async Task UpdateDiaryEntryStatusAsync(
        Guid estateId,
        Guid diaryId,
        bool isCompleted)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var diaryEntry =
            estate.DiaryEntries
                .FirstOrDefault(
                    x => x.DiaryId == diaryId)
            ?? throw new InvalidOperationException(
                $"Diary entry {diaryId} not found.");

        diaryEntry.IsCompleted =
            isCompleted;

        await _repository.SaveAsync(estate);
    }

    public async Task DeleteDiaryEntryAsync(
        Guid estateId,
        Guid diaryId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var diaryEntry =
            estate.DiaryEntries
                .FirstOrDefault(
                    x => x.DiaryId == diaryId)
            ?? throw new InvalidOperationException(
                $"Diary entry {diaryId} not found.");

        estate.DiaryEntries.Remove(
            diaryEntry);

        await _repository.SaveAsync(estate);
    }

    // =====================================================
    // REPORTS
    // =====================================================

    public async Task<List<ReportSettingDto>>
        GetReportSettingsAsync(
            Guid controllerId)
    {
        var estates =
            await _repository
                .GetByControllerIdAsync(controllerId);

        return estates.Select(e =>
        {
            var latestReport =
                e.AutomatedReports
                    .OrderByDescending(
                        r => r.ReportPeriod)
                    .FirstOrDefault();

            return new ReportSettingDto
            {
                EstateId = e.EstateId,
                FileNumber = e.FileNumber,

                DeceasedFullName =
                    e.Deceased?.FullName,

                LastSentDate =
                    latestReport?.SentDate,

                IsPaused =
                    latestReport?.IsPaused
                    ?? false
            };
        }).ToList();
    }

    public async Task ToggleReportPauseAsync(
        Guid estateId,
        bool isPaused)
    {
        await _repository.SetReportPausedAsync(
            estateId,
            isPaused);
    }

    // =====================================================
    // ESTATE
    // =====================================================

    public async Task<Guid> CreateEstateAsync(
        CreateEstateRequest request,
        Guid controllerId)
    {
        if (string.IsNullOrWhiteSpace(
                request.FileNumber))
        {
            throw new ArgumentException(
                "File number is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.DeceasedFullName))
        {
            throw new ArgumentException(
                "Deceased full name is required.");
        }

        var deceasedIdError =
            SaIdNumber.Validate(
                request.DeceasedIdNumber,
                request.DeceasedDateOfBirth,
                "Deceased");

        if (deceasedIdError is not null)
            throw new ArgumentException(deceasedIdError);

        var estate =
            new Estate
            {
                FileNumber =
                    request.FileNumber,

                MastersReference =
                    request.MastersReference,

                DateOfDeath =
                    request.DateOfDeath,

                IsDutiable =
                    request.IsDutiable,

                FirmActsAsAgent = request.FirmActsAsAgent,
                AgentName = request.AgentName,
                AgentReference = request.AgentReference,
                AgentEmail = request.AgentEmail,
                AgentPhone = request.AgentPhone,
                AdministrationExpenses = request.AdministrationExpenses,
                DateOfAccount = request.DateOfAccount,
                PlaceOfAccount = request.PlaceOfAccount,
                AccountType = request.AccountType,
                IsTestate = request.IsTestate,
                MastersOffice = request.MastersOffice,
                LettersOfExecutorshipDate = request.LettersOfExecutorshipDate,
                ExecutorFeePercentage = request.ExecutorFeePercentage,
                IsVatRegistered = request.IsVatRegistered,
                VatRate = request.VatRate,
                MastersFee = request.MastersFee,
                EstateDutyRebate = request.EstateDutyRebate,
                EstateDutyRate = request.EstateDutyRate,
                SourceOfEstate = request.SourceOfEstate,
                EstateBankName = request.EstateBankName,
                EstateBankBranchCode = request.EstateBankBranchCode,
                EstateBankAccountNumber = request.EstateBankAccountNumber,
                HasMultipleLdAccounts = request.HasMultipleLdAccounts,
                PreviousSpousesCount = request.PreviousSpousesCount,
                PreviousSpouseRebateUsed = request.PreviousSpouseRebateUsed,
                EstateDutyHigherRate = request.EstateDutyHigherRate,
                EstateDutyThreshold = request.EstateDutyThreshold,
                DeemedPropertyForDuty = request.DeemedPropertyForDuty,
                DeemedPropertyNote = request.DeemedPropertyNote,
                OtherDutyDeductions = request.OtherDutyDeductions,
                OtherDutyDeductionsNote = request.OtherDutyDeductionsNote,

                EstateStatus =
                    "Reported",

                ControllerId =
                    controllerId,

                Deceased =
                    new Deceased
                    {
                        FullName =
                            request.DeceasedFullName,

                        IdNumber =
                            request.DeceasedIdNumber,

                        MaritalStatus =
                            request.DeceasedMaritalStatus,
                        FirstName = request.DeceasedFirstName,
                        MiddleName = request.DeceasedMiddleName,
                        Surname = request.DeceasedSurname,
                        PlaceOfDeath = request.DeceasedPlaceOfDeath,
                        MatrimonialRegime = request.DeceasedMatrimonialRegime,
                        SpouseFullName = request.DeceasedSpouseFullName,
                        SpouseIdNumber = request.DeceasedSpouseIdNumber,
                        WillDate = request.DeceasedWillDate,
                        WillLocation = request.DeceasedWillLocation,
                        Gender = request.DeceasedGender,
                        DateOfBirth = request.DeceasedDateOfBirth,
                        LivedInSouthAfrica = request.DeceasedLivedInSouthAfrica,
                        CodicilDate = request.DeceasedCodicilDate
                    }
            };

        var created =
            await _repository.AddAsync(
                estate);

        return created.EstateId;
    }

    public async Task UpdateEstateAsync(
    Guid estateId,
    UpdateEstateRequest request)
    {
        var deceasedIdError =
            SaIdNumber.Validate(
                request.DeceasedIdNumber,
                request.DeceasedDateOfBirth,
                "Deceased");

        if (deceasedIdError is not null)
            throw new ArgumentException(deceasedIdError);

        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        estate.FileNumber =
            request.FileNumber;

        estate.MastersReference =
            request.MastersReference;

        estate.DateOfDeath =
            request.DateOfDeath;

        estate.IsDutiable =
            request.IsDutiable;

        estate.FirmActsAsAgent = request.FirmActsAsAgent;
        estate.AgentName = request.AgentName;
        estate.AgentReference = request.AgentReference;
        estate.AgentEmail = request.AgentEmail;
        estate.AgentPhone = request.AgentPhone;
        estate.AdministrationExpenses = request.AdministrationExpenses;

        estate.DateOfAccount = request.DateOfAccount;
        estate.PlaceOfAccount = request.PlaceOfAccount;
        estate.AccountType = request.AccountType;
        estate.IsTestate = request.IsTestate;
        estate.MastersOffice = request.MastersOffice;
        estate.LettersOfExecutorshipDate = request.LettersOfExecutorshipDate;
        estate.ExecutorFeePercentage = request.ExecutorFeePercentage;
        estate.IsVatRegistered = request.IsVatRegistered;
        estate.VatRate = request.VatRate;
        estate.MastersFee = request.MastersFee;
        estate.EstateDutyRebate = request.EstateDutyRebate;
        estate.EstateDutyRate = request.EstateDutyRate;
        estate.SourceOfEstate = request.SourceOfEstate;
        estate.EstateBankName = request.EstateBankName;
        estate.EstateBankBranchCode = request.EstateBankBranchCode;
        estate.EstateBankAccountNumber = request.EstateBankAccountNumber;
        estate.HasMultipleLdAccounts = request.HasMultipleLdAccounts;
        estate.PreviousSpousesCount = request.PreviousSpousesCount;
        estate.PreviousSpouseRebateUsed = request.PreviousSpouseRebateUsed;
        estate.EstateDutyHigherRate = request.EstateDutyHigherRate;
        estate.EstateDutyThreshold = request.EstateDutyThreshold;
        estate.DeemedPropertyForDuty = request.DeemedPropertyForDuty;
        estate.DeemedPropertyNote = request.DeemedPropertyNote;
        estate.OtherDutyDeductions = request.OtherDutyDeductions;
        estate.OtherDutyDeductionsNote = request.OtherDutyDeductionsNote;

        estate.EstateStatus =
            request.EstateStatus;

        if (estate.Deceased is null)
        {
            estate.Deceased =
                new Deceased
                {
                    EstateId = estateId,

                    FullName =
                        request.DeceasedFullName,

                    IdNumber =
                        request.DeceasedIdNumber,

                    MaritalStatus =
                        request.DeceasedMaritalStatus,

                    FirstName = request.DeceasedFirstName,
                    MiddleName = request.DeceasedMiddleName,
                    Surname = request.DeceasedSurname,
                    PlaceOfDeath = request.DeceasedPlaceOfDeath,
                    MatrimonialRegime = request.DeceasedMatrimonialRegime,
                    SpouseFullName = request.DeceasedSpouseFullName,
                    SpouseIdNumber = request.DeceasedSpouseIdNumber,
                    WillDate = request.DeceasedWillDate,
                    WillLocation = request.DeceasedWillLocation,
                    Gender = request.DeceasedGender,
                    DateOfBirth = request.DeceasedDateOfBirth,
                    LivedInSouthAfrica = request.DeceasedLivedInSouthAfrica,
                    CodicilDate = request.DeceasedCodicilDate
                };
        }
        else
        {
            estate.Deceased.FullName =
                request.DeceasedFullName;

            estate.Deceased.IdNumber =
                request.DeceasedIdNumber;

            estate.Deceased.MaritalStatus =
                request.DeceasedMaritalStatus;

            estate.Deceased.FirstName = request.DeceasedFirstName;
            estate.Deceased.MiddleName = request.DeceasedMiddleName;
            estate.Deceased.Surname = request.DeceasedSurname;
            estate.Deceased.PlaceOfDeath = request.DeceasedPlaceOfDeath;
            estate.Deceased.MatrimonialRegime = request.DeceasedMatrimonialRegime;
            estate.Deceased.SpouseFullName = request.DeceasedSpouseFullName;
            estate.Deceased.SpouseIdNumber = request.DeceasedSpouseIdNumber;
            estate.Deceased.WillDate = request.DeceasedWillDate;
            estate.Deceased.WillLocation = request.DeceasedWillLocation;
            estate.Deceased.Gender = request.DeceasedGender;
            estate.Deceased.DateOfBirth = request.DeceasedDateOfBirth;
            estate.Deceased.LivedInSouthAfrica = request.DeceasedLivedInSouthAfrica;
            estate.Deceased.CodicilDate = request.DeceasedCodicilDate;
        }

        await _repository.SaveAsync(estate);
    }

    public async Task DeleteEstateAsync(
        Guid estateId)
    {
        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        await _repository.DeleteAsync(
            estateId);
    }

    // =====================================================
    // EXECUTORS
    // =====================================================

    public async Task<Guid> AddExecutorAsync(
        Guid estateId,
        CreateExecutorRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.FullName))
        {
            throw new ArgumentException(
                "Executor full name is required.");
        }

        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException(
                "A valid executor email address is required.");
        }

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var executor =
            new Executor
            {
                ExecutorId = Guid.NewGuid(),
                EstateId = estateId,

                FullName =
                    request.FullName.Trim(),

                IdNumber =
                    request.IdNumber?.Trim()
                    ?? string.Empty,

                Role =
                    request.Role?.Trim()
                    ?? string.Empty,

                Email =
                    request.Email.Trim()
            };

        await _repository.AddExecutorAsync(
            executor);

        return executor.ExecutorId;
    }

    public async Task DeleteExecutorAsync(
        Guid estateId,
        Guid executorId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var executor =
            estate.Executors
                .FirstOrDefault(
                    x => x.ExecutorId ==
                         executorId)
            ?? throw new InvalidOperationException(
                $"Executor {executorId} not found.");

        estate.Executors.Remove(
            executor);

        await _repository.SaveAsync(
            estate);
    }
    



// ─────────────────────────────────────────────────────────────────────────────
// Also UPDATE GetEstateAsync mapping to include new executor fields + ads:
// In the ExecutorDto mapping block, add:
//
//     Phone                  = e.Phone,
//     Address                = e.Address,
//     MastersOffice          = e.MastersOffice,
//     LettersOfExecutorshipRef  = e.LettersOfExecutorshipRef,
//     LettersOfExecutorshipDate = e.LettersOfExecutorshipDate,
//     LettersDocumentUrl     = e.LettersDocumentUrl,
//     LettersDocumentName    = e.LettersDocumentName,
//
// In the EstateDto mapping, add:
//
//     Advertisements = estate.Advertisements.Select(a => new AdvertisementDto
//     {
//         AdvertisementId    = a.AdvertisementId,
//         AdvertisementType  = a.AdvertisementType,
//         GazetteNumber      = a.GazetteNumber,
//         GazetteDate        = a.GazetteDate,
//         NewspaperName      = a.NewspaperName,
//         NewspaperDate      = a.NewspaperDate,
//         ClaimDeadline      = a.ClaimDeadline,
//         ObjectionsReceived = a.ObjectionsReceived,
//         Notes              = a.Notes,
//     }).ToList(),
// ─────────────────────────────────────────────────────────────────────────────


    public async Task UpdateExecutorAsync(
    Guid estateId,
    Guid executorId,
    UpdateExecutorRequest request)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var executor =
            estate.Executors
                .FirstOrDefault(x => x.ExecutorId == executorId)
            ?? throw new InvalidOperationException(
                $"Executor {executorId} not found.");

        executor.FullName = request.FullName.Trim();
        executor.IdNumber = request.IdNumber?.Trim() ?? executor.IdNumber;
        executor.Email = request.Email?.Trim() ?? executor.Email;
        executor.Phone = request.Phone?.Trim();
        executor.Address = request.Address?.Trim();
        executor.Role = request.Role?.Trim() ?? executor.Role;
        executor.MastersOffice = request.MastersOffice?.Trim();
        executor.LettersOfExecutorshipRef = request.LettersOfExecutorshipRef?.Trim();
        executor.LettersOfExecutorshipDate = request.LettersOfExecutorshipDate;

        await _repository.SaveAsync(estate);
    }

    public async Task<string> UploadExecutorLettersAsync(
        Guid estateId,
        Guid executorId,
        IFormFile file)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var executor =
            estate.Executors
                .FirstOrDefault(x => x.ExecutorId == executorId)
            ?? throw new InvalidOperationException(
                $"Executor {executorId} not found.");

        var ext = Path.GetExtension(file.FileName);
        var blobName = $"estates/{estateId}/executors/{executorId}/letters{ext}";

        await using var stream = file.OpenReadStream();
        await _blobStorage.UploadAsync(stream, blobName, file.ContentType);

        executor.LettersOfExecutorshipRef = blobName;
        await _repository.SaveAsync(estate);

        return blobName;
    }

    public async Task UpsertAdvertisementAsync(
        Guid estateId,
        UpsertAdvertisementRequest request)
    {
        if (request.AdvertisementType is not ("Section29" or "Section35"))
        {
            throw new ArgumentException(
                "Advertisement type must be Section29 or Section35.");
        }

        if (!await _repository.ExistsAsync(estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        await _repository.UpsertAdvertisementAsync(
            new EstateAdvertisement
            {
                EstateId = estateId,
                AdvertisementType = request.AdvertisementType,
                GazetteNumber = request.GazetteNumber,
                GazetteDate = request.GazetteDate,
                NewspaperName = request.NewspaperName,
                NewspaperDate = request.NewspaperDate,
                ClaimDeadline = request.ClaimDeadline,
                ObjectionsReceived = request.ObjectionsReceived,
                Notes = request.Notes
            });
    }

    // =====================================================
    // BENEFICIARIES
    // =====================================================

    public async Task<Guid> AddBeneficiaryAsync(
        Guid estateId,
        CreateBeneficiaryRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.FullName))
        {
            throw new ArgumentException(
                "Beneficiary full name is required.");
        }

        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException(
                "A valid beneficiary email address is required.");
        }

        var heirIdError =
            SaIdNumber.Validate(
                request.IdNumber,
                request.DateOfBirth,
                "Heir");

        if (heirIdError is not null)
            throw new ArgumentException(heirIdError);

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var beneficiary =
    new Beneficiary
    {
        BeneficiaryId = Guid.NewGuid(),
        EstateId = estateId,
        FullName = request.FullName.Trim(),
        IdNumber = request.IdNumber?.Trim() ?? string.Empty,
        Relationship = request.Relationship?.Trim() ?? string.Empty,
        HeirType = request.HeirType?.Trim() ?? string.Empty,
        Email = request.Email.Trim(),
        FirstName = request.FirstName?.Trim(),
        MiddleName = request.MiddleName?.Trim(),
        Surname = request.Surname?.Trim(),
        RelationshipDescription = request.RelationshipDescription?.Trim(),
        DateOfBirth = request.DateOfBirth,
        Gender = request.Gender,
        MaritalStatus = request.MaritalStatus,
        WillClause = request.WillClause?.Trim(),
        CashLegacyAmount = request.CashLegacyAmount,
        ResidueNumerator = request.ResidueNumerator,
        ResidueDenominator = request.ResidueDenominator,
        AdvancesPaid = request.AdvancesPaid,
        IsSurvivingSpouse = request.IsSurvivingSpouse,
        IsMinor = request.IsMinor,
        GuardianOrTrustee = request.GuardianOrTrustee?.Trim(),
        IsPredeceased = request.IsPredeceased,
        IsUltimateBeneficiaryPreviousEstate = request.IsUltimateBeneficiaryPreviousEstate,
        BareDominiumFraction = request.BareDominiumFraction?.Trim(),
        DisplayOrder = request.DisplayOrder
    };

        await _repository
            .AddBeneficiaryAsync(
                beneficiary);

        return beneficiary.BeneficiaryId;
    }

    public async Task DeleteBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var beneficiary =
            estate.Beneficiaries
                .FirstOrDefault(
                    x => x.BeneficiaryId ==
                         beneficiaryId)
            ?? throw new InvalidOperationException(
                $"Beneficiary {beneficiaryId} not found.");

        estate.Beneficiaries.Remove(
            beneficiary);

        await _repository.SaveAsync(
            estate);
    }

    // =====================================================
    // MOVABLE ASSETS
    // =====================================================
    private static string ResolveHeirNames(
    string? ids,
    IEnumerable<Beneficiary> heirs)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return string.Empty;

        var wanted =
            ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
               .ToHashSet();

        return string.Join(", ",
            heirs.Where(h => wanted.Contains(h.BeneficiaryId))
                 .Select(h => h.FullName));
    }
    public async Task<Guid> AddMovableAssetAsync(
        Guid estateId,
        CreateMovableAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.Description))
        {
            throw new ArgumentException(
                "Asset description is required.");
        }

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var asset =
            new MovableAsset
            {
                AssetId = Guid.NewGuid(),
                EstateId = estateId,

                Description =
                    request.Description,

                AssetType =
                    request.AssetType,

                Value = request.Value,

                IsVehicle =
                    request.IsVehicle,

                RegistrationNumber =
                    request.RegistrationNumber,

                VinNumber =
                    request.VinNumber,

                Make =
                    request.Make,
                                
                AssetCategory = request.AssetCategory,
                ValuerName = request.ValuerName?.Trim(),
                ValuationDate = request.ValuationDate,
                AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds,
                IsSpecialBequest = request.IsSpecialBequest,
                IsSoldOrReducedToCash = request.IsSoldOrReducedToCash,
                SaleProceeds = request.SaleProceeds,
                SaleDate = request.SaleDate,
                BuyerName = request.BuyerName?.Trim(),
                ShareCompany = request.ShareCompany?.Trim(),
                NumberOfShares = request.NumberOfShares,
                ShareClass = request.ShareClass?.Trim(),
                PriceCentsPerShare = request.PriceCentsPerShare,
                CumulativeDividend = request.CumulativeDividend

            };

        await _repository.AddMovableAssetAsync(
            asset);

        return asset.AssetId;
    }

    public async Task DeleteMovableAssetAsync(
        Guid estateId,
        Guid assetId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var asset =
            estate.MovableAssets
                .FirstOrDefault(
                    x => x.AssetId == assetId)
            ?? throw new InvalidOperationException(
                $"Movable asset {assetId} not found.");

        estate.MovableAssets.Remove(
            asset);

        await _repository.SaveAsync(
            estate);
    }

    // =====================================================
    // IMMOVABLE PROPERTIES
    // =====================================================

    public async Task<Guid>
        AddImmovablePropertyAsync(
            Guid estateId,
            CreateImmovablePropertyRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.Description))
        {
            throw new ArgumentException(
                "Property description is required.");
        }

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var property =
            new ImmovableProperty
            {
                AssetId = Guid.NewGuid(),
                EstateId = estateId,

                Description =
                    request.Description,

                TitleDeedNumber =
                    request.TitleDeedNumber,

                MunicipalValue =
                    request.MunicipalValue,

                PropertyType = request.PropertyType,
                ErfNumber = request.ErfNumber?.Trim(),
                Township = request.Township?.Trim(),
                Division = request.Division?.Trim(),
                Province = request.Province,
                ExtentSquareMetres = request.ExtentSquareMetres,
                StreetAddress = request.StreetAddress?.Trim(),
                SchemeName = request.SchemeName?.Trim(),
                SectionNumber = request.SectionNumber?.Trim(),
                ExclusiveUseAreas = request.ExclusiveUseAreas?.Trim(),
                ShareNumerator = request.ShareNumerator,
                ShareDenominator = request.ShareDenominator,
                MarketValue = request.MarketValue,
                ValuerName = request.ValuerName?.Trim(),
                ValuationDate = request.ValuationDate,
                AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds,
                IsSpecialBequest = request.IsSpecialBequest,
                IsSoldByEstate = request.IsSoldByEstate,
                SalePrice = request.SalePrice,
                SaleDate = request.SaleDate,
                PurchaserName = request.PurchaserName?.Trim()
            };

        await _repository
            .AddImmovablePropertyAsync(
                property);

        return property.AssetId;
    }

    public async Task DeleteImmovablePropertyAsync(
        Guid estateId,
        Guid assetId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var property =
            estate.ImmovableProperties
                .FirstOrDefault(
                    x => x.AssetId == assetId)
            ?? throw new InvalidOperationException(
                $"Immovable property {assetId} not found.");

        estate.ImmovableProperties.Remove(
            property);

        await _repository.SaveAsync(
            estate);
    }

    // =====================================================
    // FINANCIAL CLAIMS
    // =====================================================

    public async Task<Guid>
        AddFinancialClaimAsync(
            Guid estateId,
            CreateFinancialClaimRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.InstitutionName))
        {
            throw new ArgumentException(
                "Institution name is required.");
        }

        if (!await _repository.ExistsAsync(
                estateId))
        {
            throw new InvalidOperationException(
                $"Estate {estateId} not found.");
        }

        var claim =
            new FinancialClaim
            {
                ClaimId = Guid.NewGuid(),
                EstateId = estateId,

                InstitutionName =
                    request.InstitutionName,

                InstitutionAddress =
                    request.InstitutionAddress,

                AccountReference =
                    request.AccountReference,

                Value = request.Value,
                ClaimType = request.ClaimType,
                IsInFavourOfEstate = request.IsInFavourOfEstate,
                AccruedInterest = request.AccruedInterest,
                ValuationDate = request.ValuationDate,
                AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds,
                IsSpecialBequest = request.IsSpecialBequest,
                IsCollectedAsCash = request.IsCollectedAsCash,
                CollectedAmount = request.CollectedAmount,
                CollectedDate = request.CollectedDate,
                IsCreditLifeInsurance = request.IsCreditLifeInsurance,
                LinkedLiabilityNote = request.LinkedLiabilityNote
            };

        await _repository
            .AddFinancialClaimAsync(
                claim);

        return claim.ClaimId;
    }

    public async Task DeleteFinancialClaimAsync(
        Guid estateId,
        Guid claimId)
    {
        var estate =
            await _repository.GetByIdAsync(
                estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var claim =
            estate.FinancialClaims
                .FirstOrDefault(
                    x => x.ClaimId == claimId)
            ?? throw new InvalidOperationException(
                $"Financial claim {claimId} not found.");

        estate.FinancialClaims.Remove(
            claim);

        await _repository.SaveAsync(
            estate);
    }

    // =====================================================
    // MAPPING
    // =====================================================

    private static EstateSummaryDto MapToSummary(
        Estate estate)
    {
        return new EstateSummaryDto
        {
            EstateId = estate.EstateId,
            FileNumber = estate.FileNumber,

            DeceasedFullName =
                estate.Deceased?.FullName,

            EstateStatus =
                estate.EstateStatus,

            DateOfDeath =
                estate.DateOfDeath,

            ControllerName =
                estate.Controller?.StaffName,

            OpenDiaryCount =
                estate.DiaryEntries.Count(
                    d => !d.IsCompleted)
        };
    }

    // =====================================================
    // VALIDATION
    // =====================================================

    private static bool IsValidEmail(
        string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var address =
                new MailAddress(email.Trim());

            return string.Equals(
                address.Address,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    // =====================================================
    // LIABILITIES
    // =====================================================

    public async Task<Guid> AddLiabilityAsync(
        Guid estateId,
        CreateLiabilityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CreditorName))
            throw new ArgumentException("Creditor name is required.");

        if (request.Amount < 0)
            throw new ArgumentException("Liability amount cannot be negative.");

        if (!await _repository.ExistsAsync(estateId))
            throw new InvalidOperationException($"Estate {estateId} not found.");

        var liability = new Liability
        {
            LiabilityId = Guid.NewGuid(),
            EstateId = estateId,
            CreditorName = request.CreditorName.Trim(),
            Description = request.Description?.Trim(),
            Amount = request.Amount,
            Category = request.Category,
            AccountReference = request.AccountReference?.Trim(),
            InterestAfterDeath = request.InterestAfterDeath,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Admitted" : request.Status,
            DateLodged = request.DateLodged,
            SecuredAgainst = request.SecuredAgainst?.Trim(),
            IsTakenOverByHeir = request.IsTakenOverByHeir,
            IsPaid = request.IsPaid,
            AmountPaid = request.IsPaid ? request.AmountPaid : 0,
            DatePaid = request.IsPaid ? request.DatePaid : null,
            IsCoveredByCreditLife = request.IsCoveredByCreditLife
        };

        await _repository.AddLiabilityAsync(liability);
        return liability.LiabilityId;
    }

    public async Task DeleteLiabilityAsync(
        Guid estateId,
        Guid liabilityId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var liability =
            estate.Liabilities.FirstOrDefault(
                x => x.LiabilityId == liabilityId)
            ?? throw new InvalidOperationException(
                $"Liability {liabilityId} not found.");

        estate.Liabilities.Remove(liability);
        await _repository.SaveAsync(estate);
    }

    // =====================================================
    // LIQUIDATION & DISTRIBUTION ACCOUNT (PDF)
    // =====================================================

    public async Task<(byte[] Content, string FileName)>
        GetLiquidationAccountPdfAsync(Guid estateId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var account = LiquidationAccountBuilder.Build(estate);
        var bytes = LiquidationAccountPdf.Generate(account);

        var safeFile = string.Concat(
            estate.FileNumber.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_'));

        return (bytes, $"LD_Account_{safeFile}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // =====================================================
    // LIQUIDATION & DISTRIBUTION DRAFT
    // =====================================================

    public async Task<LiquidationDistributionDraftDto>
        GetLiquidationDistributionDraftAsync(Guid estateId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        // One calculation engine: the screen, the PDF and the checks all come from here.
        var a = LiquidationAccountBuilder.Build(estate);

        var heirCount = a.Heirs.Count;

        var result = new LiquidationDistributionDraftDto
        {
            EstateId = estate.EstateId,
            FileNumber = estate.FileNumber,
            MastersReference = estate.MastersReference,
            DeceasedName = a.DeceasedName,

            MovableAssetsTotal = a.MovableTotal,
            ImmovableAssetsTotal = a.ImmovableTotal,
            FinancialClaimsTotal = a.ClaimsTotal,
            GrossEstateTotal = a.GrossTotal,
            LiabilitiesTotal = a.LiabilitiesTotal,

            InCommunity = a.InCommunity,
            SpouseHalf = a.SpouseHalf,
            DeceasedGrossTotal = a.DeceasedGross,
            DeceasedLiabilitiesTotal = a.DeceasedLiabilities,

            AdministrationExpenses = a.CostsTotal,
            MastersFee = a.MastersFee,
            MastersFeeFromScale = a.MastersFeeFromScale,
            IncomeAfterDeath = a.IncomeTotal,
            CapitalCostsTotal = a.CapitalCostsTotal,
            IncomeCostsTotal = a.IncomeCostsTotal,
            TotalDeductions = a.DeceasedLiabilities + a.CostsTotal,

            DeemedProperty = a.DeemedProperty,
            NetValueForDuty = a.NetValueForDuty,
            SpouseDeduction = a.SpouseDeduction,
            CharityDeduction = a.CharityDeduction,
            OtherDeductions = a.OtherDeductions,
            Abatement = a.Abatement,
            DutiableAmount = a.DutiableAmount,
            DutyLowRate = a.DutyLowRate,
            DutyHighRate = a.DutyHighRate,
            DutyThreshold = a.DutyThreshold,
            DutyOnLowBand = a.DutyOnLowBand,
            DutyOnHighBand = a.DutyOnHighBand,
            EstateDuty = a.EstateDuty,

            NetEstateBeforeDuty = a.NetEstate,
            CashLegaciesTotal = a.CashLegaciesTotal,
            SpecialBequestsTotal = a.SpecialBequestsTotal,
            ResidueForDistribution = a.Residue,

            BeneficiaryCount = heirCount,
            EqualIndicativeShare = heirCount > 0 ? Math.Round(a.Residue / heirCount, 2) : 0m
        };

        result.Assets.AddRange(a.Immovable.Select(x => new LiquidationDistributionLineDto
            { Category = "Immovable property", Description = x.Description, Note = x.Note, Amount = x.Amount }));
        result.Assets.AddRange(a.Movable.Select(x => new LiquidationDistributionLineDto
            { Category = "Movable asset", Description = x.Description, Note = x.Note, Amount = x.Amount }));
        result.Assets.AddRange(a.Claims.Select(x => new LiquidationDistributionLineDto
            { Category = "Claim in favour of the estate", Description = x.Description, Note = x.Note, Amount = x.Amount }));

        result.Deductions.AddRange(a.Liabilities.Select(x => new LiquidationDistributionLineDto
            { Category = "Liability", Description = x.Description, Note = x.Note, Amount = x.Amount }));

        result.Income.AddRange(a.Income.Select(x => new LiquidationDistributionLineDto
            { Category = "Income after death", Description = x.Description, Note = x.Note, Amount = x.Amount }));

        result.Costs.AddRange(a.Costs.Select(x => new LiquidationDistributionLineDto
            { Category = "Cost", Description = x.Description, Note = x.Note, Amount = x.Amount }));

        result.Beneficiaries.AddRange(a.Heirs.Select(h => new LiquidationDistributionBeneficiaryDto
        {
            FullName = h.Name,
            Relationship = h.Relationship,
            HeirType = h.HeirType,
            ResidueFraction = h.ResidueFraction,
            CashLegacy = h.CashLegacy,
            SpecialBequests = h.SpecialBequests,
            ResidueShare = h.ResidueShare,
            AdvancesPaid = h.AdvancesPaid,
            TotalDistribution = h.Total,
            IndicativeEqualShare = result.EqualIndicativeShare
        }));

        result.Notes.AddRange(a.Notes);
        result.Checks.AddRange(a.Checks);

        return result;
    }

    // =====================================================
    // CASHBOOK
    // =====================================================

    private static CashbookEntryDto MapCashbook(CashbookEntry x) =>
        new()
        {
            EntryId = x.EntryId,
            EntryDate = x.EntryDate,
            EntryType = x.EntryType,
            Category = x.Category,
            Description = x.Description,
            PayerOrPayee = x.PayerOrPayee,
            Reference = x.Reference,
            Amount = x.Amount,
            IsReconciled = x.IsReconciled
        };

    public async Task<Guid> AddCashbookEntryAsync(
        Guid estateId,
        CreateCashbookEntryRequest request)
    {
        if (request.Amount <= 0m)
            throw new ArgumentException("The amount must be greater than zero.");

        if (!CashbookTypes.All.Contains(request.EntryType))
            throw new ArgumentException("Choose a valid cashbook entry type.");

        if (string.IsNullOrWhiteSpace(request.Description) &&
            string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Enter a description or a category.");

        if (!await _repository.ExistsAsync(estateId))
            throw new InvalidOperationException($"Estate {estateId} not found.");

        var entry = new CashbookEntry
        {
            EstateId = estateId,
            EntryDate = request.EntryDate.Date,
            EntryType = request.EntryType,
            Category = (request.Category ?? string.Empty).Trim(),
            Description = (request.Description ?? string.Empty).Trim(),
            PayerOrPayee = string.IsNullOrWhiteSpace(request.PayerOrPayee) ? null : request.PayerOrPayee.Trim(),
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
            Amount = Math.Round(request.Amount, 2),
            IsReconciled = false
        };

        await _repository.AddCashbookEntryAsync(entry);
        return entry.EntryId;
    }

    public async Task<(byte[] Content, string FileName)>
        GetCashbookPdfAsync(Guid estateId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException(
                $"Estate {estateId} not found.");

        var bytes = CashbookPdf.Generate(estate);

        var safeFile = string.Concat(
            estate.FileNumber.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_'));

        return (bytes, $"Cashbook_{safeFile}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    public async Task DeleteCashbookEntryAsync(Guid estateId, Guid entryId)
    {
        if (!await _repository.DeleteCashbookEntryAsync(estateId, entryId))
            throw new InvalidOperationException($"Cashbook entry {entryId} not found.");
    }

    public async Task SetCashbookReconciledAsync(Guid estateId, Guid entryId, bool isReconciled)
    {
        if (!await _repository.SetCashbookReconciledAsync(estateId, entryId, isReconciled))
            throw new InvalidOperationException($"Cashbook entry {entryId} not found.");
    }

    // =====================================================
    // CHECKLIST (SARS / Master / creditors / inspection / transfers / closure)
    // =====================================================

    private static ChecklistItemDto MapChecklist(ChecklistItem x) =>
        new()
        {
            ItemId = x.ItemId,
            Category = x.Category,
            Title = x.Title,
            Status = x.Status,
            DueDate = x.DueDate,
            CompletedDate = x.CompletedDate,
            Reference = x.Reference,
            Notes = x.Notes,
            SortOrder = x.SortOrder
        };

    public async Task<Guid> AddChecklistItemAsync(
        Guid estateId,
        CreateChecklistItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Enter a title for the task.");

        if (!await _repository.ExistsAsync(estateId))
            throw new InvalidOperationException($"Estate {estateId} not found.");

        var nextOrder = await _repository.NextChecklistSortOrderAsync(estateId);

        var item = new ChecklistItem
        {
            EstateId = estateId,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Other" : request.Category.Trim(),
            Title = request.Title.Trim(),
            Status = ChecklistStatuses.Open,
            DueDate = request.DueDate?.Date,
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            SortOrder = nextOrder
        };

        await _repository.AddChecklistItemsAsync(new[] { item });
        return item.ItemId;
    }

    public async Task UpdateChecklistStatusAsync(Guid estateId, Guid itemId, string status)
    {
        if (!ChecklistStatuses.All.Contains(status))
            throw new ArgumentException("Choose a valid status.");

        var completed = status == ChecklistStatuses.Done ? DateTime.Today : (DateTime?)null;

        if (!await _repository.SetChecklistStatusAsync(estateId, itemId, status, completed))
            throw new InvalidOperationException($"Checklist item {itemId} not found.");
    }

    public async Task DeleteChecklistItemAsync(Guid estateId, Guid itemId)
    {
        if (!await _repository.DeleteChecklistItemAsync(estateId, itemId))
            throw new InvalidOperationException($"Checklist item {itemId} not found.");
    }

    // Adds the standard South African administration checklist. Safe to press twice: titles that
    // already exist on the estate are skipped. Dates are only set where the Administration of
    // Estates Act fixes them (s35(1): account within six months of the Letters; s35(5): 21 days'
    // inspection; s29: 30 days for creditors after the notice).
    public async Task<int> SeedChecklistAsync(Guid estateId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException($"Estate {estateId} not found.");

        var existing = new HashSet<string>(
            estate.ChecklistItems.Select(x => x.Title),
            StringComparer.OrdinalIgnoreCase);

        var letters = estate.LettersOfExecutorshipDate;
        var order = estate.ChecklistItems.Count == 0
            ? 10
            : estate.ChecklistItems.Max(x => x.SortOrder) + 10;

        var items = new List<ChecklistItem>();

        void Add(string category, string title, DateTime? due = null, string? notes = null)
        {
            if (existing.Contains(title)) return;
            items.Add(new ChecklistItem
            {
                EstateId = estateId,
                Category = category,
                Title = title,
                Status = ChecklistStatuses.Open,
                DueDate = due,
                Notes = notes,
                SortOrder = order
            });
            order += 10;
        }

        foreach (var (category, title, offsetFromLetters, notes) in ChecklistTemplate)
        {
            DateTime? due = offsetFromLetters.HasValue && letters.HasValue
                ? letters.Value.Date.AddDays(offsetFromLetters.Value)
                : null;
            Add(category, title, due, notes);
        }

        if (items.Count == 0)
            return 0;

        await _repository.AddChecklistItemsAsync(items);
        return items.Count;
    }

    // Category, title, optional due date as days after the Letters of Executorship, notes.
    private static readonly (string Category, string Title, int? DaysAfterLetters, string? Notes)[] ChecklistTemplate =
    {
        ("Master's Office", "Report the estate to the Master and lodge the death certificate, ID and original will", null,
            "Check the Master's office for the current reporting forms and requirements."),
        ("Master's Office", "Obtain Letters of Executorship (or Letters of Authority)", null, null),
        ("Master's Office", "Lodge the inventory of assets with the Master", null, "Normally within 30 days of the Letters (s9)."),
        ("Master's Office", "Provide security to the Master if required", null, null),

        ("SARS & tax", "Notify SARS of the death and obtain income tax reference for the estate", null, null),
        ("SARS & tax", "Final income tax return of the deceased up to the date of death", null, null),
        ("SARS & tax", "Capital gains tax on death (deemed disposal) calculated and included", null, null),
        ("SARS & tax", "Estate income tax returns for post-death income", null, null),
        ("SARS & tax", "Estate duty return (if the estate is dutiable)", null, "Required where the dutiable amount is above zero."),
        ("SARS & tax", "Pay estate duty and obtain SARS confirmation", null, null),
        ("SARS & tax", "Obtain tax clearance / confirmation of tax status before final distribution", null, null),

        ("Creditors", "Publish the s29 notice in the Government Gazette and a local newspaper", null, "Creditors have 30 days from the notice to lodge claims."),
        ("Creditors", "Claims period closed – assess every claim (admit or reject)", null, null),
        ("Creditors", "Notify heirs of any disputed or rejected claims", null, null),
        ("Creditors", "Pay admitted creditors in the correct order", null, null),

        ("Assets", "Open the estate bank account and record the details on the General tab", null, null),
        ("Assets", "Obtain valuations of immovable property (market value at date of death)", null, null),
        ("Assets", "Obtain valuations of vehicles, shares and other movables", null, null),
        ("Assets", "Collect cash, policies and investments into the estate bank account", null, null),
        ("Assets", "Cancel or transfer the deceased's debit orders and accounts", null, null),

        ("L&D account", "Draft the Liquidation & Distribution account", null, null),
        ("L&D account", "Lodge the L&D account with the Master", 183, "Due within six months of the Letters (s35(1)); request an extension from the Master if needed."),
        ("L&D account", "Answer the Master's query sheet", null, null),
        ("L&D account", "Advertise the account for inspection (21 days)", null, "The account lies for inspection for 21 days (s35(5))."),
        ("L&D account", "Confirm with the Master that no objections were lodged", null, null),

        ("Objections", "Record any objection lodged against the account and the response", null, "Add one task per objection."),

        ("Transfers & distribution", "Transfer immovable property to heirs / purchasers (conveyancer instructed)", null, null),
        ("Transfers & distribution", "Transfer or sell vehicles and register the new owner", null, null),
        ("Transfers & distribution", "Transfer shares and investments to heirs", null, null),
        ("Transfers & distribution", "Pay cash legacies and the residue to heirs", null, null),
        ("Transfers & distribution", "Hold back funds for minor heirs / pay into the Guardian's Fund", null, null),

        ("Closure", "Prepare the final cash statement for each heir", null, null),
        ("Closure", "Close the estate bank account", null, null),
        ("Closure", "File the final papers with the Master and close the file", null, null),
    };

    // =====================================================
    // EDIT EXISTING RECORDS
    // =====================================================

    private async Task<Estate> LoadEstateForEditAsync(Guid estateId) =>
        await _repository.GetByIdAsync(estateId)
        ?? throw new InvalidOperationException(
            $"Estate {estateId} not found.");

    public async Task UpdateBeneficiaryAsync(
        Guid estateId,
        Guid beneficiaryId,
        CreateBeneficiaryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Beneficiary full name is required.");

        if (!IsValidEmail(request.Email))
            throw new ArgumentException("A valid beneficiary email address is required.");

        var idError = SaIdNumber.Validate(request.IdNumber, request.DateOfBirth, "Heir");
        if (idError is not null)
            throw new ArgumentException(idError);

        var estate = await LoadEstateForEditAsync(estateId);

        var b =
            estate.Beneficiaries.FirstOrDefault(x => x.BeneficiaryId == beneficiaryId)
            ?? throw new InvalidOperationException($"Beneficiary {beneficiaryId} not found.");

        b.FullName = request.FullName.Trim();
        b.IdNumber = request.IdNumber?.Trim() ?? string.Empty;
        b.Relationship = request.Relationship?.Trim() ?? string.Empty;
        b.HeirType = request.HeirType?.Trim() ?? string.Empty;
        b.Email = request.Email.Trim();
        b.FirstName = request.FirstName?.Trim();
        b.MiddleName = request.MiddleName?.Trim();
        b.Surname = request.Surname?.Trim();
        b.RelationshipDescription = request.RelationshipDescription?.Trim();
        b.DateOfBirth = request.DateOfBirth;
        b.Gender = request.Gender;
        b.MaritalStatus = request.MaritalStatus;
        b.WillClause = request.WillClause?.Trim();
        b.CashLegacyAmount = request.CashLegacyAmount;
        b.ResidueNumerator = request.ResidueNumerator;
        b.ResidueDenominator = request.ResidueDenominator;
        b.AdvancesPaid = request.AdvancesPaid;
        b.IsSurvivingSpouse = request.IsSurvivingSpouse;
        b.IsMinor = request.IsMinor;
        b.GuardianOrTrustee = request.GuardianOrTrustee?.Trim();
        b.IsPredeceased = request.IsPredeceased;
        b.IsUltimateBeneficiaryPreviousEstate = request.IsUltimateBeneficiaryPreviousEstate;
        b.BareDominiumFraction = request.BareDominiumFraction?.Trim();
        b.DisplayOrder = request.DisplayOrder;

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateMovableAssetAsync(
        Guid estateId,
        Guid assetId,
        CreateMovableAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Asset description is required.");

        var estate = await LoadEstateForEditAsync(estateId);

        var asset =
            estate.MovableAssets.FirstOrDefault(x => x.AssetId == assetId)
            ?? throw new InvalidOperationException($"Movable asset {assetId} not found.");

        asset.Description = request.Description;
        asset.AssetType = request.AssetType;
        asset.Value = request.Value;
        asset.IsVehicle = request.IsVehicle;
        asset.RegistrationNumber = request.RegistrationNumber;
        asset.VinNumber = request.VinNumber;
        asset.Make = request.Make;
        asset.AssetCategory = request.AssetCategory;
        asset.ValuerName = request.ValuerName?.Trim();
        asset.ValuationDate = request.ValuationDate;
        asset.AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds;
        asset.IsSpecialBequest = request.IsSpecialBequest;
        asset.IsSoldOrReducedToCash = request.IsSoldOrReducedToCash;
        asset.SaleProceeds = request.SaleProceeds;
        asset.SaleDate = request.SaleDate;
        asset.BuyerName = request.BuyerName?.Trim();
        asset.ShareCompany = request.ShareCompany?.Trim();
        asset.NumberOfShares = request.NumberOfShares;
        asset.ShareClass = request.ShareClass?.Trim();
        asset.PriceCentsPerShare = request.PriceCentsPerShare;
        asset.CumulativeDividend = request.CumulativeDividend;

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateImmovablePropertyAsync(
        Guid estateId,
        Guid assetId,
        CreateImmovablePropertyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Property description is required.");

        var estate = await LoadEstateForEditAsync(estateId);

        var p =
            estate.ImmovableProperties.FirstOrDefault(x => x.AssetId == assetId)
            ?? throw new InvalidOperationException($"Immovable property {assetId} not found.");

        p.Description = request.Description;
        p.TitleDeedNumber = request.TitleDeedNumber;
        p.MunicipalValue = request.MunicipalValue;
        p.PropertyType = request.PropertyType;
        p.ErfNumber = request.ErfNumber?.Trim();
        p.Township = request.Township?.Trim();
        p.Division = request.Division?.Trim();
        p.Province = request.Province;
        p.ExtentSquareMetres = request.ExtentSquareMetres;
        p.StreetAddress = request.StreetAddress?.Trim();
        p.SchemeName = request.SchemeName?.Trim();
        p.SectionNumber = request.SectionNumber?.Trim();
        p.ExclusiveUseAreas = request.ExclusiveUseAreas?.Trim();
        p.ShareNumerator = request.ShareNumerator;
        p.ShareDenominator = request.ShareDenominator;
        p.MarketValue = request.MarketValue;
        p.ValuerName = request.ValuerName?.Trim();
        p.ValuationDate = request.ValuationDate;
        p.AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds;
        p.IsSpecialBequest = request.IsSpecialBequest;
        p.IsSoldByEstate = request.IsSoldByEstate;
        p.SalePrice = request.SalePrice;
        p.SaleDate = request.SaleDate;
        p.PurchaserName = request.PurchaserName?.Trim();

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateFinancialClaimAsync(
        Guid estateId,
        Guid claimId,
        CreateFinancialClaimRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InstitutionName))
            throw new ArgumentException("Institution name is required.");

        var estate = await LoadEstateForEditAsync(estateId);

        var c =
            estate.FinancialClaims.FirstOrDefault(x => x.ClaimId == claimId)
            ?? throw new InvalidOperationException($"Financial claim {claimId} not found.");

        c.InstitutionName = request.InstitutionName;
        c.InstitutionAddress = request.InstitutionAddress;
        c.AccountReference = request.AccountReference;
        c.Value = request.Value;
        c.ClaimType = request.ClaimType;
        c.IsInFavourOfEstate = request.IsInFavourOfEstate;
        c.AccruedInterest = request.AccruedInterest;
        c.ValuationDate = request.ValuationDate;
        c.AwardedToBeneficiaryIds = request.AwardedToBeneficiaryIds;
        c.IsSpecialBequest = request.IsSpecialBequest;
        c.IsCollectedAsCash = request.IsCollectedAsCash;
        c.CollectedAmount = request.CollectedAmount;
        c.CollectedDate = request.CollectedDate;
        c.IsCreditLifeInsurance = request.IsCreditLifeInsurance;
        c.LinkedLiabilityNote = request.LinkedLiabilityNote;

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateLiabilityAsync(
        Guid estateId,
        Guid liabilityId,
        CreateLiabilityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CreditorName))
            throw new ArgumentException("Creditor name is required.");

        if (request.Amount < 0)
            throw new ArgumentException("Liability amount cannot be negative.");

        var estate = await LoadEstateForEditAsync(estateId);

        var l =
            estate.Liabilities.FirstOrDefault(x => x.LiabilityId == liabilityId)
            ?? throw new InvalidOperationException($"Liability {liabilityId} not found.");

        l.CreditorName = request.CreditorName.Trim();
        l.Description = request.Description?.Trim();
        l.Amount = request.Amount;
        l.Category = request.Category;
        l.AccountReference = request.AccountReference?.Trim();
        l.InterestAfterDeath = request.InterestAfterDeath;
        l.Status = string.IsNullOrWhiteSpace(request.Status) ? "Admitted" : request.Status;
        l.DateLodged = request.DateLodged;
        l.SecuredAgainst = request.SecuredAgainst?.Trim();
        l.IsTakenOverByHeir = request.IsTakenOverByHeir;
        l.IsPaid = request.IsPaid;
        l.AmountPaid = request.IsPaid ? request.AmountPaid : 0;
        l.DatePaid = request.IsPaid ? request.DatePaid : null;
        l.IsCoveredByCreditLife = request.IsCoveredByCreditLife;

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateCashbookEntryAsync(
        Guid estateId,
        Guid entryId,
        CreateCashbookEntryRequest request)
    {
        if (request.Amount <= 0m)
            throw new ArgumentException("The amount must be greater than zero.");

        if (!CashbookTypes.All.Contains(request.EntryType))
            throw new ArgumentException("Choose a valid cashbook entry type.");

        if (string.IsNullOrWhiteSpace(request.Description) &&
            string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Enter a description or a category.");

        var estate = await LoadEstateForEditAsync(estateId);

        var entry =
            estate.CashbookEntries.FirstOrDefault(x => x.EntryId == entryId)
            ?? throw new InvalidOperationException($"Cashbook entry {entryId} not found.");

        entry.EntryDate = request.EntryDate.Date;
        entry.EntryType = request.EntryType;
        entry.Category = (request.Category ?? string.Empty).Trim();
        entry.Description = (request.Description ?? string.Empty).Trim();
        entry.PayerOrPayee = string.IsNullOrWhiteSpace(request.PayerOrPayee) ? null : request.PayerOrPayee.Trim();
        entry.Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim();
        entry.Amount = Math.Round(request.Amount, 2);

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateChecklistItemAsync(
        Guid estateId,
        Guid itemId,
        CreateChecklistItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Enter a title for the task.");

        var estate = await LoadEstateForEditAsync(estateId);

        var item =
            estate.ChecklistItems.FirstOrDefault(x => x.ItemId == itemId)
            ?? throw new InvalidOperationException($"Checklist item {itemId} not found.");

        item.Title = request.Title.Trim();

        if (!string.IsNullOrWhiteSpace(request.Category))
            item.Category = request.Category.Trim();

        item.DueDate = request.DueDate?.Date;
        item.Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim();
        item.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _repository.SaveAsync(estate);
    }

    public async Task UpdateDiaryEntryAsync(
        Guid estateId,
        Guid diaryId,
        CreateDiaryEntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
            throw new ArgumentException("Diary note is required.");

        if (request.DueDate == default)
            throw new ArgumentException("Diary due date is required.");

        var notifyBy =
            new[] { "Email", "InApp", "Both" }
                .FirstOrDefault(x => string.Equals(x, request.NotifyBy, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Notification method must be Email, InApp or Both.");

        var estate = await LoadEstateForEditAsync(estateId);

        var entry =
            estate.DiaryEntries.FirstOrDefault(x => x.DiaryId == diaryId)
            ?? throw new InvalidOperationException($"Diary entry {diaryId} not found.");

        entry.Note = request.Note.Trim();
        entry.DueDate = request.DueDate;
        entry.NotifyBy = notifyBy;

        await _repository.SaveAsync(estate);
    }

    // =====================================================
    // AUDIT TRAIL
    // =====================================================

    public async Task<List<AuditEntryDto>> GetAuditTrailAsync(Guid estateId, int take)
    {
        var rows = await _repository.GetAuditLogsAsync(estateId, Math.Clamp(take, 1, 1000));

        return rows.Select(r => new AuditEntryDto
        {
            TimestampUtc = DateTime.SpecifyKind(r.TimestampUtc, DateTimeKind.Utc),
            UserName = r.UserName,
            EntityName = r.EntityName,
            Action = r.Action,
            Changes = r.Changes
        }).ToList();
    }

}
