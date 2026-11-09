using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FREQANAL
{
    public partial class frmWeb : Form
    {
        private string url;

        public frmWeb(string _url)
        {
            InitializeComponent();
            this.url = _url;
            this.webUSGS.Navigate(url);
            this.Text = url;
        }

        private void frmWeb_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }
    }
}
