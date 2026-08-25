using System.Net.Http.Json;
using EmployeeSearch.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeSearch.Infrastructure.Notifications;

public class SmsNotificationService : INotificationService
{
    private readonly SmsSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(IOptions<SmsSettings> settings, HttpClient httpClient, ILogger<SmsNotificationService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            // No SMS gateway configured (e.g. local dev) -- log instead of failing the request.
            _logger.LogInformation(
                "[SMS:SIMULATED] To={Recipient} Body={Body}",
                message.Recipient, message.Body);
            return;
        }

        var payload = new { from = _settings.FromNumber, to = message.Recipient, body = message.Body };
        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiBaseUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("SMS sent to {Recipient}", message.Recipient);
    }
}
