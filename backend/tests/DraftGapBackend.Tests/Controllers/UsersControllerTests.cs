using DraftGapBackend.API.Controllers;
using DraftGapBackend.Application.Dtos.Users;
using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DraftGapBackend.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IFriendsService> _mockFriendsService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockFriendsService = new Mock<IFriendsService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockFriendsService.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetUserByRiotId_EmptyRiotId_Returns400(string riotId)
    {
        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task GetUserByRiotId_NullRiotId_Returns400()
    {
        // Act
        var result = await _controller.GetUserByRiotId(null!, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Theory]
    [InlineData("InvalidFormat")]
    [InlineData("NoTag")]
    [InlineData("Multiple#Hash#Tags")]
    public async Task GetUserByRiotId_InvalidFormat_Returns400(string riotId)
    {
        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Theory]
    [InlineData("#TAG")]
    [InlineData(" #TAG")]
    public async Task GetUserByRiotId_EmptyGameName_Returns400(string riotId)
    {
        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Theory]
    [InlineData("GameName#")]
    [InlineData("GameName# ")]
    public async Task GetUserByRiotId_EmptyTagLine_Returns400(string riotId)
    {
        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task GetUserByRiotId_UserNotFound_Returns404()
    {
        // Arrange
        var riotId = "NonExistent#EUW";
        _mockFriendsService.Setup(s => s.GetUserDetailsByRiotIdAsync(riotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetailsByRiotIdDto?)null);

        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public async Task GetUserByRiotId_UserFoundWithoutSecondaryData_Returns200WithNullsAndEmptyArrays()
    {
        // Arrange
        var riotId = "BasicUser#EUW";
        var userDetails = new UserDetailsByRiotIdDto
        {
            UserId = Guid.NewGuid(),
            Email = "basic@example.com",
            RiotId = riotId,
            Region = "euw1",
            LastSync = null,
            Summoner = null, // Sin datos de summoner
            RankedOverview = null, // Sin ranked
            RecentMatches = new List<DraftGapBackend.Application.Dtos.Dashboard.RecentMatchDto>(), // Array vacío
            TopChampions = new List<DraftGapBackend.Application.Dtos.Dashboard.TopChampionDto>() // Array vacío
        };

        _mockFriendsService.Setup(s => s.GetUserDetailsByRiotIdAsync(riotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDetails);

        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserDetailsByRiotIdDto>(okResult.Value);
        
        Assert.Equal(userDetails.UserId, response.UserId);
        Assert.Equal("basic@example.com", response.Email);
        Assert.Null(response.Summoner);
        Assert.Null(response.RankedOverview);
        Assert.Empty(response.RecentMatches);
        Assert.Empty(response.TopChampions);
    }

    [Fact]
    public async Task GetUserByRiotId_UserFoundWithCompleteData_Returns200WithAllData()
    {
        // Arrange
        var riotId = "CompleteUser#EUW";
        var userDetails = new UserDetailsByRiotIdDto
        {
            UserId = Guid.NewGuid(),
            Email = "complete@example.com",
            RiotId = riotId,
            Region = "euw1",
            LastSync = DateTime.UtcNow.AddHours(-1),
            Summoner = new UserSummonerInfoDto
            {
                Puuid = "test-puuid",
                SummonerName = "CompleteUser",
                ProfileIconId = 123,
                SummonerLevel = 100
            },
            RankedOverview = new DraftGapBackend.Application.Dtos.Dashboard.RankedOverviewDto
            {
                SoloQueue = new DraftGapBackend.Application.Dtos.Dashboard.RankedQueueDto
                {
                    QueueType = "RANKED_SOLO_5x5",
                    Tier = "GOLD",
                    Rank = "II",
                    LeaguePoints = 67,
                    Wins = 15,
                    Losses = 10,
                    TotalGames = 25,
                    Winrate = 60.0
                }
            },
            RecentMatches = new List<DraftGapBackend.Application.Dtos.Dashboard.RecentMatchDto>
            {
                new DraftGapBackend.Application.Dtos.Dashboard.RecentMatchDto
                {
                    MatchId = "EUW1_123",
                    ChampionName = "Aatrox",
                    Win = true,
                    Kills = 10,
                    Deaths = 5,
                    Assists = 8
                }
            },
            TopChampions = new List<DraftGapBackend.Application.Dtos.Dashboard.TopChampionDto>
            {
                new DraftGapBackend.Application.Dtos.Dashboard.TopChampionDto
                {
                    ChampionName = "Aatrox",
                    GamesPlayed = 25,
                    Wins = 15,
                    Winrate = 60.0,
                    AvgKda = 3.6
                }
            }
        };

        _mockFriendsService.Setup(s => s.GetUserDetailsByRiotIdAsync(riotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDetails);

        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserDetailsByRiotIdDto>(okResult.Value);
        
        Assert.Equal(userDetails.UserId, response.UserId);
        Assert.Equal("complete@example.com", response.Email);
        Assert.NotNull(response.Summoner);
        Assert.Equal("CompleteUser", response.Summoner.SummonerName);
        Assert.NotNull(response.RankedOverview);
        Assert.NotNull(response.RankedOverview.SoloQueue);
        Assert.Equal("GOLD", response.RankedOverview.SoloQueue.Tier);
        Assert.Single(response.RecentMatches);
        Assert.Single(response.TopChampions);
    }

    [Fact]
    public async Task GetUserByRiotId_ServiceThrowsException_Returns500()
    {
        // Arrange
        var riotId = "ErrorUser#EUW";
        _mockFriendsService.Setup(s => s.GetUserDetailsByRiotIdAsync(riotId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserByRiotId(riotId, CancellationToken.None);

        // Assert
        var statusCode = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCode.StatusCode);
    }
}
