using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public static class CreateMenuItem
    {
        [FunctionName("CreateMenuItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")]
            HttpRequest req,
            [Table("MenuItems")] IAsyncCollector<MenuItem> menuTable,
            ILogger log)
        {
            log.LogInformation("CreateMenuItem function processed a request.");

            try
            {
                // Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrEmpty(requestBody))
                {
                    return new BadRequestObjectResult("Request body cannot be empty.");
                }

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if (data == null)
                {
                    return new BadRequestObjectResult("Invalid JSON format.");
                }

                // Extract fields
                string category = data.category;
                string sku = data.sku;
                string name = data.name;
                string description = data.description;
                double price = data.price;
                bool isAvailable = data.isAvailable ?? true;

                // Validate required fields
                if (string.IsNullOrEmpty(category))
                {
                    return new BadRequestObjectResult("Category is required.");
                }

                if (string.IsNullOrEmpty(sku))
                {
                    return new BadRequestObjectResult("SKU is required.");
                }

                if (string.IsNullOrEmpty(name))
                {
                    return new BadRequestObjectResult("Name is required.");
                }

                if (price <= 0)
                {
                    return new BadRequestObjectResult("Price must be greater than zero.");
                }

                // Create menu item
                var menuItem = new MenuItem(category, sku, name, description, price, isAvailable);

                // Insert into Azure Table Storage
                await menuTable.AddAsync(menuItem);

                log.LogInformation($"Menu item '{name}' created successfully with SKU '{sku}'.");

                return new OkObjectResult(new
                {
                    message = "Menu item created successfully.",
                    item = menuItem
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error creating menu item: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}