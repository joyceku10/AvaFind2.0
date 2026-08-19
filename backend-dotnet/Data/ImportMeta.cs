using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaFind.Data;

[Table("import_meta")]
public sealed class ImportMeta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("extract_date")]
    public DateOnly? ExtractDate { get; set; }

    [Column("imported_at")]
    public DateTime ImportedAt { get; set; }

    [Column("row_count")]
    public int RowCount { get; set; }

    [Column("source_file")]
    public string SourceFile { get; set; }
}