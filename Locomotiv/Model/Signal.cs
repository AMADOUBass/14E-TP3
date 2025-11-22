using System.ComponentModel.DataAnnotations;

namespace Locomotiv.Model
{
    public class Signal
    {
        public enum TypeSignal
        {
            Arret,
            Passage,
            Danger,
            Maintenance,
            HorsService,
        }

        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public bool EstActif { get; set; }

        public int StationId { get; set; }
        public Station Station { get; set; }
    }
}
