using DraftGapBackend.Application.Dtos.Users;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DraftGapBackend.Tests.Services;

public class UserSearchByRiotIdTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPlayerRepository> _mockPlayerRepo;
    private readonly Mock<IMatchRepository> _mockMatchRepo;
    private readonly Mock<IRankedRepository> _mockRankedRepo;
    private readonly Mock<ILogger<FriendsService>> _mockLogger;
    private readonly FriendsService _service;

    public UserSearchByRiotIdTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPlayerRepo = new Mock<IPlayerRepository>();
        _mockMatchRepo = new Mock<IMatchRepository>();
        _mockRankedRepo = new Mock<IRankedRepository>();
        _mockLogger = new Mock<ILogger<FriendsService>>();

        _service = new FriendsService(
            _mockUserRepo.Object,
            _mockPlayerRepo.Object,
            _mockMatchRepo.Object,
            _mockRankedRepo.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetUserDetailsByRiotId_UserNotFound_ReturnsNull()
    {
        // Arrange
        var riotId = "NonExistent#EUW";
        _mockUserRepo.Setup(r => r.GetByRiotIdAsync(riotId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserDetailsByRiotIdAsync(riotId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserDetailsByRiotId_UserFoundNoPuuid_ReturnsBasicData()
    {
        // Arrange
        var riotId = "TestUser#EUW";
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "dummy-hash",
            RiotId = riotId,
            Region = "euw1",
            RiotPuuid = null, // Sin PUUID vinculado
            LastSync = null
        };

        _mockUserRepo.Setup(r => r.GetByRiotIdAsync(riotId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserDetailsByRiotIdAsync(riotId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(riotId, result.RiotId);
        Assert.Equal("euw1", result.Region);
        Assert.Null(result.Summoner); // Sin summoner porque no hay PUUID
        Assert.Null(result.RankedOverview); // Sin ranked porque no hay PUUID
        Assert.Empty(result.RecentMatches);
        Assert.Empty(result.TopChampions);
    }

    [Fact]
    public async Task GetUserDetailsByRiotId_UserFoundWithCompleteData_ReturnsAllData()
    {
        // Arrange
        var riotId = "CompleteUser#EUW";
        var puuid = "test-puuid-123";
        var userId = Guid.NewGuid();

        var user = new User
        {
            UserId = userId,
            Email = "complete@example.com",
            PasswordHash = "dummy-hash",
            RiotId = riotId,
            Region = "euw1",
            RiotPuuid = puuid,
            LastSync = DateTime.UtcNow.AddHours(-1)
        };

        var player = new Player
        {
            Puuid = puuid,
            SummonerName = "CompleteUser",
            ProfileIconId = 123,
            SummonerLevel = 100,
            Region = "euw1"
        };

        var rankedStats = new List<PlayerRankedStat>
        {
            new PlayerRankedStat
            {
                Puuid = puuid,
                QueueType = "RANKED_SOLO_5x5",
                Tier = "GOLD",
                Rank = "II",
                LeaguePoints = 67,
                Wins = 15,
                Losses = 10
            }
        };

        var recentMatches = new List<MatchParticipant>
        {
            new MatchParticipant
            {
                Puuid = puuid,
                MatchId = "EUW1_123",
                ChampionName = "Aatrox",
                Win = true,
                Kills = 10,
                Deaths = 5,
                Assists = 8,
                TeamPosition = "TOP",
                Match = new DraftGapBackend.Domain.Entities.Match
                {
                    MatchId = "EUW1_123",
                    GameCreation = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    GameDuration = 1800
                }
            }
        };

        _mockUserRepo.Setup(r => r.GetByRiotIdAsync(riotId))
            .ReturnsAsync(user);
        _mockPlayerRepo.Setup(r => r.GetByPuuidAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        _mockRankedRepo.Setup(r => r.GetPlayerRankedStatsAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rankedStats);
        _mockMatchRepo.Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentMatches);
        _mockMatchRepo.Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentMatches);

        // Act
        var result = await _service.GetUserDetailsByRiotIdAsync(riotId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(riotId, result.RiotId);
        
        // Summoner data
        Assert.NotNull(result.Summoner);
        Assert.Equal(puuid, result.Summoner.Puuid);
        Assert.Equal("CompleteUser", result.Summoner.SummonerName);
        Assert.Equal(123, result.Summoner.ProfileIconId);
        Assert.Equal(100, result.Summoner.SummonerLevel);
        
        // Ranked data
        Assert.NotNull(result.RankedOverview);
        Assert.NotNull(result.RankedOverview.SoloQueue);
        Assert.Equal("GOLD", result.RankedOverview.SoloQueue.Tier);
        Assert.Equal("II", result.RankedOverview.SoloQueue.Rank);
        
        // Matches
        Assert.Single(result.RecentMatches);
        Assert.Equal("Aatrox", result.RecentMatches[0].ChampionName);
        
        // Top champions
        Assert.Single(result.TopChampions);
        Assert.Equal("Aatrox", result.TopChampions[0].ChampionName);
    }

    [Theory]
    [InlineData("TestUser#EUW")]
    [InlineData("testuser#euw")]
    [InlineData("TESTUSER#EUW")]
    public async Task GetUserDetailsByRiotId_CaseInsensitiveSearch_FindsUser(string searchRiotId)
    {
        // Arrange
        var storedRiotId = "TestUser#EUW";
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "dummy-hash",
            RiotId = storedRiotId,
            Region = "euw1",
            RiotPuuid = null
        };

        // El repository debe hacer búsqueda case-insensitive
        _mockUserRepo.Setup(r => r.GetByRiotIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserDetailsByRiotIdAsync(searchRiotId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
    }
}
