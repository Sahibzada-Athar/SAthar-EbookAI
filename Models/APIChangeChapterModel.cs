namespace EBookDashboard.Models
{
    public class APIChangeChapterModel
    {
        public string UserId { get; set; }        // The user making the change
        public string BookId { get; set; }        // The book to which the chapter belongs
        public string Chapter { get; set; }       // Chapter number or ID (as string for flexibility)
        public string NewContent { get; set; }    // The updated text/content entered by the user

    }
}
