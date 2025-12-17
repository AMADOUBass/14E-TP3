using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class ItineraireDAL : IItineraireDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ItineraireDAL(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PlanifierItineraire(Itineraire itineraire)
        {
            try
            {
                _context.Itineraires.Add(itineraire);
                await _context.SaveChangesAsync();

                _logger.Info($"Itinéraire planifié (Id={itineraire.Id})");
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la planification de l'itinéraire.", ex);
                throw new AppFriendlyException("Impossible de planifier l’itinéraire pour le moment.");
            }
        }

        public List<Itineraire> GetAllItineraires()
        {
            try
            {
                var itineraires = _context.Itineraires
                    .Include(i => i.Etapes)
                    .ThenInclude(e => e.Train)
                    .ToList();

                _logger.Info($"Récupération de {itineraires.Count} itinéraires.");
                return itineraires;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la récupération des itinéraires.", ex);
                return new List<Itineraire>();
            }
        }

        public Itineraire? GetItineraireById(int id)
        {
            try
            {
                var itineraire = _context.Itineraires
                    .Include(i => i.Etapes)
                    .ThenInclude(e => e.Train)
                    .FirstOrDefault(i => i.Id == id);

                if (itineraire == null)
                {
                    _logger.Warning($"Aucun itinéraire trouvé avec l’ID {id}.");
                }
                else
                {
                    _logger.Info($"Itinéraire récupéré (Id={id}).");
                }

                return itineraire;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la récupération de l’itinéraire Id={id}.", ex);
                return null;
            }
        }

        public async Task AddItineraire(Itineraire itineraire)
        {
            try
            {
                _context.Itineraires.Add(itineraire);
                await _context.SaveChangesAsync();

                _logger.Info($"Itinéraire ajouté (Id={itineraire.Id}).");
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de l’ajout de l’itinéraire.", ex);
                throw new AppFriendlyException("Impossible d’ajouter l’itinéraire pour le moment.");
            }
        }

        public Itineraire? GetItineraireParTrain(int trainId)
        {
            try
            {
                var itineraire = _context.Itineraires
                    .Include(i => i.Train)
                    .Include(i => i.Etapes)
                    .FirstOrDefault(i => i.TrainId == trainId);

                if (itineraire == null)
                {
                    _logger.Warning($"Aucun itinéraire trouvé pour le train ID {trainId}.");
                }
                else
                {
                    _logger.Info($"Itinéraire récupéré pour le train ID {trainId}.");
                }

                return itineraire;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la récupération de l’itinéraire pour le train ID {trainId}.", ex);
                return null;
            }
        }

        public async Task SupprimerItineraireParTrain(int trainId)
        {
            try
            {
                var itineraire = GetItineraireParTrain(trainId);
                if (itineraire == null)
                {
                    _logger.Warning($"Tentative de suppression d’un itinéraire inexistant pour le train ID {trainId}.");
                    return;
                }

                _context.Etapes.RemoveRange(itineraire.Etapes);
                _context.Itineraires.Remove(itineraire);

                await _context.SaveChangesAsync();

                _logger.Info($"Itinéraire supprimé pour le train ID {trainId}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la suppression de l’itinéraire pour le train ID {trainId}.", ex);
                throw new AppFriendlyException("Impossible de supprimer l’itinéraire pour le moment.");
            }
        }
    }
}