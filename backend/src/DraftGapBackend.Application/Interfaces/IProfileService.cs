using DraftGapBackend.Application.Dtos.Profile;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for user profile management
/// </summary>
public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
