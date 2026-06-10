#define debug
//#undef debug
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
using atcData;
using static atcUtility.modDate;
using atcWDM;
using LSPCWeaComp;

namespace NCEIData
{
    public class clsEDDE
    {
        private frmMain fMain;
        private CMIP6Series cmipSeries;
        private SortedDictionary<int, GridPoint> dictGridPts;
        private SortedDictionary<string, MetGages> dictWeaGrid;
        private SortedDictionary<string, List<string>> dictSiteVars;
        private SortedDictionary<string, SortedDictionary<DateTime, string>> dictWeaData; 

        private SpatialStatusStrip strip;
        private ToolStripStatusLabel slabel;

        private string savepath, savefile, ncdffile, outfile;
        private string URLpath, cacheDir, ncdumpPath;
        private StreamWriter outsw;
        private StreamReader outrw;

        //bounding box
        private float north, south, east, west;
        private string scenario, pathway, scenarioVar, eddeVar;
        private string scenpath = string.Empty;
        private List<string> lstOfVars, lstOfGrids;
        private float minLat, minLon, maxLat, maxLon;
        private int numLat, numLon, numGrids,numMissing;
        private int numVars, numYrs, numFiles, iterNum = 0, maxIter = 0;
        private int curyear, curmon;

        //period of record
        private int begyr, endyr;
        private DateTime begDate, endDate, curdate;
        private int[] mdays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        private string URLbase = "https://epa-edde.S3.amazonaws.com/EDDE_V1/hourly/";
        private string crlf = Environment.NewLine;
        private string errmsg;

        private Dictionary<string, SortedDictionary<DateTime, string>> dictScenSeries;
        private SortedDictionary<int, List<int>> dictGridIndex;
        private List<int> lstlat = new List<int>();
        private SortedDictionary<string, string> dictScenPath;
        private string WdmFile, AnnWdmFile;
        private bool isValidScenPath = false;
        private List<string> lstDownloadedVars = new List<string>();
        private int tzone = 0;
        private string bbox = string.Empty;
            
