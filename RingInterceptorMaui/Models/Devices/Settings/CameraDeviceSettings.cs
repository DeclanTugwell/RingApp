using System.Text.Json;
using System.Text.Json.Serialization;

namespace RingClient.Models.Devices.Settings
{
    public class CameraDeviceSettings
    {
        [JsonPropertyName("motion_settings")]
        public MotionSettings MotionSettings { get; set; }

        public static string GenerateDisabledMotionSettingJson()
        {
            var settings = new CameraDeviceSettings()
            {
                MotionSettings = new()
                 {
                    MotionDetectionEnabled = false,
                    AdvancedMotionDetectionEnabled = false,
                    RecordingEnabled = false
                }
            };
            return JsonSerializer.Serialize(settings);
        }

        public static string GenerateEnabledMotionSettingJson()
        {
            var settings = new CameraDeviceSettings()
            {
                MotionSettings = new()
                {
                    MotionDetectionEnabled = true,
                    AdvancedMotionDetectionEnabled = true,
                    RecordingEnabled = true
                }
            };
            return JsonSerializer.Serialize(settings);
        }
    }
}
