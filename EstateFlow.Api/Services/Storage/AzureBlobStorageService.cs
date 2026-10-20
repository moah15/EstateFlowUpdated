using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EstateFlow.Api.Services.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly string? _connectionString;
    private readonly string _containerName;

    public AzureBlobStorageService(
        IConfiguration configuration)
    {
        // Supports all of these:
        // appsettings.json:
        //   "AzureStorage": {
        //       "ConnectionString": "...",
        //       "ContainerName": "estate-documents"
        //   }
        //
        // Azure App Service environment variables:
        //   AzureStorage__ConnectionString
        //   AzureStorage__ContainerName
        //
        // Optional fallback:
        //   ConnectionStrings:AzureStorage
        _connectionString =
            configuration["AzureStorage:ConnectionString"]
            ?? configuration.GetConnectionString("AzureStorage");

        _containerName =
            configuration["AzureStorage:ContainerName"]
            ?? "estate-documents";
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType)
    {
        var containerClient =
            GetContainerClient();

        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

        var blobClient =
            containerClient.GetBlobClient(fileName);

        var options =
            new BlobUploadOptions
            {
                HttpHeaders =
                    new BlobHttpHeaders
                    {
                        ContentType =
                            string.IsNullOrWhiteSpace(contentType)
                                ? "application/octet-stream"
                                : contentType
                    }
            };

        try
        {
            await blobClient.UploadAsync(
                stream,
                options);

            return fileName;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                BuildAzureErrorMessage(
                    "upload",
                    ex),
                ex);
        }
    }

    public async Task<Stream> DownloadAsync(
        string blobReference)
    {
        if (string.IsNullOrWhiteSpace(blobReference))
        {
            throw new FileNotFoundException(
                "This document has no Azure Blob reference stored in the database.");
        }

        var containerClient =
            GetContainerClient();

        var blobClient =
            containerClient.GetBlobClient(
                blobReference);

        try
        {
            var exists =
                await blobClient.ExistsAsync();

            if (!exists.Value)
            {
                throw new FileNotFoundException(
                    $"The file '{blobReference}' was not found in Azure Blob Storage.");
            }

            var response =
                await blobClient.DownloadStreamingAsync();

            var memoryStream =
                new MemoryStream();

            await response.Value.Content.CopyToAsync(
                memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                BuildAzureErrorMessage(
                    "download",
                    ex),
                ex);
        }
    }

    public async Task DeleteAsync(
        string blobReference)
    {
        if (string.IsNullOrWhiteSpace(blobReference))
            return;

        var containerClient =
            GetContainerClient();

        var blobClient =
            containerClient.GetBlobClient(
                blobReference);

        try
        {
            await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                BuildAzureErrorMessage(
                    "delete",
                    ex),
                ex);
        }
    }

    private BlobContainerClient GetContainerClient()
    {
        if (string.IsNullOrWhiteSpace(
                _connectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured on the API. " +
                "Set the Azure App Service environment variable " +
                "'AzureStorage__ConnectionString' and restart the API.");
        }

        try
        {
            var serviceClient =
                new BlobServiceClient(
                    _connectionString);

            return serviceClient
                .GetBlobContainerClient(
                    _containerName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "The Azure Blob Storage connection string is invalid. " +
                "Check 'AzureStorage__ConnectionString' in the API's Azure App Service settings.",
                ex);
        }
    }

    private static string BuildAzureErrorMessage(
        string operation,
        RequestFailedException ex)
    {
        var detail =
            string.IsNullOrWhiteSpace(ex.ErrorCode)
                ? ex.Message
                : $"{ex.ErrorCode}: {ex.Message}";

        return
            $"Azure Blob Storage could not {operation} the document. " +
            $"Azure returned HTTP {ex.Status}. {detail}";
    }
}
