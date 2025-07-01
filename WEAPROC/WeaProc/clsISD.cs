//Revision
//10.01.2020 -back to set of vars download, instead of individual
//11.16.2020 -added test for null varmodel, hmoments
//06.22.2021 -adjust for time zone
//
//#define debug

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using wdmuploader;
using WeaDB;
using WeaModel;
using WeaModelSDB;
using WeaWDM;
using WorldWind.Net;

namespace NCEIData
{
    class clsISD
    {
        private frmMain fMain;
        private string begdate, enddate;
        private int MinYears;
        private string MISS = "9999", MISSCLO = "99";
        private TimeSpan td;
        private List<string> lstSta;
        //private string lstOfsta = string.Empty;
        private bool SaveFiles;
        public Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        public Dictionary<string, string> dictMapVars = new Dictionary<string, string>();
        public Dictionary<string, string> dictJSONFiles = new Dictionary<string, string>();
        private string cachePath, tmpWDM, WdmFile, ModelSDB, SQLdbFile;
        private string dataDir, modelDir;
        private float TimeZoneShift;

        private string hlyFile = string.Empty;
        private int NumDaysRec;
        private string crlf = Environment.NewLine;
        private DateTime BegDateTime, EndDateTime;
        private StreamWriter wrm;

        //data dictionaries
        //missing
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>>();
        //missing count
        public SortedDictionary<string, Dictionary<string, List<string>>> dictMissCnt = new
                    SortedDictionary<string, Dictionary<string, List<string>>>();

        //ARmodel parameters
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>>();
        //harmonic moments
        private SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> dictHMoments = new
            SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>>();

        public SortedDictionary<string, MetGages> dictSelSites =
                 new SortedDictionary<string, MetGages>();

        //dictionary of selected vars, excludes >%miss missing for a series 
        public SortedDictionary<string, List<string>> dictSiteVars;

        public List<string> lstStaDownloaded = new List<string>();
        public List<string> lstSelectedVars = new List<string>();
        public List<string> DownLoadedSites = new List<string>();

