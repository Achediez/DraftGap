using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Dtos.Common;

/// <summary>
/// Parámetros estándar de paginación para endpoints que retornan listas.
/// Convención:
/// - Page es 1-based (primera página = 1)
/// - PageSize máximo: 100 (validado por CommonValidators)
/// Usado en: /api/matches, /api/admin/users (futuro), etc.
/// </summary>
public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Wrapper genérico para respuestas paginadas.
/// Incluye:
/// - Items: Datos de la página actual
/// - Metadata: page, pageSize, totalCount, totalPages
/// - Flags de navegación: hasNextPage, hasPreviousPage
/// Permite al cliente implementar paginación sin hacer queries adicionales.
/// </summary>
/// <typeparam name="T">Tipo de los items en la colección</typeparam>
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
