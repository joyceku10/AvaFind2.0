using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
var sqlConnectionString = Environment.GetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(sqlConnectionString))
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

        sqlConnectionString =
            $"Server=tcp:{sqlServer},1433;" +
            $"Initial Catalog={sqlDatabase};" +
            $"User ID={sqlUser};" +
            $"Password={sqlPassword};" +
            "Encrypt=True;" +
            "TrustServerCertificate=False;" +
            "Connection Timeout=30;";
    }
}

var configuredSqlitePath = Environment.GetEnvironmentVariable("SQLITE_DATABASE_PATH");
var sqlitePath = string.IsNullOrWhiteSpace(configuredSqlitePath)
    ? Path.GetFullPath(Path.Combine(
        builder.Environment.ContentRootPath,
        "..", "backend", "avafind.db"))
    : Path.GetFullPath(configuredSqlitePath);

if (!string.IsNullOrWhiteSpace(sqlConnectionString) && CanConnectToSqlServer(sqlConnectionString))
{
    builder.Services.AddDbContext<AvaFindDbContext>(options =>
        options.UseSqlServer(sqlConnectionString));
}
else
{
    if (!File.Exists(sqlitePath))
    {
        throw new FileNotFoundException(
            $"SQLite fallback database file was not found: {sqlitePath}",
            sqlitePath);
    }

    Console.WriteLine($"Using SQLite database: {sqlitePath}");
    builder.Services.AddDbContext<AvaFindDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}

builder.Build().Run();

static bool CanConnectToSqlServer(string connectionString)
{
    try
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 5
        };

        using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        return true;
    }
    catch (SqlException exception)
    {
        Console.Error.WriteLine(
            $"Azure SQL is unavailable (SQL error {exception.Number}); using SQLite fallback.");
        return false;
    }
}
