using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace backend_dotnet.Contracts;

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
        // Retrieve the meta information from the database
        var meta = await _db.ImportMeta.FirstOrDefaultAsync();

        var payload = new MetaResponse(
            meta?.ExtractDate,
            meta?.ImportedAt,
            meta?.RowCount ?? 0,
            meta?.SourceFile
        );

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(payload);
        return response;
    }
}