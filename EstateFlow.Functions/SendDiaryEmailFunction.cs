using System.Net;
using System.Net.Mail;
using EstateFlow.Core.Dtos;
using EstateFlow.Data.Context;
using EstateFlow.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateFlow.Functions;

public class SendDiaryEmailFunction
{
    private readonly EstateFlowDbContext _context;
    private readonly EmailNotificationService _notificationService;
    private readonly ILogger<SendDiaryEmailFunction> _logger;

    public SendDiaryEmailFunction(
        EstateFlowDbContext context,
        EmailNotificationService notificationService,
        ILogger<SendDiaryEmailFunction> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    [Function("SendDiaryEmailFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "diary/send-email")]
        HttpRequestData request)
    {
        DiaryEmailFunctionRequest? input;

        try
        {
            input =
                await request
                    .ReadFromJsonAsync<
                        DiaryEmailFunctionRequest>();
        }
        catch
        {
            return await CreateResponseAsync(
                request,
                HttpStatusCode.BadRequest,
                "The request body is invalid.");
        }

        if (input is null)
        {
            return await CreateResponseAsync(
                request,
                HttpStatusCode.BadRequest,
                "The request body is required.");
        }

        if (input.EstateId == Guid.Empty ||
            input.DiaryId == Guid.Empty)
        {
            return await CreateResponseAsync(
                request,
                HttpStatusCode.BadRequest,
                "EstateId and DiaryId are required.");
        }

        if (string.IsNullOrWhiteSpace(
                input.RecipientEmail) ||
            !IsValidEmail(
                input.RecipientEmail))
        {
            return await CreateResponseAsync(
                request,
                HttpStatusCode.BadRequest,
                "A valid recipient email address is required.");
        }

        var diaryEntry =
            await _context.DiaryEntries
                .AsNoTracking()
                .Include(d => d.Estate)
                    .ThenInclude(e => e!.Deceased)
                .Include(d => d.Estate)
                    .ThenInclude(e => e!.Controller)
                .FirstOrDefaultAsync(
                    d =>
                        d.DiaryId ==
                            input.DiaryId &&
                        d.EstateId ==
                            input.EstateId);

        if (diaryEntry is null ||
            diaryEntry.Estate is null)
        {
            return await CreateResponseAsync(
                request,
                HttpStatusCode.NotFound,
                "The selected diary entry could not be found.");
        }

        var estate =
            diaryEntry.Estate;

        var deceasedName =
            estate.Deceased?.FullName
            ?? "the deceased";

        var recipientName =
            string.IsNullOrWhiteSpace(
                input.RecipientName)
                ? "Recipient"
                : input.RecipientName.Trim();

        var subject =
            string.IsNullOrWhiteSpace(
                input.Subject)
                ? $"Estate Diary Update — File {estate.FileNumber}"
                : input.Subject.Trim();

        var controllerName =
            estate.Controller?.StaffName;

        var body =
            BuildEmailBody(
                recipientName,
                deceasedName,
                estate.FileNumber,
                diaryEntry.Note,
                diaryEntry.DueDate,
                diaryEntry.IsCompleted,
                controllerName);

        try
        {
            await _notificationService.NotifyAsync(
                input.RecipientEmail.Trim(),
                subject,
                body);

            _logger.LogInformation(
                "Manual diary email sent. Estate {estateId}, Diary {diaryId}, Recipient {recipient}.",
                estate.EstateId,
                diaryEntry.DiaryId,
                input.RecipientEmail.Trim());

            return await CreateResponseAsync(
                request,
                HttpStatusCode.OK,
                "Diary email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Manual diary email failed. Estate {estateId}, Diary {diaryId}, Recipient {recipient}.",
                estate.EstateId,
                diaryEntry.DiaryId,
                input.RecipientEmail.Trim());

            return await CreateResponseAsync(
                request,
                HttpStatusCode.BadGateway,
                $"Email delivery failed: {ex.Message}");
        }
    }

    private static string BuildEmailBody(
        string recipientName,
        string deceasedName,
        string fileNumber,
        string note,
        DateTime dueDate,
        bool isCompleted,
        string? controllerName)
    {
        var status =
            isCompleted
                ? "Completed"
                : dueDate.Date <
                    DateTime.UtcNow.Date
                    ? "Overdue"
                    : "Open";

        var signOff =
            string.IsNullOrWhiteSpace(
                controllerName)
                ? "EstateFlow Estate Administration"
                : $"{controllerName}\nEstate Controller";

        return $"""
            ESTATEFLOW
            Estate Diary Correspondence

            Dear {recipientName},

            RE: ESTATE LATE {deceasedName.ToUpperInvariant()}
            FILE NUMBER: {fileNumber}

            Please note the following diary update recorded in respect of the above estate:

            ------------------------------------------------------------
            {note}
            ------------------------------------------------------------

            Due Date: {dueDate:dd MMMM yyyy}
            Status: {status}

            Regards,
            {signOff}
            EstateFlow
            """;
    }

    private static bool IsValidEmail(
        string email)
    {
        try
        {
            _ =
                new MailAddress(
                    email.Trim());

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<HttpResponseData>
        CreateResponseAsync(
            HttpRequestData request,
            HttpStatusCode statusCode,
            string message)
    {
        var response =
            request.CreateResponse(
                statusCode);

        await response.WriteAsJsonAsync(
            new
            {
                message
            });

        return response;
    }
}