        public int PercentMiss;
        public List<string> lstIgnoredVars = new List<string>();
        private int optDataset;
        public clsISD(frmMain _fmain, string _bdate, string _edate)
        {
            this.fMain = _fmain;
            fMain.scenario = "OBSERVED";
            this.begdate = _bdate;
            this.enddate = _edate;
            this.BegDateTime = fMain.BegDateTime;
            this.EndDateTime = fMain.EndDateTime;
            this.lstSta = fMain.lstSta;
            this.PercentMiss = fMain.PercentMiss;
            this.optDataset = fMain.optDataSource;
            this.tmpWDM = fMain.WdmFile;
            this.cachePath = System.IO.Path.Combine(fMain.cacheDir, "ISD"); //fMain.cacheDir;
            this.DownLoadedSites = fMain.DownLoadedSites;
            this.lstStaDownloaded = fMain.lstStaDownloaded;
            this.WdmFile = fMain.WdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.SQLdbFile = fMain.SQLdbFile;
            this.dataDir = fMain.dataDir;
            this.modelDir = fMain.modelDir;
            //dict of station and variables downloaded (excludes series above threshold)
            //need to pass to frmdata
            this.dictSiteVars = fMain.dictSiteVars;
            this.MinYears = fMain.MinYears;
            NumDaysRec = MinYears * 365;
            fMain.WriteLogFile("Data Period: From " + BegDateTime.ToShortDateString() + " to " +
                EndDateTime.ToShortDateString());
            //Debug.WriteLine("Data Period: From " + BegDateTime.ToShortDateString() + " to " +
            //    EndDateTime.ToShortDateString());

            this.dictSelSites = fMain.dictSelSites;
            this.TimeZoneShift = 0.0F;

            SetVarMapping();
        }
        private void SetVarMapping()
        {
            dictMapVars.Add("TMP", "ATEM");
            dictMapVars.Add("WND", "WIND");
            dictMapVars.Add("WDR", "WNDD");
            dictMapVars.Add("GA1", "CLOU");
            dictMapVars.Add("DEW", "DEWP");
            dictMapVars.Add("AA1", "PREC");
            dictMapVars.Add("SLP", "ATMP");
            //10022020 add solar and net radiation
            dictMapVars.Add("GH1", "SOLR");
            dictMapVars.Add("GO1", "RNET");
        }
        private string SetJSONFileName(string site, DateTime begdt, DateTime enddt)
        {
            double dtbeg = begdt.ToOADate();
            double dtend = enddt.ToOADate();

            string sitevars = string.Empty;
            for (int i = 0; i < lstSelectedVars.Count; i++)
                sitevars += lstSelectedVars.ElementAt(i) + "_";

            string sfile = site + "_" + sitevars + dtbeg.ToString() + "_" + dtend.ToString() + ".json";
            return (Path.Combine(cachePath, sfile));
        }
        public void ProcessISDdataRev()
        {
            Cursor.Current = Cursors.WaitCursor;

            DateTime dtstart = DateTime.Now; ;
            int nsta = lstSta.Count();
            //log begin time
            fMain.WriteLogFile("Begin Processing ISD Data ..." + DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            //build list of vars to download----------------
            StringBuilder lst = new StringBuilder();
            List<string> dKeys = dictOptVars.Keys.ToList();
            List<bool> dVals = dictOptVars.Values.ToList();

            int nkeys = dKeys.Count();
            for (int i = 0; i < nkeys; i++)
                if (dVals[i]) lst.Append(dKeys[i] + ",");
            string selVars = lst.ToString().Substring(0, lst.ToString().Length - 1);
            fMain.WriteLogFile("Variables to download " + selVars);
            dKeys = null; dVals = null; lst = null;
            //end build list vars----------------------------

            //get list of selected vars from the download screen
            lstSelectedVars.Clear();
            bool option; string svar;
            foreach (KeyValuePair<string, bool> kv in dictOptVars)
            {
                dictOptVars.TryGetValue(kv.Key, out option);
                if (option)
                {
                    dictMapVars.TryGetValue(kv.Key, out svar);
                    lstSelectedVars.Add(svar);
                }
            }

            //download and process each station
            lstStaDownloaded.Clear();
            dictSiteVars.Clear();

            int isite = 0;
            foreach (string site in lstSta)
            {
                TimeZoneShift = GetTimeZoneOfGrid(site);
//#if debug
                Debug.WriteLine("TimeShift=" + TimeZoneShift.ToString());
//#endif
                //dict of site and component time series, downloaded raw series
                Dictionary<string, SortedDictionary<DateTime, string>> dictRaw =
                        new Dictionary<string, SortedDictionary<DateTime, string>>();

                //dict of site and component time series, hourly with missing and each
                //site save to <station>_hly.csv file
                Dictionary<string, SortedDictionary<DateTime, string>> dictVarHly =
                        new Dictionary<string, SortedDictionary<DateTime, string>>();
                int nsites = lstSta.Count();
                isite++;

                td = DateTime.Now - dtstart;
                fMain.WriteLogFile(crlf + "Begin Download Data for " + site + ":" +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalSeconds.ToString("F4") + " seconds.");

                string JSONfile = SetJSONFileName(site, BegDateTime, EndDateTime);
                bool isDnloaded = DownloadData_ISDFile(JSONfile, selVars, site, isite, nsites);

                td = DateTime.Now - dtstart;
                fMain.WriteLogFile("End Download Data for " + site + ":" +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalSeconds.ToString("F4") + " seconds.");

                if (isDnloaded)
                {
                    fMain.WriteStatus("Processing data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                    if (ProcessSiteDownloadedDataRev(JSONfile, site, dictRaw))
                    {
                        //process each series in the site
                        //dictVarHly is the hourly site data, process only if there are variables
                        if (dictRaw.Keys.Count > 0)
                        {
                            dictVarHly = ProcessSiteHourlyData(site, dictRaw);
                            if (dictVarHly.Keys.Count > 0)
                            {
                                //upload raw hourly data with missing data
                                UploadToWDM(tmpWDM, site, dictVarHly);
                                //UploadToSQLiteDB(SQLdbFile, site, dictVarHly);
                                List<string> svars = new List<string>();
                                svars = dictVarHly.Keys.ToList();
                                if (!dictSiteVars.ContainsKey(site))
                                    dictSiteVars.Add(site, svars);
                                svars = null;
                            }
                        }
                    }
                }
                dictRaw = null;
                dictVarHly = null;
            }

            //save model parameters to SDB database
            UploadModelToSDB();

            //check sites downloaded
            lstStaDownloaded = dictSiteVars.Keys.ToList();
            fMain.lstStaDownloaded = lstStaDownloaded;

            int isdsites = dictSiteVars.Keys.Count;
            if (isdsites > 0)
            {
                string msg = "Downloaded and processed data for " + isdsites.ToString() +
                    " ISD Station(s)" + crlf + crlf;
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            else
            {
                string msg = "No data for selected ISD sites (Please check threshold for missing records and years!" + crlf + crlf;
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Cursor.Current = Cursors.Default;
            }

            //show frmData screen
            if (dictSiteVars.Keys.Count > 0)
                ShowDataTable(lstSelectedVars);

            td = DateTime.Now - dtstart;
            fMain.WriteLogFile("End Download and Processing ISD Data: " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.");
        }

        private SortedDictionary<DateTime, string> ShiftDateTime(int iTZone, SortedDictionary<DateTime, string> rawSeries)
        {
            SortedDictionary<DateTime, string> ShiftedSeries = new SortedDictionary<DateTime, string>();
            string sval = string.Empty;
            DateTime dt;

            foreach(KeyValuePair<DateTime,string>kv in rawSeries)
            {
                dt = kv.Key.AddHours(iTZone);
                sval = kv.Value;
                ShiftedSeries.Add(dt, sval);
            }
            return ShiftedSeries;
        }
        /// <summary>
        /// UploadToWDM
        /// </summary>
        /// <param name="wdmfile"></param> WDMFile
        /// <param name="site"></param> Station
        /// <param name="dictSiteWea"></param> dictionary of series
        /// <returns></returns>
        private bool UploadToWDM(string wdmfile, string site, Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea)
        {
            WriteStatus("Uploading dataseries for station " + site + "!");
            fMain.WriteLogFile("Uploading dataseries for station " + site + "!");
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<string> siteAttrib = new List<string>();
                fMain.dictSta.TryGetValue(site, out siteAttrib);

                clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib, dictSiteWea, wdmfile, fMain.optDataSource,"hour");
                cWDM.UploadSeriesToWDM(site, wdmfile);
                cWDM = null;
                siteAttrib = null;
                Cursor.Current = Cursors.Default;

                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in uploading timeseries for site:" + site;
                ShowError(msg, ex);
                return false;
            }

        }
        private bool UploadToSQLiteDB(string sqldb, string site,
                Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea)
        {
            WriteStatus("Uploading dataseries for station " + site + "!");
            fMain.WriteLogFile("Uploading dataseries for station " + site + "!");
            SortedDictionary<DateTime, double> dictSeries;
            SortedDictionary<DateTime, string> strSeries;

            List<string> siteAttrib = new List<string>();
            string mssg = string.Empty;
            DateTime dtbeg, dtend;
            float lat, lon, elev;
            string stname, scen = string.Empty;

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                fMain.dictSta.TryGetValue(site, out siteAttrib);
                int nvars = dictSiteWea.Keys.Count();

                stname = siteAttrib[0].ToString();
                elev = Convert.ToSingle(siteAttrib[1]);
                lat = Convert.ToSingle(siteAttrib[2]);
                lon = Convert.ToSingle(siteAttrib[3]);

                //initialize cSDB
                WeaSDB cSDB = new WeaSDB(sqldb);
                cSDB.InsertRecordInStationTable(site, stname, scen, lat, lon, elev);

                int ivar = 0;
                foreach (var kv in dictSiteWea)
                {
                    ivar++;
                    string svar = kv.Key;
                    cSDB.InsertRecordInPCODETable(svar);
                    strSeries = new SortedDictionary<DateTime, string>();
                    dictSeries = new SortedDictionary<DateTime, double>();
                    dictSiteWea.TryGetValue(kv.Key, out strSeries);
                    foreach (var kv1 in strSeries)
                        dictSeries.Add(kv1.Key, Convert.ToDouble(kv1.Value));
                    strSeries = null;

                    mssg = "Uploading to db -" + site + ":" + svar + " records (" + ivar.ToString() +
                                    " of " + nvars.ToString() + " variables)";

                    WriteStatus(mssg);
                    fMain.WriteLogFile(mssg);

                    string tblName = "Met";
                    //get period of record for svar and site
                    int nRecsInDB = cSDB.GetPeriodOfRecord(tblName, svar, site);
                    //if nrecs > 0, get begin and ending dates and only upload records not in database
                    //else upload all records to database
                    if (nRecsInDB > 0)
                    {
                        dtbeg = cSDB.BeginRecordDate();
                        dtend = cSDB.EndingRecordDate();
                        dictSeries = cSDB.FilterRecordsToUpload(dtbeg, dtend, dictSeries);
                        cSDB.InsertRecordsInMetTable(tblName, dictSeries, svar, site);
                    }
                    else
                    {
                        //insert series
                        //cSDB.DeleteRecordsFromMetTable(tblName, dictSeries, svar, site);
                        cSDB.InsertRecordsInMetTable(tblName, dictSeries, svar, site);
                    }

                    dictSeries = null;
                }
                cSDB = null;
                siteAttrib = null;
                Cursor.Current = Cursors.Default;

                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in uploading timeseries for site:" + site;
                ShowError(msg, ex);
                return false;
            }
        }
        /// <summary>
        /// DownloadData_ISD
        /// downloads selected site data through webservice
        /// </summary>
        /// <param name="selVars"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool DownloadData_ISDFile(string JSONfile, string selVars, string site, int isite, int nsites)
        {
            bool isDnLoaded = false;
            dictJSONFiles.Clear();
            try
            {
                fMain.WriteLogFile("Downloading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                WriteStatus("Downloading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                double dtbeg = BegDateTime.ToOADate();
                double dtend = EndDateTime.ToOADate();
                string[] svars = selVars.Split(',');
                string sfile = string.Empty;
                string metvar = string.Empty;
                string sitevars = string.Empty;

                for (int i = 0; i < svars.Length; i++)
                    sitevars += svars[i] + "_";

                //for WDR use WND               
                //for(int i=0;i<svars.Length;i++)
                {
                    //string sitevar = svars[i];
                    //dictMapVars.TryGetValue(sitevar, out metvar);

                    WriteStatus("Downloading " + selVars + " for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");

                    //build query------------------------------------
                    StringBuilder qrys = new StringBuilder();
                    qrys.Append("https://www.ncei.noaa.gov/access/services/data/v1?");
                    //qrys.Append("dataset=global-hourly&dataTypes=WND,TMP,DEW,GA1,AA1");
                    qrys.Append("dataset=global-hourly&dataTypes=");
                    qrys.Append(selVars);
                    //qrys.Append(sitevar);
                    qrys.Append("&stations=");
                    qrys.Append(site);
                    qrys.Append("&startDate=");
                    qrys.Append(begdate);
                    qrys.Append("&endDate=");
                    qrys.Append(enddate);
                    qrys.Append("&format=json&options=includeAttributes:false,includeStationName:1");
                    string qrystr = qrys.ToString();
                    //fMain.WriteLogFile("Query " + qrystr);
                    //end build query---------------------------------

                    //uses worldwind.net download file, if existing don't download
                    //sfile = JSONfile;
                    //Debug.Write("JSONfile=" + JSONfile);
                    FileInfo fi = new FileInfo(JSONfile);
                    if (!File.Exists(JSONfile) || fi.Length < 10)
                    {
                        //download if not existing or existing but < 10 bytes
                        isDnLoaded = DownloadISD(qrystr, JSONfile);
                        if (isDnLoaded)
                        {
                            //fMain.WriteLogFile("Downloaded data for station " + site + " : " + sitevar);
                            fMain.WriteLogFile("Downloaded data for station " + site + " : " + sitevars);
                        }
                        else
                        {
                            //fMain.WriteLogFile("Error Downloading data for station " + site + " : " + sitevar);
                            fMain.WriteLogFile("Error Downloading data for station " + site + " : " + sitevars);
                            //dictJSONFiles.Remove(sitevar);
                        }
                    }
                    else
                    {
                        //fMain.WriteLogFile("Reading from cache data for station " + site + " : " + sitevar);
                        fMain.WriteLogFile("Reading from cache data for station " + site + " : " + sitevars);
                        isDnLoaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Cannot execute query, Server may be down!";
                ShowError(msg, ex);
                fMain.WriteLogFile(msg + ex);
                isDnLoaded = false;
            }
            Cursor.Current = Cursors.Default;
            return isDnLoaded;
        }
        /// <summary>
        /// With using cache
        /// </summary>
        /// <param name="selVars"></param>
        /// <param name="site"></param>
        /// <param name="isite"></param>
        /// <param name="nsites"></param>
        /// <returns></returns>
        private string DownloadData_ISD(string selVars, string site, int isite, int nsites)
        {
            string results = string.Empty;
            //build query------------------------------------
            StringBuilder qrys = new StringBuilder();
            qrys.Append("https://www.ncei.noaa.gov/access/services/data/v1?");
            //qrys.Append("dataset=global-hourly&dataTypes=WND,TMP,DEW,GA1,AA1");
            qrys.Append("dataset=global-hourly&dataTypes=");
            qrys.Append(selVars);
            qrys.Append("&stations=");
            qrys.Append(site);
            qrys.Append("&startDate=");
            qrys.Append(begdate);
            qrys.Append("&endDate=");
            qrys.Append(enddate);
            qrys.Append("&format=json&options=includeAttributes:false,includeStationName:1");
            string qrystr = qrys.ToString();
            fMain.WriteLogFile(crlf + "Query " + qrystr);
            //end build query---------------------------------

            //try
            {
                //WriteStatus("Downloading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                //results = Download(qrystr);

                //write query results to file
                double dtbeg = BegDateTime.ToOADate();
                double dtend = EndDateTime.ToOADate();

                string svars = site;
                string[] vars = selVars.Split(',');
                for (int j = 0; j < vars.Length; j++)
                    svars = svars + "_" + vars[j];

                string sfile = svars + "_" + dtbeg.ToString() + "_" + dtend.ToString() + ".json";
                string jsonfile = Path.Combine(cachePath, sfile);
                FileInfo fi = new FileInfo(jsonfile);
                if (fi.Exists && fi.Length < 10) fi.Delete();

                if (!File.Exists(jsonfile))
                {
                    try
                    {
                        WriteStatus("Downloading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                        //download if not existing
                        results = Download(qrystr);
                        StreamWriter wr = new StreamWriter(jsonfile);
                        wr.Write(results);
                        wr.Flush();
                        wr.Close();
                        wr = null;
                        //return (results);
                    }
                    catch (Exception ex)
                    {
                        string msg = "Cannot execute query, Server may be down!";
                        ShowError(msg, ex);
                        fMain.WriteLogFile(msg + ex);
                        results = null;
                    }
                }
                else
                {
                    try
                    {
                        string res = string.Empty;
                        if (fi.Length > 10)
                        {
                            //read file
                            fMain.WriteLogFile("Reading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");
                            WriteStatus("Reading data for station " + site + "(" + isite.ToString() + " of " + nsites.ToString() + ")");

                            //using (StreamReader sreader = File.OpenText(jsonfile))
                            //{
                            //    res = sreader.ReadToEnd();
                            //}

                            StringBuilder st = new StringBuilder();
                            using (StreamReader sread = new StreamReader(jsonfile))
                            {
                                //092920
                                while (sread.Peek() >= 0)
                                {
                                    string line = sread.ReadLine();
                                    st.Append(line);
                                }
                            }

                            res = st.ToString();
                            st = null;
                        }
                        results = res;
                        res = null;
                    }
                    catch (Exception ex)
                    {
                        string msg = "Error reading cache file " + jsonfile + "!";
                        //ShowError(msg, ex);
                        fMain.WriteLogFile(msg + ex);
                        results = null;
                    }
                }
            }
            Cursor.Current = Cursors.Default;
            return (results);
        }
        private string[] ReadAllLinesFromFile(string jsonfile)
        {
            string[] lines = File.ReadAllLines(jsonfile);
            return lines;
        }
        private string ReadLineFromFile(StreamReader sread)
        {
            string line = sread.ReadLine();
            Debug.WriteLine(line);
            if (line.Contains("[") || line.Contains("]"))
                return string.Empty;
            else if (line[0].Equals(","))
                return line.Substring(1);
            else
                return line;
        }
        private string ParseLine(string line, int irec)
        {
            string sline = string.Empty;
            //Debug.WriteLine(line);
            if (line.Contains("[") || line.Contains("]"))
                return string.Empty;

            if (irec > 1)
                return line.Substring(1);
            else
                return line;
        }
        private bool ProcessSiteDownloadedDataRev(string JSONfile, string site, Dictionary<string, SortedDictionary<DateTime, string>> dictVar)
        {
            Debug.WriteLine("Entering ProcessSiteDownloadedDataRev ...");
            Debug.WriteLine("jsonfile=" + JSONfile);
            Cursor.Current = Cursors.WaitCursor;
            DateTime dtBegRec, dtEndRec;
            string svar = string.Empty;
            SortedDictionary<DateTime, string> dictSeries;

            string sta = string.Empty;
            string pdate, clo, tmp, dew, wnd, aa1, msg, metvar, slp;
            string[] str;
            bool option;
            int nseries = lstSelectedVars.Count;
            int msec = 1;

            DateTime dtbeg = DateTime.Now;
            fMain.WriteLogFile(crlf + "Begin Processing Data for station  " + site + " :" +
                   DateTime.Now.ToShortDateString() + " " +
                   DateTime.Now.ToLongTimeString());
            dtbeg = DateTime.Now;
            //fMain.WriteLogFile("Site: " + site + ", Record Count=" + isd.Count.ToString() +
            //    ",BegDate=" + isd.First["DATE"].ToString() + ",EndDate=" + isd.Last["DATE"].ToString());

            //initialize dictionaries
            dictVar.Clear();
            foreach (string item in lstSelectedVars)
            {
                dictSeries = new SortedDictionary<DateTime, string>();
                if (!dictVar.ContainsKey(item))
                    dictVar.Add(item, dictSeries);
                dictSeries = null;
            }

            try
            {
                int icnt = 0, yr, oldyr = 1900;
                string json = string.Empty;

                //svar = lstSelectedVars.ElementAt(ivar);
                //dtBegRec = DateTime.Parse(isd.First["DATE"].ToString());
                //dtEndRec = DateTime.Parse(isd.Last["DATE"].ToString());

                //fMain.WriteLogFile("SiteVar: " + site + "_"+svar+", Record Count=" + isd.Count.ToString() +
                //    ",BegDate=" + isd.First["DATE"].ToString() + ",EndDate=" + isd.Last["DATE"].ToString());

                string jItem = string.Empty;
                string line;

                using (StreamReader sr = new StreamReader(JSONfile))
                {
                    int irec = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string atoken = ParseLine(line, irec);
                        irec++;

                        if (!string.IsNullOrEmpty(atoken))
                        {
                            icnt++;
                            JToken avar = JToken.Parse(atoken);
                            jItem = avar.ToString();
                            pdate = avar["DATE"].ToString();
                            DateTime dt = DateTime.Parse(pdate);
                            //10022020
                            if ((jItem.Contains("GH1") || jItem.Contains("GO1")))
                                Debug.WriteLine("Radiation in " + site);

                            yr = Convert.ToDateTime(pdate).Year;
                            if (yr > oldyr)
                            {
                                msg = "Processing " + site + " records: " + yr.ToString();
                                fMain.WriteLogFile(msg);
                                WriteStatus(msg);
                            }

                            //cloud section --------------------------
                            dictOptVars.TryGetValue("GA1", out option);
                            dictMapVars.TryGetValue("GA1", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                clo = string.Empty;
                                if (jItem.Contains("GA1"))
                                {
                                    str = avar["GA1"].ToString().Split(',');
                                    clo = MISS;
                                    if (!str[0].Contains(MISSCLO))
                                    {
                                        int cl = Convert.ToInt32(str[0]);
                                        clo = ReclassCloud(cl);
                                    }
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, clo);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, clo);
                                    }
                                }
                                //else //{ //clo = MISS;//}
                            }

                            //temperature section----------------------
                            dictOptVars.TryGetValue("TMP", out option);
                            dictMapVars.TryGetValue("TMP", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                tmp = string.Empty;
                                if (jItem.Contains("TMP"))
                                {
                                    str = avar["TMP"].ToString().Split(',');
                                    tmp = MISS;
                                    if (!str[0].Contains(MISS))
                                    {
                                        double tp = 1.8 * (Convert.ToDouble(str[0]) / 10.0) + 32.0;
                                        tmp = tp.ToString("0.0");
                                    }
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, tmp);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, tmp);
                                    }
                                }
                                //else //{ //tmp = MISS; //}
                            }

                            //dewpoint section ------------------------
                            dictOptVars.TryGetValue("DEW", out option);
                            dictMapVars.TryGetValue("DEW", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                dew = string.Empty;
                                if (jItem.Contains("DEW"))
                                {
                                    str = avar["DEW"].ToString().Split(',');
                                    dew = MISS;
                                    if (!str[0].Contains(MISS))
                                    {
                                        double tp = 1.8 * (Convert.ToDouble(str[0]) / 10.0) + 32.0;
                                        dew = tp.ToString("0.0");
                                    }
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, dew);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, dew);
                                    }
                                }
                                //else { dew = MISS; }
                            }

