using FluentValidation;

namespace DraftGapBackend.Application.Friends;

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
