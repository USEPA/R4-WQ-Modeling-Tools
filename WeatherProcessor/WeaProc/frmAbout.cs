using System;
using System.Reflection;
using System.Windows.Forms;

namespace NCEIData
{
    partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.textBoxDescription.Text = AssemblyDescription;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                return ("Model Weather Data Processor-Allows downloading available meteorological stations and data from the global hourly(ISD), Global Historical Climate Network(GHCN), COOP Hourly Rainfall, North American Land Data Assimilation System (NLDAS), Global Land Data Assimilation System (GLDAS), Tropical Rainfall Measuring Mission (TRMM) datasets and regionally downscaled climate scenario (CMIP5-EDDE and CMIP6) datasets.  Includes spatial analysis of annual time series." + "\r\n\r\n\r\n" + "Powered by DotSpatial and BASINS V4.5 Utilities" + "\r\n\r\n" + "Contact hummel.paul@epa.gov for bugs and suggestions.");
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                return "Copyright US EPA Region 4";
            }
        }

        public string AssemblyCompany
        {
            get
            {
                return ("U.S. EPA Region 4" + "\r\n" + "Surface Water Protection Branch, Water Division");
            }
        }
        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
