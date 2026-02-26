using DraftGapBackend.Application.Dtos.Champions;
using DraftGapBackend.Application.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for champion data and player champion statistics
/// </summary>
public interface IChampionService
{
    Task<List<ChampionDto>> GetAllChampionsAsync(CancellationToken cancellationToken = default);
    Task<ChampionDto?> GetChampionByIdAsync(int championId, CancellationToken cancellationToken = default);
    Task<List<ChampionStatsDto>> GetUserChampionStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}
