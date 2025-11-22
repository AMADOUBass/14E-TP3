using System.ComponentModel.DataAnnotations;

namespace Locomotiv.Model
{
    public class Itineraire
    {
        [Key]
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public DateTime DateDepart { get; set; }
        public DateTime DateArrivee { get; set; }

        public ICollection<Etape> Etapes { get; set; } = new List<Etape>();

        public int TrainId { get; set; }
        public Train Train { get; set; }
    }
}
