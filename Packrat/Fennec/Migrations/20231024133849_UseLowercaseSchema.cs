using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fennec.Migrations
{
    /// <inheritdoc />
    public partial class UseLowercaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fennec");

            migrationBuilder.RenameTable(
                name: "SingleTraces",
                schema: "Fennec",
                newName: "SingleTraces",
                newSchema: "fennec");

            migrationBuilder.RenameTable(
                name: "NetworkHosts",
                schema: "Fennec",
                newName: "NetworkHosts",
                newSchema: "fennec");

            migrationBuilder.RenameTable(
                name: "Layouts",
                schema: "Fennec",
                newName: "Layouts",
                newSchema: "fennec");

            migrationBuilder.RenameTable(
                name: "GraphNodes",
                schema: "Fennec",
                newName: "GraphNodes",
                newSchema: "fennec");

            migrationBuilder.RenameTable(
                name: "CompressedGroups",
                schema: "Fennec",
                newName: "CompressedGroups",
                newSchema: "fennec");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Fennec");

            migrationBuilder.RenameTable(
                name: "SingleTraces",
                schema: "fennec",
                newName: "SingleTraces",
                newSchema: "Fennec");

            migrationBuilder.RenameTable(
                name: "NetworkHosts",
                schema: "fennec",
                newName: "NetworkHosts",
                newSchema: "Fennec");

            migrationBuilder.RenameTable(
                name: "Layouts",
                schema: "fennec",
                newName: "Layouts",
                newSchema: "Fennec");

            migrationBuilder.RenameTable(
                name: "GraphNodes",
                schema: "fennec",
                newName: "GraphNodes",
                newSchema: "Fennec");

            migrationBuilder.RenameTable(
                name: "CompressedGroups",
                schema: "fennec",
                newName: "CompressedGroups",
                newSchema: "Fennec");
        }
    }
}
