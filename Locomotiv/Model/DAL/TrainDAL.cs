using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class TrainDAL : ITrainDAL
    {
        private readonly ApplicationDbContext _context;

        public TrainDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Train> GetAllTrains()
        {
            return _context.Trains.Include(t => t.Station).ToList();
        }

        public Train? GetTrainById(int id)
        {
            return _context.Trains.Include(t => t.Station).FirstOrDefault(t => t.Id == id);
        }

        public void AddTrain(Train train)
        {
            _context.Trains.Add(train);
            _context.SaveChanges();
        }

        public void UpdateTrain(Train train)
        {
            _context.Trains.Update(train);
            _context.SaveChanges();
        }

        public bool DeleteTrain(int id)
        {
            var train = GetTrainById(id);
            if (train != null)
            {
                _context.Trains.Remove(train);
                _context.SaveChanges();
            }
            return train != null;
        }

        public IEnumerable<Train> GetTrainsByStationId(int stationId)
        {
            return _context.Trains.Where(t => t.StationId == stationId).ToList();
        }
    }
}
