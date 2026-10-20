using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EstateFlow.Functions.Services;

public class EmailNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(
        string recipientEmail,
        string subject,
        string body)
    {
        if (string.IsNullOrWhiteSpace(
                recipientEmail))
        {
            throw new ArgumentException(
                "Recipient email address is required.",
                nameof(recipientEmail));
        }

        var apiKey =
            _configuration["SendGridApiKey"]
            ?? _configuration["SendGrid:ApiKey"];

        var fromEmail =
            _configuration["SendGridFromEmail"]
            ?? _configuration["SendGrid:FromEmail"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "SendGrid is not configured. " +
                "Set 'SendGridApiKey' in the Azure Function App environment variables.");
        }

        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "SendGrid sender email is not configured. " +
                "Set 'SendGridFromEmail' in the Azure Function App environment variables.");
        }

        var payload =
            new
            {
                personalizations =
                    new[]
                    {
                        new
                        {
                            to =
                                new[]
                                {
                                    new
                                    {
                                        email =
                                            recipientEmail
                                    }
                                }
                        }
                    },

                from =
                    new
                    {
                        email =
                            fromEmail
                    },

                subject,

                content =
                    new[]
                    {
                        new
                        {
                            type =
                                "text/plain",

                            value =
                                body
                        }
                    }
            };

        var json =
            JsonSerializer.Serialize(
                payload);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.sendgrid.com/v3/mail/send")
            {
                Content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
            };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        using var response =
            await _httpClient.SendAsync(
                request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "SendGrid accepted email for {recipient}.",
                recipientEmail);

            return;
        }

        var responseBody =
            await response.Content.ReadAsStringAsync();

        _logger.LogError(
            "SendGrid rejected email for {recipient}. Status {status}. Response: {response}",
            recipientEmail,
            (int)response.StatusCode,
            responseBody);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "SendGrid rejected the API key (401 Unauthorized). " +
                "Check 'SendGridApiKey' in the Function App settings.");
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "SendGrid rejected the email (403 Forbidden). " +
                "Make sure 'SendGridFromEmail' is a verified sender in SendGrid.");
        }

        throw new HttpRequestException(
            $"SendGrid returned HTTP {(int)response.StatusCode} " +
            $"({response.StatusCode}). {responseBody}");
    }
}
