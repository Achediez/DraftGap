using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Profile;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Riot;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para gestión del perfil de usuario.
/// Responsabilidades:
/// - Obtener información completa del perfil (User + Player/Summoner)
/// - Actualizar Riot ID y región del usuario
/// - Validar cambios con la API de Riot antes de persistir
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRiotService _riotService;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IUserRepository userRepository,
        IPlayerRepository playerRepository,
        IRiotService riotService,
        ILogger<ProfileService> logger)
    {
        _userRepository = userRepository;
        _playerRepository = playerRepository;
        _riotService = riotService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil completo del usuario incluyendo datos de summoner si está disponible.
    /// Combina información de las tablas 'users' y 'players' para construir un perfil completo.
    /// </summary>
    /// <param name="userId">ID único del usuario en la base de datos</param>
    /// <param name="cancellationToken">Token para cancelación de operación</param>
    /// <returns>Perfil completo del usuario o null si no existe</returns>
    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Buscar usuario en la base de datos
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        ProfileSummonerDto? summonerDto = null;

        // Si el usuario tiene PUUID vinculado, obtener datos del summoner
        if (!string.IsNullOrEmpty(user.RiotPuuid))
        {
            var player = await _playerRepository.GetByPuuidAsync(user.RiotPuuid, cancellationToken);
            if (player != null)
            {
                summonerDto = new ProfileSummonerDto
                {
                    Puuid = player.Puuid,
                    SummonerName = player.SummonerName,
                    ProfileIconId = player.ProfileIconId,
                    SummonerLevel = player.SummonerLevel
                };
            }
        }

        return new ProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            RiotId = user.RiotId,
            Region = user.Region,
            LastSync = user.LastSync,
            IsAdmin = false, // Set by controller based on claims
            CreatedAt = user.CreatedAt,
            Summoner = summonerDto
        };
    }

    /// <summary>
    /// Actualiza el perfil del usuario (Riot ID y/o región).
    /// Valida cambios con la API de Riot antes de persistir para garantizar que la cuenta existe.
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="request">Datos de perfil a actualizar</param>
    /// <param name="cancellationToken">Token para cancelación</param>
    /// <returns>Perfil actualizado</returns>
    /// <exception cref="InvalidOperationException">Si el usuario no existe o la cuenta de Riot no es válida</exception>
    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Actualizar Riot ID si se proporciona y es diferente al actual
        if (!string.IsNullOrWhiteSpace(request.RiotId) && request.RiotId != user.RiotId)
        {
            // Parsear Riot ID en GameName y TagLine
            var parts = request.RiotId.Split('#');
            if (parts.Length != 2)
                throw new InvalidOperationException("Invalid Riot ID format");

            // Verificar que la cuenta de Riot existe consultando la API
            // Esto previene que usuarios vinculen cuentas inexistentes
            var riotAccount = await _riotService.GetAccountByRiotIdAsync(parts[0], parts[1], request.Region ?? user.Region ?? "euw1");
            if (riotAccount == null)
                throw new InvalidOperationException("Riot account not found");

            user.RiotId = request.RiotId;
            user.RiotPuuid = riotAccount.Puuid;
        }

        // Update region if provided
        if (!string.IsNullOrWhiteSpace(request.Region))
            user.Region = request.Region;

        await _userRepository.UpdateAsync(user);

        var profile = await GetProfileAsync(userId, cancellationToken);
        return profile ?? throw new InvalidOperationException("Failed to retrieve updated profile");
    }
}
