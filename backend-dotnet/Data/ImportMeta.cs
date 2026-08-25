namespace backend_dotnet.Data;
public class ImportMeta
{
    public int Id { get; set; }
    public string? SourceFile { get; set; }
    public DateOnly? ExtractDate { get; set; }
    public DateTime? ImportedAt { get; set; }
    public int RowCount { get; set; }
}