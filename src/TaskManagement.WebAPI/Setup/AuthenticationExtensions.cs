using System;
using System.Text;
using TaskManagement.Application.Auth;
using TaskManagement.WebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TaskManagement.WebAPI.Setup;

/// <summary>
/// Registers JWT bearer authentication with consistent 401/403 JSON envelopes.
/// </summary>
public static class AuthenticationExtensions
{
    private static readonly JsonSerializerSettings ChallengeJsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            x.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    if (context.Response.HasStarted) return;
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            ApiResponse<object>.Fail("Authentication required."),
                            ChallengeJsonSettings));
                },
                OnForbidden = async context =>
                {
                    if (context.Response.HasStarted) return;
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            ApiResponse<object>.Fail("You are not allowed to perform this action."),
                            ChallengeJsonSettings));
                }
            };
        });

        return services;
    }
}
