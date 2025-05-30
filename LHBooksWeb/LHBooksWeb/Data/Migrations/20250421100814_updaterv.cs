﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class updaterv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "tb_ProductReview",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_tb_ProductReview_UserId",
                table: "tb_ProductReview",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ProductReview_AspNetUsers_UserId",
                table: "tb_ProductReview",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_ProductReview_AspNetUsers_UserId",
                table: "tb_ProductReview");

            migrationBuilder.DropIndex(
                name: "IX_tb_ProductReview_UserId",
                table: "tb_ProductReview");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "tb_ProductReview",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
