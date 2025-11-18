using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface ITrainDAL
    {
        IEnumerable<Train> GetAllTrains();
        Train? GetTrainById(int id);
        void AddTrain(Train train);
        void UpdateTrain(Train train);
        bool DeleteTrain(int id);
        IEnumerable<Train> GetTrainsByStationId(int stationId);

    }
}
