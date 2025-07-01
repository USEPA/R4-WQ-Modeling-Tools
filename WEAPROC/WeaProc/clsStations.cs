using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NCEIData
{
    class clsStations
    {
        private frmMain fMain;
        private SortedDictionary<string, string> dictGages;
        private string csvFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_csvfile"></param>
        public clsStations(string _csvfile)
        {
            this.csvFile = _csvfile;
        }
        public SortedDictionary<string, string> ReadGHCNStations()
        {
            // open the file GHCNStations.csv 
            // stationID-staname dictionary
            dictGages = new SortedDictionary<string, string>();

            using (var csv = new CachedCsvReader(new StreamReader(csvFile), true))
            {
                // Field headers will automatically be used as column names
                int fieldCount = csv.FieldCount;
                List<string> headers = csv.GetFieldHeaders().ToList();
                int idx = headers.IndexOf("StationName");
                while (csv.ReadNextRecord())
                {
                    if (!dictGages.ContainsKey(csv[0]))
                    {
                        dictGages.Add(csv[0], csv[idx]);
                    }
                }
            }
            return dictGages;
        }
        public SortedDictionary<string, string> ReadCOOPStations()
        {
            // open the file COOPStations.csv 
            dictGages = new SortedDictionary<string, string>();

            using (var csv = new CachedCsvReader(new StreamReader(csvFile), true))
            {
                // Field headers will automatically be used as column names
                int fieldCount = csv.FieldCount;
                List<string> headers = csv.GetFieldHeaders().ToList();
                int idx = headers.IndexOf("Name");
                while (csv.ReadNextRecord())
                {
                    if (!dictGages.ContainsKey(csv[0]))
                    {
                        dictGages.Add(csv[0], csv[idx]);
                    }
                }
            }
            return dictGages;
        }
        public SortedDictionary<string, string> ReadISDStations()
        {
            // open the file GHCNStations.csv 
            dictGages = new SortedDictionary<string, string>();

            using (var csv = new CachedCsvReader(new StreamReader(csvFile), true))
            {
                // Field headers will automatically be used as column names
                int fieldCount = csv.FieldCount;
                List<string> headers = csv.GetFieldHeaders().ToList();
                int idx = headers.IndexOf("STATION NAME");
                while (csv.ReadNextRecord())
                {
                    string usaf = csv[0];
                    string wban = csv[1];

                    int len = csv[0].Length;
                    for (int i = 0; i < 6 - len; i++)
                        usaf = "0" + usaf;

                    len = csv[1].Length;
                    for (int i = 0; i < 5 - len; i++)
                        wban = "0" + wban;

                    string staid = usaf + wban;
                    if (!dictGages.ContainsKey(staid))
                    {
                        dictGages.Add(staid, csv[idx]);
                    }
                }
            }
            return dictGages;
        }
    }
}