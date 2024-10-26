using RingClient.Models.Devices.Settings;
using System.Text.Json.Serialization;

namespace RingInterceptorMaui.Models.Devices.Settings
{
    public class DoorbotSettings
    {
        [JsonPropertyName("doorbot")]
        public MotionSettings DoorBot { get; set; }
    }
}
