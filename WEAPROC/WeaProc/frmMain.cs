#define debug
// Version 2.02.05
// based on V12
// Revision History GPF
// 10/07/20  -filters ISD with QA flag of 5
//           -units for COOP fixed, csv is in hundredth of in.
//           -fixed problem with GHCN not saving to wdm after estimation of missing
//           -ISD query for all variables not individual as in v1.01
// 10/12/20  -adding stochastic model fit for NLDAS 
//           -cache folder revised for log and download
// 10/14/20  -export stochastic model parameters to sqlite db
// 10/22/20  -export WDM datasets to sqlite db and csv
//           -disable writing hly raw file
//           -Added stochastic weather generator (only for PREC and PRCP for now)
//           -writes generated dataset to csv, will do writing to sqlite db
//           -fix issues with sta id, loc, stname and description, added setting STAID
// 10/30/20  -export to sqlite only using filtered records, those not in db already
// 11/13/20  -added GLDAS datasource 
// 11/23/20  -added TRMM datasource, turned off stochastic model fit; 
// 12/03/20  -automatic timezone adjustment for NLDAS and GLDAS, uses WorldTZ shape
//  1/13/21  -fixed error in CLOU, see clsNLDAS
//           -fixed error in WIND, download reset to mi/hr instead of m/s
//           -need to check writing of LSPC, EFDC and WASP
//           -need to fix GLDAS similar to NLDAS
//  9/03/23  -added Climate Scenario, CMIP6
//  7/22/24  -added EDDE dataset (EPA dynamically downscaled ensemble)
// 10/31/24  -added EDDE longwave, sensible and latent heat

using atcWDM;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Projections;
using DotSpatial.Symbology;
using DotSpatial.Topology;
using LSPCWeaComp;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using WeaEFDC;
using WeaGen;
using WeaSWAT;
using WeaWASP;
using atcUtility;
using MapWinUtility;
using atcData;
using WeaScenario;

namespace NCEIData
{
    public partial class frmMain : Form
    {
        public string appDir, dataDir, gisDir, cacheDir, logPath, modelDir;
        public FileStream fslog;
        public StreamWriter wrlog;
        private List<string> lstStates, lstRegion, lstCountry;
        private int ZoomSelectedIndex = 0;
        private string selectedArea;

        public enum ZoomRegion { States, HUC2 };
        public int optRegion = (int)ZoomRegion.States;

        //wdm
        private int numdset = 0;
        public string WdmFile = string.Empty;
        public string AnnWdmFile = string.Empty;
        public string cacheWDM = string.Empty;
        public bool isOpenWDM = false;
        public string AnnualWdmFile = string.Empty;
        public bool isOpenAnnualWDM = false;
        private CMIP6Series CMIPparams;

        public WeaSeries weatherTS;

        //dictionary of datasets in wdm, station, list of constituen
        public SortedDictionary<string, MetGages> dictWdm =
                 new SortedDictionary<string, MetGages>();
        public SortedDictionary<string, List<string>> dictSta =
                 new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, List<string>> dictSiteVars =
                 new SortedDictionary<string, List<string>>();
        //for selected stations
        public SortedDictionary<string, MetGages> dictSelSites =
                 new SortedDictionary<string, MetGages>();
        
        //dictionary of gages/grids
        public SortedDictionary<string, string> dictGages =
                 new SortedDictionary<string, string>();

        public SortedDictionary<string, List<string>> dictCMIP6Files =
                 new SortedDictionary<string, List<string>>();

        public SortedDictionary<string, List<float>> dictEDDEgrids =
                 new SortedDictionary<string, List<float>>();

        public SortedDictionary<string, List<float>> dictPRISMgrids =
                 new SortedDictionary<string, List<float>>();

        //dictionary GCM climate models
        public SortedDictionary<string, List<string>> dictGCM =
                 new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, string> dictSSP =
                 new SortedDictionary<string, string>();

        //other globals
        public enum MetDataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, PRISM, CMIP6, EDDE };
        public int optDataSource = (int)MetDataSource.ISD;

        public int numStations, numSelState;
        public bool isStateloaded = false;
        public PolygonLayer selStLayer;
        public PointLayer selMetLayer, selGageLayer;
        public string SelectedGage;
        private List<IFeature> lstGage;
        public DataTable dtSites = new DataTable();
        public List<string> DownLoadedSites = new List<string>();
        public List<string> lstStaDownloaded = new List<string>();

        //sites selected from map, station and station_id list
        public List<string> lstSta = new List<string>();
        public List<string> lstStaName = new List<string>();
        private string Beg_Date; string End_Date;
        public DateTime BegDateTime, EndDateTime;

        //SelectedGridSites is list of sites selected from datagrid
        List<string> SelectedGridSites = new List<string>();

        private Dictionary<string, bool> dictOptVars;
        //lstSelectedVars set in frmDownload/frmDownloadCMIP6
        public List<string> lstSelectedVars = new List<string>();
        public string scenario, scenarioPath;

        //private bool SaveMultFiles = false;
        private int curYear;
        public int PercentMiss, MinYears, UTCshift;
        public List<string> SeriesIgnored = new List<string>();
        private string latlonFile = string.Empty;

        //map and background layers
        private const string STATES = "States.shp";
        private const string HUC8SM = "HUC8sm.shp";
        private const string WORLD = "World.shp";
        public const string WORLDTZ = "WorldTZ.shp";
        public const string GHCN = "GHCN_Gages.shp";
        public const string COOP = "COOP_Gages.shp";
        public const string ISD = "ISD_Gages.shp";
        public string GHCNgages, COOPgages;
        public DrawRectangle aoi;
        private string mapMode = string.Empty;
        public string stateshp, huc8shp, worldshp, tzshp;
        public PolygonLayer st, huc, wrld, tzone;
        public FeatureSet timezone;
        public Shapefile tzshape;
        private Extent USextent;
        private bool WithinUS = true;
        public BoundingBox GridBounds;

        //selection from map
        private FeatureSet mapPoint;
        private MapPointLayer mapPointLayer;
        public enum SelectMode { Select, DrawPoint, None };
        private int MapMode = (int)SelectMode.None;
        public bool PointSelected = false;
        public List<CPoint> lstOfPoints;
        public Dictionary<string, CPoint> dictPoints;

        //model options
        public enum Model { LSPC, WASP, SWMM, SWAT, EFDC };
        public int optModel = (int)Model.LSPC;

        //tooltip
        private ToolTip tlTip = new ToolTip();
        private bool showForm = true;

        //sqlite databases
        public string ModelSDB, SdbFile, SQLdbFile;
        public string defaultSDB, defaultMdlSDB;

        //export options
        private enum Export { SQL, CSV };
        private int OptionExport = (int)Export.SQL;

        //SARA utility
        private string TSutilExe = "TimeseriesUtility.exe";
        private string WDMUtilExe = "WDMUtility.exe";

        private string crlf = Environment.NewLine;

        [Export("Shell", typeof(ContainerControl))]
        private static ContainerControl Shell;
        public frmMain()
        {
            InitializeComponent();

            if (DesignMode) return;
            Shell = this;

            frmAbout fAbout = new frmAbout();

            fAbout = new frmAbout();
            fAbout.btnOK.Visible = false;
            fAbout.lbltext.Text = "Initializing toolkit ...";
            fAbout.Show();

            Application.DoEvents();

            try
            {
                appManager.LoadExtensions();
                appManager.HeaderControl.Remove("kHome");
                appManager.HeaderControl.Remove("kExtensions");
                appManager.HeaderControl.Remove("None");
                appManager.Map.Projection = KnownCoordinateSystems.Projected.World.WebMercator;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }

            CreateDefaultFoldersAndFiles();
            LoadProjectMap();
            LoadBaseLayers();

            //remove other tabs except tabMap
            InitializeControls(true);
            fAbout.Close();
            fAbout.Dispose();
        }

