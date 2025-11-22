using System.ComponentModel.DataAnnotations;
using Locomotiv.Model.enums;

namespace Locomotiv.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Prenom { get; set; }

        public string Nom { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public int? StationId { get; set; }
        public Station? Station { get; set; }
    }
}
