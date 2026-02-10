// Interfaz de repositorio de usuarios
// Define las operaciones de acceso a datos para la entidad User
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DraftGapBackend.Domain.Abstractions
{
    public interface IUserRepository
    {
        // Buscar usuario por id
        Task<User?> GetByIdAsync(Guid id);
        // Buscar usuario por email
        Task<User?> GetByEmailAsync(string email);
        // Buscar usuario por nombre de usuario
        Task<User?> GetByUserNameAsync(string userName);
        // Agregar un nuevo usuario
        Task AddAsync(User user);
        // Actualizar usuario
        Task UpdateAsync(User user);
        // Obtener todos los usuarios
        Task<IEnumerable<User>> GetAllAsync();
        // ...otros métodos necesarios...
    }
}
