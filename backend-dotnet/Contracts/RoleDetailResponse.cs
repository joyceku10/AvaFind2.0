using System.Text.Json.Serialization;

namespace AvaFind.Contracts;

public sealed record RoleDetailResponse(
    [property: JsonPropertyName("role_id")]
    string RoleId,

    [property: JsonPropertyName("role_title")]
    string? RoleTitle,

    [property: JsonPropertyName("client")]
    string? Client,

    [property: JsonPropertyName("project_name")]
    string? ProjectName,
    
    [property: JsonPropertyName("work_location")]
    string? WorkLocation,

    [property: JsonPropertyName("job_family_group")]
    string? JobFamilyGroup,

    [property: JsonPropertyName("role_status")]
    string? RoleStatus,

    [property: JsonPropertyName("priority")]
    string? Priority,

    [property: JsonPropertyName("min_level")]
    int? MinLevel,

    [property: JsonPropertyName("max_level")]
    int? MaxLevel,

    [property: JsonPropertyName("start_date")]
    DateOnly? StartDate,

    [property: JsonPropertyName("end_date")]
    DateOnly? EndDate,

    [property: JsonPropertyName("duration_days")]
    int? DurationDays,

    [property: JsonPropertyName("created_date")]
    DateOnly? CreatedDate,

    [property: JsonPropertyName("primary_skill")]
    string? PrimarySkill,

    [property: JsonPropertyName("skills")] 
    IReadOnlyList<SkillResponse> Skills,

    [property: JsonPropertyName("rfe8")]
    string? Rfe8,

    [property: JsonPropertyName("rfe9")]
    string? Rfe9,

    [property: JsonPropertyName("project_id")]
    string? ProjectId,

    [property: JsonPropertyName("assigned_role")]
    string? AssignedRole,

    [property: JsonPropertyName("primary_contact")]
    string? PrimaryContact,

    [property: JsonPropertyName("fulfillment_contact")]
    string? FulfillmentContact,

    [property: JsonPropertyName("sold_role")]
    bool? SoldRole,

    [property: JsonPropertyName("charg_role")]
    bool? ChargRole,

    [property: JsonPropertyName("channel")]
    string? Channel,

    [property: JsonPropertyName("project_geo")]
    string? ProjectGeo,

    [property: JsonPropertyName("primary_skill_proficiency")]
    string? PrimarySkillProficiency,
    
    [property: JsonPropertyName("description")]
    string? Description

);