using System.Text.Json.Serialization;

namespace RingClient.Models.Devices
{
    public class BaseCamera
    {
        //TODO MotionDetectionEnabled property within the settings property.
        [JsonPropertyName("id")]
        public double Id { get; set; }
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("alerts")]
        public DeviceAlert Alerts { get; set; }
        [JsonPropertyName("features")]
        public DeviceFeatures Features { get; set; }
        [JsonPropertyName("motion_detection_enabled")]
        public bool MotionDetectionEnabled {get; set;}
    }
    public class DeviceAlert()
    {
        [JsonPropertyName("connetion")]
        public string Connection { get; set; }
    }
    public class DeviceFeatures()
    {
        [JsonPropertyName("motions_enabled")]
        public bool MotionsEnabled { get; set; }
        [JsonPropertyName("show_recordings")]
        public bool ShowRecordings { get; set; }
        [JsonPropertyName("advanced_motion_enabled")]
        public bool AdvancedMotionEnabled { get; set; }
        [JsonPropertyName("people_only_enabled")]
        public bool PeoplyOnlyEnabled { get; set; }
        [JsonPropertyName("shadow_correction_enabled")]
        public bool ShadowCorrectionEnabled { get; set; }
        [JsonPropertyName("motion_message_enabled")]
        public bool MotionMessageEnabled { get; set; }
        [JsonPropertyName("night_vision_enabled")]
        public bool NightVisionEnabled { get; set; }
    }
}
