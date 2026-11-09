using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmLogin : Form
    {

        EarthDataAuthService earthDataAuthService = new EarthDataAuthService();
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Prompt  user to overwrite if .netrc file already exists
            string netrcPath = Path.Combine(txtAuthFilesDir.Text, ".netrc");    
            if (!File.Exists(netrcPath))
            {
                //Create .netrc file if it doesn't exist
                earthDataAuthService.WriteNetrcCredentials(txtUsername.Text, txtPassword.Text);
            }
            else
            {
                DialogResult result = MessageBox.Show("A .netrc file already exists in the selected directory. Do you want to overwrite it?", "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    earthDataAuthService.WriteNetrcCredentials(txtUsername.Text, txtPassword.Text);
                }
                else
                {
                    return;
                }
            }

            earthDataAuthService.WriteNetrcCredentials(txtUsername.Text, txtPassword.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Close the dialog without saving any changes
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSelectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtAuthFilesDir.Text = folderBrowserDialog.SelectedPath;
                string netrcPath = Path.Combine(txtAuthFilesDir.Text, ".netrc");
                if (File.Exists(netrcPath))
                {                   
                    var credentials = earthDataAuthService.ReadNetrcCredentials();
                    txtUsername.Text = credentials.username;
                    txtPassword.Text = credentials.password;
                }
                else
                {
                    MessageBox.Show("No .netrc file found in the selected directory. Please create one with your EarthData credentials.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnOK.Enabled = false;
                }
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            //Get user directory
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            txtAuthFilesDir.Text = userDir;

            //See if .netrc file exists
            string netrcPath = Path.Combine(userDir, ".netrc");
            if (File.Exists(netrcPath))
            {
                var credentials = earthDataAuthService.ReadNetrcCredentials();
                txtUsername.Text = credentials.username;
                txtPassword.Text = credentials.password;

                btnOK.Enabled = true;
            }
        }

        private void btnLoginPage_Click(object sender, EventArgs e)
        {
            frmBrowser browserForm = new frmBrowser();
            browserForm.Show();
        }
    }
}
