using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace NCEIData
{
    public sealed class GesDiscTimeSeriesClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly EarthDataAuthService _authService;
        private const string TimeSeriesUrl = "https://api.giovanni.earthdata.nasa.gov/timeseries";
        private const string UserAgent = "GESDISC.Net v1.0";

        public GesDiscTimeSeriesClient()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _authService = new EarthDataAuthService();
        }

        public string GetAccessToken() => _authService.GetAccessToken();

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

            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = _httpClient.Send(request);
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {content}");

            return content;
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints) ParseCsv(string csvData)
        {
            var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var headers = new Dictionary<string, string>();
            var dataPoints = new List<TimeSeriesDataPoint>();

            if (lines.Length < 15)
            {
                throw new InvalidOperationException(
                    "The returned CSV is empty or incomplete. " +
                    "Verify bounds and EarthData credentials.");
            }

            for (int i = 0; i < 13 && i < lines.Length; i++)
            {
                var parts = lines[i].Split(',', 2);
                if (parts.Length >= 2)
                    headers[parts[0]] = parts[1].Trim();
            }

            for (int i = 15; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 2) continue;

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

            return (headers, dataPoints);
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints) GetTimeSeriesData(
            double lat, double lon, string timeStart, string timeEnd, string dataVariable)
        {
            var accessToken = GetAccessToken();
            var csvData = CallTimeSeries(lat, lon, timeStart, timeEnd, dataVariable, accessToken);
            return ParseCsv(csvData);
        }

        public bool SaveToCsv(Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                foreach (var header in headers)
                    writer.WriteLine($"{header.Key},{header.Value}");

                writer.WriteLine();
                writer.WriteLine("Timestamp,Value");

                foreach (var point in dataPoints)
                    writer.WriteLine($"{point.Timestamp:yyyy-MM-ddTHH:mm:ss},{point.Value}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _authService?.Dispose();
        }
    }

    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }
}