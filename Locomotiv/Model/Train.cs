using System.ComponentModel.DataAnnotations;

public enum EtatTrain
{
    EnGare,
    EnTransit,
    EnAttente,
    HorsService,
    Programme
}

public class Train
{
    [Key]
    public int Id { get; set; }

    public string Nom { get; set; }

    public EtatTrain Etat { get; set; }

    public int Capacite { get; set; }

    public int? StationId { get; set; }
    public Station Station { get; set; }

    public int? BlockId { get; set; } // ← clé étrangère vers Block
    public Block? Block { get; set; } // ← navigation vers Block

    public Itineraire Itineraire { get; set; }

    public ICollection<Etape> Etapes { get; set; } = new List<Etape>();

}