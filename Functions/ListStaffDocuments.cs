using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public class ListStaffDocuments
    {
        private static FileShareService GetService()
        {
            var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (conn == "UseDevelopmentStorage=true" || string.IsNullOrEmpty(conn))
            {
                conn = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;FileEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            }
            return new FileShareService(conn, "staff-docs");
        }

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                var service = GetService();
                await service.InitializeAsync();

                var recipes = await service.ListFilesAsync("Recipes");
                var manuals = await service.ListFilesAsync("Manuals");
                var policies = await service.ListFilesAsync("Policies");

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(new
                {
                    Recipes = recipes,
                    Manuals = manuals,
                    Policies = policies
                });
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error listing files: {ex.Message}");
                return response;
            }
        }
    }
}
