using GMap.NET.Internals;
using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services
{
    public class ItineraireService : IItineraireService
    {
        private readonly IItineraireDAL _itineraireDAL;
        private readonly IBookingDAL _bookingDAL;

        public ItineraireService(IItineraireDAL itineraireDAL, IBookingDAL bookingDAL)
        {
            _itineraireDAL = itineraireDAL;
            _bookingDAL = bookingDAL;
        }

        public IEnumerable<Itineraire> GetItinerairesDisponibles()
        {
            var itineraires = _itineraireDAL.GetAllItineraires();

            return itineraires.Where(i => HasPlacesDisponibles(i.Id));
        }

        public bool HasPlacesDisponibles(int itineraireId)
        {
            return GetPlacesDisponibles(itineraireId) > 0;
        }

        public IEnumerable<Itineraire> GetItinerairesByDate(DateTime date)
        {
            return GetItinerairesDisponibles()
                .Where(i => i.DateDepart.Date == date.Date);
        }


        public int GetPlacesDisponibles(int itineraireId)
        {
            var itineraire = _itineraireDAL.GetItineraireById(itineraireId);

            if (itineraire == null || itineraire.Train == null)
            {
                return 0;
            }

            int capacite = itineraire.Train.Capacite;
            int placesReservees = GetPlacesReservees(itineraireId);

            return Math.Max(0, capacite - placesReservees);
        }

        public IEnumerable<Itineraire> GetItinerairesByDestination(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return GetItinerairesDisponibles();
            }

            return GetItinerairesDisponibles()
                .Where(i => i.Etapes.Any(e => e.Lieu.Contains(destination, StringComparison.OrdinalIgnoreCase)));
        }

        public int GetPlacesReservees(int itineraireId)
        {
            return _bookingDAL.GetTotalPlacesBookded(itineraireId);
        }

        public bool CreerReservation(int itineraireId, int userId, int nombrePlaces, decimal prix)
        {
            int placesDisponibles = GetPlacesDisponibles(itineraireId);
            if (placesDisponibles < nombrePlaces)
            {
                return false;
            }

            if (_bookingDAL.HasExistingBooking(userId, itineraireId))
            {
                return false;
            }

            Booking booking = new Booking
            {
                UserId = userId,
                ItineraireId = itineraireId,
                NombrePlaces = nombrePlaces,
                DateReservation = DateTime.Now,
                PrixTotal = prix * nombrePlaces,
                Statut = "Confirmé"
            };
            try
            {
                _bookingDAL.AddBooking(booking);
                return true;
            }
            catch
            {

                return false;
            }
        }

        public IEnumerable<Booking> GetReservationsUtilisateur(int userId)
        {
            return _bookingDAL.GetBookingsByUserId(userId);
        }

        public bool AnnulerReservation(int bookingId)
        {
            return _bookingDAL.CancelBooking(bookingId);
        }
    }
}
