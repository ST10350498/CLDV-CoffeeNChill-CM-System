using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace CoffeeNChill.Functions.Services
{
    public class FileShareService
    {
        private readonly ShareClient _shareClient;

        public FileShareService(string connectionString, string shareName = "staff-docs")
        {
            var shareServiceClient = new ShareServiceClient(connectionString);
            _shareClient = shareServiceClient.GetShareClient(shareName);
            _shareClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<List<ShareFileItem>> ListFilesAsync()
        {
            var files = new List<ShareFileItem>();
            var directoryClient = _shareClient.GetRootDirectoryClient();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                var fileClient = directoryClient.GetFileClient(item.Name);
                var properties = await fileClient.GetPropertiesAsync();

                files.Add(new ShareFileItem
                {
                    Name = item.Name,
                    Size = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified
                });
            }
            return files;
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            var response = await fileClient.DownloadAsync();
            return response.Value.Content;
        }
    }

    public class ShareFileItem
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}