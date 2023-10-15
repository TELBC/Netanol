using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fennec.Migrations
{
    /// <inheritdoc />
    public partial class UseUnsignedNumericsForSingleTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PacketCount",
                schema: "Fennec",
                table: "SingleTraces",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "ByteCount",
                schema: "Fennec",
                table: "SingleTraces",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PacketCount",
                schema: "Fennec",
                table: "SingleTraces",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<int>(
                name: "ByteCount",
                schema: "Fennec",
                table: "SingleTraces",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
