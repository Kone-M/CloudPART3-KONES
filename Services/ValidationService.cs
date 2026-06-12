using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;

namespace VenueBookingSystem.Services
{
    public class ValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ApplicationDbContext context, ILogger<ValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Validate venue availability for booking (prevent double booking)
        public async Task<bool> IsVenueAvailableAsync(int venueId, DateTime bookingDateTime, int? excludeBookingId = null)
        {
            var query = _context.Bookings
                .Where(b => b.VenueID == venueId &&
                       b.BookingDate.Date == bookingDateTime.Date &&
                       b.BookingDate.Hour == bookingDateTime.Hour &&
                       b.Status != "Cancelled");

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.BookingID != excludeBookingId.Value);
            }

            var isAvailable = !await query.AnyAsync();

            if (!isAvailable)
            {
                _logger.LogWarning($"Double booking attempt for venue {venueId} at {bookingDateTime}");
            }

            return isAvailable;
        }

        // Validate venue deletion - check for active bookings
        public async Task<bool> CanDeleteVenueAsync(int venueId)
        {
            var activeBookings = await _context.Bookings
                .AnyAsync(b => b.VenueID == venueId &&
                         b.BookingDate >= DateTime.Now &&
                         b.Status == "Confirmed");

            if (activeBookings)
            {
                _logger.LogWarning($"Attempted to delete venue {venueId} with active bookings");
            }

            return !activeBookings;
        }

        // Validate event deletion - check for active bookings
        public async Task<bool> CanDeleteEventAsync(int eventId)
        {
            var activeBookings = await _context.Bookings
                .AnyAsync(b => b.EventID == eventId &&
                         b.BookingDate >= DateTime.Now &&
                         b.Status == "Confirmed");

            if (activeBookings)
            {
                _logger.LogWarning($"Attempted to delete event {eventId} with active bookings");
            }

            return !activeBookings;
        }

        // Get detailed conflict message for double booking
        public async Task<string> GetConflictMessageAsync(int venueId, DateTime bookingDateTime)
        {
            var conflictingBooking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.VenueID == venueId &&
                                      b.BookingDate.Date == bookingDateTime.Date &&
                                      b.BookingDate.Hour == bookingDateTime.Hour &&
                                      b.Status != "Cancelled");

            if (conflictingBooking != null)
            {
                return $"❌ Double Booking Detected!\n\n" +
                       $"Venue '{conflictingBooking.Venue?.VenueName}' is already booked for " +
                       $"'{conflictingBooking.Event?.EventName}' at " +
                       $"{conflictingBooking.BookingDate:yyyy-MM-dd HH:mm}.\n\n" +
                       $"Please select a different date or time for your booking.";
            }

            return "This venue is already booked at the selected time.";
        }

        // Validate booking dates
        public bool IsBookingDateValid(DateTime bookingDate)
        {
            // Cannot book in the past
            if (bookingDate < DateTime.Now)
            {
                return false;
            }

            // Cannot book more than 1 year in advance
            if (bookingDate > DateTime.Now.AddYears(1))
            {
                return false;
            }

            return true;
        }

        // Get validation error message for date
        public string GetDateValidationMessage(DateTime bookingDate)
        {
            if (bookingDate < DateTime.Now)
            {
                return "Cannot make a booking for a past date and time.";
            }

            if (bookingDate > DateTime.Now.AddYears(1))
            {
                return "Cannot make a booking more than 1 year in advance.";
            }

            return string.Empty;
        }
    }
}