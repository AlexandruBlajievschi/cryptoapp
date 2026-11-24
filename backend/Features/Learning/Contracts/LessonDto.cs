namespace backend.Features.Learning.Contracts;

public sealed record LessonDto(int Id, string Slug, string Title, int SortOrder, string Content);
