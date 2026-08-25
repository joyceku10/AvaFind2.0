using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaFind.Data;

[Table("role_skills")]
public sealed class RoleSkill
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("role_id"), MaxLength(20)]
    public required string RoleId { get; set; }

    [Column("position")]
    public int Position { get; set; }

    [Column("name"), MaxLength(200)]
    public required string Name { get; set; }

    [Column("proficiency"), MaxLength(50)]
    public string? Proficiency { get; set; }

    public Role Role { get; set; } = null!;
}
