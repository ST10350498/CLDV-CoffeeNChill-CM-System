using Azure;
using Azure.Data.Tables;

namespace asf.Models
{
    public class Assignment : ITableEntity
    {
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";

        public string StudentNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string ModuleCode { get; set; } = "";
        public string AssignmentName { get; set; } = "";


        public ETag ETag { get; set; } 
        public DateTimeOffset? Timestamp { get; set; } 
    }
}