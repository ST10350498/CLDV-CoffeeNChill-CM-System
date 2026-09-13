using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class ListStaffDocuments
{
    [FunctionName("ListStaffDocuments")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request to list staff documents.");

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
                return new OkObjectResult(new List<object>());
            }

            var directoryClient = shareClient.GetRootDirectoryClient();
            var files = new List<object>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                var fileClient = directoryClient.GetFileClient(item.Name);
                var properties = await fileClient.GetPropertiesAsync();

                files.Add(new
                {
                    fileName = item.Name,
                    size = properties.Value.ContentLength,
                    lastModified = properties.Value.LastModified
                });
            }

            return new OkObjectResult(files);
        }
        catch (Exception ex)
        {
            log.LogError($"Error listing documents: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }
}