using DotSpatial.Controls;
using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeaWDM;

namespace WeaSWAT
{
    public partial class frmSWAT : Form
    {
        private string WDMFile;
        private Map appMap;
        clsSWAT cSWAT;
        WDM cWDM;

        string errmsg = string.Empty;
        string crlf = Environment.NewLine;
        public string WeaFolder = string.Empty;
        public DateTime SimBegDate;
        public DateTime SimEndDate;
        private DataTable MetTable;
        private DateTime WDMMinDate, WDMMaxDate;
        public List<CPoint> lstOfPoints = new List<CPoint>();
        public atcData.atcTimeseries lseries;
        //public Dictionary<string, bool> dictOptVars =
        //        new Dictionary<string, bool>();
        public Dictionary<string, SortedDictionary<int, clsStation>> dictGages
            = new Dictionary<string, SortedDictionary<int, clsStation>>();
        public Dictionary<string, CPoint> dictPoints;
        private List<string> WeaVars = new List<string>()
              { "pcp", "pet", "tmp", "slr", "wnd", "dew", "clo" };
        public List<string> SWATVars = new List<string>()
              { "PREC", "PEVT", "ATEM", "SOLR", "WIND", "DEWP", "CLOU" };
        private bool showForm = true;

        public frmSWAT(Map _map, string _wdmFile, string _weaFolder, List<CPoint> _lstOfPoints)
        {
            InitializeComponent();
            this.WDMFile = _wdmFile;
            this.appMap = _map;
            this.Text += "-" + Path.GetFileName(WDMFile);
            this.WeaFolder = _weaFolder;

            //init controls
            btnClose.Enabled = true;
            btnAssign.Enabled = true;
            grpCommon.Enabled = false;

            //int year = DateTime.Now.Year - 1;
            //string dt = "#" + year.ToString("0000") + "/12/31#";
            //dtEndDate.Value = DateTime.Parse(dt);

            //WeaFolder = Path.GetDirectoryName(WDMFile);
            //txtAirPath.Text = WeaFolder;

            //points selected from map saved to dictionary
            dictPoints = new Dictionary<string, CPoint>();
            for (int i = 0; i < _lstOfPoints.Count; i++)
            {
                string s = (i + 1).ToString();
                dictPoints.Add(s, _lstOfPoints.ElementAt(i));
            }
            //wdm attributes
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
        public bool ShowFormSWAT()
        {
            return showForm;
        }
        private void GetWDMAttributes()
        {
            Cursor.Current = Cursors.WaitCursor;

            cWDM = new WDM(WDMFile, SWATVars);
            cWDM.GetWDMAttributes("Hour");
            //dtBegDate.Value = cWDM.SeriesMinDate();
            //dtEndDate.Value = cWDM.SeriesMaxDate();
            //min and max dates in wdm
            //WDMMinDate = cWDM.SeriesMinDate();
            //WDMMaxDate = cWDM.SeriesMaxDate();

            //min and max date of date selectors
            //dtBegDate.MinDate = cWDM.SeriesMinDate();
            //dtBegDate.MaxDate = cWDM.SeriesMaxDate().AddYears(-1);
            //dtEndDate.MinDate = cWDM.SeriesMinDate().AddYears(1);
            //dtEndDate.MaxDate = cWDM.SeriesMaxDate();
            //min and max date of date selectors
            DateTime dbeg = cWDM.SeriesMinDate();
            DateTime dend = cWDM.SeriesMaxDate();

            if (DateTime.Compare(dbeg, dend) <= 0)
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
            foreach (string svar in SWATVars)
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

        private void AssignNearestStations()
        {
            cSWAT = new clsSWAT(this, appMap, WDMFile);
            if (!cSWAT.GetSubbasinCentroid())
            {
                cSWAT = null;
                return;
            }
            CheckSimulationPeriod();

            if (cSWAT.AssignMetVariableByBasin())
            {
                MetTable = cSWAT.WeatherTable();
                cSWAT = null;

                //show in datagrid
                dgvAir.DataSource = null;
                dgvAir.DataSource = MetTable;
                dgvAir.Columns["Latitude"].Visible = false;
                dgvAir.Columns["Longitude"].Visible = false;
                btnClose.Enabled = true;
                btnAssign.Text = "Write SWAT Weather File(s)";
                grpLocation.Text = "SWAT Weather Locations (" +
                    MetTable.Rows.Count.ToString() + " stations)";
            }
        }
        private void btnAssign_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(WeaFolder))
                return;

            //code executed below if isVarSelected is true,
            //i.e. user selected at least one variable
            bool isOK;
            switch (btnAssign.Text)
            {
                case "Assign Nearest Station":
                    AssignNearestStations();
                    break;

                case "Write SWAT Weather File(s)":
                    //iterate on met tables rows
                    foreach (DataRow drow in MetTable.Rows)
                    {
                        List<int> lstWeaDSN = new List<int>();
                        string basin = drow[0].ToString();
                        foreach (string svar in SWATVars)
                        {
                            int wea = Convert.ToInt32(drow[svar].ToString().Split(':')[1]);
                            lstWeaDSN.Add(wea);
                        }
                        WriteSWATWeatherFiles(basin, lstWeaDSN);
                        lstWeaDSN = null;
                    }
                    string mesg = "Written SWAT weather files for " + MetTable.Rows.Count.ToString() + " locations.";
                    MessageBox.Show(mesg, "Info!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
        private bool WriteSWATWeatherFiles(string basin, List<int> lstWeaDSN)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                //add and subtract one day just to be sure
                DateTime dtBeg = SimBegDate.AddDays(1);
                DateTime dtEnd = SimEndDate.AddDays(-1);

                int nwea = lstWeaDSN.Count;
                //for (int i = 0; i < lstWeaDSN.Count; i++)
                //{
                //    int dsn = Convert.ToInt32(lstWeaDSN.ElementAt(i));
                //    string svar = SWATVars.ElementAt(i).ToString();
                //    string weafile = Path.Combine(WeaFolder, svar+basin.ToString() + "."+svar);
                //    Debug.WriteLine(weafile);
                //}
                //write SWAT files, call module ModSWAT
                WriteSwat wriSWAT = new WriteSwat(ref statuslbl, ref statusStrip, WDMFile, WeaFolder, dtBeg, dtEnd, lstWeaDSN, basin);
                wriSWAT = null;
            }
            catch (Exception ex)
            {
            }

            Cursor.Current = Cursors.Default;
            return true;
        }
        private bool CheckExistSeriesInWdm(string svar)
        {
            Debug.WriteLine("Entering CheckExistSeriesInWdm : " + svar);
            List<string> lsVars = dictGages.Keys.ToList();
            foreach (string s in lsVars)
                Debug.WriteLine("IN lsVars: " + s);
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
        private List<string> SetupSWATWeatherFiles(string svar)
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

        private void btnFolder_Click(object sender, EventArgs e)
        {
            string sfolder;
            using (FolderBrowserDialog openFD = new FolderBrowserDialog())
            {
                openFD.ShowNewFolderButton = true;
                //openFD.RootFolder = Environment.SpecialFolder.MyComputer;
                openFD.SelectedPath = WeaFolder;
                openFD.Description = "Select folder to save SWAT weather files ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sfolder = openFD.SelectedPath;
                else
                {
                    sfolder = string.Empty;
                    return;
                }
            }
            //txtAirPath.Text = sfolder;
            //WeaFolder = txtAirPath.Text;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        public void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
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
