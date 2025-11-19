using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIdToWhatever : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { "8D12FA6F-9B3A-4AFC-9E35-47DCEF1B1111", null, "Admin", "ADMIN" },
                    { "B9B07E7A-4BB0-4BD9-AEB6-242D2D4B2222", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8D12FA6F-9B3A-4AFC-9E35-47DCEF1B1111");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "B9B07E7A-4BB0-4BD9-AEB6-242D2D4B2222");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "279b0250-0f2f-4bcc-941d-e54f6c42800b", null, "User", "USER" },
                    { "d3e0acf1-9487-40eb-a4d7-b8c821512690", null, "Admin", "ADMIN" }
                });
        }
    }
}
