using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Implementación de repositorio de usuarios (ejemplo con memoria, reemplazar por EF u otro más adelante)
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;

namespace DraftGapBackend.Infrastructure.Persistence
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        public Task<User?> GetByUserNameAsync(string userName) => Task.FromResult(_users.FirstOrDefault(u => u.UserName == userName));
        public Task AddAsync(User user) { _users.Add(user); return Task.CompletedTask; }
    }
}
