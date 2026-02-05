// Entidad de dominio User
// Representa un usuario en el sistema, con sus propiedades principales
using System;
using System.Threading.Tasks;

// Proyecto de dominio: entidades y lógica de negocio
namespace DraftGapBackend.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; } // Identificador único
        public string Email { get; set; } // Correo electrónico
        public string UserName { get; set; } // Nombre de usuario
        public string PasswordHash { get; set; } // Hash de la contraseña
        // ...otros campos relevantes...
    }
}
