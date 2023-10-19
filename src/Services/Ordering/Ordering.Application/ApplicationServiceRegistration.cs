using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(o => o.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
