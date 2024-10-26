using RingClient.Models;
using RingClient.Models.Devices.Settings;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using static RingInterceptorMaui.AuthCredentials;

namespace RingInterceptorMaui
{
    public class RingClient
    {
        private RingAuthentication _auth;
        private string _clientApiBaseEndpoint = "https://api.ring.com/clients_api/";
        private string _deviceApiBaseEndpoint = "https://api.ring.com/devices/v1/";
        private string _devicesEndpoint => $"{_clientApiBaseEndpoint}ring_devices";
        private string _deviceSettingsEndpoint = "https://api.ring.com/devices/v1/devices/{0}/settings";
        private OutputWrapper _outputWrapper;
        private RingClient(RingAuthentication authentication, OutputWrapper outputWrapper)
        {
            _auth = authentication;
            _outputWrapper = outputWrapper;
        }

        public static async Task<RingClient> Create(OutputWrapper outputWrapper)
        {
            return new RingClient(await RingAuthentication.Create(outputWrapper), outputWrapper);
        }

        public static async Task<RingClient> Create(FetchAccountCredentials fetchCredentialsEvent, OutputWrapper outputWrapper)
        {
            return new RingClient(await RingAuthentication.Create(fetchCredentialsEvent, outputWrapper), outputWrapper);
        }

        public async Task<DevicesResponse?> FetchDevices()
        {
            await _outputWrapper.InvokeWriteToOutput("Fetching Ring Devices", Enums.OutputType.Neutral);
            DevicesResponse? devices = null;
            var client = await _auth.GenerateAuthorisedClient();
            await _outputWrapper.InvokeWriteToOutput($"Sending Get Request.", Enums.OutputType.Neutral);
            var response = await client.GetAsync(_devicesEndpoint);
            if (response.IsSuccessStatusCode)
            {
                await _outputWrapper.InvokeWriteToOutput("Devices Successfully Recieved.", Enums.OutputType.Positive);
                await _outputWrapper.InvokeWriteToOutput("Deserialising Ring Devices.", Enums.OutputType.Neutral);
                devices = await response.Content.ReadFromJsonAsync<DevicesResponse>();
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput($"Error fetching devices: {response.StatusCode}", Enums.OutputType.Negative);
                // TODO Handle error
            }
            return devices;
        }

        public async Task<CameraDeviceSettings?> FetchDeviceSettings(string deviceId)
        {
            await _outputWrapper.InvokeWriteToOutput($"Fetching settings for device: {deviceId}.", Enums.OutputType.Neutral);
            CameraDeviceSettings? settings = null;
            var client = await _auth.GenerateAuthorisedClient();
            await _outputWrapper.InvokeWriteToOutput($"Sending Get Request.", Enums.OutputType.Neutral);
            var endpoint = string.Format(_deviceSettingsEndpoint, deviceId);
            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                await _outputWrapper.InvokeWriteToOutput("Devices Settings Successfully Recieved.", Enums.OutputType.Positive);
                await _outputWrapper.InvokeWriteToOutput("Deserialising Ring Device Settings.", Enums.OutputType.Neutral);
                settings = await response.Content.ReadFromJsonAsync<CameraDeviceSettings>();
                var settingsJson = await response.Content.ReadAsStringAsync();
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput($"Error fetching device settings: {response.StatusCode}", Enums.OutputType.Negative);
                // TODO Handle error
            }
            return settings;
        }

        public async Task<HttpStatusCode> EnableMotionDetection(string deviceId)
        {
            await _outputWrapper.InvokeWriteToOutput($"Enabling Motion Detection for Device: {deviceId}.", Enums.OutputType.Neutral);
            return await SetMotionDetection(deviceId, true);
        }

        public async Task<HttpStatusCode> DisableMotionDetection(string deviceId)
        {
            await _outputWrapper.InvokeWriteToOutput($"Disabling Motion Detection for Device: {deviceId}.", Enums.OutputType.Neutral);
            return await SetMotionDetection(deviceId, false);
        }

        private async Task<HttpStatusCode> SetMotionDetection(string deviceId, bool isMotionDetectionEnabled)
        {
            var client = await _auth.GenerateAuthorisedClient();
            string settingsJson;
            if (isMotionDetectionEnabled)
            {
                await _outputWrapper.InvokeWriteToOutput($"Generating Enable Motion Detection Json.", Enums.OutputType.Neutral);
                settingsJson = CameraDeviceSettings.GenerateEnabledMotionSettingJson();
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput($"Generating Disable Motion Detection Json.", Enums.OutputType.Neutral);
                settingsJson = CameraDeviceSettings.GenerateDisabledMotionSettingJson();
            }
            var stringContent = new StringContent(settingsJson, Encoding.UTF8, "json/application");
            await _outputWrapper.InvokeWriteToOutput($"Patching Motion Detection Json.", Enums.OutputType.Neutral);
            var endpoint = string.Format(_deviceSettingsEndpoint, deviceId);
            var response = await client.PatchAsync(endpoint, stringContent);
            return response.StatusCode;
        }
    }
}
