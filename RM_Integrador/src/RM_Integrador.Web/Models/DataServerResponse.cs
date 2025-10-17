using System.Text.Json;
using System.Text.Json.Serialization;

namespace RM_Integrador.Web.Models
{
    public class DataServerResponse
    {
        [JsonPropertyName("messages")]
        public string[] Messages { get; set; } = Array.Empty<string>();

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("data")]
        public JsonElement[]? Data { get; set; }
    }
}