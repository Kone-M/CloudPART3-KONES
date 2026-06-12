using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VenueBookingSystem.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly string _defaultContainerName = "venueimages";
        private readonly int _maxFileSizeMB = 5;
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public BlobStorageService(ILogger<BlobStorageService> logger, IConfiguration configuration)
        {
            _logger = logger;

            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
                                   ?? configuration.GetConnectionString("AzureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Storage connection string is missing.");
                throw new InvalidOperationException("Azure Storage connection string is missing.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger.LogInformation("Azure Blob Service Client initialized.");
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                containerName = _defaultContainerName;

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);
            return containerClient;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided for upload.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                throw new ArgumentException($"Invalid file type. Only {string.Join(", ", _allowedExtensions)} allowed.");

            if (file.Length > _maxFileSizeMB * 1024 * 1024)
                throw new ArgumentException($"File size cannot exceed {_maxFileSizeMB} MB.");

            var blobName = $"{DateTime.Now:yyyy-MM-dd}_{Guid.NewGuid()}{ext}";
            var containerClient = GetContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType, CacheControl = "public, max-age=31536000" });

            _logger.LogInformation("Uploaded image - {BlobName}", blobName);
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, string containerName)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            try
            {
                var containerClient = GetContainerClient(containerName);
                var blobName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                var blobClient = containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();
                if (response.Value) _logger.LogInformation("Deleted image - {BlobName}", blobName);
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from storage - {Url}", imageUrl);
                return false;
            }
        }

        public async Task<List<string>> ListImagesAsync(string containerName)
        {
            var images = new List<string>();
            var containerClient = GetContainerClient(containerName);
            await foreach (var blobItem in containerClient.GetBlobsAsync())
                images.Add(blobItem.Name);

            return images;
        }

        public string GetBlobUrl(string blobName, string containerName)
        {
            if (string.IsNullOrEmpty(blobName)) return string.Empty;
            var containerClient = GetContainerClient(containerName);
            return containerClient.GetBlobClient(blobName).Uri.ToString();
        }
    }
}