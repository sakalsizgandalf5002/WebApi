using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexOnStockSymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "61ccb02c-6ced-488e-ae8d-4017364cb2ec");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c45f1f17-6d31-4074-84d9-35e550101bd0");

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "Stocks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "824a8483-92ba-4b21-bb3a-2475d32da3f9", null, "User", "USER" },
                    { "fab0e314-b34d-4872-9e31-d5f08b028389", null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Symbol",
                table: "Stocks",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stocks_Symbol",
                table: "Stocks");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "824a8483-92ba-4b21-bb3a-2475d32da3f9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fab0e314-b34d-4872-9e31-d5f08b028389");

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "Stocks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "61ccb02c-6ced-488e-ae8d-4017364cb2ec", null, "Admin", "ADMIN" },
                    { "c45f1f17-6d31-4074-84d9-35e550101bd0", null, "User", "USER" }
                });
        }
    }
}
