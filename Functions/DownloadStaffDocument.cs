using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public class DownloadStaffDocument
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

        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            var response = req.CreateResponse();
            try
            {
                var service = GetService();
                await service.InitializeAsync();

                string[] directories = { "Recipes", "Manuals", "Policies" };
                Stream? stream = null;

                foreach (var dir in directories)
                {
                    try
                    {
                        stream = await service.DownloadFileAsync(dir, fileName);
                        break;
                    }
                    catch (FileNotFoundException) { }
                }

                if (stream == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"File '{fileName}' not found.");
                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;
                response.Headers.Add("Content-Type", "application/octet-stream");
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                await stream.CopyToAsync(response.Body);
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }
    }
}
