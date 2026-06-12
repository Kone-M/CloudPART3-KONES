using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueBookingSystem.Models
{
    public class Venue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VenueID { get; set; }

        [Required(ErrorMessage = "Venue Name is required")]
        [Display(Name = "Venue Name")]
        [StringLength(100, ErrorMessage = "Venue Name cannot exceed 100 characters")]
        public string VenueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
        public int Capacity { get; set; }

        [Display(Name = "Venue Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // New: Availability Status
        [Display(Name = "Availability Status")]
        public string AvailabilityStatus { get; set; } = "Available"; // Available, Maintenance, Booked, Limited

        [Display(Name = "Last Maintenance Date")]
        [DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Next Available Date")]
        [DataType(DataType.DateTime)]
        public DateTime? NextAvailableDate { get; set; }

        [Display(Name = "Operating Hours")]
        public string? OperatingHours { get; set; } // e.g., "Mon-Fri 9AM-9PM, Sat-Sun 10AM-6PM"

        // Navigation property
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}