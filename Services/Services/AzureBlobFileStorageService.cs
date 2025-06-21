using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Services.Configurations;
using Services.IServices;

namespace Services.Services;

public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobFileStorageService(IOptions<AzureBlobStorageConfig> config)
        {
            _blobServiceClient = new BlobServiceClient(config.Value.ConnectionString);
            _containerName = config.Value.ContainerName;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName, string resourceType = "image")
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.", nameof(file));
            }

            // Azure blob storage không có folder vật lý, nó dùng tiền tố trong tên blob
            // Ví dụ: folderName/file_guid_name.ext
            string fileName = $"{folderName}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            // Đảm bảo container tồn tại và có public access (chỉ cần chạy một lần)
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl, string resourceType = "image")
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            if (!await containerClient.ExistsAsync())
            {
                Console.WriteLine($"Container '{_containerName}' does not exist, cannot delete blob.");
                return;
            }

            Uri uri = new Uri(fileUrl);
            string blobName = uri.AbsolutePath.Replace($"/{_containerName}/", "");

            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync();
            }
            else
            {
                Console.WriteLine($"Blob '{blobName}' does not exist, cannot delete.");
            }
        }

        public string GetFullFileUrl(string fileUrl)
        {
            return fileUrl;
        }
    }