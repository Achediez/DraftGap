// Implementación en memoria del repositorio de usuarios
// Guarda los usuarios en una lista local (solo para pruebas/desarrollo)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;

namespace DraftGapBackend.Infrastructure.Persistence
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new(); // Almacenamiento en memoria
        // Busca usuario por email
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        // Busca usuario por nombre de usuario
        public Task<User?> GetByUserNameAsync(string userName) => Task.FromResult(_users.FirstOrDefault(u => u.UserName == userName));
        // Agrega un usuario a la lista
        public Task AddAsync(User user) { _users.Add(user); return Task.CompletedTask; }
    }
}
