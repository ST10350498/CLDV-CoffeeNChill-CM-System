using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Azure.Data.Tables;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                              ?? "UseDevelopmentStorage=true";

string fileShareConnString;
if (storageConnectionString == "UseDevelopmentStorage=true")
{
    fileShareConnString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;FileEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
    Console.WriteLine("Using Azurite emulator for file storage.");
}
else
{
    fileShareConnString = storageConnectionString;
}

// Ensure functions can use the file storage connection string via FileStorageConnection env var
Environment.SetEnvironmentVariable("FileStorageConnection", fileShareConnString);

var fileService = new FileShareService(fileShareConnString, "staff-docs");
await fileService.InitializeAsync();
Console.WriteLine("staff-docs file share initialized (Recipes, Manuals, Policies).");

// Best-effort: ensure MenuItems table exists
try
{
    var tableConn = Environment.GetEnvironmentVariable("TableStorageConnection") ?? storageConnectionString;
    var tableService = new TableServiceClient(tableConn);
    var tableClient = tableService.GetTableClient("MenuItems");
    await tableClient.CreateIfNotExistsAsync();
    Console.WriteLine("MenuItems table ensured in storage.");
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: failed to ensure MenuItems table exists: {ex.Message}");
}

builder.Build().Run();
