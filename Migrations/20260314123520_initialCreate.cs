using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECL.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReadingQuestions",
                table: "ReadingQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListeningQuestions",
                table: "ListeningQuestions");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "ReadingQuestions",
                newName: "readingquestions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ListeningQuestions",
                newName: "listeningquestions",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "QuestionText",
                schema: "public",
                table: "readingquestions",
                newName: "questiontext");

            migrationBuilder.RenameColumn(
                name: "OptionD",
                schema: "public",
                table: "readingquestions",
                newName: "optiond");

            migrationBuilder.RenameColumn(
                name: "OptionC",
                schema: "public",
                table: "readingquestions",
                newName: "optionc");

            migrationBuilder.RenameColumn(
                name: "OptionB",
                schema: "public",
                table: "readingquestions",
                newName: "optionb");

            migrationBuilder.RenameColumn(
                name: "OptionA",
                schema: "public",
                table: "readingquestions",
                newName: "optiona");

            migrationBuilder.RenameColumn(
                name: "FormNumber",
                schema: "public",
                table: "readingquestions",
                newName: "formnumber");

            migrationBuilder.RenameColumn(
                name: "CorrectOption",
                schema: "public",
                table: "readingquestions",
                newName: "correctoption");

            migrationBuilder.RenameColumn(
                name: "Qno",
                schema: "public",
                table: "readingquestions",
                newName: "qno");

            migrationBuilder.RenameColumn(
                name: "OptionD",
                schema: "public",
                table: "listeningquestions",
                newName: "optiond");

            migrationBuilder.RenameColumn(
                name: "OptionC",
                schema: "public",
                table: "listeningquestions",
                newName: "optionc");

            migrationBuilder.RenameColumn(
                name: "OptionB",
                schema: "public",
                table: "listeningquestions",
                newName: "optionb");

            migrationBuilder.RenameColumn(
                name: "OptionA",
                schema: "public",
                table: "listeningquestions",
                newName: "optiona");

            migrationBuilder.RenameColumn(
                name: "FormNumber",
                schema: "public",
                table: "listeningquestions",
                newName: "formnumber");

            migrationBuilder.RenameColumn(
                name: "CorrectOption",
                schema: "public",
                table: "listeningquestions",
                newName: "correctoption");

            migrationBuilder.RenameColumn(
                name: "AudioFile",
                schema: "public",
                table: "listeningquestions",
                newName: "audiofile");

            migrationBuilder.RenameColumn(
                name: "Qno",
                schema: "public",
                table: "listeningquestions",
                newName: "qno");

            migrationBuilder.AlterColumn<string>(
                name: "optiond",
                schema: "public",
                table: "listeningquestions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "optionc",
                schema: "public",
                table: "listeningquestions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "optionb",
                schema: "public",
                table: "listeningquestions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "optiona",
                schema: "public",
                table: "listeningquestions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "audiofile",
                schema: "public",
                table: "listeningquestions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_readingquestions",
                schema: "public",
                table: "readingquestions",
                column: "qno");

            migrationBuilder.AddPrimaryKey(
                name: "PK_listeningquestions",
                schema: "public",
                table: "listeningquestions",
                column: "qno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_readingquestions",
                schema: "public",
                table: "readingquestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_listeningquestions",
                schema: "public",
                table: "listeningquestions");

            migrationBuilder.RenameTable(
                name: "readingquestions",
                schema: "public",
                newName: "ReadingQuestions");

            migrationBuilder.RenameTable(
                name: "listeningquestions",
                schema: "public",
                newName: "ListeningQuestions");

            migrationBuilder.RenameColumn(
                name: "questiontext",
                table: "ReadingQuestions",
                newName: "QuestionText");

            migrationBuilder.RenameColumn(
                name: "optiond",
                table: "ReadingQuestions",
                newName: "OptionD");

            migrationBuilder.RenameColumn(
                name: "optionc",
                table: "ReadingQuestions",
                newName: "OptionC");

            migrationBuilder.RenameColumn(
                name: "optionb",
                table: "ReadingQuestions",
                newName: "OptionB");

            migrationBuilder.RenameColumn(
                name: "optiona",
                table: "ReadingQuestions",
                newName: "OptionA");

            migrationBuilder.RenameColumn(
                name: "formnumber",
                table: "ReadingQuestions",
                newName: "FormNumber");

            migrationBuilder.RenameColumn(
                name: "correctoption",
                table: "ReadingQuestions",
                newName: "CorrectOption");

            migrationBuilder.RenameColumn(
                name: "qno",
                table: "ReadingQuestions",
                newName: "Qno");

            migrationBuilder.RenameColumn(
                name: "optiond",
                table: "ListeningQuestions",
                newName: "OptionD");

            migrationBuilder.RenameColumn(
                name: "optionc",
                table: "ListeningQuestions",
                newName: "OptionC");

            migrationBuilder.RenameColumn(
                name: "optionb",
                table: "ListeningQuestions",
                newName: "OptionB");

            migrationBuilder.RenameColumn(
                name: "optiona",
                table: "ListeningQuestions",
                newName: "OptionA");

            migrationBuilder.RenameColumn(
                name: "formnumber",
                table: "ListeningQuestions",
                newName: "FormNumber");

            migrationBuilder.RenameColumn(
                name: "correctoption",
                table: "ListeningQuestions",
                newName: "CorrectOption");

            migrationBuilder.RenameColumn(
                name: "audiofile",
                table: "ListeningQuestions",
                newName: "AudioFile");

            migrationBuilder.RenameColumn(
                name: "qno",
                table: "ListeningQuestions",
                newName: "Qno");

            migrationBuilder.AlterColumn<string>(
                name: "OptionD",
                table: "ListeningQuestions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionC",
                table: "ListeningQuestions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionB",
                table: "ListeningQuestions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionA",
                table: "ListeningQuestions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AudioFile",
                table: "ListeningQuestions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReadingQuestions",
                table: "ReadingQuestions",
                column: "Qno");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListeningQuestions",
                table: "ListeningQuestions",
                column: "Qno");
        }
    }
}
