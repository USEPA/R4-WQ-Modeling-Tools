using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeaDB;
using WeaModelSDB;

namespace WeaGen
{
    public partial class frmWeaGen : Form
    {
        private string sdbModelFile, outputSDB = string.Empty;
        private DataTable tblSDB;
        private string errmsg = string.Empty;
        private string mssg = string.Empty;
        private string crlf = Environment.NewLine;
        private WeaModelDB mdlSDB;
        private WeaSDB cSDB;
        private string modelDir;
        private DateTime BegDate, EndDate;
        private List<string> SelectedSeries = new List<string>();
        private Random rand;
        private List<string> series = new List<string>() { "MEAN", "STDV" };
        private int nhar = 12, nhfit = 2, ndiv = 2, nhperiod = 24;
        List<int> ndays = new List<int>() { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private StreamWriter wrlog;
        private const string tblName = "WeaGen";
        private SortedDictionary<string, string> GeneratedSeries;
        private DateTime DateBeg, DateEnd;

        public frmWeaGen(StreamWriter _wrlog, string _sdbfile)
        {
            InitializeComponent();
            this.sdbModelFile = _sdbfile;
            this.wrlog = _wrlog;
        }
        private void dateStart_ValueChanged(object sender, EventArgs e)
        {
            BegDate = dateStart.Value;
        }
        private void dateEnd_ValueChanged(object sender, EventArgs e)
        {
            EndDate = dateEnd.Value;
        }
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            btnClearSelection.Enabled = true;
            dgvSDB.SelectAll();
            btnGenerate.Enabled = true;
        }
        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            dgvSDB.ClearSelection();
            btnClearSelection.Enabled = false;
            btnGenerate.Enabled = false;
        }
        private void dgvSDB_Click(object sender, EventArgs e)
        {
            if (dgvSDB.SelectedRows.Count > 0)
            {
                btnClearSelection.Enabled = true;
                if (string.IsNullOrEmpty(outputSDB.Trim()))
                    btnGenerate.Enabled = false;
                else
                    btnGenerate.Enabled = true;
            }
            else
            {
                btnClearSelection.Enabled = false;
                btnGenerate.Enabled = false;
            }
        }
        private SortedDictionary<string, List<string>> GetSelectedStationVariable()
        {
            SortedDictionary<string, List<string>> dictStaVars = new SortedDictionary<string, List<string>>();
            string sta, svar, sta_svar;
            try
            {
                SelectedSeries.Clear();
                foreach (DataGridViewRow dr in dgvSDB.SelectedRows)
                {
                    sta = Convert.ToString(dr.Cells["STATION_ID"].Value);
                    svar = Convert.ToString(dr.Cells["METVAR"].Value);
                    sta_svar = sta + ":" + svar;
                    SelectedSeries.Add(sta_svar);
                    if (!dictStaVars.ContainsKey(sta))
                    {
                        List<string> pcode = new List<string>();
                        pcode.Add(svar);
                        dictStaVars.Add(sta, pcode);
                        pcode = null;
                    }
                    else
                    {
                        List<string> pcode = new List<string>();
                        dictStaVars.TryGetValue(sta, out pcode);
                        pcode.Add(svar);
                        dictStaVars.Remove(sta);
                        dictStaVars.Add(sta, pcode);
                        pcode = null;
                    }
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error getting site-variable data from datatable !" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dictStaVars;
        }
        private void lblOut_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblOut.Text.Trim())) return;
            if (!File.Exists(lblOut.Text))
            {
                string defaultdb = Path.Combine(Application.StartupPath, "WeaWDM.sqlite");
                File.Copy(defaultdb, lblOut.Text);
            }
            outputSDB = lblOut.Text;
        }
        private void dgvSDB_MouseClick(object sender, MouseEventArgs e)
        {
            if (dgvSDB.SelectedRows.Count > 0)
            {
                btnClearSelection.Enabled = true;
                btnGenerate.Enabled = true;
            }
            else
            {
                btnClearSelection.Enabled = false;
                btnGenerate.Enabled = false;
            }
        }
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(outputSDB))
            {
                MessageBox.Show("Please specify an output database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;

            string site = string.Empty, svar = string.Empty;
            //dictModel - model parameters, dictStaVars - pcodes for station
            SortedDictionary<string, double> dictModel = new SortedDictionary<string, double>();
            SortedDictionary<string, List<string>> dictStaVars = new SortedDictionary<string, List<string>>();
            List<string> pcodes = new List<string>();

            try
            {
                dictStaVars = GetSelectedStationVariable();
                int nsites = dictStaVars.Count;
                int isite = 0;
                int nSeries = 0;

                //iterate on station
                //dictStaVars is list of pcodes for each station
                foreach (var series in dictStaVars)
                {
                    isite++;
                    site = series.Key;
                    dictStaVars.TryGetValue(site, out pcodes);

                    //10262020 ignore SOLR and PEVT for now, need to work on this
                    //if (pcodes.Contains("SOLR")) pcodes.Remove("SOLR");
                    //if (pcodes.Contains("PEVT")) pcodes.Remove("PEVT");

                    //SiteSeries - generated station series for each pcode
                    Dictionary<string, SortedDictionary<DateTime, double>> SiteSeries = new
                         Dictionary<string, SortedDictionary<DateTime, double>>();

                    //iterate on pcodes
                    foreach (var pcode in pcodes)
                    {
                        //10262020 skip SOLR and PEVT for now
                        if (pcode.Contains("SOLR") || pcode.Contains("PEVT"))
                        {
                            MessageBox.Show("Generating SOLR not implemented yet!", "Information",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        svar = pcode;
                        mssg = "Generating stochastic series for " + site + ":" + svar +
                            " (" + isite.ToString() + " of " + nsites.ToString() + ")";

                        WriteStatus(mssg);
                        WriteLogFile(mssg);

                        if (!((dictModel = mdlSDB.ReadModelParameters(site, svar)) == null))
                        {
                            //generate series using stochastic model
                            SortedDictionary<DateTime, double> GenSeries = new SortedDictionary<DateTime, double>();
                            SortedDictionary<Int32, List<double>> dictHarMoments = new SortedDictionary<Int32, List<double>>();
                            switch (svar)
                            {
                                case "PREC":
                                    //use markov model
                                    GenSeries = GenerateHourlySeriesByRainModel(site, svar, BegDate, EndDate, dictModel);
                                    int num = GenSeries.Count;
                                    break;
                                case "PRCP":
                                    //use markov model
                                    GenSeries = GenerateDailySeriesByRainModel(site, svar, BegDate, EndDate, dictModel);
                                    break;
                                case "TMAX":
                                    //use AR model
                                    dictHarMoments = GeneratePeriodicDailyMoments(site, svar, dictModel, 366, 6);
                                    GenSeries = GenerateDailySeriesByWeaModel(site, svar, BegDate, EndDate,
                                            dictModel, dictHarMoments);
                                    break;
                                case "TMIN":
                                    //use AR model
                                    dictHarMoments = GeneratePeriodicDailyMoments(site, svar, dictModel, 366, 6);
                                    GenSeries = GenerateDailySeriesByWeaModel(site, svar, BegDate, EndDate,
                                            dictModel, dictHarMoments);
                                    break;
                                case "SOLR":
                                    MessageBox.Show("Generating SOLR not implemented yet!", "Information",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                case "PEVT":
                                    MessageBox.Show("Generating PEVT not implemented yet!", "Information",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                default:
                                    //use AR model
                                    dictHarMoments = GeneratePeriodicHourlyMoments(site, svar, dictModel, 24, 6);
                                    GenSeries = GenerateHourlySeriesByWeaModel(site, svar, BegDate, EndDate,
                                            dictModel, dictHarMoments);
                                    break;
                            }

                            //add to dictionary of series for the site
                            if (!(GenSeries == null))
                            {
                                if (!SiteSeries.ContainsKey(svar))
                                {
                                    SiteSeries.Add(svar, GenSeries);
                                    string sitevar = site + ":+svar";
                                    string period = BegDate.ToString() + "," + EndDate.ToString();
                                    if (!GeneratedSeries.ContainsKey(sitevar))
                                    {
                                        GeneratedSeries.Add(sitevar, period);
                                    }
                                }
                            }

                            GenSeries = null;
                            nSeries++;
                        }
                    }
                    //end loop pcodes for station

                    //write generated series in csvfiles
                    if (chkSave.Checked)
                        WriteCSVFile(modelDir, site, SiteSeries, BegDate);

                    //upload generated series to sqlite
                    UploadSiteSeries(modelDir, site, SiteSeries, BegDate);

                    SiteSeries = null;
                }
                //end loop station
                dictStaVars = null;
                dictModel = null;

                WriteStatus("Ready ...");
                if (nSeries > 0)
                {
                    mssg = "Generated " + nSeries.ToString() + " timeseries for " + nsites.ToString() + " site(s)!" +
                     Environment.NewLine + Environment.NewLine +
                    "Files save in " + modelDir + " (*_gen.csv)";
                }
                else
                {
                    mssg = "No series were generated!";
                }
                WriteLogFile(mssg);
                MessageBox.Show(mssg, "Information!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                errmsg = "Error generating timeseries for " + site + ":" + svar + "!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                WriteLogFile(mssg);
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            dictModel = null; dictStaVars = null;
            Cursor.Current = Cursors.Default;
        }
        private void btnOut_Click(object sender, EventArgs e)
        {
            string ext = ".sdb";
            string filter = "Output SQLite Database (*.sdb)|*.sdb|All files (*.*)|*.*";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = false;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = modelDir;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select or Create new output SQLite database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    lblOut.Text = sFile;
                    outputSDB = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    outputSDB = sFile;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("sfile=" + sFile);

                if (!File.Exists(sFile))
                {
                    string defaultdb = Path.Combine(Application.StartupPath, "WeaWDM.sqlite");
                    File.Copy(defaultdb, sFile);
                }
                outputSDB = sFile;
            }

            //initialize cSDB
            cSDB = new WeaSDB(outputSDB);
            if (!cSDB.TableExist(tblName))
                cSDB.CreateTable(tblName);

        }
        private void frmWeaGen_Load(object sender, EventArgs e)
        {
            mdlSDB = new WeaModelDB(wrlog, sdbModelFile);
            tblSDB = new DataTable();
            tblSDB = mdlSDB.GetStationVariables();
            dgvSDB.DataSource = null;
            dgvSDB.DataSource = tblSDB;
            lblSDB.Text = sdbModelFile;
            dgvSDB.ClearSelection();
            //string sfile = Path.GetFileNameWithoutExtension(sdbFile)+ ".sdb";
            //outSDB = Path.Combine(Path.GetDirectoryName(sdbFile), sfile);
            //lblOut.Text = outSDB;

            //initialize dates
            DateTime enddt = new DateTime(DateTime.Now.Year - 1, 12, 31);
            if (DateTime.Now.Month > 6)
                enddt = new DateTime(DateTime.Now.Year, 6, 30);
            dateEnd.Value = enddt;
            dateStart.Value = new DateTime(enddt.Year - 20, 1, 1);
            BegDate = dateStart.Value;
            EndDate = dateEnd.Value;

            //init controls
            btnClearSelection.Enabled = false;
            btnGenerate.Enabled = false;
            btnSelectAll.Enabled = true;

            //initialize csv outfile
            modelDir = Path.Combine(Application.StartupPath, "data");
            string spath = Path.GetDirectoryName(sdbModelFile);
            string sfile = Path.GetFileNameWithoutExtension(sdbModelFile);
            sfile = Path.Combine(spath, sfile + ".sdb");
            //lblOut.Text = sfile;
            //outputSDB = sfile;

            //initialize output dictionary
            GeneratedSeries = new SortedDictionary<string, string>();
        }
        private void frmWeaGen_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close databases
            mdlSDB.CloseDataBase();
            mdlSDB = null;
            if (!(cSDB == null))
            {
                cSDB.CloseDataBase();
                cSDB = null;
            }
            //clear tables
            tblSDB.Clear();
            tblSDB = null;

            GeneratedSeries = null;
        }

        #region "Stochastic Generation"

        public SortedDictionary<DateTime, double> GenerateHourlySeriesByRainModel(string site, string svar,
                DateTime BegDate, DateTime EndDate, SortedDictionary<string, double> rainModel)
        {
            //-----------------------------------------------------------------------
            // Generation of dry wet sequence
            // Given a dry day, generate uniform random number between 0,1(prob)
            //  if prob > PD | W(prob of dry given wet day), current is dry
            //  if prob < PD | W current is wet
            //
            // Given a wet day, generate random prob
            //  if prob > PW | W(prob of wet given wet), current is dry
            //  if prob < PW | W current is wet
            //-----------------------------------------------------------------------

            // initialize random number generator
            rand = new Random();
            try
            {
                SortedDictionary<DateTime, double> genSeries = new SortedDictionary<DateTime, double>();

                //Markov parameters for site and variable (PREC for ISD, GHCN, HPCP for hourly)
                //initialize variables for model
                double pww = 0.0, pdw = 0.0; //, pdd=0.0, pdw=0.0;
                double gama = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0;
                double rain = 0.0, prerain = 0.0;

                //generate series for BegDate to EndDate, spin up one year
                DateTime InitDate = BegDate.AddYears(-1);
                DateTime dt = InitDate;

                int mon;
                while (DateTime.Compare(dt, EndDate) <= 0)
                {
                    DateTime curdt = dt;
                    DateTime predt = curdt.AddHours(-1);
                    mon = predt.Month;

                    if (!genSeries.TryGetValue(predt, out prerain))
                        prerain = 0.0;

                    //get parameters
                    rainModel.TryGetValue("gamma" + mon, out gama);
                    rainModel.TryGetValue("alpha" + mon, out alpha);
                    rainModel.TryGetValue("beta" + mon, out beta);
                    rainModel.TryGetValue("pdw" + mon, out pdw);
                    rainModel.TryGetValue("pww" + mon, out pww);
                    prob = GenerateRandomProb();

                    if (prerain <= 0)                         //previous hour dry
                    {
                        if (prob > pdw)                       //current hour is dry
                            rain = 0.0;
                        else
                            rain = GenerateHourRain(alpha, beta); //current hour is wet, generate rain
                    }
                    else                                      //previous hour wet
                    {
                        if (prob > pww)                       //current hour is dry
                            rain = 0.0;                       //dry hour
                        else                                  //current hour is wet, generate
                            rain = GenerateHourRain(alpha, beta);
                    }

                    //exclude initialization in output
                    if (DateTime.Compare(curdt, BegDate) >= 0)
                        genSeries.Add(curdt, rain);
                    //if (rain > 0)
                    //Debug.WriteLine("Prb={0},PDW={1},PWW={2},{3},{4}", 
                    //  prob.ToString("F3"),pdw.ToString("F3"), pww.ToString("F3"),curdt.ToString(), rain.ToString("F5"));
                    dt = dt.AddHours(1);
                }

                //cleanup
                //rainModel = null;
                Cursor.Current = Cursors.Default;
                return genSeries;
            }
            catch (Exception ex)
            {
                string msg = "Error in generating hourly rainfall records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }

        public SortedDictionary<DateTime, double> GenerateDailySeriesByRainModel(string site, string svar,
        DateTime BegDate, DateTime EndDate, SortedDictionary<string, double> rainModel)
        {
            //-----------------------------------------------------------------------
            // Generation of dry wet sequence
            // Given a dry day, generate uniform random number between 0,1(prob)
            //  if prob > PD | W(prob of dry given wet day), current is dry
            //  if prob < PD | W current is wet
            //
            // Given a wet day, generate random prob
            //  if prob > PW | W(prob of wet given wet), current is dry
            //  if prob < PW | W current is wet
            //-----------------------------------------------------------------------

            // initialize random number generator
            rand = new Random();
            try
            {
                SortedDictionary<DateTime, double> genSeries = new SortedDictionary<DateTime, double>();

                //Markov parameters for site and variable (PREC for ISD, GHCN, HPCP for hourly)
                //initialize variables for model
                double pww = 0.0, pdw = 0.0; //, pdd=0.0, pdw=0.0;
                double gama = 0.0, prob = 0.0;
                double alpha = 0.0, beta = 0.0;
                double rain = 0.0, prerain = 0.0;

                //generate series for BegDate to EndDate, spin up one year
                DateTime InitDate = new DateTime(BegDate.Year - 1, BegDate.Month, BegDate.Day, 0, 0, 0);
                DateTime dt = InitDate;

                int mon;
                while (DateTime.Compare(dt, EndDate) <= 0)
                {
                    DateTime curdt = dt;
                    DateTime predt = curdt.AddDays(-1);
                    mon = predt.Month;

                    if (!genSeries.TryGetValue(predt, out prerain))
                        prerain = 0.0;

                    //get parameters
                    rainModel.TryGetValue("dlygamma" + mon, out gama);
                    rainModel.TryGetValue("dlyalpha" + mon, out alpha);
                    rainModel.TryGetValue("dlybeta" + mon, out beta);
                    rainModel.TryGetValue("dlypdw" + mon, out pdw);
                    rainModel.TryGetValue("dlypww" + mon, out pww);
                    prob = GenerateRandomProb();

                    if (prerain <= 0)                         //previous day dry
                    {
                        if (prob > pdw)                       //current day is dry
                            rain = 0.0;
                        else
                            rain = GenerateHourRain(alpha, beta); //current day is wet, generate rain
                    }
                    else                                      //previous hour wet
                    {
                        if (prob > pww)                       //current hour is dry
                            rain = 0.0;                       //dry hour
                        else                                  //current hour is wet, generate
                            rain = GenerateHourRain(alpha, beta);
                    }
                    if (DateTime.Compare(curdt, BegDate) >= 0)
                        genSeries.Add(curdt, rain);

                    //if (rain > 0)
                    //Debug.WriteLine("Prb={0},PDW={1},PWW={2},{3},{4}", 
                    //prob.ToString("F3"),pdw.ToString("F3"), pww.ToString("F3"),curdt.ToString(), rain.ToString("F5"));

                    dt = dt.AddDays(1);
                }

                //cleanup
                //rainModel = null;
                Cursor.Current = Cursors.Default;
                return genSeries;
            }
            catch (Exception ex)
            {
                string msg = "Error in generating daily rainfall records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
        }

        public SortedDictionary<DateTime, double> GenerateHourlySeriesByWeaModel(string site, string svar,
                DateTime BegDate, DateTime EndDate, SortedDictionary<string, double> weaModel,
        SortedDictionary<Int32, List<double>> dictHarMoments)
        {
            Debug.WriteLine("Entering GenerateHourly for " + svar);
            // initialize random number generator
            rand = new Random();

            SortedDictionary<DateTime, double> genSeries = new SortedDictionary<DateTime, double>();
            try
            {
                double xpre = 0.0, xsim = 0.0, theta = 0.0, xcur = 0.0;
                double avg = 0.0, std = 0.0, sigma = 0.0, sigma2 = 0.0;
                double ysim = 0.0;
                List<double> stats = new List<double>();

                //get AR parameter for site var
                weaModel.TryGetValue("varerr", out sigma2);
                sigma = Math.Sqrt(sigma2);
                weaModel.TryGetValue("ar1coeff", out theta);
                double prb = 0.0, noise = 0.0;

                //generate series for BegDate to EndDate, spin up one year
                DateTime InitDate = BegDate.AddYears(-1);
                DateTime dt = InitDate;

                int irec = 0;
                while (DateTime.Compare(dt, EndDate) <= 0)
                {
                    irec++;
                    DateTime curdt = dt;
                    DateTime predt = curdt.AddHours(-1);

                    prb = rand.NextDouble();

                    //get current harmonic moments
                    int monhr = 100 * dt.Month + dt.Hour;
                    dictHarMoments.TryGetValue(monhr, out stats);
                    avg = stats[0]; std = stats[1];

                    //generate noise
                    prb = rand.NextDouble();
                    noise = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, sigma, prb);

                    //get standardized residual of previous hour
                    //hresid.TryGetValue(ihrs - 1, out xpre);
                    xcur = theta * xpre + noise;
                    xsim = xcur * std + avg;

                    xsim = CheckedVariable(svar, xsim);

                    //if (irec<30)
                    //    Debug.WriteLine("dt={0},xpre={1},xcur={2},xsim={3}, ysim={4}", dt.ToString(),
                    //      xpre.ToString(), xcur.ToString(), xsim.ToString(), ysim.ToString());

                    //wri.WriteLine("{0},{1},{2},{3}", isamp.ToString(), dt.ToString(), 
                    //    xobs.ToString("F4"), xsim.ToString("F4"));
                    if (DateTime.Compare(curdt, BegDate) >= 0)
                        genSeries.Add(curdt, xsim);

                    xpre = xcur;
                    dt = dt.AddHours(1);
                } //end while
            }
            catch (Exception ex)
            {
                string msg = "Error in generating hourly weather records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
            return genSeries;
        }

        public SortedDictionary<DateTime, double> GenerateDailySeriesByWeaModel(string site, string svar,
                DateTime BegDate, DateTime EndDate, SortedDictionary<string, double> weaModel,
        SortedDictionary<Int32, List<double>> dictHarMoments)
        {
            // initialize random number generator
            rand = new Random();

            SortedDictionary<DateTime, double> genSeries = new SortedDictionary<DateTime, double>();
            try
            {
                double xpre = 0.0, xsim = 0.0, theta = 0.0, xcur = 0.0;
                double avg = 0.0, std = 0.0, sigma = 0.0, sigma2 = 0.0;
                List<double> stats = new List<double>();
                List<double> ysim = new List<double>();

                //get AR parameter for site var
                weaModel.TryGetValue("dlyvarerr", out sigma2);
                sigma = Math.Sqrt(sigma2);
                weaModel.TryGetValue("dlyar1coeff", out theta);
                double prb = 0.0, noise = 0.0;

                //generate series for BegDate to EndDate, spin up one year
                DateTime InitDate = new DateTime(BegDate.Year - 1, BegDate.Month, BegDate.Day, 0, 0, 0);
                DateTime dt = InitDate;

                while (DateTime.Compare(dt, EndDate) <= 0)
                {
                    DateTime curdt = dt;
                    DateTime predt = curdt.AddHours(-1);

                    prb = rand.NextDouble();

                    //get current harmonic moments
                    int monhr = 100 * dt.Month + dt.Hour;
                    dictHarMoments.TryGetValue(monhr, out stats);
                    avg = stats[0]; std = stats[1];

                    //generate noise
                    prb = rand.NextDouble();
                    noise = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, sigma, prb);

                    //get standardized residual of previous hour
                    xcur = theta * xpre + noise;
                    xsim = xcur * std + avg;
                    xpre = xcur;

                    xsim = CheckedVariable(svar, xsim);

                    //wri.WriteLine("{0},{1},{2},{3}", isamp.ToString(), dt.ToString(), 
                    //    xobs.ToString("F4"), xsim.ToString("F4"));
                    if (DateTime.Compare(curdt, BegDate) >= 0)
                        genSeries.Add(curdt, xsim);

                    dt = dt.AddDays(1);
                } //end while
            }
            catch (Exception ex)
            {
                string msg = "Error in generating hourly weather records for site " + site + ":" + svar;
                ShowError(msg, ex);
                return null;
            }
            return genSeries;
        }
        private SortedDictionary<Int32, List<double>> GeneratePeriodicHourlyMoments(string site, string svar,
            SortedDictionary<string, double> weaModel, int nhperiod, int nhfit)
        {
            //routine generates the periodic hourly moments
            Debug.WriteLine("Entering GeneratePeriodicMoments()");
            try
            {
                //generate monthly by hour harmonics of mean and stdev, routine OK
                SortedDictionary<Int32, List<double>> dictHarMoments = new SortedDictionary<Int32, List<double>>();
                List<double> phAvg = new List<double>();
                List<double> phStd = new List<double>();

                int nperiod = nhperiod, nh = nhfit;
                string kv;

                for (int kser = 0; kser < 2; kser++)
                {
                    for (int jmo = 1; jmo <= 12; jmo++)
                    {
                        double[] acoef = new double[nh];
                        double[] bcoef = new double[nh];
                        double mean = 0.0;

                        //get harmonic coeffients from model dictionary
                        //format of key acoef<hh>_<mon>, hh-harmonic,mon=month
                        for (int j = 0; j < nh; j++)
                        {
                            int ihar = j + 1;
                            kv = series[kser] + ":acoef" + ihar + "_" + jmo;
                            weaModel.TryGetValue(kv, out acoef[j]);
                            kv = series[kser] + ":bcoef" + ihar + "_" + jmo;
                            weaModel.TryGetValue(kv, out bcoef[j]);
                            //Debug.WriteLine("{0},{1},{2}", series[kser], acoef[j].ToString(), bcoef[j].ToString());
                        }
                        kv = series[kser] + ":mean_" + jmo;
                        weaModel.TryGetValue(kv, out mean);

                        for (int t = 1; t <= nperiod; t++)
                        {
                            double sum = 0.0, radian = 0.0;
                            for (int j = 0; j < nh; j++)
                            {
                                int ihar = j + 1;
                                radian = (2.0 * Math.PI * ihar * t) / nperiod;
                                sum += acoef[j] * Math.Cos(radian) + bcoef[j] * Math.Sin(radian);
                            }
                            if (kser == 0)
                                phAvg.Add(mean + sum);
                            else
                                phStd.Add(mean + sum);
                        }
                        acoef = null; bcoef = null;
                    }//loop for month
                }
                //end loop series
                //Debug.WriteLine("Mean and SD harmonic series for "+site+":"+svar);
                //Debug.WriteLine("Month-Hr, Mean, Std");
                int idx = -1;
                List<double> lst;
                for (int i = 1; i <= 12; i++)
                {
                    for (int j = 0; j < 24; j++)
                    {
                        idx++;
                        int hrmo = i * 100 + j;
                        //Debug.WriteLine("{0},{1},{2}", hrmo.ToString(),
                        //       phAvg[idx].ToString("F4"), phStd[idx].ToString("F4"));
                        lst = new List<double>();
                        lst.Add(phAvg[idx]); lst.Add(phStd[idx]);
                        dictHarMoments.Add(hrmo, lst);
                        lst = null;
                    }
                }
                return dictHarMoments;
            }
            catch (Exception ex)
            {
                ShowError("Error generating periodic hourly means and variances!", ex);
                return null;
            }
        }
        private SortedDictionary<Int32, List<double>> GeneratePeriodicDailyMoments(string site, string svar,
            SortedDictionary<string, double> weaModel, int nhperiod, int nhfit)
        {
            //routine generates the periodic hourly moments
            Debug.WriteLine("Entering GeneratePeriodicMoments()");
            try
            {
                //generate harmonics of mean and stdev, routine OK
                SortedDictionary<Int32, List<double>> dictHarMoments = new SortedDictionary<Int32, List<double>>();
                List<double> phAvg = new List<double>();
                List<double> phStd = new List<double>();

                int nperiod = nhperiod, nh = nhfit;
                string kv;

                for (int kser = 0; kser < 2; kser++)
                {
                    double[] acoef = new double[nh];
                    double[] bcoef = new double[nh];
                    double mean = 0.0;

                    //get harmonic coeffients from model dictionary
                    //format of key acoef<hh>_<mon>, hh-harmonic,mon=month
                    for (int j = 0; j < nh; j++)
                    {
                        int ihar = j + 1;
                        kv = series[kser] + ":dlyacoef" + ihar;
                        weaModel.TryGetValue(kv, out acoef[j]);
                        kv = series[kser] + ":dlybcoef" + ihar;
                        weaModel.TryGetValue(kv, out bcoef[j]);
                        Debug.WriteLine("{0},{1},{2}", series[kser], acoef[j].ToString(), bcoef[j].ToString());
                    }
                    kv = series[kser] + ":dlymean";
                    weaModel.TryGetValue(kv, out mean);

                    for (int t = 1; t <= nperiod; t++)
                    {
                        double sum = 0.0, radian = 0.0;
                        for (int j = 0; j < nh; j++)
                        {
                            int ihar = j + 1;
                            radian = (2.0 * Math.PI * ihar * t) / nperiod;
                            sum += acoef[j] * Math.Cos(radian) + bcoef[j] * Math.Sin(radian);
                        }
                        if (kser == 0)
                            phAvg.Add(mean + sum);
                        else
                            phStd.Add(mean + sum);
                    }
                    acoef = null; bcoef = null;
                }
                //end loop series
                //wri.WriteLine("Mean and SD harmonic series for "+site+":"+svar);
                //wri.WriteLine("Mean, Std");
                int idx = -1;
                List<double> lst;
                for (int i = 1; i <= 12; i++)
                {
                    if (i == 2) ndays[i] = 29;
                    for (int j = 1; j <= ndays[i]; j++)
                    {
                        idx++;
                        int daymo = i * 100 + j;
                        //wri.WriteLine("{0},{1},{2}", daymo.ToString(),
                        //        phAvg[idx].ToString("F4"), phStd[idx].ToString("F4"));
                        lst = new List<double>();
                        lst.Add(phAvg[idx]); lst.Add(phStd[idx]);
                        dictHarMoments.Add(daymo, lst);
                        lst = null;
                    }
                }
                return dictHarMoments;
            }
            catch (Exception ex)
            {
                ShowError("Error generating periodic daily means and variances!", ex);
                return null;
            }
        }
        private double CheckedVariable(string svar, double xsim)
        {
            double xval;

            if (xsim < 0.0) xval = 0.0;
            else xval = xsim;

            switch (svar.ToUpper().Trim())
            {
                case "WIND":
                    break;
                case "WNDD":
                    if (xsim < 10) xval = 10.0;
                    if (xsim > 360) xval = 360.0;
                    break;
                case "CLOU":
                    xval = ReClassCloud(xsim);
                    break;
                case "SOLR":
                    break;
                case "LRAD":
                    break;
                case "PEVT":
                    break;
                case "DSOL":
                    break;
                default:
                    break;
            }
            return xval;
        }
        private double ReClassCloud(double sdat)
        {
            double clou = 0.0;

            if (sdat < 0.5)
                clou = 0.0;
            else if (sdat < 1.75)
                clou = 1.0;
            else if (sdat < 3.25)
                clou = 2.5;
            else if (sdat < 4.5)
                clou = 4.0;
            else if (sdat < 5.5)
                clou = 5.0;
            else if (sdat < 6.75)
                clou = 6.0;
            else if (sdat < 8.25)
                clou = 7.5;
            else if (sdat < 9.5)
                clou = 9.0;
            else
                clou = 10.0;
            return clou;
        }
        private double GenerateRandomProb()
        {
            return rand.NextDouble();
        }
        private double GenerateHourRain(double alpha, double beta)
        {
            double rate = 1.0 / beta;
            double prb = rand.NextDouble();
            double rain = MathNet.Numerics.Distributions.Gamma.InvCDF(alpha, rate, prb);
            if (rain < 0) rain = 0.0;
            return rain;
        }

        #endregion "Stochastic Generation"

        #region "Output"
        private void UploadSiteSeries(string OutPath, string site,
            Dictionary<string, SortedDictionary<DateTime, double>> dictSeries, DateTime StartDate)
        {
            Cursor.Current = Cursors.WaitCursor;
            SortedDictionary<DateTime, double> tseries = new SortedDictionary<DateTime, double>();

            List<string> pcodes = dictSeries.Keys.ToList();
            //debug
            foreach (var s in pcodes)
                Debug.WriteLine("Station={0}, PCode={1}", site.ToString(), s.ToString());

            int nvars = pcodes.Count();
            try
            {
                int ivar = 0;
                foreach (var kv in dictSeries)
                {
                    string svar = kv.Key;
                    if (dictSeries.TryGetValue(svar, out tseries))
                    {
                        ivar++;
                        mssg = "Uploading " + site + ":" + svar + " records (" + (ivar).ToString() +
                                " of " + nvars.ToString() + ")";
                        WriteStatus(mssg);
                        WriteLogFile(mssg);
                        //cSDB.DeleteRecordsFromMetTable(tblName, tseries, svar, site);
                        //cSDB.InsertRecordsInMetTable(tblName, tseries, svar, site);
                        //103020
                        //get period of record for svar and site
                        int nRecsInDB = cSDB.GetPeriodOfRecord(tblName, svar, site);
                        //if nrecs > 0, get begin and ending dates and only upload records not in database
                        //else upload all records to database
                        if (nRecsInDB > 0)
                        {
                            DateBeg = cSDB.BeginRecordDate();
                            DateEnd = cSDB.EndingRecordDate();
                            tseries = cSDB.FilterRecordsToUpload(DateBeg, DateEnd, tseries);
                            cSDB.InsertRecordsInMetTable(tblName, tseries, svar, site);
                        }
                        else
                        {
                            //insert series
                            //cSDB.DeleteRecordsFromMetTable(tblName, dictSeries, svar, site);
                            cSDB.InsertRecordsInMetTable(tblName, tseries, svar, site);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error uploading generated timeseries for " + site + "!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            tseries = null;
            WriteStatus("Ready ..");
            Cursor.Current = Cursors.Default;
        }

        private void WriteCSVFile(string OutPath, string site,
            Dictionary<string, SortedDictionary<DateTime, double>> dictSeries, DateTime StartDate)
        {
            Cursor.Current = Cursors.WaitCursor;

            string csvfile = Path.Combine(OutPath, site + "_gen.csv");
            SortedDictionary<DateTime, double> tseries = new SortedDictionary<DateTime, double>();

            List<string> pcodes = dictSeries.Keys.ToList();
            //debug
            foreach (var s in pcodes) Debug.WriteLine("Station={0}, PCode={1}", site.ToString(), s.ToString());

            DateTime dt;
            double dval = 0.0;
            try
            {
                using (StreamWriter sr = new StreamWriter(csvfile, false))
                {
                    sr.AutoFlush = true;
                    WriteHeader(sr);

                    foreach (var kv in dictSeries)
                    {
                        string svar = kv.Key;
                        if (dictSeries.TryGetValue(svar, out tseries))
                        {
                            foreach (var kv1 in tseries)
                            {
                                dt = kv1.Key;
                                dval = kv1.Value;
                                //dont write spin up
                                if (DateTime.Compare(dt, StartDate) >= 0)
                                {
                                    sr.WriteLine("{0},{1},{2},{3}",
                                        site, svar, dt.ToString(), FormatVariable(svar, dval));
                                    sr.Flush();
                                }
                            }
                        }
                    }
                    sr.Flush(); sr.Close();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error writing generated timeseries for " + site + "!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            tseries = null;
            Cursor.Current = Cursors.Default;
        }
        private string FormatVariable(string pcode, double value)
        {
            string sval = string.Empty;
            float val = Convert.ToSingle(value);
            switch (pcode.ToUpper())
            {
                case "PREC":
                    sval = val.ToString("F3");
                    break;
                case "PRCP":
                    sval = val.ToString("F3");
                    break;
                case "ATEM":
                    sval = val.ToString("F2");
                    break;
                case "TMAX":
                    sval = val.ToString("F2");
                    break;
                case "TMIN":
                    sval = val.ToString("F2");
                    break;
                case "DEWP":
                    sval = val.ToString("F2");
                    break;
                case "SOLR":
                    sval = val.ToString("F5");
                    break;
                case "LRAD":
                    sval = val.ToString("F5");
                    break;
                case "WIND":
                    sval = val.ToString("F3");
                    break;
                case "WNDD":
                    sval = val.ToString("F2");
                    break;
                case "WINDU":
                    sval = val.ToString("F3");
                    break;
                case "WINDV":
                    sval = val.ToString("F3");
                    break;
                case "CLOU":
                    sval = val.ToString("F2");
                    break;
                case "ATMP":
                    sval = val.ToString("F2");
                    break;
                case "PEVT":
                    sval = val.ToString("F5");
                    break;
            }
            return sval;
        }
        private void WriteHeader(StreamWriter wri)
        {
            string head = "Station_ID, Variable, DateTime, Value";
            wri.WriteLine(head);
            wri.Flush();
        }

        #endregion

        public void WriteLogFile(string msg)
        {
            wrlog.WriteLine(msg);
            wrlog.AutoFlush = true;
            wrlog.Flush();
        }
        private void WriteStatus(string msg)
        {
            statuslbl.Text = msg;
            statusStrip.Refresh();
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += crlf + ex.Message + crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
