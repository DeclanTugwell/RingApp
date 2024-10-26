using System.Text.Json.Serialization;

namespace RingClient.Models.Devices
{
    public class Camera : BaseCamera
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("battery_life")]
        public string BatteryLife { get; set; }
        [JsonPropertyName("battery_life_2")]
        public string? BatteryLife2 { get; set; }
        [JsonPropertyName("external_connection")]
        public bool ExternalConnection { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("ring_id")]
        public string RingId { get; set; }
        [JsonPropertyName("stolen")]
        public bool Stolen { get; set; }
    }
}
