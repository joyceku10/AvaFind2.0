using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace backend_dotnet;

public class GetMeta
{
    private readonly AvaFindDbContext _db;

    public GetMeta(AvaFindDbContext db)
    {
        _db = db;
    }

    [Function("GetMeta")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "meta")] HttpRequestData req)
    {
        var meta = await _db.ImportMeta.FirstOrDefaultAsync();

        var payload = new
        {
            extractDate = meta?.ExtractDate?.ToString("yyyy-MM-dd"),
            importedAt = meta?.ImportedAt?.ToString("o"),
            rowCount = meta?.RowCount ?? 0,
            sourceFile = meta?.SourceFile
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(payload);
        return response;
    }
}