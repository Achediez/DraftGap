using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Profile;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Riot;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

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

    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        ProfileSummonerDto? summonerDto = null;

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

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Update Riot ID if provided
        if (!string.IsNullOrWhiteSpace(request.RiotId) && request.RiotId != user.RiotId)
        {
            var parts = request.RiotId.Split('#');
            if (parts.Length != 2)
                throw new InvalidOperationException("Invalid Riot ID format");

            // Verify Riot account exists
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
