using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public static class UploadStaffDocument
    {
        [FunctionName("UploadStaffDocument")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to upload a staff document.");

            try
            {
                var formFile = req.Form.Files["file"];

                if (formFile == null || formFile.Length == 0)
                {
                    return new BadRequestObjectResult("No file uploaded. Please provide a file in the 'file' field.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var fileExtension = Path.GetExtension(formFile.FileName).ToLower();

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    return new BadRequestObjectResult($"File type '{fileExtension}' not allowed. Allowed types: PDF, DOC, DOCX, TXT.");
                }

                // Get connection string from environment
                string connectionString = Environment.GetEnvironmentVariable("FileStorageConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "UseDevelopmentStorage=true";
                }

                var shareServiceClient = new ShareServiceClient(connectionString);
                var shareClient = shareServiceClient.GetShareClient("staff-docs");
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(formFile.FileName);

                // Upload the file
                using (var stream = formFile.OpenReadStream())
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadAsync(stream);
                }

                return new OkObjectResult(new
                {
                    message = "File uploaded successfully.",
                    fileName = formFile.FileName,
                    fileSize = formFile.Length
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error uploading document: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
