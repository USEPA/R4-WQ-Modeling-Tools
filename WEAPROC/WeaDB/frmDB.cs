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

namespace WeaSDB
{
    public partial class frmDB : Form
    {
        private string WDMFile = string.Empty;
        private string SDBFile = string.Empty;
        private string dataDir;
        public frmDB(string _sdbfile, string _wdmfile)
        {
            InitializeComponent();
            dataDir = Path.Combine(Application.StartupPath, "data");
            this.WDMFile = _wdmfile;
            this.SDBFile = _sdbfile;
            txtWDM.Text = WDMFile;
            txtSDB.Text = SDBFile;
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
                    WDMFile = sFile;
                    txtWDM.Text = sFile;
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
                {
                    sFile = openFD.FileName;
                    txtSDB.Text = sFile;
                }
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
                SDBFile = sFile;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(WDMFile) || string.IsNullOrEmpty(SDBFile))
                return;
            else
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
