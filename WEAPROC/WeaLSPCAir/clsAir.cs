using atcData;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Projections;
using DotSpatial.Topology;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
//using WeaUtil;

namespace NCEIData
{
    //public class clsStation
    //{
    //    public string Station { get; set; }
    //    public string Scenario { get; set; }
    //    public string Constituent { get; set; }
    //    public int DSN { get; set; }
    //    public DateTime BegDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public string Latitude { get; set; }
    //    public string Longitude { get; set; }
    //    public double LatPrj { get; set; }
    //    public double LonPrj { get; set; }
    //}

    //public class CPoint
    //{
    //    public double X { get; set; }
    //    public double Y { get; set; }
    //}
    public class clsAir
    {
        private string WDMFile, WDMFileName, Subbasin;
        private IMap appMap;
        private ProjectionInfo mapProjection;
        //dictionary of gages for each variable, contains gage dictionary keyed on dsn
        private Dictionary<string, SortedDictionary<int, clsStation>> dictGages;
        //dictionary of basins centroid
        private SortedDictionary<int, CPoint> BasinCentroid;
        private DataTable MetTable;
        private List<string> LSPCVars = new List<string>()
            {"PREC","PEVT","ATEM","WIND","SOLR","DEWP","CLOU"};
        private frmLSPC fBasin;
        string errmsg = string.Empty;
        string crlf = Environment.NewLine;
        DateTime MinDate, MaxDate;
        DateTime SimBegDate, SimEndDate;
        private string lspcWeaFile;

