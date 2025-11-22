using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IItineraireDAL
    {
        List<Itineraire> GetAllItineraires();
        Itineraire? GetItineraireById(int id);
        void AddItineraire(Itineraire itineraire);

        void PlanifierItineraire(Itineraire itineraire);

        Itineraire? GetItineraireParTrain(int trainId);

        void SupprimerItineraireParTrain(int trainId);
    }
}
