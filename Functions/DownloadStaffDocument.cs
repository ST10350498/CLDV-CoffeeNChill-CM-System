using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

public static class DownloadStaffDocument
{
    [FunctionName("DownloadStaffDocument")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
        string fileName,
        ILogger log)
    {
        log.LogInformation($"C# HTTP trigger function processed a request to download document: {fileName}");

        if (string.IsNullOrEmpty(fileName))
        {
            return new BadRequestObjectResult("File name parameter is required.");
        }

        try
        {
            string connectionString = Environment.GetEnvironmentVariable("FileStorageConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "UseDevelopmentStorage=true";
            }

            var shareServiceClient = new ShareServiceClient(connectionString);
            var shareClient = shareServiceClient.GetShareClient("staff-docs");

            if (!await shareClient.ExistsAsync())
            {
                return new NotFoundObjectResult($"Staff documents share not found.");
            }

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            if (!await fileClient.ExistsAsync())
            {
                return new NotFoundObjectResult($"File '{fileName}' not found.");
            }

            var response = await fileClient.DownloadAsync();
            var stream = response.Value.Content;

            // Determine content type based on file extension
            string contentType = GetContentType(fileName);

            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            log.LogError($"Error downloading document: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}