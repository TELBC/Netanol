using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fennec.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Fennec");

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

            migrationBuilder.CreateTable(
                name: "GraphNodes",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LayoutPresetId = table.Column<long>(type: "bigint", nullable: false),
                    LayoutResetId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    NetworkDeviceId1 = table.Column<long>(type: "bigint", nullable: true),
                    NetworkDeviceId = table.Column<long>(type: "bigint", nullable: true),
                    IslandGroupId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphNodes_IslandGroups_IslandGroupId",
                        column: x => x.IslandGroupId,
                        principalSchema: "Fennec",
                        principalTable: "IslandGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GraphNodes_LayoutPresets_LayoutPresetId",
                        column: x => x.LayoutPresetId,
                        principalSchema: "Fennec",
                        principalTable: "LayoutPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId",
                        column: x => x.NetworkDeviceId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GraphNodes_NetworkDevices_NetworkDeviceId1",
                        column: x => x.NetworkDeviceId1,
                        principalSchema: "Fennec",
                        principalTable: "NetworkDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NetworkHosts",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: false),
                    DnsInformation_DnsName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DnsInformation_LastAccessedDnsName = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DnsInformation_NetworkDeviceId = table.Column<long>(type: "bigint", nullable: true),
                    NetworkDeviceId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkHosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkHosts_NetworkDevices_DnsInformation_NetworkDeviceId",
                        column: x => x.DnsInformation_NetworkDeviceId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkHosts_NetworkDevices_NetworkDeviceId",
                        column: x => x.NetworkDeviceId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompressedGroups",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GraphNodeId = table.Column<long>(type: "bigint", nullable: false),
                    NetworkDeviceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompressedGroups_GraphNodes_GraphNodeId",
                        column: x => x.GraphNodeId,
                        principalSchema: "Fennec",
                        principalTable: "GraphNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompressedGroups_NetworkDevices_NetworkDeviceId",
                        column: x => x.NetworkDeviceId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingleTraces",
                schema: "Fennec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExporterIp = table.Column<IPAddress>(type: "inet", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Protocol = table.Column<int>(type: "integer", nullable: false),
                    SourceHostId = table.Column<long>(type: "bigint", nullable: false),
                    SourcePort = table.Column<int>(type: "integer", nullable: false),
                    DestinationHostId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationPort = table.Column<int>(type: "integer", nullable: false),
                    ByteCount = table.Column<int>(type: "integer", nullable: false),
                    PacketCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleTraces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleTraces_NetworkHosts_DestinationHostId",
                        column: x => x.DestinationHostId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SingleTraces_NetworkHosts_SourceHostId",
                        column: x => x.SourceHostId,
                        principalSchema: "Fennec",
                        principalTable: "NetworkHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompressedGroups_GraphNodeId",
                schema: "Fennec",
                table: "CompressedGroups",
                column: "GraphNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompressedGroups_NetworkDeviceId",
                schema: "Fennec",
                table: "CompressedGroups",
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
                name: "IX_GraphNodes_NetworkDeviceId1",
                schema: "Fennec",
                table: "GraphNodes",
                column: "NetworkDeviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutPresets_Name",
                schema: "Fennec",
                table: "LayoutPresets",
                column: "Name",
                unique: true);

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
                name: "IX_SingleTraces_DestinationHostId",
                schema: "Fennec",
                table: "SingleTraces",
                column: "DestinationHostId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleTraces_SourceHostId",
                schema: "Fennec",
                table: "SingleTraces",
                column: "SourceHostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedGroups",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "SingleTraces",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "GraphNodes",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "NetworkHosts",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "IslandGroups",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "LayoutPresets",
                schema: "Fennec");

            migrationBuilder.DropTable(
                name: "NetworkDevices",
                schema: "Fennec");
        }
    }
}
