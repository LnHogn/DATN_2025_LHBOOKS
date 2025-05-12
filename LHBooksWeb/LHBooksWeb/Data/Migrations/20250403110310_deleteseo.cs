using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class deleteseo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "tb_ProductSubCategory");

            migrationBuilder.DropColumn(
                name: "SeoKeywords",
                table: "tb_ProductSubCategory");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "tb_ProductSubCategory");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "tb_ProductCategory");

            migrationBuilder.DropColumn(
                name: "SeoKeywords",
                table: "tb_ProductCategory");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "tb_ProductCategory");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "SeoKeyword",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "tb_New");

            migrationBuilder.DropColumn(
                name: "SeoKeyword",
                table: "tb_New");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "tb_New");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "tb_ProductSubCategory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "tb_ProductSubCategory",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "tb_ProductSubCategory",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "tb_ProductCategory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "tb_ProductCategory",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "tb_ProductCategory",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "tb_Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeyword",
                table: "tb_Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "tb_Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "tb_New",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoKeyword",
                table: "tb_New",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "tb_New",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
