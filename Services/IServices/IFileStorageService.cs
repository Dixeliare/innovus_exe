
using Microsoft.AspNetCore.Http;

namespace Services.IServices;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName, string resourceType = "image");
    Task DeleteFileAsync(string fileUrl, string resourceType = "image");
    string GetFullFileUrl(string fileUrl);
    
    // Download methods
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task<byte[]> DownloadFileAsBytesAsync(string fileUrl);
    Task<string> GetDownloadUrlAsync(string fileUrl, TimeSpan? expiry = null);
}