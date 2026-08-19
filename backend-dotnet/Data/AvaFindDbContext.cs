using Microsoft.EntityFrameworkCore;

namespace backend_dotnet;

public class AvaFindDbContext : DbContext
{
    public AvaFindDbContext(DbContextOptions<AvaFindDbContext> options)
        : base(options)
    {
    }

    public DbSet<ImportMeta> ImportMeta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportMeta>(entity =>
        {
            entity.ToTable("import_meta");
            entity.Property(meta => meta.SourceFile).HasColumnName("source_file");
            entity.Property(meta => meta.ExtractDate).HasColumnName("extract_date");
            entity.Property(meta => meta.ImportedAt).HasColumnName("imported_at");
            entity.Property(meta => meta.RowCount).HasColumnName("row_count");
        });
    }
}

public class ImportMeta
{
    public int Id { get; set; }
    public string? SourceFile { get; set; }
    public DateTime? ExtractDate { get; set; }
    public DateTime? ImportedAt { get; set; }
    public int RowCount { get; set; }
}