// Infrastructure/Services/CloudinaryStorageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using SocialApp.PostService.Domain.Repositories;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IConfiguration configuration)
    {
        var config = configuration.GetSection("Cloudinary");
        var account = new Account(config["CloudName"], config["ApiKey"], config["ApiSecret"]);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string Url, string PublicId, long FileSize)> UploadAsync(Stream fileStream, string fileName, string mediaType)
    {
        var fileSize = fileStream.Length;
        RawUploadResult uploadResult;

        if (mediaType.ToLower().Contains("video"))
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "social_app/videos"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        else // Mặc định là ảnh
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "social_app/images",
                Transformation = new Transformation().Quality("auto").FetchFormat("auto") // Tự động tối ưu dung lượng ảnh
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId, fileSize);
    }
}