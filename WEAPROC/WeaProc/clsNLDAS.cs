using atcData;
using atcUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using WeaModel;
using WeaModelSDB;
using WeaWDM;
using static atcUtility.modDate;

namespace NCEIData
{
    class clsNLDAS
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
        private string WdmFile, AnnWdmFile, ModelSDB, modelDir;
        private int NumDaysRec;
        private float TimeZoneShift;
        //hourly data lWDM
        private atcWDM.atcDataSourceWDM lWDM = new atcWDM.atcDataSourceWDM();
        //annual data annWDM
        private atcWDM.atcDataSourceWDM annualWDM = new atcWDM.atcDataSourceWDM();
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
        //dictionary of selected stations
        public SortedDictionary<string, MetGages> dictSelSites =
                 new SortedDictionary<string, MetGages>();

        public List<string> lstStaDownloaded = new List<string>();
        public List<string> lstSelectedVars = new List<string>();
        public List<string> lstSelVariables = new List<string>();

        private enum TimeStep { Hourly, Daily };
        private string SelectedSta, SelectedVar;
        int irec = 0;

        public clsNLDAS(frmMain _fmain, DateTime _bdate, DateTime _edate, int _UTCshift)
        {
            this.fMain = _fmain;
            this.BegDate = _bdate;
            this.EndDate = _edate;
            this.lstSta = fMain.lstSta;
            this.DownloadedGrid = fMain.DownLoadedSites;
            this.lstSelectedVars = fMain.lstSelectedVars;
            cacheFolder = System.IO.Path.Combine(fMain.cacheDir, "NLDAS"); //fMain.cacheDir;
            dataFolder = fMain.dataDir;
            this.optDataset = fMain.optDataSource;
            this.WdmFile = fMain.WdmFile;
            this.AnnWdmFile = fMain.AnnWdmFile;
            this.ModelSDB = fMain.ModelSDB;
            this.modelDir = fMain.modelDir;

            //dict of station and variables downloaded (excludes series above threshold)
            //need to pass to frmdata
            this.dictSiteVars = fMain.dictSiteVars;
            //dictionary of gages selected, has the attribute in Metgage object
            this.dictSelSites = fMain.dictSelSites;

            this.MinYears = fMain.MinYears;
            NumDaysRec = MinYears * 365;
            this.TimeZoneShift = 0.0F;
            SetVarMapping();
        }
        private void SetVarMapping()
        {
            dictMapVars.Clear();
            dictMapVars.Add("PREC", "Rainf");
            dictMapVars.Add("PRCP", "Rainf");
            dictMapVars.Add("TEMP", "Tair");
            dictMapVars.Add("VWND", "Wind_N");
            dictMapVars.Add("UWND", "Wind_E");
            dictMapVars.Add("SOLR", "SWdown");
            dictMapVars.Add("HUMI", "Qair");
            dictMapVars.Add("LRAD", "LWdown");
            dictMapVars.Add("ATMP", "PSurf");
            //dictMapVars.Add("PEVT", "PEVAPsfc");

            NLDASVars.Clear();
            NLDASVars.Add("Rainf", "PREC");
            NLDASVars.Add("Tair", "ATEM");
            NLDASVars.Add("Wind_N", "WINDV");
            NLDASVars.Add("Wind_E", "WINDU");
            NLDASVars.Add("SWdown", "SOLR");
            NLDASVars.Add("Qair", "DEWP");
            NLDASVars.Add("PSurf", "ATMP"); //surface pressure
            NLDASVars.Add("LWdown", "LRAD");//longwave rad
            //NLDASVars.Add("PEVAPsfc", "PEVT");
        }
        public void ProcessNLDASdata()
        {
            Cursor.Current = Cursors.WaitCursor;

            lstSelVariables.Clear();
            lstStaDownloaded.Clear();
            dictSiteVars.Clear();

            DateTime dtstart = DateTime.Now; ;
            int nsta = lstSta.Count();
            //log begin time
            fMain.WriteLogFile("Begin Download NLDAS ..." + DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            //get list of selected vars from the download screen
            //Debug.WriteLine("Selected Variables");
            string svar;
            foreach (string s in lstSelectedVars)
            {
                dictMapVars.TryGetValue(s, out svar);
                lstSelVariables.Add(svar);
            }

            //download and process each selected grid point
            int isite = 0;
            foreach (string site in lstSta)
            {
                int nsites = lstSta.Count();
                isite++;

                TimeZoneShift = GetTimeZoneOfGrid(site);

                if (DownloadData_NLDAS(lstSelVariables, site, isite, nsites))
                {
                    if (!lstStaDownloaded.Contains(site))
                    {
                        lstStaDownloaded.Add(site);
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

            fMain.lstStaDownloaded = lstStaDownloaded;

            //show frmData screen
            if (lstStaDownloaded.Count > 0) ShowDataTable(lstSelectedVars);

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
        /// DownloadData_NLDAS
        /// Downloads NLDAS data (all variables) for a grid point and saves to WDMFile
        /// </summary>
        /// <param name="selectedVars"></param>
        /// <param name="site"></param>
        /// <param name="isite"></param>
        /// <param name="nsites"></param>
        /// <returns></returns>
        private bool DownloadData_NLDAS(List<string> selectedVars, string site, int isite, int nsites)
        {
            Cursor.Current = Cursors.WaitCursor;
            //nldas base url e.g ----------------------------------------------
            //site is X000Y000
            //download and imports to wdm file
            //-----------------------------------------------------------------
            string nldasfile = string.Empty;
            string gridX = site.Substring(1, 3);
            string gridY = site.Substring(5, 3);

            MetGages sta = new MetGages();
            dictSelSites.TryGetValue(site, out sta);
            string gridLat = Convert.ToString(sta.LATITUDE);
            string gridLng = Convert.ToString(sta.LONGITUDE);

            //string urlpath = "https://hydro1.sci.gsfc.nasa.gov/daac-bin/access/timeseries.cgi?variable=NLDAS:NLDAS_FORA0125_H.002:";
            //"https://hydro1.gesdisc.eosdis.nasa.gov/daac-bin/access/timeseries.cgi?variable=NLDAS2:NLDAS_FORA0125_H_v2.0:Tair&startDate=2024-01-01T00&endDate=2025-01-01T00&location=GEOM:POINT(-96.1875,%2043.9375)&type=asc2";
            string urlpath = "https://hydro1.gesdisc.eosdis.nasa.gov/daac-bin/access/timeseries.cgi?variable=NLDAS2:NLDAS_FORA0125_H_v2.0:";

            atcDateFormat lDateFormat = new atcUtility.atcDateFormat();
            lDateFormat.DateOrder = atcDateFormat.DateOrderEnum.YearMonthDay;
            lDateFormat.IncludeMinutes = false;
            lDateFormat.DateSeparator = "-";
            lDateFormat.DateTimeSeparator = "T";
            lDateFormat.Midnight24 = false;

            string lStartDate = lDateFormat.JDateToString(BegDate.ToOADate());
            string lEndDate = lDateFormat.JDateToString(EndDate.ToOADate());

            //iterate for all selected variables in the grid
            List<string> lstProcessedVar = new List<string>();

            foreach (string svar in selectedVars)
            {
                string lURL = string.Empty;
                double bdt = BegDate.Date.ToOADate();
                double edt = EndDate.Date.ToOADate();
                nldasfile = Path.Combine(cacheFolder, site);
                nldasfile += "-" + svar + "_" + bdt.ToString() + "_" + edt.ToString() + ".txt";
                WriteStatus("Downloading " + svar + " data for " + site +
                          "(" + isite.ToString() + " of " + nsites.ToString() + ")");

                lURL = urlpath + svar;
                lURL += "&startDate=" + lStartDate;
                lURL += "&endDate=" + lEndDate;
                lURL += "&location=GEOM:POINT(" + gridLng + ",%20" + gridLat + ")&type=asc2";

                try
                {
                    bool isDloaded = false;
                    D4EM.Data.Download.DisableHttpsCertificateCheck();
                    D4EM.Data.Download.SetSecurityProtocol();

                    if (!File.Exists(nldasfile))
                        isDloaded = D4EM.Data.Download.DownloadURL(lURL, nldasfile);
                    else //file exist
                        isDloaded = true;
                    if (isDloaded)
                    {
                        fMain.WriteLogFile("Downloaded " + svar + " data for " + site);
                        //get elevation since download does not read elevation attribute
                        double elev = GetGridElevation(nldasfile);
                        if (ProcessSiteDownloadedData(site, svar, nldasfile, elev,isite, nsites))
                        {
                            switch (svar)
                            {
                                case "Rainf":
                                    lstProcessedVar.Add("PREC");
                                    break;
                                case "Tair":
                                    lstProcessedVar.Add("ATEM");
                                    break;
                                case "Wind_E":
                                    lstProcessedVar.Add("WIND");
                                    lstProcessedVar.Add("WNDD");
                                    break;
                                case "SWdown":
                                    lstProcessedVar.Add("SOLR");
                                    lstProcessedVar.Add("LRAD");
                                    lstProcessedVar.Add("CLOU");
                                    break;
                                case "Qair":
                                    lstProcessedVar.Add("DEWP");
                                    break;
                                case "PSurf":
                                    lstProcessedVar.Add("ATMP");
                                    break;
                                case "PEVAPsfc":
                                    //lstProcessedVar.Add("PEVT");
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error downloading and processing NLDAS data: " + site + "-" + svar;
                    ShowError(msg, ex);
                }
            }

            //add grid to dictionary
            dictSiteVars.Add(site, lstProcessedVar);
            lstProcessedVar = null;

            Cursor.Current = Cursors.Default;
            WriteStatus("Ready ..");

            if (!File.Exists(nldasfile)) //file for each variable
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
        private bool ProcessSiteDownloadedData(string grid, string svar, string nldasfile,
            double elev, int isite, int nsites)
        {
            Cursor.Current = Cursors.WaitCursor;
            fMain.WriteLogFile("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(nldasfile));
            fMain.WriteLogFile("Saving " + svar + " for grid " + grid + ": " + Path.GetFileName(WdmFile));
            WriteStatus("Processing " + svar + " for grid " + grid + ": " + Path.GetFileName(nldasfile) +
                "(" + isite.ToString() + " of " + nsites.ToString() + ")");

            string lCons = string.Empty;
            string lDesc = string.Empty;
            string lScen = "NLDAS";
            string datasrc = "WeaProc";

            //timeseries
            SortedDictionary<DateTime, string> dictHourly = new SortedDictionary<DateTime, string>();
            atcData.atcDataAttributes aAttributes = new atcData.atcDataAttributes();
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

                    atcTimeseriesGDS.atcTimeseriesGDS lGDS = new atcTimeseriesGDS.atcTimeseriesGDS();
                    lGDS.Open(nldasfile);
                    lCons = string.Empty; lDesc = string.Empty;
                    string errmsg = "AddDataset failed when adding NLDAS ";

                    foreach (atcData.atcTimeseries lTS in lGDS.DataSets)
                    {
                        lNextDSN += 1;
                        int iTZone = (int)Math.Round(TimeZoneShift,0,MidpointRounding.AwayFromZero);
                        Debug.WriteLine("NLDAS timezone=" + iTZone.ToString());
                        atcTimeseries ltseries = (atcTimeseries)D4EM.Data.MetCmp.modMetDataProcess.ShiftDates(lTS, atcTimeUnit.TUHour, iTZone);
                        atcTimeseries lConvertedTseries = ltseries;
                        bool isExist = false;

                        switch (svar)
                        {
                            case "Rainf":
                                isExist = CheckWDMForSeries(lWDM, grid, "PREC");
                                if (isExist) fMain.WriteLogFile("WDM aready contains PREC for " + grid.ToString());
                                lConvertedTseries = ltseries / 25.4; //OK
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", "PREC");
                                lConvertedTseries.Attributes.SetValue("Description", "Hourly Precip in Inches");
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                //if (elev > 0)
                                //    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace))
                                { }
                                    //CalculateAnnualTimeSeries((int)atcTran.TranSumDiv, lWDM, annualWDM, lConvertedTseries, "PREC", grid, elev, "Inches");
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
                                ltser.Attributes.SetValue("Scenario", "NLDAS");
                                ltser.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    ltser.Attributes.SetValue("Elevation", elev);
                                ltser.Attributes.SetValue("STANAM", "NLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lts.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);

                                ltser = null;
                                break;

                            case "PEVAPsfc":
                                lNextDSN += 1;
                                isExist = CheckWDMForSeries(lWDM, grid, "PEVT");
                                if (isExist) fMain.WriteLogFile("WDM aready contains PEVT for " + grid.ToString());
                                lConvertedTseries = ltseries / 25.4; //OK
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", "PEVT");
                                lConvertedTseries.Attributes.SetValue("Description", "Hourly Potential Evaporation in Inches");
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace))
                                {
                                    //CalculateAnnualTimeSeries((int)atcTran.TranSumDiv, lWDM, annualWDM, lConvertedTseries, "PEVT", grid, elev, "Inches");
                                }
                                else
                                    fMain.WriteLogFile(errmsg + " PEVT!");
                                break;

                            case "Tair":
                                lCons = "ATEM";
                                lDesc = "Hourly Air Temperature in Degrees F";
                                lConvertedTseries = (ltseries * 9 / 5) - 459.67; //OK
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
                                lTmin.Attributes.SetValue("Scenario", "NLDAS");
                                lTmin.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lTmin.Attributes.SetValue("Elevation", elev);
                                lTmin.Attributes.SetValue("STANAM", "NLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lTmin.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(lTmin, atcData.atcDataSource.EnumExistAction.ExistReplace);
                                //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, lTmin, "TMIN", grid, elev, "Temperature Degrees F");
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
                                lTmax.Attributes.SetValue("Scenario", "NLDAS");
                                lTmax.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lTmax.Attributes.SetValue("Elevation", elev);
                                lTmax.Attributes.SetValue("STANAM", "NLDAS Lat=" + lConvertedTseries.Attributes.GetValue("Latitude") +
                                    " Long=" + lConvertedTseries.Attributes.GetValue("Longitude"));
                                //lTmin.Attributes.SetValue("COMPFG", 1);
                                lWDM.AddDataSet(lTmax, atcData.atcDataSource.EnumExistAction.ExistReplace);
                                //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, lTmax, "TMAX", grid, elev, "Temperature Degrees F");
                                lTmax = null;
                                break;

                            case "Wind_E":
                                lCons = "WINDU";
                                lDesc = "Hourly Zonal Wind in mph";
                                lConvertedTseries = (atcTimeseries)ltseries.Clone(); //* 2.237 for mph
                                //01.12.21
                                lConvertedTseries = lConvertedTseries * 2.237; 

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains WINDU for " + grid.ToString());
                                break;

                            case "Wind_N":
                                lCons = "WINDV";
                                lDesc = "Hourly Meridional Wind in m/s";
                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains WINDV for " + grid.ToString());
                                lConvertedTseries = (atcTimeseries)ltseries.Clone(); // * 2.237 for mph
                                //01.12.21
                                lConvertedTseries = lConvertedTseries * 2.237;
                                break;

                            case "PSurf":
                                lCons = "ATMP";
                                lDesc = "Hourly Sea Level Pressure in mmHg";
                                lConvertedTseries = ltseries * 0.00750062; //1 Pa = 0.000750062 mmHg
                                // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding NLDAS " + lConvertedTseries.ToString());
                                break;

                            case "LWdown":
                                lCons = "LRAD";
                                lDesc = "Hourly Longwave Radiation in Langleys";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains LRAD for " + grid.ToString());

                                lConvertedTseries = ltseries * 0.085985; //1 Watt/m2 = 0.085985 Lang/hour
                                                                         // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                {
                                    atcTimeseries drad = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                                    //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, drad, "LRAD", grid, elev, "Langleys");
                                    drad = null;
                                }
                                else
                                    Debug.WriteLine("AddDataset failed when adding NLDAS " + lConvertedTseries.ToString());
                                break;

                            case "SWdown":
                                lCons = "SOLR";
                                lDesc = "Hourly Solar Radiation in Langleys";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains SOLR for " + grid.ToString());

                                lConvertedTseries = ltseries * 0.085985; //1 Watt/m2 = 0.085985 Lang/hour
                                                                         // now write the new timeseries to wdm
                                lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                                lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                                lConvertedTseries.Attributes.SetValue("Description", lDesc);
                                lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                                //Save "SWdown"
                                if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                {
                                    atcTimeseries dsol = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                                    //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, dsol, "SOLR", grid, elev, "Langleys");
                                    dsol = null;
                                }
                                else
                                    Debug.WriteLine("AddDataset failed when adding NLDAS " + lConvertedTseries.ToString());

                                //also use for cloud cover
                                lNextDSN++;
                                lCons = "CLOU";
                                lDesc = "Hourly Cloud Cover from solar radiation";

                                isExist = CheckWDMForSeries(lWDM, grid, lCons);
                                if (isExist) fMain.WriteLogFile("WDM aready contains CLOU for " + grid.ToString());

                                atcTimeseries hlySolar = lConvertedTseries;
                                double llat = Convert.ToDouble(lConvertedTseries.Attributes.GetValue("Latitude"));
                                atcTimeseries lDailyTseries = atcData.modTimeseriesMath.Aggregate(hlySolar, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);
                                //atcTimeseries lDailyTseries = atcData.modTimeseriesMath.Aggregate(lConvertedTseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv, lWDM);

                                //01/12/2021 FIX
                                //atcTimeseries lDailyCCTseries = (atcTimeseries)D4EM.Data.MetCmp.modMetDataUtil.CloudCoverTimeseriesFromSolar(lDailyTseries, lWDM, llat);
                                atcTimeseries lDailyCCTseries = atcMetCmp.modMetCompute.CloudCoverTimeseriesFromSolar(lDailyTseries, lWDM, llat);
                                Debug.WriteLine("Calculated lDailyCCTseries");
                                //for (int i = 1; i < 48; i++)
                                //    Debug.WriteLine(lDailyCCTseries.Dates.Values[i].ToString() + ", " + lDailyCCTseries.Values[i].ToString());

                                for (int lValIndex = 1; lValIndex <= lConvertedTseries.numValues; lValIndex++)
                                {
                                    double val = 0;
                                    int lDayIndex = lDailyCCTseries.Dates.IndexOfValue((int)(lConvertedTseries.Dates.Values[lValIndex]), true);
                                    if (lDayIndex > 0)
                                    {
                                        val = lDailyCCTseries.Values[lDayIndex];
                                        if (val >= 0)
                                            lConvertedTseries.Values[lValIndex] = lDailyCCTseries.Values[lDayIndex];
                                        else
                                        {
                                            lConvertedTseries.Values[lValIndex] = 10;
                                            //Debug.WriteLine("DayIndex={0}, Val={1}, {2}", lDayIndex.ToString(), val.ToString(), lConvertedTseries.Values[lValIndex].ToString());
                                        }
                                    }
                                    else
                                    {
                                        lConvertedTseries.Values[lValIndex] = 0.0;
                                        //Debug.WriteLine("DayIndex={0}, Val={1}, {2}", lDayIndex.ToString(), val.ToString(), lConvertedTseries.Values[lValIndex].ToString());
                                    }
                                    //Debug.WriteLine("DayIndex={0}, Val={1}, {2}", lDayIndex.ToString(), val.ToString(), lConvertedTseries.Values[lValIndex].ToString());
                                    //debug
                                    //if (lValIndex < 100)
                                    //{
                                    //    Debug.WriteLine("{0},{1},{2},{3}", lValIndex.ToString(),
                                    //        lDayIndex.ToString(), val.ToString(), lConvertedTseries.Values[lValIndex]);
                                    //}
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
                                lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                                lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                                if (elev > 0)
                                    lConvertedTseries.Attributes.SetValue("Elevation", elev);
                                lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                                lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                                lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                                lConvertedTseries.Attributes.GetValue("Longitude"));
                                lConvertedTseries.Attributes.SetValue("COMPFG", 1);
                                
                                //Save "CLOUD"
                                if (!lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                                    Debug.WriteLine("AddDataset failed when adding NLDAS " + lConvertedTseries.ToString());
                                break;

                            case "Qair":
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
                                           (lDSet.Attributes.GetValue("Scenario").ToString().Contains("NLDAS")))
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
                        if (svar.Contains("Tair") || svar.Contains("Qair") || svar.Contains("Wind_E") || svar.Contains("Wind_N"))
                        {
                            lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                            lConvertedTseries.Attributes.SetValue("Constituent", lCons);
                            lConvertedTseries.Attributes.SetValue("Description", lDesc);
                            lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                            lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                            if (elev > 0)
                                lConvertedTseries.Attributes.SetValue("Elevation", elev);
                            lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                            lConvertedTseries.Attributes.GetValue("Latitude") + " Long=" +
                            lConvertedTseries.Attributes.GetValue("Longitude"));
                            lConvertedTseries.Attributes.SetValue("COMPFG", 1);

                            if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistReplace))
                            {
                                if ((svar.Contains("Tair") || svar.Contains("Qair")))
                                {
                                    //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, lConvertedTseries, lCons, grid, elev, "deg F");
                                }
                            } else
                                Debug.WriteLine("AddDataset failed when adding NLDAS " + ltseries.ToString());
                        }
                        //Wind_N is processed before Wind_E
                        if (svar.Contains("Wind_E"))
                        {
                            //if we have both Wind_N and Wind_E, compute WIND,  find Wind_N
                            atcTimeseries lUWINDTseries = lConvertedTseries;
                            atcTimeseries lVWINDTseries = new atcTimeseries(lWDM);
                            atcTimeseries lWndDir = new atcTimeseries(lWDM);
                            lWndDir = (atcTimeseries)lConvertedTseries.Clone();

                            lNextDSN++;
                            if (!(lWDM == null))
                            {
                                if (lWDM.DataSets.Count > 0)
                                {
                                    foreach (atcData.atcDataSet lDSet in lWDM.DataSets)
                                    {
                                        if ((lDSet.Attributes.GetValue("Location").ToString().Contains(grid.ToString())) &&
                                            (lDSet.Attributes.GetValue("Constituent").ToString().Contains("WINDV")) &&
                                            (lDSet.Attributes.GetValue("Scenario").ToString().Contains("NLDAS")))
                                        {
                                            lVWINDTseries = (atcTimeseries)lDSet;
                                            break;
                                        }
                                    }
                                }
                            }
                            for (int lValIndex = 1; lValIndex <= lVWINDTseries.numValues; lValIndex++)
                            {
                                double uw = lUWINDTseries.Values[lValIndex];
                                double vw = lVWINDTseries.Values[lValIndex];
                                double wnd = (vw * vw) + (uw * uw);
                                //deg vect from north -180 to 180
                                double wdeg = 180 * Math.Atan2(uw, vw) / Math.PI;
                                //deg met, deg vect+180 , 0 to 360
                                wdeg = wdeg + 180;
                                lConvertedTseries.Values[lValIndex] = Math.Pow(wnd, 0.5);
                                lWndDir.Values[lValIndex] = wdeg;
                            } //next lValIndex

                            isExist = CheckWDMForSeries(lWDM, grid, "WIND");
                            if (isExist) fMain.WriteLogFile("WDM aready contains WIND for " + grid.ToString());

                            //now write the new timeseries to wdm
                            lConvertedTseries.Attributes.SetValue("ID", lNextDSN);
                            lConvertedTseries.Attributes.SetValue("Constituent", "WIND");
                            lConvertedTseries.Attributes.SetValue("Description", "Hourly Wind Speed in mi/hr");
                            lConvertedTseries.Attributes.SetValue("Location", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("Scenario", "NLDAS");
                            lConvertedTseries.Attributes.SetValue("Data Source", datasrc.ToString());
                            if (elev > 0)
                                lConvertedTseries.Attributes.SetValue("Elevation", elev);
                            lConvertedTseries.Attributes.SetValue("STAID", grid.ToString());
                            lConvertedTseries.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                            lConvertedTseries.Attributes.GetValue("Latitude").ToString() + " Long=" +
                                  lConvertedTseries.Attributes.GetValue("Longitude").ToString());
                            lConvertedTseries.Attributes.SetValue("COMPFG", 1);
                            if (lWDM.AddDataSet(lConvertedTseries, atcData.atcDataSource.EnumExistAction.ExistRenumber))
                            {
                                //CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWDM, annualWDM, lConvertedTseries, lCons, grid, elev, "mi/hr");
                            } else
                                Debug.WriteLine("AddDataset failed when adding NLDAS " + lConvertedTseries.ToString());

                            lNextDSN++;
                            isExist = CheckWDMForSeries(lWDM, grid, "WNDD");
                            if (isExist) fMain.WriteLogFile("WDM aready contains WNDD for " + grid.ToString());
                            //now write wind direction to wdm
                            lWndDir.Attributes.SetValue("ID", lNextDSN);
                            lWndDir.Attributes.SetValue("Constituent", "WNDD");
                            lWndDir.Attributes.SetValue("Description", "Hourly Wind Direction");
                            lWndDir.Attributes.SetValue("Location", grid.ToString());
                            lWndDir.Attributes.SetValue("Scenario", "NLDAS");
                            lWndDir.Attributes.SetValue("Data Source", datasrc.ToString());
                            if (elev > 0)
                                lWndDir.Attributes.SetValue("Elevation", elev);
                            lWndDir.Attributes.SetValue("STAID", grid.ToString());
                            lWndDir.Attributes.SetValue("STANAM", "NLDAS Lat=" +
                            lWndDir.Attributes.GetValue("Latitude").ToString() + " Long=" +
                                  lWndDir.Attributes.GetValue("Longitude").ToString());
                            lWndDir.Attributes.SetValue("COMPFG", 1);
                            if (!lWDM.AddDataSet(lWndDir, atcData.atcDataSource.EnumExistAction.ExistReplace))
                                Debug.WriteLine("AddDataset failed when adding NLDAS " + lWndDir.ToString());
                            lWndDir = null;

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
              atcTimeseries hlyseries, string svar, string grid,double elev, string units)
        {
            string datasrc = "WeaProc";
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
                    ltser.Attributes.SetValue("Description", "Annual "+svar+ " in "+units);
                    ltser.Attributes.SetValue("Location", grid.ToString());
                    ltser.Attributes.SetValue("STAID", grid.ToString());
                    ltser.Attributes.SetValue("Scenario", "NLDAS");
                    ltser.Attributes.SetValue("Data Source", datasrc.ToString());
                    if (elev > 0)
                        ltser.Attributes.SetValue("Elevation", elev);
                    ltser.Attributes.SetValue("STANAM", "NLDAS Lat=" + hlyseries.Attributes.GetValue("Latitude") +
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
            mdlDB.InsertRecordsInStationTable(grid, sta.StationName, "NLDAS",
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
                            (lDSet.Attributes.GetValue("Scenario").ToString().Contains("NLDAS")))
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
                    lDate = DateTime.FromOADate(tseries.Dates.Values[lIndex]);
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
                        (lDSet.Attributes.GetValue("Constituent").ToString().Contains(svar)))
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
                        (lDSet.Attributes.GetValue("Scenario").ToString().Contains("NLDAS")))
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
                lDate = DateTime.FromOADate(tseries.Dates.Values[lIndex]);
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
                    //if we have both Wind_N and Wind_E, compute WIND
                    //find Wind_E
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
