#define debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Data;
using wdmuploader;
using WeaWDM;
using System.Windows.Forms;
using atcData;
using static atcUtility.modDate;

namespace NCEIData
{
    class clsAnnualStats
    {
        private frmMain fMain;
        private frmData fData;
        private WDM cWDM;
        private string MISS = "9999";
        private TimeSpan td;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        private Dictionary<string, string> dictMapVars = new Dictionary<string, string>();
        private Dictionary<string, SortedDictionary<DateTime, string>> dictSiteData =
                    new Dictionary<string, SortedDictionary<DateTime, string>>();
        private SortedDictionary<string, List<string>> dictSiteVars = 
                    new SortedDictionary<string, List<string>>();
        private SortedDictionary<string, List<string>> dictSiteVarsCMIP6 =
                    new SortedDictionary<string, List<string>>();
        private List<string> lstSta = new List<string>();

        private List<string> lstSelectedVars = new List<string>();
        private int optDataset;
        private string WdmFile=string.Empty, AnnWdmFile=string.Empty;
        private string TimeUnit;
        private clsStation curSite;
        private string errmsg;
        private string crlf = Environment.NewLine;
        private enum Interval { Hourly, Daily };
        private string scenario, pathway;

        private List<string> lstVars = new List<string>()
                     {"PREC","ATEM","WIND","WNDD",
                      "CLOU","DEWP","SOLR","LRAD", "ATMP",
                      "TMAX","TMIN","PRCP","TEMP", "HUMI"};

        //dictionary of gages keyed on variable and sortedDictionary of dsn and Station info
        public Dictionary<string, SortedDictionary<int, clsStation>> dictWDMGages = new
             Dictionary<string, SortedDictionary<int, clsStation>>();
        private SortedDictionary<string, List<string>> dictSta = new SortedDictionary<string, List<string>>();
        private SortedDictionary<string, MetGages> dictSelSites =
                       new SortedDictionary<string, MetGages>();
        private enum DataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6 };
        private atcWDM.atcDataSourceWDM lwdm = new atcWDM.atcDataSourceWDM();
        private atcWDM.atcDataSourceWDM annwdm = new atcWDM.atcDataSourceWDM();
        private enum DataSRC { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, CMIP6 };
        private int DatasetNum = 0;

        public clsAnnualStats(frmMain _fMain, Dictionary<string, SortedDictionary<int, clsStation>> _dictWDMGages)
        {
            fMain = _fMain;
            this.optDataset = _fMain.optDataSource;
            if (optDataset == (int)DataSource.CMIP6)
            {
                scenario = _fMain.scenarioPath;
                this.dictSiteVarsCMIP6 = fMain.dictSiteVars;
                ReviseDictkeys();
            }
            else
            {
                scenario = _fMain.scenario;
                this.dictSiteVars = fMain.dictSiteVars;
            }

            //dictionary of processed site variables
            lstSta = dictSiteVars.Keys.ToList();
            //dictionary of selected sites
            this.dictSelSites = fMain.dictSelSites;
            this.dictWDMGages = _dictWDMGages;

            this.WdmFile = fMain.WdmFile;
            this.AnnWdmFile = fMain.AnnWdmFile;
            this.dictSta = fMain.dictSta;

            //debug
#if debug
            ListSiteVars();
#endif
        }

        private void ReviseDictkeys()
        {
            //need to revise dictionary keys since for CMIP6 it's ID_Grid
            //i.e. kv.key=0_C0387368
#if debug
            Debug.WriteLine("Entering ReviseDictKeys()");
#endif
            dictSiteVars.Clear();
            foreach(KeyValuePair<string,List<string>>kv in dictSiteVarsCMIP6)
            {
                string[] token = kv.Key.Split('_');
                string grd = token[1].ToString().Trim();
                dictSiteVars.Add(grd, kv.Value);
#if debug
                Debug.WriteLine("kv.key="+kv.Key.ToString()+ ", Station =" + grd);
#endif
            }
        }

        private void ListSiteVars()
        {
            foreach (string sta in dictSiteVars.Keys.ToList())
            {
                List<string> lstOfVars = new List<string>();
                if (dictSiteVars.TryGetValue(sta, out lstOfVars))
                {
                    foreach (string svar in lstOfVars)
                    {
                        Debug.WriteLine("Station {0}, Variable {1}", sta, svar);
                    }
                }
            }
        }

