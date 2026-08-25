using System.Net;
using System.Net.Mail;
using EmployeeSearch.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeSearch.Infrastructure.Notifications;

public class EmailNotificationService : INotificationService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IOptions<EmailSettings> settings, ILogger<EmailNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            // No SMTP server configured (e.g. local dev) -- log instead of failing the request.
            _logger.LogInformation(
                "[EMAIL:SIMULATED] To={Recipient} Subject={Subject} Body={Body}",
                message.Recipient, message.Subject, message.Body);
            return;
        }

        using var mail = new MailMessage(_settings.FromAddress, message.Recipient, message.Subject, message.Body);
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("Email sent to {Recipient}", message.Recipient);
    }
}
