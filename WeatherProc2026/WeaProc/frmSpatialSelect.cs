using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmSpatialSelect : Form
    {
        private DataTable tblSelect;
        private frmSpatial fSpatial;
        public frmSpatialSelect(frmSpatial _fSpatial)
        {
            InitializeComponent();
            fSpatial = _fSpatial;
        }

        public void ShowDataGrid(DataTable _tblSelect)
        {
            tblSelect = _tblSelect;
            dgvSelect.DataSource = tblSelect;
            dgvSelect.ClearSelection();
        }

        public void ReSelect(DataTable _tblSelect)
        {
            dgvSelect.DataSource = null;
            dgvSelect.DataSource = _tblSelect;
            dgvSelect.ClearSelection();
        }

        private void frmSpatialSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
            fSpatial.chkSelected.Checked = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            fSpatial.chkSelected.Checked = false;
            this.Hide();
        }
    }
}
