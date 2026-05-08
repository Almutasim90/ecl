using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECL.Migrations
{
    /// <inheritdoc />
    public partial class AddGrammarLevelAndExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: the table may have been provisioned outside of EF (seed bank);
            // ALTER ... ADD COLUMN IF NOT EXISTS lets the migration run safely on existing DBs.
            migrationBuilder.Sql(
                """
                ALTER TABLE public.grammarquestions
                    ADD COLUMN IF NOT EXISTS level       text NULL,
                    ADD COLUMN IF NOT EXISTS explanation text NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.grammarquestions
                    DROP COLUMN IF EXISTS explanation,
                    DROP COLUMN IF EXISTS level;
                """);
        }
    }
}
