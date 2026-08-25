using System.Text.Json.Serialization;
 
namespace backend_dotnet.Contracts;
 
public sealed record MetaResponse(
    [property: JsonPropertyName("extract_date")]
    DateOnly? ExtractDate,
 
    [property: JsonPropertyName("imported_at")]
    DateTime? ImportedAt,
 
    [property: JsonPropertyName("row_count")]
    int RowCount,
 
    [property: JsonPropertyName("source_file")]
    string? SourceFile);