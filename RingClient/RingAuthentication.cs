using RingClient.Models;
using System.Net;
using System.Net.Http.Json;
using static RingClient.AuthCredentials;

namespace RingClient
{
    public class RingAuthentication
    {
        private AuthCredentials _credentials;
        private const string _authEndpoint = "https://oauth.ring.com/oauth/token";
        private AccessToken _accessToken = new AccessToken();
        public RingAuthentication()
        {
            _credentials = new AuthCredentials();
        } 

        public RingAuthentication(FetchAccountCredentials fetchCredentialsEvent)
        {
            _credentials = new AuthCredentials(fetchCredentialsEvent);
        }

        public async Task<bool> RequestAuthentication()
        {
            var httpClient = GenerateAuthHttpClient();
            var jsonContent = _credentials.FetchRequestBody(out RequestType requestType);
            var response = await httpClient.PostAsync(_authEndpoint, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                await OnSuccessfulRequest(response);
            }
            else
            {
                OnUnsuccessfulRequest(requestType, response.StatusCode);
                if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    await RequestAuthentication();
                }
            }
            Console.WriteLine(_accessToken.Value);
            return true;
        }

        public async Task<HttpClient> GenerateAuthorisedClient()
        {
            if (_accessToken.IsTokenAvailable() == false)
            {
                await RequestAuthentication();
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_accessToken.Value}");
            client.DefaultRequestHeaders.Add("User-Agent", "android/com.ringapp");
            return client;
        }

        private async Task OnSuccessfulRequest(HttpResponseMessage response)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse != null)
            {
                File.WriteAllText("RefreshToken.txt", authResponse.RefreshToken);
                _accessToken.Value = authResponse.AccessToken;
                _credentials.TryUpdateRefreshCredentials();
            }
            else
            {
                // TODO Add error Handling
            }
        }

        private void OnUnsuccessfulRequest(RequestType requestType, HttpStatusCode statusCode)
        {
            if (requestType == RequestType.Password)
            {
                Console.WriteLine("Response Unsuccessful with code: " + statusCode.ToString());
                _credentials.TwoFACode = Console.ReadLine() ?? "";
            }
            else
            {
                File.Delete("RefreshToken.txt");
                _credentials.TryUpdateRefreshCredentials();
            }
        }

        private HttpClient GenerateAuthHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("2fa-support", "true");
            httpClient.DefaultRequestHeaders.Add("2fa-code", _credentials.TwoFACode);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "android/com.ringapp");
            return httpClient;
        }
    }
}
