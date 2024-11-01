using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("8b2cde0c-d627-4947-b9e3-59c9478adb1d"), "Payment.API" },
                    { new Guid("8cd7dca6-2f0a-4dd8-8256-c90d57b56fd7"), "Stock.API" },
                    { new Guid("ef938c33-4872-4729-8ba2-62ee1999609c"), "Order.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("8b2cde0c-d627-4947-b9e3-59c9478adb1d"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("8cd7dca6-2f0a-4dd8-8256-c90d57b56fd7"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("ef938c33-4872-4729-8ba2-62ee1999609c"));
        }
    }
}
