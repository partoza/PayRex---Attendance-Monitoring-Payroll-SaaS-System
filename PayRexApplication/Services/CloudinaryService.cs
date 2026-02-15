using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PayRexApplication.Services
{
 /// <summary>
    /// Service interface for Cloudinary image uploads
    /// </summary>
  public interface ICloudinaryService
  {
        /// <summary>
        /// Upload a profile image to Cloudinary
        /// </summary>
        /// <param name="imageStream">Image data stream</param>
    /// <param name="fileName">Original file name</param>
        /// <param name="userId">User ID for folder organization</param>
 /// <returns>Secure URL of uploaded image or null if failed</returns>
    Task<string?> UploadProfileImageAsync(Stream imageStream, string fileName, string userId);

        /// <summary>
        /// Delete an image from Cloudinary by public ID
     /// </summary>
        /// <param name="publicId">Cloudinary public ID</param>
        /// <returns>True if deleted successfully</returns>
  Task<bool> DeleteImageAsync(string publicId);

        /// <summary>
        /// Extract public ID from Cloudinary URL
        /// </summary>
        /// <param name="imageUrl">Full Cloudinary URL</param>
        /// <returns>Public ID or null</returns>
   string? ExtractPublicIdFromUrl(string? imageUrl);
    }

    /// <summary>
    /// Cloudinary service implementation for image management
    /// </summary>
  public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly bool _isConfigured;
        private const string ProfileImagesFolder = "payrex/profiles";
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
     private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
      {
      _logger = logger;

         var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

 if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
     {
       _logger.LogWarning("Cloudinary is not configured. Image upload will be disabled.");
  _isConfigured = false;
         _cloudinary = null!;
    return;
        }

            var account = new Account(cloudName, apiKey, apiSecret);
 _cloudinary = new Cloudinary(account)
         {
        Api = { Secure = true } // Always use HTTPS
            };
            _isConfigured = true;

            _logger.LogInformation("Cloudinary service initialized for cloud: {CloudName}", cloudName);
        }

        public async Task<string?> UploadProfileImageAsync(Stream imageStream, string fileName, string userId)
        {
    if (!_isConfigured)
        {
        _logger.LogWarning("Cloudinary is not configured. Cannot upload image.");
                return null;
     }

        // Validate file extension
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
      if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
    _logger.LogWarning("Invalid file extension: {Extension}. Allowed: {Allowed}",
           extension, string.Join(", ", AllowedExtensions));
 return null;
            }

            // Validate file size
     if (imageStream.Length > MaxFileSizeBytes)
  {
                _logger.LogWarning("File size {Size} exceeds maximum allowed {Max}",
  imageStream.Length, MaxFileSizeBytes);
                return null;
            }

            try
          {
      var publicId = $"{ProfileImagesFolder}/user_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

    var uploadParams = new ImageUploadParams
            {
  File = new FileDescription(fileName, imageStream),
     PublicId = publicId,
     Overwrite = true,
Transformation = new Transformation()
  .Width(400)
                .Height(400)
       .Crop("fill")
         .Gravity("face") // Auto-detect face for cropping
          .Quality("auto")
  .FetchFormat("auto"), // Auto-select best format
  Tags = "profile,avatar"
      };

           var uploadResult = await _cloudinary.UploadAsync(uploadParams);

           if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
         {
       _logger.LogInformation("Profile image uploaded successfully for user {UserId}: {Url}",
            userId, uploadResult.SecureUrl);
            return uploadResult.SecureUrl?.ToString();
           }

      _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
           return null;
            }
    catch (Exception ex)
      {
   _logger.LogError(ex, "Error uploading image to Cloudinary for user {UserId}", userId);
         return null;
         }
        }

     public async Task<bool> DeleteImageAsync(string publicId)
  {
     if (!_isConfigured || string.IsNullOrEmpty(publicId))
          return false;

            try
        {
 var deleteParams = new DeletionParams(publicId);
    var result = await _cloudinary.DestroyAsync(deleteParams);

     if (result.Result == "ok")
      {
     _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
          return true;
      }

     _logger.LogWarning("Failed to delete image: {PublicId}, Result: {Result}",
                publicId, result.Result);
      return false;
            }
          catch (Exception ex)
          {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
              return false;
      }
  }

        public string? ExtractPublicIdFromUrl(string? imageUrl)
        {
       if (string.IsNullOrEmpty(imageUrl))
           return null;

     try
            {
         // URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{publicId}.{ext}
         var uri = new Uri(imageUrl);
        var path = uri.AbsolutePath;

  // Find the position after /upload/ or /upload/v{version}/
                var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
                if (uploadIndex == -1)
           return null;

    var afterUpload = path.Substring(uploadIndex + 8); // 8 = "/upload/".Length

  // Remove version prefix if present (v12345678/)
 if (afterUpload.StartsWith("v") && afterUpload.Contains('/'))
          {
            afterUpload = afterUpload.Substring(afterUpload.IndexOf('/') + 1);
    }

     // Remove file extension
                var lastDot = afterUpload.LastIndexOf('.');
  if (lastDot > 0)
          {
            afterUpload = afterUpload.Substring(0, lastDot);
       }

         return afterUpload;
            }
            catch (Exception ex)
      {
         _logger.LogWarning(ex, "Failed to extract public ID from URL: {Url}", imageUrl);
    return null;
            }
        }
    }
}
