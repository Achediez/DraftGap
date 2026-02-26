using DraftGapBackend.Application.Dtos.Friends;
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
}
