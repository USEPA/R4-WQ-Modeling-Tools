using atcData;
using DotSpatial.Controls;
using DotSpatial.Projections;
using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WeaWASP
{
    class clsWASP
    {
        private string WDMFile, WDMFileName;
        private IMap appMap;
        private ProjectionInfo mapProjection;
        //dictionary of gages for each variable, contains gage dictionary keyed on dsn
        //contains entries for lstvars
        private Dictionary<string, SortedDictionary<int, clsStation>> dictGages;
        //dictionary of basins/point centroid
        private SortedDictionary<int, CPoint> BasinCentroid;
        private DataTable MetTable;
        private List<string> WASPVars;
        private frmWASP fWASP;
        string errmsg = string.Empty;
        string crlf = Environment.NewLine;
        DateTime MinDate, MaxDate;
        DateTime SimBegDate, SimEndDate;
        private Dictionary<string, CPoint> dictPoints = new Dictionary<string, CPoint>();
        public clsWASP(frmWASP _fWASP, IMap _map, string _wdmFile)
        {
            WDMFile = _wdmFile;
            appMap = _map;
            fWASP = _fWASP;
            dictGages = fWASP.dictGages;
            dictPoints = fWASP.dictPoints;
            WASPVars = fWASP.WASPVars;

            //initialize dictionaries of gages and basin centroid
            //dictGages = new Dictionary<string, SortedDictionary<int, clsStation>>();
            BasinCentroid = new SortedDictionary<int, CPoint>();

            mapProjection = appMap.Projection;
            WDMFileName = Path.GetFileName(WDMFile);

            //init dates
            //MinDate = fWASP.dtBegDate.MinDate;
            //MaxDate = fWASP.dtEndDate.MaxDate;
            SimBegDate = fWASP.dtBegDate.Value;
            SimEndDate = fWASP.dtEndDate.Value;

            InitializeMetTable();
        }
        private void InitializeMetTable()
        {
            //mettable is Location, Lat, Lon, Variables
            MetTable = new DataTable();
            MetTable.Columns.Add("Location", typeof(string));
            MetTable.Columns.Add("Latitude", typeof(string));
            MetTable.Columns.Add("Longitude", typeof(string));
            foreach (string s in WASPVars)
                MetTable.Columns.Add(s, typeof(string));
        }
        public bool GetSubbasinCentroid()
        {
            Debug.WriteLine("Entering GetSubbasinCentroid");
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < dictPoints.Count; i++)
                {
                    CPoint pt = dictPoints.Values.ElementAt(i);
                    BasinCentroid.Add(Convert.ToInt32(i + 1), pt);
                    pt = null;
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error in setting selected point!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }
        private bool GetDictionaryOfDatasets()
        {
            Debug.WriteLine("Entering GetDictionaryOfDatasets");
            DateTime dbeg, dend;
            int dsn;
            string sloc, svar, scen, lat, lon, tunit;
            atcWDM.atcDataSourceWDM lwdm;
            var projFrom = KnownCoordinateSystems.Geographic.World.WGS1984;
            var projTo = KnownCoordinateSystems.Projected.World.WebMercator;

            SortedDictionary<int, clsStation> dsnGage;

            //get the dates again if changed
            SimBegDate = fWASP.dtBegDate.Value;
            SimEndDate = fWASP.dtEndDate.Value;

            try
            {
                lwdm = new atcWDM.atcDataSourceWDM();
                if (!lwdm.Open(WDMFile)) return false;

                int ncnt = 0;
                int numdset = lwdm.DataSets.Count;
                foreach (atcTimeseries lDataSet in lwdm.DataSets)
                {
                    ncnt++;
                    sloc = lDataSet.Attributes.GetValue("Location").ToString().Trim();
                    dsn = (int)lDataSet.Attributes.GetValue("ID");
                    svar = lDataSet.Attributes.GetValue("Constituent").ToString().Trim();
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

                    //reproject lat-lon to mercator----
                    List<double> lpts = new List<double>() { Convert.ToDouble(lon), Convert.ToDouble(lat) };
                    double[] pts = lpts.ToArray();
                    var z = new double[pts.Count() / 2];
                    Reproject.ReprojectPoints(pts, z, projFrom, projTo, 0, pts.Length / 2);
                    lpts = null;
                    //---------------------------------

                    fWASP.WriteStatus("Reading attributes of " + sloc + ":" +
                        svar + "(" + ncnt.ToString() + " of " + numdset.ToString() + ")");

                    //add to dictionary of gages, check only period of series
                    //when doing assignment, select only hourly
                    if (WASPVars.Contains(svar) && tunit.Contains("Hour"))
                    {
                        clsStation cMet = new clsStation();
                        cMet.Station = sloc;
                        cMet.DSN = dsn;
                        cMet.BegDate = dbeg;
                        cMet.EndDate = dend;
                        cMet.Latitude = lat;
                        cMet.Longitude = lon;
                        cMet.Scenario = scen;
                        cMet.LonPrj = pts[0];
                        cMet.LatPrj = pts[1];

                        if (!dictGages.ContainsKey(svar))
                        {
                            dsnGage = new SortedDictionary<int, clsStation>();
                            dsnGage.Add(dsn, cMet);
                            dictGages.Add(svar, dsnGage);
                        }
                        else
                        {
                            dictGages.TryGetValue(svar, out dsnGage);
                            dsnGage.Add(dsn, cMet);
                        }
                        cMet = null;
                    }
                }
                lwdm.Clear();
                fWASP.WriteStatus("Ready ...");
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
        private List<string> GetListOfVarsDSN(string svar)
        {
            Debug.WriteLine("Entering GetListOfVars");
            string sta;
            int dsn;
            DateTime dtbeg, dtend;
            List<string> lstDSN = new List<string>();
            SortedDictionary<int, clsStation> dsnGage = new SortedDictionary<int, clsStation>();

            if (!dictGages.ContainsKey(svar)) return lstDSN;

            dictGages.TryGetValue(svar, out dsnGage);
            foreach (KeyValuePair<int, clsStation> kv in dsnGage)
            {
                dsn = kv.Key;
                sta = kv.Value.Station.ToString();
                //Debug.WriteLine("{0}, {1}, {2}", svar, kv.Key.ToString(), sta);
                //check dates of series
                dtbeg = kv.Value.BegDate;
                dtend = kv.Value.EndDate;
                if (DateTime.Compare(dtbeg, SimBegDate) <= 0 && DateTime.Compare(dtend, SimEndDate) >= 0)
                    lstDSN.Add(sta + ":" + dsn.ToString());
            }
            dsnGage = null;
            return lstDSN;
        }
        public bool AssignMetVariableByBasin()
        {
            Debug.WriteLine("Entering AssignMetVariablesByBasin");
            try
            {
                Dictionary<string, string> dictDsn;
                int nbasin = BasinCentroid.Count;
                int icount = 0;

                //dict key is subbasin, value is centroid coordinates
                foreach (KeyValuePair<int, CPoint> kv in BasinCentroid)
                {
                    icount++;
                    int dsn = 0;
                    int basin = kv.Key;
                    string gage;
                    CPoint cpt = kv.Value;
                    dictDsn = new Dictionary<string, string>();

                    fWASP.WriteStatus("Assigning met stations to selected location " + basin.ToString() + "(" +
                        icount.ToString() + " of " + nbasin.ToString() + ")");
                    foreach (string svar in WASPVars)
                    {
                        if ((dsn = AssignMetStationVar(svar, cpt)) > 0)
                        {
                            clsStation cSta = new clsStation();
                            cSta = GetStationInfo(svar, dsn);
                            gage = cSta.Station;
                            dictDsn.Add(svar, gage + ":" + dsn.ToString());
                            cSta = null;
                        }
                    }

                    if (dictDsn.Count > 0)
                    {
                        //add to datarows of MetTable
                        DataRow dr = MetTable.NewRow();
                        foreach (string svar in WASPVars)
                        {
                            dictDsn.TryGetValue(svar, out gage);
                            dr[svar] = gage;
                        }
                        dr["Location"] = basin.ToString();
                        dr["Latitude"] = cpt.Y.ToString("0.0000");
                        dr["Longitude"] = cpt.X.ToString("0.0000");
                        MetTable.Rows.Add(dr);
                        dr = null;
                        dictDsn = null;
                        cpt = null;
                    }
                }
                fWASP.WriteStatus("Ready ...");
                if (MetTable.Rows.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                errmsg = "Error in assigning met varible!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }

            //debug
            //foreach (DataRow drow in MetTable.Rows)
            //{
            //    Debug.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}",
            //        drow.ItemArray[0].ToString(),drow.ItemArray[1].ToString(),drow.ItemArray[2].ToString(),
            //        drow.ItemArray[3].ToString(),drow.ItemArray[4].ToString(),drow.ItemArray[5].ToString(),
            //        drow.ItemArray[6].ToString(),drow.ItemArray[7].ToString());
            //}
        }

        /// <summary>
        /// AssignMetStationVar
        /// Gets station for each variable nearest the subbasin centroid
        /// </summary>
        /// <param name="svar"></param>
        /// variable
        /// <param name="cpt"></param>
        /// subbasin centroid
        /// <returns></returns>
        private int AssignMetStationVar(string svar, CPoint cpt)
        {
            Debug.WriteLine("AssignMetStationVar");
            List<string> lstDSN = new List<string>();
            double mindist = 9999999999999;
            int mxDSN = 0;
            string mxSta = string.Empty;
            double dist = 0.0;
            string sta = string.Empty;
            double x = 0.0, y = 0.0;

            try
            {
                //get list of stations for indicated variable svar
                //if list is null return 
                lstDSN = GetListOfVarsDSN(svar);
                if (lstDSN.Count > 0)
                {
                    foreach (var str in lstDSN)
                    {
                        string[] arr = str.Split(':');
                        int dsn = Convert.ToInt32(arr[1]);
                        sta = arr[0];
                        //Debug.WriteLine("svar=" + svar + ", sta=" + sta + ", dsn=" + dsn.ToString());

                        //get station info for given dsn and variable
                        clsStation cSta = GetStationInfo(svar, dsn);

                        //reproject lat-lon to mercator----
                        var projFrom = KnownCoordinateSystems.Geographic.World.WGS1984;
                        var projTo = KnownCoordinateSystems.Projected.World.WebMercator;
                        List<double> lpts = new List<double>()
                            {Convert.ToDouble(cSta.Longitude), Convert.ToDouble(cSta.Latitude)};
                        double[] pts = lpts.ToArray();
                        var z = new double[pts.Count() / 2];
                        Reproject.ReprojectPoints(pts, z, projFrom, projTo, 0, pts.Length / 2);
                        lpts = null;
                        //---------------------------------

                        //x = cSta.LonPrj;
                        //y = cSta.LatPrj;
                        x = pts[0];
                        y = pts[1];

                        dist = GetDistanceToBasin(x, y, cpt);
                        if (dist < mindist)
                        {
                            mindist = dist;
                            mxDSN = dsn;
                            mxSta = sta;
                        }
                        cSta = null;
                    }
                    //Debug.WriteLine("svar=" + svar + ", sta=" + sta + ", x=" + x.ToString("0.0000") +
                    //", y=" + y.ToString("0.0000") + ", ptx=" + cpt.X.ToString("0.0000") +
                    //", pty=" + cpt.Y.ToString("0.0000") + ", dist=" + dist.ToString("0.0000") +
                    // ", maxd=" + mindist.ToString("0.0000"));
                }
                else
                {
                    string sfile = Path.GetFileName(WDMFile);
                    errmsg = "There are no stations for " + svar + " in " + sfile + "!";
                    WriteMessage("Error!", errmsg);
                    mxDSN = 0;
                }
            }
            catch (Exception ex)
            {
                mxDSN = 0;
            }
            return mxDSN;
        }

        /// <summary>
        /// GetStationInfo
        /// Get the station for given variable and dsn
        /// </summary>
        /// <param name="svar"></param>
        /// <param name="dsnum"></param>
        /// <returns></returns>
        private clsStation GetStationInfo(string svar, int dsnum)
        {
            Debug.WriteLine("GetStationInfo");
            SortedDictionary<int, clsStation> dsnGage = new SortedDictionary<int, clsStation>();
            clsStation cSta = new clsStation();

            try
            {
                dictGages.TryGetValue(svar, out dsnGage);
                //get station object for given dsn
                if (!dsnGage.TryGetValue(dsnum, out cSta))
                    return null;
                dsnGage = null;
                return cSta;
            }
            catch (Exception ex)
            {
                errmsg = "Cannnot find station with dsn " + dsnGage.ToString() + " in WDMFile";
                WriteMessage("Error!", errmsg);
                return null;
            }
        }
        private double GetDistanceToBasin(double x, double y, CPoint cpt)
        {
            double dist = 0.0;
            dist = (x - cpt.X) * (x - cpt.X) + (y - cpt.Y) * (y - cpt.Y);
            dist = Math.Sqrt(dist);
            return dist;
        }
        private void WriteStatus(string msg)
        {
            fWASP.statuslbl.Text = msg;
            fWASP.statusStrip.Refresh();
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

        #region "Properties"
        public DataTable WeatherTable()
        {
            return MetTable;
        }
        public DateTime WDMMinDate()
        {
            return MinDate;
        }
        public DateTime WDMMaxDate()
        {
            return MaxDate;
        }
        #endregion "Properties"

    }
}
