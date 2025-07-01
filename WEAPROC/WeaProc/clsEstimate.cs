using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using wdmuploader;
using WeaWDM;

namespace NCEIData
{
    class clsEstimate
    {
        private frmMain fMain;
        private frmData fData;
        private WDM cWDM;
        private string MISS = "9999";
        private TimeSpan td;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        private Dictionary<string, string> dictMapVars = new Dictionary<string, string>();
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> dictHMoments;
        private Dictionary<string, SortedDictionary<DateTime, string>> dictSiteData =
                    new Dictionary<string, SortedDictionary<DateTime, string>>();
        private SortedDictionary<string, List<string>> dictSiteVars;

        //dictionary of gages keyed on variable and sortedDictionary of dsn and Station info
        private Dictionary<string, SortedDictionary<int, clsStation>> dictNearbyGage =
                    new Dictionary<string, SortedDictionary<int, clsStation>>();
        private SortedDictionary<int, clsStation> NearbyGage =
                    new SortedDictionary<int, clsStation>();

        private List<string> lstSelectedVars = new List<string>();
        private List<string> lstSiteVarsEstimated = new List<string>();
        private List<string> lstMissVars;
        private enum MetDataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6 };
        private int optDataset;
        private Random rand;
        private string WdmFile;
        private string TimeUnit;
        private clsStation curSite;
        private string errmsg;
        private double SearchRadius;
        private int LimitStations;
        private string crlf = Environment.NewLine;
        private DateTime MisDateBeg, MisDateEnd;
        private enum Interval { Hourly, Daily };
        private WeaSeries weatherTS;

        public clsEstimate(frmMain _fMain, string site,
            SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> _dictMiss,
            SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> _dictModel,
            SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> _dictHMoments,
            Dictionary<string, SortedDictionary<int, clsStation>> _dictNearbyGage, string _TimeUnit)
        {

            fMain = _fMain;
            weatherTS = fMain.weatherTS;
            this.dictMiss = _dictMiss;
            this.dictModel = _dictModel;
            this.dictHMoments = _dictHMoments;
            this.optDataset = fMain.optDataSource;
            this.dictSiteVars = fMain.dictSiteVars;
            this.WdmFile = fMain.WdmFile;
            this.TimeUnit = _TimeUnit;
            this.dictNearbyGage = _dictNearbyGage;
            //this.wri = _wri;
            //this.lstSiteVarsEstimated = fData.lstSiteVarsEstimated;
        }

        public Dictionary<string, SortedDictionary<DateTime, string>> FillMissingDataByModel(string site)
        {
            Debug.WriteLine(crlf + "Entering FillMissingDataByModel ...");

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DateTime dtbeg = DateTime.Now;
                fMain.WriteLogFile(crlf + "Begin Estimating data for station: " + site + " ... " +
                    DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToLongTimeString());
                fMain.appManager.UpdateProgress("Estimating missing data for station: " + site + " ... ");
                fData.WriteStatus("Estimating missing data for station: " + site + " ... ");
                dtbeg = DateTime.Now;

                //temporary dictionaries of missing, stats for a site and filled data
                SortedDictionary<DateTime, string> dictSeries = new SortedDictionary<DateTime, string>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicMissing = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicFill = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<string, double>> dicSiteModel = new Dictionary<string, SortedDictionary<string, double>>();
               
                //get dictionary of missing and stats for the site
                dictModel.TryGetValue(site, out dicSiteModel);     //model parameters for all vars in site
                if (optDataset==(int)MetDataSource.GHCN)
                    dicMissing = weatherTS.GetMissingSeriesForSite(site);
                else
                    dictMiss.TryGetValue(site, out dicMissing);    //missing series for all variables in site
                //loop for all variables
                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dicMissing)
                {
                    string svar = kv.Key;
                    if (optDataset == (int)MetDataSource.GHCN)
                        dictSeries = GetSeriesFromDict(site, svar);
                    else
                        dictSeries = GetSeriesFromWDM(site, svar);
                    if (!dictSiteData.ContainsKey(svar))
                        //dictSiteData contains series for the site
                        //key is variable, value is series
                        dictSiteData.Add(svar, dictSeries);

                    SortedDictionary<DateTime, string> varmiss;
                    if (optDataset == (int)MetDataSource.GHCN)
                        varmiss = GetMissSeriesFromDict(site, svar);
                    else
                        dicMissing.TryGetValue(svar, out varmiss);

                    SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                    switch (svar)
                    {
                        case "PREC":
                            dicVarFill = FillMissingHourlyDataByRainModel(site, svar, varmiss);
                            dicFill.Add(svar, dicVarFill);
                            break;
                        case "PRCP":
                            dicVarFill = FillMissingDailyDataByRainModel(site, svar, varmiss);
                            dicFill.Add(svar, dicVarFill);
                            break;
                        case "TMAX":
                            dicVarFill = FillMissingDailyDataByWeaModel(site, svar, varmiss);
                            dicFill.Add(svar, dicVarFill);
                            break;
                        case "TMIN":
                            dicVarFill = FillMissingDailyDataByWeaModel(site, svar, varmiss);
                            dicFill.Add(svar, dicVarFill);
                            break;
                        default:
                            dicVarFill = FillMissingHourlyDataByWeaModel(site, svar, varmiss);
                            dicFill.Add(svar, dicVarFill);
                            break;
                    }
                    dicVarFill = null;
                } //end loop svar

                //upload site data (dictSiteData) to wdm
                SaveSiteSeriesToWDM(site);

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End Estimating Hourly Data for station " + site + ": " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                fMain.appManager.UpdateProgress("Ready ...");
                fData.WriteStatus("Ready ... ");

