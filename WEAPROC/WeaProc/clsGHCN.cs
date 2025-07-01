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
    class clsGHCN
    {
        private frmMain fMain;
        private DateTime BegDate, EndDate;
        private TimeSpan td;
        private int MinYears;

        private List<string> lstSta;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        private string ghcnfile;
        public string cacheFolder, dataFolder;
        private List<string> DownloadedSites;
        private List<string> GHCNVars = new List<string>() { "TMAX", "TMIN", "PRCP", "EVAP"};
        private const string MISS = "-9999";
        private const string NEWMISS = "9999";
        private double VarPercentMiss;
        private int optDataset;
        private string WdmFile, ModelSDB;
        private int NumDaysRec;
        private string crlf = Environment.NewLine;
        private StreamWriter wrm;
        private string modelDir;
        private WeaSeries GHCNseries=new WeaSeries();

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

        //dictionary of selected vars, excludes >%miss missing for a series 
        public SortedDictionary<string, List<string>> dictSelVars = new SortedDictionary<string, List<string>>();
        //dictionary of selected vars, excludes >%miss missing for a series 
        public SortedDictionary<string, List<string>> dictSiteVars;

        public List<string> lstStaDownloaded = new List<string>();
        public List<string> lstSelectedVars = new List<string>();
        public List<string> lstSelectedVarsRev = new List<string>();
        public List<string> lstIgnoredVars = new List<string>();

        private double PercentMiss;

        public clsGHCN(frmMain _fmain, DateTime _bdate, DateTime _edate)
        {
            this.fMain = _fmain;
            fMain.weatherTS = this.GHCNseries;

            fMain.scenario = "OBSERVED";
            this.BegDate = _bdate;
            this.EndDate = _edate;
            this.lstSta = fMain.lstSta;
            this.DownloadedSites = fMain.DownLoadedSites;
            this.lstSelectedVars = fMain.lstSelectedVars;
            cacheFolder = System.IO.Path.Combine(fMain.cacheDir, "GHCN");
            dataFolder = fMain.dataDir;
            this.PercentMiss = fMain.PercentMiss;
            this.optDataset = fMain.optDataSource;
            this.WdmFile = fMain.WdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.modelDir = fMain.modelDir;

            //dict of station and variables downloaded (excludes series above threshold)
            //need to pass to frmdata
            this.dictSiteVars = fMain.dictSiteVars;
            this.MinYears = fMain.MinYears;
            NumDaysRec = MinYears * 365;
        }
        public int ProcessGHCNdata()
        {
            int nsites = 0;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string urlpath = "ftp://ftp.ncdc.noaa.gov/pub/data/ghcn/daily/all/";

                DateTime dtstart = DateTime.Now;
                int nsta = lstSta.Count;
                //log begin time
                fMain.WriteLogFile(crlf + "Begin Download GHCN Data ..." +
                    DateTime.Now.ToShortDateString() + "  " +
                    DateTime.Now.ToLongTimeString());

                //download and process each station
                lstStaDownloaded.Clear();
                DownloadedSites.Clear();

                int ista = 0;
                foreach (string station in lstSta)
                {
                    ista++;
                    string msg = "Downloading site " + station + " (" + ista.ToString() + " of " + nsta.ToString() + ")";
                    WriteStatus(msg);

                    string site = station + ".dly";
                    string url = string.Concat(urlpath, site);
                    ghcnfile = Path.Combine(cacheFolder, site);

                    bool results = DownloadData_GHCN(url, ghcnfile);
                    if (results)
                    {
                        fMain.WriteLogFile("Downloaded data for site: " + site);
                        //site is the filename, e.g US1NCBR0076.dly
                        DownloadedSites.Add(site.ToString());
                        //station is station ID, e.g US1NCBR0076
                        lstStaDownloaded.Add(station.ToString());
                    }
                    else
                    {
                        fMain.WriteLogFile("No data for site: " + station);
                    }
                }

                td = DateTime.Now - dtstart;
                fMain.WriteLogFile("End Download GHCN Data: " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");

                nsites = DownloadedSites.Count;
                if (nsites > 0)
                {
                    fMain.WriteLogFile("Downloaded data for " + nsites.ToString() + " GHCN Station(s)");
                }
                else
                {
                    fMain.WriteLogFile("No data available for selected GHCN stations!");
                }
                WriteStatus("Ready ...");
                Cursor.Current = Cursors.Default;
                return nsites;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public bool ProcessDailyDataOLD()
        {
            Cursor.Current = Cursors.WaitCursor;

            int yr, mo, ndays, nrecs, nSeriesCnt = 0;
            string svar;
            string[] sval = new string[31];
            DateTime dt;
            bool hasGHCN = false;

            SortedDictionary<DateTime, string> dictSeries;
            SortedDictionary<DateTime, string> dictMissing;
            Dictionary<string, List<string>> dictCnt;

            //daily data dictionary, key = variable, value - dict of series
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarDly;
            //daily missing data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarMiss;

            Dictionary<string, SortedDictionary<string, double>> siteModel;
            Dictionary<string, SortedDictionary<Int32, List<double>>> siteHMoments;
            SortedDictionary<Int32, List<double>> hMoments; //periodic stats for site/var

            string msg;
            DateTime dtstart = DateTime.Now;
            int nsta = lstSta.Count;
            //log begin time
            msg = "Begin Processing GHCN Data ..." + DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToLongTimeString();
            fMain.WriteLogFile(crlf + msg);
            WriteStatus(msg);

            //clear the dictionary
            dictSiteVars.Clear();
            try
            {
                DateTime dtime;
                //site is station ID, eg US1NCBR0076
                foreach (string site in lstStaDownloaded)
                {
                    string sta = site + ".dly";
                    msg = "Processing GHCN data for site " + site;
                    fMain.WriteLogFile(crlf + msg);
                    WriteStatus(msg);

                    //dictionary keyed on variable, value is dictionary of timeseries (datetime-value)
                    dictVarDly = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictVarMiss = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    //dictionary of missing counts
                    dictCnt = new Dictionary<string, List<string>>();

                    //dictionary for stochastic model parameters
                    siteModel = new Dictionary<string, SortedDictionary<string, double>>();
                    siteHMoments = new Dictionary<string, SortedDictionary<Int32, List<double>>>();

                    string sfile = Path.Combine(cacheFolder, sta);
                    string[] srlines = System.IO.File.ReadAllLines(sfile);
                    //
                    //each line in file is
                    //US1NCBR0076201606PRCP-9999   -9999   -9999
                    //
                    foreach (string sitevar in lstSelectedVars)
                    {
                        dictSeries = new SortedDictionary<DateTime, string>();
                        dictMissing = new SortedDictionary<DateTime, string>();
                        hMoments = new SortedDictionary<Int32, List<double>>();

                        foreach (var srline in srlines)
                        {
                            svar = srline.Substring(17, 4); //variable PRCP,TMAX,TMIN
                            if (sitevar.Contains(svar))     //sitevar is selected var if frmdownload
                            {
                                yr = Convert.ToInt32(srline.Substring(11, 4));
                                mo = Convert.ToInt32(srline.Substring(15, 2));
                                ndays = DateTime.DaysInMonth(yr, mo);
                                dtime = new DateTime(yr, mo, 1);
                                string dat;

                                //process only records within selected timeframe
                                //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                {
                                    switch (svar)
                                    {
                                        case "TMAX":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i + 1).Date;
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i].Contains(MISS)))
                                                    {
                                                        double tp = 1.8 * (Convert.ToDouble(sval[i]) / 10.0) + 32.0;
                                                        dat = tp.ToString("F2");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                    {
                                                        //dictMissing.Add(dt, NEWMISS);
                                                        dictSeries.Add(dt, dat);
                                                    }
                                                }
                                            }
                                            break;

                                        case "TMIN":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i + 1).Date;
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = 1.8 * (Convert.ToDouble(sval[i]) / 10.0) + 32.0;
                                                        dat = tp.ToString("F2");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        //    dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                    {
                                                        //dictMissing.Add(dt, NEWMISS);
                                                        dictSeries.Add(dt, dat);
                                                    }

                                                }
                                            }
                                            break;

                                        case "PRCP":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i + 1).Date;
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = Convert.ToDouble(sval[i]) / 254.0;
                                                        dat = tp.ToString("#0.000");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        //    dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                    {
                                                        //dictMissing.Add(dt, NEWMISS);
                                                        dictSeries.Add(dt, dat);
                                                    }

                                                }
                                            }
                                            break;

                                        case "EVAP":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i + 1).Date;
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = Convert.ToDouble(sval[i]) / 254.0;
                                                        dat = tp.ToString("#0.000");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        //    dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                    {
                                                        //dictMissing.Add(dt, NEWMISS);
                                                        dictSeries.Add(dt, dat);
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        //insert missing days
                        AddMissingDays(dictSeries, dictMissing, site, sitevar);

                        //count missing only if missing series count > 0
                        try
                        {
                            nrecs = dictSeries.Count();
                            nSeriesCnt = dictSeries.Keys.Count();
                            if (dictMissing.Count > 0)
                            {
                                //List is nrecs, num missing, percent missing, max hr missing
                                List<string> missCnt = new List<string>();
                                missCnt = CountMissingDays(site, sitevar, dictMissing, nrecs);
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
                            dictVarDly.Add(sitevar, dictSeries);
                            dictVarMiss.Add(sitevar, dictMissing);

                            //create model log file
                            //string spath = Path.Combine(Application.StartupPath, "data");
                            string ModelLogFile = Path.Combine(modelDir, site + "_model.log");
                            wrm = new StreamWriter(ModelLogFile, true);
                            wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                                BegDate.Date.ToShortDateString() + " - " + EndDate.Date.ToShortDateString() + " : " +
                                DateTime.Now.ToString());
                            wrm.AutoFlush = true;

                            Cursor.Current = Cursors.WaitCursor;
                            SortedDictionary<string, double> varmodel = new SortedDictionary<string, double>();
                            if (!sitevar.Contains("PRCP") && !sitevar.Contains("PREC")) //min and max temp
                            {
                                fMain.appManager.UpdateProgress("Fitting AR model for site " + site + ":" + sitevar);
                                LinearAR cModel = new LinearAR(site, sitevar, dictSeries, wrm);
                                cModel.FitLinearModel(1);
                                varmodel = cModel.ARmodel();
                                hMoments = cModel.HarmonicMoments();
                                cModel = null;
                            }
                            else //if rain or evap
                            {
                                fMain.appManager.UpdateProgress("Fitting Markov model for site " + site + ":" + sitevar);
                                Markov cModel = new Markov(site, sitevar, dictSeries, wrm);
                                cModel.FitMarkovModel(1);
                                varmodel = cModel.MarkovModel();
                                cModel = null;
                            }
                            Cursor.Current = Cursors.Default;

                            siteModel.Add(sitevar, varmodel);
                            siteHMoments.Add(sitevar, hMoments);
                            varmodel = null; hMoments = null;

                            wrm.Flush();
                            wrm.Close();
                            wrm.Dispose();

                            fMain.appManager.UpdateProgress("Ready ...");
                            //debug -------------------------------------
                            Debug.WriteLine("Daily series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                            Debug.WriteLine("Daily missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                            fMain.WriteLogFile("Daily series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                            fMain.WriteLogFile("Daily missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                            //debug -------------------------------------
                        }
                        dictSeries = null;
                        dictMissing = null;
                    }
                    srlines = null;
                    //end loop of variables for site

                    //Save files, if there is a variable series in dictVarDly
                    if (dictVarDly.Keys.Count > 0)
                    {
                        SaveDailyData("dly", site, dictVarDly);
                        //SaveDailyData("mis", site, dictVarMiss);

                        if (!dictMiss.ContainsKey(site)) dictMiss.Add(site, dictVarMiss);
                        if (!dictMissCnt.ContainsKey(site)) dictMissCnt.Add(site, dictCnt);
                        if (!dictModel.ContainsKey(site)) dictModel.Add(site, siteModel);
                        if (!dictHMoments.ContainsKey(site)) dictHMoments.Add(site, siteHMoments);

                        //uploads to wdm                             
                        UploadToWDM(WdmFile, site, dictVarDly);
                        List<string> svars = new List<string>();
                        svars = dictVarDly.Keys.ToList();
                        if (!dictSiteVars.ContainsKey(site))
                            dictSiteVars.Add(site, svars);
                        svars = null;
                        hasGHCN = true;
                    }
                    dictVarDly = null; dictVarMiss = null; dictCnt = null;
                    siteModel = null; siteHMoments = null;

                    msg = "End processing GHCN for " + site + " from " +
                           BegDate.Date.ToString() + " to " + EndDate.Date.ToString();
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);
                } //end loop station

                //Save model to sdbfile
                UploadModelToSDB();

                //show num sites downloaded and processed
                int nsites = dictSiteVars.Keys.Count;
                if (nsites > 0)
                {
                    StringBuilder st = new StringBuilder();
                    st.Append("Downloaded and processed data for " + nsites.ToString() + " GHCN Station(s)" + crlf + crlf);
                    msg = st.ToString();
                    fMain.WriteLogFile("Downloaded and processed data for " + nsites.ToString() + " GHCN Station(s)");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    st = null;
                }
                else
                {
                    msg = "No data available for selected GHCN stations!";
                    fMain.WriteLogFile("No data available for selected GHCN stations!");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                WriteStatus("Ready ...");
            }
            catch (Exception ex)
            {
                msg = "Error processing GHCN files ...";
                ShowError(msg, ex);
                return false;
            }

            td = DateTime.Now - dtstart;
            msg = "End Processing GHCN Data Files ... " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.";
            fMain.WriteLogFile(msg);
            WriteStatus(msg);
            WriteStatus("Ready ...");

            Cursor.Current = Cursors.Default;
            return hasGHCN;
        }
        public bool ProcessDailyData()
        {
            Cursor.Current = Cursors.WaitCursor;

            int yr, mo, ndays, nrecs, nSeriesCnt = 0;
            string svar;
            string[] sval = new string[31];
            DateTime dt;
            bool hasGHCN = false;

            SortedDictionary<DateTime, string> dictSeries;
            SortedDictionary<DateTime, string> dictMissing;
            Dictionary<string, List<string>> dictCnt;

            //daily data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarDly;
            //daily missing data dictionary
            Dictionary<string, SortedDictionary<DateTime, string>> dictVarMiss;

            Dictionary<string, SortedDictionary<string, double>> siteModel;
            Dictionary<string, SortedDictionary<Int32, List<double>>> siteHMoments;
            SortedDictionary<Int32, List<double>> hMoments; //periodic stats for site/var

            string msg;
            DateTime dtstart = DateTime.Now;
            int nsta = lstSta.Count;
            //log begin time
            msg = "Begin Processing GHCN Data ..." + DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToLongTimeString();
            fMain.WriteLogFile(crlf + msg);
            WriteStatus(msg);

            //clear the dictionary
            dictSiteVars.Clear();
            try
            {
                DateTime dtime;
                //site is station ID, eg US1NCBR0076
                foreach (string site in lstStaDownloaded)
                {
                    string sta = site + ".dly";
                    msg = "Processing GHCN data for site " + site;
                    fMain.WriteLogFile(crlf + msg);
                    WriteStatus(msg);

                    //dictionary keyed on variable, value is dictionary of timeseries (datetime-value)
                    dictVarDly = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    dictVarMiss = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    //dictionary of missing counts
                    dictCnt = new Dictionary<string, List<string>>();

                    //dictionary for stochastic model parameters
                    siteModel = new Dictionary<string, SortedDictionary<string, double>>();
                    siteHMoments = new Dictionary<string, SortedDictionary<Int32, List<double>>>();

                    string sfile = Path.Combine(cacheFolder, sta);
                    string[] srlines = System.IO.File.ReadAllLines(sfile);
                    //
                    //each line in file is
                    //US1NCBR0076201606PRCP-9999   -9999   -9999
                    //
                    foreach (string sitevar in lstSelectedVars)
                    {
                        dictSeries = new SortedDictionary<DateTime, string>();
                        dictMissing = new SortedDictionary<DateTime, string>();
                        hMoments = new SortedDictionary<Int32, List<double>>();
                        
                        //parse downloaded daily GHCN file
                        foreach (var srline in srlines)
                        {
                            svar = srline.Substring(17, 4); //variable PRCP,TMAX,TMIN
                            if (sitevar.Contains(svar))     //sitevar is selected var if frmdownload
                            {
                                yr = Convert.ToInt32(srline.Substring(11, 4));
                                mo = Convert.ToInt32(srline.Substring(15, 2));
                                ndays = DateTime.DaysInMonth(yr, mo);
                                dtime = new DateTime(yr, mo, 1);
                                string dat;

                                //process only records within selected timeframe
                                //if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                //{
                                    switch (svar)
                                    {
                                        case "TMAX":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i+1, 0, 0, 0).Date;
                                                //dt = dt.AddDays(1);
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = 1.8 * (Convert.ToDouble(sval[i]) / 10.0) + 32.0;
                                                        dat = tp.ToString("F2");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                            dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        dictSeries.Add(dt, dat);
                                                }
                                            }
                                            break;

                                        case "TMIN":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i+1, 0, 0, 0).Date;
                                                //dt = dt.AddDays(1);
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = 1.8 * (Convert.ToDouble(sval[i]) / 10.0) + 32.0;
                                                        dat = tp.ToString("F2");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                            dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        dictSeries.Add(dt, dat);
                                                }
                                            }
                                            break;

                                        case "PRCP":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i+1, 0, 0, 0).Date;
                                                //dt = dt.AddDays(1);
                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = Convert.ToDouble(sval[i]) / 254.0;
                                                        dat = tp.ToString("#0.000");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                            dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        dictSeries.Add(dt, dat);
                                                }
                                            }
                                            break;

                                        case "EVAP":
                                            ReadValues(srline, sval, ndays);
                                            for (int i = 0; i < ndays; i++)
                                            {
                                                dt = new DateTime(yr, mo, i+1, 0, 0, 0).Date;
                                                //dt = dt.AddDays(1);

                                                if (!dictSeries.Keys.Contains(dt))
                                                {
                                                    if (!(sval[i] == MISS))
                                                    {
                                                        double tp = Convert.ToDouble(sval[i]) / 254.0;
                                                        dat = tp.ToString("#0.000");
                                                    }
                                                    else
                                                    {
                                                        dat = NEWMISS;
                                                        if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                            dictMissing.Add(dt, NEWMISS);
                                                    }
                                                    if (dtime.CompareTo(BegDate) >= 0 && dtime.CompareTo(EndDate) <= 0)
                                                        dictSeries.Add(dt, dat);
                                                }
                                            }
                                            break;
                                    }
                                //}
                            }
                        }

                        //insert missing days
                        AddMissingDays(dictSeries, dictMissing, site, sitevar);

                        //count missing only if missing series count > 0
                        try
                        {
                            nrecs = dictSeries.Count();
                            nSeriesCnt = dictSeries.Keys.Count();
                            if (dictMissing.Count > 0)
                            {
                                //List is nrecs, num missing, percent missing, max hr missing
                                List<string> missCnt = new List<string>();
                                missCnt = CountMissingDays(site, sitevar, dictMissing, nrecs);
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
                            GHCNseries.AddSeries(site, sitevar, dictSeries);
                            GHCNseries.AddMissSeries(site, sitevar, dictMissing);

                            dictVarDly.Add(sitevar, dictSeries);
                            dictVarMiss.Add(sitevar, dictMissing);

                            //create model log file
                            //string spath = Path.Combine(Application.StartupPath, "data");
                            string ModelLogFile = Path.Combine(modelDir, site + "_model.log");
                            wrm = new StreamWriter(ModelLogFile, true);
                            wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                                BegDate.Date.ToShortDateString() + " - " + EndDate.Date.ToShortDateString() + " : " +
                                DateTime.Now.ToString());
                            wrm.AutoFlush = true;

                            Cursor.Current = Cursors.WaitCursor;
                            SortedDictionary<string, double> varmodel = new SortedDictionary<string, double>();
                            if (!sitevar.Contains("PRCP") && !sitevar.Contains("PREC")) //min and max temp
                            {
                                fMain.appManager.UpdateProgress("Fitting AR model for site " + site + ":" + sitevar);
                                LinearAR cModel = new LinearAR(site, sitevar, dictSeries, wrm);
                                cModel.FitLinearModel(1);
                                varmodel = cModel.ARmodel();
                                hMoments = cModel.HarmonicMoments();
                                cModel = null;
                            }
                            else //if rain or evap
                            {
                                fMain.appManager.UpdateProgress("Fitting Markov model for site " + site + ":" + sitevar);
                                Markov cModel = new Markov(site, sitevar, dictSeries, wrm);
                                cModel.FitMarkovModel(1);
                                varmodel = cModel.MarkovModel();
                                cModel = null;
                            }
                            Cursor.Current = Cursors.Default;

                            siteModel.Add(sitevar, varmodel);
                            siteHMoments.Add(sitevar, hMoments);
                            varmodel = null; hMoments = null;

                            wrm.Flush();
                            wrm.Close();
                            wrm.Dispose();

                            fMain.appManager.UpdateProgress("Ready ...");
                            //debug -------------------------------------
                            Debug.WriteLine("Daily series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                            Debug.WriteLine("Daily missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                            fMain.WriteLogFile("Daily series for " + sitevar + ", " + dictSeries.Count.ToString() + " records.");
                            fMain.WriteLogFile("Daily missing series for " + sitevar + ", " + dictMissing.Count.ToString() + " records.");
                            //debug -------------------------------------
                        }
                        dictSeries = null;
                        dictMissing = null;
                    }
                    srlines = null;
                    //end loop of variables for site

                    //Save files, if there is a variable series in dictVarDly
                    if (dictVarDly.Keys.Count > 0)
                    {
                        SaveDailyData("dly", site, dictVarDly);
                        //SaveDailyData("mis", site, dictVarMiss);

                        if (!dictMiss.ContainsKey(site)) dictMiss.Add(site, dictVarMiss);
                        if (!dictMissCnt.ContainsKey(site)) dictMissCnt.Add(site, dictCnt);
                        if (!dictModel.ContainsKey(site)) dictModel.Add(site, siteModel);
                        if (!dictHMoments.ContainsKey(site)) dictHMoments.Add(site, siteHMoments);

                        //uploads to wdm                             
                        UploadToWDM(WdmFile, site, dictVarDly);
                        List<string> svars = new List<string>();
                        svars = dictVarDly.Keys.ToList();
                        if (!dictSiteVars.ContainsKey(site))
                            dictSiteVars.Add(site, svars);
                        svars = null;
                        hasGHCN = true;
                    }
                    dictVarDly = null; dictVarMiss = null; dictCnt = null;
                    siteModel = null; siteHMoments = null;

                    msg = "End processing GHCN for " + site + " from " +
                           BegDate.Date.ToString() + " to " + EndDate.Date.ToString();
                    fMain.WriteLogFile(msg);
                    WriteStatus(msg);
                } //end loop station

                //Save model to sdbfile
                UploadModelToSDB();

                //show num sites downloaded and processed
                int nsites = dictSiteVars.Keys.Count;
                if (nsites > 0)
                {
                    StringBuilder st = new StringBuilder();
                    st.Append("Downloaded and processed data for " + nsites.ToString() + " GHCN Station(s)" + crlf + crlf);
                    msg = st.ToString();
                    fMain.WriteLogFile("Downloaded and processed data for " + nsites.ToString() + " GHCN Station(s)");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    st = null;
                }
                else
                {
                    msg = "No data available for selected GHCN stations!";
                    fMain.WriteLogFile("No data available for selected GHCN stations!");
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                WriteStatus("Ready ...");
            }
            catch (Exception ex)
            {
                msg = "Error processing GHCN files ...";
                ShowError(msg, ex);
                return false;
            }

            td = DateTime.Now - dtstart;
            msg = "End Processing GHCN Data Files ... " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.";
            fMain.WriteLogFile(msg);
            WriteStatus(msg);
            WriteStatus("Ready ...");

            Cursor.Current = Cursors.Default;
            return hasGHCN;
        }
        private bool UploadToWDM(string wdmfile, string site, Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<string> siteAttrib = new List<string>();
                fMain.dictSta.TryGetValue(site, out siteAttrib);

                clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib,
                                         dictSiteWea, wdmfile, fMain.optDataSource,"Day");
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

        private List<string> CountMissingDays(string site, string svar,
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
                if (lstdat[0].Contains(NEWMISS))
                {
                    curmxhr = 1; mxhrs = 1; ncount = 1;
                }
                else
                {
                    curmxhr = 0; mxhrs = 0;
                }

                for (int i = 1; i < lstdt.Count; i++)
                {
                    if (lstdat[i].Contains(NEWMISS))
                    {
                        TimeSpan ts = lstdt[i] - lstdt[i - 1];
                        if (ts.TotalDays == 1)
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
                //{
                //percent = lstPercent.ElementAt(i);
                //if (percent >= PercentMiss)
                //    lstIgnoredVars.Add(lstSelectedVars[i]);
                //}
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
        
        private void AddMissingDays(SortedDictionary<DateTime, string> dicSeries,
                SortedDictionary<DateTime, string> dicMissing, string sta, string svar)
        {
            try
            {
                double ndays = 0;
                DateTime dt;
                TimeSpan td;

                List<DateTime> lstdt = dicSeries.Keys.ToList();
                List<string> lstdat = dicSeries.Values.ToList();

                for (int i = 1; i < lstdt.Count; i++)
                {
                    td = lstdt[i] - lstdt[i - 1];
                    if ((ndays = td.TotalDays) > 1)
                    {
                        fMain.WriteLogFile("Inserting days of " + ndays.ToString() +
                            " for " + svar + " of site " + sta + ", " +
                            lstdt[i-1].ToString() + ", " + lstdt[i].ToString());
                        for (int j = 1; j <= ndays - 1; j++)
                        {
                            dt = lstdt[i - 1].AddDays(j);
                            if (!dicMissing.ContainsKey(dt))
                                dicMissing.Add(dt, NEWMISS);
                            if (!dicSeries.ContainsKey(dt))
                                dicSeries.Add(dt, NEWMISS);
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        public void ShowDataTable(List<string> lstSelectedVars)
        {
            //debug----------------------------
            foreach (var it in lstSelectedVars)
                Debug.WriteLine("In ShowDataTable:var = " + it);
            //debug----------------------------

            frmData fdata = new frmData(fMain, dictMiss,
                dictModel, dictHMoments, dictMissCnt, dictSelVars, lstSelectedVars, dictOptVars);
            if (fdata.ShowDialog() == DialogResult.OK)
            {
                //092920
                fdata = null;
                fdata.Dispose();
            }
        }

        private void ReadValues(string srline, string[] sval, int ndays)
        {
            int ival = 21;
            try
            {
                for (int i = 0; i < ndays; i++)
                {
                    sval[i] = srline.Substring(i * 8 + ival, 5);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private bool DownloadData_GHCN(string url, string savefile)
        {
            //ghcn url ftp, e.g ----------------------------------------------
            //ftp://ftp.ncdc.noaa.gov/pub/data/ghcn/daily/all/USC00243712.dly
            //ftp://ftp.ncdc.noaa.gov/pub/data/ghcn/daily/all/
            //----------------------------------------------------------------
            Cursor.Current = Cursors.WaitCursor;
            string sta = Path.GetFileNameWithoutExtension(savefile);
            Debug.WriteLine("Station in DownloadData_GHCN=" + sta);
            //WriteStatus("Downloading data for " + sta);

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
                    System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                string msg = "Error downloading GHCN data! Server may be down.";
                ShowError(msg, ex);
            }

            Cursor.Current = Cursors.Default;
            if (!File.Exists(savefile))
                return false;
            else
                return true;
        }

        #region "SaveFiles"
        public void SaveDailyData(string ext, string site, Dictionary<string, SortedDictionary<DateTime, string>> dictSeries)
        {
            //exports hourly with missing, filled and missing data to csv
            //ext=dly - raw hourly
            //ext=dlyest - hourly with estimate
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
            foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dictSeries)
            {
                string svar = kv.Key;
                dictSeries.TryGetValue(svar, out siteData);
                WriteDlyData(wri, site, svar, siteData);
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
        private void WriteDlyData(StreamWriter wri, string site, string svar, SortedDictionary<DateTime, string> siteData)
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

        #endregion "SaveFiles"
        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + crlf + ex.Message + crlf + crlf + ex.StackTrace;
            fMain.WriteLogFile(msg);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void WriteStatus(string msg)
        {
            fMain.WriteStatus(msg);
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
                    Debug.WriteLine("{0},{1},{2},{3}", site, sta.StationName,
                        sta.Latitude, sta.Longitude);

                    mdlDB.InsertRecordsInStationTable(site, sta.StationName, "GHCN",
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
            //Debug.WriteLine("VarModel variable count = " + varModel.Count.ToString());

            try
            {
                mdlDB.DeleteRecordsFromModelTable(site, svar, 1);
                foreach (KeyValuePair<string, double> st in varModel)
                {
                    //Debug.WriteLine("{0},{1},{2}", irec.ToString(),st.Key, st.Value.ToString());
                    //switch (tstep)
                    //{
                    //    case (int)TimeStep.Hourly:
                    //string[] stvar = st.Key.Split('_');
                    double val = st.Value;
                    //Debug.WriteLine("{0},{1},{2}", stvar[0].ToString(), stvar[1].ToString(), val.ToString());
                    //mdlDB.DeleteRecordsFromModelTable(site, svar, st.Key, val, 1);
                    mdlDB.InsertRecordsInModelTable(site, svar, st.Key, val, 1);
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
