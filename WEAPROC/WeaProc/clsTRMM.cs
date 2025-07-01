using atcData;
using atcUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeaModel;
using WeaModelSDB;
using WeaWDM;
using static atcUtility.modDate;

namespace NCEIData
{
    class clsTRMM
    {
        private frmMain fMain;
        private DateTime BegDate, EndDate;
        private TimeSpan td;
        private int MinYears;

        private List<string> lstSta;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        public Dictionary<string, string> dictMapVars = new Dictionary<string, string>();
        public string cacheFolder, dataFolder;
        private List<string> DownloadedGrid;
        private Dictionary<string, string> NLDASVars = new Dictionary<string, string>();
        private const string MISS = "-9999";
        //private double VarPercentMiss;
        private int optDataset;
        private string WdmFile, ModelSDB, modelDir;
        private int NumDaysRec;
        private float TimeZoneShift;
        private atcWDM.atcDataSourceWDM lWDM = new atcWDM.atcDataSourceWDM();
        private int lNextDSN = 0;
        private string crlf = Environment.NewLine;
        private StreamWriter wrm;

        //ARmodel parameters
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>>();
        //harmonic moments
        private SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> dictHMoments = new
            SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>>();
        //dictionary of selected vars, excludes %miss missing for a series 
        public SortedDictionary<string, List<string>> dictSelVars = new SortedDictionary<string, List<string>>();
        //dictionary of selected vars 
        public SortedDictionary<string, List<string>> dictSiteVars;

        //dict of selected sites
        public SortedDictionary<string, MetGages> dictSelSites =
                    new SortedDictionary<string, MetGages>();

        public List<string> lstStaDownloaded = new List<string>();
        public List<string> lstSelectedVars = new List<string>();
        public List<string> lstSelVariables = new List<string>();

        private enum TimeStep { Hourly, Daily };
        private string SelectedSta, SelectedVar;
        int irec = 0;

