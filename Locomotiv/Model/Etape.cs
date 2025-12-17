using System.ComponentModel.DataAnnotations;

namespace Locomotiv.Model
{
    public class Etape
    {
        [Key]
        public int Id { get; set; }

        public string Lieu { get; set; }

        public DateTime HeureArrivee { get; set; }

        public DateTime? HeureDepart { get; set; }

        public int Ordre { get; set; }

        public int ItineraireId { get; set; }
        public Itineraire Itineraire { get; set; }

        public int? BlockId { get; set; }
        public Block? Block { get; set; }

        public int TrainId { get; set; }
        public Train Train { get; set; }

        public int? StationId { get; set; }
        public Station? Station { get; set; }
    }
}
