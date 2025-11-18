using System.ComponentModel.DataAnnotations;

public class Signal
{
    public enum TypeSignal
    {
        Arret,
        Passage,
        Danger,
        Maintenance,
        HorsService
    }

    [Key]
    public int Id { get; set; }

    public string Type { get; set; } // Ex: "Arrêt", "Passage", "Danger"

    public bool EstActif { get; set; }

    public int StationId { get; set; }
    public Station Station { get; set; }
}