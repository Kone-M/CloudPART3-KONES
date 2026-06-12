using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueBookingSystem.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event Name is required")]
        [Display(Name = "Event Name")]
        [StringLength(100, ErrorMessage = "Event Name cannot exceed 100 characters")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event Date is required")]
        [Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Display(Name = "Duration (Hours)")]
        [Range(1, 72, ErrorMessage = "Duration must be between 1 and 72 hours")]
        public int DurationHours { get; set; }

        [Required(ErrorMessage = "Organizer Name is required")]
        [Display(Name = "Organizer Name")]
        [StringLength(100, ErrorMessage = "Organizer Name cannot exceed 100 characters")]
        public string OrganizerName { get; set; } = string.Empty;

        // New: Event Type Foreign Key
        [Display(Name = "Event Type")]
        public int? EventTypeID { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("EventTypeID")]
        public virtual EventType? EventType { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}