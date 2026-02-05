// Interfaz de repositorio de usuarios
// Define las operaciones de acceso a datos para la entidad User
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions
{
    public interface IUserRepository
    {
        // Buscar usuario por email
        Task<User?> GetByEmailAsync(string email);
        // Buscar usuario por nombre de usuario
        Task<User?> GetByUserNameAsync(string userName);
        // Agregar un nuevo usuario
        Task AddAsync(User user);
        // ...otros métodos necesarios...
    }
}
