using asf.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace asg
{
    public class GetAssignments
    {
        [Function("GetAssignments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            string connectionString = "UseDevelopmentStorage=true";

            TableClient table =
                new TableClient(connectionString, "AssignmentSubmissions");

            var assignments = table.Query<Assignment>().ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(assignments);

            return response;
        }
    }
}