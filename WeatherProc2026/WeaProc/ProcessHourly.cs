public bool ProcessHourlyDataOLD()
{
    try
    {
        Cursor.Current = Cursors.WaitCursor;
        DateTime dtbeg = DateTime.Now;
        fMain.WriteLogFile("Begin Processing Hourly Data :" + DateTime.Now.ToShortDateString() + " " +
                            DateTime.Now.ToLongTimeString());
        dtbeg = DateTime.Now;

        List<clsISDdata> rawData;
        SortedDictionary<DateTime, clsISDdata> dicHlyData;
        SortedDictionary<DateTime, clsISDdata> dicMissing;
        SortedDictionary<DateTime, List<double>> dsiteStats; //daily
        SortedDictionary<DateTime, List<double>> msiteStats; //monthly
        SortedDictionary<DateTime, List<double>> asiteStats; //annual
        DateTime curdt, prevdt, curday, prevday, curmon, prevmon, dyr;
        int curyr, prevyr;
        TimeSpan td;
        double nhrs = 0;
        int ilast = 0;

        // process each station
        foreach (string site in lstStaDownloaded)
        {
            fMain.WriteLogFile("Processing Hourly Data for station  " + site + ".");
            fMain.appManager.UpdateProgress("Processing Hourly Data for station  " + site + ".");

            dicHlyData = new SortedDictionary<DateTime, clsISDdata>();
            dsiteStats = new SortedDictionary<DateTime, List<double>>();
            msiteStats = new SortedDictionary<DateTime, List<double>>();
            asiteStats = new SortedDictionary<DateTime, List<double>>();
            dicMissing = new SortedDictionary<DateTime, clsISDdata>();

            //clsStats stStats = new clsStats(lstSelectedVars, dsiteStats);
            clsStats stStats = new clsStats(lstSelectedVars);
            List<double> dStats = new List<double>();
            List<double> mStats = new List<double>();
            List<double> aStats = new List<double>();

            //series for site
            rawData = new List<clsISDdata>();
            dictISDdata.TryGetValue(site, out rawData);

            ilast = rawData.Count;
            prevdt = GetDateTime(rawData.ElementAt(0).Date);
            //date only
            prevday = prevdt.Date;
            prevmon = (new DateTime(prevday.Year, prevday.Month, 1)).Date;
            prevyr = prevdt.Year;

            foreach (var dat in rawData)
            {
                curdt = GetDateTime(dat.Date);
                curday = curdt.Date;
                curmon = (new DateTime(curday.Year, curday.Month, 1)).Date;
                curyr = curdt.Year;

                if (!dicHlyData.ContainsKey(curdt))
                {
                    dat.Date = curdt.ToString();
                    dicHlyData.Add(curdt, dat);
                    //check missing
                    if (CheckMissingData(dat))
                    {
                        if (!dicMissing.ContainsKey(curdt))
                            dicMissing.Add(curdt, dat);
                    }
                }

                // daily stats
                if ((curday - prevday).TotalHours > 0)
                {   //new day
                    dStats = stStats.DailyAverage();
                    if (!dsiteStats.ContainsKey(prevday))
                    {
                        //check missing day stats, fill with previous day, OK
                        CheckMissingDailyStat(prevday, dStats, dsiteStats);
                        dsiteStats.Add(prevday, dStats);
                    }
                    stStats.InitDaily();
                    stStats.DailySum(dat);
                }
                else
                {
                    stStats.DailySum(dat);
                }

                //monthly stats
                if ((curmon - prevmon).TotalHours > 0)
                {   //new day
                    mStats = stStats.MonthlyAverage();
                    if (!msiteStats.ContainsKey(prevmon))
                    {
                        CheckMissingMonStat(prevmon, mStats, msiteStats);
                        msiteStats.Add(prevmon, mStats);
                    }

                    stStats.InitMonthly();
                    stStats.MonthlySum(dat);
                }
                else
                {
                    stStats.MonthlySum(dat);
                }

                //annual stats
                if ((curyr - prevyr) > 0)
                {
                    aStats = stStats.AnnualAverage();
                    dyr = (new DateTime(prevyr, 1, 1)).Date;
                    if (!asiteStats.ContainsKey(dyr))
                        asiteStats.Add(dyr, aStats);

                    stStats.InitAnnual();
                    stStats.AnnualSum(dat);
                }
                else
                {
                    stStats.AnnualSum(dat);
                }

                prevday = curday;
                prevmon = curmon;
                prevyr = curyr;

                //check for missing hours since last record
                td = curdt - prevdt;
                if ((nhrs = td.TotalHours) > 1)
                    InsertMissingHours(dicHlyData, dicMissing, site, prevdt, nhrs);
                prevdt = curdt;
            } //end of rawdata loop

            //last day
            dStats = stStats.DailyAverage();
            if (!dsiteStats.ContainsKey(prevday))
            {
                //check missing day stats, fill with previous day
                CheckMissingDailyStat(prevday, dStats, dsiteStats);
                dsiteStats.Add(prevday, dStats);
            }

            //check missing month stats, fill with previous month
            mStats = stStats.MonthlyAverage();
            {
                CheckMissingMonStat(prevmon, mStats, msiteStats);
                msiteStats.Add(prevmon, mStats);
            }
            aStats = stStats.AnnualAverage();
            dyr = (new DateTime(prevyr, 1, 1)).Date;
            asiteStats.Add(dyr, aStats);

            CountMissingHours(site, dicMissing, 0);
            dictMISS.Add(site, dicMissing);
            dictData.Add(site, dicHlyData);

            //stats for the site
            stStats.FillMissingDlyStats(dsiteStats);
            dlyStats.Add(site, dsiteStats);
            monStats.Add(site, msiteStats);
            annStats.Add(site, asiteStats);

            dsiteStats = null;
            msiteStats = null;
            asiteStats = null;
            rawData = null;
            dicHlyData = null;
            dicMissing = null;
        } //end station loop

        td = DateTime.Now - dtbeg;
        fMain.WriteLogFile("End Processing Hourly Data for station: " +
            DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
            td.TotalMinutes.ToString("F4") + " minutes.");
        fMain.appManager.UpdateProgress("Ready ...");
        //WriteStatus();

        Cursor.Current = Cursors.Default;
        return true;
    }
    catch (Exception ex)
    {
        string msg = "Error processing hourly data!";
        ShowError(msg, ex);
        return false;
    }
}
