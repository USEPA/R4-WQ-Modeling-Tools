using DotSpatial.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
//using WeaUtil;
using WeaWDM;

namespace NCEIData
{
    public partial class frmLSPC : Form
    {
        private string WDMFile, SubbasinFile;
        private IMap appMap;
        clsAir cAir;
        WDM cWDM;

        string errmsg = string.Empty;
        string crlf = Environment.NewLine;
        public string WeaFolder = string.Empty;
        public DateTime SimBegDate;
        public DateTime SimEndDate;
        private DataTable MetTable;
        private DateTime WDMMinDate, WDMMaxDate;
        private string dsnFileName;
        private string dsnFile = "dsn.wea";
        public Dictionary<string, SortedDictionary<int, clsStation>> dictGages
            = new Dictionary<string, SortedDictionary<int, clsStation>>();
        public List<string> LSPCVars = new List<string>() {"PREC","PEVT",
                        "ATEM","WIND","SOLR","DEWP","CLOU"};
        private bool showForm = true;

        public frmLSPC(IMap _map, string _wdmFile)
        {
            InitializeComponent();
            this.WDMFile = _wdmFile;
            this.appMap = _map;

            //init controls
            btnClose.Enabled = false;
            btnAssign.Enabled = false;
            grpCommon.Enabled = false;
            //int year = DateTime.Now.Year - 1;
            //string dt = "#" + year.ToString("0000") + "/12/31#";
            //dtEndDate.Value = DateTime.Parse(dt);

            WeaFolder = Path.GetDirectoryName(WDMFile);
            txtAirPath.Text = WeaFolder;

            //get WDMAttributes
            GetWDMAttributes();
            List<string> lstMiss = CheckForMissingWDMVars();
            if (lstMiss.Count > 0)
            {
                Debug.WriteLine("MetMissing Count=" + lstMiss.Count.ToString());
                string svars = string.Join(",", lstMiss.ToArray());
                errmsg = "Variables missing in WDMFile " + svars;
                WriteMessage("Error!", errmsg);
                showForm = false;
            }
        }
        public bool ShowFormLSPC()
        {
            return showForm;
        }
        private void GetWDMAttributes()
        {
            Cursor.Current = Cursors.WaitCursor;

            cWDM = new WDM(WDMFile, LSPCVars);
            cWDM.GetWDMAttributes("Hour");

            //dtBegDate.Value = cWDM.SeriesMinDate(); //return min date
            //dtEndDate.Value = cWDM.SeriesMaxDate(); //return max date

            //min and max date of date selectors
            DateTime dbeg = cWDM.SeriesMinDate();
            DateTime dend = cWDM.SeriesMaxDate();

            if (DateTime.Compare(dbeg,dend)<=0)
            {
                dtBegDate.MinDate = cWDM.SeriesMinDate();
                dtBegDate.MaxDate = cWDM.SeriesMaxDate();
                dtEndDate.MinDate = cWDM.SeriesMinDate();
                dtEndDate.MaxDate = cWDM.SeriesMaxDate();
                dtBegDate.Value = cWDM.SeriesMinDate(); //return min date
                dtEndDate.Value = cWDM.SeriesMaxDate(); //return max date
                WDMMinDate = cWDM.SeriesMinDate();
                WDMMaxDate = cWDM.SeriesMaxDate();
            }
            else
            {
                dtBegDate.MinDate = cWDM.SeriesMaxDate();
                dtBegDate.MaxDate = cWDM.SeriesMaxDate();
                dtEndDate.MinDate = cWDM.SeriesMaxDate();
                dtEndDate.MaxDate = cWDM.SeriesMaxDate();
                dtBegDate.Value = cWDM.SeriesMaxDate(); //return min date
                dtEndDate.Value = cWDM.SeriesMaxDate(); //return max date
                WDMMinDate = cWDM.SeriesMaxDate();
                WDMMaxDate = cWDM.SeriesMaxDate();
            }

            //dtBegDate.MaxDate = cWDM.SeriesMaxDate().AddYears(-1);
            //dtEndDate.MinDate = cWDM.SeriesMinDate().AddYears(1);
            //dtEndDate.MinDate = cWDM.SeriesMinDate();
            //dtEndDate.MaxDate = cWDM.SeriesMaxDate();

            dictGages = cWDM.dictWDMSeries();
            Debug.WriteLine("BegDate=" + dtBegDate.Value.ToString());
            Debug.WriteLine("EndDate=" + dtEndDate.Value.ToString());
            cWDM = null;

            SetCommonPeriod();

            Cursor.Current = Cursors.Default;
        }
        private List<string> CheckForMissingWDMVars()
        {
            //NEED to check here if dictGages contains timeseries for the selected vars
            bool isOK;
            List<string> metMissing = new List<string>();
            foreach (string svar in LSPCVars)
            {
                isOK = CheckExistSeriesInWdm(svar);
                Debug.WriteLine("CheckExistSeriesInWdm is OK " + isOK + ", :" + svar);
                if (!isOK)
                {
                    metMissing.Add(svar);
                }
            }
            return metMissing;
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string ext = ".shp";
            string filter = "Shapefiles (*.shp)|*.shp|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = true;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Path.Combine(Application.StartupPath, "gis");
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select Subbasin shapefile...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sFile = openFD.FileName;
                else
                {
                    sFile = string.Empty;
                    return;
                }
            }

