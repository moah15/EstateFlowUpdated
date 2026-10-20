using EstateFlow.Data.Context;
using EstateFlow.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateFlow.Functions;

public class MonthlyReportFunction
{
    private readonly EstateFlowDbContext _context;
    private readonly EmailNotificationService _notificationService;
    private readonly ILogger<MonthlyReportFunction> _logger;

    public MonthlyReportFunction(
        EstateFlowDbContext context,
        EmailNotificationService notificationService,
        ILogger<MonthlyReportFunction> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    // Runs at 06:00 UTC on the 1st of every month.
    [Function("MonthlyReportFunction")]
    public async Task Run(
        [TimerTrigger("0 0 6 1 * *")] TimerInfo timer)
    {
        _logger.LogInformation(
            "MonthlyReportFunction started at: {time}",
            DateTime.UtcNow);

        var estates =
            await _context.Estates
                .Include(e => e.Deceased)
                .Include(e => e.Executors)
                .Include(e => e.Beneficiaries)
                .Include(e => e.ProgressStages)
                .Include(e => e.AutomatedReports)
                .ToListAsync();

        var sentCount = 0;
        var skippedCount = 0;

        foreach (var estate in estates)
        {
            var latestReport =
                estate.AutomatedReports
                    .OrderByDescending(
                        r => r.ReportPeriod)
                    .FirstOrDefault();

            if (latestReport?.IsPaused == true)
            {
                _logger.LogInformation(
                    "Skipping estate {file} — reports paused.",
                    estate.FileNumber);

                skippedCount++;
                continue;
            }

            var summary =
                BuildReportSummary(estate);

            var recipients =
                estate.Executors
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x.Email))
                    .Select(x => new
                    {
                        x.FullName,
                        Email = x.Email!,
                        RecipientType = "Executor"
                    })
                    .Concat(
                        estate.Beneficiaries
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(
                                    x.Email))
                            .Select(x => new
                            {
                                x.FullName,
                                Email = x.Email!,
                                RecipientType = "Beneficiary"
                            }))
                    .GroupBy(
                        x => x.Email,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning(
                    "Estate {file} has no executor or beneficiary email addresses. No monthly report was sent.",
                    estate.FileNumber);

                skippedCount++;
                continue;
            }

            foreach (var recipient in recipients)
            {
                try
                {
                    await _notificationService.NotifyAsync(
                        recipientEmail:
                            recipient.Email,

                        subject:
                            $"Monthly Estate Update — File {estate.FileNumber}",

                        body:
                            BuildPersonalisedMessage(
                                recipient.FullName,
                                summary));

                    sentCount++;

                    _logger.LogInformation(
                        "Monthly report for estate {file} sent to {recipientType} {recipient}.",
                        estate.FileNumber,
                        recipient.RecipientType,
                        recipient.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not send monthly report for estate {file} to {recipient}.",
                        estate.FileNumber,
                        recipient.Email);
                }
            }

            _context.AutomatedReports.Add(
                new Core.Models.AutomatedReport
                {
                    EstateId =
                        estate.EstateId,

                    ReportPeriod =
                        DateTime.UtcNow,

                    RecipientType =
                        "Executor/Beneficiary",

                    SentDate =
                        DateTime.UtcNow,

                    IsPaused =
                        false
                });
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "MonthlyReportFunction completed. Estates: {estateCount}; emails sent: {sentCount}; estates skipped: {skippedCount}.",
            estates.Count,
            sentCount,
            skippedCount);
    }

    private static string BuildPersonalisedMessage(
        string recipientName,
        string summary)
    {
        return $"""
            Dear {recipientName},

            {summary}

            Kind regards,
            
            EstateFlow Automated Reporting
            """;
    }

    private static string BuildReportSummary(
        Core.Models.Estate estate)
    {
        var completedStages =
            estate.ProgressStages.Count(
                s => s.Status == "Complete");

        var totalStages =
            estate.ProgressStages.Count;

        var nextStage =
            estate.ProgressStages
                .Where(s =>
                    s.Status != "Complete")
                .OrderBy(s =>
                    s.TargetDate)
                .FirstOrDefault();

        var nextStageText =
            nextStage is null
                ? "No outstanding progress stages."
                : $"{nextStage.StageName}" +
                  (nextStage.TargetDate.HasValue
                      ? $" — target {nextStage.TargetDate.Value:dd MMMM yyyy}"
                      : string.Empty);

        return $"""
            Estate File: {estate.FileNumber}
            Deceased: {estate.Deceased?.FullName}
            Status: {estate.EstateStatus}
            Progress: {completedStages}/{totalStages} stages complete
            Next outstanding stage: {nextStageText}

            This is your automated monthly update .
            """;
    }
}
