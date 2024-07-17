using API.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary cloudinary;

    public PhotoService(IOptions<CloudinarySettings> options)
    {
        var cloud = options.Value.CloudName;
        var apiKey = options.Value.ApiKey;
        var apiSecret = options.Value.ApiSecrete;

        var account = new Account(cloud, apiKey, apiSecret);

        this.cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length <= 0) return uploadResult;

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation()
                .Height(500)
                .Width(500)
                .Crop("fill")
                .Gravity("face"),
            Folder = "da-net8",
        };

        uploadResult = await this.cloudinary.UploadAsync(uploadParams);

        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        return await this.cloudinary.DestroyAsync(deleteParams);
    }
}
