using System.ComponentModel.DataAnnotations;

namespace VenueBookingSystem.Models
{
    public class BookingViewModel
    {
        // Booking Information
        public int BookingID { get; set; }

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = string.Empty;

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Booking Created")]
        public DateTime BookingCreatedAt { get; set; }

        // Venue Information
        public int VenueID { get; set; }

        [Display(Name = "Venue Name")]
        public string VenueName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }

        [Display(Name = "Venue Image")]
        public string? ImageUrl { get; set; }

        // Event Information
        public int EventID { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [Display(Name = "Duration (Hours)")]
        public int DurationHours { get; set; }

        [Display(Name = "Organizer")]
        public string OrganizerName { get; set; } = string.Empty;

        // Calculated Properties
        [Display(Name = "Event End Time")]
        public DateTime EventEndTime => EventDate.AddHours(DurationHours);

        [Display(Name = "Display Title")]
        public string DisplayTitle => $"{EventName} @ {VenueName}";

        [Display(Name = "Is Upcoming")]
        public bool IsUpcoming => EventDate > DateTime.Now;

        [Display(Name = "Booking Summary")]
        public string BookingSummary => $"{VenueName} - {EventName} on {BookingDate:yyyy-MM-dd HH:mm}";
    }
}