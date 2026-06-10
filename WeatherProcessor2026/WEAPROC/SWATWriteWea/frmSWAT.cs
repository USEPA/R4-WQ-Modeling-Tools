using DotSpatial.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmSWAT : Form
    {
        private string WDMFile;
        private Map appMap;
        clsSWMM cSWMM;

        string errmsg = string.Empty;
        string crlf = Environment.NewLine;
        public string WeaFolder = string.Empty;
        public DateTime SimBegDate;
        public DateTime SimEndDate;
        private DataTable MetTable;
        private DateTime WDMMinDate, WDMMaxDate;
        public List<SWMMPoint> lstOfPoints = new List<SWMMPoint>();
        public atcData.atcTimeseries lseries;
        public Dictionary<string, bool> dictOptVars =
                new Dictionary<string, bool>();
        public Dictionary<string, SortedDictionary<int, clsStation>> dictGages
            = new Dictionary<string, SortedDictionary<int, clsStation>>();
        public Dictionary<string,SWMMPoint> dictPoints = new Dictionary<string, SWMMPoint>();

        public frmSWMM(Map _map, string _wdmFile)
        {
            InitializeComponent();
            this.WDMFile = _wdmFile;
            this.appMap = _map;
            this.Text += "-" + Path.GetFileName(WDMFile);

            //init controls
            btnClose.Enabled = true;
            btnAssign.Enabled = true;
            grpCommon.Enabled = false;

            int year = DateTime.Now.Year - 1;
            string dt = "#" + year.ToString("0000") + "/12/31#";
            dtEndDate.Value = DateTime.Parse(dt);

            WeaFolder = Path.GetDirectoryName(WDMFile);
            txtAirPath.Text = WeaFolder;

            //init dictionary of options
            dictOptVars.Add("PREC", false);
            dictOptVars.Add("PEVT", false);
        }
        private void btnAssign_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("Weather Folder=" + WeaFolder);
            bool isVarSelected = true;
            if (string.IsNullOrEmpty(WeaFolder))
                return;
            List<bool> lstSelVars = dictOptVars.Values.ToList();
            if (!lstSelVars.Contains(true)) isVarSelected = false;

            foreach (var s in lstSelVars)
            {
                Debug.WriteLine("Var Option =" + s.ToString());
                isVarSelected = s;
                if (isVarSelected) break;
            }
            Debug.WriteLine("isVarSelected =" + isVarSelected);

            if (!isVarSelected)
            {
                errmsg = "Please select at least one variable!";
                WriteMessage("Warning!", errmsg);
                WriteStatus(errmsg);
                return;
            }
            lstSelVars = null;

            //code executed below if isVarSelected is true,
            //i.e. user selected at least one variable
            switch (btnAssign.Text)
            {
                case "Assign Nearest Station":

                    cSWMM = new clsSWMM(this, appMap, WDMFile);
                    if (!cSWMM.GetSubbasinCentroid())
                    {
                        cSWMM = null;
                        return;
                    }

                    if (cSWMM.GetWDMAttributes())
                    {
                        //NEED to check here if dictGages contains timeseries
                        //for the selected vars
                        string msg = string.Empty;
                        List<string> metMissing = new List<string>();
                        foreach (KeyValuePair<string,bool>kv in dictOptVars)
                        {
                            string svar = kv.Key;
                            if (kv.Value)
                            {
                                //check if wdmfile has timeseries for the variable
                                if (!CheckExistSeriesInWdm(svar))
                                {
                                    bool val;
                                    msg+=msg+"No available timeseries for "+svar+crlf;
                                    dictOptVars.TryGetValue(kv.Key, out val);
                                    val = false;
                                }
                            }
                        }
                        if (metMissing.Count > 0)
                        {
                            WriteMessage("Error!", msg);
                            metMissing = null;
                            return;
                        }

                        //check simulation times
                        WDMMinDate = cSWMM.WDMMinDate();
                        WDMMaxDate = cSWMM.WDMMaxDate();
                        SetCommonPeriod();
                        CheckSimulationPeriod();

                        if (cSWMM.AssignMetVariableByBasin())
                        {
                            MetTable = cSWMM.WeatherTable();
                            cSWMM = null;

                            //show in datagrid
                            dgvAir.DataSource = null;
                            dgvAir.DataSource = MetTable;
                            dgvAir.Columns["Latitude"].Visible = false;
                            dgvAir.Columns["Longitude"].Visible = false;
                            btnClose.Enabled = true;
                            btnAssign.Text = "Write SWMM Weather File(s)";
                            grpLocation.Text = "SWMM Weather Locations (" +
                                MetTable.Rows.Count.ToString() + " stations)";
                        }
                        //isSetupAir = SetUpAirFiles();
                    }
                    break;
                case "Write SWMM Weather File(s)":
                    foreach (KeyValuePair<string, bool> kv in dictOptVars)
                    {
                        string svar = kv.Key;
                        bool optWrite = kv.Value;

                        if (optWrite)
                        {
                            List<string> lstWea = new List<string>();
                            lstWea = SetupSWMMWeatherFiles(svar);
                            if (lstWea.Count > 0)
                                WriteSWMMWeatherFiles(lstWea, svar);
                            string msg = lstWea.Count.ToString() + " SWMM file(s) written.";
                            WriteMessage("Info!", msg);
                            WriteStatus("Ready ...");
                            lstWea = null;
                        }
                    }
                    break;
            }
        }

        private bool CheckExistSeriesInWdm(string svar)
        {
            List<string> lsVars = dictGages.Keys.ToList();
            if (lsVars.Contains(svar))
                return true;
            else
                return false;
        }
        private void SetCommonPeriod()
        {
            grpCommon.Enabled = true;
            lblBegDate.Text = WDMMinDate.ToShortDateString();
            lblEndDate.Text = WDMMaxDate.ToShortDateString();
        }
        private void CheckSimulationPeriod()
        {
            SimBegDate = dtBegDate.Value;
            SimEndDate = dtEndDate.Value;

            if (DateTime.Compare(SimBegDate, WDMMinDate) < 0)
                SimBegDate = WDMMinDate;
            if (DateTime.Compare(SimEndDate, WDMMaxDate) > 0)
                SimEndDate = WDMMaxDate;
        }
        private List<string> SetupSWMMWeatherFiles(string svar)
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> lstWeaDsn = new List<string>();
            try
            {
                string weafile = string.Empty;
                foreach (DataRow drow in MetTable.Rows)
                {
                    string tsdsn = drow[svar].ToString();

                    if (!lstWeaDsn.Contains(tsdsn))
                        lstWeaDsn.Add(tsdsn);
                }

            }
            catch (Exception ex)
            {
                errmsg = "Error writing weather files!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                lstWeaDsn.Clear();
            }

            Cursor.Current = Cursors.Default;
            return lstWeaDsn;
        }
        private bool WriteSWMMWeatherFiles(List<string> lstWea, string svar)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                //add and subtract one day just to be sure
                DateTime dtBeg = SimBegDate.AddDays(1);
                DateTime dtEnd = SimEndDate.AddDays(-1);

                int nrows = lstWea.Count;
                foreach (var wea in lstWea)
                {
                    string sta = wea.Split(':')[0].ToString();
                    int dsn = Convert.ToInt32(wea.Split(':')[1]);
                    string weafile = Path.Combine(WeaFolder, sta + "_" + svar.ToLower() + ".dat");

                    WriteTimeSeries(weafile, svar, dsn, dtBeg, dtEnd);
                }
            }
            catch (Exception ex)
            {
            }

            Cursor.Current = Cursors.Default;
            return true;
        }
        private bool WriteTimeSeries(string weafile, string svar, int dsn,
            DateTime BegDate, DateTime EndDate)
        {
            atcWDM.atcDataSourceWDM lwdm;
            atcData.atcTimeseries ltseries = new atcData.atcTimeseries(null);
            int ldsn = 0;
            string loc = string.Empty;

            StreamWriter srdsn = new StreamWriter(weafile);
            srdsn.AutoFlush = true;

            try
            {
                lwdm = new atcWDM.atcDataSourceWDM();
                if (!lwdm.Open(WDMFile)) return false;

                //find series for dsn
                int numDS = lwdm.DataSets.Count;
                for (int ii = 0; ii < numDS - 1; ii++)
                {
                    ldsn = (int)lwdm.DataSets[ii].Attributes.GetValue("ID");
                    if (ldsn == dsn)
                    {
                        ltseries = lwdm.DataSets[ii];
                        lseries = ltseries;
                        loc = ltseries.Attributes.GetValue("Location").ToString();
                        break;
                    }
                }
                string msg = "Writing weather series for " + loc + ":" + svar;
                WriteStatus(msg);

                Debug.WriteLine("In GetSeries numvals" + ltseries.numValues.ToString());
                int yr, mon, day, hr, min;
                for (int j = 0; j < ltseries.numValues - 2; j++)
                {
                    DateTime dt = DateTime.FromOADate(lseries.Dates.Values[j]);
                    if (DateTime.Compare(dt, BegDate) >= 0 && DateTime.Compare(dt, EndDate) <= 0)
                    {
                        double v = ltseries.Values[j + 1];
                        yr = dt.Year;
                        mon = dt.Month;
                        day = dt.Day;
                        hr = dt.Hour;
                        min = dt.Minute;
                        StringBuilder st = new StringBuilder();
                        st.Append(loc);
                        st.Append(" " + yr.ToString("0000"));
                        st.Append("    " + mon.ToString("00"));
                        st.Append("      " + day.ToString("00"));
                        st.Append("      " + hr.ToString("00"));
                        st.Append("      " + min.ToString("00"));
                        st.Append("     " + v.ToString("F3"));
                        srdsn.WriteLine(st.ToString());
                        srdsn.Flush();
                        st = null;
                    }
                    //Debug.WriteLine("{0},{1}", dt.ToString(),v.ToString());
                }
                ltseries.Clear();
                ltseries = null;
                lwdm.Clear();
                lwdm = null;

                srdsn.Close();
                srdsn = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error in writing SWMM weather " + weafile + "!" + crlf + ex.Message + crlf + ex.StackTrace;
                WriteMessage("Error!", errmsg);
                return false;
            }
            return true;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void frmSWMM_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private void btnFolder_Click(object sender, EventArgs e)
        {
            string sfolder;
            using (FolderBrowserDialog openFD = new FolderBrowserDialog())
            {
                openFD.ShowNewFolderButton = true;
                openFD.RootFolder = Environment.SpecialFolder.MyComputer;
                openFD.Description = "Select folder to save SWMM weather files ...";
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
        public void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
        }

        private void chkRain_CheckedChanged(object sender, EventArgs e)
        {
            dictOptVars["PREC"] = chkRain.Checked;
            if (chkRain.Checked) WriteStatus("Ready ...");
        }

        private void chkPET_CheckedChanged(object sender, EventArgs e)
        {
            dictOptVars["PEVT"] = chkPET.Checked;
            if (chkPET.Checked) WriteStatus("Ready ...");
        }

        private void dtBegDate_ValueChanged(object sender, EventArgs e)
        {
            SimBegDate = dtBegDate.Value;
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
                    MessageBox.Show(msg, "Information!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
