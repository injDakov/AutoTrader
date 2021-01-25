using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AT.Data.Migrations
{
    public partial class InitDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetailedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ClientOrderId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderLevel = table.Column<int>(type: "int", nullable: false),
                    Side = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PriceAverage = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    ProfitRatio = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    PreviousOrderExecutedPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    CurrentMarketPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    PreviousOrderProfitRatio = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    PreviousOrderProfitRatioToCurrentPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousOrderId = table.Column<long>(type: "bigint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Orders_PreviousOrderId",
                        column: x => x.PreviousOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pairs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaxOrderLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pairs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PairHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OrderLevel = table.Column<int>(type: "int", nullable: false),
                    ActiveHours = table.Column<int>(type: "int", nullable: false),
                    ExecutedSellOrderCount = table.Column<int>(type: "int", nullable: false),
                    ExecutedBuyOrderCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PairId = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PairHistory_Pairs_PairId",
                        column: x => x.PairId,
                        principalTable: "Pairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderId",
                table: "Orders",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PreviousOrderId",
                table: "Orders",
                column: "PreviousOrderId",
                unique: true,
                filter: "[PreviousOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PairHistory_PairId",
                table: "PairHistory",
                column: "PairId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_Name",
                table: "Pairs",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PairHistory");

            migrationBuilder.DropTable(
                name: "Pairs");
        }
    }
}