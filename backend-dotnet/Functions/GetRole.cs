using AvaFind.Data;
using AvaFind.Mapping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace backend_dotnet;

public sealed class GetRole(AvaFindDbContext database)
{
    [Function("GetRole")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{roleId}")] HttpRequest req,
        string roleId
    )
    {
        Role? role = await database.Roles
            .AsNoTracking()
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(
                candidate => candidate.RoleId == roleId
            );

        if (role is null)
        {
            return new NotFoundObjectResult(new {
                Message = $"Role with ID '{roleId}' not found"
            });
        }

        return new OkObjectResult(RoleResponseMapper.ToDetailResponse(role));
    }
}
