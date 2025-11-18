using System.ComponentModel.DataAnnotations;
using GMap.NET;

public class Block
{
    public enum SignalType
    {
        Vert,
        Rouge,
        Jaune
    }
    [Key]
    public int Id { get; set; }

    public string Nom { get; set; }

    // Coordonnées géographiques
    public double LatitudeDepart { get; set; } = 0;
    public double LongitudeDepart { get; set; } = 0;

    public double LatitudeArrivee { get; set; } = 0;
    public double LongitudeArrivee { get; set; } = 0;




    // Signal associé
    public SignalType Signal { get; set; }

    // Occupation
    public bool EstOccupe { get; set; }


    public int? TrainId { get; set; }
    public Train? Train { get; set; }
}