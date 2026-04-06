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

public class LoginServiceTests
{
    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldThrowInvalidEmailException()
    {
        var cryptographyMock = new Mock<ICryptographyService>();
        var authTokenMock = new Mock<IAuthTokenService>();
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var hasherMock = new Mock<IRefreshTokenHasher>();
        var timeProviderMock = new Mock<ITimeProvider>();

        ConfigureAuthSettings(authTokenMock);
        timeProviderMock.SetupGet(x => x.Now).Returns(new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc));
        userQueryMock.Setup(x => x.GetUserByEmail("ghost@test.com")).ReturnsAsync((User)null!);

        var service = new LoginService(
            cryptographyMock.Object,
            authTokenMock.Object,
            userQueryMock.Object,
            userCommandMock.Object,
            refreshTokenCommandMock.Object,
            hasherMock.Object,
            timeProviderMock.Object);

        var request = new LoginRequest { Email = "ghost@test.com", Password = "Password123!" };

        Func<Task> act = () => service.Login(request);

        await act.Should().ThrowAsync<InvalidEmailException>();
        userCommandMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenUserIsInactive_ShouldThrowInactiveUserException()
    {
        var cryptographyMock = new Mock<ICryptographyService>();
        var authTokenMock = new Mock<IAuthTokenService>();
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var hasherMock = new Mock<IRefreshTokenHasher>();
        var timeProviderMock = new Mock<ITimeProvider>();

        ConfigureAuthSettings(authTokenMock);
        timeProviderMock.SetupGet(x => x.Now).Returns(new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc));

        userQueryMock.Setup(x => x.GetUserByEmail("user@test.com")).ReturnsAsync(new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@test.com",
            Password = "hashed",
            IsActive = false,
            IsEmailVerified = true
        });

        var service = new LoginService(
            cryptographyMock.Object,
            authTokenMock.Object,
            userQueryMock.Object,
            userCommandMock.Object,
            refreshTokenCommandMock.Object,
            hasherMock.Object,
            timeProviderMock.Object);

        var request = new LoginRequest { Email = "user@test.com", Password = "Password123!" };

        Func<Task> act = () => service.Login(request);

        await act.Should().ThrowAsync<InactiveUserException>();
    }

    [Fact]
    public async Task Login_WhenPasswordIsInvalidAndThresholdReached_ShouldLockUserAndThrowInvalidValueException()
    {
        var now = new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc);

        var cryptographyMock = new Mock<ICryptographyService>();
        var authTokenMock = new Mock<IAuthTokenService>();
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var hasherMock = new Mock<IRefreshTokenHasher>();
        var timeProviderMock = new Mock<ITimeProvider>();
        var userId = Guid.NewGuid();

        ConfigureAuthSettings(authTokenMock, maxFailedAttempts: 3, lockoutMinutes: 15, refreshLifetimeMinutes: 60);

        var user = new User
        {
            UserId = userId,
            Email = "user@test.com",
            Password = "hashed",
            IsActive = true,
            IsEmailVerified = true,
            AccessFailedCount = 2,
            LockoutEndDate = null
        };

        timeProviderMock.SetupGet(x => x.Now).Returns(now);
        userQueryMock.Setup(x => x.GetUserByEmail("user@test.com")).ReturnsAsync(user);
        cryptographyMock.Setup(x => x.VerifyPassword("hashed", "bad-pass")).ReturnsAsync(false);

        var service = new LoginService(
            cryptographyMock.Object,
            authTokenMock.Object,
            userQueryMock.Object,
            userCommandMock.Object,
            refreshTokenCommandMock.Object,
            hasherMock.Object,
            timeProviderMock.Object);

        var request = new LoginRequest { Email = "user@test.com", Password = "bad-pass" };

        Func<Task> act = () => service.Login(request);

        await act.Should().ThrowAsync<InvalidValueException>();
        user.AccessFailedCount.Should().Be(3);
        user.LockoutEndDate.Should().Be(now.AddMinutes(15));
        userCommandMock.Verify(x => x.Update(user), Times.Once);
        refreshTokenCommandMock.Verify(x => x.Insert(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnTokensAndResetLockoutData()
    {
        var now = new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc);

        var cryptographyMock = new Mock<ICryptographyService>();
        var authTokenMock = new Mock<IAuthTokenService>();
        var userQueryMock = new Mock<IUserQuery>();
        var userCommandMock = new Mock<IUserCommand>();
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var hasherMock = new Mock<IRefreshTokenHasher>();
        var timeProviderMock = new Mock<ITimeProvider>();
        var userId = Guid.NewGuid();

        ConfigureAuthSettings(authTokenMock, refreshLifetimeMinutes: 30);

        var user = new User
        {
            UserId = userId,
            Email = "user@test.com",
            Password = "stored-hash",
            IsActive = true,
            IsEmailVerified = true,
            AccessFailedCount = 4,
            LockoutEndDate = now.AddMinutes(-1)
        };

        RefreshToken? insertedToken = null;

        timeProviderMock.SetupGet(x => x.Now).Returns(now);
        userQueryMock.Setup(x => x.GetUserByEmail("user@test.com")).ReturnsAsync(user);
        cryptographyMock.Setup(x => x.VerifyPassword("stored-hash", "Password123!")).ReturnsAsync(true);
        authTokenMock.Setup(x => x.GenerateRefreshToken()).ReturnsAsync("raw-refresh-token");
        hasherMock.Setup(x => x.Hash("raw-refresh-token")).Returns("hashed-refresh-token");
        authTokenMock.Setup(x => x.GenerateAccessToken(user)).ReturnsAsync("access-token");
        refreshTokenCommandMock.Setup(x => x.Insert(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => insertedToken = token)
            .Returns(Task.CompletedTask);

        var service = new LoginService(
            cryptographyMock.Object,
            authTokenMock.Object,
            userQueryMock.Object,
            userCommandMock.Object,
            refreshTokenCommandMock.Object,
            hasherMock.Object,
            timeProviderMock.Object);

        var request = new LoginRequest { Email = "user@test.com", Password = "Password123!" };

        var result = await service.Login(request);

        result.Result.Should().BeTrue();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");
        user.AccessFailedCount.Should().Be(0);
        user.LockoutEndDate.Should().BeNull();

        insertedToken.Should().NotBeNull();
        insertedToken!.Token.Should().Be("hashed-refresh-token");
        insertedToken.CreateDate.Should().Be(now);
        insertedToken.ExpireDate.Should().Be(now.AddMinutes(30));
        insertedToken.UserId.Should().Be(user.UserId);
        insertedToken.IsActive.Should().BeTrue();
        insertedToken.LastUsed.Should().Be(now);

        userCommandMock.Verify(x => x.Update(user), Times.Once);
        refreshTokenCommandMock.Verify(x => x.Insert(It.IsAny<RefreshToken>()), Times.Once);
    }

    private static void ConfigureAuthSettings(
        Mock<IAuthTokenService> authTokenMock,
        int maxFailedAttempts = 5,
        int lockoutMinutes = 15,
        int refreshLifetimeMinutes = 60)
    {
        authTokenMock.Setup(x => x.GetMaxFailedAccessAttempts()).Returns(maxFailedAttempts);
        authTokenMock.Setup(x => x.GetLockoutDurationMinutes()).Returns(lockoutMinutes);
        authTokenMock.Setup(x => x.GetRefreshTokenLifetimeInMinutes()).ReturnsAsync(refreshLifetimeMinutes);
    }
}
