using atcWDM;
using System;
using System.IO;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmDB : Form
    {
        private string WDMFile = string.Empty;
        private string SDBFile = string.Empty;
        private string dataDir;
        private frmMain fMain;
        private string crlf = Environment.NewLine;
        private string wdmDir = string.Empty;
        public frmDB(frmMain _fMain, string _sdbfile, string _wdmfile)
        {
            InitializeComponent();
            dataDir = Path.Combine(Application.StartupPath, "data");
            this.WDMFile = _wdmfile;
            this.SDBFile = _sdbfile;
            this.fMain = _fMain;
            lblWDM.Text = WDMFile;
            lblSDB.Text = SDBFile;
            if (!string.IsNullOrEmpty(WDMFile))
                wdmDir = Path.GetDirectoryName(WDMFile);
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
                    if (IsValidWDM(sFile))
                    {
                        WDMFile = sFile;
                        lblWDM.Text = sFile;
                        wdmDir = Path.GetDirectoryName(WDMFile);
                    }
                    else
                    {
                        WDMFile = string.Empty; ;
                        lblWDM.Text = string.Empty;
                        wdmDir = string.Empty;
                    }
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }
            }
        }
        private bool IsValidWDM(string sfile)
        {
            int numds = 0;
            atcDataSourceWDM lWdmDS = new atcWDM.atcDataSourceWDM();
            try
            {
                lWdmDS.Open(sfile);
                numds = lWdmDS.DataSets.Count;
                fMain.appManager.UpdateProgress(numds.ToString() + " datasets in " + Path.GetFileName(sfile));
                lWdmDS = null;
            }
            catch (Exception ex)
            {
                fMain.WriteLogFile("Error opening datasource!" + crlf + ex.Message + ex.StackTrace + crlf + ex.Source);
                MessageBox.Show("Error opening datasource!" + crlf + ex.Message + ex.StackTrace + crlf + ex.Source);
                return false;
            }

            if (numds > 0)
                return true;
            else
            {
                string msg = sfile + " does not contain any dataset!" + crlf + crlf +
                    "Please select a valid wdm file.";
                fMain.WriteLogFile(msg);
                MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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

                if (!string.IsNullOrEmpty(wdmDir))
                    openFD.InitialDirectory = wdmDir;
                else
                    openFD.InitialDirectory = dataDir;

                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select or Create new SQLite database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    lblSDB.Text = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("sfile=" + sFile);

                if (!File.Exists(sFile))
                {
                    string defaultdb = Path.Combine(Application.StartupPath, "WeaWDM.sqlite");
                    File.Copy(defaultdb, sFile);
                }
                SDBFile = sFile;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            WDMFile = lblWDM.Text;
            SDBFile = lblSDB.Text;

            if (string.IsNullOrEmpty(WDMFile) || string.IsNullOrEmpty(SDBFile))
                return;
            else
            {
                if (!IsValidWDM(WDMFile))
                {
                    lblWDM.Text = string.Empty;
                    return;
                }
                else
                {
                    //if valid file
                    fMain.SdbFile = SDBFile;
                    fMain.WdmFile = WDMFile;
                }
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public string WDMdb()
        {
            return WDMFile;
        }
        public string SDBdb()
        {
            return SDBFile;
        }

    }
}
