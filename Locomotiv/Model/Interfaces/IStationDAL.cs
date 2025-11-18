using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IStationDAL
    {

        IEnumerable<Station> GetAllStations();
        Station? GetStationById(int id);
        List<PointArret> GetAllStationsAsPointArrets();


    }
}
