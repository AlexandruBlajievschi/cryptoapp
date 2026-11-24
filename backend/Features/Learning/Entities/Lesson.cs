namespace backend.Features.Learning.Entities;

public class Lesson
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public int SortOrder { get; set; }
}
