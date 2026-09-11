using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    /// <summary>
    /// Represents a menu item stored in Azure Table Storage.
    /// PartitionKey: Category (e.g., "Hot Drinks", "Cold Drinks", "Pastries", "Sandwiches")
    /// RowKey: Unique SKU/ID (e.g., "COF-001", "PAS-104")
    /// </summary>
    public class MenuItem : ITableEntity
    {
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}