using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Locomotiv.Migrations
{
    public partial class AjoutDuSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Train_TrainId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Voie_VoieId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Etape_Blocks_BlockId",
                table: "Etape");

            migrationBuilder.DropForeignKey(
                name: "FK_Etape_Itineraire_ItineraireId",
                table: "Etape");

            migrationBuilder.DropForeignKey(
                name: "FK_Etape_Train_TrainId",
                table: "Etape");

            migrationBuilder.DropForeignKey(
                name: "FK_Itineraire_Train_TrainId",
                table: "Itineraire");

            migrationBuilder.DropForeignKey(
                name: "FK_Signau_Station_StationId",
                table: "Signau");

            migrationBuilder.DropForeignKey(
                name: "FK_Train_Blocks_BlockId",
                table: "Train");

            migrationBuilder.DropForeignKey(
                name: "FK_Train_Station_StationId",
                table: "Train");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Station_StationId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Voie_Station_StationId",
                table: "Voie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Voie",
                table: "Voie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Train",
                table: "Train");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Station",
                table: "Station");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signau",
                table: "Signau");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PointArret",
                table: "PointArret");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Itineraire",
                table: "Itineraire");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Etape",
                table: "Etape");

            migrationBuilder.RenameTable(
                name: "Voie",
                newName: "Voies");

            migrationBuilder.RenameTable(
                name: "Train",
                newName: "Trains");

            migrationBuilder.RenameTable(
                name: "Station",
                newName: "Stations");

            migrationBuilder.RenameTable(
                name: "Signau",
                newName: "Signaux");

            migrationBuilder.RenameTable(
                name: "PointArret",
                newName: "PointArrets");

            migrationBuilder.RenameTable(
                name: "Itineraire",
                newName: "Itineraires");

            migrationBuilder.RenameTable(
                name: "Etape",
                newName: "Etapes");

            migrationBuilder.RenameIndex(
                name: "IX_Voie_StationId",
                table: "Voies",
                newName: "IX_Voies_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Train_StationId",
                table: "Trains",
                newName: "IX_Trains_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Train_BlockId",
                table: "Trains",
                newName: "IX_Trains_BlockId");

            migrationBuilder.RenameIndex(
                name: "IX_Signau_StationId",
                table: "Signaux",
                newName: "IX_Signaux_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Itineraire_TrainId",
                table: "Itineraires",
                newName: "IX_Itineraires_TrainId");

            migrationBuilder.RenameIndex(
                name: "IX_Etape_TrainId",
                table: "Etapes",
                newName: "IX_Etapes_TrainId");

            migrationBuilder.RenameIndex(
                name: "IX_Etape_ItineraireId",
                table: "Etapes",
                newName: "IX_Etapes_ItineraireId");

            migrationBuilder.RenameIndex(
                name: "IX_Etape_BlockId",
                table: "Etapes",
                newName: "IX_Etapes_BlockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Voies",
                table: "Voies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trains",
                table: "Trains",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stations",
                table: "Stations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signaux",
                table: "Signaux",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointArrets",
                table: "PointArrets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Itineraires",
                table: "Itineraires",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Etapes",
                table: "Etapes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Trains_TrainId",
                table: "Blocks",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Voies_VoieId",
                table: "Blocks",
                column: "VoieId",
                principalTable: "Voies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Etapes_Blocks_BlockId",
                table: "Etapes",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Etapes_Itineraires_ItineraireId",
                table: "Etapes",
                column: "ItineraireId",
                principalTable: "Itineraires",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Etapes_Trains_TrainId",
                table: "Etapes",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itineraires_Trains_TrainId",
                table: "Itineraires",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signaux_Stations_StationId",
                table: "Signaux",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Blocks_BlockId",
                table: "Trains",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Stations_StationId",
                table: "Trains",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stations_StationId",
                table: "Users",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Voies_Stations_StationId",
                table: "Voies",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Trains_TrainId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Voies_VoieId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Etapes_Blocks_BlockId",
                table: "Etapes");

            migrationBuilder.DropForeignKey(
                name: "FK_Etapes_Itineraires_ItineraireId",
                table: "Etapes");

            migrationBuilder.DropForeignKey(
                name: "FK_Etapes_Trains_TrainId",
                table: "Etapes");

            migrationBuilder.DropForeignKey(
                name: "FK_Itineraires_Trains_TrainId",
                table: "Itineraires");

            migrationBuilder.DropForeignKey(
                name: "FK_Signaux_Stations_StationId",
                table: "Signaux");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Blocks_BlockId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Stations_StationId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stations_StationId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Voies_Stations_StationId",
                table: "Voies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Voies",
                table: "Voies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trains",
                table: "Trains");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stations",
                table: "Stations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signaux",
                table: "Signaux");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PointArrets",
                table: "PointArrets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Itineraires",
                table: "Itineraires");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Etapes",
                table: "Etapes");

            migrationBuilder.RenameTable(
                name: "Voies",
                newName: "Voie");

            migrationBuilder.RenameTable(
                name: "Trains",
                newName: "Train");

            migrationBuilder.RenameTable(
                name: "Stations",
                newName: "Station");

            migrationBuilder.RenameTable(
                name: "Signaux",
                newName: "Signau");

            migrationBuilder.RenameTable(
                name: "PointArrets",
                newName: "PointArret");

            migrationBuilder.RenameTable(
                name: "Itineraires",
                newName: "Itineraire");

            migrationBuilder.RenameTable(
                name: "Etapes",
                newName: "Etape");

            migrationBuilder.RenameIndex(
                name: "IX_Voies_StationId",
                table: "Voie",
                newName: "IX_Voie_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Trains_StationId",
                table: "Train",
                newName: "IX_Train_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Trains_BlockId",
                table: "Train",
                newName: "IX_Train_BlockId");

            migrationBuilder.RenameIndex(
                name: "IX_Signaux_StationId",
                table: "Signau",
                newName: "IX_Signau_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_Itineraires_TrainId",
                table: "Itineraire",
                newName: "IX_Itineraire_TrainId");

            migrationBuilder.RenameIndex(
                name: "IX_Etapes_TrainId",
                table: "Etape",
                newName: "IX_Etape_TrainId");

            migrationBuilder.RenameIndex(
                name: "IX_Etapes_ItineraireId",
                table: "Etape",
                newName: "IX_Etape_ItineraireId");

            migrationBuilder.RenameIndex(
                name: "IX_Etapes_BlockId",
                table: "Etape",
                newName: "IX_Etape_BlockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Voie",
                table: "Voie",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Train",
                table: "Train",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Station",
                table: "Station",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signau",
                table: "Signau",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointArret",
                table: "PointArret",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Itineraire",
                table: "Itineraire",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Etape",
                table: "Etape",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Train_TrainId",
                table: "Blocks",
                column: "TrainId",
                principalTable: "Train",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Voie_VoieId",
                table: "Blocks",
                column: "VoieId",
                principalTable: "Voie",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Etape_Blocks_BlockId",
                table: "Etape",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Etape_Itineraire_ItineraireId",
                table: "Etape",
                column: "ItineraireId",
                principalTable: "Itineraire",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Etape_Train_TrainId",
                table: "Etape",
                column: "TrainId",
                principalTable: "Train",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itineraire_Train_TrainId",
                table: "Itineraire",
                column: "TrainId",
                principalTable: "Train",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signau_Station_StationId",
                table: "Signau",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Train_Blocks_BlockId",
                table: "Train",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Train_Station_StationId",
                table: "Train",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Station_StationId",
                table: "Users",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Voie_Station_StationId",
                table: "Voie",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
