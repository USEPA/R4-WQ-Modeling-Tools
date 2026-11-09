using System;
using System.Collections.Generic;

namespace NCEIData
{
    public class clsNLDAS_GES_DISC : IDisposable
    {
        private readonly GesDiscTimeSeriesClient _client;

        public clsNLDAS_GES_DISC()
        {
            _client = new GesDiscTimeSeriesClient();
        }

        public string GetAccessToken()
        {
            return _client.GetAccessToken();
        }

        public string CallTimeSeries(double lat, double lon, string timeStart, string timeEnd, string dataVariable, string accessToken)
        {
            return _client.CallTimeSeries(lat, lon, timeStart, timeEnd, dataVariable, accessToken);
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints) ParseCsv(string csvData)
        {
            return _client.ParseCsv(csvData);
        }

        public (Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints) GetTimeSeriesData(
            double lat, double lon, string timeStart, string timeEnd, string dataVariable = "NLDAS_FORA0125_H_2_0_Rainf")
        {
            return _client.GetTimeSeriesData(lat, lon, timeStart, timeEnd, dataVariable);
        }

        public bool SaveToCsv(Dictionary<string, string> headers, List<TimeSeriesDataPoint> dataPoints, string filePath)
        {
            return _client.SaveToCsv(headers, dataPoints, filePath);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}