using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NCEIData
{
    class clsStations
    {
        private frmMain fMain;
        private SortedDictionary<string, string> dictGages;
        private string csvFile;

        public clsStations(string _csvfile)
        {
            this.csvFile = _csvfile;
        }
        public SortedDictionary<string, string> ReadGHCNStations()
        {
            dictGages = new SortedDictionary<string, string>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string stationId = dict.Values.ElementAt(0)?.ToString();
                    string stationName = dict.ContainsKey("StationName") ? dict["StationName"]?.ToString() : null;
                    if (!string.IsNullOrEmpty(stationId) && !dictGages.ContainsKey(stationId))
                    {
                        dictGages.Add(stationId, stationName);
                    }
                }
            }
            return dictGages;
        }
        public SortedDictionary<string, string> ReadCOOPStations()
        {
            dictGages = new SortedDictionary<string, string>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string stationId = dict.Values.ElementAt(0)?.ToString();
                    string name = dict.ContainsKey("Name") ? dict["Name"]?.ToString() : null;
                    if (!string.IsNullOrEmpty(stationId) && !dictGages.ContainsKey(stationId))
                    {
                        dictGages.Add(stationId, name);
                    }
                }
            }
            return dictGages;
        }
        public SortedDictionary<string, string> ReadISDStations()
        {
            dictGages = new SortedDictionary<string, string>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string usaf = dict.Values.ElementAt(0)?.ToString();
                    string wban = dict.Values.ElementAt(1)?.ToString();
                    //string stationName = dict.ContainsKey("STATION NAME") ? dict["STATION NAME"]?.ToString() : null;
                    if (usaf != null && wban != null)
                    {
                        usaf = usaf.PadLeft(6, '0');
                        wban = wban.PadLeft(5, '0');
                        string staid = wban; // usaf + wban;
                        string stationName = usaf + wban;
                        if (!dictGages.ContainsKey(staid))
                        {
                            dictGages.Add(staid, stationName);
                        }
                    }
                }
            }
            return dictGages;
        }
    }
}