using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate email doesn't exist
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Hash password
        var passwordHash = BC.HashPassword(request.Password, workFactor: 11);

        var user = new User
        {
            // Validaciones de seguridad y formato (acumula todos los errores)
            var validationErrors = UserValidator.ValidateAll(request.UserName, request.Email, request.Password);
            if (validationErrors.Any())
            {
                // Muestra todos los errores en consola
                foreach (var err in validationErrors)
                    Console.WriteLine($"[VALIDATION ERROR] {err}");
                // Lanza una excepción con todos los errores concatenados
                throw new ArgumentException(string.Join(" | ", validationErrors));
            }

            await EnsureUserDoesNotExist(request.Email, request.UserName);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = HashPassword(request.Password)
            };
            await _userRepository.AddAsync(user);
            // Imprime la lista de usuarios tras registrar
            LogAllUsers();
            return user;
        }
        // Lógica de login de usuario
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            UserId = createdUser.UserId,
            Email = createdUser.Email,
            Message = "Registration successful"
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUserName);

        // Validate user exists and password is correct
        if (user == null || !BC.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is disabled");
        }

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Message = "Login successful"
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        return await _userRepository.GetAllActiveUsersAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _userRepository.UpdateAsync(user);
    }

    public async Task<User?> GetUserByRiotIdAsync(string riotId)
    {
        return await _userRepository.GetByRiotIdAsync(riotId);
    }

}