using AvaFind.Contracts;
using AvaFind.Data;

namespace AvaFind.Mapping;

public static class RoleResponseMapper
{
    public static RoleDetailResponse ToDetailResponse(Role role)
    {
        int? durationDays =
            role.StartDate is not null && role.EndDate is not null
                ? (role.EndDate.Value.DayNumber - role.StartDate.Value.DayNumber)
                : null;
        
        SkillResponse[] skills = role.Skills
            .OrderBy(skill => skill.Position)
            .Select(skill => new SkillResponse(
                skill.Name,
                skill.Proficiency
            ))
            .ToArray();

        return new RoleDetailResponse(
            role.RoleId,
            role.RoleTitle,
            role.Client,
            role.ProjectName,
            role.WorkLocation,
            role.JobFamilyGroup,
            role.RoleStatus,
            role.Priority,
            role.MinLevel,
            role.MaxLevel,
            role.StartDate,
            role.EndDate,
            durationDays,
            role.CreatedDate,
            role.PrimarySkillName,
            skills,
            role.Rfe8,
            role.Rfe9,
            role.ProjectId,
            role.AssignedRole,
            role.PrimaryContact,
            role.FulfillmentContact,
            role.SoldRole,
            role.ChargRole,
            role.Channel,
            role.ProjectGeo,
            role.PrimarySkillProficiency,
            role.Description
        );

    }
}

