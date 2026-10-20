using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixAuthorBirthDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("102b566b-ba1f-404c-b2df-e2cde39ade09"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1971, 3, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2902b665-1190-4c70-9915-b9c2d7680450"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1968, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2aadd2df-7caf-45ab-9355-7f6332985a87"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1978, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1981, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1974, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1984, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1987, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1960, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1963, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("102b566b-ba1f-404c-b2df-e2cde39ade09"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1702, 3, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2902b665-1190-4c70-9915-b9c2d7680450"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1701, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2aadd2df-7caf-45ab-9355-7f6332985a87"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1723, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1721, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1690, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1710, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1695, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1650, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                column: "DateOfBirth",
                value: new DateTimeOffset(new DateTime(1668, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));
        }
    }
}
