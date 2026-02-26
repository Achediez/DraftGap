using DraftGapBackend.Application.Dtos.Common;
using FluentValidation;

namespace DraftGapBackend.Application.Validators;

/// <summary>
/// Validador para parámetros de paginación.
/// Reglas:
/// - Page: >= 1 (primera página es 1, no 0)
/// - PageSize: entre 1 y 100 (límite para prevenir queries muy pesadas)
/// Usado en: GET /api/matches y otros endpoints paginados
/// </summary>
public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");
    }
}
