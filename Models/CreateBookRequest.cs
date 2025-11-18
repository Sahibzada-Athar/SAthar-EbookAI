using System.ComponentModel.DataAnnotations;
namespace EBookDashboard.Models
{
    public class CreateBookRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        // Add these with default values since they're required in your service
        public string Dedication { get; set; } = string.Empty;
        public string Ghostwriting { get; set; } = string.Empty;
        public string Epigraph { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AuthorId { get; set; }    

        public string? Genre { get; set; }

        public int LanguageId { get; set; } = 1; // Default to English

        public string? CoverImagePath { get; set; }

        public string? ManuscriptPath { get; set; }

        public int WordCount { get; set; } = 0;
    }
}
