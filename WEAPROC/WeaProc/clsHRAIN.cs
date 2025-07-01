//revision
//10-08-20 - add qa check for hourly rain
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using wdmuploader;
using WeaModel;
using WeaModelSDB;
using WeaWDM;
using WorldWind.Net;

namespace NCEIData
{
    class clsHRAIN
    {
        private frmMain fMain;
        private DateTime BegDate, EndDate;
        private TimeSpan td;
        private int MinYears;

        private string rainfile;
        private string cacheFolder, dataFolder;

        public Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        public Dictionary<string, string> dictMapVars = new Dictionary<string, string>();

        private List<string> lstSta;
        private List<string> DownloadedSites;
        private StreamWriter wrm;

        //data dictionaries
        //hourly data
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictData = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>>();
        //missing
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>>();
        //missing count
        public SortedDictionary<string, Dictionary<string, List<string>>> dictMissCnt = new
                    SortedDictionary<string, Dictionary<string, List<string>>>();

        //Markov model parameters
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>>();

        //dictionary of selected vars, excludes >%miss missing for a series 
        public SortedDictionary<string, List<string>> dictSelVars = new SortedDictionary<string, List<string>>();
        //dictionary of selected vars, excludes >%miss missing for a series 
        public SortedDictionary<string, List<string>> dictSiteVars;

        public List<string> lstStaDownloaded = new List<string>();
        public List<string> lstSelectedVars = new List<string>();
        public List<string> lstSelectedVarsRev = new List<string>();
        public List<string> lstIgnoredVars = new List<string>();

        private double PercentMiss;
        private int optDataset;
        //missing in file is -9999; change to 9999 to be compatible with ISD
        private string MISS = "9999";
        private double VarPercentMiss;
        private string sitevar = "PREC";
        private string WdmFile, ModelSDB;
        private int NumDaysRec;
        private string crlf = Environment.NewLine;
        private string modelDir;

