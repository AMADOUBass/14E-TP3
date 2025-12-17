using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Locomotiv.Migrations
{
    public partial class AjoutStationIdDansEtape : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "Etapes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItineraireId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombrePlaces = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DateReservation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PrixTotal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Confirmé")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Itineraires_ItineraireId",
                        column: x => x.ItineraireId,
                        principalTable: "Itineraires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Etapes_StationId",
                table: "Etapes",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DateReservation",
                table: "Bookings",
                column: "DateReservation");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ItineraireId",
                table: "Bookings",
                column: "ItineraireId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Etapes_Stations_StationId",
                table: "Etapes",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etapes_Stations_StationId",
                table: "Etapes");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Etapes_StationId",
                table: "Etapes");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "Etapes");
        }
    }
}
