namespace EstateFlow.Api.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string recipientEmail, string subject, string body);
}