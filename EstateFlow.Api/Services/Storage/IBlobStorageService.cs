namespace EstateFlow.Api.Services.Storage;

public interface IBlobStorageService
{
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType);

    Task<Stream> DownloadAsync(
        string blobReference);

    Task DeleteAsync(
        string blobReference);
}