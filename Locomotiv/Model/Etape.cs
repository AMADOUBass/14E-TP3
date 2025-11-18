using System.ComponentModel.DataAnnotations;

public class Etape
{
    [Key]
    public int Id { get; set; }

    public string Lieu { get; set; }

    public DateTime HeureArrivee { get; set; }

    public DateTime HeureDepart { get; set; }

    public int Ordre { get; set; }

    public int ItineraireId { get; set; }
    public Itineraire Itineraire { get; set; }

    public int? BlockId { get; set; }       // ← clé étrangère vers Block
    public Block? Block { get; set; }       // ← navigation vers Block

    public int TrainId { get; set; }           // 🔹 Clé étrangère vers Train
    public Train Train { get; set; }           // 🔹 Navigation vers Train
}