        public clsTRMM(frmMain _fmain, DateTime _bdate, DateTime _edate, int _UTCshift)
        {
            this.fMain = _fmain;
            this.BegDate = _bdate;
            this.EndDate = _edate;
            //list of grid points selected
            this.lstSta = fMain.lstSta;
            //dict of gridpoints
            this.dictSelSites = fMain.dictSelSites;
            this.DownloadedGrid = fMain.DownLoadedSites;
            //list of selected vars from frmdownload
            this.lstSelectedVars = fMain.lstSelectedVars;
            //fMain.cacheDir;
            cacheFolder = System.IO.Path.Combine(fMain.cacheDir, "TRMM");
            dataFolder = fMain.dataDir;
            this.optDataset = fMain.optDataSource;
            this.WdmFile = fMain.WdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.modelDir = fMain.modelDir;

            //dict of station and variables downloaded (excludes series above threshold)
            //need to pass to frmdata
            this.dictSiteVars = fMain.dictSiteVars;

            this.MinYears = fMain.MinYears;
            NumDaysRec = MinYears * 365;
            this.TimeZoneShift = 0.0F;
            SetVarMapping();
        }
        private void SetVarMapping()
        {
            dictMapVars.Clear();
            dictMapVars.Add("PREC", "precipitation");
            dictMapVars.Add("PRCP", "precipitation");

            NLDASVars.Clear();
            NLDASVars.Add("precipitation", "PREC");
        }
        public void ProcessTRMMdata()
        {
            lstSelVariables.Clear();
            lstStaDownloaded.Clear();
            dictSiteVars.Clear();

            DateTime dtstart = DateTime.Now; ;
            int nsta = lstSta.Count();
            //log begin time
            fMain.WriteLogFile("Begin Download TRMM ..." + DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            //get list of selected vars from the download screen
            Debug.WriteLine("Selected Variables");
            string svar;
            foreach (string s in lstSelectedVars)
            {
                //map download vars to NLDAS vars, eg ATEM to TMP2M
                dictMapVars.TryGetValue(s, out svar);
                lstSelVariables.Add(svar);
            }

            //download and process each selected grid point
            int isite = 0;
            int nsites = lstSta.Count();
            Cursor.Current = Cursors.WaitCursor;
            foreach (string site in lstSta)
            {
                isite++;

                TimeZoneShift = GetTimeZoneOfGrid(site);

                if (DownloadData_TRMM(lstSelVariables, site, isite, nsites))
                {
                    if (!lstStaDownloaded.Contains(site))
                    {
                        lstStaDownloaded.Add(site);
                        //Fit stochastic model for TRMM grid point
                        StochasticModelForGrid(site, lstSelVariables);
                    }
                    else
                    {
                        string msg = "No data for grid site " + site;
                        fMain.WriteLogFile(msg);
                        MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

            } //end loop grid sites 
            Cursor.Current = Cursors.Default;

            fMain.lstStaDownloaded = lstStaDownloaded;

            //show frmData screen
            if (lstStaDownloaded.Count > 0)
                ShowDataTable(lstSelectedVars);

            //logfile
            foreach (string item in lstStaDownloaded)
                fMain.WriteLogFile("Downloaded grid data: " + item.ToString());

            td = DateTime.Now - dtstart;
            fMain.WriteLogFile("End Download Data: " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.");
        }

        /// <summary>
        /// DownloadData_TRMM
        /// Downloads TRMM data (all variables) for a grid point and saves to WDMFile
        /// </summary>
        /// <param name="selectedVars"></param>
        /// <param name="site"></param>
        /// <param name="isite"></param>
        /// <param name="nsites"></param>
        /// <returns></returns>
        private bool DownloadData_TRMM(List<string> selectedVars, string site, int isite, int nsites)
        {
            //nldas base url e.g ----------------------------------------------
            //site is X000Y000
            //download and imports to wdm file
            //-----------------------------------------------------------------

            MetGages grid = new MetGages();
            string xlon = string.Empty, ylat = string.Empty;
            if (dictSelSites.TryGetValue(site, out grid))
            {
                xlon = grid.LONGITUDE;
                ylat = grid.LATITUDE;
            }
            string TRMMfile = string.Empty;
            string gridX = site.Substring(1, 3);
            string gridY = site.Substring(5, 3);
            string urlpath = "https://hydro1.gesdisc.eosdis.nasa.gov/daac-bin/access/timeseries.cgi?variable=TRMM:TRMM_3B42.7:precipitation";

            atcDateFormat lDateFormat = new atcUtility.atcDateFormat();
            lDateFormat.DateOrder = atcDateFormat.DateOrderEnum.YearMonthDay;
            lDateFormat.IncludeMinutes = false;
            lDateFormat.DateSeparator = "-";
            lDateFormat.DateTimeSeparator = "T";

            // .Year & "-" & aStartDate.Month & "-" & aStartDate.Day & "T" & aStartDate.Hour
            // ' .Year & "-" & aStartDate.Month & "-" & aStartDate.Day & "T" & aStartDate.Hour
            //string lStartDate = lDateFormat.JDateToString(BegDate.ToOADate());
            //string lEndDate = lDateFormat.JDateToString(EndDate.ToOADate());
            string lStartDate = BegDate.Year.ToString() + "-" + BegDate.Month.ToString("00") +
                              "-" + BegDate.Day.ToString("00") + "T00";
            string lEndDate = EndDate.Year.ToString() + "-" + EndDate.Month.ToString("00") +
                              "-" + EndDate.Day.ToString("00") + "T23";

            //iterate for all selected variables in the grid
            List<string> lstProcessedVar = new List<string>();
            Cursor.Current = Cursors.WaitCursor;

            //foreach (string svar in selectedVars)
            {
                string svar = "precipitation";
                string lURL = string.Empty;
                double bdt = BegDate.Date.ToOADate();
                double edt = EndDate.Date.ToOADate();
                TRMMfile = Path.Combine(cacheFolder, site);
                TRMMfile += "-" + svar + "_" + bdt.ToString() + "_" + edt.ToString() + ".txt";
                WriteStatus("Downloading " + svar + " data for " + site +
                          "(" + isite.ToString() + " of " + nsites.ToString() + ")");

                lURL = urlpath;// + svar;
                lURL += "&startDate=" + lStartDate;
                lURL += "&endDate=" + lEndDate;
                lURL += "&location=GEOM:POINT(" + xlon + ",%20" + ylat + ")&type=asc2";

                try
                {
                    bool isDloaded = false;
                    D4EM.Data.Download.DisableHttpsCertificateCheck();
                    D4EM.Data.Download.SetSecurityProtocol();

                    if (!File.Exists(TRMMfile))
                        isDloaded = D4EM.Data.Download.DownloadURL(lURL, TRMMfile);
                    else //file exist
                        isDloaded = true;
                    if (isDloaded)
                    {
                        fMain.WriteLogFile("Downloaded precipitation data for " + site);
                        if (ProcessSiteDownloadedData(site, svar, TRMMfile))
                        {
                            lstProcessedVar.Add("PREC");
                            //add grid to dictionary
                            dictSiteVars.Add(site, lstProcessedVar);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error downloading and processing TRMM data: " + site + "-" + svar;
                    ShowError(msg, ex);
                }
            }

            //add grid to dictionary
            //dictSiteVars.Add(site, lstProcessedVar); MOVED UP
            lstProcessedVar = null;

            Cursor.Current = Cursors.Default;
            WriteStatus("Ready ..");

            if (!File.Exists(TRMMfile)) //file for each variable
                return false;
            else
                return true;
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


        /// <summary>
        /// ProcessSiteDownloadedData
        /// Processeses downloaded TRMM txt for given variable
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="svar"></param>
        /// <param name="nldasfile"></param>
        /// <param name="elev"></param>
        /// <returns></returns>
        private bool ProcessSiteDownloadedData(string grid, string svar, string TRMMfile)
        {
            Cursor.Current = Cursors.WaitCursor;
            fMain.WriteLogFile("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(TRMMfile));
            fMain.WriteLogFile("Saving " + svar + " for grid " + grid + ": " + Path.GetFileName(WdmFile));
            WriteStatus("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(TRMMfile));

            string lCons = string.Empty;
            string lDesc = string.Empty;
            string lScen = "TRMM";
            double elev = 0.0;

            //timeseries
            SortedDictionary<DateTime, string> dictHourly = new SortedDictionary<DateTime, string>();
            atcData.atcDataAttributes aAttributes = new atcData.atcDataAttributes();
            try
            {
                if (lWDM.Open(WdmFile))
                {
                    //if there are any existing datasets, write new data after them 
                    if (lWDM.DataSets.Count > 0)
                    {
                        int lLastDSN = 0;
                        foreach (atcData.atcDataSet lds in lWDM.DataSets)
                            lLastDSN = Math.Max((int)lLastDSN, (int)lds.Attributes.GetValue("ID"));
                        lNextDSN = lLastDSN;
                    }
                    else
                        lNextDSN = 0;

                    atcTimeseriesTRMM.atcTimeseriesTRMM lTRMM = new atcTimeseriesTRMM.atcTimeseriesTRMM();
                    lTRMM.Open(TRMMfile);
                    lCons = string.Empty; lDesc = string.Empty;
                    string errmsg = "AddDataset failed when adding TRMM ";

                    foreach (atcData.atcTimeseries lTS in lTRMM.DataSets)
                    {
                        lNextDSN += 1;

                        int iTZone = (int)Math.Round(TimeZoneShift, 0, MidpointRounding.AwayFromZero);

                        atcTimeseries ltseries = (atcTimeseries)D4EM.Data.MetCmp.modMetDataProcess.ShiftDates(lTS, atcTimeUnit.TUHour, iTZone);
                        atcTimeseries lConvertedTseries = ltseries;
                        bool isExist = false;

                        isExist = CheckWDMForSeries(lWDM, grid, "PREC");
                        if (isExist) fMain.WriteLogFile("WDM aready contains PREC for " + grid.ToString());
                        lConvertedTseries = ltseries;// / 25.4; //3-hourly rain rate mm/hr convert to in/hr
                        lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                        lConvertedTseries.Attributes.SetValue("Constituent", "PREC");
                        lConvertedTseries.Attributes.SetValue("Description", "From 3-Hourly Precip in Inches");
                        lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                        lConvertedTseries.Attributes.SetValue("Scenario", "TRMM");
                        lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                        //if (elev > 0)
                        //    lConvertedTseries.Attributes.SetValue("Elevation", elev);

                        lConvertedTseries.Attributes.SetValue("STANAM", "TRMM Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                         " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                        lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                        if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace)) { }
                        else
                            fMain.WriteLogFile(errmsg + " PREC!");

                        //compute daily
                        lNextDSN++;
                        atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                        ltser.Attributes.SetValue("ID", lNextDSN);
                        ltser.Attributes.SetValue("Constituent", "PRCP");
                        ltser.Attributes.SetValue("Description", "Daily TRMM Precip in Inches");
                        ltser.Attributes.SetValue("Location", grid.ToString());
                        ltser.Attributes.SetValue("STAID", grid.ToString());
                        ltser.Attributes.SetValue("Scenario", "TRMM");
                        if (elev > 0)
                            ltser.Attributes.SetValue("Elevation", elev);
                        ltser.Attributes.SetValue("STANAM", "TRMM Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                              " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                        lWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);

                        //clean up
                        ltser = null;
                        lConvertedTseries = null;
                        ltseries = null;
                    } //next

                    lTRMM = null;
                    lWDM.Clear();
                }
                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in saving computed series in wdm!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                //MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private double GetGridElevation(string nldasfile)
        {
            double elev = 0.0;
            using (StreamReader wri = new StreamReader(nldasfile))
            {
                for (int i = 0; i < 20; i++)
                {
                    string sline = wri.ReadLine();
                    if (sline.Contains("Elevation"))
                    {
                        string[] arr = sline.Split('=');
                        if (!string.IsNullOrEmpty(arr[1]))
                            elev = Convert.ToDouble(arr[1]);
                        else
                            elev = -999;
                        Debug.WriteLine("NLDAS elev = " + elev.ToString());
                    }
                }
            }
            return elev;
        }

        /// <summary>
        /// StochasticModelForGrid
        /// Setup and fit stochastic model for grid variables
        /// </summary>
        /// <param name="site"></param>
        /// <param name="lstSelVariables"></param>
        private void StochasticModelForGrid(string grid, List<string> lstVariables)
        {
            //initialize model dictionaries
            Dictionary<string, SortedDictionary<string, double>> siteModel =
                    new Dictionary<string, SortedDictionary<string, double>>();
            Dictionary<string, SortedDictionary<Int32, List<double>>> siteHMoments =
                    new Dictionary<string, SortedDictionary<Int32, List<double>>>();
            //periodic stats for site/var
            SortedDictionary<Int32, List<double>> hMoments =
                    new SortedDictionary<Int32, List<double>>();

            //create model log file
            //string spath = Path.Combine(Application.StartupPath, "data");
            string ModelLogFile = Path.Combine(modelDir, grid + "_model.log");
            wrm = new StreamWriter(ModelLogFile, true);
            wrm.WriteLine(crlf + "STOCHASTIC MODEL Parameters: " +
                BegDate.Date.ToShortDateString() + " - " + EndDate.Date.ToShortDateString() + " : " +
                DateTime.Now.ToString());
            wrm.AutoFlush = true;

            //Get site info
            WDM cWDM = new WDM(WdmFile, optDataset);
            clsStation sta = cWDM.GetSiteInfo(grid);
            //Debug.WriteLine("{0},{1},{2},{3}",grid, sta.StationName, sta.Latitude,sta.Longitude);
            cWDM = null;

            //ModelDB
            WeaModelDB mdlDB = new WeaModelDB(fMain.wrlog, ModelSDB);
            mdlDB.InsertRecordsInStationTable(grid, sta.StationName, "TRMM",
                 Convert.ToSingle(sta.Latitude), Convert.ToSingle(sta.Longitude),
                 Convert.ToSingle(sta.Elevation), null, null);

            try
            {
                /*Process each downloaded variables for the grid/site*/
                SortedDictionary<DateTime, string> dictHourly = new SortedDictionary<DateTime, string>();
                string nldasvar;
                foreach (string svar in lstVariables)
                {
                    NLDASVars.TryGetValue(svar, out nldasvar);
                    dictHourly = new SortedDictionary<DateTime, string>();
                    dictHourly = TimeSeriesToDictionary(grid, nldasvar);

                    if (!(dictHourly == null))
                        FitStochasticModel(mdlDB, grid, nldasvar, dictHourly, siteModel, siteHMoments);

                    dictHourly = null;
                }

                //add grid model parameters
                if (!dictModel.ContainsKey(grid)) dictModel.Add(grid, siteModel);
                if (!dictHMoments.ContainsKey(grid)) dictHMoments.Add(grid, siteHMoments);
                siteModel = null; siteHMoments = null;
            }
            catch (Exception ex)
            {
                string msg = "Error in fitting stochastic model!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                //MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            mdlDB = null;

            //close streamwriter
            wrm.Flush();
            wrm.Close();
            wrm.Dispose();
        }

        /// <summary>
        /// TimeSeriesToDictionary
        /// Converts atcTimeseries to dictionary
        /// </summary>
        /// <param name="dseries"></param>
        /// <returns>dictionary of timeseries data</returns>
        private SortedDictionary<DateTime, string> TimeSeriesToDictionary(string grid, string parm)
        {
            SortedDictionary<DateTime, string> dictSeries = new SortedDictionary<DateTime, string>();
            atcTimeseries tseries = new atcTimeseries(lWDM);

            try
            {
                if (lWDM.Open(WdmFile))
                {
                    foreach (atcData.atcDataSet lDSet in lWDM.DataSets)
                    {
                        if ((lDSet.Attributes.GetValue("Location").ToString().Contains(grid.ToString())) &&
                            (lDSet.Attributes.GetValue("Constituent").ToString().Contains(parm)) &&
                            (lDSet.Attributes.GetValue("Scenario").ToString().Contains("TRMM")))
                        {
                            Debug.WriteLine(lDSet.Attributes.GetValue("Location").ToString());
                            tseries = (atcTimeseries)lDSet;
                            break;
                        }
                    }
                }

                DateTime lDate; Double lValue;
                for (int lIndex = 1; lIndex <= tseries.numValues; lIndex++)
                {
                    lDate = DateTime.FromOADate(tseries.Dates.Values[lIndex - 1]);
                    lValue = tseries.Values[lIndex];
                    if (!dictSeries.ContainsKey(lDate))
                        dictSeries.Add(lDate, lValue.ToString());
                    //ok
                    //Debug.WriteLine(lDate.ToString() + ", " + lValue.ToString());
                }
                lWDM.Clear();
                tseries = null;
                return dictSeries;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool FitStochasticModel(WeaModelDB mdlDB, string grid, string svar,
                                        SortedDictionary<DateTime, string> dictHourly,
                                        Dictionary<string, SortedDictionary<string, double>> siteModel,
                                        Dictionary<string, SortedDictionary<Int32, List<double>>> siteHMoments)
        {
            Cursor.Current = Cursors.WaitCursor;
            /*varmodel is dictionary of model parameters*/
            SortedDictionary<string, double> varModel = new SortedDictionary<string, double>();
            SortedDictionary<Int32, List<double>> harMoments = new SortedDictionary<Int32, List<double>>();
            try
            {
                SelectedVar = svar;
                SelectedSta = grid;

                if (!svar.Contains("PREC"))
                {
                    fMain.appManager.UpdateProgress("Fitting AR model for site " + grid + ":" + svar);
                    LinearAR cModel = new LinearAR(grid, svar, dictHourly, wrm);
                    cModel.FitLinearModel(0);
                    varModel = cModel.ARmodel();
                    harMoments = cModel.HarmonicMoments();
                    cModel = null;
                    fMain.appManager.UpdateProgress("Saving AR model parameters for site " + grid + ":" + svar);
                    SaveToDB(0, varModel, mdlDB, svar, grid);
                }
                else //if rain
                {
                    fMain.appManager.UpdateProgress("Fitting Markov model for site " + grid + ":" + svar);
                    Markov cModel = new Markov(grid, svar, dictHourly, wrm);
                    cModel.FitMarkovModel(0);
                    varModel = cModel.MarkovModel();
                    cModel = null;
                    fMain.appManager.UpdateProgress("Saving Markov model parameters for site " + grid + ":" + svar);
                    SaveToDB(0, varModel, mdlDB, svar, grid);
                }
                fMain.appManager.UpdateProgress("Ready ...");
                //clean up
                siteModel.Add(svar, varModel); varModel = null;
                siteHMoments.Add(svar, harMoments); harMoments = null;
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in fitting stochastic model!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                return false;
            }
        }

        private void SaveToDB(int tstep, SortedDictionary<string, double> aVarModel, WeaModelDB mdlDB,
                     string svar, string site)
        {
            Debug.WriteLine(Environment.NewLine + "Entering SaveToDB ....");
            Debug.WriteLine("VarModel variable count = " + aVarModel.Count.ToString());
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                mdlDB.DeleteRecordsFromModelTable(site, svar, 0);
                foreach (KeyValuePair<string, double> st in aVarModel)
                {
                    //Debug.WriteLine("{0},{1}", st.Key, st.Value.ToString());
                    double val = st.Value;
                    //mdlDB.DeleteRecordsFromModelTable(site, svar, st.Key, val, 0);
                    mdlDB.InsertRecordsInModelTable(site, svar, st.Key, val, 0);
                }
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                string errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private double ConvertToWindFrom(double y, double x, double deg)
        {
            double degfrom = 0.0;

            if (x > 0 && y > 0)
                degfrom = 270 - deg;
            else if (x > 0 && y < 0)
                degfrom = 270 + deg;
            else if (x < 0 && y < 0)
                degfrom = 90 - deg;
            else if (x < 0 && y > 0)
                degfrom = 90 + deg;
            //on axis
            else if (x > 0 && y == 0)
                degfrom = 270;
            else if (x < 0 && y == 0)
                degfrom = 90;
            else if (x == 0 && y > 0)
                degfrom = 180;
            else if (x == 0 && y < 0)
                degfrom = 360;
            return degfrom;
        }
        private bool CheckWDMForSeries(atcWDM.atcDataSourceWDM lWdmDS, string sta, string svar)
        {
            bool found = false;
            //atcTimeseriesGroup tser = lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "PEVT");
            //atcTimeseries tser = (atcTimeseries)lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "PEVT")(0);
            if (lWdmDS.DataSets.Count > 0)
            {
                atcData.atcDataSet lseries = new atcData.atcDataSet();
                foreach (atcData.atcDataSet lDSet in lWdmDS.DataSets)
                {
                    if ((lDSet.Attributes.GetValue("Location").ToString().Contains(sta.ToString())) &&
                        (lDSet.Attributes.GetValue("Constituent").ToString().Contains(svar)) &&
                        (lDSet.Attributes.GetValue("Scenario").ToString().Contains("TRMM")))
                    {
                        lseries = lDSet;
                        found = true;
                        break;
                    }
                }
                if (found)
                    lWdmDS.DataSets.Remove(lseries);
                lseries = null;
            }
            return found;
        }
        private atcTimeseries GetTimeSeries(string grid, string parm)
        {
            atcTimeseries tseries = new atcTimeseries(lWDM);
            //SortedDictionary<DateTime, string> dictSeries = new SortedDictionary<DateTime, string>();
            if (lWDM.Open(WdmFile))
            {
                foreach (atcData.atcDataSet lDSet in lWDM.DataSets)
                {
                    if ((lDSet.Attributes.GetValue("Location").ToString().Contains(grid.ToString())) &&
                        (lDSet.Attributes.GetValue("Constituent").ToString().Contains(parm)) &&
                        (lDSet.Attributes.GetValue("Scenario").ToString().Contains("TRMM")))
                    {
                        Debug.WriteLine(lDSet.Attributes.GetValue("Location").ToString());
                        tseries = (atcTimeseries)lDSet;
                        break;
                    }
                }
            }

            DateTime lDate;
            Double lValue;
            for (int lIndex = 1; lIndex <= tseries.numValues; lIndex++)
            {
                lDate = DateTime.FromOADate(tseries.Dates.Values[lIndex - 1]);
                lValue = tseries.Values[lIndex];
                //if (!dictSeries.ContainsKey(lDate))
                //    dictSeries.Add(lDate, lValue.ToString());
                //ok
                //Debug.WriteLine(lDate.ToString() + ", " + lValue.ToString());
            }

            lWDM.Clear();
            lWDM = null;
            //tseries = null;
            return tseries;
        }
        private bool ProcessSiteWind(string grid)
        {
            atcTimeseries lWindTseries = new atcTimeseries(lWDM);
            try
            {
                if (lWDM.Open(WdmFile))
                {
                    //if we have both VGRD10m and UGRD10m, compute WIND
                    //find UGRD10m
                    atcTimeseries lUWINDTseries = GetTimeSeries(grid, "WINDU");
                    atcTimeseries lVWINDTseries = GetTimeSeries(grid, "WINDV");

                    for (int lValIndex = 1; lValIndex <= lVWINDTseries.numValues; lValIndex++)
                    {
                        double uw = lUWINDTseries.Values[lValIndex];
                        double vw = lVWINDTseries.Values[lValIndex];
                        double wnd = (vw * vw) + (uw * uw);
                        lWindTseries.Values[lValIndex] = 2.237 * Math.Pow(wnd, 0.5);
                    } //next lValIndex

                    //now write the new timeseries to wdm
                    lWindTseries.Attributes.SetValue("ID", lNextDSN);
                    lWindTseries.Attributes.SetValue("Constituent", "WIND");
                    lWindTseries.Attributes.SetValue("Description", "Hourly Wind Speed in mph");
                    lWindTseries.Attributes.SetValue("Location", grid.ToString());
                    lWindTseries.Attributes.SetValue("Scenario", "NLDAS");
                    lWindTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                    lWindTseries.Attributes.GetValue("Latitude").ToString() + " Long=" +
                          lWindTseries.Attributes.GetValue("Longitude").ToString());
                    lWindTseries.Attributes.SetValue("COMPFG", 1);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void ShowDataTable(List<string> lstSelectedVars)
        {
            {
                frmData fdata = new frmData(fMain, null,
                    dictModel, dictHMoments, null, dictSiteVars, lstSelectedVars, dictOptVars);
                if (fdata.ShowDialog() == DialogResult.OK)
                {
                    //092920
                    fdata = null;
                    fdata.Dispose();
                }
                //fdata = null;
            }
        }
        public Dictionary<string, bool> OptionVars
        {
            set { dictOptVars = value; }
        }
        private void ShowError(string msg, Exception ex)
        {
            string nl = System.Environment.NewLine;
            msg += nl + nl + ex.Message + nl + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void WriteStatus(string msg)
        {
            fMain.appManager.UpdateProgress(msg);
        }
    }
}
