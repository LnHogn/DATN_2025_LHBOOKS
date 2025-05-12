using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateOrderDetail2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_OrderDetail_FlashSales_FlashSaleId",
                table: "tb_OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_tb_OrderDetail_FlashSaleId",
                table: "tb_OrderDetail");

            migrationBuilder.DropColumn(
                name: "FlashSaleId",
                table: "tb_OrderDetail");

            migrationBuilder.DropColumn(
                name: "IsFlashSale",
                table: "tb_OrderDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlashSaleId",
                table: "tb_OrderDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlashSale",
                table: "tb_OrderDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_tb_OrderDetail_FlashSaleId",
                table: "tb_OrderDetail",
                column: "FlashSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_OrderDetail_FlashSales_FlashSaleId",
                table: "tb_OrderDetail",
                column: "FlashSaleId",
                principalTable: "FlashSales",
                principalColumn: "Id");
        }
    }
}
