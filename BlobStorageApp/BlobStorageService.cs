using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(string storageAccountName)
    {
        var credential = new DefaultAzureCredential();
        _blobServiceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), credential);
    }

    public async Task<List<BlobInfo>> ListBlobsAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobInfos = new List<BlobInfo>();

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var properties = await blobClient.GetPropertiesAsync();

            var blobInfo = new BlobInfo
            {
                Name = blobItem.Name,
                Size = properties.Value.ContentLength,
                LastModified = properties.Value.LastModified
            };

            blobInfos.Add(blobInfo);
        }

        return blobInfos;
    }
    public async Task UploadFileAsync(string containerName, string blobName, Stream stream)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, true);
    }

    public async Task<bool> BlobExists(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
    }
}

public class BlobInfo
{
    public string? Name { get; set; }
    public long Size { get; set; }
    public DateTimeOffset LastModified { get; set; }
}