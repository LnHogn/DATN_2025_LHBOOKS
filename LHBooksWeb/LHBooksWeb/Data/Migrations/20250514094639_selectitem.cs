using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LHBooksWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class selectitem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "CartItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "CartItem");
        }
    }
}
