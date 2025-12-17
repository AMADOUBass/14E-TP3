using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    public class BookingDAL : IBookingDAL
    {
        private readonly ApplicationDbContext _context;

        public BookingDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddBooking(Booking booking)
        {
            _context.Bookings.Add(booking); 
            _context.SaveChanges();
        }

        public bool CancelBooking(int id)
        {
            Booking? booking = GetBookingById(id);
            if(booking != null && booking.Statut == "Confirmé")
            {
                booking.Statut = "Annulé";
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteBooking(int id)
        {
           Booking? booking = GetBookingById(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Booking> GetAllBookings()
        {
            return _context.Bookings
                .Include(b => b.User)

                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Train)

                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Etapes)
                        .ThenInclude(e => e.Station)

                .OrderByDescending(b => b.DateReservation)
                .ToList();
                    

        }

        public Booking? GetBookingById(int id)
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Train)
                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Etapes)
                        .ThenInclude(e => e.Station)
                .FirstOrDefault(b => b.Id == id);

        }

        public IEnumerable<Booking> GetBookingsByItineraireId(int itineraireId)
        {
            return _context.Bookings
               .Include(b => b.User)
               .Where(b => b.ItineraireId == itineraireId)
               .OrderBy(b => b.DateReservation)
               .ToList();
        }

        public IEnumerable<Booking> GetBookingsByUserId(int userId)
        {
            return _context.Bookings
                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Train)
                .Include(b => b.Itineraire)
                    .ThenInclude(i => i.Etapes)
                        .ThenInclude(e => e.Station)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateReservation)
                .ToList();
        }

        public int GetTotalPlacesBookded(int itineraireId)
        {
            return _context.Bookings
                .Where(b => b.ItineraireId == itineraireId && b.Statut == "Confirmé")
                .Sum(b => b.NombrePlaces);
        }

        public bool HasExistingBooking(int userId, int itineraireId)
        {
            return _context.Bookings
                .Any(b => b.UserId == userId && b.ItineraireId == itineraireId && b.Statut == "Confirmé");
        }

        public void UpdateBooking(Booking booking)
        {
            _context.Bookings.Update(booking);
            _context.SaveChanges();
        }

    }
}
