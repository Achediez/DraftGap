using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Common;

/// <summary>
/// Standard pagination request parameters
/// </summary>
public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Standard paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the result set</typeparam>
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
