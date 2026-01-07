using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlegriaPosApi.Migrations
{
    /// <inheritdoc />
    public partial class AdddCostPerUnitToMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPerUnit",
                table: "materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPerUnit",
                table: "materials");
        }
    }
}
