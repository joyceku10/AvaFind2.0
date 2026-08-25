using System.Text.Json.Serialization;

namespace AvaFind.Contracts;

public sealed record SkillResponse(
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("proficiency")]
    string? Proficiency
);