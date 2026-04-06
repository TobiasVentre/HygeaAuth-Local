using Application.Interfaces.IServices.ICryptographyService;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AuthMS.Tests.Application.UseCase.CryptographyService;

public class CryptographyServiceTests
{
    [Fact]
    public async Task VerifyPassword_WhenStoredHashWasCreatedWithLegacyHashSize_ShouldStillValidate()
    {
        var legacyHasher = BuildService(hashSize: "32");
        var currentVerifier = BuildService(hashSize: "64");

        var hashedPassword = await legacyHasher.HashPassword("Password123!");

        var isValid = await currentVerifier.VerifyPassword(hashedPassword, "Password123!");

        isValid.Should().BeTrue();
    }

    private static ICryptographyService BuildService(
        string saltSize = "32",
        string hashSize = "64",
        string degreeOfParallelism = "8",
        string memorySize = "8192",
        string iterations = "40")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SaltSettings:SaltSize"] = saltSize,
                ["SaltSettings:HashSize"] = hashSize,
                ["Argon2Settings:DegreeOfParallelism"] = degreeOfParallelism,
                ["Argon2Settings:MemorySize"] = memorySize,
                ["Argon2Settings:Iterations"] = iterations
            })
            .Build();

        return new global::Infrastructure.Service.CryptographyService(configuration);
    }
}
