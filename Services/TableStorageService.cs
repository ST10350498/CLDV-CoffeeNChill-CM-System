using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Services
{
    /// <summary>
    /// Service for managing menu items in Azure Table Storage.
    /// Provides CRUD operations for menu items organized by category.
    /// </summary>
    public class TableStorageService
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private TableClient _tableClient;

        public TableStorageService(string connectionString, string tableName = "MenuItems")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? "MenuItems";
        }

        /// <summary>
        /// Initialize the table storage and ensure the table exists.
        /// </summary>
        public async Task InitializeAsync()
        {
            var tableServiceClient = new TableServiceClient(_connectionString);
            _tableClient = tableServiceClient.GetTableClient(_tableName);
            await _tableClient.CreateIfNotExistsAsync();
            Console.WriteLine($"Table '{_tableName}' initialized successfully.");
        }

        /// <summary>
        /// Get all menu items from the table.
        /// </summary>
        /// <returns>List of all menu items</returns>
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var items = new List<MenuItem>();

            try
            {
                await foreach (var entity in _tableClient.QueryAsync<MenuItem>())
                {
                    items.Add(entity);
                }

                Console.WriteLine($"Retrieved {items.Count} menu items.");
                return items.OrderBy(m => m.PartitionKey).ThenBy(m => m.RowKey).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all menu items: {ex.Message}");
                return items;
            }
        }

        /// <summary>
        /// Get menu items filtered by category (PartitionKey).
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>List of menu items in the specified category</returns>
        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));

            var items = new List<MenuItem>();

            try
            {
                var filter = TableClient.CreateQueryFilter($"PartitionKey eq {category}");

                await foreach (var entity in _tableClient.QueryAsync<MenuItem>(filter))
                {
                    items.Add(entity);
                }

                Console.WriteLine($"Retrieved {items.Count} menu items for category '{category}'.");
                return items.OrderBy(m => m.RowKey).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving menu items for category '{category}': {ex.Message}");
                return items;
            }
        }

        /// <summary>
        /// Get a specific menu item by category and SKU.
        /// </summary>
        /// <param name="category">Category (PartitionKey)</param>
        /// <param name="sku">SKU/ID (RowKey)</param>
        /// <returns>Menu item if found, null otherwise</returns>
        public async Task<MenuItem> GetMenuItemAsync(string category, string sku)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU cannot be empty.", nameof(sku));

            try
            {
                var response = await _tableClient.GetEntityAsync<MenuItem>(category, sku);
                Console.WriteLine($"Retrieved menu item: {category}/{sku}");
                return response.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Menu item not found: {category}/{sku}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving menu item {category}/{sku}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a new menu item.
        /// </summary>
        /// <param name="category">Category (PartitionKey)</param>
        /// <param name="sku">SKU/ID (RowKey)</param>
        /// <param name="name">Item name</param>
        /// <param name="description">Item description</param>
        /// <param name="price">Item price</param>
        /// <param name="isAvailable">Availability status</param>
        /// <returns>Created menu item</returns>
        public async Task<MenuItem> CreateMenuItemAsync(string category, string sku, string name,
            string description, double price, bool isAvailable = true)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU cannot be empty.", nameof(sku));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            var item = new MenuItem(category, sku, name, description, price, isAvailable);

            try
            {
                await _tableClient.AddEntityAsync(item);
                Console.WriteLine($"Created menu item: {category}/{sku} - {name}");
                return item;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 409)
            {
                throw new InvalidOperationException($"Menu item already exists: {category}/{sku}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating menu item: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing menu item.
        /// </summary>
        /// <param name="item">Menu item to update</param>
        /// <param name="updateMode">Update mode (Replace or Merge)</param>
        /// <returns>Updated menu item</returns>
        public async Task<MenuItem> UpdateMenuItemAsync(MenuItem item, TableUpdateMode updateMode = TableUpdateMode.Replace)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.PartitionKey))
                throw new ArgumentException("PartitionKey (category) cannot be empty.", nameof(item));
            if (string.IsNullOrWhiteSpace(item.RowKey))
                throw new ArgumentException("RowKey (SKU) cannot be empty.", nameof(item));

            try
            {
                item.Timestamp = DateTimeOffset.UtcNow;
                await _tableClient.UpdateEntityAsync(item, ETag.All, updateMode);
                Console.WriteLine($"Updated menu item: {item.PartitionKey}/{item.RowKey}");
                return item;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                throw new InvalidOperationException($"Menu item not found: {item.PartitionKey}/{item.RowKey}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating menu item: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a menu item.
        /// </summary>
        /// <param name="category">Category (PartitionKey)</param>
        /// <param name="sku">SKU/ID (RowKey)</param>
        /// <returns>True if successful, false if not found</returns>
        public async Task<bool> DeleteMenuItemAsync(string category, string sku)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU cannot be empty.", nameof(sku));

            try
            {
                await _tableClient.DeleteEntityAsync(category, sku, ETag.All);
                Console.WriteLine($"Deleted menu item: {category}/{sku}");
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Menu item not found: {category}/{sku}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting menu item: {ex.Message}");
                throw;
            }
        }
    }
}
