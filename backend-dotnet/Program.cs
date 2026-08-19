using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

using AvaFind.Data;
using Microsoft.EntityFrameworkCore;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var connectionString =
    builder.Configuration["AvaFindConnectionString"] 
    ?? throw new InvalidOperationException("AvaFindConnectionString is not configured.");

var databaseProvider =
    builder.Configuration["AvaFindDatabaseProvider"]
    ?? throw new InvalidOperationException("AvaFindDatabaseProvider is not configured.");

builder.Services.AddDbContext<AvaFindDbContext>(options =>
{
    if (databaseProvider.Equals(
        "Sqlite",
        StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else if (databaseProvider.Equals(
        "SqlServer",
        StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(
            connectionString,
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
    }
    else {
        throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
    }
});

builder.Build().Run();
