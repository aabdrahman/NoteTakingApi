using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteTaking.Api.Migrations
{
    /// <inheritdoc />
    public partial class PrepareCreateView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Notes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 9, 15, 23, 38, 1, 484, DateTimeKind.Local).AddTicks(517));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Notes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 9, 15, 23, 38, 1, 484, DateTimeKind.Local).AddTicks(517),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");
        }
    }
}
