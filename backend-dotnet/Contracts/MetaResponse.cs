using System.Text.Json.Serialization;

namespace AvaFind.Contracts;

public sealed record MetaResponse(
    [property: JsonPropertyName("extract_date")]
    DateOnly? ExtractDate,

    [property: JsonPropertyName("imported_at")]
    DateTime? ImportedAt,

    [property: JsonPropertyName("row_count")]
    int RowCount,

    [property: JsonPropertyName("source_file")]
    string? SourceFile);