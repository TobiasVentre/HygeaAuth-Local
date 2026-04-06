using Application.Dtos.Request;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.UseCase.AuthServices;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace AuthMS.Tests.Application.UseCase.AuthServices;

public class LogoutServiceTests
{
    [Fact]
    public async Task Logout_WhenRefreshTokenDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var refreshTokenQueryMock = new Mock<IRefreshTokenQuery>();

        refreshTokenQueryMock.Setup(x => x.GetByToken("missing-token")).ReturnsAsync((RefreshToken)null!);

        var service = new LogoutService(refreshTokenCommandMock.Object, refreshTokenQueryMock.Object);
        var request = new LogoutRequest { RefreshToken = "missing-token" };

        // Act
        Func<Task> act = () => service.Logout(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        refreshTokenCommandMock.Verify(x => x.Delete(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_WhenRefreshTokenExists_ShouldDeleteTokenAndReturnSuccessMessage()
    {
        // Arrange
        var refreshTokenCommandMock = new Mock<IRefreshTokenCommand>();
        var refreshTokenQueryMock = new Mock<IRefreshTokenQuery>();

        var token = new RefreshToken
        {
            RefreshTokenId = 1,
            Token = "stored-token",
            IsActive = true,
            UserId = Guid.NewGuid()
        };

        refreshTokenQueryMock.Setup(x => x.GetByToken("stored-token")).ReturnsAsync(token);
        refreshTokenCommandMock.Setup(x => x.Delete(token)).Returns(Task.CompletedTask);

        var service = new LogoutService(refreshTokenCommandMock.Object, refreshTokenQueryMock.Object);
        var request = new LogoutRequest { RefreshToken = "stored-token" };

        // Act
        var result = await service.Logout(request);

        // Assert
        result.Message.Should().Be("Cierre de sesión exitoso.");
        refreshTokenCommandMock.Verify(x => x.Delete(token), Times.Once);
    }
}
