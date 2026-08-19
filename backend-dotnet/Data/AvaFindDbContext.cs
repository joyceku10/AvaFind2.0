using Microsoft.EntityFrameworkCore;

namespace AvaFind.Data;

public sealed class AvaFindDbContext(DbContextOptions<AvaFindDbContext> options) : DbContext(options)
{
    public DbSet<ImportMeta> ImportMetadata => Set<ImportMeta>();
}

