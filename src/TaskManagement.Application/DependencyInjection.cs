using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.UseCases.Auth;
using TaskManagement.Application.UseCases.Tasks;

namespace TaskManagement.Application;

/// <summary>
/// Registers application-layer services: use cases and FluentValidation validators.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegisterUseCase, RegisterUseCase>();
        services.AddScoped<ISignInUseCase, SignInUseCase>();
        services.AddScoped<ITaskUseCase, TaskUseCase>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
