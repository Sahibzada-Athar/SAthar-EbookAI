namespace EBookDashboard.Models.DTO
{
    public class ChapterDto
    {
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

    }
}
