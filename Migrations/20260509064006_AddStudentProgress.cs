using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ECL.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "studentusers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    passwordhash = table.Column<string>(type: "text", nullable: false),
                    createdatutc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studentusers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quizattempts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentuserid = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    formnumber = table.Column<int>(type: "integer", nullable: true),
                    grammartype = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<string>(type: "text", nullable: true),
                    totalquestions = table.Column<int>(type: "integer", nullable: false),
                    correctcount = table.Column<int>(type: "integer", nullable: false),
                    startedatutc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finishedatutc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizattempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizattempts_studentusers_studentuserid",
                        column: x => x.studentuserid,
                        principalSchema: "public",
                        principalTable: "studentusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizattemptanswers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quizattemptid = table.Column<long>(type: "bigint", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    questionqno = table.Column<int>(type: "integer", nullable: false),
                    selectedoption = table.Column<int>(type: "integer", nullable: true),
                    correctoption = table.Column<int>(type: "integer", nullable: false),
                    iscorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizattemptanswers", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizattemptanswers_quizattempts_quizattemptid",
                        column: x => x.quizattemptid,
                        principalSchema: "public",
                        principalTable: "quizattempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quizattemptanswers_quizattemptid_questionqno",
                schema: "public",
                table: "quizattemptanswers",
                columns: new[] { "quizattemptid", "questionqno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quizattempts_studentuserid",
                schema: "public",
                table: "quizattempts",
                column: "studentuserid");

            migrationBuilder.CreateIndex(
                name: "IX_studentusers_username",
                schema: "public",
                table: "studentusers",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quizattemptanswers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "quizattempts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "studentusers",
                schema: "public");
        }
    }
}
