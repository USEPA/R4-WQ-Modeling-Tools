using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeaDB;
using WeaWDM;

namespace NCEIData
{
    public partial class frmExport : Form
    {
        private string sdbFile, wdmFile, wdmFileName;
        private string dataDir;
        private DataTable tblWDM;
        private List<int> lstSelectedDSN = new List<int>();
        private WeaSDB cSDB;
        private WDM cwdm;
        private string errmsg, mssg;
        private string crlf = Environment.NewLine;
        private enum Export { SQL, CSV };
        private int OptionExport = (int)Export.SQL;
        private StreamWriter wrlog;
        DateTime dtbeg, dtend;
        private string OutputFolder=string.Empty;

        public frmExport(StreamWriter _wrlog, int _optExport, string _sdbfile, string _wdmfile)
        {
            Debug.WriteLine("Loading frmWeaSDB ...");
            InitializeComponent();
            this.sdbFile = _sdbfile;
            this.wdmFile = _wdmfile;
            this.OptionExport = _optExport;
            this.wrlog = _wrlog;
            this.wdmFileName = Path.GetFileName(wdmFile);
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
            dgvWDM.Columns["TimeUnit"].Visible = false;

            if (dgvWDM.SelectedRows.Count > 0)
                dgvWDM.ClearSelection();

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
        private Dictionary<string, List<string>> GetStationVariableDSN()
        {
            //Columns of table, all strings
            //"DSN", "Station", "StaName", "Scenario", "Constituent"
            //"Latitude""Longitude", "Elevation"

            Dictionary<string, List<string>> dictStaVars = new Dictionary<string, List<string>>();
            try
            {
                List<string> lstSta = new List<string>();
                foreach (DataGridViewRow dr in dgvWDM.SelectedRows)
                {
                    string sta = dr.Cells["Station"].Value.ToString();
                    if (!lstSta.Contains(sta))
                        lstSta.Add(sta);
                }

                //build dict of station and list of dsns
                foreach (var sta in lstSta)
                {
                    List<string> lstDsn = new List<string>();
                    foreach (DataGridViewRow dr in dgvWDM.SelectedRows)
                    {
                        if (dr.Cells["Station"].Value.ToString().Contains(sta))
                        {
                            string dsn = dr.Cells["DSN"].Value.ToString();
                            lstDsn.Add(dsn);
                        }
                    }
                    dictStaVars.Add(sta, lstDsn);
                    lstDsn = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error!" + ex.Message + ex.StackTrace);
                return null;
            }

            return dictStaVars;
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
            switch (OptionExport)
            {
                case (int)Export.SQL:
                    ExportToSDB();
                    break;
                case (int)Export.CSV:
                    if (!string.IsNullOrEmpty(OutputFolder))
                        ExportToCSV();
                    else
                        MessageBox.Show("Please specify an Output Folder!", "Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    break;
            }
        }

        private void ExportToSDB()
        {
            SortedDictionary<DateTime, double> dictSeries = new
                       SortedDictionary<DateTime, double>();
            Cursor.Current = Cursors.WaitCursor;
            string site = string.Empty;
            string svar = string.Empty;
            float lat = 0.0F;
            float lon = 0.0F;
            float elev = 0.0F;
            DateTime dtbeg, dtend;
            string scen = string.Empty;
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
                    svar = met.Constituent;
                    site = met.Station;
                    scen = met.Scenario;
                    lat = (float)(string.IsNullOrEmpty(met.Latitude) ? 0.0F : Convert.ToSingle(met.Latitude));
                    lon = (float)(string.IsNullOrEmpty(met.Longitude) ? 0.0F : Convert.ToSingle(met.Longitude));
                    elev = (float)(string.IsNullOrEmpty(met.Elevation) ? 0.0F : Convert.ToSingle(met.Elevation));

                    if (!(met == null))
                        cSDB.InsertRecordInStationTable(site, met.StationName, scen, lat, lon, elev);

                    cSDB.InsertRecordInPCODETable(svar);

                    dictSeries = cwdm.GetTimeSeries(dsn);
                    mssg = "Uploading " + site + ":" + svar + " records (" + isite.ToString() +
                        " of " + nsites.ToString() + " series)";

                    WriteStatus(mssg);
                    WriteLogFile(mssg);

                    string tblName = "Met";
                    //get period of record for svar and site
                    int nRecsInDB = cSDB.GetPeriodOfRecord(tblName, svar, site);
                    //if nrecs > 0, get begin and ending dates and only upload records not in database
                    //else upload all records to database
                    if (nRecsInDB > 0)
                    {
                        dtbeg = cSDB.BeginRecordDate();
                        dtend = cSDB.EndingRecordDate();
                        dictSeries = cSDB.FilterRecordsToUpload(dtbeg, dtend, dictSeries);
                        cSDB.InsertRecordsInMetTable(tblName, dictSeries, svar, site);
                    }
                    else
                    {
                        //insert series
                        //cSDB.DeleteRecordsFromMetTable(tblName, dictSeries, svar, site);
                        cSDB.InsertRecordsInMetTable(tblName, dictSeries, svar, site);
                    }
                }

                WriteStatus("Ready ..");
                dictSeries = null;
                cSDB.CloseDataBase();
                cwdm = null;
                Cursor.Current = Cursors.Default;

                mssg = "Uploaded " + nsites.ToString() + " series.";
                MessageBox.Show(mssg, "Informtion!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                errmsg = "Error uploading timeseries to " + sdbFile + crlf + crlf +
                    ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportToCSV()
        {
            SortedDictionary<DateTime, double> dictSeries = new SortedDictionary<DateTime, double>();
            Dictionary<string, clsStation> staInfo = new Dictionary<string, clsStation>();
            Cursor.Current = Cursors.WaitCursor;

            Debug.WriteLine("Output Folder = " + OutputFolder);
            try
            {
                //get dictionary of selected stations-variables, station-list of variables
                Dictionary<string, List<string>> dictStaVars = GetStationVariableDSN();
                Dictionary<string, SortedDictionary<DateTime, double>> SiteSeries;

                //initialize cWDM
                cwdm = new WDM(wdmFile);

                int isite = 0;
                string stname, site, pcode = string.Empty;
                float lat, lon, elev;
                int nsites = dictStaVars.Count();

                //iterate on list of selected series for each site
                foreach (var kvsite in dictStaVars)
                {
                    isite++;
                    string station = kvsite.Key;
                    List<string> lstdsn = new List<string>();
                    dictStaVars.TryGetValue(station, out lstdsn);
                    //debug
                    //Debug.WriteLine("Station = " + station);
                    //foreach (var s in lstdsn) Debug.WriteLine("Pcode = " + s);
                    int nvars = lstdsn.Count();
                    mssg = "Exporting " + station + " records (" + isite.ToString() +
                                " of " + nsites.ToString() + " sites)";

                    WriteStatus(mssg);
                    WriteLogFile(mssg);

                    SiteSeries = new Dictionary<string, SortedDictionary<DateTime, double>>();
                    //iterate on list of dsn for selected station

                    CsvProcessor cCSV = new CsvProcessor(OutputFolder);
                    clsStation met = new clsStation();
                    foreach (var sdsn in lstdsn)
                    {
                        int dsn = Convert.ToInt32(sdsn);
                        met = GetSiteInfo(dsn);
                        if (!(met == null))
                        {
                            site = met.Station;
                            stname = met.StationName;
                            pcode = met.Constituent;

                            if (!staInfo.ContainsKey(site)) staInfo.Add(site, met);

                            //Get series for station-variable
                            dictSeries = cwdm.GetTimeSeries(dsn);
                            //ok
                            //Debug.WriteLine("Site={0}, DSN={1},Series Count={2}", station, dsn.ToString(), dictSeries.Count().ToString());
                            SiteSeries.Add(pcode, dictSeries);
                        }
                        met = null;
                    }

                    //write sta info
                    cCSV.WriteStationInfoHeader(station);
                    met = new clsStation();
                    staInfo.TryGetValue(station, out met);
                    site = met.Station;
                    stname = met.StationName;
                    //lat = Convert.ToSingle(met.Latitude);
                    //lon = Convert.ToSingle(met.Longitude);
                    //elev = Convert.ToSingle(met.Elevation);

                    lat = (float)(string.IsNullOrEmpty(met.Latitude) ? 0.0F : Convert.ToSingle(met.Latitude));
                    lon = (float)(string.IsNullOrEmpty(met.Longitude) ? 0.0F : Convert.ToSingle(met.Longitude));
                    elev = (float)(string.IsNullOrEmpty(met.Elevation) ? 0.0F : Convert.ToSingle(met.Elevation));

                    cCSV.WriteStationInfo(site, stname, lat, lon, elev);

                    //Write station series
                    cCSV.WriteCSVFile(station, SiteSeries);
                    SiteSeries = null;
                    cCSV = null;
                    met = null;
                }

                WriteStatus("Ready ..");
                dictSeries = null;
                cwdm = null;
                Cursor.Current = Cursors.Default;

                MessageBox.Show("Exported timeseries from "+ nsites.ToString()+" site(s).", "Information!", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                errmsg = "Error exporting timeseries to csv files!" + crlf + crlf +
                    ex.Message + crlf + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void frmWeaSDB_Load(object sender, EventArgs e)
        {
            lblSDB.Text = sdbFile;
            txtOut.Height = 30;
            txtOut.Text = Path.GetDirectoryName(wdmFile);
            OutputFolder = txtOut.Text;

            //buttons
            btnClearSelection.Enabled = false;
            btnExport.Enabled = false;

            //folders
            dataDir = Path.Combine(Application.StartupPath, "data");

            if (OptionExport == (int)Export.SQL)
            {
                this.Text = "Export WDM Timeseries to SQLite DB";
                lblExport.Text = "Routine exports selected WDM TimeSeries to SQLite tables.";
                lblSDB.Visible = true;
                dblabel.Visible = true;
                layoutMain.RowStyles[3].Height = 30;

                lblOut.Visible = false;
                btnOut.Visible = false;
                txtOut.Visible = false;
                layoutMain.RowStyles[2].Height = 5;
            }
            else
            {
                this.Text = "Export WDM Timeseries to CSV files";
                lblExport.Text = "Routine exports selected WDM TimeSeries to csv files.";
                lblSDB.Visible = false;
                dblabel.Visible = false;
                layoutMain.RowStyles[3].Height = 5;

                lblOut.Visible = true;
                btnOut.Visible = true;
                txtOut.Visible = true;
                layoutMain.RowStyles[2].Height = 30;

            }

            dgvWDM.ClearSelection();
        }
        private void frmExport_Load(object sender, EventArgs e)
        {
            lblSDB.Text = sdbFile;
            txtOut.Height = 30;
            txtOut.Text = Path.GetDirectoryName(wdmFile);
            OutputFolder = txtOut.Text;

            //buttons
            btnClearSelection.Enabled = false;
            btnExport.Enabled = false;

            //folders
            dataDir = Path.Combine(Application.StartupPath, "data");

            if (OptionExport == (int)Export.SQL)
            {
                this.Text = "Export WDM Timeseries to SQLite DB";
                lblExport.Text = "Routine exports selected WDM TimeSeries to SQLite tables.";
                lblSDB.Visible = true;
                dblabel.Visible = true;
                layoutMain.RowStyles[3].Height = 30;

                lblOut.Visible = false;
                btnOut.Visible = false;
                txtOut.Visible = false;
                layoutMain.RowStyles[2].Height = 5;
            }
            else
            {
                this.Text = "Export WDM Timeseries to CSV files";
                lblExport.Text = "Routine exports selected WDM TimeSeries to csv files.";
                lblSDB.Visible = false;
                dblabel.Visible = false;
                layoutMain.RowStyles[3].Height = 5;

                lblOut.Visible = true;
                btnOut.Visible = true;
                txtOut.Visible = true;
                layoutMain.RowStyles[2].Height = 30;

            }

            dgvWDM.ClearSelection();
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
        public void WriteLogFile(string msg)
        {
            wrlog.WriteLine(msg);
            wrlog.AutoFlush = true;
            wrlog.Flush();
        }

        private void lblSDB_Click(object sender, EventArgs e)
        {

        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog openFD = new FolderBrowserDialog())
                {
                    openFD.ShowNewFolderButton = true;
                    openFD.SelectedPath = dataDir;
                    openFD.Description = "Select folder to save text files ...";
                    if (openFD.ShowDialog() == DialogResult.OK)
                    {
                        txtOut.Text = openFD.SelectedPath;
                        if (Directory.Exists(txtOut.Text))
                            OutputFolder = txtOut.Text;
                        else
                        {
                            OutputFolder = string.Empty;
                            txtOut.Text = string.Empty;
                        }
                    }
                    else
                    {
                        OutputFolder = string.Empty;
                        txtOut.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                OutputFolder = string.Empty;
                txtOut.Text = string.Empty;
            }
        }

        #region "Property"
        public string SDBFile
        {
            get { return sdbFile; }
            set { sdbFile = value; }
        }

        private void dgvWDM_MouseClick(object sender, MouseEventArgs e)
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

        public string WDMFile
        {
            get { return wdmFile; }
            set { wdmFile = value; }
        }
        #endregion
    }
}
