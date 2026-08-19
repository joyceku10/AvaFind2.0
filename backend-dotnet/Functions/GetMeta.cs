using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AvaFind.Data;
using AvaFind.Contracts;

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

        ImportMeta? meta = await _database.ImportMetadata.AsNoTracking().FirstOrDefaultAsync();

        if (meta is null)
        {
            return new OkObjectResult(
                new MetaResponse(null, null, 0, null)
            );
        }

        return new OkObjectResult(new MetaResponse(
            meta.ExtractDate,
            meta.ImportedAt,
            meta.RowCount,
            meta.SourceFile
        ));
    }
}
