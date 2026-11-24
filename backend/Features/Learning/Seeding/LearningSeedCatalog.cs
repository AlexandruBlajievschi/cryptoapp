using System.Text.Json;
using System.Text.RegularExpressions;
using backend.Features.Learning.Entities;

namespace backend.Features.Learning.Seeding;

public sealed record LearningSeedCatalog(
    IReadOnlyList<Lesson> Lessons,
    IReadOnlyList<GlossaryTerm> GlossaryTerms)
{
    public static LearningSeedCatalog Load()
    {
        var entries = JsonSerializer.Deserialize<LessonEntry[]>(ReadResource("lessons.json"),
            JsonSerializerOptions.Web) ?? throw new InvalidOperationException("Missing lesson seed entries.");
        var lessons = entries.Select(entry => new Lesson
        {
            Slug = entry.Slug,
            Title = entry.Title,
            SortOrder = entry.SortOrder,
            Content = ReadResource(entry.ContentFile)
        }).ToArray();
        var glossary = JsonSerializer.Deserialize<GlossaryTerm[]>(ReadResource("glossary.json"),
            JsonSerializerOptions.Web) ?? throw new InvalidOperationException("Missing glossary seed entries.");

        if (lessons.Length < 6 || glossary.Length < 15
            || lessons.Select(lesson => lesson.Slug).Distinct(StringComparer.OrdinalIgnoreCase).Count() != lessons.Length
            || lessons.Select(lesson => lesson.SortOrder).Distinct().Count() != lessons.Length
            || glossary.Select(term => term.Term).Distinct(StringComparer.OrdinalIgnoreCase).Count() != glossary.Length
            || lessons.Any(lesson => !Regex.IsMatch(lesson.Slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$")
                || lesson.Slug.Length > 160 || string.IsNullOrWhiteSpace(lesson.Title)
                || lesson.Title.Length > 200 || string.IsNullOrWhiteSpace(lesson.Content) || lesson.SortOrder <= 0)
            || glossary.Any(term => string.IsNullOrWhiteSpace(term.Term) || term.Term.Length > 120
                || string.IsNullOrWhiteSpace(term.Definition)))
        {
            throw new InvalidOperationException("Learning seed content is incomplete or contains invalid/duplicate entries.");
        }

        return new LearningSeedCatalog(lessons, glossary);
    }

    private static string ReadResource(string name)
    {
        var assembly = typeof(LearningSeedCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Features.Learning.SeedData.{name}")
            ?? throw new InvalidOperationException($"Missing learning seed resource: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed record LessonEntry(string Slug, string Title, int SortOrder, string ContentFile);
}
