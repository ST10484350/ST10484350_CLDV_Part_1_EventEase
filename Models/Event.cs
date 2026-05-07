using System.ComponentModel.DataAnnotations;

namespace ST10484350_CLDV_Part_1_EventEase.Models
{
    public class Event : IValidatableObject
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Expected Guests")]
        [Range(1, 100000)]
        public int ExpectedGuests { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Event Poster")]
        public string? ImageUrl { get; set; }

        // --- NEW: Link to Venue ---
        [Required(ErrorMessage = "Please assign a venue to this event")]
        [Display(Name = "Assigned Venue")]
        public int? VenueId { get; set; }

        public Venue? Venue { get; set; }
        // -------------------------

        public ICollection<Booking>? Bookings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End Date must be after Start Date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}