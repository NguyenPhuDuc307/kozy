using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using kozy_api.Controllers;
using kozy_api.Dtos;
using kozy_api.Models;
using kozy_api.Services;
using kozy_api.Data;

namespace kozy_api.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Create in-memory database context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _jwtServiceMock = new Mock<IJwtService>();
        _controller = new AuthController(_userManagerMock.Object, _context, _jwtServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user123", Email = "test@example.com", UserName = "test@example.com" };
        var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(user.Id)).Returns("test-token");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("test-token", response.Token);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "wrong@example.com", Password = "password123" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user123", Email = "test@example.com", UserName = "test@example.com" };
        var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong-password" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Password = "password123" };
        var user = new ApplicationUser { Id = "new-user-123", Email = registerDto.Email, UserName = registerDto.Email };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                       .ReturnsAsync(IdentityResult.Success)
                       .Callback<ApplicationUser, string>((u, p) => u.Id = user.Id);
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns("new-token");

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("new-token", response.Token);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Password = "weak" };
        var errors = new[] {
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
            new IdentityError { Code = "InvalidEmail", Description = "Email is invalid" }
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                       .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedErrors = Assert.IsAssignableFrom<IEnumerable<IdentityError>>(badRequestResult.Value);
        Assert.Equal(2, returnedErrors.Count());
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "existing@example.com", Password = "password123" };
        var error = new IdentityError { Code = "DuplicateUserName", Description = "Username already exists" };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                       .ReturnsAsync(IdentityResult.Failed(error));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedErrors = Assert.IsAssignableFrom<IEnumerable<IdentityError>>(badRequestResult.Value);
        Assert.Single(returnedErrors);
        Assert.Equal("DuplicateUserName", returnedErrors.First().Code);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithListOfUsers()
    {
        // Arrange
        _context.Users.AddRange(new[]
        {
            new ApplicationUser { Id = "user1", Email = "user1@example.com", UserName = "user1@example.com" },
            new ApplicationUser { Id = "user2", Email = "user2@example.com", UserName = "user2@example.com" }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }
}
