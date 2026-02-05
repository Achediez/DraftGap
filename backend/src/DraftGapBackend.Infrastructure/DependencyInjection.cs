// Configuración de DI y servicios de infraestructura
using Microsoft.Extensions.DependencyInjection;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Persistence;

namespace DraftGapBackend.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, InMemoryUserRepository>();
            services.AddScoped<IUserService, UserService>();
            // ...otros servicios...
            return services;
        }
    }
}
