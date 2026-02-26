using DraftGapBackend.Application.Common;
using DraftGapBackend.Application.Friends;
using DraftGapBackend.Application.Matches;
using DraftGapBackend.Application.Profile;
using FluentValidation.TestHelper;

namespace DraftGapBackend.Tests.Validators;

public class ValidationTests
{
    [Fact]
    public void PaginationRequest_InvalidPage_FailsValidation()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest { Page = 0, PageSize = 10 };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void PaginationRequest_InvalidPageSize_FailsValidation()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest { Page = 1, PageSize = 101 };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void PaginationRequest_ValidRequest_PassesValidation()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest { Page = 1, PageSize = 20 };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SearchUserRequest_InvalidRiotId_FailsValidation()
    {
        // Arrange
        var validator = new SearchUserRequestValidator();
        var request = new SearchUserRequest { RiotId = "InvalidFormat" };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RiotId);
    }

    [Fact]
    public void SearchUserRequest_ValidRiotId_PassesValidation()
    {
        // Arrange
        var validator = new SearchUserRequestValidator();
        var request = new SearchUserRequest { RiotId = "TestUser#EUW" };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfileRequest_ValidRiotId_PassesValidation()
    {
        // Arrange
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            RiotId = "NewUser#NA12",
            Region = "na1"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MatchFilterRequest_InvalidDateRange_FailsValidation()
    {
        // Arrange
        var validator = new MatchFilterRequestValidator();
        var request = new MatchFilterRequest
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }
}
