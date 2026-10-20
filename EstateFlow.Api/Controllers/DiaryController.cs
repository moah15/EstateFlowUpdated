using EstateFlow.Api.Services;
using EstateFlow.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace EstateFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DiaryController : ControllerBase
{
    private readonly IEstateService _estateService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public DiaryController(
        IEstateService estateService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _estateService = estateService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // ======================================================
    // GET ALL DIARY ENTRIES
    // ======================================================

    [HttpGet]
    public async Task<ActionResult<List<DiaryEntryWithEstateDto>>>
        GetAll()
    {
        var controllerIdClaim =
            User.FindFirst("controllerId")?.Value;

        if (controllerIdClaim is null ||
            !Guid.TryParse(controllerIdClaim, out var controllerId))
        {
            return Forbid();
        }

        var entries =
            await _estateService
                .GetAllDiaryEntriesAsync(controllerId);

        return Ok(entries);
    }

    // ======================================================
    // ADD DIARY ENTRY
    // ======================================================

    [HttpPost("estate/{estateId:guid}")]
    public async Task<ActionResult> Add(
        Guid estateId,
        [FromBody] CreateDiaryEntryRequest request)
    {
        try
        {
            var diaryId =
                await _estateService
                    .AddDiaryEntryAsync(
                        estateId,
                        request);

            return Ok(new
            {
                diaryId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ======================================================
    // COMPLETE / UNCOMPLETE
    // ======================================================

    [HttpPatch(
        "estate/{estateId:guid}/{diaryId:guid}/status")]
    public async Task<ActionResult> UpdateStatus(
        Guid estateId,
        Guid diaryId,
        [FromBody] UpdateDiaryEntryStatusRequest request)
    {
        try
        {
            await _estateService
                .UpdateDiaryEntryStatusAsync(
                    estateId,
                    diaryId,
                    request.IsCompleted);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ======================================================
    // SEND DIARY ENTRY AS EMAIL
    //
    // The desktop calls the secured EstateFlow API.
    // The API then calls the Azure HTTP-trigger Function.
    // The Function URL/key stays in Azure configuration and
    // is never embedded in the WPF desktop application.
    // ======================================================

    [HttpPost(
        "estate/{estateId:guid}/{diaryId:guid}/send-email")]
    public async Task<ActionResult> SendEmail(
        Guid estateId,
        Guid diaryId,
        [FromBody] SendDiaryEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.RecipientEmail))
        {
            return BadRequest(new
            {
                message =
                    "Recipient email address is required."
            });
        }

        var functionUrl =
            _configuration["DiaryEmailFunctionUrl"];

        if (string.IsNullOrWhiteSpace(
                functionUrl))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Diary email Function is not configured. " +
                        "Set 'DiaryEmailFunctionUrl' in the EstateFlow API App Service environment variables."
                });
        }

        var functionRequest =
            new DiaryEmailFunctionRequest
            {
                EstateId = estateId,
                DiaryId = diaryId,
                RecipientName =
                    request.RecipientName?.Trim()
                    ?? string.Empty,
                RecipientEmail =
                    request.RecipientEmail.Trim(),
                Subject =
                    request.Subject?.Trim()
                    ?? string.Empty
            };

        try
        {
            var client =
                _httpClientFactory.CreateClient();

            using var response =
                await client.PostAsJsonAsync(
                    functionUrl,
                    functionRequest);

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    message =
                        "Diary email sent successfully."
                });
            }

            var functionMessage =
                TryReadMessage(responseBody);

            return StatusCode(
                (int)response.StatusCode,
                new
                {
                    message =
                        string.IsNullOrWhiteSpace(
                            functionMessage)
                            ? "The diary email Function rejected the request."
                            : functionMessage
                });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message =
                        "EstateFlow could not reach the diary email Function. " +
                        ex.Message
                });
        }
    }

    // ======================================================
    // DELETE
    // ======================================================

    [HttpDelete(
        "estate/{estateId:guid}/{diaryId:guid}")]
    public async Task<ActionResult> Delete(
        Guid estateId,
        Guid diaryId)
    {
        try
        {
            await _estateService
                .DeleteDiaryEntryAsync(
                    estateId,
                    diaryId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    private static string TryReadMessage(
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(
                responseBody))
        {
            return string.Empty;
        }

        try
        {
            using var json =
                System.Text.Json.JsonDocument.Parse(
                    responseBody);

            if (json.RootElement.TryGetProperty(
                    "message",
                    out var messageElement))
            {
                return messageElement.GetString()
                       ?? string.Empty;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // The Function may have returned plain text.
        }

        return responseBody;
    }
}
