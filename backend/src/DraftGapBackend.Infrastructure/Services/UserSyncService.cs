using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Sync;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para sincronizaciones iniciadas por el usuario.
/// Responsabilidades:
/// - Permitir que usuarios disparen sync manual de sus datos
/// - Proveer historial de sincronizaciones (exitosas/fallidas)
/// - Rastrear estado de jobs en tiempo real
/// Diferencia con IDataSyncService: este es para operaciones de usuario final,
/// mientras que IDataSyncService es para admin y background workers.
/// </summary>
public class UserSyncService : IUserSyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserSyncService> _logger;

    public UserSyncService(
        IUserRepository userRepository,
        IDataSyncService dataSyncService,
        ApplicationDbContext context,
        ILogger<UserSyncService> logger)
    {
        _userRepository = userRepository;
        _dataSyncService = dataSyncService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Dispara una sincronización manual para el usuario autenticado.
    /// Crea un SyncJob que será procesado por el RiotSyncBackgroundService.
    /// El job actualizará:
    /// - Ranked stats (Solo/Duo, Flex)
    /// - Match history (nuevas partidas desde último sync)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="request">Opciones de sync (forceRefresh, etc.)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Job creado con estado PENDING</returns>
    public async Task<SyncJobDto> TriggerUserSyncAsync(Guid userId, TriggerSyncRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var job = await _dataSyncService.TriggerSyncForUserAsync(userId);

        return new SyncJobDto
        {
            JobId = (int)job.JobId,
            Puuid = job.Puuid,
            JobType = job.JobType,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage
        };
    }

    /// <summary>
    /// Obtiene el historial completo de sincronizaciones del usuario.
    /// Incluye:
    /// - LastSync: Última vez que se actualizó exitosamente
    /// - Contadores: total, exitosas, fallidas
    /// - LatestJob: Último job con su estado actual
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Historial de sincronizaciones</returns>
    public async Task<UserSyncHistoryDto> GetUserSyncHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var puuid = user.RiotPuuid;

        var allJobs = await _context.SyncJobs
            .Where(j => j.Puuid == puuid)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

        var latestJob = allJobs.FirstOrDefault();

        return new UserSyncHistoryDto
        {
            LastSync = user.LastSync,
            TotalSyncs = allJobs.Count,
            SuccessfulSyncs = allJobs.Count(j => j.Status == "COMPLETED"),
            FailedSyncs = allJobs.Count(j => j.Status == "FAILED"),
            LatestJob = latestJob != null ? new SyncJobDto
            {
                JobId = (int)latestJob.JobId,
                Puuid = latestJob.Puuid,
                JobType = latestJob.JobType,
                Status = latestJob.Status,
                CreatedAt = latestJob.CreatedAt,
                StartedAt = latestJob.StartedAt,
                CompletedAt = latestJob.CompletedAt,
                ErrorMessage = latestJob.ErrorMessage
            } : null
        };
    }
}
