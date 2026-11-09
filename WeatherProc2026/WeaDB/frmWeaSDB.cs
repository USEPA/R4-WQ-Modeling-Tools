using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using WeaWDM;
using NCEIData;

namespace WeaSDB
{
    public partial class frmWeaSDB : Form
    {
        private string sdbFile, wdmFile;
        private string dataDir;
        private DataTable tblWDM;
        private List<int> lstSelectedDSN = new List<int>();
        private WeaSDB cSDB;
        private WDM cwdm;
        private string errmsg;
        private string crlf = Environment.NewLine;

        public frmWeaSDB(string _sdbfile, string _wdmfile)
        {
            Debug.WriteLine("Loading frmWeaSDB ...");
            InitializeComponent();
            this.sdbFile = _sdbfile;
            this.wdmFile = _wdmfile;
        }
        public bool GetWDMSeries()
        {
            WDM cwdm = new WDM(wdmFile);
            tblWDM = cwdm.GetWDMAllSeries();
            if (tblWDM == null) return false;
            cwdm = null;

            //fill dataviewer
            dgvWDM.DataSource = tblWDM;
            int nrows = dgvWDM.Rows.Count;
            grpTable.Text = nrows.ToString() + " Series";
            dgvWDM.ClearSelection();
            dgvWDM.Columns["DSN"].Visible = false;
            dgvWDM.Columns["Scenario"].Visible = false;
            dgvWDM.Columns["Latitude"].Visible = false;
            dgvWDM.Columns["Longitude"].Visible = false;
            dgvWDM.Columns["Elevation"].Visible = false;

            btnSelectAll.Enabled = true;
            return true;
        }
        private void GetListOfSelectedDSN()
        {
            try
            {
                lstSelectedDSN.Clear();
                foreach (DataGridViewRow dr in dgvWDM.SelectedRows)
                {
                    int dsn = Convert.ToInt32(dr.Cells["DSN"].Value);
                    lstSelectedDSN.Add(dsn);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error!" + ex.Message + ex.StackTrace);
            }
        }
        private clsStation GetSiteInfo(int dsn)
        {
            //Columns(DSN,Station,StaName,Scenario,Constituent,Latitude,Longitude,Elevation);
            clsStation met = new clsStation();
            try
            {
                string filter = "DSN = '" + dsn.ToString() + "'";
                DataRow[] drow = tblWDM.Select(filter);
                met.Station = drow[0]["Station"].ToString();
                met.StationName = drow[0]["StaName"].ToString();
                met.Constituent = drow[0]["Constituent"].ToString();
                met.Scenario = drow[0]["Scenario"].ToString();
                met.Latitude = drow[0]["Latitude"].ToString();
                met.Longitude = drow[0]["Longitude"].ToString();
                met.Elevation = drow[0]["Elevation"].ToString();
                drow = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error getting timeseries attributes!" + crlf + crlf +
                    ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return met;
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            SortedDictionary<DateTime, double> dictSeries = new 
                                 SortedDictionary<DateTime, double>();
            try
            {
                //get the selected series index on DSN
                GetListOfSelectedDSN();

                //initialize cSDB
                cSDB = new WeaSDB(SDBFile);

                //initialize cWDM
                cwdm = new WDM(wdmFile);

                //iterate on list of selected series
                int nsites = lstSelectedDSN.Count();
                int isite = 0;
                foreach (var dsn in lstSelectedDSN)
                {
                    isite++;
                    clsStation met = GetSiteInfo(dsn);
                    if (!(met==null))
                        cSDB.InsertRecordInStationTable(met.Station, met.StationName, met.Scenario,
                            Convert.ToSingle(met.Latitude), Convert.ToSingle(met.Longitude),
                            Convert.ToSingle(met.Elevation));

                    cSDB.InsertRecordInPCODETable(met.Constituent);

                    dictSeries = cwdm.GetTimeSeries(dsn);
                    WriteStatus("Uploading " + met.Station + " records ("+isite.ToString()+
                        " of " + nsites.ToString()+" sites)");
                    string tblName = "Met";
                    cSDB.InsertRecordsInMetTable(tblName, dictSeries, met.Constituent,met.Station);
                }
                WriteStatus("Ready ..");
                dictSeries = null;
                cSDB.CloseDataBase();
                cwdm = null;
            }
            catch (Exception ex)
            {
                errmsg = "Error uploading timeseries to " + sdbFile + crlf + crlf +
                    ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void frmWeaSDB_Load(object sender, EventArgs e)
        {
            lblSDB.Text = sdbFile;
            lblWDM.Text = wdmFile;

            //buttons
            btnClearSelection.Enabled = false;
            btnExport.Enabled = false;

            //folders
            dataDir = Path.Combine(Application.StartupPath, "data");
        }
        private void btnWDM_Click(object sender, EventArgs e)
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
                openFD.Title = "Select a WDM database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    wdmFile = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("sfile=" + sFile);

            }

        }
        private void btnSDB_Click(object sender, EventArgs e)
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
                openFD.Title = "Select or Create new SQLite database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                    sFile = openFD.FileName;
                else
                {
                    sFile = string.Empty;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("sfile=" + sFile);

                if (!File.Exists(sFile))
                {
                    string defaultdb = Path.Combine(Application.StartupPath, "WeaSDB.sqlite");
                    File.Copy(defaultdb, sFile);
                }
                sdbFile = sFile;
            }
        }
        private void txtSDB_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtWDM_TextChanged(object sender, EventArgs e)
        {

        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            btnClearSelection.Enabled = true;
            dgvWDM.SelectAll();
            btnExport.Enabled = true;
            btnExport.Visible = true;

            //lstSelectedSeries.Clear();
            //dictSelectedSeries.Clear();
            //AddSelectedSeries();
        }
        private void btnSelect_Click(object sender, EventArgs e)
        {
            btnExport.Enabled = true;
            btnExport.Visible = true;
            if (!btnClearSelection.Enabled)
                btnClearSelection.Enabled = true;
        }
        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            dgvWDM.ClearSelection();
            btnClearSelection.Enabled = false;
            btnExport.Enabled = false;
        }
        private void btnClearSelSta_Click(object sender, EventArgs e)
        {

        }
        private void dgvWDM_Click(object sender, EventArgs e)
        {
            if (dgvWDM.SelectedRows.Count > 0)
            {
                btnClearSelection.Enabled = true;
                btnExport.Enabled = true;
            }
            else
            {
                btnClearSelection.Enabled = false;
                btnExport.Enabled = false;
            }
        }
        private void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
        }

        #region "Property"
        public string SDBFile
        {
            get { return sdbFile; }
            set { sdbFile = value; }
        }
        public string WDMFile
        {
            get { return wdmFile; }
            set { wdmFile = value; }
        }
        #endregion
    }
}
