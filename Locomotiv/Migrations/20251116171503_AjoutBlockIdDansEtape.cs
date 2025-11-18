using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Locomotiv.Migrations
{
    public partial class AjoutBlockIdDansEtape : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointArret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Localisation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    EstStation = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointArret", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Station",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Localisation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    CapaciteMaxTrains = table.Column<int>(type: "INTEGER", nullable: false),
                    StationType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Station", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Signau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signau_Station_StationId",
                        column: x => x.StationId,
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Prenom = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Station_StationId",
                        column: x => x.StationId,
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Voie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voie_Station_StationId",
                        column: x => x.StationId,
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LatitudeDepart = table.Column<double>(type: "REAL", nullable: false),
                    LongitudeDepart = table.Column<double>(type: "REAL", nullable: false),
                    LatitudeArrivee = table.Column<double>(type: "REAL", nullable: false),
                    LongitudeArrivee = table.Column<double>(type: "REAL", nullable: false),
                    Signal = table.Column<int>(type: "INTEGER", nullable: false),
                    EstOccupe = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrainId = table.Column<int>(type: "INTEGER", nullable: true),
                    VoieId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Voie_VoieId",
                        column: x => x.VoieId,
                        principalTable: "Voie",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Train",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Etat = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacite = table.Column<int>(type: "INTEGER", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Train", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Train_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Train_Station_StationId",
                        column: x => x.StationId,
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Itineraire",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    DateDepart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateArrivee = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TrainId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraire", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itineraire_Train_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Train",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Etape",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lieu = table.Column<string>(type: "TEXT", nullable: false),
                    HeureArrivee = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HeureDepart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ordre = table.Column<int>(type: "INTEGER", nullable: false),
                    ItineraireId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockId = table.Column<int>(type: "INTEGER", nullable: true),
                    TrainId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etape", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Etape_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Etape_Itineraire_ItineraireId",
                        column: x => x.ItineraireId,
                        principalTable: "Itineraire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Etape_Train_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Train",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_TrainId",
                table: "Blocks",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_VoieId",
                table: "Blocks",
                column: "VoieId");

            migrationBuilder.CreateIndex(
                name: "IX_Etape_BlockId",
                table: "Etape",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Etape_ItineraireId",
                table: "Etape",
                column: "ItineraireId");

            migrationBuilder.CreateIndex(
                name: "IX_Etape_TrainId",
                table: "Etape",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraire_TrainId",
                table: "Itineraire",
                column: "TrainId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signau_StationId",
                table: "Signau",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Train_BlockId",
                table: "Train",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Train_StationId",
                table: "Train",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StationId",
                table: "Users",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Voie_StationId",
                table: "Voie",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Train_TrainId",
                table: "Blocks",
                column: "TrainId",
                principalTable: "Train",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Train_TrainId",
                table: "Blocks");

            migrationBuilder.DropTable(
                name: "Etape");

            migrationBuilder.DropTable(
                name: "PointArret");

            migrationBuilder.DropTable(
                name: "Signau");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Itineraire");

            migrationBuilder.DropTable(
                name: "Train");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Voie");

            migrationBuilder.DropTable(
                name: "Station");
        }
    }
}
