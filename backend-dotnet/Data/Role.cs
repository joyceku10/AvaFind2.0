using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaFind.Data;

[Table("roles")]
public sealed class Role
{
    [Key]
    [Column("role_id"), MaxLength(20)]
    public required string RoleId {get; set; }

    [Column("rfe8"), MaxLength(100)]
    public string? Rfe8 { get; set;}

    [Column("rfe9"), MaxLength(100)]
    public string? Rfe9 { get; set; }

    [Column("client"), MaxLength(200)]
    public string? Client { get; set; }

    [Column("project_id"), MaxLength(20)]
    public string? ProjectId { get; set; }

    [Column("project_name"), MaxLength(300)]
    public string? ProjectName { get; set; }

    [Column("role_title"), MaxLength(300)]
    public string? RoleTitle { get; set; }

    [Column("assigned_role"), MaxLength(200)]
    public string? AssignedRole { get; set; }

    [Column("primary_contact"), MaxLength(200)]
    public string? PrimaryContact { get; set; }

    [Column("fulfillment_contact"), MaxLength(200)]
    public string? FulfillmentContact { get; set; }

    [Column("job_family_group"), MaxLength(100)]
    public string? JobFamilyGroup { get; set; }

    [Column("role_status"), MaxLength(100)]
    public string? RoleStatus { get; set; }

    [Column("sold_role")]
    public bool? SoldRole { get; set; }

    [Column("charg_role")]
    public bool? ChargRole { get; set; }

    [Column("channel"), MaxLength(100)]
    public string? Channel { get; set; }

    [Column("min_level")]
    public int? MinLevel { get; set; }

    [Column("max_level")]
    public int? MaxLevel { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("primary_skill_name"), MaxLength(200)]
    public string? PrimarySkillName { get; set; }

    [Column("primary_skill_proficiency"), MaxLength(50)]
    public string? PrimarySkillProficiency { get; set; }

    [Column("skills_raw")]
    public string? SkillsRaw { get; set; }

    [Column("project_geo"), MaxLength(100)]
    public string? ProjectGeo { get; set; }

    [Column("work_location"), MaxLength(100)]
    public string? WorkLocation { get; set; }

    [Column("priority"), MaxLength(50)]
    public string? Priority { get; set; }

    [Column("created_date")]
    public DateOnly? CreatedDate { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    public ICollection<RoleSkill> Skills { get; set; } = new List<RoleSkill>();
}