            SubbasinFile = sFile;
            txtBasin.Text = sFile;
            SubbasinFile = txtBasin.Text;
            btnAssign.Enabled = true;
        }
        private void btnAssign_Click(object sender, EventArgs e)
        {
            bool isOK = false;
            switch (btnAssign.Text)
            {
                case "Assign Met Stations":
                    if (string.IsNullOrEmpty(SubbasinFile) && string.IsNullOrEmpty(WeaFolder))
                        return;
                    cAir = new clsAir(this, appMap, WDMFile, SubbasinFile);
                    isOK = cAir.GetSubbasinCentroid();
                    Debug.WriteLine("GetSubbasinCentroid is OK " + isOK);
                    if (!isOK)
                    {
                        cAir = null;
                        return;
                    }
                    //isOK = cAir.GetWDMAttributes();
                    //Debug.WriteLine("GetWDMAttributes is OK " + isOK);
                    //if (isOK)
                    //{
                    //    string msg = string.Empty;
                    //    List<string> metMissing = new List<string>();

                    //    foreach (string svar in LSPCVars)
                    //    {
                    //check if wdmfile has timeseries for the variable
                    //        isOK = CheckExistSeriesInWdm(svar);
                    //Debug.WriteLine("CheckExistSeriesInWdm is OK " + isOK+", "+svar);
                    //        if (!isOK)
                    //        {
                    //            msg += msg + "No available timeseries for " + svar + crlf;
                    //            metMissing.Add(svar);
                    //        }
                    //    }
                    //    Debug.WriteLine("Met missing count=" + metMissing.Count.ToString());
                    //    if (metMissing.Count > 0)
                    //    {
                    //        WriteMessage("Error", msg);
                    //        metMissing = null;
                    //        return;
                    //    }

                    //check simulation times
                    //WDMMinDate = cAir.WDMMinDate();
                    //WDMMaxDate = cAir.WDMMaxDate();
                    //SetCommonPeriod();
                    CheckSimulationPeriod();

                    if (cAir.AssignMetVariableByBasin())
                    {
                        MetTable = cAir.WeatherTable();
                        cAir = null;

                        //show in datagrid
                        dgvAir.DataSource = null;
                        dgvAir.DataSource = MetTable;
                        btnClose.Enabled = true;
                        btnAssign.Text = "Write LSPC AirFiles";
                    }
                    //}
                    break;
                case "Write LSPC AirFiles":
                    if (SetUpAirFiles())
                        WriteAirFiles(dsnFileName);
                    break;
            }
        }

        private bool CheckExistSeriesInWdm(string svar)
        {
            Debug.WriteLine("Entering CheckExistSeriesInWdm ....");
            List<string> lsVars = dictGages.Keys.ToList();
            if (lsVars.Contains(svar))
                return true;
            else
                return false;
        }

        private void SetCommonPeriod()
        {
            Debug.WriteLine("Entering SetCommonPeriod ....");

            grpCommon.Enabled = true;
            lblBegDate.Text = WDMMinDate.ToString();
            lblEndDate.Text = WDMMaxDate.ToString();
        }

        private void CheckSimulationPeriod()
        {
            Debug.WriteLine("Entering CheckSimulationPeriod ....");

            SimBegDate = dtBegDate.Value;
            SimEndDate = dtEndDate.Value;

            if (DateTime.Compare(SimBegDate, WDMMinDate) < 0)
                SimBegDate = WDMMinDate;
            if (DateTime.Compare(SimEndDate, WDMMaxDate) > 0)
                SimEndDate = WDMMaxDate;
        }
        private bool SetUpAirFiles()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                dsnFileName = Path.Combine(WeaFolder, dsnFile);
                StreamWriter srdsn = new StreamWriter(dsnFileName);
                srdsn.AutoFlush = true;

                string sbasin = Path.GetFileNameWithoutExtension(SubbasinFile) + ".airf";
                string lspcWeaFile = Path.Combine(WeaFolder, "LSPC_" + sbasin);
                StreamWriter wr = new StreamWriter(lspcWeaFile);
                wr.AutoFlush = true;

