
using Microsoft.AspNetCore.Http;

namespace Services.IServices;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName, string resourceType = "image");
    Task DeleteFileAsync(string fileUrl, string resourceType = "image");
    string GetFullFileUrl(string fileUrl);
}