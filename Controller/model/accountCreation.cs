using System.Text.Json;
using System.Text.Json.Serialization;

namespace Controller.model
{
    
    public class accountCreation
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string City { get; set; }
        public string Institute { get; set; }
        public string Role { get; set; }
    }
}
