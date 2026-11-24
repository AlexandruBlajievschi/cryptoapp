namespace backend.Features.Learning.Entities;

public class GlossaryTerm
{
    public int Id { get; set; }
    public required string Term { get; set; }
    public required string Definition { get; set; }
}
