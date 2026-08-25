namespace EmployeeSearch.Infrastructure.Notifications;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string FromAddress { get; set; } = "no-reply@employeesearch.local";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;
}

public class SmsSettings
{
    public const string SectionName = "Sms";

    public string ApiBaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string FromNumber { get; set; } = string.Empty;
}
