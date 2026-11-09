#define debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotSpatial.Controls;
using System.IO;
using System.Diagnostics;
using wdmuploader;

namespace NCEIData
{
    public class clsCMIP6
    {
        private frmMain fMain;
        private CMIP6Series cmipSeries;
        private SortedDictionary<string, List<string>> dictCatalog;
        private SortedDictionary<int, GridPoint> dictGridPts;
        private SortedDictionary<string, List<string>> dictSiteVars;

        private SpatialStatusStrip strip;
        private ToolStripStatusLabel slabel;

        private string savepath, savefile, ncdffile, outfile;
        private string URLpath, cacheDir, ncdumpPath;
        private StreamWriter outsw;

        //bounding box
        private float north, south, east, west;
        private string scenario, pathway, variant, scenarioVar;
        private string location, scenpath=string.Empty;
        private List<string> lstOfVars;
        private float minLat, minLon, maxLat, maxLon;
        private int numLat, numLon, numGrids;
        private int numVars, numYrs, numFiles, iterNum=0, maxIter=0, curyear;
        private double missval;
        private string MISS = "-9999";

        //period of record
        private int begyr, endyr;
        private DateTime begDate, endDate;

        private string URLbase = "https://ds.nccs.nasa.gov/thredds/ncss/grid/AMES/NEX/GDDP-CMIP6/";
        private string crlf = Environment.NewLine;
        private string errmsg;

        private Dictionary<string, SortedDictionary<DateTime, string>> dictScenSeries;
        private Dictionary<string, SortedDictionary<DateTime, string>> dictAnnSeries;
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictSeries;

        private SortedDictionary<string, string> dictScenPath;
        private List<SortedDictionary<DateTime, string>> varSeries;
        private List<SortedDictionary<DateTime, string>> annSeries;
        private string WdmFile, AnnWdmFile;
        private bool isValidScenPath = false;
        private List<string> lstDownloadedVars = new List<string>();

