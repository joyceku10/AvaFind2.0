using System.Net;
using backend_dotnet;
using backend_dotnet.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;


namespace backend_dotnet;

public sealed class GetRole
{
    private readonly AvaFindDbContext _db;

    public GetRole(AvaFindDbContext db)
    {
        _db = db;
    }

    [Function("GetRole")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "roles/{roleId}")]
        HttpRequestData request,
        string roleId)
    {
        // Retrieve the role from the database based on the provided roleId
        var role = await _db.Roles
            .AsNoTracking() //doesn't need to track edits bc the endpoint only returns data
            .Include(role => role.Skills)
            .SingleOrDefaultAsync(role => role.RoleId == roleId); //finds one row where roleId matches the provided roleId

        // If the role is not found, return a 404 Not Found response
        if (role is null)
        {
            var notFound = request.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteAsJsonAsync(new { detail = "Role not found" });
            return notFound;
        }
        //calculates duration of the role in days if both start and end dates are available
        var durationDays =
            role.StartDate is not null && role.EndDate is not null
                ? role.EndDate.Value.DayNumber - role.StartDate.Value.DayNumber
                : -1;

        //after the role is found, return the role details in the expected JSON shape
        var roleDetail = new RoleDetailResponse(
            role.RoleId,
            role.Rfe8,
            role.Rfe9,
            role.Client,
            role.ProjectId,
            role.ProjectName,
            role.RoleTitle,
            role.AssignedRole,
            role.PrimaryContact,
            role.FulfillmentContact,
            role.JobFamilyGroup,
            role.RoleStatus,
            role.SoldRole,
            role.ChargRole,
            role.MinLevel,
            role.MaxLevel,
            role.StartDate,
            role.EndDate,
            durationDays >= 0 ? durationDays : null,
            role.PrimarySkillName,
            role.PrimarySkillProficiency,
            role.Skills.Select(skill => new RoleSkillResponse(skill.Name, skill.Proficiency)).ToList(), //selecting skill name and proficiency from the skills list and creating a new RoleSkillResponse for each skill
            role.ProjectGeo,
            role.WorkLocation,
            role.Priority,
            role.CreatedDate,
            role.Description
        );

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(roleDetail);
        return response;
    }
}