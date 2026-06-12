using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;
using VenueBookingSystem.Services;

namespace VenueBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ValidationService _validationService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            ApplicationDbContext context,
            ValidationService validationService,
            ILogger<BookingsController> logger)
        {
            _context = context;
            _validationService = validationService;
            _logger = logger;
        }

        // GET: Bookings - Enhanced with advanced filtering
        public async Task<IActionResult> Index(string searchTerm, int? eventTypeId, DateTime? startDate, DateTime? endDate, string venueAvailability)
        {
            var viewModel = new BookingFilterViewModel();

            // Store filter values for the view
            viewModel.SearchTerm = searchTerm;
            viewModel.EventTypeID = eventTypeId;
            viewModel.StartDate = startDate;
            viewModel.EndDate = endDate;
            viewModel.VenueAvailability = venueAvailability;

            // Get event types for filter dropdown
            viewModel.EventTypes = await _context.EventTypes
                .Where(et => et.IsActive)
                .OrderBy(et => et.DisplayOrder)
                .ToListAsync();

            // 1. Get available venues with availability filtering
            var venuesQuery = _context.Venues.Where(v => v.IsActive);

            if (!string.IsNullOrEmpty(venueAvailability) && venueAvailability != "All")
            {
                venuesQuery = venuesQuery.Where(v => v.AvailabilityStatus == venueAvailability);
            }

            viewModel.AvailableVenues = await venuesQuery
                .OrderBy(v => v.VenueName)
                .ToListAsync();

            // 2. Get available events with event type and date filtering
            var eventsQuery = _context.Events
                .Include(e => e.EventType)
                .AsQueryable();

            if (eventTypeId.HasValue && eventTypeId.Value > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.EventTypeID == eventTypeId.Value);
                var selectedType = viewModel.EventTypes.FirstOrDefault(et => et.EventTypeID == eventTypeId.Value);
                viewModel.SelectedEventTypeName = selectedType?.CategoryName ?? "";
            }

            if (startDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate <= endDate.Value);
            }

            viewModel.AvailableEvents = await eventsQuery
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            // 3. Get existing bookings with search and filters
            var bookings = _context.vw_EnhancedBookings.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                bookings = bookings.Where(b =>
                    b.BookingID.ToString().Contains(searchTermLower) ||
                    b.EventName.ToLower().Contains(searchTermLower) ||
                    b.VenueName.ToLower().Contains(searchTermLower) ||
                    b.OrganizerName.ToLower().Contains(searchTermLower)
                );
            }

            // Apply event type filter to bookings
            if (eventTypeId.HasValue && eventTypeId.Value > 0)
            {
                var eventIds = await _context.Events
                    .Where(e => e.EventTypeID == eventTypeId.Value)
                    .Select(e => e.EventID)
                    .ToListAsync();

                bookings = bookings.Where(b => eventIds.Contains(b.EventID));
            }

            // Apply date filters to bookings
            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.EventDate <= endDate.Value);
            }

            viewModel.Bookings = await bookings
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            viewModel.FilteredResultsCount = viewModel.Bookings.Count;

            // 4. Get statistics
            viewModel.TotalVenues = await _context.Venues.CountAsync();
            viewModel.TotalEvents = await _context.Events.CountAsync();
            viewModel.TotalBookings = await _context.Bookings.CountAsync();
            viewModel.AvailableVenuesCount = viewModel.AvailableVenues.Count;
            viewModel.UpcomingEventsCount = await _context.Events
                .CountAsync(e => e.EventDate > DateTime.Now);

            if (!string.IsNullOrEmpty(searchTerm) && viewModel.FilteredResultsCount == 0)
            {
                TempData["InfoMessage"] = $"No bookings found matching your filters.";
            }

            return View(viewModel);
        }

        // GET: Bookings/Create with optional pre-selected IDs
        public async Task<IActionResult> Create(int? venueId, int? eventId)
        {
            ViewBag.Venues = await _context.Venues
                .Where(v => v.IsActive)
                .OrderBy(v => v.VenueName)
                .ToListAsync();

            ViewBag.Events = await _context.Events
                .Include(e => e.EventType)
                .OrderBy(e => e.EventName)
                .ToListAsync();

            var booking = new Booking();
            if (venueId.HasValue && venueId.Value > 0)
            {
                booking.VenueID = venueId.Value;
                var venue = await _context.Venues.FindAsync(venueId.Value);
                if (venue != null)
                {
                    TempData["InfoMessage"] = $"Creating booking for venue: {venue.VenueName}";
                }
            }

            if (eventId.HasValue && eventId.Value > 0)
            {
                booking.EventID = eventId.Value;
                var evnt = await _context.Events.FindAsync(eventId.Value);
                if (evnt != null)
                {
                    TempData["InfoMessage"] = $"Creating booking for event: {evnt.EventName}";
                }
            }

            // Set default booking date to current time + 1 hour
            booking.BookingDate = DateTime.Now.AddHours(1);

            return View(booking);
        }

        // POST: Bookings/Create with double-booking validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,EventID,BookingDate,SpecialRequests")] Booking booking)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            // Validate booking date
            if (!_validationService.IsBookingDateValid(booking.BookingDate))
            {
                ModelState.AddModelError("BookingDate", _validationService.GetDateValidationMessage(booking.BookingDate));
            }

            if (ModelState.IsValid)
            {
                // Check for double booking
                var isAvailable = await _validationService.IsVenueAvailableAsync(booking.VenueID, booking.BookingDate);

                if (!isAvailable)
                {
                    var conflictMessage = await _validationService.GetConflictMessageAsync(booking.VenueID, booking.BookingDate);
                    ModelState.AddModelError("BookingDate", conflictMessage);
                    TempData["ErrorMessage"] = conflictMessage;

                    // Re-populate dropdowns
                    ViewBag.Venues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
                    ViewBag.Events = await _context.Events.ToListAsync();
                    return View(booking);
                }

                booking.CreatedAt = DateTime.Now;
                booking.Status = "Confirmed";

                _context.Add(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Booking created: ID {booking.BookingID} for Venue {booking.VenueID}");
                TempData["SuccessMessage"] = $"✅ Booking created successfully! Booking ID: {booking.BookingID}";
                return RedirectToAction(nameof(Index));
            }

            // Handle model errors
            ViewBag.Venues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    TempData["ErrorMessage"] = error.ErrorMessage;
                }
            }

            return View(booking);
        }

        // GET: Bookings/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.vw_EnhancedBookings
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete - Cancel booking instead of hard delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Booking {id} cancelled");
                TempData["SuccessMessage"] = "Booking cancelled successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.vw_EnhancedBookings
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}