                List<string> lstAir = new List<string>();
                foreach (DataRow drow in MetTable.Rows)
                {
                    string airfile = string.Empty;
                    string prec = drow["PREC"].ToString().Split(':')[0].ToString();
                    string pevt = drow["PEVT"].ToString().Split(':')[0].ToString();
                    string atem = drow["ATEM"].ToString().Split(':')[0].ToString();
                    string wind = drow["WIND"].ToString().Split(':')[0].ToString();
                    string solr = drow["SOLR"].ToString().Split(':')[0].ToString();
                    string dewp = drow["DEWP"].ToString().Split(':')[0].ToString();
                    string clou = drow["CLOU"].ToString().Split(':')[0].ToString();

                    airfile = prec + "_" + pevt + "_" + atem + "_" +
                          wind + "_" + solr + "_" + dewp + "_" + clou + ".air";

                    sbasin = "LSPC" + drow["Subbasin"].ToString();
                    wr.WriteLine(sbasin + "," + airfile);
                    wr.Flush();

                    airfile = Path.Combine(WeaFolder, airfile.Trim());
                    prec = drow["PREC"].ToString().Split(':')[1].ToString();
                    pevt = drow["PEVT"].ToString().Split(':')[1].ToString();
                    atem = drow["ATEM"].ToString().Split(':')[1].ToString();
                    wind = drow["WIND"].ToString().Split(':')[1].ToString();
                    solr = drow["SOLR"].ToString().Split(':')[1].ToString();
                    dewp = drow["DEWP"].ToString().Split(':')[1].ToString();
                    clou = drow["CLOU"].ToString().Split(':')[1].ToString();
                    //add dsn's
                    airfile = airfile + "," + prec + "," + pevt + "," + atem + "," +
                          wind + "," + solr + "," + dewp + "," + clou;
                    if (!lstAir.Contains(airfile.Trim()))
                        lstAir.Add(airfile);
                }
                wr.Flush();
                wr.Close();
                wr = null;

                //add and subtract one day just to be sure
                SimBegDate = SimBegDate.AddDays(1);
                SimEndDate = SimEndDate.AddDays(-1);

                int nrows = lstAir.Count;
                string sline = WDMFile + "," + nrows.ToString() + "," +
                        SimBegDate.ToShortDateString() + "," + SimEndDate.ToShortDateString();
                srdsn.WriteLine(sline);
                foreach (var air in lstAir)
                {
                    srdsn.WriteLine(air);
                    srdsn.Flush();
                }
                lstAir = null;
                srdsn.Flush();
                srdsn.Close();
                srdsn = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error writing dsn.wea!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            Cursor.Current = Cursors.Default;

            return true;
        }
        private bool WriteAirFiles(string sargs)
        {
            try
            {
                Process pLSPC = new Process();
                string aExeName = Path.Combine(Application.StartupPath, "LSPCWriteAir.exe");
                pLSPC.StartInfo.FileName = aExeName;
                if (!string.IsNullOrEmpty(sargs))
                {
                    pLSPC.StartInfo.Arguments = sargs;
                    pLSPC.Start();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error writing airfiles!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            return true;
        }
        public void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
        }
        private void btnFolder_Click(object sender, EventArgs e)
        {
            string sfolder;
            using (FolderBrowserDialog openFD = new FolderBrowserDialog())
            {
                openFD.ShowNewFolderButton = true;
                //openFD.RootFolder = Environment.SpecialFolder.MyComputer;
                openFD.SelectedPath = WeaFolder;
                openFD.Description = "Select folder to save LSPC weather files ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sfolder = openFD.SelectedPath;
                else
                {
                    sfolder = string.Empty;
                    return;
                }
            }
            txtAirPath.Text = sfolder;
            WeaFolder = txtAirPath.Text;
        }

        private void txtBasin_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SubbasinFile) && !string.IsNullOrEmpty(WeaFolder))
            {
                //dtBegDate.Enabled = true;
                //dtEndDate.Enabled = true;
                btnAssign.Enabled = true;
            }
            else
            {
                //dtBegDate.Enabled = false;
                //dtEndDate.Enabled = false;
                btnAssign.Enabled = false;
            }
        }

        private void txtAirPath_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SubbasinFile) && !string.IsNullOrEmpty(WeaFolder))
            {
                //dtBegDate.Enabled = true;
                //dtEndDate.Enabled = true;
                btnAssign.Enabled = true;
            }
            else
            {
                //dtBegDate.Enabled = false;
                //dtEndDate.Enabled = false;
                btnAssign.Enabled = false;
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void dtBegDate_ValueChanged(object sender, EventArgs e)
        {
            SimBegDate = dtBegDate.Value;
        }

        private void frmLSPC_Load(object sender, EventArgs e)
        {

        }

        private void dtEndDate_ValueChanged(object sender, EventArgs e)
        {
            SimEndDate = dtEndDate.Value;
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
    }
}
