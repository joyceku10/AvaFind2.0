using Microsoft.EntityFrameworkCore;
using backend_dotnet.Data;

namespace backend_dotnet;

public class AvaFindDbContext : DbContext
{
    public AvaFindDbContext(DbContextOptions<AvaFindDbContext> options)
        : base(options)
    {
    }

    public DbSet<ImportMeta> ImportMeta { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleSkill> RoleSkills { get; set; }

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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(role => role.RoleId); //pk

            entity.Property(role => role.RoleId).HasColumnName("role_id");
            entity.Property(role => role.Rfe8).HasColumnName("rfe8");
            entity.Property(role => role.Rfe9).HasColumnName("rfe9");
            entity.Property(role => role.Client).HasColumnName("client");
            entity.Property(role => role.ProjectId).HasColumnName("project_id");
            entity.Property(role => role.ProjectName).HasColumnName("project_name");
            entity.Property(role => role.RoleTitle).HasColumnName("role_title");
            entity.Property(role => role.AssignedRole).HasColumnName("assigned_role");
            entity.Property(role => role.PrimaryContact).HasColumnName("primary_contact");
            entity.Property(role => role.FulfillmentContact).HasColumnName("fulfillment_contact");
            entity.Property(role => role.JobFamilyGroup).HasColumnName("job_family_group");
            entity.Property(role => role.RoleStatus).HasColumnName("role_status");
            entity.Property(role => role.SoldRole).HasColumnName("sold_role");
            entity.Property(role => role.ChargRole).HasColumnName("charg_role");
            entity.Property(role => role.Channel).HasColumnName("channel");
            entity.Property(role => role.MinLevel).HasColumnName("min_level");
            entity.Property(role => role.MaxLevel).HasColumnName("max_level");
            entity.Property(role => role.StartDate).HasColumnName("start_date");
            entity.Property(role => role.EndDate).HasColumnName("end_date");
            entity.Property(role => role.PrimarySkillName).HasColumnName("primary_skill_name");
            entity.Property(role => role.PrimarySkillProficiency).HasColumnName("primary_skill_proficiency");
            entity.Property(role => role.SkillsRaw).HasColumnName("skills_raw");
            entity.Property(role => role.ProjectGeo).HasColumnName("project_geo");
            entity.Property(role => role.WorkLocation).HasColumnName("work_location");
            entity.Property(role => role.Priority).HasColumnName("priority");
            entity.Property(role => role.CreatedDate).HasColumnName("created_date");
            entity.Property(role => role.Description).HasColumnName("description");

            entity.HasMany(role => role.Skills)
                .WithOne(skill => skill.Role)
                .HasForeignKey(skill => skill.RoleId);
        });

        modelBuilder.Entity<RoleSkill>(entity =>
        {
            entity.ToTable("role_skills");
            entity.Property(skill => skill.Id).HasColumnName("id");
            entity.Property(skill => skill.RoleId).HasColumnName("role_id");
            entity.Property(skill => skill.Position).HasColumnName("position");
            entity.Property(skill => skill.Name).HasColumnName("name");
            entity.Property(skill => skill.Proficiency).HasColumnName("proficiency");
        });
    }
}
