using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
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

    private readonly IConfigurationService _configService;

    public ApplicationDbContext(IConfigurationService configService)
    {
        _configService = configService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = ConfigurationManager
            .ConnectionStrings["LocomotivDb"]?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La chaîne de connexion 'LocomotivDb' est introuvable.");

        var resolvedConnectionString = Environment.ExpandEnvironmentVariables(connectionString);

        optionsBuilder.UseSqlite(resolvedConnectionString);
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
