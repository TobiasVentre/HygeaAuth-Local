using Application.Dtos.Request;
using Application.Validators;
using FluentAssertions;

namespace AuthMS.Tests.Application.Validators;

public class LoginRequestValidatorTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "SomePassword123!"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveEmailError()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = "SomePassword123!"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = string.Empty
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Password));
    }
}

public class RefreshTokenRequestValidatorTests
{
    [Fact]
    public void Validate_WithMissingTokens_ShouldHaveTwoErrors()
    {
        // Arrange
        var validator = new RefreshTokenRequestValidator();
        var request = new RefreshTokenRequest
        {
            ExpiredAccessToken = string.Empty,
            RefreshToken = string.Empty
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Validate_WithValidTokens_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new RefreshTokenRequestValidator();
        var request = new RefreshTokenRequest
        {
            ExpiredAccessToken = "expired-access-token",
            RefreshToken = "refresh-token"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class PasswordChangeRequestValidatorTests
{
    [Fact]
    public void Validate_WithSameCurrentAndNewPassword_ShouldHaveError()
    {
        // Arrange
        var validator = new PasswordChangeRequestValidator();
        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "Password123!"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PasswordChangeRequest.NewPassword));
    }

    [Fact]
    public void Validate_WithWeakNewPassword_ShouldHaveError()
    {
        // Arrange
        var validator = new PasswordChangeRequestValidator();
        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "weak"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PasswordChangeRequest.NewPassword));
    }

    [Fact]
    public void Validate_WithStrongDifferentPassword_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new PasswordChangeRequestValidator();
        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPass456@"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class UserRequestValidatorTests
{
    [Fact]
    public void Validate_WithInvalidRole_ShouldHaveRoleError()
    {
        // Arrange
        var validator = new UserRequestValidator();
        var request = BuildValidRequest();
        request.Role = "operator";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRequest.Role));
    }

    [Fact]
    public void Validate_WithTechnicianWithoutSpecialty_ShouldHaveSpecialtyError()
    {
        // Arrange
        var validator = new UserRequestValidator();
        var request = BuildValidRequest();
        request.Role = "technician";
        request.Specialty = string.Empty;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRequest.Specialty));
    }

    [Fact]
    public void Validate_WithClientAndSpecialty_ShouldHaveSpecialtyError()
    {
        // Arrange
        var validator = new UserRequestValidator();
        var request = BuildValidRequest();
        request.Role = "client";
        request.Specialty = "dermatology";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRequest.Specialty));
    }

    [Fact]
    public void Validate_WithValidTechnician_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new UserRequestValidator();
        var request = BuildValidRequest();
        request.Role = "technician";
        request.Specialty = "plagas";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static UserRequest BuildValidRequest()
    {
        return new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@test.com",
            Dni = "ABC12345",
            Password = "Password123!",
            Role = "client",
            Phone = "1122334455",
            Specialty = null
        };
    }
}
