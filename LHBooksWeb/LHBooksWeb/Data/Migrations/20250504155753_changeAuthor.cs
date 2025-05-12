using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_Product_tb_Author_AuthorId",
                table: "tb_Product");

            migrationBuilder.DropIndex(
                name: "IX_tb_Product_AuthorId",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "tb_Product");

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "tb_Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "tb_Product");

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "tb_Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Product_AuthorId",
                table: "tb_Product",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Product_tb_Author_AuthorId",
                table: "tb_Product",
                column: "AuthorId",
                principalTable: "tb_Author",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
