using Locomotiv.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Locomotiv.Model.DAL
{
    public class StationDAL : IStationDAL
    {
        private readonly ApplicationDbContext _context;
        // Implementation for Train Data Access Layer
        
        public StationDAL(ApplicationDbContext context)
        {
            _context = context;
        }

  
        public IEnumerable<Station> GetAllStations()
        {
            return _context.Stations
                .Include (s => s.Employes)
                .Include (s => s.Train)
                .Include (s => s.Voies)
                .Include (s => s.Signaux)
                .ToList();
        }
        public Station? GetStationById(int id)
        {
            return _context.Stations
                .Include(s => s.Train)
                .Include(s => s.Employes)
                .Include(s => s.Voies)
                .Include(s => s.Signaux)
                .FirstOrDefault(s => s.Id == id);
        }

        public List<PointArret> GetAllStationsAsPointArrets()
        {
            return _context.Stations.Select(s => new PointArret
            {
                Id = s.Id,
                Nom = s.Nom,
                Localisation = s.Localisation,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                EstStation = true
            }).ToList();
        }

    }
}
