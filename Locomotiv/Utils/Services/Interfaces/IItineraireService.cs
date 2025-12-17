using Locomotiv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IItineraireService
    {
        IEnumerable<Itineraire> GetItinerairesDisponibles();

        IEnumerable<Itineraire> GetItinerairesByDate(DateTime date);

        IEnumerable<Itineraire> GetItinerairesByDestination(string destination);

        int GetPlacesReservees(int itineraireId);

        int GetPlacesDisponibles(int itineraireId);

        bool HasPlacesDisponibles(int itineraireId);

        bool CreerReservation(int itineraireId, int userId, int nombrePlaces, decimal prix);

        IEnumerable<Booking> GetReservationsUtilisateur(int userId);

        bool AnnulerReservation(int bookingId);
    }
}