        public clsHRAIN(frmMain _fmain, DateTime _bdate, DateTime _edate)
        {
            this.fMain = _fmain;
            fMain.scenario = "OBSERVED";
            this.BegDate = _bdate;
            this.EndDate = _edate;
            this.lstSta = fMain.lstSta;
            this.DownloadedSites = fMain.DownLoadedSites;
            this.optDataset = fMain.optDataSource;
            cacheFolder = System.IO.Path.Combine(fMain.cacheDir, "COOP");
            dataFolder = fMain.dataDir;
            this.PercentMiss = Convert.ToDouble(fMain.PercentMiss);
            this.WdmFile = fMain.WdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.modelDir = fMain.modelDir;
            //dict of station and variables downloaded (excludes series above threshold)
            //need to pass to frmdata
            this.dictSiteVars = fMain.dictSiteVars;
            this.MinYears = fMain.MinYears;
            NumDaysRec = (MinYears * 365 * 24) - 48; //two days buffer

        }
        public bool ProcessHRAINdata()
        {
            Cursor.Current = Cursors.WaitCursor;
            string urlpath = "https://www.ncei.noaa.gov/data/coop-hourly-precipitation/v2/access/";

            DateTime dtstart = DateTime.Now;
            int nsta = lstSta.Count;
            //log begin time
            fMain.WriteLogFile("Begin Download of COOP Hourly Rain Data ..." +
                DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            DownloadedSites.Clear();
            lstStaDownloaded.Clear();

            int isite = 0;
            int nSites = lstSta.Count;
            foreach (string station in lstSta)
            {
                isite++;
                WriteStatus("Downloading data for site: " + station + " (" + isite.ToString() + " of " + nSites.ToString() + ")");
                //string[] sta = station.Split(':');
                //string pcpsta = "USC00" + sta[1].ToString();
                //string site = pcpsta + ".csv";
                string site = station + ".csv";

                string url = string.Concat(urlpath, site);
                rainfile = Path.Combine(cacheFolder, site);

                //download if not existing
                if (!File.Exists(rainfile))
                {
                    bool results = DownloadData_HRAIN(url, rainfile);
                    if (results)
                    {
                        DownloadedSites.Add(site);
                        lstStaDownloaded.Add(station);
                    }
                    else
                    {
                        fMain.WriteLogFile("No data for site: " + site);
                    }
                }
                else
                {
                    fMain.WriteLogFile("Downloaded data for site: " + site);
                    DownloadedSites.Add(site);
                    lstStaDownloaded.Add(station);
                }

            }

            td = DateTime.Now - dtstart;
            fMain.WriteLogFile("End Download COOP Hourly Rain Data: " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.");
            WriteStatus("Ready ..");

            Cursor.Current = Cursors.Default;
            if (DownloadedSites.Count > 0)
            {
                return true;
            }
            else
            {
                //    string msg = "No data available for selected COOP Hourly Rain stations!";
                //    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    Cursor.Current = Cursors.Default;
                return false;
            }
        }
        private bool DownloadData_HRAIN(string url, string savefile)
        {
            Cursor.Current = Cursors.WaitCursor;

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
                //string msg = "Error downloading hourly precipitation data!";
                //ShowError(msg, ex);
            }

            Cursor.Current = Cursors.Default;
            if (!File.Exists(savefile))
                return false;
            else
                return true;
        }
        public bool ProcessHourlyRainOld()
        {
            Cursor.Current = Cursors.WaitCursor;

            int year, mon, day, ndays, nrecs, nSeriesCnt = 0;
            string[] sval = new string[25];
            string[] qacode = new string[25];
            string srline = string.Empty;
            string space = " ";
            DateTime dt;
            bool hasCOOP = false;

            SortedDictionary<DateTime, string> dictSeries;
            SortedDictionary<DateTime, string> dictMissing;
            Dictionary<string, SortedDictionary<string, double>> siteModel;
            Dictionary<string, List<string>> dictCnt;

            //daily data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarHly;
            //daily missing data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarMiss;

            string msg, dat;
            DateTime dtstart = DateTime.Now;
            int nsta = lstSta.Count;
            //log begin time
            msg = "Begin Processing hourly rainfall data ..." + DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToLongTimeString();
            fMain.WriteLogFile(msg);
            WriteStatus(msg);

            try
            {
                DateTime dtime;
                //initialize dictSiteVars
                dictSiteVars.Clear();
                foreach (string site in lstStaDownloaded)
                {
                    string sta = site + ".csv";
                    msg = "Reading hourly data for site " + site;
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);

                    dictVarHly = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictVarMiss = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictCnt = new Dictionary<string, List<string>>();

                    //Markov model parameters
                    siteModel = new Dictionary<string, SortedDictionary<string, double>>();

                    //WeaModelDB mdlDB = new WeaModelDB(ModelSDB);

                    string sfile = Path.Combine(cacheFolder, sta);
                    string[] srlines = System.IO.File.ReadAllLines(sfile);
                    int nlines = srlines.Count();

                    CsvProcessor csv = new CsvProcessor(BegDate,EndDate);
                    csv.ReadCsvFile(sfile, MISS);

                    Application.Exit();

                    dictSeries = new SortedDictionary<DateTime, string>();
                    dictMissing = new SortedDictionary<DateTime, string>();

                    //date format 1971-03-06
                    //loop through all lines

                    for (int jj = 1; jj < nlines; jj++)
                    {
                        srline = srlines[jj];
                        string[] scols = srline.Split(',');
                        year = Convert.ToInt32(scols[4].Substring(0, 4));
                        mon = Convert.ToInt32(scols[4].Substring(5, 2));
                        day = Convert.ToInt32(scols[4].Substring(8, 2));
                        dtime = new DateTime(year, mon, day);

                        //process only records within selected timeframe
                        if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                        {
                            ReadValues(srline, sval, qacode);
                            for (int i = 0; i < 24; i++)
                            {
                                dt = new DateTime(year, mon, day, i, 0, 0);
                                if (!dictSeries.Keys.Contains(dt))
                                {
                                    if (!(sval[i].Contains(MISS)) && qacode[i] == space)
                                    {
                                        double tp = Convert.ToDouble(sval[i]) / 100.0;
                                        dat = tp.ToString("#0.000");
                                    }
                                    else
                                    {
                                        dat = MISS;
                                        dictMissing.Add(dt, MISS);
                                    }
                                    //Debug.WriteLine("{0},{1}", dt.ToString(), dat);
                                    dictSeries.Add(dt, dat);
                                }
                            }
                        }
                    }
                    srlines = null;

                    //insert missing days
                    AddMissingHours(dictSeries, dictMissing, site, sitevar);

                    //count missing only if missing series count > 0
                    try
                    {
                        nrecs = dictSeries.Count();
                        nSeriesCnt = dictSeries.Keys.Count();
                        if (dictMissing.Count > 0)
                        {
                            //List is nrecs, num missing, percent missing, max hr missing
                            List<string> missCnt = new List<string>();
                            missCnt = CountMissingHours(site, sitevar, dictMissing, nrecs);
                            if (!(missCnt == null) || !(missCnt.Count == 0))
                            {
                                dictCnt.Add(sitevar, missCnt);
                                VarPercentMiss = Convert.ToDouble(missCnt.ElementAt(3));
                                Debug.WriteLine("Percent Miss of {0}, {1} : {2}", site, sitevar, VarPercentMiss.ToString("F2"));
                            }
                            missCnt = null;
                        }
                        else
                        {
                            List<string> missCnt = new List<string>() { nrecs.ToString(), "0", "0", "0" };
                            dictCnt.Add(sitevar, missCnt);
                            missCnt = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = "Error getting missing counts for " + sitevar + " for" + site + " !";
                        ShowError(msg, ex);
                    }

                    //only add to dictionaries and compute statistics if count > 0 and
                    //if percent missing is less than threshold
                    if (nSeriesCnt > NumDaysRec && VarPercentMiss < PercentMiss)
                    {
                        //create model log file
                        //string spath = Path.Combine(Application.StartupPath, "data");
                        string ModelLogFile = Path.Combine(modelDir, site + "_model.log");
                        wrm = new StreamWriter(ModelLogFile, true);
                        wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                                BegDate.Date.ToShortDateString() + " - " + EndDate.Date.ToShortDateString() + " : " +
                                DateTime.Now.ToString());
                        wrm.AutoFlush = true;

                        dictVarHly.Add(sitevar, dictSeries);
                        dictVarMiss.Add(sitevar, dictMissing);

                        SortedDictionary<string, double> varmodel = new SortedDictionary<string, double>();
                        Cursor.Current = Cursors.WaitCursor;
                        {
                            fMain.appManager.UpdateProgress("Fitting Markov model for site " + site + ":" + sitevar);
                            Markov cModel = new Markov(site, sitevar, dictSeries, wrm);
                            cModel.FitMarkovModel(0);
                            varmodel = cModel.MarkovModel();
                            cModel = null;
                            fMain.appManager.UpdateProgress("Saving Markov model parameters for site " + site + ":" + sitevar);
                            //SaveToDB(0, varmodel, mdlDB, sitevar, site);
                        }
                        Cursor.Current = Cursors.Default;
                        siteModel.Add(sitevar, varmodel);
                        varmodel = null;

                        wrm.Flush();
                        wrm.Close();
                        wrm.Dispose();

                        //calculate daily,monthly and annual statistics for a variable
                        //fMain.appManager.UpdateProgress("Calculating Statistics for site " + site + " ...");
                        //debug -------------------------------------
                        //Debug.WriteLine("Hourly series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                        //Debug.WriteLine("Hourly missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                        //debug -------------------------------------
                    }

                    dictSeries = null;
                    dictMissing = null;
                    //end loop for site

                    //Save files
                    if (dictVarHly.Keys.Count > 0)
                    {
                        //SaveHourlyData("hly", site, dictVarHly);
                        if (!dictMiss.ContainsKey(site)) dictMiss.Add(site, dictVarMiss);
                        if (!dictMissCnt.ContainsKey(site)) dictMissCnt.Add(site, dictCnt);
                        if (!dictModel.ContainsKey(site)) dictModel.Add(site, siteModel);

                        //save to wdm                            
                        UploadToWDM(WdmFile, site, dictVarHly);
                        List<string> svars = new List<string>();
                        svars = dictVarHly.Keys.ToList();
                        if (!dictSiteVars.ContainsKey(site))
                            dictSiteVars.Add(site, svars);
                        if (!lstStaDownloaded.Contains(site))
                            lstStaDownloaded.Add(site);
                        svars = null;

                        //WDM cWDM = new WDM(WdmFile, optDataset);
                        //clsStation coop = cWDM.GetSiteInfo(site);
                        //Debug.WriteLine("{0},{1},{2},{3}", site, sta.StationName, sta.Latitude, sta.Longitude);
                        //cWDM = null;

                        //ModelDB
                        //ModelDB mdlDB = new ModelDB(ModelSDB);
                        //mdlDB.InsertRecordsInStationTable(site, coop.StationName, "COOP",
                        //     Convert.ToSingle(coop.Latitude), Convert.ToSingle(coop.Longitude),
                        //     Convert.ToSingle(coop.Elevation), null, null);

                    }
                    fMain.dictSiteVars = dictSiteVars;

                    dictVarHly = null; dictVarMiss = null; dictCnt = null;
                    siteModel = null;

                    msg = "End reading hourly precipitation data for " + site + " from " +
                           BegDate.Date.ToString() + " to " + EndDate.Date.ToString();
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);
                    //mdlDB = null;
                }
                //end loop station

                //Save stochastic model parameters to sdbfile
                UploadModelToSDB();

                //show num sites downloaded and processed
                int nsites = dictSiteVars.Keys.Count;
                if (nsites > 0)
                {
                    StringBuilder st = new StringBuilder();
                    st.Append("Downloaded and processed data for " + nsites.ToString() + " COOP hourly rainfall station(s)" + crlf + crlf);
                    msg = st.ToString();
                    fMain.WriteLogFile("Downloaded and processed data for " + nsites.ToString() + " COOP hourly rainfall station(s)");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    st = null;
                    hasCOOP = true;
                }
                else
                {
                    msg = "No data available for selected COOP hourly rainfall stations!";
                    fMain.WriteLogFile("No data available for selected COOP hourly rainfall stations!");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    hasCOOP = false;
                }
                WriteStatus("Ready ...");
            }
            catch (Exception ex)
            {
                msg = "Error reading hourly precipitaion file...";
                ShowError(msg, ex);
                return false;
            }

