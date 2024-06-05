using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpBlockerNetcore.Migrations
{
    /// <inheritdoc />
    public partial class domainNameddBlacklist1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "BlackList",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "BlackList");
        }
    }
}
