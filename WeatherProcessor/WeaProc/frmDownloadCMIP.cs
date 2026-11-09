#define debug
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmDownloadCMIP : Form
    {
        private int _begYear, _endYear;
        private string _scenario, _pathway, _variant;
        private Dictionary<string, bool> dictOptVars = 
              new Dictionary<string, bool>();
        private List<string> lstSelectedVars = new List<string>();
        private List<string> lstSelectedGrid;

        //private int numYrs = 10;
        private frmMain fMain;
        private int PercentMiss = 50;
        private bool isValidEntry = false;
        private int UTCShift;
        private string WDMFile;
        private CMIP6Series CMIPseries;
        private BoundingBox GridBndry;
        private SortedDictionary<string, List<string>> dictGCM= new
                SortedDictionary<string, List<string>>();
        private SortedDictionary<string, string> dictSSP = new
                SortedDictionary<string, string>();
        private string crlf = Environment.NewLine;

        public frmDownloadCMIP(frmMain _fmain, BoundingBox _bbox)
        {
            InitializeComponent();
            this.fMain = _fmain;
            WDMFile = fMain.WdmFile;
            GridBndry = _bbox;
            dictGCM = fMain.dictGCM;
            dictSSP = fMain.dictSSP;
            lstSelectedGrid = new List<string>(fMain.dictGages.Keys);

            InitializeForm();
        }

        private void InitializeForm()
        {
            if (!(dictSSP.Count > 0))
            {
                dictSSP.Add("SSP585", "Future scenario with high radiative forcing by the end of century. Following approximately RCP8.5 global forcing pathway but with new forcing based on SSP5. Concentration-driven.");
                dictSSP.Add("SSP370", "Future scenario with high radiative forcing by the end of century. Reaches about 7.0 W/m2 by 2100; fills gap in RCP forcing pathways between 6.0 and 8.5 W/m2. Concentration-driven.");
                dictSSP.Add("SSP245", "Future scenario with medium radiative forcing by the end of century. Following approximately RCP4.5 global forcing pathway but with new forcing based on SSP2. Concentration-driven.");
                dictSSP.Add("SSP126", "Future scenario with low radiative forcing by the end of century. Following approximately RCP2.6 global forcing pathway but with new forcing based on SSP1. Concentration-driven.");
                dictSSP.Add("Historical", "Historical experiment for the GCM model for the period 1950-2014");
            }

            string spath = Path.GetDirectoryName(WDMFile);
            btnOK.Enabled = true;
            optSW.Checked = false;
            optLW.Checked = false;
            optWND.Checked = false;
            optTMP.Checked = true;
            optPCP.Checked = true;
            optRH.Checked = false;

            numPercentMiss.Value = PercentMiss;
            cboScenario.SelectedIndex = 0;
            cboPathway.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
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
            _begYear = (int)numYearFrom.Value;
            _endYear = (int)numYearTo.Value;
            _scenario = cboScenario.SelectedItem.ToString();
            _pathway = cboPathway.SelectedItem.ToString().ToLower();
            PercentMiss = Convert.ToInt32(numPercentMiss.Value);
            fMain.PercentMiss = PercentMiss;

            dictOptVars.Clear();
            dictOptVars.Add("PCP", optPCP.Checked);
            dictOptVars.Add("TMP", optTMP.Checked);
            dictOptVars.Add("WND", optWND.Checked); //both vertical/horinzontal
            dictOptVars.Add("SWR", optSW.Checked); //shortwave radiation
            dictOptVars.Add("LWR", optLW.Checked); //relative humidity
            dictOptVars.Add("HUM", optRH.Checked);

            bool option;
            lstSelectedVars.Clear();
            foreach (KeyValuePair<string, bool> kv in dictOptVars)
            {
                dictOptVars.TryGetValue(kv.Key, out option);
                switch (kv.Key)
                {
                    case "PCP":
                        if (option) lstSelectedVars.Add("PRCP");
                        //if (option) lstSelectedVars.Add("DPCP");
                        break;
                    case "TMP":
                        if (option)
                        {
                            //lstSelectedVars.Add("TEMP");
                            lstSelectedVars.Add("DTMP");
                            lstSelectedVars.Add("TMAX");
                            lstSelectedVars.Add("TMIN");
                        }
                        break;
                    case "SWR":
                        // (option) lstSelectedVars.Add("SOLR");
                        if (option) lstSelectedVars.Add("DSOL");
                        break;
                    case "LWR":
                        //if (option) lstSelectedVars.Add("LRAD");
                        if (option) lstSelectedVars.Add("DLWR");
                        break;
                    case "HUM":
                        //if (option) lstSelectedVars.Add("HUMI");
                        if (option)
                        {
                            lstSelectedVars.Add("DHUM"); //rel humidity (%)
                            lstSelectedVars.Add("DSHM"); //specific humudity (kg/kg)
                        }
                        break;
                    case "WND":
                        //if (option) lstSelectedVars.Add("WIND");
                        if (option) lstSelectedVars.Add("DWIN");
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
                
            //create a CMIP6 object
            CMIPseries = new CMIP6Series(_begYear, _endYear, _scenario, _pathway,
                            lstSelectedVars, lstSelectedGrid, GridBndry);

            return true;
        }

        public CMIP6Series ScenarioParms()
        {
            return CMIPseries;
        }

        public bool ValidFormEntry()
        {
            return isValidEntry;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public Dictionary<string, bool> OptionVars()
        {
            return dictOptVars;
        }
        
        public List<string> SelectedVars()
        {
            return lstSelectedVars;
        }

        private void cboPathway_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pathway = cboPathway.SelectedItem.ToString();
            grpSSP.Text = _pathway;

            string info;
            if (dictSSP.TryGetValue(_pathway, out info))
            {
                lblSSP.Text = crlf + " " + info;
            }
            else
                lblSSP.Text = " ";

            if (cboPathway.SelectedItem.ToString().Contains("Historical"))
            {
                numYearFrom.Minimum = 1950;
                numYearFrom.Maximum = 2014;
                numYearTo.Minimum = 1950;
                numYearTo.Maximum = 2014;
                numYearTo.Value = 2014;
                numYearFrom.Value = 2014;
            }
            else
            {
                numYearFrom.Minimum = 2015;
                numYearFrom.Maximum = 2100;
                numYearTo.Minimum = 2015;
                numYearTo.Maximum = 2100;
                numYearTo.Value = 2015;
                numYearFrom.Value = 2015;
            }
        }

        private void cboScenario_SelectedIndexChanged(object sender, EventArgs e)
        {
            _scenario = cboScenario.SelectedItem.ToString();

            grpModel.Text = _scenario;
            List<string> info;
            if (dictGCM.TryGetValue(_scenario, out info))
            {
                if (!string.IsNullOrEmpty(info[1]))
                    lblGCM.Text = crlf + " Institution: " + info[0].ToString() + crlf+ crlf +
                             " Reference: " + info[1].ToString();
                else
                    lblGCM.Text = crlf + " Institution: " + info[0].ToString();
            }
            else
            {
                lblGCM.Text = crlf + " Institution: " + crlf + crlf + " Reference: ";
            }
        }

        private void CMIP6link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://doi.org/10.7917/OFSG3345");
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
        
        private void numPercentMiss_ValueChanged(object sender, EventArgs e)
        {
            PercentMiss = Convert.ToInt32(numPercentMiss.Value);
        }
    }
}
