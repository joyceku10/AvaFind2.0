using Microsoft.EntityFrameworkCore;

namespace AvaFind.Data;

public sealed class AvaFindDbContext(DbContextOptions<AvaFindDbContext> options) : DbContext(options)
{
    public DbSet<ImportMeta> ImportMetadata => Set<ImportMeta>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleSkill> RoleSkills => Set<RoleSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasMany(role => role.Skills)
            .WithOne(skill => skill.Role)
            .HasForeignKey(skill => skill.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

