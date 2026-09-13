using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;

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
            [Table("MenuItems")] TableClient menuTable,
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
                // Filter by PartitionKey (Category)
                var queryResults = menuTable.Query<MenuItem>(
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
