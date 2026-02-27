using DraftGapBackend.Application.Dtos.Friends;
using DraftGapBackend.Application.Dtos.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for user search and friend functionality
/// </summary>
public interface IFriendsService
{
    Task<UserSearchResultDto?> SearchUserByRiotIdAsync(string riotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene detalles completos de un usuario por su Riot ID.
    /// Incluye: perfil, summoner, ranked stats, partidas recientes, top champions
    /// </summary>
    /// <param name="riotId">Riot ID en formato GameName#TAG</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalles agregados del usuario o null si no existe</returns>
    Task<UserDetailsByRiotIdDto?> GetUserDetailsByRiotIdAsync(string riotId, CancellationToken cancellationToken = default);
}
