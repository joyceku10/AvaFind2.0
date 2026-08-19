using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AvaFind.Data;

namespace backend_dotnet;

public class GetMeta
{
    private readonly ILogger<GetMeta> _logger;
    private readonly AvaFindDbContext _database;

    public GetMeta(ILogger<GetMeta> logger, AvaFindDbContext database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("GetMeta")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "meta")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        ImportMeta? meta = await _database.ImportMetadata.FindAsync(1);

        if (meta is null)
        {
            return new OkObjectResult(new
            {
                extract_date = (DateOnly?)null,
                imported_at = (DateTime?)null,
                row_count = 0,
                source_file = (string?)null
            });
        }

        return new OkObjectResult(new
        {
            extract_date = meta.ExtractDate,
            imported_at = meta.ImportedAt,
            row_count = meta.RowCount,
            source_file = meta.SourceFile
        });
    }
}
