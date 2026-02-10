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
        // Busca usuario por id
        public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        // Busca usuario por email
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        // Busca usuario por nombre de usuario
        public Task<User?> GetByUserNameAsync(string userName) => Task.FromResult(_users.FirstOrDefault(u => u.UserName == userName));
        // Agrega un usuario a la lista
        public Task AddAsync(User user) { _users.Add(user); return Task.CompletedTask; }
        // Actualiza un usuario (reemplaza por id)
        public Task UpdateAsync(User user)
        {
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx >= 0) _users[idx] = user;
            return Task.CompletedTask;
        }
        // Método auxiliar para depuración: obtener todos los usuarios
        public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(_users);
        public List<User> GetAllUsers() => _users;
    }
}
