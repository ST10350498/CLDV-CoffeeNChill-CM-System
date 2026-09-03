using asf.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace asg
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("SubmitAssignment")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Assignment received.");

            // Read JSON from Postman
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            var assignment = JsonSerializer.Deserialize<Assignment>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _logger.LogInformation($"BODY = {body}");

            if (assignment != null)
            {
                _logger.LogInformation($"StudentNumber = {assignment.StudentNumber}");
                _logger.LogInformation($"StudentName = {assignment.StudentName}");
                _logger.LogInformation($"ModuleCode = {assignment.ModuleCode}");
                _logger.LogInformation($"AssignmentName = {assignment.AssignmentName}");
            }

            if (assignment == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid JSON.");
                return bad;
            }

            // Azure Table Storage requires these keys
            assignment.PartitionKey = "Assignment";
            assignment.RowKey = Guid.NewGuid().ToString();

           //Connect to Azurite
           string connectionString = "UseDevelopmentStorage=true";

           //Connect to table
           TableClient table =
                new TableClient(connectionString, "AssignmentSubmissions");

            // Create the table if it doesn't exist
            await table.CreateIfNotExistsAsync();

            //Save the assignment
            await table.AddEntityAsync(assignment);

            _logger.LogInformation("Assignment saved to Azure Table Storage with PartitionKey");
            _logger.LogInformation($"StudentNumber: {assignment.StudentNumber}");

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync("Assignment submitted successfully.");

            return response;

        }
    }
}