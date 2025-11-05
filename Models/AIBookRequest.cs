using Newtonsoft.Json;

namespace EBookDashboard.Models
{
    public class AIBookRequest
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("book_id")]
        public string BookId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("chapterno")]
        public string ChapterNo { get; set; }
        [JsonProperty("chapter")]
        public int Chapter { get; set; }   // 👈 Should be INT (not string)

        [JsonProperty("user_input")]
        public string UserInput { get; set; }
    }
}
