using DraftGapBackend.Application.Dtos.Friends;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para búsqueda de usuarios y funcionalidad de amigos.
/// Responsabilidades:
/// - Buscar usuarios registrados por Riot ID
/// - Proveer información pública de summoner
/// - Base para futura funcionalidad de sistema de amigos
/// </summary>
public class FriendsService : IFriendsService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<FriendsService> _logger;

    public FriendsService(
        IUserRepository userRepository,
        IPlayerRepository playerRepository,
        ILogger<FriendsService> logger)
    {
        _userRepository = userRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Busca un usuario en la base de datos por su Riot ID.
    /// Útil para:
    /// - Comparar stats con otros jugadores
    /// - Agregar amigos (futura funcionalidad)
    /// - Verificar si un jugador está registrado
    /// </summary>
    /// <param name="riotId">Riot ID en formato GameName#TAG</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del usuario o null si no está registrado</returns>
    public async Task<UserSearchResultDto?> SearchUserByRiotIdAsync(string riotId, CancellationToken cancellationToken = default)
    {
        // Buscar en la tabla users por riot_id
        var user = await _userRepository.GetByRiotIdAsync(riotId);
        if (user == null)
            return null;

        var player = !string.IsNullOrEmpty(user.RiotPuuid)
            ? await _playerRepository.GetByPuuidAsync(user.RiotPuuid, cancellationToken)
            : null;

        return new UserSearchResultDto
        {
            UserId = user.UserId,
            RiotId = user.RiotId ?? string.Empty,
            Region = user.Region ?? "euw1",
            SummonerName = player?.SummonerName,
            ProfileIconId = player?.ProfileIconId,
            SummonerLevel = player?.SummonerLevel,
            IsActive = user.IsActive
        };
    }
}
