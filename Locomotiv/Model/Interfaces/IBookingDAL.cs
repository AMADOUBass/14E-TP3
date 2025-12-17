using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IBookingDAL
    {
        IEnumerable<Booking> GetAllBookings();

        Booking? GetBookingById(int id);

        IEnumerable<Booking> GetBookingsByUserId(int Userid);
        IEnumerable<Booking> GetBookingsByItineraireId(int itineraireId);

        void AddBooking(Booking booking);

        void UpdateBooking(Booking booking);
        bool DeleteBooking(int id);
        bool CancelBooking(int id);

        int GetTotalPlacesBookded(int ItineraireId);

        bool HasExistingBooking(int userId, int itineraireId);
    }
}
