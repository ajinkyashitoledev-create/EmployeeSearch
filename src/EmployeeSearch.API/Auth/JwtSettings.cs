namespace EmployeeSearch.API.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "EmployeeSearchApi";
    public string Audience { get; set; } = "EmployeeSearchApiClients";
    public int ExpiryMinutes { get; set; } = 60;
}