                Cursor.Current = Cursors.Default;
                return dicFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for " + site;
                ShowError(msg, ex);
                return null;
            }
        }
        public Dictionary<string, SortedDictionary<DateTime, string>> FillMissingDataBySpatial(StreamWriter wri, string site)
        {
            Debug.WriteLine(crlf + "Entering FillMissingDataBySpatial ...");
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                DateTime dtbeg = DateTime.Now;
                fMain.WriteLogFile(crlf + "Begin estimating data (spatial) for station: " + site + " ... " +
                    DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToLongTimeString());
                dtbeg = DateTime.Now;

                //temporary dictionaries of missing, stats for a site and filled data
                SortedDictionary<DateTime, string> dictSeries = new SortedDictionary<DateTime, string>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicMissing = new Dictionary<string, SortedDictionary<DateTime, string>>();
                //returned by routine, missing record estimated
                Dictionary<string, SortedDictionary<DateTime, string>> dicFill = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<string, double>> dicSiteModel = new Dictionary<string, SortedDictionary<string, double>>();

                //get dictionary of missing and stats for the site
                dictModel.TryGetValue(site, out dicSiteModel); //model
                
                //10/4/23 usign weatherTS dictionary
                if (optDataset == (int)MetDataSource.GHCN)
                    dicMissing = weatherTS.GetMissingSeriesForSite(site);
                else
                    dictMiss.TryGetValue(site, out dicMissing);    //missing series for all variables in site

                //debug nearby gages
                //Debug.WriteLine("Debugging gages in WDM ...");
                //DebugGageList();

                //loop for all variables
                SortedDictionary<double, clsStation> NearbySta = new SortedDictionary<double, clsStation>();

                //dicMissing is dictionary of time series with missing records for svar (key)
                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dicMissing)
                {
                    string svar = kv.Key;
                    SortedDictionary<DateTime, string> varmiss;
                    //10/4/23 usign weatherTS dictionary
                    if (optDataset == (int)MetDataSource.GHCN)
                        varmiss = GetMissSeriesFromDict(site, svar);
                    else
                        dicMissing.TryGetValue(svar, out varmiss);

                    if (varmiss.Keys.Count > 0)
                    {
                        DateTime MisDateBeg = varmiss.Keys.First();
                        DateTime MisDateEnd = varmiss.Keys.Last();
                        Debug.WriteLine(crlf + "Miss BegDate=" + MisDateBeg.ToString() + ", Miss EndDate=" + MisDateEnd.ToString());

                        fData.WriteStatus("Estimating missing data for : " + site + "-" + svar + " ... ");
                        fMain.WriteLogFile("Searching for NLDAS or GLDAS stations with " + svar);
                        Debug.WriteLine("Estimating missing data for : " + site + "-" + svar + " ... ");
                        NearbySta = GetNearbyStations(site, svar, MisDateBeg, MisDateEnd);

                        if (!(NearbySta.Count > 0))
                        {
                            string msg = "There's no available nearby station with " + svar + " for spatial analysis!" + crlf +
                                "Please estimate missing records using stochastic model.";
                            fMain.WriteLogFile(msg);
                            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }

                        //get site data
                        //dictSiteData contains series for the site, key is variable, value is series
                        //10/4/23 usign weatherTS dictionary
                        if (optDataset == (int)MetDataSource.GHCN)
                            dictSeries = GetSeriesFromDict(site, svar);
                        else
                            dictSeries = GetSeriesFromWDM(site, svar);
                        if (!dictSiteData.ContainsKey(svar))
                            dictSiteData.Add(svar, dictSeries);

                        //varmiss is time series of missing records for svar
                        //dicVarFill is varmiss with missing estimated
                        //SortedDictionary<DateTime, string> varmiss;
                        //dicMissing.TryGetValue(svar, out varmiss);
                        SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();

                        switch (svar)
                        {
                            case "PRCP":
                                //dicVarFill = FillMissingDailyDataBySpatial(site, svar, varmiss, NearbySta);
                                fMain.WriteLogFile("Estimating missing daily data for " + svar);
                                dicVarFill = FillMissingHourlyDataBySpatial(wri, 1, site, svar, varmiss, NearbySta);
                                dicFill.Add(svar, dicVarFill);
                                break;
                            case "TMAX":
                                //dicVarFill = FillMissingDailyDataBySpatial(site, svar, varmiss, NearbySta);
                                fMain.WriteLogFile("Estimating missing daily data for " + svar);
                                dicVarFill = FillMissingHourlyDataBySpatial(wri, 1, site, svar, varmiss, NearbySta);
                                dicFill.Add(svar, dicVarFill);
                                break;
                            case "TMIN":
                                //dicVarFill = FillMissingDailyDataBySpatial(site, svar, varmiss, NearbySta);
                                fMain.WriteLogFile("Estimating missing daily data for " + svar);
                                dicVarFill = FillMissingHourlyDataBySpatial(wri, 1, site, svar, varmiss, NearbySta);
                                dicFill.Add(svar, dicVarFill);
                                break;
                            default:
                                fMain.WriteLogFile("Estimating missing hourly data for " + svar);
                                dicVarFill = FillMissingHourlyDataBySpatial(wri, 0, site, svar, varmiss, NearbySta);
                                dicFill.Add(svar, dicVarFill);
                                break;
                        }
                        dicVarFill = null;
                    }
                } //end loop svar

                //upload wdm with missing records estimated
                SaveSiteSeriesToWDM(site);

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End estimating missing data for site " + site + ": " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                fMain.appManager.UpdateProgress("Ready ...");
                fData.WriteStatus("Ready ... ");

                Cursor.Current = Cursors.Default;
                return dicFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for " + site;
                fMain.WriteLogFile(msg);
                ShowError(msg, ex);
                return null;
            }
        }
        private SortedDictionary<double, clsStation> GetNearbyStations(string site, string svar,
                                            DateTime MisBegDate, DateTime MisEndDate)
        {
            //site is station with missing svar records
            //svar is series with missing records
            Debug.WriteLine("Entering GetNearbyGages ..." + site + ", " + svar);
            SortedDictionary<double, clsStation> OtherSta =
                   new SortedDictionary<double, clsStation>();
            try
            {
                SortedDictionary<int, clsStation> dsngage =
                    new SortedDictionary<int, clsStation>();
                SortedDictionary<int, clsStation> selgage =
                    new SortedDictionary<int, clsStation>();
                clsStation curSta = new clsStation();
                double curStaLon = 0.0;
                double curStaLat = 0.0;

                //get info of station with missing records
                clsStation csta = new clsStation();
                WDM cWDM = new WDM(WdmFile, optDataset);
                csta = cWDM.GetTimeSeriesInfo(svar, site);
                curStaLon = Convert.ToDouble(csta.Longitude);
                curStaLat = Convert.ToDouble(csta.Latitude);
                Debug.WriteLine("Current DSN site={0}, lat={1}, lon={2}, sta={3}",
                    csta.DSN.ToString(), curStaLat, curStaLon, csta.Station); //OK
                cWDM = null;

                dictNearbyGage.TryGetValue(svar, out dsngage);
                //debug
                Debug.WriteLine("In GetNearbyStations: DsnGage Count = " + dsngage.Keys.Count.ToString());
                foreach (var st in dsngage)
                    Debug.WriteLine("{0},{1},{2}", st.Value.Station, st.Value.Constituent, st.Value.Scenario);

                //select only nearby stations with MisBegdate and MisEndDate
                selgage = SelectNearbyStations(dsngage, MisBegDate, MisEndDate, "NLDAS");

                //get current station dsn and station info
                if (selgage.Keys.Count > 0)
                {
                    //get distances to the other stations 
                    Debug.WriteLine("Number of DSN in list is " + selgage.Keys.ToList().Count.ToString());
                    foreach (int dsn in selgage.Keys.ToList())
                    {
                        clsStation sta = new clsStation();
                        if (selgage.TryGetValue(dsn, out sta))
                        {
                            double lon2 = Convert.ToDouble(sta.Longitude);
                            double lat2 = Convert.ToDouble(sta.Latitude);
                            double distMi = CalcDistance(curStaLat, lat2, curStaLon, lon2);

                            //get all stations
                            //if (distMi < SearchRadius)
                            {
                                //Debug.WriteLine("Selected nearby gages: {0},{1},{2},{3},dist={4}",
                                //    svar, sta.DSN.ToString(), sta.Station, sta.Constituent, distMi.ToString());
                                if (!OtherSta.ContainsKey(distMi))
                                    OtherSta.Add(distMi, sta);
                            }
                        }
                        else
                            Debug.WriteLine("No timeseries for DSN=" + dsn);
                        sta = null;
                    }

                    //debug
                    //int niter = OtherSta.Keys.Count > LimitStations ? LimitStations : OtherSta.Keys.Count;
                    //for (int i=0; i<niter;i++)
                    //{
                    //    double distmi = OtherSta.Keys.ElementAt(i);
                    //    clsStation sta = new clsStation();
                    //    OtherSta.TryGetValue(distmi, out sta);
                    //    Debug.WriteLine("Selected nearby gages: {0},{1},{2}, dist={3}",
                    //        svar, sta.DSN.ToString(), sta.Station,  distmi.ToString());
                    //    fMain.WriteLogFile("Selected nearby gages: "+svar+ ", " +
                    //        sta.Station + ", Dist=" + distmi.ToString());
                    //}
                }

                dsngage = null;
                selgage = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error in getting nearby gages ...";
                ShowError(errmsg, ex);
            }
            Debug.WriteLine("Exiting GetNearbyGages ...");
            return OtherSta;
        }
        //modified 11/24/20 includes GLDAS
        private SortedDictionary<int, clsStation> SelectNearbyStations(SortedDictionary<int, clsStation> dsngage,
                    DateTime MisBegDate, DateTime MisEndDate, string scenario)
        {
            SortedDictionary<int, clsStation> selgage = new SortedDictionary<int, clsStation>();
            try
            {
                Debug.WriteLine("Entering SelectNearbyStations ...");
                Debug.WriteLine("dsngage count =" + dsngage.Keys.Count.ToString());
                //select only NLDAS stations
                List<clsStation> lstNLDAS = dsngage.Values.ToList();
                //List<int> lstRemoveDSN = new List<int>();
                foreach (clsStation sta in lstNLDAS)
                {
                    Debug.WriteLine("sta =" + sta.Station + ", svar=" + sta.Constituent + ", scen=" + sta.Scenario);
                    //remove sites that are not NLDAS or GLDAS
                    if (scenario.Contains("NLDAS"))
                    {
                        if (sta.Scenario.Contains("NLDAS") || sta.Scenario.Contains("GLDAS"))
                        {
                            //filter by date
                            if (sta.BegDate.CompareTo(MisBegDate) <= 0)
                            {
                                if (!selgage.ContainsKey(sta.DSN))
                                    selgage.Add(sta.DSN, sta);
                                //lstRemoveDSN.Add(sta.DSN);
                                //Debug.WriteLine("Ignoring " + sta.Station + " from nearby gages due to period of record.");
                            }
                        }
                        //else
                        //{
                        //    lstRemoveDSN.Add(sta.DSN);
                        //    Debug.WriteLine("Ignoring " + sta.Station + " from nearby gages.");
                        //}
                    }
                }

                //remove sites that are in the lstRemoveDSN
                //if (lstRemoveDSN.Count > 0)
                //{
                //    foreach (int dsn in lstRemoveDSN)
                //        dsngage.Remove(dsn);
                //}
                //debug
                foreach (var dsn in selgage)
                    Debug.WriteLine("{0},{1}", dsn.Value.Station, dsn.Value.Constituent);
                Debug.WriteLine("Exiting SelectNearbyStations ...");
                return selgage;
            }
            catch (Exception ex)
            {
                string msg = "Error in selecting nearby gages... ";
                ShowError(msg, ex);
                return null;
            }
        }
        public SortedDictionary<DateTime, string> FillMissingHourlyDataByWeaModel(string site, string svar,
                    SortedDictionary<DateTime, string> varmiss)
        {
            try
            {
                //initialize random generator
                rand = new Random();

                Dictionary<string, SortedDictionary<Int32, List<double>>> dicSiteMoments =
                                          new Dictionary<string, SortedDictionary<Int32, List<double>>>();
                dictHMoments.TryGetValue(site, out dicSiteMoments); //site moments from global dictionary

                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();

                //series for variable
                SortedDictionary<DateTime, string> siteVar;
                dictSiteData.TryGetValue(svar, out siteVar);

                //Variable moments from site dictionary
                SortedDictionary<Int32, List<double>> varMoments;   //harmonic moments for variable
                dicSiteMoments.TryGetValue(svar, out varMoments);

                //AR model parameters for site and variable
                Dictionary<string, SortedDictionary<string, double>> siteModel; //AR model parameters for site
                SortedDictionary<string, double> varModel;                      //get AR model parameters for variable svar
                dictModel.TryGetValue(site, out siteModel);
                siteModel.TryGetValue(svar, out varModel);                      //get AR model parameters for variable svar

                //loop for all data in variable series
                string misdat = string.Empty, predat = string.Empty;
                double theta = 0.0;

                //get AR parameter for site var
                double std, sigma2;
                varModel.TryGetValue("varerr", out sigma2);
                std = Math.Sqrt(sigma2);
                varModel.TryGetValue("ar1coeff", out theta);
                double prb = 0.0, err;

                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    DateTime curdt = kv1.Key;
                    DateTime predt = curdt.AddHours(-1);
                    int curmonhr = 100 * curdt.Month + curdt.Hour;
                    int premonhr = 100 * predt.Month + predt.Hour;

                    //get current and previous harmonic moments
                    List<double> curStats; List<double> preStats;
                    varMoments.TryGetValue(curmonhr, out curStats);
                    varMoments.TryGetValue(premonhr, out preStats);

                    //case for first observation-WORK ON THIS
                    if (!siteVar.TryGetValue(predt, out predat))
                    {
                        prb = rand.NextDouble();
                        double val = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, std, prb);
                        predat = (preStats[1] * val + preStats[0]).ToString();
                    }

                    prb = rand.NextDouble();
                    err = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, std, prb);

                    if (!predat.Contains(MISS))
                    {
                        double preresid = (Convert.ToDouble(predat) - preStats[0]) / preStats[1];
                        double dat = ((theta * preresid) + err) * curStats[1] + curStats[0];

                        if (svar.Contains("WIND"))
                        {
                            if (dat < 0) dat = 0.0;
                        }
                        else if (svar.Contains("WNDD"))
                        {
                            if (dat < 10) dat = 10.0;
                            if (dat > 360) dat = 360.0;
                        }
                        else if (svar.Contains("CLOU"))
                        {
                            if (dat < 0) dat = 0.0;
                            else if (dat > 10) dat = 10.0;
                            dat = ReClassCloud(dat);
                        }
                        misdat = dat.ToString("F3");
                    }
                    else
                    {
                        double dat = curStats[0];
                        misdat = dat.ToString("F3");
                    }
                    //replace missing record for curdt in siteVar 
                    //add estimate to dicVarFill
                    siteVar.Remove(curdt);
                    siteVar.Add(curdt, misdat);
                    dicVarFill.Add(curdt, misdat);
                }
                //replace siteVar series in dictSiteData
                dictSiteData.Remove(svar);
                dictSiteData.Add(svar, siteVar);

                //clean up
                dicSiteMoments = null; siteModel = null; siteVar = null;
                varModel = null; varMoments = null;

                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }
        public SortedDictionary<DateTime, string> FillMissingDailyDataByWeaModel(string site, string svar,
                    SortedDictionary<DateTime, string> varmiss)
        {
            Debug.WriteLine("Entering FillMissingDataByWeaModel");
            try
            {
                //initialize random generator
                rand = new Random();

                Dictionary<string, SortedDictionary<DateTime, string>> dicSite =
                                          new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<Int32, List<double>>> dicSiteMoments =
                                          new Dictionary<string, SortedDictionary<Int32, List<double>>>();

                //dictData.TryGetValue(site, out dicSite);            //site data from global dictionary
                dictHMoments.TryGetValue(site, out dicSiteMoments);   //site moments from global dictionary

                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                SortedDictionary<DateTime, string> siteVar;        //series for variable
                dictSiteData.TryGetValue(svar, out siteVar);       //get variable series for site

                //Variable moments from site dictionary
                SortedDictionary<Int32, List<double>> varMoments;   //harmonic moments for variable
                dicSiteMoments.TryGetValue(svar, out varMoments);

                //AR model parameters for site and variable
                Dictionary<string, SortedDictionary<string, double>> siteModel; //AR model parameters for site
                SortedDictionary<string, double> varModel;                      //get AR model parameters for variable svar
                dictModel.TryGetValue(site, out siteModel);
                siteModel.TryGetValue(svar, out varModel);                      //get AR model parameters for variable svar

                //loop for all data in variable series
                string misdat = string.Empty, predat = string.Empty;
                double theta = 0.0;

                //get AR parameter for site var
                double std, sigma2;
                varModel.TryGetValue("dlyvarerr", out sigma2);
                std = Math.Sqrt(sigma2);
                varModel.TryGetValue("dlyar1coeff", out theta);
                double prb = 0.0, err;

                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    DateTime curdt = kv1.Key;
                    DateTime predt = curdt.AddDays(-1);
                    int curmonhr = 100 * curdt.Month + curdt.Day;
                    int premonhr = 100 * predt.Month + predt.Day;

                    //get current and previous harmonic moments
                    List<double> curStats; List<double> preStats;
                    varMoments.TryGetValue(curmonhr, out curStats);
                    varMoments.TryGetValue(premonhr, out preStats);

                    //case for first observation-WORK ON THIS
                    if (!siteVar.TryGetValue(predt, out predat))
                    {
                        prb = rand.NextDouble();
                        double val = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, std, prb);
                        predat = (preStats[1] * val + preStats[0]).ToString();
                    }

                    prb = rand.NextDouble();
                    err = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, std, prb);

                    if (!predat.Contains(MISS))
                    {
                        double preresid = (Convert.ToDouble(predat) - preStats[0]) / preStats[1];
                        double dat = ((theta * preresid) + err) * curStats[1] + curStats[0];
                        misdat = dat.ToString("F3");
                    }
                    else
                    {
                        double dat = curStats[0];
                        misdat = dat.ToString("F3");
                    }
                    siteVar.Remove(curdt);
                    siteVar.Add(curdt, misdat);

                    dicVarFill.Add(curdt, misdat);
                }
                //replace siteVar series in dictSiteData
                dictSiteData.Remove(svar);
                dictSiteData.Add(svar, siteVar);

                //clean up
                dicSite = null; dicSiteMoments = null; siteModel = null; siteVar = null;
                varModel = null; varMoments = null;

                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }
        public SortedDictionary<DateTime, string> FillMissingHourlyDataByRainModel(string site, string svar,
                    SortedDictionary<DateTime, string> varmiss)
        {
            //-----------------------------------------------------------------------
            // Generation of dry wet sequence
            // Given a dry day, generate uniform random number between 0,1(prob)
            //  if prob > PD | W(prob of dry given wet day), current is dry
            //  if prob < PD | W current is wet
            //
            // Given a wet day, generate random prob
            //  if prob > PW | W(prob of wet given wet), current is dry
            //  if prob < PW | W current is wet
            //-----------------------------------------------------------------------

            // initialize random number generator
            rand = new Random();
            try
            {
                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                SortedDictionary<DateTime, string> siteRain; //rainfall series dictionary
                //siteRain = GetSeriesFromWDM(site, svar);
                //series for variable
                dictSiteData.TryGetValue(svar, out siteRain);


                //Markov parameters for site and variable (PREC for ISD, GHCN, HPCP for hourly)
                Dictionary<string, SortedDictionary<string, double>> siteModel;
                SortedDictionary<string, double> rainModel;
                dictModel.TryGetValue(site, out siteModel);
                siteModel.TryGetValue(svar, out rainModel);

                //initialize variables for model
                double pww = 0.0, pdw = 0.0; //, pdd=0.0, pdw=0.0;
                double gama = 0.0, prerain = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0;

                //loop for all data in variable series
                int mon;
                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    string sdat, misrain;
                    DateTime curdt = kv1.Key;
                    DateTime predt = curdt.AddHours(-1);
                    mon = predt.Month;

                    if (!siteRain.TryGetValue(predt, out sdat))
                        sdat = "0.0";

                    //get parameters
                    rainModel.TryGetValue("gamma" + mon, out gama);
                    rainModel.TryGetValue("alpha" + mon, out alpha);
                    rainModel.TryGetValue("beta" + mon, out beta);
                    rainModel.TryGetValue("pdw" + mon, out pdw);
                    rainModel.TryGetValue("pww" + mon, out pww);
                    prerain = Convert.ToDouble(sdat);
                    prob = GenerateRandomProb();

                    if (prerain <= 0)                         //previous hour dry
                    {
                        if (prob > pdw)                       //missing is dry
                            misrain = "0.0";
                        else
                            misrain = GenerateHourRain(alpha, beta); //missing is wet, generate rain
                    }
                    else                                      //previous hour wet
                    {
                        if (prob > pww)                       //missing hour is dry
                            misrain = "0.0";                  //dry hour
                        else                                  //missing is wet, generate
                            misrain = GenerateHourRain(alpha, beta);
                    }
                    dicVarFill.Add(curdt, misrain);
                    siteRain.Remove(curdt);
                    siteRain.Add(curdt, misrain);
                }
                //replace siteVar series in dictSiteData
                dictSiteData.Remove(svar);
                dictSiteData.Add(svar, siteRain);

                //cleanup
                siteRain = null; siteModel = null; rainModel = null;
                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing hourly rainfall records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }
        public SortedDictionary<DateTime, string> FillMissingDailyDataByRainModel(string site, string svar,
                    SortedDictionary<DateTime, string> varmiss)
        {
            //-----------------------------------------------------------------------
            // Generation of dry wet sequence
            // Given a dry day, generate uniform random number between 0,1(prob)
            //  if prob > PD | W(prob of dry given wet day), current is dry
            //  if prob < PD | W current is wet
            //
            // Given a wet day, generate random prob
            //  if prob > PW | W(prob of wet given wet), current is dry
            //  if prob < PW | W current is wet
            //-----------------------------------------------------------------------

            // initialize random number generator
            rand = new Random();
            try
            {
                Dictionary<string, SortedDictionary<DateTime, string>> dicSite =
                                              new Dictionary<string, SortedDictionary<DateTime, string>>();
                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                SortedDictionary<DateTime, string> siteRain; //rainfall series dictionary
                //dictData.TryGetValue(site, out dicSite);     //site data from global dictionary
                dictSiteData.TryGetValue(svar, out siteRain);     //get rainfall series for site

                //Markov parameters for site and variable (PREC for ISD,GHCN, HPCP for hourly)
                Dictionary<string, SortedDictionary<string, double>> siteModel;
                SortedDictionary<string, double> rainModel;
                dictModel.TryGetValue(site, out siteModel);
                siteModel.TryGetValue(svar, out rainModel);

                //initialize variables for model
                double pww = 0.0, pdw = 0.0; //, pdd=0.0, pdw=0.0;
                double gama = 0.0, prerain = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0;

                //loop for all data in variable series
                int mon;
                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    string sdat, misrain;
                    DateTime curdt = kv1.Key;
                    DateTime predt = curdt.AddDays(-1);
                    mon = predt.Month;

                    if (!siteRain.TryGetValue(predt, out sdat))
                        sdat = "0.0";

                    //get parameters
                    rainModel.TryGetValue("dlygamma" + mon, out gama);
                    rainModel.TryGetValue("dlyalpha" + mon, out alpha);
                    rainModel.TryGetValue("dlybeta" + mon, out beta);
                    rainModel.TryGetValue("dlypdw" + mon, out pdw);
                    rainModel.TryGetValue("dlypww" + mon, out pww);
                    prerain = Convert.ToDouble(sdat);
                    prob = GenerateRandomProb();

                    if (prerain <= 0)                         //previous day dry
                    {
                        if (prob > pdw)                       //missing is dry
                            misrain = "0.0";
                        else
                            misrain = GenerateHourRain(alpha, beta); //missing is wet, generate rain
                    }
                    else                                      //previous day wet
                    {
                        if (prob > pww)                       //missing day is dry
                            misrain = "0.0";                  //dry day
                        else                                  //missing is wet, generate
                            misrain = GenerateHourRain(alpha, beta);
                    }
                    dicVarFill.Add(curdt, misrain);
                    siteRain.Remove(curdt);
                    siteRain.Add(curdt, misrain);
                }
                //replace siteVar series in dictSiteData
                dictSiteData.Remove(svar);
                dictSiteData.Add(svar, siteRain);

                //clean up
                dicSite = null; siteRain = null; siteModel = null; rainModel = null;
                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing daily rainfall records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }
        private SortedDictionary<DateTime, string> GetSeriesFromDict(string sta, string svar)
        {
            Debug.WriteLine("Entering Estimate: GetSeriesFromDict ...");
            try
            {
                SortedDictionary<DateTime, string> tSeries;
                tSeries = weatherTS.GetSeries(sta, svar);
                return tSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from dictionary!", ex);
                return null;
            }
        }
        private SortedDictionary<DateTime, string> GetMissSeriesFromDict(string sta, string svar)
        {
            Debug.WriteLine("Entering Estimate: GetMissSeriesFromDict ...");
            try
            {
                SortedDictionary<DateTime, string> tSeries;
                tSeries = weatherTS.GetMissSeries(sta, svar);
                Debug.WriteLine("Num missing is " + tSeries.Count.ToString());
                return tSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series missing data from dictionary!", ex);
                return null;
            }
        }
        private SortedDictionary<DateTime, string> GetSeriesFromWDM(string sta, string svar)
        {
            Debug.WriteLine("Entering GetSeriesFromWDM ...");
            try
            {
                SortedDictionary<DateTime, string> dictVarSeries;

                clsWdm cWDM = new clsWdm(WdmFile, optDataset);
                dictVarSeries = cWDM.ReadWeatherSeries(sta, svar);
                cWDM = null;
                return dictVarSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from wdm file!", ex);
                return null;
            }
        }
        private SortedDictionary<DateTime, string> GetSeriesFromWDMbyDSN(int dsn)
        {
            Debug.WriteLine("Entering GetSeriesFromWDMbyDSN ...");
            try
            {
                SortedDictionary<DateTime, string> dictVarSeries;

                clsWdm cWDM = new clsWdm(WdmFile, optDataset);
                dictVarSeries = cWDM.ReadWeatherSeriesByDSN(dsn);
                cWDM = null;
                return dictVarSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from wdm file!", ex);
                return null;
            }
        }
        private SortedDictionary<DateTime, string> FillMissingDailyDataBySpatial(string site, string svar,
                    SortedDictionary<DateTime, string> varmiss, SortedDictionary<double, clsStation> NearbySta)
        {
            Debug.WriteLine(crlf + "Entering FillMissingDailyDataBySpatial ...");
            try
            {
                int nSta = 0;
                Cursor.Current = Cursors.WaitCursor;

                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                //dictOtherSta is dictionary of values for the other stations for each datetime in varmiss
                SortedDictionary<DateTime, List<string>> dictOtherSta = new SortedDictionary<DateTime, List<string>>();
                //series for variable
                SortedDictionary<DateTime, string> siteVar;
                dictSiteData.TryGetValue(svar, out siteVar);

                List<DateTime> missdt = varmiss.Keys.ToList();
                List<clsStation> OtherSta = NearbySta.Values.ToList();
                fMain.WriteLogFile("Number of Nearby Station is " + OtherSta.Count.ToString());

                //initialize dictionary of nearby stations for all missing dates
                for (int i = 0; i < missdt.Count; i++)
                    dictOtherSta.Add(missdt.ElementAt(i), new List<string>());

                nSta = OtherSta.Count > LimitStations ? LimitStations : OtherSta.Count;
                fMain.WriteLogFile("Limit of Nearby Stations for Analysis is " + nSta.ToString());

                //calculate weights
                List<double> weights = new List<double>();
                for (int i = 0; i < nSta; i++)
                {
                    double dis = NearbySta.Keys.ElementAt(i);
                    weights.Add(1.0 / Math.Pow(dis, 2.0));
                }

                //get nearby station data
                for (int i = 0; i < nSta; i++)
                {
                    SortedDictionary<DateTime, string> dseries =
                              new SortedDictionary<DateTime, string>();
                    var dsn = OtherSta.ElementAt(i).DSN;
                    dseries = GetSeriesFromWDMbyDSN(dsn);
                    //Debug.WriteLine("Num in dseries =" + dseries.Count.ToString());

                    foreach (KeyValuePair<DateTime, string> kv in varmiss)
                    {
                        string svalue;
                        DateTime dt = kv.Key;
                        dseries.TryGetValue(dt, out svalue);

                        List<string> svals = new List<string>();
                        dictOtherSta.TryGetValue(dt, out svals);
                        svals.Add(svalue);
                        dictOtherSta.Remove(dt);
                        dictOtherSta.Add(dt, svals);
                        svals = null;
                    }
                    dseries = null;
                }

                //debug
                //DebugNearbyStationData(dictOtherSta);
                //fill in missing data

                List<string> otherVals = new List<string>();
                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    DateTime curdt = kv1.Key;
                    string dat = kv1.Value;

                    dictOtherSta.TryGetValue(curdt, out otherVals);
                    if (dat.Contains(MISS))
                    {
                        double sum = 0.0, sumwt = 0.0;
                        for (int i = 0; i < nSta; i++)
                        {
                            if (!otherVals[i].Contains(MISS))
                            {
                                sum += weights[i] * Convert.ToDouble(otherVals[i]);
                                sumwt += weights[i];
                            }
                        }
                        dat = (sumwt > 0.0) ? (sum / sumwt).ToString() : MISS;
                    }
                    //debug
                    //Debug.WriteLine("Estimate: {0}, {1}, {2}", svar, curdt.ToString(), dat);
                    siteVar.Remove(curdt);
                    siteVar.Add(curdt, dat);
                    dicVarFill.Add(curdt, dat);
                }
                //clean up
                siteVar = null; dictOtherSta = null;
                otherVals = null; OtherSta = null; missdt = null;

                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }
        
        private SortedDictionary<DateTime, string> FillMissingHourlyDataBySpatial(StreamWriter wri, int tstep, string site, string svar,
            SortedDictionary<DateTime, string> varmiss, SortedDictionary<double, clsStation> NearbySta)
        {
            Debug.WriteLine(crlf + "Entering FillMissingHourlyDataBySpatial ...");
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                int nSta = 0;
                double SiteVarMean = 0.0;
                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                //dictOtherSta is dictionary of nearby station data, dict values is a list
                SortedDictionary<DateTime, List<string>> dictOtherSta = new SortedDictionary<DateTime, List<string>>();
                //dictOtherSeries is dictionary of nearby station series keyed on dsn, dict values is a dictionary of  series
                Dictionary<int, SortedDictionary<DateTime, string>> dictOtherSeries = new Dictionary<int, SortedDictionary<DateTime, string>>();

                //sitevar is the timeseries for variable
                SortedDictionary<DateTime, string> siteVar;
                dictSiteData.TryGetValue(svar, out siteVar);
                SiteVarMean = GetLongTermMean(siteVar, true);

                List<DateTime> missdt = varmiss.Keys.ToList();
                List<clsStation> OtherSta = NearbySta.Values.ToList();
                fMain.WriteLogFile("Number of Nearby Station is " + OtherSta.Count.ToString());

                //initialize dictionary of nearby stations for all missing dates
                for (int i = 0; i < missdt.Count; i++)
                    dictOtherSta.Add(missdt.ElementAt(i), new List<string>());

                nSta = OtherSta.Count > LimitStations ? LimitStations : OtherSta.Count;
                fMain.WriteLogFile("Limit of Nearby Stations for Analysis is " + nSta.ToString());

                //calculate weights
                List<double> weights = new List<double>();
                List<double> Distance = new List<double>();
                for (int i = 0; i < nSta; i++)
                {
                    double dis = NearbySta.Keys.ElementAt(i);
                    weights.Add(1.0 / Math.Pow(dis, 2.0));
                    Distance.Add(dis);
                }

                fMain.WriteLogFile("Selected station-var to estimate: " + site + "-" + svar +
                      ", LongTerm Mean=" + SiteVarMean.ToString());

                //get nearby station data and save to dictOtherSta dictionary
                List<double> NearbySeriesMean = new List<double>();
                List<string> NearbyStations = new List<string>();
                for (int i = 0; i < nSta; i++)
                {
                    SortedDictionary<DateTime, string> dseries =
                              new SortedDictionary<DateTime, string>();
                    var dsn = OtherSta.ElementAt(i).DSN;
                    dseries = GetSeriesFromWDMbyDSN(dsn);

                    //for validation
                    if (!dictOtherSeries.ContainsKey(i))
                        dictOtherSeries.Add(i, dseries);

                    //get longterm mean of nearby series i
                    double mean = GetLongTermMean(dseries, false);
                    NearbySeriesMean.Add(mean);
                    NearbyStations.Add(OtherSta.ElementAt(i).Station);

                    //debug nearby stations
                    double distmi = NearbySta.Keys.ElementAt(i);
                    clsStation sta = new clsStation();
                    NearbySta.TryGetValue(distmi, out sta);
                    fMain.WriteLogFile("Selected nearby gages: " + svar + ", " +
                        sta.Station + ", Dist=" + distmi.ToString() + ", Mean=" + mean);
                    sta = null;

                    //get nearby series values for varmiss dates and saved to dictOtherSta
                    foreach (KeyValuePair<DateTime, string> kv in varmiss)
                    {
                        string svalue;
                        DateTime dt = kv.Key;
                        dseries.TryGetValue(dt, out svalue);

                        List<string> svals = new List<string>();
                        dictOtherSta.TryGetValue(dt, out svals);
                        svals.Add(svalue);
                        dictOtherSta.Remove(dt);
                        dictOtherSta.Add(dt, svals);
                        svals = null;
                    }
                    dseries = null;
                }

                //debug
                //DebugNearbyStationData(dictOtherSta);

                //estimate missing data weighted by distance and longterm mean
                List<string> otherVals = new List<string>();
                foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                {
                    DateTime curdt = kv1.Key;
                    string dat = kv1.Value;

                    dictOtherSta.TryGetValue(curdt, out otherVals);
                    int numdat = 0;
                    if (dat.Contains(MISS))
                    {
                        double sum = 0.0, sumwt = 0.0;
                        for (int ii = 0; ii < nSta; ii++)
                        {
                            double meanratio = SiteVarMean / NearbySeriesMean[ii];
                            if (!(string.IsNullOrEmpty(otherVals[ii])))
                            {
                                if (!(otherVals[ii].Contains(MISS)))
                                {
                                    numdat++;
                                    sum += weights[ii] * Convert.ToDouble(otherVals[ii]) * meanratio;
                                    sumwt += weights[ii];
                                }
                            }
                        }
                        dat = (sumwt > 0.0) ? (sum / (sumwt)).ToString("F3") : MISS;
                    }
                    //debug
                    Debug.WriteLine("Estimate: {0}, {1}, {2}", svar, curdt.ToString(), dat);
                    siteVar.Remove(curdt);
                    siteVar.Add(curdt, dat);
                    dicVarFill.Add(curdt, dat);
                }
                //replace siteVar series in dictSiteData
                dictSiteData.Remove(svar);
                dictSiteData.Add(svar, siteVar);

                fMain.WriteLogFile("Estimated missing records for " + site + "-" + svar);

                //validate estimation process
                clsValidateSpatial Validate = new clsValidateSpatial(fMain, wri, siteVar, dictOtherSeries,
                                NearbySeriesMean, weights, Distance, NearbyStations, svar, site);
                Validate.CrossValidateSpatial(tstep, nSta, SiteVarMean);
                Validate = null;

                //clean up
                siteVar = null; dictOtherSta = null;
                otherVals = null; OtherSta = null; missdt = null;
                NearbySeriesMean = null; NearbyStations = null;
                weights = null; Distance = null;

                Cursor.Current = Cursors.Default;
                return dicVarFill;
            }
            catch (Exception ex)
            {
                string msg = "Error in estimating missing records for " + site;
                ShowError(msg, ex);
                return null;
            }
        }

        private double GetLongTermMean(SortedDictionary<DateTime, string> dseries, bool WithMissing)
        {
            //get longterm mean of nearby series i
            double mean = 0.0;
            List<double> lstvals = new List<double>();
            if (WithMissing)
                lstvals = (from num in dseries.Values
                           where !num.Contains(MISS)
                           select Convert.ToDouble(num)).ToList();
            else
                lstvals = (from num in dseries.Values
                           select Convert.ToDouble(num)).ToList();

            mean = lstvals.Average();
            lstvals = null;
            return mean;
        }
        private bool SaveSiteSeriesToWDM(string site)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<string> siteAttrib = new List<string>();
                fMain.dictSta.TryGetValue(site, out siteAttrib);

                clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib,
                                         dictSiteData, WdmFile, fMain.optDataSource,TimeUnit);
                cWDM.UploadSeriesToWDM(site, WdmFile);
                cWDM = null;
                siteAttrib = null;
                Cursor.Current = Cursors.Default;

                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in uploading timeseries for site:" + site;
                fMain.WriteLogFile(msg);
                ShowError(msg, ex);
                return false;
            }
        }
        private double ReClassCloud(double sdat)
        {
            double clou = 0.0;

            if (sdat < 0.5)
                clou = 0.0;
            else if (sdat < 1.75)
                clou = 1.0;
            else if (sdat < 3.25)
                clou = 2.5;
            else if (sdat < 4.5)
                clou = 4.0;
            else if (sdat < 5.5)
                clou = 5.0;
            else if (sdat < 6.75)
                clou = 6.0;
            else if (sdat < 8.25)
                clou = 7.5;
            else if (sdat < 9.5)
                clou = 9.0;
            else
                clou = 10.0;
            return clou;
        }
        private double GenerateRandomProb()
        {
            return rand.NextDouble();
        }
        private string GenerateHourRain(double alpha, double beta)
        {
            string srain = string.Empty;
            double rate = 1.0 / beta;
            double prb = rand.NextDouble();
            double rain = MathNet.Numerics.Distributions.Gamma.InvCDF(alpha, rate, prb);
            if (rain < 0) rain = 0.0;
            return rain.ToString("F3");
        }
        public void ReplaceMissingData(string site, Dictionary<string, SortedDictionary<DateTime, string>> dictFillSite)
        {
            try
            {
                SortedDictionary<DateTime, string> dictHlyData;
                SortedDictionary<DateTime, string> dictVarFill;

                //do for all vars in the site
                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dictFillSite)
                {
                    string svar = kv.Key;
                    dictHlyData = new SortedDictionary<DateTime, string>();
                    dictVarFill = new SortedDictionary<DateTime, string>();
                    dictFillSite.TryGetValue(svar, out dictVarFill);

                    //get dataseries from cache WDM
                    dictHlyData = GetSeriesFromWDM(site, svar);
                    foreach (KeyValuePair<DateTime, string> kv1 in dictVarFill)
                    {
                        DateTime dt = kv1.Key;
                        string dat = kv1.Value;
                        dictHlyData.Remove(dt);
                        dictHlyData.Add(dt, dat);
                        //Debug.WriteLine("{0},{1}", dt.ToString(), dat.ToString());
                    }
                    dictVarFill = null;
                    dictHlyData = null;
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in replacing missing records for " + site;
                ShowError(msg, ex);
            }
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + ex.Message + crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            fMain.WriteLogFile(msg);
        }
        public frmData FormData
        {
            set { fData = value; }  // set method
        }
        public double RadiusOfSearch
        {
            set { SearchRadius = value; }  // set method
        }
        public int LimitNumOfStations
        {
            set { LimitStations = value; }  // set method
        }
        private double CalcDistance(double lat1, double lat2, double lon1, double lon2)
        {
            lon1 = lon1 * Math.PI / 180;
            lon2 = lon2 * Math.PI / 180;
            lat1 = lat1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;

            // Haversine formula  
            double dlon = lon2 - lon1;
            double dlat = lat2 - lat1;
            double a = Math.Pow(Math.Sin(dlat / 2), 2) +
                               Math.Cos(lat1) * Math.Cos(lat2) *
                               Math.Pow(Math.Sin(dlon / 2), 2);

            double c = 2 * Math.Asin(Math.Sqrt(a));

            // Radius of earth in kilometers. Use 3956 for miles, 6371 for km
            double r = 3956;

            // calculate the result 
            return (c * r);
        }

        #region "debug routines"
        private void DebugNearbyStationData(SortedDictionary<DateTime, List<string>> dictOtherSta)
        {
            int j = 0;
            foreach (KeyValuePair<DateTime, List<string>> kv in dictOtherSta)
            {
                string st = string.Empty;
                List<string> svals = kv.Value;
                for (int i = 0; i < svals.Count - 1; i++)
                    st += svals[i] + ",";
                st += svals[svals.Count - 1];
                //if (j < 10)
                Debug.WriteLine("{0},{1}", kv.Key.ToString(), st);
                j++;
            }
        }
        private void DebugGageList()
        {
            Debug.WriteLine("Entering debugging for nearby gages ...");
            //foreach (string s in lstMissVars)
            //    Debug.WriteLine("Missing records in var =" + s);

            Debug.WriteLine("Var Count = " + dictNearbyGage.Keys.ToList().Count.ToString());
            foreach (string s in dictNearbyGage.Keys.ToList())
            {
                Debug.WriteLine("key=" + s);
                SortedDictionary<int, clsStation> dsngage = new SortedDictionary<int, clsStation>();
                dictNearbyGage.TryGetValue(s, out dsngage);
                foreach (int dsn in dsngage.Keys.ToList())
                {
                    clsStation csta = new clsStation();
                    if (dsngage.TryGetValue(dsn, out csta))
                    {
                        Debug.WriteLine("Gages: {0},{1},{2},{3},{4}", s, dsn.ToString(), csta.Station,
                            csta.BegDate.ToString(), csta.EndDate.ToString());
                    }
                }
            }
        }
        #endregion
    }
}
