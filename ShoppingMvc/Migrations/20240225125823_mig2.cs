using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingMvc.Migrations
{
    public partial class mig2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopState",
                table: "SellerDatas");

            migrationBuilder.AddColumn<int>(
                name: "ShopCategory",
                table: "SellerDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopCategory",
                table: "SellerDatas");

            migrationBuilder.AddColumn<string>(
                name: "ShopState",
                table: "SellerDatas",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
