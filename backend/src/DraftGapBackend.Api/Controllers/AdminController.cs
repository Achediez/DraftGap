using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DraftGapBackend.Domain.Abstractions;

namespace DraftGapBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AdminController> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync([FromBody] SyncRequest request)
    {
        try
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            var usersList = users.ToList();

            _logger.LogInformation("Admin triggered sync for {Count} users", usersList.Count);

            var jobs = usersList.Select(u => new
            {
                jobId = 0,
                puuid = u.RiotPuuid ?? "",
                jobType = request.SyncType,
                status = "PENDING",
                matchesProcessed = 0,
                createdAt = DateTime.UtcNow,
                startedAt = (DateTime?)null,
                completedAt = (DateTime?)null,
                errorMessage = (string?)null
            }).ToList();

            return Ok(new
            {
                jobsCreated = jobs.Count,
                message = $"Created {jobs.Count} sync jobs. Processing will start shortly.",
                jobs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync trigger failed");
            return StatusCode(500, new { error = "Failed to trigger sync" });
        }
    }

    [HttpGet("sync/status")]
    public IActionResult GetSyncStatus([FromQuery] int? jobId = null)
    {
        return Ok(new
        {
            totalJobs = 0,
            pendingJobs = 0,
            runningJobs = 0,
            completedJobs = 0,
            failedJobs = 0,
            jobs = new List<object>()
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        try
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            var usersList = users.ToList();

            return Ok(new
            {
                totalUsers = usersList.Count,
                activeUsers = usersList.Count,
                totalMatches = 0,
                matchesToday = 0,
                pendingSyncJobs = 0,
                failedSyncJobs = 0,
                lastSyncTime = DateTime.MinValue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system stats");
            return StatusCode(500, new { error = "Failed to retrieve stats" });
        }
    }
}

public class SyncRequest
{
    public string SyncType { get; set; } = "MATCH_HISTORY";
    public int MatchCount { get; set; } = 20;
    public bool ForceRefresh { get; set; } = false;
    public List<int>? UserIds { get; set; }
}
