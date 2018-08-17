using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FREQANAL
{
    public partial class frmReturn : Form
    {
        private frmMain fMain;
        private float num;

        public frmReturn(frmMain _FMain)
        {
            InitializeComponent();
            this.fMain = _FMain;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            fMain.ReturnPeriod = (Int16)numReturn.Value;
            //fMain.ReturnPeriod = (Int16)num;
            this.Close();
        }

        private void numReturn_ValueChanged(object sender, EventArgs e)
        {
            num = (Int16)numReturn.Value;
        }

        private void numTR_TextChanged(object sender, EventArgs e)
        {
            //if (Single.Parse(numTR.Text) >0 & Single.Parse(numTR.Text)<=100)
            {
                //num = Single.Parse(numTR.Text);        
            }
        
        }
    }
}
