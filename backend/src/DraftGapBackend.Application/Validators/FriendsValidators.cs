using DraftGapBackend.Application.Dtos.Friends;
using FluentValidation;

namespace DraftGapBackend.Application.Validators;

/// <summary>
/// Validador para búsqueda de usuario por Riot ID.
/// Reglas:
/// - riotId: obligatorio, no vacío
/// - formato correcto: GameName#TAG
/// - máximo 16 caracteres antes del #
/// Ejemplo válido: "Faker#KR1", "TSM Doublelift#NA1"
/// </summary>
public class SearchUserRequestValidator : AbstractValidator<SearchUserRequest>
{
    public SearchUserRequestValidator()
    {
        RuleFor(x => x.RiotId)
            .NotEmpty()
            .WithMessage("Riot ID is required")
            .Matches(@"^[a-zA-Z0-9]{3,16}#[a-zA-Z0-9]{3,5}$")
            .WithMessage("Invalid Riot ID format. Must be GameName#TAG");
    }
}
