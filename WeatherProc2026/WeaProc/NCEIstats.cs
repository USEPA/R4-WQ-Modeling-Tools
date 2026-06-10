using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NCEIData
{
    class NCEIstats
    {
        private SortedDictionary<DateTime, string> dictSeries;
        //daily
        private SortedDictionary<DateTime, double> dsiteStats = new SortedDictionary<DateTime, double>();
        //monthly
        private SortedDictionary<DateTime, double> msiteStats = new SortedDictionary<DateTime, double>();
        //annual
        private SortedDictionary<DateTime, double> asiteStats = new SortedDictionary<DateTime, double>();
        private NCEImessage NCEIMsg = new NCEImessage();
        private enum MetDataSource { ISD, GHCN, HRAIN };
        private int optDataSet = 0;

        private string MISS = "9999";

        public NCEIstats(int _optDataset, SortedDictionary<DateTime, string> _dictSeries)
        {
            this.dictSeries = _dictSeries;
            this.optDataSet = _optDataset;
        }
        public bool GetDataStats(string site, string svar)
        {
            switch (optDataSet)
            {
                case (int)MetDataSource.ISD:
                    MISS = "9999";
                    if (GetDataStatsISD(site, svar))
                        return true;
                    break;
                case (int)MetDataSource.GHCN:
                    MISS = "-9999";
                    if (GetDataStatsGHCN(site, svar))
                        return true;
                    break;
                case (int)MetDataSource.HRAIN:
                    MISS = "9999";
                    if (GetDataStatsHRAIN(site, svar))
                        return true;
                    break;
            }
            return false;
        }
        private bool GetDataStatsISD(string site, string svar)
        {
            DateTime curdt, prevdt, curday, prevday, curmon, prevmon, dyr;
            int curyr, prevyr;

            try
            {
                double dStats, mStats, aStats;
                clsStats stStats = new clsStats(MISS);

                string[] dat = dictSeries.Values.ToArray();
                DateTime[] dtime = dictSeries.Keys.ToArray();
                string curdat;

                //get first value of date
                prevdt = dictSeries.Keys.ElementAt(0);
                prevday = prevdt.Date;
                prevmon = (new DateTime(prevday.Year, prevday.Month, 1)).Date;
                prevyr = prevdt.Year;
                int ncount = dtime.Length;

                for (int i = 0; i < ncount; i++)
                {
                    curdt = dtime[i];
                    curday = curdt.Date;
                    curmon = (new DateTime(curday.Year, curday.Month, 1)).Date;
                    curyr = curdt.Year;
                    curdat = dat[i];

                    // daily stats
                    if ((curday - prevday).TotalHours > 0)
                    {   //new day
                        dStats = stStats.DailyAverage();
                        if (!dsiteStats.ContainsKey(prevday))
                        {
                            //check missing day stats, fill with previous day, 
                            //problem when first day is missing
                            //if (i != 0)
                            CheckMissingDailyStat(i, site, prevday, dStats);
                            dsiteStats.Add(prevday, dStats);
                        }
                        stStats.InitDaily();
                    }
                    stStats.DailySum(curdat);

                    //monthly stats
                    if ((curmon - prevmon).TotalHours > 0)
                    {   //new month
                        mStats = stStats.MonthlyAverage();
                        if (!msiteStats.ContainsKey(prevmon))
                        {
                            if (i != 0)
                                CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        stStats.InitMonthly();
                    }
                    stStats.MonthlySum(curdat);

                    //annual stats
                    if ((curyr - prevyr) > 0)
                    {
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        if (!asiteStats.ContainsKey(dyr))
                            asiteStats.Add(dyr, aStats);
                        stStats.InitAnnual();
                    }
                    stStats.AnnualSum(curdat);

                    prevday = curday;
                    prevmon = curmon;
                    prevyr = curyr;

                    if (i == ncount - 1)
                    {
                        //last day
                        dStats = stStats.DailyAverage();
                        if (!dsiteStats.ContainsKey(prevday))
                        {
                            //check missing day stats, fill with previous day
                            //CheckMissingDailyStat(i, site, prevday, dStats);
                            dsiteStats.Add(prevday, dStats);
                        }

                        //check missing month stats, fill with previous month
                        mStats = stStats.MonthlyAverage();
                        {
                            CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        asiteStats.Add(dyr, aStats);
                    }
                }
                //fill in missing statistics for the day
                FillMissingDlyStats(dsiteStats);

                //estimate missing daily statistics
                EstimateMissingDailyVarStat(dsiteStats);

                stStats = null;
                dat = null;
                dtime = null;
                curdat = null;
                //Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                //string msg = "Error in calculating series statistics for "+ site +"!";
                //NCEIMsg.ShowError(msg, ex);
                return false;
            }
        }
        private bool GetDataStatsHRAIN(string site, string svar)
        {
            DateTime curdt, prevdt, curday, prevday, curmon, prevmon, dyr;
            int curyr, prevyr;

            try
            {
                double dStats, mStats, aStats;
                clsStats stStats = new clsStats(MISS);

                string[] dat = dictSeries.Values.ToArray();
                DateTime[] dtime = dictSeries.Keys.ToArray();
                string curdat;

                //get first value of date
                prevdt = dictSeries.Keys.ElementAt(0);
                prevday = prevdt.Date;
                prevmon = (new DateTime(prevday.Year, prevday.Month, 1)).Date;
                prevyr = prevdt.Year;
                int ncount = dtime.Length;

                for (int i = 0; i < ncount; i++)
                {
                    curdt = dtime[i];
                    curday = curdt.Date;
                    curmon = (new DateTime(curday.Year, curday.Month, 1)).Date;
                    curyr = curdt.Year;
                    curdat = dat[i];

                    //monthly stats
                    if ((curmon - prevmon).TotalHours > 0)
                    {   //new month
                        mStats = stStats.MonthlyAverage();
                        if (!msiteStats.ContainsKey(prevmon))
                        {
                            if (i != 0)
                                CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        stStats.InitMonthly();
                    }
                    stStats.MonthlySum(curdat);

                    //annual stats
                    if ((curyr - prevyr) > 0)
                    {
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        if (!asiteStats.ContainsKey(dyr))
                            asiteStats.Add(dyr, aStats);
                        stStats.InitAnnual();
                    }
                    stStats.AnnualSum(curdat);

                    prevday = curday;
                    prevmon = curmon;
                    prevyr = curyr;

                    if (i == ncount - 1)
                    {
                        //last day
                        dStats = stStats.DailyAverage();
                        if (!dsiteStats.ContainsKey(prevday))
                        {
                            //check missing day stats, fill with previous day
                            //CheckMissingDailyStat(i, site, prevday, dStats);
                            dsiteStats.Add(prevday, dStats);
                        }

                        //check missing month stats, fill with previous month
                        mStats = stStats.MonthlyAverage();
                        {
                            CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        asiteStats.Add(dyr, aStats);
                    }
                }
                //fill in missing statistics for the day
                FillMissingDlyStats(dsiteStats);

                //estimate missing daily statistics
                EstimateMissingDailyVarStat(dsiteStats);

                stStats = null;
                dat = null;
                dtime = null;
                curdat = null;
                //Cursor.Current = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                //string msg = "Error in calculating series statistics for "+ site +"!";
                //NCEIMsg.ShowError(msg, ex);
                return false;
            }
        }
        private bool GetDataStatsGHCN(string site, string svar)
        {
            DateTime curdt, prevdt, curmon, prevmon, dyr;
            int curyr, prevyr;

            try
            {
                double mStats, aStats;
                clsStats stStats = new clsStats(MISS);

                string[] dat = dictSeries.Values.ToArray();
                DateTime[] dtime = dictSeries.Keys.ToArray();
                string curdat;

                //get first value of date
                prevdt = dictSeries.Keys.ElementAt(0);
                prevmon = (new DateTime(prevdt.Year, prevdt.Month, 1)).Date;
                prevyr = prevdt.Year;
                int ncount = dtime.Length;

                for (int i = 0; i < ncount; i++)
                {
                    curdt = dtime[i];
                    curyr = curdt.Year;
                    curdat = dat[i];
                    curmon = (new DateTime(curdt.Year, curdt.Month, 1)).Date;

                    //monthly stats
                    if ((curmon - prevmon).TotalDays > 0)
                    {   //new month
                        mStats = stStats.MonthlyAverage();
                        if (!msiteStats.ContainsKey(prevmon))
                        {
                            if (i != 0)
                                CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        stStats.InitMonthly();
                    }
                    stStats.MonthlySum(curdat);

                    //annual stats
                    if ((curyr - prevyr) > 0)
                    {
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        if (!asiteStats.ContainsKey(dyr))
                            asiteStats.Add(dyr, aStats);
                        stStats.InitAnnual();
                    }
                    stStats.AnnualSum(curdat);

                    prevmon = curmon;
                    prevyr = curyr;

                    if (i == ncount - 1)
                    {
                        //check missing month stats, fill with previous month
                        mStats = stStats.MonthlyAverage();
                        {
                            CheckMissingMonStat(site, prevmon, mStats);
                            msiteStats.Add(prevmon, mStats);
                        }
                        aStats = stStats.AnnualAverage();
                        dyr = (new DateTime(prevyr, 1, 1)).Date;
                        asiteStats.Add(dyr, aStats);
                    }
                }
                //fill in missing statistics for the day
                //FillMissingDlyStats(dsiteStats);

                //estimate missing daily statistics
                //EstimateMissingDailyVarStat(dsiteStats);

                stStats = null;
                dat = null;
                dtime = null;
                curdat = null;
                return true;
            }
            catch (Exception ex)
            {
                //string msg = "Error in calculating series statistics for "+ site +"!";
                //NCEIMsg.ShowError(msg, ex);
                return false;
            }
        }
        private void FillMissingDlyStats(SortedDictionary<DateTime, double> dsitestats)
        {
            try
            {
                DateTime curdt, prevdt;
                double ndays;
                TimeSpan tspan;
                SortedDictionary<DateTime, double> dicMissStat = new SortedDictionary<DateTime, double>();

                prevdt = dsitestats.Keys.ElementAt(0);
                foreach (KeyValuePair<DateTime, double> kv in dsitestats)
                {
                    curdt = kv.Key;
                    tspan = curdt - prevdt;
                    ndays = tspan.TotalDays;

                    //Debug.WriteLine("Processing daily stats {0},{1},{2}", curdt.ToString(), prevdt.ToString(),ndays.ToString());
                    if (ndays > 1)
                    {
                        Debug.WriteLine("Missing daily " + curdt.ToString() + ", " + prevdt.ToString());
                        dicMissStat.Add(prevdt, ndays);
                    }
                    prevdt = curdt;
                }

                foreach (KeyValuePair<DateTime, double> kv in dicMissStat)
                {
                    double nmissdy;
                    dicMissStat.TryGetValue(kv.Key, out nmissdy);
                    //Debug.WriteLine("Inserting Missing Stat {0},{1}", kv.Key.ToString(), nmissdy.ToString());
                    InsertMissingDlyStat(nmissdy, kv.Key, dsitestats);
                }
                dicMissStat = null;
            }
            catch (Exception ex)
            {
                NCEIMsg.ShowError("Error getting missing daily stats!", ex);
            }
        }
        private void InsertMissingDlyStat(double ndays, DateTime prevdt, SortedDictionary<DateTime, double> dsitestats)
        {
            try
            {
                double prevstats;
                dsitestats.TryGetValue(prevdt, out prevstats);
                for (double j = 1; j < ndays; j++)
                {
                    DateTime dt = prevdt.AddDays(j);
                    if (!dsitestats.ContainsKey(dt))
                    {
                        //Debug.WriteLine("Inserting dt=" + dt.ToString());
                        dsitestats.Add(dt, prevstats);
                    }
                }
            }
            catch (Exception ex)
            {
                NCEIMsg.ShowError("Error inserting missing daily stats!", ex);
            }
        }
        private void EstimateMissingDailyVarStat(SortedDictionary<DateTime, double> dsiteStats)
        {
            //estimate missing daily, use previous day
            try
            {
                DateTime dt, prevdt;
                double curStats, prevStats;
                DateTime[] dtime = dsiteStats.Keys.ToArray();

                //get first value of date
                prevdt = dtime[0];
                int ncount = dtime.Length;

                for (int j = 0; j < ncount; j++)
                {
                    dt = dtime[j];
                    if (j == 0)
                        prevdt = dt.AddDays(1);
                    else
                        prevdt = dt.AddDays(-1);

                    dsiteStats.TryGetValue(dt, out curStats);
                    dsiteStats.TryGetValue(prevdt, out prevStats);

                    if (curStats > 9990)
                        curStats = prevStats; //problem here
                }
            }
            catch (Exception ex)
            {
                NCEIMsg.ShowError("Error getting missing daily stats!", ex);
            }
        }
        public SortedDictionary<DateTime, double> DailyStatistics()
        {
            return dsiteStats;
        }
        public SortedDictionary<DateTime, double> MonthlyStatistics()
        {
            return msiteStats;
        }
        public SortedDictionary<DateTime, double> AnnualStatistics()
        {
            return asiteStats;
        }
        private void CheckMissingDailyStat(int iday, string site, DateTime dt, double dStats)
        {
            try
            {
                DateTime prevdt;
                //int nvars = lstSelectedVarsRev.Count;
                if (iday == 0)
                    prevdt = dt.AddDays(1);
                else
                    prevdt = dt.AddDays(-1);

                double prevStats;
                if (!dsiteStats.TryGetValue(prevdt, out prevStats))
                    return;

                if (dStats > 9990)
                    dStats = prevStats; //problem here
            }
            catch (Exception ex)
            {
                string msg = "Error in checking missing daily statistics for " + site + "!";
                NCEIMsg.ShowError(msg, ex);
            }
        }
        private void CheckMissingMonStat(string site, DateTime dt, double mStats)
        {
            try
            {
                int mon = dt.Month;
                DateTime prevmon = dt.AddMonths(-1);
                double prevStats;
                msiteStats.TryGetValue(prevmon, out prevStats);

                if (mStats > 9990)
                {
                    mStats = prevStats;
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in checking missing monthly statistics for " + site + "!";
                NCEIMsg.ShowError(msg, ex);
            }
        }
    }
}
