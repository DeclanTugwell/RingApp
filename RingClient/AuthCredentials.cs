using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RingClient
{
    public enum RequestType
    {
        Refresh,
        Password
    }

    public class Credentials
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "ring_official_android";
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "client";
    }

    public class AuthCredentials : Credentials
    {
        public delegate void FetchAccountCredentials(out string username, out string password);
        public event FetchAccountCredentials FetchAccountCredentialsEvent = DefaultFetchAccountCredentials;

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
        

        public AuthCredentials()
        {
            if (TryUpdateRefreshCredentials() == false)
            {
                UpdateAccountCredentials();
            }
        }

        public AuthCredentials(FetchAccountCredentials fetchCredentialsEvent) : this()
        {
            FetchAccountCredentialsEvent = fetchCredentialsEvent;
        }

        public bool TryUpdateRefreshCredentials()
        {
            RefreshCredentials = RefreshCredentials.TryFetchRefreshCredentials();
            return RefreshCredentials != null;
        }

        private void UpdateAccountCredentials()
        {
            FetchAccountCredentialsEvent.Invoke(out string email, out string password);
            Email = email;
            Password = password;
        }

        public StringContent FetchRequestBody(out RequestType requestType)
        {
            string credentialsJson;
            if (RefreshCredentials == null)
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    UpdateAccountCredentials();
                }
                credentialsJson = JsonSerializer.Serialize(this);
                requestType = RequestType.Password;
                Console.WriteLine("Password Request Type Chosen");
            }
            else
            {
                credentialsJson = JsonSerializer.Serialize(RefreshCredentials);
                requestType = RequestType.Refresh;
                Console.WriteLine("Refresh Request Type Chosen");
            }
            return new StringContent(credentialsJson, Encoding.UTF8, "application/json");
        }

        private static void DefaultFetchAccountCredentials(out string username, out string password)
        {
            Console.WriteLine("Email:");
            username = Console.ReadLine() ?? "";
            Console.WriteLine("Password:");
            password = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Input must not be empty...");
                DefaultFetchAccountCredentials(out username, out password);
            }
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

        public static RefreshCredentials? TryFetchRefreshCredentials(string pathToFile = "RefreshToken.txt")
        {
            RefreshCredentials? credentials = null;
            if (File.Exists(pathToFile))
            {
                var refreshToken = File.ReadAllText(pathToFile);
                if (string.IsNullOrEmpty(refreshToken) == false)
                {
                    credentials = new RefreshCredentials(refreshToken);
                }
            }

            return credentials;
        }
    }
}
