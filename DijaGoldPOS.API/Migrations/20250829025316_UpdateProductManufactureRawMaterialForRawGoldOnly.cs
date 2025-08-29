using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijaGoldPOS.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductManufactureRawMaterialForRawGoldOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductManufactureRawMaterials_PurchaseOrderItems_PurchaseOrderItemId",
                table: "ProductManufactureRawMaterials");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                newName: "RawGoldPurchaseOrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductManufactureRawMaterials_PurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                newName: "IX_ProductManufactureRawMaterials_RawGoldPurchaseOrderItemId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ProductManufactureRawMaterials",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductManufactureRawMaterials_RawGoldPurchaseOrderItems_RawGoldPurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                column: "RawGoldPurchaseOrderItemId",
                principalTable: "RawGoldPurchaseOrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductManufactureRawMaterials_RawGoldPurchaseOrderItems_RawGoldPurchaseOrderItemId",
                table: "ProductManufactureRawMaterials");

            migrationBuilder.RenameColumn(
                name: "RawGoldPurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                newName: "PurchaseOrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductManufactureRawMaterials_RawGoldPurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                newName: "IX_ProductManufactureRawMaterials_PurchaseOrderItemId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ProductManufactureRawMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductManufactureRawMaterials_PurchaseOrderItems_PurchaseOrderItemId",
                table: "ProductManufactureRawMaterials",
                column: "PurchaseOrderItemId",
                principalTable: "PurchaseOrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
