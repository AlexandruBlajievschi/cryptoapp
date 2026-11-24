using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261124181412_InitialLearningSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GlossaryTerms",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Term = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Definition = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GlossaryTerms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Lessons",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Lessons", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GlossaryTerms_Term",
            table: "GlossaryTerms",
            column: "Term",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Lessons_Slug",
            table: "Lessons",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GlossaryTerms");

        migrationBuilder.DropTable(
            name: "Lessons");
    }
}
