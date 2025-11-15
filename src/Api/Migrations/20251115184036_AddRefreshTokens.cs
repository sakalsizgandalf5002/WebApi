using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29cc4e24-67cc-479a-897a-888420fe5352");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8ad75201-b047-4468-b984-aa50fb4b5e62");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "279b0250-0f2f-4bcc-941d-e54f6c42800b", null, "User", "USER" },
                    { "d3e0acf1-9487-40eb-a4d7-b8c821512690", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "279b0250-0f2f-4bcc-941d-e54f6c42800b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e0acf1-9487-40eb-a4d7-b8c821512690");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "29cc4e24-67cc-479a-897a-888420fe5352", null, "Admin", "ADMIN" },
                    { "8ad75201-b047-4468-b984-aa50fb4b5e62", null, "User", "USER" }
                });
        }
    }
}
