
using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;


namespace Locomotiv.Model.DAL
{
    public class ItineraireDAL : IItineraireDAL
    {
        private readonly ApplicationDbContext _context;

        // Implementation for Train Data Access Layer
        
        public ItineraireDAL(ApplicationDbContext context)
        {
            _context = context;
        }
        public void PlanifierItineraire(Itineraire itineraire)
        {
            _context.Itineraires.Add(itineraire);
            _context.SaveChangesAsync();
        }
        public List<Itineraire> GetAllItineraires()
        {
            return _context.Itineraires
                           .Include(i => i.Etapes)
                           .ThenInclude(e => e.Train)
                           .ToList();
        }

        public Itineraire? GetItineraireById(int id)
        {
            return _context.Itineraires
                           .Include(i => i.Etapes)
                           .ThenInclude(e => e.Train)
                           .FirstOrDefault(i => i.Id == id);
        }

        public void AddItineraire(Itineraire itineraire)
        {
            _context.Itineraires.Add(itineraire);
            _context.SaveChangesAsync();
        }

        public Itineraire? GetItineraireParTrain(int trainId)
        {
            return _context.Itineraires.Include(i => i.Train)
                                          .Include(i => i.Etapes)
                                          .FirstOrDefault(i => i.TrainId == trainId);
        }
        public void SupprimerItineraireParTrain(int trainId)
        {
            var itineraire = GetItineraireParTrain(trainId);
            if (itineraire != null)
            {
                _context.Etapes.RemoveRange(itineraire.Etapes);
                _context.Itineraires.Remove(itineraire);
                _context.SaveChangesAsync();
            }
        }
    }
}