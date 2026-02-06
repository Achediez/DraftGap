// Configuración de inyección de dependencias para la infraestructura
// Registra los servicios y repositorios concretos en el contenedor DI
using Microsoft.Extensions.DependencyInjection;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Persistence;

namespace DraftGapBackend.Infrastructure
{
    public static class DependencyInjection
    {
        // Método de extensión para registrar servicios de infraestructura
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Cambiado a Singleton para que la lista de usuarios se comparta entre peticiones
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            // Servicio de usuario de aplicación
            services.AddScoped<IUserService, UserService>();
            // ...otros servicios...
            return services;
        }
    }
}
