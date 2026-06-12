using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueBookingSystem.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingID { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [Display(Name = "Venue")]
        public int VenueID { get; set; }

        [Required(ErrorMessage = "Event is required")]
        [Display(Name = "Event")]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Booking Date and Time is required")]
        [Display(Name = "Booking Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Confirmed";

        [Display(Name = "Special Requests")]
        [StringLength(500, ErrorMessage = "Special requests cannot exceed 500 characters")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VenueID")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }
    }
}