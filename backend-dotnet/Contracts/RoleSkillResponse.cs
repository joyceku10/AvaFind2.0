using System.Text.Json.Serialization;

namespace backend_dotnet.Contracts;

public sealed record RoleSkillResponse(
    //[property: JsonPropertyName("role_id")] string RoleId,
    //[property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("proficiency")] string? Proficiency);