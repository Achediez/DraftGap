// Interfaces de repositorios y servicios de dominio
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);
        Task AddAsync(User user);
        // ...otros métodos necesarios...
    }
}
