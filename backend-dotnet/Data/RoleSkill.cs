public class RoleSkill {
    public int Id { get; set; }
    public required string RoleId { get; set; }
    public int Position { get; set; }
    public required string? Name { get; set; }
    public string? Proficiency { get; set; }
    public Role Role { get; set; } = null!;
}