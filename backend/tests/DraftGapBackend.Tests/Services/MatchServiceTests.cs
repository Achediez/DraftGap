using DraftGapBackend.Application.Dtos.Common;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Matches;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchEntity = DraftGapBackend.Domain.Entities.Match;

namespace DraftGapBackend.Tests.Services;

public class MatchServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ILogger<MatchService>> _mockLogger;
    private readonly IMatchService _matchService;

    public MatchServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockLogger = new Mock<ILogger<MatchService>>();

        _matchService = new MatchService(
            _mockUserRepository.Object,
            _mockMatchRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserMatches_ValidUser_ReturnsPaginatedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var puuid = "test-puuid-12345";

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            RiotPuuid = puuid
        };

        var participants = new List<MatchParticipant>
        {
            new()
            {
                ParticipantId = 1,
                MatchId = "EUW1_123456",
                Puuid = puuid,
                ChampionId = 266,
                ChampionName = "Aatrox",
                Win = true,
                Kills = 10,
                Deaths = 3,
                Assists = 7,
                TeamPosition = "TOP",
                Match = new MatchEntity
                {
                    MatchId = "EUW1_123456",
                    GameCreation = 1700000000000,
                    GameDuration = 1800,
                    GameMode = "CLASSIC",
                    GameType = "MATCHED_GAME",
                    QueueId = 420,
                    PlatformId = "EUW1",
                    GameVersion = "13.24"
                }
            }
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockMatchRepository
            .Setup(r => r.GetUserMatchCountAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMatchRepository
            .Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participants);

        var pagination = new PaginationRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _matchService.GetUserMatchesAsync(userId, pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        var firstMatch = result.Items.First();
        firstMatch.ChampionName.Should().Be("Aatrox");
        firstMatch.Win.Should().BeTrue();
        firstMatch.Kills.Should().Be(10);
    }

    [Fact]
    public async Task GetUserMatches_UserWithoutPuuid_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            RiotPuuid = null
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var pagination = new PaginationRequest { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _matchService.GetUserMatchesAsync(userId, pagination));
    }
}
