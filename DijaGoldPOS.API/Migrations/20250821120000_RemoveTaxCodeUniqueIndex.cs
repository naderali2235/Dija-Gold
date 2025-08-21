using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijaGoldPOS.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaxCodeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaxConfigurations_TaxCode",
                table: "TaxConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TaxConfigurations_TaxCode",
                table: "TaxConfigurations",
                column: "TaxCode",
                unique: true);
        }
    }
}
