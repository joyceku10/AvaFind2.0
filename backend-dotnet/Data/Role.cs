using backend_dotnet;
public class Role
{
    public required string RoleId { get; set; } = string.Empty;
    public string? Rfe8 { get; set; }
    public string? Rfe9 { get; set; }
    public string? Client { get; set; }
    public string? ProjectId {get; set;}
    public string? ProjectName { get; set; }
    public string? RoleTitle { get; set; }
    public string? AssignedRole { get; set; }
    public string? PrimaryContact { get; set; }
    public string? FulfillmentContact { get; set; }
    public string? JobFamilyGroup { get; set; }
    public string? RoleStatus { get; set; }
    public bool? SoldRole { get; set; }
    public bool? ChargRole { get; set; }
    public string? Channel { get; set; }
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? PrimarySkillName { get; set; }
    public string? PrimarySkillProficiency { get; set; }
    public string? SkillsRaw { get; set; }
    public string? ProjectGeo { get; set; }
    public string? WorkLocation { get; set; }
    public string? Priority { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public string? Description { get; set; }

public ICollection<RoleSkill> Skills { get; set; } = new List<RoleSkill>(); //or [] for more cleaner, modern design; compiler infers the type based on the target
//ICollection<RoleSkill> - an interface repping the collection of RoleSkill (the interface created in RoleSkill.cs) objects
//public interface ICollection<T> - creates a new interface that can be implemented by any class

}