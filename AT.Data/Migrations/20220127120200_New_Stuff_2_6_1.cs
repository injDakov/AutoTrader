using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AT.Data.Migrations
{
    public partial class New_Stuff_2_6_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TODO :
            // 1 Calculate the sum of the groups
            // 2 Update the first line

            migrationBuilder.Sql("DELETE FROM [PairHistory] WHERE [Id] NOT IN (SELECT MIN([Id]) FROM [PairHistory] GROUP BY [PairId])", true);

            migrationBuilder.DropIndex(
                name: "IX_PairHistory_OrderLevel_IsActive_PairId",
                table: "PairHistory");

            migrationBuilder.DropIndex(
                name: "IX_PairHistory_PairId",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "OrderAmount",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "OrderLevel",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "OrderLevel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "MaxOrderLevel",
                table: "Pairs",
                newName: "MaxOrderLevelCount");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Pairs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PairHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE [Pairs] SET [StartDate] = [CreateDate]", true);
            migrationBuilder.Sql("UPDATE [PairHistory] SET [StartDate] = [CreateDate]", true);

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_IsActive_PairId",
                table: "PairHistory",
                columns: new[] { "IsActive", "PairId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_PairId",
                table: "PairHistory",
                column: "PairId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PairHistory_IsActive_PairId",
                table: "PairHistory");

            migrationBuilder.DropIndex(
                name: "IX_PairHistory_PairId",
                table: "PairHistory");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Pairs");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PairHistory");

            migrationBuilder.RenameColumn(
                name: "MaxOrderLevelCount",
                table: "Pairs",
                newName: "MaxOrderLevel");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderAmount",
                table: "PairHistory",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OrderLevel",
                table: "PairHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderLevel",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_OrderLevel_IsActive_PairId",
                table: "PairHistory",
                columns: new[] { "OrderLevel", "IsActive", "PairId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_PairId",
                table: "PairHistory",
                column: "PairId");
        }
    }
}