            td = DateTime.Now - dtstart;
            msg = "End Processing hourly precipitation data ... " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.";
            fMain.WriteLogFile(msg);
            WriteStatus(msg);
            WriteStatus("Ready ...");

            Cursor.Current = Cursors.Default;
            return hasCOOP;
        }

        public bool ProcessHourlyRain()
        {
            Cursor.Current = Cursors.WaitCursor;

            int nrecs, nSeriesCnt = 0;
            DateTime dt;
            bool hasCOOP = false;

            SortedDictionary<DateTime, string> dictSeries;
            SortedDictionary<DateTime, string> dictMissing;
            Dictionary<string, SortedDictionary<string, double>> siteModel;
            Dictionary<string, List<string>> dictCnt;

            //daily data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarHly;
            //daily missing data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarMiss;

            string msg, dat;
            DateTime dtstart = DateTime.Now;
            int nsta = lstSta.Count;
            //log begin time
            msg = "Begin Processing hourly rainfall data ..." + DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToLongTimeString();
            fMain.WriteLogFile(msg);
            WriteStatus(msg);

            try
            {
                DateTime dtime;
                //initialize dictSiteVars
                dictSiteVars.Clear();
                foreach (string site in lstStaDownloaded)
                {
                    string sta = site + ".csv";
                    msg = "Reading hourly data for site " + site;
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);

                    dictVarHly = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictVarMiss = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictCnt = new Dictionary<string, List<string>>();

                    //Markov model parameters
                    siteModel = new Dictionary<string, SortedDictionary<string, double>>();

                    //WeaModelDB mdlDB = new WeaModelDB(ModelSDB);
                    dictSeries = new SortedDictionary<DateTime, string>();
                    dictMissing = new SortedDictionary<DateTime, string>();

                    string sfile = Path.Combine(cacheFolder, sta);
                    //string[] srlines = System.IO.File.ReadAllLines(sfile);
                    //int nlines = srlines.Count();

                    CsvProcessor csv = new CsvProcessor(BegDate, EndDate);
                    dictSeries = csv.ReadCsvFile(sfile,MISS);

                    //insert missing days
                    AddMissingHours(dictSeries, dictMissing, site, sitevar);
                    //Application.Exit();

                    //count missing only if missing series count > 0
                    try
                    {
                        nrecs = dictSeries.Count();
                        nSeriesCnt = dictSeries.Keys.Count();
                        if (dictMissing.Count > 0)
                        {
                            //List is nrecs, num missing, percent missing, max hr missing
                            List<string> missCnt = new List<string>();
                            missCnt = CountMissingHours(site, sitevar, dictMissing, nrecs);
                            if (!(missCnt == null) || !(missCnt.Count == 0))
                            {
                                dictCnt.Add(sitevar, missCnt);
                                VarPercentMiss = Convert.ToDouble(missCnt.ElementAt(3));
                                Debug.WriteLine("Percent Miss of {0}, {1} : {2}", site, sitevar, VarPercentMiss.ToString("F2"));
                            }
                            missCnt = null;
                        }
                        else
                        {
                            List<string> missCnt = new List<string>() { nrecs.ToString(), "0", "0", "0" };
                            dictCnt.Add(sitevar, missCnt);
                            missCnt = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = "Error getting missing counts for " + sitevar + " for" + site + " !";
                        ShowError(msg, ex);
                    }

                    //only add to dictionaries and compute statistics if count > 0 and
                    //if percent missing is less than threshold
                    if (nSeriesCnt > NumDaysRec && VarPercentMiss < PercentMiss)
                    {
                        //create model log file
                        //string spath = Path.Combine(Application.StartupPath, "data");
                        string ModelLogFile = Path.Combine(modelDir, site + "_model.log");
                        wrm = new StreamWriter(ModelLogFile, true);
                        wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                                BegDate.Date.ToShortDateString() + " - " + EndDate.Date.ToShortDateString() + " : " +
                                DateTime.Now.ToString());
                        wrm.AutoFlush = true;

                        dictVarHly.Add(sitevar, dictSeries);
                        dictVarMiss.Add(sitevar, dictMissing);

                        SortedDictionary<string, double> varmodel = new SortedDictionary<string, double>();
                        Cursor.Current = Cursors.WaitCursor;
                        {
                            fMain.appManager.UpdateProgress("Fitting Markov model for site " + site + ":" + sitevar);
                            Markov cModel = new Markov(site, sitevar, dictSeries, wrm);
                            cModel.FitMarkovModel(0);
                            varmodel = cModel.MarkovModel();
                            cModel = null;
                            fMain.appManager.UpdateProgress("Saving Markov model parameters for site " + site + ":" + sitevar);
                            //SaveToDB(0, varmodel, mdlDB, sitevar, site);
                        }
                        Cursor.Current = Cursors.Default;
                        siteModel.Add(sitevar, varmodel);
                        varmodel = null;

                        wrm.Flush();
                        wrm.Close();
                        wrm.Dispose();

                        //calculate daily,monthly and annual statistics for a variable
                        //fMain.appManager.UpdateProgress("Calculating Statistics for site " + site + " ...");
                        //debug -------------------------------------
                        //Debug.WriteLine("Hourly series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                        //Debug.WriteLine("Hourly missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                        //debug -------------------------------------
                    }

                    dictSeries = null;
                    dictMissing = null;
                    //end loop for site

                    //Save files
                    if (dictVarHly.Keys.Count > 0)
                    {
                        //SaveHourlyData("hly", site, dictVarHly);
                        if (!dictMiss.ContainsKey(site)) dictMiss.Add(site, dictVarMiss);
                        if (!dictMissCnt.ContainsKey(site)) dictMissCnt.Add(site, dictCnt);
                        if (!dictModel.ContainsKey(site)) dictModel.Add(site, siteModel);

                        //save to wdm                            
                        UploadToWDM(WdmFile, site, dictVarHly);
                        List<string> svars = new List<string>();
                        svars = dictVarHly.Keys.ToList();
                        if (!dictSiteVars.ContainsKey(site))
                            dictSiteVars.Add(site, svars);
                        if (!lstStaDownloaded.Contains(site))
                            lstStaDownloaded.Add(site);
                        svars = null;

                        //WDM cWDM = new WDM(WdmFile, optDataset);
                        //clsStation coop = cWDM.GetSiteInfo(site);
                        //Debug.WriteLine("{0},{1},{2},{3}", site, sta.StationName, sta.Latitude, sta.Longitude);
                        //cWDM = null;

                        //ModelDB
                        //ModelDB mdlDB = new ModelDB(ModelSDB);
                        //mdlDB.InsertRecordsInStationTable(site, coop.StationName, "COOP",
                        //     Convert.ToSingle(coop.Latitude), Convert.ToSingle(coop.Longitude),
                        //     Convert.ToSingle(coop.Elevation), null, null);

                    }
                    fMain.dictSiteVars = dictSiteVars;

                    dictVarHly = null; dictVarMiss = null; dictCnt = null;
                    siteModel = null;

                    msg = "End reading hourly precipitation data for " + site + " from " +
                           BegDate.Date.ToString() + " to " + EndDate.Date.ToString();
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);
                    //mdlDB = null;
                }
                //end loop station

