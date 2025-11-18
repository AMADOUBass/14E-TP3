using Locomotiv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IPointArretDAL
    {
        List<PointArret> GetAllPointArrets();

    }
}

//public interface IPointArretDAL
//{
//    List<PointArret> GetAllPointArrets();
//    PointArret? GetPointArretById(int id);
//    void AjouterPointArret(PointArret pointArret);
//    void SupprimerPointArret(int id);
//    // autres méthodes spécifiques aux POI
//}