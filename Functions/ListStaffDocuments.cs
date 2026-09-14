using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public static class ListStaffDocuments
    {
        [FunctionName("ListStaffDocuments")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ListStaffDocuments function processed a request to list staff documents.");

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
                    return new OkObjectResult(new List<object>()); // empty list if share doesn't exist yet
                }

                var directoryClient = shareClient.GetRootDirectoryClient();
                var results = new List<object>();

                await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var props = await fileClient.GetPropertiesAsync();
                        results.Add(new
                        {
                            fileName = item.Name,
                            size = props.Value.ContentLength,
                            lastModified = props.Value.LastModified.UtcDateTime,
                            contentType = props.Value.ContentType
                        });
                    }
                }

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                log.LogError($"Error listing documents: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