                            //wind section ----------------------------
                            dictOptVars.TryGetValue("WND", out option);
                            dictMapVars.TryGetValue("WND", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                wnd = string.Empty;
                                if (jItem.Contains("WND"))
                                {
                                    str = avar["WND"].ToString().Split(',');
                                    wnd = MISS;
                                    if (!str[3].Contains(MISS))
                                        wnd = (2.237*Convert.ToSingle(str[3]) / 10.0).ToString("0.0");
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, wnd);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, wnd);
                                    }
                                }
                                //else { wnd = MISS; }
                            }

                            //wind direction section ----------------------------
                            dictOptVars.TryGetValue("WND", out option);
                            dictMapVars.TryGetValue("WDR", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                wnd = string.Empty;
                                if (jItem.Contains("WND"))
                                {
                                    str = avar["WND"].ToString().Split(',');
                                    wnd = MISS;
                                    if (!str[0].Contains("999"))
                                        wnd = str[0];
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, wnd);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, wnd);
                                    }
                                }
                                //else { wnd = MISS; }
                            }

                            //pressure section ----------------------------
                            dictOptVars.TryGetValue("SLP", out option);
                            dictMapVars.TryGetValue("SLP", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                slp = string.Empty;
                                if (jItem.Contains("SLP"))
                                {
                                    str = avar["SLP"].ToString().Split(',');
                                    slp = MISS;
                                    if (!str[0].Contains(MISS))
                                        slp = (0.750062 * Convert.ToSingle(str[0]) / 10.0).ToString("0.0");
                                    if (!dictSeries.ContainsKey(dt))
                                        dictSeries.Add(dt, slp);
                                    else
                                    {
                                        int isec = msec++;
                                        DateTime dtadd = dt.AddMilliseconds(isec);
                                        if (!dictSeries.ContainsKey(dtadd))
                                            dictSeries.Add(dtadd, slp);
                                    }
                                }
                                //else { slp = MISS; }
                            }

                            //precipitation section ------------------
                            dictOptVars.TryGetValue("AA1", out option);
                            dictMapVars.TryGetValue("AA1", out metvar);
                            dictVar.TryGetValue(metvar, out dictSeries);
                            if (option)
                            {
                                aa1 = string.Empty;
                                if (jItem.Contains("AA1"))
                                {
                                    str = avar["AA1"].ToString().Split(',');
                                    //100820 correction, QA flags 1,5,9 means passed quality checks
                                    int intval = Convert.ToInt32(str[0]);
                                    if (intval == 1 && (str[3].Contains("5")))
                                    {
                                        aa1 = MISS;
                                        if (!str[1].Contains(MISS))
                                        {
                                            double p = Convert.ToDouble(str[1]) / 254.0;
                                            aa1 = p.ToString("0.000");
                                        }
                                        if (!dictSeries.ContainsKey(dt))
                                            dictSeries.Add(dt, aa1);
                                        //else
                                        //{
                                        //    int isec = msec++;
                                        //    DateTime dtadd = dt.AddMilliseconds(isec);
                                        //    if (!dictSeries.ContainsKey(dtadd))
                                        //        dictSeries.Add(dtadd, aa1);
                                        //}
                                    }
                                }
                                //else { aa1 = MISS; }
                            }
                            oldyr = yr;
                            jItem = null;
                            avar = null;
                        }
                    }
                }

                //remove null series
                List<string> RemoveSeries = new List<string>();
                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dictVar)
                {
                    dictSeries = kv.Value;
                    if (!(dictSeries.Keys.Count > 0))
                        RemoveSeries.Add(kv.Key);
                    //fMain.WriteLogFile(kv.Key + ": Num Recs " + dictSeries.Keys.Count.ToString());
                }

                foreach (var skey in RemoveSeries)
                {
                    dictVar.Remove(skey);
                    fMain.WriteLogFile("Remove series " + skey + " from " + site);
                    Debug.WriteLine("Remove series " + skey + " from " + site);
                }

                //msg = "Processed " + site + ": " + icnt.ToString() + " records";
                //WriteStatus(msg);
                //isd = null;

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End Processing Data for station  " + site + " :" +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (System.OutOfMemoryException exs)
            {
                fMain.WriteLogFile("System.OutOfMemoryException! " + crlf + exs.Message + crlf + exs.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                fMain.WriteLogFile("Error in processing downloaded data for station  " + site);
                ShowError("Error in processing downloaded data for station  " + site, ex);
                return false;
            }
            //debug---------------
        }
        /// <summary>
        /// Generate hourly data from raw file and uploads to a WDM file
        /// </summary>
        /// <param name="site"></param>
        /// <param name="dictSiteVar"></param>
        /// <returns></returns>
        public Dictionary<string, SortedDictionary<DateTime, string>> ProcessSiteHourlyData(
               string site, Dictionary<string, SortedDictionary<DateTime, string>> dictSiteVar)
        {
            string cursite = string.Empty;
            double VarPercentMiss = 0.0;
            int nCountThreshold = (NumDaysRec * 24) - 24; // 2days buffer

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DateTime dtbeg = DateTime.Now;
                fMain.WriteLogFile(crlf + "Begin Processing Hourly Data :" + DateTime.Now.ToShortDateString() + " " +
                                    DateTime.Now.ToLongTimeString());
                dtbeg = DateTime.Now;

                SortedDictionary<DateTime, string> dictSeries, dictHourly, dictMissing;
                Dictionary<string, List<string>> dictCnt;
                //hourly data and missing dictionary
                Dictionary<string, SortedDictionary<DateTime, string>> dictVarHly, dictVarMiss;

                //model dictionaries
                Dictionary<string, SortedDictionary<string, double>> siteModel;
                Dictionary<string, SortedDictionary<Int32, List<double>>> siteHMoments;
                SortedDictionary<Int32, List<double>> hMoments; //periodic stats for site/var

                DateTime curdt, prevdt, curday, curhr, prevhr;
                int curyr, prevyr;
                TimeSpan td;
                double nhrs = 0;
                int nrecs = 0;

                // Process each var in a site
                cursite = site;

                fMain.WriteStatus("Processing Hourly ISD Data for station  " + site + ".");

                dictVarHly = new Dictionary<string, SortedDictionary<DateTime, string>>();
                dictVarMiss = new Dictionary<string, SortedDictionary<DateTime, string>>();
                dictCnt = new Dictionary<string, List<string>>();

                siteModel = new Dictionary<string, SortedDictionary<string, double>>();
                siteHMoments = new Dictionary<string, SortedDictionary<Int32, List<double>>>();

                List<string> lstDownloadedVars = dictSiteVar.Keys.ToList();
                //foreach (string s in lstDownloadedVars)
                //Debug.WriteLine("Downloaded Var is " + s);
                //loop each variable for a station

                foreach (string svar in lstDownloadedVars)
                {
                    fMain.WriteLogFile(crlf + "Processing Hourly Data for station  " + site + ":" + svar);
                    dictSeries = new SortedDictionary<DateTime, string>();
                    dictHourly = new SortedDictionary<DateTime, string>();
                    dictMissing = new SortedDictionary<DateTime, string>();
                    hMoments = new SortedDictionary<Int32, List<double>>();

                    //series for site and variable, adjust for time zone
                    SortedDictionary<DateTime, string> rawSeries = new SortedDictionary<DateTime, string>();

                    int iTZone = (int)Math.Round(TimeZoneShift, 0, MidpointRounding.AwayFromZero);
                    dictSiteVar.TryGetValue(svar, out rawSeries);
                    dictSeries = ShiftDateTime(iTZone, rawSeries);
#if debug
                    for (int i = 0; i < 48; i++)
                        Debug.WriteLine("{0},{1}", rawSeries.Keys.ToArray()[i].ToString(),
                            dictSeries.Keys.ToArray()[i].ToString());
#endif
                    rawSeries = null;

                    prevdt = dictSeries.Keys.ElementAt(0);
                    prevhr = GetDateTime(prevdt);

                    //process loop for series
                    double sum = 0, cnt = 0.0;
                    VarPercentMiss = 0.0;
                    foreach (KeyValuePair<DateTime, string> kv in dictSeries)
                    {
                        curdt = kv.Key;
                        if (DateTime.Compare(curdt, BegDateTime) >= 0 && DateTime.Compare(curdt, EndDateTime) <= 0)
                        {
                            curhr = GetDateTime(curdt);
                            curday = curdt.Date; //just date no time
                            curyr = curdt.Year;

                            td = curhr - prevhr;
                            if ((nhrs = td.TotalHours) > 0)
                            {
                                //new hour
                                if (cnt > 0)
                                {
                                    //100220 correction
                                    if (!svar.Contains("PREC"))
                                        sum /= cnt;
                                    //add to hourly series dictionary
                                    if (!dictHourly.ContainsKey(prevhr))
                                        dictHourly.Add(prevhr, sum.ToString("F2"));
                                }
                                else
                                {
                                    sum = 9999;
                                    if (!dictMissing.ContainsKey(prevhr))
                                        dictMissing.Add(prevhr, MISS);
                                    if (!dictHourly.ContainsKey(prevhr))
                                        dictHourly.Add(prevhr, MISS);
                                }

                                if (!kv.Value.Contains(MISS))
                                {
                                    sum = Convert.ToDouble(kv.Value);
                                    cnt = 1.0;
                                }
                                else
                                {
                                    cnt = 0.0;
                                    sum = 0.0;
                                }
                            }
                            else //same hour
                            {
                                if (!kv.Value.Contains(MISS))
                                {
                                    sum += Convert.ToDouble(kv.Value);
                                    cnt += 1.0;
                                }
                            }
                            prevhr = curhr;
                            prevyr = curyr;
                        }
                    }
                    //end of rawdata (site data) loop
                    //last record
                    if (cnt > 0)
                    {
                        //100220 correction
                        if (!svar.Contains("PREC"))
                            sum /= cnt;
                        //add to hourly series dictionary
                        if (!dictHourly.ContainsKey(prevhr))
                            dictHourly.Add(prevhr, sum.ToString("F2"));
                    }
                    else
                    {
                        sum = 9999;
                        if (!dictMissing.ContainsKey(prevhr))
                            dictMissing.Add(prevhr, MISS);
                        if (!dictHourly.ContainsKey(prevhr))
                            dictHourly.Add(prevhr, MISS);
                    }

                    //insert missing hours, raw series often includes missing hours
                    AddMissingHours(dictHourly, dictMissing, site, svar);
                    dictSeries = null;

                    //count missing only if missing series count > 0
                    nrecs = dictHourly.Count();
                    try
                    {
                        if (dictMissing.Count > 0)
                        {
                            //List is nrecs, num missing, percent missing, max hr missing
                            List<string> missCnt = new List<string>();
                            missCnt = CountMissingHours(site, svar, dictMissing, nrecs);
                            if (!(missCnt == null) || !(missCnt.Count == 0))
                            {
                                dictCnt.Add(svar, missCnt);
                                VarPercentMiss = Convert.ToDouble(missCnt.ElementAt(3));
                                Debug.WriteLine("Percent Miss of {0}, {1} : {2}", site, svar, VarPercentMiss.ToString("F2"));
                            }
                            missCnt = null;
                        }
                        else
                        {
                            List<string> missCnt = new List<string>() { nrecs.ToString(), "0", "0", "0" };
                            dictCnt.Add(svar, missCnt);
                            missCnt = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "Error getting missing counts for " + svar + " for" + cursite + " !";
                        ShowError(msg, ex);
                    }

                    //now that we get complete hourly with missing records for the set period
                    //see if meets minimum hourly record count
                    int ncount = dictHourly.Keys.Count;
                    if (ncount >= nCountThreshold)
                    {
                        //add to dictionaries and model missing only if percent missing is less than user threshold
                        if (nrecs > 0 && VarPercentMiss < PercentMiss)
                        {
                            dictVarHly.Add(svar, dictHourly);
                            dictVarMiss.Add(svar, dictMissing);

                            //create model log file
                            string ModelLogFile = Path.Combine(modelDir, site + "_model.log");
                            wrm = new StreamWriter(ModelLogFile, true);
                            wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                                BegDateTime.Date.ToShortDateString() + " - " + EndDateTime.Date.ToShortDateString() + " : " +
                                DateTime.Now.ToString());
                            wrm.AutoFlush = true;

                            //varmodel is dictionary of model parameters, see readme
                            SortedDictionary<string, double> varmodel = new SortedDictionary<string, double>();
                            Cursor.Current = Cursors.WaitCursor;
                            if (!svar.Contains("PREC"))
                            {
                                fMain.appManager.UpdateProgress("Fitting AR model for site " + site + ":" + svar);
                                LinearAR cModel = new LinearAR(site, svar, dictHourly, wrm);
                                cModel.FitLinearModel(0);
                                varmodel = cModel.ARmodel();
                                hMoments = cModel.HarmonicMoments();
                                cModel = null;
                            }
                            else //if rain
                            {
                                fMain.appManager.UpdateProgress("Fitting Markov model for site " + site + ":" + svar);
                                Markov cModel = new Markov(site, svar, dictHourly, wrm);
                                cModel.FitMarkovModel(0);
                                varmodel = cModel.MarkovModel();
                                cModel = null;
                            }

                            wrm.Flush();
                            wrm.Close();
                            wrm.Dispose();

                            if (!(varmodel == null))
                                siteModel.Add(svar, varmodel);
                            if (!(hMoments == null))
                                siteHMoments.Add(svar, hMoments);
                            varmodel = null; hMoments = null;

                            fMain.appManager.UpdateProgress("Ready ...");
                            Cursor.Current = Cursors.Default;
                        }
                        dictHourly = null;
                        dictMissing = null;
                    }
                }
                //end loop of variables for a station
                //save files to csv and add to global dictionaries
                if (dictVarHly.Keys.Count > 0)
                {
                    SaveHourlyData("hly", site, dictVarHly);
                    if (!dictMiss.ContainsKey(site)) dictMiss.Add(site, dictVarMiss);
                    if (!dictMissCnt.ContainsKey(site)) dictMissCnt.Add(site, dictCnt);
                    if (!dictModel.ContainsKey(site)) dictModel.Add(site, siteModel);
                    if (!dictHMoments.ContainsKey(site)) dictHMoments.Add(site, siteHMoments);
                }

                dictVarMiss = null; dictCnt = null;
                siteModel = null; siteHMoments = null;
                //End processing site

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End Processing Hourly Data for station: " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                fMain.appManager.UpdateProgress("Ready ...");

                Cursor.Current = Cursors.Default;
                return dictVarHly;
            }
            catch (Exception ex)
            {
                string msg = "Error processing hourly data for " + cursite + " !";
                ShowError(msg, ex);
                return null;
            }
        }
        private void AddMissingHours(SortedDictionary<DateTime, string> dicHourly,
                SortedDictionary<DateTime, string> dicMissing,
                string sta, string svar)
        {
            try
            {
                double nhrs = 0;
                DateTime dt;
                TimeSpan td;

                List<DateTime> lstdt = dicHourly.Keys.ToList();
                List<string> lstdat = dicHourly.Values.ToList();

                for (int i = 1; i < lstdt.Count; i++)
                {
                    td = lstdt[i] - lstdt[i - 1];
                    if ((nhrs = td.TotalHours) > 1)
                    {
                        //Debug.WriteLine("Inserting hours of " + nhrs.ToString() + " for " + svar + " of site " + sta + ", " +
                        //    lstdt[i].ToString() + ", " + lstdt[i - 1].ToString());
                        for (int j = 1; j <= nhrs - 1; j++)
                        {
                            dt = lstdt[i - 1].AddHours(j);
                            if (!dicMissing.ContainsKey(dt))
                                dicMissing.Add(dt, MISS);
                            if (!dicHourly.ContainsKey(dt))
                                dicHourly.Add(dt, MISS);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private List<string> CountMissingHours(string site, string svar, SortedDictionary<DateTime, string> dicMissing, int nrecs)
        {
            List<string> lstCntMiss = new List<string>();
            try
            {
                List<DateTime> lstdt = dicMissing.Keys.ToList();
                List<string> lstdat = dicMissing.Values.ToList();

                string msg;
                double percent = 0;

                int mxhrs = 0; int ncount = 0; int curmxhr = 0;
                //check first record
                if (lstdat[0].Contains(MISS))
                {
                    curmxhr = 1; mxhrs = 1; ncount = 1;
                }
                else
                {
                    curmxhr = 0; mxhrs = 0;
                }

                for (int i = 1; i < lstdt.Count; i++)
                {
                    if (lstdat[i].Contains(MISS))
                    {
                        TimeSpan ts = lstdt[i] - lstdt[i - 1];
                        if (ts.TotalHours == 1)
                        {
                            curmxhr += 1;
                            mxhrs = (mxhrs > curmxhr) ? mxhrs : curmxhr;
                        }
                        else
                        {
                            mxhrs = (mxhrs > curmxhr) ? mxhrs : curmxhr;
                            //Debug.WriteLine("{0},{1},{2},{3}", ts.TotalHours.ToString(), i.ToString(),
                            //    curmxhr.ToString(), mxhrs.ToString());
                            curmxhr = 1;
                        }
                        ncount += 1;
                    }
                    else
                    {
                        mxhrs = (mxhrs > curmxhr) ? mxhrs : curmxhr;
                        curmxhr = 0;
                    }
                }
                //add to list
                lstCntMiss.Add(nrecs.ToString());
                lstCntMiss.Add(ncount.ToString());
                lstCntMiss.Add(mxhrs.ToString());
                percent = Convert.ToDouble(ncount * 100) / nrecs;
                lstCntMiss.Add(percent.ToString("F2"));

                msg = "Count of missing : " + ncount.ToString() + ", Max  = " + mxhrs.ToString() +
                                ", Percent = " + percent.ToString("F2") + " for site " + site;
                fMain.WriteLogFile(msg);
                Debug.WriteLine("Count of missing Var = {0}, Max = {1}, Percent = {2} for site {3}",
                     ncount.ToString(), mxhrs.ToString(), percent.ToString("F2"), site);

                //add list to dictionary

                //lstIgnoredVars.Clear();
                //for (int i = 0; i < lstSelectedVars.Count; i++)
                {
                    //percent = lstPercent.ElementAt(i);
                    //if (percent >= PercentMiss)
                    //    lstIgnoredVars.Add(lstSelectedVars[i]);
                }
                //debug
                //foreach (var item in lstIgnoredVars)
                //    Debug.WriteLine("In CountMiss Routine, Ignore Var=" + item);
                //lstCnt = null; lstMaxHr = null;
                return lstCntMiss;
            }
            catch (Exception ex)
            {
                ShowError("Error summarizing missing records!", ex);
                return null;
            }
        }
        private DateTime GetDateTime(DateTime sdate)
        {
            //DateTime dt = Convert.ToDateTime(sdate);
            DateTime dtnew;

            int yr = sdate.Year;
            int mo = sdate.Month;
            int da = sdate.Day;
            int hr = sdate.Hour;

            dtnew = new DateTime(yr, mo, da, hr, 0, 0);
            return dtnew;
        }
        public void SaveHourlyData(string ext, string site, Dictionary<string, SortedDictionary<DateTime, string>> dicthourly)
        {
            //exports hourly with missing, filled and missing data to csv
            //ext=hly - raw hourly
            //ext=hlyest - hourly with estimate
            //ext=mis - missing records

            string sFile;
            sFile = SetFileName(ext, site);
            string sPath = Path.Combine(fMain.dataDir, sFile);
            hlyFile = sPath;


            StreamWriter wri = new StreamWriter(sPath);
            wri.AutoFlush = true;

            //write header
            WriteHeader(wri);

            //write hourly data to csv: site, svar, datetime, value
            SortedDictionary<DateTime, string> siteData;
            foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dicthourly)
            {
                string svar = kv.Key;
                dicthourly.TryGetValue(svar, out siteData);
                WriteHlyData(wri, site, svar, siteData);
            }

            wri.Flush();
            wri.Close();
            wri.Dispose();
        }
        private string SetFileName(string sopt, string site)
        {
            string sfile = string.Empty;
            sfile = site.ToString() + "_" + sopt + ".csv";
            return sfile;
        }
        private void WriteHeader(StreamWriter wri)
        {
            string head = "Station_ID, Variable, DateTime, Value";
            wri.WriteLine(head);
            wri.Flush();
        }
        private void WriteHlyData(StreamWriter wri, string site, string svar, SortedDictionary<DateTime, string> siteData)
        {
            foreach (var dat in siteData)
            {
                StringBuilder st = new StringBuilder();
                DateTime dt = dat.Key;
                double val = Convert.ToDouble(dat.Value);

                st.Append(site + "," + svar + "," + dt.ToString() + ",");
                st.Append(val.ToString("F3"));

                int len = st.ToString().Length;
                wri.WriteLine(st.ToString().Substring(0, len - 1));
                wri.Flush();
            }
        }
        public void ShowDataTable(List<string> lstSelectedVars)
        {
            {
                frmData fdata = new frmData(fMain, dictMiss,
                    dictModel, dictHMoments, dictMissCnt, dictSiteVars, lstSelectedVars, dictOptVars);
                if (fdata.ShowDialog() == DialogResult.OK)
                {
                    fdata = null;
                    fdata.Dispose();
                }
            }
        }
        private string ReclassCloud(int cloud)
        {
            string clcover = string.Empty;
            switch (cloud)
            {
                case 0:
                    return ("0.0");
                case 1:
                    return ("1.0");
                case 2:
                    return ("2.5");
                case 3:
                    return ("4.0");
                case 4:
                    return ("5.0");
                case 5:
                    return ("6.0");
                case 6:
                    return ("7.5");
                case 7:
                    return ("9.0");
                case 8:
                    return ("10.0");
                case 9:
                    return ("9999");
            }
            return ("9999");
        }
        private string Download(string uri_address)
        {
            Cursor.Current = Cursors.WaitCursor;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string reply = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                //wc.Timeout = 600 * 60 * 1000;
                reply = wc.DownloadString(uri_address);
                if (!wc.IsBusy)
                    wc.Dispose();
            }
            catch (System.OutOfMemoryException exs)
            {
                reply = null;
                return null;
            }
            catch (Exception ex)
            {
                string msg = "Error in NCEI ISD service!";
                ShowError(msg, ex);
                reply = null;
            }
            Cursor.Current = Cursors.Default;
            return (reply);
        }
        private bool DownloadISD(string url, string savefile)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool isExist;

            WebDownload webGet = new WebDownload();
            try
            {
                webGet.Url = url;
                webGet.DownloadFile(savefile);
                //wait for download to complete before returning

                while (webGet.IsDownloadInProgress)
                    System.Threading.Thread.Sleep(50);

                if (!webGet.IsComplete)
                    //Logger.Dbg("Waiting another half second for completion")
                    System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                string msg = "Error NCEI ISD data download!";
                ShowError(msg, ex);
            }

            Cursor.Current = Cursors.Default;
            FileInfo fi = new FileInfo(savefile);
            if (!fi.Exists)
                isExist = false;
            else if (fi.Length < 10)
            {
                isExist = false;
                fi.Delete();
            }
            else
                isExist = true;

            return isExist;
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + crlf + ex.Message + crlf + crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void WriteStatus(string msg)
        {
            fMain.WriteStatus(msg);
        }
        public bool SaveMultipleFiles
        {
            set { SaveFiles = value; }
        }
        public Dictionary<string, bool> OptionVars
        {
            set { dictOptVars = value; }
        }
        private float GetTimeZoneOfGrid(string grid)
        {
            float tz = 0.0F;
            try
            {
                MetGages sta = new MetGages();
                dictSelSites.TryGetValue(grid, out sta);
                return Convert.ToSingle(sta.TZONE);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

#region "Save Model to DB"
        private void UploadModelToSDB()
        {
            string svar, site;

            //Get site info
            WDM cWDM = new WDM(WdmFile, optDataset);
            //ModelDB
            WeaModelDB mdlDB = new WeaModelDB(fMain.wrlog, ModelSDB);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Dictionary<string, SortedDictionary<string, double>> sitemodel;
                SortedDictionary<string, double> varmodel;

                //iterate for all sites in dictModel
                foreach (var kvsite in dictModel)
                {
                    site = kvsite.Key;
                    sitemodel = kvsite.Value;

                    fMain.WriteStatus("Saving stochastic model parameters for site " + site);
                    fMain.WriteLogFile("Saving stochastic model parameters for site " + site);

                    clsStation sta = cWDM.GetSiteInfo(site);
                    Debug.WriteLine("{0},{1},{2},{3}", site, sta.StationName,
                        sta.Latitude, sta.Longitude);

                    mdlDB.InsertRecordsInStationTable(site, sta.StationName, "ISD",
                            Convert.ToSingle(sta.Latitude), Convert.ToSingle(sta.Longitude),
                            Convert.ToSingle(sta.Elevation), null, null);
                    sta = null;

                    //iterate for all vars in sitemodel
                    foreach (var kvmdl in sitemodel)
                    {
                        svar = kvmdl.Key;
                        varmodel = kvmdl.Value;
                        SaveToDB(0, varmodel, mdlDB, svar, site);
                    }
                }

                //cleanup
                sitemodel = null;
                varmodel = null;
                cWDM = null;
                mdlDB.CloseDataBase();
                mdlDB = null;
            }
            catch (Exception ex)
            {

            }
            Cursor.Current = Cursors.Default;
        }
        private void SaveToDB(int tstep, SortedDictionary<string, double> varModel, WeaModelDB mdlDB,
            string svar, string site)
        {
            Debug.WriteLine(Environment.NewLine + "Entering SaveToDB ....");
            Debug.WriteLine("VarModel variable count = " + varModel.Count.ToString());

            try
            {
                mdlDB.DeleteRecordsFromModelTable(site, svar, 0);
                foreach (KeyValuePair<string, double> st in varModel)
                {
                    double val = st.Value;
                    //Debug.WriteLine("{0},{1},{2}", stvar[0].ToString(), stvar[1].ToString(), val.ToString());
                    mdlDB.InsertRecordsInModelTable(site, svar, st.Key, val, 0);
                }
            }
            catch (Exception ex)
            {
                string errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

#endregion
    }
}
