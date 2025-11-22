using System.ComponentModel.DataAnnotations;
using Locomotiv.Model.enums;

namespace Locomotiv.Model
{
    public class Station
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [StringLength(200)]
        public string? Localisation { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(1, int.MaxValue)]
        public int CapaciteMaxTrains { get; set; }

        public ICollection<User> Employes { get; set; } = new List<User>();

        public ICollection<Train> Train { get; set; } = new List<Train>();

        public ICollection<Voie> Voies { get; set; } = new List<Voie>();

        public ICollection<Signal> Signaux { get; set; } = new List<Signal>();

        public string StationType { get; set; } = StationTypeEnum.Connexion.ToString();
    }
}
