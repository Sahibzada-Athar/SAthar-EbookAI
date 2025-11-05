using System.ComponentModel.DataAnnotations;

namespace EBookDashboard.Models
{
    public enum BookStatus { Draft, Published, Archived }

    public class Books
    {
        [Key]
        public int BookId { get; set; }
        public int AuthorId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }


        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
        public string AuthorCode { get; set; } = string.Empty;
        public int LanguageId { get; set; }


        [Required]
        
        public string? CoverImagePath { get; set; } = null!; // FK to AspNetUsers

        public string? ManuscriptPath { get; set; } = null!; // FK to AspNetUsers

        public string Genre {  get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        public int WordCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public ICollection<Chapters> Chapters { get; set; } = new List<Chapters>();
        public ICollection<BookPrice> BookPrice { get; set; } = new List<BookPrice>();
    }

}
