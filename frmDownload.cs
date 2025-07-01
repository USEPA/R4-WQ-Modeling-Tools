using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmDownload : Form
    {
        private string BegDate, EndDate;
        private DateTime dtBegDate, dtEndDate;
        private Dictionary<string, bool> dictOptVars = new Dictionary<string, bool>();
        private List<string> lstSelectedVars = new List<string>();
        private int numYrs = 10;
        private frmMain fMain;
        private int optDataSource;
        private int PercentMiss = 50;
        private int MinYears = 5;
        private bool isValidEntry = false;
        private int UTCShift;
        private string ModelSDB = string.Empty;
        private string WDMFile;

        public enum MetDataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, PRISM, CMIP6, EDDE };

        public frmDownload(frmMain _fmain)
        {
            InitializeComponent();
            this.fMain = _fmain;
            WDMFile = fMain.WdmFile;
            Debug.WriteLine("frmDownload WDMFile = " + WDMFile);
            InitializeForm();
        }
        private void InitializeForm()
        {
            btnOK.Enabled = false;
            optDataSource = fMain.optDataSource;

            switch (optDataSource)
            {
                case (int)MetDataSource.ISD:
                    lnkLabel.Text = "Readme ISD Dataset";
                    optDEW.Text = "DewPt";
                    optCLO.Text = "Cloud";

                    optCLO.Enabled = true;
                    optDEW.Enabled = true;
                    optWND.Enabled = true;
                    optTMP.Enabled = true;
                    optPCP.Enabled = true;
                    optCLO.Checked = true;
                    optDEW.Checked = true;
                    optWND.Checked = true;
                    optTMP.Checked = true;
                    optPCP.Checked = true;
                    optSLP.Enabled = true;
                    optSLP.Checked = true;
                    lblUTC.Enabled = true;
                    numUTC.Enabled = false;
                    optPAN.Enabled = false;
                    optPAN.Checked = false;
                    break;
                case (int)MetDataSource.GHCN:
                    lnkLabel.Text = "Readme GHCN Dataset";
                    optCLO.Enabled = false;
                    optDEW.Enabled = false;
                    optWND.Enabled = false;
                    optCLO.Checked = false;
                    optDEW.Checked = false;
                    optWND.Checked = false;
                    optTMP.Enabled = true;
                    optTMP.Checked = true;
                    optPCP.Enabled = true;
                    optPCP.Checked = true;
                    optPAN.Enabled = true;
                    optPAN.Checked = true;
                    lblUTC.Enabled = false;
                    numUTC.Enabled = false;
                    optSLP.Enabled = false;
                    optSLP.Checked = false;

                    break;
                case (int)MetDataSource.HRAIN:
                    lnkLabel.Text = "Readme COOP Dataset";

                    optCLO.Enabled = false;
                    optDEW.Enabled = false;
                    optWND.Enabled = false;
                    optTMP.Enabled = false;
                    optCLO.Checked = false;
                    optDEW.Checked = false;
                    optWND.Checked = false;
                    optTMP.Checked = false;
                    optPCP.Enabled = true;
                    optPCP.Checked = true;
                    lblUTC.Enabled = false;
                    numUTC.Enabled = false;
                    optSLP.Enabled = false;
                    optSLP.Checked = false;
                    optPAN.Enabled = false;
                    optPAN.Checked = false;

                    break;
                case (int)MetDataSource.NLDAS:
                    lnkLabel.Text = "Readme NLDAS Dataset";
                    optCLO.Enabled = false;
                    optDEW.Text = "DewPt";
                    optCLO.Text = "Solar";

                    optCLO.Enabled = true;
                    optDEW.Enabled = true;
                    optWND.Enabled = true;
                    optTMP.Enabled = true;
                    optPCP.Enabled = true;

                    optCLO.Checked = true;
                    optDEW.Checked = true;
                    optWND.Checked = true;
                    optTMP.Checked = true;
                    optPCP.Checked = true;
                    lblUTC.Enabled = true;
                    numUTC.Enabled = true;
                    optSLP.Enabled = false;
                    optSLP.Checked = false;
                    optSLP.Visible = false;
                    optPAN.Enabled = false;
                    optPAN.Checked = false;
                    break;
                    lnkLabel.Text = "Readme PRISM Dataset";

                case (int)MetDataSource.GLDAS:
                    lnkLabel.Text = "Readme GLDAS Dataset";

                    optDEW.Text = "DewPt";
                    optCLO.Text = "Solar";

                    optCLO.Enabled = true;
                    optDEW.Enabled = true;
                    optWND.Enabled = true;
                    optTMP.Enabled = true;
                    optPCP.Enabled = true;

                    optCLO.Checked = true;
                    optDEW.Checked = true;
                    optWND.Checked = true;
                    optTMP.Checked = true;
                    optPCP.Checked = true;
                    lblUTC.Enabled = true;
                    numUTC.Enabled = false;
                    optSLP.Enabled = true;
                    optSLP.Checked = true;
                    optSLP.Visible = true;
                    optPAN.Enabled = false;
                    optPAN.Checked = false;
                    break;
                case (int)MetDataSource.TRMM:
                    lnkLabel.Text = "Readme TRMM Dataset";

                    optCLO.Enabled = false;
                    optDEW.Enabled = false;
                    optWND.Enabled = false;
                    optTMP.Enabled = false;
                    optCLO.Checked = false;
                    optDEW.Checked = false;
                    optWND.Checked = false;
                    optTMP.Checked = false;
                    optPCP.Enabled = true;
                    optPCP.Checked = true;
                    lblUTC.Enabled = true;
                    numUTC.Enabled = false;
                    optSLP.Enabled = false;
                    optSLP.Checked = false;
                    optPAN.Enabled = false;
                    optPAN.Checked = false;
                    break;
                case (int)MetDataSource.PRISM:
                    lnkLabel.Text = "Readme PRISM Dataset";
                    optCLO.Enabled = false;
                    optDEW.Enabled = false;
                    optWND.Enabled = false;
                    optCLO.Checked = false;
                    optDEW.Checked = false;
                    optWND.Checked = false;
                    optTMP.Enabled = true;
                    optTMP.Checked = true;
                    optPCP.Enabled = true;
                    optPCP.Checked = true;
                    optPAN.Enabled = true;
                    optPAN.Checked = true;
                    lblUTC.Enabled = false;
                    numUTC.Enabled = false;
                    optSLP.Enabled = false;
                    optSLP.Checked = false;
                    break;
            }

            DateTime dtnow = DateTime.Now;
            DateTime enddt = new DateTime(dtnow.Year - 1, 12, 31);
            dtEnd.Value = enddt;
            DateTime begdt = enddt.AddYears(-numYrs);
            dtStart.Value = new DateTime(dtnow.Year - 10, 1, 1);

            if (optDataSource == (int)MetDataSource.NLDAS || optDataSource == (int)MetDataSource.GLDAS ||
                optDataSource == (int)MetDataSource.TRMM)
            {
                int mon = dtnow.Month;
                int day = dtnow.Day;
                enddt = dtnow.AddDays(-30);
                dtEnd.Value = enddt;
            }

            if (optDataSource == (int)MetDataSource.TRMM)
            {
                enddt = new DateTime(2019, 12, 31);
                dtEnd.Value = enddt;
                begdt = new DateTime(1997, 12, 31);
                dtStart.Value = enddt.AddYears(-numYrs);

                dtStart.MinDate = begdt;
                dtStart.MaxDate = enddt;
                dtEnd.MinDate = begdt;
                dtEnd.MaxDate = enddt;
            }
            else if (optDataSource == (int)MetDataSource.NLDAS)
            {
                enddt = new DateTime(dtnow.Year - 1, 12, 31);
                dtEnd.Value = enddt;
                begdt = new DateTime(1979, 12, 31);
                dtStart.Value = enddt.AddYears(-numYrs);

                dtStart.MinDate = new DateTime(1979, 12, 31);
                dtEnd.MinDate = new DateTime(1979, 12, 31) ;
            }
            else if (optDataSource == (int)MetDataSource.GLDAS)
            {
                enddt = new DateTime(2019, 12, 31);
                dtEnd.Value = enddt;
                begdt = new DateTime(2000,1,1);
                dtStart.Value = enddt.AddYears(-numYrs);

                dtStart.MinDate = new DateTime(2000,1,1);
                dtEnd.MinDate = new DateTime(2000,1,1);
            }

            numPercentMiss.Value = PercentMiss;
            numMinYears.Value = MinYears;

            string spath = Path.GetDirectoryName(WDMFile);
            lblSDB.Text = Path.Combine(spath, Path.GetFileNameWithoutExtension(WDMFile) + ".mdl");
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            //check modeldb
            if (string.IsNullOrEmpty(ModelSDB))
            {
                MessageBox.Show("Please specify a model database filename!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                if (!File.Exists(ModelSDB))
                {
                    string defaultDB = Path.Combine(Application.StartupPath, "WeaModel.sqlite");
                    File.Copy(defaultDB, ModelSDB);
                    Debug.WriteLine("New model sdbFile=" + ModelSDB);
                }
                //else
                //    ModelSDB = lblSDB.Text;

                fMain.ModelSDB = ModelSDB;
            }

            if (ProcessSelection())
            {
                isValidEntry = true;
            }
            else
            {
                isValidEntry = false;
                return;
            }
        }

        private bool ProcessSelection()
        {
            PercentMiss = Convert.ToInt32(numPercentMiss.Value);
            MinYears = Convert.ToInt32(numMinYears.Value);
            fMain.PercentMiss = PercentMiss;
            fMain.MinYears = MinYears;

            dtBegDate = dtStart.Value;
            dtEndDate = dtEnd.Value;

            UTCShift = (int)numUTC.Value;

            if ((dtEndDate - dtBegDate).TotalDays < 365 * MinYears)
            {
                string msg = "Period of record should be greater than or equal to minmum years!";
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            dictOptVars.Clear();
            switch (optDataSource)
            {
                case (int)MetDataSource.ISD:
                    BegDate = dtStart.Value.Year.ToString("0000") + "-" +
                            dtStart.Value.Month.ToString("00") + "-" +
                            dtStart.Value.Day.ToString("00");
                    EndDate = dtEnd.Value.Year.ToString("0000") + "-" +
                        dtEnd.Value.Month.ToString("00") + "-" +
                        dtEnd.Value.Day.ToString("00");

                    // ISD vars-WND,TMP,DEW,GA1,AA1,WDR-wind direction
                    dictOptVars.Add("TMP", optTMP.Checked);
                    dictOptVars.Add("WND", optWND.Checked);
                    dictOptVars.Add("GA1", optCLO.Checked); //cloud
                    //10022020 added solar
                    dictOptVars.Add("GH1", optCLO.Checked); //solar
                    dictOptVars.Add("GO1", optCLO.Checked); //net rad
                    dictOptVars.Add("DEW", optDEW.Checked);
                    dictOptVars.Add("AA1", optPCP.Checked);
                    dictOptVars.Add("SLP", optSLP.Checked);
                    //09252020 added direction
                    if (optWND.Checked)
                        dictOptVars.Add("WDR", optWND.Checked);

                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;

                    break;
                case (int)MetDataSource.GHCN:
                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;
                    //Debug.WriteLine("In frmDownload, BegDate=" + dtBegDate.ToString());
                    //Debug.WriteLine("In frmDownload, EndDate=" + dtEndDate.ToString());
                    dictOptVars.Add("TMP", optTMP.Checked);
                    dictOptVars.Add("AA1", optPCP.Checked);
                    dictOptVars.Add("PAN", optPAN.Checked);
                    break;
                case (int)MetDataSource.HRAIN:
                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;
                    //Debug.WriteLine("In frmDownload, BegDate=" + dtBegDate.ToString());
                    //Debug.WriteLine("In frmDownload, EndDate=" + dtEndDate.ToString());
                    dictOptVars.Add("AA1", optPCP.Checked);
                    break;
                case (int)MetDataSource.NLDAS:
                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;
                    //Debug.WriteLine("In frmDownload, BegDate=" + dtBegDate.ToString());
                    //Debug.WriteLine("In frmDownload, EndDate=" + dtEndDate.ToString());
                    dictOptVars.Add("AA1", optPCP.Checked);
                    dictOptVars.Add("TMP", optTMP.Checked);
                    dictOptVars.Add("WND", optWND.Checked); //both vertical/horinzontal
                    dictOptVars.Add("GA1", optCLO.Checked); //shortwave radiation
                    dictOptVars.Add("DEW", optDEW.Checked); //relative humidity
                    dictOptVars.Add("SLP", optSLP.Checked);
                    fMain.scenario = "NLDAS";
                    break;

                case (int)MetDataSource.GLDAS:
                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;
                    dictOptVars.Add("AA1", optPCP.Checked);
                    dictOptVars.Add("TMP", optTMP.Checked);
                    dictOptVars.Add("WND", optWND.Checked); //speed only
                    dictOptVars.Add("GA1", optCLO.Checked); //shortwave radiation
                    dictOptVars.Add("DEW", optDEW.Checked); //relative humidity
                    dictOptVars.Add("SLP", optSLP.Checked); //pressure
                    fMain.scenario = "GLDAS";
                    break;

                case (int)MetDataSource.TRMM:
                    dtBegDate = dtStart.Value;
                    dtEndDate = dtEnd.Value;
                    dictOptVars.Add("AA1", optPCP.Checked);
                    fMain.scenario = "TRMM";
                    break;
            }

            bool option;
            lstSelectedVars.Clear();
            foreach (KeyValuePair<string, bool> kv in dictOptVars)
            {
                dictOptVars.TryGetValue(kv.Key, out option);
                switch (kv.Key)
                {
                    case "TMP":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.ISD:
                                if (option) lstSelectedVars.Add("TEMP");
                                break;
                            
                            case (int)MetDataSource.GHCN:
                                if (option)
                                {
                                    lstSelectedVars.Add("TMAX");
                                    lstSelectedVars.Add("TMIN");
                                }
                                break;
                            case (int)MetDataSource.NLDAS:
                                if (option) lstSelectedVars.Add("TEMP");
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option) lstSelectedVars.Add("TEMP");
                                break;
                        }
                        break;
                    case "WND":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.ISD:
                                if (option)
                                {
                                    lstSelectedVars.Add("WIND"); //wind speed
                                    //09252020 added direction
                                    lstSelectedVars.Add("WNDD"); //wind direction
                                }
                                break;
                            case (int)MetDataSource.NLDAS:
                                if (option)
                                {
                                    lstSelectedVars.Add("VWND"); //vertical
                                    lstSelectedVars.Add("UWND"); //horizontal
                                }
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option)
                                {
                                    lstSelectedVars.Add("WIND"); //SPEED
                                }
                                break;
                        }
                        break;
                    case "GA1": //cloud in ISD, solar in NLDAS
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.ISD:
                                if (option) lstSelectedVars.Add("CLOU");
                                //10022020 added solar for ISD
                                if (option) lstSelectedVars.Add("RNET");
                                if (option) lstSelectedVars.Add("SOLR");
                                break;
                            case (int)MetDataSource.NLDAS:
                                if (option) lstSelectedVars.Add("SOLR");
                                if (option) lstSelectedVars.Add("LRAD");
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option) lstSelectedVars.Add("SOLR");
                                if (option) lstSelectedVars.Add("LRAD");
                                break;
                        }
                        break;
                    case "DEW":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.ISD:
                                if (option) lstSelectedVars.Add("DEWP");
                                break;
                            case (int)MetDataSource.NLDAS:
                                if (option) lstSelectedVars.Add("HUMI");
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option) lstSelectedVars.Add("HUMI");
                                break;
                        }
                        break;
                    case "AA1":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.HRAIN:
                                if (option) lstSelectedVars.Add("PRCP");
                                break;
                            case (int)MetDataSource.ISD:
                                if (option) lstSelectedVars.Add("PREC");
                                break;
                            case (int)MetDataSource.GHCN:
                                if (option) lstSelectedVars.Add("PRCP");
                                break;
                            case (int)MetDataSource.NLDAS:
                                if (option) lstSelectedVars.Add("PREC");
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option) lstSelectedVars.Add("PREC");
                                break;
                            case (int)MetDataSource.TRMM:
                                if (option) lstSelectedVars.Add("PREC");
                                break;
                        }
                        break;
                    case "SLP":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.ISD:
                                if (option) lstSelectedVars.Add("ATMP");
                                break;
                            case (int)MetDataSource.GLDAS:
                                if (option) lstSelectedVars.Add("ATMP");
                                break;
                        }
                        break;
                    case "PAN":
                        switch (optDataSource)
                        {
                            case (int)MetDataSource.GHCN:
                                if (option) lstSelectedVars.Add("EVAP");
                                break;
                        }
                        break;
                }
            }
            if (lstSelectedVars.Count == 0)
            {
                string msg = "Please select at least one variable to download!";
                MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
                fMain.lstSelectedVars = lstSelectedVars;
            return true;
        }
        public bool ValidFormEntry()
        {
            return isValidEntry;
        }
        private void dtStart_ValueChanged(object sender, EventArgs e)
        {
            if (dtStart.Value.CompareTo(dtEnd.Value) >= 0)
                ShowError("Ending Period should be later than starting period!");

            //dtStart.Value = dtEnd.Value.AddYears(-numYrs);
        }
        private void dtEnd_ValueChanged(object sender, EventArgs e)
        {
            if (dtEnd.Value.CompareTo(dtStart.Value) <= 0)
                ShowError("Ending Period should be later than starting period!");
            //dtEnd.Value = dtStart.Value.AddYears(numYrs);
        }
        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public string BeginDate()
        {
            return BegDate;
        }
        public string EndingDate()
        {
            return EndDate;
        }
        public DateTime BeginDateTime()
        {
            return dtBegDate;
        }
        public DateTime EndingDateTime()
        {
            return dtEndDate;
        }
        public Dictionary<string, bool> OptionVars()
        {
            return dictOptVars;
        }
        public List<string> SelectedVars()
        {
            return lstSelectedVars;
        }
        public int TimeZoneShift()
        {
            return UTCShift;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void optPCP_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void optDEW_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnMdl_Click(object sender, EventArgs e)
        {
            string ext = ".mdl";
            string filter = "Stochastic Model Database (*.mdl)|*.mdl|All files (*.*)|*.*";
            string sdbFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.Title = "Select or Create a Stochastic Model Parameter Database ...";
                openFD.AddExtension = true;
                openFD.CheckFileExists = false;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = fMain.dataDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.FileName = lblSDB.Text;
                openFD.RestoreDirectory = true;
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    lblSDB.Text = openFD.FileName;
                    ModelSDB = lblSDB.Text;
                    btnOK.Enabled = true;
                }
                else
                {
                    lblSDB.Text = string.Empty;
                    ModelSDB = string.Empty;
                    btnOK.Enabled = false;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("modelfile=" + ModelSDB);
            }
        }

        private void lblSDB_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblSDB.Text))
                btnOK.Enabled = false;
            else
            {
                btnOK.Enabled = true;
                ModelSDB = lblSDB.Text;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lnkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (optDataSource)
            {
                case (int)MetDataSource.HRAIN:
                    Process.Start("https://www.ncei.noaa.gov/products/land-based-station/cooperative-observer-network\r\n");
                    break;
                case (int)MetDataSource.ISD:
                    Process.Start("https://www.ncei.noaa.gov/products/land-based-station/integrated-surface-database");
                    break;
                case (int)MetDataSource.GHCN:
                    Process.Start("https://www.ncei.noaa.gov/products/land-based-station/global-historical-climatology-network-daily");
                    break;
                case (int)MetDataSource.NLDAS:
                    Process.Start("https://ldas.gsfc.nasa.gov/nldas");
                    break;
                case (int)MetDataSource.GLDAS:
                    Process.Start("https://ldas.gsfc.nasa.gov/gldas");
                    break;
                case (int)MetDataSource.TRMM:
                    Process.Start("https://gpm.nasa.gov/missions/trmm");
                    break;
                case (int)MetDataSource.PRISM:
                    Process.Start("https://prism.oregonstate.edu/");
                    break;
            }
        }

        private void numMinYears_ValueChanged(object sender, EventArgs e)
        {
            MinYears = Convert.ToInt32(numMinYears.Value);
        }

        private void numPercentMiss_ValueChanged(object sender, EventArgs e)
        {
            PercentMiss = Convert.ToInt32(numPercentMiss.Value);
        }
    }
}
