using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpBlockerNetcore.Migrations
{
    /// <inheritdoc />
    public partial class dfsfdflk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackList_IPAddresi_IpAdresiId",
                table: "BlackList");

            migrationBuilder.DropForeignKey(
                name: "FK_ScanIp_IPAddresi_IpAdresiId",
                table: "ScanIp");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteList_IPAddresi_IpAdresiId",
                table: "WhiteList");

            migrationBuilder.DropTable(
                name: "IPAddresi");

            migrationBuilder.DropIndex(
                name: "IX_WhiteList_IpAdresiId",
                table: "WhiteList");

            migrationBuilder.DropIndex(
                name: "IX_ScanIp_IpAdresiId",
                table: "ScanIp");

            migrationBuilder.DropIndex(
                name: "IX_BlackList_IpAdresiId",
                table: "BlackList");

            migrationBuilder.DropColumn(
                name: "IpAdresiId",
                table: "WhiteList");

            migrationBuilder.DropColumn(
                name: "IpAdresiId",
                table: "ScanIp");

            migrationBuilder.DropColumn(
                name: "IpAdresiId",
                table: "BlackList");

            migrationBuilder.AddColumn<string>(
                name: "IpAdresi",
                table: "WhiteList",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAdresi",
                table: "ScanIp",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAdresi",
                table: "BlackList",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAdresi",
                table: "WhiteList");

            migrationBuilder.DropColumn(
                name: "IpAdresi",
                table: "ScanIp");

            migrationBuilder.DropColumn(
                name: "IpAdresi",
                table: "BlackList");

            migrationBuilder.AddColumn<int>(
                name: "IpAdresiId",
                table: "WhiteList",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpAdresiId",
                table: "ScanIp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpAdresiId",
                table: "BlackList",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_WhiteList_IpAdresiId",
                table: "WhiteList",
                column: "IpAdresiId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanIp_IpAdresiId",
                table: "ScanIp",
                column: "IpAdresiId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackList_IpAdresiId",
                table: "BlackList",
                column: "IpAdresiId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackList_IPAddresi_IpAdresiId",
                table: "BlackList",
                column: "IpAdresiId",
                principalTable: "IPAddresi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScanIp_IPAddresi_IpAdresiId",
                table: "ScanIp",
                column: "IpAdresiId",
                principalTable: "IPAddresi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteList_IPAddresi_IpAdresiId",
                table: "WhiteList",
                column: "IpAdresiId",
                principalTable: "IPAddresi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
