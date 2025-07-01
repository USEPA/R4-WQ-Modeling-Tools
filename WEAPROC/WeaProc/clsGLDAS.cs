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
    class clsGLDAS
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
        private double dMiss = 999;
        private double VarPercentMiss;
        private int optDataset;
        private string WdmFile, AnnWdmFile, ModelSDB, modelDir;
        private int NumDaysRec;
        private float TimeZoneShift;
        private atcWDM.atcDataSourceWDM lWDM = new atcWDM.atcDataSourceWDM();
        //annual data annWDM
        private atcWDM.atcDataSourceWDM annualWDM = new atcWDM.atcDataSourceWDM();
        private int lNextDSN = 0;
        private string crlf = Environment.NewLine;
        private StreamWriter wrm;
        private int PercentMiss;

        //data dictionaries missing
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

        public clsGLDAS(frmMain _fmain, DateTime _bdate, DateTime _edate, int _UTCshift)
        {
            this.fMain = _fmain;
            this.BegDate = _bdate;
            this.EndDate = _edate;
            this.PercentMiss = fMain.PercentMiss;
            //list of grid points selected
            this.lstSta = fMain.lstSta;
            //dict of gridpoints
            this.dictSelSites = fMain.dictSelSites;
            this.DownloadedGrid = fMain.DownLoadedSites;
            //list of selected vars from frmdownload
            this.lstSelectedVars = fMain.lstSelectedVars;
            //fMain.cacheDir;
            cacheFolder = System.IO.Path.Combine(fMain.cacheDir, "GLDAS");
            dataFolder = fMain.dataDir;
            this.optDataset = fMain.optDataSource;
            this.WdmFile = fMain.WdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.modelDir = fMain.modelDir;
            this.AnnWdmFile = fMain.AnnWdmFile;

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
            dictMapVars.Add("PREC", "Rainf_tavg");
            dictMapVars.Add("PRCP", "Rainf_tavg");
            dictMapVars.Add("TEMP", "Tair_f_inst");
            dictMapVars.Add("WIND", "Wind_f_inst");
            dictMapVars.Add("SOLR", "SWdown_f_tavg");
            dictMapVars.Add("HUMI", "Qair_f_inst");
            dictMapVars.Add("LRAD", "LWdown_f_tavg");
            dictMapVars.Add("ATMP", "Psurf_f_inst");

            NLDASVars.Clear();
            NLDASVars.Add("Rainf_tavg", "PREC");
            NLDASVars.Add("Tair_f_inst", "ATEM");
            NLDASVars.Add("Wind_f_inst", "WIND");
            NLDASVars.Add("SWdown_f_tavg", "SOLR");
            NLDASVars.Add("Qair_f_inst", "DEWP");
            NLDASVars.Add("Psurf_f_inst", "ATMP"); //surface pressure
            NLDASVars.Add("LWdown_f_tavg", "LRAD");//longwave rad
            //NLDASVars.Add("PEVAPsfc", "PEVT");
        }
        public void ProcessGLDASdata()
        {
            lstSelVariables.Clear();
            lstStaDownloaded.Clear();
            dictSiteVars.Clear();

            DateTime dtstart = DateTime.Now; ;
            int nsta = lstSta.Count();
            //log begin time
            fMain.WriteLogFile("Begin Download GLDAS ..." + DateTime.Now.ToShortDateString() + "  " +
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

                if (DownloadData_GLDAS(lstSelVariables, site, isite, nsites))
                {
                    if (!lstStaDownloaded.Contains(site))
                    {
                        lstStaDownloaded.Add(site);
                        //Fit stochastic model for GLDAS grid point
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
        /// DownloadData_GLDAS
        /// Downloads GLDAS data (all variables) for a grid point and saves to WDMFile
        /// </summary>
        /// <param name="selectedVars"></param>
        /// <param name="site"></param>
        /// <param name="isite"></param>
        /// <param name="nsites"></param>
        /// <returns></returns>
        private bool DownloadData_GLDAS(List<string> selectedVars, string site,
                                        int isite, int nsites)
        {
            //gldas base url e.g ----------------------------------------------
            //site is 000:000, can be 0000:000 for international
            //download and imports to wdm file
            //-----------------------------------------------------------------

            MetGages grid = new MetGages();
            string xlon = string.Empty, ylat = string.Empty;
            if (dictSelSites.TryGetValue(site, out grid))
            {
                xlon = grid.LONGITUDE;
                ylat = grid.LATITUDE;
            }
            string gldasfile = string.Empty;
            string gridX = site.Substring(1, 3);
            string gridY = site.Substring(5, 3);
            string urlpath = "https://hydro1.gesdisc.eosdis.nasa.gov/daac-bin/access/timeseries.cgi?variable=GLDAS2:GLDAS_NOAH025_3H_v2.1:";

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

            foreach (string svar in selectedVars)
            {
                string lURL = string.Empty;
                double bdt = BegDate.Date.ToOADate();
                double edt = EndDate.Date.ToOADate();
                gldasfile = Path.Combine(cacheFolder, site);
                gldasfile += "-" + svar + "_" + bdt.ToString() + "_" + edt.ToString() + ".txt";
                WriteStatus("Downloading " + svar + " data for " + site +
                          "(" + isite.ToString() + " of " + nsites.ToString() + ")");

                lURL = urlpath + svar;
                lURL += "&startDate=" + lStartDate;
                lURL += "&endDate=" + lEndDate;
                lURL += "&location=GEOM:POINT(" + xlon + ",%20" + ylat + ")&type=asc2";

                try
                {
                    bool isDloaded = false;
                    D4EM.Data.Download.DisableHttpsCertificateCheck();
                    D4EM.Data.Download.SetSecurityProtocol();

                    if (!File.Exists(gldasfile))
                        isDloaded = D4EM.Data.Download.DownloadURL(lURL, gldasfile);
                    else //file exist
                        isDloaded = true;
                    if (isDloaded)
                    {
                        fMain.WriteLogFile("Downloaded " + svar + " data for " + site);
                        if (ProcessSiteDownloadedData(site, svar, gldasfile))
                        {
                            switch (svar)
                            {
                                case "Rainf_tavg":
                                    lstProcessedVar.Add("PREC");
                                    break;
                                case "Tair_f_inst":
                                    lstProcessedVar.Add("ATEM");
                                    break;
                                case "Wind_f_inst":
                                    lstProcessedVar.Add("WIND");
                                    break;
                                case "SWdown_f_tavg":
                                    lstProcessedVar.Add("SOLR");
                                    lstProcessedVar.Add("LRAD");
                                    lstProcessedVar.Add("CLOU");
                                    break;
                                case "Qair_f_inst":
                                    lstProcessedVar.Add("DEWP");
                                    break;
                                case "Psurf_f_inst":
                                    lstProcessedVar.Add("ATMP");
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error downloading and processing GLDAS data: " + site + "-" + svar;
                    ShowError(msg, ex);
                }
            }

            //add grid to dictionary
            dictSiteVars.Add(site, lstProcessedVar);
            lstProcessedVar = null;

            Cursor.Current = Cursors.Default;
            WriteStatus("Ready ..");

            if (!File.Exists(gldasfile)) //file for each variable
                return false;
            else
                return true;
        }

        /// <summary>
        /// ProcessSiteDownloadedData
        /// Processeses downloaded NLDAS txt for given variable
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="svar"></param>
        /// <param name="nldasfile"></param>
        /// <param name="elev"></param>
        /// <returns></returns>
        private bool ProcessSiteDownloadedData(string grid, string svar, string gldasfile)
        {
            Cursor.Current = Cursors.WaitCursor;
            fMain.WriteLogFile("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(gldasfile));
            fMain.WriteLogFile("Saving " + svar + " for grid " + grid + ": " + Path.GetFileName(WdmFile));
            WriteStatus("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(gldasfile));

            string lCons = string.Empty;
            string lDesc = string.Empty;
            string lScen = "GLDAS";
            double elev = 0.0;
            bool isExist = false;
            bool isMissAboveThreshold = false;

            //timeseries
            SortedDictionary<DateTime, string> dictHourly = new SortedDictionary<DateTime, string>();
            atcData.atcDataAttributes aAttributes = new atcData.atcDataAttributes();

            SortedDictionary<DateTime, string> dictMissing = new SortedDictionary<DateTime, string>();
            Dictionary<string, List<string>> dictCnt = new Dictionary<string, List<string>>();

            //hourly data and missing dictionary
            //Dictionary<string, SortedDictionary<DateTime, string>> dictVarMiss;
            try
            {
                if (!annualWDM.Open(AnnWdmFile))
                    Debug.WriteLine("Cannot open annual WDM!");

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

                    atcTimeseriesGLDS.atcTimeseriesGLDS lGDS = new atcTimeseriesGLDS.atcTimeseriesGLDS();
                    //open gldasfile and read series and adds tseries in lgds
                    lGDS.Open(gldasfile);
                    //
                    lCons = string.Empty; lDesc = string.Empty;
                    string errmsg = "AddDataset failed when adding GLDAS ";

                    //actually,lGDS contains only one series
                    foreach (atcData.atcTimeseries lTS in lGDS.DataSets)
                    {
                        lNextDSN += 1;
                        int iTZone = (int)Math.Round(TimeZoneShift, 0, MidpointRounding.AwayFromZero);
                        atcTimeseries ltseries = (atcTimeseries)D4EM.Data.MetCmp.modMetDataProcess.ShiftDates(lTS, atcTimeUnit.TUHour, iTZone);
                        atcTimeseries lConvertedTseries = ltseries;
                        isExist = false;
                        
                        isMissAboveThreshold = CheckMissing(ltseries, grid, svar);
                        if (isMissAboveThreshold)
                        {
                            ltseries = null;
                            lConvertedTseries = null;
                            return false;
                        }

                        //processes and uploads to WDM
                        //1/28/21 all conversions in units in atcTimeseriesGLDS15
                        switch (svar)
                        {
                            case "Rainf_tavg":
                                isExist = CheckWDMForSeries(lWDM, grid, "PREC");
                                if (isExist) fMain.WriteLogFile("WDM aready contains PREC for " + grid.ToString());
                                lConvertedTseries = ltseries; // * 3600.0 / 25.4; //3-hourly rain rate kg/m2/s convert to in
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", "PREC");
                                lConvertedTseries.Attributes.SetValue("Description", "From 3-Hourly Precip in Inches");
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                //if (elev > 0)
                                //    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace)) 
                                    CalculateAnnualTimeSeries((int)atcTran.TranSumDiv, lWDM, annualWDM, lConvertedTseries, "PREC", grid, elev, "Inches");
                                else
                                    fMain.WriteLogFile(errmsg + " PREC!");

                                //compute daily
                                lNextDSN++;
                                atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                                ltser.Attributes.SetValue("ID", lNextDSN);
                                ltser.Attributes.SetValue("Constituent", "PRCP");
                                ltser.Attributes.SetValue("Description", "Daily Precip in Inches");
                                ltser.Attributes.SetValue("Location", grid.ToString());
                                ltser.Attributes.SetValue("STAID", grid.ToString());
                                ltser.Attributes.SetValue("Scenario", "GLDAS");
                                if (elev > 0)
                                    ltser.Attributes.SetValue("Elevation", elev);
                                ltser.Attributes.SetValue("STANAM", "GLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lts.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);
                                ltser = null;
                                break;

                            case "Tair_f_inst":
                                lCons = "ATEM";
                                lDesc = "Hourly Air Temperature in Degrees F";
                                lConvertedTseries = ltseries;// (ltseries * 9 / 5) - 459.67;
                                //tmin

                                isExist = CheckWDMForSeries(lWDM, grid, "ATEM");
                                if (isExist) fMain.WriteLogFile("WDM aready contains ATEM for " + grid.ToString());

                                isExist = CheckWDMForSeries(lWDM, grid, "TMIN");
                                if (isExist) fMain.WriteLogFile("WDM aready contains TMIN for " + grid.ToString());

                                atcTimeseries lTmin = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranMin, lWDM);
                                lTmin.Attributes.SetValue("ID", lNextDSN++);
                                lTmin.Attributes.SetValue("Constituent", "TMIN");
                                lTmin.Attributes.SetValue("Description", "Minimum Temperature Degrees F");
                                lTmin.Attributes.SetValue("Location", grid.ToString());
                                lTmin.Attributes.SetValue("STAID", grid.ToString());
                                lTmin.Attributes.SetValue("Scenario", "GLDAS");
                                if (elev > 0)
                                    lTmin.Attributes.SetValue("Elevation", elev);
                                lTmin.Attributes.SetValue("STANAM", "GLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lTmin.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(lTmin, atcData.atcDataSource.EnumExistAction.ExistReplace);
                                lTmin = null;

                                //tmax
                                isExist = CheckWDMForSeries(lWDM, grid, "TMAX");
                                if (isExist) fMain.WriteLogFile("WDM aready contains TMAX for " + grid.ToString());

                                atcTimeseries lTmax = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranMax, lWDM);
                                lTmax.Attributes.SetValue("ID", lNextDSN++);
                                lTmax.Attributes.SetValue("Constituent", "TMAX");
                                lTmax.Attributes.SetValue("Description", "Maximum Temperature Degrees F");
                                lTmax.Attributes.SetValue("Location", grid.ToString());
                                lTmax.Attributes.SetValue("STAID", grid.ToString());
                                lTmax.Attributes.SetValue("Scenario", "GLDAS");
                                if (elev > 0)
                                    lTmax.Attributes.SetValue("Elevation", elev);
                                lTmax.Attributes.SetValue("STANAM", "GLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lTmin.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(lTmax, atcData.atcDataSource.EnumExistAction.ExistReplace);
                                lTmax = null;
                                break;

                            case "Wind_f_inst":
                                lCons = "WIND";
                                lDesc = "Hourly Wind Speed in mi/hr";
                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains WIND for " + grid.ToString());
                                lConvertedTseries = (atcTimeseries)ltseries.Clone(); // * 2.237 for mph
                                break;

                            case "Psurf_f_inst":
                                lCons = "ATMP";
                                lDesc = "Hourly Sea Level Pressure in mmHg";
                                lConvertedTseries = ltseries;// * 0.00750062; //1 Pa = 0.000750062 mmHg
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding GLDAS " + lConvertedTseries.ToString());
                                break;

                            case "LWdown_f_tavg":
                                lCons = "LRAD";
                                lDesc = "Hourly Longwave Radiation in Langleys";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains LRAD for " + grid.ToString());

                                lConvertedTseries = ltseries;// * 0.085985; //1 Watt/m2 = 0.085985 Lang/hour
                                                                         // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding GLDAS " + lConvertedTseries.ToString());
                                break;

                            case "SWdown_f_tavg":
                                lCons = "SOLR";
                                lDesc = "Hourly Solar Radiation in Langleys";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains SOLR for " + grid.ToString());

                                lConvertedTseries = ltseries;// * 0.085985; //1 Watt/m2 = 0.085985 Lang/hour
                                                                         // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);


                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding GLDAS " + lConvertedTseries.ToString());

                                //also use for cloud cover
                                lNextDSN++;
                                lCons = "CLOU";
                                lDesc = "Hourly Cloud Cover from solar radiation";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains CLOU for " + grid.ToString());

                                double llat = Convert.ToDouble(lConvertedTseries.Attributes.GetValue("Latitude"));

                                atcTimeseries lDailyTseries = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                                //Debug.WriteLine("Calculated llDailyTseries");
                                //for (int i = 1; i < 20; i++)
                                //    Debug.WriteLine(lDailyTseries.Dates.Values[i].ToString() + ", " + lDailyTseries.Values[i].ToString());
                                //atcTimeseries lDailyCCTseries = (atcTimeseries)D4EM.Data.MetCmp.modMetDataUtil.CloudCoverTimeseriesFromSolar(lDailyTseries, lWDM, llat);
                                atcTimeseries lDailyCCTseries = (atcTimeseries)atcMetCmp.modMetCompute.CloudCoverTimeseriesFromSolar(lDailyTseries, lWDM, llat);
                                //Debug.WriteLine("Calculated llDailyCCTseries");
                                //for (int i = 1; i < 20; i++)
                                //    Debug.WriteLine(lDailyCCTseries.Dates.Values[i].ToString() + ", " + lDailyCCTseries.Values[i].ToString());

                                for (int lValIndex = 1; lValIndex <= lConvertedTseries.numValues; lValIndex++)
                                {
                                    int lDayIndex = lDailyCCTseries.Dates.IndexOfValue((int)(lConvertedTseries.Dates.Values[lValIndex]), true);
                                    if (lDayIndex > 0)
                                    {
                                        double val = lDailyCCTseries.Values[lDayIndex];
                                        if (val > 0)
                                            lConvertedTseries.Values[lValIndex] = lDailyCCTseries.Values[lDayIndex];
                                        else
                                            lConvertedTseries.Values[lValIndex] = 10;
                                    }
                                    else
                                    {
                                        lConvertedTseries.Values[lValIndex] = 0.0;
                                    }
                                }
                                lConvertedTseries.Attributes.SetValue("TSTYPE", lCons);
                                //convert solar back to W/m2
                                //ltseries = lConvertedTseries / 0.085985;
                                //lConvertedTseries = atcMetCmp.modMetCompute.CloudCoverTimeseriesFromSolar(lConvertedTseries, lWDM, llat);
                                //lConvertedTseries = atcMetCmp.modMetCompute.NLDASCloudCoverTimeseriesFromSolar(ltseries, lWDM);
                                // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding GLDAS " + lConvertedTseries.ToString());
                                break;

                            case "Qair_f_inst": //specific humidity
                                lCons = "DEWP";
                                lDesc = "Hourly Dew Point Temperature";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains DEWP for " + grid.ToString());

                                //find ATEM
                                atcTimeseries lATEMTimeseries = new atcTimeseries(lWDM);
                                if (lWDM.DataSets.Count > 0)
                                {
                                    foreach (atcData.atcDataSet lDSet in lWDM.DataSets)
                                    {
                                        if ((lDSet.Attributes.GetValue("Location").ToString().Contains(grid.ToString())) &&
                                           (lDSet.Attributes.GetValue("Constituent").ToString().Contains("ATEM")) &&
                                           (lDSet.Attributes.GetValue("Scenario").ToString().Contains("GLDAS")))
                                        {
                                            lATEMTimeseries = (atcTimeseries)lDSet;
                                            break;
                                        }
                                    }
                                }
                                try
                                {
                                    //lConvertedTseries = atcMetCmp.modMetCompute.NLDASDewpointTimeseriesFromSpecificHumidity(ltseries, lATEMTimeseries, lWDM);
                                    lConvertedTseries = D4EM.Data.MetCmp.modMetDataUtil.NLDASDewpointTimeseriesFromSpecificHumidity(ltseries, lATEMTimeseries, lWDM);
                                }
                                catch (Exception exs) { }
                                break;
                        }
                        //now write the new timeseries to wdm (solar and prec has been written)
                        if (svar.Contains("Tair_f_inst") || svar.Contains("Qair_f_inst") || svar.Contains("Wind_f_inst"))
                        {
                            lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                            lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                            lConvertedTseries.Attributes.SetValue("Description", lDesc);
                            lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                            if (elev > 0)
                                lConvertedTseries.Attributes.SetValue("Elevation", elev);
                            lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                            lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                            lConvertedTseries.Attributes.GetValue("Longitude"));
                            lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                            if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace))
                                Debug.WriteLine("AddDataset failed when adding GLDAS " + ltseries.ToString());
                        }
                        if (svar.Contains("Wind_f_inst"))
                        {
                            lNextDSN++;
                            isExist = CheckWDMForSeries(lWDM, grid, "WIND");
                            if (isExist) fMain.WriteLogFile("WDM aready contains WIND for " + grid.ToString());

                            //now write the new timeseries to wdm
                            lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                            lConvertedTseries.Attributes.SetValue("Constituent", "WIND");
                            lConvertedTseries.Attributes.SetValue("Description", "Hourly Wind Speed in m/s");
                            lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("Scenario", "GLDAS");
                            if (elev > 0)
                                lConvertedTseries.Attributes.SetValue("Elevation", elev);
                            lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("STANAM", "GLDAS Lat=" +
                            lConvertedTseries.Attributes.GetValue("Latitude").ToString() + " Long=" +
                                  lConvertedTseries.Attributes.GetValue("Longitude").ToString());
                            lConvertedTseries.Attributes.SetValue("COMPFG", 1);
                            if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                Debug.WriteLine("AddDataset failed when adding GLDAS " + lConvertedTseries.ToString());
                        }

                        //clean up
                        lConvertedTseries = null;
                        ltseries = null;
                    } //next
                    lGDS = null;
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
        private bool CalculateAnnualTimeSeries(int enumTrans, atcWDM.atcDataSourceWDM hrWDM, atcWDM.atcDataSourceWDM yrWDM,
              atcTimeseries hlyseries, string svar, string grid, double elev, string units)
        {
            try
            {
                //string annWdmFile = Path.Combine(Path.GetDirectoryName(WdmFile),
                //      Path.GetFileNameWithoutExtension(WdmFile) + "_Annual.wdm");
                int lDSN = 0;
                //if (yrWDM.Open(AnnWdmFile))
                {
                    lDSN = GetNextDSN(yrWDM);
                    lDSN++;

                    //atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(hlyseries, atcTimeUnit.TUYear, 1, atcTran.TranSumDiv,hrWDM);
                    atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(hlyseries, atcTimeUnit.TUYear, 1, (atcTran)enumTrans, hrWDM);
                    ltser.Attributes.SetValue("ID", lDSN);
                    ltser.Attributes.SetValue("Constituent", svar.ToString());
                    ltser.Attributes.SetValue("Description", "Annual " + svar + " in " + units);
                    ltser.Attributes.SetValue("Location", grid.ToString());
                    ltser.Attributes.SetValue("STAID", grid.ToString());
                    ltser.Attributes.SetValue("Scenario", "GLDAS");
                    if (elev > 0)
                        ltser.Attributes.SetValue("Elevation", elev);
                    ltser.Attributes.SetValue("STANAM", "GLDAS Lat=" + hlyseries.Attributes.GetValue("Latitude") +
                    " Long=" + hlyseries.Attributes.GetValue("Longitude"));
                    //lts.Attributes.SetValue("COMPFG", 1);
                    yrWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in generating annual series!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private int GetNextDSN(atcWDM.atcDataSourceWDM aWDM)
        {
            int lDSN = 0;
            //if there are any existing datasets, write new data after them 
            if (aWDM.DataSets.Count > 0)
            {
                int lLastDSN = 0;
                foreach (atcData.atcDataSet lds in aWDM.DataSets)
                    lLastDSN = Math.Max((int)lLastDSN, (int)lds.Attributes.GetValue("ID"));
                lDSN = lLastDSN;
            }
            else
                lDSN = 0;
            return lDSN;
        }

        private bool CheckMissing(atcData.atcTimeseries lTS, string grid, string svar)
        {
            //dMiss set at 999; 
            bool isMiss = false;
            try
            {
                int numvals = (int)lTS.numValues;
                int cntMiss = 0;
                int pcntMiss = 0;
                for (int j = 1; j <= lTS.numValues; j++)
                {
                    double val = lTS.Values[j];
                    if (val > dMiss) cntMiss++;
                }
                pcntMiss = (int)(cntMiss * 100 / numvals);
                if (pcntMiss > PercentMiss)
                {
                    fMain.WriteLogFile("Percent of missing records (" + pcntMiss.ToString() +
                        " for " + grid + ":" + svar +
                        " exceeds threshold!");
                    Debug.WriteLine("Percent of missing records (" + pcntMiss.ToString() +
                        " for " + grid + ":" + svar +
                        " exceeds threshold!");
                    isMiss = true;
                }
                else
                    isMiss = false;
            }
            catch (Exception ex)
            {
                isMiss = true;
            }
            return isMiss;
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
            mdlDB.InsertRecordsInStationTable(grid, sta.StationName, "GLDAS",
                 Convert.ToSingle(sta.Latitude), Convert.ToSingle(sta.Longitude),
                 Convert.ToSingle(sta.Elevation), null, null);

            try
            {
                /*Process each downloaded variables for the grid/site*/
                SortedDictionary<DateTime, string> dictHourly = new SortedDictionary<DateTime, string>();
                string nldasvar;
                foreach (string svar in lstVariables)
                {
                    //102620 ignore SOLR and PEVT first, still to work on this
                    if (svar.Contains("SOLR") || svar.Contains("PEVT")) continue;
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
                            (lDSet.Attributes.GetValue("Scenario").ToString().Contains("GLDAS")))
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
                        (lDSet.Attributes.GetValue("Scenario").ToString().Contains("GLDAS")))
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
                        (lDSet.Attributes.GetValue("Scenario").ToString().Contains("GLDAS")))
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
