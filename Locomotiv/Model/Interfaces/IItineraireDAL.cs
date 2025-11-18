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
        //void UpdateItineraire(Itineraire itineraire);
        //void DeleteItineraire(int id);

        void PlanifierItineraire(Itineraire itineraire);

        // 🔹 Nouveau : récupérer l’itinéraire actuel d’un train (s’il existe)
        Itineraire? GetItineraireParTrain(int trainId);

        // 🔹 Nouveau : supprimer l’itinéraire d’un train (quand il a terminé)
        void SupprimerItineraireParTrain(int trainId);
    }
}
