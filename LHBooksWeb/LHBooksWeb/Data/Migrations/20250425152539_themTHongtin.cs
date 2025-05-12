using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class themTHongtin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "tb_Product",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "tb_Product",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "tb_Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishYear",
                table: "tb_Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "tb_Product",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "PublishYear",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "tb_Product");
        }
    }
}
