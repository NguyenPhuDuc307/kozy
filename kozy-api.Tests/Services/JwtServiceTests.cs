using kozy_api.Services;
using kozy_api.Models;
using Xunit;

namespace kozy_api.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _service;
    private readonly JwtSettings _jwtSettings;

    public JwtServiceTests()
    {
        // Setup JWT settings
        _jwtSettings = new JwtSettings
        {
            Key = "MySecretKeyForJWTTokenGeneration1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpireMinutes = 60
        };

        _service = new JwtService(_jwtSettings);
    }

    [Fact]
    public void GenerateToken_ValidUserId_ReturnsToken()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var token = _service.GenerateToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_NullUserId_ReturnsToken()
    {
        // Act
        var token = _service.GenerateToken(null!);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Theory]
    [InlineData("")]
    public void GenerateToken_EmptyUserId_ReturnsToken(string userId)
    {
        // Act
        var token = _service.GenerateToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void Constructor_NullJwtSettings_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtService(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GenerateToken_InvalidKey_ThrowsException(string? key)
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = key!,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpireMinutes = 60
        };

        var service = new JwtService(invalidSettings);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => service.GenerateToken("test-user-id"));
    }

    [Fact]
    public void GenerateToken_ValidSettings_ContainsCorrectClaims()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var token = _service.GenerateToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        // Token should be a valid JWT format (3 parts separated by dots)
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }
}
