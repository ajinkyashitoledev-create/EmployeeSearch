using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Infrastructure.Notifications;
using EmployeeSearch.Infrastructure.Persistence;
using EmployeeSearch.Infrastructure.Persistence.Repositories;
using EmployeeSearch.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeSearch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));

        services.AddHttpClient();

        // Keyed DI: each INotificationService implementation is resolved by its NotificationChannel key.
        services.AddKeyedScoped<INotificationService, EmailNotificationService>(NotificationChannel.Email);
        services.AddKeyedScoped<INotificationService>(NotificationChannel.Sms, (sp, _) =>
            new SmsNotificationService(
                sp.GetRequiredService<IOptions<SmsSettings>>(),
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(SmsNotificationService)),
                sp.GetRequiredService<ILogger<SmsNotificationService>>()));

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}
