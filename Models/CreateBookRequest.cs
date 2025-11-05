using System.ComponentModel.DataAnnotations;
namespace EBookDashboard.Models
{
    public class CreateBookRequest
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Genre { get; set; }

        public int LanguageId { get; set; } = 1; // Default to English

        public string? CoverImagePath { get; set; }

        public string? ManuscriptPath { get; set; }

        public int WordCount { get; set; } = 0;
    }
}
