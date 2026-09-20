using System.Text;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Enums;
using EventFlow.Infrastructure.Data;
using EventFlow.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EventFlow.Infrastructure; 

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        // services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]))
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

        return services;
    }
}