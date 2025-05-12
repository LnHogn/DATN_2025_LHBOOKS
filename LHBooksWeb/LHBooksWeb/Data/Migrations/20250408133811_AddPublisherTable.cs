using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublisherTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHome",
                table: "tb_Product");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "tb_Product",
                newName: "Name");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "tb_Product",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "tb_Product",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_Publisher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Publisher", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_Product_PublisherId",
                table: "tb_Product",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Product_tb_Publisher_PublisherId",
                table: "tb_Product",
                column: "PublisherId",
                principalTable: "tb_Publisher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_Product_tb_Publisher_PublisherId",
                table: "tb_Product");

            migrationBuilder.DropTable(
                name: "tb_Publisher");

            migrationBuilder.DropIndex(
                name: "IX_tb_Product_PublisherId",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "tb_Product");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "tb_Product");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "tb_Product",
                newName: "Title");

            migrationBuilder.AddColumn<bool>(
                name: "IsHome",
                table: "tb_Product",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
