using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeNChill.Functions.Functions
{
    public static class GetAllMenuItems
    {
        [FunctionName("GetAllMenuItems")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")]
            HttpRequest req,
            [Table("MenuItems")] TableClient menuTable,
            ILogger log)
        {
            log.LogInformation("GetAllMenuItems function processed a request.");

            try
            {
                // Query all menu items
                var queryResults = menuTable.Query<MenuItem>(filter: "");
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