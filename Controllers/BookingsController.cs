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

        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue);

            return View(await bookings.ToListAsync());
        }

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

        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,CustomerName,BookingDate")] Booking booking)
        {
            if (await HasVenueConflict(booking))
            {
                ModelState.AddModelError("", "This venue is already booked for another event during the selected event dates.");
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            PopulateDropDowns(booking.VenueId, booking.EventId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,CustomerName,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (await HasVenueConflict(booking))
            {
                ModelState.AddModelError("", "This venue is already booked for another event during the selected event dates.");
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropDowns(object? selectedVenue = null, object? selectedEvent = null)
        {
            ViewData["VenueId"] = new SelectList(_context.Venues.OrderBy(v => v.Name), "VenueId", "Name", selectedVenue);
            ViewData["EventId"] = new SelectList(_context.Events.OrderBy(e => e.Name), "EventId", "Name", selectedEvent);
        }

        private async Task<bool> HasVenueConflict(Booking booking)
        {
            var selectedEvent = await _context.Events.FindAsync(booking.EventId);

            if (selectedEvent == null)
                return false;

            return await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingId != booking.BookingId &&
                    b.Event != null &&
                    selectedEvent.StartDate < b.Event.EndDate &&
                    selectedEvent.EndDate > b.Event.StartDate);
        }
    }
}