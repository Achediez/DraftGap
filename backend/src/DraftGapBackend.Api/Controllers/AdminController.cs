using DraftGapBackend.Application.Admin;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DraftGapBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDataSyncService _dataSyncService;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _configuration;

    public AdminController(
        IDataSyncService dataSyncService,
        IUserRepository userRepository,
        ApplicationDbContext context,
        ILogger<AdminController> logger,
        IConfiguration configuration)
    {
        _dataSyncService = dataSyncService;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns a single user by ID for the admin panel.
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            var adminEmails = _configuration.GetSection("Admin:AllowedEmails").Get<List<string>>() ?? new List<string>();

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                riotId = user.RiotId,
                region = user.Region,
                lastSync = user.LastSync,
                hasPuuid = !string.IsNullOrEmpty(user.RiotPuuid),
                isAdmin = adminEmails.Contains(user.Email),
                createdAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user by id.");
            return StatusCode(500, new { error = "Failed to retrieve user." });
        }
    }

    /// <summary>
    /// Enqueues sync jobs for all active users or a specific subset via UserIds.
    /// The background worker picks up PENDING jobs automatically.
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync([FromBody] SyncRequest request)
    {
        try
        {
            request ??= new SyncRequest();

            _logger.LogInformation(
                "Admin triggered sync. SyncType={SyncType}, ForceRefresh={ForceRefresh}",
                request.SyncType, request.ForceRefresh);

            SyncTriggerResult result;

            if (request.UserIds != null && request.UserIds.Count > 0)
            {
                // Targeted sync: enqueue only the specified users.
                var jobs = new List<DraftGapBackend.Domain.Entities.SyncJob>();
                foreach (var userId in request.UserIds)
                {
                    var job = await _dataSyncService.TriggerSyncForUserAsync(userId);
                    jobs.Add(job);
                }

                result = new SyncTriggerResult(
                    jobs.Count,
                    $"Created {jobs.Count} sync jobs for specified users.",
                    jobs.AsReadOnly()
                );
            }
            else
            {
                // Bulk sync: enqueue all eligible active users.
                result = await _dataSyncService.TriggerSyncForAllUsersAsync();
            }

            var response = new SyncTriggerResponseDto
            {
                JobsCreated = result.JobsCreated,
                Message = result.Message,
                Jobs = result.Jobs.Select(j => new SyncJobInfoDto
                {
                    JobId = (int)j.JobId,
                    UserId = j.Puuid,
                    JobType = j.JobType,
                    Status = j.Status,
                    CreatedAt = j.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Sync trigger failed due to database constraints.");
            return BadRequest(new { error = "Failed to enqueue sync jobs due to inconsistent player data." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Sync trigger rejected: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync trigger failed unexpectedly.");
            return StatusCode(500, new { error = "Failed to trigger sync." });
        }
    }

    /// <summary>
    /// Returns aggregate counts of sync jobs by status, plus the last completion timestamp.
    /// </summary>
    [HttpGet("sync/status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var status = await _dataSyncService.GetSyncStatusAsync();

            return Ok(new
            {
                pendingJobs = status.PendingJobs,
                processingJobs = status.ProcessingJobs,
                completedJobs = status.CompletedJobs,
                failedJobs = status.FailedJobs,
                lastCompletedAt = status.LastCompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve sync status.");
            return StatusCode(500, new { error = "Failed to retrieve sync status." });
        }
    }

    /// <summary>
    /// Returns system-wide statistics: user counts, match counts, and sync job summary.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        try
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            var usersList = users.ToList();

            var totalMatches = await _context.Matches.CountAsync();

            var today = DateTime.UtcNow.Date;
            var todayUnixMs = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeMilliseconds();
            var tomorrowUnixMs = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1)).ToUnixTimeMilliseconds();

            var matchesToday = await _context.Matches
                .CountAsync(m => m.GameCreation >= todayUnixMs && m.GameCreation < tomorrowUnixMs);


            var syncStatus = await _dataSyncService.GetSyncStatusAsync();

            return Ok(new
            {
                totalUsers = usersList.Count,
                activeUsers = usersList.Count(u => u.IsActive),
                totalMatches,
                matchesToday,
                pendingSyncJobs = syncStatus.PendingJobs,
                failedSyncJobs = syncStatus.FailedJobs,
                lastSyncTime = syncStatus.LastCompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve system stats.");
            return StatusCode(500, new { error = "Failed to retrieve stats." });
        }
    }

    /// <summary>
    /// Returns all registered users with their sync state — useful for the admin panel table.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            var adminEmails = _configuration.GetSection("Admin:AllowedEmails").Get<List<string>>() ?? new List<string>();

            var userDtos = users.Select(u => new AdminUserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                RiotId = u.RiotId,
                Region = u.Region,
                LastSync = u.LastSync,
                HasPuuid = !string.IsNullOrEmpty(u.RiotPuuid),
                IsAdmin = adminEmails.Contains(u.Email),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user list.");
            return StatusCode(500, new { error = "Failed to retrieve users." });
        }
    }

    /// <summary>
    /// Deletes a user by ID for the admin panel.
    /// </summary>
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            await _userRepository.DeleteAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user.");
            return StatusCode(500, new { error = "Failed to delete user." });
        }
    }
}

// Request body for POST /api/admin/sync.
public class SyncRequest
{
    public string SyncType { get; set; } = "FULL_SYNC";
    public bool ForceRefresh { get; set; } = false;

    // When provided, only these users are synced. When null/empty, all active users are synced.
    public List<Guid>? UserIds { get; set; }
}
