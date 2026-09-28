using System.Net.Http.Headers;
using System.Text;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Settings;
using EventFlow.Domain.Users.Enums;
using EventFlow.Infrastructure.Data;
using EventFlow.Infrastructure.Helpers;
using EventFlow.Infrastructure.PaymentGateways.Paymob;
using EventFlow.Infrastructure.PaymentGateways.Paymob.Service;
using EventFlow.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventFlow.Infrastructure; 

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,IWebHostEnvironment environment)
    {

        // ── Database ──────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sqlOptions =>
                {
                    // Retry on transient SQL failures (connection drops etc.)
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        services.AddScoped<IAppDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SigningKey"]))
            };
        });
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailSender, SmtpEmailSender>(); // or LogEmailSender for dev

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddSingleton<ITokenSettings>(sp =>
            sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<JwtSettings>>()
                .Value);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentTenant,CurrentTenant>();

                // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantOwner", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(UserRole.Owner.ToString()));

            options.AddPolicy("TenantStaff", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("tenant_id"));

            options.AddPolicy("CheckInStaff", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(UserRole.Employee.ToString())
                    .RequireClaim("tenant_id"));
        });
        
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<EmailService>();

        if (environment.IsDevelopment())
        {
            // Logs to console — no real emails sent while building
            services.AddScoped<IEmailSender, LogEmailSender>();
        }
        else
        {
            // Real SMTP — only active in staging/production
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10), // L2, L3
            LocalCacheExpiration = TimeSpan.FromSeconds(30), // L1
        });
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // In DependencyInjection.cs
        services.Configure<PaymobSettings>(configuration.GetSection("Paymob"));

        // Register named HttpClient for Paymob
        // The Authorization header is set once here — every request uses it
        services.AddHttpClient("Paymob", (IServiceProvider sp,HttpClient client) =>
        {
            var settings = sp.GetRequiredService<IOptions<PaymobSettings>>().Value;

            // Paymob uses "Token " prefix — not "Bearer "
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", settings.SecretKey.Replace("Token ", ""));

            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<IPaymentGateway, PaymobPaymentGateway>();
        services.Configure<QrCodeSettings>(configuration.GetSection("QrCode"));
        services.AddScoped<IQrCodeService, QrCodeService>();
        return services;
        
    }
}