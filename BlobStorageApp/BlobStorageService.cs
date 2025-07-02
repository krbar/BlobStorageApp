using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(string storageAccountName)
    {
        var clientId = Environment.GetEnvironmentVariable("ManagedIdentityClientId")
            ?? throw new InvalidOperationException("Configuration value 'ManagedIdentityClientId' is missing.");
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = clientId
        });
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
    public async Task UploadFileAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        
        // For files larger than 100MB, use parallel uploads for better performance
        if (stream.Length >= 100 * 1024 * 1024) // 100 MB
        {
            var uploadOptions = new BlobUploadOptions
            {
                TransferOptions = new Azure.Storage.StorageTransferOptions
                {
                    // Use parallel uploads for large files
                    MaximumConcurrency = Environment.ProcessorCount,
                    InitialTransferSize = 4 * 1024 * 1024, // 4 MB initial transfer size
                    MaximumTransferSize = 4 * 1024 * 1024  // 4 MB maximum transfer size per operation
                }
            };
            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
        }
        else
        {
            // Use simple upload for smaller files
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);
        }
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