using GMap.NET;
using System.ComponentModel.DataAnnotations;

public class Voie
{
    [Key]
    public int Id { get; set; }

    public string Nom { get; set; }

    public int StationId { get; set; }
    public Station Station { get; set; }
    public List<Block> Blocks { get; set; } = new();
}


