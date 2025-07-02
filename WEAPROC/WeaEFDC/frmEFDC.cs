using DotSpatial.Controls;
using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeaWDM;

namespace WeaEFDC
{
    public partial class frmEFDC : Form
    {
        private string WDMFile;
        private Map appMap;
        clsEFDC cEFDC;
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
        public List<string> EFDCVars = new List<string>()
              { "ATMP", "ATEM", "DEWP","PREC", "PEVT","SOLR", "CLOU","WIND","WNDD"};
        public List<string> WDMVars;
        private bool showForm = true;
        public frmEFDC(Map _map, string _wdmFile, List<CPoint> _lstOfPoints)
        {
            InitializeComponent();
            this.WDMFile = _wdmFile;
            this.appMap = _map;
            this.Text += "-" + Path.GetFileName(WDMFile);

            //init controls
            btnClose.Enabled = true;
            btnAssign.Enabled = true;
            grpCommon.Enabled = false;

            //int year = DateTime.Now.Year - 1;
            //string dt = "#" + year.ToString("0000") + "/12/31#";
            //dtEndDate.Value = DateTime.Parse(dt).AddHours(24);
            //dtBegDate.Value = DateTime.Parse(dt).AddYears(-10);
            //dtBegDate.Value = dtBegDate.Value.AddHours(24);

            WeaFolder = Path.GetDirectoryName(WDMFile);
            txtAirPath.Text = WeaFolder;

            //points selected from map saved to dictionary
            dictPoints = new Dictionary<string, CPoint>();
            for (int i = 0; i < _lstOfPoints.Count; i++)
            {
                string s = (i + 1).ToString();
                dictPoints.Add(s, _lstOfPoints.ElementAt(i));
            }

            //wdm attributes
            //WriteStatus("Reading time series attributes of WDMFile ...");
            GetWDMAttributes();
            //WriteStatus("Ready ...");
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

        public bool ShowFormEFDC()
        {
            return showForm;
        }
        private void GetWDMAttributes()
        {
            Cursor.Current = Cursors.WaitCursor;

            cWDM = new WDM(WDMFile, EFDCVars);
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
            Debug.WriteLine("In GetWDMAttributes: BegDate=" + dtBegDate.Value.ToString());
            Debug.WriteLine("In GetWDMAttributes: EndDate=" + dtEndDate.Value.ToString());
            cWDM = null;

            SetCommonPeriod();

            Cursor.Current = Cursors.Default;
        }
        private List<string> CheckForMissingWDMVars()
        {
            //NEED to check here if dictGages contains timeseries for the selected vars
            bool isOK;
            List<string> metMissing = new List<string>();
            foreach (string svar in EFDCVars)
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
        private void btnAssign_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(WeaFolder))
                return;

            //code executed below if isVarSelected is true,
            //i.e. user selected at least one variable
            //bool isOK;
            switch (btnAssign.Text)
            {
                case "Assign Nearest Station":

                    cEFDC = new clsEFDC(this, appMap, WDMFile);
                    if (!cEFDC.GetSubbasinCentroid())
                    {
                        cEFDC = null;
                        return;
                    }

                    //isOK = cEFDC.GetWDMAttributes();
                    //Debug.WriteLine("GetWDMAttributes is OK " + isOK);
                    //if (isOK)
                    {
                        //NEED to check here if dictGages contains timeseries
                        //for the selected vars
                        //string msg = string.Empty;
                        //List<string> metMissing = new List<string>();
                        //foreach (string svar in EFDCVars)
                        //{
                        //check if wdmfile has timeseries for the variable
                        //use DEWP for RH
                        //if (!svar.Contains("RHUM"))
                        //{
                        //    isOK = CheckExistSeriesInWdm(svar);
                        //    Debug.WriteLine("CheckExistSeriesInWdm is OK " + isOK + ", :" + svar);
                        //    if (!isOK)
                        //    {
                        //        msg += "No available timeseries for " + svar + crlf;
                        //        metMissing.Add(svar);
                        //    }
                        //}
                        //}

                        //Debug.WriteLine("MetMissing Count=" + metMissing.Count.ToString());
                        //if (metMissing.Count > 0)
                        //{
                        //    WriteMessage("Error!", msg);
                        //    metMissing = null;
                        //    return;
                        //}

                        //check simulation times
                        //WDMMinDate = cEFDC.WDMMinDate();
                        //WDMMaxDate = cEFDC.WDMMaxDate();
                        //SetCommonPeriod();
                        CheckSimulationPeriod();

                        if (cEFDC.AssignMetVariableByBasin())
                        {
                            MetTable = cEFDC.WeatherTable();
                            cEFDC = null;

                            //show in datagrid
                            dgvAir.DataSource = null;
                            dgvAir.DataSource = MetTable;
                            dgvAir.Columns["Latitude"].Visible = false;
                            dgvAir.Columns["Longitude"].Visible = false;
                            btnClose.Enabled = true;
                            btnAssign.Text = "Write EFDC Weather File(s)";
                            grpLocation.Text = "EFDC Weather Locations (" +
                                MetTable.Rows.Count.ToString() + " stations)";
                        }
                    }
                    break;

                case "Write EFDC Weather File(s)":
                    //iterate on met tables rows
                    foreach (DataRow drow in MetTable.Rows)
                    {
                        List<int> lstWeaDSN = new List<int>();
                        string basin = drow[0].ToString();
                        foreach (string svar in EFDCVars)
                        {
                            int wea = Convert.ToInt32(drow[svar].ToString().Split(':')[1]);
                            lstWeaDSN.Add(wea);
                        }
                        WriteEFDCWeatherFiles(basin, lstWeaDSN);
                        lstWeaDSN = null;
                    }
                    string mesg = "Written EFDC weather files for " + MetTable.Rows.Count.ToString() + " locations.";
                    MessageBox.Show(mesg, "Info!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
        private bool WriteEFDCWeatherFiles(string basin, List<int> lstWeaDSN)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                //add and subtract one hour just to be sure
                DateTime dtBeg = SimBegDate; //.AddHours(1);
                DateTime dtEnd = SimEndDate; //.AddHours(-1);
                
                Debug.WriteLine("Begin Date="+dtBeg.ToString());
                Debug.WriteLine("End Date="+dtEnd.ToString());

                int nwea = lstWeaDSN.Count;
                string stdsn = string.Empty;
                for (int i = 0; i < lstWeaDSN.Count; i++)
                {
                    int dsn = Convert.ToInt32(lstWeaDSN.ElementAt(i));
                    if (i < lstWeaDSN.Count - 1)
                        stdsn += dsn.ToString() + ",";
                    else
                        stdsn += dsn.ToString();
                }
                Debug.WriteLine(stdsn);
                WriteEFDC wriEFDC = new WriteEFDC(ref statuslbl, ref statusStrip,
                    WDMFile, WeaFolder, dtBeg, dtEnd, lstWeaDSN, basin);
                wriEFDC = null;
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
            if (lsVars.Contains(svar))
                return true;
            else
                return false;
        }
        private void SetCommonPeriod()
        {
            grpCommon.Enabled = true;
            lblBegDate.Text = WDMMinDate.ToString();
            lblEndDate.Text = WDMMaxDate.ToString();
        }
        private void CheckSimulationPeriod()
        {
            SimBegDate = dtBegDate.Value;
            SimEndDate = dtEndDate.Value;

            SimBegDate = dtBegDate.Value.Date;
            SimEndDate = dtEndDate.Value.AddDays(1).Date;

            if (DateTime.Compare(SimBegDate, WDMMinDate) < 0)
                SimBegDate = WDMMinDate;
            if (DateTime.Compare(SimEndDate, WDMMaxDate) > 0)
                SimEndDate = WDMMaxDate;
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            string sfolder;
            using (FolderBrowserDialog openFD = new FolderBrowserDialog())
            {
                openFD.ShowNewFolderButton = true;
                //openFD.RootFolder = Environment.SpecialFolder.MyComputer;
                openFD.SelectedPath = WeaFolder;
                openFD.Description = "Select folder to save EFDC weather files ...";
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
