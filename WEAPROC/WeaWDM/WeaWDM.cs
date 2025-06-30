//#define debug

using atcData;
using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace WeaWDM
{
    public class WDM : atcTimeseriesSource
    {
        private string WDMFile;
        private List<string> lstOfSelectedVars = new List<string>();
        //dictionary of series for each variable, contains gage dictionary keyed on dsn
        private Dictionary<string, SortedDictionary<int, clsStation>> dictSeries =
             new Dictionary<string, SortedDictionary<int, clsStation>>();
        string errmsg; string crlf = Environment.NewLine;
        private DateTime MinDate, MaxDate;
        private string TimeUnit;
        private DataTable tblSeries;
        public enum MetDataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6, EDDE };
        private int optDataset = 0;
        int MetData;

        public WDM(string _WDMFile, List<string> _lstOfSelectedVars)
        {
            WDMFile = _WDMFile;
            lstOfSelectedVars = _lstOfSelectedVars;
            MaxDate = DateTime.Parse("#2100/12/31#");
            MinDate = DateTime.Parse("#1900/01/01#");
        }

        public WDM(string _WDMFile, int MetData)
        {
            WDMFile = _WDMFile;
            this.MetData = MetData;
            Debug.WriteLine("MetData is " + MetData.ToString());
        }

        public WDM(string _WDMFile)
        {
            WDMFile = _WDMFile;
        }

        public bool GetWDMAttributes(string TimeUnit)
        {
            Debug.WriteLine("WeaWDM: Entering GetWDMAttributes");
            Cursor.Current = Cursors.WaitCursor;
            try
            {

                if (!GetDictionaryOfDatasets(TimeUnit))
                    return false;
                //debug
                foreach (KeyValuePair<string, SortedDictionary<int, clsStation>> kv in dictSeries)
                    Debug.WriteLine("WeaWDM: GetWDMAttributes: kv.key in dict=" + kv.Key);
            }
            catch (Exception ex)
            {
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }

        /// <summary>
        /// Get all series in wdm and create dictionary of gages
        /// </summary>
        /// <returns></returns>
        private bool GetDictionaryOfDatasets(string TimeUnit)
        {
            Debug.WriteLine("Entering GetDictionaryOfDatasets");
            DateTime dbeg, dend;
            int dsn;
            string sloc, svar, scen, lat, lon, tunit, stnam, desc, staid, elev;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            SortedDictionary<int, clsStation> dsnGage;

            try
            {
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                Debug.WriteLine("Number of Dataset = " + numdset.ToString());

                if (numdset <= 0) return false;

                int ncnt = 0;
                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    ncnt++;
                    svar = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();
                    desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();
                    stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();
                    staid = lDataSet.Attributes.GetValue("STAID").ToString().Trim();
                    try
                    {
                        if (!(lDataSet.Attributes.GetValue("Elevation") == null))
                            elev = lDataSet.Attributes.GetValue("Elevation").ToString().Trim();
                        else
                            elev = string.Empty;
                    }
                    catch (Exception ex)
                    { elev = string.Empty; }

                    Int32 numval = (Int32)lDataSet.numValues;
                    //series dates
                    dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                    lDataSet.Clear();
                    

                    //add to dictionary of gages, check only period of series
                    //when doing assignment, select only hourly
                    if (lstOfSelectedVars.Contains(svar) && tunit.Contains(TimeUnit))
                    {
                        //compare series min and max date with global min and max
                        if (DateTime.Compare(dbeg, MinDate) >= 0)
                            MinDate = dbeg;
                        if (DateTime.Compare(dend, MaxDate) <= 0)
                            MaxDate = dend;

                        clsStation cMet = new clsStation();
                        cMet.DSN = dsn;
                        cMet.BegDate = dbeg;
                        cMet.EndDate = dend;
                        cMet.Latitude = lat;
                        cMet.Longitude = lon;
                        cMet.Scenario = scen;
                        cMet.Description = desc;
                        cMet.STAID = staid;
                        cMet.Constituent = svar;
                        cMet.Station = sloc;
                        cMet.TimeStep = TimeUnit;

                        Debug.WriteLine("Adding station " + sloc + " with desc of " + desc);

                        //in export table
                        //DSN, Station, StaName,Scenario,COnstituent,Latitude,Longitude,Elevation
                        if (scen.Contains("NLDAS") || scen.Contains("GLDAS") || scen.Contains("TRMM"))
                        {
                            //wdm NLDAS
                            //(dsn, sloc, stnam, scen, svar, lat, lon, elev);
                            cMet.StationName = stnam;
                            cMet.Station = sloc;
                        }
                        else if (desc.Contains("GHCN:") || desc.Contains("COOP:") || desc.Contains("ISD:"))
                        {
                            //wdm is from the tool
                            //(dsn, stnam, desc, scen, svar, lat, lon, elev);
                            cMet.StationName = desc;
                            cMet.Station = stnam;
                        }
                        else if (desc.Contains("CMIP6"))
                        {
                            //wdm contains climate scenario
                            //(dsn, stnam, desc, scen, svar, lat, lon, elev);
                            cMet.StationName = desc;
                            cMet.Station = sloc;
                        }
                        else if (desc.Contains("EDDE"))
                        {
                            //wdm contains climate scenario
                            //(dsn, stnam, desc, scen, svar, lat, lon, elev);
                            cMet.StationName = sloc;
                            cMet.Station = sloc;
                        }
                        else
                        {
                            //wdm is from BASINS
                            //(dsn, sloc, stnam, scen, svar, lat, lon, elev);
                            cMet.StationName = stnam;
                            cMet.Station = sloc;
                        }

                        //cMet.LonPrj = pts[0];
                        //cMet.LatPrj = pts[1];

                        if (!dictSeries.ContainsKey(svar))
                        {
                            dsnGage = new SortedDictionary<int, clsStation>();
                            dsnGage.Add(dsn, cMet);
                            dictSeries.Add(svar, dsnGage);
                        }
                        else
                        {
                            dictSeries.TryGetValue(svar, out dsnGage);
                            dsnGage.Add(dsn, cMet);
                        }
                        cMet = null;
                    }
                }
                lwdm.Clear();
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            dsnGage = null;
            return true;
        }
        public DataTable GetWDMAllSeries()
        {
            Debug.WriteLine("Entering GetWDMAttributes");
            Cursor.Current = Cursors.WaitCursor;
            try
            {

                if (!GetTableOfAllDatasets())
                    return null;
                //debug
                //foreach (KeyValuePair<string, SortedDictionary<int, clsStation>> kv in dictSeries)
                //    Debug.WriteLine("GetWDMAllSeies: kv.key in dict=" + kv.Key);
            }
            catch (Exception ex)
            {
                return null;
            }
            Cursor.Current = Cursors.Default;
            return tblSeries;
        }
        public DataTable GetWDMAllSeries(List<string> timeunit)
        {
            Debug.WriteLine("Entering GetWDMAttributes for Year/Month");
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!GetTableOfAllDatasets(timeunit))
                    return null;
                //debug
                //foreach (KeyValuePair<string, SortedDictionary<int, clsStation>> kv in dictSeries)
                //    Debug.WriteLine("GetWDMAllSeies: kv.key in dict=" + kv.Key);
            }
            catch (Exception ex)
            {
                return null;
            }
            Cursor.Current = Cursors.Default;
            return tblSeries;
        }
        private bool GetTableOfAllDatasets(List<string> timeunit)
        {
            Debug.WriteLine("Entering GetTableOfAllDatasets for Year");
            int dsn;
            string sloc, svar, scen, lat, lon, tunit, desc, stnam, elev;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            DateTime sdate, edate;

            CreateTableOfWDMSeries();

            try
            {
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    atcDataAttributes attrib = new atcDataAttributes();
                    attrib = lDataSet.Attributes;
                    svar = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();
                    stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    //sdate = (DateTime)lDataSet.Attributes.GetValue("Start Date");
                    //edate = (DateTime)lDataSet.Attributes.GetValue("End Date");
                    //sdate = (DateTime) attrib.GetValue("Start Date");
                    //edate = (DateTime)attrib.GetValue("End Date");
                    int num = (int)lDataSet.numValues;
                    //sdate = DateTime.FromOADate(lDataSet.Dates.Values.First());
                    //edate = DateTime.FromOADate(lDataSet.Dates.Values.Last());
                    sdate = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    edate = DateTime.FromOADate(lDataSet.Dates.Values[num-1]);
//#if debug
                    Debug.WriteLine("Numvalues = "+ num.ToString());
                    Debug.WriteLine("{0},{1},{2},{3},{4},{5}", stnam,svar,sdate.Year.ToString(), edate.Year.ToString(),
                    sdate.ToString(), edate.ToString());
                    //for (int ii= 0; ii < num; ii++)
                    {
                        //DateTime dt = DateTime.FromOADate(lDataSet.Dates.Values[ii]);
                        //double val = lDataSet.Values[ii+1];
                        //Debug.WriteLine("{0},{1}",dt.ToString(),val.ToString("F2"));
                    }                    
//#endif 
                    try
                    {
                        if ((elev = lDataSet.Attributes.GetValue("Elevation").ToString().Trim()) == string.Empty)
                            elev = string.Empty;
                    }
                    catch (Exception ex)
                    { elev = string.Empty; }
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();

                    Int32 numval = (Int32)lDataSet.numValues;
                    lDataSet.Clear();
                    //datarow- dsn,sloc,stnam,scen,svar,lat,lon,elev
                    //columns-DSN,Station,StaName,Scenario,COnstituent,Latitude,Longitude,Elevation
                    //only add if timeunit is annual
                    //if (tunit.Contains("Year") || tunit.Contains("Month"))
                    if (tunit.Contains("Year"))
                    {
                        if (scen.Contains("NLDAS") || scen.Contains("GLDAS"))
                            tblSeries.Rows.Add(dsn, sloc, stnam, scen, svar, lat, lon, elev,tunit,sdate,edate);
                        //wdm is from the tool
                        else if (desc.Contains("GHCN:") || desc.Contains("COOP:") || desc.Contains("ISD:"))
                            tblSeries.Rows.Add(dsn, stnam, desc, scen, svar, lat, lon, elev, tunit, sdate, edate);
                        //wdm is from BASINS
                        else
                            tblSeries.Rows.Add(dsn, sloc, stnam, scen, svar, lat, lon, elev, tunit, sdate, edate);
                    }
                }
                lwdm.Clear();
            }
            catch (Exception ex)
            {   
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            return true;
        }
        private bool GetTableOfAllDatasets()
        {
            Debug.WriteLine("Entering GetTableOfAllDatasets");
            int dsn;
            string sloc, svar, scen, lat, lon, tunit, desc, stnam, elev;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();

            CreateTableOfWDMSeries();

            try
            {
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    svar = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();
                    stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    try
                    {
                        if ((elev = lDataSet.Attributes.GetValue("Elevation").ToString().Trim()) == string.Empty)
                            elev = string.Empty;
                    }
                    catch (Exception ex)
                    { elev = string.Empty; }
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();

                    Int32 numval = (Int32)lDataSet.numValues;
                    lDataSet.Clear();
                    //datarow- dsn,sloc,stnam,scen,svar,lat,lon,elev
                    //columns-DSN,Station,StaName,Scenario,COnstituent,Latitude,Longitude,Elevation
                    if (scen.Contains("NLDAS") || scen.Contains("GLDAS"))
                        tblSeries.Rows.Add(dsn, sloc, stnam, scen, svar, lat, lon, elev);
                    //wdm is from the tool
                    else if (desc.Contains("GHCN:") || desc.Contains("COOP:") || desc.Contains("ISD:"))
                        tblSeries.Rows.Add(dsn, stnam, desc, scen, svar, lat, lon, elev);
                    //wdm is from BASINS
                    else
                        tblSeries.Rows.Add(dsn, sloc, stnam, scen, svar, lat, lon, elev);
                }
                lwdm.Clear();
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            return true;
        }
        private bool CreateTableOfWDMSeries()
        {
            try
            {
                tblSeries = new DataTable();
                tblSeries.Columns.Add("DSN", typeof(string));
                tblSeries.Columns.Add("Station", typeof(string));
                tblSeries.Columns.Add("StaName", typeof(string));
                tblSeries.Columns.Add("Scenario", typeof(string));
                tblSeries.Columns.Add("Constituent", typeof(string));
                tblSeries.Columns.Add("Latitude", typeof(string));
                tblSeries.Columns.Add("Longitude", typeof(string));
                tblSeries.Columns.Add("Elevation", typeof(string));
                tblSeries.Columns.Add("TimeUnit", typeof(string));
                tblSeries.Columns.Add("StartDate", typeof(DateTime));
                tblSeries.Columns.Add("EndDate", typeof(DateTime));
                tblSeries.TableName = "dtAllSeries";
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public clsStation GetTimeSeriesInfo(string svar, string loc)
        {
            Debug.WriteLine("Entering GetTimeSeriesInfo");
            DateTime dbeg, dend;
            int dsn;
            string sloc, scen, lat, lon, tunit, cons, stnam;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            clsStation sta = new clsStation();

            try
            {
                if (!lwdm.Open(WDMFile))
                {
                    return null;
                }

                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0)
                {
                    return null;
                }

                int ncnt = 0;
                clsStation cMet = new clsStation();
                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    ncnt++;
                    cons = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();
                    stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();

                    Int32 numval = (Int32)lDataSet.numValues;
                    dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                    lDataSet.Clear();

                    //add to dictionary of gages, check only period of series
                    //when doing assignment, select only hourly
                    if (svar.Contains(cons) && loc.Contains(stnam))
                    {
                        cMet.Station = stnam;
                        cMet.DSN = dsn;
                        cMet.BegDate = dbeg;
                        cMet.EndDate = dend;
                        cMet.Latitude = lat;
                        cMet.Longitude = lon;
                        cMet.Scenario = scen;
                        cMet.Constituent = cons;
                        break;
                    }
                }
                lwdm.Clear();
                return cMet;
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return null;
            }
        }
        public clsStation GetSiteInfo(string site)
        {
            Debug.WriteLine("Entering GetSiteInfo");
            DateTime dbeg, dend;
            int dsn;
            string sloc, scen, lat, lon, tunit, cons, stnam, elv, desc;
            string siteloc = string.Empty, sitename = string.Empty;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            clsStation sta = new clsStation();

            try
            {
                if (!lwdm.Open(WDMFile)) { return null; }

                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) { return null; }

                int ncnt = 0;
                clsStation cMet = new clsStation();

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    ncnt++;
                    cons = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    if (!(lDataSet.Attributes.GetValue("Elevation") == null))
                        elv = lDataSet.Attributes.GetValue("Elevation").ToString();
                    else
                        elv = "0";
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();
                    stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();
                    desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();

                    Int32 numval = (Int32)lDataSet.numValues;
                    dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                    lDataSet.Clear();

                    //add to dictionary of gages, check only period of series
                    //when doing assignment, select only hourly
                    //get first site 
                    Debug.WriteLine("MetData is " + MetData.ToString());
                    switch (MetData)
                    {
                        case (int)MetDataSource.NLDAS:
                            siteloc = site;
                            sitename = site;
                            break;
                        case (int)MetDataSource.GLDAS:
                            siteloc = site;
                            sitename = site;
                            break;
                        case (int)MetDataSource.TRMM:
                            siteloc = site;
                            sitename = site;
                            break;
                        case (int)MetDataSource.ISD:
                            siteloc = stnam;
                            sitename = desc;
                            break;
                        case (int)MetDataSource.GHCN:
                            siteloc = stnam;
                            sitename = desc;
                            break;
                        case (int)MetDataSource.HRAIN:
                            siteloc = stnam;
                            sitename = desc;
                            break;
                    }
                    //Debug.WriteLine("After switch: gage={0},site={1},name={2}", site, siteloc, sitename);

                    if (site.Contains(siteloc))
                    {
                        cMet.Station = siteloc;
                        cMet.StationName = sitename;
                        cMet.DSN = dsn;
                        cMet.BegDate = dbeg;
                        cMet.EndDate = dend;
                        cMet.Latitude = lat;
                        cMet.Longitude = lon;
                        cMet.Elevation = elv;
                        cMet.Scenario = scen;
                        cMet.Constituent = cons;
                        Debug.WriteLine("{0},{1},{2},{3},{4}", siteloc, sitename, lat, lon, elv);
                        break;
                    }
                }
                lwdm.Clear();
                return cMet;
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return null;
            }
        }
        public clsStation GetSiteInfoByDSN(int dsn)
        {
            Debug.WriteLine("Entering GetSiteInfo");
            DateTime dbeg, dend;
            string sloc, scen, lat, lon, tunit, cons, stnam, elv, desc;
            string siteloc = string.Empty, sitename = string.Empty;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            clsStation sta = new clsStation();

            try
            {
                if (!lwdm.Open(WDMFile)) { return null; }

                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) { return null; }

                clsStation cMet = new clsStation();
                atcTimeseries lDataSet = lwdm.DataSets.FindData("ID", Convert.ToString(dsn))[0];

                //foreach (atcTimeseries lDataSet in lwdm.DataSets)
                cons = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                if (!(lDataSet.Attributes.GetValue("Elevation") == null))
                    elv = lDataSet.Attributes.GetValue("Elevation").ToString();
                else
                    elv = "0";
                tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();
                stnam = lDataSet.Attributes.GetValue("StaNam").ToString().Trim();
                desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();

                Int32 numval = (Int32)lDataSet.numValues;
                dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                lDataSet.Clear();

                //add to dictionary of gages, check only period of series
                Debug.WriteLine("MetData is " + MetData.ToString());
                switch (MetData)
                {
                    case (int)MetDataSource.NLDAS:
                        siteloc = sloc;
                        sitename = sloc;
                        break;
                    case (int)MetDataSource.GLDAS:
                        siteloc = sloc;
                        sitename = sloc;
                        break;
                    case (int)MetDataSource.TRMM:
                        siteloc = sloc;
                        sitename = sloc;
                        break;
                    case (int)MetDataSource.ISD:
                        siteloc = stnam;
                        sitename = desc;
                        break;
                    case (int)MetDataSource.GHCN:
                        siteloc = stnam;
                        sitename = desc;
                        break;
                    case (int)MetDataSource.HRAIN:
                        siteloc = stnam;
                        sitename = desc;
                        break;
                }
                //Debug.WriteLine("After switch: gage={0},site={1},name={2}", site, siteloc, sitename);

                //if (site.Contains(siteloc))
                cMet.Station = siteloc;
                cMet.StationName = sitename;
                cMet.DSN = dsn;
                cMet.BegDate = dbeg;
                cMet.EndDate = dend;
                cMet.Latitude = lat;
                cMet.Longitude = lon;
                cMet.Elevation = elv;
                cMet.Scenario = scen;
                cMet.Constituent = cons;
                Debug.WriteLine("{0},{1},{2},{3},{4}", siteloc, sitename, lat, lon, elv);
                lwdm.Clear();
                lwdm = null;
                return cMet;
            }
            catch (Exception ex)
            {
                errmsg = "Error in getting timeseries attributes!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return null;
            }
        }
        /// <summary>
        /// GetTimeSeries, get annual series from annual wdm, needs dsn
        /// </summary>
        /// <param name="dsn"></param>
        /// <returns></returns>
        public SortedDictionary<DateTime, double> GetTimeSeries(int dsn)
        {
            Debug.WriteLine("Entering GetTimeSeries ...");
            SortedDictionary<DateTime, double> dSeries = new SortedDictionary<DateTime, double>();
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            try
            {
                if (!lwdm.Open(WDMFile)) { return null; }
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) { return null; }

                atcTimeseries ltseries = lwdm.DataSets.FindData("ID", Convert.ToString(dsn))[0];
                for (int j = 0; j < ltseries.numValues; j++)
                {
                    DateTime dt = DateTime.FromOADate(ltseries.Dates.Values[j]);
                    double val = ltseries.Values[j+1];
                    dSeries.Add(dt, val);
                    //if (j < 5 || j > ltseries.numValues-10)
                    Debug.WriteLine("In GetTimeSeries: {0},{1},{2}", dt.ToString(), dt.Year.ToString(), val.ToString("F5"));
                }
                ltseries = null;
                //For j As Integer = 0 To lseries.numValues - 1
                //Dim ddate As Date = Date.FromOADate(lseries.Dates.Value(j))
                //'Dim val As Double = lseries.Value(j + 1) 'per email from Mark 12 / 6 / 07 need to use next index
                //'revise 07/28/2020
                //Dim val As Double = lseries.Value(j)
                //Dim svalue As String = String.Empty
                //If Not(Double.IsNaN(val) Or Double.IsInfinity(val)) Then
                //End If
                //Next
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return null;
            }
            lwdm = null;
            return dSeries;
        }
        public SortedDictionary<int, double> GetTimeSeriesByDSN(int dsn)
        {
            SortedDictionary<int, double> dSeries = new SortedDictionary<int, double>();
            atcWDM.atcDataSourceWDM lwdmn = new atcWDM.atcDataSourceWDM();
            try
            {
                if (!lwdmn.Open(WDMFile)) { return null; }
                int numdset = lwdmn.DataSets.Count;
                if (numdset <= 0) { return null; }

                atcTimeseries ltseries = lwdmn.DataSets.FindData("ID", Convert.ToString(dsn))[0];
                for (int j = 1; j <= ltseries.numValues; j++)
                {
                    DateTime dt = DateTime.FromOADate(ltseries.Dates.Values[j]);
                    int yr = dt.Year;
                    double val = ltseries.Values[j];
                    dSeries.Add(yr, val);
                    //if (j < 10 || j > ltseries.numValues-10)
                    //Debug.WriteLine("{0},{1}", dt.ToString(), val.ToString("F5"));
                }
                ltseries = null;
                //For j As Integer = 0 To lseries.numValues - 1
                //Dim ddate As Date = Date.FromOADate(lseries.Dates.Value(j))
                //'Dim val As Double = lseries.Value(j + 1) 'per email from Mark 12 / 6 / 07 need to use next index
                //'revise 07/28/2020
                //Dim val As Double = lseries.Value(j)
                //Dim svalue As String = String.Empty
                //If Not(Double.IsNaN(val) Or Double.IsInfinity(val)) Then
                //End If
                //Next
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return null;
            }
            lwdmn = null;
            return dSeries;
        }
        public bool HasLDAS()
        {
            bool isLDAS = false;
            try
            {
                atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    string scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    if (scen.Contains("NLDAS") || scen.Contains("GLDAS"))
                    {
                        isLDAS = true;
                        continue;
                    }
                }
                lwdm.Clear();
                lwdm = null;
            }
            catch (Exception ex)
            {
                return false;
            }
            return isLDAS;
        }

        public bool HasCMIP()
        {
            bool isCMIP = false;
            try
            {
                atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    string desc = lDataSet.Attributes.GetValue("Description").ToString().Trim();
                    if (desc.Contains("CMIP6"))
                    {
                        isCMIP = true;
                        continue;
                    }
                }
                lwdm.Clear();
                lwdm = null;
            }
            catch (Exception ex)
            {
                return false;
            }
            return isCMIP;
        }
        public bool WriteWDMDataSet()
        {
            try
            {
                atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    string scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                }
                lwdm.Clear();
                lwdm = null;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool GetDictionaryOfDatasetsWithScenario(string TimeUnit, string scenario)
        {
            Debug.WriteLine("Entering GetDictionaryOfDatasets");
            DateTime dbeg, dend;
            int dsn;
            string sloc, svar, scen, lat, lon, tunit;
            atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            SortedDictionary<int, clsStation> dsnGage;

            try
            {
                if (!lwdm.Open(WDMFile)) return false;
                int numdset = lwdm.DataSets.Count;
                if (numdset <= 0) return false;

                int ncnt = 0;
                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    ncnt++;
                    svar = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    scen = lDataSet.Attributes.GetValue("Scenario").ToString().Trim();
                    lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim();
                    lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();
                    tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();

                    Int32 numval = (Int32)lDataSet.numValues;
                    dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                    lDataSet.Clear();

                    if (DateTime.Compare(dbeg, MinDate) >= 0)
                        MinDate = dbeg;
                    if (DateTime.Compare(dend, MaxDate) <= 0)
                        MaxDate = dend;

                    //add to dictionary of gages, check only period of series
                    //when doing assignment, select only TimeUnit
                    if (lstOfSelectedVars.Contains(svar) && tunit.Contains(TimeUnit) && scen.Contains(scenario))
                    {
                        clsStation cMet = new clsStation();
                        cMet.Station = sloc;
                        cMet.DSN = dsn;
                        cMet.BegDate = dbeg;
                        cMet.EndDate = dend;
                        cMet.Latitude = lat;
                        cMet.Longitude = lon;
                        cMet.Scenario = scen;
                        //cMet.LonPrj = pts[0];
                        //cMet.LatPrj = pts[1];

                        if (!dictSeries.ContainsKey(svar))
                        {
                            dsnGage = new SortedDictionary<int, clsStation>();
                            dsnGage.Add(dsn, cMet);
                            dictSeries.Add(svar, dsnGage);
                        }
                        else
                        {
                            dictSeries.TryGetValue(svar, out dsnGage);
                            dsnGage.Add(dsn, cMet);
                        }
                        cMet = null;
                    }
                }
                lwdm.Clear();
            }
            catch (Exception ex)
            {
                errmsg = "Error in reading wdmfile!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            dsnGage = null;
            return true;
        }
        public List<int> SelectedVarsDSN(string svar)
        {
            SortedDictionary<int, clsStation> dsnGage;

            if (dictSeries.TryGetValue(svar, out dsnGage))
                return (dsnGage.Keys.ToList());
            else
                return null;
        }
        public DateTime SeriesMinDate()
        {
            return MinDate;
        }
        public DateTime SeriesMaxDate()
        {
            return MaxDate;
        }
        public Dictionary<string, SortedDictionary<int, clsStation>> dictWDMSeries()
        {
            return dictSeries;
        }
        private void WriteMessage(string msgtype, string msg)
        {
            switch (msgtype)
            {
                case "Error!":
                    MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "Warning!":
                    MessageBox.Show(msg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "Info!":
                    MessageBox.Show(msg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
