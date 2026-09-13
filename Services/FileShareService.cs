using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace CoffeeNChill.Functions.Services
{
    public class FileShareService
    {
        private readonly ShareClient _shareClient;
        private readonly string _shareName;

        public FileShareService(string connectionString, string shareName = "staff-docs")
        {
            _shareName = shareName;
            _shareClient = new ShareClient(connectionString, shareName);
        }

        public async Task InitializeAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();

            var directories = new[] { "Recipes", "Manuals", "Policies" };
            foreach (var dir in directories)
            {
                var dirClient = _shareClient.GetDirectoryClient(dir);
                await dirClient.CreateIfNotExistsAsync();
            }
        }

        public async Task<Uri> UploadFileAsync(string directoryName, string fileName, Stream fileStream, string contentType)
        {
            var directory = _shareClient.GetDirectoryClient(directoryName);
            await directory.CreateIfNotExistsAsync();

            var file = directory.GetFileClient(fileName);
            await file.CreateAsync(fileStream.Length, new ShareFileHttpHeaders
            {
                ContentType = contentType
            });

            fileStream.Position = 0;
            await file.UploadAsync(fileStream);
            return file.Uri;
        }

        public async Task<IEnumerable<FileMetadata>> ListFilesAsync(string directoryName)
        {
            var files = new List<FileMetadata>();
            var directory = _shareClient.GetDirectoryClient(directoryName);

            if (!await directory.ExistsAsync())
                return files;

            await foreach (var item in directory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    var fileClient = directory.GetFileClient(item.Name);
                    var props = await fileClient.GetPropertiesAsync();
                    files.Add(new FileMetadata
                    {
                        Name = item.Name,
                        Size = props.Value.ContentLength,
                        LastModified = props.Value.LastModified.DateTime,
                        ContentType = props.Value.ContentType
                    });
                }
            }
            return files;
        }

        public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        {
            var directory = _shareClient.GetDirectoryClient(directoryName);
            var file = directory.GetFileClient(fileName);

            if (!await file.ExistsAsync())
                throw new FileNotFoundException($"File '{fileName}' not found in '{directoryName}'.");

            var download = await file.DownloadAsync();
            return download.Value.Content;
        }
    }

    public class FileMetadata
    {
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string ContentType { get; set; } = "";
    }
}