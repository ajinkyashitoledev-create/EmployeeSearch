using System.Text;
using EmployeeSearch.API.Auth;
using EmployeeSearch.API.Middleware;
using EmployeeSearch.Application;
using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Domain.Entities;
using EmployeeSearch.Infrastructure;
using EmployeeSearch.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

const string AngularDevCorsPolicy = "AngularDevClient";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Search API",
        Version = "v1",
        Description = "Search Developers and Managers with Clean Architecture, EF Core, and SQL Server."
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by POST /api/auth/login (no \"Bearer \" prefix needed here).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, Array.Empty<string>() } });
});

// Global exception handling: every unhandled exception is funneled through
// GlobalExceptionHandler and rendered as RFC 7807 ProblemDetails.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- JWT authentication ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// --- CORS: allow the Angular dev server to call this API ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// In containers there's no interactive `dotnet ef database update` step, so
// migrations can optionally be applied automatically on startup. Left off by
// default for local dev, where migrations are typically run explicitly.
if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Seed two demo accounts on first run so the UI has something to log in with
// out of the box: admin/Admin@123 (Admin role), recruiter/Recruiter@123 (User role).
using (var seedScope = app.Services.CreateScope())
{
    var dbContext = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (!await dbContext.Users.AnyAsync())
    {
        dbContext.Users.AddRange(
            new ApplicationUser { Username = "admin", Email = "admin@employeesearch.local", PasswordHash = passwordHasher.Hash("Admin@123"), Role = "Admin" },
            new ApplicationUser { Username = "recruiter", Email = "recruiter@employeesearch.local", PasswordHash = passwordHasher.Hash("Recruiter@123"), Role = "User" });

        await dbContext.SaveChangesAsync();
    }
}

app.UseExceptionHandler(); // delegates to the GlobalExceptionHandler registered above

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Search API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors(AngularDevCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { } // exposed for integration testing
