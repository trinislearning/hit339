using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGames.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEmailLogColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerUserId",
                table: "PosSales");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "PosSales");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "PosSales");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "PosSales",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "PosSales",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PosSales",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Audience",
                table: "EmailLogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "EmailLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetGroup",
                table: "EmailLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PosSales_ProductId",
                table: "PosSales",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PosSales_Products_ProductId",
                table: "PosSales",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PosSales_Products_ProductId",
                table: "PosSales");

            migrationBuilder.DropIndex(
                name: "IX_PosSales_ProductId",
                table: "PosSales");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "PosSales");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PosSales");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "TargetGroup",
                table: "EmailLogs");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "PosSales",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "CustomerUserId",
                table: "PosSales",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "PosSales",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "PosSales",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Audience",
                table: "EmailLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
