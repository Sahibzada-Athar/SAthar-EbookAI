namespace EBookDashboard.Models
{
    public class APIChangeChapterModel
    {
        public string? UserId { get; set; } = string.Empty;       // The user making the change
        public string? BookId { get; set; } = string.Empty;      // The book to which the chapter belongs
        public string? Chapter { get; set; } = string.Empty;     // Chapter number or ID (as string for flexibility)
        public string? NewContent { get; set; } = string.Empty;     // The updated text/content entered by the user

    }
}
