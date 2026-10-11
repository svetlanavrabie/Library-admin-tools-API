using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LibraryAPI.Migrations
{
    public partial class AddMoreSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "DateOfBirth", "DateOfDeath", "FirstName", "LastName", "MainCategory" },
                values: new object[,]
                {
                    { new Guid("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"), new DateTimeOffset(new DateTime(1710, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)), null, "Josephine", "Silver Tongue Marlowe", "Navigation" },
                    { new Guid("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"), new DateTimeOffset(new DateTime(1695, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)), null, "Bartholomew", "Iron Hook Dunmore", "Treasure" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "AuthorId", "Description", "Title" },
                values: new object[,]
                {
                    { new Guid("e5b8a7f6-afb2-40ce-9b54-5c6d7e8f9a01"), new Guid("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"), "A practical guide to charting coastlines and hidden coves.", "Mapping Uncharted Islands" },
                    { new Guid("f6c9b8a7-b0c3-41df-8c65-6d7e8f9a0b12"), new Guid("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"), "Make rum from ship supplies when the barrels run dry.", "Brewing Your Own Rum at Sea" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "AuthorId", "Description", "Title" },
                values: new object[] { new Guid("c3f6e5d4-8d90-4eac-9f32-3a4b5c6d7e8f"), new Guid("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"), "Learn to navigate open seas using only the night sky.", "Reading the Stars Without a Compass" });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "AuthorId", "Description", "Title" },
                values: new object[] { new Guid("d4a7f6e5-9ea1-4fbd-8a43-4b5c6d7e8f90"), new Guid("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"), "Techniques for hiding your loot and drawing maps only you can read.", "Burying Treasure Like a Professional" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("c3f6e5d4-8d90-4eac-9f32-3a4b5c6d7e8f"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("d4a7f6e5-9ea1-4fbd-8a43-4b5c6d7e8f90"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("e5b8a7f6-afb2-40ce-9b54-5c6d7e8f9a01"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("f6c9b8a7-b0c3-41df-8c65-6d7e8f9a0b12"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"));
        }
    }
}
