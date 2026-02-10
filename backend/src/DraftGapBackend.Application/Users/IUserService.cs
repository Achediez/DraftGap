using DraftGapBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Users;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllActiveUsersAsync();
}