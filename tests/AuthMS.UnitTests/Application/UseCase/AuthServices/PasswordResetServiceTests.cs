using System.Security.Claims;
using Application.Dtos.Request;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Application.UseCase.AuthServices;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace AuthMS.Tests.Application.UseCase.AuthServices;

public class PasswordResetServiceTests
{
    [Fact]
    public async Task ChangePassword_WhenUserIsNotAuthenticated_ShouldThrowInvalidValueException()
    {
        // Arrange
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        currentUserContextMock.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Current123!",
            NewPassword = "NewPassword456@"
        };

        // Act
        Func<Task> act = () => service.ChangePassword(request);

        // Assert
        await act.Should().ThrowAsync<InvalidValueException>();
        userCommandMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WhenCurrentPasswordIsInvalid_ShouldThrowExceptionWithExpectedMessage()
    {
        // Arrange
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        var userId = Guid.NewGuid();
        currentUserContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var user = new User
        {
            UserId = userId,
            Email = "user@test.com",
            Password = "stored-hash",
            IsActive = true,
            IsEmailVerified = true
        };

        userQueryMock.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
        cryptographyMock.Setup(x => x.VerifyPassword("stored-hash", "Current123!")).ReturnsAsync(false);

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Current123!",
            NewPassword = "NewPassword456@"
        };

        // Act
        Func<Task> act = () => service.ChangePassword(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*contraseña actual es incorrecta*");
        userCommandMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WhenRequestIsValid_ShouldUpdateUserPassword()
    {
        // Arrange
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        var userId = Guid.NewGuid();
        currentUserContextMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var user = new User
        {
            UserId = userId,
            Email = "user@test.com",
            Password = "stored-hash",
            IsActive = true,
            IsEmailVerified = true
        };

        userQueryMock.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
        cryptographyMock.Setup(x => x.VerifyPassword("stored-hash", "Current123!")).ReturnsAsync(true);
        cryptographyMock.Setup(x => x.HashPassword("NewPassword456@")).ReturnsAsync("new-hash");
        userCommandMock.Setup(x => x.Update(user)).Returns(Task.CompletedTask);

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        var request = new PasswordChangeRequest
        {
            CurrentPassword = "Current123!",
            NewPassword = "NewPassword456@"
        };

        // Act
        var result = await service.ChangePassword(request);

        // Assert
        result.Message.Should().Contain("cambiada exitosamente");
        user.Password.Should().Be("new-hash");
        userCommandMock.Verify(x => x.Update(user), Times.Once);
    }

    [Fact]
    public async Task GenerateResetCode_WhenCalled_ShouldDeleteExpiredTokensInsertNewTokenAndSendEmail()
    {
        // Arrange
        var now = new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc);

        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        var expired1 = new PasswordResetToken { Email = "user@test.com", Token = "111111", Expiration = now.AddMinutes(-20) };
        var expired2 = new PasswordResetToken { Email = "user@test.com", Token = "222222", Expiration = now.AddMinutes(-10) };

        PasswordResetToken? insertedToken = null;

        timeProviderMock.SetupGet(x => x.Now).Returns(now);
        passwordResetQueryMock
            .Setup(x => x.GetExpiredTokensByEmail("user@test.com"))
            .ReturnsAsync(new List<PasswordResetToken> { expired1, expired2 });
        resetCodeGeneratorMock.Setup(x => x.GenerateResetCode(6)).Returns("ABC123");
        passwordResetCommandMock
            .Setup(x => x.Insert(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(token => insertedToken = token)
            .Returns(Task.CompletedTask);

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        // Act
        var result = await service.GenerateResetCode("user@test.com");

        // Assert
        result.Message.Should().Contain("código de restablecimiento");
        passwordResetCommandMock.Verify(x => x.Delete(expired1), Times.Once);
        passwordResetCommandMock.Verify(x => x.Delete(expired2), Times.Once);

        insertedToken.Should().NotBeNull();
        insertedToken!.Email.Should().Be("user@test.com");
        insertedToken.Token.Should().Be("ABC123");
        insertedToken.Expiration.Should().Be(now.AddMinutes(10));

        emailServiceMock.Verify(x => x.SendPasswordResetEmail("user@test.com", "ABC123"), Times.Once);
    }

    [Fact]
    public async Task ValidateResetCode_WhenTokenIsExpired_ShouldThrowExceptionWithExpectedMessage()
    {
        // Arrange
        var now = new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc);

        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        timeProviderMock.SetupGet(x => x.Now).Returns(now);
        passwordResetQueryMock
            .Setup(x => x.GetByEmailAndCode("user@test.com", "ABC123"))
            .ReturnsAsync(new PasswordResetToken
            {
                Email = "user@test.com",
                Token = "ABC123",
                Expiration = now.AddMinutes(-1)
            });

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        var request = new PasswordResetConfirmRequest
        {
            Email = "user@test.com",
            ResetCode = "ABC123",
            NewPassword = "NewPassword456@"
        };

        // Act
        Func<Task> act = () => service.ValidateResetCode(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*código no es válido o ha expirado*");
        userCommandMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ValidateResetCode_WhenRequestIsValid_ShouldUpdatePasswordAndDeleteToken()
    {
        // Arrange
        var now = new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc);

        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var cryptographyMock = new Mock<ICryptographyService>();
        var currentUserContextMock = new Mock<ICurrentUserContext>();
        var passwordResetCommandMock = new Mock<IPasswordResetCommand>();
        var passwordResetQueryMock = new Mock<IPasswordResetQuery>();
        var emailServiceMock = new Mock<IEmailService>();
        var resetCodeGeneratorMock = new Mock<IResetCodeGenerator>();
        var timeProviderMock = new Mock<ITimeProvider>();

        var resetToken = new PasswordResetToken
        {
            Email = "user@test.com",
            Token = "ABC123",
            Expiration = now.AddMinutes(5)
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@test.com",
            Password = "old-hash",
            IsActive = true,
            IsEmailVerified = true
        };

        timeProviderMock.SetupGet(x => x.Now).Returns(now);
        passwordResetQueryMock
            .Setup(x => x.GetByEmailAndCode("user@test.com", "ABC123"))
            .ReturnsAsync(resetToken);
        userQueryMock.Setup(x => x.GetUserByEmail("user@test.com")).ReturnsAsync(user);
        cryptographyMock.Setup(x => x.HashPassword("NewPassword456@")).ReturnsAsync("new-hash");
        userCommandMock.Setup(x => x.Update(user)).Returns(Task.CompletedTask);
        passwordResetCommandMock.Setup(x => x.Delete(resetToken)).Returns(Task.CompletedTask);

        var service = new PasswordResetService(
            userQueryMock.Object,
            userCommandMock.Object,
            cryptographyMock.Object,
            currentUserContextMock.Object,
            passwordResetCommandMock.Object,
            passwordResetQueryMock.Object,
            emailServiceMock.Object,
            resetCodeGeneratorMock.Object,
            timeProviderMock.Object);

        var request = new PasswordResetConfirmRequest
        {
            Email = "user@test.com",
            ResetCode = "ABC123",
            NewPassword = "NewPassword456@"
        };

        // Act
        var result = await service.ValidateResetCode(request);

        // Assert
        result.Message.Should().Contain("restablecida exitosamente");
        user.Password.Should().Be("new-hash");
        userCommandMock.Verify(x => x.Update(user), Times.Once);
        passwordResetCommandMock.Verify(x => x.Delete(resetToken), Times.Once);
    }
}

