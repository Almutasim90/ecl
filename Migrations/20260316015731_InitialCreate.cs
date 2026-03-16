using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ECL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "listeningquestions",
                schema: "public",
                columns: table => new
                {
                    qno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    formnumber = table.Column<int>(type: "integer", nullable: false),
                    audiofile = table.Column<string>(type: "text", nullable: false),
                    optiona = table.Column<string>(type: "text", nullable: false),
                    optionb = table.Column<string>(type: "text", nullable: false),
                    optionc = table.Column<string>(type: "text", nullable: false),
                    optiond = table.Column<string>(type: "text", nullable: false),
                    correctoption = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listeningquestions", x => x.qno);
                });

            migrationBuilder.CreateTable(
                name: "readingquestions",
                schema: "public",
                columns: table => new
                {
                    qno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    formnumber = table.Column<int>(type: "integer", nullable: false),
                    questiontext = table.Column<string>(type: "text", nullable: false),
                    optiona = table.Column<string>(type: "text", nullable: false),
                    optionb = table.Column<string>(type: "text", nullable: false),
                    optionc = table.Column<string>(type: "text", nullable: false),
                    optiond = table.Column<string>(type: "text", nullable: false),
                    correctoption = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readingquestions", x => x.qno);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listeningquestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "readingquestions",
                schema: "public");
        }
    }
}
