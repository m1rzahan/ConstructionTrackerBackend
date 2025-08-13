using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionTracker.Migrations
{
    /// <inheritdoc />
    public partial class UserEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AbpUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AbpUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "AbpUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "AbpUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "AbpUsers",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "AbpUsers");
        }
    }
}