        private void InitializeControls(Boolean setTab)
        {
            mnuSearchDatasetCMIP6.Enabled = true;
            mnuSearchDatasetCOOP.Enabled = true;
            mnuSearchDatasetEDDE.Enabled = true;
            mnuSearchDatasetGHCN.Enabled = true;
            mnuSearchDataSetGLDAS.Enabled = true;
            mnuSearchDatasetISD.Enabled = true;
            mnuSearchDatasetNLDAS.Enabled = true;
            mnuSearchDatasetTRMM.Enabled = true;

            optSta.Checked = true;

            curYear = DateTime.Now.Year;
            numBegYr.Value = curYear - 10;
            numEndYr.Value = curYear;

            //Station,Station_ID,BEG_DATE,END_DATE,LATITUDE,LONGITUDE,ELEVATION
            //STATE,BegYear,EndYear 
            //Table of sites
            dtSites.Columns.Add("Station", typeof(string));
            dtSites.Columns.Add("Station_ID", typeof(string));
            dtSites.Columns.Add("BEG_DATE", typeof(DateTime));
            dtSites.Columns.Add("END_DATE", typeof(DateTime));
            dtSites.Columns.Add("LATITUDE", typeof(string));
            dtSites.Columns.Add("LONGITUDE", typeof(string));
            dtSites.Columns.Add("ELEVATION", typeof(string));
            dtSites.Columns.Add("STATE", typeof(string));
            dtSites.Columns.Add("ZONE", typeof(string));
            //dtSites.Columns.Add("BegYear", typeof(string));
            //dtSites.Columns.Add("EndYear", typeof(string));

            if (setTab)
            {
                if (tabCtrlMain.Contains(tabPageSta))
                    tabCtrlMain.TabPages.Remove(tabPageSta);
            }
            else
            {
                if (!tabCtrlMain.Contains(tabPageSta))
                    tabCtrlMain.TabPages.Add(tabPageSta);
            }

            splitMap.Panel1Collapsed = true;
        }
        public void SetTabControls(Boolean setTab)
        {
            if (!tabCtrlMain.Contains(tabPageSta))
                tabCtrlMain.TabPages.Add(tabPageSta);
        }
        private void LoadProjectMap()
        {
            try
            {
                string basemap = Path.Combine(appDir, "basemap.txt");
                if (!File.Exists(basemap))
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
            }
        }
        private int LoadBaseLayers()
        {
            WriteLogFile("Entering LoadBaseLayer ...");
            stateshp = Path.Combine(gisDir, STATES);
            huc8shp = Path.Combine(gisDir, HUC8SM);
            worldshp = Path.Combine(gisDir, WORLD);
            tzshp= Path.Combine(gisDir, WORLDTZ);

            try
            {
                FeatureSet fs0 = (FeatureSet)FeatureSet.Open(stateshp);
                //Debug.WriteLine("States Projection =" + fs0.ProjectionString);
                ProjectionInfo stp = fs0.Projection;
                //Debug.WriteLine("States Projection Proj4 =" + stp.ToProj4String());
                //Debug.WriteLine("States Projection ESRI =" + stp.ToEsriString());

                fs0.Reproject(appManager.Map.Projection);
                //Debug.WriteLine("After STATES re-Projection");
                //Debug.WriteLine("After Projection =" + fs0.ProjectionString);
                stp = fs0.Projection;
                //Debug.WriteLine("After Projection Proj4 =" + stp.ToProj4String());
                //Debug.WriteLine("After Projection ESRI =" + stp.ToEsriString());

                st = (PolygonLayer)appManager.Map.Layers.Add(fs0);
                PolygonSymbolizer stsym = new PolygonSymbolizer(Color.FromArgb(10, Color.White), Color.Red);
                PolygonSymbolizer stselsym = new PolygonSymbolizer(Color.FromArgb(10, Color.Blue), Color.Aqua);
                st.Symbolizer = stsym;
                st.SelectionSymbolizer = stselsym;
                st.DataSet.Name = "State";
                st.IsSelected = false;
                st.SelectionEnabled = false;

                //USextent = st.Extent;
                //Debug.WriteLine("US extent {0},{1}", USextent.Center.X.ToString(), USextent.Center.Y.ToString());

                FeatureSet fs1 = (FeatureSet)FeatureSet.Open(huc8shp);
                Debug.WriteLine("HUC8SHP Projection =" + fs1.ProjectionString);
                fs1.Reproject(appManager.Map.Projection);
                huc = (PolygonLayer)appManager.Map.Layers.Add(fs1);
                PolygonSymbolizer hucsym = new PolygonSymbolizer(Color.FromArgb(10, Color.White), Color.Gray);
                PolygonSymbolizer hucselsym = new PolygonSymbolizer(Color.FromArgb(10, Color.Blue), Color.Aqua);
                huc.Symbolizer = hucsym;
                huc.SelectionSymbolizer = hucselsym;
                huc.DataSet.Name = "HUC8";
                huc.IsSelected = false;
                huc.SelectionEnabled = false;

                FeatureSet fs2 = (FeatureSet)FeatureSet.Open(worldshp);
                Debug.WriteLine("World Projection =" + fs2.ProjectionString);
                fs2.Reproject(appManager.Map.Projection);
                wrld = (PolygonLayer)appManager.Map.Layers.Add(fs2);
                PolygonSymbolizer wrldsym = new PolygonSymbolizer(Color.FromArgb(10, Color.White), Color.LightGray);
                PolygonSymbolizer wrldselsym = new PolygonSymbolizer(Color.FromArgb(10, Color.Blue), Color.Aqua);
                wrld.Symbolizer = wrldsym;
                wrld.SelectionSymbolizer = wrldselsym;
                wrld.DataSet.Name = "Countries";
                wrld.IsSelected = false;
                wrld.SelectionEnabled = false;
                //MapLineLayer hydro = (MapLineLayer)appManager.Map.Layers.Add(fs1);
                ////LineSymbolizer hydsym = new LineSymbolizer(Color.LightBlue, 1);
                ////LineSymbolizer hydsym = new LineSymbolizer(Color.FromArgb(10, Color.Blue), 5);
                //LineSymbolizer hydselsym = new LineSymbolizer(Color.FromArgb(10, Color.Red), 5);
                //hydro.Symbolizer = hydsym;
                //hydro.SelectionSymbolizer = hydselsym;
                //hydro.DataSet.Name = "NHD";
                //hydro.IsSelected = false;
                timezone = (FeatureSet)FeatureSet.Open(tzshp);
                Debug.WriteLine("TimeZone Projection =" + timezone.ProjectionString);
                //timezone.Reproject(appManager.Map.Projection);

                appManager.Map.Refresh();

                //set the layer
                selStLayer = st;
                isStateloaded = true;
                st.Selection.Clear();
            }
            catch (Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Cannot find GIS base layers ...");
                msg.Append("\r\nFeature dataset State \r\n");
                msg.Append("should be in the \\gis folder of the toolkit.\r\n\r\n");
                msg.Append(ex.Message);

                MessageBox.Show(msg.ToString());
                isStateloaded = false;
                return (-1);
                //Application.Exit();
            }
            return (1);
        }
        private void CreateDefaultFoldersAndFiles()
        {
            appDir = Application.StartupPath;
            string datapath = Path.Combine(appDir, "data");
            defaultSDB = Path.Combine(appDir, "WeaWDM.sqlite");
            defaultMdlSDB = Path.Combine(appDir, "WeaModel.sqlite");

            string cachepath = Path.Combine(appDir, "cache");
            gisDir = Path.Combine(appDir, "gis");
            cacheWDM = Path.Combine(cachepath, "tmpWDM.wdm");
            SetupLogFile(cachepath);
            WriteLogFile("Executing CreateDataDirectory()");

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(datapath);
                dataDir = datapath;
                di = Directory.CreateDirectory(cachepath);
                cacheDir = cachepath;
                modelDir = Path.Combine(dataDir, "model");
                if (!Directory.Exists(modelDir))
                    Directory.CreateDirectory(modelDir);

                if (!Directory.Exists(Path.Combine(cachepath, "GHCN")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "GHCN"));
                if (!Directory.Exists(Path.Combine(cachepath, "ISD")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "ISD"));
                if (!Directory.Exists(Path.Combine(cachepath, "COOP")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "COOP"));
                if (!Directory.Exists(Path.Combine(cachepath, "NLDAS")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "NLDAS"));
                if (!Directory.Exists(Path.Combine(cachepath, "GLDAS")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "GLDAS"));
                if (!Directory.Exists(Path.Combine(cachepath, "TRMM")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "TRMM"));
                if (!Directory.Exists(Path.Combine(cachepath, "CMIP6")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "CMIP6"));
                if (!Directory.Exists(Path.Combine(cachepath, "GIS")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "GIS"));
                if (!Directory.Exists(Path.Combine(cachepath, "EDDE")))
                    Directory.CreateDirectory(Path.Combine(cachepath, "EDDE"));
            }
            catch (Exception e)
            {
                WriteLogFile("Cannot create default folders!");
            }
            finally { }
        }
        private void SetupLogFile(string spath)
        {
            logPath = Path.Combine(spath, "log");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            double dt = DateTime.Now.ToOADate();
            string logFile = Path.Combine(logPath, "WeatherProcessor_"+dt.ToString()+".log");
            if (File.Exists(logFile)) File.Delete(logFile);
            fslog = new FileStream(logFile, FileMode.Create);
            wrlog = new StreamWriter(fslog);
        }

        private void mnuDownload_Click(object sender, EventArgs e)
        {
            //show form for download, depends on datasource
            dictOptVars = new Dictionary<string, bool>();
            
            if (optDataSource==(int)MetDataSource.CMIP6)
            {
                //CMIP6 climate scenario data
                frmDownloadCMIP fGetCMIPData = new frmDownloadCMIP(this, GridBounds);
                if (fGetCMIPData.ShowDialog() == DialogResult.OK)
                {
                    if (!fGetCMIPData.ValidFormEntry()) return;
                    //get the parameters of the climate scenario
                    CMIPparams = fGetCMIPData.ScenarioParms();
                    fGetCMIPData.Dispose();
                }
                else
                {
                    fGetCMIPData.Dispose();
                    return;
                }
            }

            else if ((optDataSource == (int)MetDataSource.EDDE))
            {
                //EDDE downscaled data
                frmDownloadEDDE fGetEDDEData = new frmDownloadEDDE(this, GridBounds);
                if (fGetEDDEData.ShowDialog() == DialogResult.OK)
                {
                    if (!fGetEDDEData.ValidFormEntry()) return;
                    //get the parameters of the climate scenario
                    CMIPparams = fGetEDDEData.ScenarioParms();
                    fGetEDDEData.Dispose();
                }
                else
                {
                    fGetEDDEData.Dispose();
                    return;
                }
            }

            else
            {
                // all other datasources
                frmDownload fGetData = new frmDownload(this);
                if (fGetData.ShowDialog() == DialogResult.OK)
                {
                    if (!fGetData.ValidFormEntry()) return;

                    switch (optDataSource)
                    {
                        case (int)MetDataSource.ISD:
                            Beg_Date = fGetData.BeginDate();
                            End_Date = fGetData.EndingDate();
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            dictOptVars = fGetData.OptionVars();
                            break;

                        case (int)MetDataSource.GHCN:
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            dictOptVars = fGetData.OptionVars();
                            break;

                        case (int)MetDataSource.HRAIN:
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            break;

                        case (int)MetDataSource.NLDAS:
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            dictOptVars = fGetData.OptionVars();
                            UTCshift = fGetData.TimeZoneShift();
                            break;

                        case (int)MetDataSource.GLDAS:
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            dictOptVars = fGetData.OptionVars();
                            UTCshift = fGetData.TimeZoneShift();
                            break;

                        case (int)MetDataSource.TRMM:
                            BegDateTime = fGetData.BeginDateTime();
                            EndDateTime = fGetData.EndingDateTime();
                            dictOptVars = fGetData.OptionVars();
                            UTCshift = fGetData.TimeZoneShift();
                            break;
                    }
                    fGetData.Dispose();
                }
                else
                {
                    fGetData.Dispose();
                    return;
                }
            }

            switch (optDataSource)
            {
                //Integrated Surface Data Dataset
                case (int)MetDataSource.ISD:
                    clsISD isd = new clsISD(this, Beg_Date, End_Date);
                    isd.OptionVars = dictOptVars;
                    //reads raw data file 
                    isd.ProcessISDdataRev();
                    dictOptVars = null;
                    isd = null;
                    break;

                //Global Hourly Climatological Network dataset
                case (int)MetDataSource.GHCN:
                    clsGHCN ghcn = new clsGHCN(this, BegDateTime, EndDateTime);
                    ghcn.OptionVars = dictOptVars;
                    if (ghcn.ProcessGHCNdata() > 0)
                    {
                        if (ghcn.ProcessDailyData())
                            ghcn.ShowDataTable(lstSelectedVars);
                    }
                    ghcn = null;
                    break;

                //hourly precipitation
                case (int)MetDataSource.HRAIN:
                    clsHRAIN hlyrain = new clsHRAIN(this, BegDateTime, EndDateTime);
                    //download rainfiles
                    if (hlyrain.ProcessHRAINdata())
                    {
                        if (hlyrain.ProcessHourlyRain())
                            hlyrain.ShowDataTable(lstSelectedVars);
                    }
                    hlyrain = null;
                    break;

                //NLDAS Data Dataset
                case (int)MetDataSource.NLDAS:
                    clsNLDAS nldas = new clsNLDAS(this, BegDateTime, EndDateTime, UTCshift);
                    nldas.OptionVars = dictOptVars;
                    //reads raw data file 
                    nldas.ProcessNLDASdata();
                    dictOptVars = null;
                    nldas = null;
                    break;

                //GLDAS Data Dataset
                case (int)MetDataSource.GLDAS:
                    clsGLDAS gldas = new clsGLDAS(this, BegDateTime, EndDateTime, UTCshift);
                    gldas.OptionVars = dictOptVars;
                    //reads raw data file 
                    gldas.ProcessGLDASdata();
                    dictOptVars = null;
                    gldas = null;
                    break;

                //TRMM Data Dataset
                case (int)MetDataSource.TRMM:
                    clsTRMM trmm = new clsTRMM(this, BegDateTime, EndDateTime, UTCshift);
                    trmm.OptionVars = dictOptVars;
                    //reads raw data file 
                    trmm.ProcessTRMMdata();
                    dictOptVars = null;
                    trmm = null;
                    break;

                case (int)MetDataSource.CMIP6:
                    //clsCMIP6 processes selected grids for all selected variables and
                    //specified scenario and pathway
                    clsCMIP6 cmip6 = new clsCMIP6(this, CMIPparams);
                    if (!cmip6.IsValidScenarioPathway()) return;
                    cmip6.ProcessFilesToDownload();
                    break;
                
                case (int)MetDataSource.EDDE:
                    //clsEDDE processes selected grids for all selected variables and
                    //specified scenario and pathway
                    clsEDDE edde = new clsEDDE(this, CMIPparams);
                    if (!edde.IsValidScenarioPathway()) return;
                    edde.ProcessFilesToDownload();
                    break;
            }
            appManager.UpdateProgress("Ready ..");
        }

        #region "MapEvents
        private void appMap_SelectionChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Event triggered: appMap_SelectionChanged");
            int selgage = 0;

            if (optDataSource == (int)MetDataSource.CMIP6)
                selgage = GetGridFromMap();
            else if (optDataSource == (int)MetDataSource.EDDE)
                selgage = GetEDDEGridFromMap();
            else
                selgage = GetGageFromMap();

            if (selgage > 0)
            {
                mnuDownload.Visible = true;
                mnuDownload.Enabled = true;
            }
            else
            {
                mnuDownload.Visible = true;
                mnuDownload.Enabled = false;
            }
        }
        private void appMap_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Event triggered: appMap_MouseUp: selected gages=" + lstSta.Count.ToString());
        }
        /// <summary>
        /// GetGageFromMap
        /// Executed when map selection change
        /// </summary>
        /// <returns></returns>
        
        public int GetGageFromMap()
        {
            Cursor.Current = Cursors.WaitCursor;
            int numGage = 0;

            selGageLayer = null;
            foreach (var fs in appManager.Map.Layers)
            {
                if ((string)fs.DataSet.Name == "Stations")
                {
                    selGageLayer = (PointLayer)fs;
                    selGageLayer.IsSelected = true;
                    break;
                }
            }

            if (selGageLayer == null) return (-1);

            lstGage = new List<IFeature>();
            lstGage = selGageLayer.Selection.ToFeatureList();
            numGage = lstGage.Count;

            Debug.WriteLine("In getgagemap: numgages = " + numGage.ToString());

            //dictSelSites is dict of selected MetGages
            if (lstSta.Count > 0) lstSta.Clear();
            if (dictSta.Count > 0) dictSta.Clear();
            if (dictSelSites.Count > 0) dictSelSites.Clear();

            //lstGage is list of seleted grid/stations
            float north = -60, south=90, west =180, east =-180;
            
            try
            {
                foreach (var item in lstGage)
                {
                    string st = string.Empty;
                    string stnam = string.Empty;
                    DataRow dr = item.DataRow;
                    st = dr["Station_ID"].ToString();
                    stnam = dr["Station"].ToString();
                    
                    //for bounding box
                    north = Math.Max(north, Convert.ToSingle(dr["Lat"]));
                    south = Math.Min(south, Convert.ToSingle(dr["Lat"]));
                    east = Math.Max(east, Convert.ToSingle(dr["Lon"]));
                    west = Math.Min(west, Convert.ToSingle(dr["Lon"]));
                    
                    //for dictSta, duplicate 
                    List<string> attrib = new List<string>();
                    attrib.Add(dr["Station"].ToString()); //0-station name
                    attrib.Add(dr["Elev"].ToString());    //1
                    attrib.Add(dr["Lat"].ToString());     //2
                    attrib.Add(dr["Lon"].ToString());     //3

                    // for dictWDM
                    MetGages gage = new MetGages();
                    gage.Station = dr["Station"].ToString();
                    gage.Station_ID = dr["Station_ID"].ToString();
                    gage.ELEVATION = dr["Elev"].ToString();
                    gage.LATITUDE = dr["Lat"].ToString();
                    gage.LONGITUDE = dr["Lon"].ToString();

                    switch (optDataSource)
                    {
                        case (int)MetDataSource.ISD:
                            attrib.Add(dr["Zone"].ToString());
                            gage.TYPE = "ISD";
                            //gage.TZONE = "0";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                        case (int)MetDataSource.GHCN:
                            gage.TYPE = "GHCN";
                            gage.TZONE = "0";
                            break;
                        case (int)MetDataSource.HRAIN:
                            gage.TYPE = "COOP";
                            gage.TZONE = "0";
                            break;
                        case (int)MetDataSource.NLDAS:
                            attrib.Add(dr["Zone"].ToString());
                            gage.TYPE = "NLDAS";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                        case (int)MetDataSource.GLDAS:
                            attrib.Add(dr["Zone"].ToString());
                            gage.TYPE = "GLDAS";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                        case (int)MetDataSource.TRMM:
                            attrib.Add(dr["Zone"].ToString());
                            gage.TYPE = "TRMM";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                        case (int)MetDataSource.CMIP6:
                            attrib.Add(dr["Zone"].ToString()); //4
                            gage.TYPE = "CMIP6";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                        case (int)MetDataSource.EDDE:
                            attrib.Add(dr["Zone"].ToString()); //4
                            gage.TYPE = "EDDE";
                            gage.TZONE = dr["Zone"].ToString();
                            break;
                    }

                    lstSta.Add(st);
                    lstStaName.Add(stnam);

                    if (!dictSta.ContainsKey(st))
                        dictSta.Add(st, attrib);

                    if (!dictWdm.ContainsKey(st))
                        dictWdm.Add(st, gage);

                    if (!dictSelSites.ContainsKey(st))
                        dictSelSites.Add(st, gage);

                    gage = null;
                    attrib = null;
                    st = null;
                }

                GridBounds = new BoundingBox(north, south, west, east);
#if debug
                Debug.WriteLine("north=" + north.ToString("F3"));
                Debug.WriteLine("south=" + south.ToString("F3"));
                Debug.WriteLine("east=" + east.ToString("F3"));
                Debug.WriteLine("west=" + west.ToString("F3"));
#endif
            }
            catch (Exception ex)
            {
                ShowError("Error in getting station attributes!", ex);
                return 0;
            }

            Debug.WriteLine("Executing GetGagesFromMap...");
            SelectRowsFromTable();

            Cursor.Current = Cursors.Default;
            return lstGage.Count();
        }

        public int GetGridFromMap()
        {
            Debug.WriteLine("Entering GetGridFromMap ...");
            
            //for CMIP6 grid, always use the grids within bounding box
            Cursor.Current = Cursors.WaitCursor;
            int numGage = 0;

            selGageLayer = null;
            foreach (var fs in appManager.Map.Layers)
            {
                if ((string)fs.DataSet.Name == "Stations")
                {
                    selGageLayer = (PointLayer)fs;
                    selGageLayer.IsSelected = true;
                    break;
                }
            }

            if (selGageLayer == null) return (-1);

            lstGage = new List<IFeature>();
            lstGage = selGageLayer.Selection.ToFeatureList();
            numGage = lstGage.Count;

            //dictSelSites is dict of selected MetGages
            if (lstSta.Count > 0) lstSta.Clear();
            if (dictSta.Count > 0) dictSta.Clear();
            if (dictSelSites.Count > 0) dictSelSites.Clear();

            //lstGage is list of selected grid/stations
            float north = -60, south = 90, west = 180, east = -180;
            try
            {
                //get the grids in the bounding box
                foreach (var item in lstGage)
                {
                    DataRow dr = item.DataRow;
                    //for bounding box
                    north = Math.Max(north, Convert.ToSingle(dr["Lat"]));
                    south = Math.Min(south, Convert.ToSingle(dr["Lat"]));
                    east = Math.Max(east, Convert.ToSingle(dr["Lon"]));
                    west = Math.Min(west, Convert.ToSingle(dr["Lon"]));
                }

                GridBounds = new BoundingBox(north, south, west, east);
#if debug
                //Debug.WriteLine("north=" + north.ToString("F3"));
                //Debug.WriteLine("south=" + south.ToString("F3"));
                //Debug.WriteLine("east=" + east.ToString("F3"));
                //Debug.WriteLine("west=" + west.ToString("F3"));
#endif
                float elev = 2.0F;
                for (float lat = south; lat <= north; lat=lat+0.25F)
                {
                    for (float lon = west; lon <= east; lon = lon + 0.25F)
                    {
                        string st = GetGridIDFromLatLon(lon, lat);
                        string stnam = st;

                        //for dictSta, duplicate 
                        List<string> attrib = new List<string>();
                        attrib.Add(st.ToString());      //0-station name
                        attrib.Add(elev.ToString());    //1
                        attrib.Add(lat.ToString());     //2
                        attrib.Add(lon.ToString());     //3
                        attrib.Add("0");                //4
                        attrib.Add(string.Empty);       //5 scenario
                        attrib.Add(string.Empty);       //6 pathway
                        attrib.Add(string.Empty);       //7 scenario-pathway

                        // for dictWDM
                        MetGages gage = new MetGages();
                        gage.Station = st.ToString();
                        gage.Station_ID = st.ToString();
                        gage.ELEVATION = elev.ToString();
                        gage.LATITUDE = lat.ToString();
                        gage.LONGITUDE = lon.ToString();
                        gage.TYPE = "CMIP6";
                        gage.TZONE = "0";

                        lstSta.Add(st);
                        lstStaName.Add(stnam);

                        if (!dictSta.ContainsKey(st))
                            dictSta.Add(st, attrib);

                        if (!dictWdm.ContainsKey(st))
                            dictWdm.Add(st, gage);

                        if (!dictSelSites.ContainsKey(st))
                            dictSelSites.Add(st, gage);

                        gage = null;
                        attrib = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error in getting station attributes!", ex);
                return 0;
            }

            Debug.WriteLine("Executing GetGagesFromMap...");
            SelectRowsFromTable();

            Cursor.Current = Cursors.Default;
            return lstSta.Count();
        }

        public int GetEDDEGridFromMap()
        {
            WriteLogFile("Entering GetEDDEGridFromMap ...");
            Debug.WriteLine("Entering GetEDDEGridFromMap ...");

            //for EDDE grid
            Cursor.Current = Cursors.WaitCursor;
            int numGage = 0;

            selGageLayer = null;
            foreach (var fs in appManager.Map.Layers)
            {
                if ((string)fs.DataSet.Name == "Stations")
                {
                    selGageLayer = (PointLayer)fs;
                    selGageLayer.IsSelected = true;
                    break;
                }
            }

            if (selGageLayer == null) return (-1);

            lstGage = new List<IFeature>();
            lstGage = selGageLayer.Selection.ToFeatureList();
            numGage = lstGage.Count;

            //dictSelSites is dict of selected MetGages
            if (lstSta.Count > 0) lstSta.Clear();
            if (dictSta.Count > 0) dictSta.Clear();
            if (dictSelSites.Count > 0) dictSelSites.Clear();

            //lstGage is list of selected grid/stations
            float north = -60, south = 90, west = 180, east = -180;
            try
            {
                //get the grids in the bounding box
                float elev = 2.0F;
                float lat, lon, zone;
                string stnam;

                foreach (var item in lstGage)
                {
                    DataRow dr = item.DataRow;
                    
                    stnam = dr["Station_ID"].ToString();
                    elev = Convert.ToSingle(dr["Elev"].ToString());
                    lat = Convert.ToSingle(dr["Lat"]);
                    lon = Convert.ToSingle(dr["Lon"]);
                    zone = Convert.ToSingle(dr["Zone"]);

                    //for bounding box
                    north = Math.Max(north, lat);
                    south = Math.Min(south, lat);
                    east = Math.Max(east, lon);
                    west = Math.Min(west, lon);

                    //for dictSta, duplicate 
                    List<string> attrib = new List<string>();
                    attrib.Add(stnam.ToString());   //0-station name
                    attrib.Add(elev.ToString());    //1
                    attrib.Add(lat.ToString());     //2
                    attrib.Add(lon.ToString());     //3
                    attrib.Add("0");                //4
                    attrib.Add(string.Empty);       //5 scenario
                    attrib.Add(string.Empty);       //6 pathway
                    attrib.Add(string.Empty);       //7 scenario-pathway

                    // for dictWDM
                    MetGages gage = new MetGages();
                    gage.Station = stnam.ToString();
                    gage.Station_ID = stnam.ToString();
                    gage.ELEVATION = elev.ToString();
                    gage.LATITUDE = lat.ToString();
                    gage.LONGITUDE = lon.ToString();
                    gage.TYPE = "EDDE";
                    gage.TZONE = zone.ToString();

                    lstSta.Add(stnam);
                    lstStaName.Add(stnam);

                    if (!dictSta.ContainsKey(stnam))
                        dictSta.Add(stnam, attrib);

                    if (!dictWdm.ContainsKey(stnam))
                        dictWdm.Add(stnam, gage);

                    if (!dictSelSites.ContainsKey(stnam))
                        dictSelSites.Add(stnam, gage);
                    
                    gage = null;
                    attrib = null;
                }

                GridBounds = new BoundingBox(north, south, west, east);
#if debug
                Debug.WriteLine("north=" + north.ToString("F4"));
                Debug.WriteLine("south=" + south.ToString("F4"));
                Debug.WriteLine("east=" + east.ToString("F4"));
                Debug.WriteLine("west=" + west.ToString("F4"));
#endif
            }
            catch (Exception ex)
            {
                ShowError("Error in getting EDDE grid attributes!", ex);
                return 0;
            }

            WriteLogFile("Executing GetEDDEGridFromMap...");
            Debug.WriteLine("Executing GetEDDEGridFromMap...");
            SelectRowsFromTable();

            Cursor.Current = Cursors.Default;
            return lstSta.Count();
        }

        private string GetGridIDFromLatLon(float xlon, float ylat)
        {
            string ID = string.Empty;

            float mnlon = -179.875F, mxlon = 179.875F;
            float mnlat = -59.875F, mxlat = 89.875F;

            //for x and y labels
            double xid = 1439.0 * (xlon - mnlon) / (mxlon - mnlon);
            double yid = 599.0 * (ylat - mnlat) / (mxlat - mnlat);

            if (xid < 1000)
                ID = "C0" + (Convert.ToInt32(xid)).ToString() + (Convert.ToInt32(yid)).ToString();
            else
                ID = "C" + (Convert.ToInt32(xid)).ToString() + (Convert.ToInt32(yid)).ToString();

            Debug.WriteLine("GridX = " + xlon.ToString() +", GridY = "+ylat.ToString() + ", ID=" + ID);
            return ID;
        }

        private void SelectRowsFromTable()
        {
            try
            {
                SelectedGridSites.Clear();
                foreach (string item in lstSta)
                {
                    foreach (DataGridViewRow dr in dgvSta.Rows)
                    {
                        string s = (string)dr.Cells["Station_ID"].Value;
                        if (item.Contains(s))
                        {
                            dr.Selected = true;
                            SelectedGridSites.Add(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        //not used
        public void ShowSitesTable()
        {
            SetTabControls(true);
            try
            {
                dgvSta.DataSource = null;
                dgvSta.DataSource = dtSites;
                dgvSta.ClearSelection();
                //dgvSta.Sort(dgvSta.Columns["BegYear"], ListSortDirection.Descending);
                //dgvSta.Columns["Latitude"].Visible = false;
                //dgvSta.Columns["Longitude"].Visible = false;
                //dgvSta.Columns["BegYear"].Visible = false;
                //dgvSta.Columns["EndYear"].Visible = false;
                //dgvSta.Rows[0].Selected = false;
                //Debug.WriteLine("Num selected rows = " + dgvSta.SelectedRows.Count.ToString());
            }
            catch (Exception ex)
            {
                ShowError("Error in ShowSitesTable!", ex);
            }
        }

        #region "FilterData"
        private void numBegYr_ValueChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(numBegYr.Value) > Convert.ToInt32(numEndYr.Value))
                numBegYr.Value = numEndYr.Value - 10;
        }

        private void numEndYr_ValueChanged(object sender, EventArgs e)
        {
            int maxYr = curYear;
            if (Convert.ToInt32(numEndYr.Value) < Convert.ToInt32(numBegYr.Value))
                numEndYr.Value = curYear;
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                int byear = Convert.ToInt32(numBegYr.Value);
                int eyear = Convert.ToInt32(numEndYr.Value);

                string filter = "BegYear>=" + byear + " && EndYear<=" + eyear;

                dgvSta.ClearSelection();
                foreach (DataGridViewRow dr in dgvSta.Rows)
                {
                    int byr = ((DateTime)dr.Cells["BEG_DATE"].Value).Year;
                    int eyr = ((DateTime)dr.Cells["END_DATE"].Value).Year;
                    if (byr >= byear && eyr <= eyear)
                        dr.Selected = true;
                }

                if (dgvSta.SelectedRows.Count > 0)
                    //map it
                    SitesGridSelectionChanged();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        private void SitesGridSelectionChanged()
        {
            Cursor.Current = Cursors.WaitCursor;
            Debug.WriteLine("Num Selected Sites in SitesGridSelectionChanged " +
                dgvSta.SelectedRows.Count.ToString());

            //List<string> SelectedGridSites = new List<string>();
            if (dgvSta.SelectedRows.Count > 0)
            {
                mnuDownload.Enabled = true;
                mnuDownload.Visible = true;
                SelectedGridSites = GetSelectedGridSites();
                MapSelectedSites(SelectedGridSites);
            }
            else
            {
                mnuDownload.Enabled = false;
                mnuDownload.Visible = true;
            }
            Cursor.Current = Cursors.Default;
        }
        private void MapSelectedSites(List<string> selSites)
        {
            foreach (var fs in appManager.Map.Layers)
            {
                if ((string)fs.DataSet.Name == "Stations")
                {
                    selMetLayer = (PointLayer)fs;
                    selMetLayer.IsSelected = true;
                    selMetLayer.SelectionEnabled = true;

                    List<string> stgage = new List<string>();
                    foreach (string sgage in selSites)
                    {
                        //Debug.WriteLine("selected gage = " + sgage);
                        string s = "'" + sgage + "'";
                        stgage.Add(s);
                    }
                    string st = string.Join(",", stgage);
                    Debug.WriteLine("selected = " + st);
                    selMetLayer.SelectByAttribute("[Station_ID] IN (" + st + ")");
                    stgage = null;
                }
            }
        }
        private void dgvSta_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SitesGridSelectionChanged();
        }
        private void dgvSta_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SitesGridSelectionChanged();
        }
        private void dgvSta_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("MouseUp event:" + sender.ToString());
            SitesGridSelectionChanged();
        }
        private void dgvSta_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Debug.WriteLine("ColumnHeaderMouse event:" + sender.ToString());
            Debug.WriteLine("In columnheader click: Map Site Count=" + lstSta.Count.ToString());
            Debug.WriteLine("In columnheader click: SelectedGrid Site Count=" + SelectedGridSites.Count.ToString());

            if (!(lstSta.Count > 0))
            {
                Debug.WriteLine("In columnheader click: Site Count=" + lstSta.Count.ToString());
                dgvSta.ClearSelection();
            }
            else
            {
                //sorting changes selection
                dgvSta.ClearSelection();
                SelectRowsFromTable();
            }
        }
        private List<string> GetSelectedGridSites()
        {
            List<string> lSites = new List<string>();
            foreach (DataGridViewRow row in dgvSta.SelectedRows)
            {
                string site = (string)row.Cells["Station_ID"].Value;
                lSites.Add(site);
            }
            foreach (string item in lSites)
                Debug.WriteLine("selected site = " + item);
            return (lSites.Count() > 0 ? lSites : null);
        }
        private void tabCtrlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabCtrlMain.SelectedTab.Equals(tabPageSta))
            {
                //when first shown, need to clear selection 'coz clearselection doesn't work
                if (lstSta.Count == 0)
                {
                    if (dgvSta.SelectedRows.Count > 0)
                        dgvSta.ClearSelection();
                }
                Debug.WriteLine("TabSelection: num selected rows =" + dgvSta.SelectedRows.Count.ToString());
            }
        }
        
        private void mnuAbout_Click(object sender, EventArgs e)
        {
            frmAbout fAbout = new frmAbout();
            fAbout = new frmAbout();
            fAbout.btnOK.Visible = true;
            fAbout.lbltext.Visible = false;
            fAbout.ShowDialog();
            fAbout.Dispose();
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void ClearMapSites()
        {
            try
            {
                List<IMapLayer> lstLayers = new List<IMapLayer>();
                foreach (var fs in appManager.Map.Layers)
                {
                    if ((string)fs.DataSet.Name == "Stations")
                    {
                        lstLayers.Add(fs);
                    }
                }
                foreach (var ilay in lstLayers)
                    appManager.Map.Layers.Remove(ilay);
            }
            catch (Exception ex)
            {
                ShowError("Error in removing site layer!", ex);
            }
        }
        private void DrawGagesLayer(string shpfile, List<MetGages> lstGages)
        {
            WriteLogFile("Drawing Gages Layer ..." + shpfile);

            string fname = Path.Combine(dataDir, shpfile);

            FeatureSet fs = new FeatureSet(FeatureType.Point);
            // Add Some Columns
            fs.DataTable.Columns.Add(new DataColumn("Station_ID", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Station", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("BegDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("Lat", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Lon", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Elev", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("State", typeof(string)));

            fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;
            //fs.Projection = KnownCoordinateSystems.Projected.World.WebMercator;

            double x, y;
            foreach (var site in lstGages)
            {
                x = Convert.ToDouble(site.LONGITUDE);
                y = Convert.ToDouble(site.LATITUDE);

                Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                IFeature curFeature = fs.AddFeature(p);
                curFeature.DataRow.BeginEdit();
                curFeature.DataRow["Station"] = site.Station;
                curFeature.DataRow["Station_ID"] = site.Station_ID;
                curFeature.DataRow["BegDate"] = site.BEG_DATE;
                curFeature.DataRow["EndDate"] = site.END_DATE;
                curFeature.DataRow["Lat"] = site.LATITUDE;
                curFeature.DataRow["Lon"] = site.LONGITUDE;
                curFeature.DataRow["Elev"] = site.ELEVATION;
                curFeature.DataRow["State"] = site.STATE;
                curFeature.DataRow.EndEdit();
            }
            //reproject to webmercator
            fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
            fs.SaveAs(fname, true);
            fs = null;
        }
        private void AddLayerToMap(string shpfile, string slegend)
        {
            FeatureSet fs = (FeatureSet)FeatureSet.Open(shpfile);

            MapPointLayer METsites = (MapPointLayer)appManager.Map.Layers.Add(fs);
            METsites.Symbolizer = new PointSymbolizer(Color.DarkOrange, DotSpatial.Symbology.PointShape.Ellipse, 8);
            METsites.SelectionSymbolizer = new PointSymbolizer(Color.Aqua, DotSpatial.Symbology.PointShape.Ellipse, 8);
            METsites.Symbolizer.SetOutline(Color.Black, 1);
            METsites.DataSet.Name = "Stations";
            METsites.SelectionSymbolizer.SetOutline(Color.Black, 1);
            METsites.LegendText = slegend;
            METsites.IsSelected = true;

            //generate label for facilities
            //string fontname = "Tahoma";
            //double fsize = 8.0;
            //Color fcolor = Color.Black;
            //appManager.Map.AddLabels(METsites, "[" + "WBAN" + "]", new Font("" + fontname + "", (float)fsize), fcolor);

            //selHUCLayer.IsSelected = false;

            //fMain.appManager.Map.ViewExtents = fs.Extent; //METsites.DataSet.Extent;
            //set extent to selected state
            //appManager.Map.ViewExtents = selExtent;
            appManager.Map.Refresh();
            //fMain.dtSites = fs.DataTable;
            fs = null;
        }

        public void SelectGages()
        {
            SearchGages gages = new SearchGages(this);
            int ngages = gages.GetSites();
            gages = null;
        }

        public void SearchByDatasetSource()
        {
            uxMap.FunctionMode = FunctionMode.Select;
            string gageCSV = string.Empty;
            string sfile = string.Empty;

            try
            {
                switch (optDataSource)
                {
                    case (int)MetDataSource.ISD:
                        gageCSV = "ISD_Stations.csv";
                        sfile = Path.Combine(Application.StartupPath, gageCSV);
                        appManager.UpdateProgress("Reading gage coverage " + sfile);
                        clsStations cISD = new clsStations(sfile);
                        dictGages = cISD.ReadISDStations();
                        cISD = null;
                        break;
                    case (int)MetDataSource.GHCN:
                        gageCSV = "GHCN_Stations.csv";
                        sfile = Path.Combine(Application.StartupPath, gageCSV);
                        appManager.UpdateProgress("Reading gage coverage " + sfile);
                        clsStations cGHCN = new clsStations(sfile);
                        dictGages = cGHCN.ReadGHCNStations();
                        cGHCN = null;
                        break;
                    case (int)MetDataSource.HRAIN:
                        gageCSV = "COOP_Stations.csv";
                        sfile = Path.Combine(Application.StartupPath, gageCSV);
                        appManager.UpdateProgress("Reading gage coverage " + sfile);
                        clsStations cCOOP = new clsStations(sfile);
                        dictGages = cCOOP.ReadCOOPStations();
                        cCOOP = null;
                        break;
                    case (int)MetDataSource.CMIP6:
                        if (dictCMIP6Files.Count <= 0)
                        {
                            string catfile = "CMIP6_Catalog.csv";
                            sfile = Path.Combine(Application.StartupPath, catfile);
                            appManager.UpdateProgress("Reading CMIP6 file catalog " + sfile);
                            ReadCMIPCatalog(sfile);
                        }

                        //read the file of descriptions of GCM models
                        if (dictGCM.Count <= 0)
                        {
                            string gcm = "GCMmodel.txt";
                            sfile = Path.Combine(Application.StartupPath, gcm);
                            appManager.UpdateProgress("Reading GCM model info file " + sfile);
                            ReadGCMInfo(sfile);
                        }
                        break;
                    case (int)MetDataSource.EDDE:
                        if (dictEDDEgrids.Count <= 0)
                        {
                            sfile = "EDDEgrid.csv";
                            sfile = Path.Combine(Application.StartupPath, sfile);
                            appManager.UpdateProgress("Reading EDDE grids " + sfile);
                            ReadEDDEgrids(sfile);
                        }
                        break;
                    case (int)MetDataSource.PRISM:
                        if (dictPRISMgrids.Count <= 0)
                        {
                            sfile = "PRISMgrid.csv";
                            sfile = Path.Combine(Application.StartupPath, sfile);
                            appManager.UpdateProgress("Reading PRISM grids " + sfile);
                            ReadPRISMgrids(sfile);
                        }
                        break;
                }
                //debug
                //foreach (KeyValuePair<string, string> kv in dictGages)
                //    Debug.WriteLine("{0},{1}", kv.Key, kv.Value);
                SearchDataset();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in getting gage coverage!" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        
        public void SearchDataset()
        {
            if (mnuDownload.Enabled) mnuDownload.Enabled = false;

            lstSta.Clear();
            lstStaName.Clear();
            dictSta.Clear();

            ClearMapSites();

            WriteStatus("Drag cursor to draw area of interest ...");
            //statuslbl.ForeColor = Color.Red;
            //statusStrip.Refresh();
            //appManager.UpdateProgress("Drag cursor to draw area of interest ...");
            if (!(aoi == null))
                aoi.Deactivate();

            mapMode = "Select";
            aoi = new DrawRectangle(this, appManager.Map);
            aoi.Activate();
        }

        private void ReadCMIPCatalog(string sfile)
        {
            //reads and build catalog of available CMIP6 files
            string[] lines = File.ReadAllLines(sfile);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] arr = lines[i].Split(',');
                int len = arr.Length;

                //file header catalog csv is
                //0-scenario, 1-pathway, 2-variant, 3-var, 4-year, 5-file
                //key is scenario_pathway_variable_year
                //value is List of variant, filename
                string skey = arr[0] + "_" + arr[1] + "_" +
                     arr[3] + "_" + arr[4];
                List<string> aList = new List<string>();
                aList.Add(arr[2].ToString());
                aList.Add(arr[5].ToString());
#if debug
         //Debug.WriteLine("{0},{1},{2}",skey, arr[2].ToString(),arr[5].ToString());
#endif
                if (!dictCMIP6Files.ContainsKey(skey))
                    dictCMIP6Files.Add(skey, aList);

                arr = null;
                aList = null;
            }
        }

        private void ReadGCMInfo(string sfile)
        {
            //reads and build catalog of GCM model info
            string[] lines = File.ReadAllLines(sfile);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] arr = lines[i].Split('\t');
                int len = arr.Length;

                //file header catalog csv is
                //0-model, 1-Institution, 2-reference
                //key is model
                //value is List of institution, reference
                string skey = arr[0].ToString();
                List<string> aList = new List<string>();
                aList.Add(arr[1].ToString());
                aList.Add(arr[2].ToString());
#if debug
                //Debug.WriteLine("{0},{1},{2}",skey, arr[1].ToString(),arr[2].ToString());
#endif
                if (!dictGCM.ContainsKey(skey))
                    dictGCM.Add(skey, aList);

                arr = null;
                aList = null;
            }
        }

        private void ReadEDDEgrids(string sfile)
        {
            //reads the EDDE grids, first line is header (grid, long, lat)
            string[] lines = File.ReadAllLines(sfile);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] arr = lines[i].Split(',');
                int len = arr.Length;

                //file header is
                //0-grid (Exxxyyy, 1-longitude, 2-latitude
                //key is grid, value is List of long - lat
                string skey = arr[0].ToString();
                List<float> aList = new List<float>();
                aList.Add(Convert.ToSingle(arr[1].Trim()));
                aList.Add(Convert.ToSingle(arr[2].Trim()));
#if debug
                //Debug.WriteLine("{0},{1},{2}",skey, arr[1].ToString(),arr[2].ToString());
#endif
                if (!dictEDDEgrids.ContainsKey(skey))
                    dictEDDEgrids.Add(skey, aList);

                arr = null;
                aList = null;
            }
        }

        private void ReadPRISMgrids(string sfile)
        {
            dictPRISMgrids.Clear();
            //reads the prism grid file, first line is header (grid, long, lat)
            string[] lines = File.ReadAllLines(sfile);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] arr = lines[i].Split(',');
                int len = arr.Length;

                //file header is
                //0-grid (xxxyyy, 1-longitude, 2-latitude
                //key is grid, value is List of long - lat
                string skey = arr[0].ToString();
                List<float> aList = new List<float>();
                aList.Add(Convert.ToSingle(arr[1].Trim()));
                aList.Add(Convert.ToSingle(arr[2].Trim()));
