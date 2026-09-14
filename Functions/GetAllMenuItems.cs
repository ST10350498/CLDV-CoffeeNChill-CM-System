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
    public static class GetAllMenuItems
    {
        [FunctionName("GetAllMenuItems")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetAllMenuItems function processed a request.");

            try
            {
                // Create TableClient and query all menu items
                var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var serviceClient = new TableServiceClient(conn);
                var tableClient = serviceClient.GetTableClient("MenuItems");
                var queryResults = tableClient.Query<MenuItem>();
                var menuItems = queryResults.ToList();

                log.LogInformation($"Retrieved {menuItems.Count} menu items.");

                return new OkObjectResult(menuItems);
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving menu items: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}