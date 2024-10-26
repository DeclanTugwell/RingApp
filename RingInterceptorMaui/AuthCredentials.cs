using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RingInterceptorMaui.Enums;
using RingInterceptorMaui.Models;

namespace RingInterceptorMaui
{
    

    public class Credentials
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "ring_official_android";
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "client";
    }

    public class AuthCredentials : Credentials
    {
        public delegate Task<AccountCredentials> FetchAccountCredentials();
        public event FetchAccountCredentials FetchAccountCredentialsEvent = DefaultFetchAccountCredentials;

        public delegate void WriteToOutput(string textValue, OutputType outputType);
        public event WriteToOutput WriteToOutputEvent = DefaultWriteToOutput;

        [JsonPropertyName("username")]
        public string Email { get; set; } = "";
        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "password";

        [JsonIgnore]
        public RefreshCredentials? RefreshCredentials { get; set; }
        ///<summary>
        /// Resets to default value of "" after fetching
        /// </summary>
        [JsonIgnore]
        public string TwoFACode {
            get {
                var temp2FA = _twoFACode;
                _twoFACode = "";
                return temp2FA;
            }
            set { if (value != _twoFACode)
                {
                    _twoFACode = value;
                }
            }
        }
        private string _twoFACode = "";
        private OutputWrapper _outputWrapper;

        public static async Task<AuthCredentials> Create(OutputWrapper outputWrapper)
        {
            var credentials = new AuthCredentials(outputWrapper);
            await outputWrapper.InvokeWriteToOutput("Successfully Initialised AuthCredentials.", OutputType.Neutral);
            if (await credentials.TryUpdateRefreshCredentials() == false)
            {
                await outputWrapper.InvokeWriteToOutput("Failed to Fetch Refresh Token.", OutputType.Negative);
                await outputWrapper.InvokeWriteToOutput("Requesting Account Credentials.", OutputType.Neutral);
                await credentials.UpdateAccountCredentials();
                await outputWrapper.InvokeWriteToOutput("Retreived Account Credentials.", OutputType.Positive);
            }
            else
            {
                await outputWrapper.InvokeWriteToOutput("Successfully Loaded Refresh Token.", OutputType.Positive);
            }
            await outputWrapper.InvokeWriteToOutput("Returning credentials.", OutputType.Neutral);
            return credentials;
        }

        public static async Task<AuthCredentials> Create(FetchAccountCredentials fetchCredentialsEvent, OutputWrapper outputWrapper)
        {
            var credentials = new AuthCredentials(fetchCredentialsEvent, outputWrapper);
            await outputWrapper.InvokeWriteToOutput("Successfully Initialised AuthCredentials.", OutputType.Neutral);
            if (await credentials.TryUpdateRefreshCredentials() == false)
            {
                await outputWrapper.InvokeWriteToOutput("Failed to Fetch Refresh Token.", OutputType.Negative);
                await outputWrapper.InvokeWriteToOutput("Requesting Account Credentials.", OutputType.Neutral);
                await credentials.UpdateAccountCredentials();
                await outputWrapper.InvokeWriteToOutput("Retreived Account Credentials.", OutputType.Positive);
            }
            else
            {
                await outputWrapper.InvokeWriteToOutput("Successfully Loaded Refresh Token.", OutputType.Positive);
            }
            await outputWrapper.InvokeWriteToOutput("Returning Credentials.", OutputType.Neutral);
            return credentials;
        }

        private AuthCredentials(FetchAccountCredentials fetchCredentialsEvent, OutputWrapper outputWrapper)
        {
            FetchAccountCredentialsEvent = fetchCredentialsEvent;
            _outputWrapper = outputWrapper;
        }

        private AuthCredentials(OutputWrapper outputWrapper)
        {
            _outputWrapper = outputWrapper;
        }

        public async Task<bool> TryUpdateRefreshCredentials()
        {
            await _outputWrapper.InvokeWriteToOutput("Trying to Update Credentials.", OutputType.Neutral);
            RefreshCredentials = RefreshCredentials.TryFetchRefreshCredentials();
            if (RefreshCredentials != null)
            {
                await _outputWrapper.InvokeWriteToOutput("Successfully Fetched Refresh Token.", OutputType.Positive);
            }
            else
            {
                await _outputWrapper.InvokeWriteToOutput("Refresh Token not Found.", OutputType.Negative);
            }
            return RefreshCredentials != null;
        }

        private async Task UpdateAccountCredentials()
        {
            var credentials = await FetchAccountCredentialsEvent.Invoke();
            Email = credentials.Email;
            await _outputWrapper.InvokeWriteToOutput("Updated Email.", OutputType.Neutral);
            Password = credentials.Password;
            await _outputWrapper.InvokeWriteToOutput("Updated Password.", OutputType.Neutral);
        }

        public async Task<(StringContent, RequestType)> FetchRequestBody()
        {
            RequestType requestType;
            string credentialsJson;
            if (RefreshCredentials == null)
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    await UpdateAccountCredentials();
                }
                credentialsJson = JsonSerializer.Serialize(this);
                await _outputWrapper.InvokeWriteToOutput("Serialised Account Credentials.", OutputType.Neutral);
                requestType = RequestType.Password;
                await _outputWrapper.InvokeWriteToOutput("Password Request Type Chosen.", OutputType.Neutral);
            }
            else
            {
                credentialsJson = JsonSerializer.Serialize(RefreshCredentials);
                await _outputWrapper.InvokeWriteToOutput("Serialised Refresh Credentials.", OutputType.Neutral);
                requestType = RequestType.Refresh;
                await _outputWrapper.InvokeWriteToOutput("Refresh Request Type Chosen.", OutputType.Neutral);
            }
            await _outputWrapper.InvokeWriteToOutput("Generated Request Body.", OutputType.Neutral);
            return (new StringContent(credentialsJson, Encoding.UTF8, "application/json"), requestType);
        }

        private static async Task<AccountCredentials> DefaultFetchAccountCredentials()
        {
            var credentials = new AccountCredentials();
            Console.WriteLine("Email:");
            credentials.Email = Console.ReadLine() ?? "";
            Console.WriteLine("Password:");
            credentials.Password = Console.ReadLine() ?? "";
            //TODO Sleep thread until details entered.
            if (string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                Console.WriteLine("Input must not be empty...");
                credentials = await DefaultFetchAccountCredentials();
            }

            return credentials;
        }

        private static void DefaultWriteToOutput(string outputText, OutputType outputType)
        {
        }
    }

    public class RefreshCredentials : Credentials
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "refresh_token";
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        private RefreshCredentials(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public static RefreshCredentials? TryFetchRefreshCredentials()
        {
            RefreshCredentials? credentials = null;
            if (File.Exists(FilePaths.RefreshTokenPath))
            {
                var refreshToken = File.ReadAllText(FilePaths.RefreshTokenPath);
                if (string.IsNullOrEmpty(refreshToken) == false)
                {
                    credentials = new RefreshCredentials(refreshToken);
                }
            }

            return credentials;
        }
    }
}
