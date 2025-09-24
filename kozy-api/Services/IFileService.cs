namespace kozy_api.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folderPath = "uploads");
    Task<byte[]> DownloadFileAsync(string fileName, string folderPath = "uploads");
    Task<bool> DeleteFileAsync(string fileName, string folderPath = "uploads");
    Task<List<string>> GetFilesAsync(string folderPath = "uploads");
    bool IsValidFile(IFormFile file, string[] allowedExtensions = null!, long maxSizeBytes = 10485760); // 10MB default
    string GetFileExtension(string fileName);
    string GetContentType(string fileName);
}