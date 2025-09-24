using kozy_api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace kozy_api.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly FileService _fileService;
    private readonly string _testRootPath;
    private readonly string _testUploadPath;

    public FileServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<FileService>>();

        _testRootPath = Path.Combine(Path.GetTempPath(), "FileServiceTests", Guid.NewGuid().ToString());
        _testUploadPath = Path.Combine(_testRootPath, "wwwroot", "uploads");

        _mockEnvironment.Setup(x => x.ContentRootPath).Returns(_testRootPath);
        _fileService = new FileService(_mockEnvironment.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_NullEnvironment_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FileService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FileService(_mockEnvironment.Object, null!));
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ReturnsUniqueFileName()
    {
        var file = CreateMockFile("test.txt", "Hello World");
        var result = await _fileService.UploadFileAsync(file, "uploads");

        Assert.NotNull(result);
        Assert.EndsWith(".txt", result);
        Assert.True(File.Exists(Path.Combine(_testUploadPath, result)));
    }

    [Fact]
    public async Task UploadFileAsync_NullFile_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _fileService.UploadFileAsync(null!, "uploads"));
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ThrowsArgumentException()
    {
        var file = CreateMockFile("test.txt", "");
        await Assert.ThrowsAsync<ArgumentException>(() => _fileService.UploadFileAsync(file, "uploads"));
    }

    [Fact]
    public async Task DownloadFileAsync_ExistingFile_ReturnsFileBytes()
    {
        var content = "Hello World";
        var file = CreateMockFile("test.txt", content);
        var fileName = await _fileService.UploadFileAsync(file, "uploads");

        var result = await _fileService.DownloadFileAsync(fileName, "uploads");

        Assert.NotNull(result);
        Assert.Equal(Encoding.UTF8.GetBytes(content), result);
    }

    [Fact]
    public async Task DownloadFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _fileService.DownloadFileAsync("nonexistent.txt", "uploads"));
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ReturnsTrue()
    {
        var file = CreateMockFile("test.txt", "Hello World");
        var fileName = await _fileService.UploadFileAsync(file, "uploads");

        var result = await _fileService.DeleteFileAsync(fileName, "uploads");

        Assert.True(result);
        Assert.False(File.Exists(Path.Combine(_testUploadPath, fileName)));
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistentFile_ReturnsFalse()
    {
        var result = await _fileService.DeleteFileAsync("nonexistent.txt", "uploads");
        Assert.False(result);
    }

    [Fact]
    public async Task GetFilesAsync_ExistingDirectory_ReturnsFileList()
    {
        var file1 = CreateMockFile("test1.txt", "Content 1");
        var file2 = CreateMockFile("test2.txt", "Content 2");

        var fileName1 = await _fileService.UploadFileAsync(file1, "uploads");
        var fileName2 = await _fileService.UploadFileAsync(file2, "uploads");

        var result = await _fileService.GetFilesAsync("uploads");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(fileName1, result);
        Assert.Contains(fileName2, result);
    }

    [Fact]
    public async Task GetFilesAsync_NonExistentDirectory_ReturnsEmptyList()
    {
        var result = await _fileService.GetFilesAsync("nonexistent");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IsValidFile_ValidFile_ReturnsTrue()
    {
        var file = CreateMockFile("test.txt", "content", 100);
        var allowedExtensions = new[] { ".txt", ".pdf" };

        var result = _fileService.IsValidFile(file, allowedExtensions);

        Assert.True(result);
    }

    [Fact]
    public void IsValidFile_NullFile_ReturnsFalse()
    {
        var result = _fileService.IsValidFile(null!);
        Assert.False(result);
    }

    [Fact]
    public void IsValidFile_EmptyFile_ReturnsFalse()
    {
        var file = CreateMockFile("test.txt", "", 0);
        var result = _fileService.IsValidFile(file);
        Assert.False(result);
    }

    [Fact]
    public void IsValidFile_FileTooLarge_ReturnsFalse()
    {
        var file = CreateMockFile("test.txt", "content", 20000000); // 20MB
        var result = _fileService.IsValidFile(file);
        Assert.False(result);
    }

    [Fact]
    public void GetFileExtension_ReturnsCorrectExtension()
    {
        var result = _fileService.GetFileExtension("test.TXT");
        Assert.Equal(".txt", result);
    }

    [Fact]
    public void GetContentType_ReturnsCorrectContentType()
    {
        Assert.Equal("text/plain", _fileService.GetContentType("test.txt"));
        Assert.Equal("application/pdf", _fileService.GetContentType("test.pdf"));
        Assert.Equal("application/octet-stream", _fileService.GetContentType("test.unknown"));
    }

    // Exception handling tests
    [Fact]
    public async Task UploadFileAsync_IOException_ThrowsException()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.txt");
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new IOException("IO error"));

        await Assert.ThrowsAsync<IOException>(() =>
            _fileService.UploadFileAsync(mockFile.Object, "uploads"));
    }

    [Fact]
    public async Task GetFilesAsync_PathException_ThrowsException()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(x => x.ContentRootPath).Returns((string)null!);
        var fileService = new FileService(mockEnv.Object, _mockLogger.Object);

        await Assert.ThrowsAnyAsync<Exception>(() => fileService.GetFilesAsync("uploads"));
    }

    private static IFormFile CreateMockFile(string fileName, string content, long? size = null)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(size ?? bytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .Returns((Stream target, CancellationToken token) =>
               {
                   stream.Position = 0;
                   return stream.CopyToAsync(target, token);
               });

        return mockFile.Object;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRootPath))
        {
            try
            {
                Directory.Delete(_testRootPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}