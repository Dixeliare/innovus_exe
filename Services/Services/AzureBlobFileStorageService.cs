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
        private readonly string _connectionString;

        public AzureBlobFileStorageService(IOptions<AzureBlobStorageConfig> config)
        {
            _connectionString = config.Value.ConnectionString;
            _blobServiceClient = new BlobServiceClient(_connectionString);
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

        // Download file as Stream
        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("File URL is empty or null.", nameof(fileUrl));
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            if (!await containerClient.ExistsAsync())
            {
                throw new InvalidOperationException($"Container '{_containerName}' does not exist.");
            }

            Uri uri = new Uri(fileUrl);
            string blobName = uri.AbsolutePath.Replace($"/{_containerName}/", "");

            var blobClient = containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File '{blobName}' does not exist.");
            }

            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        // Download file as byte array
        public async Task<byte[]> DownloadFileAsBytesAsync(string fileUrl)
        {
            using var stream = await DownloadFileAsync(fileUrl);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        // Generate download URL with optional expiry
        public async Task<string> GetDownloadUrlAsync(string fileUrl, TimeSpan? expiry = null)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("File URL is empty or null.", nameof(fileUrl));
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            if (!await containerClient.ExistsAsync())
            {
                throw new InvalidOperationException($"Container '{_containerName}' does not exist.");
            }

            Uri uri = new Uri(fileUrl);
            string blobName = uri.AbsolutePath.Replace($"/{_containerName}/", "");

            var blobClient = containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File '{blobName}' does not exist.");
            }

            // Generate SAS token for download
            var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b", // blob
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)), // Default 1 hour
            };

            sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(
                new Azure.Storage.StorageSharedKeyCredential(
                    _blobServiceClient.AccountName,
                    ExtractAccountKey(_connectionString)
                )
            ).ToString();

            return $"{fileUrl}?{sasToken}";
        }

        // Helper method to extract account key from connection string
        private string ExtractAccountKey(string connectionString)
        {
            var parts = connectionString.Split(';');
            var accountKeyPart = parts.FirstOrDefault(p => p.StartsWith("AccountKey="));
            return accountKeyPart?.Replace("AccountKey=", "") ?? "";
        }
    }