namespace kozy_api.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderPath = "uploads")
    {
        try
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            // Create upload directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.ContentRootPath, "wwwroot", folderPath);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            throw;
        }
    }

    public async Task<byte[]> DownloadFileAsync(string fileName, string folderPath = "uploads")
    {
        try
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", folderPath, fileName);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {fileName}");

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            _logger.LogInformation("File downloaded successfully: {FileName}", fileName);
            return fileBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, string folderPath = "uploads")
    {
        try
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", folderPath, fileName);
            
            if (!File.Exists(filePath))
                return false;

            await Task.Run(() => File.Delete(filePath));
            _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<string>> GetFilesAsync(string folderPath = "uploads")
    {
        try
        {
            var uploadPath = Path.Combine(_environment.ContentRootPath, "wwwroot", folderPath);
            
            if (!Directory.Exists(uploadPath))
                return new List<string>();

            var files = await Task.Run(() => 
                Directory.GetFiles(uploadPath)
                         .Select(Path.GetFileName)
                         .Where(f => f != null)
                         .Cast<string>()
                         .ToList());

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files from folder: {FolderPath}", folderPath);
            throw;
        }
    }

    public bool IsValidFile(IFormFile file, string[] allowedExtensions = null!, long maxSizeBytes = 10485760)
    {
        if (file == null || file.Length == 0)
            return false;

        // Check file size
        if (file.Length > maxSizeBytes)
            return false;

        // Check file extension if specified
        if (allowedExtensions != null && allowedExtensions.Length > 0)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return false;
        }

        return true;
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    public string GetContentType(string fileName)
    {
        var extension = GetFileExtension(fileName);
        
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
}