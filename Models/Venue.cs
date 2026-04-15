using System.ComponentModel.DataAnnotations;

namespace ST10484350_CLDV_Part_1_EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(1, 100000)]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [Url]
        public string? ImageUrl { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}