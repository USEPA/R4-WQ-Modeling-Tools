#define debug
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NCEIData
{
    public partial class frmDownloadEDDE : Form
    {
        private int _begYear, _endYear;
        private string _scenario, _pathway;
        private Dictionary<string, bool> dictOptVars =
              new Dictionary<string, bool>();
        private List<string> lstSelectedVars = new List<string>();
        private List<string> lstSelectedGrid;

        private frmMain fMain;
        private int PercentMiss = 50;
        private bool isValidEntry = false;
        private int UTCShift;
        private string WDMFile;
        private CMIP6Series CMIPseries;
        private BoundingBox GridBndry;
        private SortedDictionary<string, string> dictGCM = new
                SortedDictionary<string, string>();
        private SortedDictionary<string, string> dictSSP = new
                SortedDictionary<string, string>();
        private string crlf = Environment.NewLine;

        public frmDownloadEDDE(frmMain _fmain, BoundingBox _bbox)
        {
            InitializeComponent();
            this.fMain = _fmain;
            WDMFile = fMain.WdmFile;
            GridBndry = _bbox;
            lstSelectedGrid = new List<string>(fMain.dictGages.Keys);
            InitializeForm();

        }
        private void InitializeForm()
        {
            if (!(dictSSP.Count > 0))
            {
                dictSSP.Add("RCP8.5", "The RCP8.5 scenario (Riahi et al. 2011) assumes high growth in population, energy demand, and emissions of greenhouse gases (GHGs), resulting in 8.5 W m−2 of radiative forcing at 2100 (relative to preindustrial conditions), and mean temperature increases over the conterminous U.S. of 3.2–6.6°C relative to 1976–2005 (Vose et al. 2017).");
                dictSSP.Add("Historical", "Historical experiment for the GCM model for the period 1995-2005");
            }

            dictGCM.Clear();
            dictGCM.Add("CESM", "National Center for Atmospheric Research (USA), Danabasoglu et al. (2020)  https://doi.org/10.22033/ESGF/CMIP6.10026  https://doi.org/10.22033/ESGF/CMIP6.2201");
            dictGCM.Add("GFDL-CM3", "NOAA-Geophysical Fluid Dynamics Laboratory(USA), Donner et al. (2011), The dynamical core, physical parameterizations, and basic simulation characteristics of the atmospheric AM3 of the GFDL Global Coupled Model CM3.Journal of CLimate, 24(130, doi: 10.1175 / 2011JCLI3955.1");

            string spath = Path.GetDirectoryName(WDMFile);
            btnOK.Enabled = true;
            optSolar.Checked = false;
            optWND.Checked = false;
            optTMP.Checked = false;
            optPCP.Checked = false;
            optRH.Checked = false;
            optPres.Checked = false;
            optCLO.Checked = false;

            optLWdown.Checked = false;
            optLWout.Checked = false;
            optSensible.Checked = false;
            optLatent.Checked = false;

            numPercentMiss.Value = PercentMiss;
            cboScenario.SelectedIndex = 0;
            cboPathway.SelectedIndex = 0;
        }

        private bool ProcessSelection()
        {
            _begYear = (int)numYearFrom.Value;
            _endYear = (int)numYearTo.Value;
            _scenario = cboScenario.SelectedItem.ToString();
            _pathway = cboPathway.SelectedItem.ToString();
            PercentMiss = Convert.ToInt32(numPercentMiss.Value);
            fMain.PercentMiss = PercentMiss;

            dictOptVars.Clear();
            dictOptVars.Add("PCP", optPCP.Checked);
            dictOptVars.Add("TMP", optTMP.Checked);
            dictOptVars.Add("DEW", optDEW.Checked);
            dictOptVars.Add("WND", optWND.Checked);   //wind speed
            dictOptVars.Add("WDR", optWDIR.Checked);  //wind direction
            dictOptVars.Add("HUM", optRH.Checked);    //RH
            dictOptVars.Add("SOL", optSolar.Checked); //shortwave radiation
            dictOptVars.Add("CLO", optCLO.Checked);   //cloud
            dictOptVars.Add("ATM", optPres.Checked);  //pressure
            //11/1/2024
            dictOptVars.Add("LDN", optLWdown.Checked);  //pressure
            dictOptVars.Add("LUP", optLWout.Checked);  //pressure
            dictOptVars.Add("HFS", optSensible.Checked);  //pressure
            dictOptVars.Add("HFL", optLatent.Checked);  //pressure

            bool option;
            lstSelectedVars.Clear();
            foreach (KeyValuePair<string, bool> kv in dictOptVars)
            {
                dictOptVars.TryGetValue(kv.Key, out option);
                switch (kv.Key)
                {
                    case "PCP":
                        if (option) lstSelectedVars.Add("PREC");
                        break;
                    case "TMP":
                        if (option)
                        {
                            lstSelectedVars.Add("ATEM");
                            //lstSelectedVars.Add("TMAX");
                            //lstSelectedVars.Add("TMIN");
                        }
                        break;
                    case "SOL":
                        if (option)
                        {
                            lstSelectedVars.Add("SOLR");
                        }
                        break;
                    case "CLO":
                        if (option)
                            lstSelectedVars.Add("CLOU");
                        break;
                    case "LDN":
                        if (option) lstSelectedVars.Add("LRAD"); //incoming LongWave
                        break;
                    case "LUP":
                        if (option) lstSelectedVars.Add("LWUP"); //outgoing LongWave
                        break;
                    case "HFS":
                        if (option) lstSelectedVars.Add("HFSS"); //sensible heat flux
                        break;
                    case "HFL":
                        if (option) lstSelectedVars.Add("HFLS"); //laten heat flux
                        break;
                    case "DEW":
                        if (option)
                            lstSelectedVars.Add("DEWP"); //dew point
                        break;
                    case "WND":
                        if (option) 
                            lstSelectedVars.Add("WIND"); //speed
                        break;
                    case "WDR":
                        if (option)
                            lstSelectedVars.Add("WNDD"); //direction
                        break;
                    case "ATM":
                        if (option)
                            lstSelectedVars.Add("ATMP"); //atm pressure
                        break;
                    case "HUM":
                        if (option)
                            lstSelectedVars.Add("HUMI"); //atm pressure
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

            //create a CMIP5 object (same as in CMIP6 datasource)
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
                numYearFrom.Minimum = 1995;
                numYearFrom.Maximum = 2005;
                numYearTo.Minimum = 1995;
                numYearTo.Maximum = 2005;
                numYearTo.Value = 2005;
                numYearFrom.Value = 2005;
            }
            else
            {
                numYearFrom.Minimum = 2025;
                numYearFrom.Maximum = 2100;
                numYearTo.Minimum = 2025;
                numYearTo.Maximum = 2100;
                numYearTo.Value = 2025;
                numYearFrom.Value = 2025;
            }
        }

        private void cboScenario_SelectedIndexChanged(object sender, EventArgs e)
        {
            _scenario = cboScenario.SelectedItem.ToString();

            grpModel.Text = _scenario;
            string info;
            if (dictGCM.TryGetValue(_scenario, out info))
            {
                if (!string.IsNullOrEmpty(info))
                    lblGCM.Text = crlf+ info.ToString();
                else
                    lblGCM.Text = string.Empty;
            }
            else
            {
                lblGCM.Text = string.Empty;
            }
        }

        public Dictionary<string, bool> OptionVars()
        {
            return dictOptVars;
        }

        public List<string> SelectedVars()
        {
            return lstSelectedVars;
        }

        public List<string> SelectedGrid()
        {
            return lstSelectedGrid;
        }

        private void EDDElink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.epa.gov/climate-research/epa-dynamically-downscaled-ensemble-edde");
        }

        private void btnUse_Click(object sender, EventArgs e)
        {
            frmEDDEuse fUse = new frmEDDEuse();
            fUse.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        public int TimeZoneShift()
        {
            return UTCShift;
        }
    }
}
