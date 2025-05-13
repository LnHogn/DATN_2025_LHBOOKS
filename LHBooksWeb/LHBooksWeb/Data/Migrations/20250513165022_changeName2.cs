using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeName2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleProducts_tb_Product_ProductId",
                table: "FlashSaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_CartItem_tb_Product_ProductId",
                table: "tb_CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_OrderDetail_FlashSales_FlashSaleId",
                table: "tb_OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_OrderDetail_tb_Order_OrderId",
                table: "tb_OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_OrderDetail_tb_Product_ProductId",
                table: "tb_OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_Product_tb_ProductCategory_ProductCategoryId",
                table: "tb_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_Product_tb_ProductSubCategory_ProductSubCategoryId",
                table: "tb_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_Product_tb_Publisher_PublisherId",
                table: "tb_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_ProductImage_tb_Product_ProductId",
                table: "tb_ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_ProductReview_AspNetUsers_UserId",
                table: "tb_ProductReview");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_ProductReview_tb_Product_ProductId",
                table: "tb_ProductReview");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_ProductSubCategory_tb_ProductCategory_ProductCategoryId",
                table: "tb_ProductSubCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_Publisher",
                table: "tb_Publisher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_ProductSubCategory",
                table: "tb_ProductSubCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_ProductReview",
                table: "tb_ProductReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_ProductImage",
                table: "tb_ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_ProductCategory",
                table: "tb_ProductCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_Product",
                table: "tb_Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_OrderDetail",
                table: "tb_OrderDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_Order",
                table: "tb_Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_CartItem",
                table: "tb_CartItem");

            migrationBuilder.RenameTable(
                name: "tb_Publisher",
                newName: "Publisher");

            migrationBuilder.RenameTable(
                name: "tb_ProductSubCategory",
                newName: "ProductSubCategory");

            migrationBuilder.RenameTable(
                name: "tb_ProductReview",
                newName: "ProductReview");

            migrationBuilder.RenameTable(
                name: "tb_ProductImage",
                newName: "ProductImage");

            migrationBuilder.RenameTable(
                name: "tb_ProductCategory",
                newName: "ProductCategory");

            migrationBuilder.RenameTable(
                name: "tb_Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "tb_OrderDetail",
                newName: "OrderDetail");

            migrationBuilder.RenameTable(
                name: "tb_Order",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "tb_CartItem",
                newName: "CartItem");

            migrationBuilder.RenameIndex(
                name: "IX_tb_ProductSubCategory_ProductCategoryId",
                table: "ProductSubCategory",
                newName: "IX_ProductSubCategory_ProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_ProductReview_UserId",
                table: "ProductReview",
                newName: "IX_ProductReview_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_ProductReview_ProductId",
                table: "ProductReview",
                newName: "IX_ProductReview_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_ProductImage_ProductId",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_Product_PublisherId",
                table: "Products",
                newName: "IX_Products_PublisherId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_Product_ProductSubCategoryId",
                table: "Products",
                newName: "IX_Products_ProductSubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_Product_ProductCategoryId",
                table: "Products",
                newName: "IX_Products_ProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_OrderDetail_ProductId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_OrderDetail_FlashSaleId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_FlashSaleId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_CartItem_ProductId",
                table: "CartItem",
                newName: "IX_CartItem_ProductId");

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Publisher",
                table: "Publisher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSubCategory",
                table: "ProductSubCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductReview",
                table: "ProductReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetail",
                table: "OrderDetail",
                columns: new[] { "OrderId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_Products_ProductId",
                table: "FlashSaleProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_FlashSales_FlashSaleId",
                table: "OrderDetail",
                column: "FlashSaleId",
                principalTable: "FlashSales",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Orders_OrderId",
                table: "OrderDetail",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Products_ProductId",
                table: "OrderDetail",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReview_AspNetUsers_UserId",
                table: "ProductReview",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReview_Products_ProductId",
                table: "ProductReview",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategory_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductSubCategory_ProductSubCategoryId",
                table: "Products",
                column: "ProductSubCategoryId",
                principalTable: "ProductSubCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Publisher_PublisherId",
                table: "Products",
                column: "PublisherId",
                principalTable: "Publisher",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSubCategory_ProductCategory_ProductCategoryId",
                table: "ProductSubCategory",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleProducts_Products_ProductId",
                table: "FlashSaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_FlashSales_FlashSaleId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Orders_OrderId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Products_ProductId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReview_AspNetUsers_UserId",
                table: "ProductReview");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReview_Products_ProductId",
                table: "ProductReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategory_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductSubCategory_ProductSubCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Publisher_PublisherId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSubCategory_ProductCategory_ProductCategoryId",
                table: "ProductSubCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Publisher",
                table: "Publisher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSubCategory",
                table: "ProductSubCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductReview",
                table: "ProductReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetail",
                table: "OrderDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Publisher",
                newName: "tb_Publisher");

            migrationBuilder.RenameTable(
                name: "ProductSubCategory",
                newName: "tb_ProductSubCategory");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "tb_Product");

            migrationBuilder.RenameTable(
                name: "ProductReview",
                newName: "tb_ProductReview");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "tb_ProductImage");

            migrationBuilder.RenameTable(
                name: "ProductCategory",
                newName: "tb_ProductCategory");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "tb_Order");

            migrationBuilder.RenameTable(
                name: "OrderDetail",
                newName: "tb_OrderDetail");

            migrationBuilder.RenameTable(
                name: "CartItem",
                newName: "tb_CartItem");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSubCategory_ProductCategoryId",
                table: "tb_ProductSubCategory",
                newName: "IX_tb_ProductSubCategory_ProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_PublisherId",
                table: "tb_Product",
                newName: "IX_tb_Product_PublisherId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProductSubCategoryId",
                table: "tb_Product",
                newName: "IX_tb_Product_ProductSubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProductCategoryId",
                table: "tb_Product",
                newName: "IX_tb_Product_ProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductReview_UserId",
                table: "tb_ProductReview",
                newName: "IX_tb_ProductReview_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductReview_ProductId",
                table: "tb_ProductReview",
                newName: "IX_tb_ProductReview_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId",
                table: "tb_ProductImage",
                newName: "IX_tb_ProductImage_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_ProductId",
                table: "tb_OrderDetail",
                newName: "IX_tb_OrderDetail_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_FlashSaleId",
                table: "tb_OrderDetail",
                newName: "IX_tb_OrderDetail_FlashSaleId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItem_ProductId",
                table: "tb_CartItem",
                newName: "IX_tb_CartItem_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_Publisher",
                table: "tb_Publisher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_ProductSubCategory",
                table: "tb_ProductSubCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_Product",
                table: "tb_Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_ProductReview",
                table: "tb_ProductReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_ProductImage",
                table: "tb_ProductImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_ProductCategory",
                table: "tb_ProductCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_Order",
                table: "tb_Order",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_OrderDetail",
                table: "tb_OrderDetail",
                columns: new[] { "OrderId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_CartItem",
                table: "tb_CartItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_tb_Product_ProductId",
                table: "FlashSaleProducts",
                column: "ProductId",
                principalTable: "tb_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_CartItem_tb_Product_ProductId",
                table: "tb_CartItem",
                column: "ProductId",
                principalTable: "tb_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_OrderDetail_FlashSales_FlashSaleId",
                table: "tb_OrderDetail",
                column: "FlashSaleId",
                principalTable: "FlashSales",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_OrderDetail_tb_Order_OrderId",
                table: "tb_OrderDetail",
                column: "OrderId",
                principalTable: "tb_Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_OrderDetail_tb_Product_ProductId",
                table: "tb_OrderDetail",
                column: "ProductId",
                principalTable: "tb_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Product_tb_ProductCategory_ProductCategoryId",
                table: "tb_Product",
                column: "ProductCategoryId",
                principalTable: "tb_ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Product_tb_ProductSubCategory_ProductSubCategoryId",
                table: "tb_Product",
                column: "ProductSubCategoryId",
                principalTable: "tb_ProductSubCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Product_tb_Publisher_PublisherId",
                table: "tb_Product",
                column: "PublisherId",
                principalTable: "tb_Publisher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ProductImage_tb_Product_ProductId",
                table: "tb_ProductImage",
                column: "ProductId",
                principalTable: "tb_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ProductReview_AspNetUsers_UserId",
                table: "tb_ProductReview",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ProductReview_tb_Product_ProductId",
                table: "tb_ProductReview",
                column: "ProductId",
                principalTable: "tb_Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ProductSubCategory_tb_ProductCategory_ProductCategoryId",
                table: "tb_ProductSubCategory",
                column: "ProductCategoryId",
                principalTable: "tb_ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
