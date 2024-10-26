using RingClient.Models;
using RingInterceptorMaui.Enums;
using System.Net;
using System.Net.Http.Json;
using static RingInterceptorMaui.AuthCredentials;

namespace RingInterceptorMaui
{
    public class RingAuthentication
    {
        private AuthCredentials _credentials;
        private const string _authEndpoint = "https://oauth.ring.com/oauth/token";
        private AccessToken _accessToken = new AccessToken();
        private OutputWrapper _outputWrapper;
        private RingAuthentication(AuthCredentials authCredentials, OutputWrapper outputWrapper)
        {
            _credentials = authCredentials;
            _outputWrapper = outputWrapper;
        } 

        public static async Task<RingAuthentication> Create(OutputWrapper outputWrapper)
        {
            var authentication = new RingAuthentication(await AuthCredentials.Create(outputWrapper), outputWrapper);
            return authentication;
        }

        public static async Task<RingAuthentication> Create(FetchAccountCredentials fetchCredentialsEvent, OutputWrapper outputWrapper)
        {
            var authentication = new RingAuthentication(await AuthCredentials.Create(fetchCredentialsEvent, outputWrapper), outputWrapper);
            return authentication;
        }

        public async Task<bool> RequestAuthentication()
        {
            await _outputWrapper.InvokeWriteToOutput("Requesting Authentication.", OutputType.Neutral);
            var success = false;
            var httpClient = GenerateAuthHttpClient();
            await _outputWrapper.InvokeWriteToOutput("Generated Authorisation Header.", OutputType.Neutral);
            var request = await _credentials.FetchRequestBody();
            var jsonContent = request.Item1;
            var requestType = request.Item2;
            await _outputWrapper.InvokeWriteToOutput("Sending Post Request.", OutputType.Neutral);
            var response = await httpClient.PostAsync(_authEndpoint, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                await OnSuccessfulRequest(response);
                success = true;
            }
            else
            {
                if (await OnUnsuccessfulRequest(requestType, response.StatusCode))
                {
                    await _outputWrapper.InvokeWriteToOutput("Re-attempting Authentication Request.", OutputType.Neutral);
                    success = await RequestAuthentication();
                }
            }
            return success;
        }

        public async Task<HttpClient> GenerateAuthorisedClient()
        {
            await _outputWrapper.InvokeWriteToOutput("Generating Authorised Client.", OutputType.Neutral);
            if (_accessToken.IsTokenAvailable() == false)
            {
                await RequestAuthentication();
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput("Access Token Available.", OutputType.Positive);
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_accessToken.Value}");
            client.DefaultRequestHeaders.Add("User-Agent", "android/com.ringapp");
            await _outputWrapper.InvokeWriteToOutput("Generated Authorised Client.", OutputType.Positive);
            return client;
        }

        private async Task OnSuccessfulRequest(HttpResponseMessage response)
        {
            await _outputWrapper.InvokeWriteToOutput("Request Successfull.", OutputType.Positive);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse != null)
            {
                await _outputWrapper.InvokeWriteToOutput("Saving Refresh Token.", OutputType.Neutral);
                await File.WriteAllTextAsync(FilePaths.RefreshTokenPath, authResponse.RefreshToken);
                await _outputWrapper.InvokeWriteToOutput("Refresh Token Saved.", OutputType.Positive);
                _accessToken.Value = authResponse.AccessToken;
                await _outputWrapper.InvokeWriteToOutput("Updated Access Token.", OutputType.Positive);
                await _credentials.TryUpdateRefreshCredentials();
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput("Authorisation Response Success but Response Body is Null.", OutputType.Negative);
            }
        }

        private async Task<bool> OnUnsuccessfulRequest(RequestType requestType, HttpStatusCode statusCode)
        {
            await _outputWrapper.InvokeWriteToOutput("Request Unsuccessful.", OutputType.Negative);
            var tryAgain = false;
            if (requestType == RequestType.Password)
            {
                if (statusCode == HttpStatusCode.PreconditionFailed)
                {
                    await _outputWrapper.InvokeWriteToOutput("2FA Required.", OutputType.Neutral);
                    await _outputWrapper.InvokeWriteToOutput("Requesting 2FA.", OutputType.Neutral);
                    _credentials.TwoFACode = await Application.Current!.MainPage!.DisplayPromptAsync("2FA", "Enter 2FA Code.", "Done");
                    await _outputWrapper.InvokeWriteToOutput("2FA Received.", OutputType.Positive);
                    tryAgain = true;
                }
                else
                {
                    await _outputWrapper.InvokeWriteToOutput("Input User Credentials Are Incorrect.", OutputType.Negative);
                }
                await _outputWrapper.InvokeWriteToOutput("Response Unsuccessful with Code: " + statusCode.ToString(), OutputType.Negative);
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput("Refresh Token Expired.", OutputType.Negative);
                await _outputWrapper.InvokeWriteToOutput("Deleting Refresh Token.", OutputType.Neutral);
                File.Delete("RefreshToken.txt");
                await _outputWrapper.InvokeWriteToOutput("Refresh Token Deleted.", OutputType.Positive);
                await _credentials.TryUpdateRefreshCredentials();
            }

            return tryAgain;
        }

        private HttpClient GenerateAuthHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("2fa-support", "true");
            httpClient.DefaultRequestHeaders.Add("2fa-code", _credentials.TwoFACode);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "android/com.ringapp");
            return httpClient;
            //TODO May need to add `Hardware ID`
        }
    }
}
