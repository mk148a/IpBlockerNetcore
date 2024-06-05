using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpBlockerNetcore.Migrations
{
    /// <inheritdoc />
    public partial class Ipblocker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entry",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rowNumber = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    clientIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    protocol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    responseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    qname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    qtype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    qclass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entry", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanLog");

            migrationBuilder.DropTable(
                name: "Entry");
        }
    }
}
