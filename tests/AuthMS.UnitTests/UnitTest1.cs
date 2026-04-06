using FluentAssertions;
using Infrastructure.Service;

namespace AuthMS.Tests;

public class TimeProviderTests
{
    [Fact]
    public void UtcTimeProvider_Now_ShouldBeCloseToUtcNow()
    {
        // Arrange
        var provider = new UtcTimeProvider();
        var before = DateTime.UtcNow;

        // Act
        var result = provider.Now;
        var after = DateTime.UtcNow;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void ArgentinaTimeProvider_Now_ShouldBeCloseToArgentinaLocalTime()
    {
        // Arrange
        var provider = new ArgentinaTimeProvider();
        var argentinaZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        var expected = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaZone);

        // Act
        var result = provider.Now;

        // Assert
        (result - expected).Duration().Should().BeLessThan(TimeSpan.FromSeconds(2));
    }
}
