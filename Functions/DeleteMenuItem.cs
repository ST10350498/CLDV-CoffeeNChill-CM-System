using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;

namespace CoffeeNChill.Functions.Functions
{
    public static class DeleteMenuItem
    {
        [FunctionName("DeleteMenuItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "menu/{category}/{id}")]
            HttpRequest req,
            string category,
            string id,
            ILogger log)
        {
            log.LogInformation($"DeleteMenuItem function processed request for: {category}/{id}");

            // Validate parameters
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult("Category and ID parameters are required.");
            }

            try
            {
                // Create TableClient and check if item exists
                var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var serviceClient = new TableServiceClient(conn);
                var menuTable = serviceClient.GetTableClient("MenuItems");
                var existingItem = await menuTable.GetEntityAsync<MenuItem>(category, id);

                if (existingItem == null || existingItem.Value == null)
                {
                    return new NotFoundObjectResult(
                        $"Menu item with category '{category}' and SKU '{id}' not found.");
                }

                // Delete entity
                await menuTable.DeleteEntityAsync(category, id, existingItem.Value.ETag);

                log.LogInformation($"Menu item '{id}' deleted successfully.");

                return new OkObjectResult(new
                {
                    message = $"Menu item '{id}' deleted successfully."
                });
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundObjectResult(
                    $"Menu item with category '{category}' and SKU '{id}' not found.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error deleting menu item: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}