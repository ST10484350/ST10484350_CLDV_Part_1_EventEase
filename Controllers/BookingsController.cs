using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10484350_CLDV_Part_1_EventEase.Data;
using ST10484350_CLDV_Part_1_EventEase.Models;

namespace ST10484350_CLDV_Part_1_EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        // I implemented this search function to meet the requirement for filtering via Booking ID or Event Name.
        // I used LINQ to Query the database asynchronously for better performance (Microsoft, 2024).
        public async Task<IActionResult> Index(string searchString)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // This logic allows for a broad search. If it's a number, it checks IDs; otherwise, it checks names.
                bookingsQuery = bookingsQuery.Where(s =>
                    s.Event!.Name.Contains(searchString) ||
                    s.BookingId.ToString() == searchString);
            }

            return View(await bookingsQuery.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,CustomerName,BookingDate")] Booking booking)
        {
            // This is a critical business rule I added to prevent double booking of a venue on the same date.
            // This ensures data integrity within the scheduling system (Sarka, 2023).
            if (await HasVenueConflict(booking))
            {
                // I’m adding a model error here so the UI can display a clear alert to the user.
                ModelState.AddModelError("", "Validation Error: This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropDowns(booking.VenueId, booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            PopulateDropDowns(booking.VenueId, booking.EventId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,CustomerName,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            // I re-verify the conflict on Edit to make sure a user doesn't update a booking 
            // into a slot that was taken by someone else in the meantime.
            if (await HasVenueConflict(booking))
            {
                ModelState.AddModelError("", "Validation Error: This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bookings.Any(e => e.BookingId == booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropDowns(booking.VenueId, booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method to keep my dropdowns populated and sorted alphabetically for a better UX.
        private void PopulateDropDowns(object? selectedVenue = null, object? selectedEvent = null)
        {
            ViewData["VenueId"] = new SelectList(_context.Venues.OrderBy(v => v.Name), "VenueId", "Name", selectedVenue);
            ViewData["EventId"] = new SelectList(_context.Events.OrderBy(e => e.Name), "EventId", "Name", selectedEvent);
        }

        // This is the private helper logic I wrote for the double-booking requirement. 
        // It checks if any other booking exists for the same venue on the same day.
        private async Task<bool> HasVenueConflict(Booking booking)
        {
            return await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date &&
                               b.BookingId != booking.BookingId);
        }
    }
}