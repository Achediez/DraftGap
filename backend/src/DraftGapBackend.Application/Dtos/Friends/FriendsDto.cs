using System;

namespace DraftGapBackend.Application.Dtos.Friends;

/// <summary>
/// Request to search for a user by Riot ID
/// </summary>
public class SearchUserRequest
{
    public string RiotId { get; set; } = string.Empty;
}

/// <summary>
/// User search result
/// </summary>
public class UserSearchResultDto
{
    public Guid UserId { get; set; }
    public string RiotId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? SummonerName { get; set; }
    public int? ProfileIconId { get; set; }
    public int? SummonerLevel { get; set; }
    public bool IsActive { get; set; }
}
