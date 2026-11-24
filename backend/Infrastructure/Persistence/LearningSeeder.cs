using backend.Features.Learning.Entities;
using backend.Features.Learning.Seeding;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Persistence;

public static class LearningSeeder
{
    private const string RequiredSchema = "20261124181412_InitialLearningSchema";

    public static void Seed(DbContext context)
    {
        // EF also invokes seeding after downgrades, including a rollback to an empty database.
        if (!context.Database.GetAppliedMigrations().Contains(RequiredSchema))
        {
            return;
        }

        Apply(context, LearningSeedCatalog.Load(),
            context.Set<Lesson>().ToList(), context.Set<GlossaryTerm>().ToList());
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (!(await context.Database.GetAppliedMigrationsAsync(cancellationToken)).Contains(RequiredSchema))
        {
            return;
        }

        Apply(context, LearningSeedCatalog.Load(),
            await context.Set<Lesson>().ToListAsync(cancellationToken),
            await context.Set<GlossaryTerm>().ToListAsync(cancellationToken));
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void Apply(DbContext context, LearningSeedCatalog catalog,
        List<Lesson> existingLessons, List<GlossaryTerm> existingTerms)
    {
        var lessonsBySlug = existingLessons.ToDictionary(lesson => lesson.Slug);
        foreach (var seed in catalog.Lessons)
        {
            if (!lessonsBySlug.TryGetValue(seed.Slug, out var lesson))
            {
                context.Set<Lesson>().Add(seed);
                continue;
            }

            lesson.Title = seed.Title;
            lesson.Content = seed.Content;
            lesson.SortOrder = seed.SortOrder;
        }

        var termsByName = existingTerms.ToDictionary(term => term.Term);
        foreach (var seed in catalog.GlossaryTerms)
        {
            if (!termsByName.TryGetValue(seed.Term, out var term))
            {
                context.Set<GlossaryTerm>().Add(seed);
                continue;
            }

            term.Definition = seed.Definition;
        }
    }
}
