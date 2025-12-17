using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class TrainDAL : ITrainDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public TrainDAL(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Train> GetAllTrains()
        {
            try
            {
                var trains = _context.Trains.Include(t => t.Station).ToList();
                _logger.Info($"Récupération de {trains.Count} trains.");
                return trains;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la récupération des trains.", ex);
                return Enumerable.Empty<Train>();
            }
        }

        public Train? GetTrainById(int id)
        {
            try
            {
                var train = _context.Trains.Include(t => t.Station).FirstOrDefault(t => t.Id == id);

                if (train == null)
                {
                    _logger.Warning($"Aucun train trouvé avec l’ID {id}.");
                }
                else
                {
                    _logger.Info($"Train récupéré (Id={id}).");
                }

                return train;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la récupération du train Id={id}.", ex);
                return null;
            }
        }

        public void AddTrain(Train train)
        {
            try
            {
                _context.Trains.Add(train);
                _context.SaveChanges();
                _logger.Info($"Train ajouté (Id={train.Id}).");
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de l’ajout du train.", ex);
                throw new AppFriendlyException("Impossible d’ajouter le train pour le moment.");
            }
        }

        public void UpdateTrain(Train train)
        {
            try
            {
                _context.Trains.Update(train);
                _context.SaveChanges();
                _logger.Info($"Train mis à jour (Id={train.Id}).");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la mise à jour du train Id={train.Id}.", ex);
                throw new AppFriendlyException("Impossible de mettre à jour le train pour le moment.");
            }
        }

        public bool DeleteTrain(int id)
        {
            try
            {
                var train = GetTrainById(id);
                if (train != null)
                {
                    _context.Trains.Remove(train);
                    _context.SaveChanges();
                    _logger.Info($"Train supprimé (Id={id}).");
                    return true;
                }

                _logger.Warning($"Tentative de suppression d’un train inexistant (Id={id}).");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la suppression du train Id={id}.", ex);
                throw new AppFriendlyException("Impossible de supprimer le train pour le moment.");
            }
        }

        public IEnumerable<Train> GetTrainsByStationId(int stationId)
        {
            try
            {
                var trains = _context.Trains.Where(t => t.StationId == stationId).ToList();
                _logger.Info($"Récupération de {trains.Count} trains pour la station Id={stationId}.");
                return trains;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors de la récupération des trains pour la station Id={stationId}.", ex);
                return Enumerable.Empty<Train>();
            }
        }
    }
}