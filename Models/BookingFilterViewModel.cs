namespace VenueBookingSystem.Models
{
    public class BookingFilterViewModel
    {
        // Search and Filter Properties
        public string? SearchTerm { get; set; }
        public int? EventTypeID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? VenueAvailability { get; set; }

        // Filter Options
        public List<EventType> EventTypes { get; set; } = new();
        public List<string> AvailabilityOptions { get; set; } = new()
        {
            "All", "Available", "Maintenance", "Booked", "Limited"
        };

        // Results
        public List<BookingViewModel> Bookings { get; set; } = new();
        public List<Venue> AvailableVenues { get; set; } = new();
        public List<Event> AvailableEvents { get; set; } = new();

        // Statistics
        public int TotalVenues { get; set; }
        public int TotalEvents { get; set; }
        public int TotalBookings { get; set; }
        public int AvailableVenuesCount { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int FilteredResultsCount { get; set; }

        // For display
        public string SelectedEventTypeName { get; set; } = string.Empty;
    }
}