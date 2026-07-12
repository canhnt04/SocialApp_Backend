public interface IFileStorageService
{
    Task<(string Url, string PublicId, long FileSize)> UploadAsync(Stream fileStream, string fileName, string mediaType);
}