        public bool ProcessDatasets()
        {
#if debug
            fMain.WriteLogFile("Entering ProcessDatasets for Annual Summaries...");
            Debug.WriteLine("Entering ProcessDatasets");
#endif

            atcData.atcTimeseries dataseries = new atcData.atcTimeseries();
            atcData.atcTimeseries tseries = new atcData.atcTimeseries();
            string tunits = "";
            int dsn=0;
            try
            {
                //clsStation seriesInfo = new clsStation();
                foreach(string sta in lstSta)
                {
                    List<string> lstOfVars = new List<string>();
                    if(dictSiteVars.TryGetValue(sta, out lstOfVars))
                    {
                        foreach(string svar in lstOfVars)
                        {
                            fMain.WriteLogFile("Calculating annual series for site " + sta +
                                " : " + svar+" : "+scenario);
                            //seriesInfo includes DSN and dataset parameters
                            //seriesInfo=GetDataSetInfoDSN(sta, svar);
                            //if (!(seriesInfo == null))
                            {
                                //dsn = (int)seriesInfo.DSN;
                                //get dataset for given dsn
                                if (optDataset == (int)DataSource.CMIP6)
                                {
                                    scenario = fMain.scenarioPath;
                                    dataseries = GetDataSetCMIP6(svar, sta, scenario, dsn);
                                }
                                else
                                {
                                    scenario = fMain.scenario;
                                    dataseries = GetDataSet(svar, sta, scenario, dsn);
                                }
                                Debug.WriteLine("Annual Processing sta={0},svar={1},dsn={2},scenario={3}", sta, svar, DatasetNum.ToString(), scenario);

                                if (dataseries.Values.Count() > 0)
                                {
                                    //atcDataAttributes attrib = new atcDataAttributes();
                                    //attrib = dataseries.Attributes;
                                    string staid = (string)dataseries.Attributes.GetValue("STAID");
                                    string location = (string)dataseries.Attributes.GetValue("Location");
                                    //string scenario = (string)dataseries.Attributes.GetValue("Scenario");
                                    //int[] lSDate = new int[6];
                                    //double lSJDate = (double)dataseries.Attributes.GetValue("SJDAY");
                                    //double lEJDate = (double)dataseries.Attributes.GetValue("EJDAY");
                                    //atcUtility.modDate.J2Date(lSJDate, ref lSDate);
                                    //if (lSDate[1] >1 || lSDate[2]  >1) //remove partial year at the start
                                    //{
                                    //    atcUtility.modDate.J2DateRounddown(lSJDate, atcTimeUnit.TUYear, ref lSDate);
                                    //    lSDate[0]++;
                                    //    lSJDate = Date2J(lSDate);
                                    //    dataseries = modTimeseriesMath.SubsetByDate(dataseries, lSJDate, lEJDate, null);
                                    //}


                                    switch (svar)
                                    {
                                        case "PREC":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranSumDiv);
                                            tunits = "Inches.";
                                            break;
                                        case "PRCP":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranSumDiv);
                                            tunits = "Inches.";
                                            break;
                                        case "PEVT":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranSumDiv);
                                            tunits = "Inches.";
                                            break;
                                        case "SOLR":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv);
                                            tseries = atcData.modTimeseriesMath.Aggregate(tseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "Langley.";
                                            break;
                                        case "LRAD":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv);
                                            tseries = atcData.modTimeseriesMath.Aggregate(tseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "Langley.";
                                            break;
                                        case "ATEM":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "deg F.";
                                            break;
                                        case "TMIN":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "deg F.";
                                            break;
                                        case "TMAX":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "deg F.";
                                            break;
                                        case "TEMP":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "deg F.";
                                            break;
                                        case "DEWP":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "deg F.";
                                            break;
                                        case "HUMI":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "%";
                                            break;
                                        case "WIND":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "mi/hr.";
                                            break;
                                        case "CLOU":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "0-10";
                                            break;
                                        case "DCLO":
                                            tseries = atcData.modTimeseriesMath.Aggregate(dataseries, atcTimeUnit.TUYear, 1, atcTran.TranAverSame);
                                            tunits = "0-10";
                                            break;
                                    }
                                    if (!CalculateAnnualTimeSeries(lwdm, annwdm, tseries, svar, location, scenario, tunits))
                                    {
                                        Debug.WriteLine("Error in generating annual series!");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error processing dataseries! " + crlf + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private clsStation GetDataSetInfoDSN(string sta, string svar)
        {
            Debug.WriteLine("Entering GetDataSetInfoDSN ...");
            clsStation staInfo = new clsStation();
            int dsnnum=-1;
            try
            {
                SortedDictionary<int, clsStation> dictStaInfo = new SortedDictionary<int, clsStation>();
                if (dictWDMGages.TryGetValue(svar, out dictStaInfo))
                {
                    Debug.WriteLine("Num series in dictStaInfo ..."+ dictStaInfo.Count.ToString());
                    foreach (KeyValuePair<int,clsStation>kv in dictStaInfo)
                    {
                        Debug.WriteLine("{0},{1},{2},{3},{4}", kv.Value.Constituent, kv.Value.Station,
                            kv.Value.Scenario, kv.Value.STAID.ToString(), kv.Value.DSN.ToString());

                        if (kv.Value.Constituent.Contains(svar) && kv.Value.STAID.Contains(sta))
                        {
                            dsnnum = (int)kv.Key; 
                            staInfo = kv.Value;
                            Debug.WriteLine("{0},{1},{2},{3},{4}", staInfo.Station, staInfo.Constituent,
                                staInfo.Scenario, staInfo.DSN.ToString(), staInfo.STAID);
                            break;
                        }
                    }
                }
                dictStaInfo = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error getting details of dataset! " + crlf + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("Exiting GetDataSetInfoDSN ...");
            return staInfo;
        }
        private atcData.atcTimeseries GetDataSet(string svar, string sta, string scenario, int dsn)
        {
            fMain.WriteLogFile("Entering GetDataset for " + sta + " : " + svar+"-"+scenario);
            Debug.WriteLine("Entering GetDataSet ...");
            atcData.atcTimeseries lseries = new atcData.atcTimeseries();
            try
            {
                lwdm.Open(WdmFile);
                Debug.WriteLine("Num datasets =" + lwdm.DataSets.Count.ToString());//OK

                for (int i = 0; i < lwdm.DataSets.Count; i++)
                {
                    int id = (int)lwdm.DataSets[i].Attributes.GetValue("ID");
                    //use STAID instead of location
                    string loc = lwdm.DataSets[i].Attributes.GetValue("STAID").ToString(); 
                    string var = lwdm.DataSets[i].Attributes.GetValue("Constituent").ToString();
                    string tstep = lwdm.DataSets[i].Attributes.GetValue("Time Unit").ToString();
                    string scen = lwdm.DataSets[i].Attributes.GetValue("Scenario").ToString();

                    Debug.WriteLine("sta ={0},svar ={1},tstep ={2},scenario ={3}", sta, svar, tstep.ToString(), scen);
                    
                    //if (svar.Contains(var) && sta.Contains(loc) && scenario.ToUpper().Trim().Equals(scen.ToUpper().Trim()) &&
                    if (svar.Contains(var) && sta.Contains(loc) && (tstep.Contains("Day") || tstep.Contains("Hour")))
                    {
                        lseries = lwdm.DataSets[i];
                        DatasetNum = id;
                        dsn = id;
                        break;
                    }
                }
                Debug.WriteLine("Num value lseries = " + lseries.Values.Count().ToString());
                Debug.WriteLine("Dataset Number, DSN  = " + DatasetNum.ToString());
            }
            catch (Exception ex)
            {
                errmsg = "Error getting series for dataset num " + dsn.ToString() + "! " + crlf + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("Exiting GetDataSet ...");
            return lseries;
        }
        
        private atcData.atcTimeseries GetDataSetCMIP6(string svar, string sta, string scenario, int dsn)
        {
            fMain.WriteLogFile("Entering GetDataset for " + sta + " : " + svar + "-" + scenario);
            Debug.WriteLine("Entering GetDataSet ...");
            atcData.atcTimeseries lseries = new atcData.atcTimeseries();
            try
            {
                lwdm.Open(WdmFile);
                Debug.WriteLine("Num datasets =" + lwdm.DataSets.Count.ToString());//OK

                for (int i = 0; i < lwdm.DataSets.Count; i++)
                {
                    int id = (int)lwdm.DataSets[i].Attributes.GetValue("ID");
                    //use STAID instead of location
                    string loc = lwdm.DataSets[i].Attributes.GetValue("STAID").ToString();
                    string var = lwdm.DataSets[i].Attributes.GetValue("Constituent").ToString();
                    string tstep = lwdm.DataSets[i].Attributes.GetValue("Time Unit").ToString();
                    string scen = lwdm.DataSets[i].Attributes.GetValue("Scenario").ToString();

                    Debug.WriteLine("sta ={0},svar ={1},tstep ={2},scenario ={3}", sta, svar, tstep.ToString(), scen);

                    //should use scenpath instead of just scenario for CMIP6 since
                    //scenpath is scen-path combination from dictionary
                    if (svar.Contains(var) && sta.Contains(loc) && scenario.ToUpper().Trim().Equals(scen.ToUpper().Trim()) &&
                        (tstep.Contains("Day") || tstep.Contains("Hour")))
                    {
                        lseries = lwdm.DataSets[i];
                        DatasetNum = id;
                        dsn = id;
                        break;
                    }
                }
                Debug.WriteLine("Num value lseries = " + lseries.Values.Count().ToString());
                Debug.WriteLine("Dataset Number, DSN  = " + DatasetNum.ToString());
            }
            catch (Exception ex)
            {
                errmsg = "Error getting series for dataset num " + dsn.ToString() + "! " + crlf + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("Exiting GetDataSet ...");
            return lseries;
        }
        private atcData.atcTimeseries GetDataSetOLD(int dsn)
        {
            Debug.WriteLine("Entering GetDataSet ...");
            atcData.atcTimeseries lseries = new atcData.atcTimeseries();
            string sdsn = dsn.ToString();
            try
            {
                lwdm.Open(WdmFile);
                Debug.WriteLine("Num datasets =" + lwdm.DataSets.Count.ToString());//OK
                
                for (int i=0;i<lwdm.DataSets.Count;i++)
                {
                     int id = (int)lwdm.DataSets[i].Attributes.GetValue("ID"); 
                     if (id == dsn)
                     {
                        lseries = lwdm.DataSets[i];
                        break;
                     }
                }

                //lseries = lwdm.DataSets.FindData("ID", sdsn)[0];
                Debug.WriteLine("Num value lseries =" + lseries.Values.Count().ToString());
                string loc = lseries.Attributes.GetValue("Location").ToString();
                string var = lseries.Attributes.GetValue("Constituent").ToString();
                string tstep = lseries.Attributes.GetValue("Time Unit").ToString();
            }
            catch (Exception ex)
            {
                errmsg = "Error getting series for dataset num "+ dsn.ToString()+"! " + crlf + crlf + ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("Exiting GetDataSet ...");
            return lseries;
        }

        private bool CalculateAnnualTimeSeries(atcWDM.atcDataSourceWDM hrWDM, 
            atcWDM.atcDataSourceWDM yrWDM, atcTimeseries dataseries, string svar, string loc, string scen, string units)
        {
            Debug.WriteLine("Entering CalculateAnnualTimeSeries ...");

            try
            {
                yrWDM.Open(AnnWdmFile);
                int lDSN = 0;
                {
                    lDSN = GetNextDSN(yrWDM);
                    lDSN++;

                    //atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(hlyseries, atcTimeUnit.TUYear, 1, atcTran.TranSumDiv,hrWDM);
                    //atcTimeseries ltser = atcData.modTimeseriesMath.Aggregate(hlyseries, atcTimeUnit.TUYear, 1, (atcTran)enumTrans, hrWDM);
                    atcTimeseries ltser = dataseries;
                    ltser.Attributes.SetValue("ID", lDSN);
                    ltser.Attributes.SetValue("Constituent", svar.ToString());
                    if (optDataset==(int)DataSRC.CMIP6)
                    {
                        string desc = (string)ltser.Attributes.GetValue("Description");
                        ltser.Attributes.SetValue("Description", desc+": Annual " + svar + " in " + units);
                    }
                    else
                        ltser.Attributes.SetValue("Description", "Annual " + svar + " in " + units);

                    ltser.Attributes.SetValue("Location", loc.ToString());
                    ltser.Attributes.SetValue("STAID", loc.ToString());
                    ltser.Attributes.SetValue("Scenario", scen.ToString());
                    //string scen = (string)ltser.Attributes.GetValue("Scenario");
                    //if (elev > 0)
                    //    ltser.Attributes.SetValue("Elevation", elev);
                    ltser.Attributes.SetValue("STANAM", scen+" Lat=" + dataseries.Attributes.GetValue("Latitude") +
                    " Long=" + dataseries.Attributes.GetValue("Longitude"));
                    //lts.Attributes.SetValue("COMPFG", 1);
                    yrWDM.AddDataSet(ltser, atcData.atcDataSource.EnumExistAction.ExistReplace);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in generating annual series!" + crlf + ex.Message + crlf + ex.StackTrace;
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Debug.WriteLine("Exiting CalculateAnnualTimeSeries ...");
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
                lDSN = lLastDSN;
            }
            else
                lDSN = 0;
            return lDSN;
        }

        public int DataSetID()
        {
            return DatasetNum;
        }
    }
}
