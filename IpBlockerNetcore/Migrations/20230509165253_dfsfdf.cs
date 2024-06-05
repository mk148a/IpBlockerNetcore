using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpBlockerNetcore.Migrations
{
    /// <inheritdoc />
    public partial class dfsfdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IPAddresi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddressBytes = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPAddresi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlackList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAdresiId = table.Column<int>(type: "int", nullable: false),
                    DangerLevel = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackList_IPAddresi_IpAdresiId",
                        column: x => x.IpAdresiId,
                        principalTable: "IPAddresi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScanIp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAdresiId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanIp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanIp_IPAddresi_IpAdresiId",
                        column: x => x.IpAdresiId,
                        principalTable: "IPAddresi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAdresiId = table.Column<int>(type: "int", nullable: false),
                    DangerLevel = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteList_IPAddresi_IpAdresiId",
                        column: x => x.IpAdresiId,
                        principalTable: "IPAddresi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlackList_IpAdresiId",
                table: "BlackList",
                column: "IpAdresiId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanIp_IpAdresiId",
                table: "ScanIp",
                column: "IpAdresiId");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteList_IpAdresiId",
                table: "WhiteList",
                column: "IpAdresiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackList");

            migrationBuilder.DropTable(
                name: "ScanIp");

            migrationBuilder.DropTable(
                name: "WhiteList");

            migrationBuilder.DropTable(
                name: "IPAddresi");
        }
    }
}
