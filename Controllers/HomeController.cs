using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;

namespace VenueBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var venueCount = await _context.Venues.CountAsync();
            var eventCount = await _context.Events.CountAsync();
            var bookingCount = await _context.Bookings.CountAsync();
            var eventTypeCount = await _context.EventTypes.CountAsync();
            var availableVenuesCount = await _context.Venues.CountAsync(v => v.AvailabilityStatus == "Available");
            var cancelledBookingsCount = await _context.Bookings.CountAsync(b => b.Status == "Cancelled");
            var upcomingEventsCount = await _context.Events.CountAsync(e => e.EventDate > DateTime.Now);
            var confirmedBookingsCount = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");
            var completedBookingsCount = await _context.Bookings.CountAsync(b => b.Status == "Completed");

            return Json(new
            {
                venueCount,
                eventCount,
                bookingCount,
                eventTypeCount,
                availableVenuesCount,
                cancelledBookingsCount,
                upcomingEventsCount,
                confirmedBookingsCount,
                completedBookingsCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypes()
        {
            var eventTypes = await _context.EventTypes
                .Where(et => et.IsActive)
                .OrderBy(et => et.DisplayOrder)
                .Select(et => new { et.EventTypeID, et.CategoryName })
                .ToListAsync();

            return Json(eventTypes);
        }

        [HttpGet]
        public async Task<IActionResult> GetVenuesByStatus()
        {
            var venuesByStatus = await _context.Venues
                .GroupBy(v => v.AvailabilityStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(venuesByStatus);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsByStatus()
        {
            var bookingsByStatus = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(bookingsByStatus);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentBookings()
        {
            var recentBookings = await _context.vw_EnhancedBookings
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .Select(b => new
                {
                    b.BookingID,
                    b.EventName,
                    b.VenueName,
                    b.BookingDate,
                    b.EventDate,
                    b.Status,
                    b.DisplayTitle
                })
                .ToListAsync();

            return Json(recentBookings);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopVenues()
        {
            var topVenues = await _context.Bookings
                .GroupBy(b => b.VenueID)
                .Select(g => new
                {
                    VenueID = g.Key,
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .Join(_context.Venues, x => x.VenueID, v => v.VenueID, (x, v) => new
                {
                    v.VenueName,
                    x.BookingCount
                })
                .ToListAsync();

            return Json(topVenues);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyBookingStats()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyStats = await _context.Bookings
                .Where(b => b.BookingDate.Year == currentYear)
                .GroupBy(b => b.BookingDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Json(monthlyStats);
        }

        [HttpGet]
        public async Task<IActionResult> SearchBookingsQuick(string searchTerm, int? eventTypeId, DateTime? startDate, DateTime? endDate, string venueAvailability)
        {
            var query = _context.vw_EnhancedBookings.AsQueryable();

            // Apply search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.BookingID.ToString().Contains(searchTerm) ||
                    b.EventName.ToLower().Contains(searchTerm) ||
                    b.VenueName.ToLower().Contains(searchTerm) ||
                    b.OrganizerName.ToLower().Contains(searchTerm)
                );
            }

            // Apply event type filter
            if (eventTypeId.HasValue && eventTypeId.Value > 0)
            {
                var eventIds = await _context.Events
                    .Where(e => e.EventTypeID == eventTypeId.Value)
                    .Select(e => e.EventID)
                    .ToListAsync();

                query = query.Where(b => eventIds.Contains(b.EventID));
            }

            // Apply date filters
            if (startDate.HasValue)
            {
                query = query.Where(b => b.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.EventDate <= endDate.Value);
            }

            // Apply venue availability filter
            if (!string.IsNullOrEmpty(venueAvailability) && venueAvailability != "All")
            {
                var venueIds = await _context.Venues
                    .Where(v => v.AvailabilityStatus == venueAvailability)
                    .Select(v => v.VenueID)
                    .ToListAsync();

                query = query.Where(b => venueIds.Contains(b.VenueID));
            }

            var results = await query
                .OrderByDescending(b => b.BookingDate)
                .Take(20)
                .Select(b => new
                {
                    b.BookingID,
                    b.EventName,
                    b.VenueName,
                    b.BookingDate,
                    b.EventDate,
                    b.Status,
                    b.DisplayTitle
                })
                .ToListAsync();

            return Json(results);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}