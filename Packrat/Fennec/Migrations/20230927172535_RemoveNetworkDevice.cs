using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fennec.Migrations
{
    public partial class RemoveNetworkDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompressedGroups_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "CompressedGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_IslandGroups_IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_LayoutPresets_LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId1",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkHosts_NetworkDevices_DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkHosts_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropTable(
                name: "IslandGroups",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "LayoutPresets",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "NetworkDevices",
                schema: "Fennec");

            migrationBuilder.DropIndex(
                name: "IX_NetworkHosts_DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropIndex(
                name: "IX_NetworkHosts_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_NetworkDeviceId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropColumn(
                name: "NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts");

            migrationBuilder.DropColumn(
                name: "IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.RenameColumn(
                name: "DnsInformation_DnsName",
                schema: "Fennec",
                table: "NetworkHosts",
                newName: "DnsInfo_DnsName");

            migrationBuilder.RenameColumn(
                name: "DnsInformation_LastAccessedDnsName",
                schema: "Fennec",
                table: "NetworkHosts",
                newName: "DnsInfo_LastChecked");

            migrationBuilder.RenameColumn(
                name: "NetworkDeviceId1",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "NetworkHostId");

            migrationBuilder.RenameColumn(
                name: "NetworkDeviceId",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "IslandGroup");

            migrationBuilder.RenameColumn(
                name: "LayoutResetId",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "LayoutId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphNodes_NetworkDeviceId1",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "IX_GraphNodes_NetworkHostId");

            migrationBuilder.RenameColumn(
                name: "NetworkDeviceId",
                schema: "Fennec",
                table: "CompressedGroups",
                newName: "NetworkHostId");

            migrationBuilder.RenameIndex(
                name: "IX_CompressedGroups_NetworkDeviceId",
                schema: "Fennec",
                table: "CompressedGroups",
                newName: "IX_CompressedGroups_NetworkHostId");

            migrationBuilder.AlterColumn<string>(
                name: "DnsInfo_DnsName",
                schema: "Fennec",
                table: "NetworkHosts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                schema: "Fennec",
                table: "GraphNodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Position_X",
                schema: "Fennec",
                table: "GraphNodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position_Y",
                schema: "Fennec",
                table: "GraphNodes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Layouts",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_LayoutId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_Name",
                schema: "Fennec",
                table: "Layouts",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompressedGroups_NetworkHosts_NetworkHostId",
                schema: "Fennec",
                table: "CompressedGroups",
                column: "NetworkHostId",
                principalSchema: "Fennec",
                principalTable: "NetworkHosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_Layouts_LayoutId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "LayoutId",
                principalSchema: "Fennec",
                principalTable: "Layouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_NetworkHosts_NetworkHostId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "NetworkHostId",
                principalSchema: "Fennec",
                principalTable: "NetworkHosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompressedGroups_NetworkHosts_NetworkHostId",
                schema: "Fennec",
                table: "CompressedGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_Layouts_LayoutId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_NetworkHosts_NetworkHostId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropTable(
                name: "Layouts",
                schema: "Fennec");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_LayoutId",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "Position_X",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "Position_Y",
                schema: "Fennec",
                table: "GraphNodes");

            migrationBuilder.RenameColumn(
                name: "DnsInfo_DnsName",
                schema: "Fennec",
                table: "NetworkHosts",
                newName: "DnsInformation_DnsName");

            migrationBuilder.RenameColumn(
                name: "DnsInfo_LastChecked",
                schema: "Fennec",
                table: "NetworkHosts",
                newName: "DnsInformation_LastAccessedDnsName");

            migrationBuilder.RenameColumn(
                name: "NetworkHostId",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "NetworkDeviceId1");

            migrationBuilder.RenameColumn(
                name: "LayoutId",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "LayoutResetId");

            migrationBuilder.RenameColumn(
                name: "IslandGroup",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "NetworkDeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphNodes_NetworkHostId",
                schema: "Fennec",
                table: "GraphNodes",
                newName: "IX_GraphNodes_NetworkDeviceId1");

            migrationBuilder.RenameColumn(
                name: "NetworkHostId",
                schema: "Fennec",
                table: "CompressedGroups",
                newName: "NetworkDeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_CompressedGroups_NetworkHostId",
                schema: "Fennec",
                table: "CompressedGroups",
                newName: "IX_CompressedGroups_NetworkDeviceId");

            migrationBuilder.AlterColumn<string>(
                name: "DnsInformation_DnsName",
                schema: "Fennec",
                table: "NetworkHosts",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "IslandGroups",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IslandGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LayoutPresets",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NetworkDevices",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DnsName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDevices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NetworkHosts_DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                column: "DnsInformation_NetworkDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkHosts_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                column: "NetworkDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "IslandGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "LayoutPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_NetworkDeviceId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "NetworkDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutPresets_Name",
                schema: "Fennec",
                table: "LayoutPresets",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompressedGroups_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "CompressedGroups",
                column: "NetworkDeviceId",
                principalSchema: "Fennec",
                principalTable: "NetworkDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_IslandGroups_IslandGroupId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "IslandGroupId",
                principalSchema: "Fennec",
                principalTable: "IslandGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_LayoutPresets_LayoutPresetId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "LayoutPresetId",
                principalSchema: "Fennec",
                principalTable: "LayoutPresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "GraphNodes",
                column: "NetworkDeviceId",
                principalSchema: "Fennec",
                principalTable: "NetworkDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId1",
                schema: "Fennec",
                table: "GraphNodes",
                column: "NetworkDeviceId1",
                principalSchema: "Fennec",
                principalTable: "NetworkDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkHosts_NetworkDevices_DnsInformation_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                column: "DnsInformation_NetworkDeviceId",
                principalSchema: "Fennec",
                principalTable: "NetworkDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkHosts_NetworkDevices_NetworkDeviceId",
                schema: "Fennec",
                table: "NetworkHosts",
                column: "NetworkDeviceId",
                principalSchema: "Fennec",
                principalTable: "NetworkDevices",
                principalColumn: "Id");
        }
    }
}