                //Save stochastic model parameters to sdbfile
                UploadModelToSDB();

                //show num sites downloaded and processed
                int nsites = dictSiteVars.Keys.Count;
                if (nsites > 0)
                {
                    StringBuilder st = new StringBuilder();
                    st.Append("Downloaded and processed data for " + nsites.ToString() + " COOP hourly rainfall station(s)" + crlf + crlf);
                    msg = st.ToString();
                    fMain.WriteLogFile("Downloaded and processed data for " + nsites.ToString() + " COOP hourly rainfall station(s)");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    st = null;
                    hasCOOP = true;
                }
                else
                {
                    msg = "No data available for selected COOP hourly rainfall stations!";
                    fMain.WriteLogFile("No data available for selected COOP hourly rainfall stations!");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    hasCOOP = false;
                }
                WriteStatus("Ready ...");
            }
            catch (Exception ex)
            {
                msg = "Error reading hourly precipitaion file...";
                ShowError(msg, ex);
                return false;
            }

            td = DateTime.Now - dtstart;
            msg = "End Processing hourly precipitation data ... " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.";
            fMain.WriteLogFile(msg);
            WriteStatus(msg);
            WriteStatus("Ready ...");

            Cursor.Current = Cursors.Default;
            return hasCOOP;
        }

        private void SaveToDBold(int tstep, SortedDictionary<string, double> varModel, WeaModelDB mdlDB,
                                string svar, string site)
        {
            Debug.WriteLine(Environment.NewLine + "Entering SaveToDB ....");
            Debug.WriteLine("VarModel variable count = " + varModel.Count.ToString());

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (KeyValuePair<string, double> st in varModel)
                {
                    //Debug.WriteLine("{0},{1},{2}", irec.ToString(),st.Key, st.Value.ToString());
                    //switch (tstep)
                    //{
                    //    case (int)TimeStep.Hourly:
                    //string[] stvar = st.Key.Split('_');
                    double val = st.Value;
                    //Debug.WriteLine("{0},{1},{2}", stvar[0].ToString(), stvar[1].ToString(), val.ToString());
                    mdlDB.InsertRecordsInModelTable(site, svar, st.Key, val, 0);
                    //        break;
                    //    case (int)TimeStep.Daily:
                    //        break;
                    //}
                }
            }
            catch (Exception ex)
            {
                string errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private bool UploadToWDM(string wdmfile, string site, Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<string> siteAttrib = new List<string>();
                fMain.dictSta.TryGetValue(site, out siteAttrib);

                clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib,
                                         dictSiteWea, wdmfile, fMain.optDataSource,"Hour");
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
        private void ReadValues(string srline, string[] sval, string[] qacode)
        {
            //col 7,  8,     9 ... and so on
            //HR00Val,HR00MF,HR00QF ....
            try
            {
                string[] scols = srline.Split(',');
                for (int i = 1; i <= 24; i++)
                {
                    sval[i - 1] = string.Empty;
                    qacode[i - 1] = string.Empty;
                    sval[i - 1] = scols[i * 5 + 1];
                    qacode[i - 1] = scols[i * 5 + 3];
                }
                //daily sum
                sval[24] = scols[126];
            }
            catch (Exception ex)
            {

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
        private List<string> CountMissingHours(string site, string svar,
                             SortedDictionary<DateTime, string> dicMissing, int nrecs)
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

                lstIgnoredVars.Clear();
                for (int i = 0; i < lstSelectedVars.Count; i++)
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

        #region "SaveData"
        public void SaveHourlyData(string ext, string site, Dictionary<string, SortedDictionary<DateTime, string>> dicthourly)
        {
            //exports hourly with missing, filled and missing data to csv
            //ext=hly - raw hourly
            //ext=hlyest - hourly with estimate
            //ext=mis - missing records

            string sFile;
            sFile = SetFileName(ext, site);
            string sPath = Path.Combine(fMain.dataDir, sFile);

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
        private void WriteData(StreamWriter wri, string site, string svar, SortedDictionary<DateTime, double> siteData)
        {
            foreach (var dat in siteData)
            {
                StringBuilder st = new StringBuilder();
                DateTime dt = dat.Key;
                double val = dat.Value;

                st.Append(site + "," + svar + "," + dt.Date.ToString() + ",");
                st.Append(val.ToString("F3"));

                int len = st.ToString().Length;
                wri.WriteLine(st.ToString().Substring(0, len - 1));
                wri.Flush();
            }
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
        #endregion

        public void ShowDataTable(List<string> lstSelectedVars)
        {
            //debug----------------------------
            //foreach (var it in lstSelectedVars)
            //    Debug.WriteLine("In ShowDataTable:var = " + it);
            //debug----------------------------
            //if (dictData.Keys.Count > 0)
            {
                frmData fdata = new frmData(fMain, dictMiss,
                    dictModel, null, dictMissCnt, dictSelVars, lstSelectedVars, dictOptVars);
                if (fdata.ShowDialog() == DialogResult.OK)
                {
                    //092920
                    fdata = null;
                    fdata.Dispose();
                }
            }
            //else
            {
                //    string msg = "There are no stations and variables within threshold of missing records!";
                //    MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void WriteStatus(string msg)
        {
            fMain.appManager.UpdateProgress(msg);
        }
        public Dictionary<string, bool> OptionVars
        {
            set { dictOptVars = value; }
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
                    //Debug.WriteLine("{0},{1},{2},{3}", site, sta.StationName,sta.Latitude, sta.Longitude);

                    mdlDB.InsertRecordsInStationTable(site, sta.StationName, "COOP",
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
                    //Debug.WriteLine("{0},{1},{2}", irec.ToString(),st.Key, st.Value.ToString());
                    //switch (tstep)
                    //{
                    //    case (int)TimeStep.Hourly:
                    //string[] stvar = st.Key.Split('_');
                    double val = st.Value;
                    //Debug.WriteLine("{0},{1},{2}", stvar[0].ToString(), stvar[1].ToString(), val.ToString());
                    //mdlDB.DeleteRecordsFromModelTable(site, svar, st.Key, val, 0);
                    mdlDB.InsertRecordsInModelTable(site, svar, st.Key, val, 0);
                    //        break;
                    //    case (int)TimeStep.Daily:
                    //        break;
                    //}
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
