using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AT.Data.Migrations
{
    public partial class AddedSupportForMultipleExchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pairs_Name",
                table: "Pairs");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "PairHistory",
                newName: "CreateDate");

            migrationBuilder.AddColumn<int>(
                name: "ActiveHours",
                table: "Pairs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Pairs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Exchange",
                table: "Pairs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "PairHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Exchange",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_Name_Exchange",
                table: "Pairs",
                columns: new[] { "Name", "Exchange" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_OrderLevel_IsActive_PairId",
                table: "PairHistory",
                columns: new[] { "OrderLevel", "IsActive", "PairId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pairs_Name_Exchange",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_PairHistory_OrderLevel_IsActive_PairId",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "ActiveHours",
                table: "Pairs");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Pairs");

            migrationBuilder.DropColumn(
                name: "Exchange",
                table: "Pairs");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "Exchange",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "PairHistory",
                newName: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_Name",
                table: "Pairs",
                column: "Name",
                unique: true);
        }
    }
}
