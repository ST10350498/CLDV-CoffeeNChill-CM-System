using Azure.Monitor.OpenTelemetry.Exporter;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using System;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

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

var fileService = new FileShareService(fileShareConnString, "staff-docs");
await fileService.InitializeAsync();
Console.WriteLine("staff-docs file share initialized (Recipes, Manuals, Policies).");

builder.Build().Run();