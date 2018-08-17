//
// Frequency Analysis Toolkit
// Version 1.0.0.0
//
// Revision History
// 07-01-15 Written
// 08-26-15 Latest modification
//
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
using RDotNet;
using RDotNet.NativeLibrary;
using System.Runtime.InteropServices;
using System.Reflection;
using ZedGraph;
using DotSpatial.Controls;
using DotSpatial.Projections;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Symbology;
using System.ComponentModel.Composition;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearRegression;
using Microsoft.Office;

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FREQANAL
{
    public partial class frmMain : Form
    { 
        public string dataDir, appDir, gisDir;
        public DataTable tblSeries,tblSelGages;
        public DataTable tbl7Q10, tblResults, tblModel, tblStats;
        public DataTable tblModelPred;
        public DataView tblViewGages;
        public DataFrame rData, rInfo1,rInfo2;
        private int seriesCount;
        private string selSiteNum;
        private REngine engine;
        private bool isSeriesDownloaded = false;
        private bool isGagesSelected = false;
        private string httpGage;
        private FileStream fslog;
        private StreamWriter wrlog;
        public DataTable tblUSGSGages = new DataTable();

        //data series
        private List<double> yseries = new List<double>();
        private List<float> xseries = new List<float>();
        private List<double> xdate = new List<double>();
        private List<SiteInfo> siteinfo = new List<SiteInfo>();
        private List<Frequency> siteprob = new List<Frequency>();
        private string OptSeries = "Annual";

        private List<DailyQ> dlyQ = new List<DailyQ>();
        private List<List<DailyQ>> SelDlyQ = new List<List<DailyQ>>();
        private List<List<AnnualQ>> SelAnnQ7 = new List<List<AnnualQ>>();

        public List<string> selGages = new List<string>();
        public List<string> selCurrentGages = new List<string>();
        private List<double> dArea = new List<double>();
        private List<RegData> lstRegData = new List<RegData>();


        private BindingSource bs;

        //graph vars
        PointPairList rawSeries = new PointPairList();
        PointPairList obsSeries = new PointPairList();
        PointPairList fitSeries = new PointPairList();
        private GraphPane currentPane;

        //web
        private frmWeb fweb;
        
        //webclient
        public WebClient webclient = new WebClient();

        //about box
        private frmAbout fAbout;

        //selected estimation point
        private FeatureSet estPoint;
        private MapPointLayer estPointLayer;

        //other globals
        private enum SelectMode { Select, Estimate };
        private int MapMode = (int)SelectMode.Select;
        public bool PointSelected = false;
        private double Xlon; 
        private double Ylat; 


        public string AnalMode;
        public int nDayQ;
        private int[] iNDayQ = new int[4] { 1, 3, 7, 30 };

        private int minYears = 10;
        private int minDays = 300;
        public Int16 ReturnPeriod = 10;
        private int FromYear = 1900;
        private int ToYear = 2015;

        private string RegModelResults;
        private Frequency SiteProb = new Frequency();
        private DataTable tblSiteProb = new DataTable();
        private int ModelNo = 0;

        //stats
        private SiteStats gageStats = new SiteStats();

        //manual
        private Microsoft.Office.Interop.Word.Application WordApp;
        
        [Export("Shell", typeof(ContainerControl))]
        private static ContainerControl Shell;

        public frmMain()
        {
            fAbout = new frmAbout();
            fAbout.lblAbout.Text = "Initializing toolkit ...";
            fAbout.Show();

            Application.DoEvents();

            Cursor.Current = Cursors.WaitCursor;

            InitializeComponent();
            if (DesignMode) return;
            Shell = this;

            try
            {
                appManager.LoadExtensions();
                appManager.HeaderControl.Remove("kHome");
                appManager.HeaderControl.Remove("kExtensions");
                appManager.HeaderControl.Remove("None");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }

            //position form on screen
            StartPosition = FormStartPosition.CenterScreen;
            string path = Application.StartupPath.ToString();
            appDir = path;
            CreateDataDirectory();

            //check for R
            if (!CheckIfRExist()){this.Dispose();return;}

            //create map of gages
            //CreateMap();

            //LoadProjectMap();
            CreateMap();
            //SetDataGrid();


            //fAbout.Hide();
            //fAbout.lblAbout.Text = "";

            try
            {
                //SetDataGrid();
                CreateTableSeries();

                //set menu items
                mnuGetSeries.Enabled = false;
                mnuCalc7Q10.Enabled = false;
                mnuRegAnal.Enabled = false;
                mnuTR.Enabled = false;
                txtTR.Enabled = false;

                //initialize analysis parameters 
                cboDay.SelectedIndex = 3;
                AnalMode = "Low Flow";
                nDayQ = 7;

                //set what tab is shown
                if (tabCtl.Contains(tabGages))
                    tabCtl.TabPages.Remove(tabGages);
                if (tabCtl.Contains(tabSeries))
                    tabCtl.TabPages.Remove(tabSeries);
                if (tabCtl.Contains(tabResults))
                    tabCtl.TabPages.Remove(tabResults);
                if (tabCtl.Contains(tabRegional))
                    tabCtl.TabPages.Remove(tabRegional);

                //txtInfo
                StringBuilder stxt = new StringBuilder();

                //stxt.Append("Filter gages to analyze using the dropdown list of gages. \r\n");
                //stxt.Append("Select individual gages by clicking header of row or click <Select All>. \r\n");
                //stxt.Append("Select averaging period from dropdown list: 1,3,5,7,15, or 30 days. \r\n");
                stxt.Append("Select minimum years and minimum days per year of data for analysis. \r\n");
                stxt.Append("Click <Query NWIS> to download and process data series for selected gages. \r\n");
                txtInfo.Text = stxt.ToString();
                stxt = null;

                //period of record
                yrFrom.Value = FromYear;
                yrTo.Value = ToYear;
                yrTo.Minimum = 1910;
                yrTo.Maximum = 2050;
                yrFrom.Minimum = 1910;
                yrFrom.Maximum = 2050;

                splitModelResults.Panel2Collapsed = true;
                splitContainerLegend.Panel2Collapsed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }

            Cursor.Current = Cursors.Default;

            fAbout.Hide();
            fAbout.lblAbout.Text = "";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            mnuMain.Items.RemoveAt(0);
            //LoadProjectMap();
            //CreateMap();
            //SetDataGrid();
            //foreach (System.Windows.Forms.Control ctl in spatialToolStripPanel1.Controls)
            //{
            //    Debug.WriteLine("contorl=", ctl.Name);
            //}
        }

        private void LoadProjectMap()
        {
            try
            {
                string basemap = Path.Combine(appDir, "basemap.txt");
                if  (!File.Exists(basemap))
                {
                    //File.WriteAllText(basemap, Properties.Resources.basemap);
                }
                appManager.Map.Projection = KnownCoordinateSystems.Projected.World.WebMercator;
                appManager.SerializationManager.OpenProject(basemap);
                appManager.Map.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load project map!\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
                Application.Exit();
            }
        }

        private bool CheckIfRExist()
        {
            //start R
            try
            {
                REngine.SetEnvironmentVariables();
                engine = REngine.GetInstance();

                // REngine requires explicit initialization.
                // You can set some parameters.

                engine.Initialize();

                //load library
                StringBuilder s = new StringBuilder();
                s.Clear();
                s.Append("library(EGRET)");
                string st = s.ToString();
                engine.Evaluate(st);
                st = "library(dataRetrieval)";
                engine.Evaluate(st);
                return (true);
            }
            catch (Exception ex)
            {

                string msg = "Error in initializing R, please install R to use the application ...\r\n\r\n";
                msg = msg + ex.Message;
                msg = msg + "\r\n";
                msg = msg + "Get R from http://www.r-project.org/ " + "and install EGRET and dataRetrieval packages\r\n";

                MessageBox.Show(msg);
                return (false);
            }
        }

        private void CreateDataDirectory()
        {
            //WriteLogFile("Executing CreateDataDirectory()");
            string datapath = Application.StartupPath.ToString() + "\\data";
            string gispath = Application.StartupPath.ToString() + "\\gis";
            string exepath = Application.StartupPath.ToString();
            SetupLogFile(exepath);

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(datapath);
                dataDir = datapath;

                if (!Directory.Exists(gispath))
                {
                    Directory.CreateDirectory(gispath);
                }
                gisDir = gispath;

            }
            catch (Exception e)
            {
                WriteLogFile(e.Message.ToString());
            }
            finally { }
        }

        private void SetDataGrid()
        {
            try
            {
                //bs = new BindingSource();
                //bs.DataSource = tblUSGSGages;
                //dgvGages.DataSource = bs;
                dgvGages.Columns["PRECIP"].HeaderText = "RAINFALL";
                dgvGages.Sort(dgvGages.Columns["STATE"], ListSortDirection.Ascending);
                dgvGages.ClearSelection();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        private void CreateTableSeries()
        {
            tblSeries = new DataTable();
            tblSeries.Columns.Add("Date", typeof(DateTime));
            tblSeries.Columns.Add("Qcfs", typeof(double));
            tblSeries.Columns.Add("DecYr", typeof(double));
            tblSeries.Columns.Add("logQ", typeof(double));
            tblSeries.Columns.Add("Q7", typeof(double));
            tblSeries.Columns.Add("Q30", typeof(double));

            tblStats = new DataTable();
            tblStats.Columns.Add("Statistic", typeof(string));
            tblStats.Columns.Add("Value", typeof(double));


            tbl7Q10 = new DataTable();
            tbl7Q10.Columns.Add("Year", typeof(Int32));
            tbl7Q10.Columns.Add("7Q10", typeof(double));
            //tbl7Q10.Columns.Add("logQ", typeof(double));
            //tbl7Q10.Columns.Add("N", typeof(Int32));

            //frequency result table
            tblResults = new DataTable();
            tblResults.Columns.Add("Gage", typeof(String));
            tblResults.Columns.Add("NYrs", typeof(Int16));
            tblResults.Columns.Add("PrZero", typeof(double));
            tblResults.Columns.Add("PrTr", typeof(double));
            tblResults.Columns.Add("LCL90", typeof(double));
            tblResults.Columns.Add("UCL90", typeof(double));
            tblResults.Columns.Add("P10", typeof(double));
            tblResults.Columns.Add("P20", typeof(double));
            tblResults.Columns.Add("P25", typeof(double));
            tblResults.Columns.Add("P50", typeof(double));
            tblResults.Columns.Add("P90", typeof(double));
            tblResults.Columns.Add("P99", typeof(double));
            tblResults.Columns.Add("Skew", typeof(double));
            tblResults.Columns.Add("SqMi", typeof(double));
            tblResults.Columns.Add("PRECIP", typeof(double));
            tblResults.Columns.Add("MEANQ", typeof(double));
            tblResults.Columns.Add("Slope", typeof(double));

            tblSiteProb.Columns.Add("Gage", typeof(String));
            tblSiteProb.Columns.Add("P10", typeof(double));
            tblSiteProb.Columns.Add("P20", typeof(double));
            tblSiteProb.Columns.Add("P25", typeof(double));
            tblSiteProb.Columns.Add("P50", typeof(double));
            tblSiteProb.Columns.Add("P90", typeof(double));
            tblSiteProb.Columns.Add("P99", typeof(double));

            //model table
            tblModel = new DataTable();
            tblModel.Columns.Add("ModelNo", typeof(String));
            tblModel.Columns.Add("Model", typeof(String));
            tblModel.Columns.Add("N", typeof(Int16));
            tblModel.Columns.Add("b0", typeof(double));
            tblModel.Columns.Add("b1", typeof(double));
            tblModel.Columns.Add("b2", typeof(double));
            tblModel.Columns.Add("b3", typeof(double));
            tblModel.Columns.Add("b4", typeof(double));
            tblModel.Columns.Add("b5", typeof(double));
            tblModel.Columns.Add("b6", typeof(double));
            tblModel.Columns.Add("R-sqr", typeof(double));

            tblModelPred = new DataTable();
            tblModelPred.Columns.Add("Model", typeof(String));
            tblModelPred.Columns.Add("Estimate", typeof(double));

        }

        private void CreateMap()
        {
            //appManager.Map.ClearLayers();
            appManager.Map.Projection = KnownCoordinateSystems.Projected.World.WebMercator;

            string shp1 = gisDir + "\\" + "HydroWebMerc.shp";
            string shp2 = gisDir + "\\" + "USGSWEBMerc.shp";
            string selfs = gisDir + "\\" + "SelectedGages.shp";

            try
            {
                //hydro lines
                FeatureSet fs1 = (FeatureSet)FeatureSet.Open(shp1);
                //fs1.FillAttributes();
                //tblUSGSGages = fs2.DataTable;
                MapLineLayer hyd = (MapLineLayer)appManager.Map.Layers.Add(fs1);
                LineSymbolizer hydsym = new LineSymbolizer();
                hydsym.SetWidth(.5);
                hydsym.SetFillColor(Color.Blue);
                hydsym.SetOutline(Color.Blue, .5);
                hyd.Symbolizer = hydsym;
                //huc.Symbolizer.SetFillColor(Color.Empty);
                hyd.LegendText = "Major Rivers";
                hyd.IsSelected = false;
                hyd.SelectionEnabled = false;

                //gages layer
                FeatureSet fs2 = (FeatureSet)FeatureSet.Open(shp2);
                fs2.FillAttributes();
                tblUSGSGages = fs2.DataTable;
                MapPointLayer gage = (MapPointLayer)appManager.Map.Layers.Add(fs2);
                PointSymbolizer gagesym = new PointSymbolizer(Color.Green, DotSpatial.Symbology.PointShape.Ellipse, 8);
                gage.SelectionSymbolizer = new PointSymbolizer(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 12);
                gage.Symbolizer = gagesym;
                gage.Symbolizer.SetOutline(Color.Black, 1);
                gage.SelectionSymbolizer.SetOutline(Color.Black, 1);
                gage.LegendText = "USGS Gages";
                gage.DataSet.Name = gage.LegendText;
                gage.IsSelected = true;

                int idx = appManager.Map.Layers.IndexOf(gage);
                appManager.Map.Layers.SelectLayer(idx);
                appManager.Map.Refresh();
            }
            catch (Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Cannot find GIS base layers ...\r\n");
                msg.Append("Feature dataset USGSWebMerc\r\n");
                msg.Append("should be in the \\data folder of the toolkit.\r\n\r\n");
                msg.Append(ex.Message);
                msg.Append("\n\n" + ex.Source);

                MessageBox.Show(msg.ToString());
                Application.Exit();
            }
        }

        private void tabCtl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabCtl.SelectedIndex == 0) // map tab
                {
                    appManager.Map.FunctionMode = FunctionMode.Select;
                    foreach (Control ctl in spatialToolStripPanel1.Controls)
                    {
                        Debug.WriteLine("ctl name = {0}, ctl text = {1}", ctl.Name, ctl.Text);
                        if (ctl.Name != "mnuMain")
                            ctl.Enabled = true;
                    }
                    if (MapMode == (int)SelectMode.Estimate)
                    {
                        splitContainerLegend.Panel2Collapsed = false;
                    }
                    else if (MapMode == (int)SelectMode.Select)
                    {
                        splitContainerLegend.Panel2Collapsed = true;
                    }

                    // can only select gages layers
                    List<ILayer> mLayers = new List<ILayer>();
                    mLayers = appManager.Map.GetLayers();

                    foreach (var mp in mLayers)
                    {
                        if (mp.DataSet.Name == "USGS Gages")
                            mp.IsSelected = true;
                        else
                            mp.IsSelected = false;
                    }
                    mLayers = null;
                }
                else
                {
                    appManager.Map.FunctionMode = FunctionMode.None;
                    foreach (Control ctl in spatialToolStripPanel1.Controls)
                    {
                        if (ctl.Name != "mnuMain")
                            ctl.Enabled = false;
                    }
                    if (tabCtl.SelectedIndex == 1) //selected gages
                    {
                        if (tblSelGages.Rows.Count > 0)
                            mnuGetSeries.Enabled = true;
                        else
                            mnuGetSeries.Enabled = false;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
            if (!isSeriesDownloaded) { return; }
        }

        private void CreateGraph(string splot, List<double> xdate, List<float> xseries, List<double> yseries)
        {
            string sdly;
            if (splot == "Annual")
            {
                sdly = nDayQ + "-day Mean Discharge, cfs";
            }
            else
            {
                sdly = "Daily Mean Discharge, cfs";
            }
            //AxisType axisDly = AxisType.Linear;
            //AxisType axisAnn = AxisType.Linear;

            zedGraph.Size = new Size(ClientRectangle.Width - 20,
                                    ClientRectangle.Height - 20);

            // get a reference to the GraphPane
            GraphPane seriesPane = zedGraph.GraphPane;
            currentPane = seriesPane;
            seriesPane.Legend.IsVisible = false;

            seriesPane.Title.Text = "Gage: " + selSiteNum;
            seriesPane.YAxis.Title.Text = sdly;
            seriesPane.YAxis.Type = AxisType.Exponent;
            seriesPane.XAxis.Scale.FontSpec.Size = 8;
            seriesPane.YAxis.Scale.FontSpec.Size = 8;

            if (OptSeries == "Daily")
            {
                seriesPane.XAxis.Title.Text = "Date";
                //seriesPane.XAxis.Type = axisDly;
            }
            else if (OptSeries == "Annual")
            {
                seriesPane.XAxis.Title.Text = "Year";
                //seriesPane.XAxis.Type = axisAnn;
            }

            // Add gridlines to the plot, and make them gray
            seriesPane.XAxis.MajorGrid.IsVisible = true;
            seriesPane.YAxis.MajorGrid.IsVisible = true;
            seriesPane.XAxis.MajorGrid.Color = Color.LightGray;
            seriesPane.YAxis.MajorGrid.Color = Color.LightGray;

            rawSeries.Clear();
            for (int i = 0; i < xseries.Count; i++)
            {
                if (splot == "Daily")
                {
                    rawSeries.Add(xseries[i], yseries[i]);
                }
                else 
                { 
                    rawSeries.Add(xseries[i], yseries[i]);
                }
            }

            // Generate a red curve with diamond
            LineItem seriesCurve1 = seriesPane.AddCurve("",
                 rawSeries, Color.Red, ZedGraph.SymbolType.Circle);
            seriesCurve1.Line.IsVisible = false;

            // Generate a blue curve with circle
            //LineItem seriesCurve2 = seriesPane.AddCurve("Piper",
            //      list2, Color.Blue, SymbolType.Circle);

            // Tell ZedGraph to refigure the
            // axes since the data have changed
            zedGraph.AxisChange();
            zedGraph.Refresh();
            isSeriesDownloaded = true;
        }

        private void dgvGages_SelectionChanged(object sender, EventArgs e)
        {
            seriesCount = dgvGages.SelectedRows.Count;
            if (seriesCount > 0)
            {
            }
            else
            {
            }
        }

        private void SetFormControls(bool enabled)
        {
            cboDay.Enabled = enabled;
            txtDly.Enabled = enabled;
            txtYr.Enabled=enabled;
            mnuCalc7Q10.Enabled = enabled;
            mnuTR.Enabled = enabled;
            txtTR.Enabled = enabled;
            yrFrom.Enabled = enabled;
            yrTo.Enabled = enabled;
        }

        /// <summary>
        /// mnuGetSeries - queries data for each gage selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void mnuGetSeries_Click(object sender, EventArgs e)
        {
            WriteLogFile("Executing GetGageData()");
            //tabs
            if (tabCtl.TabPages.Contains(tabSeries))
                tabCtl.TabPages.Remove(tabSeries);
            if (tabCtl.TabPages.Contains(tabResults))
                tabCtl.TabPages.Remove(tabResults);
            if (tabCtl.TabPages.Contains(tabRegional))
                tabCtl.TabPages.Remove(tabRegional);

            mnuRegAnal.Enabled = false;
            isSeriesDownloaded = false;

            SetFormControls(false);

            Cursor.Current = Cursors.WaitCursor;
            //seriesCount = dgvGages.SelectedRows.Count;            

            //clear list of selected gages and list of data
            SelAnnQ7.Clear();
            SelDlyQ.Clear();
            selGages.Clear();
            selCurrentGages.Clear();
            dArea.Clear();
            siteinfo.Clear();
            ModelNo = 0;
            lstRegData.Clear();
            tblModel.Clear();
            tblModelPred.Clear();
            dgvModel.DataSource = null;

            //loop through all selected rows and get info of the gage (gage no and precip)
            foreach (DataGridViewRow rw in dgvGages.Rows)
            {               
                int idx = dgvGages.Rows.IndexOf(rw);
                string gage = dgvGages.Rows[idx].Cells["Site_No"].Value.ToString();
                double prec = Convert.ToDouble(dgvGages.Rows[idx].Cells["Precip"].Value);

                SiteInfo sinfo = new SiteInfo();
                sinfo.SiteNo = gage;
                sinfo.Precip = prec;
                selCurrentGages.Add(gage);

                siteinfo.Add(sinfo);
                sinfo = null;
            }

            // download data for each gage
            for (int iser = 0; iser < selCurrentGages.Count; iser++)
            {
                int no = iser + 1;
                int percent = (no*100)/selCurrentGages.Count;
                selSiteNum = selCurrentGages[iser].Trim();
                string lbl = "Querying NWIS for USGS Gage " + selSiteNum + " (" + no + " of " + selCurrentGages.Count + ")";

                appManager.ProgressHandler.Progress("", percent, lbl);

                if (QueryNWIS(iser, selSiteNum) > 0)
                {
                    selGages.Add(selSiteNum);
                    // get the information for gage and build siteinfo list
                    ProcessGageInfo(iser, selSiteNum);
                }
                appManager.ProgressHandler.Progress("", 0, "Ready...");
            }

            // create shapefile of selected stations
            // CreateSelectedStationsShape();

            if (!tabCtl.TabPages.Contains(tabSeries))
                tabCtl.TabPages.Add(tabSeries);

            int isr = 0;
            try
            {
                CreateGridViewDaily(isr);
            }
            catch (Exception ex)
            {
                WriteLogFile(ex.Message + "\r\n" + ex.StackTrace);
                //Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }

            //list of selected gages/series to a combo box
            cboSeries.DataSource = null;
            cboSeries.DataSource = selGages;
            cboSeries.SelectedIndex = 0;
            
            //show tabseries
            //tabCtl.SelectedIndex = 1;
            tabCtl.SelectedTab = tabSeries;
            isSeriesDownloaded = true;

            SetFormControls(true);

            Cursor.Current = Cursors.Default;
            //lblStatus.Text = "Ready ...";
            //statusStrip.Refresh();
        }

        private int QueryNWIS(int iser, string gage)
        {
            Cursor.Current = Cursors.WaitCursor;
            WriteLogFile("Executing QueryNWIS(), Gage: " + gage);
            //
            // get daily flow data and info for all selected gages
            //

            try
            {
                string bday = Convert.ToString(FromYear)+"-01-01";
                string eday = Convert.ToString(ToYear) + "-12-31";

                engine.Evaluate("parmCD<-'00060'");
                engine.Evaluate("siteno<-'" + gage + "'");

                //
                //rData and rInfo contains data and site info
                //
                string sq = "DailyQ <- readNWISdv(siteno,parmCD,'" + bday + "','" + eday + "')";
                Debug.WriteLine("Query sq: " + sq);
                rData = engine.Evaluate(sq).AsDataFrame();
                Debug.WriteLine("rData: " + rData.ToString());

                string da = "InfoQ <- readNWISInfo(siteno,parmCD,interactive=FALSE)";
                Debug.WriteLine("Query rInfo1: " + da);
                rInfo1 = engine.Evaluate(da).AsDataFrame();
                Debug.WriteLine("rInfo1: " + rInfo1.ToString());

                //MarCH 18. 2018
                //string info = "InfoQ <- whatNWISdata(siteno,service='dv',parmCD)";
                //Debug.WriteLine("Query rInfo2: " + info);
                //rInfo2 = engine.Evaluate(info).AsDataFrame();
                //Debug.WriteLine("rInfo2: " + rInfo2.ToString());

                Debug.WriteLine("Data COunt = " + rData.RowCount.ToString());
                WriteLogFile("Gage " + gage + " record count = " + rData.RowCount.ToString());
            }
            catch (Exception ex)
            {
                WriteLogFile(ex.Message + "\r\n" + "Error retrieving data for USGS" + gage);
                MessageBox.Show("Error retrieving data for USGS" + gage+"!\r\n\r\n" + ex.Message+"\r\n\r\n"+ex.StackTrace);
                return (-1);
            }
            //Debug.WriteLine("Gage {0}, num records {1}", gage, rData.RowCount);
            return (rData.RowCount);
        }

        private int ProcessGageInfo(int iser, string gage)
        {
            WriteLogFile("Executing ProcessGageInfo for gage: " + gage);
                
            List<double> qser = new List<double>();
            List<double> xqser = new List<double>();

            //get site info
            try
            {
                var qry = siteinfo.Where(site => site.SiteNo == gage);
                var sinfo = qry.First<SiteInfo>();
                    
                double sqmi = Convert.ToDouble(rInfo1[0, "drain_area_va"]);
                dArea.Add(sqmi);
                double xlong = Convert.ToDouble(rInfo1[0, "dec_long_va"]);
                double ylat = Convert.ToDouble(rInfo1[0, "dec_lat_va"]);

                //MAR 19, 2018
                //double xlong = Convert.ToDouble(rInfo2[0, "dec_long_va"]);
                //double ylat = Convert.ToDouble(rInfo2[0, "dec_lat_va"]);
                //double begdate = Convert.ToDouble(rInfo2[0, "begin_date"]);
                //double enddate = Convert.ToDouble(rInfo2[0, "end_date"]);
                //Int32 reccnt = Convert.ToInt32(rInfo2[0, "count_nu"]);

                //sinfo.BegDate = begdate;
                //sinfo.EndDate = enddate;
                //sinfo.RecCount = reccnt;
                sinfo.Lat = ylat;
                sinfo.Lon = xlong;
                sinfo.DArea = sqmi;
                
                //
                //get the Comid for the gage
                //
                WriteLogFile("Executing GetCOMID for gage: " + gage);
                long comid = GetCOMIDFromPoint(xlong, ylat);
                sinfo.Comid = Convert.ToString(comid);

                //
                //get landuse characteristics for the gage
                //
                WriteLogFile("Executing GetBasinCharacteristic for gage: " + gage);
                GetBasinCharacteristics(sinfo);
                
                qser.Clear(); xqser.Clear();

                foreach (DataFrameRow dr  in rData.GetRows())
                {
                    double q = Convert.ToDouble(dr[3]);
                    double x = Convert.ToDouble(dr[2]);
                    if (!double.IsNaN(q))
                    {
                        qser.Add(q);
                        xqser.Add(x);
                    }
                    //Debug.WriteLine("{0},{1},{2},{3},{4},{5}", x, q, dr[0],dr[1],dr[2],dr[3]);
                }

                sinfo.MeanQ = qser.Mean();

                //get the annual series based on aggregation days
                int ndata = GetDailyAndAnnualSeries(xqser, qser, iser, gage);
                //int n = ReDoGetDailyAndAnnualSeries(xqser, qser, iser, gage);
                WriteLogFile("Annual records for gage " + gage + ": " + ndata);

                qser = null;
                xqser = null;
                return (ndata);
            }
            catch (Exception ex)
            {
                WriteLogFile(ex.Message + "\r\n" + "Error in processing gage: " + gage);
                qser = null;
                xqser = null;
                return (-1);
            }
        }

        private long GetCOMIDFromPoint(double selX, double selY)
        {
            long comid=0L;
            try
            {
                Debug.WriteLine("Entering GetCOMIDFromPoint ...");
                string qry = "http://ofmpub.epa.gov/waters10/PointIndexing.Service?" +
                "pGeometry=POINT(" + selX + " " + selY + ")&optOutFormat=JSON";

                string json = DownloadString(qry);

                Debug.WriteLine("In GETCOMIDFromPoint : " + qry);
                Debug.WriteLine("In GETCOMIDFromPoint : " + json);

                comid = GetCOMID(json);
                if ( comid < 0)
                    return (-1);
                return (comid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in COMID query!\r\n\r\n " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                return (-1);
            }
            //return (comid);
        }

        public string DownloadString(string uri_address)
        {
            string reply;
            try
            {
                WebClient wc = new WebClient();
                reply = wc.DownloadString(uri_address);
                if (!wc.IsBusy)
                    wc.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in mapserver query!\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
                return (null);
            }
            return (reply);

        }

        private long GetCOMID(string json)
        {
            //Debug.WriteLine("Entering GetCOMID ...");
            try
            {
                JObject o = JObject.Parse(json);

                string cid = (string)o["output"]["ary_flowlines"][0]["comid"];
                //string reach = (string)o["output"]["ary_flowlines"][0]["reachcode"];
                //double meas = (double)o["output"]["ary_flowlines"][0]["fmeasure"];
                //string huc12 = (string)o["output"]["ary_flowlines"][0]["wbd_huc12"];
                //Debug.WriteLine("In GetCOMID : Comid={0}, Measure={1}, HUC12={2}", comid, meas, huc12);
                
                //Debug.WriteLine("In GetCOMID : Comid="+ cid);
                long comid = Convert.ToInt64(cid);
                return (comid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WriteLogFile("GETCOMID: " + ex.Message + "\r\n" + ex.StackTrace);
                return (-1);
            }
        }

        private int GetBasinCharacteristicsOld_1017(SiteInfo sinfo)
        {
            try
            {
                string selComid = sinfo.Comid;
                double val;

                //old
                //string qry = "http://ofmpub.epa.gov/waters10/watershed_characterization.control?" +
                //    "pComID=" + selComid +
                //    "&optOutFormat=JSON&optOutPrettyPrint=1";

                //updated from EPA
                string qry = "https://ofmpub.epa.gov/waters10/nhdplus.jsonv25?pcomid=" +
                    selComid + "&pFilenameOverride=AUTO&optOutPrettyPrint=1";

                string json = DownloadString(qry);

                Debug.WriteLine(qry);
                Debug.WriteLine(json);

                WriteLogFile("Web Query : " + qry);
                WriteLogFile("Response = " + json);
               
                //parse json and get basin characteristics

                JObject obj = JObject.Parse(json);
                SiteLULC sitelu = new SiteLULC();

                val = (double)obj["output"]["cumdrainag"];
                sinfo.DArea = val / (2.589988);
                val = (double)obj["output"]["Q0001E"];
                sinfo.MeanQ = val;
                val = (double)obj["output"]["cumprecipvc"];
                sinfo.Precip = val / (100.0 * 25.4);

                val = (double)obj["output"]["cumnlcd_11"];
                sitelu.Water = val;
                val = (double)obj["output"]["cumnlcd_21"];
                sitelu.OpenSpace = val;
                val = (double)obj["output"]["cumnlcd_22"];
                sitelu.LowInt = val;
                val = (double)obj["output"]["cumnlcd_23"];
                sitelu.MedInt = val;
                val = (double)obj["output"]["cumnlcd_24"];
                sitelu.HighInt = val;
                val = (double)obj["output"]["cumnlcd_31"];
                sitelu.Barren = val;
                val = (double)obj["output"]["cumnlcd_41"];
                sitelu.DecidForest = val;
                val = (double)obj["output"]["cumnlcd_42"];
                sitelu.EvergForest = val;
                val = (double)obj["output"]["cumnlcd_43"];
                sitelu.MixedForest = val;
                val = (double)obj["output"]["cumnlcd_52"];
                sitelu.Shrubland = val;
                val = (double)obj["output"]["cumnlcd_71"];
                sitelu.Grassland = val;
                val = (double)obj["output"]["cumnlcd_81"];
                sitelu.Pasture = val;
                val = (double)obj["output"]["cumnlcd_82"];
                sitelu.Crops = val;
                val = (double)obj["output"]["cumnlcd_90"];
                sitelu.WoodyWetland = val;
                val = (double)obj["output"]["cumnlcd_95"];
                sitelu.HerbWetland = val;

                sinfo.Forest = sitelu.DecidForest + sitelu.EvergForest + sitelu.MixedForest;
                sinfo.Urban = sitelu.LowInt + sitelu.MedInt + sitelu.HighInt;
                sinfo.Wetlands = sitelu.HerbWetland + sitelu.WoodyWetland;
                sinfo.Water = sitelu.Water;

                sitelu = null;
                obj = null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error in watershed charcterization web query!\r\n\r\n " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                return (-1);
            }
            return (0);
        }

        private int GetBasinCharacteristics(SiteInfo sinfo)
        {
             try
             {
                 string selComid = sinfo.Comid;
                 double val;

                //old
                //string qry = "http://ofmpub.epa.gov/waters10/watershed_characterization.control?" +
                //    "pComID=" + selComid +
                //    "&optOutFormat=JSON&optOutPrettyPrint=1";

                //updated from EPA
                 string qry = "https://ofmpub.epa.gov/waters10/nhdplus.jsonv25?pcomid=" +
                    selComid + "&pFilenameOverride=AUTO&optOutPrettyPrint=1";

                 string json = DownloadString(qry);

                //WriteLogFile("Web Query : " + qry);
                //WriteLogFile("Response = " + json);

                //parse json and get basin characteristics

                SiteLULC sitelu = new SiteLULC();
                JObject obj = JObject.Parse(json);

                JArray arr0 = (JArray)obj["output"]["header"]["attributes"];
                //Debug.WriteLine("attributes = " + arr0);

                JArray arr1 = (JArray)obj["output"]["categories"][1]["subcategories"][0]["attributes"];
                //Debug.WriteLine("attributes = " + arr1);

                JArray arr2 = (JArray)obj["output"]["categories"][1]["subcategories"][1]["attributes"];
                //Debug.WriteLine("attributes = " + arr2);

                List<string> header = new List<string>();
                header = arr0.Select(m =>
                    (string)m.SelectToken("value")).ToList();
                foreach (string s in header)
                    Debug.WriteLine("header attrib = " + s);

                List<string> common = new List<string>(); 
                common = arr1.Select(m =>
                    (string)m.SelectToken("value")).ToList();
                foreach (string s in common)
                    Debug.WriteLine("common attrib = " + s);

                List<string> nlcdval = new List<string>();
                nlcdval = arr2.Select(m =>
                    (string)m.SelectToken("value")).ToList();
                foreach (string s in nlcdval)
                    Debug.WriteLine("nlcdval attrib = " + s);


                //common arrays
                //area in sqkm converted to sqmi
                val = Convert.ToDouble(common[0]);
                sinfo.DArea = val / (2.589988);

                //meanQ from header array
                val = Convert.ToDouble(header[3]);
                sinfo.MeanQ = val;

                //rainfall in mm converted to in
                val = Convert.ToDouble(common[2]);
                sinfo.Precip = val / (100.0 * 25.4);

                val = Convert.ToDouble(nlcdval[0]);
                sitelu.Water = val;

                val = Convert.ToDouble(nlcdval[1]);
                sitelu.LowInt = val;
                val = Convert.ToDouble(nlcdval[2]);
                sitelu.HighInt = val;

                val = Convert.ToDouble(nlcdval[3]);
                sitelu.DecidForest = val;
                val = Convert.ToDouble(nlcdval[4]);
                sitelu.EvergForest = val;
                val = Convert.ToDouble(nlcdval[5]);
                sitelu.MixedForest = val;

                sinfo.Others = Convert.ToDouble(nlcdval[6]);
                sinfo.Forest = sitelu.DecidForest + sitelu.EvergForest + sitelu.MixedForest;
                sinfo.Urban = sitelu.LowInt + sitelu.HighInt;
                sinfo.Water = sitelu.Water;

                sitelu = null;
                obj = null;
                arr0 = null;
                arr1 = null;
                arr2 = null;
                header = null;
                common = null;
                nlcdval = null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error in watershed charcterization web query!\r\n\r\n " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                return (-1);
            }
            return (0);
        }

        private int GetDailyAndAnnualSeries(List<Double> xqser, List<double> qser, int iser, string gage)
        {
            WriteLogFile("Executing GetDailyAndAnnualSeries for gage: " + gage);

            List<DailyQ> dlyQ = new List<DailyQ>();
            List<AnnualQ> lstQnDay = new List<AnnualQ>();

            DateTime baseDate = new System.DateTime(1970, 01, 01, 0, 0, 0);
            DateTime newbase = new System.DateTime(1850, 01, 01, 0, 0, 0);

            int nDayQQ;

            for (int i = 0; i < xqser.Count; i++)
            {
                int k = i;
                //if (!(qser[i].ToString() == "NaN"))
                {
                    DailyQ dly = new DailyQ();

                    double qd = qser[i];

                    dly.Date = baseDate.AddDays(xqser[i]);
                    TimeSpan tday = dly.Date.Subtract(newbase);
                    dly.Day = tday.Days;
                    dly.Qdly = qd;
                    dly.Year = dly.Date.Year;

                    //decimal year

                    int mon, day, yr, hr, min, sec;
                    DateTime dt = dly.Date;
                    mon = dt.Month;
                    day = dt.Day;
                    yr = dt.Year;
                    hr = dt.Hour;
                    min = dt.Minute;
                    sec = dt.Second;

                    double decyr = GetDecimalYear(yr, mon, day, hr, min, sec);
                    dly.DecYear = decyr;

                    dly.Qnday = Convert.ToDouble("NaN");

                    // accumulate based on nDayQ

                    nDayQQ = nDayQ;
                    if (nDayQQ == 1)
                    {
                        dly.Qnday = qser[i];

                        AnnualQ dq = new AnnualQ();
                        dq.QnDay = qser[i];
                        dq.Year = dly.Date.Year;
                        
                        //if (dq.Year >= FromYear && dq.Year <= ToYear)
                        //{
                            lstQnDay.Add(dq);
                        //}

                        dq = null;
                    }
                    else if (k >= nDayQQ - 1)
                    {
                        int jj = k - (nDayQQ - 1);

                        int diff = Convert.ToInt32(dly.Day - dlyQ[jj].Day);

                        if (diff == nDayQQ - 1)
                        {
                            double avgQ = 0;
                            for (int j = k - (nDayQQ - 1); j <= k; j++)
                            {
                                qd = qser[j];
                                if (qd < 0.0) qd = 0.0;
                                avgQ = avgQ + qd / nDayQQ;
                            }

                            dly.Qnday = avgQ;

                            AnnualQ dq = new AnnualQ();
                            dq.QnDay = avgQ;
                            dq.Year = dly.Date.Year;
                            //if (dq.Year >= FromYear && dq.Year <= ToYear)
                            //{
                                lstQnDay.Add(dq);
                            //}
                            dq = null;
                        }
                    }

                    //if (dly.Year >= FromYear && dly.Year <= ToYear)
                    //{
                        dlyQ.Add(dly);
                    //}
                    dly = null;
                }
            }

            // add list of daily Q to Daily list
            SelDlyQ.Add(dlyQ);
            dlyQ = null;

            //-------annual calculations ---------
            //get minimum of n-day Q for each year
            //
            var query = lstQnDay.GroupBy(r => r.Year)
                    .Select(grp => new
                    {
                        Year = grp.Key,
                        minQ = grp.Min(t => t.QnDay),
                        ncnt = grp.Count()
                    });

            List<AnnualQ> annQ = new List<AnnualQ>();
            foreach (var item in query)
            {
                if (item.ncnt > minDays)
                {
                    AnnualQ dq = new AnnualQ();
                    dq.Year = item.Year;
                    dq.QnDay = item.minQ;
                    dq.logQ = Math.Log10(item.minQ);
                    annQ.Add(dq);
                    dq = null;
                }
            }

            SelAnnQ7.Add(annQ);

            int nrec = annQ.Count;
            annQ = null;
            query = null;

            return (nrec);
        }
    
        private void CreateGridViewAnnual(int iser)
        {
            splitContainer1.Panel2Collapsed = true;

            List<AnnualQ> curlst = new List<AnnualQ>();
            curlst = SelAnnQ7.ElementAt(iser);
            selSiteNum = selGages[iser];

            //Debug.WriteLine("gage = {0}, lst count={1}",selSiteNum, curlst.Count);

            dgvSeries.DataSource = null;
            dgvSeries.DataSource = curlst;
            dgvSeries.Columns["logQ"].Visible = false;
            dgvSeries.Columns["Qnday"].HeaderText = "Q" + nDayQ;
            
            //create graph

            yseries.Clear(); xseries.Clear();
            yseries = (from item in curlst
                       where !double.IsNaN(Convert.ToDouble(item.QnDay))
                        select Convert.ToDouble(item.QnDay)).ToList();
            xseries = (from item in curlst
                       select Convert.ToSingle(item.Year)).ToList();
            string splot = "Annual";
            CreateGraph(splot,xdate, xseries, yseries);
            curlst = null;

            //isSeriesDownloaded = true;
        }

        private void CreateGridViewDaily(int iser)
        {
            splitContainer1.Panel2Collapsed = false;

            List<DailyQ> curlst = new List<DailyQ>();
            curlst = SelDlyQ.ElementAt(iser);
            selSiteNum = selGages[iser];

            dgvSeries.DataSource = null;
            dgvSeries.DataSource = curlst;
            dgvSeries.Columns["Day"].Visible = false;
            dgvSeries.Columns["Year"].Visible = false;
            dgvSeries.Columns["DecYear"].Visible = false;
            dgvSeries.Columns["Qnday"].HeaderText = "Q" + nDayQ;

            //create graph

            yseries.Clear(); xseries.Clear(); xdate.Clear();
            yseries = (from item in curlst
                       where !double.IsNaN(Convert.ToDouble(item.Qdly))
                       select Convert.ToDouble(item.Qdly)).ToList();
            //xseries = (from item in curlst
            //           select Convert.ToSingle(item.Year)).ToList();
            xseries = (from item in curlst
                       select Convert.ToSingle(item.DecYear)).ToList();
            xdate = (from item in curlst
                       select Convert.ToDouble(item.DecYear)).ToList();
            string splot = "Daily";
            CreateGraph(splot, xdate, xseries, yseries);
            curlst = null;

            //isSeriesDownloaded = true;
            //calc statistics
            List<double> logser= new List<double>();
            List<double> over_y = new List<double>();
            foreach (double item in yseries)
            {
                if (item > 0)
                {
                    logser.Add(Math.Log10(item));
                    over_y.Add(1.0 / item);
                }
            }

            double logmean = MathNet.Numerics.Statistics.Statistics.Mean(logser);
            double invmean = MathNet.Numerics.Statistics.Statistics.Mean(over_y);

            tblStats.Clear();

            DataRow g = tblStats.NewRow();
            g["Statistic"] = "Mean";
            g["Value"] = MathNet.Numerics.Statistics.Statistics.Mean(yseries);
            tblStats.Rows.Add(g);
            g = null;

            g = tblStats.NewRow();
            g["Statistic"] = "Stand Deviation";
            g["Value"] = MathNet.Numerics.Statistics.Statistics.StandardDeviation(yseries);
            tblStats.Rows.Add(g);
            g = null;

            g = tblStats.NewRow();
            g["Statistic"] = "Median";
            g["Value"] = MathNet.Numerics.Statistics.Statistics.Median(yseries);
            tblStats.Rows.Add(g);
            g = null;

            g = tblStats.NewRow();
            g["Statistic"] = "Geometric Mean";
            g["Value"] = Math.Pow(10,logmean);
            tblStats.Rows.Add(g);
            g = null;

            g = tblStats.NewRow();
            g["Statistic"] = "Harmonic Mean";
            g["Value"] = 1.0 / invmean;
            tblStats.Rows.Add(g);
            g = null;

            logser = null;

            dgvStats.DataSource = null;
            dgvStats.DataSource = tblStats;

        } 
        private void cboSeries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSeries.SelectedIndex < 0) { return; }
            int selIndex = cboSeries.SelectedIndex;
            //Debug.WriteLine("select index in cboseries =" + selIndex);

            if (OptSeries=="Annual")
            {
                CreateGridViewAnnual(selIndex);
            }
            else
            {
                CreateGridViewDaily(selIndex);
            }
        }
        private void GetSelectedGagesFromMap()
        {
            dgvGages.ClearSelection();
            if (selGages.Count > 0)
            {
                foreach(var item in selGages)
                {
                    Debug.WriteLine("sel gages = " + item.ToString());
                }
                foreach (DataGridViewRow drow in dgvGages.Rows)
                {
                    if (selGages.Contains(drow.Cells["Site_No"].Value))
                    {
                        drow.Selected = true;
                    }
                }
            }
            else
            {
                //lblStatus.Text = "No gages selected from map ...";
                //statusStrip.Refresh();
            }
        }

        private void mnuCalc7Q10_Click(object sender, EventArgs e)
        {
            // click menu to calculate frequency 
            if (!isSeriesDownloaded) { return; }

            frmReturn fReturn = new frmReturn(this);
            if (fReturn.ShowDialog() ==DialogResult.OK) {}
            fReturn = null;

            try
            {
                mnuEstimate.Enabled = false;
                tabCtl.TabPages.Remove(tabResults);
                if (tabCtl.TabPages.Contains(tabRegional))
                {
                    if (tblModelPred.Rows.Count > 0)
                        dgvEstimate.DataSource = null;
                    if (tblModel.Rows.Count > 0)
                    {
                        tblModel.Clear();
                        dgvModel.DataSource = null;
                    }
                    tabCtl.TabPages.Remove(tabRegional);
                }
            }
            finally { }

            tabCtl.TabPages.Add(tabResults);
            Cursor.Current = Cursors.WaitCursor;
            tblResults.Clear();
            dgvResults.DataSource = null;
            dgvResults.DataSource = tblResults;

            dgvResults.Columns["P10"].Visible = false;
            dgvResults.Columns["P20"].Visible = false;
            dgvResults.Columns["P25"].Visible = false;
            dgvResults.Columns["P50"].Visible = false;
            dgvResults.Columns["P90"].Visible = false;
            dgvResults.Columns["P99"].Visible = false;
            dgvResults.Columns["Skew"].Visible = false;
            dgvResults.Columns["Slope"].Visible = false;
            dgvResults.Columns["PrTr"].HeaderText = nDayQ + "Q" + ReturnPeriod;

            tabResults.Text = nDayQ + "Q" + ReturnPeriod;
            tabCtl.SelectedTab = tabResults;

            double Qcfs;
            for (int iser = 0; iser < selGages.Count; iser++)
            {
                selSiteNum = selGages[iser].Trim();
                string gage = selSiteNum;
                Qcfs = CalculateFrequency(iser, gage);
            }
            
            if (tblResults.Rows.Count == 0)
            {
                //lblStatus.Text = "Frequency Analysis not performed, selected gages with less than "
                //+ "10 yrs of data ...";
                //statusStrip.Refresh();
                btnRegion.Enabled = false;
                mnuRegAnal.Enabled = false;
                return;
            }

            //lblStatus.Text = "Click on row header for probability plot ...";
            //statusStrip.Refresh();

            //dgvResults.DataSource = tblResults;
            //tabCtl.SelectedTab = tabResults;
            //Debug.WriteLine("sel gages = " + selGages.Count);
            //dgvResults.DataSource = tblResults;

            // do regional analysis if more than 2

            btnRegion.Enabled = false;
            mnuRegAnal.Enabled = false;
            if (tblResults.Rows.Count > 2)
            {
                mnuRegAnal.Enabled = true;
                btnRegion.Enabled = true;
            }

            Cursor.Current = Cursors.Default;
            //lblStatus.Text = "Ready ...";
            //statusStrip.Refresh();
        }

        private double CalculateFrequency(int iser, string gage)
        {
            WriteLogFile("Executing CalculateFrequency for gage: " + gage);

            List<double> prb = new List<double>() { 0.1, 0.2, 0.25, 0.5, 0.90, 0.99 };
            List<double> qpr = new List<double>();
            double precip=0.0; 
            double slope=0;
            double meanq = 0.0;
            double probzero = 0.0;
            double probTr = 0.0;
            
            //select gage from table of gages
            try
            {
                var qry = siteinfo.Where(site => site.SiteNo==gage);
                var qq = qry.First<SiteInfo>();
                precip = qq.Precip;
                slope = qq.Slope;
                meanq=qq.MeanQ;
                qq=null;
                qry=null;
            }
            catch (Exception ex)
            {
                WriteLogFile(ex.Message);
            }
                       
            List<AnnualQ> curlst = new List<AnnualQ>();
            curlst = SelAnnQ7.ElementAt(iser);
            selSiteNum = selGages[iser];

            //bypass if < minimum years of data
            if (curlst.Count < minYears)
            {
                WriteLogFile("Annual series for gage: " + gage + " is less than minimum years of "+minYears);
                Debug.WriteLine("Annual series for gage: {0} is less than minimum years of {1}",gage,minYears);
                return (-1);
            }

            List<double> annualser = new List<double>();

            yseries.Clear();
            //
            //annualser is raw annual series, yseries is series with Q > 0
            //
            annualser = (from item in curlst
                       where Convert.ToString(item.QnDay) != "NaN" 
                       select Convert.ToDouble(item.QnDay)).ToList();
            int nCount = annualser.Count;

            //
            // qcfs > 0, nCount of qcfs>0, yseries is series of qcfs > 0
            //
            foreach(var item in annualser)
                if (item > 0.0) yseries.Add(Math.Log10(item));

            int nCountZero = nCount - yseries.Count;
            probzero = Convert.ToDouble(nCountZero) / nCount;

            annualser = null;

            //if (nCount < minYears) return(-1);

            Frequency freq = new Frequency();
            siteprob.Clear(); qpr.Clear();
            probTr = 1.0 / ReturnPeriod;

            double zscore; double k1; double kt; double q; double pgx; double padj;
            try
            {
                // stats of log series > 0
                double mean = MathNet.Numerics.Statistics.Statistics.Mean(yseries);
                double stdev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(yseries);
                double skew = MathNet.Numerics.Statistics.Statistics.Skewness(yseries);
                double ncnt = Convert.ToDouble(yseries.Count);
                double df = ncnt;
                double df1 = ncnt - 1.0;
                //
                // do for all probs - 0.1,0.2,0.25,0.50, 0.90, 0.99
                // and indicated return period
                //
                for (int k = 0; k < prb.Count; k++)
                {
                    //adjusted probability based on num of zeroes
                    pgx = (nCount/ncnt)*(prb[k]-probzero);
                    //
                    //from R-IHA, this is same as pgx
                    //padj = (prb[k] * nCount / ncnt) - ((nCount - ncnt) / ncnt);
                    //
                    q = 0.0;
                    if (pgx > 0)
                    {
                        zscore = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, pgx);
                        k1 = 1 + skew * (zscore / 6.0) - (skew * skew) / 36.0;
                        kt = (2.0 / skew) * (k1 * k1 * k1 - 1.0);
                        q = Math.Pow(10.0, mean + kt * stdev);
                    }
                    qpr.Add(q);
                    //Debug.WriteLine("{0},{1},{2}", prb[k], pgx, padj);
                }

                // calculate for probTr
                probTr = 1.0 / ReturnPeriod;
                if (probTr > probzero)
                {
                    pgx = (nCount / ncnt) * (probTr - probzero);
                    zscore = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, pgx);
                    k1 = 1 + skew * (zscore / 6.0) - (skew * skew) / 36.0;
                    kt = (2.0 / skew) * (k1 * k1 * k1 - 1.0);
                    q = Math.Pow(10.0, mean + kt * stdev);
                    freq.PTr = q;
                    
                    // 90% confidence level
                    double lamb = Math.Pow((1.0 + (skew * kt) + 0.5 * (1.0 + 0.75 * skew * skew) * kt * kt) / 
                        (1.0 + (0.5 * zscore*zscore)), 0.5);

                    string script;
                    double eps1, eps2;
                    double qucl, qlcl,t;
                    try
                    {
                        engine.Evaluate("df<-" + df);
                        engine.Evaluate("df1<-" + df1);
                        engine.Evaluate("pr<-" + pgx);
                        engine.Evaluate("p1<-0.95");
                        engine.Evaluate("p2<-0.05");

                        script = "out<-qt(p1,df1,qnorm(pr)*sqrt(df))/sqrt(df)";
                        NumericVector uclp = engine.Evaluate(script).AsNumeric();
                        eps2 = Convert.ToDouble(uclp[0]);

                        script = "out<-qt(p2,df1,qnorm(pr)*sqrt(df))/sqrt(df)";
                        NumericVector lclp = engine.Evaluate(script).AsNumeric();
                        eps1 = Convert.ToDouble(lclp[0]);
                        
                        t = mean + stdev * (kt + lamb * (eps1 - zscore));
                        qlcl = Math.Pow(10.0, t);
                        t = mean + stdev * (kt + lamb * (eps2 - zscore));
                        qucl = Math.Pow(10.0, t);
                        //Debug.WriteLine("mean={0},stdev={1}", mean, stdev);
                        //Debug.WriteLine("kt={0},zscore={1},lamb={2}", kt, zscore, lamb);
                        //Debug.WriteLine("{0},{1},{2},{3},{4}", eps1, eps2, q, qlcl, qucl);
                        freq.LCL90 = qlcl;
                        freq.UCL90 = qucl;
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile(ex.Message+"\r\n"+ex.StackTrace);
                        Debug.WriteLine(ex.Message + ex.Source);
                    }
                }
                else
                {
                    freq.PTr = 0.0;
                }

                //add results to list
                freq.SiteNo = gage;
                freq.Mean = mean;
                freq.StDev = stdev;
                freq.Skew = skew;
                freq.P0 = probzero;
                freq.P10 = qpr[0];
                if (probzero > 0.1) { freq.P10 = 0.0; }
                freq.P20 = qpr[1];
                if (probzero > 0.2) { freq.P20 = 0.0; }
                freq.P25 = qpr[2];
                if (probzero > 0.25) { freq.P25 = 0.0; }
                freq.P50 = qpr[3];
                if (probzero > 0.5) { freq.P50 = 0.0; }
                freq.P90 = qpr[4];
                if (probzero > 0.90) { freq.P90 = 0.0; }
                freq.P99 = qpr[5];
                if (probzero > 0.99) { freq.P99 = 0.0; }

                //freq.Precip = precip / (1000.0 * 2.54);  //in
                freq.Precip = precip;                       //in
                freq.Slope = slope;

                freq.SqMi = dArea[iser];

                freq.NumYrs = Convert.ToInt16(nCount);
                siteprob.Add(freq);

                DataRow g = tblResults.NewRow();
                g["Gage"] = selSiteNum;
                g["NYrs"] = freq.NumYrs;
                g["SqMi"] = freq.SqMi;
                g["Skew"] = skew;
                g["PrZero"] = probzero;
                g["PrTr"] = freq.PTr;
                g["LCL90"] = freq.LCL90;
                g["UCL90"] = freq.UCL90;
                g["P10"] = freq.P10; 
                g["P20"] = freq.P20;
                g["P25"] = freq.P25;
                g["P50"] = freq.P50;
                g["P90"] = freq.P90;
                g["P99"] = freq.P99;
                g["PRECIP"] = freq.Precip;
                g["MEANQ"] = meanq;
                g["Slope"] = freq.Slope;
                tblResults.Rows.Add(g);

                g = null;
                freq = null;
                curlst = null;
                return (qpr[0]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error in adding row g");

                curlst = null;
                return (-1.0);
            }
        }

        private void cboGages_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cboGages.SelectedIndex == 0)
            //{
            //   bs.Filter = string.Format("REFERENCE = 'Y'"); 
            //}
            //else if (cboGages.SelectedIndex == 1)
            //{
            //    bs.Filter = string.Format("REFERENCE = 'N'");
            //}
            //else if (cboGages.SelectedIndex == 2)
            //{
            //   bs.Filter = string.Format("REFERENCE = 'Y' OR REFERENCE = 'N'");
            //}
        }
        private void optAnn_Click(object sender, EventArgs e)
        {
            if (cboSeries.SelectedIndex < 0) { return; }
            
            int selIndex = cboSeries.SelectedIndex;
            //Debug.WriteLine("select index in cboseries =" + selIndex);
            OptSeries = "Annual";
            CreateGridViewAnnual(selIndex);
        }
        private void optDly_Click(object sender, EventArgs e)
        {
            if (cboSeries.SelectedIndex < 0) { return; }
            int selIndex = cboSeries.SelectedIndex;
            //Debug.WriteLine("select index in cboseries =" + selIndex);
            OptSeries = "Daily";
            CreateGridViewDaily(selIndex);
        }

        private void dgvResults_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            splitResultsUpper.Panel2Collapsed = false;

            double thres = Math.Log(0.00001);
            //plot cdf
            int row = e.RowIndex;
            string sgage = dgvResults.Rows[row].Cells["Gage"].Value.ToString();


            // find index in the selGages
            int idx = selGages.IndexOf(sgage);
            //Debug.WriteLine("selected result gage = {0}, index={1}" , sgage,idx);

            List<AnnualQ> curlst = new List<AnnualQ>();
            curlst = SelAnnQ7.ElementAt(idx);
            selSiteNum = selGages[idx];

            yseries.Clear();
            yseries = (from item in curlst
                       where Convert.ToString(item.logQ) != "NaN" &&
                             Convert.ToDouble(item.logQ) > thres
                       select Convert.ToDouble(item.logQ)).ToList();
            yseries.Sort();


            //
            //calculate LP3 fit
            //
            double mean = MathNet.Numerics.Statistics.Statistics.Mean(yseries);
            double stdev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(yseries);
            double skew = MathNet.Numerics.Statistics.Statistics.Skewness(yseries);

            //set series for plotting
            List<double> xseries = new List<double>();
            List<double> yobs = new List<double>();
            List<double> yfit = new List<double>();

            int nCnt = yseries.Count;
            int nCntNoZero = nCnt;
            int nCount = curlst.Count;
            int nZero = nCount - nCntNoZero;

            //Debug.WriteLine("Counts: {0},{1},{2},{3}", selSiteNum,nCount, nCntNoZero,nZero);

            double padj, zscore,k1,kt,q;

            for(int k=0; k<yseries.Count; k++)
            {
                int kk = k + nZero+1;
                double pi = Convert.ToDouble(kk)/(nCount+1);
                //double pi = Convert.ToDouble(k + 1) / (nCnt + 1);
                //pgx = (nCount / ncnt) * (prb[k] - probzero);
                //padj = (prb[k] * nCount / ncnt) - ((nCount - ncnt) / ncnt);

                padj = ((pi * nCount) / Convert.ToDouble(nCntNoZero)) - 
                    ((nCount - nCntNoZero) / Convert.ToDouble(nCntNoZero));

                q = 0.0;
                //if (padj > 0.0)
                {
                    zscore = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, padj);
                    k1 = 1 + skew * (zscore / 6.0) - (skew * skew) / 36.0;
                    kt = (2.0 / skew) * (k1 * k1 * k1 - 1.0);
                    q = Math.Pow(10.0, mean + kt * stdev);
                }
                
                //Debug.WriteLine("{0},{1},{2},{3},{4}",
                //    kk,pi,padj,yseries[k],q);
                
                //xseries.Add(pi);
                xseries.Add(pi);
                yobs.Add(Math.Pow(10.0, yseries[k]));
                yfit.Add(q);
            }

            CreateCDFGraph(xseries, yobs, yfit);
            xseries = null;
            yobs = null;
            yfit = null;

            //fill dgvProb
            DataRow[] drow = tblResults.Select("Gage=" + sgage);

            tblSiteProb.Clear();
            DataRow g = tblSiteProb.NewRow();
            g["Gage"] = drow[0][0].ToString();
            g["P10"] = Convert.ToDouble(drow[0][6]);
            g["P20"] = Convert.ToDouble(drow[0][7]);
            g["P25"] = Convert.ToDouble(drow[0][8]);
            g["P50"] = Convert.ToDouble(drow[0][9]);
            g["P90"] = Convert.ToDouble(drow[0][10]);
            g["P99"] = Convert.ToDouble(drow[0][11]);
            tblSiteProb.Rows.Add(g);

            dgvProb.DataSource = null;
            dgvProb.DataSource = tblSiteProb;

        }
        private void CreateCDFGraph(List<double> xseries, List<double> yobs, List<double> yfit)
        {
            zedGraphCDF.Size = new Size(ClientRectangle.Width - 20,
                                    ClientRectangle.Height - 20);

            // get a reference to the GraphPane
            GraphPane seriesPane = zedGraphCDF.GraphPane;
            currentPane = seriesPane;
            seriesPane.Legend.IsVisible = false;

            seriesPane.Title.Text = "Gage: " + selSiteNum;
            seriesPane.XAxis.Title.Text = "Probability";
            seriesPane.YAxis.Title.Text = nDayQ+"-Day Discharge, cfs";
            seriesPane.XAxis.Type = AxisType.Probability;
            seriesPane.YAxis.Type = AxisType.Log;
            seriesPane.XAxis.Scale.FontSpec.Size = 8;
            seriesPane.YAxis.Scale.FontSpec.Size = 8;

            // Add gridlines to the plot, and make them gray
            seriesPane.XAxis.MajorGrid.IsVisible = true;
            seriesPane.YAxis.MajorGrid.IsVisible = true;
            seriesPane.XAxis.MajorGrid.Color = Color.LightGray;
            seriesPane.YAxis.MajorGrid.Color = Color.LightGray;

            obsSeries.Clear();
            fitSeries.Clear();
            for (int i = 0; i < xseries.Count; i++)
            {
                obsSeries.Add(xseries[i], yobs[i]);
                fitSeries.Add(xseries[i], yfit[i]);
            }

            // Generate a red curve with diamond
            LineItem seriesCurve1 = seriesPane.AddCurve("",
                 obsSeries, Color.Red, ZedGraph.SymbolType.Circle);
            seriesCurve1.Line.IsVisible = false;

            // Generate a blue curve with circle
            LineItem seriesCurve2 = seriesPane.AddCurve("",
                  fitSeries, Color.Blue, ZedGraph.SymbolType.Circle);
            seriesCurve2.Line.IsVisible = true;
            seriesCurve2.Symbol.IsVisible = false;

            // Tell ZedGraph to refigure the
            // axes since the data have changed
            zedGraphCDF.AxisChange();
            zedGraphCDF.Refresh();
        }
        private void dgvGages_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int row = e.RowIndex;
            string site = dgvGages.Rows[row].Cells["SITE_NO"].Value.ToString();
            httpGage = "http://waterdata.usgs.gov/nwis/nwisman/?site_no=" + site;
            fweb = new frmWeb(httpGage);
            fweb.ShowDialog();
        }
        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count < 1)
            {
                splitResultsUpper.Panel2Collapsed = true;
            }
            else
            {
                splitResultsUpper.Panel2Collapsed = false;
            }
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {

        }
        private void cboDay_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx =cboDay.SelectedIndex;
            nDayQ = Convert.ToInt32(cboDay.SelectedItem);
        }

        private double GetDecimalYear(int yr, int mon, int day, int hr, int min, int sec)
        {
            int DayOfYr;
            float fracDay;
            double dyr;

            fracDay = Convert.ToSingle((hr + (min + sec / 60.0) / 60.0) / 24.0);

            int[] comyr = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 335 };
            int[] leapyr = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 336 };

            if ((yr % 4) == 0)
            {
                DayOfYr = leapyr[mon - 1] + day;
                dyr = yr + (DayOfYr + fracDay) / 366.0;
            }
            else
            {
                DayOfYr = comyr[mon - 1] + day;
                dyr = yr + (DayOfYr + fracDay) / 365.0;
            }
            return dyr;
        }

        private void btnCLear_Click(object sender, EventArgs e)
        {
            seriesCount = dgvGages.SelectedRows.Count;
            if (seriesCount > 0)
            {
                dgvGages.ClearSelection();
                seriesCount = 0;
                //btnCLear.Enabled = false;
            }
        }

        private void DoRegionalAnalysis()
        {
            //clear controls and initialize, select first 
            lstRegData.Clear();
            lstDepVar.SelectedIndex = 0;
            splitModelResults.Panel2Collapsed = true;
            txtLat.Text = "";
            txtLon.Text = "";
            txtFlow.Text = "";
            txtFor.Text = "";
            txtRain.Text = "";
            txtWet.Text = "";
            txtUrb.Text = "";
            txtArea.Text = "";
            btnCalc.Enabled = false;

            int mingage = 4;

            DataRow[] drows = tblResults.Select("PrTr > 0");
            if (drows.Count() < mingage)
            {
                MessageBox.Show("Regional Analysis requires at least " + mingage + " gages with Q > 0.0.");
                return;
            }

            foreach (DataRow row in drows)
            {
                RegData dt = new RegData();
                dt.SiteNo = row["Gage"].ToString();
                dt.Qcfs = Convert.ToDouble(row["PrTr"]);
                dt.Area = Convert.ToDouble(row["SqMi"]);
                dt.Rainfall = Convert.ToDouble(row["Precip"]);

                var qry = siteinfo.Where(site => site.SiteNo == dt.SiteNo);
                var sinfo = qry.First<SiteInfo>();
                dt.MeanFlow = sinfo.MeanQ;
                dt.Forest = sinfo.Forest;
                dt.Urban = sinfo.Urban;
                //dt.Wetland = sinfo.Wetlands;
                dt.Others = sinfo.Others;

                //if (dt.Qcfs > 0 && Convert.ToString(dt.Qcfs) != "NaN" && Convert.ToString(dt.MeanFlow) != "NaN")
                if (dt.Qcfs > 0 && !double.IsNaN(dt.Area) && !double.IsNaN(dt.MeanFlow) && !double.IsNaN(dt.Rainfall))
                    lstRegData.Add(dt);
                dt = null;
            }

            if (lstRegData.Count() < mingage)
            {
                string s = "Regional Analysis requires at least " + mingage + " gages with Q > 0.0.\r\n";
                s = s + "Some stations have no data for Area or Rainfall or Mean Daily Flow";
                MessageBox.Show(s);
                return;
            }

            tabCtl.TabPages.Remove(tabRegional);
            tabCtl.TabPages.Add(tabRegional);
            tabCtl.SelectedTab = tabRegional;

            dgvSelRegGages.DataSource = lstRegData;
            //dgvSelRegGages.Columns["Qcfs"].HeaderText = nDayQ + "Q" + ReturnPeriod;
            dgvSelRegGages.ClearSelection();
            dgvSelRegGages.SelectAll();

            StringBuilder msg = new StringBuilder();
            msg.Clear();
            msg.Append("To perform regional analysis, select gages by clicking row header and ");
            msg.Append("selecting the independent variables: drainage area, basin rainfall and mean daily flow, ");
            msg.Append("Percent Forest, Percent Urban and Percent Others (non-urban or forest).");
            msg.Append("The Regression equations are developed using the natural logarithms of the variables.");
            msg.Append("The general model is ln(Q) = b0 + b1*ln(Area) + b2*ln(Rain) + b3*ln(MeanQ) + b4*ln(Forest) ");
            msg.Append("+ b5*ln(Urban) + b6*ln(Others) and its subsets, i.e. lnQ a function of ");
            msg.Append("combination of the six independent variables.\r\n");
            msg.Append("Click on the header of a grid row to view diagnostic plots.");
            txtRegInfo.Text = msg.ToString();
        }

        private void btnRegion_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Use of landuse for regional analysis is disabled.\r\n Problems with EPA webservice for Basin Characteristics");
            DoRegionalAnalysis();
        }

        private void dgvSelRegGages_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int gageCount = dgvSelRegGages.SelectedRows.Count;
        }

        private void CalcRegressionInR_old(int gageCount, List<string> indepVar)
        {
            double[] qcfs = new double[gageCount];
            double[] darea = new double[gageCount];
            double[] rain = new double[gageCount];
            double[] avgq = new double[gageCount];
            double[] forst = new double[gageCount];
            double[] urban = new double[gageCount];
            double[] wetl = new double[gageCount];

            int ii = 0;
            foreach (DataGridViewRow rw in dgvSelRegGages.SelectedRows)
            {
                int idx = dgvSelRegGages.Rows.IndexOf(rw);
                double g_qcfs = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Qcfs"].Value);
                double g_darea = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Area"].Value);
                double g_rain = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Rainfall"].Value);
                double g_avgq = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["MeanFlow"].Value);
                double g_forst = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Forest"].Value);
                double g_urban = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Urban"].Value);
                double g_wetl = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Wetland"].Value);

                //Debug.WriteLine("{0},{1},{2},{3}", g_qcfs, g_darea, g_rain, g_avgq);

                if (g_qcfs > 0)
                {
                    qcfs[ii] = Math.Log(g_qcfs);
                    if (!double.IsNaN(g_darea) && !double.IsNaN(g_rain) && !double.IsNaN(g_avgq))
                    {
                        darea[ii] = Math.Log(g_darea);
                        rain[ii] = Math.Log(g_rain);
                        avgq[ii] = Math.Log(g_avgq);
                        forst[ii] = Math.Log(g_forst);
                        urban[ii] = Math.Log(g_urban);
                        wetl[ii] = Math.Log(g_wetl);
                        //Debug.WriteLine("{0},{1},{2},{3}", qcfs[ii], darea[ii], rain[ii],avgq[ii]);
                        ii++;
                    }
                }
            }

            string script = "";
            string model = "";
            string lmModel = "model" + ModelNo;

            List<string> svars = new List<string>() { "lnda", "lnrr", "lnaQ", "lnFor", "lnUrbn", "lnWet" };
            List<string> smod = new List<string>() { "Area", "Rain", "MeanQ", "Forest", "Urban", "Wetlnd" };

            switch (indepVar.Count)
            {
                case 1:
                    string dvar = indepVar[0];
                    if (dvar == "Drainage Area")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnda)";
                        model = "Q=fn(Area)";
                    }
                    else if (dvar == "Mean Annual Rainfall")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnrr)";
                        model = "Q=fn(Rain)";
                    }
                    else if (dvar == "Mean Annual Flow")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnaQ)";
                        model = "Q=fn(MeanQ)";
                    }
                    break;

                case 2:
                    int id = lstDepVar.Items.IndexOf(indepVar[0]);
                    string s = svars[id];
                    string sm = smod[id];

                    //Debug.WriteLine("id=" + id);
                    id = lstDepVar.Items.IndexOf(indepVar[1]);
                    //Debug.WriteLine("id=" + id);
                    s = s + "+" + svars[id];
                    sm = sm + "," + smod[id];

                    script = "model" + ModelNo + "<-lm(lnq~" + s + ")";
                    model = "Q=fn(" + sm + ")";
                    break;

                case 3:
                    script = "model" + ModelNo + "<-lm(lnq~lnda+lnrr+lnaQ)";
                    model = "Q=fn(Area, Rain, MeanQ)";
                    break;
            }

            try
            {
                NumericVector v_qcfs = engine.CreateNumericVector(qcfs);
                engine.SetSymbol("lnq", v_qcfs);
                NumericVector v_area = engine.CreateNumericVector(darea);
                engine.SetSymbol("lnda", v_area);
                NumericVector v_rain = engine.CreateNumericVector(rain);
                engine.SetSymbol("lnrr", v_rain);
                NumericVector v_avgq = engine.CreateNumericVector(avgq);
                engine.SetSymbol("lnaQ", v_avgq);

                engine.Evaluate(script);
                //Debug.WriteLine("script"+script);

                //GenericVector res = engine.Evaluate("out<-summary(model)").AsList();
                GenericVector res = engine.Evaluate("out<-summary(" + lmModel + ")").AsList();
                //Debug.WriteLine("res=" + res);

                double rsqr = res["r.squared"].AsNumeric().First();
                double adjrsqr = res["adj.r.squared"].AsNumeric().First();
                double sigma = res["sigma"].AsNumeric().First();
                //Debug.WriteLine("rsqr={0},adjrsqr={1},sigma={2}", rsqr, adjrsqr,sigma);
                NumericVector coef = res["coefficients"].AsNumeric();
                //foreach(var it in coef)  Debug.WriteLine("coef=" + it);
                NumericVector fstat = res["fstatistic"].AsNumeric();
                //foreach (var it in fstat) Debug.WriteLine("ftats=" + it);

                DataRow rw = tblModel.NewRow();
                rw["ModelNo"] = lmModel;
                rw["Model"] = model;
                rw["N"] = gageCount;
                rw["R-sqr"] = rsqr;
                rw["b0"] = coef[0];
                rw["b1"] = coef[1];
                if (indepVar.Count == 2)
                {
                    rw["b1"] = coef[1];
                    rw["b2"] = coef[2];
                }
                if (indepVar.Count == 3)
                {
                    rw["b1"] = coef[1];
                    rw["b2"] = coef[2];
                    rw["b3"] = coef[3];
                }
                tblModel.Rows.Add(rw);

                dgvModel.DataSource = tblModel;
                dgvModel.Columns["ModelNo"].Visible = false;

                dgvModel.ClearSelection();

                if (dgvModel.RowCount > 0)
                {
                    //lblStatus.Text = "Click on row header of Model Results for diagnostic plots ...";
                }
                else
                {
                    //lblStatus.Text = "Ready ...";
                }
                //statusStrip.Refresh();

            }
            catch (Exception exr)
            {
                Debug.WriteLine(exr.Message + "\r\n" + exr.StackTrace);
                WriteLogFile(exr.Message + "\r\n" + exr.StackTrace);
            }
        }

        /// <summary>
        /// Fits regression models
        /// </summary>
        /// <param name="gageCount"></param>
        /// <param name="indepVar"></param>
        private void CalcRegressionInR(int gageCount, List<string> indepVar)
        {
            double[] qcfs = new double[gageCount];
            double[] darea = new double[gageCount];
            double[] rain = new double[gageCount];
            double[] avgq = new double[gageCount];
            double[] forst = new double[gageCount];
            double[] urban = new double[gageCount];
            double[] wetl = new double[gageCount];

            int ii = 0;
            foreach (DataGridViewRow rw in dgvSelRegGages.SelectedRows)
            {
                int idx = dgvSelRegGages.Rows.IndexOf(rw);
                double g_qcfs = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Qcfs"].Value);
                double g_darea = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Area"].Value);
                double g_rain = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Rainfall"].Value);
                double g_avgq = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["MeanFlow"].Value);
                double g_forst = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Forest"].Value);
                double g_urban = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Urban"].Value);
                double g_wetl = Convert.ToDouble(dgvSelRegGages.Rows[idx].Cells["Others"].Value);

                //Debug.WriteLine("{0},{1},{2},{3}", g_qcfs, g_darea, g_rain, g_avgq);

                //DISABLE LANDUSE
                //if (g_qcfs > 0 && g_darea > 0 && g_rain > 0 && g_avgq > 0 && g_forst> 0
                //    && g_urban > 0 && g_wetl >0)
                if (g_qcfs > 0 && g_darea > 0 && g_rain > 0 && g_avgq > 0 && g_forst >= 0
                      && g_urban >= 0 && g_wetl >= 0)
                {
                    qcfs[ii] = Math.Log(g_qcfs);
                    if (!double.IsNaN(g_darea) && !double.IsNaN(g_rain) && !double.IsNaN(g_avgq))
                    {
                        darea[ii] = Math.Log(g_darea);
                        rain[ii] = Math.Log(g_rain);
                        avgq[ii] = Math.Log(g_avgq);
                        forst[ii] = Math.Log(g_forst);
                        urban[ii] = Math.Log(g_urban);
                        wetl[ii] = Math.Log(g_wetl);
                        //Debug.WriteLine("{0},{1},{2},{3}", qcfs[ii], darea[ii], rain[ii],avgq[ii]);
                        ii++;
                    }
                }
            }

            string script = "";
            string model = "";
            string lmModel = "model" + ModelNo;

            List<string> svars = new List<string>() { "lnda", "lnrr", "lnaQ", "lnFor", "lnUrbn", "lnOther" };
            List<string> smod = new List<string>() { "Area", "Rain", "MeanQ", "Forest", "Urban", "Others" };

            int id;
            string s = "";
            string sm = "";

            switch (indepVar.Count)
            {
                case 1:
                    string dvar = indepVar[0];
                    if (dvar == "Drainage Area")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnda)";
                        model = "Q=fn(Area)";
                    }
                    else if (dvar == "Mean Annual Rainfall")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnrr)";
                        model = "Q=fn(Rain)";
                    }
                    else if (dvar == "Mean Annual Flow")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnaQ)";
                        model = "Q=fn(MeanQ)";
                    }
                    else if (dvar == "Percent Forest")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnFor)";
                        model = "Q=fn(Forest)";
                    }
                    else if (dvar == "Percent Urban")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnUrbn)";
                        model = "Q=fn(Urban)";
                    }
                    else if (dvar == "Percent Others")
                    {
                        script = "model" + ModelNo + "<-lm(lnq~lnOther)";
                        model = "Q=fn(Others)";
                    }
                    break;

                case 2:
                    id = lstDepVar.Items.IndexOf(indepVar[0]);
                    s = svars[id];
                    sm = smod[id];

                    id = lstDepVar.Items.IndexOf(indepVar[1]);
                    s = s + "+" + svars[id];
                    sm = sm + "," + smod[id];

                    script = "model" + ModelNo + "<-lm(lnq~" + s + ")";
                    model = "Q=fn(" + sm + ")";
                    break;

                case 3:
                    id = lstDepVar.Items.IndexOf(indepVar[0]);
                    s = svars[id];
                    sm = smod[id];

                    for (int k = 1; k < indepVar.Count; k++)
                    {
                        id = lstDepVar.Items.IndexOf(indepVar[k]);
                        s = s + "+" + svars[id];
                        sm = sm + "," + smod[id];
                    }

                    script = "model" + ModelNo + "<-lm(lnq~" + s + ")";
                    model = "Q=fn(" + sm + ")";
                    break;

                case 4:
                    id = lstDepVar.Items.IndexOf(indepVar[0]);
                    s = svars[id];
                    sm = smod[id];

                    for (int k = 1; k < indepVar.Count; k++)
                    {
                        id = lstDepVar.Items.IndexOf(indepVar[k]);
                        s = s + "+" + svars[id];
                        sm = sm + "," + smod[id];
                    }

                    script = "model" + ModelNo + "<-lm(lnq~" + s + ")";
                    model = "Q=fn(" + sm + ")";
                    break;

                case 5:
                    id = lstDepVar.Items.IndexOf(indepVar[0]);
                    s = svars[id];
                    sm = smod[id];

                    for (int k = 1; k < indepVar.Count; k++)
                    {
                        id = lstDepVar.Items.IndexOf(indepVar[k]);
                        s = s + "+" + svars[id];
                        sm = sm + "," + smod[id];
                    }

                    script = "model" + ModelNo + "<-lm(lnq~" + s + ")";
                    model = "Q=fn(" + sm + ")";
                    break;

                case 6:
                    script = "model" + ModelNo + "<-lm(lnq~lnda+lnrr+lnaQ+lnFor+lnUrbn+lnOther)";
                    model = "Q=fn(Area,Rain,MeanQ,Forest,Urban,Others)";
                    break;
            }

            try
            {
                NumericVector v_qcfs = engine.CreateNumericVector(qcfs);
                engine.SetSymbol("lnq", v_qcfs);
                NumericVector v_area = engine.CreateNumericVector(darea);
                engine.SetSymbol("lnda", v_area);
                NumericVector v_rain = engine.CreateNumericVector(rain);
                engine.SetSymbol("lnrr", v_rain);
                NumericVector v_avgq = engine.CreateNumericVector(avgq);
                engine.SetSymbol("lnaQ", v_avgq);

                NumericVector v_forst = engine.CreateNumericVector(forst);
                engine.SetSymbol("lnFor", v_forst);
                NumericVector v_urban = engine.CreateNumericVector(urban);
                engine.SetSymbol("lnUrbn", v_urban);
                NumericVector v_wetl = engine.CreateNumericVector(wetl);
                engine.SetSymbol("lnOther", v_wetl);

                engine.Evaluate(script);

                GenericVector res = engine.Evaluate("out<-summary(" + lmModel + ")").AsList();

                double rsqr = res["r.squared"].AsNumeric().First();
                double adjrsqr = res["adj.r.squared"].AsNumeric().First();
                double sigma = res["sigma"].AsNumeric().First();
                NumericVector coef = res["coefficients"].AsNumeric();
                NumericVector fstat = res["fstatistic"].AsNumeric();

                // get the regression coefficients
                DataRow rw = tblModel.NewRow();
                rw["ModelNo"] = lmModel;
                rw["Model"] = model;
                rw["N"] = gageCount;
                rw["R-sqr"] = rsqr;
                rw["b0"] = coef[0];

                string bcoef = "";
                for (int k = 0; k < indepVar.Count; k++)
                {
                    id = lstDepVar.Items.IndexOf(indepVar[k]);
                    bcoef = "b" + Convert.ToString(id+1);
                    rw[bcoef] = coef[k+1];
                }

                tblModel.Rows.Add(rw);

            }
            catch (Exception exr)
            {
                //Debug.WriteLine(exr.Message + "\r\n" + exr.StackTrace);
                WriteLogFile(exr.Message + "\r\n" + exr.StackTrace);
            }
        }

        private void btnModel_Click(object sender, EventArgs e)
        {
            dgvSelRegGages.SelectAll();
            int gageCount = dgvSelRegGages.SelectedRows.Count;
            int indepCnt = lstDepVar.SelectedItems.Count;

            int cnt = gageCount - 2;
            if (gageCount - 2 < indepCnt)
            {
                MessageBox.Show("Can only select at most " + cnt + " independent variables!");
                return;
            }

            ModelNo = ModelNo + 1;
            List<string> indepVar = new List<string>();
            foreach (string item in lstDepVar.SelectedItems)
                 indepVar.Add(item);

            indepCnt = indepVar.Count();

            if (gageCount > 3 && indepCnt > 0) 
                 CalcRegressionInR(gageCount, indepVar);

            if (tblModel.Rows.Count > 0)
            {
                dgvModel.DataSource = tblModel;
                dgvModel.Columns["ModelNo"].Visible = false;
                dgvModel.Columns["N"].Visible = false;
                dgvModel.ClearSelection();

                if (dgvModel.RowCount > 0)
                {
                    //lblStatus.Text = "Click on row header of Model Results for diagnostic plots ...";
                    splitModelResults.Panel2Collapsed = false;
                    btnEstMap.Enabled = true;
                    mnuEstimate.Enabled = true;
                }
                else
                {
                    splitModelResults.Panel2Collapsed = true;
                    btnEstMap.Enabled = false;
                    mnuEstimate.Enabled = false;
                }
                // enable estimation
                mnuEstimate.Enabled = true;
            }
            else
                mnuEstimate.Enabled = false;
            //statusStrip.Refresh();
        }

        private void dgvSelRegGages_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSelRegGages.SelectedRows.Count > 0)
            {
                btnModel.Enabled = true;
            }
            else
            {
                btnModel.Enabled = false;
            }
        }
        private void txtTR_TextChanged(object sender, EventArgs e)
        {
            ReturnPeriod = Convert.ToInt16(txtTR.Text);
        }
        private void txtTR_Validating(object sender, CancelEventArgs e)
        {
            int tr = Convert.ToInt16(txtTR.Text);
            if (tr < 0 || tr > 500)
            {
                txtTR.Text = "2";
            }
            ReturnPeriod = Convert.ToInt16(txtTR.Text);
        }
        private void txtYr_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtYr_Validated(object sender, EventArgs e)
        {
            int iyr = Convert.ToInt16(txtYr.Text);
            if (iyr < 10 || iyr > 120)
            {
                txtYr.Text = "10";
            }
            minYears = Convert.ToInt32(txtYr.Text);
        }
        private void txtDly_TextChanged(object sender, EventArgs e)
        {
            int ida = Convert.ToInt16(txtDly.Text);
            if (ida < 300 || ida > 365)
            {
                txtDly.Text = "300";
            }
            minDays = Convert.ToInt32(txtDly.Text);
        }

        private double CalculateFrequencyWithR(int iser, string gage)
        {
            List<double> prb = new List<double>() { 0.1, 0.2, 0.25, 0.5, 0.90, 0.99 };
            List<double> qpr = new List<double>();
            double precip = 0.0;
            double slope = 0;
            double probzero = 0.0;
            double probTr = 0.0;

            //select gage from table of gages
            try
            {
                var qry = siteinfo.Where(site => site.SiteNo == gage);
                var qq = qry.First<SiteInfo>();
                precip = qq.Precip;
                slope = qq.Slope;
                qq = null;
                qry = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            List<AnnualQ> curlst = new List<AnnualQ>();
            curlst = SelAnnQ7.ElementAt(iser);
            selSiteNum = selGages[iser];

            List<double> annualser = new List<double>();

            yseries.Clear();
            //yseries = (from item in curlst
            //           where Convert.ToString(item.logQ) != "NaN" && Convert.ToString(item.logQ) != "Infinity"
            //           select Convert.ToDouble(item.logQ)).ToList();

            //raw annual series
            annualser = (from item in curlst
                         where Convert.ToString(item.QnDay) != "NaN"
                         //Convert.ToDouble(item.QnDay) > 0.0
                         select Convert.ToDouble(item.QnDay)).ToList();

            foreach (var item in annualser)
                if (item > 0.0) yseries.Add(Math.Log10(item));
            int nCount = annualser.Count;

            int nCountZero = nCount - yseries.Count;
            probzero = Convert.ToDouble(nCountZero) / nCount;

            //Debug.WriteLine("Counts = {0},{1},{2},{3}", selSiteNum, nCount, nCountZero, probzero);
            annualser = null;

            if (nCount < minYears)
            {
                //return(-1);
            }

            try
            {
                //NumericVector value = engine.CreateNumericVector(yseries);
                //string script = "out<-seakenall(Year, PCode, Site)";
                //string[] sk = engine.Evaluate(script).AsCharacter().ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Frequency freq = new Frequency();
            siteprob.Clear(); qpr.Clear();
            probTr = 1.0 / ReturnPeriod;

            double zscore; double k1; double kt; double q;
            try
            {
                double mean = MathNet.Numerics.Statistics.Statistics.Mean(yseries);
                double stdev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(yseries);
                double skew = MathNet.Numerics.Statistics.Statistics.Skewness(yseries);

                // do for all probs - 0.1,0.2,0.25,0.50, 0.90, 0.99
                // and indicated return period
                for (int k = 0; k < prb.Count; k++)
                {
                    zscore = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, prb[k]);
                    k1 = 1 + skew * (zscore / 6.0) - (skew * skew) / 36.0;
                    kt = (2.0 / skew) * (k1 * k1 * k1 - 1.0);
                    q = Math.Pow(10.0, mean + kt * stdev);
                    qpr.Add(q);
                }

                // calculate for probTr
                if (probTr > probzero)
                {
                    zscore = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, probTr);
                    k1 = 1 + skew * (zscore / 6.0) - (skew * skew) / 36.0;
                    kt = (2.0 / skew) * (k1 * k1 * k1 - 1.0);
                    q = Math.Pow(10.0, mean + kt * stdev);
                    freq.PTr = q;
                }
                else
                {
                    freq.PTr = 0.0;
                }

                //add results to list
                freq.SiteNo = gage;
                freq.Mean = mean;
                freq.StDev = stdev;
                freq.Skew = skew;
                freq.P0 = probzero;
                freq.P10 = qpr[0];
                if (probzero > 0.1) { freq.P10 = 0.0; }
                freq.P20 = qpr[1];
                if (probzero > 0.2) { freq.P20 = 0.0; }
                freq.P25 = qpr[2];
                if (probzero > 0.25) { freq.P25 = 0.0; }
                freq.P50 = qpr[3];
                if (probzero > 0.5) { freq.P50 = 0.0; }
                freq.P90 = qpr[4];
                if (probzero > 0.90) { freq.P90 = 0.0; }
                freq.P99 = qpr[5];
                if (probzero > 0.99) { freq.P99 = 0.0; }

                freq.Precip = precip / (1000.0 * 2.54);  //in
                freq.Slope = slope;
                freq.SqMi = dArea[iser];

                //freq.NumYrs = Convert.ToInt16(yseries.Count());
                freq.NumYrs = Convert.ToInt16(nCount);
                siteprob.Add(freq);

                //Debug.WriteLine("{0},{1},{2},{3}", freq.SiteNo,freq.P10,freq.P99,freq.NumYrs);

                DataRow g = tblResults.NewRow();
                g["Gage"] = selSiteNum;
                g["NYrs"] = freq.NumYrs;
                g["SqMi"] = freq.SqMi;
                g["Skew"] = skew;
                g["PrZero"] = probzero;
                g["PrTr"] = freq.PTr;

                if (probzero > 0.1)
                {
                    g["P10"] = 0.0;
                }
                else
                {
                    g["P10"] = freq.P10;
                }
                g["P20"] = freq.P20;
                g["P25"] = freq.P25;
                g["P50"] = freq.P50;
                g["P90"] = freq.P90;
                g["P99"] = freq.P99;
                g["Precip"] = freq.Precip;
                g["Slope"] = freq.Slope;
                tblResults.Rows.Add(g);
                //Debug.WriteLine("{0},{1},{2},{3}", g[0],g["Skew"],g["P10"],g["NYrs"]);

                g = null;
                freq = null;
                curlst = null;
                return (qpr[0]);
            }
            catch (Exception ex)
            {
                //SelAnnQ7.ElementAt(iser).RemoveAt(iser);
                //selGages.RemoveAt(iser);
                Debug.WriteLine("error in adding row g");

                curlst = null;
                return (-1.0);
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            dgvGages.SelectAll();
        }

        private void SetupLogFile(string exepath)
        {
            StringBuilder str = new StringBuilder();

            str.Append(exepath + "\\");
            str.Append("FrequencyAnalysis.log");
            string logFile = str.ToString();

            fslog = new FileStream(logFile, FileMode.Create);
            wrlog = new StreamWriter(fslog);
            str = null;
        }

        private void WriteLogFile(string msg)
        {
            Debug.WriteLine(msg);
            wrlog.WriteLine(msg);
            wrlog.Flush();
            //wrlog.WriteLine("\r\n");
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            wrlog.Close();
            fslog.Close();

        }

        private void yrFrom_ValueChanged(object sender, EventArgs e)
        {
            if (yrFrom.Value > yrTo.Value)
            {
                yrFrom.Value = yrTo.Value - 10;
            }

            if (yrFrom.Value < yrFrom.Minimum)
            {
                yrFrom.Value = yrFrom.Minimum;
            }
            FromYear = Convert.ToInt32(yrFrom.Value);
        }

        private void yrTo_ValueChanged(object sender, EventArgs e)
        {
            if (yrTo.Value < yrFrom.Value)
            {
                if (yrTo.Value + 10 > yrTo.Maximum)
                {
                    yrTo.Value = yrTo.Maximum;
                }
                else
                {
                    yrTo.Value = yrTo.Value + 10;
                }
            }

            if (yrTo.Value > yrTo.Maximum)
            {
                yrTo.Value = yrTo.Maximum;
                if (yrTo.Value - yrFrom.Value < 10)
                {
                    yrFrom.Value = yrTo.Value - 10;
                }
            }
            ToYear = Convert.ToInt32(yrTo.Value);
            FromYear = Convert.ToInt32(yrFrom.Value);
        }

        private void dgvModel_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int row = e.RowIndex;
            string smodel = dgvModel.Rows[row].Cells["ModelNo"].Value.ToString();

            engine.Evaluate("oldpar <- par(oma=c(0,0,3,0), mfrow=c(2,2))");
            engine.Evaluate("plot("+smodel+")");
            engine.Evaluate("par(oldpar)");
        }

        private void mnuDoc_Click(object sender, EventArgs e)
        {
            WordApp = new Microsoft.Office.Interop.Word.Application();

            object sfile = Application.StartupPath.ToString() + "\\" + "Frequency Analysis Tool.docx";
            //object sfile = dataDir + "\\" + "Frequency Analysis Tool.docx";
            object missing = System.Reflection.Missing.Value;
            // Make word visible, so you can see what's happening
            WordApp.Visible = true;
            object readOnly = false;
            object isVisible = true;

            // Open the document that was chosen by the dialog
            try
            {
                Microsoft.Office.Interop.Word.Document aDoc = WordApp.Documents.Open(ref sfile, ref missing, ref readOnly, ref missing, 
                ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref isVisible);
                aDoc.Activate();
            }
            catch (Exception ex)
            {
                sfile = dataDir + "\\" + "Frequency Analysis Tool.docx";
                Microsoft.Office.Interop.Word.Document aDoc = WordApp.Documents.Open(ref sfile, ref missing, ref readOnly, ref missing,
                ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref isVisible);
                aDoc.Activate();
            }
            // Activate the document so it shows up in front
        }

        private void mnuWhat_Click(object sender, EventArgs e)
        {
            fAbout.Show();
        }

        private void mnuRegAnal_Click(object sender, EventArgs e)
        {
            DoRegionalAnalysis();
        }

        private void btnEstMap_Click(object sender, EventArgs e)
        {
            // provide instructions
            string lbl = "Left click on map to select point ...";
            appManager.UpdateProgress(lbl);
            // clear drawing layer if present
            MapMode = (int)SelectMode.Estimate;

            // clear drawing layer if exist
            if (appManager.Map.MapFrame.DrawingLayers.Contains(estPointLayer))
            {
                // Remove our drawing layer from the map.
                appManager.Map.MapFrame.DrawingLayers.Remove(estPointLayer);

                // Request a redraw
                appManager.Map.MapFrame.Invalidate();
            }                


            //show map tab
            tabCtl.SelectedTab = tabMap;

            //set map for selection of point
            // Enable left click panning and mouse wheel zooming
            uxMap.FunctionMode = FunctionMode.Pan;
            //this.Cursor = Cursors.Cross;

            uxMap.Cursor = Cursors.Cross;

            // The FeatureSet starts with no data; be sure to set it to the point featuretype
            estPoint = new FeatureSet(FeatureType.Point);

            // The MapPointLayer controls the drawing of the marker features
            estPointLayer = new MapPointLayer(estPoint);

            // The Symbolizer controls what the points look like
            estPointLayer.Symbolizer = new PointSymbolizer(Color.Blue, DotSpatial.Symbology.PointShape.Ellipse, 12);

            // A drawing layer draws on top of data layers, but is still georeferenced.
            uxMap.MapFrame.DrawingLayers.Add(estPointLayer);

            ZoomToSelectedGages();
            HighLightSelectedStations(selGages);
        }

        private void GetBasinCharacteristicsFromPoint()
        {
            SiteInfo st = new SiteInfo();
            //
            //get the Comid for the gage
            //

            double ylat = Convert.ToDouble(txtLat.Text);
            double xlon = Convert.ToDouble(txtLon.Text);

            WriteLogFile("Executing GetCOMID for point: " + ylat + "," + xlon);
            long comid = GetCOMIDFromPoint(xlon, ylat);
            st.Comid = Convert.ToString(comid);

            //
            //get landuse characteristics for the gage
            //
            WriteLogFile("Executing GetBasinCharacteristic for point: " + ylat + "," + xlon);
            GetBasinCharacteristics(st);
            
            //show in textboxes
            txtArea.Text = st.DArea.ToString();
            txtRain.Text = st.Precip.ToString();
            txtFlow.Text = st.MeanQ.ToString();
            txtFor.Text = st.Forest.ToString();
            txtUrb.Text = st.Urban.ToString();
            txtWet.Text = st.Others.ToString();

            st = null;

            // estimate flows forall models
            // EstimateFlow();
        }

        /// <summary>
        /// Estimation button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalc_Click(object sender, EventArgs e)
        {
            EstimateFlow();
        }

        private void EstimateFlow()
        {
            //clear table of estimate and create new site
            tblModelPred.Clear();

            //get from textboxes
            double[] svars = new double[6];
            svars[0] = Convert.ToDouble(txtArea.Text);
            svars[1] = Convert.ToDouble(txtRain.Text);
            svars[2] = Convert.ToDouble(txtFlow.Text);
            svars[3] = Convert.ToDouble(txtFor.Text);
            svars[4] = Convert.ToDouble(txtUrb.Text);
            svars[5] = Convert.ToDouble(txtWet.Text);

            //estmate each model
            foreach (DataRow drow in tblModel.Rows)
            {
                double b0 = Convert.ToDouble(drow["b0"]);
                double val = b0;
                for (int icol = 1; icol <= svars.Count(); icol++)
                {
                    string bcoef = "b" + icol;
                    if (drow[bcoef] != DBNull.Value)
                    {
                        val += Math.Log(svars[icol - 1]) * Convert.ToDouble(drow[bcoef]);
                        //Debug.WriteLine("{0},{1}", icol, svars[icol - 1]);
                    }
                }
                string model = drow["Model"].ToString();

                DataRow rw = tblModelPred.NewRow();
                rw["Model"] = model;
                rw["Estimate"] = Math.Exp(val);
                tblModelPred.Rows.Add(rw);
            }

            // show estimates in grid
            dgvEstimate.DataSource = tblModelPred;
            dgvEstimate.ClearSelection();

            //string slab = nDayQ + "Q" + ReturnPeriod;
            //dgvEstimate.Columns["Estimate"].Name = slab + " Estimate";
        }

        /// <summary>
        /// Create shape of selected stations
        /// </summary>
        private void CreateSelectedStationsShape()
        {
            string shpFile = "SelectedGages.shp";
            string shp = dataDir + "\\" + shpFile;

            FeatureSet fs = new FeatureSet(FeatureType.Point);
            fs.DataTable.Columns.Add(new DataColumn("SiteNo", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Lat", typeof(double)));
            fs.DataTable.Columns.Add(new DataColumn("Long", typeof(double)));

            fs.Projection = KnownCoordinateSystems.Geographic.NorthAmerica.NorthAmericanDatum1983;

            double x, y;
            foreach (SiteInfo sinfo in siteinfo)
            {
                x = sinfo.Lon;
                y = sinfo.Lat;

                Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                IFeature curFeature = fs.AddFeature(p);
                curFeature.DataRow.BeginEdit();
                curFeature.DataRow["SiteNo"] = sinfo.SiteNo;
                curFeature.DataRow["Lat"] = y;
                curFeature.DataRow["Long"] = x;
                curFeature.DataRow.EndEdit();
            }

            //reproject to webmercator
            fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
            fs.SaveAs(shp, true);
            fs.Dispose();
        }

        /// <summary>
        /// uxMap_SelectionChanged
        /// Gages Selected from Map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxMap_SelectionChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Function=" + uxMap.FunctionMode.ToString());
            if (isSeriesDownloaded)
            {
                if (uxMap.FunctionMode == FunctionMode.Info)
                    return;
            }

            if (MapMode == (int)SelectMode.Select)
            {
                // check if layer is gages layer
                IFeatureLayer selLayer = (IFeatureLayer)appManager.Map.Layers.SelectedLayer;
                if (selLayer.DataSet.Name != "USGS Gages")
                    return;

                //remove tabs if user changed selection
                if (tabCtl.Contains(tabGages))
                {
                    //dgvGages.Rows.Clear();
                    tabCtl.TabPages.Remove(tabGages);
                }
                if (tabCtl.Contains(tabSeries))
                    tabCtl.TabPages.Remove(tabSeries);

                if (tabCtl.Contains(tabRegional))
                {
                    //dgvSelRegGages.Rows.Clear();
                    //dgvModel.Rows.Clear();
                    tabCtl.TabPages.Remove(tabRegional);
                }
                if (tabCtl.Contains(tabResults))
                {
                    //dgvResults.Rows.Clear();
                    tabCtl.TabPages.Remove(tabResults);
                }

                if (mnuCalc7Q10.Enabled) mnuCalc7Q10.Enabled = false;
                if (mnuEstimate.Enabled) mnuEstimate.Enabled = false;
                if (mnuGetSeries.Enabled) mnuGetSeries.Enabled = false;

                List<IFeature> lstFeature = new List<IFeature>();
                lstFeature = selLayer.Selection.ToFeatureList();

                if (lstFeature.Count > 0)
                {
                    string selfs = dataDir + "\\" + "SelectedGages.shp";

                    IFeatureSet fs = selLayer.Selection.ToFeatureSet();
                    fs.FillAttributes();
                    tblSelGages = fs.DataTable;
                    dgvGages.DataSource = tblSelGages;
                    fs.SaveAs(selfs, true);

                    //get the selected gages into a list
                    selGages.Clear();
                    foreach (DataRow rw in tblSelGages.Rows)
                    {
                        string site = rw["Site_No"].ToString();
                        this.selGages.Add(site);
                    }

                    //show selected gages tab
                    if (!tabCtl.Contains(tabGages))
                    {
                        tabCtl.TabPages.Add(tabGages);
                        dgvGages.ClearSelection();
                        //tabCtl.SelectedTab = tabGages;
                    }

                    //5/12/17
                    isGagesSelected = true;
                }
                else
                {
                    isGagesSelected = false;
                    mnuGetSeries.Enabled = false;
                }
                lstFeature = null;
            }
        }
        private void SetSelectableLayer()
        {
            // can only select gages layers
            List<ILayer> mLayers = new List<ILayer>();
            mLayers = appManager.Map.GetLayers();

            foreach (var mp in mLayers)
            {
                if (mp.DataSet.Name != "USGS Gages")
                {
                    //mp.SelectionEnabled = false;
                    mp.IsSelected = false;
                }
                else
                {
                    //mp.SelectionEnabled = true;
                    mp.IsSelected = true;
                }
            }
            mLayers = null;
        }

        public void GetCoordinatesOfPoint(MouseEventArgs e)
        {
            if (MapMode == (int)SelectMode.Estimate)
            {
                // Intercept only the right click for adding markers
                if (e.Button != MouseButtons.Right) return;

                // Get the geographic location that was clicked
                Coordinate c = uxMap.PixelToProj(e.Location);

                //Debug.WriteLine("selected ex=" + c.X);
                //Debug.WriteLine("selected ey=" + c.Y);

                double[] xy = new double[2];
                double[] z = new double[1];
                z[0] = 1;
                xy[0] = c.X;
                xy[1] = c.Y;

                ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
                ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
                Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                Debug.WriteLine("In GetSelected Sites: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);
                
                Xlon = xy[0];
                Ylat = xy[1];
                
                txtLat.Text = string.Format("{0:0.00000}", Ylat);
                txtLon.Text = string.Format("{0:0.00000}", Xlon);
                
                btnPointOK.Enabled = true;

                // Add the new coordinate as a "point" to the point featureset
                if (estPoint.Features.Count > 0)
                {
                    estPoint.Features.Clear();
                    estPoint.AddFeature(new DotSpatial.Topology.Point(c));
                    btnPointOK.Enabled = true;
                }
                else
                {
                    estPoint.AddFeature(new DotSpatial.Topology.Point(c));
                    btnPointOK.Enabled = false;
                }

                // Drawing will take place from a bitmap buffer, so if data is updated,
                // we need to tell the map to refresh the buffer 
                appManager.Map.MapFrame.Invalidate();

                //GetBasinCharacteristicsFromPoint();
                //EstimateFlow();

            }
        }

        private void uxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (MapMode == (int)SelectMode.Estimate)
            {
                // Intercept only the right click for adding markers
                if (e.Button != MouseButtons.Right) return;
            }
        }

        //private void mnuProject_Click(object sender, EventArgs e)
        //{
           //OpenFileDialog fDialog = new OpenFileDialog();
           // if (fDialog.ShowDialog() == DialogResult.OK)
           // {
           //     string ProjectFile = fDialog.FileName;
           //     appManager.SerializationManager.OpenProject(ProjectFile);
           //     SetDataGrid();
           // }
        //}

        // estimate point flow
        private void mnuEstimate_Click(object sender, EventArgs e)
        {
            btnEstMap_Click(sender, e);
        }

        public void ClearDrawingLayer()
        {
            if (appManager.Map.MapFrame.DrawingLayers.Contains(estPointLayer))
            {
                    // Remove our drawing layer from the map.
                    appManager.Map.MapFrame.DrawingLayers.Remove(estPointLayer);

                    // Request a redraw
                    appManager.Map.MapFrame.Invalidate();
            }                
        }

        private void ZoomToSelectedGages()
        {
            string selfs = dataDir + "\\" + "SelectedGages.shp";

            try
            {
                if (MapMode == (int)SelectMode.Estimate)
                {
                    FeatureSet fs3 = (FeatureSet)FeatureSet.Open(selfs);
                    fs3.Reproject(appManager.Map.Projection);
                    Extent ext = fs3.Extent;

                    appManager.Map.ViewExtents = ext;
                    appManager.Map.Refresh();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void HighLightSelectedStations(List<string> selGages)
        {
            List<ILayer> mLayers = new List<ILayer>();
            mLayers = appManager.Map.GetLayers();

            foreach (var mp in mLayers)
            {
                if (mp.DataSet.Name == "USGS Gages")
                {
                    mp.SelectionEnabled = true;
                    mp.IsSelected = true;
                    IFeatureLayer selLayer = (IFeatureLayer)mp;
                    //Debug.WriteLine("In HighLightSelectedStations: selLayer "+ selLayer.DataSet.Name);

                    List<string> stgage = new List<string>();

                    foreach (string sgage in selGages)
                    {
                        //Debug.WriteLine("selected gage = " + sgage);
                        string s = "'" + sgage + "'";
                        stgage.Add(s);
                    }
                    string st = string.Join(",", stgage);
                    //Debug.WriteLine("selected = " + st);
                    selLayer.SelectByAttribute("[SITE_NO] IN (" + st + ")");
                    break;
                }
            }
            mLayers = null;
        }

        private void btnPointOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            bool coordboxIsEmpty = true;
            while (coordboxIsEmpty)
            {
                if (!string.IsNullOrEmpty(txtLat.Text) && !string.IsNullOrEmpty(txtLon.Text))
                {
                    coordboxIsEmpty = false;
                }
            }

            if (!string.IsNullOrEmpty(txtLat.Text) && !string.IsNullOrEmpty(txtLon.Text))
            {
                GetBasinCharacteristicsFromPoint();
                EstimateFlow();
                tabCtl.SelectedTab = tabRegional;
                btnCalc.Enabled = true;

                // change menu to enabled re-select gages
                mnuGetSeries.Visible = false;
                mnuReSelectGages.Visible = true;
            }
            else
            {
                btnCalc.Enabled = false;
            }
            
            this.Cursor = Cursors.Default;
        }

        private void uxMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (MapMode == (int)SelectMode.Estimate)
            {
                //intercept left button click
                if (e.Button != MouseButtons.Right) return;
                // Get the geographic location that was clicked
                Coordinate c = uxMap.PixelToProj(e.Location);

                //Debug.WriteLine("selected ex=" + c.X);
                //Debug.WriteLine("selected ey=" + c.Y);

                double[] xy = new double[2];
                double[] z = new double[1];
                z[0] = 1;
                xy[0] = c.X;
                xy[1] = c.Y;

                ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
                ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
                Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                Debug.WriteLine("In GetSelected Sites from mouseclick: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);
                Xlon = xy[0];
                Ylat = xy[1];
                txtLat.Text = string.Format("{0:0.00000}", Ylat);
                txtLon.Text = string.Format("{0:0.00000}", Xlon);
                
                Debug.WriteLine("In GetSelected Sites from mouseclick: textlon={0}, textlat={1}", txtLon.Text,txtLat.Text);

                //add point to drawing layers
                if (estPoint.Features.Count > 0)
                {
                    Debug.WriteLine("num point est features = " + estPoint.Features.Count);
                    estPoint.Features.Clear();
                    estPoint.AddFeature(new DotSpatial.Topology.Point(c));
                    btnPointOK.Enabled = true;
                    Debug.WriteLine("num point est features = " + estPoint.Features.Count);
                }
                else
                {
                    estPoint.AddFeature(new DotSpatial.Topology.Point(c));
                    btnPointOK.Enabled = true;
                }
                appManager.Map.MapFrame.Invalidate();
            }
            else
                return;
        }

        private void mnuReSelectGages_Click(object sender, EventArgs e)
        {
            // set map mode to select
            MapMode = (int)SelectMode.Select;
            mnuReSelectGages.Visible = false;
            mnuGetSeries.Visible = false;
            tabCtl.SelectedTab = tabMap;
            appManager.Map.ClearSelection();

            if (!splitContainerLegend.Panel2Collapsed)
                splitContainerLegend.Panel2Collapsed = true;

            //hide other tabs
            //remove tabs if user changed selection
            if (tabCtl.Contains(tabGages))
            {
                //dgvGages.Rows.Clear();
                tabCtl.TabPages.Remove(tabGages);
            }
            if (tabCtl.Contains(tabSeries))
                tabCtl.TabPages.Remove(tabSeries);

            if (tabCtl.Contains(tabRegional))
            {
                //dgvSelRegGages.Rows.Clear();
                //dgvModel.Rows.Clear();
                tabCtl.TabPages.Remove(tabRegional);
            }
            if (tabCtl.Contains(tabResults))
            {
                //dgvResults.Rows.Clear();
                tabCtl.TabPages.Remove(tabResults);
            }

            if (mnuCalc7Q10.Enabled) mnuCalc7Q10.Enabled = false;
            if (mnuEstimate.Enabled) mnuEstimate.Enabled = false;
            if (mnuGetSeries.Enabled) mnuGetSeries.Enabled = false;

            //clear drawing layer if exist
            ClearDrawingLayer();
        }

        private void uxMap_LayerAdded(object sender, LayerEventArgs e)
        {
            // can only select gages layers
            //SetSelectableLayer();
        }

    }
}
