//#define debug


//Revision
// 05.21.21 Added spatial mapping for NLDAS rain, PEVT, SOLR
// 05.25.21 save all annual series to dictionary, include all parameters
//          added NLDAS and GLDAS, working need to fix map extent
// 06.21.21 fixed dates offset, NLDAS OK need to test GHCN

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using WeaWDM;
using ZedGraph;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Projections;
using DotSpatial.Controls;
using DotSpatial.Symbology;
using MathNet.Numerics;
using DotSpatial.Analysis;

namespace NCEIData
{
    public partial class frmSpatial : Form
    {
        private bool ifdebug = true;
        private string WdmFile;
        private frmMain fMain;

        private DataTable tblWDM, tblData, tblStats;
        public DataTable tblSelect;
        private WDM cwdm;
        private string errmsg;
        private string crlf = Environment.NewLine;
        private enum TimePeriod { Annual, Monthly };
        private int OptionPeriod = (int)TimePeriod.Annual;
        private StreamWriter wrlog;
        DateTime dtbeg, dtend;
        private string mssg;

        //dictionaries: dicsStation key = DSN; dictStaSeries key=dsn
        SortedDictionary<int, clsStation> dictStations = new SortedDictionary<int, clsStation>();
        //Dictionary of dictionary of annual series, keys Variable and grid/gage
        //               Constituent              DSN                      DateTime  Value
        SortedDictionary<string, SortedDictionary<string, SortedDictionary<DateTime, double>>> dictDataSeries = new
            SortedDictionary<string, SortedDictionary<string, SortedDictionary<DateTime, double>>>();
        SortedDictionary<int, string> dictDSN = new SortedDictionary<int, string>();
        SortedDictionary<string, GridPoint> dictGeog = new SortedDictionary<string, GridPoint>();

        private int StartYear, EndYear;
        private List<string> lstColNames = new List<string>();
        private List<string> WeaParam = new List<string>();
        //{ "Rainfall", "Potential ET", "Solar Radiation", "Longwave Radiation",
        //  "Temperature", "Min Temperature", "Max Temperature", "Dew Point", "Wind" };
        private List<string> WeaUnits = new List<string>();
        //{ "in.", "in.", "ly", "ly", "deg F", "deg F",  "deg F", "deg F","mi/hr" };
        private List<string> WeaVars = new List<string>();
        //{ "PREC", "PEVT", "SOLR", "LRAD", "ATEM", "TMIN","TMAX","DEWP","WIND" };
        private List<int> numCat = new List<int>();
        // { 10, 5, 5, 5, 5,5,5, 5, 5};
        private List<string> lstVars = new List<string>();

        //map
        private FeatureSet[] fsAnnual = new FeatureSet[7];
        private FeatureSet Annualfs;
        private IFeatureLayer AnnualLayer = new MapPointLayer();
        private PointLayer AnnualPtLayer = new MapPointLayer();
        private PolygonLayer AnnualPolyLayer = new MapPolygonLayer();
        public string selectedParam = string.Empty;
        public string selectedVar = string.Empty;
        public string selectedUnit = string.Empty;
        public int selectedVarIndex = 0;
        private double mndata = 9999999.0, mxdata = 0.0;
        private int numCategories = 10;
        private Extent layerExtent;

        //graph
        private PointPairList varSeries = new PointPairList();

        private bool isWDMloaded = false;
        private int WidthCollapsed = 526;
        private int HeightCollapsed = 178;
        private int WidthExpand = 850;
        private int HeightExpand = 500;

        //debug
        private frmSpatialSelect fSelect;
        private bool ShowSelected = false;

