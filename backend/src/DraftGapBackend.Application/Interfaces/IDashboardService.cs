using DraftGapBackend.Application.Dtos.Dashboard;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for dashboard summary and overview
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
}