        public clsEDDE(frmMain _fmain, CMIP6Series _cmipSeries)
        {
            fMain = _fmain;
            AnnWdmFile = fMain.AnnWdmFile;
            WdmFile = _fmain.WdmFile;
            dictSiteVars = _fmain.dictSiteVars;
            cmipSeries = _cmipSeries;
            strip = _fmain.statusStrip;
            slabel = _fmain.statuslbl;
            cacheDir = _fmain.cacheDir;
            savepath = Path.Combine(cacheDir, "EDDE");
            ncdumpPath = Path.Combine(Application.StartupPath, "netcdf");

            //get EDDE parameters
            lstOfVars = cmipSeries.ClimateVar;
            lstOfGrids = fMain.dictSelSites.Keys.ToList();
            cmipSeries.ClimateGrid = lstOfGrids;
            scenario = cmipSeries.Scenario;
            pathway = cmipSeries.Pathway;
            begyr = cmipSeries.BeginYear;
            endyr = cmipSeries.EndYear;
            begDate = new DateTime(begyr, 1, 1, 0, 0, 0);
            endDate = new DateTime(endyr, 12, 31, 23, 0, 0);
            numVars = lstOfVars.Count;
            numYrs = (endyr - begyr) + 1;
            numFiles = numVars * numYrs*12; //each file is a month
            maxIter = numFiles;

            //max and min lat-lon of the selected grid, to get bounding box
            //not really needed, grid not rectangular

            north = cmipSeries.GridBox.North;
            south = cmipSeries.GridBox.South;
            west = cmipSeries.GridBox.West;
            east = cmipSeries.GridBox.East;

            bbox = west.ToString("F4") + "_" + east.ToString("F4") + "_" +
                south.ToString("F4") + "_" + north.ToString("F4");
            
#if debug
            Debug.WriteLine("In clsEDDE = " + bbox);
            Debug.WriteLine("Scenario = " + scenario);
            Debug.WriteLine("Scenario = " + pathway);
#endif
            ScenarioMapping();
            SetupGridVars();
            SetupGridIndices();

            //combine scenario 
            isValidScenPath = dictScenPath.TryGetValue(scenario + "_" + pathway, out scenpath);

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

        private void SetupGridIndices()
        {
            //creates dictionary of grid indices
            //key = latitude, value = list of longitude
            dictGridIndex = new SortedDictionary<int, List<int>>();
            foreach (string grid in lstOfGrids)
            {
                int xIndex = Convert.ToInt32(grid.Substring(1, 3));
                int yIndex = Convert.ToInt32(grid.Substring(4, 3));
                if (!dictGridIndex.ContainsKey(yIndex))
                {
                    List<int> xlist = new List<int>();
                    xlist.Add(xIndex);
                    dictGridIndex.Add(yIndex, xlist);
                    xlist = null;
                }
                else
                {
                    List<int> xlist;
                    dictGridIndex.TryGetValue(yIndex, out xlist);
                    xlist.Add(xIndex);
                    xlist = null;
                }
            }

            lstlat = dictGridIndex.Keys.ToList();
#if debug
            foreach (KeyValuePair<int,List<int>> kv in dictGridIndex)
            {
                List<int> val = kv.Value;
                foreach (int ival in val)
                {
                    Debug.WriteLine("ylat = " + kv.Key.ToString()+" xlon = "+ival.ToString());
                }
            }
#endif
        }

        private void ScenarioMapping()
        {
            dictScenPath = new SortedDictionary<string, string>();
            dictScenPath.Add("CESM_Historical", "CESM_OBS");
            dictScenPath.Add("CESM_RCP8.5", "CESM_R85");
            dictScenPath.Add("GFDL-CM3_Historical", "GFDL_OBS");
            dictScenPath.Add("GFDL-CM3_RCP8.5", "GFDL_R85");
        }

        private void SetupGridVars()
        {
            dictSiteVars.Clear();
            //Debug.WriteLine("In SetupGridVars : " + string.Join(",", lstOfGrids));
            foreach (string sgrd in lstOfGrids)
            {
                List<string> alst = new List<string>();
                dictSiteVars.Add(sgrd, alst);
                alst = null;
            }
        }

        public void ProcessFilesToDownload()
        {
            Cursor.Current = Cursors.WaitCursor;
            DateTime dtstart = DateTime.Now;
            fMain.WriteLogFile(crlf + "Begin Download EDDE Data ..." +
                DateTime.Now.ToShortDateString() + "  " +
                DateTime.Now.ToLongTimeString());

            scenarioVar = string.Empty;
            eddeVar = string.Empty;
            numGrids = lstOfGrids.Count;
            
            foreach (string climateVar in lstOfVars)
            {
                //use this instead of the list varSeries
                dictWeaData = new SortedDictionary<string, SortedDictionary<DateTime, string>>();                
                foreach (string sgrid in lstOfGrids)
                {
                    var wseries = new SortedDictionary<DateTime, string>();
                    dictWeaData.Add(sgrid, wseries);
                    wseries = null;
                }

                //foreach variable, download all years for that var
                //climateVar is the wdm variable constituent, need the EDDE file variable in netcdf file
                scenarioVar = string.Empty;
                switch (climateVar)
                {
                    case "PREC": // kg m-2 s-1
                        scenarioVar = "pr";
                        eddeVar = "PRECIP";
                        break;
                    case "ATEM": // deg K
                        scenarioVar = "ts";
                        eddeVar = "T2";
                        break;
                    case "WNDD": // deg, wind dir from
                        scenarioVar = "wdirs";
                        eddeVar = "WDIR10";
                        break;
                    case "WIND": //m s-1
                        scenarioVar = "wspds";
                        eddeVar = "WSPD10";
                        break;
                    case "DEWP":  // %
                        scenarioVar = "td";
                        eddeVar = "DEWPT";
                        break;
                    case "HUMI": // % 
                        scenarioVar = "hur";
                        eddeVar = "RH";
                        break;
                    case "ATMP": //mbar
                        scenarioVar = "ps";
                        eddeVar = "PSFC";
                        break;
                    case "SOLR":  // W m-2
                        scenarioVar = "rsds";
                        eddeVar = "SWDNB";
                        break;
                    case "CLOU":  // decimal (0-1)
                        scenarioVar = "clt";
                        eddeVar = "CLDF";
                        break;
                    case "LRAD":  // W m-2
                        scenarioVar = "rlds";
                        eddeVar = "LWDNB";
                        break;
                    case "LWUP":  // W m-2
                        scenarioVar = "rlut";
                        eddeVar = "LWUPT";
                        break;
                    case "HFSS":  // W m-2
                        scenarioVar = "hfss";
                        eddeVar = "HFX";
                        break;
                    case "HFLS":  // W m-2
                        scenarioVar = "hfls";
                        eddeVar = "LH";
                        break;
                }
#if debug
                Debug.WriteLine("Scenario Var = " + scenarioVar);
                Debug.WriteLine("EDDE Var = " + eddeVar);
#endif
                //scenarioVar is the climate scenario variable name like pr, ts etc
                //climateVar is the WDM variable name like PREC, ATEM etc (wdm constituent)
                if (!string.IsNullOrEmpty(scenarioVar))
                {
                    //if output file does not exist, download and process scenario file
                    //else process the output file (*.txt)
                    lstDownloadedVars.Add(climateVar);
                    if (!OutputFileExists(climateVar)) 
                        DownloadScenarioFiles(climateVar);
                    else
                    {
                        WriteStatus("Processing " + outfile + "...");
                        if (!ReadProcessOutputFile()) return;
                    }

                    //upload all grid series for variable to wdm
                    dictScenSeries = new Dictionary<string, SortedDictionary<DateTime, string>>();
                    foreach (KeyValuePair<string, SortedDictionary<DateTime, string>> kv in dictWeaData)
                    {
                        string site = kv.Key.Trim();
                        var dict = kv.Value;
                        dictScenSeries.Add(climateVar, dict);

#if debug
                        for(int i=0;i<30;i++)
                        {
                            Debug.WriteLine("{0},{1},{2}", site.Trim(), dict.Keys.ElementAt(i).ToString(),
                                dict.Values.ElementAt(i).ToString());
                        }
#endif
                        UploadToWDM(WdmFile, site, dictScenSeries, "Hour");
                        dictScenSeries.Clear();
                        SetSiteVars(site, climateVar);

                    }//end foreach site and timeseries
                    dictScenSeries = null;
                    dictWeaData = null;
                }
            }
            // end loop for lstOfVars
            
            TimeSpan td = DateTime.Now - dtstart;
            fMain.WriteLogFile("End Download EDDE Data: " +
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ... " +
                td.TotalMinutes.ToString("F4") + " minutes.");

            //additional calculations
            //calculate TMAX and TMIN from ATEM, PEVT from TMAX,TMIN using Hamon
            List<string> varlist;
            foreach (KeyValuePair<string, List<string>>kv in dictSiteVars)
            {
                string site = kv.Key;
                dictSiteVars.TryGetValue(site, out varlist);
                Debug.WriteLine("site=" + site + "vars=" + string.Join(",", varlist));
                foreach (string svar in varlist)
                {
                    if (svar.Contains("ATEM"))
                    {
                        CalculateMaxMinTemp(WdmFile, site, svar, scenpath);
                        CalculateHamonPET(WdmFile, site, scenpath);
                    }
                }
            }

            //annual calculation
            foreach (KeyValuePair<string, List<string>> kv in dictSiteVars)
            {
                string site = kv.Key;
                dictSiteVars.TryGetValue(site, out varlist);
                foreach (string svar in varlist)
                    CalculateAnnual(WdmFile, AnnWdmFile, site, svar, scenpath);
            }

            fMain.dictSiteVars = dictSiteVars;
            fMain.pBar.Visible = false;
            ShowDataTable();

            Cursor.Current = Cursors.Default;
        }

        private bool OutputFileExists(string climateVar)
        {
            //setup output files and write header if it does not exist
            //else return false
            outfile = scenario + "_" + pathway + "_" + climateVar + "_" +
                         begyr.ToString() + "_" + endyr.ToString() +
                         "_" + bbox + ".txt";
            outfile = Path.Combine(savepath, outfile);
            if (!File.Exists(outfile))
            {
                outsw = File.CreateText(outfile);
                outsw.AutoFlush = true;
                WriteFileHeader(outsw);
                return false;
            }
            else
                return true;
        }

        private bool ReadProcessOutputFile()
        {
            string[] sdata, sgrid, sval;
            string avalue = string.Empty;
            DateTime dt;
            int numgrid = 0;
            maxIter = numFiles;

            try
            {
                sdata = File.ReadAllLines(outfile);
                int numlines = sdata.Count();
                maxIter = numlines;

                Debug.WriteLine("Numlines = " + sdata.Count().ToString());
                Debug.WriteLine("Grid  = " + sdata.ElementAt(0).ToString());
                sgrid = sdata.ElementAt(0).Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
                numgrid = sgrid.Count();

                dictWeaData.Clear();
                SortedDictionary<DateTime,string>[] gridSeries = new SortedDictionary<DateTime, string>[numgrid];
                for(int j = 0; j < sgrid.Count(); j++)
                {
                    //var wseries = new SortedDictionary<DateTime, string>();
                    //gridSeries.Add(wseries);
                    //wseries = null;
                    gridSeries[j] = new SortedDictionary<DateTime, string>();
                }

                for (int i=1; i<numlines; i++)
                {
                    sval = sdata.ElementAt(i).Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
                    //Debug.WriteLine("Values = " + string.Join(",",sval));
                    
                    for (int j=0; j<numgrid;j++)
                    {
                        //var dict=gridSeries.ElementAt(j);
                        dt = Convert.ToDateTime(sval[0]);
                        avalue = sval[j + 1];
                        //Debug.WriteLine(i.ToString()+","+(j+1).ToString() + "," + dt.ToString() + "," + avalue.ToString());
                        gridSeries[j].Add(dt, avalue);
                    }
                    UpdateProgress(i);
                }

#if debug
                for (int i = 0; i < 30; i++)
                {
                    List<string> lstval = new List<string>();
                    for (int j = 0; j < numgrid; j++)
                        lstval.Add(gridSeries[j].Values.ElementAt(i));
                    Debug.WriteLine(gridSeries[0].Keys.ElementAt(i).ToString(), string.Join(",", lstval));
                    lstval = null;
                }
#endif
                //add to dictWeaData dictionary
                for (int j = 0; j < numgrid; j++)
                {
                    //var dict = gridSeries.ElementAt(j);
                    string site = sgrid.ElementAt(j);
                    dictWeaData.Add(site, gridSeries[j]);
                    //dict = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                errmsg = "Error reading and processing "+outfile+"!" + crlf + crlf + ex.Message + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetSiteVars(string site, string climateVar)
        {
            List<string> svarlst;
            dictSiteVars.TryGetValue(site, out svarlst);
            svarlst.Add(climateVar);
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
            string scenfile;
            string scurmo, scuryr;
            string scen, rcp;
            int numdays=0;
            maxIter = numFiles;

            try
            {
                //scenario is CESM, GFDL-CM3
                //pathway is RCP8.5 and Historical
                scen = scenario.Contains("CESM") ? "cesm" : "gfdl";
                rcp = pathway.Contains("RCP8.5") ? "rcp85" : "hist";

                //download file for each year, month, day and hour
                //EDDE file is by year and month for whole CONUS

                curdate = begDate.AddHours(-1);
                for (int yr = begyr; yr <= endyr; yr++)
                {
                    mdays[1] = (yr % 4) > 0 ? 28 : 29;
                    curyear = yr;
                    scuryr = yr.ToString();
                    for ( int mon = 1;  mon <= 12 ; mon++ )
                    {
                        numdays = mdays[mon - 1]; //array is 0-based
                        curmon = mon;
                        scurmo = mon.ToString("D2");
                        iterNum++;
                        UpdateProgress(iterNum);

                        //sample for 1hr
                        //hur.rcp85.cesm.EDDE-WRF.1hr.NA-36.2025-01.raw.nc
                        //hur.rcp85.gfdl.EDDE-WRF.1hr.NA-36.2025-01.raw.nc

                        scenfile = scenarioVar+"."+rcp+"."+scen+".EDDE-WRF.1hr.NA-36."+
                                         scuryr+"-"+scurmo+".raw.nc";

                        //ncdffile = Path.GetFileNameWithoutExtension(scenfile) + 
                        //    Path.GetExtension(scenfile);
                        ncdffile = scenfile;
                        savefile = Path.Combine(savepath, ncdffile);
                        URLpath = GenerateURL(scuryr, scenfile, scenario, pathway);

                        //download the climate file
                        WriteStatus("Downloading " + scenfile + "...");

                        if (!DownloadEDDEFile(URLpath, savefile)) 
                            continue;
                        else
                        {
                            WriteStatus("Processing " + scenfile + "...");
                            ReadDownloadedFile(savefile, numdays, curyear, curmon);
                            isValid = true;
                        }
                    }
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

        private string GenerateURL(string year, string sfile, string scen, string pathway)
        {
            string period = pathway.Contains("RCP8.5") ? "/2025-2100/" : "/1995-2005/";
            StringBuilder st = new StringBuilder();
            st.Append("WRF-" + scen+"/"+pathway + period+year + "/" );
            st.Append(sfile);
            Debug.WriteLine(st.ToString());
            return URLbase + st.ToString();
        }

        public bool DownloadEDDEFile(string _URL, string _filename)
        {
            bool isdownloaded = false;
            try
            {
                D4EM.Data.Download.DisableHttpsCertificateCheck();
                D4EM.Data.Download.SetSecurityProtocol();

                //download if not yet downloaded
                if (!File.Exists(_filename))
                    isdownloaded = D4EM.Data.Download.DownloadURL(_URL, _filename);

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

        private bool ReadDownloadedFile(string ncdfile, int numdays, int curyear, int curmon)
        {
            //downloaded file is one month of data for all CONUS grids,
            //need to process only the selected grids, latlon combination, 
            //does not consider leap years, all feb are 28 days, need to adjust for leap years

            Cursor.Current = Cursors.WaitCursor;

            //data section of ncdump output
            //data section header T2 =               ...edde variable
            //dataline header      // T2(1-198 ,1,1) ... 1,1-> lat index, hr - index

            string linehead = " " + eddeVar.Trim() + " =";
            string datahead = "// " + eddeVar;
            bool dataline = false;
            string header = string.Empty;
#if debug
            //Debug.WriteLine("LineHead = " + linehead.ToString());
            //Debug.WriteLine("DataHead = " + datahead.ToString());
#endif

            int len = 4096; //string lenght to get all of 198 longitude data
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
                        Arguments = "-bf -l" + len.ToString() + " -v " + eddeVar.Trim() + " " + ncdfile,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                int numlines = 0;
                string sdata = string.Empty;
                string[] token, vardata;
                char[] sep = { ',',')' };

                //process the standardoutput and data in list cdldata
                //cdldata line is - 047,1:  0, 0 (lat, cumhr, data1 ... dataN)
                List<string> cdldata = new List<string>();
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line.Contains(linehead)) dataline = true;
                    if (dataline)
                    {
                        if (!line.Contains(linehead))
                        {
                            if (line.Contains(datahead)) header = line.Trim();
                            else if (!line.Contains("}"))
                            {
                                // e.g. header = // T2(1-198 ,1,1)
                                // token[0] = // T2(1-198), token[1]=1 (lat), token[2]=1 (hr)
                                token = header.Split(sep);
                                int ylat = Convert.ToInt32(token[1]); 
                                if (lstlat.Contains(ylat))
                                {
                                    List<int> xlon;
                                    dictGridIndex.TryGetValue(ylat, out xlon);
                                    List<string> values = new List<string>();
                                    List<string> sgrid = new List<string>();

                                    vardata = line.Split(','); //0-indexed
                                    
                                    foreach (int lon in xlon)
                                    {
                                        values.Add(vardata[lon - 1]);
                                        sgrid.Add("E" + lon.ToString("D3")+ ylat.ToString("D3"));
                                    }
                                    //token[1] is the lat, token[2] is time (1-daysInmon*24)
                                    //values is list of data for the long
                                    
                                    //sw.WriteLine(token[1].ToString() + "," + token[2].ToString() + ": " + string.Join(",",values));
                                    
                                    sdata = ylat.ToString("D3") + "," + token[2].ToString() + ": " + string.Join(",", values);
                                    cdldata.Add(sdata.Trim());
                                    
                                    //sw.WriteLine(sdata.Trim());
                                    
                                    //sw.WriteLine(string.Join(",", sgrid) + "," + curdate.AddHours(Convert.ToInt32(token[2])) + ": " + string.Join(",", values));
                                    //Sample for two grids CDL file with grid and datetime
                                    //E144047, E145047,1/1/2025 12:00:00 AM:  0, 0
                                    //E144048, E145048,1/1/2025 12:00:00 AM:  0, 0

                                    values = null; sgrid = null;
                                    sw.Flush();
                                }
                                numlines++;
                            }
                        }
                    }
                }

                //adjust for leap years, centurial years (1700,1800,1900,2100 are not leap years)
                //1600, 2000 and 2400 are leap years

                if (!(curyear == 2100))
                {
                    if (curyear % 4 == 0 && curmon == 2)
                    {
                        char[] delim = { ',',':' };
                        int icnt = cdldata.Count;
                        int ibeg = icnt - (24 * numLat);

                        //Debug.WriteLine("icnt=" + icnt.ToString() + ", ibeg=" + ibeg.ToString() + ", iend=" + icnt.ToString());
                        for(int i=ibeg; i<icnt; i++)
                        {
                            List<string> svals = new List<string>();
                            string sdat = cdldata.ElementAt(i);
                            string[] arr = sdat.Split(delim);
                            sdata = arr[0].ToString() + "," + (Convert.ToInt32(arr[1]) + 24).ToString()+ ", ";
                            for (int j = 2; j < arr.Count(); j++)
                                svals.Add(arr[j].ToString());
                            sdata = sdata + string.Join(",", svals);
                            svals = null;

                            cdldata.Add(sdata.Trim());
                        
                            //sw.WriteLine(sdata.Trim());
                        }
                        sw.Flush();
                        numlines += 24 * numLat;
                    }
                }

                numMissing = 126 * numdays * 24 - numlines;
                process.WaitForExit();
                
                sw.Close();
                sw = null;
                process = null;

                //delete cdlfile and netcdf file, only for debugging
                if (File.Exists(cdlfile)) File.Delete(cdlfile);
                if (File.Exists(ncdfile)) File.Delete(ncdfile);

                //Process the downloaded data stored in a list by gridpoint
                numLat = lstlat.Count;
                if (cdldata.Count > 0)
                {
                    ProcessDownloadedData(cdldata, numLat, numLon);
                    cdldata = null;
                    Cursor.Current = Cursors.Default;
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
        
        private void ProcessDownloadedData(List<string> cdldata, int numlat, int numlon)
        {
        /// <summary>
        /// ProcessDownloadedData
        /// Processes the cdldata and saves it to a dictionary of timeseries dictionaries
        /// Processes one month of data for the selected grid
        /// </summary>
        /// <param name="cdldata"></param>
        /// <param name="numY"></param>
        /// <param name="numX"></param>
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string[] sdata;
                int icnt = 0, xlon=0, ylat=0;
                int numdays = 0, nummiss = 0;
                int hrsprocessed = 0;
                DateTime dt, begdt;
                string sgrid, slon, slat;
                
                //cdldata.count is num of items in cdldata list
                //each line is data for all longitude for each latitude and hour,
                //there are n lines for numlat latitudes
                //i.e   047,1:  0, 0 lat, hour of month, 1..n data for each long
                //      048,1:  0, 0

                int numIter = cdldata.Count / numlat;
                int numhrs = cdldata.Count / numlat;

                Debug.WriteLine("In ProcessDownloadedData, numhrs = " + numhrs.ToString());
                hrsprocessed = 0;
                begdt = new DateTime(curyear, curmon, 1, 0, 0, 0).AddHours(-1);
                dt = new DateTime(curyear, curmon, 1, 0, 0, 0);

                for (int i = 0; i < numhrs; i++)      //for each hour
                {
                    for (int j = 0; j < numlat; j++)  //for each lat
                    {
                        //sdata[0] - lat, sdata[1] - hrs , sdata[2...n] - data
                        sdata = cdldata[icnt].Split(',',':');
                        ylat = Convert.ToInt32(sdata[0]);
                        //dt = begdt.AddHours(Convert.ToInt32(sdata[1]));

                        List<int> lstlon;
                        dictGridIndex.TryGetValue(ylat, out lstlon);

                        for (int k = 0; k < lstlon.Count; k++)      // for each long
                        {
                            xlon = lstlon[k];
                            sgrid = "E" + xlon.ToString("D3") + ylat.ToString("D3");

                            float dvar, miss=-1e20f;
                            float.TryParse(sdata[k+2].Trim(), out dvar);
                            
                            if (dvar <= miss) dvar = -9999F;
                            else
                            {
                                if (scenarioVar == "pr")
                                    dvar = dvar / 25.4F;             // in/hr
                                else if (scenarioVar.Contains("ts"))
                                    dvar = (float)(1.8 * (dvar - 273.15F) + 32F); // temp, deg F
                                else if (scenarioVar.Contains("td"))
                                    dvar = (float)(1.8 * (dvar - 273.15F) + 32F); // dewpoint, deg F
                                else if (scenarioVar == "wspds")
                                    dvar = dvar * 2.23694F;           // windspeed, mph
                                else if (scenarioVar == "rsds")
                                    dvar = dvar*0.085985F;          // W/m2 to ly
                                else if (scenarioVar == "rlds")
                                    dvar = dvar * 0.085985F;        // W/m2 to ly
                                else if (scenarioVar == "rlut")
                                    dvar = dvar * 0.085985F;        // W/m2 to ly
                                else if (scenarioVar == "hfss")
                                    dvar = dvar * 0.085985F;        // W/m2 to ly
                                else if (scenarioVar == "hfls")
                                    dvar = dvar * 0.085985F;        // W/m2 to ly
                                else if (scenarioVar == "clt")
                                    dvar = dvar*10.0F;             // ly
                                else if (scenarioVar == "ps")
                                    dvar = dvar * 0.750062F;       // mmhg
                            }
                            //Debug.WriteLine("grid="+sgrid+", dt = " + dt.ToString());

                            //adjust series for timezone
                            SortedDictionary<DateTime, string> dict;
                            dictWeaData.TryGetValue(sgrid, out dict);
                            tzone = GetTimeZoneOfGrid(sgrid);
                            dict.Add(dt.AddHours(tzone), dvar.ToString());
                            //dict.Add(dt, dvar.ToString());
                        }
                        icnt++;
                    }
                    //end for each lat
                    dt = dt.AddHours(1);
                    hrsprocessed++;
                }

                fMain.WriteLogFile("Hours Processed for " + curyear.ToString() + ":" +
                    curmon.ToString() + " is " + hrsprocessed.ToString());

                //debug writeout data for all grids (*.txt)
                dt = new DateTime(curyear, curmon, 1, 0, 0, 0);
                dt = dt.AddHours(tzone);
                for (int i = 0; i < numIter; i++)
                {
                    string sval;
                    List<string> sdat = new List<string>();
                    sdat.Add(dt.ToString());
                    foreach (KeyValuePair<string, SortedDictionary<DateTime,string>>kv
                           in dictWeaData)
                    {
                        var dict = kv.Value;
                        dict.TryGetValue(dt, out sval);
                        sdat.Add(sval);
                    }
                    outsw.WriteLine(string.Join(",",sdat));
                    outsw.Flush();
                    dt = dt.AddHours(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "/r/n" + ex.StackTrace);
                fMain.WriteLogFile("Processing Error!"+crlf+ex.Message + "/r/n" + ex.StackTrace);
            }
            Cursor.Current = Cursors.Default;
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
                bool isSite = fMain.dictSta.TryGetValue(site.Trim(), out siteAttrib);
                Debug.WriteLine("In UploadToWDM IsSite = " + isSite.ToString());

                //add attributes for CMIP6/EDDE, 5-scenario, 6-pathway, 7-scenpath               
                siteAttrib[5] = scenario;
                siteAttrib[6] = pathway;
                siteAttrib[7] = scenpath;
#if debug
                Debug.WriteLine(siteAttrib[0].ToString()+", scenario = "+
                siteAttrib[5].ToString()+", pathway="+siteAttrib[6].ToString()+", scenpath"+ siteAttrib[7].ToString());
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
            //string lat = string.Empty, lon = string.Empty;
            // (KeyValuePair<int, GridPoint> kv in dictGridPts)
            //{
            //    lat = lat + (kv.Value.ylat).ToString("F3") + ",";
            //    lon = lon + (kv.Value.xlon).ToString("F3") + ",";
            //}
            string grids = string.Join(", ", lstOfGrids);
            sw.WriteLine(grids.ToString());

            //sw.WriteLine(lat.ToString());
            //sw.WriteLine(lon.ToString());
            //sw.Flush();
        }

        private void WriteStatus(string msg)
        {
            Debug.WriteLine(msg);
            fMain.WriteLogFile(msg);
            slabel.Text = msg;
            strip.Refresh();
        }

        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + crlf + ex.Message + crlf + crlf + ex.StackTrace;
            fMain.WriteLogFile(msg);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private int GetTimeZoneOfGrid(string grid)
        {
            try
            {
                MetGages sta = new MetGages();
                fMain.dictSelSites.TryGetValue(grid, out sta);
                return Convert.ToInt32(sta.TZONE);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public void ShowDataTable()
        {
                frmDataEDDE fdataEDDE = new frmDataEDDE(fMain, dictSiteVars, lstOfVars, null, scenario, pathway);
                if (fdataEDDE.ShowDialog() == DialogResult.OK)
                {
                    fdataEDDE = null;
                    fdataEDDE.Dispose();
                }
        }

        private void CalculateAnnual(string WdmFile, string AnnWdmFile, string site, string svar, string scen)
        {
            Debug.WriteLine("CalculateAnnual: Site ="+site + ", Var=" + svar + ", Scenario=" + scen);
            Debug.WriteLine("Entering CalculateAnnual ...");
            fMain.WriteLogFile("Entering CalculateAnnual ..." + site+"-"+svar);
            try
            {
                atcWDM.atcDataSourceWDM lWdmHr = new atcWDM.atcDataSourceWDM();
                lWdmHr.Open(WdmFile);
                atcWDM.atcDataSourceWDM lWdmAnn = new atcWDM.atcDataSourceWDM();
                lWdmAnn.Open(AnnWdmFile);

                bool isFound = false;
                atcData.atcTimeseries lseries = new atcData.atcTimeseries();
                foreach (atcData.atcTimeseries ts in lWdmHr.DataSets)
                {
                    if ((ts.Attributes.GetValue("Location").ToString().Contains(site.ToString())) &&
                        (ts.Attributes.GetValue("Constituent").ToString().Contains(svar)) &&
                        (ts.Attributes.GetValue("Scenario").ToString().Contains(scen)))

                    {
                        lseries = ts;
                        isFound = true;
                        break;
                    }
                }
                Debug.WriteLine("In CalculateAnnual: Found Timeseries = " + isFound.ToString());

                if (isFound)
                {
                    if (svar.Contains("PREC"))
                        CalculateAnnualTimeSeries((int)atcTran.TranSumDiv, lWdmHr, lWdmAnn, lseries, svar, site, 2.0f);
                    else
                        CalculateAnnualTimeSeries((int)atcTran.TranAverSame, lWdmHr, lWdmAnn, lseries, svar, site, 2.0f);
                }
                lWdmAnn = null;
                lWdmHr = null;
            }
            catch (Exception ex)
            {
                string msg = "Error calculating annual series!" + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool CalculateAnnualTimeSeries(int enumTrans, atcWDM.atcDataSourceWDM hrWDM, atcWDM.atcDataSourceWDM yrWDM,
              atcData.atcTimeseries hlyseries, string svar, string grid, float elev)
        {
            Debug.WriteLine("Entering CalculateAnnualTimeSeries ...");
            try
            {
                int lDSN = 0;
                {
                    lDSN = GetNextDSN(yrWDM);
                    lDSN++;

                    atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(hlyseries, atcTimeUnit.TUYear, 1, (atcTran)enumTrans, hrWDM);
                    //ltser.Attributes.SetValue("ID", lDSN++);
                    ltser.Attributes.SetValue("Constituent", svar.ToString());
                    ltser.Attributes.SetValue("Description", "EDDE: Annual " + svar);
                    ltser.Attributes.SetValue("Location", grid.ToString());
                    ltser.Attributes.SetValue("STAID", grid.ToString());
                    ltser.Attributes.SetValue("Scenario", hlyseries.Attributes.GetValue("Scenario"));
                    if (elev > 0)
                        ltser.Attributes.SetValue("Elevation", elev);
                    ltser.Attributes.SetValue("STANAM", "EDDE Lat=" + hlyseries.Attributes.GetValue("Latitude") +
                    " Long=" + hlyseries.Attributes.GetValue("Longitude"));
                    //lts.Attributes.SetValue("COMPFG", 1);
                    yrWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);
                    fMain.WriteLogFile("Uploaded annual " + svar + " for grid " + grid.ToString());
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in generating annual series!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void CalculateMaxMinTemp(string WdmFile, string grid, string svar, string scen)
        {
            Debug.WriteLine("Entering calculate tmax and tmin ...");
            fMain.WriteLogFile("Entering CalculateMaxMinTemp for site ..."+ grid);
            
            bool isFound = false;
            int lNextDSN = 0;
            float elev = 2.0f;
            try
            {
                atcWDM.atcDataSourceWDM lWdmHr = new atcWDM.atcDataSourceWDM();
                lWdmHr.Open(WdmFile);
                lNextDSN = GetNextDSN(lWdmHr);
                lNextDSN++;

                atcData.atcTimeseries atemseries = new atcData.atcTimeseries();
                foreach (atcData.atcTimeseries ts in lWdmHr.DataSets)
                {
                    if ((ts.Attributes.GetValue("Location").ToString().Contains(grid.ToString())) &&
                       (ts.Attributes.GetValue("Constituent").ToString().Contains(svar)) &&
                       (ts.Attributes.GetValue("Scenario").ToString().Contains(scen)))
                    {
                        atemseries = ts;
                        isFound = true;
                        break;
                    }
                }
                Debug.WriteLine("Found Timeseries in Calc TMAX & TMIN = " + isFound.ToString());

                if (isFound)
                {
                     //for TMIN
                     atcTimeseries lTmin = atcData.modTimeseriesMath.Aggregate(atemseries, atcTimeUnit.TUDay, 1, atcTran.TranMin, lWdmHr);
                    lTmin.Attributes.SetValue("ID", lNextDSN++);
                    lTmin.Attributes.SetValue("Constituent", "TMIN");
                    lTmin.Attributes.SetValue("Description", "EDDE: Minimum Temperature Degrees F");
                    lTmin.Attributes.SetValue("Location", grid.ToString());
                    lTmin.Attributes.SetValue("STAID", grid.ToString());
                    lTmin.Attributes.SetValue("Scenario", scen);
                    lTmin.Attributes.SetValue("Data Source", "EDDE: From hourly ATEM");
                     if (elev > 0f)
                        lTmin.Attributes.SetValue("Elevation", elev);
                    lTmin.Attributes.SetValue("STANAM", "EDDE Lat=" + atemseries.Attributes.GetValue("Latitude") +
                            " Long=" + atemseries.Attributes.GetValue("Longitude"));
                     lWdmHr.AddDataSet(lTmin, atcData.atcDataSource.EnumExistAction.ExistReplace);
                    //for TMAX
                    atcTimeseries lTmax = atcData.modTimeseriesMath.Aggregate(atemseries, atcTimeUnit.TUDay, 1, atcTran.TranMax, lWdmHr);
                    lTmax.Attributes.SetValue("ID", lNextDSN++);
                    lTmax.Attributes.SetValue("Constituent", "TMAX");
                    lTmax.Attributes.SetValue("Description", "EDDE: Maximum Temperature Degrees F");
                    lTmax.Attributes.SetValue("Location", grid.ToString());
                    lTmax.Attributes.SetValue("STAID", grid.ToString());
                    lTmax.Attributes.SetValue("Scenario", scen);
                    lTmax.Attributes.SetValue("Data Source", "EDDE: From hourly ATEM");
                     if (elev > 0f)
                        lTmax.Attributes.SetValue("Elevation", elev);
                    lTmax.Attributes.SetValue("STANAM", "EDDE Lat=" + atemseries.Attributes.GetValue("Latitude") +
                            " Long=" + atemseries.Attributes.GetValue("Longitude"));
                    lWdmHr.AddDataSet(lTmax, atcData.atcDataSource.EnumExistAction.ExistReplace);
                    lTmax = null;
                    lTmin = null;
                }
                lWdmHr = null;
            }
            catch (Exception ex)
            {
                string msg = "Error calculating max and min temperature!" + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool CalculateHamonPET(string WdmFile, string grid, string scen)
        {
            fMain.WriteLogFile("Entering CalculateHamonPET for site ... "+grid);

            int lNextDSN = 0;
            try
            {
                atcWDM.atcDataSourceWDM lWdm = new atcWDM.atcDataSourceWDM();
                lWdm.Open(WdmFile);
                lNextDSN = GetNextDSN(lWdm);
                lNextDSN++;

                atcTimeseries lTemp = lWdm.DataSets.FindData("Location", grid).FindData("Constituent", "ATEM").FindData("Scenario", scen)[0];
                atcTimeseries ltmin = lWdm.DataSets.FindData("Location", grid).FindData("Constituent", "TMIN").FindData("Scenario", scen)[0];
                atcTimeseries ltmax = lWdm.DataSets.FindData("Location", grid).FindData("Constituent", "TMAX").FindData("Scenario", scen)[0];
                //float lLatitude = (float)lTemp.Attributes.GetDefinedValue("Latitude").Value;
                float lLongitude = Convert.ToSingle(lTemp.Attributes.GetValue("Longitude"));
                float lLatitude = Convert.ToSingle(lTemp.Attributes.GetValue("Latitude"));
                double[] lCTS = {0.0055f, 0.0055f, 0.0055f, 0.0055f, 0.0055f, 0.0055f,
                                    0.0055f, 0.0055f, 0.0055f, 0.0055f, 0.0055f, 0.0055f, 0.0055f};

                atcTimeseries lPET = modComputeWea.PanEvaporationTimeseriesComputedByHamon(null, ltmin, ltmax, lWdm, true, lLatitude, lCTS);
                lTemp = null;ltmax = null;ltmin = null;

                lPET = modComputeWea.DisSolPet(lPET, null, 2, lLatitude);
                lPET.Attributes.SetValue("ID", lNextDSN++);
                lPET.Attributes.SetValue("Location", grid);
                lPET.Attributes.SetValue("Constituent", "PEVT");
                lPET.Attributes.SetValue("TSTYPE", "PEVT");
                lPET.Attributes.AddHistory("EDDE: Calculated using Hamon");
                lPET.Attributes.SetValue("Description", "EDDE: PET calculated by Hamon method");
                lPET.Attributes.SetValue("Scenario", scen);
                lPET.Attributes.SetValue("STAID", grid);
                lPET.Attributes.SetValue("StaNam", grid);
                lWdm.AddDataSet(lPET, atcDataSource.EnumExistAction.ExistReplace);
                lWdm = null;
            }
            catch (Exception exc)
            {
                string msg = "Error calculating Hamon PET!" + crlf + exc.Message + crlf + exc.StackTrace;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private int GetNextDSN(atcWDM.atcDataSourceWDM aWDM)
        {
            int lDSN = 0;
            //if there are any existing datasets, write new data after them 
            if (aWDM.DataSets.Count > 0)
            {
                int lLastDSN = 0;
                foreach (atcData.atcDataSet lds in aWDM.DataSets)
                    lLastDSN = Math.Max((int)lLastDSN, (int)lds.Attributes.GetValue("ID"));
                //just to be sure nothing is overwritten
                lDSN = lLastDSN+2;
            }
            else
                lDSN = 0;
            return lDSN;
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
            Debug.WriteLine("Entering clsEDDE");
            Debug.WriteLine("Scenario=" + scenario);
            Debug.WriteLine("Pathway=" + pathway);
            Debug.WriteLine("Scenario_Pathway=" + scenpath);
            Debug.WriteLine("BegYear=" + begyr.ToString());
            Debug.WriteLine("EndYear=" + endyr.ToString());

            //foreach (string var in lstOfVars)
            Debug.WriteLine("Vars = " + string.Join(",", lstOfVars));

            //foreach (string grd in lstOfGrids)
            Debug.WriteLine("Grid = " + string.Join(",",lstOfGrids));

            //OK 7/29 correct lstOfVars (PREC, ATEM etc) and grid(EXXXYYY

            //foreach (KeyValuePair<int, GridPoint> kv in dictGridPts)
            //{
            //    GridPoint grd;
            //    dictGridPts.TryGetValue(kv.Key, out grd);
            //    Debug.WriteLine("Ylat= {0}, Xlon= {1}",
            //             grd.ylat.ToString("F3"), grd.xlon.ToString("F3"));
            //}
        }
    }
}
