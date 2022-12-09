using System.Text.Json;
using System.Text.Json.Serialization;

namespace Controller.model
{   
    public class accountCreation
    {
        [JsonPropertyName ("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("institute")]
        public string Institute { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
