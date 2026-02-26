using DraftGapBackend.Application.Friends;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

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

    public async Task<UserSearchResultDto?> SearchUserByRiotIdAsync(string riotId, CancellationToken cancellationToken = default)
    {
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
