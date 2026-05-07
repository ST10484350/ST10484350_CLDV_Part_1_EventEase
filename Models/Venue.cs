using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [NotMapped]

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

        // This stays as a string to store the URL in the database
        [Display(Name = "Venue Image")]
        public string? ImageUrl { get; set; }

        // This is the NEW part: It handles the file upload but is NOT saved to the database
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}