        public clsAir(frmLSPC _fBasin, IMap _map, string _wdmFile, string _subbasin)
        {
            WDMFile = _wdmFile;
            appMap = _map;
            Subbasin = _subbasin;
            fBasin = _fBasin;

            //initialize dictionaries of gages and basin centroid
            //dictGages = new Dictionary<string, SortedDictionary<int, clsStation>>();
            BasinCentroid = new SortedDictionary<int, CPoint>();
            dictGages = fBasin.dictGages;

            mapProjection = appMap.Projection;
            WDMFileName = Path.GetFileName(WDMFile);

            //init dates
            //MinDate = fBasin.dtBegDate.MinDate;
            //MaxDate = fBasin.dtEndDate.MaxDate;
            SimBegDate = fBasin.dtBegDate.Value;
            SimEndDate = fBasin.dtEndDate.Value;

            InitializeMetTable();

            //string sbasin = Path.GetFileNameWithoutExtension(Subbasin) + ".wea";
            //lspcWeaFile = Path.Combine(fBasin.WeaFolder, "LSPC_" + sbasin);
        }
        private void InitializeMetTable()
        {
            MetTable = new DataTable();
            MetTable.Columns.Add("Subbasin", typeof(string));
            MetTable.Columns.Add("PREC", typeof(string));
            MetTable.Columns.Add("PEVT", typeof(string));
            MetTable.Columns.Add("ATEM", typeof(string));
            MetTable.Columns.Add("WIND", typeof(string));
            MetTable.Columns.Add("SOLR", typeof(string));
            MetTable.Columns.Add("DEWP", typeof(string));
            MetTable.Columns.Add("CLOU", typeof(string));
        }
        private bool GetDictionaryOfDatasets()
        {
            Debug.WriteLine("Entering GetDictionaryOfDatasets ...");

            DateTime dbeg, dend;
            int dsn;
            string sloc, svar, scen, lat, lon, tunit;
            atcWDM.atcDataSourceWDM lwdm;

            SortedDictionary<int, clsStation> dsnGage;

            //get the dates again if changed
            SimBegDate = fBasin.dtBegDate.Value;
            SimEndDate = fBasin.dtEndDate.Value;

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
                    tunit = lDataSet.Attributes.GetValue("Longitude").ToString().Trim();

                    //Debug.WriteLine("sloc=" + sloc + " ,lon=" + lon.ToString() + ", lat=" + lat.ToString());

                    Int32 numval = (Int32)lDataSet.numValues;
                    dbeg = DateTime.FromOADate(lDataSet.Dates.Values[0]);
                    dend = DateTime.FromOADate(lDataSet.Dates.Values[numval - 1]);
                    lDataSet.Clear();

                    if (DateTime.Compare(dbeg, MinDate) >= 0)
                        MinDate = dbeg;
                    if (DateTime.Compare(dend, MaxDate) <= 0)
                        MaxDate = dend;

                    //reproject lat-lon to mercator----
                    var projFrom = KnownCoordinateSystems.Geographic.World.WGS1984;
                    var projTo = KnownCoordinateSystems.Projected.World.WebMercator;
                    List<double> lpts = new List<double>() { Convert.ToDouble(lon), Convert.ToDouble(lat) };
                    double[] pts = lpts.ToArray();
                    var z = new double[pts.Count() / 2];
                    Reproject.ReprojectPoints(pts, z, projFrom, projTo, 0, pts.Length / 2);
                    lpts = null;
                    //---------------------------------

                    fBasin.WriteStatus("Reading attributes of " + sloc + ":" +
                        svar + "(" + ncnt.ToString() + " of " + numdset.ToString() + ")");
                    //Debug.WriteLine("Reading attributes of " + sloc + ":" +
                    //    svar + "(" + ncnt.ToString() + " of " + numdset.ToString() + ")");

                    //add to dictionary of gages, check only period of series
                    //when doing assignment
                    //if (DateTime.Compare(dbeg, SimBegDate) <= 0 &&
                    //    DateTime.Compare(dend, SimEndDate) >= 0)
                    if (LSPCVars.Contains(svar) && tunit.Contains("Hour"))
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
                fBasin.WriteStatus("Ready ...");
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
        public bool GetSubbasinCentroid()
        {
            Debug.WriteLine("Entering GetBasinCentroid ...");
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                FeatureSet fs0 = (FeatureSet)FeatureSet.Open(Subbasin);
                fs0.Reproject(appMap.Projection);
                DataTable dt = fs0.DataTable;
                string sbasincol = GetSubbasinColumn(dt);
                //Debug.WriteLine("Subbasin Column=" + sbasincol);

                foreach (IFeature ifeat in fs0.Features)
                {
                    DataRow drow = ifeat.DataRow;
                    string basin = drow[sbasincol].ToString();
                    List<Coordinate> coord = (List<Coordinate>)ifeat.Centroid().BasicGeometry.Coordinates;
                    //Debug.WriteLine("In SubBasinCentroid : {0}, {1}, {2}, {3}", ifeat.Fid.ToString(), basin, coord[0].X.ToString(), coord[0].Y.ToString());
                    CPoint pt = new CPoint();
                    pt.X = (double)coord[0].X;
                    pt.Y = (double)coord[0].Y;
                    BasinCentroid.Add(Convert.ToInt32(basin), pt);
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error in getting centriod of subbasins!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            Cursor.Current = Cursors.Default;
            foreach (KeyValuePair<int, CPoint> kv in BasinCentroid)
            {
                Debug.WriteLine("In Subbasin Centroid: " + kv.Key.ToString() + "," + kv.Value.X.ToString() + "," + kv.Value.Y.ToString());
            }
            return true;
        }
        private string GetSubbasinColumn(DataTable aTable)
        {
            string sbasin = string.Empty;
            if (aTable.Columns.Contains("Subbasin"))
                sbasin = "Subbasin";
            else if (aTable.Columns.Contains("SUBBASIN"))
                sbasin = "SUBBASIN";
            else if (aTable.Columns.Contains("StreamLink"))
                sbasin = "StreamLink";
            else if (aTable.Columns.Contains("PolygonID"))
                sbasin = "PolygonID";
            else
                sbasin = "FID";
            return sbasin;
        }
        /// <summary>
        /// AssignMetVariableByBasin
        /// Assigns dsn's for each variable for each subbasin (string)
        /// </summary>
        public bool AssignMetVariableByBasin()
        {
            Debug.WriteLine("Entering AssignMetVariableByBasin ....");
            try
            {
                Dictionary<string, string> dictDsn;
                int nbasin = BasinCentroid.Count;
                int icount = 0;

                //dictionary key is subbasin, value is centroid coordinates
                foreach (KeyValuePair<int, CPoint> kv in BasinCentroid)
                {
                    icount++;
                    int dsn = 0;
                    int basin = kv.Key;
                    string gage;
                    CPoint cpt = kv.Value;
                    //Debug.WriteLine("In AssignMet: Basin={0},X={1},Y={2}", basin, cpt.X.ToString(), cpt.Y.ToString());
                    dictDsn = new Dictionary<string, string>();

                    fBasin.WriteStatus("Assigning met stations for subbasin " + basin.ToString() + "(" +
                        icount.ToString() + " of " + nbasin.ToString() + ")");
                    foreach (string svar in LSPCVars)
                    {
                        dsn = AssignMetStationVar(svar, cpt);
                        clsStation cSta = new clsStation();
                        cSta = GetStationInfo(svar, dsn);
                        gage = cSta.Station;
                        dictDsn.Add(svar, gage + ":" + dsn.ToString());
                        cSta = null;
                    }
                    cpt = null;

                    //add to datarows of MetTable
                    DataRow dr = MetTable.NewRow();
                    foreach (string svar in LSPCVars)
                    {
                        dictDsn.TryGetValue(svar, out gage);
                        dr[svar] = gage;
                    }
                    dr["Subbasin"] = basin.ToString();
                    MetTable.Rows.Add(dr);
                    dr = null;
                    dictDsn = null;
                }
                fBasin.WriteStatus("Ready ...");
                return true;
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
            List<string> lstDSN = new List<string>();
            double maxdist = 9999999999999;
            int mxDSN = 0;
            string mxSta = string.Empty;

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
                        string sta = arr[0];
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
                        double x = pts[0];
                        double y = pts[1];

                        //double x = cSta.LonPrj;
                        //double y = cSta.LatPrj;
                        double dist = GetDistanceToBasin(x, y, cpt);
                        if (dist < maxdist)
                        {
                            maxdist = dist;
                            mxDSN = dsn;
                            mxSta = sta;
                        }
                        cSta = null;
                    }
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
        private SortedDictionary<double, int> AssignMetStationVarIDW(string svar, CPoint cpt)
        {
            //dictionary of neighbors,dsn and distance
            SortedDictionary<double,int> dictNeighbors = new SortedDictionary<double,int>();
            List<string> lstDSN = new List<string>();
            string mxSta = string.Empty;

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
                        string sta = arr[0];
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
                        double x = pts[0];
                        double y = pts[1];

                        //double x = cSta.LonPrj;
                        //double y = cSta.LatPrj;
                        double dist = GetDistanceToBasin(x, y, cpt);
                        dictNeighbors.Add(dist,dsn);
                        cSta = null;
                    }
                    //debug
                    int icnt = 0;
                    foreach (KeyValuePair<double,int>kv in dictNeighbors)
                    {
                        icnt++;
                        if (icnt<=6)
                        Debug.WriteLine("svar=" + svar + ", dist=" + kv.Key.ToString() + ", dsn=" + kv.Value.ToString());
                    }
                }
                else
                {
                    string sfile = Path.GetFileName(WDMFile);
                    errmsg = "There are no stations for " + svar + " in " + sfile + "!";
                    WriteMessage("Error!", errmsg);
                    dictNeighbors=null;
                }
            }
            catch (Exception ex)
            {
                dictNeighbors = null;
            }
            return dictNeighbors;
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
            SortedDictionary<int, clsStation> dsnGage = new SortedDictionary<int, clsStation>();
            clsStation cSta = new clsStation();

            try
            {
                dictGages.TryGetValue(svar, out dsnGage);
                //get station object for given dsn
                dsnGage.TryGetValue(dsnum, out cSta);
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
            fBasin.statuslbl.Text = msg;
            fBasin.statusStrip.Refresh();
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
