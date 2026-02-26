using DraftGapBackend.Application.Dtos.Profile;
using FluentValidation;
using System.Linq;

namespace DraftGapBackend.Application.Validators;

/// <summary>
/// Validador para actualización de perfil.
/// Reglas:
/// - riotId: formato correcto GameName#TAG, máximo 16 chars antes del #
/// - region: debe ser un platform ID válido (euw1, na1, kr, etc.)
/// Al menos un campo debe ser proporcionado para la actualización.
/// </summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.RiotId), () =>
        {
            RuleFor(x => x.RiotId)
                .Matches(@"^[a-zA-Z0-9]{3,16}#[a-zA-Z0-9]{3,5}$")
                .WithMessage("Invalid Riot ID format. Must be GameName#TAG");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Region), () =>
        {
            RuleFor(x => x.Region)
                .Must(region => new[] { "br1", "eun1", "euw1", "jp1", "kr", "la1", "la2", "na1", "oc1", "ph2", "ru", "sg2", "th2", "tr1", "tw2", "vn2" }
                    .Contains(region))
                .WithMessage("Invalid region. Must be a valid Riot platform ID (e.g., euw1, na1, kr)");
        });
    }
}
