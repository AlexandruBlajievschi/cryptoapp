using backend.Features.Learning.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<GlossaryTerm> GlossaryTerms => Set<GlossaryTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.Property(lesson => lesson.Slug).HasMaxLength(160);
            entity.HasIndex(lesson => lesson.Slug).IsUnique();
            entity.Property(lesson => lesson.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<GlossaryTerm>(entity =>
        {
            entity.Property(term => term.Term).HasMaxLength(120);
            entity.HasIndex(term => term.Term).IsUnique();
        });
    }
}
