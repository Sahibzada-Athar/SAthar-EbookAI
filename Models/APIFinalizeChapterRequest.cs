using Newtonsoft.Json;

namespace EBookDashboard.Models
{
    public class APIFinalizeChapterRequest
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("book_id")]
        public string BookId { get; set; }

        [JsonProperty("chapter")]
        public string Chapter { get; set; }

        [JsonProperty("approve")]
        public bool Approve { get; set; } = true;


    }
}
