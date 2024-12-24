using Xunit;
using FluentAssertions;
using System;
using ArchitectureTest.Infrastructure.Services;

namespace ArchitectureTest.Infrastructure.Tests.Services;

public class PasswordHasherTests {
    private readonly PasswordHasher _systemUnderTest;
    public PasswordHasherTests(){
        _systemUnderTest = new();
    }

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnCorrectHash()
    {
        // Arrange
        var password = "P455w0rd";

        // Act
        var result = _systemUnderTest.Hash(password);

        // Assert
        var hashParts = result.Split('.');
        result.Should().NotBe(null);
        hashParts.Should().NotBeEmpty();
        hashParts[0].Should().BeEquivalentTo("10000");
        hashParts.Length.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("anyHash")]
    [InlineData("only.twoparts")]
    [InlineData("this.has.four.parts")]
    [InlineData("more.than.three.parts.should.fail")]
    public void Check_WithInvalidHash_ShouldThrowException(string hash)
    {
        // Arrange
        var password = "P455w0rd";

        // Act
        Action act = () => _systemUnderTest.Check(hash, password);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Unexpected hash format. Should be formatted as `{iterations}.{salt}.{hash}`");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Check_WithValidHash_ShouldReturnCorrectResult(bool replaceIterations)
    {
        // Arrange
        var password = "P455w0rd";
        var hash = _systemUnderTest.Hash(password);
        if (replaceIterations)
            hash = hash.Replace("10000.", "20000.");

        // Act
        var (verified, needsUpgrade) = _systemUnderTest.Check(hash, password);

        // Assert
        verified.Should().Be(!replaceIterations);
        needsUpgrade.Should().Be(replaceIterations);
    }
}