#if debug
                //Debug.WriteLine("{0},{1},{2}",skey, arr[1].ToString(),arr[2].ToString());
#endif
                if (!dictPRISMgrids.ContainsKey(skey))
                    dictPRISMgrids.Add(skey, aList);

                arr = null;
                aList = null;
            }
        }

        /// <summary>
        /// /Select WDM file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuWDM_Click(object sender, EventArgs e)
        {
            string ext = ".wdm";
            string filter = "WDM files (*.wdm)|*.wdm|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = false;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select or Create new WDM file...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sFile = openFD.FileName;
                else
                    sFile = string.Empty;
            }

            Cursor.Current = Cursors.WaitCursor;
            if (!string.IsNullOrEmpty(sFile))
            {
                appManager.UpdateProgress("Checking datasets in " + Path.GetFileName(sFile));

                if (!File.Exists(sFile))
                {
                    //create file if not exist
                    atcDataSourceWDM lWdmDS = new atcWDM.atcDataSourceWDM();
                    lWdmDS.Open(sFile);
                    WdmFile = sFile;
                    mnuSearchDataset.Enabled = true;
                    mnuTimeSeries.Enabled = true;
                    lWdmDS = null;

                    //create SQLite dbfile
                    string path = Path.GetDirectoryName(WdmFile);
                    sFile = Path.GetFileNameWithoutExtension(WdmFile);
                    SQLdbFile = Path.Combine(path, sFile + ".db");
                    //if(!File.Exists(SQLdbFile))
                    //    File.Copy(defaultSDB, SQLdbFile);
                    isOpenWDM = true;

                    //create annual wdmfile
                    AnnWdmFile = Path.Combine(Path.GetDirectoryName(WdmFile),
                        Path.GetFileNameWithoutExtension(WdmFile) + "_Annual.wdm");
                    //create file if not exist
                    if (!File.Exists(AnnWdmFile))
                    {
                        atcDataSourceWDM aWdm = new atcWDM.atcDataSourceWDM();
                        aWdm.Open(AnnWdmFile);
                        aWdm = null;
                    }
                }
                else
                {
                    //check if valid file
                    if (IsValidWDM(sFile))
                    {
                        string fname = Path.GetFileName(sFile);
                        this.Text = "Model WeatherData Processor: " + fname;
                        WdmFile = sFile;
                        mnuSearchDataset.Enabled = true;
                        mnuTimeSeries.Enabled = true;
                        mnuExport.Enabled = true;
                        this.Text = "Weather Processor: " + WdmFile;
                        isOpenWDM = true;

                        string path = Path.GetDirectoryName(WdmFile);
                        sFile = Path.GetFileNameWithoutExtension(WdmFile);
                        SQLdbFile = Path.Combine(path, sFile + ".db");

                        //create annual wdmfile
                        AnnWdmFile = Path.Combine(Path.GetDirectoryName(WdmFile),
                            Path.GetFileNameWithoutExtension(WdmFile) + "_Annual.wdm");
                        if (!File.Exists(AnnWdmFile))
                        {
                            atcDataSourceWDM aWdm = new atcWDM.atcDataSourceWDM();
                            aWdm.Open(AnnWdmFile);
                            aWdm = null;
                        }
                    }
                    else
                    {
                        mnuTimeSeries.Enabled = false;
                        mnuExport.Enabled = false;

                        string msg = sFile + " is not a valid wdm file!";
                        MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        isOpenWDM = false;
                        return;
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }
        private bool IsValidWDM(string sfile)
        {
            atcDataSourceWDM lWdmDS = new atcWDM.atcDataSourceWDM();
            try
            {
                lWdmDS.Open(sfile);
                int numds = lWdmDS.DataSets.Count;
                appManager.UpdateProgress(numds.ToString() + " datasets in " + Path.GetFileName(sfile));
                numdset = numds;
                lWdmDS = null;
                return true;
            }
            catch (Exception ex)
            {
                //WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
                //MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
                return false;
            }
        }
        private bool IsValidAnnualWDM(string sfile)
        {
            atcDataSourceWDM lWdmDS = new atcWDM.atcDataSourceWDM();
            int numYr = 0;
            try
            {
                lWdmDS.Open(sfile);
                int numds = lWdmDS.DataSets.Count;
                foreach (atcTimeseries lDataSet in lWdmDS.DataSets)
                {
                    string tunit = lDataSet.Attributes.GetValue("Time Unit").ToString().Trim();
                    if (tunit.Contains("Year"))
                        numYr++; 
                    lDataSet.Clear();
                }
                lWdmDS = null;

                if (numYr > 0)
                {
                    appManager.UpdateProgress(numYr.ToString() + " annual timeseries in " + Path.GetFileName(sfile));
                    numdset = numds;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                //WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
                //MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
                return false;
            }
        }
        private void mnuManual_Click(object sender, EventArgs e)
        {
            string spath = Application.StartupPath.ToString();
            string sfile = Path.Combine(spath, "WeatherProcessor.pdf");
            ShowManual(sfile);
        }
        private void ShowManual(string aManual)
        {
            try
            {
                System.Diagnostics.Process.Start(aManual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find " + aManual + "!");
            }
        }
        private void mnuCompute_Click(object sender, EventArgs e)
        {
            bool isWDM = true;
            if (string.IsNullOrEmpty(WdmFile))
                isWDM = BrowseForWDM();

            if (isWDM)
            {
                frmWeaComp fcom = new frmWeaComp(WdmFile);
                fcom.ShowDialog();
                fcom = null;
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (File.Exists(cacheWDM))
            //    File.Delete(cacheWDM);
            Application.Exit();
        }

        #region "WDM Exports"
        private void mnuExportLSPC_Click(object sender, EventArgs e)
        {
            ClearMapSites();
            optModel = (int)Model.LSPC;
            if (!ValidWDM()) return;

            WriteStatus("Reading time series attributes of WDMFile ...");
            frmLSPC fBasin = new frmLSPC(appManager.Map, WdmFile);
            WriteStatus("Ready ...");
            if (fBasin.ShowFormLSPC())
                fBasin.ShowDialog();
            fBasin.Dispose();
        }

        private void SelectPointFromMap()
        {
            ClearMapSites();
            // provide instructions
            //string lbl = "Left click on map to select point ...";
            //appManager.UpdateProgress(lbl);
            // clear drawing layer if present
            MapMode = (int)SelectMode.DrawPoint;
            uxMap.MouseClick += new System.Windows.Forms.MouseEventHandler(Map_OnMouseClick);

            // clear drawing layer if exist
            if (appManager.Map.MapFrame.DrawingLayers.Contains(mapPointLayer))
            {
                // Remove our drawing layer from the map.
                appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);

                // Request a redraw
                appManager.Map.MapFrame.Invalidate();
            }

            //set map for selection of point
            // Enable left click panning and mouse wheel zooming
            uxMap.FunctionMode = FunctionMode.Pan;
            uxMap.Cursor = Cursors.Cross;

            // The FeatureSet starts with no data; be sure to set it to the point featuretype
            mapPoint = new FeatureSet(FeatureType.Point);

            // The MapPointLayer controls the drawing of the marker features
            mapPointLayer = new MapPointLayer(mapPoint);

            // The Symbolizer controls what the points look like
            mapPointLayer.Symbolizer = new PointSymbolizer(Color.Blue, DotSpatial.Symbology.PointShape.Ellipse, 12);

            // A drawing layer draws on top of data layers, but is still georeferenced.
            uxMap.MapFrame.DrawingLayers.Add(mapPointLayer);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //position form on screen
            StartPosition = FormStartPosition.CenterScreen;

            mnuMain.Items.RemoveAt(0);
            foreach (System.Windows.Forms.Control ctl in spatialToolStripPanel1.Controls)
            {
                Debug.WriteLine("contorl=", ctl.Name);
            }

            //cboDataSet.SelectedIndex = optDataSource;
            //mnuSelectGages.Visible = false;
            mnuDownload.Visible = true;
            mnuDownload.Enabled = false;

            lstStates = new List<string>()
            { "US","AL","AR","AZ","CA","CO","CT","DC","DE","FL","GA",
                "IA","ID","IL","IN","KS","KY","LA","MA","MD","ME",
                "MI","MN","MO","MS","MT","NC","ND","NE","NH","NJ",
                "NM","NV","NY","OH","OK","OR","PA","SC","SD","TN",
                "TX","UT","VA","VT","WA","WI","WV","WY"};

            lstRegion = new List<string>
            {
                "US",
                "Arkansas-White-Red Region",
                "California Region",
                "Great Basin Region",
                "Great Lakes Region",
                "Lower Colorado Region",
                "Lower Mississippi Region",
                "Mid Atlantic Region",
                "Missouri Region",
                "New England Region",
                "Ohio Region",
                "Pacific Northwest Region",
                "Rio Grande Region",
                "Souris-Red-Rainy Region",
                "South Atlantic-Gulf Region",
                "Tennessee Region",
                "Texas-Gulf Region",
                "Upper Colorado Region",
                "Upper Mississippi Region"
            };

            lstCountry = new List<string>()
            {
                "World","Afghanistan","Albania","Algeria","American Samoa","Andorra","Angola","Anguilla",
                "Antarctica","Antigua and Barbuda","Argentina","Armenia","Aruba","Australia","Austria","Azerbaijan",
                "Bahamas","Bahrain","Bangladesh","Barbados","Belarus","Belgium","Belize","Benin","Bermuda","Bhutan",
                "Bolivia","Bonaire","Bosnia and Herzegovina","Botswana","Bouvet Island","Brazil","British Indian Ocean Territory",
                "British Virgin Islands","Brunei Darussalam","Bulgaria","Burkina Faso","Burundi","Cte dIvoire","Cabo Verde","Cambodia",
                "Cameroon","Canada","Canarias","Cayman Islands","Central African Republic","Chad","Chile","China",
                "Christmas Island","Cocos Islands","Colombia","Comoros","Congo","Congo DRC","Cook Islands","Costa Rica",
                "Croatia","Cuba","Curacao","Cyprus","Czech Republic","Denmark","Djibouti","Dominica","Dominican Republic",
                "Ecuador","Egypt","El Salvador","Equatorial Guinea","Eritrea","Estonia","Eswatini","Ethiopia","Falkland Islands",
                "Faroe Islands","Fiji","Finland","France","French Guiana","French Polynesia","French Southern Territories","Gabon",
                "Gambia","Georgia","Germany","Ghana","Gibraltar","Glorioso Islands","Greece","Greenland","Grenada",
                "Guadeloupe","Guam","Guatemala","Guernsey","Guinea","Guinea-Bissau","Guyana","Haiti","Heard Island and McDonald Islands",
                "Honduras","Hungary","Iceland","India","Indonesia","Iran","Iraq","Ireland","Isle of Man","Israel","Italy",
                "Jamaica","Japan","Jersey","Jordan","Juan De Nova Island","Kazakhstan","Kenya","Kiribati","Kuwait","Kyrgyzstan",
                "Laos","Latvia","Lebanon","Lesotho","Liberia","Libya","Liechtenstein","Lithuania","Luxembourg","Madagascar",
                "Malawi","Malaysia","Maldives","Mali","Malta","Marshall Islands","Martinique","Mauritania","Mauritius","Mayotte",
                "Mexico","Micronesia","Moldova","Monaco","Mongolia","Montenegro","Montserrat","Morocco","Mozambique","Myanmar",
                "Namibia","Nauru","Nepal","Netherlands","New Caledonia","New Zealand","Nicaragua","Niger","Nigeria",
                "Niue","Norfolk Island","North Korea","North Macedonia","Northern Mariana Islands","Norway","Oman",
                "Pakistan","Palau","Palestinian Territory","Panama","Papua New Guinea","Paraguay","Peru","Philippines","Pitcairn",
                "Poland","Portugal","Puerto Rico","Qatar","R??union","Romania","Russian Federation","Rwanda","Saba","Saint Barthelemy",
                "Saint Eustatius","Saint Helena","Saint Kitts and Nevis","Saint Lucia","Saint Martin","Saint Pierre and Miquelon",
                "Saint Vincent and the Grenadines","Samoa","San Marino","Sao Tome and Principe","Saudi Arabia","Senegal","Serbia",
                "Seychelles","Sierra Leone","Singapore","Sint Maarten","Slovakia","Slovenia","Solomon Islands","Somalia","South Africa",
                "South Georgia and South Sandwich Islands","South Korea","South Sudan","Spain","Sri Lanka","Sudan","Suriname","Svalbard",
                "Sweden","Switzerland","Syria","Tajikistan","Tanzania","Thailand","Timor-Leste","Togo","Tokelau","Tonga",
                "Trinidad and Tobago","Tunisia","Turkey","Turkmenistan","Turks and Caicos Islands","Tuvalu","Uganda","Ukraine","United Arab Emirates",
                "United Kingdom","United States","United States Minor Outlying Islands","Uruguay","US Virgin Islands","Uzbekistan",
                "Vanuatu","Vatican City","Venezuela","Vietnam","Wallis and Futuna","Yemen","Zambia","Zimbabwe"
            };

            if (optSta.Checked)
                cboZoom.DataSource = lstStates;
            else if (optHUC2.Checked)
                //GPF11-13-20
                cboZoom.DataSource = lstCountry;
            cboZoom.SelectedIndex = 0;

            //Generating tool tips
            mnuMain.ShowItemToolTips = true;
        }

        private void cboZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZoomSelectedIndex = cboZoom.SelectedIndex;
            if (ZoomSelectedIndex < 0) return;

            selectedArea = cboZoom.Items[ZoomSelectedIndex].ToString();
            switch (optRegion)
            {
                case (int)ZoomRegion.States:
                    optRegion = (int)ZoomRegion.States;
                    break;

                case (int)ZoomRegion.HUC2:
                    optRegion = (int)ZoomRegion.HUC2;
                    break;
            }
            ZoomToSelectedArea();
        }

        private void ZoomToSelectedArea()
        {
            switch (optRegion)
            {
                case (int)ZoomRegion.States:
                    //Debug.WriteLine("Area=" + selectedArea);
                    if (!selectedArea.Contains("US"))
                    {
                        wrld.SelectionEnabled = false;
                        st.SelectionEnabled = true;
                        st.SelectByAttribute("[ST] = '" + selectedArea + "'");
                        st.ZoomToSelectedFeatures();
                        st.IsSelected = false;
                        st.SelectionEnabled = false;
                        st.Selection.Clear();
                    }
                    else
                    {
                        wrld.SelectionEnabled = false;
                        st.SelectionEnabled = true;
                        st.SelectByAttribute("[COUNTRY] = '" + selectedArea + "'");
                        st.ZoomToSelectedFeatures();
                        st.IsSelected = false;
                        st.SelectionEnabled = false;
                        st.Selection.Clear();
                    }
                    break;

                case (int)ZoomRegion.HUC2:
                    //Debug.WriteLine("Area=" + selectedArea);
                    if (!selectedArea.Contains("World"))
                    {
                        //huc.SelectionEnabled = true;
                        //huc.SelectByAttribute("[COUNTRY] = '" + selectedArea + "'");
                        //huc.ZoomToSelectedFeatures();
                        //huc.IsSelected = false;
                        //huc.SelectionEnabled = false;
                        //huc.Selection.Clear();

                        st.SelectionEnabled = false;
                        wrld.SelectionEnabled = true;
                        wrld.SelectByAttribute("[COUNTRY] = '" + selectedArea + "'");
                        wrld.ZoomToSelectedFeatures();
                        wrld.IsSelected = false;
                        wrld.SelectionEnabled = false;
                        wrld.Selection.Clear();
                    }
                    else
                    {
                        st.SelectionEnabled = false;
                        wrld.SelectionEnabled = true;
                        wrld.SelectByAttribute("[REGION] = '" + selectedArea + "'");
                        wrld.ZoomToSelectedFeatures();
                        wrld.IsSelected = false;
                        wrld.SelectionEnabled = false;
                        wrld.Selection.Clear();

                    }
                    break;
            }
        }

        private void mnuSWMMmap_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.SWMM;

            splitMap.Panel1Collapsed = false;
            mnuMain.Enabled = false;
            lstOfPoints = new List<CPoint>();
            SelectPointFromMap();
        }

        private void mnuSWMMfile_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.SWMM;

            //check first if there are time series in wdm
            if (!ValidWDM()) return;

            string ext = ".csv";
            string filter = "Comma delimited file (*.csv)|*.csv|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Path.Combine(Application.StartupPath, "data");
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select lat-lon file ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    latlonFile = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }

                Debug.WriteLine("LatLon File = " + latlonFile);
                dictPoints = new Dictionary<string, CPoint>();

                if (ReadLatLonFile())
                {
                    lstOfPoints = new List<CPoint>();
                    if (dictPoints.Count > 0)
                    {
                        foreach (CPoint pt in dictPoints.Values)
                        {
                            double[] xy = new double[2];
                            double[] z = new double[1];
                            z[0] = 1;
                            xy[0] = pt.X;
                            xy[1] = pt.Y;

                            ProjectionInfo pS = KnownCoordinateSystems.Geographic.World.WGS1984;
                            ProjectionInfo pE = KnownCoordinateSystems.Projected.World.WebMercator;
                            Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                            CPoint newPt = new CPoint();
                            newPt.X = xy[0];
                            newPt.Y = xy[1];
                            lstOfPoints.Add(newPt);
                            newPt = null;
                        }

                        WriteStatus("Reading time series attributes of WDMFile ...");
                        frmSWMM fSWMM = new frmSWMM(uxMap, WdmFile, lstOfPoints);
                        WriteStatus("Ready ..");
                        if (fSWMM.ShowFormSWMM())
                            fSWMM.ShowDialog();
                        fSWMM.Dispose();
                    }
                    else
                    {
                        mnuMain.Enabled = true;
                        MessageBox.Show("No sites selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool ReadLatLonFile()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(latlonFile);

                for (int i = 1; i < lines.Length; i++)
                {
                    Debug.WriteLine(lines[i]);
                    string[] sline = lines[i].Split(',');
                    CPoint pt = new CPoint();
                    pt.X = Convert.ToDouble(sline[1]);
                    pt.Y = Convert.ToDouble(sline[2]);
                    string staid = sline[0];
                    dictPoints.Add(staid, pt);
                    //Debug.WriteLine("{0},{1},{2}", sline[0].ToString(), sline[1].ToString(), sline[2].ToString());
                    pt = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error reading lat-lon file!";
                WriteMessage("Error!", msg);
                return false;
            }
        }

        private void btnZoom_Click(object sender, EventArgs e)
        {
            ZoomToSelectedArea();
        }
        private void optSta_CheckedChanged(object sender, EventArgs e)
        {
            if (optSta.Checked)
            {
                cboZoom.DataSource = lstStates;
                optRegion = (int)ZoomRegion.States;
                WithinUS = true;
            }
            else
            {
                //cboZoom.DataSource = lstRegion;
                cboZoom.DataSource = lstCountry;
                optRegion = (int)ZoomRegion.HUC2;
                WithinUS = false;
            }
            cboZoom.SelectedIndex = 0;
            selectedArea = cboZoom.Items[cboZoom.SelectedIndex].ToString();

        }
        private void mnuSWATmap_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.SWAT;

            splitMap.Panel1Collapsed = false;
            mnuMain.Enabled = false;
            lstOfPoints = new List<CPoint>();
            SelectPointFromMap();
        }
        private void mnuSWATfile_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.SWAT;
            //check first if there are time series in wdm
            if (!ValidWDM()) return;

            string ext = ".csv";
            string filter = "Comma delimited file (*.csv)|*.csv|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Path.Combine(Application.StartupPath, "data");
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select lat-lon file ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    latlonFile = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }

                Debug.WriteLine("LatLon File = " + latlonFile);
                dictPoints = new Dictionary<string, CPoint>();

                if (ReadLatLonFile())
                {
                    lstOfPoints = new List<CPoint>();
                    if (dictPoints.Count > 0)
                    {
                        foreach (CPoint pt in dictPoints.Values)
                        {
                            double[] xy = new double[2];
                            double[] z = new double[1];
                            z[0] = 1;
                            xy[0] = pt.X;
                            xy[1] = pt.Y;

                            ProjectionInfo pS = KnownCoordinateSystems.Geographic.World.WGS1984;
                            ProjectionInfo pE = KnownCoordinateSystems.Projected.World.WebMercator;
                            Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                            CPoint newPt = new CPoint();
                            newPt.X = xy[0];
                            newPt.Y = xy[1];
                            lstOfPoints.Add(newPt);
                            newPt = null;
                        }

                        //browse for output folder
                        string sfolder = BrowseForFolder("Select folder to save SWAT weather files ...");
                        if (!string.IsNullOrEmpty(sfolder))
                        {
                            WriteStatus("Reading time series attributes of WDMFile ...");
                            frmSWAT fSWAT = new frmSWAT(uxMap, WdmFile, sfolder, lstOfPoints);
                            WriteStatus("Ready ..");
                            if (fSWAT.ShowFormSWAT())
                                fSWAT.ShowDialog();
                            fSWAT.Dispose();
                        }
                    }
                    else
                    {
                        mnuMain.Enabled = true;
                        MessageBox.Show("No sites selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void mnuEFDCmap_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.EFDC;
            splitMap.Panel1Collapsed = false;
            mnuMain.Enabled = false;
            lstOfPoints = new List<CPoint>();
            SelectPointFromMap();
        }
        private void mnuEFDCfile_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.EFDC;
            //check first if there are time series in wdm
            if (!ValidWDM()) return;

            string ext = ".csv";
            string filter = "Comma delimited file (*.csv)|*.csv|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Path.Combine(Application.StartupPath, "data");
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select lat-lon file ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    latlonFile = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }
            }

            Debug.WriteLine("LatLon File = " + latlonFile);
            dictPoints = new Dictionary<string, CPoint>();

            if (ReadLatLonFile())
            {
                lstOfPoints = new List<CPoint>();
                if (dictPoints.Count > 0)
                {
                    foreach (CPoint pt in dictPoints.Values)
                    {
                        double[] xy = new double[2];
                        double[] z = new double[1];
                        z[0] = 1;
                        xy[0] = pt.X;
                        xy[1] = pt.Y;

                        ProjectionInfo pS = KnownCoordinateSystems.Geographic.World.WGS1984;
                        ProjectionInfo pE = KnownCoordinateSystems.Projected.World.WebMercator;
                        Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                        CPoint newPt = new CPoint();
                        newPt.X = xy[0];
                        newPt.Y = xy[1];
                        lstOfPoints.Add(newPt);
                        newPt = null;
                    }
                    WriteStatus("Reading time series attributes of WDMFile ...");
                    frmEFDC fEFDC = new frmEFDC(uxMap, WdmFile, lstOfPoints);
                    WriteStatus("Ready ..");
                    if (fEFDC.ShowFormEFDC())
                        fEFDC.ShowDialog();
                    fEFDC.Dispose();
                }
                else
                {
                    mnuMain.Enabled = true;
                    MessageBox.Show("No sites selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void mnuWASPmap_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.WASP;
            splitMap.Panel1Collapsed = false;
            mnuMain.Enabled = false;
            lstOfPoints = new List<CPoint>();
            SelectPointFromMap();
        }
        private void mnuWASPfile_Click(object sender, EventArgs e)
        {
            optModel = (int)Model.WASP;
            //check first if there are time series in wdm
            if (!ValidWDM()) return;

            string ext = ".csv";
            string filter = "Comma delimited file (*.csv)|*.csv|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Path.Combine(Application.StartupPath, "data");
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select lat-lon file ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    latlonFile = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }
            }

            Debug.WriteLine("LatLon File = " + latlonFile);
            dictPoints = new Dictionary<string, CPoint>();

            if (ReadLatLonFile())
            {
                lstOfPoints = new List<CPoint>();
                if (dictPoints.Count > 0)
                {
                    foreach (CPoint pt in dictPoints.Values)
                    {
                        double[] xy = new double[2];
                        double[] z = new double[1];
                        z[0] = 1;
                        xy[0] = pt.X;
                        xy[1] = pt.Y;

                        ProjectionInfo pS = KnownCoordinateSystems.Geographic.World.WGS1984;
                        ProjectionInfo pE = KnownCoordinateSystems.Projected.World.WebMercator;
                        Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                        CPoint newPt = new CPoint();
                        newPt.X = xy[0];
                        newPt.Y = xy[1];
                        lstOfPoints.Add(newPt);
                        newPt = null;
                    }

                    //browse for output sdb database
                    string dbfile = BrowseForSDB();
                    if (!string.IsNullOrEmpty(dbfile))
                    {
                        WriteStatus("Reading time series attributes of WDMFile ...");
                        frmWASP fWASP = new frmWASP(uxMap, WdmFile, dbfile, lstOfPoints);
                        WriteStatus("Ready ..");
                        if (fWASP.ShowFormWASP())
                            fWASP.ShowDialog();
                        fWASP.Dispose();
                    }
                    else
                        WriteStatus("Cancelling ...");

                }
                else
                {
                    mnuMain.Enabled = true;
                    MessageBox.Show("No sites selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void optHUC2_CheckedChanged(object sender, EventArgs e)
        {
            if (optHUC2.Checked)
            {
                //cboZoom.DataSource = lstRegion; gpf111320
                cboZoom.DataSource = lstCountry;
                optRegion = (int)ZoomRegion.HUC2;
                WithinUS = false;
            }
            else
            {
                cboZoom.DataSource = lstStates;
                optRegion = (int)ZoomRegion.States;
                WithinUS = true;
            }
            cboZoom.SelectedIndex = 0;
            selectedArea = cboZoom.Items[cboZoom.SelectedIndex].ToString();
        }
        private void Map_OnMouseClick(object sender, MouseEventArgs e)
        {
            double Xlon, Ylat;
            if (MapMode == (int)SelectMode.DrawPoint)
            {
                //intercept left button click
                if (e.Button != MouseButtons.Left) return;
                // Get the geographic location that was clicked
                Coordinate c = appManager.Map.PixelToProj(e.Location);

                Debug.WriteLine("selected pt=" + c.X + "," + c.Y);

                double[] xy = new double[2];
                double[] z = new double[1];
                z[0] = 1;
                xy[0] = c.X;
                xy[1] = c.Y;

                //add point to list, list is in projection coordinates
                CPoint pt = new CPoint();
                pt.X = c.X;
                pt.Y = c.Y;
                lstOfPoints.Add(pt);
                pt = null;

                ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
                ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
                Debug.WriteLine("In mouseclick: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);

                Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                Xlon = xy[0];
                Ylat = xy[1];
                string txtLat = string.Format("{0:0.00000}", Ylat);
                string txtLon = string.Format("{0:0.00000}", Xlon);

                Debug.WriteLine("In mouseclick reproject: textlon={0}, textlat={1}", txtLon, txtLat);
                string result = GetTimezone(Ylat, Xlon);
                Debug.WriteLine("In GetTimeZone = {0},{1},{2}", txtLat, txtLon, result);

                //add point to drawing layers
                Debug.WriteLine("num point est features = " + mapPoint.Features.Count);
                mapPoint.AddFeature(new DotSpatial.Topology.Point(c));
                //Debug.WriteLine("num point est features = " + mapPoint.Features.Count);
                appManager.Map.MapFrame.Invalidate();
            }
            else
                return;
        }
        private void btnOKPt_Click(object sender, EventArgs e)
        {
            int numPoints = mapPoint.Features.Count;
            //Debug.WriteLine("In btn OK, num point=" + numPoints.ToString());
            // Remove our drawing layer from the map.
            //appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);

            // Request a redraw
            appManager.Map.MapFrame.Invalidate();
            uxMap.MouseClick -= new System.Windows.Forms.MouseEventHandler(Map_OnMouseClick);
            MapMode = (int)SelectMode.None;
            splitMap.Panel1Collapsed = true;

            Cursor.Current = Cursors.Default;

            //check first if there are time series in wdm
            if (!ValidWDM()) return;

            if (lstOfPoints.Count > 0)
            {
                switch (optModel)
                {
                    case (int)Model.SWMM:
                        ShowSWMMForm();
                        break;
                    case (int)Model.SWAT:
                        string sfolder = string.Empty;
                        sfolder = BrowseForFolder("Select folder to save SWAT weather files ...");
                        if (!string.IsNullOrEmpty(sfolder))
                            ShowSWATForm(sfolder);
                        break;
                    case (int)Model.EFDC:
                        ShowEFDCForm();
                        break;
                    case (int)Model.WASP:
                        string dbFile = string.Empty;
                        dbFile = BrowseForSDB();
                        if (!string.IsNullOrEmpty(dbFile))
                            ShowWASPForm(dbFile);
                        break;
                }
            }
            else
            {
                mnuMain.Enabled = true;
                MessageBox.Show("No sites selected!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void mnuExport_Click(object sender, EventArgs e)
        {
            ClearMapSites();
        }
        private void mnuExportSWAT_Click(object sender, EventArgs e)
        {
            ClearMapSites();
        }
        private void mnuExportEFDC_Click(object sender, EventArgs e)
        {
            ClearMapSites();
        }
        private void mnuSearchDatasetNLDAS_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.NLDAS;
            SearchByDatasetSource();
        }
        private void mnuSearchDatasetISD_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.ISD;
            SearchByDatasetSource();
        }
        private void mnuSearchDatasetCOOP_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.HRAIN;
            SearchByDatasetSource();
        }
        private void mnuGenerate_Click(object sender, EventArgs e)
        {
            string ext = ".mdl";
            string filter = "Model database (*.mdl)|*.mdl|All files (*.*)|*.*";
            string sdbModelFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select Stochastic Model Parameters Database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sdbModelFile = openFD.FileName;
                }
                else
                {
                    sdbModelFile = string.Empty;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("sfile=" + sdbModelFile);
            }

            //show weather generator form
            frmWeaGen wgen = new frmWeaGen(wrlog, sdbModelFile);
            wgen.ShowDialog();
            wgen = null;
        }
        private void mnuExportSDB_Click(object sender, EventArgs e)
        {
            OptionExport = (int)Export.SQL;
            frmDB aDB = new frmDB(this, SdbFile, WdmFile);
            if (aDB.ShowDialog() == DialogResult.OK)
            {
                Debug.WriteLine("SDBFile in main: " + SdbFile);
                Debug.WriteLine("WDMFile in main: " + WdmFile);
                aDB.Dispose();
            }
            else
            {
                Debug.WriteLine("User cancel!");
                aDB.Dispose();
                return;
            }

            //show form
            Cursor.Current = Cursors.WaitCursor;
            frmExport fExport = new frmExport(wrlog, OptionExport, SdbFile, WdmFile);
            WriteStatus("Reading " + WdmFile);
            if (fExport.GetWDMSeries())
            {
                Cursor.Current = Cursors.Default;

                WriteStatus("Ready ...");
                fExport.Text = Path.GetFileName(WdmFile);
                fExport.ShowDialog();
            }
            fExport.Dispose();
        }
        private void mnuExportCSV_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(WdmFile))
            OptionExport = (int)Export.CSV;
            if (BrowseForWDM())
            {
                Debug.WriteLine("WDMFile in main: " + WdmFile);

                //show form
                frmExport fWeaSDB = new frmExport(wrlog, OptionExport, SdbFile, WdmFile);
                WriteStatus("Reading " + WdmFile);
                if (fWeaSDB.GetWDMSeries())
                {
                    WriteStatus("Ready ...");
                    fWeaSDB.ShowDialog();
                }
                fWeaSDB.Dispose();
            }
        }

        private void mnuSearchDataset_Click(object sender, EventArgs e)
        {
            Extent ext = appManager.Map.ViewExtents;
            Debug.WriteLine("{0},{1}", ext.Center.X.ToString(), ext.Center.Y.ToString());

            WithinUS = optSta.Checked;

            {
                if (!WithinUS) //world
                {
                    mnuSearchDatasetCOOP.Enabled = false;
                    mnuSearchDatasetNLDAS.Enabled = false;
                    mnuSearchDatasetEDDE.Enabled = false;
                    mnuSearchDatasetPRISM.Enabled = false;
                }
                else
                {
                    mnuSearchDatasetCOOP.Enabled = true;
                    mnuSearchDatasetNLDAS.Enabled = true;
                    mnuSearchDatasetEDDE.Enabled = true;
                    mnuSearchDatasetPRISM.Enabled = true;
                }
            }
            //if (optRegion == (int)ZoomRegion.HUC2) //world
            //{
            //    mnuSearchDatasetCOOP.Enabled = false;
            //    mnuSearchDatasetNLDAS.Enabled = false;
            //}
            //else
            //{
            //    mnuSearchDatasetCOOP.Enabled = true;
            //    mnuSearchDatasetNLDAS.Enabled = true;
            //}
        }
        private void uxMap_ViewExtentsChanged(object sender, ExtentArgs e)
        {
            CPoint prjPt = new CPoint();
            Extent ext = appManager.Map.ViewExtents;
            //Debug.WriteLine("In viewextents Center: {0},{1}", ext.Center.X.ToString(), ext.Center.Y.ToString());
            //Debug.WriteLine("In viewextents Lon: {0},{1}", ext.MinX.ToString(), ext.MaxX.ToString());
            //Debug.WriteLine("In viewextents Lat: {0},{1}", ext.MinY.ToString(), ext.MaxY.ToString());
            prjPt = GetViewLatLon(ext.Center.X, ext.Center.Y);
            //Debug.WriteLine("In viewextents Center: {0},{1}", prjPt.X.ToString(), prjPt.Y.ToString());
            prjPt = GetViewLatLon(ext.MinX, ext.MinY);
            //Debug.WriteLine("In viewextents LowerLeft: {0},{1}", prjPt.X.ToString(), prjPt.Y.ToString());
            prjPt = GetViewLatLon(ext.MaxX, ext.MaxY);
            //Debug.WriteLine("In viewextents UpperRight: {0},{1}", prjPt.X.ToString(), prjPt.Y.ToString());

            Coordinate c = new Coordinate(ext.Center.X, ext.Center.Y);
            if (!(selStLayer == null))
            {
                USextent = selStLayer.Extent;
                if (USextent.Intersects(c))
                {
                    WithinUS = true;
                    //Debug.WriteLine("Center point within US extent");
                }
                else
                {
                    WithinUS = false;
                    //Debug.WriteLine("Center point outside US extent");
                }
            }
        }

        private void mnuManageData_Click(object sender, EventArgs e)
        {
        }

        private void mnuManageSARA_ClickOLD(object sender, EventArgs e)
        {
            int iFiltIndex = 0;
            if (string.IsNullOrEmpty(TSutilExe))
            {
                TSutilExe = atcUtility.modFile.FindFile("Please locate SARA TimeSeries Utility ...",
                 "TimeseriesUtility.exe", "", "", false, false, ref iFiltIndex);
            }

            if (!File.Exists(TSutilExe))
            {
                string msg = "Please install the SARA TimeSeries Utility Program!" + Environment.NewLine + Environment.NewLine +
                    "Available from https://www.respec.com/product/modeling-optimization/sara-timeseries-utility/";
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Process sara = new Process();
                sara.StartInfo.FileName = Path.Combine(Application.StartupPath, TSutilExe);
                if (!string.IsNullOrEmpty(WdmFile))
                    sara.StartInfo.Arguments = WdmFile;
                sara.Start();
            }
        }

        private void mnuManageSARA_Click(object sender, EventArgs e)
        {
            Process sara = new Process();
            sara.StartInfo.FileName = Path.Combine(Application.StartupPath, TSutilExe);
            if (!string.IsNullOrEmpty(WdmFile))
                  sara.StartInfo.Arguments = WdmFile;
            sara.Start();
        }
        private void mnuManageWDM_Click(object sender, EventArgs e)
        {
            string pdir = Application.StartupPath;
            pdir = Path.Combine(pdir, WDMUtilExe);
            {
                Process wdmutil = new Process();
                wdmutil.StartInfo.FileName = pdir;
                if (!string.IsNullOrEmpty(WdmFile))
                    wdmutil.StartInfo.Arguments = WdmFile;
                wdmutil.Start();
            }
        }
        private void mnuSpatialAnalysis_Click(object sender, EventArgs e)
        {
            if (BrowseForAnnualWDM())
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    //show form; default to annual
                    List<string> timeunit = new List<string>();
                    timeunit.Add("Year");
                    timeunit.Add("Month");

                    frmSpatial fSpatial = new frmSpatial(this, AnnualWdmFile, wrlog);
                    WriteStatus("Reading " + AnnualWdmFile);
                    if (fSpatial.GetWDMSeries(timeunit))
                    {
                        WriteStatus("Ready ...");
                        //fSpatial.optAnnual.Checked = true;
                        //if (fSpatial.FilterWDMSeriesForSelectedVar())
                        fSpatial.Show();
                    }
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    string msg = "Error in spatial analysis routine! "  + crlf + crlf + ex.Message + ex.StackTrace;
                    MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isOpenAnnualWDM = false;
                }
            }
        }

        private void gCMToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void mnuDB_Click(object sender, EventArgs e)
        {

        }

        private void mnuMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private CPoint GetViewLatLon(double X, double Y)
        {
            double[] xy = new double[2];
            double[] z = new double[1];
            z[0] = 1;
            xy[0] = X;
            xy[1] = Y;

            ProjectionInfo pTo = KnownCoordinateSystems.Geographic.World.WGS1984;
            ProjectionInfo pFrom = KnownCoordinateSystems.Projected.World.WebMercator;
            Reproject.ReprojectPoints(xy, z, pFrom, pTo, 0, 1);

            CPoint newPt = new CPoint();
            newPt.X = xy[0];
            newPt.Y = xy[1];
            return newPt;
        }

        private void mnuSearchDatasetPRISM_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.PRISM;
            SearchByDatasetSource();
        }
        private void mnuSearchDatasetCMIP6_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.CMIP6;
            SearchByDatasetSource();
        }

        private void mnuSearchDatasetEDDE_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.EDDE;
            SearchByDatasetSource();
        }

        private void mnuCMIP6_Click(object sender, EventArgs e)
        {
            string spath = Application.StartupPath.ToString();
            string sfile = Path.Combine(spath, "NEX-GDDP-CMIP6-TechnicalNote.pdf");
            ShowManual(sfile);
        }

        private void mnuSearchDatasetTRMM_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.TRMM;
            SearchByDatasetSource();
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Request a redraw
            uxMap.MouseClick -= new System.Windows.Forms.MouseEventHandler(Map_OnMouseClick);
            MapMode = (int)SelectMode.None;
            if (appManager.Map.MapFrame.DrawingLayers.Contains(mapPointLayer))
            {
                // Remove our drawing layer from the map.
                appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                // Request a redraw
                appManager.Map.MapFrame.Invalidate();
            }
            splitMap.Panel1Collapsed = true;
            mnuMain.Enabled = true;
            MapMode = (int)SelectMode.None;
            uxMap.Cursor = Cursors.Default;
        }

        private void mnuSearchDataSetGLDAS_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.GLDAS;
            SearchByDatasetSource();
        }
        private void mnuSearchDatasetGHCN_Click(object sender, EventArgs e)
        {
            optDataSource = (int)MetDataSource.GHCN;
            SearchByDatasetSource();
        }
        private bool BrowseForAnnualWDM()
        {
            bool isValid = false;
            Cursor.Current = Cursors.WaitCursor;

            string ext = ".wdm";
            string filter = "WDM files (*.wdm)|*.wdm|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select a WDM file with Annual/Monthly Data ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sFile = openFD.FileName;
                else
                    sFile = string.Empty;
            }

            try
            {
                if (!string.IsNullOrEmpty(sFile))
                {
                    appManager.UpdateProgress("Checking datasets in " + Path.GetFileName(sFile));

                    if (File.Exists(sFile))
                    {
                        //check if valid file
                        if (IsValidAnnualWDM(sFile))
                        {
                            string fname = Path.GetFileName(sFile);
                            this.Text = "Model WeatherData Processor: " + fname;
                            AnnualWdmFile = sFile;
                            isOpenAnnualWDM = true;

                            string path = Path.GetDirectoryName(AnnualWdmFile);
                            sFile = Path.GetFileNameWithoutExtension(AnnualWdmFile);
                            isValid = true;
                        }
                        else
                        {
                            string msg = sFile + " does not have an annual or monthly timeseries!";
                            MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            isOpenAnnualWDM = false;
                            isValid = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string msg = "Error opening "+ sFile +crlf+crlf+ex.Message+ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isOpenAnnualWDM = false;
                isValid = false;
            }
            Cursor.Current = Cursors.Default;
            return isValid;
        }
        private bool BrowseForWDM()
        {
            string ext = ".wdm";
            string filter = "WDM database (*.wdm)|*.wdm|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.FileName = WdmFile;
                openFD.Title = "Select a WDM database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    if (IsValidWDM(sFile))
                    {
                        WdmFile = sFile;
                    }
                    else
                    {
                        WdmFile = string.Empty; ;
                    }
                    return true;
                }
                else
                {
                    sFile = string.Empty;
                    return false;
                }
            }
        }
        private string BrowseForSDB()
        {
            string ext = ".sdb";
            string filter = "SQLite database (*.sdb)|*.sdb|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = false;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.FileName = WdmFile;
                openFD.Title = "Select or create a SQLite database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    if (!File.Exists(sFile))
                    {
                        File.Copy(defaultSDB, sFile);
                    }
                }
                else
                {
                    sFile = string.Empty;
                }
            }
            return sFile;
        }
        private void uxMap_Load(object sender, EventArgs e)
        {

        }

        private string BrowseForFolder(string desc)
        {
            string sfolder = string.Empty;
            try
            {
                using (FolderBrowserDialog openFD = new FolderBrowserDialog())
                {
                    openFD.ShowNewFolderButton = true;
                    //openFD.RootFolder = Environment.SpecialFolder.MyComputer;
                    openFD.SelectedPath = dataDir;
                    //openFD.Description = "Select folder to save SWAT weather files ...";
                    openFD.Description = desc;
                    if (openFD.ShowDialog() == DialogResult.OK)
                        sfolder = openFD.SelectedPath;
                    else
                    {
                        sfolder = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                sfolder = string.Empty;
            }
            return sfolder;
        }
        private void ShowSWMMForm()
        {
            WriteStatus("Reading time series attributes of WDMFile ...");
            frmSWMM fSWMM = new frmSWMM(uxMap, WdmFile, lstOfPoints);
            WriteStatus("Ready ...");
            if (fSWMM.ShowFormSWMM())
            {

                fSWMM.ShowDialog();
                {
                    mnuMain.Enabled = true;
                    // Remove our drawing layer from the map.
                    appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                    appManager.Map.MapFrame.Invalidate();
                }
            }
            mnuMain.Enabled = true;
            appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
            appManager.Map.MapFrame.Invalidate();
            fSWMM.Dispose();
        }
        private void ShowSWATForm(string sfolder)
        {
            WriteStatus("Reading time series attributes of WDMFile ...");
            frmSWAT fSWAT = new frmSWAT(uxMap, WdmFile, sfolder, lstOfPoints);
            WriteStatus("Ready ...");
            if (fSWAT.ShowFormSWAT())
            {
                fSWAT.ShowDialog();
                {
                    mnuMain.Enabled = true;
                    // Remove our drawing layer from the map.
                    appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                    appManager.Map.MapFrame.Invalidate();
                }
            }
            mnuMain.Enabled = true;
            appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
            appManager.Map.MapFrame.Invalidate();
            fSWAT.Dispose();
        }
        private void ShowEFDCForm()
        {
            WriteStatus("Reading time series attributes of WDMFile ...");
            frmEFDC fEFDC = new frmEFDC(uxMap, WdmFile, lstOfPoints);
            WriteStatus("Ready ...");
            if (fEFDC.ShowFormEFDC())
            {
                fEFDC.ShowDialog();
                {
                    mnuMain.Enabled = true;
                    // Remove our drawing layer from the map.
                    appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                    appManager.Map.MapFrame.Invalidate();
                }
            }
            mnuMain.Enabled = true;
            appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
            appManager.Map.MapFrame.Invalidate();
            fEFDC.Dispose();
        }
        private void ShowWASPForm(string dbFile)
        {
            WriteStatus("Reading time series attributes of WDMFile ...");
            frmWASP fWASP = new frmWASP(uxMap, WdmFile, dbFile, lstOfPoints);
            WriteStatus("Ready ...");
            if (fWASP.ShowFormWASP())
            {
                fWASP.ShowDialog();
                {
                    mnuMain.Enabled = true;
                    // Remove our drawing layer from the map.
                    appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                    appManager.Map.MapFrame.Invalidate();
                }
            }
            mnuMain.Enabled = true;
            appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
            appManager.Map.MapFrame.Invalidate();
            fWASP.Dispose();
        }
        private bool ValidWDM()
        {
            //check first if there are time series in wdm
            atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
            if (!lwdm.Open(WdmFile)) return false;
            int numdset = lwdm.DataSets.Count;
            if (numdset == 0)
            {
                MessageBox.Show("There are no timeseries in " + WdmFile, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lwdm.Clear();
                lwdm = null;

                mnuMain.Enabled = true;
                // Remove our drawing layer from the map.
                appManager.Map.MapFrame.DrawingLayers.Remove(mapPointLayer);
                appManager.Map.MapFrame.Invalidate();

                return false;
            }
            else { lwdm.Clear(); lwdm = null; }
            return true;
        }
        #endregion "WDM Exports"

        public void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
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
                    MessageBox.Show(msg, "Information!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
        public void WriteLogFile(string msg)
        {
            wrlog.WriteLine(msg);
            wrlog.AutoFlush = true;
            wrlog.Flush();
        }

        private string GetTimezone(double lat, double lon)
        {
            string key = System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            string hereKey = System.Environment.GetEnvironmentVariable("HERE_API_KEY");
            Debug.WriteLine("GoogleKey =" + key + ", HereKey=" + hereKey);
            if (key != null)
            {
                string baseUrl = "https://maps.googleapis.com/maps/api/timezone/json?";
                string location = "location=" + lat.ToString() + "," + lon.ToString();
                string timeStamp = "timestamp=1331161200";
                string completeUrl = baseUrl + location + "&" + timeStamp + "&key=" + key;
                Debug.WriteLine("Url=" + completeUrl);
                try
                {
                    WebClient wc = new WebClient();
                    byte[] buffer = wc.DownloadData(completeUrl);
                    string resultString = Encoding.UTF8.GetString(buffer);
                    //Dictionary<string, object> tz = JsonSerializer.Deserialize<Dictionary<string, object>>(resultString);
                    //return new Timezone() { Name = tz["timeZoneId"].ToString(), Offset = Convert.ToDouble(tz["rawOffset"].ToString()) / 3600, DLS = false };
                    return resultString;
                }
                catch (Exception ex)
                {
                    //errorMsg = "ERROR: " + ex.Message;
                    //return new Timezone();
                    return null;
                }
            }
            else if (hereKey != null)
            {
                string baseUrl = "https://reverse.geocoder.ls.hereapi.com/6.2/reversegeocode.json?";
                string location = "prox=" + lat.ToString() + "," + lon.ToString();
                string completeUrl = baseUrl + location + "&mode=retrieveAddresses&maxresults=1&gen=9&key=" + hereKey;
                Debug.WriteLine("Url=" + completeUrl);
                try
                {
                    WebClient wc = new WebClient();
                    byte[] buffer = wc.DownloadData(completeUrl);
                    string resultString = Encoding.UTF8.GetString(buffer);
                    //Dictionary<string, object> tz = JsonSerializer.Deserialize<Dictionary<string, object>>(resultString);
                    //return new Timezone() { Name = tz["timeZoneId"].ToString(), Offset = Convert.ToDouble(tz["rawOffset"].ToString()) / 3600, DLS = false };
                    return resultString;
                }
                catch (Exception ex)
                {
                    //errorMsg = "ERROR: " + ex.Message;
                    //return new Timezone();
                    return null;
                }
            }
            else
            {
                //errorMsg = "ERROR: Unable to retrieve timezone data.";
                //return new Timezone();
                return null;
            }
        }
    }
}
