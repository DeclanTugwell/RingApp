using System.Text.Json.Serialization;

namespace RingClient.Models.Devices.Settings
{
    public class MotionSettings
    {
        [JsonPropertyName("motion_detection_enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? MotionDetectionEnabled { get; set; } = null;

        [JsonPropertyName("advanced_motion_detection_enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AdvancedMotionDetectionEnabled { get; set; } = null;

        [JsonPropertyName("enable_recording")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? RecordingEnabled { get; set; } = null;

        [JsonPropertyName("enable_ir_led")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableIrLed { get; set; } = null;
    }
}
