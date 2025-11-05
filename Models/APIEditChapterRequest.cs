using Newtonsoft.Json;

namespace EBookDashboard.Models
{
    public class APIEditChapterRequest
    {
        // Keep same naming style as external API expects if you forward body directly
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("book_id")]
        public string BookId { get; set; }

        // External API previously used string for chapter in example; keep as string to be flexible.
        [JsonProperty("chapter")]
        public string Chapter { get; set; }

        [JsonProperty("changes")]
        public string Changes { get; set; }

    }
}
