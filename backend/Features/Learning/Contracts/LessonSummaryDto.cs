namespace backend.Features.Learning.Contracts;

public sealed record LessonSummaryDto(int Id, string Slug, string Title, int SortOrder);