        public frmSpatial(frmMain _fMain, string _Wdm, StreamWriter _wrlog)
        {
            InitializeComponent();
            this.fMain = _fMain;
            this.WdmFile = _Wdm;
            this.wrlog = _wrlog;
            InitializeTableOfTimeSeries();

            selectedVarIndex = -1;
            //selectedVar = WeaVars.ElementAt(selectedVarIndex);
            //selectedUnit = WeaUnits.ElementAt(selectedVarIndex);
            //selectedParam = WeaParam.ElementAt(selectedVarIndex);

            this.Width = WidthCollapsed;
            this.Height = HeightCollapsed;

            //set default number of bins
            //numBin.Value = (int)numCat[selectedVarIndex];
            EnableControls(false);
#if debug
            chkSelected.Visible = true;
            fSelect = new frmSpatialSelect(this);
#endif
        }
        #region "Methods"
        private void EnableControls(bool isEnabled)
        {
            //show other controls if true
            cboPeriod.Enabled = isEnabled;
            numBin.Enabled = isEnabled;
            chkShowTable.Enabled = isEnabled;
            chkShowGrpTable.Enabled = isEnabled;
        }
        private bool InitializeTableOfTimeSeries()
        {
            try
            {
                //data
                tblData = new DataTable();
                tblData.TableName = "dtData";

                //stats
                tblStats = new DataTable();
                tblStats.TableName = "dtStats";
                tblStats.Columns.Add("Mean");
                tblStats.Columns.Add("Stdev");
                tblStats.Columns.Add("Median");
                tblStats.Columns.Add("Minimum");
                tblStats.Columns.Add("Maximum");
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool CreateTableOfTimeSeries()
        {
            try
            {
                //clear columns
                if (tblData.Columns.Count > 0) tblData.Columns.Clear();

                tblData.Columns.Add("DSN", typeof(int));
                tblData.Columns.Add("Station", typeof(string));
                foreach (string syr in lstColNames) tblData.Columns.Add(syr, typeof(string));
                if (tblData.Rows.Count > 0) tblData.Rows.Clear();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// GetWDMSeries, gets all series DSN's for timeunit- annual
        /// and also the dictionary of series saved to dictDataSeries
        /// called from main program
        /// </summary>
        /// <param name="timeunit"></param>
        /// <returns></returns>
        public bool GetWDMSeries(List<string> timeunit)
        {
            try
            {
                WDM cwdm = new WDM(WdmFile);

                //GetWDMAllSeries should select all annual series for constituent/scenario
                //and return a table of attributes of the series
                tblWDM = cwdm.GetWDMAllSeries(timeunit);
                if (tblWDM == null) return false;
                cwdm = null;

                //Get all annual time series
                if (GetAllAnnualTimeSeriesFromWDM())
                {
                    Application.DoEvents();
#if debug //OK
                    //DebugDictDataSeries();
#endif
                    //list of variables with annual data;
                    GetListOfVariables();

                    //set combo variables
                    //cboVar.Items.Add("-Select Variable-");
                    foreach (string s in WeaParam)
                        cboVar.Items.Add(s);
                    cboVar.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                mssg = "WDM does not contain annual series! Please build series using " +
                       "WDMUtil or SARA.";
                MessageBox.Show(mssg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        private void GetListOfVariables()
        {
            //setup the list of parameters, units and variables in wdm
            //WeaParam = { "Rainfall", "Potential ET", "Solar Radiation", "Longwave Radiation",
            //            "Temperature", "Min Temperature", "Max Temperature", "Dew Point", "Wind" };
            //WeaUnits = { "in.", "in.", "ly", "ly", "deg F", "deg F",  "deg F", "deg F","mi/hr" };
            //WeaVars =  { "PREC", "PEVT", "SOLR", "LRAD", "ATEM", "TMIN","TMAX","DEWP","WIND" };
            //numCat = new List<int>() { 10, 5, 5, 5, 5, 5, 5, 5, 5 };
            WeaParam.Add("-Select Variable-");
            WeaUnits.Add("");
            WeaVars.Add("");
            numCat.Add(0);

            try
            {
                lstVars = dictDataSeries.Keys.ToList();
#if debug
                foreach (string s in lstVars)
                    Debug.WriteLine("Variable : " + s);
#endif
                foreach (string svar in lstVars)
                {
                    switch (svar)
                    {
                        case "PREC":
                            if (!WeaParam.Contains("Rainfall"))
                            {
                                WeaParam.Add("Rainfall");
                                WeaUnits.Add("in.");
                                WeaVars.Add("PREC");
                                numCat.Add(10);
                                numBin.Value = 10;
                            }
                            break;
                        case "PRCP":
                            if (!WeaParam.Contains("Rainfall"))
                            {
                                WeaParam.Add("Rainfall");
                                WeaUnits.Add("in.");
                                WeaVars.Add("PREC");
                                numCat.Add(10);
                                numBin.Value = 10;
                            }
                            break;
                        case "PEVT":
                            WeaParam.Add("Potential ET");
                            WeaUnits.Add("in.");
                            WeaVars.Add("PEVT");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "SOLR":
                            WeaParam.Add("Solar Radiation");
                            WeaUnits.Add("ly.");
                            WeaVars.Add("SOLR");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "LRAD":
                            WeaParam.Add("Longwave Radiation");
                            WeaUnits.Add("ly.");
                            WeaVars.Add("LRAD");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "ATEM":
                            WeaParam.Add("Temperature");
                            WeaUnits.Add("deg F.");
                            WeaVars.Add("ATEM");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "TMIN":
                            WeaParam.Add("Min Temperature");
                            WeaUnits.Add("deg F.");
                            WeaVars.Add("TMIN");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "TMAX":
                            WeaParam.Add("Max Temperature");
                            WeaUnits.Add("deg F.");
                            WeaVars.Add("TMAX");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "DEWP":
                            WeaParam.Add("Dew Point");
                            WeaUnits.Add("deg F.");
                            WeaVars.Add("DEWP");
                            numCat.Add(5);
                            numBin.Value = 5;
                            break;
                        case "WIND":
                            WeaParam.Add("Wind Speed");
                            WeaUnits.Add("mi/hr.");
                            WeaVars.Add("WIND");
                            numBin.Value = 5;
                            numCat.Add(5);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void DebugDictDataSeries()
        {
            Debug.WriteLine("In DebugDictDataSeries ...");
            List<string> cons = dictDataSeries.Keys.ToList();
            SortedDictionary<string, SortedDictionary<DateTime, double>> dsta = new SortedDictionary<string, SortedDictionary<DateTime, double>>();
            SortedDictionary<DateTime, double> dSeries = new SortedDictionary<DateTime, double>();
            foreach (string scon in cons)
            {
                dictDataSeries.TryGetValue(scon, out dsta);
                foreach (KeyValuePair<string, SortedDictionary<DateTime, double>> kv in dsta)
                {
                    string dsn = kv.Key;
                    dsta.TryGetValue(dsn, out dSeries);
                    Debug.WriteLine("{0},{1},{2},{3},{4},{5}", scon.ToString(),dsn, 
                        dSeries.Keys.First().ToString(), dSeries.Keys.Last().ToString(),
                        dSeries.Keys.First().Year.ToString(), dSeries.Keys.Last().Year.ToString());
                }
            }
        }
        private bool GetYearsFromTableSelect()
        {
            Debug.WriteLine("Entering GetYears From Table Select ...");
            try
            {
                DateTime BegTime = DateTime.Now;
                DateTime EndTime = DateTime.Now.AddYears(-100);

                foreach (DataRow drow in tblSelect.Rows)
                {
                    DateTime dtbeg = (DateTime)drow["StartDate"];
                    DateTime dtend = (DateTime)drow["EndDate"];
                    if (DateTime.Compare(dtbeg, BegTime) <= 0)
                        BegTime = dtbeg;

                    if (DateTime.Compare(dtend, EndTime) >= 0)
                        EndTime = dtend;
                }
#if debug
                Debug.WriteLine("Begin Date {0}, End Date {1}", BegTime.ToString(), EndTime.ToString());
#endif
                StartYear = BegTime.Year;
                //EndYear = EndTime.Year - 1;
                EndYear = EndTime.Year;
#if debug
                Debug.WriteLine("Start {0}, End {1}", StartYear.ToString(), EndYear.ToString());
#endif

                //add years to column names for datatable
                lstColNames.Clear();
                for (int i = StartYear; i <= EndYear; i++) lstColNames.Add(i.ToString());
                lstColNames.Add("Annual");
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// FilterWDMSeriesForSelectedVar
        /// Filters table of series for selected parameter
        /// </summary>
        /// <returns></returns>
        public bool FilterWDMSeriesForSelectedVar()
        {
            Debug.WriteLine("Entering FilterWDMSeriesForSelectedVar()");
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                dictGeog.Clear();

                //fill dataviewer, default to year, if querying both annual and monthly
                //filter annual series, monthly to be implemented and save to new table
                //tblSelect, display in dgvSelect for debugging

                string period = "TUYear";
                string filter0 = "TimeUnit = '" + period.ToString() + "'";
                string filter1 = string.Empty;

                switch (selectedParam)
                {
                    case "Rainfall":
                        filter1 = " AND Constituent in ('PREC','PRCP')";
                        break;
                    case "Potential ET":
                        filter1 = " AND Constituent in ('PEVT')";
                        break;
                    case "Solar Radiation":
                        filter1 = " AND Constituent in ('SOLR')";
                        break;
                    case "Longwave Radiation":
                        filter1 = " AND Constituent in ('LRAD')";
                        break;
                    case "Temperature":
                        filter1 = " AND Constituent in ('ATEM')";
                        break;
                    case "Min Temperature":
                        filter1 = " AND Constituent in ('TMIN')";
                        break;
                    case "Max Temperature":
                        filter1 = " AND Constituent in ('TMAX')";
                        break;
                    case "Dew Point":
                        filter1 = " AND Constituent in ('DEWP')";
                        break;
                    case "Wind":
                        filter1 = " AND Constituent in ('WIND')";
                        break;
                }

                tblSelect = tblWDM.Select(filter0 + filter1).CopyToDataTable();
#if debug
                Debug.WriteLine("Table Select Rows=" + tblSelect.Rows.Count.ToString());
#endif
                dictGeog.Clear();
                if (tblSelect.Rows.Count > 0)
                {
                    //build dictionary of selected gage locations - dictGeog
                    foreach (DataRow drow in tblSelect.Rows)
                    {
                        string sta = drow["Station"].ToString();
                        double x = Convert.ToDouble(drow["Longitude"].ToString());
                        double y = Convert.ToDouble(drow["Latitude"].ToString());
                        GridPoint pt = new GridPoint();
                        pt.xlon = x;
                        pt.ylat = y;
                        if (!dictGeog.ContainsKey(sta))
                            dictGeog.Add(sta, pt);
                    }
                    //debug
                    //foreach (KeyValuePair<string, GridPoint> kv in dictGeog)
                    //    Debug.WriteLine("{0},{1},{2}", kv.Key, kv.Value.xlon.ToString(), kv.Value.ylat.ToString());

                    dictDSN.Clear();
                    dictDSN = GetDictionaryOfDSN();
#if debug
                    foreach (KeyValuePair<int, string> kv in dictDSN)
                        Debug.WriteLine("DSN={0},Sta={1}", kv.Key.ToString(), kv.Value);
#endif
                    GetYearsFromTableSelect();
                    CreateTableOfTimeSeries();

                    //get dictionary of selected parameter
                    SortedDictionary<string, SortedDictionary<DateTime, double>> dCurVarSeries =
                           new SortedDictionary<string, SortedDictionary<DateTime, double>>();
                    dictDataSeries.TryGetValue(selectedVar, out dCurVarSeries);
                    GetTimeSeriesFromDataDictionary(dCurVarSeries);
                    dCurVarSeries = null;

                    //draw shapefile and load in map
                    ClearMapOfAnnualSeries();
                    DrawAnnualGridLayer();
                }
                else
                {
                    mssg = "WDM does not contain annual series for " + selectedVar + "! Please build series using " +
                        "WDMUtil or SARA.";
                    MessageBox.Show(mssg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

#if debug
                if (ShowSelected)
                    fSelect.ReSelect(tblSelect);
#endif
            }
            catch (Exception ex)
            {
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }
        public SortedDictionary<int, string> GetDictionaryOfDSN()
        {
            SortedDictionary<int, string> dDSN = new SortedDictionary<int, string>();

            try
            {
                foreach (DataRow drow in tblSelect.Rows)
                {
                    string sta = drow["Station"].ToString();
                    int dsn = Convert.ToInt32(drow["DSN"].ToString());
                    if (!dDSN.Keys.Contains(dsn))
                        dDSN.Add(dsn, sta);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error getting WDM dsn's!";
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dDSN;
        }

        /// <summary>
        /// GetTimeSeriesFromWDM()
        /// called from GetWDMSeries
        /// </summary>
        /// <returns></returns>
        public bool GetTimeSeriesFromDataDictionary(SortedDictionary<string, SortedDictionary<DateTime, double>> dVarSeries)
        {
            Debug.WriteLine("In GetTimeSeriesFromDataDictionary...");

            Cursor.Current = Cursors.WaitCursor;

            //setup period selector combo box, clear box first
            cboPeriod.Items.Clear();
            cboPeriod.Items.Add("-Select Period-");
            foreach (string syr in lstColNames) cboPeriod.Items.Add(syr);
            cboPeriod.SelectedIndex = 0;

            mndata = 9999999.0; mxdata = 0.0;
            try
            {
                string dsn = string.Empty; string sta = string.Empty;
                SortedDictionary<DateTime, double> dictSeries; 

                //clear previous datatable and build current one
                if (tblData.Rows.Count > 0) tblData.Rows.Clear();

#if debug
                Debug.WriteLine("In GetTimeSeriesFromDataDictionary...");
#endif
                foreach (KeyValuePair<int, string> kv in dictDSN)
                {
                    dsn = kv.Key.ToString();
                    sta = kv.Value;
                    //debug
#if debug
                    Debug.WriteLine("Processing DSN={0},Station={1}", dsn.ToString(), sta);
#endif
                    //Get series for dsn
                    dictSeries =  new SortedDictionary<DateTime, double>();
                    if (dVarSeries.TryGetValue(dsn, out dictSeries))
                    {
                        //Fill table of annual series, tblData
                        DataRow dr = tblData.NewRow();
                        dr["DSN"] = dsn;
                        dr["Station"] = sta;

                        //fill row with missing data and new row
                        foreach (string scol in lstColNames)
                            dr[scol] = "-999";
                        tblData.Rows.Add(dr);

                        //edit row with data
                        int ncount = dictSeries.Count;
                        int yr = 0;
                        for (int i = 0; i < ncount; i++)
                        {
                            yr = (dictSeries.Keys.ElementAt(i).Year);
                            string val = dictSeries.Values.ElementAt(i).ToString("F2");
                            double dval = 0.0;
                            dval = Convert.ToDouble(val);
                            val = dval.ToString("F1");
                            //string scol = lstColNames.ElementAt(i);
                            string scol = yr.ToString();
                            dr[scol] = val;
#if debug
                            Debug.WriteLine("{0},{1},{2},{3}", dsn, sta, scol, val.ToString());
#endif
                        }

                        double mean = GetAnnualStatistics(dictSeries.Values.ToList());
                        double dmin = MathNet.Numerics.Statistics.Statistics.Minimum(dictSeries.Values.ToList());
                        double dmax = MathNet.Numerics.Statistics.Statistics.Maximum(dictSeries.Values.ToList());

                        mndata = (mndata < dmin ? mndata : dmin);
                        mxdata = (mxdata > dmax ? mxdata : dmax);
                        //Debug.WriteLine("Min = {0}, Max = {1}", mndata.ToString(), mxdata.ToString());

                        dr["Annual"] = mean.ToString("F2");

                        //tblData.Rows.Add(dr);
                        dr = null;
                        dictSeries = null;
                    }
                }

                dgvSeaWDM.DataSource = null;
                dgvSeaWDM.DataSource = tblData;
                dgvSeaWDM.Columns["DSN"].Visible = false;
                dgvSeaWDM.ClearSelection();

                //setup period selector combo box, clear box first
                //cboPeriod.Items.Clear();
                //cboPeriod.Items.Add("-Select Period-");
                //foreach (string syr in lstColNames) cboPeriod.Items.Add(syr);
                //cboPeriod.SelectedIndex = 0;

                //draw grid layer
                //ClearMapOfAnnualSeries();
                //DrawAnnualGridLayer();
            }
            catch (Exception ex)
            {
                string msg = "Error getting timeseries from WDM!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }
        /// <summary>
        /// GetAllAnnualTimeSeriesFromWDM
        /// Gets all the annual time series from the WDM 
        /// Returned as a dictionary
        /// </summary>
        /// <returns></returns>
        public bool GetAllAnnualTimeSeriesFromWDM()
        {
            //SortedDictionary<string, SortedDictionary<string, SortedDictionary<DateTime, double>>> dictDataSeries = new
            //    SortedDictionary<string, SortedDictionary<string, SortedDictionary<DateTime, double>>>();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                WDM cwdm = new WDM(WdmFile);
                string dsn = string.Empty; string sta = string.Empty; string cons = string.Empty;
                int idsn = 0;

                //dictionary key=variable, value is series
                SortedDictionary<string, SortedDictionary<DateTime, double>> dictStaSeries;

                //dictionary key=datetime, value is annual data
                SortedDictionary<DateTime, double> dictSeries =
                            new SortedDictionary<DateTime, double>();
                dictDataSeries.Clear();
                foreach (DataRow dr in tblWDM.Rows)
                {
                    dsn = dr["DSN"].ToString();
                    sta = dr["Station"].ToString();
                    cons = dr["Constituent"].ToString();
                    idsn = Convert.ToInt32(dsn);

                    //Get series for dsn
                    dictSeries = cwdm.GetTimeSeries(idsn);
//#if debug
                    Debug.WriteLine("In GetAllAnnualTimeSeriesFromWDM ..");
                    foreach (KeyValuePair<DateTime, double> kv in dictSeries)
                    {
                        Debug.WriteLine("{0},{1},{2},{3},{4}", dsn, sta, kv.Key.ToString(), kv.Key.Year.ToString(),
                            kv.Value.ToString());
                    }
//#endif
                    //RemoveMissing(dictSeries);

                    //save to global dictionary of series, dictDataSeries is the global dict 
                    //change PRCP to PREC
                    if (cons.Contains("PRCP")) cons = "PREC";
                    if (!dictDataSeries.ContainsKey(cons))
                    {
                        dictStaSeries = new SortedDictionary<string, SortedDictionary<DateTime, double>>();
                        dictDataSeries.Add(cons, dictStaSeries);
                        dictStaSeries.Add(dsn, dictSeries);
                    }
                    else
                    {
                        dictDataSeries.TryGetValue(cons, out dictStaSeries);
                        dictStaSeries.Add(dsn, dictSeries);
                    }
                    dictSeries = null;
                }
                cwdm = null;
            }
            catch (Exception ex)
            {
                string msg = "Error getting timeseries from WDM!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }

        private void RemoveMissing(SortedDictionary<DateTime, double> dictSeries)
        {
            try
            {
                foreach (KeyValuePair<DateTime, double> kv in dictSeries)
                {
                    if (kv.Value.ToString().Contains("Missing"))
                        dictSeries.Remove(kv.Key);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearSelection()
        {
            //foreach(DataRow dr in dgvSeaWDM.SelectedRows)
        }
        private double GetAnnualStatistics(List<double> tseries)
        {
            double mean = MathNet.Numerics.Statistics.Statistics.Mean(tseries);
            double stdev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(tseries);
            return mean;
        }
        private bool CalculateSeriesStats(double[] tseries)
        {
            Cursor.Current = Cursors.WaitCursor;
            double mean, std, min, max, med;
            try
            {
                mean = MathNet.Numerics.Statistics.Statistics.Mean(tseries);
                std = MathNet.Numerics.Statistics.Statistics.StandardDeviation(tseries);
                med = MathNet.Numerics.Statistics.Statistics.Median(tseries);
                min = MathNet.Numerics.Statistics.Statistics.Minimum(tseries);
                max = MathNet.Numerics.Statistics.Statistics.Maximum(tseries);

                if (tblStats.Rows.Count > 0) tblStats.Rows.Clear();
                DataRow dr = tblStats.NewRow();
                dr["Mean"] = mean.ToString("F2");
                dr["Stdev"] = std.ToString("F2");
                dr["Median"] = med.ToString("F2");
                dr["Minimum"] = min.ToString("F2");
                dr["Maximum"] = max.ToString("F2");
                tblStats.Rows.Add(dr);
                dr = null;

                dgvStats.DataSource = null;
                dgvStats.ClearSelection();
                dgvStats.DataSource = tblStats;
                dgvStats.ClearSelection();
                dgvStats.Refresh();
            }
            catch (Exception ex)
            {
                mssg = "Error calculating statistics of series!" + crlf + crlf + ex.Message + ex.StackTrace;
                MessageBox.Show(mssg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }
        private bool CreateSeriesGraph(List<string> stname, double[] sites, double[] series, string syear)
        {
            CalculateSeriesStats(series);

            Debug.WriteLine("Entering CreateGraphSeries....");
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                zedSeries.IsShowPointValues = true;
                string[] labels = stname.ToArray();
                zedSeries.Size = new Size(ClientRectangle.Width - 20,
                                        ClientRectangle.Height - 20);
                zedSeries.Invalidate();

                // get a reference to the GraphPane
                GraphPane seriesPane = zedSeries.GraphPane;
                seriesPane.Legend.IsVisible = false;

                // Set the Titles
                seriesPane.Title.Text = syear + " " + selectedParam + " Distribution";
                seriesPane.XAxis.Title.Text = "Station";
                seriesPane.YAxis.Title.Text = syear + " " + selectedParam + ", " + selectedUnit;
                //seriesPane.XAxis.Type = AxisType.LinearAsOrdinal;
                seriesPane.XAxis.Type = AxisType.Text;
                seriesPane.XAxis.Scale.FontSpec.Size = 8;
                seriesPane.YAxis.Scale.FontSpec.Size = 8;
                seriesPane.XAxis.Scale.TextLabels = labels;

                // Series arrays
                if (varSeries.Count > 0) varSeries.Clear();

                //for bar curve
                double min = 9999, max = -99;
                for (int i = 0; i < series.Count(); i++)
                {
                    double val = series[i];
                    double dsn = sites[i];
                    varSeries.Add(dsn, val);
                    if (val < min) min = val;
                    if (val > max) max = val;
                    //Debug.WriteLine("{0},{1}", dsn.ToString(), val.ToString());
                }
                //BarItem mbar = seriesPane.AddBar("", null, series, Color.Red);
                //mbar.Bar.Fill = new Fill(Color.Red, Color.White, Color.Red);
                //seriesPane.YAxis.Scale.TextLabels = null;
                //seriesPane.XAxis.Scale.TextLabels = labels;
                //seriesPane.XAxis.Type = AxisType.Text;

                LineItem curve = seriesPane.AddCurve("", varSeries, Color.Blue,
                       ZedGraph.SymbolType.Circle);
                curve.Line.IsVisible = false;
                curve.Symbol.IsVisible = true;
                curve.Symbol.Fill = new Fill(Color.Blue);
                curve.Symbol.Border.Color = Color.Blue;
                curve.Symbol.Size = 8;

                zedSeries.AxisChange();
                zedSeries.Refresh();
            }
            catch (Exception ex)
            {
                string msg = "Error generating annual series plot!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }

        #endregion "Methods"

        #region "Mapping"
        private bool DrawAnnualGridLayer()
        {
            Debug.WriteLine("Entering Draw Grid Layer ...");
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string fname = System.IO.Path.GetTempFileName().Replace(".tmp", ".shp");
                string gisdir = Path.Combine(fMain.cacheDir, "GIS");

                fname = Path.Combine(gisdir, "AnnualGrid_" + selectedVar + ".shp");
                Annualfs = new FeatureSet(FeatureType.Point);

                //fsAnnual[selectedVarIndex] = new FeatureSet(FeatureType.Point);
                //Annualfs = fsAnnual[selectedVarIndex];

                // Add Some Columns
                Annualfs.DataTable.Columns.Add(new DataColumn("Station_ID", typeof(string)));
                Annualfs.DataTable.Columns.Add(new DataColumn("Station", typeof(string)));
                foreach (string s in lstColNames)
                    Annualfs.DataTable.Columns.Add(new DataColumn(s, typeof(double)));

                Annualfs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

                //fill feature attribute table
                double x, y;
                foreach (KeyValuePair<string, GridPoint> site in dictGeog)
                {
                    string idsta = site.Key;
                    GridPoint pt = site.Value;

                    x = pt.xlon;
                    y = pt.ylat;

                    Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                    DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                    IFeature curFeature = Annualfs.AddFeature(p);
                    curFeature.DataRow.BeginEdit();
                    curFeature.DataRow["Station"] = idsta;
                    curFeature.DataRow["Station_ID"] = idsta;

                    string filter = "Station = '" + idsta.ToString() + "'";
                    DataRow[] drows = tblData.Select(filter);
                    {
                        foreach (string syr in lstColNames)
                        {
                            //Debug.WriteLine("{0},{1}", syr, drows[0][syr].ToString());
                            curFeature.DataRow[syr] = Convert.ToDouble(drows[0][syr].ToString());
                        }
                    }
                    drows = null;
                    curFeature.DataRow.EndEdit();
                }

                //reproject to webmercator
                Annualfs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
                Annualfs.SaveAs(fname, true);

                //add to map
                Annualfs = (FeatureSet)FeatureSet.Open(fname);

                MapPointLayer GageSites = (MapPointLayer)fMain.appManager.Map.Layers.Add(Annualfs);
                GageSites.Symbolizer = new PointSymbolizer(Color.DarkOrange, DotSpatial.Symbology.PointShape.Ellipse, 8);
                GageSites.SelectionSymbolizer = new PointSymbolizer(Color.Aqua, DotSpatial.Symbology.PointShape.Hexagon, 12);
                GageSites.Symbolizer.SetOutline(Color.Black, 1);
                GageSites.SelectionSymbolizer.SetOutline(Color.Red, 2);
                //GageSites.DataSet.Name = "AnnualSeries_"+selectedVar;
                //GageSites.LegendText = "Annual "+selectedParam+" Series";
                GageSites.DataSet.Name = selectedParam;
                GageSites.LegendText = "Annual " + selectedParam + " Series";
                AnnualPtLayer = GageSites;

                MapLabelLayer fslbl = new MapLabelLayer(Annualfs);
                fslbl.Symbology.Categories[0].Expression = "[Station_ID]";
                fslbl.Symbolizer.Orientation = ContentAlignment.TopRight;
                fslbl.Symbolizer.PreventCollisions = false;
                fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
                GageSites.LabelLayer = fslbl;
                GageSites.ShowLabels = true;
                GageSites.IsSelected = true;

                //fMain.appManager.Map.ViewExtents = Annualfs.Extent;
                fMain.appManager.Map.ViewExtents = Annualfs.Extent;

                //fMain.appManager.Map.ZoomToNext();
                //mapExtent = fMain.appManager.Map.Extent;
                //fs = null;

                double minx, maxx, miny, maxy;
                minx = Annualfs.Extent.MinX;
                maxx = Annualfs.Extent.MaxX;
                miny = Annualfs.Extent.MinY;
                maxy = Annualfs.Extent.MaxY;

                Debug.WriteLine("{0},{1},{2},{3}", minx.ToString(), maxx.ToString(), miny.ToString(), maxy.ToString());

                Annualfs.Extent.ExpandBy(10000);

                minx = Annualfs.Extent.MinX;
                maxx = Annualfs.Extent.MaxX;
                miny = Annualfs.Extent.MinY;
                maxy = Annualfs.Extent.MaxY;

                Debug.WriteLine("{0},{1},{2},{3}", minx.ToString(), maxx.ToString(), miny.ToString(), maxy.ToString());

                layerExtent = fMain.appManager.Map.ViewExtents;
                fMain.appManager.Map.Refresh();

                //cboPeriod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                string msg = "Error drawing annual series map!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }

        private void ClearMapOfAnnualSeries()
        {
            IMapPointLayer[] ptLayers = fMain.appManager.Map.GetPointLayers();
            Debug.WriteLine("Count of point layers " + ptLayers.Count().ToString());

            if (ptLayers.Count() <= 0) return;
            try
            {
                foreach (MapPointLayer slayer in fMain.appManager.Map.GetPointLayers())
                {
                    if (!slayer.DataSet.Name.Contains(selectedParam))
                        fMain.appManager.Map.Layers.Remove(slayer);
                }
            }
            catch (Exception ex)
            {
                mssg = "Error removing layer!" + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(mssg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool DrawPolygonLayer()
        {
            Debug.WriteLine("Entering Draw Polygon Layer ...");
            try
            {
                string fname = System.IO.Path.GetTempFileName().Replace(".tmp", ".shp");
                string gisdir = Path.Combine(fMain.cacheDir, "GIS");

                fname = Path.Combine(gisdir, "AnnualPoly_" + selectedVar + ".shp");

                FeatureSet fs = new FeatureSet(FeatureType.Polygon);
                fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;
                // Add Some Columns
                fs.DataTable.Columns.Add(new DataColumn("Station_ID", typeof(string)));
                fs.DataTable.Columns.Add(new DataColumn("Station", typeof(string)));
                foreach (string s in lstColNames)
                    fs.DataTable.Columns.Add(new DataColumn(s, typeof(float)));

                fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

                //fill feature attribute table
                double x, y;
                double halfg = 0.0625;
                foreach (KeyValuePair<string, GridPoint> site in dictGeog)
                {
                    string idsta = site.Key;
                    GridPoint pt = site.Value;

                    x = pt.xlon;
                    y = pt.ylat;

                    Coordinate[] coord = new Coordinate[]
                    {
                        new Coordinate(x-halfg,y+halfg),
                        new Coordinate(x+halfg,y+halfg),
                        new Coordinate(x+halfg,y-halfg),
                        new Coordinate(x-halfg,y-halfg),
                        new Coordinate(x-halfg,y+halfg),
                    };
                    Polygon p = new Polygon(new LinearRing(coord));
                    IFeature curFeature = fs.AddFeature(p);

                    curFeature.DataRow.BeginEdit();
                    curFeature.DataRow["Station"] = idsta;
                    curFeature.DataRow["Station_ID"] = idsta;
                    string filter = "Station = '" + idsta.ToString() + "'";
                    DataRow[] drows = tblData.Select(filter);
                    {
                        foreach (string syr in lstColNames)
                            curFeature.DataRow[syr] = Convert.ToSingle(drows[0][syr].ToString());
                    }
                    drows = null;
                    curFeature.DataRow.EndEdit();
                }

                //reproject to webmercator
                fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
                fs.SaveAs(fname, true);

                MapPolygonLayer grid = (MapPolygonLayer)fMain.appManager.Map.Layers.Add(fs);
                grid.Symbolizer = new PolygonSymbolizer(Color.DarkOrange, Color.DarkOrange, 1);
                grid.SelectionSymbolizer = new PolygonSymbolizer(Color.Aqua, Color.Aqua, 2);
                grid.Symbolizer.SetOutline(Color.Black, 1);
                grid.SelectionSymbolizer.SetOutline(Color.Red, 2);
                grid.DataSet.Name = "AnnualSeries_" + selectedVar;
                grid.LegendText = "Annual " + selectedParam + " Series";
                AnnualPolyLayer = grid;

                MapLabelLayer fslbl = new MapLabelLayer(fs);
                fslbl.Symbology.Categories[0].Expression = "[Station_ID]";
                fslbl.Symbolizer.Orientation = ContentAlignment.TopRight;
                fslbl.Symbolizer.PreventCollisions = false;
                fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
                grid.LabelLayer = fslbl;
                grid.ShowLabels = true;
                grid.IsSelected = true;

                fMain.appManager.Map.ViewExtents = fs.Extent;
                fMain.appManager.Map.Refresh();
            }
            catch (Exception ex)
            {
                string msg = "Error drawing annual polygon layer!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private bool RefreshPointLayer(string syear)
        {
            Debug.WriteLine("Entering Refresh Map ....");
            try
            {
                IFeatureSet fs = Annualfs;

                PointScheme ptScheme = GeneratePointSymbologyScheme(syear, fs);
                AnnualPtLayer.Symbology = (IPointScheme)ptScheme;
                AnnualPtLayer.LegendText = "Annual " + selectedParam;
                AnnualPtLayer.DataSet.Name = "Annual_" + selectedVar;
                AnnualPtLayer.LegendItemVisible = true;

                MapLabelLayer fslbl = new MapLabelLayer(fs);
                fslbl.Symbology.Categories[0].Expression = "[Station]" + crlf + "[" + syear + "]";
                //fslbl.Symbology.Categories[0].Expression = "[Station]";
                fslbl.Symbolizer.Orientation = ContentAlignment.TopRight;
                fslbl.Symbolizer.PreventCollisions = false;
                fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
                fslbl.Symbolizer.FontSize = 10;
                fslbl.Symbolizer.FontStyle = FontStyle.Bold;
                AnnualPtLayer.LabelLayer = fslbl;
                AnnualPtLayer.ShowLabels = true;
                AnnualPtLayer.IsSelected = true;

                fMain.appManager.Map.ViewExtents = AnnualPtLayer.DataSet.Extent;
                //fMain.appManager.Map.ZoomOut();
                //fMain.appManager.Map.ViewExtents = mapExtent;
                fMain.appManager.Map.Refresh();

                ptScheme = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in loading annual series map! \r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }
        }
        private bool RefreshMap(string syear)
        {
            Debug.WriteLine("Entering Refresh Map ....");
            try
            {
                IFeatureSet fs = Annualfs;

                PointScheme ptScheme = GeneratePointSymbologyScheme(syear, fs);
                AnnualPtLayer.Symbology = (IPointScheme)ptScheme;
                AnnualPtLayer.LegendText = "Annual " + selectedParam;
                AnnualPtLayer.DataSet.Name = "Annual_" + selectedVar;
                AnnualPtLayer.LegendItemVisible = true;
                fMain.appManager.Map.ViewExtents = AnnualPtLayer.DataSet.Extent;
                fMain.appManager.Map.Refresh();

                ptScheme = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in loading annual series map! \r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }
        }
        private PointScheme GeneratePointSymbologyScheme(string syear, IFeatureSet fset)
        {
            PointScheme ptScheme = new PointScheme();

            int lBreaks = 0;
            if (selectedVar.Contains("PREC"))
                lBreaks = numCat[selectedVarIndex];
            else if (selectedVar.Contains("PEVT"))
                lBreaks = numCat[selectedVarIndex];
            else if (selectedVar.Contains("SOLR"))
                lBreaks = numCat[selectedVarIndex];

            lBreaks = (int)numBin.Value;

            //double cMin = 20.0;
            double cMin = (int)mndata;
            double cMax = (int)(mxdata + 1.0);

            double diff = (cMax - cMin) / (lBreaks);
#if debug
            Debug.WriteLine("Min {0}, Max {1}, Diff {2}",
                 cMin.ToString(), cMax.ToString(), diff.ToString());
#endif

            try
            {
                ptScheme.LegendText = syear;
                ptScheme.EditorSettings.RampColors = true;
                ptScheme.EditorSettings.ClassificationType = ClassificationType.Quantities;
                ptScheme.EditorSettings.IntervalMethod = IntervalMethod.EqualInterval;
                ptScheme.EditorSettings.NumBreaks = lBreaks;
                ptScheme.EditorSettings.FieldName = syear;
                ptScheme.EditorSettings.IntervalSnapMethod = IntervalSnapMethod.Rounding;
                ptScheme.AppearsInLegend = true;
                IPointCategory pCat = null;
                double curMin = cMin;
                double curMax = cMin + diff;
                ptScheme.Categories.Clear();

                int intv = (int)(255.00 / (lBreaks - 1));
                double isym = 10;

                for (int i = 0; i < lBreaks; i++)
                {
                    pCat = new PointCategory(Color.FromArgb(255, 255 - (i * intv), 0), DotSpatial.Symbology.PointShape.Ellipse, isym + 2 * i);

                    //Debug.WriteLine("{0},{1}", (255 - (i * intv)).ToString(), (isym + 2 * i).ToString());
                    string filter = "";
                    if (i == lBreaks - 1)
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<=" + curMax;
                    }
                    else
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<" + curMax;
                    }
                    pCat.FilterExpression = filter;
                    pCat.LegendText = curMin + " - " + curMax;
                    ptScheme.AddCategory(pCat);
                    curMin = curMax;
                    curMax = curMin + diff;
                }
                return ptScheme;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Generating Point Symbology Scheme \r\n\r\n" + ex.Message + ex.StackTrace);
                return null;
            }
        }
        private PolygonScheme GeneratePolygonSymbologyScheme(string syear, IFeatureSet fset)
        {
            PolygonScheme ptScheme = new PolygonScheme();

            int lBreaks = 0;
            if (selectedVar.Contains("PREC"))
                lBreaks = 10;
            else if (selectedVar.Contains("PEVT"))
                lBreaks = 4;
            else if (selectedVar.Contains("SOLR"))
                lBreaks = 5;

            //double cMin = 20.0;
            double cMin = (int)mndata;
            double cMax = (int)(mxdata + 1.0);

            double diff = (cMax - cMin) / (lBreaks);
            Debug.WriteLine("Min {0}, Max {1}, Diff {2}",
                 cMin.ToString(), cMax.ToString(), diff.ToString());

            try
            {
                ptScheme.LegendText = syear;
                ptScheme.EditorSettings.RampColors = true;
                ptScheme.EditorSettings.ClassificationType = ClassificationType.Quantities;
                ptScheme.EditorSettings.IntervalMethod = IntervalMethod.EqualInterval;
                ptScheme.EditorSettings.NumBreaks = lBreaks;
                ptScheme.EditorSettings.FieldName = syear;
                ptScheme.EditorSettings.IntervalSnapMethod = IntervalSnapMethod.Rounding;
                ptScheme.AppearsInLegend = true;
                IPolygonCategory pCat = null;
                double curMin = cMin;
                double curMax = cMin + diff;
                ptScheme.Categories.Clear();

                int intv = (int)(255.00 / (lBreaks - 1));
                for (int i = 0; i < lBreaks; i++)
                {
                    //pCat = new PointCategory(Color.FromArgb(255, 255, 0), DotSpatial.Symbology.PointShape.Ellipse, 4);
                    pCat = new PolygonCategory(Color.FromArgb(255, 255 - (i * intv), 0), Color.FromArgb(255, 255 - (i * intv), 0), 0);

                    string filter = "";
                    if (i == lBreaks - 1)
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<=" + curMax;
                    }
                    else
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<" + curMax;
                    }
                    pCat.FilterExpression = filter;
                    pCat.LegendText = curMin + " - " + curMax;
                    ptScheme.AddCategory(pCat);
                    curMin = curMax;
                    curMax = curMin + diff;
                }

                //ptScheme.EditorSettings.ClassificationType = ClassificationType.UniqueValues;
                //ptScheme.EditorSettings.FieldName = Indicator;
                //ptScheme.CreateCategories(fset.DataTable);
                return ptScheme;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Generating Point Symbology Scheme \r\n\r\n" + ex.Message + ex.StackTrace);
                return null;
            }
        }
        private PolygonScheme GeneratePolygonSymbologySchemeOLD(string syear, IFeatureSet fset)
        {
            PolygonScheme ptScheme = new PolygonScheme();

            int lBreaks = 0;
            if (selectedVar.Contains("PREC"))
                lBreaks = 10;
            else if (selectedVar.Contains("PEVT"))
                lBreaks = 4;
            else if (selectedVar.Contains("SOLR"))
                lBreaks = 5;

            //double cMin = 20.0;
            double cMin = (int)mndata;
            double cMax = (int)(mxdata + 1.0);

            double diff = (cMax - cMin) / (lBreaks);
            Debug.WriteLine("Min {0}, Max {1}, Diff {2}",
                 cMin.ToString(), cMax.ToString(), diff.ToString());

            try
            {
                ptScheme.LegendText = syear;
                ptScheme.EditorSettings.RampColors = true;
                ptScheme.EditorSettings.ClassificationType = ClassificationType.Quantities;
                ptScheme.EditorSettings.IntervalMethod = IntervalMethod.EqualInterval;
                ptScheme.EditorSettings.NumBreaks = lBreaks;
                ptScheme.EditorSettings.FieldName = syear;
                ptScheme.EditorSettings.IntervalSnapMethod = IntervalSnapMethod.Rounding;
                ptScheme.AppearsInLegend = true;
                IPolygonCategory pCat = null;
                double curMin = cMin;
                double curMax = cMin + diff;
                ptScheme.Categories.Clear();

                for (int i = 0; i < lBreaks; i++)
                {
                    //pCat = new PointCategory(Color.FromArgb(255, 255, 0), DotSpatial.Symbology.PointShape.Ellipse, 4);
                    switch (i)
                    {
                        //PointCategory(Color color, PointShape shape, double size);
                        case 0: //yellow to blue
                            pCat = new PolygonCategory(Color.FromArgb(255, 0, 0), Color.FromArgb(255, 0, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 2);
                            break;
                        case 1:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 195, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 50, 0), Color.FromArgb(255, 50, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 4);
                            break;
                        case 2:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 130, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 73, 0), Color.FromArgb(255, 73, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 6);
                            break;
                        case 3:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 65, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 96, 0), Color.FromArgb(255, 96, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 8);
                            break;
                        case 4:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 0, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 118, 0), Color.FromArgb(255, 118, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 10);
                            break;
                        case 5:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 0, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 141, 0), Color.FromArgb(255, 141, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 12);
                            break;
                        case 6:
                            //pCat = new PolygonCategory(Color.FromArgb(255, 0, 0), Color.Black, 1);
                            pCat = new PolygonCategory(Color.FromArgb(255, 164, 0), Color.FromArgb(255, 164, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 14);
                            break;
                        case 7:
                            pCat = new PolygonCategory(Color.FromArgb(255, 187, 0), Color.FromArgb(255, 187, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 16);
                            break;
                        case 8:
                            pCat = new PolygonCategory(Color.FromArgb(255, 209, 0), Color.FromArgb(255, 209, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 16);
                            break;
                        case 9:
                            pCat = new PolygonCategory(Color.FromArgb(255, 232, 0), Color.FromArgb(255, 232, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 16);
                            break;
                        case 10:
                            pCat = new PolygonCategory(Color.FromArgb(255, 255, 0), Color.FromArgb(255, 255, 0), 0);
                            //pCat = new PointCategory(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 16);
                            break;
                    }

                    string filter = "";
                    if (i == lBreaks - 1)
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<=" + curMax;
                    }
                    else
                    {
                        filter = "[" + syear + "]>=" + curMin + " AND [" + syear + "]<" + curMax;
                    }
                    pCat.FilterExpression = filter;
                    pCat.LegendText = curMin + " - " + curMax;
                    ptScheme.AddCategory(pCat);
                    curMin = curMax;
                    curMax = curMin + diff;
                }

                //ptScheme.EditorSettings.ClassificationType = ClassificationType.UniqueValues;
                //ptScheme.EditorSettings.FieldName = Indicator;
                //ptScheme.CreateCategories(fset.DataTable);
                return ptScheme;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Generating Point Symbology Scheme \r\n\r\n" + ex.Message + ex.StackTrace);
                return null;
            }
        }
        private bool RefreshPolygonLayer(string syear)
        {
            Debug.WriteLine("Entering Refresh Map ....");
            try
            {
                IFeatureSet fs = Annualfs;

                PolygonScheme ptScheme = GeneratePolygonSymbologyScheme(syear, fs);
                AnnualPolyLayer.Symbology = (IPolygonScheme)ptScheme;
                AnnualPolyLayer.LegendText = "Annual " + selectedParam;
                AnnualPolyLayer.DataSet.Name = "Annual_" + selectedVar;
                AnnualPolyLayer.LegendItemVisible = true;

                MapLabelLayer fslbl = new MapLabelLayer(fs);
                //fslbl.Symbology.Categories[0].Expression = "[Station_ID]";
                fslbl.Symbology.Categories[0].Expression = "[" + syear + "]";
                fslbl.Symbolizer.Orientation = ContentAlignment.MiddleCenter;
                fslbl.Symbolizer.PreventCollisions = false;
                fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
                AnnualPolyLayer.LabelLayer = fslbl;
                AnnualPolyLayer.ShowLabels = true;
                AnnualPolyLayer.IsSelected = true;


                fMain.appManager.Map.ViewExtents = AnnualPolyLayer.DataSet.Extent;
                fMain.appManager.Map.Refresh();

                ptScheme = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in loading annual series map! \r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }
        }

        #endregion "Mapping"

        #region "Events"
        private void chkShowTable_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowTable.Checked)
            {
                splitTableGraph.Panel1Collapsed = true;
                chkShowTable.Text = "Show Datatable";
            }
            else
            {
                splitTableGraph.Panel1Collapsed = false;
                chkShowTable.Text = "Hide Datatable";
            }
        }
        private void chkShowGraph_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void cboVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            //return if all series not loaded, isWDMloaded is true after call to GetWDMSeries
            selectedVarIndex = cboVar.SelectedIndex;
            if (selectedVarIndex <= 0)
            {
                chkShowGrpTable.Enabled = false;
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            chkShowGrpTable.Checked = false;
            numBin.Value = (int)numCat[selectedVarIndex];

            //continue if selectedVarIndex>0 and enable form controls
            selectedVar = WeaVars.ElementAt(selectedVarIndex);
            selectedUnit = WeaUnits.ElementAt(selectedVarIndex);
            selectedParam = WeaParam.ElementAt(selectedVarIndex);

            EnableControls(true);

            if (!FilterWDMSeriesForSelectedVar())
            {
                mssg = "WDM does not contain annual series for " + selectedVar + "! Please build series using " +
                        "WDMUtil or SARA.";
                MessageBox.Show(mssg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Cursor.Current = Cursors.Default;
        }
        private void optAnnual_CheckedChanged(object sender, EventArgs e)
        {
            //CheckPeriodOption();
        }
        private void splitTableGraph_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }
        private void optMonthly_CheckedChanged(object sender, EventArgs e)
        {
            //CheckPeriodOption();
        }
        private void zedSeries_MouseClick(object sender, MouseEventArgs e)
        {
            HighlightRecord(e);
        }
        private void zedSeries_MouseHover(object sender, EventArgs e)
        {
        }
        private void zedSeries_MouseEnter(object sender, EventArgs e)
        {
            //grpGraph.Text = "Hover to Show Point Values";
            zedSeries.IsShowPointValues = true;
        }
        private void frmSpatial_Load(object sender, EventArgs e)
        {
            this.Width = WidthCollapsed;
            this.Height = HeightCollapsed;
        }
        private void frmSpatial_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
        private void chkShowGrpTable_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowGrpTable.Checked)
            {
                this.Width = WidthExpand;
                this.Height = HeightExpand;
                chkShowTable.Visible = true;
            }
            else
            {
                this.Width = WidthCollapsed;
                this.Height = HeightCollapsed;
                chkShowTable.Visible = false;

            }
            this.Refresh();
        }

        private void zedSeries_MouseLeave(object sender, EventArgs e)
        {
            //grpGraph.Text = "";
        }

        private void chkSelected_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelected.Checked)
            {
                ShowSelected = true;
                fSelect.ShowDataGrid(tblSelect);
                fSelect.ShowDialog();
            }
            else
            {
                ShowSelected = false;
                fSelect.Hide();
            }
        }
        private void LabelDataPoint(MouseEventArgs e)
        {
            string station = string.Empty;
            System.Drawing.Point ept = e.Location;
            Debug.WriteLine("pt e.x=" + ept.X.ToString() + "pt e.y=" + ept.Y.ToString());

            if (e.Button.Equals(MouseButtons.Left))
            {
                Debug.WriteLine("pt e.x=" + e.X.ToString() + "pt e.y=" + e.Y.ToString());
                try
                {
                    CurveItem curve;
                    int iPt;

                    if (zedSeries.GraphPane.FindNearestPoint(new PointF(e.X, e.Y), out curve, out iPt))
                    {
                        //Debug.WriteLine(curve.Label + ": " + curve[iPt].ToString());
                        double X = curve[iPt].X;
                        double Y = curve[iPt].Y;

                        var dr = from DataRow drow in tblData.Rows
                                 where drow["DSN"].ToString().Contains(X.ToString())
                                 select drow;

                        if (!(dr == null))
                        {
                            //clear selection first
                            dgvSeaWDM.ClearSelection();

                            DataRow drow = dr.ElementAt(0);
                            string sta = drow["Station"].ToString();
                            grpGraph.Text = "Gage: " + sta;

                            //select row in datagrid
                            var sr = from DataGridViewRow srow in dgvSeaWDM.Rows
                                     where srow.Cells["Station"].Value.ToString().Equals(sta)
                                     select srow;
                            sr.ElementAt(0).Selected = true;
                            sr = null;
                        }
                        else
                        {
                            Debug.WriteLine("Cannot identify the huc12");
                        }
                        dr = null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in checking point!\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
                }
            }
        }
        private void HighlightRecord(MouseEventArgs e)
        {
            string station = string.Empty;
            System.Drawing.Point ept = e.Location;
            Debug.WriteLine("pt e.x=" + ept.X.ToString() + "pt e.y=" + ept.Y.ToString());

            if (e.Button.Equals(MouseButtons.Left))
            {
                Debug.WriteLine("pt e.x=" + e.X.ToString() + "pt e.y=" + e.Y.ToString());
                try
                {
                    CurveItem curve;
                    int iPt;

                    if (zedSeries.GraphPane.FindNearestPoint(new PointF(e.X, e.Y), out curve, out iPt))
                    {
                        //Debug.WriteLine(curve.Label + ": " + curve[iPt].ToString());
                        double X = curve[iPt].X;
                        double Y = curve[iPt].Y;

                        var dr = from DataRow drow in tblData.Rows
                                 where drow["DSN"].ToString().Contains(X.ToString())
                                 select drow;

                        if (!(dr == null))
                        {
                            //clear selection first
                            dgvSeaWDM.ClearSelection();

                            DataRow drow = dr.ElementAt(0);
                            string sta = drow["Station"].ToString();
                            //grpGraph.Text = "Gage: " + sta;

                            //select row in datagrid
                            var sr = from DataGridViewRow srow in dgvSeaWDM.Rows
                                     where srow.Cells["Station"].Value.ToString().Equals(sta)
                                     select srow;
                            sr.ElementAt(0).Selected = true;
                            sr = null;
                        }
                        else
                        {
                            Debug.WriteLine("Cannot identify the station!");
                        }
                        dr = null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in checking point!\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
                }
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void cboPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Selected index change " + cboPeriod.SelectedIndex.ToString());
            try
            {
                //return if first selection
                int selIndex = cboPeriod.SelectedIndex;
                if (selIndex <= 0)
                {
                    chkShowGrpTable.Enabled = false;
                    return;
                }

                chkShowGrpTable.Enabled = true;
                string syear = cboPeriod.Items[selIndex].ToString();
                Debug.WriteLine("Selected Year " + syear.ToString());

                RefreshPointLayer(syear);

                List<double> dsn = new List<double>();
                List<double> series = new List<double>();
                List<string> stname = new List<string>();

                int nrows = 0;
                foreach (DataGridViewRow dr in dgvSeaWDM.Rows)
                {
                    nrows++;
                    string id = dr.Cells["DSN"].Value.ToString();
                    string sta = dr.Cells["Station"].Value.ToString();
                    string sval = dr.Cells[syear].Value.ToString();
                    //exclude missing values of -999
                    if (!sval.Contains("-999"))
                    {
                        stname.Add(sta);
                        double ddsn = Convert.ToDouble(id);
                        dsn.Add(ddsn);
                        double val = Convert.ToDouble(sval);
                        series.Add(val);
                    }
                    id = null; sval = null;
                }

                double[] idsn = dsn.ToArray();
                double[] dseries = series.ToArray();
                dsn = null; series = null;

                CreateSeriesGraph(stname, idsn, dseries, syear);
            }
            catch (Exception ex)
            {
                mssg = "Error in generation map and graph!" + crlf + crlf + ex.Message + ex.StackTrace;
                MessageBox.Show(mssg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
#endregion "Events"
        public bool GetTimeSeriesFromWDM()
        {
            Debug.WriteLine("Spatial: Entering GetTimeSeriesFromWDM ...");
            Cursor.Current = Cursors.WaitCursor;
            mndata = 9999999.0; mxdata = 0.0;
            try
            {
                WDM cwdm = new WDM(WdmFile);
                int dsn = 0; string sta = string.Empty;
                SortedDictionary<DateTime, double> dictSeries =
                            new SortedDictionary<DateTime, double>();
                dictDataSeries.Clear();

                DateTime BegTime = DateTime.Now;
                DateTime EndTime = DateTime.Now.AddYears(-100);

                int icnt = 0;
                if (tblData.Rows.Count > 0) tblData.Rows.Clear();

                foreach (KeyValuePair<int, string> kv in dictDSN)
                {
                    icnt++;
                    dsn = kv.Key;
                    sta = kv.Value;
                    //Get series for dsn
                    dictSeries = cwdm.GetTimeSeries(dsn);
                    
                    //debug
                    foreach (KeyValuePair<DateTime, double> kva in dictSeries)
                        Debug.WriteLine(dsn.ToString(), kva.Key.ToString(), kva.Value.ToString());
                    
                    //6.7.2021
                    if (DateTime.Compare(dictSeries.Keys.First(), BegTime) <= 0)
                        BegTime = dictSeries.Keys.First();

                    if (DateTime.Compare(dictSeries.Keys.Last(), EndTime) >= 0)
                        EndTime = dictSeries.Keys.Last();

                    if (icnt == 1)
                    {
                        lstColNames.Clear();
                        StartYear = BegTime.Year - 1;
                        EndYear = EndTime.Year - 1;
                        Debug.WriteLine("Start {0}, End {1}", StartYear.ToString(), EndYear.ToString());
                        for (int i = StartYear; i <= EndYear; i++)
                            lstColNames.Add(i.ToString());
                        lstColNames.Add("Annual");

                        CreateTableOfTimeSeries();

                        cboPeriod.DataSource = null;
                        cboPeriod.DataSource = lstColNames;
                        cboPeriod.SelectedIndex = -1;
                    }

                    //debug
                    //Debug.WriteLine("Processing DSN={0},Station={1}, Beg={2}, End={3}", 
                    //    dsn.ToString(), sta,BegTime.ToShortDateString(), EndTime.ToShortDateString());

                    //save to global dictionary of series
                    //if (!dictDataSeries.ContainsKey(sta))
                    //    dictDataSeries.Add(sta, dictSeries);

                    //Fill table of annual series, tblData

                    DataRow dr = tblData.NewRow();
                    dr["DSN"] = dsn;
                    dr["Station"] = sta;

                    for (int i = 0; i < dictSeries.Count; i++)
                    {
                        int yr = (dictSeries.Keys.ElementAt(i).Year) - 1;
                        string val = dictSeries.Values.ElementAt(i).ToString("F2");
                        double dval = 0.0;
                        dval = Convert.ToDouble(val);
                        val = dval.ToString("F1");
                        string scol = lstColNames.ElementAt(i);
                        dr[scol] = val;
                    }

                    double mean = GetAnnualStatistics(dictSeries.Values.ToList());
                    double dmin = MathNet.Numerics.Statistics.Statistics.Minimum(dictSeries.Values.ToList());
                    double dmax = MathNet.Numerics.Statistics.Statistics.Maximum(dictSeries.Values.ToList());

                    mndata = (mndata < dmin ? mndata : dmin);
                    mxdata = (mxdata > dmax ? mxdata : dmax);
                    //Debug.WriteLine("Min = {0}, Max = {1}", mndata.ToString(), mxdata.ToString());

                    dr["Annual"] = mean.ToString("F2");

                    tblData.Rows.Add(dr);
                    dr = null;
                }

                dictSeries = null;
                cwdm = null;

                dgvSeaWDM.DataSource = null;
                dgvSeaWDM.DataSource = tblData;
                dgvSeaWDM.Columns["DSN"].Visible = false;
                //dgvSeaWDM.Columns["Label"].Visible = false;
                dgvSeaWDM.ClearSelection();

                //draw grid layer
                ClearMapOfAnnualSeries();
                DrawAnnualGridLayer();
                //DrawPolygonLayer();
            }
            catch (Exception ex)
            {
                string msg = "Error getting timeseries from WDM!" + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor.Current = Cursors.Default;
            return true;
        }
    }
}
