using System.ComponentModel.DataAnnotations;
using Locomotiv.Model.enums;

namespace Locomotiv.Model
{
    public class Block
    {
        [Key]
        public int Id { get; set; }

        public string Nom { get; set; }

        public double LatitudeDepart { get; set; } = 0;
        public double LongitudeDepart { get; set; } = 0;

        public double LatitudeArrivee { get; set; } = 0;
        public double LongitudeArrivee { get; set; } = 0;

        public SignalType Signal { get; set; }

        public bool EstOccupe { get; set; }

        public int? TrainId { get; set; }
        public Train? Train { get; set; }
    }
}
