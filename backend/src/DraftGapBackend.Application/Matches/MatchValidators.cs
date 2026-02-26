using FluentValidation;
using System.Linq;

namespace DraftGapBackend.Application.Matches;

public class MatchFilterRequestValidator : AbstractValidator<MatchFilterRequest>
{
    public MatchFilterRequestValidator()
    {
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be before or equal to end date");
        });

        When(x => !string.IsNullOrWhiteSpace(x.TeamPosition), () =>
        {
            RuleFor(x => x.TeamPosition)
                .Must(pos => new[] { "TOP", "JUNGLE", "MIDDLE", "BOTTOM", "UTILITY" }.Contains(pos?.ToUpper()))
                .WithMessage("Team position must be one of: TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY");
        });

        RuleFor(x => x.QueueId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Queue ID must be non-negative");
    }
}
