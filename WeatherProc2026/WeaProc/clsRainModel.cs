using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NCEIData
{
    class clsRainModel
    {
        private frmMain fMain;
        private string site, svar;
        private SortedDictionary<DateTime, string> dseries;
        private int tstep;
        private string MISS = "9999";
        private string logfile = string.Empty;
        private enum Interval { Hourly, Daily };
        private SortedDictionary<string, double> model = new SortedDictionary<string, double>();
        private NCEImessage nceimsg = new NCEImessage();
        private StreamWriter wri;
        private int ndiv = 2;
        private Random rand;
        private string Crlf = Environment.NewLine;

        public clsRainModel(frmMain _fMain, string _site, string _svar,
                 SortedDictionary<DateTime, string> _series, StreamWriter _wri)
        {
            this.fMain = _fMain;
            this.dseries = _series;
            this.site = _site; 
            this.svar = _svar; 
            this.wri = _wri;
            wri.AutoFlush = true;
        }

        public void FitMarkovModel(int timestep)
        {
            switch (timestep)
            {
                case (int)Interval.Hourly:
                    CalculateHourlyProbabilities();
                    CrossValidateHourlyModel(dseries);
                    break;
                case (int)Interval.Daily:
                    CalculateDailyProbabilities();
                    CrossValidateDailyModel(dseries);
                    break;
            }
        }

        private void CalculateHourlyProbabilities()
        {
            fMain.WriteLogFile("Calculating transition probabilities for hourly " + svar + ": " + site);

            try
            {
                int mon;

                //initialize probabilities arrays
                double[] pdd = new double[12];
                //double[] pdw = new double[12];
                //double[] pwd = new double[12];
                double[] pww = new double[12];
                double[] sumx = new double[12];
                double[] sumxx = new double[12];
                double[] nwcnt = new double[12];
                double[] ndcnt = new double[12];
                int[] ncnt = new int[12];
                double gama = 0.0, pdw = 0.0, pwd = 0.0;
                double avg = 0.0, std2 = 0.0;
                double alpha = 0.0, beta = 0.0;

                for (int i = 0; i < 12; i++)
                {
                    pdd[i] = 0.0; pww[i] = 0.0;          //pdw[i] = 0.0; pwd[i] = 0.0;
                    ncnt[i] = 0; sumx[i] = 0.0; sumxx[i] = 0.0;
                    nwcnt[i] = 0; ndcnt[i] = 0;
                }

                double curval, nexval;
                string scurval, snexval;

                //for (int i = 0; i < dseries.Keys.Count-1; i++)
                foreach (KeyValuePair<DateTime, string> kv in dseries)
                {
                    DateTime dt = kv.Key;
                    DateTime nextdt = dt.AddHours(1);

                    //curmon = dseries.Keys.ElementAt(i).Month;
                    //mon = curmon - 1; //array is 0 based
                    mon = dt.Month - 1;
                    ncnt[mon]++;

                    //scurval = dseries.Values.ElementAt(i);
                    //snexval = dseries.Values.ElementAt(i + 1);
                    dseries.TryGetValue(dt, out scurval);
                    if (!dseries.TryGetValue(nextdt, out snexval)) break;

                    if (!scurval.Contains(MISS) && !snexval.Contains(MISS))
                    {
                        curval = Convert.ToDouble(scurval);
                        nexval = Convert.ToDouble(snexval);

                        if (curval > 0) //wet
                        {
                            sumx[mon] += curval;
                            sumxx[mon] += curval * curval;
                            nwcnt[mon] += 1.0;
                            if (nexval > 0) pww[mon]++;
                            //else
                            //    pwd[mon]++;
                        }
                        else //dry
                        {
                            ndcnt[mon] += 1.0;
                            if (!(nexval > 0)) pdd[mon]++;
                            //pdw[mon]++;
                            //else
                            //    pdd[mon]++;
                        }
                    }
                }//end loop series

                //summarize probabilities
                wri.WriteLine(Crlf + "Transitional probabilities and parameters:" + site);
                wri.WriteLine("e.g. PD|W, probability of dry hour given a previous wet hour.");
                wri.WriteLine("Month, PD|D, PD|W, PW|D, PW|W, Mean, Variance, Exponential-Lambda, Gamma-Alpha, Gamma-Beta");

                for (int i = 0; i < 12; i++)
                {
                    mon = i + 1;
                    if (ndcnt[i] > 0)
                    {
                        pdd[i] /= ndcnt[i];
                        //pdw[i] /= ndcnt[i];
                        pdw = 1 - pdd[i];
                        model.Add("pdd" + mon, pdd[i]);
                        model.Add("pdw" + mon, pdw);
                    }
                    if (nwcnt[i] > 0)
                    {
                        //pwd[i] /= nwcnt[i];
                        pww[i] /= nwcnt[i];
                        pwd = 1 - pww[i];
                        avg = sumx[i] / nwcnt[i];
                        std2 = (sumxx[i] - (1.0 / nwcnt[i]) * sumx[i] * sumx[i]) / (nwcnt[i]);
                        model.Add("pwd" + mon, pwd);
                        model.Add("pww" + mon, pww[i]);
                        model.Add("mean" + mon, avg);
                        model.Add("variance" + mon, std2);
                        gama = 1.0 / avg;
                        alpha = (avg * avg) / std2;
                        beta = std2 / avg;
                        model.Add("gamma" + mon, gama);
                        model.Add("alpha" + mon, alpha);
                        model.Add("beta" + mon, beta);
                    }
                    //save
                    wri.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", mon.ToString(),
                        pdd[i].ToString("F4"), pdw.ToString("F4"),
                        pwd.ToString("F4"), pww[i].ToString("F4"),
                        avg.ToString("F4"), std2.ToString("F4"), gama.ToString("F4"),
                        alpha.ToString("F4"), beta.ToString("F4"));
                }
                pdd = null; pww = null;//pdw = null;pwd = null;
                sumx = null; sumxx = null; nwcnt = null; ndcnt = null; ncnt = null;
            }
            catch (Exception ex)
            {
                nceimsg.ShowError("Error in calculating transitional probabilities of rainfall for site " + site +
                    "!", ex);
            }
        }
        private void CalculateDailyProbabilities()
        {
            try
            {
                fMain.WriteLogFile("Calculating transition probabilities for daily " + svar + ": " + site);

                int mon;

                //initialize probabilities arrays
                double[] pdd = new double[12];
                double[] pww = new double[12];
                double[] sumx = new double[12];
                double[] sumxx = new double[12];
                double[] nwcnt = new double[12];
                double[] ndcnt = new double[12];
                int[] ncnt = new int[12];
                double gama = 0.0, pdw = 0.0, pwd = 0.0;
                double avg = 0.0, std2 = 0.0;
                double alpha = 0.0, beta = 0.0;

                for (int i = 0; i < 12; i++)
                {
                    pdd[i] = 0.0; pww[i] = 0.0;          //pdw[i] = 0.0; pwd[i] = 0.0;
                    ncnt[i] = 0; sumx[i] = 0.0; sumxx[i] = 0.0;
                    nwcnt[i] = 0; ndcnt[i] = 0;
                }

                double curval, nexval;
                string scurval, snexval;

                foreach (KeyValuePair<DateTime, string> kv in dseries)
                {
                    DateTime dt = kv.Key;
                    DateTime nextdt = dt.AddDays(1);

                    mon = dt.Month - 1;
                    ncnt[mon]++;

                    dseries.TryGetValue(dt, out scurval);
                    if (!dseries.TryGetValue(nextdt, out snexval)) break;

                    if (!scurval.Contains(MISS) && !snexval.Contains(MISS))
                    {
                        curval = Convert.ToDouble(scurval);
                        nexval = Convert.ToDouble(snexval);

                        if (curval > 0) //wet
                        {
                            sumx[mon] += curval;
                            sumxx[mon] += curval * curval;
                            nwcnt[mon] += 1.0;
                            if (nexval > 0) pww[mon]++;
                        }
                        else //dry
                        {
                            ndcnt[mon] += 1.0;
                            if (!(nexval > 0)) pdd[mon]++;
                        }
                    }
                }//end loop series

                //summarize probabilities
                wri.WriteLine(Crlf + "Transitional probabilities and parameters:" + site);
                wri.WriteLine("e.g. PD|W, probability of dry day given a previous wet day.");
                wri.WriteLine("Month, PD|D, PD|W, PW|D, PW|W, Mean, Variance, Exponential-Lambda, Gamma-Alpha, Gamma-Beta");

                for (int i = 0; i < 12; i++)
                {
                    mon = i + 1;
                    if (ndcnt[i] > 0)
                    {
                        pdd[i] /= ndcnt[i];
                        pdw = 1 - pdd[i];
                        model.Add("dlypdd" + mon, pdd[i]);
                        model.Add("dlypdw" + mon, pdw);
                    }
                    if (nwcnt[i] > 0)
                    {
                        pww[i] /= nwcnt[i];
                        pwd = 1 - pww[i];
                        avg = sumx[i] / nwcnt[i];
                        std2 = (sumxx[i] - (1.0 / nwcnt[i]) * sumx[i] * sumx[i]) / (nwcnt[i]);
                        model.Add("dlypwd" + mon, pwd);
                        model.Add("dlypww" + mon, pww[i]);
                        model.Add("dlymean" + mon, avg);
                        model.Add("dlyvariance" + mon, std2);
                        gama = 1.0 / avg;
                        alpha = (avg * avg) / std2;
                        beta = std2 / avg;
                        model.Add("dlygamma" + mon, gama);
                        model.Add("dlyalpha" + mon, alpha);
                        model.Add("dlybeta" + mon, beta);
                    }
                    //save
                    wri.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", mon.ToString(),
                        pdd[i].ToString("F4"), pdw.ToString("F4"),
                        pwd.ToString("F4"), pww[i].ToString("F4"),
                        avg.ToString("F4"), std2.ToString("F4"), gama.ToString("F4"),
                        alpha.ToString("F4"), beta.ToString("F4"));
                }
                pdd = null; pww = null;//pdw = null;pwd = null;
                sumx = null; sumxx = null; nwcnt = null; ndcnt = null; ncnt = null;
            }
            catch (Exception ex)
            {
                nceimsg.ShowError("Error in calculating transitional probabilities of rainfall for site " + site +
                    "!", ex);
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
                fMain.WriteLogFile(msg);
                wri.WriteLine(msg);
                fMain.appManager.UpdateProgress(msg);

                //initialize random generator
                rand = new Random();

                //get first hour of observation
                DateTime basedt = tseries.Keys.ElementAt(0);
                DateTime dt, predt;
                double prb = 0.0;

                //initialize variables for model
                double pww = 0.0, pdw = 0.0;
                double gama = 0.0, prerain = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0, obs = 0.0;
                string sdat, simrain, curobs;

                //summary
                int nobsdry = 0, nobswet = 0, nsimdry = 0, nsimwet = 0;
                double corr = 0, sper = 0, nse = 0, varobs = 0, varerr = 0;
                List<double> xsim = new List<double>();
                List<double> xobs = new List<double>();
                List<double> xerr = new List<double>();

                for (int isamp = 0; isamp < nsample; isamp++)
                {
                    prb = rand.NextDouble();
                    int ihrs = Convert.ToInt32(prb * nrecs);
                    dt = basedt.AddHours(ihrs);
                    predt = dt.AddHours(-1);

                    if (tseries.TryGetValue(dt, out curobs) && !curobs.Contains(MISS))
                    {
                        obs = Convert.ToDouble(curobs);
                        if (obs > 0) //wet
                        {
                            nobswet++;
                            //xobs.Add(obs);
                        }
                        else
                            nobsdry++;

                        //get parameters
                        int mon = predt.Month;
                        model.TryGetValue("gamma" + mon, out gama);
                        model.TryGetValue("alpha" + mon, out alpha);
                        model.TryGetValue("beta" + mon, out beta);
                        model.TryGetValue("pdw" + mon, out pdw);
                        model.TryGetValue("pww" + mon, out pww);
                        if (!tseries.TryGetValue(predt, out sdat))
                            sdat = "0.0";
                        prerain = Convert.ToDouble(sdat);

                        prob = rand.NextDouble();
                        if (prerain <= 0)                         //previous hour dry
                        {
                            if (prob > pdw)                       //missing is dry
                                simrain = "0.0";
                            else
                                simrain = GenerateRain(alpha, beta); //missing is wet, generate rain
                        }
                        else                                      //previous hour wet
                        {
                            if (prob > pww)                       //missing hour is dry
                                simrain = "0.0";                  //dry hour
                            else                                  //missing is wet, generate
                                simrain = GenerateRain(alpha, beta);
                        }

                        double sim = Convert.ToDouble(simrain);
                        if (obs > 0)
                        {
                            xobs.Add(obs);
                            xsim.Add(sim);
                            xerr.Add(sim - obs);
                        }

                        if (sim > 0) //wet
                        {
                            nsimwet++;
                            //xsim.Add(sim);
                        }
                        else
                            nsimdry++;
                    }
                }

                double avgerr = Statistics.Mean(xerr);
                double stderr = Statistics.StandardDeviation(xerr);

                corr = Correlation.Pearson(xobs, xsim);
                sper = Correlation.Spearman(xobs, xsim);
                varobs = Statistics.Variance(xobs);
                varerr = Statistics.Variance(xerr);
                nse = 1.0 - (varerr / varobs);


                wri.WriteLine("Statistics of model fit for " + svar + ": " + site);
                wri.WriteLine("Average Error of hourly rain: " + avgerr.ToString("F4"));
                wri.WriteLine("Stand Deviation of Error of hourly rain: " + stderr.ToString("F4"));
                wri.WriteLine("Observed dry hours: " + nobsdry.ToString());
                wri.WriteLine("Simulated dry hours: " + nsimdry.ToString());
                wri.WriteLine("Observed wet hours: " + nobswet.ToString());
                wri.WriteLine("Simulated wet hours: " + nsimwet.ToString());
                wri.WriteLine("Pearson Correlation: " + corr.ToString("F4"));
                wri.WriteLine("Spearman Correlation: " + sper.ToString("F4"));
                wri.WriteLine("Nash-Sutcliffe Coefficient: " + nse.ToString("F4") + Crlf);

                xsim = null; xobs = null; xerr = null;

                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                nceimsg.ShowError("Error validating model!", ex);
                return false;
            }
        }
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
                fMain.WriteLogFile(msg);
                wri.WriteLine(msg);
                fMain.appManager.UpdateProgress(msg);

                //initialize random generator
                rand = new Random();

                //get first hour of observation
                DateTime basedt = tseries.Keys.ElementAt(0);
                DateTime dt, predt;
                double prb = 0.0;
                double avg = 0.0, std = 0.0;

                //initialize variables for model
                double pww = 0.0, pdw = 0.0;
                double gama = 0.0, prerain = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0, obs = 0.0;
                string sdat, simrain, curobs;

                //summary
                int nobsdry = 0, nobswet = 0, nsimdry = 0, nsimwet = 0;
                double corr = 0, sper = 0, nse = 0, varobs = 0, varerr = 0;
                List<double> xsim = new List<double>();
                List<double> xobs = new List<double>();
                List<double> xerr = new List<double>();

                for (int isamp = 0; isamp < nsample; isamp++)
                {
                    prb = rand.NextDouble();
                    int ndys = Convert.ToInt32(prb * nrecs);
                    dt = basedt.AddDays(ndys);
                    predt = dt.AddDays(-1);

                    if (tseries.TryGetValue(dt, out curobs) && !curobs.Contains(MISS))
                    {
                        obs = Convert.ToDouble(curobs);
                        if (obs > 0) //wet
                        {
                            nobswet++;
                            //xobs.Add(obs);
                        }
                        else
                            nobsdry++;

                        //get parameters
                        int mon = predt.Month;
                        model.TryGetValue("dlygamma" + mon, out gama);
                        model.TryGetValue("dlyalpha" + mon, out alpha);
                        model.TryGetValue("dlybeta" + mon, out beta);
                        model.TryGetValue("dlypdw" + mon, out pdw);
                        model.TryGetValue("dlypww" + mon, out pww);
                        if (!tseries.TryGetValue(predt, out sdat))
                            sdat = "0.0";
                        prerain = Convert.ToDouble(sdat);

                        prob = rand.NextDouble();
                        if (prerain <= 0)                         //previous day dry
                        {
                            if (prob > pdw)                       //missing is dry
                                simrain = "0.0";
                            else
                                simrain = GenerateRain(alpha, beta); //missing is wet, generate rain
                        }
                        else                                      //previous day wet
                        {
                            if (prob > pww)                       //missing day is dry
                                simrain = "0.0";                  //dry day
                            else                                  //missing is wet, generate
                                simrain = GenerateRain(alpha, beta);
                        }

                        double sim = Convert.ToDouble(simrain);
                        //only when obs rain > 0
                        if (obs > 0)
                        {
                            xobs.Add(obs);
                            xsim.Add(sim);
                            xerr.Add(sim - obs);
                        }

                        if (sim > 0) //wet
                        {
                            nsimwet++;
                            //xsim.Add(sim);
                        }
                        else
                            nsimdry++;
                    }
                }

                //double avgobs = Statistics.Mean(xobs);
                //double avgsim = Statistics.Mean(xsim);
                double avgerr = Statistics.Mean(xerr);
                double stderr = Statistics.StandardDeviation(xerr);
                corr = Correlation.Pearson(xobs, xsim);
                sper = Correlation.Spearman(xobs, xsim);
                varobs = Statistics.Variance(xobs);
                varerr = Statistics.Variance(xerr);
                nse = 1.0 - (varerr / varobs);

                wri.WriteLine("Statistics of model fit for " + svar + ": " + site);
                wri.WriteLine("Average Error of daily rain: " + avgerr.ToString("F4"));
                wri.WriteLine("Stand Deviation of Error of daily rain: " + stderr.ToString("F4"));
                wri.WriteLine("Observed dry days: " + nobsdry.ToString());
                wri.WriteLine("Simulated dry days: " + nsimdry.ToString());
                wri.WriteLine("Observed wet days: " + nobswet.ToString());
                wri.WriteLine("Simulated wet days: " + nsimwet.ToString());
                wri.WriteLine("Pearson Correlation: " + corr.ToString("F4"));
                wri.WriteLine("Spearman Correlation: " + sper.ToString("F4"));
                wri.WriteLine("Nash-Sutcliffe Coefficient: " + nse.ToString("F4"));

                xsim = null; xobs = null; xerr = null;

                Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                nceimsg.ShowError("Error validating model!", ex);
                return false;
            }
        }
        private string GenerateRain(double alpha, double beta)
        {
            string srain = string.Empty;
            double rate = 1.0 / beta;
            double prb = rand.NextDouble();
            double rain = MathNet.Numerics.Distributions.Gamma.InvCDF(alpha, rate, prb);
            if (rain < 0) rain = 0.0;
            return rain.ToString("F3");
        }
        public SortedDictionary<string, double> MarkovModel()
        {
            return model;
        }
    }
}