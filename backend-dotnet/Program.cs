using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using backend_dotnet;
using System.IO;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}
//have to setup database, which is EF Core (equivalent to SQLAlchemy SessionLocal)

var connectionString = Environment.GetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AvaFindDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    var sqlServer = Environment.GetEnvironmentVariable("SQL_SERVER");

    if (!string.IsNullOrWhiteSpace(sqlServer))
    {
        var sqlDatabase = Environment.GetEnvironmentVariable("SQL_DATABASE")
            ?? throw new InvalidOperationException("SQL_DATABASE must be configured.");

        var sqlUser = Environment.GetEnvironmentVariable("SQL_USER")
            ?? throw new InvalidOperationException("SQL_USER must be configured.");

        var sqlPassword = Environment.GetEnvironmentVariable("SQL_PASSWORD")
            ?? throw new InvalidOperationException("SQL_PASSWORD must be configured.");

        var azureSqlConnectionString =
            $"Server=tcp:{sqlServer},1433;" +
            $"Initial Catalog={sqlDatabase};" +
            $"User ID={sqlUser};" +
            $"Password={sqlPassword};" +
            "Encrypt=True;" +
            "TrustServerCertificate=False;" +
            "Connection Timeout=30;";

        builder.Services.AddDbContext<AvaFindDbContext>(options =>
            options.UseSqlServer(azureSqlConnectionString));
    }
    else
    {
        var sqlitePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "backend", "avafind.db"));

        builder.Services.AddDbContext<AvaFindDbContext>(options =>
            options.UseSqlite($"Data Source={sqlitePath}"));
    }
}

builder.Build().Run();
