using RingClient.Models;
using RingClient.Models.Devices.Settings;
using System.Net.Http.Json;
using System.Text;
using static RingClient.AuthCredentials;

namespace RingClient
{
    public class Ring
    {
        private RingAuthentication _auth;
        private string _clientApiBaseEndpoint = "https://api.ring.com/clients_api/";
        private string _deviceApiBaseEndpoint = "https://api.ring.com/devices/v1/";
        private string _devicesEndpoint => $"{_clientApiBaseEndpoint}ring_devices";
        private string _deviceSettingsEndpoint => $"{_deviceApiBaseEndpoint}devices/{0}/settings";

        public Ring(FetchAccountCredentials fetchCredentialsEvent)
        {
            _auth = new RingAuthentication(fetchCredentialsEvent);
        }

        public Ring()
        {
            _auth = new RingAuthentication();
        }

        public async Task<DevicesResponse?> FetchDevices()
        {
            DevicesResponse? devices = null;
            var client = await _auth.GenerateAuthorisedClient();
            var response = client.GetAsync(_devicesEndpoint).Result;
            if (response.IsSuccessStatusCode)
            {
                devices = await response.Content.ReadFromJsonAsync<DevicesResponse>();
            }
            else
            {
                Console.WriteLine($"Error fetching devices: {response.StatusCode}");
                // TODO Handle error
            }
            return devices;
        }

        public async Task<CameraDeviceSettings?> FetchDeviceSettings(string deviceId)
        {
            CameraDeviceSettings? settings = null;
            var client = await _auth.GenerateAuthorisedClient();
            var response = await client.GetAsync(string.Format(_deviceSettingsEndpoint, deviceId));
            if (response.IsSuccessStatusCode)
            {
                settings = await response.Content.ReadFromJsonAsync<CameraDeviceSettings>();
            }
            else
            {
                Console.WriteLine($"Error fetching settings: {response.StatusCode}");
                // TODO Handle error
            }
            return settings;
        }

        public async Task<bool> EnableMotionDetection(string deviceId)
        {
            return await SetMotionDetection(deviceId, true);
        }

        public async Task<bool> DisableMotionDetection(string deviceId)
        {
            return await SetMotionDetection(deviceId, false);
        }

        private async Task<bool> SetMotionDetection(string deviceId, bool isMotionDetectionEnabled)
        {
            var client = await _auth.GenerateAuthorisedClient();
            string settingsJson;
            if (isMotionDetectionEnabled)
            {
                settingsJson = CameraDeviceSettings.GenerateEnabledMotionSettingJson();
            }
            else
            {
                settingsJson = CameraDeviceSettings.GenerateDisabledMotionSettingJson();
            }
            var stringContent = new StringContent(settingsJson, Encoding.UTF8, "json/application");
            var response = await client.PatchAsync(string.Format(_deviceSettingsEndpoint, deviceId), stringContent);
            return response.IsSuccessStatusCode;
        }
    }
}
