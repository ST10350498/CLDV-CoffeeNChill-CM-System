using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public class UploadStaffDocument
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

        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                var form = await req.ReadFormAsync();
                var file = form.Files?["file"];
                if (file == null || file.Length == 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("No file uploaded. Use form-data field 'file'.");
                    return response;
                }

                string directory = form["directory"] ?? "Recipes";
                var allowed = new[] { "Recipes", "Manuals", "Policies" };
                if (Array.IndexOf(allowed, directory) < 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync($"Invalid directory. Allowed: {string.Join(", ", allowed)}");
                    return response;
                }

                string contentType = file.Headers.ContentType ?? "application/octet-stream";
                var stream = file.OpenReadStream();

                var service = GetService();
                await service.InitializeAsync();
                var uri = await service.UploadFileAsync(directory, file.FileName, stream, contentType);

                response.StatusCode = HttpStatusCode.Created;
                await response.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully.",
                    fileName = file.FileName,
                    directory = directory,
                    size = file.Length,
                    uri = uri.ToString()
                });
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error uploading file: {ex.Message}");
                return response;
            }
        }
    }
}
