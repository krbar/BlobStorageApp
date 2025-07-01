# BlobStorageApp

This simple app demonstrates uploading files to Azure Blob Storage and listing the files in a container.

> Note: The app is intended for demo purposes only and not for production use.

## Configuration

### Environment Variables

To run this app, you need to set the following environment variables in the Azure App Service configuration:

- `AZURE_STORAGE_ACCOUNT_NAME`: The name of the Azure Storage account.
- `AZURE_STORAGE_CONTAINER_NAME`: The name of the blob container where files will be uploaded.
- `ManagedIdentityClientId`: The client ID of the managed identity used to authenticate with Azure

### Permissions

Ensure that the managed identity has the following permissions on the Azure Storage account:

- `Reader` on the Storage Account: This role allows the app to read the storage account properties.
- `Storage Blob Data Contributor` on the Blob Container: This role allows the app to read and upload files to the specified blob container.
