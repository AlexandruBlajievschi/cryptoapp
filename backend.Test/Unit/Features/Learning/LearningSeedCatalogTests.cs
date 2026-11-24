using backend.Features.Learning.Seeding;

namespace backend.Test.Unit.Features.Learning;

public class LearningSeedCatalogTests
{
    [Fact]
    public void Catalog_CoversTheSixRequiredTopicsInOrder()
    {
        var catalog = LearningSeedCatalog.Load();

        Assert.Equal(new[] { "what-is-crypto", "blockchain", "bitcoin-vs-altcoins",
            "wallets-and-private-keys", "exchanges", "risks-and-scams" },
            catalog.Lessons.OrderBy(lesson => lesson.SortOrder).Select(lesson => lesson.Slug));
        Assert.Equal(Enumerable.Range(1, 6), catalog.Lessons.Select(lesson => lesson.SortOrder));
        Assert.All(catalog.Lessons, lesson =>
        {
            foreach (var section in new[] { "Learning goal", "Example", "Common misconception",
                "Key takeaways", "Related glossary terms" })
            {
                Assert.Contains($"## {section}", lesson.Content);
            }

            var relatedTerms = lesson.Content.Split("## Related glossary terms")[1]
                .Trim().TrimEnd('.').Split(';', StringSplitOptions.TrimEntries);
            Assert.All(relatedTerms, name => Assert.Contains(catalog.GlossaryTerms, term => term.Term == name));
        });
    }

    [Fact]
    public void Glossary_Contains25UniqueTermsWithDefinitions()
    {
        var terms = LearningSeedCatalog.Load().GlossaryTerms;

        Assert.Equal(25, terms.Count);
        Assert.Equal(25, terms.Select(term => term.Term).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(terms, term => Assert.False(string.IsNullOrWhiteSpace(term.Definition)));
    }

    [Fact]
    public void LoadingAgain_ReturnsIndependentEntities()
    {
        var first = LearningSeedCatalog.Load();
        first.Lessons[0].Title = "Changed in memory";
        first.GlossaryTerms[0].Definition = "Changed in memory";

        var second = LearningSeedCatalog.Load();

        Assert.NotEqual(first.Lessons[0].Title, second.Lessons[0].Title);
        Assert.NotEqual(first.GlossaryTerms[0].Definition, second.GlossaryTerms[0].Definition);
    }
}
