using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace CoffeeNChill.Functions.Functions
{
    public static class GetMenuItemsByCategory
    {
        [FunctionName("GetMenuItemsByCategory")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "menu/category/{category}")]
            HttpRequest req,
            string category,
            ILogger log)
        {
            log.LogInformation($"GetMenuItemsByCategory function processed request for category: {category}");

            // Validate category parameter
            if (string.IsNullOrEmpty(category))
            {
                return new BadRequestObjectResult("Category parameter is required.");
            }

            try
            {
                // Create TableClient and filter by PartitionKey (Category)
                var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var serviceClient = new TableServiceClient(conn);
                var tableClient = serviceClient.GetTableClient("MenuItems");
                var queryResults = tableClient.Query<MenuItem>(
                    filter: $"PartitionKey eq '{category}'");

                var menuItems = queryResults.ToList();

                log.LogInformation($"Retrieved {menuItems.Count} items for category '{category}'.");

                return new OkObjectResult(menuItems);
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving menu items by category: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
