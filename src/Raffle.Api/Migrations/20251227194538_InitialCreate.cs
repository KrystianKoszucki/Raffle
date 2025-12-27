using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raffle.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RaffleDrawId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaffleDraws",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WinnerMemberId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaffleDraws", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaffleDraws_Members_WinnerMemberId",
                        column: x => x.WinnerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_RaffleDrawId",
                table: "Members",
                column: "RaffleDrawId");

            migrationBuilder.CreateIndex(
                name: "IX_RaffleDraws_Name",
                table: "RaffleDraws",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaffleDraws_WinnerMemberId",
                table: "RaffleDraws",
                column: "WinnerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_RaffleDraws_RaffleDrawId",
                table: "Members",
                column: "RaffleDrawId",
                principalTable: "RaffleDraws",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_RaffleDraws_RaffleDrawId",
                table: "Members");

            migrationBuilder.DropTable(
                name: "RaffleDraws");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
