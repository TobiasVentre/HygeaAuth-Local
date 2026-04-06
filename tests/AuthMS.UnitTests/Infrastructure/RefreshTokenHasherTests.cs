using FluentAssertions;
using Infrastructure.Service;
using Microsoft.Extensions.Configuration;

namespace AuthMS.Tests.Infrastructure;

public class RefreshTokenHasherTests
{
    [Fact]
    public void Constructor_WithMissingHashKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        Action act = () => _ = new RefreshTokenHasher(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Hash_WithSameTokenAndKey_ShouldReturnSameHash()
    {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            ["RefreshTokenSettings:HashKey"] = "this-is-a-strong-test-key-with-length-over-32"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var hasher = new RefreshTokenHasher(configuration);

        // Act
        var first = hasher.Hash("token-value");
        var second = hasher.Hash("token-value");

        // Assert
        first.Should().Be(second);
        first.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_WithEmptyToken_ShouldThrowArgumentException()
    {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            ["RefreshTokenSettings:HashKey"] = "this-is-a-strong-test-key-with-length-over-32"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var hasher = new RefreshTokenHasher(configuration);

        // Act
        Action act = () => hasher.Hash(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
