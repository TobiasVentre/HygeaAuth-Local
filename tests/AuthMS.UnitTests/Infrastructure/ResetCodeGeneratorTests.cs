using FluentAssertions;
using Infrastructure.Service;
using Xunit;

namespace AuthMS.Tests.Infrastructure
{
    public class ResetCodeGeneratorTests
    {
        [Fact]
        public void GenerateResetCode_ShouldReturnExpectedLength()
        {
            // Arrange
            var generator = new ResetCodeGenerator();
            var length = 12;

            // Act
            var code = generator.GenerateResetCode(length);

            // Assert
            code.Should().NotBeNullOrWhiteSpace();
            code.Length.Should().Be(length);
        }
    }
}
