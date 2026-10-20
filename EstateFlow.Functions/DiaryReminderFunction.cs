using EstateFlow.Data.Context;
using EstateFlow.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateFlow.Functions;

public class DiaryReminderFunction
{
    private readonly EstateFlowDbContext _context;
    private readonly EmailNotificationService _notificationService;
    private readonly ILogger<DiaryReminderFunction> _logger;

    public DiaryReminderFunction(
        EstateFlowDbContext context,
        EmailNotificationService notificationService,
        ILogger<DiaryReminderFunction> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    // Runs daily at 06:00 — resolves pain point B: "Diary reminders are only
    // visible when the LegalEase application is open." This runs independently
    // of the Desktop Client entirely.
    [Function("DiaryReminderFunction")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("DiaryReminderFunction started at: {time}", DateTime.UtcNow);

        var today = DateTime.UtcNow.Date;
        var dueSoonCutoff = today.AddDays(7);

        var dueEntries = await _context.DiaryEntries
            .Include(d => d.Estate)
                .ThenInclude(e => e!.Controller)
            .Where(d => !d.IsCompleted && d.DueDate.Date <= dueSoonCutoff)
            .Where(d => d.NotifyBy == "Email" || d.NotifyBy == "Both")
            .ToListAsync();

        foreach (var entry in dueEntries)
        {
            var controller = entry.Estate?.Controller;
            if (controller is null) continue;

            var urgency = entry.DueDate.Date < today ? "OVERDUE" : "due soon";

            await _notificationService.NotifyAsync(
                recipientEmail: controller.Email,
                subject: $"[{urgency}] Diary reminder — File {entry.Estate?.FileNumber}",
                body: $"""
                    {entry.Note}

                    Due: {entry.DueDate:dd MMMM yyyy}
                    Estate File: {entry.Estate?.FileNumber}

                    This reminder was sent automatically, independent of whether
                    the EstateFlow Desktop Client is open.
                    """);
        }

        _logger.LogInformation("DiaryReminderFunction completed. Notified {count} diary entries.", dueEntries.Count);
    }
}