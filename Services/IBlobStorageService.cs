using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VenueBookingSystem.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<bool> DeleteImageAsync(string imageUrl, string containerName);
        Task<List<string>> ListImagesAsync(string containerName);
        string GetBlobUrl(string blobName, string containerName);
    }
}