        public clsCMIP6(frmMain _fmain, CMIP6Series _cmipSeries)
        {
            fMain = _fmain;
            AnnWdmFile = fMain.AnnWdmFile;
            WdmFile = _fmain.WdmFile;
            dictSiteVars = _fmain.dictSiteVars;
            cmipSeries = _cmipSeries;
            strip = _fmain.statusStrip;
            slabel = _fmain.statuslbl;
            dictCatalog = _fmain.dictCMIP6Files;
            cacheDir = _fmain.cacheDir;
            savepath = Path.Combine(cacheDir, "CMIP6");
            ncdumpPath = Path.Combine(Application.StartupPath, "netcdf");

            //get CMIP6 parameters
            lstOfVars = cmipSeries.ClimateVar;
            scenario = cmipSeries.Scenario;
            pathway = cmipSeries.Pathway;
            begyr = cmipSeries.BeginYear;
            endyr = cmipSeries.EndYear;
            begDate = new DateTime(begyr, 1, 1, 12, 0, 0);
            endDate = new DateTime(endyr, 12, 31, 12, 0, 0);
            numVars = lstOfVars.Count;
            numYrs = (endyr - begyr) + 1;
            numFiles = numVars * numYrs;
            maxIter = numFiles;

            //max and min lat-lon of the selected grid, to get bounding box
            //will need to add/subtract 0.0625
            north = cmipSeries.GridBox.North;
            south = cmipSeries.GridBox.South;
            west = cmipSeries.GridBox.West;
            east = cmipSeries.GridBox.East;
            minLon = west;
            maxLon = east;
            minLat = south;
            maxLat = north;

            ScenarioMapping();
            SetupGridCoordinates();

            //combine scenario 
            isValidScenPath = dictScenPath.TryGetValue(scenario + "_" + pathway, out scenpath);
            Debug.WriteLine("In clsCMIP6, scenpath = " + scenpath.ToString());
            if (!isValidScenPath)
            {
                errmsg = "There are no timeseries for the selected scenario " + scenario +
                    " and pathway " + pathway;
                MessageBox.Show(errmsg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
                fMain.scenarioPath = scenpath;
            fMain.pBar.Visible = true;
#if debug
debugScenario();
#endif
        }
        public bool IsValidScenarioPathway()
        {
            return isValidScenPath;
        }

        private void ScenarioMapping()
        {
            dictScenPath = new SortedDictionary<string, string>();
            dictScenPath.Add("ACCESS-CM2_historical", "S01_OBS");
            dictScenPath.Add("ACCESS-CM2_ssp126", "S01_P126");
            dictScenPath.Add("ACCESS-CM2_ssp245", "S01_P245");
            dictScenPath.Add("ACCESS-CM2_ssp370", "S01_P370");
            dictScenPath.Add("ACCESS-CM2_ssp585", "S01_P585");
            dictScenPath.Add("ACCESS-ESM1-5_historical", "S02_OBS");
            dictScenPath.Add("ACCESS-ESM1-5_ssp126", "S02_P126");
            dictScenPath.Add("ACCESS-ESM1-5_ssp245", "S02_P245");
            dictScenPath.Add("ACCESS-ESM1-5_ssp370", "S02_P370");
            dictScenPath.Add("ACCESS-ESM1-5_ssp585", "S02_P585");
            dictScenPath.Add("BCC-CSM2-MR_historical", "S03_OBS");
            dictScenPath.Add("BCC-CSM2-MR_ssp126", "S03_P126");
            dictScenPath.Add("BCC-CSM2-MR_ssp245", "S03_P245");
            dictScenPath.Add("BCC-CSM2-MR_ssp370", "S03_P370");
            dictScenPath.Add("BCC-CSM2-MR_ssp585", "S03_P585");
            dictScenPath.Add("CanESM5_historical", "S04_OBS");
            dictScenPath.Add("CanESM5_ssp126", "S04_P126");
            dictScenPath.Add("CanESM5_ssp245", "S04_P245");
            dictScenPath.Add("CanESM5_ssp370", "S04_P370");
            dictScenPath.Add("CanESM5_ssp585", "S04_P585");
            dictScenPath.Add("CESM2_historical", "S05_OBS");
            dictScenPath.Add("CESM2_ssp126", "S05_P126");
            dictScenPath.Add("CESM2_ssp245", "S05_P245");
            dictScenPath.Add("CESM2_ssp370", "S05_P370");
            dictScenPath.Add("CESM2_ssp585", "S05_P585");
            dictScenPath.Add("CESM2-WACCM_historical", "S06_OBS");
            dictScenPath.Add("CESM2-WACCM_ssp245", "S06_P245");
            dictScenPath.Add("CESM2-WACCM_ssp585", "S06_P585");
            dictScenPath.Add("CMCC-CM2-SR5_historical", "S07_OBS");
            dictScenPath.Add("CMCC-CM2-SR5_ssp126", "S07_P126");
            dictScenPath.Add("CMCC-CM2-SR5_ssp245", "S07_P245");
            dictScenPath.Add("CMCC-CM2-SR5_ssp370", "S07_P370");
            dictScenPath.Add("CMCC-CM2-SR5_ssp585", "S07_P585");
            dictScenPath.Add("CMCC-ESM2_historical", "S08_OBS");
            dictScenPath.Add("CMCC-ESM2_ssp126", "S08_P126");
            dictScenPath.Add("CMCC-ESM2_ssp245", "S08_P245");
            dictScenPath.Add("CMCC-ESM2_ssp370", "S08_P370");
            dictScenPath.Add("CMCC-ESM2_ssp585", "S08_P585");
            dictScenPath.Add("CNRM-CM6-1_historical", "S09_OBS");
            dictScenPath.Add("CNRM-CM6-1_ssp126", "S09_P126");
            dictScenPath.Add("CNRM-CM6-1_ssp245", "S09_P245");
            dictScenPath.Add("CNRM-CM6-1_ssp370", "S09_P370");
            dictScenPath.Add("CNRM-CM6-1_ssp585", "S09_P585");
            dictScenPath.Add("CNRM-ESM2-1_historical", "S10_OBS");
            dictScenPath.Add("CNRM-ESM2-1_ssp126", "S10_P126");
            dictScenPath.Add("CNRM-ESM2-1_ssp245", "S10_P245");
            dictScenPath.Add("CNRM-ESM2-1_ssp370", "S10_P370");
            dictScenPath.Add("CNRM-ESM2-1_ssp585", "S10_P585");
            dictScenPath.Add("EC-Earth3_historical", "S11_OBS");
            dictScenPath.Add("EC-Earth3_ssp126", "S11_P126");
            dictScenPath.Add("EC-Earth3_ssp245", "S11_P245");
            dictScenPath.Add("EC-Earth3_ssp370", "S11_P370");
            dictScenPath.Add("EC-Earth3_ssp585", "S11_P585");
            dictScenPath.Add("EC-Earth3-Veg-LR_historical", "S12_OBS");
            dictScenPath.Add("EC-Earth3-Veg-LR_ssp126", "S12_P126");
            dictScenPath.Add("EC-Earth3-Veg-LR_ssp245", "S12_P245");
            dictScenPath.Add("EC-Earth3-Veg-LR_ssp370", "S12_P370");
            dictScenPath.Add("EC-Earth3-Veg-LR_ssp585", "S12_P585");
            dictScenPath.Add("FGOALS-g3_historical", "S13_OBS");
            dictScenPath.Add("FGOALS-g3_ssp126", "S13_P126");
            dictScenPath.Add("FGOALS-g3_ssp245", "S13_P245");
            dictScenPath.Add("FGOALS-g3_ssp370", "S13_P370");
            dictScenPath.Add("FGOALS-g3_ssp585", "S13_P585");
            dictScenPath.Add("GFDL-CM4_gr2_historical", "S14_OBS");
            dictScenPath.Add("GFDL-CM4_gr2_ssp245", "S14_P245");
            dictScenPath.Add("GFDL-CM4_gr2_ssp585", "S14_P585");
            dictScenPath.Add("GFDL-CM4_historical", "S15_OBS");
            dictScenPath.Add("GFDL-CM4_ssp245", "S15_P245");
            dictScenPath.Add("GFDL-CM4_ssp585", "S15_P585");
            dictScenPath.Add("GFDL-ESM4_historical", "S16_OBS");
            dictScenPath.Add("GFDL-ESM4_ssp126", "S16_P126");
            dictScenPath.Add("GFDL-ESM4_ssp245", "S16_P245");
            dictScenPath.Add("GFDL-ESM4_ssp370", "S16_P370");
            dictScenPath.Add("GFDL-ESM4_ssp585", "S16_P585");
            dictScenPath.Add("GISS-E2-1-G_historical", "S17_OBS");
            dictScenPath.Add("GISS-E2-1-G_ssp126", "S17_P126");
            dictScenPath.Add("GISS-E2-1-G_ssp245", "S17_P245");
            dictScenPath.Add("GISS-E2-1-G_ssp370", "S17_P370");
            dictScenPath.Add("GISS-E2-1-G_ssp585", "S17_P585");
            dictScenPath.Add("HadGEM3-GC31-LL_historical", "S18_OBS");
            dictScenPath.Add("HadGEM3-GC31-LL_ssp126", "S18_P126");
            dictScenPath.Add("HadGEM3-GC31-LL_ssp245", "S18_P245");
            dictScenPath.Add("HadGEM3-GC31-LL_ssp585", "S18_P585");
            dictScenPath.Add("HadGEM3-GC31-MM_historical", "S19_OBS");
            dictScenPath.Add("HadGEM3-GC31-MM_ssp126", "S19_P126");
            dictScenPath.Add("HadGEM3-GC31-MM_ssp585", "S19_P585");
            dictScenPath.Add("IITM-ESM_historical", "S20_OBS");
            dictScenPath.Add("IITM-ESM_ssp126", "S20_P126");
            dictScenPath.Add("IITM-ESM_ssp245", "S20_P245");
            dictScenPath.Add("IITM-ESM_ssp370", "S20_P370");
            dictScenPath.Add("IITM-ESM_ssp585", "S20_P585");
            dictScenPath.Add("INM-CM4-8_historical", "S21_OBS");
            dictScenPath.Add("INM-CM4-8_ssp126", "S21_P126");
            dictScenPath.Add("INM-CM4-8_ssp245", "S21_P245");
            dictScenPath.Add("INM-CM4-8_ssp370", "S21_P370");
            dictScenPath.Add("INM-CM4-8_ssp585", "S21_P585");
            dictScenPath.Add("INM-CM5-0_historical", "S22_OBS");
            dictScenPath.Add("INM-CM5-0_ssp126", "S22_P126");
            dictScenPath.Add("INM-CM5-0_ssp245", "S22_P245");
            dictScenPath.Add("INM-CM5-0_ssp370", "S22_P370");
            dictScenPath.Add("INM-CM5-0_ssp585", "S22_P585");
            dictScenPath.Add("IPSL-CM6A-LR_historical", "S23_OBS");
            dictScenPath.Add("IPSL-CM6A-LR_ssp126", "S23_P126");
            dictScenPath.Add("IPSL-CM6A-LR_ssp245", "S23_P245");
            dictScenPath.Add("IPSL-CM6A-LR_ssp370", "S23_P370");
            dictScenPath.Add("IPSL-CM6A-LR_ssp585", "S23_P585");
            dictScenPath.Add("KACE-1-0-G_historical", "S24_OBS");
            dictScenPath.Add("KACE-1-0-G_ssp126", "S24_P126");
            dictScenPath.Add("KACE-1-0-G_ssp245", "S24_P245");
            dictScenPath.Add("KACE-1-0-G_ssp370", "S24_P370");
            dictScenPath.Add("KACE-1-0-G_ssp585", "S24_P585");
            dictScenPath.Add("KIOST-ESM_historical", "S25_OBS");
            dictScenPath.Add("KIOST-ESM_ssp126", "S25_P126");
            dictScenPath.Add("KIOST-ESM_ssp245", "S25_P245");
            dictScenPath.Add("KIOST-ESM_ssp585", "S25_P585");
            dictScenPath.Add("MIROC6_historical", "S26_OBS");
            dictScenPath.Add("MIROC6_ssp126", "S26_P126");
            dictScenPath.Add("MIROC6_ssp245", "S26_P245");
            dictScenPath.Add("MIROC6_ssp370", "S26_P370");
            dictScenPath.Add("MIROC6_ssp585", "S26_P585");
            dictScenPath.Add("MIROC-ES2L_historical", "S27_OBS");
            dictScenPath.Add("MIROC-ES2L_ssp126", "S27_P126");
            dictScenPath.Add("MIROC-ES2L_ssp245", "S27_P245");
            dictScenPath.Add("MIROC-ES2L_ssp370", "S27_P370");
            dictScenPath.Add("MIROC-ES2L_ssp585", "S27_P585");
            dictScenPath.Add("MPI-ESM1-2-HR_historical", "S28_OBS");
            dictScenPath.Add("MPI-ESM1-2-HR_ssp126", "S28_P126");
            dictScenPath.Add("MPI-ESM1-2-HR_ssp245", "S28_P245");
            dictScenPath.Add("MPI-ESM1-2-HR_ssp370", "S28_P370");
            dictScenPath.Add("MPI-ESM1-2-HR_ssp585", "S28_P585");
            dictScenPath.Add("MPI-ESM1-2-LR_historical", "S29_OBS");
            dictScenPath.Add("MPI-ESM1-2-LR_ssp126", "S29_P126");
            dictScenPath.Add("MPI-ESM1-2-LR_ssp245", "S29_P245");
            dictScenPath.Add("MPI-ESM1-2-LR_ssp370", "S29_P370");
            dictScenPath.Add("MPI-ESM1-2-LR_ssp585", "S29_P585");
            dictScenPath.Add("MRI-ESM2-0_historical", "S30_OBS");
            dictScenPath.Add("MRI-ESM2-0_ssp126", "S30_P126");
            dictScenPath.Add("MRI-ESM2-0_ssp245", "S30_P245");
            dictScenPath.Add("MRI-ESM2-0_ssp370", "S30_P370");
            dictScenPath.Add("MRI-ESM2-0_ssp585", "S30_P585");
            dictScenPath.Add("NESM3_historical", "S31_OBS");
            dictScenPath.Add("NESM3_ssp126", "S31_P126");
            dictScenPath.Add("NESM3_ssp245", "S31_P245");
            dictScenPath.Add("NESM3_ssp585", "S31_P585");
            dictScenPath.Add("NorESM2-LM_historical", "S32_OBS");
            dictScenPath.Add("NorESM2-LM_ssp126", "S32_P126");
            dictScenPath.Add("NorESM2-LM_ssp245", "S32_P245");
            dictScenPath.Add("NorESM2-LM_ssp370", "S32_P370");
            dictScenPath.Add("NorESM2-LM_ssp585", "S32_P585");
            dictScenPath.Add("NorESM2-MM_historical", "S33_OBS");
            dictScenPath.Add("NorESM2-MM_ssp126", "S33_P126");
            dictScenPath.Add("NorESM2-MM_ssp245", "S33_P245");
            dictScenPath.Add("NorESM2-MM_ssp370", "S33_P370");
            dictScenPath.Add("NorESM2-MM_ssp585", "S33_P585");
            dictScenPath.Add("TaiESM1_historical", "S34_OBS");
            dictScenPath.Add("TaiESM1_ssp126", "S34_P126");
            dictScenPath.Add("TaiESM1_ssp245", "S34_P245");
            dictScenPath.Add("TaiESM1_ssp370", "S34_P370");
            dictScenPath.Add("TaiESM1_ssp585", "S34_P585");
            dictScenPath.Add("UKESM1-0-LL_historical", "S35_OBS");
            dictScenPath.Add("UKESM1-0-LL_ssp126", "S35_P126");
            dictScenPath.Add("UKESM1-0-LL_ssp245", "S35_P245");
            dictScenPath.Add("UKESM1-0-LL_ssp370", "S35_P370");
            dictScenPath.Add("UKESM1-0-LL_ssp585", "S35_P585");
        }

        private void SetupGridCoordinates()
        {
            int numx, numy;

            dictGridPts = new SortedDictionary<int, GridPoint>();
            
            numy = (int)((north - south) / 0.25) + 1;
            numx = (int)((east - west) / 0.25) + 1;
            numLat = numy;
            numLon = numx;
            numGrids = numLat * numLon;

            //set up dictionary of gridpoints
            int indx = 0;
            for (int j = 0; j < numy; j++) //for each lat
            {
                for (int k = 0;  k < numx; k++) //for each lon
                {
                    GridPoint grd = new GridPoint();

                    grd.ylat= south + j * 0.25F;
                    grd.xlon= west + k * 0.25F;
                    dictGridPts.Add(indx, grd);
                    indx++;
                }
            }

            dictSiteVars.Clear();
            for (int i = 0; i < numGrids; i++)
            {
                string sgrd = GetGridID(i);
                sgrd = i.ToString() + "_" + sgrd;
                List<string> alst = new List<string>();
                dictSiteVars.Add(sgrd, alst);
                alst = null;
            }
        }

        public void ProcessFilesToDownload()
        {
            Cursor.Current = Cursors.WaitCursor;

            DateTime dtstart = DateTime.Now;
            fMain.WriteLogFile(crlf + "Begin Download CMIP6 Data ..." +
                DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            scenarioVar = string.Empty;
            List<string> lstOfGrid = dictSiteVars.Keys.ToList();
            foreach (string climateVar in lstOfVars)
            {
                //create list of dictionary for each grid series, list is timeseries for each
                //grid point and selected variable
                varSeries = new List<SortedDictionary<DateTime, string>>();
                annSeries = new List<SortedDictionary<DateTime, string>>();
                for (int i = 0; i < numGrids; i++)
                {
                    var dict = new SortedDictionary<DateTime, string>();
                    varSeries.Add(dict);
                    var adict = new SortedDictionary<DateTime, string>();
                    annSeries.Add(adict);
                    dict = null; adict = null;
                }

                //foreach variable, download all years for that var for all
                //the selected gridpoints in dictGridPts
                switch (climateVar)
                {
                    case "PRCP": // kg m-2 s-1
                    //case "DPCP": // kg m-2 s-1
                        scenarioVar = "pr";
                        break;
                    //case "TEMP": // deg K
                    case "DTMP": // deg K
                        scenarioVar = "tas";
                        break;
                    case "TMAX": // deg K
                        scenarioVar = "tasmax";
                        break;
                    case "TMIN": // deg K
                        scenarioVar = "tasmin";
                        break;
                    //case "WIND": //m s-1
                    case "DWIN": //m s-1
                        scenarioVar = "sfcWind";
                        break;
                    //case "HUMI":  // %
                    case "DHUM":  // %
                        scenarioVar = "hurs";
                        break;
                    //case "SOLR":  // W m-2
                    case "DSOL":  // W m-2
                        scenarioVar = "rsds";
                        break;
                    //case "LRAD":  // W m-2
                    case "DLWR":  // W m-2
                        scenarioVar = "rlds";
                        break;
                }

                //scenarioVar is the climate scenario variable name like pr, tas etc
                //climateVar is the daily WDM variable name like PRCP, TEMP etc
                if (!string.IsNullOrEmpty(scenarioVar))
                {
                    string series = scenario + "_" + pathway + "_" + scenarioVar+"_"+begyr;
                    if (CheckIfSeriesAvailable(series))
                    {
                        DownloadScenarioFiles(climateVar);
                        lstDownloadedVars.Add(climateVar);

                        //upload all grid series for variable to wdm
                        dictScenSeries = new Dictionary<string, SortedDictionary<DateTime, string>>();
                        for (int i = 0; i < numGrids; i++)
                        {
                            var dict = varSeries.ElementAt(i);
                            dictScenSeries.Add(climateVar, dict);
                            string site = GetGridID(i);
                            UploadToWDM(WdmFile, site, dictScenSeries,"Day");
                            dictScenSeries.Clear();

                            // annual series
                            dict = annSeries.ElementAt(i);
                            dictScenSeries.Add(climateVar, dict);
                            UploadToWDM(AnnWdmFile, site, dictScenSeries,"Year");
                            dictScenSeries.Clear();

#if debug
                            //debugAnnual(climateVar, dict, site);
#endif
                            SetSiteVars(i, climateVar, lstOfGrid);
                        }
                        dictScenSeries = null;
                    }
                }
                varSeries = null;
            }
            // end loop for
            // 
            TimeSpan td = DateTime.Now - dtstart;
            fMain.WriteLogFile("End Download CMIP6 Data: " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.");

            fMain.pBar.Visible = false;

            ShowDataTable();

            Cursor.Current = Cursors.Default;
        }

        private bool CheckIfSeriesAvailable(string skey)
        {
            bool isAvailable = false;
            List<string> lst;
            if (dictCatalog.TryGetValue(skey, out lst))
                isAvailable=true;
            return isAvailable;
        }

        private void SetSiteVars(int index, string climateVar,List<string>lstOfGrid)
        {
            List<string> svarlst;
            foreach (string item in lstOfGrid)
            {
                string[] arr = item.Split('_');
                if (Convert.ToInt32(arr[0]) == index)
                {
                    dictSiteVars.TryGetValue(item, out svarlst);
                    svarlst.Add(climateVar);
                }
            }
        }

        private string GetGridID(int igrd)
        {
            string ID = string.Empty;
            GridPoint grd;

            dictGridPts.TryGetValue(igrd, out grd);
            double lat = grd.ylat;
            double lon = grd.xlon;

            double mnlon = -179.875, mxlon = 179.875;
            double mnlat = -59.875, mxlat = 89.875;

            //for x and y labels
            double xid = 1439.0 * (lon - mnlon) / (mxlon - mnlon);
            double yid = 599.0 * (lat - mnlat) / (mxlat - mnlat);

            if (xid < 1000)
                ID = "C0" + (Convert.ToInt32(xid)).ToString() + (Convert.ToInt32(yid)).ToString();
            else
                ID = "C" + (Convert.ToInt32(xid)).ToString() + (Convert.ToInt32(yid)).ToString();

            Debug.WriteLine("Grid = " + igrd.ToString() + ", ID=" + ID);
            return ID;
        }

        private bool DownloadScenarioFiles(string climateVar)
        {
            Cursor.Current = Cursors.WaitCursor;

            bool isValid = false;
            List<string> aList;
            string scenfile;
            string bbox = "_N" + north.ToString() + "_W" + west.ToString() +
                   "_E" + east.ToString() + "_S" + south.ToString();

            //setup output files and write header
            outfile = scenario + "_" + pathway + "_" + climateVar + bbox+ ".txt";
            outfile = Path.Combine(savepath, outfile);
            outsw = File.CreateText(outfile);
            outsw.AutoFlush = true;
            WriteFileHeader(outsw);

            try
            {
                //download file for each year
                for (int yr = begyr; yr <= endyr; yr++)
                {
                    iterNum++;
                    UpdateProgress(iterNum);

                    curyear = yr;
                    string skey = scenario + "_"+ pathway + "_"+ scenarioVar + "_" + yr;

                    if (dictCatalog.TryGetValue(skey, out aList))
                    {
                        variant = aList[0];
                        scenfile = aList[1];

                        ncdffile = Path.GetFileNameWithoutExtension(scenfile) + bbox +
                            Path.GetExtension(scenfile);
                        savefile = Path.Combine(savepath, ncdffile);
                        URLpath = GenerateURL(yr, variant, scenfile, scenarioVar, scenario, pathway);

                        //download the climate file
                        WriteStatus("Downloading " + scenfile + "...");

                        if (!DownloadCMIPFile(URLpath, savefile)) continue;
                        else
                        {
                            ReadDownloadedFile(savefile);
                            isValid = true;
                        }
                    }
                    else
                    {
                        string msg = "Error getting file for " + skey + "!";
                        MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                    //iterNum++;
                    //UpdateProgress(iterNum);
                }

                outsw.Flush();
                outsw.Close();outsw = null;
                WriteStatus("Ready ...");
                
                Cursor.Current = Cursors.Default;
                return isValid;
            }
            catch (Exception ex)
            {
                string sfile = Path.GetFileName(savefile);
                string msg = "Error downloading " + sfile + "!" + crlf + crlf + ex.Message +
                    crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        private void UpdateProgress(int iter)
        {
            fMain.pBar.Value = (int)(iter * 100 / maxIter);
            fMain.statusStrip.Refresh();
        }

        private string GenerateURL(int year, string variant, string sfile,
                                    string svar, string scen, string pathway)
        {
            StringBuilder st = new StringBuilder();
            st.Append(scen + "/" + pathway + "/" + variant + "/" + svar + "/");
            st.Append(sfile + "?");
            st.Append("var=" + svar + "&north=" + (north+0.0625F).ToString());
            st.Append("&west=" + (west - 0.0625F).ToString() + "&east=" + (east + 0.0625F).ToString() 
                + "&south=" + (south - 0.0625F).ToString());
            st.Append("&disableProjSubset=on&horizStride=1&");
            st.Append("time_start=" + year.ToString() + "-01-01T12%3A00%3A00Z&");
            st.Append("time_end=" + year.ToString() + "-12-31T12%3A00%3A00Z&");
            st.Append("timeStride=1&addLatLon=true");
            Debug.WriteLine(st.ToString());
            return URLbase + st.ToString();
        }

        public bool DownloadCMIPFile(string _URL, string _filename)
        {
            bool is_downloaded = false;
            try
            {
                D4EM.Data.Download.DisableHttpsCertificateCheck();
                D4EM.Data.Download.SetSecurityProtocol();

                //download if not yet downloaded
                if (!File.Exists(_filename))
                    is_downloaded = D4EM.Data.Download.DownloadURL(_URL, _filename);

                //delete xml and cdl files
                //string cfile = Path.ChangeExtension(_filename, "cdl");
                //if (File.Exists(cfile)) File.Delete(cfile);
                string xfile = _filename + ".xml";
                if (File.Exists(xfile)) File.Delete(xfile);
            }
            catch (Exception ex)
            {
                errmsg = "Problem downloading timeseries! Server maybe down.";
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private bool ReadDownloadedFile(string ncdfile)
        {
            int len = numLon * 20;
            try
            {
                string cdlfile = Path.GetFileNameWithoutExtension(ncdfile) + ".cdl";
                cdlfile = Path.Combine(savepath, cdlfile);

                StreamWriter sw = File.CreateText(cdlfile);
                sw.AutoFlush = true;

                //use ncdump.exe to convert ncdf file to a text file
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(ncdumpPath, "ncdump.exe"),
                        Arguments = "-l" + len.ToString() + " -v " + scenarioVar + " " + ncdfile,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                int numlines = 0;
                string[] token;
                char[] sep = { '=', ';',',' };

                List<string> cdldata = new List<string>();
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine().Trim();
                    token = line.Split(sep);
                    
                    double sdat;
                    if (double.TryParse(token[0].ToString(), out sdat )) 
                    {
                        //remove trailing ;
                        if (line.Contains(';'))
                            line = line.Replace(';', ',');
                        sw.WriteLine(line.Trim());
                        cdldata.Add(line.Trim());
                        numlines++;
                    }
                }
#if debug
        Debug.WriteLine("Number of Lines process = " + numlines.ToString());
#endif
                process.WaitForExit();
                
                sw.Close();
                sw = null;
                process = null;

                //delete cdlfile
                if (File.Exists(cdlfile)) File.Delete(cdlfile);

                //Process the downloaded data stored in a list by gridpoint
                if (cdldata.Count>0)
                {
                    ProcessDownloadedData(cdldata, numLat, numLon);
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                errmsg = "Error executing ncdump.exe!" + crlf + crlf + ex.Message + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        /// <summary>
        /// ProcessDownloadedData
        /// Processes the cdldata and saves it to a list of timeseries dictionaries
        /// Processes one year of data
        /// </summary>
        /// <param name="cdldata"></param>
        /// <param name="numY"></param>
        /// <param name="numX"></param>
        private void ProcessDownloadedData(List<string> cdldata, int numY, int numX)
        {
            //revised version
            try
            {
                string[] sdata;
                int icnt = 0, indx = 0;
                int numdays = 0, nummiss = 0;
                DateTime dt;
                //for annual means
                float[] annual = new float[numX*numY];
                for (int i = 0; i < numX * numY; i++) annual[i] = 0.0F;

                //cdldata.count is num of items in cdldata list
                //each line is all longitude for each latitude and day
                int numIter = cdldata.Count / numY;
                numdays = curyear%4>0 ? 365 : 366;
                nummiss = numdays - numIter;

                dt = new DateTime(curyear, 1, 1, 0, 0, 0);
                for (int i = 1; i <= numIter; i++) //for each day
                {
                    dt = dt.AddDays(1);
                    indx = 0;
                    for (int j = 0; j < numY; j++)          //for each lat
                    {
                        sdata = cdldata[icnt].Split(',');
                        for (int k = 0; k < numX; k++)      // for each long
                        {
                            double dvar;
                            double.TryParse(sdata[k].Trim(), out dvar);
                            if (scenarioVar=="pr")
                                dvar = dvar * 86400.0/25.4; // in/day
                            else if(scenarioVar.Contains("tas"))
                                dvar = 1.8*dvar -459.67;    // deg F
                            else if (scenarioVar=="sfcWind")
                                dvar = dvar *2.23694;       // mph
                            else if (scenarioVar == "rlds")
                                dvar = dvar / 0.484583;       // ly
                            else if (scenarioVar == "rsds")
                                dvar = dvar / 0.484583;       // ly

                            //mean annual 
                            if (scenarioVar == "pr")
                                annual[indx] += (float)(dvar);
                            else
                                annual[indx] += (float)(dvar / numIter);

                            //all others same units
                            //vardata[indx].Add(dvar.ToString("0.00000"));
                            //get corresponding dictionary for the list element
                            var dict = varSeries.ElementAt(indx);
                            if (scenarioVar == "pr")
                                dict.Add(dt, (dvar.ToString("F4")));
                            else if (scenarioVar.Contains("tas"))
                                dict.Add(dt, (dvar.ToString("F2")));
                            else 
                                dict.Add(dt, (dvar.ToString("F3")));

                            indx++;
                        }
                        icnt++;
                    }
                }

                //add annual to list 
                DateTime ydt = new DateTime(curyear+1, 1, 1, 0, 0, 0);
                for (int i = 0; i < numX * numY; i++)
                {
                    var adict = annSeries.ElementAt(i);
                    adict.Add(ydt.AddDays(1), annual[i].ToString("F4"));
                    adict = null;
                }

                //writeout data for all grids
                dt = new DateTime(curyear, 1 ,1 , 0, 0, 0);
                int ndx = numX * numY;
                for (int i = 0; i < numIter; i++)
                {
                    dt = dt.AddDays(1);
                    string var = string.Empty;
                    var sdat = string.Empty;
                    for (int j = 0; j < ndx; j++)
                    {
                        sdat = varSeries.ElementAt(j)[dt].ToString();
                        var = var + sdat + ",";
                    }
                    //sdat = varSeries.ElementAt(ndx - 1)[dt].ToString();
                    //var = var + sdat;

                    outsw.WriteLine(dt.AddDays(-1).ToString()+","+var.ToString());
                    //outsw.WriteLine(dt.ToString() + "," + var.ToString());
                    outsw.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "/r/n" + ex.StackTrace);
                fMain.WriteLogFile("Processing Error!"+crlf+ex.Message + "/r/n" + ex.StackTrace);
            }
        }

        private bool UploadToWDM(string wdmfile, string site, 
            Dictionary<string, SortedDictionary<DateTime, string>> dictSiteWea, string tstep)
        {
            try
            {
#if debug
                Debug.WriteLine("Entering UploadToWDM ...");
                Debug.WriteLine("Site = " + site);
#endif
                List<string> siteAttrib = new List<string>();
                bool isSite = fMain.dictSta.TryGetValue(site, out siteAttrib);
                Debug.WriteLine("In UploadtoWDM IsSite = " + isSite.ToString());

                //add attributes for CMIP6, 5-scenario, 6-pathway, 7-scenpath               
                siteAttrib[5] = scenario;
                siteAttrib[6] = pathway;
                siteAttrib[7] = scenpath;
#if debug
                //Debug.WriteLine(siteAttrib[0].ToString()+", "+
                //siteAttrib[5].ToString()+", "+siteAttrib[6].ToString()+", "+ siteAttrib[7].ToString());
#endif
                clsWdm cWDM = new clsWdm(fMain.wrlog, site, siteAttrib,
                                    dictSiteWea, wdmfile, fMain.optDataSource, tstep);
                cWDM.UploadSeriesToWDM(site, wdmfile);
                cWDM = null;
                siteAttrib = null;
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error in uploading timeseries for site:" + site;
                ShowError(msg, ex);
                return false;
            }

        }

        private void WriteFileHeader(StreamWriter sw)
        {
            string lat = string.Empty, lon = string.Empty;
            foreach (KeyValuePair<int, GridPoint> kv in dictGridPts)
            {
                lat = lat + (kv.Value.ylat).ToString("F3") + ",";
                lon = lon + (kv.Value.xlon).ToString("F3") + ",";
            }
            sw.WriteLine(lat.ToString());
            sw.WriteLine(lon.ToString());
            sw.Flush();
        }

        private void WriteStatus(string msg)
        {
            Debug.WriteLine(msg);
            slabel.Text = msg;
            strip.Refresh();
        }

        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + crlf + ex.Message + crlf + crlf + ex.StackTrace;
            fMain.WriteLogFile(msg);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowDataTable()
        {
            {
                frmDataCMIP6 fdataCMIP = new frmDataCMIP6(fMain, dictSiteVars, lstOfVars, null, scenario,pathway);
                if (fdataCMIP.ShowDialog() == DialogResult.OK)
                {
                    fdataCMIP = null;
                    fdataCMIP.Dispose();
                }
            }
        }
        
        private void debugAnnual(string svar, SortedDictionary<DateTime, string> dict, string site)
        {
            foreach (KeyValuePair<DateTime, string> kv in dict)
            {
                Console.WriteLine("{0}:{1}, Year={2}, Annual={3}",site, svar, kv.Key.ToString(), kv.Value.ToString());
            }
        }

        private void debugScenario()
        {
            Debug.WriteLine("Entering clsCMIG6");
            Debug.WriteLine("{0},{1},{2},{3}", north.ToString(), south.ToString(),
                west.ToString(), east.ToString());
            Debug.WriteLine("Scenario=" + scenario);
            Debug.WriteLine("Pathway=" + pathway);
            Debug.WriteLine("Scenario_Pathway=" + scenpath);
            Debug.WriteLine("BegYear=" + begyr.ToString());
            Debug.WriteLine("EndYear=" + endyr.ToString());

            foreach (KeyValuePair<int, GridPoint> kv in dictGridPts)
            {
                GridPoint grd;
                dictGridPts.TryGetValue(kv.Key, out grd);
                Debug.WriteLine("Ylat= {0}, Xlon= {1}",
                         grd.ylat.ToString("F3"), grd.xlon.ToString("F3"));
            }
        }
    }
}
