using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace NCEIData
{
    class clsValidateSpatial
    {
        private frmMain fMain;
        private StreamWriter wri;
        private double ndiv = 0.5;
        private string site, svar;
        private SortedDictionary<DateTime, string> dseries;
        private string MISS = "9999";
        private string logfile = string.Empty;
        private enum Interval { Hourly, Daily };
        private Random rand;

        private Dictionary<int, SortedDictionary<DateTime, string>> NearbySeries;
        private List<double> NearbySeriesMean;
        private List<double> NearbySeriesWts;
        private List<string> NearbySites;
        private List<double> Distance;

        private string crlf = Environment.NewLine;

        public clsValidateSpatial(frmMain _fMain, StreamWriter _wri,
                                  SortedDictionary<DateTime, string> _tseries,
                                  Dictionary<int, SortedDictionary<DateTime, string>> _NearbySeries,
                                  List<double> _NearbySeriesMean, List<double> _NearbySeriesWts,
                                  List<double> _Distance, List<string> _NearbySites,
                                  string _svar, string _site)
        {
            this.fMain = _fMain;
            this.dseries = _tseries;
            this.NearbySeries = _NearbySeries;
            this.NearbySeriesMean = _NearbySeriesMean;
            this.NearbySeriesWts = _NearbySeriesWts;
            this.NearbySites = _NearbySites;
            this.Distance = _Distance;
            this.site = _site;
            this.svar = _svar;
            this.wri = _wri;
        }
        public bool CrossValidateSpatial(int tstep, int nsta, double SeriesMean)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                int nrecs = dseries.Count;
                int nsample = Convert.ToInt32(nrecs * ndiv);

                string msg = crlf + "Validating spatial model for " + svar + ": " + site +
                        " using " + nsample.ToString() + " random samples.";
                fMain.WriteLogFile(msg);
                wri.WriteLine(msg);
                fMain.appManager.UpdateProgress(msg);

                //log info
                wri.WriteLine("Series Mean : " + SeriesMean.ToString("F3"));
                wri.WriteLine("Nearby stations used for interpolation");
                for (int i = 0; i < nsta; i++)
                    wri.WriteLine(NearbySites[i] + ": Mean = " + NearbySeriesMean[i].ToString("F3") +
                        ", Distance = " + Distance[i].ToString("F3") + " mi.");

                //initialize random generator
                rand = new Random();

                //get first observation time
                DateTime basedt = dseries.Keys.ElementAt(0);
                DateTime dt;
                string sobs;
                double xsim = 0.0, rmse = 0.0, varobs = 0.0, varerr = 0.0, nse = 0.0;
                double avg = 0.0, std = 0.0, corr = 0.0, xobs = 0.0, sper = 0.0;
                List<double> yobs = new List<double>();
                List<double> ysim = new List<double>();
                List<double> xerr = new List<double>();
                int numObsWet = 0, numObsDry = 0, numSimWet = 0, numSimDry = 0;

                for (int isamp = 0; isamp < nsample; isamp++)
                {
                    double prb = rand.NextDouble();

                    int inter = Convert.ToInt32(prb * nrecs);
                    if (tstep == (int)Interval.Hourly)  //hourly
                        dt = basedt.AddHours(inter);
                    else                              //daily                        
                        dt = basedt.AddDays(inter);

                    if (dseries.TryGetValue(dt, out sobs) && !sobs.Contains(MISS))
                    {
                        xobs = Convert.ToDouble(sobs);

                        //estimate record
                        xsim = EstimateData(nsta, dt, xobs, SeriesMean);
                        if (!(xsim > 9990)) //missing or error is 9999
                        {
                            if (svar.Contains("WIND"))
                                if (xsim < 0) xsim = 0.0;
                                else if (svar.Contains("CLOU"))
                                {
                                    if (xsim < 0) xsim = 0.0;
                                    else if (xsim > 10) xsim = 10.0;
                                    xsim = ReClassCloud(xsim);
                                }
                            if (svar.Contains("PREC") || svar.Contains("PRCP"))
                            {
                                //get only when rain > 0
                                //if (xobs > 0)
                                {
                                    yobs.Add(xobs);
                                    ysim.Add(xsim);
                                    xerr.Add(xsim - xobs);
                                    if (xobs > 0)
                                        numObsWet++;
                                    else
                                        numObsDry++;

                                    if (xsim > 0)
                                        numSimWet++;
                                    else
                                        numSimDry++;

                                    //if (xsim > 0) numSimWet++;
                                    //else numSimDry++;
                                }
                                //else numObsDry++;
                            }
                            else
                            {
                                yobs.Add(xobs);
                                ysim.Add(xsim);
                                xerr.Add(xsim - xobs);
                            }
                        }
                        //wri.WriteLine("{0},{1},{2},{3}", isamp.ToString(), dt.ToString(), 
                        //    xobs.ToString("F4"), xsim.ToString("F4"));
                    }
                } //end for

                //calculate statistics of fit
                avg = Statistics.Mean(xerr);
                std = Statistics.StandardDeviation(xerr);
                rmse = Statistics.RootMeanSquare(xerr);
                corr = Correlation.Pearson(yobs, ysim);
                sper = Correlation.Spearman(yobs, ysim);
                varobs = Statistics.Variance(yobs);
                varerr = Statistics.Variance(xerr);
                nse = 1.0 - (varerr / varobs);

                wri.WriteLine("Statistics of spatial estimates for " + svar + ": " + site);
                wri.WriteLine("Random Sample: " + nsample.ToString());
                if (svar.Contains("PREC") || svar.Contains("PRCP"))
                {
                    wri.WriteLine("Average Error when rain>0: " + avg.ToString("F4"));
                    wri.WriteLine("Standard Deviation of Error: " + std.ToString("F4"));
                    wri.WriteLine("No. of observed wet interval: " + numObsWet.ToString());
                    wri.WriteLine("No. of simulated wet interval: " + numSimWet.ToString());
                    wri.WriteLine("No. of observed dry interval: " + numObsDry.ToString());
                    wri.WriteLine("No. of simulated dry interval: " + numSimDry.ToString());
                }
                else
                {
                    wri.WriteLine("Average Error: " + avg.ToString("F4"));
                    wri.WriteLine("Standard Deviation of Error: " + std.ToString("F4"));
                }
                wri.WriteLine("Pearson Correlation: " + corr.ToString("F4"));
                wri.WriteLine("Spearman Correlation: " + sper.ToString("F4"));
                wri.WriteLine("Nash-Sutcliffe Coefficient: " + nse.ToString("F4"));
                //wri.WriteLine(crlf);
                wri.Flush();

                xerr = null;
                yobs = null; ysim = null;
                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Error validating model!", ex);
                return false;
            }
        }

        private double EstimateData(int nSta, DateTime curdt, double xobs, double SeriesMean)
        {
            double xsim;
            try
            {
                List<string> otherVals = new List<string>();
                otherVals = GetOtherSeriesData(nSta, curdt);
                if (otherVals == null)
                    return 9999;

                int numdat = 0;
                double sum = 0.0, sumwt = 0.0;

                for (int ii = 0; ii < nSta; ii++)
                {
                    double meanratio = SeriesMean / NearbySeriesMean[ii];
                    if (!(otherVals[ii].Contains(MISS)))
                    {
                        numdat++;
                        sum += NearbySeriesWts[ii] * Convert.ToDouble(otherVals[ii]) * meanratio;
                        sumwt += NearbySeriesWts[ii];
                    }
                }
                xsim = (sumwt > 0.0) ? (sum / (sumwt)) : 9999;
            }
            catch (Exception ex)
            {
                return 9999;
            }
            return xsim;
        }

        private List<string> GetOtherSeriesData(int nSta, DateTime dt)
        {
            List<string> svals = new List<string>();
            try
            {
                SortedDictionary<DateTime, string> tseries;
                for (int ii = 0; ii < nSta; ii++)
                {
                    string val;
                    if (NearbySeries.TryGetValue(ii, out tseries))
                    {
                        if (tseries.TryGetValue(dt, out val))
                            svals.Add(val);
                        else
                            svals.Add(MISS);
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                ShowError("Error getting other series values for " + dt.ToString(), ex);
                return null;
            }
            return svals;
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
        public void ShowError(string msg, Exception ex)
        {
            msg += crlf + ex.Message + crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
