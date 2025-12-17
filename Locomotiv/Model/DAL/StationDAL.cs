using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class StationDAL : IStationDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public StationDAL(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Station> GetAllStations()
        {
            try
            {
                var stations = _context.Stations
                    .Include(s => s.Employes)
                    .Include(s => s.Train)
                    .Include(s => s.Voies)
                    .Include(s => s.Signaux)
                    .ToList();

                _logger.Info($"Récupération de {stations.Count} stations.");
                return stations;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la récupération des stations.", ex);
                return Enumerable.Empty<Station>();
            }
        }

        public Station? GetStationById(int id)
        {
            try
            {
                var station = _context.Stations
                    .Include(s => s.Train)
                    .Include(s => s.Employes)
                    .Include(s => s.Voies)
                    .Include(s => s.Signaux)
                    .FirstOrDefault(s => s.Id == id);

                if (station == null)
                {
                    _logger.Warning($"Aucune station trouvée avec l’ID {id}.");
                }
                else
                {
                    _logger.Info($"Station récupérée (Id={id}).");
                }

                return station;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la récupération de la station Id={id}.", ex);
                return null;
            }
        }

        public List<PointArret> GetAllStationsAsPointArrets()
        {
            try
            {
                var pointArrets = _context.Stations
                    .Select(s => new PointArret
                    {
                        Id = s.Id,
                        Nom = s.Nom,
                        Localisation = s.Localisation,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        EstStation = true,
                    })
                    .ToList();

                _logger.Info($"Conversion de {pointArrets.Count} stations en points d’arrêt.");
                return pointArrets;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la conversion des stations en points d’arrêt.", ex);
                return new List<PointArret>();
            }
        }
    }
}