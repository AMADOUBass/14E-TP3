using System.ComponentModel.DataAnnotations;
using Locomotiv.Model.enums;

namespace Locomotiv.Model
{
    public class Train
    {
        [Key]
        public int Id { get; set; }

        public string Nom { get; set; }

        public EtatTrain Etat { get; set; }

        public int Capacite { get; set; }

        public int? StationId { get; set; }
        public Station Station { get; set; }

        public int? BlockId { get; set; }
        public Block? Block { get; set; }

        public Itineraire Itineraire { get; set; }

        public ICollection<Etape> Etapes { get; set; } = new List<Etape>();
    }
}
