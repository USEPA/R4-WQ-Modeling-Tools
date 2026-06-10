using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace NCEIData
{
    class CsvProcessor
    {
        private Dictionary<string, SortedDictionary<DateTime, string>> dictData;
        private string OutPath;
        private DateTime BegDate, EndDate;

        public CsvProcessor(DateTime _begdt, DateTime _enddt)
        {
            BegDate = _begdt;
            EndDate = _enddt;
        }

        public CsvProcessor(string _outpath)
        {
            this.OutPath = _outpath;
        }

        public Dictionary<string, SortedDictionary<DateTime, string>> ReadCsvData(string csvFile)
        {
            dictData = new Dictionary<string, SortedDictionary<DateTime, string>>();
            SortedDictionary<DateTime, string> dSeries;

            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string key = dict.ElementAt(1).Value?.ToString();
                    string dateStr = dict.ElementAt(2).Value?.ToString();
                    string value = dict.ElementAt(3).Value?.ToString();

                    if (!dictData.ContainsKey(key))
                    {
                        dSeries = new SortedDictionary<DateTime, string>();
                        dictData.Add(key, dSeries);
                    }
                    dictData.TryGetValue(key, out dSeries);
                    dSeries.Add(DateTime.Parse(dateStr), value);
                }
            }
            return dictData;
        }

        public SortedDictionary<DateTime, string> ReadCsvData(string csvFile, string svar)
        {
            SortedDictionary<DateTime, string> dSeries = new SortedDictionary<DateTime, string>();

            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string key = dict.ElementAt(1).Value?.ToString();
                    string dateStr = dict.ElementAt(2).Value?.ToString();
                    string value = dict.ElementAt(3).Value?.ToString();

                    if (key != null && key.Contains(svar))
                        dSeries.Add(DateTime.Parse(dateStr), value);
                }
            }
            foreach (KeyValuePair<DateTime, string> kv in dSeries)
            {
                Debug.WriteLine("{0},{1},{2}", svar, kv.Key.ToString(), kv.Value.ToString());
            }
            return dSeries;
        }

        public SortedDictionary<DateTime, string> ReadCsvFile(string csvFile, string MISS)
        {
            SortedDictionary<DateTime, string> dSeries = new SortedDictionary<DateTime, string>();

            DateTime dt, curdt;
            int yr, mon, day, hr;
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var dict = (IDictionary<string, object>)record;
                    string dateStr = dict.ElementAt(5).Value?.ToString();
                    dt = DateTime.Parse(dateStr);
                    if (dt.CompareTo(BegDate) >= 0 && dt.CompareTo(EndDate) <= 0)
                    {
                        yr = dt.Year;
                        mon = dt.Month;
                        day = dt.Day;

                        for (int i = 6; i <= 121; i += 5)
                        {
                            hr = (i / 5) - 1;
                            curdt = new DateTime(yr, mon, day, hr, 0, 0);
                            string val = dict.ElementAt(i).Value?.ToString();
                            string val2 = dict.ElementAt(i + 2).Value?.ToString();
                            if (!(val.Contains(MISS)) && val2 == " ")
                            {
                                double tp = Convert.ToDouble(val) / 100.0;
                                string dat = tp.ToString("#0.000");
                                dSeries.Add(curdt, dat);
                            }
                        }
                    }
                }
            }
            return dSeries;
        }

        public void WriteStationInfoHeader(string site)
        {
            string csvfile = Path.Combine(OutPath, site + "Info.csv");
            using (StreamWriter sr = new StreamWriter(csvfile, false))
            {
                string head = "Station_ID, Station_Name, Latitude, Longitude, Elevation";
                sr.WriteLine(head);
                sr.Flush();
            }
        }
        public void WriteStationInfo(string site, string stname, float lat, float lon, float elev)
        {
            string csvfile = Path.Combine(OutPath, site + "Info.csv");
            using (StreamWriter sr = new StreamWriter(csvfile, true))
            {
                StringBuilder st = new StringBuilder();
                st.Append(site + ",");
                st.Append(stname + ",");
                st.Append(lat.ToString("F5") + ",");
                st.Append(lon.ToString("F5") + ",");
                st.Append(elev.ToString("F4"));
                sr.WriteLine(st.ToString());
                sr.Flush();
            }
        }
        public void WriteCSVFile(string site, Dictionary<string, SortedDictionary<DateTime, double>> dictSeries)
        {
            Cursor.Current = Cursors.WaitCursor;

            string csvfile = Path.Combine(OutPath, site + ".csv");
            SortedDictionary<DateTime, double> tseries = new SortedDictionary<DateTime, double>();

            List<string> pcodes = dictSeries.Keys.ToList();
            foreach (var s in pcodes) Debug.WriteLine("Station={0}, PCode={1}", site, s);
            try
            {
                using (StreamWriter sr = new StreamWriter(csvfile, false))
                {
                    sr.AutoFlush = true;
                    WriteHeader(sr);

                    foreach (var kv in dictSeries)
                    {
                        string svar = kv.Key;
                        if (dictSeries.TryGetValue(svar, out tseries))
                        {
                            foreach (var kv1 in tseries)
                            {
                                sr.WriteLine("{0},{1},{2},{3}", site, svar, kv1.Key.ToString(), FormatVariable(svar, kv1.Value));
                                sr.Flush();
                            }
                        }
                    }
                    sr.Flush(); sr.Close();
                }
            }
            catch (Exception ex)
            {

            }
            tseries = null;
            Cursor.Current = Cursors.Default;
        }
        private string FormatVariable(string pcode, double value)
        {
            string sval = string.Empty;
            float val = Convert.ToSingle(value);
            switch (pcode.ToUpper())
            {
                case "PREC":
                    sval = val.ToString("F3");
                    break;
                case "PRCP":
                    sval = val.ToString("F3");
                    break;
                case "ATEM":
                    sval = val.ToString("F2");
                    break;
                case "TMAX":
                    sval = val.ToString("F2");
                    break;
                case "TMIN":
                    sval = val.ToString("F2");
                    break;
                case "DEWP":
                    sval = val.ToString("F2");
                    break;
                case "SOLR":
                    sval = val.ToString("F5");
                    break;
                case "LRAD":
                    sval = val.ToString("F5");
                    break;
                case "WIND":
                    sval = val.ToString("F3");
                    break;
                case "WNDD":
                    sval = val.ToString("F2");
                    break;
                case "WINDU":
                    sval = val.ToString("F3");
                    break;
                case "WINDV":
                    sval = val.ToString("F3");
                    break;
                case "CLOU":
                    sval = val.ToString("F2");
                    break;
                case "ATMP":
                    sval = val.ToString("F2");
                    break;
                case "PEVT":
                    sval = val.ToString("F5");
                    break;
            }
            return sval;
        }
        private void WriteHeader(StreamWriter wri)
        {
            string head = "Station_ID, Variable, DateTime, Value";
            wri.WriteLine(head);
            wri.Flush();
        }
    }
}
