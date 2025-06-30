using NCEIData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WeaDB;
using WeaWDM;

namespace WeaWASP
{
    class clsWriteWASP
    {
        private string WDMFile, SDBFile;
        private DateTime BegDate, EndDate;
        private List<string> lstSelectedDSN;
        private WeaSDB cSDB;
        private WDM cwdm;
        private frmWASP fWASP;
        private string errmsg;
        private string crlf = Environment.NewLine;
        public Dictionary<string, SortedDictionary<int, clsStation>> dictGages
                    = new Dictionary<string, SortedDictionary<int, clsStation>>();
        private DateTime DateBeg, DateEnd;

        public clsWriteWASP(frmWASP _fWASP, string _wdmFile, string _sdbFile, List<string> _lstDSN, DateTime _BegDate, DateTime _EndDate,
                Dictionary<string, SortedDictionary<int, clsStation>> _dictGages)
        {
            this.WDMFile = _wdmFile;
            this.SDBFile = _sdbFile;
            this.BegDate = _BegDate;
            this.EndDate = _EndDate;
            this.lstSelectedDSN = _lstDSN;
            this.dictGages = _dictGages;
            this.fWASP = _fWASP;
        }
        public void UploadWASPWeather()
        {
            SortedDictionary<DateTime, double> dictSeries = new
                        SortedDictionary<DateTime, double>();
            SortedDictionary<DateTime, double> ConvertedSeries = new
                        SortedDictionary<DateTime, double>();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                //initialize cSDB
                cSDB = new WeaSDB(SDBFile);

                //initialize cWDM
                cwdm = new WDM(WDMFile);

                //iterate on list of selected series
                int isite = 0;
                int nsites = lstSelectedDSN.Count();

                foreach (var vardsn in lstSelectedDSN)
                {
                    //Debug.WriteLine("Selected in WASP: " + vardsn);
                    isite++;
                    string svar = vardsn.Split(':')[0].ToString();
                    string dsn = vardsn.Split(':')[1].ToString();
                    int dsnum = Convert.ToInt32(dsn);

                    clsStation met = GetStationInfo(dsnum, svar);
                    Debug.WriteLine("{0},{1},{2},{3},{4}",
                        met.Station, met.StationName, met.Scenario, met.Latitude, met.Longitude);
                    string site = met.Station;

                    if (!(met == null))
                    {
                        //Debug.WriteLine("Inserting Station " + met.Station);
                        cSDB.InsertRecordInStationTable(met.Station, met.StationName, met.Scenario,
                            Convert.ToSingle(met.Latitude), Convert.ToSingle(met.Longitude),
                            Convert.ToSingle(met.Elevation));
                        //Debug.WriteLine("Inserted Station " + met.Station);

                        //Debug.WriteLine("Inserting PCODE " + svar);
                        cSDB.InsertRecordInPCODETable(svar);
                        Debug.WriteLine("Inserted PCODE " + met.Constituent);
                    }

                    dictSeries = cwdm.GetTimeSeries(dsnum);
                    //Debug.WriteLine("Series Count " + dictSeries.Count);

                    ConvertedSeries = ConvertSeriesUnits(svar, dictSeries);
                    fWASP.WriteStatus("Uploading " + met.Station + " records (" + isite.ToString() +
                        " of " + nsites.ToString() + " sites)");
                    //Debug.WriteLine("Uploading " + met.Station + " records (" + isite.ToString() +
                    //        " of " + nsites.ToString() + " sites)");

                    //create table if not existing
                    string tblName = "WASPMet";
                    if (!cSDB.TableExist(tblName)) cSDB.CreateTable(tblName);
                    //cSDB.InsertRecordsInMetTable(tblName, ConvertedSeries, svar, met.Station);
                    //Debug.WriteLine("Uploaded series DSN " + dsnum.ToString() + "-" + svar);

                    //103020
                    //get period of record for svar and site
                    int nRecsInDB = cSDB.GetPeriodOfRecord(tblName, svar, site);
                    //if nrecs > 0, get begin and ending dates and only upload records not in database
                    //else upload all records to database
                    if (nRecsInDB > 0)
                    {
                        DateBeg = cSDB.BeginRecordDate();
                        DateEnd = cSDB.EndingRecordDate();
                        ConvertedSeries = cSDB.FilterRecordsToUpload(DateBeg, DateEnd, ConvertedSeries);
                        cSDB.InsertRecordsInMetTable(tblName, ConvertedSeries, svar, site);
                    }
                    else
                    {
                        //insert series
                        //cSDB.DeleteRecordsFromMetTable(tblName, dictSeries, svar, site);
                        cSDB.InsertRecordsInMetTable(tblName, ConvertedSeries, svar, site);
                    }
                }
                fWASP.WriteStatus("Ready ..");

                //clean up
                cSDB.CloseDataBase();
                cSDB = null;
                cwdm = null;
                dictSeries = null;
                ConvertedSeries = null;

                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                errmsg = "Error uploading timeseries to " + SDBFile + crlf + crlf +
                    ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private SortedDictionary<DateTime, double> ConvertSeriesUnits(string svar, SortedDictionary<DateTime, double> dictSeries)
        {
            SortedDictionary<DateTime, double> ConvertedSeries = new SortedDictionary<DateTime, double>();
            switch (svar)
            {
                case "PREC": // inches
                    ConvertedSeries = dictSeries;
                    break;
                case "ATEM"://to Celcius
                    foreach (var kv in dictSeries)
                    {
                        double val = 5.0 * (kv.Value - 32.0) / 9.0;
                        ConvertedSeries.Add(kv.Key, val);
                    }
                    break;
                case "DEWP"://to Celcius
                    foreach (var kv in dictSeries)
                    {
                        double val = 5.0 * (kv.Value - 32.0) / 9.0;
                        ConvertedSeries.Add(kv.Key, val);
                    }
                    break;
                case "WIND"://to m/s
                    //ConvertedSeries = dictSeries;
                    foreach (var kv in dictSeries)
                    {
                        double val = kv.Value/2.237; 
                        ConvertedSeries.Add(kv.Key, val);
                    }
                    break;
                case "CLOU":// to 0-1 fraction
                    foreach (var kv in dictSeries)
                    {
                        double val = kv.Value * 0.1;
                        ConvertedSeries.Add(kv.Key, val);
                    }
                    break;
                case "SOLR"://to W/m2
                    foreach (var kv in dictSeries)
                    {
                        double val = kv.Value * 11.629935; 
                        ConvertedSeries.Add(kv.Key, val);
                    }
                    break;
            }
            return ConvertedSeries;
        }
        private clsStation GetStationInfo(int dsnum, string svar)
        {
            Debug.WriteLine("Entering GetStationInfo");
            SortedDictionary<int, clsStation> dsnGage = new SortedDictionary<int, clsStation>();
            clsStation cSta = new clsStation();

            try
            {
                dictGages.TryGetValue(svar, out dsnGage);
                //get station object for given dsn
                if (!dsnGage.TryGetValue(dsnum, out cSta))
                    return null;
                dsnGage = null;
                return cSta;
            }
            catch (Exception ex)
            {
                errmsg = "Cannnot find station with dsn " + dsnGage.ToString() + " in WDMFile";
                WriteMessage("Error!", errmsg);
                return null;
            }
        }
        private void WriteMessage(string msgtype, string msg)
        {
            switch (msgtype)
            {
                case "Error!":
                    MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "Warning!":
                    MessageBox.Show(msg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "Info!":
                    MessageBox.Show(msg, "Information!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
