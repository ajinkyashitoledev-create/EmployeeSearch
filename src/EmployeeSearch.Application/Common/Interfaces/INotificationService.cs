namespace EmployeeSearch.Application.Common.Interfaces;

public enum NotificationChannel
{
    Email,
    Sms
}

public record NotificationMessage(string Recipient, string Subject, string Body);

/// <summary>
/// Contract implemented by each notification channel (Email, SMS, ...).
/// Concrete implementations live in the Infrastructure layer and are
/// registered as keyed services, keyed by <see cref="NotificationChannel"/>.
/// </summary>
public interface INotificationService
{
    NotificationChannel Channel { get; }
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}

public record ChannelNotification(NotificationChannel Channel, NotificationMessage Message);

/// <summary>
/// Fans notifications out across one or more channels. The application layer
/// depends only on this dispatcher, never on a specific channel implementation.
/// </summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(IEnumerable<ChannelNotification> notifications, CancellationToken cancellationToken = default);
}
