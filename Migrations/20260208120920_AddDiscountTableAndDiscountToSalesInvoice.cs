using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlegriaPosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountTableAndDiscountToSalesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "salesinvoices",
                newName: "DiscountAmount");

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "salesinvoices",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_DiscountId",
                table: "salesinvoices",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_salesinvoices_discounts_DiscountId",
                table: "salesinvoices",
                column: "DiscountId",
                principalTable: "discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_salesinvoices_discounts_DiscountId",
                table: "salesinvoices");

            migrationBuilder.DropTable(
                name: "discounts");

            migrationBuilder.DropIndex(
                name: "IX_salesinvoices_DiscountId",
                table: "salesinvoices");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "salesinvoices");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "salesinvoices",
                newName: "Discount");
        }
    }
}
