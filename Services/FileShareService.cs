using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Services
{
    /// <summary>
    /// Service for managing staff documents in Azure File Share.
    /// Handles uploading, downloading, and listing operational documents (recipes, manuals, policies).
    /// </summary>
    public class FileShareService
    {
        private readonly string _connectionString;
        private readonly string _shareNameStaffDocs;
        private ShareClient _shareClient;
        private ShareDirectoryClient _rootDirectoryClient;

        public FileShareService(string connectionString, string shareNameStaffDocs = "staff-docs")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _shareNameStaffDocs = shareNameStaffDocs ?? "staff-docs";
        }

        /// <summary>
        /// Initialize the file share and ensure it exists.
        /// Creates the share if it doesn't exist.
        /// </summary>
        public async Task InitializeAsync()
        {
            var fileServiceClient = new ShareServiceClient(_connectionString);
            _shareClient = fileServiceClient.GetShareClient(_shareNameStaffDocs);

            // Create share if it doesn't exist
            await _shareClient.CreateIfNotExistsAsync();
            _rootDirectoryClient = _shareClient.GetRootDirectoryClient();

            Console.WriteLine($"File share '{_shareNameStaffDocs}' initialized successfully.");
        }

        /// <summary>
        /// Upload a document to the staff-docs file share.
        /// </summary>
        /// <param name="fileName">Name of the file to upload</param>
        /// <param name="fileStream">Stream containing file content</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UploadDocumentAsync(string fileName, Stream fileStream)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            try
            {
                var fileClient = _rootDirectoryClient.GetFileClient(fileName);
                fileStream.Position = 0;
                await fileClient.UploadAsync(fileStream, overwrite: true);
                Console.WriteLine($"Document '{fileName}' uploaded successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading document '{fileName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Download a document from the staff-docs file share.
        /// </summary>
        /// <param name="fileName">Name of the file to download</param>
        /// <returns>Stream containing file content, or null if file not found</returns>
        public async Task<Stream> DownloadDocumentAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            try
            {
                var fileClient = _rootDirectoryClient.GetFileClient(fileName);
                var download = await fileClient.DownloadAsync();
                Console.WriteLine($"Document '{fileName}' downloaded successfully.");
                return download.Value.Content;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Document '{fileName}' not found.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading document '{fileName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// List all documents in the staff-docs file share.
        /// </summary>
        /// <returns>List of file names and their properties</returns>
        public async Task<List<DocumentInfo>> ListDocumentsAsync()
        {
            var documents = new List<DocumentInfo>();

            try
            {
                await foreach (var item in _rootDirectoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = _rootDirectoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        documents.Add(new DocumentInfo
                        {
                            FileName = item.Name,
                            Size = properties.Value.ContentLength,
                            LastModified = properties.Value.LastModified,
                            ContentType = properties.Value.ContentType ?? "application/octet-stream"
                        });
                    }
                }

                Console.WriteLine($"Listed {documents.Count} documents from file share.");
                return documents.OrderByDescending(d => d.LastModified).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing documents: {ex.Message}");
                return documents;
            }
        }

        /// <summary>
        /// Delete a document from the staff-docs file share.
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteDocumentAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            try
            {
                var fileClient = _rootDirectoryClient.GetFileClient(fileName);
                await fileClient.DeleteAsync();
                Console.WriteLine($"Document '{fileName}' deleted successfully.");
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Document '{fileName}' not found.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document '{fileName}': {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Represents metadata about a document in the file share.
    /// </summary>
    public class DocumentInfo
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string ContentType { get; set; }
    }
}
