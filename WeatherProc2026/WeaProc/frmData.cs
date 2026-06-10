using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using wdmuploader;
using WeaWDM;

namespace NCEIData
{
    public partial class frmData : Form
    {
        private WeaSeries climateSeries;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss;
        private SortedDictionary<string, Dictionary<string, List<string>>> dicMissCnt;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> dictModel;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> dictHMoments;
        //private SortedDictionary<string, List<string>> dicSelectedVars;
        public SortedDictionary<string, List<string>> dictSiteVars;

        private List<string> lstSta = new List<string>();
        private List<string> lstStaName = new List<string>();
        private Dictionary<string, bool> OptVars;
        private List<string> lstStaDownloaded = new List<string>();
        private List<string> lstSelectedVars = new List<string>();
        private List<string> lstSelectedVarsRev = new List<string>();
        private List<string> lstEstimated = new List<string>();
        public List<string> lstSiteVarsEstimated = new List<string>();
        private List<string> lstVars = new List<string>()
                     {"PREC","ATEM","WIND","WNDD",
                      "CLOU","DEWP","SOLR","ATMP",
                      "TMAX","TMIN","PRCP"};
        private string SelectedSta, SelectedVar;
        private SortedDictionary<string, List<string>> dictSta = new SortedDictionary<string, List<string>>();
        private Dictionary<string, List<string>> dictWDMds = new Dictionary<string, List<string>>();
        private SortedDictionary<string, MetGages> dictSelSites =
                    new SortedDictionary<string, MetGages>();

        //dictionary of gages keyed on variable and sortedDictionary of dsn and Station info
        public Dictionary<string, SortedDictionary<int, clsStation>> dictWDMGages = new
             Dictionary<string, SortedDictionary<int, clsStation>>();

        private enum EstimateMiss { Interpolate, OtherGage };
        private enum EstimateSite { All, BySite };
        private int EstimateMethod = (int)EstimateMiss.Interpolate;
        private int EstimateSites = (int)EstimateSite.All;
        private frmMain fMain;
        private int optDataSource;
        private int maxHours;
        private double PercentMiss;
        private const string MISS = "9999";
        private enum DataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6 };
        clsEstimate cEstimate;
        string errMsg;

        private clsGraph tsGraph;
        private bool WithMiss = false;
        private TreeNode SelectedNode;
        private string WdmFile, tempWDM, cachePath;
        private string TimeUnit;
        public double SearchRadius;
        public int LimitStations;
        private string crlf = Environment.NewLine;
        private StreamWriter wrs;
        private DateTime BegDateTime, EndDateTime;

