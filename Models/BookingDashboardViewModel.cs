using System.ComponentModel.DataAnnotations;

namespace VenueBookingSystem.Models
{
    public class BookingDashboardViewModel
    {
        // Available items for booking
        public List<Venue> AvailableVenues { get; set; } = new();
        public List<Event> AvailableEvents { get; set; } = new();

        // Existing bookings
        public List<BookingViewModel> ExistingBookings { get; set; } = new();

        // Statistics
        public int TotalVenues { get; set; }
        public int TotalEvents { get; set; }
        public int TotalBookings { get; set; }
        public int AvailableVenuesCount { get; set; }
        public int UpcomingEventsCount { get; set; }

        // Search properties
        public string? SearchTerm { get; set; }
        public int SearchCount { get; set; }
    }
}