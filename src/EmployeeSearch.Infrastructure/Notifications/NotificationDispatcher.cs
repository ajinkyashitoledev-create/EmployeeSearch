using EmployeeSearch.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeSearch.Infrastructure.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(IServiceProvider serviceProvider, ILogger<NotificationDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<ChannelNotification> notifications, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(notifications.Select(n => SendSafeAsync(n, cancellationToken)));
    }

    private async Task SendSafeAsync(ChannelNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            // Resolved by key so the caller depends only on NotificationChannel, never on a concrete class.
            var service = _serviceProvider.GetRequiredKeyedService<INotificationService>(notification.Channel);
            await service.SendAsync(notification.Message, cancellationToken);
        }
        catch (Exception ex)
        {
            // A notification failure must never fail the business operation that triggered it.
            _logger.LogError(ex, "Failed to dispatch {Channel} notification to {Recipient}",
                notification.Channel, notification.Message.Recipient);
        }
    }
}
