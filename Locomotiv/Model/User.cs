using System.ComponentModel.DataAnnotations;

public class User

{
    public enum UserRole
    {
        Admin,
        Employe
    }

    [Key]
    public int Id { get; set; } // Clé primaire

    public string Prenom { get; set; }

    public string Nom { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public int? StationId { get; set; }
    public Station? Station { get; set; }


    public User()
    {

    }
}
