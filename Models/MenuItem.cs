using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    public class MenuItem : ITableEntity
    {
        // PartitionKey = Category (e.g., "Hot Drinks", "Cold Drinks")
        public string PartitionKey { get; set; }

        // RowKey = Unique Item SKU/ID (e.g., "COF-001", "PAS-104")
        public string RowKey { get; set; }

        // Item name (e.g., "Espresso", "Ham & Cheese Croissant")
        public string Name { get; set; }

        // Short menu description
        public string Description { get; set; }

        // Item price
        public double Price { get; set; }

        // Availability status
        public bool IsAvailable { get; set; }

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public MenuItem()
        {
            ETag = ETag.All;
        }

        public MenuItem(string category, string sku, string name,
                       string description, double price, bool isAvailable)
        {
            PartitionKey = category;
            RowKey = sku;
            Name = name;
            Description = description;
            Price = price;
            IsAvailable = isAvailable;
            ETag = ETag.All;
        }
    }
}