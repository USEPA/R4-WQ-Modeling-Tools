#define debug
//#undef debug
//using Microsoft.Office.Interop.Word;
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
    public partial class frmDataCMIP6 : Form
    {
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
                      "CLOU","DEWP","SOLR","ATMP","LRAD",
                      "TMAX","TMIN","PRCP","TEMP","HUMI"};
        private string SelectedSta, SelectedVar;
        private SortedDictionary<string, List<string>> dictSta = new SortedDictionary<string, List<string>>();
        private Dictionary<string, List<string>> dictWDMds = new Dictionary<string, List<string>>();
        private SortedDictionary<string, MetGages> dictSelSites =
                    new SortedDictionary<string, MetGages>();

        //dictionary of gages keyed on variable and sortedDictionary of dsn and Station info
        public Dictionary<string, SortedDictionary<int, clsStation>> dictWDMGages = new
             Dictionary<string, SortedDictionary<int, clsStation>>();

        private frmMain fMain;
        private int optDataSource;
        private int maxHours;
        private double PercentMiss;
        private const string MISS = "9999";
        private enum DataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6, EDDE };
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
        private string scenario = string.Empty, pathway = string.Empty;
        private string scenpath = string.Empty;

        public frmDataCMIP6(frmMain _fMain,SortedDictionary<string, List<string>> _dicSelectedVars,
            List<string> _lstSelectedVars, Dictionary<string, bool> _optVars, string _scen, string _path)
        {
            InitializeComponent();

            this.fMain = _fMain;
            this.scenpath = _fMain.scenarioPath;
            //this.scenario = _fMain.scenarioPath;

            this.dictSta = _fMain.dictSta;
            this.WdmFile = fMain.WdmFile;
            this.tempWDM = fMain.WdmFile;
            this.cachePath = fMain.cacheDir;
            this.optDataSource = fMain.optDataSource;
            this.BegDateTime = fMain.BegDateTime;
            this.EndDateTime = fMain.EndDateTime;
            this.scenario = _scen;
            this.pathway = _path;
            //this.scenpath = scenario + "_" + pathway;

            this.lblGrid.Text = "Grid";
            this.Text = "Selected Climate Scenario Dataseries : "+ scenario + "-" + pathway.ToUpper();
            tabData.TabPages[0].Text = "Daily";
            splitDataView.Panel2Collapsed = true;
            splitDataTable.Panel2Collapsed = true;

            if (tabData.Contains(tabPageMiss))
            tabData.TabPages.Remove(tabPageMiss);
            TimeUnit = "Day";

            //dictionary of processed site variables
            this.dictSiteVars = fMain.dictSiteVars;
            lstSta = dictSiteVars.Keys.ToList();
            //dictionary of sites selected from map
            this.dictSelSites = fMain.dictSelSites;
            //this.lstSelectedVars = _lstSelectedVars;
            //this.lstSelectedVars = lstSta;
            this.OptVars = _optVars;
            this.lstStaName = fMain.lstStaName;
            //this.PercentMiss = fMain.PercentMiss;

            optOther.Enabled = true;
            tabData.SelectedTab = tabPageData;
            dgvMissCnt.ClearSelection();
            optInter.Checked = true;
            optAll.Checked = true;
            chkShowData.Checked = false;

            if (tabData.Contains(tabPageDaily))
                tabData.TabPages.Remove(tabPageDaily);

            //debug
            //DebugSiteVars();
            //dictWDMds = GetWDMDataSets();

            //Get attributes of WDM series, returns true if with NLDAS or GLDAS data
            //false otherwise; enable spatial interpolation if with NLDAS/GLDAS
            //if (ReadWDMGageAttributes())
            //EnableSpatial(true);
            //else
            //EnableSpatial(false);
            ReadWDMGageAttributes();

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
                Debug.WriteLine("Entering GetWDMDataSets in frmDataCMIP6...");
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
            bool isCMIP = false;
            try
            {
                fMain.WriteStatus("Reading " + WdmFile + " : Getting timeseries attributes ...");
                fMain.WriteLogFile("Reading " + WdmFile + " : Getting timeseries attributes ...");

                WDM cWDM = new WDM(WdmFile, lstVars);
#if debug
    //WriteList(lstVars);
#endif
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
                isCMIP = cWDM.HasCMIP();
                cWDM = null;

                fMain.WriteStatus("Ready ...");
                fMain.WriteLogFile("Ready ...");

                //debug time series in WDM
#if debug
    ListWDMSeries();
#endif
            }
            catch (Exception ex)
            {
                string msg = "Error getting timeseries attributes from " + WdmFile;
                fMain.WriteLogFile(msg + crlf + ex.Message + crlf + ex.StackTrace);
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return isCMIP;
        }
        private void ListWDMSeries()
        {
            fMain.WriteLogFile("Timeseries in WDMFile: " + WdmFile);
            Debug.WriteLine("Timeseries in WDMFile: " + WdmFile);
            Debug.WriteLine("No of vars in dictWDMGages: " + dictWDMGages.Count.ToString());
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

#if debug
    Debug.WriteLine(msg);
#endif
                }
                gage = null;
            }
        }
        private void WriteList(List<string> aList)
        {
            Debug.WriteLine("Items in List: ");
            foreach(string item in aList)
            {
                Debug.WriteLine(item.ToString());
            }
        }
        private void BuildDataTreeView()
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> siteAttrib = new List<string>();
            MetGages gage = new MetGages();
            List<string> siteVars;
            List<string> lstOfSta=new List<string>();
            List<string> lstOfGrids = new List<string>();

            //Clear the TreeView each time the method is called.
            dataTree.Nodes.Clear();
            try
            {
                dataTree.BeginUpdate();

                //format of keys = 0_C0404392 in dictSiteVars, need to extract the site ID
                //which is C0000000
                lstOfGrids = dictSiteVars.Keys.ToList();
                foreach (string s in dictSiteVars.Keys)
                {
                    string[] token = s.Split('_');
                    lstOfSta.Add(token[1].ToString());
                    Debug.WriteLine("In frmDataCMIP: Site ID = " + token[1].ToString());
                }
                //return if no stations
                if (lstOfGrids.Count == 0) return;

                foreach (var sta in lstOfGrids)
                {
                    string stname = GridName(sta);
                    dictSelSites.TryGetValue(sta, out gage);

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
                            Debug.WriteLine("Treenode: " + stname + "." + svar);
                            dataTree.Nodes[lstOfSta.IndexOf(stname)].Nodes.Add(
                            new TreeNode(stname + "." + svar));
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
        private string GridName(string sta)
        {
            string[] token = sta.Split('_');
            return token[1].ToString();
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
            catch (Exception ex)
            { }
            return dtSeries;
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
        private void RefreshDataTable(bool isGraphMiss)
        {
            Debug.WriteLine(crlf + "Entering RefreshDataTable...");
            //get index
            DisplayStationTable(SelectedSta, SelectedVar);
            GenerateGraph(false, SelectedSta, SelectedVar);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void optInter_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void optOther_CheckedChanged(object sender, EventArgs e)
        {
            if (optOther.Checked)
            {
            }
            else
                Debug.WriteLine("Estimate using Stochastic Model ...");
        }
        private void optAll_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void optSite_CheckedChanged(object sender, EventArgs e)
        {
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
            Debug.WriteLine("Selected node : " + nodeVal);

            if (!lstStaName.Contains(nodeVal) && !nodeVal.Contains("Gages"))
            {
                string[] st = nodeVal.Split('.');
                SelectedSta = st[0].Trim();
                SelectedVar = st[1].Trim();

                WriteStatus("Reading " + SelectedVar + " for " + SelectedSta);

                lblSite.Text = StationName(SelectedSta);
                DisplayStationTable(SelectedSta, SelectedVar);
                GenerateGraph(false, SelectedSta, SelectedVar);

                WriteStatus("Ready...");
            }
            Cursor.Current = Cursors.Default;
        }
        private void GenerateGraph(bool WithMiss, string sta, string svar)
        {
            try
            {
                SortedDictionary<DateTime, string> tseries = GetSeries(sta, svar);
                List<DateTime> xdat = tseries.Keys.ToList();
                List<string> ydat = tseries.Values.ToList();
                tseries = null;

                tsGraph.GenerateGraph(false, sta, svar, xdat, ydat, null, null);
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

            //WriteStatus("Calculating Annual Series ...");
            //fMain.WriteLogFile("Calculating Annual Series ...");
            //clsAnnualStats aStats = new clsAnnualStats(fMain, dictWDMGages);
            //aStats.ProcessDatasets();
            //aStats = null;

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
