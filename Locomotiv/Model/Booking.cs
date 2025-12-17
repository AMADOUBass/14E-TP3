using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId {get; set;}
        public User User { get; set; }

        [Required]
        public Itineraire Itineraire { get; set; }
        public int ItineraireId { get; set; }   

        [Required]
        [Range(0 ,10)]

        public int NombrePlaces { get; set; }

        [Required]

        public DateTime DateReservation { get; set; } = DateTime.Now;

        [Required]

        public decimal PrixTotal { get; set; }
        public string Statut { get; set; } = "Confirmé";
        
    }
}
