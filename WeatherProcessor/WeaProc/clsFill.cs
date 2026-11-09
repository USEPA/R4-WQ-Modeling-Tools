using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using wdmuploader;

namespace NCEIData
{
    class clsFill
    {
        private frmMain fMain;
        private string MISS = "9999";
        private TimeSpan td;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        private Dictionary<string, string> dictMapVars = new Dictionary<string, string>();
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictData;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> dictHMoments;
        private List<string> lstSelectedVars = new List<string>();
        private int optDataset;
        private Random rand;
        private string tmpWDM;


        public clsFill(frmMain _fMain, string site,
            SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> _dictData,
            SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> _dictMiss,
            SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> _dictModel,
            SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> _dictHMoments)
        {

            fMain = _fMain;
            this.dictMiss = _dictMiss;
            this.dictData = _dictData;
            this.dictModel = _dictModel;
            this.dictHMoments = _dictHMoments;
            this.optDataset = fMain.optDataSource;
            this.tmpWDM = fMain.cacheWDM;
        }

        public Dictionary<string, SortedDictionary<DateTime, string>> FillMissingDataByModel(string site)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DateTime dtbeg = DateTime.Now;
                fMain.WriteLogFile("Begin Estimating data for station: " + site + " ... " +
                    DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToLongTimeString());
                fMain.appManager.UpdateProgress("Estimating missing data for station: " + site + " ... ");
                dtbeg = DateTime.Now;

                //temporary dictionaries of missing, stats for a site and filled data
                Dictionary<string, SortedDictionary<DateTime, string>> dicMissing = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicFill = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<string, double>> dicSiteModel = new Dictionary<string, SortedDictionary<string, double>>();

                //get dictionary of missing and stats for the site
                dictModel.TryGetValue(site, out dicSiteModel);
                dictMiss.TryGetValue(site, out dicMissing);

                //loop for all variables
                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dicMissing)
                {
                    string svar = kv.Key;
                    Debug.WriteLine("Variable = " + svar);
                    SortedDictionary<DateTime, string> varmiss;
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
                    //debug
                    //Debug.WriteLine("Site: {0}, Variable: {1}", site,svar);
                    //foreach (var item in dicVarFill)
                    //{
                    //    Debug.WriteLine("{0},{1}", item.Key.ToString(), item.Value.ToString());
                    //}
                    dicVarFill = null;
                }

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End Estimating Hourly Data for station " + site + ": " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                fMain.appManager.UpdateProgress("Ready ...");

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
        public SortedDictionary<DateTime, string> FillMissingHourlyDataByWeaModel(string site, string svar, SortedDictionary<DateTime, string> varmiss)
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
                SortedDictionary<DateTime, string> siteVar;     //series for variable
                siteVar = GetSeriesFromWDM(site, svar);

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
                    siteVar.Remove(curdt);
                    siteVar.Add(curdt, misdat);

                    dicVarFill.Add(curdt, misdat);
                }
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
        public SortedDictionary<DateTime, string> FillMissingDailyDataByWeaModel(string site, string svar, SortedDictionary<DateTime, string> varmiss)
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

                dictData.TryGetValue(site, out dicSite);            //site data from global dictionary
                dictHMoments.TryGetValue(site, out dicSiteMoments); //site moments from global dictionary

                //dicVarFill is dicMissing with estimated data for given variable
                SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();
                SortedDictionary<DateTime, string> siteVar;   //series for variable
                dicSite.TryGetValue(svar, out siteVar);       //get variable series for site

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
                siteRain = GetSeriesFromWDM(site, svar);

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
                }

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
                dictData.TryGetValue(site, out dicSite);     //site data from global dictionary
                dicSite.TryGetValue(svar, out siteRain);     //get rainfall series for site

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
                }

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
        private SortedDictionary<DateTime, string> GetSeriesFromWDM(string sta, string svar)
        {
            try
            {
                SortedDictionary<DateTime, string> dictVarSeries;

                clsWdm cTmpWDM = new clsWdm(tmpWDM);
                dictVarSeries = cTmpWDM.ReadWeatherSeries(sta, svar);
                cTmpWDM = null;
                return dictVarSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from wdm file!", ex);
                return null;
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
        public Dictionary<string, SortedDictionary<DateTime, string>> FillMissingDataByOtherSite(string site, string OtherSite)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DateTime dtbeg = DateTime.Now;
                fMain.WriteLogFile("Begin Estimating Missing Data for station: " + site + " using " + OtherSite + " ... " +
                    DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToLongTimeString());
                fMain.appManager.UpdateProgress("Estimating Missing Data for station: " + site + " ... ");
                dtbeg = DateTime.Now;

                //temporary dictionaries of missing, stats for a site and filled data
                Dictionary<string, SortedDictionary<DateTime, string>> dicMissing = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicOther = new Dictionary<string, SortedDictionary<DateTime, string>>();
                Dictionary<string, SortedDictionary<DateTime, string>> dicFill = new Dictionary<string, SortedDictionary<DateTime, string>>();

                //get dictionary of missing for current site and data for other site
                dictMiss.TryGetValue(site, out dicMissing);
                dictData.TryGetValue(OtherSite, out dicOther);

                foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dicMissing)
                {
                    string svar = kv.Key;
                    SortedDictionary<DateTime, string> varmiss;
                    SortedDictionary<DateTime, string> varOther;
                    dicOther.TryGetValue(svar, out varOther);
                    dicMissing.TryGetValue(svar, out varmiss);
                    //dicVarFill is dicMissing with estimated data for given variable
                    SortedDictionary<DateTime, string> dicVarFill = new SortedDictionary<DateTime, string>();

                    string Otherdat;
                    //loop for all data in variable series
                    foreach (KeyValuePair<DateTime, string> kv1 in varmiss)
                    {
                        DateTime curdt = kv1.Key;
                        DateTime dt = curdt.Date;
                        //DateTime modt = (new DateTime(dt.Year, dt.Month, 1)).Date;

                        string dat = kv1.Value;
                        varOther.TryGetValue(dt, out Otherdat);

                        if (dat.Contains(MISS))
                        {
                            //Debug.WriteLine(curdt.ToString() + "," + item + "," + j.ToString() + "," + lst_dstats[j].ToString());
                            if (!Otherdat.Contains(MISS)) //not missing, use current day stats
                                dat = Otherdat;
                            else
                                dat = MISS;
                        }
                        dicVarFill.Add(curdt, dat);
                    }
                    dicFill.Add(svar, dicVarFill);
                    dicVarFill = null;
                }
                dicMissing = null;
                dicOther = null;

                td = DateTime.Now - dtbeg;
                fMain.WriteLogFile("End Estimating Missing Data for station: " + site + " using " + OtherSite + " ... " +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                    td.TotalMinutes.ToString("F4") + " minutes.");
                fMain.appManager.UpdateProgress("Ready ...");

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
        private void WriteStatus(string msg)
        {
            fMain.statuslbl.Text = msg;
            fMain.statusStrip.Refresh();
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
