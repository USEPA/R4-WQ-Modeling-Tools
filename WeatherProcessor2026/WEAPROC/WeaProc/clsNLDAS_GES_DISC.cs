using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NCEIData
{
    public class clsNLDAS_GES_DISC : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string TimeSeriesUrl = "https://api.giovanni.earthdata.nasa.gov/timeseries";
        private const string UserAgent = "GESDISC.Net v1.0";

        public clsNLDAS_GES_DISC()
        {
            var handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        private (string username, string password) ReadNetrcCredentials()
        {
            string netrcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netrc");
            if (!File.Exists(netrcPath))
                throw new FileNotFoundException("No .netrc file found. Please create one with your EarthData credentials.");
            var lines = File.ReadAllLines(netrcPath);
            string? username = null;
            string? password = null;
            foreach (string line in lines)
            {
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 2)
                {
                    if (tokens[0] == "machine" && tokens[1].Contains("earthdata.nasa.gov"))
                        continue;
                    else if (tokens[0] == "login")
                        username = tokens[1];
                    else if (tokens[0] == "password")
                        password = tokens[1];
                }
            }
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Could not find valid EarthData credentials in .netrc file.");
            return (username, password);
        }

        public string GetAccessToken()
        {
            try
            {
                var (username, password) = ReadNetrcCredentials();
                string? accessToken = FindOrCreateToken(username, password);
                if (string.IsNullOrEmpty(accessToken))
                    throw new InvalidOperationException("Failed to obtain access token");
                return accessToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Authentication with NASA EarthData failed. " +
                    "Please ensure that your .netrc file is stored and contains valid credentials. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        private string? FindOrCreateToken(string username, string password)
        {
            try
            {
                var findOrCreateTokenUrl = "https://urs.earthdata.nasa.gov/api/users/find_or_create_token";
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                var request = new HttpRequestMessage(HttpMethod.Post, findOrCreateTokenUrl);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                request.Headers.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = _httpClient.Send(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var tokenResponse = JObject.Parse(responseContent);
                    return tokenResponse["access_token"]?.ToString();
                }
                else
                {
                    var errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    throw new HttpRequestException($"Token request failed with status {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find or create token: {ex.Message}", ex);
            }
        }

        public string CallTimeSeries(double lat, double lon, string timeStart, string timeEnd, string dataVariable, string accessToken)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["data"] = dataVariable,
                ["location"] = $"[{lat},{lon}]",
                ["time"] = $"{timeStart}/{timeEnd}"
            };
            var uriBuilder = new UriBuilder(TimeSeriesUrl);
            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            uriBuilder.Query = query;
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = _httpClient.Send(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
            }
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints) ParseCsv(string csvData)
        {
            var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var headers = new Dictionary<string, string>();
            var dataPoints = new List<TimeSeriesDataPoint>();
            if (lines.Length < 15)
            {
                throw new InvalidOperationException(
                    "The returned CSV is empty or incomplete.\n" +
                    "Please ensure that your subsetting bounds are within the extent of your dataset\n" +
                    "or that your .netrc file is stored and contains valid credentials.");
            }
            for (int i = 0; i < 13 && i < lines.Length; i++)
            {
                var parts = lines[i].Split(',', 2);
                if (parts.Length >= 2)
                {
                    headers[parts[0]] = parts[1].Trim();
                }
            }
            for (int i = 15; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 2)
                {
                    if (DateTime.TryParse(parts[0], out var timestamp) &&
                        double.TryParse(parts[1], out var value))
                    {
                        dataPoints.Add(new TimeSeriesDataPoint
                        {
                            Timestamp = timestamp,
                            Value = value
                        });
                    }
                }
            }
            return (headers, dataPoints);
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints)
            GetTimeSeriesData(double lat, double lon, string timeStart, string timeEnd,
            string dataVariable = "NLDAS_FORA0125_H_2_0_Rainf")
        {
            var accessToken = GetAccessToken();
            var csvData = CallTimeSeries(lat, lon, timeStart, timeEnd, dataVariable, accessToken);
            return ParseCsv(csvData);
        }

        public bool SaveToCsv(Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints, string filePath)
        {
            bool success = false;
            try
            {
                using var writer = new StreamWriter(filePath);
                foreach (var header in headers)
                {
                    writer.WriteLine($"{header.Key},{header.Value}");
                }
                writer.WriteLine();
                writer.WriteLine("Timestamp,Value");
                foreach (var point in dataPoints)
                {
                    writer.WriteLine($"{point.Timestamp:yyyy-MM-ddTHH:mm:ss},{point.Value}");
                }
                success = true;
            }
            catch (Exception ex)
            {

            }
            return success;
        }


        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

}
