using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingMvc.Migrations
{
    public partial class mig3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SellerId",
                table: "Tags",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SellerId",
                table: "Categories",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_SellerDatas_SellerId",
                table: "Categories",
                column: "SellerId",
                principalTable: "SellerDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_SellerDatas_SellerId",
                table: "Tags",
                column: "SellerId",
                principalTable: "SellerDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_SellerDatas_SellerId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_SellerDatas_SellerId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_SellerId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_SellerId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Categories");
        }
    }
}