        public frmData(frmMain _fMain,
            SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> _dictMiss,
            SortedDictionary<string, Dictionary<string, SortedDictionary<string, double>>> _dictModel,
            SortedDictionary<string, Dictionary<string, SortedDictionary<Int32, List<double>>>> _dictHMoments,
            SortedDictionary<string, Dictionary<string, List<string>>> _dicMissCnt,
            SortedDictionary<string, List<string>> _dicSelectedVars,
            List<string> _lstSelectedVars,
            Dictionary<string, bool> _optVars)
        {
            InitializeComponent();

            this.fMain = _fMain;
            this.climateSeries = _fMain.weatherTS;
            //Debug.WriteLine("Num Sites = " + climateSeries.NumSites().ToString());
            this.dictMiss = _dictMiss;
            this.dictSta = _fMain.dictSta;
            this.dictModel = _dictModel;
            this.WdmFile = fMain.WdmFile;
            this.tempWDM = fMain.WdmFile;
            this.cachePath = fMain.cacheDir;
            this.optDataSource = fMain.optDataSource;
            this.BegDateTime = fMain.BegDateTime;
            this.EndDateTime = fMain.EndDateTime;

            if (optDataSource == (int)DataSource.ISD)
            {
                this.dictHMoments = _dictHMoments;
                this.lblGages.Text = "Gages";
                this.Text = "Selected ISD Dataseries";
                tabData.TabPages[0].Text = "Hourly";
                splitDataView.Panel2Collapsed = false;
                splitDataTable.Panel2Collapsed = false;
                TimeUnit = "Hour";
            }
            else if (optDataSource == (int)DataSource.GHCN)
            {
                this.dictHMoments = _dictHMoments;
                this.lblGages.Text = "Gages";
                this.Text = "Selected GHCN Dataseries";
                tabData.TabPages[0].Text = "Daily";
                splitDataView.Panel2Collapsed = false;
                splitDataTable.Panel2Collapsed = false;
                grpMissing.Enabled = true;
                TimeUnit = "Day";
            }
            else if (optDataSource == (int)DataSource.HRAIN)
            {
                this.lblGages.Text = "Gages";
                this.Text = "Selected Hourly Rainfall Dataseries";
                tabData.TabPages[0].Text = "Hourly";
                splitDataView.Panel2Collapsed = false;
                splitDataTable.Panel2Collapsed = false;
                TimeUnit = "Hour";
            }
            else if (optDataSource == (int)DataSource.NLDAS)
            {
                this.lblGages.Text = "Grid";
                this.Text = "Selected Hourly NLDAS Dataseries";
                tabData.TabPages[0].Text = "Hourly";
                splitDataView.Panel2Collapsed = true;
                splitDataTable.Panel2Collapsed = true;

                if (tabData.Contains(tabPageMiss))
                    tabData.TabPages.Remove(tabPageMiss);
                TimeUnit = "Hour";
                fMain.scenario = "NLDAS";
            }
            else if (optDataSource == (int)DataSource.GLDAS)
            {
                this.lblGages.Text = "Grid";
                this.Text = "Selected Hourly GLDAS Dataseries";
                tabData.TabPages[0].Text = "Hourly";
                splitDataView.Panel2Collapsed = true;
                splitDataTable.Panel2Collapsed = true;

                if (tabData.Contains(tabPageMiss))
                    tabData.TabPages.Remove(tabPageMiss);
                TimeUnit = "Hour";
                fMain.scenario = "GLDAS";
            }
            else if (optDataSource == (int)DataSource.TRMM)
            {
                this.lblGages.Text = "Grid";
                this.Text = "Selected Hourly TRMM Dataseries";
                tabData.TabPages[0].Text = "Hourly";
                splitDataView.Panel2Collapsed = true;
                splitDataTable.Panel2Collapsed = true;

                if (tabData.Contains(tabPageMiss))
                    tabData.TabPages.Remove(tabPageMiss);
                TimeUnit = "Hour";
                fMain.scenario = "TRMM";
            }
            else if (optDataSource == (int)DataSource.CMIP6)
            {
                this.lblGages.Text = "Grid";
                this.Text = "Selected Daily Climate Scenario Dataseries";
                tabData.TabPages[0].Text = "Daily";
                splitDataView.Panel2Collapsed = true;
                splitDataTable.Panel2Collapsed = true;

                if (tabData.Contains(tabPageMiss))
                    tabData.TabPages.Remove(tabPageMiss);
                TimeUnit = "Day";
            }

            this.dicMissCnt = _dicMissCnt;
            //dictionary of processed site variables
            this.dictSiteVars = fMain.dictSiteVars;
            lstSta = dictSiteVars.Keys.ToList();
            //dictionary of sites selected from map
            this.dictSelSites = fMain.dictSelSites;
            this.lstSelectedVars = _lstSelectedVars;
            this.OptVars = _optVars;
            this.lstStaName = fMain.lstStaName;
            this.optDataSource = fMain.optDataSource;
            this.PercentMiss = fMain.PercentMiss;
            this.SearchRadius = Convert.ToDouble(numRadius.Value);
            this.LimitStations = Convert.ToInt32(numMaxStations.Value);

            optOther.Enabled = true;
            //cboOther.Enabled = true;
            tabData.SelectedTab = tabPageData;
            dgvMissCnt.ClearSelection();
            optInter.Checked = true;
            optAll.Checked = true;
            EstimateMethod = (int)EstimateMiss.Interpolate;
            EstimateSites = (int)EstimateSite.All;
            chkShowData.Checked = false;

            if (tabData.Contains(tabPageDaily))
                tabData.TabPages.Remove(tabPageDaily);

            //debug
            //DebugSiteVars();
            //dictWDMds = GetWDMDataSets();

            //Get attributes of WDM series, returns true if with NLDAS or GLDAS data
            //false otherwise; enable spatial interpolation if with NLDAS/GLDAS
            if (ReadWDMGageAttributes())
                EnableSpatial(true);
            else
                EnableSpatial(false);

            BuildDataTreeView();

            //initialize zedgraph
            tsGraph = new clsGraph(zgvSeries);
        }

