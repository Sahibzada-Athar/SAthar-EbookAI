using Newtonsoft.Json;

namespace EBookDashboard.Models
{
    public class AIChapterResponse
    {
        [JsonProperty("title")]
        public string? Title { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string? Content { get; set; }= string.Empty;

    }
}
