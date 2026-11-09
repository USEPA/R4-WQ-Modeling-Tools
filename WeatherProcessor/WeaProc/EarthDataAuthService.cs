using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace NCEIData
{
    public sealed class EarthDataAuthService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string UserAgent = "GESDISC.Net v1.0";

        public EarthDataAuthService()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public (string username, string password) ReadNetrcCredentials()
        {
            string netrcPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netrc");

            if (!File.Exists(netrcPath))
                throw new FileNotFoundException(
                    "No .netrc file found. Please create one with your EarthData credentials.");

            var lines = File.ReadAllLines(netrcPath);
            string username = null;
            string password = null;

            foreach (string line in lines)
            {
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) continue;

                if (tokens[0] == "machine" && tokens[1].Contains("earthdata.nasa.gov"))
                    continue;
                if (tokens[0] == "login")
                    username = tokens[1];
                else if (tokens[0] == "password")
                    password = tokens[1];
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException(
                    "Could not find valid EarthData credentials in .netrc file.");

            return (username, password);
        }

        public void WriteNetrcCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            string netrcPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netrc");

            string netrcContent =
                "machine urs.earthdata.nasa.gov" + Environment.NewLine +
                $"    login {username}" + Environment.NewLine +
                $"    password {password}" + Environment.NewLine;

            File.WriteAllText(netrcPath, netrcContent);
        }

        public string GetAccessToken()
        {
            try
            {
                var (username, password) = ReadNetrcCredentials();
                var accessToken = FindOrCreateToken(username, password);

                if (string.IsNullOrEmpty(accessToken))
                    throw new InvalidOperationException("Failed to obtain access token.");

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Authentication with NASA EarthData failed. " +
                    "Please ensure your .netrc file exists and has valid credentials. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        public string FindOrCreateToken(string username, string password)
        {
            var url = "https://urs.earthdata.nasa.gov/api/users/find_or_create_token";
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var response = _httpClient.Send(request);
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Token request failed with status {response.StatusCode}: {content}");

            var tokenResponse = JObject.Parse(content);
            return tokenResponse["access_token"]?.ToString();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}