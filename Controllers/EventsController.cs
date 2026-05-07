using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10484350_CLDV_Part_1_EventEase.Data;
using ST10484350_CLDV_Part_1_EventEase.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ST10484350_CLDV_Part_1_EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public EventsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // --- INDEX (With Search) ---
        public async Task<IActionResult> Index(string searchString)
        {
            var eventsQuery = _context.Events.Include(e => e.Venue).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Allowing search by event name or description (Microsoft, 2024).
                eventsQuery = eventsQuery.Where(s => s.Name.Contains(searchString) || s.Description.Contains(searchString));
            }

            return View(await eventsQuery.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings!)
                .ThenInclude(b => b.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null) return NotFound();
            return View(eventItem);
        }

        public IActionResult Create()
        {
            ViewBag.VenueId = new SelectList(_context.Venues.OrderBy(v => v.Name), "VenueId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string connectionString = _configuration.GetConnectionString("AzureStorage");
                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("event-images");
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = imageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }
                    eventItem.ImageUrl = blobClient.Uri.ToString();
                }

                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "Name", eventItem.VenueId);
            return View(eventItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();
            ViewBag.VenueId = new SelectList(_context.Venues.OrderBy(v => v.Name), "VenueId", "Name", eventItem.VenueId);
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem, IFormFile? imageFile)
        {
            if (id != eventItem.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string connectionString = _configuration.GetConnectionString("AzureStorage");
                        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("event-images");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        BlobClient blobClient = containerClient.GetBlobClient(fileName);
                        using (var stream = imageFile.OpenReadStream()) { await blobClient.UploadAsync(stream, true); }
                        eventItem.ImageUrl = blobClient.Uri.ToString();
                    }
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Events.Any(e => e.EventId == eventItem.EventId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "Name", eventItem.VenueId);
            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null) return NotFound();
            if (eventItem.Bookings != null && eventItem.Bookings.Any())
            {
                TempData["Error"] = "Validation Error: This event cannot be deleted because it is linked to active bookings.";
                return RedirectToAction(nameof(Index));
            }
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}