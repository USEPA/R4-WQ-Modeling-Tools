// Version 2.01
// Revision History GPF
// 11/16/20  - turn off error message for linearfit
//-added test for null varmodel, hmoments

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WeaModel
{
    public class LinearAR
    {
        private string site, svar;
        private SortedDictionary<DateTime, string> dseries;
        //nfit -number or harmonics to fit, nhar-max to estimate, half day
        //ndiv -factor of total series count for validation

        private int nhar = 12, nhfit = 2, ndiv = 2, nhperiod = 24;
        private string MISS = "9999";
        private string logfile = string.Empty;
        private List<int> ndays = new List<int>() { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private enum Interval { Hourly, Daily };
        private enum ModelType { Seasonal, Difference };
        private SortedDictionary<Int32, List<double>> moments = new SortedDictionary<Int32, List<double>>();
        private SortedDictionary<string, double> model = new SortedDictionary<string, double>();
        private SortedDictionary<int, int> dictIndex = new SortedDictionary<int, int>();
        private SortedDictionary<Int32, double> hresid = new SortedDictionary<Int32, double>();
        private SortedDictionary<DateTime, double> dresid = new SortedDictionary<DateTime, double>();
        private SortedDictionary<Int32, List<double>> harmoments = new SortedDictionary<Int32, List<double>>();
        private List<string> series = new List<string>() { "MEAN", "STDV" };
        private StreamWriter wri, wrs;
        private double[] nharhly = new double[13];
        private double[] pAvg = new double[8761];
        private double[] pStd = new double[8761];
        private int nlag = 48;
        private bool diff = false;
        private int IModelType = (int)ModelType.Seasonal;
        private string Crlf = Environment.NewLine;
        private Random rand;

        public LinearAR(string _site, string _svar,
                       SortedDictionary<DateTime, string> _series, StreamWriter _wri)
        {
            this.dseries = _series;
            this.site = _site;
            this.svar = _svar;
            this.wri = _wri;
        }

        public void FitLinearModel(int timestep)
        {
            switch (timestep)
            {
                case (int)Interval.Hourly:
                    nhar = 12; nhfit = 6; ndiv = 2; nhperiod = 24;

                    switch (IModelType)
                    {
                        case (int)ModelType.Seasonal:
                            nharhly[0] = 1;
                            for (int i = 1; i <= 12; i++) nharhly[i] = 365 * i;
                            if (!((moments = CalculateSeasonalHourlyMoments(dseries)) == null))
                            {
                                if (!((model = CalculateSeasonalHourlyHarmonics()) == null))
                                {
                                    //if (!((harmoments = GeneratePeriodicHourlyMoments()) == null))
                                    if (!((harmoments = GeneratePeriodicHourlyMoments()) == null))
                                    {
                                        if (StandardizeSeasonalHourly())
                                            AutoCorrelationHourly();
                                        CrossValidateHourlyModel(dseries);
                                    }
                                }
                            }
                            break;
                        //differencing option disable, takes too long
                        case (int)ModelType.Difference:
                            DifferenceSeries(dseries);
                            break;
                    }
                    break;

                case (int)Interval.Daily:
                    nhar = 182; nhfit = 90; ndiv = 2; nhperiod = 366;
                    moments = CalculateSeasonalDailyMoments(dseries);
                    if (!(moments == null))
                    {
                        model = CalculateSeasonalDailyHarmonics();
                        if (!((harmoments = GeneratePeriodicDailyMoments()) == null))
                        {
                            if (StandardizeSeasonalDaily())
                                AutoCorrelationDaily();
                        }
                        CrossValidateDailyModel(dseries);
                    }
                    break;
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
        private bool DifferenceSeries(SortedDictionary<DateTime, string> tseries)
        {
            try
            {
                string curval, preval;
                double sdiff = 0.0, theta = 0.0;
                List<double> diffSeries = new List<double>();
                double[] serial = new double[nlag];

                diffSeries.Add(0.0); //first record
                for (int i = 1; i < tseries.Count; i++)
                {
                    curval = tseries.Values.ElementAt(i);
                    preval = tseries.Values.ElementAt(i - 1);

                    if (!curval.Contains(MISS) && !preval.Contains(MISS))
                    {
                        sdiff = Convert.ToDouble(curval) - Convert.ToDouble(preval);
                        diffSeries.Add(sdiff);
                    }
                    else
                        diffSeries.Add(0.0);

                }

                double[] dser = diffSeries.ToArray();
                double avg = Statistics.Mean(dser);
                double std = Statistics.Variance(dser);
                wri.WriteLine("Means and Stand Deviation for difference series for {0}:{1}", site, svar);
                wri.WriteLine("Difference Series Mean: {0}, Variance: {1}", avg.ToString("F4"), (std * std).ToString("F4"));
                wri.WriteLine("Serial Correlation of residuals");

                //calculate autocorrelation
                serial = Correlation.Auto(dser, 1, nlag);

                //use mathnumerics routine
                theta = serial[0];
                for (int i = 0; i < nlag / 4; i++)
                {
                    int j = i + 1;
                    wri.WriteLine("{0},{1}", j.ToString(), serial[i].ToString("F4"));
                }

                //estimate sigma of error
                List<double> xerr = new List<double>();
                for (int k = 1; k < dser.Count(); k++)
                    xerr.Add(dser[k] - theta * dser[k - 1]);
                double avgerr = Statistics.Mean(xerr);
                double varerr = Statistics.Variance(xerr);

                //save
                wri.WriteLine("AR1 Model : Theta={0}, ErrorVar={1}", theta.ToString("F4"), varerr.ToString("F4"));
                model.Add("varerr", varerr);
                model.Add("ar1coeff", theta);
                model.Add("avgerr", avgerr);

                dser = null;
                diffSeries = null;
                xerr = null;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating serial correlation of difference hourly series!", ex);
                return false;
            }
        }

        #region "Validation"
        private bool CrossValidateDailyModel(SortedDictionary<DateTime, string> tseries)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                //use a random sample of 1/n of series lenght
                int nrecs = tseries.Count;
                int nsample = nrecs / ndiv;

                string msg = "Validating model for " + svar + ": " + site +
                        " using " + nsample.ToString() + " random samples.";
                wri.WriteLine(Crlf + msg);

                //initialize random generator
                rand = new Random();

                //get first hour of observation
                DateTime basedt = tseries.Keys.ElementAt(0);
                DateTime dt;
                string sobs;
                double xpre = 0.0, xsim = 0.0, theta = 0.0, rmse = 0.0;
                double avg = 0.0, std = 0.0, sigma = 0.0, sigma2 = 0.0, xobs = 0.0;
                double corr = 0, sper = 0, varobs = 0, varerr = 0, nse = 0;
                List<double> stats = new List<double>();
                List<double> xerr = new List<double>();
                List<double> yobs = new List<double>();
                List<double> ysim = new List<double>();

                //get AR parameter for site var
                model.TryGetValue("dlyvarerr", out sigma2);
                sigma = Math.Sqrt(sigma2);
                model.TryGetValue("dlyar1coeff", out theta);
                double prb = 0.0, err = 0.0;

                for (int isamp = 0; isamp < nsample; isamp++)
                {
                    prb = rand.NextDouble();
                    int idays = Convert.ToInt32(prb * nrecs);
                    dt = basedt.AddDays(idays);

                    if (tseries.TryGetValue(dt, out sobs) && !sobs.Contains(MISS))
                    {
                        xobs = Convert.ToDouble(sobs);
                        //get current harmonic moments
                        int monhr = 100 * dt.Month + dt.Day;
                        harmoments.TryGetValue(monhr, out stats);
                        avg = stats[0]; std = stats[1];

                        //generate noise
                        prb = rand.NextDouble();
                        err = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, sigma, prb);

                        //get standardized residual of previous hour
                        hresid.TryGetValue(idays - 1, out xpre);
                        xsim = theta * xpre + err;
                        xsim = xsim * std + avg;
                        xerr.Add(xsim - xobs);
                        yobs.Add(xobs);
                        ysim.Add(xsim);

                        //wri.WriteLine("{0},{1},{2},{3}", isamp.ToString(), dt.ToString(), 
                        //    xobs.ToString("F4"), xsim.ToString("F4"));
                    }
                } //end for
                avg = Statistics.Mean(xerr);
                std = Statistics.StandardDeviation(xerr);
                rmse = Statistics.RootMeanSquare(xerr);
                corr = Correlation.Pearson(yobs, ysim);
                sper = Correlation.Spearman(yobs, ysim);
                varobs = Statistics.Variance(yobs);
                varerr = Statistics.Variance(xerr);
                nse = 1.0 - (varerr / varobs);

                wri.WriteLine("Statistics of model fit for " + svar + ": " + site);
                wri.WriteLine("Average Error: " + avg.ToString("F4"));
                wri.WriteLine("Standard Deviation: " + std.ToString("F4"));
                wri.WriteLine("Pearson Correlation: " + corr.ToString("F4"));
                wri.WriteLine("Spearman Correlation: " + sper.ToString("F4"));
                wri.WriteLine("Nash-Sutcliffe Coefficient: " + nse.ToString("F4"));

                //wri.WriteLine("Root Mean Square Error: " + rmse.ToString("F4"));

                xerr = null; yobs = null; ysim = null;
                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error validating model!", ex);
                return false;
            }
        }
        private bool CrossValidateHourlyModel(SortedDictionary<DateTime, string> tseries)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                //use a random sample of 1/n of series lenght
                int nrecs = tseries.Count;
                int nsample = nrecs / ndiv;

                string msg = "Validating model for " + svar + ": " + site +
                        " using " + nsample.ToString() + " random samples.";
                wri.WriteLine(Crlf + msg);

                //initialize random generator
                rand = new Random();

                //get first hour of observation
                DateTime basedt = tseries.Keys.ElementAt(0);
                DateTime dt;
                string sobs;
                double xpre = 0.0, xsim = 0.0, theta = 0.0, rmse = 0.0;
                double avg = 0.0, std = 0.0, sigma = 0.0, sigma2 = 0.0, xobs = 0.0;
                List<double> stats = new List<double>();
                List<double> xerr = new List<double>();
                List<double> yobs = new List<double>();
                List<double> ysim = new List<double>();
                double corr = 0, sper = 0, varobs = 0, varerr = 0, nse = 0;

                //get AR parameter for site var
                model.TryGetValue("varerr", out sigma2);
                sigma = Math.Sqrt(sigma2);
                model.TryGetValue("ar1coeff", out theta);
                double prb = 0.0, err = 0.0;

                for (int isamp = 0; isamp < nsample; isamp++)
                {
                    prb = rand.NextDouble();
                    int ihrs = Convert.ToInt32(prb * nrecs);
                    dt = basedt.AddHours(ihrs);

                    if (tseries.TryGetValue(dt, out sobs) && !sobs.Contains(MISS))
                    {
                        xobs = Convert.ToDouble(sobs);
                        //get current harmonic moments
                        int monhr = 100 * dt.Month + dt.Hour;
                        harmoments.TryGetValue(monhr, out stats);
                        avg = stats[0]; std = stats[1];

                        //generate noise
                        prb = rand.NextDouble();
                        err = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, sigma, prb);

                        //get standardized residual of previous hour
                        hresid.TryGetValue(ihrs - 1, out xpre);
                        xsim = theta * xpre + err;
                        xsim = xsim * std + avg;

                        if (svar.Contains("WIND"))
                        {
                            if (xsim < 0) xsim = 0.0;
                        }
                        else if (svar.Contains("WNDD"))
                        {
                            if (xsim < 10) xsim = 10.0;
                            if (xsim > 360) xsim = 360.0;
                        }
                        else if (svar.Contains("CLOU"))
                        {
                            if (xsim < 0) xsim = 0.0;
                            else if (xsim > 10) xsim = 10.0;
                            xsim = ReClassCloud(xsim);
                        }

                        yobs.Add(xobs);
                        ysim.Add(xsim);
                        xerr.Add(xsim - xobs);

                        //wri.WriteLine("{0},{1},{2},{3}", isamp.ToString(), dt.ToString(), 
                        //    xobs.ToString("F4"), xsim.ToString("F4"));
                    }
                } //end for
                avg = Statistics.Mean(xerr);
                std = Statistics.StandardDeviation(xerr);
                rmse = Statistics.RootMeanSquare(xerr);
                corr = Correlation.Pearson(yobs, ysim);
                sper = Correlation.Spearman(yobs, ysim);
                varobs = Statistics.Variance(yobs);
                varerr = Statistics.Variance(xerr);
                nse = 1.0 - (varerr / varobs);

                wri.WriteLine("Statistics of model fit for " + svar + ": " + site);
                wri.WriteLine("Average Error: " + avg.ToString("F4"));
                wri.WriteLine("Standard Deviation: " + std.ToString("F4"));
                wri.WriteLine("Pearson Correlation: " + corr.ToString("F4"));
                wri.WriteLine("Spearman Correlation: " + sper.ToString("F4"));
                wri.WriteLine("Nash-Sutcliffe Coefficient: " + nse.ToString("F4"));
                //wri.WriteLine("Root Mean Square Error: " + rmse.ToString("F4"));

                xerr = null; yobs = null; ysim = null;
                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error validating model!", ex);
                return false;
            }
        }

        #endregion

        #region "Hourly Moments and harmonics"
        private SortedDictionary<Int32, List<double>> CalculateSeasonalHourlyMoments(SortedDictionary<DateTime, string> tseries)
        {
            Debug.WriteLine("Calculating Seasonal Means and Stand Deviations for {0}:{1}", site, svar);
            wri.WriteLine(Crlf + "Seasonal Means and Stand Deviations for {0}:{1}", site, svar);
            SortedDictionary<Int32, List<double>> calc = new SortedDictionary<Int32, List<double>>();
            SortedDictionary<Int32, List<double>> mom = new SortedDictionary<Int32, List<double>>();
            int mon, hr, monhr;

            try
            {
                List<double> lstcalc = new List<double>();
                foreach (var item in tseries)
                {
                    if (!item.Value.Contains(MISS))
                    {
                        double val = Convert.ToDouble(item.Value);

                        hr = item.Key.Hour;
                        mon = item.Key.Month;
                        monhr = mon * 100 + hr;

                        if (calc.ContainsKey(monhr))
                        {
                            calc.TryGetValue(monhr, out lstcalc);
                            lstcalc.Add(val);
                        }
                        else
                        {
                            lstcalc = new List<double>();
                            lstcalc.Add(val);
                            calc.Add(monhr, lstcalc);
                        }
                    }
                }

                //calculate mean and sd for each day-hr
                int ix = 0;
                double avg = 0, std = 0;
                foreach (var kv in calc)
                {
                    List<double> lstdat = new List<double>();
                    calc.TryGetValue(kv.Key, out lstdat);
                    List<double> lst = new List<double>();
                    ix++;

                    if (lstdat.Count > 0)
                    {
                        avg = Statistics.Mean(lstdat);
                        if (lstdat.Count >= 2)
                            std = Statistics.StandardDeviation(lstdat);
                        else
                            std = Statistics.PopulationStandardDeviation(lstdat);
                        lst.Add(avg); lst.Add(std);
                    }
                    else
                    {
                        Debug.WriteLine("Cannot calculate Means and Stand Deviations for {0}:{1} for {2}", site, svar, kv.Key.ToString());
                        wri.WriteLine(Crlf + "Cannot calculate Means and Stand Deviations for {0}:{1} for {2}", site, svar, kv.Key.ToString());
                        lst.Add(Convert.ToSingle(MISS)); lst.Add(Convert.ToSingle(MISS));
                    }
                    mom.Add(kv.Key, lst);
                    //Debug.WriteLine("{0},{1},{2}", kv.Key.ToString(), lst[0].ToString(), lst[1].ToString());
                    lst = null;
                    lstdat = null;
                }
                calc = null;

                //complete series, by adding missing stats
                for (int imo = 1; imo <= 12; imo++)
                {
                    for (int ihr = 0; ihr <= 23; ihr++)
                    {
                        List<double> lst = new List<double>();
                        int imohr = imo * 100 + ihr;
                        if (!mom.TryGetValue(imohr, out lst))
                        {
                            List<double> lstnew = new List<double>();
                            lstnew.Add(Convert.ToSingle(MISS)); lstnew.Add(Convert.ToSingle(MISS));
                            mom.Add(imohr, lstnew);
                            lstnew = null;
                        }
                        mom.TryGetValue(imohr, out lst);
                        //Debug.WriteLine("{0},{1},{2}", imohr.ToString(), lst[0].ToString(), lst[1].ToString());
                        lst = null;
                    }
                }

                //fill in missing stats
                List<double> lstats = new List<double>();
                avg = 0.0; std = 0.0;
                int i = 0;
                while (i < mom.Keys.Count)
                {
                    lstats = mom.Values.ElementAt(i);
                    avg = lstats[0];
                    if (avg > 9990) //missing
                    {
                        i = EstimateMissingStat(i, avg, mom);
                    }
                    else
                        i++;
                }
                //debug
                //foreach(var kv in mom)
                //    Debug.WriteLine("{0},{1},{2}", kv.Key.ToString(), 
                //        kv.Value[0].ToString(), kv.Value[1].ToString());

                return mom;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating Seasonal Hourly Moments!", ex);
                return null;
            }
        }

        private int EstimateMissingStat(int i, double savg, SortedDictionary<int, List<double>> mom)
        {
            int j = i;
            int j1 = i - 1; int j2 = 0;
            List<double> lstats;
            double avg = savg;
            while (avg > 9990)
            {
                j++;
                lstats = mom.Values.ElementAt(j);
                avg = lstats[0];
            }
            j2 = j;
            // interpolate
            int inter = (j2 - j1) - 1;
            double delta = (mom.Values.ElementAt(j2)[0] - mom.Values.ElementAt(j1)[0]) / (j2 - j1);
            for (int k = 1; k <= inter; k++)
                mom.Values.ElementAt(j1 + k)[0] = mom.Values.ElementAt(j1)[0] + delta * k;

            delta = (mom.Values.ElementAt(j2)[1] - mom.Values.ElementAt(j1)[1]) / (j2 - j1);
            for (int k = 1; k <= inter; k++)
                mom.Values.ElementAt(j1 + k)[1] = mom.Values.ElementAt(j1)[1] + delta * k;

            return j2 + 1;
        }

        /// <summary>
        /// Fits harmonics to hourly means and sd
        /// Period is 24, harmonic fit is 12
        /// </summary>
        /// <returns></returns>
        private SortedDictionary<string, double> CalculateSeasonalHourlyHarmonics()
        {
            //routine fits harmonics to the hourly means and std by month
            try
            {
                SortedDictionary<string, double> mdl = new SortedDictionary<string, double>();

                //nhar is 12, half of daily period of 24
                int nh = nhar;
                List<List<double>> lstParms;
                for (int imo = 1; imo <= 12; imo++)
                {
                    lstParms = new List<List<double>>();
                    //get monthly data
                    int prejmohr = 0;
                    for (int jhr = 0; jhr < 24; jhr++)
                    {
                        List<double> parms = new List<double>();
                        int jmohr = imo * 100 + jhr;
                        if (!(moments.TryGetValue(jmohr, out parms)))
                        {
                            //check if missing
                            //if (Convert.ToDouble(parms[0]) < -9990) //11-20-20 error here
                            if (jhr == 0)
                            {
                                if (imo == 1)
                                    prejmohr = 12 * 100 + 23;
                                else
                                    prejmohr = (imo - 1) * 100 + 23;
                            }
                            else //jhr > 0
                                prejmohr = imo * 100 + (jhr - 1);

                            moments.TryGetValue(prejmohr, out parms);
                        }
                        lstParms.Add(parms);
                        parms = null;
                    }

                    int ncnt = lstParms.Count;
                    //kser=0 mean, kser=1 stdev
                    for (int kser = 0; kser < 2; kser++)
                    {
                        int ncntser = 0; double sval;
                        double[] stats = new double[ncnt];
                        double sumx = 0, sumxx = 0, xavg, xsd, xvar;
                        for (int i = 0; i < ncnt; i++)
                        {
                            sval = lstParms[i].ElementAt(kser);
                            if (sval < Convert.ToSingle(MISS))
                            {
                                stats[i] = sval;
                                sumx += stats[i];
                                sumxx += stats[i] * stats[i];
                                ncntser += 1;
                            }
                        }
                        xavg = sumx / ncntser;
                        xvar = (sumxx - (1.0 / ncntser) * sumx * sumx) / (ncntser - 1.0);
                        xsd = Math.Sqrt(xvar);

                        mdl.Add(series[kser] + ":mean_" + imo, xavg);
                        mdl.Add(series[kser] + ":stdev_" + imo, xsd);

                        wri.WriteLine(series[kser] + "{0}: Avg={1}, Stdev={2}", imo.ToString("00"),
                            xavg.ToString("F3"), xsd.ToString("F3"));

                        double[] suma = new double[nh + 1];
                        double[] sumb = new double[nh + 1];
                        double[] acoef = new double[nh + 1];
                        double[] bcoef = new double[nh + 1];
                        double[] ph = new double[nh + 1];
                        double phsum = 0;

                        //calculate harmonics, nh is 12 for hourly
                        wri.WriteLine("Harmonic, ACoef, BCoef, %Var, %SumVar");
                        for (int j = 0; j < nh; j++)
                        {
                            suma[j] = 0.0; sumb[j] = 0.0; acoef[j] = 0.0; bcoef[j] = 0; ph[j] = 0;
                            double icnt = 0;
                            int ihar = j + 1;
                            ncntser = 0;

                            for (int kval = 0; kval < ncnt; kval++)
                            {
                                icnt += 1.0;
                                double val = stats[kval];
                                if (val < Convert.ToSingle(MISS))
                                {
                                    suma[j] += val * Math.Cos((2.0 * Math.PI * ihar * icnt) / ncnt);
                                    sumb[j] += val * Math.Sin((2.0 * Math.PI * ihar * icnt) / ncnt);
                                }
                            }
                            acoef[j] = (2.0 * suma[j] / ncnt);
                            bcoef[j] = (2.0 * sumb[j] / ncnt);
                            ph[j] = 100.0 * (acoef[j] * acoef[j] + bcoef[j] * bcoef[j]) / (2 * xvar);
                            phsum += ph[j];

                            //writeout log only half of the harmonics
                            if (ihar <= 2)
                                wri.WriteLine("{0}, {1}, {2},{3}, {4}", ihar.ToString(),
                                  acoef[j].ToString("F4"), bcoef[j].ToString("F4"),
                                  ph[j].ToString("F3"), phsum.ToString("F3"));

                            //add to dict if ihar<=6, only save 6 harmonics, 1/4 of period of 24
                            if (ihar <= nh / 2)
                            {
                                mdl.Add(series[kser] + ":acoef" + ihar.ToString() + "_" + imo, acoef[j]);
                                mdl.Add(series[kser] + ":bcoef" + ihar.ToString() + "_" + imo, bcoef[j]);
                            }
                        }
                        stats = null;
                    }
                    lstParms = null;
                }

                //debug
                //for (int i = 0; i < mdl.Count; i++)
                //    Debug.WriteLine("{0},{1}", mdl.Keys.ElementAt(i), mdl.Values.ElementAt(i));
                return mdl;
            }
            catch (Exception ex)
            {
                //ShowError("Error calculating periodicities in hourly mean and variance!", ex);
                return null;
            }
        }

        /// <summary>
        /// Generates the periodic means and sd from fitted harmnic coefficients
        /// Period is 24, harmonic fit is 12
        /// </summary>
        /// <returns></returns>
        private SortedDictionary<Int32, List<double>> GeneratePeriodicHourlyMoments()
        {
            //routine generates the periodic hourly moments
            Debug.WriteLine("Entering GeneratePeriodicMoments()");
            try
            {
                //generate monthly by hour harmonics of mean and stdev, routine OK
                SortedDictionary<Int32, List<double>> dicthar = new SortedDictionary<Int32, List<double>>();
                List<double> phAvg = new List<double>();
                List<double> phStd = new List<double>();

                int nperiod = 24, nh = nhfit; //nhfit=6
                string kv;

                for (int kser = 0; kser < 2; kser++)
                {
                    for (int jmo = 1; jmo <= 12; jmo++)
                    {
                        double[] acoef = new double[nh];
                        double[] bcoef = new double[nh];
                        double mean = 0.0;

                        //get harmonic coeffients from model dictionary
                        //format of key acoef<hh>_<mon>, hh-harmonic,mon=month
                        //uses only 2 harmonics for mean & sd
                        for (int j = 0; j < nh; j++)
                        {
                            int ihar = j + 1;
                            kv = series[kser] + ":acoef" + ihar + "_" + jmo;
                            model.TryGetValue(kv, out acoef[j]);
                            kv = series[kser] + ":bcoef" + ihar + "_" + jmo;
                            model.TryGetValue(kv, out bcoef[j]);
                            //Debug.WriteLine("{0},{1},{2}", series[kser], acoef[j].ToString(), bcoef[j].ToString());
                        }
                        kv = series[kser] + ":mean_" + jmo;
                        model.TryGetValue(kv, out mean);

                        for (int t = 1; t <= nperiod; t++)
                        {
                            double sum = 0.0, radian = 0.0;
                            for (int j = 0; j < nh; j++)
                            {
                                int ihar = j + 1;
                                radian = (2.0 * Math.PI * ihar * t) / nperiod;
                                sum += acoef[j] * Math.Cos(radian) + bcoef[j] * Math.Sin(radian);
                            }
                            if (kser == 0)
                                phAvg.Add(mean + sum);
                            else
                                phStd.Add(mean + sum);
                        }
                        acoef = null; bcoef = null;
                    }//loop for month
                }
                //end loop series
                //wri.WriteLine("Mean and SD harmonic series for "+site+":"+svar);
                //wri.WriteLine("Month-Hr, Mean, Std");
                int idx = -1;
                List<double> lst;
                for (int i = 1; i <= 12; i++)
                {
                    for (int j = 0; j < 24; j++)
                    {
                        idx++;
                        int hrmo = i * 100 + j;
                        //wri.WriteLine("{0},{1},{2}", hrmo.ToString(),
                        //        phAvg[idx].ToString("F4"), phStd[idx].ToString("F4"));
                        lst = new List<double>();
                        lst.Add(phAvg[idx]); lst.Add(phStd[idx]);
                        dicthar.Add(hrmo, lst);
                        lst = null;
                    }
                }
                return dicthar;
            }
            catch (Exception ex)
            {
                //ShowError("Error generating periodic hourly means and variances!", ex);
                return null;
            }
        }
        private bool StandardizeSeasonalHourly()
        {
            Debug.WriteLine("Entering standardized series routine");
            try
            {
                int day, hr, mon, mohr;
                int ix = 0;
                double xavg = 0.0, xstd = 0.0, val = 0.0;
                string sval;
                List<double> stats = new List<double>();
                //hresid is int32,double
                hresid.Clear();
                //dresid.Clear();

                foreach (KeyValuePair<DateTime, string> kvp in dseries)
                {
                    ix++;
                    DateTime dt = kvp.Key;
                    sval = kvp.Value;
                    day = dt.Day; mon = dt.Month; hr = dt.Hour;

                    //get periodic moments for month-hour
                    mohr = 100 * mon + hr;
                    harmoments.TryGetValue(mohr, out stats);
                    xavg = stats[0]; xstd = stats[1];

                    if (!sval.Contains(MISS))
                        val = (Convert.ToDouble(sval) - xavg) / xstd;
                    else
                        val = 0;
                    hresid.Add(ix, val);
                    //wri.WriteLine("{0},{1},{2}", dt.ToString(), ix.ToString(), val.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating standardized hourly series!", ex);
                return false;
            }
        }
        private bool AutoCorrelationHourly()
        {
            Debug.WriteLine("Entering Autocorrelation routine");
            double theta = 0.0, sigma = 0;
            try
            {
                int ncnt = hresid.Count;
                double[] xresid = hresid.Values.ToArray();
                double mean = Statistics.Mean(xresid);
                double stdev = Statistics.StandardDeviation(xresid);

                wri.WriteLine("Standardized Series Mean: {0}, Variance: {1}", mean.ToString("F4"), (stdev * stdev).ToString("F4"));
                wri.WriteLine("Serial Correlation of residuals");

                double[] serial = new double[nlag];
                double[] pacf = new double[nlag];
                if (diff)
                {
                    wrs.WriteLine("Difference series: j, standX, difStandX");
                    List<double> xdiff = new List<double>();
                    for (int j = 1; j < ncnt; j++)
                    {
                        double dif = xresid[j] - xresid[j - 1];
                        xdiff.Add(dif);
                        wrs.WriteLine("{0},{1},{2}", j.ToString(), xresid[j].ToString("F5"), dif.ToString("F5"));
                    }
                    xresid = xdiff.ToArray();
                    serial = Correlation.Auto(xresid, 1, nlag);
                    xdiff = null;
                    ncnt = xresid.Count();
                }
                else
                {
                    serial = Correlation.Auto(xresid, 1, nlag);
                }

                //pacf = PartialAutoCorrelationHourly(serial,nlag);

                //use mathnumerics routine
                theta = serial[0];
                for (int i = 0; i < 6; i++)
                {
                    int j = i + 1;
                    wri.WriteLine("{0},{1}", j.ToString(), serial[i].ToString("F4"));
                }

                //estimate sigma of error
                List<double> xerr = new List<double>();
                for (int k = 1; k < ncnt - 1; k++)
                    xerr.Add(xresid[k] - theta * xresid[k - 1]);
                double avgerr = Statistics.Mean(xerr);
                double varerr = Statistics.Variance(xerr);

                //save
                wri.WriteLine("AR1 Model : Theta={0}, ErrorVar={1}", theta.ToString("F4"), varerr.ToString("F4"));
                model.Add("varerr", varerr);
                model.Add("ar1coeff", theta);
                model.Add("avgerr", avgerr);

                xresid = null;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating serial correlation of standardized hourly series!", ex);
                return false;
            }

        }
        private double[] PartialAutoCorrelationHourly(double[] serial, int nlag)
        {
            double[] partial = new double[nlag];

            for (int ilag = 2; ilag < nlag; ilag++)
            {
                Matrix<double> numerM = Matrix<double>.Build.Random(2, 2);
                Matrix<double> denomM = Matrix<double>.Build.Random(2, 2);
                for (int j = 0; j < ilag; j++)
                {
                    for (int k = 0; k < ilag; k++)
                    {
                        if (j == k)
                            numerM[j, k] = 1.0;
                        else
                            numerM[j, k] = serial[j];
                    }
                }
            }
            return partial;
        }
        public SortedDictionary<string, double> ARmodel()
        {
            return model;
        }
        public SortedDictionary<Int32, List<double>> HarmonicMoments()
        {
            return harmoments;
        }
        #endregion

        #region "Daily Moments and Harmonics"
        private SortedDictionary<Int32, List<double>> CalculateSeasonalDailyMoments(SortedDictionary<DateTime, string> tseries)
        {
            wri.WriteLine("Seasonal Means and Stand Deviations for {0}:{1}", site, svar);

            SortedDictionary<Int32, List<double>> calc = new SortedDictionary<Int32, List<double>>();
            SortedDictionary<Int32, List<double>> mom = new SortedDictionary<Int32, List<double>>();
            int mon, moday, day;

            List<double> lstcalc;
            foreach (var item in tseries)
            {
                if (!item.Value.Contains(MISS))
                {
                    double val = Convert.ToDouble(item.Value);

                    mon = item.Key.Month;
                    day = item.Key.Day;
                    moday = mon * 100 + day;

                    if (calc.ContainsKey(moday))
                    {
                        calc.TryGetValue(moday, out lstcalc);
                        lstcalc.Add(val);
                    }
                    else
                    {
                        lstcalc = new List<double>();
                        lstcalc.Add(val);
                        calc.Add(moday, lstcalc);
                    }
                }
            }
            //feb 29
            calc.TryGetValue(228, out lstcalc);
            List<double> l229;
            if (calc.TryGetValue(229, out l229))
            {
                foreach (var val in lstcalc)
                    l229.Add(val);
                calc.Remove(229);
                calc.Add(229, l229);
            }
            else
            {
                l229 = new List<double>();
                foreach (var val in lstcalc)
                    l229.Add(val);
                calc.Add(229, l229);
            }
            lstcalc = null;

            try
            {
                //calculate mean and sd for each month-day
                int ix = 0; int prekey = 0;
                double avg, std;
                foreach (var kv in calc)
                {
                    List<double> lstdat = new List<double>();
                    calc.TryGetValue(kv.Key, out lstdat);
                    avg = Statistics.Mean(lstdat);
                    if (lstdat.Count >= 2)
                        std = Statistics.StandardDeviation(lstdat);
                    else
                        std = Statistics.PopulationStandardDeviation(lstdat);

                    ix++;
                    List<double> lst = new List<double>();
                    lst.Add(avg);
                    lst.Add(std);
                    mom.Add(kv.Key, lst);
                    prekey = kv.Key;
                    //debug
                    //Debug.WriteLine("{0},{1},{2},{3}", kv.Key.ToString(), ix.ToString(),
                    //    avg.ToString("F4"), std.ToString("F4"));

                    lst = null;
                    lstdat = null;
                }
                calc = null;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating seasonal daily moments!", ex);
                return null;
            }
            return mom;
        }
        private SortedDictionary<string, double> CalculateSeasonalDailyHarmonics()
        {
            //routine fits harmonics to the daily means and std by month
            try
            {
                SortedDictionary<string, double> mdl = new SortedDictionary<string, double>();

                int nh = nhar;  //nhar is 182, half of daily period of 366
                List<List<double>> lstParms;

                //get the daily means and sd
                lstParms = moments.Values.ToList();
                int ncnt = lstParms.Count;
                //kser=0, mean; kser=1 stdev
                for (int kser = 0; kser < 2; kser++)
                {
                    double[] stats = new double[ncnt];
                    double sumx = 0, sumxx = 0, xavg, xsd, xvar;
                    for (int i = 0; i < ncnt; i++)
                    {
                        stats[i] = lstParms[i].ElementAt(kser);
                        sumx += stats[i];
                        sumxx += stats[i] * stats[i];
                    }
                    xavg = sumx / ncnt;
                    xvar = (sumxx - (1.0 / ncnt) * sumx * sumx) / (ncnt - 1.0);
                    xsd = Math.Sqrt(xvar);
                    mdl.Add(series[kser] + ":dlymean", xavg);
                    mdl.Add(series[kser] + ":dlystdev", xsd);
                    wri.WriteLine(series[kser] + ": Avg={0}, Stdev={1}",
                        xavg.ToString("F3"), xsd.ToString("F3"));

                    double[] suma = new double[nh + 1];
                    double[] sumb = new double[nh + 1];
                    double[] acoef = new double[nh + 1];
                    double[] bcoef = new double[nh + 1];
                    double[] ph = new double[nh + 1];
                    double phsum = 0;

                    //calculate harmonics
                    wri.WriteLine("Harmonic, ACoef, BCoef, %Var, %SumVar");
                    for (int j = 0; j < nh; j++)
                    {
                        suma[j] = 0.0; sumb[j] = 0.0; acoef[j] = 0.0; bcoef[j] = 0; ph[j] = 0;
                        double icnt = 0;
                        int ihar = j + 1;

                        for (int kval = 0; kval < ncnt; kval++)
                        {
                            icnt += 1.0;
                            double val = stats[kval];
                            suma[j] += val * Math.Cos((2.0 * Math.PI * ihar * icnt) / ncnt);
                            sumb[j] += val * Math.Sin((2.0 * Math.PI * ihar * icnt) / ncnt);
                        }
                        acoef[j] = (2.0 * suma[j] / ncnt);
                        bcoef[j] = (2.0 * sumb[j] / ncnt);
                        ph[j] = 100.0 * (acoef[j] * acoef[j] + bcoef[j] * bcoef[j]) / (2 * xvar);
                        phsum += ph[j];

                        //write only half of the harmonics
                        if (ihar <= 2)
                            wri.WriteLine("{0}, {1}, {2},{3}, {4}", ihar.ToString(),
                              acoef[j].ToString("F4"), bcoef[j].ToString("F4"),
                              ph[j].ToString("F3"), phsum.ToString("F3"));

                        //add to dict
                        mdl.Add(series[kser] + ":dlyacoef" + ihar.ToString(), acoef[j]);
                        mdl.Add(series[kser] + ":dlybcoef" + ihar.ToString(), bcoef[j]);
                    }

                    stats = null;
                }
                lstParms = null;

                //debug
                //for (int i = 0; i < mdl.Count; i++)
                //    Debug.WriteLine("{0},{1}", mdl.Keys.ElementAt(i), mdl.Values.ElementAt(i));
                return mdl;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating periodicities in daily mean and variance!", ex);
                return null;
            }
        }
        private SortedDictionary<Int32, List<double>> GeneratePeriodicDailyMoments()
        {
            //routine generates the periodic hourly moments
            Debug.WriteLine("Entering GeneratePeriodicMoments()");
            try
            {
                //generate harmonics of mean and stdev, routine OK
                SortedDictionary<Int32, List<double>> dicthar = new SortedDictionary<Int32, List<double>>();
                List<double> phAvg = new List<double>();
                List<double> phStd = new List<double>();

                int nperiod = nhperiod, nh = nhfit;
                string kv;

                for (int kser = 0; kser < 2; kser++)
                {
                    double[] acoef = new double[nh];
                    double[] bcoef = new double[nh];
                    double mean = 0.0;

                    //get harmonic coeffients from model dictionary
                    //format of key acoef<hh>_<mon>, hh-harmonic,mon=month
                    for (int j = 0; j < nh; j++)
                    {
                        int ihar = j + 1;
                        kv = series[kser] + ":dlyacoef" + ihar;
                        model.TryGetValue(kv, out acoef[j]);
                        kv = series[kser] + ":dlybcoef" + ihar;
                        model.TryGetValue(kv, out bcoef[j]);
                        //Debug.WriteLine("{0},{1},{2}", series[kser], acoef[j].ToString(), bcoef[j].ToString());
                    }
                    kv = series[kser] + ":dlymean";
                    model.TryGetValue(kv, out mean);

                    for (int t = 1; t <= nperiod; t++)
                    {
                        double sum = 0.0, radian = 0.0;
                        for (int j = 0; j < nh; j++)
                        {
                            int ihar = j + 1;
                            radian = (2.0 * Math.PI * ihar * t) / nperiod;
                            sum += acoef[j] * Math.Cos(radian) + bcoef[j] * Math.Sin(radian);
                        }
                        if (kser == 0)
                            phAvg.Add(mean + sum);
                        else
                            phStd.Add(mean + sum);
                    }
                    acoef = null; bcoef = null;
                }
                //end loop series
                //wri.WriteLine("Mean and SD harmonic series for "+site+":"+svar);
                //wri.WriteLine("Mean, Std");
                int idx = -1;
                List<double> lst;
                for (int i = 1; i <= 12; i++)
                {
                    if (i == 2) ndays[i] = 29;
                    for (int j = 1; j <= ndays[i]; j++)
                    {
                        idx++;
                        int daymo = i * 100 + j;
                        //wri.WriteLine("{0},{1},{2}", daymo.ToString(),
                        //        phAvg[idx].ToString("F4"), phStd[idx].ToString("F4"));
                        lst = new List<double>();
                        lst.Add(phAvg[idx]); lst.Add(phStd[idx]);
                        dicthar.Add(daymo, lst);
                        lst = null;
                    }
                }
                return dicthar;
            }
            catch (Exception ex)
            {
                ShowError("Error generating periodic daily means and variances!", ex);
                return null;
            }
        }
        private bool StandardizeSeasonalDaily()
        {
            Debug.WriteLine("Entering standardized series routine");
            try
            {
                int day, hr, mon, moday;
                int ix = 0;
                double xavg = 0.0, xstd = 0.0, val = 0.0;
                string sval;
                List<double> stats = new List<double>();
                //hresid is int32,double
                hresid.Clear();
                //dresid.Clear();

                foreach (KeyValuePair<DateTime, string> kvp in dseries)
                {
                    ix++;
                    DateTime dt = kvp.Key;
                    sval = kvp.Value;
                    day = dt.Day; mon = dt.Month; hr = dt.Hour;

                    //get periodic moments for month-day
                    moday = 100 * mon + day;
                    harmoments.TryGetValue(moday, out stats);
                    xavg = stats[0]; xstd = stats[1];

                    if (!sval.Contains(MISS))
                    {
                        if (xstd > 0)
                            val = (Convert.ToDouble(sval) - xavg) / xstd;
                        else
                            val = (Convert.ToDouble(sval) - xavg);
                    }
                    else
                        val = 0;
                    hresid.Add(ix, val);
                    //wri.WriteLine("{0},{1},{2}", dt.ToString(), ix.ToString(), val.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating standardized hourly series!", ex);
                return false;
            }
        }
        private bool AutoCorrelationDaily()
        {
            Debug.WriteLine("Entering Autocorrelation routine");
            double theta = 0.0, sigma = 0;
            try
            {
                int ncnt = hresid.Count;
                double[] xresid = hresid.Values.ToArray();
                double mean = Statistics.Mean(xresid);
                double stdev = Statistics.StandardDeviation(xresid);

                wri.WriteLine("Standardized Series Mean: {0}, Variance: {1}", mean.ToString("F4"), (stdev * stdev).ToString("F4"));
                wri.WriteLine("Serial Correlation of residuals");

                double[] serial = new double[nlag];
                double[] pacf = new double[nlag];
                if (diff)
                {
                    wrs.WriteLine("Difference series: j, standX, difStandX");
                    List<double> xdiff = new List<double>();
                    for (int j = 1; j < ncnt; j++)
                    {
                        double dif = xresid[j] - xresid[j - 1];
                        xdiff.Add(dif);
                        wrs.WriteLine("{0},{1},{2}", j.ToString(), xresid[j].ToString("F5"), dif.ToString("F5"));
                    }
                    xresid = xdiff.ToArray();
                    serial = Correlation.Auto(xresid, 1, nlag);
                    xdiff = null;
                    ncnt = xresid.Count();
                }
                else
                {
                    serial = Correlation.Auto(xresid, 1, nlag);
                }

                //pacf = PartialAutoCorrelationHourly(serial,nlag);

                //use mathnumerics routine
                theta = serial[0];
                for (int i = 0; i < 6; i++)
                {
                    int j = i + 1;
                    wri.WriteLine("{0},{1}", j.ToString(), serial[i].ToString("F4"));
                }

                //estimate sigma of error
                List<double> xerr = new List<double>();
                for (int k = 1; k < ncnt - 1; k++)
                    xerr.Add(xresid[k] - theta * xresid[k - 1]);
                double avgerr = Statistics.Mean(xerr);
                double varerr = Statistics.Variance(xerr);

                //save
                wri.WriteLine("AR1 Model : Theta={0}, ErrorVar={1}", theta.ToString("F4"), varerr.ToString("F4"));
                model.Add("dlyvarerr", varerr);
                model.Add("dlyar1coeff", theta);
                model.Add("dlyavgerr", avgerr);

                xresid = null;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating serial correlation of standardized daily series!", ex);
                return false;
            }

        }

        #endregion

        private void ShowError(string msg, Exception ex)
        {
            msg += Crlf + Crlf + ex.Message + Crlf + Crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        #region "Not used"
        private SortedDictionary<Int32, List<double>> CalculateHourlyMoments(SortedDictionary<DateTime, string> tseries)
        {
            //fMain.WriteLogFile("Calculating hourly mean and variance for " + svar + ": " + site);
            wri.WriteLine("Moments: {0},{1}", site, svar);

            SortedDictionary<Int32, List<double>> calc = new SortedDictionary<Int32, List<double>>();
            SortedDictionary<Int32, List<double>> mom = new SortedDictionary<Int32, List<double>>();
            int dayhr, day, mon;

            //calculate sumx, sumxx and n for each day-hr
            foreach (var item in tseries)
            {
                if (!item.Value.Contains(MISS))
                {
                    double val = Convert.ToDouble(item.Value);
                    day = item.Key.Day;
                    mon = item.Key.Month;
                    dayhr = 10000 * mon + 100 * day + item.Key.Hour;
                    if (mon == 2 && day == 29) //leap day add to 2/28
                        dayhr = 22800 + item.Key.Hour;

                    if (calc.ContainsKey(dayhr))
                    {
                        List<double> lstcalc = new List<double>();
                        calc.TryGetValue(dayhr, out lstcalc);
                        lstcalc[0] += val;
                        lstcalc[1] += val * val;
                        lstcalc[2] += 1.0;
                        lstcalc = null;
                    }
                    else
                    {
                        List<double> lstcalc = new List<double>();
                        lstcalc.Add(val);
                        lstcalc.Add(val * val);
                        lstcalc.Add(1.0);
                        calc.Add(dayhr, lstcalc);
                        lstcalc = null;
                    }
                }
            }

            //calculate mean and sd for each day-hr
            int ix = 0;
            foreach (var kv in calc)
            {
                double avg = -9999, sd = -9999, var;
                double sumx = kv.Value.ElementAt(0);
                double sumxx = kv.Value.ElementAt(1);
                double n = kv.Value.ElementAt(2);
                if (n > 0)
                {
                    avg = sumx / n;
                    var = (sumxx - (1.0 / n) * avg * avg) / (n - 1);
                    sd = Math.Sqrt(var);
                }
                List<double> lst = new List<double>();
                lst.Add(avg); lst.Add(sd);
                ix++;
                //wri.WriteLine("{0},{1},{2},{3}", kv.Key.ToString(), ix.ToString(), avg.ToString(), sd.ToString());
                mom.Add(kv.Key, lst);
                lst = null;
            }
            calc = null;
            return mom;
        }
        private bool AutoCorrelationSeasonalHourly()
        {
            //need to work on this
            Debug.WriteLine("Entering Autocorrelation routine");
            //fMain.WriteLogFile("Calculating Autocorrelation for " + svar + " for site: " + site);

            try
            {
                int ncnt = dresid.Count;
                double[] xresid = dresid.Values.ToArray();
                double mean = Statistics.Mean(xresid);
                double stdev = Statistics.StandardDeviation(xresid);
                xresid = null;

                wri.WriteLine("Standardized Series Mean: {0}, Variance: {1}", mean.ToString("F4"), (stdev * stdev).ToString("F4"));
                wri.WriteLine("Serial Correlation of residuals");

                double[] serial = new double[nlag + 1];
                DateTime predt;
                double varerr = 0.0, avgerr = 0.0, theta = 0.0;
                for (int imo = 1; imo <= 12; imo++)
                {
                    for (int klag = 1; klag < nlag; klag++)
                    {
                        List<double> xres0 = new List<double>();
                        List<double> xres1 = new List<double>();

                        //iterate on series
                        for (int j = klag; j < ncnt; j++)
                        {
                            DateTime dt = dresid.Keys.ElementAt(j);
                            if (dt.Month == imo)
                            {
                                double val = 0.0;
                                xres0.Add(dresid.Values.ElementAt(j));
                                predt = dt.AddHours(-klag);
                                dresid.TryGetValue(predt, out val);
                                xres1.Add(val);
                            }
                        }
                        serial[klag] = Correlation.Pearson(xres0, xres1);
                        if (klag <= nlag / 2)
                            wri.WriteLine("{0},{1},{2}", imo.ToString(), klag.ToString(),
                                serial[klag].ToString("F4"));

                        //estimate sigma of error for AR1
                        if (klag == 1)
                        {
                            theta = serial[1];
                            List<double> xerr = new List<double>();
                            for (int k = 0; k < xres0.Count; k++)
                                xerr.Add(xres0[k] - theta * xres1[k]);
                            avgerr = Statistics.Mean(xerr);
                            varerr = Statistics.Variance(xerr);
                            model.Add("ar1coeff_" + imo, theta);
                            model.Add("varerr_" + imo, varerr);
                            model.Add("avgerr_" + imo, avgerr);
                            xerr = null;
                        }

                        xres0 = null;
                        xres1 = null;
                    }

                    wri.WriteLine("AR1 Model: Month{0}, Theta={1}, ErrorVar={2}",
                            imo.ToString("00"), theta.ToString("F4"), varerr.ToString("F4"));

                }

                //save
                dresid = null;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating serial correlation of standardized hourly series!", ex);
                return false;
            }

        }
        private SortedDictionary<string, double> CalculateHourlyHarmonics()
        {
            //fMain.WriteLogFile("Calculating periodicities of hourly mean and variance for " + svar + ": " + site);
            try
            {
                SortedDictionary<string, double> mdl = new SortedDictionary<string, double>();

                int nh = nhar; int slen = moments.Keys.Count;
                double ncnt = moments.Keys.Count;
                double[] stats = new double[slen];
                List<double> parms = new List<double>();

                for (int kser = 0; kser < 2; kser++)
                {
                    double sumx = 0, sumxx = 0, xavg, xsd, xvar;
                    for (int i = 0; i < ncnt; i++)
                    {
                        parms = moments.Values.ElementAt(i);
                        stats[i] = parms[kser];
                        sumx += stats[i];
                        sumxx += stats[i] * stats[i];
                    }
                    xavg = sumx / ncnt;
                    xvar = (sumxx - (ncnt * xavg * xavg)) / (ncnt - 1.0);
                    xsd = Math.Sqrt(xvar);
                    mdl.Add(series[kser] + ":mean", xavg);
                    mdl.Add(series[kser] + ":stdev", xsd);
                    wri.WriteLine(series[kser] + ": Avg, Stdev");
                    wri.WriteLine("{0}, {1}", xavg.ToString(), xsd.ToString());
                    //Debug.WriteLine(series[kser] + ": Avg, Stdev");
                    //Debug.WriteLine("{0}, {1}", xavg.ToString(), xsd.ToString());

                    double[] suma = new double[nh + 1];
                    double[] sumb = new double[nh + 1];
                    double[] acoef = new double[nh + 1];
                    double[] bcoef = new double[nh + 1];
                    double[] ph = new double[nh + 1];
                    double phsum = 0;

                    //calculate harmonics
                    wri.WriteLine("ACoef, BCoef, %Var, %SumVar");
                    for (int j = 0; j < nh; j++)
                    {
                        suma[j] = 0.0; sumb[j] = 0.0; acoef[j] = 0.0; bcoef[j] = 0; ph[j] = 0;
                        double icnt = 0;
                        int ihar = Convert.ToInt32(nharhly[j]); //1,365,730 ...
                        for (int k = 0; k < ncnt; k++)
                        {
                            icnt += 1.0;
                            double val = stats[k];
                            suma[j] += val * Math.Cos((2.0 * Math.PI * ihar * icnt) / ncnt);
                            sumb[j] += val * Math.Sin((2.0 * Math.PI * ihar * icnt) / ncnt);
                        }
                        acoef[j] = (2.0 * suma[j] / ncnt);
                        bcoef[j] = (2.0 * sumb[j] / ncnt);
                        ph[j] = (acoef[j] * acoef[j] + bcoef[j] * bcoef[j]) / (2 * xvar);
                        phsum += ph[j];
                        wri.WriteLine("{0}, {1}, {2},{3}, {4}", ihar.ToString(),
                           acoef[j].ToString(), bcoef[j].ToString(), ph[j].ToString(), phsum.ToString());
                        //if ((j==1) || (j == 365))
                        //Debug.WriteLine("{0}, {1}, {2}, {3},{4}", ihar.ToString(),
                        //   acoef[j].ToString(), bcoef[j].ToString(), ph[j].ToString(), phsum.ToString());
                        //add to dict
                        mdl.Add(series[kser] + ":acoef" + ihar.ToString(), acoef[j]);
                        mdl.Add(series[kser] + ":bcoef" + ihar.ToString(), bcoef[j]);
                    }
                }

                //debug
                //for (int i = 0; i < mdl.Count; i++)
                //    Debug.WriteLine("{0},{1}", mdl.Keys.ElementAt(i), mdl.Values.ElementAt(i));
                return mdl;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating periodicities in hourly mean and variance!", ex);
                return null;
            }
        }
        private SortedDictionary<Int32, List<double>> CalculateDailyMoments()
        {
            SortedDictionary<Int32, List<double>> calc = new SortedDictionary<Int32, List<double>>();
            SortedDictionary<Int32, List<double>> moments = new SortedDictionary<Int32, List<double>>();
            int day, mon, daymo;

            //calculate sumx, sumxx and n for each day-hr
            foreach (var item in dseries)
            {
                if (!item.Value.Contains(MISS))
                {
                    double val = Convert.ToDouble(item.Value);
                    day = item.Key.Day;
                    mon = item.Key.Month;
                    daymo = mon * 100 + day;
                    if (mon == 2 && day == 29)
                        daymo = 228;

                    if (calc.ContainsKey(daymo))
                    {
                        List<double> lstcalc = new List<double>();
                        calc.TryGetValue(day, out lstcalc);
                        lstcalc[0] += val;
                        lstcalc[1] += val * val;
                        lstcalc[2] += 1.0;
                        lstcalc = null;
                    }
                    else
                    {
                        List<double> lstcalc = new List<double>();
                        lstcalc[0] = val;
                        lstcalc[1] = val * val;
                        lstcalc[2] = 1.0;
                        calc.Add(daymo, lstcalc);
                        lstcalc = null;
                    }
                }
            }

            //calculate mean and sd for each day-hr
            foreach (var kv in calc)
            {
                double avg = -9999, sd = -9999, var;
                double sumx = kv.Value.ElementAt(0);
                double sumxx = kv.Value.ElementAt(1);
                double n = kv.Value.ElementAt(2);
                if (n > 0)
                {
                    avg = sumx / n;
                    var = (sumxx - (1.0 / n) * avg * avg) / (n - 1);
                    sd = Math.Sqrt(var);
                }
                List<double> lst = new List<double>();
                lst.Add(avg); lst.Add(sd);
                moments.Add(kv.Key, lst);
                lst = null;
            }
            calc = null;
            return moments;
        }
        private bool StandardizeHourly()
        {
            Debug.WriteLine("Entering standardized series routine");
            //fMain.WriteLogFile("Standardizing series " + svar + " for site: " + site);
            try
            {
                int day, hr, mon, dayhr;
                int ix = 0, jday;
                double xavg = 0.0, xstd = 0.0, val = 0.0;
                string sval;
                hresid.Clear();

                foreach (KeyValuePair<DateTime, string> kvp in dseries)
                {
                    ix++;
                    DateTime dt = kvp.Key;
                    sval = kvp.Value;
                    day = dt.Day; mon = dt.Month; hr = dt.Hour;

                    dayhr = 10000 * mon + 100 * day + hr;
                    if (mon == 2 && day == 29) //use 2/28
                        dayhr = 22800 + hr;

                    dictIndex.TryGetValue(dayhr, out jday);
                    xavg = pAvg[jday]; xstd = pStd[jday];

                    if (!sval.Contains(MISS))
                        val = (Convert.ToDouble(sval) - xavg) / xstd;
                    else
                        val = 0;
                    hresid.Add(ix, val);
                    //wri.WriteLine("{0},{1},{2},{3}", dayhr.ToString(), jday.ToString(), ix.ToString(), val.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error calculating standardized hourly series!", ex);
                return false;
            }
        }
        private void GeneratePeriodicMoments()
        {
            Debug.WriteLine("Entering GeneratePeriodicMoments()");
            try
            {
                //generate harmonics of mean and stdev, routine OK
                int ncnt = 8760;
                string kv;
                for (int kser = 0; kser < 2; kser++)
                {
                    double[] acoef = new double[nhar + 1];
                    double[] bcoef = new double[nhar + 1];
                    double mean = 0.0;

                    for (int j = 0; j < nhar; j++)
                    {
                        int ihar = Convert.ToInt32(nharhly[j]); //1,365,730 ...

                        kv = series[kser] + ":acoef" + ihar;
                        model.TryGetValue(kv, out acoef[j]);
                        kv = series[kser] + ":bcoef" + ihar;
                        model.TryGetValue(kv, out bcoef[j]);
                        //Debug.WriteLine("{0},{1},{2}", series[kser], acoef[j].ToString(), bcoef[j].ToString());
                    }
                    kv = series[kser] + ":mean";
                    model.TryGetValue(kv, out mean);
                    //Debug.WriteLine("{0},{1}", series[kser], mean.ToString());

                    for (int t = 1; t <= ncnt; t++)
                    {
                        double sum = 0.0, radian = 0.0;
                        for (int j = 0; j < nhar; j++)
                        {
                            int ihar = Convert.ToInt32(nharhly[j]); //1,365,730 ...
                            radian = (2.0 * Math.PI * ihar * t) / ncnt;
                            sum += acoef[j] * Math.Cos(radian) + bcoef[j] * Math.Sin(radian);
                        }
                        if (kser == 0)
                            pAvg[t] = mean + sum;
                        else
                            pStd[t] = mean + sum;
                    }
                    acoef = null; bcoef = null;
                }
                //wri.WriteLine("Mean and SD harmonic series");
                //for (int t = 1; t <= ncnt; t++)
                //    wri.WriteLine("{0},{1}", pAvg[t].ToString(), pStd[t].ToString());
            }
            catch (Exception ex)
            {
                ShowError("Error generating periodic hourly means and variances!", ex);
                //return null;
            }
        }
        private void GetDayTimeIndex(SortedDictionary<int, int> dictIndex)
        {
            int jday = 0, jdy = 0;
            for (int mon = 1; mon <= 12; mon++)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(2019, mon); day++)
                {
                    for (int hr = 0; hr < 24; hr++)
                    {
                        jday = mon * 10000 + day * 100 + hr;
                        jdy++;
                        dictIndex.Add(jday, jdy);
                    }
                }
            }
        }
        public SortedDictionary<DateTime, double> StandardizeDaily(SortedDictionary<Int32, List<double>> dictStats)
        {
            int day, hr, mon, daymo;
            List<double> stats;
            double val;

            SortedDictionary<DateTime, double> dresid = new SortedDictionary<DateTime, double>();
            foreach (var item in dseries)
            {
                DateTime dt = item.Key;
                day = dt.Day; hr = dt.Hour; mon = dt.Month;
                daymo = 100 * mon + day;
                if (mon == 2 && day == 29) daymo = 228;
                dictStats.TryGetValue(daymo, out stats);

                if (!item.Value.Contains(MISS))
                    val = (Convert.ToDouble(item.Value) - stats[0]) / stats[1];
                else
                    val = -9999;
                dresid.Add(dt, val);
            }
            return dresid;
        }
        public SortedDictionary<Int32, List<double>> CalculateHourlyMomentsLINQ(SortedDictionary<DateTime, string> tseries)
        {
            SortedDictionary<Int32, List<double>> moments = new SortedDictionary<Int32, List<double>>();
            int dayhr;

            StreamWriter wri = new StreamWriter(logfile, true);
            wri.WriteLine("{0},{1}", site, svar);

            for (int i = 1; i <= 12; i++)
            {
                int days = DateTime.DaysInMonth(2019, i);
                for (int j = 1; j <= days; j++)
                {
                    for (int k = 0; k < 24; k++)
                    {
                        List<double> stat = new List<double>();
                        var dict = tseries.Where(p => p.Key.Month == i &
                            p.Key.Day == j & p.Key.Hour == k & (!p.Value.Contains("9999"))).Select(p => p.Value);

                        int ncnt = dict.Count();
                        double[] x = new double[ncnt];
                        for (int kk = 0; kk < ncnt; kk++)
                            x[kk] = Double.Parse(dict.ElementAt(kk));

                        stat.Add(Statistics.Mean(x));
                        stat.Add(Statistics.StandardDeviation(x));
                        dayhr = i * 10000 + j * 100 + k;
                        wri.WriteLine("{0},{1},{2}", dayhr.ToString(), stat[0].ToString(), stat[1].ToString());
                        moments.Add(dayhr, stat);
                        stat = null;
                    }
                }
            }
            wri.Flush();
            wri.Close();
            return moments;
        }
        public SortedDictionary<DateTime, double> StandardizeHourlyOld(SortedDictionary<Int32, List<double>> dictStats)
        {
            int day, hr, mon, dayhr;
            List<double> stats;
            double avg, sd, val;

            StreamWriter wri = new StreamWriter(logfile, true);
            wri.WriteLine("Standardized Values");

            SortedDictionary<DateTime, double> hresid = new SortedDictionary<DateTime, double>();
            int ix = 0;
            foreach (var item in dseries)
            {
                DateTime dt = item.Key;
                day = dt.Day; hr = dt.Hour; mon = dt.Month;
                dayhr = 10000 * mon + 100 * day + hr;
                if (mon == 2 && day == 29) //use 2/28
                    dayhr = 22800 + hr;

                dictStats.TryGetValue(dayhr, out stats);

                if (!item.Value.Contains(MISS))
                    val = (Convert.ToDouble(item.Value) - stats[0]) / stats[1];
                else
                    val = 0;

                ix++;
                wri.WriteLine("{0},{1},{2}", dayhr.ToString(), ix.ToString(), val.ToString());
                hresid.Add(dt, val);
            }
            wri.Flush();
            wri.Close();
            return hresid;
        }
        #endregion
    }
}
