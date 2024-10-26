using RingClient.Models.Devices;
using System.Text.Json.Serialization;

namespace RingClient.Models
{
    public class DevicesResponse
    {
        [JsonPropertyName("doorbots")]
        public List<Camera> DoorBots { get; set; }
        [JsonPropertyName("authorized_doorbots")]
        public List<Camera> AuthorisedDoorBots { get; set; }
    }
}
