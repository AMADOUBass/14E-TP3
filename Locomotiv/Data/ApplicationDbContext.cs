using Locomotiv.Model;
using Microsoft.EntityFrameworkCore;
using System.IO;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Train> Trains { get; set; }
    public DbSet<Itineraire> Itineraires { get; set; }
    public DbSet<Signal> Signaux { get; set; }
    public DbSet<Voie> Voies { get; set; }
    public DbSet<Etape> Etapes { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<PointArret> PointArrets { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Locomotiv",
            "Locomotiv.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        var connectionString = $"Data Source={dbPath}";

        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