        private void EnableSpatial(bool isEnabled)
        {
            optOther.Enabled = isEnabled;
            numMaxStations.Enabled = isEnabled;
            lblnoSta.Enabled = isEnabled;
            lbRadius.Enabled = false;
            numRadius.Enabled = false;
        }
        private void DebugSiteVars()
        {
            foreach (KeyValuePair<string, List<string>> kv in dictSiteVars)
            {
                WriteLogFile("Site:" + kv.Key);
                List<string> svars = kv.Value.ToList();
                string st = string.Empty;
                foreach (var s in kv.Value.ToList())
                    st += s + ",";
                WriteLogFile("Variables in Site: " + st.Substring(0, st.Length - 1));
            }
        }
        private Dictionary<string, List<string>> GetWDMDataSets()
        {
            clsWdm cTmpWDM;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                cTmpWDM = new clsWdm(WdmFile, optDataSource);
                dictWDMds = cTmpWDM.GetListOfWDMDatasets();
                cTmpWDM = null;

                //debug
                Debug.WriteLine("Entering GetWDMDataSets in frmData...");
                foreach (KeyValuePair<string, List<string>> kv in dictWDMds)
                {
                    //Debug.WriteLine("Site = " + kv.Key);
                    foreach (var item in kv.Value)
                        Debug.WriteLine("Site {0} : Variable {1}", kv.Key, item.ToString());
                }
                return dictWDMds;
            }
            catch (Exception ex)
            {
                string msg = "Error getting datasets attributes from " + WdmFile;
                ShowError(msg, ex);
                return null;
            }
        }
        private bool ReadWDMGageAttributes()
        {
            bool isLDAS = false;
            try
            {
                fMain.WriteStatus("Reading " + WdmFile + " : Getting timeseries attributes ...");
                fMain.WriteLogFile("Reading " + WdmFile + " : Getting timeseries attributes ...");

                WDM cWDM = new WDM(WdmFile, lstVars);
                if (!cWDM.GetWDMAttributes(TimeUnit))
                {
                    string msg = "Error getting timeseries attributes from " + WdmFile;
                    fMain.WriteLogFile(msg);
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cWDM = null;
                    return false;
                }
                //dictNearbyGage is key=variable, value=dictionary of station info, key=dsn,value-clsStation
                dictWDMGages = cWDM.dictWDMSeries();
                isLDAS = cWDM.HasLDAS();
                cWDM = null;

                //debug time series in WDM
                //ListWDMSeries();
            }
            catch (Exception ex)
            {
                string msg = "Error getting timeseries attributes from " + WdmFile;
                fMain.WriteLogFile(msg + crlf + ex.Message + crlf + ex.StackTrace);
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return isLDAS;
        }
        private void ListWDMSeries()
        {
            fMain.WriteLogFile("Timeseries in WDMFile: " + WdmFile);
            foreach (KeyValuePair<string, SortedDictionary<int, clsStation>> kv in dictWDMGages)
            {
                SortedDictionary<int, clsStation> gage;
                dictWDMGages.TryGetValue(kv.Key, out gage);
                foreach (KeyValuePair<int, clsStation> kv1 in gage)
                {
                    clsStation sta = kv1.Value;
                    string msg = kv.Key + ", " + kv1.Key.ToString() + ", " + sta.Station + ", " +
                        sta.BegDate.ToString() + ", " + sta.EndDate.ToString();
                    sta = null;
                    fMain.WriteLogFile(msg);
                }
                gage = null;
            }
        }

        private void BuildDataTreeView()
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> siteAttrib = new List<string>();
            MetGages gage = new MetGages();
            List<string> siteVars;
            List<string> lstOfSta;

            //Clear the TreeView each time the method is called.
            dataTree.Nodes.Clear();
            try
            {
                dataTree.BeginUpdate();

                lstOfSta = dictSiteVars.Keys.ToList();
                //foreach (string s in lstOfSta)
                //    Debug.WriteLine("In frmData: Site ID = " + s);

                //return if no stations
                if (lstOfSta.Count == 0) return;

                //if (lstOfSta.Count > 0)
                //foreach (var sta in lstOfSta)
                //{
                //attrib.Add(dr["Station"].ToString());
                //attrib.Add(dr["Elev"].ToString());
                //attrib.Add(dr["Lat"].ToString());
                //attrib.Add(dr["Lon"].ToString());

                //dictSta.TryGetValue(sta, out siteAttrib);
                //string stname = siteAttrib.ElementAt(0);
                //Debug.WriteLine("In buildtree: sta = {0},  Name = {1}", sta, stname);
                //dictSelSites.TryGetValue(sta, out gage);                    
                //}

                foreach (var sta in lstOfSta)
                {
                    dictSelSites.TryGetValue(sta, out gage);
                    string stname;
                    if (optDataSource == (int)DataSource.NLDAS || optDataSource == (int)DataSource.GLDAS ||
                        optDataSource == (int)DataSource.TRMM)
                        stname = sta;
                    else
                        stname = StationName(sta);

                    siteVars = new List<string>();
                    dictSiteVars.TryGetValue(sta, out siteVars);
                    if (siteVars.Count > 0)
                    {
                        //add node if there are variables in the station
                        //stname is name of station, actually the description when series is uploaded
                        dataTree.Nodes.Add(new TreeNode(stname));

                        // Add the variable nodes
                        foreach (string svar in siteVars)
                        {
                            Debug.WriteLine("Treenode: " + sta + "." + svar);
                            dataTree.Nodes[lstOfSta.IndexOf(sta)].Nodes.Add(
                            new TreeNode(sta + "." + svar));
                        }
                    }
                }

                dataTree.ExpandAll();
                // Begin repainting the TreeView.

                dataTree.EndUpdate();
                // Reset the cursor to the default for all controls.

                //TreeNode node =
                dataTree.SelectedNode = dataTree.Nodes[0].Nodes[0];
                SelectedNode = dataTree.Nodes[0].Nodes[0];

                siteAttrib = null;
                siteVars = null;
                gage = null;
                lstOfSta = null;
            }
            catch (Exception ex)
            {
                errMsg = "Error generating treeview of gages series!";
                ShowError(errMsg, ex);
            }
            Cursor.Current = Cursors.Default;
        }
        private string StationName(string sta)
        {
            string stname = string.Empty;
            List<string> siteAttrib = new List<string>();
            dictSta.TryGetValue(sta, out siteAttrib);
            stname = siteAttrib.ElementAt(0);
            siteAttrib = null;
            return stname;
        }
        private DataTable GetSeriesTables(string sta, string svar)
        {
            DataTable dtSeries = new DataTable();
            try
            {
                dtSeries.Columns.Add("DateTime", typeof(DateTime));
                dtSeries.Columns.Add(svar, typeof(string));

                //foreach (KeyValuePair<DateTime, string> kv in GetSeriesFromCSV(sta, svar))
                //foreach (KeyValuePair<DateTime, string> kv in GetSeries(sta, svar))
                if (optDataSource==(int)DataSource.GHCN)
                {
                    foreach (KeyValuePair<DateTime, string> kv in GetSeriesFromDict(sta, svar))
                    {
                        DataRow dr = dtSeries.NewRow();
                        string svalue = string.Empty;
                        if (kv.Value.Contains(MISS))
                            svalue = MISS;
                        else
                            svalue = kv.Value;
                        dtSeries.Rows.Add(kv.Key, svalue);
                        dr = null;
                    }
                }
                else
                {
                    foreach (KeyValuePair<DateTime, string> kv in GetSeries(sta, svar))
                    {
                        DataRow dr = dtSeries.NewRow();
                        string svalue = string.Empty;
                        if (kv.Value.Contains(MISS))
                            svalue = MISS;
                        else
                            svalue = kv.Value;
                        dtSeries.Rows.Add(kv.Key, svalue);
                        dr = null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return dtSeries;
        }
        private DataTable GetMissingTables(string sta, string svar)
        {
            SortedDictionary<DateTime, string> MissingSeries;

            if (optDataSource==(int)DataSource.GHCN)
                MissingSeries = GetMissingSeriesFromDict(sta, svar);
            else
                MissingSeries = GetMissingSeries(sta, svar);

            if (!(MissingSeries == null))
            {
                DataTable dtMiss = new DataTable();
                dtMiss.Columns.Add("DateTime", typeof(DateTime));
                dtMiss.Columns.Add(svar, typeof(string));

                foreach (KeyValuePair<DateTime, string> kv in MissingSeries)
                {
                    DataRow dr = dtMiss.NewRow();
                    dtMiss.Rows.Add(kv.Key, kv.Value.ToString());
                    dr = null;
                }
                return dtMiss;
            }
            else //no missing data
                return null;
        }
        private void HighlightMissing(string sta, string svar)
        {
            try
            {
                if (dgvMiss.Rows.Count > 0)
                {
                    foreach (DataGridViewRow dr in dgvData.Rows)
                    {
                        string sdt = dr.Cells[0].Value.ToString();
                        DateTime dt = DateTime.Parse(sdt);

                        foreach (DataGridViewRow drow in dgvMiss.Rows)
                        {
                            DateTime ldt = DateTime.Parse(drow.Cells[0].Value.ToString());
                            if (dt.CompareTo(ldt) == 0)
                                dr.Cells[1].Style.BackColor = System.Drawing.Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error in highlighting missing record!", ex);
            }
        }
        private DataTable GetDailyStatsTables(string sta, string svar)
        {
            SortedDictionary<DateTime, double> DailyStatsSeries = GetDailyStatsSeries(sta, svar);

            if (!(DailyStatsSeries == null))
            {
                DataTable dtSeries = new DataTable();
                dtSeries.Columns.Add("DateTime", typeof(DateTime));
                dtSeries.Columns.Add(svar, typeof(string));

                foreach (KeyValuePair<DateTime, double> kv in DailyStatsSeries)
                {
                    DataRow dr = dtSeries.NewRow();
                    dtSeries.Rows.Add(kv.Key, kv.Value.ToString());
                    dr = null;
                }
                return dtSeries;
            }
            else //no missing data
                return null;
        }
        private DataTable GetMonthlyStatsTables(string sta, string svar)
        {
            SortedDictionary<DateTime, double> MonthlyStatsSeries = GetMonthlyStatsSeries(sta, svar);

            if (!(MonthlyStatsSeries == null))
            {
                DataTable dtSeries = new DataTable();
                dtSeries.Columns.Add("DateTime", typeof(DateTime));
                dtSeries.Columns.Add(svar, typeof(string));

                foreach (KeyValuePair<DateTime, double> kv in MonthlyStatsSeries)
                {
                    DataRow dr = dtSeries.NewRow();
                    dtSeries.Rows.Add(kv.Key, kv.Value.ToString());
                    dr = null;
                }
                return dtSeries;
            }
            else //no missing data
                return null;
        }
        /// <summary>
        /// GetSeries
        /// </summary>
        /// <param name="sta"></param>
        /// <param name="svar"></param>
        /// <returns></returns>
        private SortedDictionary<DateTime, string> GetSeries(string sta, string svar)
        {
            Debug.WriteLine("Entering GetSeries ...");
            try
            {
                SortedDictionary<DateTime, string> dictVarSeries;

                clsWdm cTmpWDM = new clsWdm(tempWDM, optDataSource);
                dictVarSeries = cTmpWDM.ReadWeatherSeries(sta, svar);
                cTmpWDM = null;
                return dictVarSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from wdm file " + WdmFile + "!", ex);
                return null;
            }
        }

        private SortedDictionary<DateTime, string> GetSeriesFromDict(string sta, string svar)
        {
            Debug.WriteLine("Entering GetSeriesFromDict ...");
            try
            {
                SortedDictionary<DateTime, string> dictSeries = new SortedDictionary<DateTime, string>();

                dictSeries = climateSeries.GetSeries(sta, svar);
                return dictSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting weather series from dictionary!", ex);
                return null;
            }
        }
        /// <summary>
        /// GetSeriesFromCSV
        /// Reads hourly data from hly.csv and returns as a sorted dictionary of datetime-value
        /// </summary>
        /// <param name="sta"></param> station ID
        /// <param name="svar"></param> variable
        /// <returns></returns>
        //private SortedDictionary<DateTime, string> GetSeriesFromCSV(string sta, string svar)
        //{
        //string csvFile = Path.Combine(fMain.dataDir, string.Concat(sta, "_hly.csv"));
        //try
        //{
        //Dictionary<string, SortedDictionary<DateTime, string>> dictStaSeries;
        //SortedDictionary<DateTime, string> dictVarSeries;

        //clsCsvReader csvRead = new clsCsvReader(fMain, csvFile);
        //dictVarSeries = csvRead.ReadCsvData(svar);
        //csvRead = null;
        //dictStaSeries.TryGetValue(sta, out dictVarSeries);
        //dictStaSeries = null;
        //return dictVarSeries;
        //}
        //catch (Exception ex)
        //{
        //ShowError("Error in getting weather series from csv file " + csvFile + "!", ex);
        //return null;
        //}
        //}
        private SortedDictionary<DateTime, string> GetMissingSeries(string sta, string svar)
        {
            SortedDictionary<DateTime, string> dictMissSeries;
            Dictionary<string, SortedDictionary<DateTime, string>> dictStaSeries;
            try
            {
                //no missing series for nldas,gldas,trmm, just return null
                if (optDataSource == (int)DataSource.NLDAS || optDataSource == (int)DataSource.GLDAS
                    || optDataSource == (int)DataSource.TRMM)
                    return null;
                if (dictMiss.TryGetValue(sta, out dictStaSeries))
                {
                    if (dictStaSeries.TryGetValue(svar, out dictMissSeries))
                        return dictMissSeries;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                ShowError("Error in retreiving missing series!", ex);
                return null;
            }
        }

        private SortedDictionary<DateTime, string> GetMissingSeriesFromDict(string sta, string svar)
        {
            Debug.WriteLine("Entering GetMissingSeriesFromDict ...");
            try
            {
                SortedDictionary<DateTime, string> missSeries= new SortedDictionary<DateTime, string>(); 

                missSeries = climateSeries.GetMissSeries(sta, svar);
                Debug.WriteLine("Num missing series values = " + missSeries.Keys.Count.ToString());
                return missSeries;
            }
            catch (Exception ex)
            {
                ShowError("Error in getting mising series from dictionary!", ex);
                return null;
            }
        }

        private SortedDictionary<DateTime, double> GetDailyStatsSeries(string sta, string svar)
        {
            //SortedDictionary<DateTime, double> dictSeries;
            //Dictionary<string, SortedDictionary<DateTime, double>> dictStaSeries;
            //dlyStats.TryGetValue(sta, out dictStaSeries);
            //if (dictStaSeries.TryGetValue(svar, out dictSeries))
            //    return dictSeries;
            //else
            return null;
        }
        private SortedDictionary<DateTime, double> GetMonthlyStatsSeries(string sta, string svar)
        {
            //SortedDictionary<DateTime, double> dictSeries;
            //Dictionary<string, SortedDictionary<DateTime, double>> dictStaSeries;
            //monStats.TryGetValue(sta, out dictStaSeries);
            //if (dictStaSeries.TryGetValue(svar, out dictSeries))
            //    return dictSeries;
            //else
            return null;
        }
        
        private void DisplayStationTable(string sta, string svar)
        {
            Debug.WriteLine(crlf + "Entering DisplayStationTable...");

            DataTable sTable = GetSeriesTables(sta, svar);
            dgvData.DataSource = null;
            dgvData.DataSource = sTable;
            sTable = null;

            //HighlightMissing(sta, svar);
        }
        private void DisplayMissingTable(string sta, string svar)
        {
            Debug.WriteLine(crlf + "Entering DisplayMissingTable...");
            DataTable mTable = GetMissingTables(sta, svar);
            if (!(mTable == null))
            {
                dgvMiss.DataSource = null;
                dgvMiss.DataSource = mTable;
            }
            else
                dgvMiss.DataSource = null;
            mTable = null;
        }
        private void ShowMissingCountTable(string sta, string svar)
        {
            try
            {
                //dictionary for station sta
                Dictionary<string, List<string>> dicCnt;
                dicMissCnt.TryGetValue(sta, out dicCnt);

                //counts for variable svar
                List<string> lstCnt = new List<string>();
                dicCnt.TryGetValue(svar, out lstCnt);

                DataTable dtbl = new DataTable();
                dtbl.Columns.Add("Missing", typeof(string));
                dtbl.Columns.Add(svar, typeof(string));

                DataRow dr = dtbl.NewRow();
                dtbl.Rows.Add("Num Series", lstCnt.ElementAt(0));
                dtbl.Rows.Add("Missing", lstCnt.ElementAt(1));
                dtbl.Rows.Add("Max Missing", lstCnt.ElementAt(2));
                dtbl.Rows.Add("Percent Miss", lstCnt.ElementAt(3));

                dgvMissCnt.DataSource = null;
                dgvMissCnt.DataSource = dtbl;
                dtbl = null;

            }
            catch (Exception ex)
            {
                ShowError("Error getting missing counts!", ex);
            }
        }
        private void EstimateMissingData()
        {
            //allows re-estimation, switching between stochastic and spatial
            string site = string.Empty;
            //lstEstimated.Clear();
            bool isGraphMiss = true;
            try
            {
                switch (EstimateSites)
                {
                    case (int)EstimateSite.All:
                        switch (EstimateMethod)
                        {
                            case (int)EstimateMiss.Interpolate:
                                foreach (string sta in lstSta)
                                {
                                    //if (!lstEstimated.Contains(sta))
                                    {
                                        if (EstimateByInterpolate(sta))
                                        {
                                            isGraphMiss = true;
                                            if (!lstEstimated.Contains(sta))
                                                lstEstimated.Add(sta);
                                        }
                                        else
                                            isGraphMiss = false;
                                    }
                                }
                                break;

                            case (int)EstimateMiss.OtherGage:
                                foreach (string sta in lstSta)
                                {
                                    //if (!lstEstimated.Contains(sta))
                                    {
                                        if (EstimateBySpatial(wrs, sta))
                                        {
                                            isGraphMiss = true;
                                            if (!lstEstimated.Contains(sta))
                                                lstEstimated.Add(sta);
                                        }
                                        else
                                            isGraphMiss = false;
                                    }
                                }
                                break;
                        }
                        break;

                    case (int)EstimateSite.BySite:
                        switch (EstimateMethod)
                        {
                            case (int)EstimateMiss.Interpolate:
                                site = SelectedSta;
                                //if (!lstEstimated.Contains(site))
                                {
                                    if (EstimateByInterpolate(site))
                                    {
                                        isGraphMiss = true;
                                        if (!lstEstimated.Contains(site))
                                            lstEstimated.Add(site);
                                    }
                                    else
                                        isGraphMiss = false;
                                }
                                break;

                            case (int)EstimateMiss.OtherGage:
                                site = SelectedSta;
                                //if (!lstEstimated.Contains(site))
                                {
                                    if (EstimateBySpatial(wrs, site))
                                    {
                                        isGraphMiss = true;
                                        if (!lstEstimated.Contains(site))
                                            lstEstimated.Add(site);
                                    }
                                    else
                                        isGraphMiss = false;
                                }
                                break;
                        }
                        break;
                }
                RefreshDataTable(isGraphMiss);
            }
            catch (Exception ex)
            {
            }
        }
        private bool UploadToWdm(string site)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;

                //Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea =
                //      new Dictionary<string, SortedDictionary<DateTime, string>>();

                //dictData.TryGetValue(site, out dictSiteWea);
                //List<string> siteAttrib = new List<string>();
                //fMain.dictSta.TryGetValue(site, out siteAttrib);

                //debug
                //foreach (var item in siteAttrib)
                //    Debug.WriteLine("{0},{1}", site, item.ToString());

                //clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib,
                //                         dictSiteWea, WdmFile, fMain.optDataSource);
                //cWDM.UploadSeriesToWDM(site, WdmFile);
                //cWDM = null;
                //dictSiteWea = null;
                //siteAttrib = null;
                //Cursor.Current = Cursors.Default;

                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in uploading timeseries for site:" + site;
                ShowError(msg, ex);
                return false;
            }
        }
        private void RefreshDataTable(bool isGraphMiss)
        {
            Debug.WriteLine(crlf + "Entering RefreshDataTable...");
            //get index
            DisplayStationTable(SelectedSta, SelectedVar);
            if (optDataSource == (int)DataSource.NLDAS || optDataSource == (int)DataSource.GLDAS ||
                optDataSource == (int)DataSource.TRMM)
            {
                //for NLDAS/GLDAS don't show missing
                GenerateGraph(false, SelectedSta, SelectedVar);
            }
            else
            {
                DisplayMissingTable(SelectedSta, SelectedVar);
                GenerateGraph(isGraphMiss, SelectedSta, SelectedVar);
            }
        }
        private bool EstimateByInterpolate(string site)
        {
            Debug.WriteLine(crlf + "Entering EstimateByIntepolate...");
            try
            {
                cEstimate = new clsEstimate(fMain, site, dictMiss, dictModel, dictHMoments, dictWDMGages, TimeUnit);
                cEstimate.FormData = this;
                Dictionary<string, SortedDictionary<DateTime, string>> dicFillSite =
                                new Dictionary<string, SortedDictionary<DateTime, string>>();
                dicFillSite = cEstimate.FillMissingDataByModel(site);
                if (!(dicFillSite == null))
                {
                    //replacement already in FillMissingDataByModel
                    //cEstimate.ReplaceMissingData(site, dicFillSite);
                    //refresh dictMiss table
                    if (optDataSource==(int)DataSource.GHCN)
                        climateSeries.ReplaceMissingSeriesForSite(site, dicFillSite);
                    else
                    {
                        dictMiss.Remove(site);
                        dictMiss.Add(site, dicFillSite);
                    }
                    dicFillSite = null;
                    cEstimate = null;
                    return true;
                }
                else
                {
                    //do nothing on dicFillSite
                    cEstimate = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = "Error estimating missing records using model for " + site;
                ShowError(errMsg, ex);
                return false;
            }
        }
        private bool EstimateBySpatial(StreamWriter wrs, string site)
        {
            Debug.WriteLine(crlf + "Entering EstimateBySpatial...");
            //check
            if (!(dictWDMGages.Count > 0))
            {
                string msg = "No available nearby station for spatial interpolation!" + crlf +
                    "Please estimate missing records using stochastic model.";
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                cEstimate = new clsEstimate(fMain, site, dictMiss, dictModel, dictHMoments, dictWDMGages, TimeUnit);
                cEstimate.FormData = this;
                cEstimate.RadiusOfSearch = SearchRadius;
                cEstimate.LimitNumOfStations = LimitStations;
                Dictionary<string, SortedDictionary<DateTime, string>> dicFillSite =
                                new Dictionary<string, SortedDictionary<DateTime, string>>();
                dicFillSite = cEstimate.FillMissingDataBySpatial(wrs, site);
                if (!(dicFillSite == null))
                {
                    //replacement already in FillMissingDataBySpatial
                    //cEstimate.ReplaceMissingData(site, dicFillSite);
                    //refresh dictMiss table
                    //dictMiss.Remove(site);
                    //dictMiss.Add(site, dicFillSite);
                    //10-04-23
                    if (optDataSource == (int)DataSource.GHCN)
                        climateSeries.ReplaceMissingSeriesForSite(site, dicFillSite);
                    else
                    {
                        dictMiss.Remove(site);
                        dictMiss.Add(site, dicFillSite);
                    }
                    dicFillSite = null;
                    cEstimate = null;
                    return true;
                }
                else
                {
                    //do nothing
                    cEstimate = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = "Error estimating missing records using model for " + site;
                ShowError(errMsg, ex);
                return false;
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void btnEstimate_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (EstimateMethod)
                {
                    case (int)EstimateMiss.Interpolate:
                        break;
                    case (int)EstimateMiss.OtherGage:
                        //logfile
                        double begDt = BegDateTime.Date.ToOADate();
                        double endDt = EndDateTime.Date.ToOADate();
                        string spath = Path.Combine(Application.StartupPath, "data");
                        string sfile = "SpatialEstimateIDW_" + begDt.ToString() + "_" + endDt.ToString() + ".log";

                        if (wrs == null)
                        {
                            wrs = new StreamWriter(Path.Combine(spath, sfile), true);
                            wrs.AutoFlush = true;
                        }
                        wrs.WriteLine(crlf + "SPATIAL MODEL Validation: " + BegDateTime.ToShortDateString() + " - " +
                              EndDateTime.ToShortDateString() + " : " + DateTime.Now.ToString());
                        spath = null; sfile = null;
                        break;
                }

                EstimateMissingData();

                //wrs.Flush();
                //wrs.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error estimating missing records!", ex);
            }
            Cursor.Current = Cursors.Default;
        }
        private void optInter_CheckedChanged(object sender, EventArgs e)
        {
            if (optInter.Checked)
            {
                EstimateMethod = (int)EstimateMiss.Interpolate;
                Debug.WriteLine("Estimate using Stochastic Model ...");
            }
            else
                Debug.WriteLine("Estimate using Spatial Interpolation ...");
        }
        private void optOther_CheckedChanged(object sender, EventArgs e)
        {
            if (optOther.Checked)
            {
                EstimateMethod = (int)EstimateMiss.OtherGage;
                Debug.WriteLine("Estimate using Spatial Interpolation ...");
                //cboOther.DataSource = null;
                //if (lstEstimated.Count > 0)
                //{
                //    cboOther.DataSource = lstEstimated;
                //    cboOther.SelectedIndex = 0;
                //}
            }
            else
                Debug.WriteLine("Estimate using Stochastic Model ...");
        }
        private void optAll_CheckedChanged(object sender, EventArgs e)
        {
            if (optAll.Checked)
                EstimateSites = (int)EstimateSite.All;
        }
        private void optSite_CheckedChanged(object sender, EventArgs e)
        {
            if (optSite.Checked)
                EstimateSites = (int)EstimateSite.BySite;
        }

        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void dataTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedNode = dataTree.SelectedNode;
            ShowSeriesTableAndGraph();
        }

        private void ShowSeriesTableAndGraph()
        {
            Cursor.Current = Cursors.WaitCursor;
            string nodeVal = dataTree.SelectedNode.Text;

            if (!lstStaName.Contains(nodeVal) && !nodeVal.Contains("Gages"))
            {
                string[] st = nodeVal.Split('.');
                SelectedSta = st[0].Trim();
                SelectedVar = st[1].Trim();

                WriteStatus("Reading " + SelectedVar + " for " + SelectedSta);

                if (optDataSource == (int)DataSource.NLDAS || optDataSource == (int)DataSource.GLDAS ||
                    optDataSource == (int)DataSource.TRMM)
                    lblSite.Text = SelectedSta;
                else
                    lblSite.Text = StationName(SelectedSta);

                DisplayStationTable(SelectedSta, SelectedVar);
                if (optDataSource == (int)DataSource.NLDAS || optDataSource == (int)DataSource.GLDAS ||
                    optDataSource == (int)DataSource.TRMM)
                {
                    GenerateGraph(false, SelectedSta, SelectedVar);
                }
                else
                {
                    DisplayMissingTable(SelectedSta, SelectedVar);
                    ShowMissingCountTable(SelectedSta, SelectedVar);

                    if (lstEstimated.Contains(SelectedSta))
                        GenerateGraph(true, SelectedSta, SelectedVar);
                    else
                        GenerateGraph(false, SelectedSta, SelectedVar);
                }

                WriteStatus("Ready...");

                //HighlightMissing(SelectedSta, SelectedVar);
            }
            Cursor.Current = Cursors.Default;
        }

        private void GenerateGraph(bool WithMiss, string sta, string svar)
        {
            try
            {
                SortedDictionary<DateTime, string> tseries;
                if (optDataSource == (int)DataSource.GHCN)
                     tseries = GetSeriesFromDict(sta, svar);
                else
                    tseries = GetSeries(sta, svar);

                List<DateTime> xdat = tseries.Keys.ToList();
                List<string> ydat = tseries.Values.ToList();
                tseries = null;

                //SortedDictionary<DateTime, string> tmissseries = GetMissingSeries(sta, svar);
                SortedDictionary<DateTime, string> tmissseries; //= GetMissingSeriesFromDict(sta, svar);
                if (optDataSource == (int)DataSource.GHCN)
                    tmissseries = GetMissingSeriesFromDict(sta, svar);
                else
                    tmissseries = GetMissingSeries(sta, svar);

                if (tmissseries == null)
                {
                    Debug.WriteLine("tmissseries is null! ...");
                    WithMiss = false;
                }
                else WithMiss = true;

                if (WithMiss)
                {
                    Debug.WriteLine("Timeseries is with missing data! ...");
                    Debug.WriteLine("Missing data = "+ tmissseries.Keys.Count.ToString());
                    if (tmissseries.Keys.Count > 0)
                    {
                        List<DateTime> xmissdat = new List<DateTime>();
                        List<string> ymissdat = new List<string>();
                        xmissdat = tmissseries.Keys.ToList();
                        ymissdat = tmissseries.Values.ToList();
                        tmissseries = null;
                        tsGraph.GenerateGraph(true, sta, svar, xdat, ydat, xmissdat, ymissdat);
                        xmissdat = null; ymissdat = null;
                    }
                    else
                    {
                        tsGraph.GenerateGraph(false, sta, svar, xdat, ydat, null, null);
                    }
                }
                else
                {
                    List<DateTime> xmissdat = null;
                    List<string> ymissdat = null;
                    tsGraph.GenerateGraph(WithMiss, sta, svar, xdat, ydat, xmissdat, ymissdat);
                }
            }
            catch (Exception ex)
            {
                errMsg = "Error generating series graph!";
                ShowError(errMsg, ex);
            }
        }

        private void chkShowData_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowData.Checked)
                splitDataGrid.Panel1Collapsed = false;
            else
                splitDataGrid.Panel1Collapsed = true;
        }
        private void dataTree_MouseEnter(object sender, EventArgs e)
        {
            dataTree.SelectedNode = SelectedNode;
        }
        private void numRadius_ValueChanged(object sender, EventArgs e)
        {
            SearchRadius = Convert.ToDouble(numRadius.Value);
        }

        private void frmData_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!(wrs == null))
            {
                wrs.Flush();
                wrs.Close();
                wrs = null;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            WriteStatus("Calculating Annual Series ...");
            clsAnnualStats aStats = new clsAnnualStats(fMain, dictWDMGages);
            aStats.ProcessDatasets();
            aStats = null;

            Cursor.Current = Cursors.Default;


            this.Close();
            this.Dispose();
        }

        private void numMaxStations_ValueChanged(object sender, EventArgs e)
        {
            LimitStations = (int)numMaxStations.Value;
        }

        private void tabData_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabData.SelectedTab = tabData.TabPages[tabData.SelectedIndex];
        }
        public void WriteStatus(string msg)
        {
            lblStatus.Text = msg;
            statusStrip.Refresh();
        }
        public void WriteLogFile(string msg)
        {
            fMain.WriteLogFile(msg);
        }

    }
}
