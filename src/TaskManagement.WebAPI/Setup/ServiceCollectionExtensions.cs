using System.Linq;
using Asp.Versioning;
using TaskManagement.Application.Mapping;
using TaskManagement.WebAPI.Filters;
using TaskManagement.WebAPI.Models;
using TaskManagement.WebAPI.OpenApi;
using TaskManagement.WebAPI.Setup.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace TaskManagement.WebAPI.Setup;

/// <summary>
/// Registers API-layer services: options, controllers, versioning, and validation.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddApplicationApi(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.Filters.Add<FluentValidationActionFilter>();
            })
            .AddNewtonsoftJson(op =>
            {
                op.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                op.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });

        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi(options =>
            {
                options.Document.AddDocumentTransformer<BearerSecurityDocumentTransformer>();
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                    .ToArray();

                return new BadRequestObjectResult(
                    ApiResponse<object>.Fail("Validation failed", errors));
            };
        });

        services.AddAutoMapper(cfg => { }, typeof(DomainToDtoMappingProfile), typeof(RequestToDomainMappingProfile));

        return services;
    }
}
