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
    public static class UpdateMenuItem
    {
        [FunctionName("UpdateMenuItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put",
                Route = "menu/{category}/{id}")]
            HttpRequest req,
            string category,
            string id,
            [Table("MenuItems")] TableClient menuTable,
            ILogger log)
        {
            log.LogInformation($"UpdateMenuItem function processed request for: {category}/{id}");

            // Validate parameters
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult("Category and ID parameters are required.");
            }

            try
            {
                // Retrieve existing item
                var existingItem = await menuTable.GetEntityAsync<MenuItem>(category, id);

                if (existingItem == null || existingItem.Value == null)
                {
                    return new NotFoundObjectResult(
                        $"Menu item with category '{category}' and SKU '{id}' not found.");
                }

                // Read update data
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

                var item = existingItem.Value;

                // Update fields if provided
                if (data.price != null)
                {
                    double newPrice = (double)data.price;
                    if (newPrice <= 0)
                    {
                        return new BadRequestObjectResult("Price must be greater than zero.");
                    }
                    item.Price = newPrice;
                }

                if (data.isAvailable != null)
                {
                    item.IsAvailable = (bool)data.isAvailable;
                }

                if (data.name != null)
                {
                    item.Name = (string)data.name;
                }

                if (data.description != null)
                {
                    item.Description = (string)data.description;
                }

                // Update entity
                await menuTable.UpdateEntityAsync(item, item.ETag);

                log.LogInformation($"Menu item '{id}' updated successfully.");

                return new OkObjectResult(new
                {
                    message = "Menu item updated successfully.",
                    item = item
                });
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundObjectResult(
                    $"Menu item with category '{category}' and SKU '{id}' not found.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error updating menu item: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}