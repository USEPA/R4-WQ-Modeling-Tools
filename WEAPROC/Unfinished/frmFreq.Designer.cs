namespace FREQANAL
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuGetSeries = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReSelectGages = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTR = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCalc7Q10 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRegAnal = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEstimate = new System.Windows.Forms.ToolStripMenuItem();
            this.txtTR = new System.Windows.Forms.ToolStripTextBox();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuWhat = new System.Windows.Forms.ToolStripMenuItem();
            this.emailFernandezGlennepagovForProblemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabCtl = new System.Windows.Forms.TabControl();
            this.tabMap = new System.Windows.Forms.TabPage();
            this.spatialDockManager1 = new DotSpatial.Controls.SpatialDockManager();
            this.splitContainerLegend = new System.Windows.Forms.SplitContainer();
            this.tabCtlLegend = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.uxLegend = new DotSpatial.Controls.Legend();
            this.btnPointOK = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.uxMap = new DotSpatial.Controls.Map();
            this.tabGages = new System.Windows.Forms.TabPage();
            this.splitTab0 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutGages = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.yrTo = new System.Windows.Forms.NumericUpDown();
            this.lblTo = new System.Windows.Forms.Label();
            this.yrFrom = new System.Windows.Forms.NumericUpDown();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.txtDly = new System.Windows.Forms.TextBox();
            this.txtYr = new System.Windows.Forms.TextBox();
            this.lblDly = new System.Windows.Forms.Label();
            this.btnQueryNWIS = new System.Windows.Forms.Button();
            this.lblYr = new System.Windows.Forms.Label();
            this.lblSumd = new System.Windows.Forms.Label();
            this.cboDay = new System.Windows.Forms.ComboBox();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.dgvGages = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.tabSeries = new System.Windows.Forms.TabPage();
            this.splitTimeSeries = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.grpGageCbo = new System.Windows.Forms.GroupBox();
            this.optDly = new System.Windows.Forms.RadioButton();
            this.optAnn = new System.Windows.Forms.RadioButton();
            this.cboSeries = new System.Windows.Forms.ComboBox();
            this.lblUSGS = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvSeries = new System.Windows.Forms.DataGridView();
            this.dgvStats = new System.Windows.Forms.DataGridView();
            this.zedGraph = new ZedGraph.ZedGraphControl();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitResults = new System.Windows.Forms.SplitContainer();
            this.splitResultsUpper = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.btnRegion = new System.Windows.Forms.Button();
            this.tableLayoutCDFplot = new System.Windows.Forms.TableLayoutPanel();
            this.zedGraphCDF = new ZedGraph.ZedGraphControl();
            this.dgvProb = new System.Windows.Forms.DataGridView();
            this.pnlRegAnalysis = new System.Windows.Forms.Panel();
            this.grpBoxRegAnal = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.tabRegional = new System.Windows.Forms.TabPage();
            this.layoutRegional = new System.Windows.Forms.TableLayoutPanel();
            this.LayoutRegAnalTop = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btnModel = new System.Windows.Forms.Button();
            this.grpSelDepVar = new System.Windows.Forms.GroupBox();
            this.lstDepVar = new System.Windows.Forms.ListBox();
            this.grpRegGages = new System.Windows.Forms.GroupBox();
            this.dgvSelRegGages = new System.Windows.Forms.DataGridView();
            this.LayoutModelResults = new System.Windows.Forms.TableLayoutPanel();
            this.splitModelResults = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtRegInfo = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dgvModel = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.grpEstimate = new System.Windows.Forms.GroupBox();
            this.txtWet = new System.Windows.Forms.TextBox();
            this.txtUrb = new System.Windows.Forms.TextBox();
            this.txtFor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFlow = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRain = new System.Windows.Forms.TextBox();
            this.txtArea = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Area = new System.Windows.Forms.Label();
            this.btnEstMap = new System.Windows.Forms.Button();
            this.btnCalc = new System.Windows.Forms.Button();
            this.txtLon = new System.Windows.Forms.TextBox();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.lblLon = new System.Windows.Forms.Label();
            this.lblLat = new System.Windows.Forms.Label();
            this.dgvEstimate = new System.Windows.Forms.DataGridView();
            this.appManager = new DotSpatial.Controls.AppManager();
            this.spatialHeaderControl1 = new DotSpatial.Controls.SpatialHeaderControl();
            this.spatialToolStripPanel1 = new DotSpatial.Controls.SpatialToolStripPanel();
            this.pnlTabCtl = new System.Windows.Forms.Panel();
            this.mnuMain.SuspendLayout();
            this.tabCtl.SuspendLayout();
            this.tabMap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spatialDockManager1)).BeginInit();
            this.spatialDockManager1.Panel1.SuspendLayout();
            this.spatialDockManager1.Panel2.SuspendLayout();
            this.spatialDockManager1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLegend)).BeginInit();
            this.splitContainerLegend.Panel1.SuspendLayout();
            this.splitContainerLegend.Panel2.SuspendLayout();
            this.splitContainerLegend.SuspendLayout();
            this.tabCtlLegend.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabGages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitTab0)).BeginInit();
            this.splitTab0.Panel1.SuspendLayout();
            this.splitTab0.Panel2.SuspendLayout();
            this.splitTab0.SuspendLayout();
            this.tableLayoutGages.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yrTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yrFrom)).BeginInit();
            this.grpInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGages)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabSeries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitTimeSeries)).BeginInit();
            this.splitTimeSeries.Panel1.SuspendLayout();
            this.splitTimeSeries.Panel2.SuspendLayout();
            this.splitTimeSeries.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.grpGageCbo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).BeginInit();
            this.tabResults.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitResults)).BeginInit();
            this.splitResults.Panel1.SuspendLayout();
            this.splitResults.Panel2.SuspendLayout();
            this.splitResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitResultsUpper)).BeginInit();
            this.splitResultsUpper.Panel1.SuspendLayout();
            this.splitResultsUpper.Panel2.SuspendLayout();
            this.splitResultsUpper.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.tableLayoutCDFplot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProb)).BeginInit();
            this.pnlRegAnalysis.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabRegional.SuspendLayout();
            this.layoutRegional.SuspendLayout();
            this.LayoutRegAnalTop.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.grpSelDepVar.SuspendLayout();
            this.grpRegGages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelRegGages)).BeginInit();
            this.LayoutModelResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitModelResults)).BeginInit();
            this.splitModelResults.Panel1.SuspendLayout();
            this.splitModelResults.Panel2.SuspendLayout();
            this.splitModelResults.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModel)).BeginInit();
            this.tableLayoutPanel8.SuspendLayout();
            this.grpEstimate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEstimate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spatialHeaderControl1)).BeginInit();
            this.spatialToolStripPanel1.SuspendLayout();
            this.pnlTabCtl.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Dock = System.Windows.Forms.DockStyle.None;
            this.mnuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGetSeries,
            this.mnuReSelectGages,
            this.mnuTR,
            this.mnuCalc7Q10,
            this.mnuRegAnal,
            this.mnuEstimate,
            this.txtTR,
            this.mnuAbout});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(1113, 26);
            this.mnuMain.TabIndex = 0;
            this.mnuMain.Text = "menuStrip1";
            // 
            // mnuGetSeries
            // 
            this.mnuGetSeries.Name = "mnuGetSeries";
            this.mnuGetSeries.Size = new System.Drawing.Size(83, 23);
            this.mnuGetSeries.Text = "Query NWIS";
            this.mnuGetSeries.Visible = false;
            this.mnuGetSeries.Click += new System.EventHandler(this.mnuGetSeries_Click);
            // 
            // mnuReSelectGages
            // 
            this.mnuReSelectGages.Name = "mnuReSelectGages";
            this.mnuReSelectGages.Size = new System.Drawing.Size(103, 23);
            this.mnuReSelectGages.Text = "Re-Select Gages";
            this.mnuReSelectGages.Visible = false;
            this.mnuReSelectGages.Click += new System.EventHandler(this.mnuReSelectGages_Click);
            // 
            // mnuTR
            // 
            this.mnuTR.Name = "mnuTR";
            this.mnuTR.Size = new System.Drawing.Size(116, 23);
            this.mnuTR.Text = "Return Period (Yr):";
            this.mnuTR.Visible = false;
            // 
            // mnuCalc7Q10
            // 
            this.mnuCalc7Q10.Enabled = false;
            this.mnuCalc7Q10.Name = "mnuCalc7Q10";
            this.mnuCalc7Q10.Size = new System.Drawing.Size(126, 22);
            this.mnuCalc7Q10.Text = "Calculate Frequency";
            this.mnuCalc7Q10.Click += new System.EventHandler(this.mnuCalc7Q10_Click);
            // 
            // mnuRegAnal
            // 
            this.mnuRegAnal.Enabled = false;
            this.mnuRegAnal.Name = "mnuRegAnal";
            this.mnuRegAnal.Size = new System.Drawing.Size(111, 23);
            this.mnuRegAnal.Text = "Regional Analysis";
            this.mnuRegAnal.Visible = false;
            this.mnuRegAnal.Click += new System.EventHandler(this.mnuRegAnal_Click);
            // 
            // mnuEstimate
            // 
            this.mnuEstimate.Enabled = false;
            this.mnuEstimate.Name = "mnuEstimate";
            this.mnuEstimate.Size = new System.Drawing.Size(108, 22);
            this.mnuEstimate.Text = "Estimate at Point";
            this.mnuEstimate.Click += new System.EventHandler(this.mnuEstimate_Click);
            // 
            // txtTR
            // 
            this.txtTR.Name = "txtTR";
            this.txtTR.Size = new System.Drawing.Size(40, 23);
            this.txtTR.Text = "10";
            this.txtTR.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTR.Visible = false;
            this.txtTR.Validating += new System.ComponentModel.CancelEventHandler(this.txtTR_Validating);
            // 
            // mnuAbout
            // 
            this.mnuAbout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDoc,
            this.mnuWhat,
            this.emailFernandezGlennepagovForProblemsToolStripMenuItem});
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(52, 22);
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // mnuDoc
            // 
            this.mnuDoc.Name = "mnuDoc";
            this.mnuDoc.Size = new System.Drawing.Size(318, 22);
            this.mnuDoc.Text = "How-To and Documentation";
            this.mnuDoc.Click += new System.EventHandler(this.mnuDoc_Click);
            // 
            // mnuWhat
            // 
            this.mnuWhat.Name = "mnuWhat";
            this.mnuWhat.Size = new System.Drawing.Size(318, 22);
            this.mnuWhat.Text = "About Frequency Analysis";
            this.mnuWhat.Click += new System.EventHandler(this.mnuWhat_Click);
            // 
            // emailFernandezGlennepagovForProblemsToolStripMenuItem
            // 
            this.emailFernandezGlennepagovForProblemsToolStripMenuItem.Name = "emailFernandezGlennepagovForProblemsToolStripMenuItem";
            this.emailFernandezGlennepagovForProblemsToolStripMenuItem.Size = new System.Drawing.Size(318, 22);
            this.emailFernandezGlennepagovForProblemsToolStripMenuItem.Text = "Email Fernandez.Glenn@epa.gov for problems";
            // 
            // tabCtl
            // 
            this.tabCtl.Controls.Add(this.tabMap);
            this.tabCtl.Controls.Add(this.tabGages);
            this.tabCtl.Controls.Add(this.tabSeries);
            this.tabCtl.Controls.Add(this.tabResults);
            this.tabCtl.Controls.Add(this.tabRegional);
            this.tabCtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtl.Location = new System.Drawing.Point(0, 0);
            this.tabCtl.Name = "tabCtl";
            this.tabCtl.SelectedIndex = 0;
            this.tabCtl.Size = new System.Drawing.Size(1111, 600);
            this.tabCtl.TabIndex = 2;
            this.tabCtl.SelectedIndexChanged += new System.EventHandler(this.tabCtl_SelectedIndexChanged);
            // 
            // tabMap
            // 
            this.tabMap.Controls.Add(this.spatialDockManager1);
            this.tabMap.Location = new System.Drawing.Point(4, 22);
            this.tabMap.Name = "tabMap";
            this.tabMap.Padding = new System.Windows.Forms.Padding(3);
            this.tabMap.Size = new System.Drawing.Size(1103, 574);
            this.tabMap.TabIndex = 4;
            this.tabMap.Text = "Map";
            this.tabMap.UseVisualStyleBackColor = true;
            // 
            // spatialDockManager1
            // 
            this.spatialDockManager1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.spatialDockManager1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spatialDockManager1.Location = new System.Drawing.Point(3, 3);
            this.spatialDockManager1.Name = "spatialDockManager1";
            // 
            // spatialDockManager1.Panel1
            // 
            this.spatialDockManager1.Panel1.Controls.Add(this.splitContainerLegend);
            // 
            // spatialDockManager1.Panel2
            // 
            this.spatialDockManager1.Panel2.Controls.Add(this.uxMap);
            this.spatialDockManager1.Size = new System.Drawing.Size(1097, 568);
            this.spatialDockManager1.SplitterDistance = 173;
            this.spatialDockManager1.TabControl1 = null;
            this.spatialDockManager1.TabControl2 = null;
            this.spatialDockManager1.TabIndex = 0;
            // 
            // splitContainerLegend
            // 
            this.splitContainerLegend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLegend.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerLegend.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLegend.Name = "splitContainerLegend";
            this.splitContainerLegend.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLegend.Panel1
            // 
            this.splitContainerLegend.Panel1.Controls.Add(this.tabCtlLegend);
            // 
            // splitContainerLegend.Panel2
            // 
            this.splitContainerLegend.Panel2.Controls.Add(this.btnPointOK);
            this.splitContainerLegend.Panel2.Controls.Add(this.textBox1);
            this.splitContainerLegend.Size = new System.Drawing.Size(173, 568);
            this.splitContainerLegend.SplitterDistance = 465;
            this.splitContainerLegend.TabIndex = 1;
            // 
            // tabCtlLegend
            // 
            this.tabCtlLegend.Controls.Add(this.tabPage1);
            this.tabCtlLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtlLegend.Location = new System.Drawing.Point(0, 0);
            this.tabCtlLegend.Name = "tabCtlLegend";
            this.tabCtlLegend.SelectedIndex = 0;
            this.tabCtlLegend.Size = new System.Drawing.Size(171, 463);
            this.tabCtlLegend.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.uxLegend);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(163, 437);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Legend";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // uxLegend
            // 
            this.uxLegend.BackColor = System.Drawing.Color.White;
            this.uxLegend.ControlRectangle = new System.Drawing.Rectangle(0, 0, 157, 431);
            this.uxLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uxLegend.DocumentRectangle = new System.Drawing.Rectangle(0, 0, 8, 8);
            this.uxLegend.HorizontalScrollEnabled = true;
            this.uxLegend.Indentation = 30;
            this.uxLegend.IsInitialized = false;
            this.uxLegend.Location = new System.Drawing.Point(3, 3);
            this.uxLegend.MinimumSize = new System.Drawing.Size(5, 5);
            this.uxLegend.Name = "uxLegend";
            this.uxLegend.ProgressHandler = null;
            this.uxLegend.ResetOnResize = false;
            this.uxLegend.SelectionFontColor = System.Drawing.Color.Black;
            this.uxLegend.SelectionHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(238)))), ((int)(((byte)(252)))));
            this.uxLegend.Size = new System.Drawing.Size(157, 431);
            this.uxLegend.TabIndex = 0;
            this.uxLegend.Text = "legend1";
            this.uxLegend.VerticalScrollEnabled = true;
            // 
            // btnPointOK
            // 
            this.btnPointOK.Enabled = false;
            this.btnPointOK.Location = new System.Drawing.Point(103, 62);
            this.btnPointOK.Name = "btnPointOK";
            this.btnPointOK.Size = new System.Drawing.Size(61, 20);
            this.btnPointOK.TabIndex = 1;
            this.btnPointOK.Text = "OK";
            this.btnPointOK.UseVisualStyleBackColor = true;
            this.btnPointOK.Click += new System.EventHandler(this.btnPointOK_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.SkyBlue;
            this.textBox1.Location = new System.Drawing.Point(7, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(156, 44);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Right click on map to select point then click OK to estimate flow ...";
            // 
            // uxMap
            // 
            this.uxMap.AllowDrop = true;
            this.uxMap.BackColor = System.Drawing.Color.White;
            this.uxMap.CollectAfterDraw = false;
            this.uxMap.CollisionDetection = false;
            this.uxMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uxMap.ExtendBuffer = false;
            this.uxMap.FunctionMode = DotSpatial.Controls.FunctionMode.None;
            this.uxMap.IsBusy = false;
            this.uxMap.IsZoomedToMaxExtent = false;
            this.uxMap.Legend = this.uxLegend;
            this.uxMap.Location = new System.Drawing.Point(0, 0);
            this.uxMap.Name = "uxMap";
            this.uxMap.ProgressHandler = null;
            this.uxMap.ProjectionModeDefine = DotSpatial.Controls.ActionMode.Prompt;
            this.uxMap.ProjectionModeReproject = DotSpatial.Controls.ActionMode.Prompt;
            this.uxMap.RedrawLayersWhileResizing = false;
            this.uxMap.SelectionEnabled = true;
            this.uxMap.Size = new System.Drawing.Size(918, 566);
            this.uxMap.TabIndex = 0;
            this.uxMap.ZoomOutFartherThanMaxExtent = true;
            this.uxMap.SelectionChanged += new System.EventHandler(this.uxMap_SelectionChanged);
            this.uxMap.LayerAdded += new System.EventHandler<DotSpatial.Symbology.LayerEventArgs>(this.uxMap_LayerAdded);
            this.uxMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.uxMap_MouseClick);
            this.uxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.uxMap_MouseDown);
            // 
            // tabGages
            // 
            this.tabGages.Controls.Add(this.splitTab0);
            this.tabGages.Location = new System.Drawing.Point(4, 22);
            this.tabGages.Name = "tabGages";
            this.tabGages.Padding = new System.Windows.Forms.Padding(3);
            this.tabGages.Size = new System.Drawing.Size(1103, 574);
            this.tabGages.TabIndex = 0;
            this.tabGages.Tag = "tabGages";
            this.tabGages.Text = "Selected Gages";
            this.tabGages.UseVisualStyleBackColor = true;
            // 
            // splitTab0
            // 
            this.splitTab0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTab0.Location = new System.Drawing.Point(3, 3);
            this.splitTab0.Name = "splitTab0";
            // 
            // splitTab0.Panel1
            // 
            this.splitTab0.Panel1.Controls.Add(this.tableLayoutGages);
            // 
            // splitTab0.Panel2
            // 
            this.splitTab0.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitTab0.Panel2Collapsed = true;
            this.splitTab0.Size = new System.Drawing.Size(1097, 568);
            this.splitTab0.SplitterDistance = 598;
            this.splitTab0.TabIndex = 1;
            // 
            // tableLayoutGages
            // 
            this.tableLayoutGages.ColumnCount = 1;
            this.tableLayoutGages.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutGages.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutGages.Controls.Add(this.dgvGages, 0, 1);
            this.tableLayoutGages.Controls.Add(this.tableLayoutPanel6, 0, 2);
            this.tableLayoutGages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutGages.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutGages.Name = "tableLayoutGages";
            this.tableLayoutGages.RowCount = 3;
            this.tableLayoutGages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutGages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutGages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutGages.Size = new System.Drawing.Size(1097, 568);
            this.tableLayoutGages.TabIndex = 16;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 412F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.grpInfo, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1091, 119);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.yrTo);
            this.panel1.Controls.Add(this.lblTo);
            this.panel1.Controls.Add(this.yrFrom);
            this.panel1.Controls.Add(this.lblFrom);
            this.panel1.Controls.Add(this.lblPeriod);
            this.panel1.Controls.Add(this.txtDly);
            this.panel1.Controls.Add(this.txtYr);
            this.panel1.Controls.Add(this.lblDly);
            this.panel1.Controls.Add(this.btnQueryNWIS);
            this.panel1.Controls.Add(this.lblYr);
            this.panel1.Controls.Add(this.lblSumd);
            this.panel1.Controls.Add(this.cboDay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(406, 111);
            this.panel1.TabIndex = 0;
            // 
            // yrTo
            // 
            this.yrTo.Location = new System.Drawing.Point(154, 66);
            this.yrTo.Maximum = new decimal(new int[] {
            2050,
            0,
            0,
            0});
            this.yrTo.Minimum = new decimal(new int[] {
            1910,
            0,
            0,
            0});
            this.yrTo.Name = "yrTo";
            this.yrTo.Size = new System.Drawing.Size(59, 20);
            this.yrTo.TabIndex = 15;
            this.yrTo.Value = new decimal(new int[] {
            2016,
            0,
            0,
            0});
            this.yrTo.ValueChanged += new System.EventHandler(this.yrTo_ValueChanged);
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(125, 68);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(23, 13);
            this.lblTo.TabIndex = 14;
            this.lblTo.Text = "To:";
            // 
            // yrFrom
            // 
            this.yrFrom.Location = new System.Drawing.Point(153, 40);
            this.yrFrom.Maximum = new decimal(new int[] {
            2015,
            0,
            0,
            0});
            this.yrFrom.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.yrFrom.Name = "yrFrom";
            this.yrFrom.Size = new System.Drawing.Size(60, 20);
            this.yrFrom.TabIndex = 13;
            this.yrFrom.Value = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.yrFrom.ValueChanged += new System.EventHandler(this.yrFrom_ValueChanged);
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(114, 42);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(33, 13);
            this.lblFrom.TabIndex = 12;
            this.lblFrom.Text = "From:";
            // 
            // lblPeriod
            // 
            this.lblPeriod.AutoSize = true;
            this.lblPeriod.Location = new System.Drawing.Point(13, 40);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(87, 13);
            this.lblPeriod.TabIndex = 11;
            this.lblPeriod.Text = "Period of Record";
            // 
            // txtDly
            // 
            this.txtDly.Location = new System.Drawing.Point(362, 33);
            this.txtDly.Name = "txtDly";
            this.txtDly.Size = new System.Drawing.Size(30, 20);
            this.txtDly.TabIndex = 10;
            this.txtDly.Text = "300";
            this.txtDly.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDly.TextChanged += new System.EventHandler(this.txtDly_TextChanged);
            // 
            // txtYr
            // 
            this.txtYr.Location = new System.Drawing.Point(362, 8);
            this.txtYr.Name = "txtYr";
            this.txtYr.Size = new System.Drawing.Size(30, 20);
            this.txtYr.TabIndex = 9;
            this.txtYr.Text = "10";
            this.txtYr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtYr.TextChanged += new System.EventHandler(this.txtYr_TextChanged);
            this.txtYr.Validated += new System.EventHandler(this.txtYr_Validated);
            // 
            // lblDly
            // 
            this.lblDly.AutoSize = true;
            this.lblDly.Location = new System.Drawing.Point(251, 35);
            this.lblDly.Name = "lblDly";
            this.lblDly.Size = new System.Drawing.Size(108, 13);
            this.lblDly.TabIndex = 8;
            this.lblDly.Text = "Minimum Days/Year :";
            // 
            // btnQueryNWIS
            // 
            this.btnQueryNWIS.Location = new System.Drawing.Point(308, 68);
            this.btnQueryNWIS.Name = "btnQueryNWIS";
            this.btnQueryNWIS.Size = new System.Drawing.Size(84, 28);
            this.btnQueryNWIS.TabIndex = 2;
            this.btnQueryNWIS.Text = "Query NWIS";
            this.btnQueryNWIS.UseVisualStyleBackColor = true;
            this.btnQueryNWIS.Click += new System.EventHandler(this.mnuGetSeries_Click);
            // 
            // lblYr
            // 
            this.lblYr.AutoSize = true;
            this.lblYr.Location = new System.Drawing.Point(251, 11);
            this.lblYr.Name = "lblYr";
            this.lblYr.Size = new System.Drawing.Size(84, 13);
            this.lblYr.TabIndex = 7;
            this.lblYr.Text = "Minimum Years :";
            // 
            // lblSumd
            // 
            this.lblSumd.AutoSize = true;
            this.lblSumd.Location = new System.Drawing.Point(13, 11);
            this.lblSumd.Name = "lblSumd";
            this.lblSumd.Size = new System.Drawing.Size(124, 13);
            this.lblSumd.TabIndex = 5;
            this.lblSumd.Text = "Averaging Period (Days):";
            // 
            // cboDay
            // 
            this.cboDay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDay.FormattingEnabled = true;
            this.cboDay.Items.AddRange(new object[] {
            "1",
            "3",
            "5",
            "7",
            "15",
            "30"});
            this.cboDay.Location = new System.Drawing.Point(162, 8);
            this.cboDay.Name = "cboDay";
            this.cboDay.Size = new System.Drawing.Size(51, 21);
            this.cboDay.TabIndex = 6;
            this.cboDay.SelectedIndexChanged += new System.EventHandler(this.cboDay_SelectedIndexChanged);
            // 
            // grpInfo
            // 
            this.grpInfo.Controls.Add(this.txtInfo);
            this.grpInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInfo.Location = new System.Drawing.Point(417, 4);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(670, 111);
            this.grpInfo.TabIndex = 1;
            this.grpInfo.TabStop = false;
            this.grpInfo.Text = "Instruction";
            // 
            // txtInfo
            // 
            this.txtInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Location = new System.Drawing.Point(3, 16);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfo.Size = new System.Drawing.Size(664, 92);
            this.txtInfo.TabIndex = 0;
            // 
            // dgvGages
            // 
            this.dgvGages.AllowUserToAddRows = false;
            this.dgvGages.AllowUserToDeleteRows = false;
            this.dgvGages.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGages.Location = new System.Drawing.Point(3, 128);
            this.dgvGages.Name = "dgvGages";
            this.dgvGages.ReadOnly = true;
            this.dgvGages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGages.ShowEditingIcon = false;
            this.dgvGages.Size = new System.Drawing.Size(1091, 429);
            this.dgvGages.TabIndex = 0;
            this.dgvGages.RowHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvGages_RowHeaderMouseDoubleClick);
            this.dgvGages.SelectionChanged += new System.EventHandler(this.dgvGages_SelectionChanged);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 5;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.24972F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.37514F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.37514F));
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 563);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1091, 2);
            this.tableLayoutPanel6.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.webBrowser, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(96, 100);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.Location = new System.Drawing.Point(3, 3);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(90, 94);
            this.webBrowser.TabIndex = 0;
            // 
            // tabSeries
            // 
            this.tabSeries.Controls.Add(this.splitTimeSeries);
            this.tabSeries.Location = new System.Drawing.Point(4, 22);
            this.tabSeries.Name = "tabSeries";
            this.tabSeries.Padding = new System.Windows.Forms.Padding(3);
            this.tabSeries.Size = new System.Drawing.Size(1103, 574);
            this.tabSeries.TabIndex = 1;
            this.tabSeries.Tag = "tabSeries";
            this.tabSeries.Text = "Time Series";
            this.tabSeries.UseVisualStyleBackColor = true;
            // 
            // splitTimeSeries
            // 
            this.splitTimeSeries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitTimeSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTimeSeries.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitTimeSeries.Location = new System.Drawing.Point(3, 3);
            this.splitTimeSeries.Name = "splitTimeSeries";
            // 
            // splitTimeSeries.Panel1
            // 
            this.splitTimeSeries.Panel1.Controls.Add(this.tableLayoutPanel3);
            // 
            // splitTimeSeries.Panel2
            // 
            this.splitTimeSeries.Panel2.Controls.Add(this.zedGraph);
            this.splitTimeSeries.Size = new System.Drawing.Size(1097, 568);
            this.splitTimeSeries.SplitterDistance = 333;
            this.splitTimeSeries.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.grpGageCbo, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(331, 566);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // grpGageCbo
            // 
            this.grpGageCbo.Controls.Add(this.optDly);
            this.grpGageCbo.Controls.Add(this.optAnn);
            this.grpGageCbo.Controls.Add(this.cboSeries);
            this.grpGageCbo.Controls.Add(this.lblUSGS);
            this.grpGageCbo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpGageCbo.Location = new System.Drawing.Point(3, 3);
            this.grpGageCbo.Name = "grpGageCbo";
            this.grpGageCbo.Size = new System.Drawing.Size(325, 39);
            this.grpGageCbo.TabIndex = 1;
            this.grpGageCbo.TabStop = false;
            // 
            // optDly
            // 
            this.optDly.AutoSize = true;
            this.optDly.Location = new System.Drawing.Point(240, 9);
            this.optDly.Name = "optDly";
            this.optDly.Size = new System.Drawing.Size(48, 17);
            this.optDly.TabIndex = 3;
            this.optDly.Text = "Daily";
            this.optDly.UseVisualStyleBackColor = true;
            this.optDly.Click += new System.EventHandler(this.optDly_Click);
            // 
            // optAnn
            // 
            this.optAnn.AutoSize = true;
            this.optAnn.Checked = true;
            this.optAnn.Location = new System.Drawing.Point(165, 9);
            this.optAnn.Name = "optAnn";
            this.optAnn.Size = new System.Drawing.Size(58, 17);
            this.optAnn.TabIndex = 2;
            this.optAnn.TabStop = true;
            this.optAnn.Text = "Annual";
            this.optAnn.UseVisualStyleBackColor = true;
            this.optAnn.Click += new System.EventHandler(this.optAnn_Click);
            // 
            // cboSeries
            // 
            this.cboSeries.FormattingEnabled = true;
            this.cboSeries.Location = new System.Drawing.Point(48, 8);
            this.cboSeries.Name = "cboSeries";
            this.cboSeries.Size = new System.Drawing.Size(97, 21);
            this.cboSeries.TabIndex = 1;
            this.cboSeries.SelectedIndexChanged += new System.EventHandler(this.cboSeries_SelectedIndexChanged);
            // 
            // lblUSGS
            // 
            this.lblUSGS.AutoSize = true;
            this.lblUSGS.Location = new System.Drawing.Point(6, 11);
            this.lblUSGS.Name = "lblUSGS";
            this.lblUSGS.Size = new System.Drawing.Size(36, 13);
            this.lblUSGS.TabIndex = 0;
            this.lblUSGS.Text = "Gage:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(3, 48);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvSeries);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgvStats);
            this.splitContainer1.Size = new System.Drawing.Size(325, 515);
            this.splitContainer1.SplitterDistance = 358;
            this.splitContainer1.TabIndex = 2;
            // 
            // dgvSeries
            // 
            this.dgvSeries.AllowUserToAddRows = false;
            this.dgvSeries.AllowUserToDeleteRows = false;
            this.dgvSeries.AllowUserToOrderColumns = true;
            this.dgvSeries.AllowUserToResizeRows = false;
            this.dgvSeries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSeries.Location = new System.Drawing.Point(0, 0);
            this.dgvSeries.Name = "dgvSeries";
            this.dgvSeries.ReadOnly = true;
            this.dgvSeries.Size = new System.Drawing.Size(323, 356);
            this.dgvSeries.TabIndex = 0;
            // 
            // dgvStats
            // 
            this.dgvStats.AllowUserToAddRows = false;
            this.dgvStats.AllowUserToDeleteRows = false;
            this.dgvStats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStats.Location = new System.Drawing.Point(0, 0);
            this.dgvStats.Name = "dgvStats";
            this.dgvStats.Size = new System.Drawing.Size(323, 151);
            this.dgvStats.TabIndex = 0;
            // 
            // zedGraph
            // 
            this.zedGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraph.Location = new System.Drawing.Point(0, 0);
            this.zedGraph.Margin = new System.Windows.Forms.Padding(4);
            this.zedGraph.Name = "zedGraph";
            this.zedGraph.ScrollGrace = 0D;
            this.zedGraph.ScrollMaxX = 0D;
            this.zedGraph.ScrollMaxY = 0D;
            this.zedGraph.ScrollMaxY2 = 0D;
            this.zedGraph.ScrollMinX = 0D;
            this.zedGraph.ScrollMinY = 0D;
            this.zedGraph.ScrollMinY2 = 0D;
            this.zedGraph.Size = new System.Drawing.Size(758, 566);
            this.zedGraph.TabIndex = 0;
            // 
            // tabResults
            // 
            this.tabResults.Controls.Add(this.tableLayoutPanel2);
            this.tabResults.Location = new System.Drawing.Point(4, 22);
            this.tabResults.Name = "tabResults";
            this.tabResults.Padding = new System.Windows.Forms.Padding(3);
            this.tabResults.Size = new System.Drawing.Size(1103, 574);
            this.tabResults.TabIndex = 2;
            this.tabResults.Text = "nQy Flows";
            this.tabResults.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1097, 568);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.splitResults);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1091, 554);
            this.panel2.TabIndex = 0;
            // 
            // splitResults
            // 
            this.splitResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitResults.Location = new System.Drawing.Point(0, 0);
            this.splitResults.Name = "splitResults";
            this.splitResults.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitResults.Panel1
            // 
            this.splitResults.Panel1.Controls.Add(this.splitResultsUpper);
            // 
            // splitResults.Panel2
            // 
            this.splitResults.Panel2.Controls.Add(this.pnlRegAnalysis);
            this.splitResults.Panel2Collapsed = true;
            this.splitResults.Size = new System.Drawing.Size(1091, 554);
            this.splitResults.SplitterDistance = 196;
            this.splitResults.TabIndex = 1;
            // 
            // splitResultsUpper
            // 
            this.splitResultsUpper.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitResultsUpper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitResultsUpper.Location = new System.Drawing.Point(0, 0);
            this.splitResultsUpper.Name = "splitResultsUpper";
            // 
            // splitResultsUpper.Panel1
            // 
            this.splitResultsUpper.Panel1.Controls.Add(this.tableLayoutPanel9);
            // 
            // splitResultsUpper.Panel2
            // 
            this.splitResultsUpper.Panel2.Controls.Add(this.tableLayoutCDFplot);
            this.splitResultsUpper.Size = new System.Drawing.Size(1091, 554);
            this.splitResultsUpper.SplitterDistance = 299;
            this.splitResultsUpper.TabIndex = 1;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 3;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this.tableLayoutPanel9.Controls.Add(this.label7, 1, 0);
            this.tableLayoutPanel9.Controls.Add(this.dgvResults, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.btnRegion, 0, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.263923F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.73608F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(297, 552);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.tableLayoutPanel9.SetColumnSpan(this.label7, 2);
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(128, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 26);
            this.label7.TabIndex = 2;
            this.label7.Text = "Click on the row header of grid for probability plot ...";
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel9.SetColumnSpan(this.dgvResults, 3);
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(3, 43);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.Size = new System.Drawing.Size(291, 506);
            this.dgvResults.StandardTab = true;
            this.dgvResults.TabIndex = 0;
            this.dgvResults.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvResults_RowHeaderMouseClick);
            this.dgvResults.SelectionChanged += new System.EventHandler(this.dgvResults_SelectionChanged);
            // 
            // btnRegion
            // 
            this.btnRegion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnRegion.Location = new System.Drawing.Point(3, 8);
            this.btnRegion.Name = "btnRegion";
            this.btnRegion.Size = new System.Drawing.Size(119, 23);
            this.btnRegion.TabIndex = 0;
            this.btnRegion.Text = "Regional Analysis";
            this.btnRegion.UseVisualStyleBackColor = true;
            this.btnRegion.Click += new System.EventHandler(this.btnRegion_Click);
            // 
            // tableLayoutCDFplot
            // 
            this.tableLayoutCDFplot.ColumnCount = 1;
            this.tableLayoutCDFplot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutCDFplot.Controls.Add(this.zedGraphCDF, 0, 0);
            this.tableLayoutCDFplot.Controls.Add(this.dgvProb, 0, 1);
            this.tableLayoutCDFplot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutCDFplot.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutCDFplot.Name = "tableLayoutCDFplot";
            this.tableLayoutCDFplot.RowCount = 2;
            this.tableLayoutCDFplot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutCDFplot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutCDFplot.Size = new System.Drawing.Size(786, 552);
            this.tableLayoutCDFplot.TabIndex = 1;
            // 
            // zedGraphCDF
            // 
            this.zedGraphCDF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphCDF.Location = new System.Drawing.Point(4, 4);
            this.zedGraphCDF.Margin = new System.Windows.Forms.Padding(4);
            this.zedGraphCDF.Name = "zedGraphCDF";
            this.zedGraphCDF.ScrollGrace = 0D;
            this.zedGraphCDF.ScrollMaxX = 0D;
            this.zedGraphCDF.ScrollMaxY = 0D;
            this.zedGraphCDF.ScrollMaxY2 = 0D;
            this.zedGraphCDF.ScrollMinX = 0D;
            this.zedGraphCDF.ScrollMinY = 0D;
            this.zedGraphCDF.ScrollMinY2 = 0D;
            this.zedGraphCDF.Size = new System.Drawing.Size(778, 469);
            this.zedGraphCDF.TabIndex = 0;
            // 
            // dgvProb
            // 
            this.dgvProb.AllowUserToAddRows = false;
            this.dgvProb.AllowUserToDeleteRows = false;
            this.dgvProb.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProb.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProb.Location = new System.Drawing.Point(3, 480);
            this.dgvProb.Name = "dgvProb";
            this.dgvProb.ReadOnly = true;
            this.dgvProb.Size = new System.Drawing.Size(780, 69);
            this.dgvProb.TabIndex = 1;
            // 
            // pnlRegAnalysis
            // 
            this.pnlRegAnalysis.Controls.Add(this.grpBoxRegAnal);
            this.pnlRegAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRegAnalysis.Location = new System.Drawing.Point(0, 0);
            this.pnlRegAnalysis.Name = "pnlRegAnalysis";
            this.pnlRegAnalysis.Size = new System.Drawing.Size(150, 46);
            this.pnlRegAnalysis.TabIndex = 0;
            // 
            // grpBoxRegAnal
            // 
            this.grpBoxRegAnal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxRegAnal.Location = new System.Drawing.Point(0, 0);
            this.grpBoxRegAnal.Name = "grpBoxRegAnal";
            this.grpBoxRegAnal.Size = new System.Drawing.Size(150, 46);
            this.grpBoxRegAnal.TabIndex = 0;
            this.grpBoxRegAnal.TabStop = false;
            this.grpBoxRegAnal.Text = "Regional Analysis";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label6);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 563);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1091, 2);
            this.panel3.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(144, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(248, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Click on the row header of grid for probability plot ...";
            // 
            // tabRegional
            // 
            this.tabRegional.Controls.Add(this.layoutRegional);
            this.tabRegional.Location = new System.Drawing.Point(4, 22);
            this.tabRegional.Name = "tabRegional";
            this.tabRegional.Padding = new System.Windows.Forms.Padding(3);
            this.tabRegional.Size = new System.Drawing.Size(1103, 574);
            this.tabRegional.TabIndex = 3;
            this.tabRegional.Text = "Regional Analysis";
            this.tabRegional.UseVisualStyleBackColor = true;
            // 
            // layoutRegional
            // 
            this.layoutRegional.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.layoutRegional.ColumnCount = 1;
            this.layoutRegional.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRegional.Controls.Add(this.LayoutRegAnalTop, 0, 0);
            this.layoutRegional.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRegional.Location = new System.Drawing.Point(3, 3);
            this.layoutRegional.Name = "layoutRegional";
            this.layoutRegional.RowCount = 1;
            this.layoutRegional.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRegional.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 567F));
            this.layoutRegional.Size = new System.Drawing.Size(1097, 568);
            this.layoutRegional.TabIndex = 0;
            // 
            // LayoutRegAnalTop
            // 
            this.LayoutRegAnalTop.ColumnCount = 2;
            this.LayoutRegAnalTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.81072F));
            this.LayoutRegAnalTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.18928F));
            this.LayoutRegAnalTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.LayoutRegAnalTop.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.LayoutRegAnalTop.Controls.Add(this.LayoutModelResults, 1, 0);
            this.LayoutRegAnalTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutRegAnalTop.Location = new System.Drawing.Point(4, 4);
            this.LayoutRegAnalTop.Name = "LayoutRegAnalTop";
            this.LayoutRegAnalTop.RowCount = 1;
            this.LayoutRegAnalTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutRegAnalTop.Size = new System.Drawing.Size(1089, 560);
            this.LayoutRegAnalTop.TabIndex = 1;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.btnModel, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.grpSelDepVar, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.grpRegGages, 0, 1);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 3;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 109F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(460, 554);
            this.tableLayoutPanel7.TabIndex = 1;
            // 
            // btnModel
            // 
            this.btnModel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnModel.Enabled = false;
            this.btnModel.Location = new System.Drawing.Point(350, 530);
            this.btnModel.Name = "btnModel";
            this.btnModel.Size = new System.Drawing.Size(107, 21);
            this.btnModel.TabIndex = 1;
            this.btnModel.Text = "Fit Model";
            this.btnModel.UseVisualStyleBackColor = true;
            this.btnModel.Click += new System.EventHandler(this.btnModel_Click);
            // 
            // grpSelDepVar
            // 
            this.grpSelDepVar.Controls.Add(this.lstDepVar);
            this.grpSelDepVar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelDepVar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpSelDepVar.Location = new System.Drawing.Point(3, 3);
            this.grpSelDepVar.Name = "grpSelDepVar";
            this.grpSelDepVar.Size = new System.Drawing.Size(454, 103);
            this.grpSelDepVar.TabIndex = 0;
            this.grpSelDepVar.TabStop = false;
            this.grpSelDepVar.Text = "Select Independent Variables";
            // 
            // lstDepVar
            // 
            this.lstDepVar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDepVar.FormattingEnabled = true;
            this.lstDepVar.Items.AddRange(new object[] {
            "Drainage Area",
            "Mean Annual Rainfall",
            "Mean Annual Flow",
            "Percent Forest",
            "Percent Urban",
            "Percent Others"});
            this.lstDepVar.Location = new System.Drawing.Point(3, 16);
            this.lstDepVar.Name = "lstDepVar";
            this.lstDepVar.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstDepVar.Size = new System.Drawing.Size(448, 84);
            this.lstDepVar.TabIndex = 0;
            // 
            // grpRegGages
            // 
            this.grpRegGages.Controls.Add(this.dgvSelRegGages);
            this.grpRegGages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRegGages.Location = new System.Drawing.Point(3, 112);
            this.grpRegGages.Name = "grpRegGages";
            this.grpRegGages.Size = new System.Drawing.Size(454, 412);
            this.grpRegGages.TabIndex = 0;
            this.grpRegGages.TabStop = false;
            this.grpRegGages.Text = "Select Gages";
            // 
            // dgvSelRegGages
            // 
            this.dgvSelRegGages.AllowUserToAddRows = false;
            this.dgvSelRegGages.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSelRegGages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSelRegGages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSelRegGages.Location = new System.Drawing.Point(3, 16);
            this.dgvSelRegGages.Name = "dgvSelRegGages";
            this.dgvSelRegGages.Size = new System.Drawing.Size(448, 393);
            this.dgvSelRegGages.TabIndex = 0;
            this.dgvSelRegGages.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSelRegGages_RowHeaderMouseClick);
            this.dgvSelRegGages.SelectionChanged += new System.EventHandler(this.dgvSelRegGages_SelectionChanged);
            // 
            // LayoutModelResults
            // 
            this.LayoutModelResults.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.LayoutModelResults.ColumnCount = 1;
            this.LayoutModelResults.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutModelResults.Controls.Add(this.splitModelResults, 0, 0);
            this.LayoutModelResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutModelResults.Location = new System.Drawing.Point(469, 3);
            this.LayoutModelResults.Name = "LayoutModelResults";
            this.LayoutModelResults.RowCount = 1;
            this.LayoutModelResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 786F));
            this.LayoutModelResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 786F));
            this.LayoutModelResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 786F));
            this.LayoutModelResults.Size = new System.Drawing.Size(617, 554);
            this.LayoutModelResults.TabIndex = 5;
            // 
            // splitModelResults
            // 
            this.splitModelResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitModelResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitModelResults.Location = new System.Drawing.Point(4, 4);
            this.splitModelResults.Name = "splitModelResults";
            this.splitModelResults.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitModelResults.Panel1
            // 
            this.splitModelResults.Panel1.Controls.Add(this.tableLayoutPanel4);
            // 
            // splitModelResults.Panel2
            // 
            this.splitModelResults.Panel2.Controls.Add(this.tableLayoutPanel8);
            this.splitModelResults.Size = new System.Drawing.Size(609, 780);
            this.splitModelResults.SplitterDistance = 349;
            this.splitModelResults.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(607, 347);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtRegInfo);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(601, 114);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            // 
            // txtRegInfo
            // 
            this.txtRegInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtRegInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRegInfo.Location = new System.Drawing.Point(3, 16);
            this.txtRegInfo.Multiline = true;
            this.txtRegInfo.Name = "txtRegInfo";
            this.txtRegInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRegInfo.Size = new System.Drawing.Size(595, 95);
            this.txtRegInfo.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dgvModel);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 123);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(601, 221);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Model Results";
            // 
            // dgvModel
            // 
            this.dgvModel.AllowUserToAddRows = false;
            this.dgvModel.AllowUserToDeleteRows = false;
            this.dgvModel.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvModel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModel.Location = new System.Drawing.Point(3, 16);
            this.dgvModel.Name = "dgvModel";
            this.dgvModel.Size = new System.Drawing.Size(595, 202);
            this.dgvModel.TabIndex = 3;
            this.dgvModel.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvModel_RowHeaderMouseClick);
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 285F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.grpEstimate, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.dgvEstimate, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(607, 425);
            this.tableLayoutPanel8.TabIndex = 1;
            // 
            // grpEstimate
            // 
            this.grpEstimate.Controls.Add(this.txtWet);
            this.grpEstimate.Controls.Add(this.txtUrb);
            this.grpEstimate.Controls.Add(this.txtFor);
            this.grpEstimate.Controls.Add(this.label5);
            this.grpEstimate.Controls.Add(this.label4);
            this.grpEstimate.Controls.Add(this.label3);
            this.grpEstimate.Controls.Add(this.txtFlow);
            this.grpEstimate.Controls.Add(this.label1);
            this.grpEstimate.Controls.Add(this.txtRain);
            this.grpEstimate.Controls.Add(this.txtArea);
            this.grpEstimate.Controls.Add(this.label2);
            this.grpEstimate.Controls.Add(this.Area);
            this.grpEstimate.Controls.Add(this.btnEstMap);
            this.grpEstimate.Controls.Add(this.btnCalc);
            this.grpEstimate.Controls.Add(this.txtLon);
            this.grpEstimate.Controls.Add(this.txtLat);
            this.grpEstimate.Controls.Add(this.lblLon);
            this.grpEstimate.Controls.Add(this.lblLat);
            this.grpEstimate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpEstimate.Location = new System.Drawing.Point(3, 3);
            this.grpEstimate.Name = "grpEstimate";
            this.grpEstimate.Size = new System.Drawing.Size(279, 419);
            this.grpEstimate.TabIndex = 0;
            this.grpEstimate.TabStop = false;
            this.grpEstimate.Text = "Estimate Flow";
            // 
            // txtWet
            // 
            this.txtWet.Location = new System.Drawing.Point(220, 120);
            this.txtWet.Name = "txtWet";
            this.txtWet.ReadOnly = true;
            this.txtWet.Size = new System.Drawing.Size(50, 20);
            this.txtWet.TabIndex = 17;
            // 
            // txtUrb
            // 
            this.txtUrb.Location = new System.Drawing.Point(220, 94);
            this.txtUrb.Name = "txtUrb";
            this.txtUrb.ReadOnly = true;
            this.txtUrb.Size = new System.Drawing.Size(50, 20);
            this.txtUrb.TabIndex = 16;
            // 
            // txtFor
            // 
            this.txtFor.Location = new System.Drawing.Point(220, 68);
            this.txtFor.Name = "txtFor";
            this.txtFor.ReadOnly = true;
            this.txtFor.Size = new System.Drawing.Size(50, 20);
            this.txtFor.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(152, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "% Wetlands";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(154, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "% Urban";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(154, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "% Forest";
            // 
            // txtFlow
            // 
            this.txtFlow.Location = new System.Drawing.Point(96, 120);
            this.txtFlow.Name = "txtFlow";
            this.txtFlow.ReadOnly = true;
            this.txtFlow.Size = new System.Drawing.Size(52, 20);
            this.txtFlow.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Annual Flow";
            // 
            // txtRain
            // 
            this.txtRain.Location = new System.Drawing.Point(96, 92);
            this.txtRain.Name = "txtRain";
            this.txtRain.ReadOnly = true;
            this.txtRain.Size = new System.Drawing.Size(52, 20);
            this.txtRain.TabIndex = 9;
            // 
            // txtArea
            // 
            this.txtArea.Location = new System.Drawing.Point(96, 68);
            this.txtArea.Name = "txtArea";
            this.txtArea.ReadOnly = true;
            this.txtArea.Size = new System.Drawing.Size(52, 20);
            this.txtArea.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Annual Rain";
            // 
            // Area
            // 
            this.Area.AutoSize = true;
            this.Area.Location = new System.Drawing.Point(6, 68);
            this.Area.Name = "Area";
            this.Area.Size = new System.Drawing.Size(75, 13);
            this.Area.TabIndex = 6;
            this.Area.Text = "Drainage Area";
            // 
            // btnEstMap
            // 
            this.btnEstMap.Enabled = false;
            this.btnEstMap.Location = new System.Drawing.Point(9, 161);
            this.btnEstMap.Name = "btnEstMap";
            this.btnEstMap.Size = new System.Drawing.Size(116, 21);
            this.btnEstMap.TabIndex = 5;
            this.btnEstMap.Text = "Select From Map";
            this.btnEstMap.UseVisualStyleBackColor = true;
            this.btnEstMap.Visible = false;
            this.btnEstMap.Click += new System.EventHandler(this.btnEstMap_Click);
            // 
            // btnCalc
            // 
            this.btnCalc.Enabled = false;
            this.btnCalc.Location = new System.Drawing.Point(179, 159);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(91, 23);
            this.btnCalc.TabIndex = 4;
            this.btnCalc.Text = "Estimate";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // txtLon
            // 
            this.txtLon.Location = new System.Drawing.Point(207, 28);
            this.txtLon.Name = "txtLon";
            this.txtLon.ReadOnly = true;
            this.txtLon.Size = new System.Drawing.Size(66, 20);
            this.txtLon.TabIndex = 3;
            // 
            // txtLat
            // 
            this.txtLat.Location = new System.Drawing.Point(59, 29);
            this.txtLat.Name = "txtLat";
            this.txtLat.ReadOnly = true;
            this.txtLat.Size = new System.Drawing.Size(66, 20);
            this.txtLat.TabIndex = 2;
            // 
            // lblLon
            // 
            this.lblLon.AutoSize = true;
            this.lblLon.Location = new System.Drawing.Point(147, 32);
            this.lblLon.Name = "lblLon";
            this.lblLon.Size = new System.Drawing.Size(54, 13);
            this.lblLon.TabIndex = 1;
            this.lblLon.Text = "Longitude";
            // 
            // lblLat
            // 
            this.lblLat.AutoSize = true;
            this.lblLat.Location = new System.Drawing.Point(6, 32);
            this.lblLat.Name = "lblLat";
            this.lblLat.Size = new System.Drawing.Size(45, 13);
            this.lblLat.TabIndex = 0;
            this.lblLat.Text = "Latitude";
            // 
            // dgvEstimate
            // 
            this.dgvEstimate.AllowUserToAddRows = false;
            this.dgvEstimate.AllowUserToDeleteRows = false;
            this.dgvEstimate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEstimate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEstimate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEstimate.Location = new System.Drawing.Point(288, 3);
            this.dgvEstimate.Name = "dgvEstimate";
            this.dgvEstimate.ReadOnly = true;
            this.dgvEstimate.Size = new System.Drawing.Size(316, 419);
            this.dgvEstimate.TabIndex = 1;
            // 
            // appManager
            // 
            this.appManager.Directories = ((System.Collections.Generic.List<string>)(resources.GetObject("appManager.Directories")));
            this.appManager.DockManager = this.spatialDockManager1;
            this.appManager.HeaderControl = this.spatialHeaderControl1;
            this.appManager.Legend = this.uxLegend;
            this.appManager.Map = this.uxMap;
            this.appManager.ProgressHandler = null;
            this.appManager.ShowExtensionsDialogMode = DotSpatial.Controls.ShowExtensionsDialogMode.Default;
            // 
            // spatialHeaderControl1
            // 
            this.spatialHeaderControl1.ApplicationManager = this.appManager;
            this.spatialHeaderControl1.MenuStrip = this.mnuMain;
            this.spatialHeaderControl1.ToolbarsContainer = this.spatialToolStripPanel1;
            // 
            // spatialToolStripPanel1
            // 
            this.spatialToolStripPanel1.Controls.Add(this.mnuMain);
            this.spatialToolStripPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.spatialToolStripPanel1.Location = new System.Drawing.Point(0, 0);
            this.spatialToolStripPanel1.Name = "spatialToolStripPanel1";
            this.spatialToolStripPanel1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.spatialToolStripPanel1.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.spatialToolStripPanel1.Size = new System.Drawing.Size(1113, 51);
            // 
            // pnlTabCtl
            // 
            this.pnlTabCtl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTabCtl.Controls.Add(this.tabCtl);
            this.pnlTabCtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabCtl.Location = new System.Drawing.Point(0, 51);
            this.pnlTabCtl.Name = "pnlTabCtl";
            this.pnlTabCtl.Size = new System.Drawing.Size(1113, 602);
            this.pnlTabCtl.TabIndex = 4;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1113, 653);
            this.Controls.Add(this.pnlTabCtl);
            this.Controls.Add(this.spatialToolStripPanel1);
            this.MainMenuStrip = this.mnuMain;
            this.Name = "frmMain";
            this.Text = "Low Flow Frequency Analysis";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.tabCtl.ResumeLayout(false);
            this.tabMap.ResumeLayout(false);
            this.spatialDockManager1.Panel1.ResumeLayout(false);
            this.spatialDockManager1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spatialDockManager1)).EndInit();
            this.spatialDockManager1.ResumeLayout(false);
            this.splitContainerLegend.Panel1.ResumeLayout(false);
            this.splitContainerLegend.Panel2.ResumeLayout(false);
            this.splitContainerLegend.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLegend)).EndInit();
            this.splitContainerLegend.ResumeLayout(false);
            this.tabCtlLegend.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabGages.ResumeLayout(false);
            this.splitTab0.Panel1.ResumeLayout(false);
            this.splitTab0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTab0)).EndInit();
            this.splitTab0.ResumeLayout(false);
            this.tableLayoutGages.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yrTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yrFrom)).EndInit();
            this.grpInfo.ResumeLayout(false);
            this.grpInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGages)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabSeries.ResumeLayout(false);
            this.splitTimeSeries.Panel1.ResumeLayout(false);
            this.splitTimeSeries.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTimeSeries)).EndInit();
            this.splitTimeSeries.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.grpGageCbo.ResumeLayout(false);
            this.grpGageCbo.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).EndInit();
            this.tabResults.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.splitResults.Panel1.ResumeLayout(false);
            this.splitResults.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitResults)).EndInit();
            this.splitResults.ResumeLayout(false);
            this.splitResultsUpper.Panel1.ResumeLayout(false);
            this.splitResultsUpper.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitResultsUpper)).EndInit();
            this.splitResultsUpper.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.tableLayoutCDFplot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProb)).EndInit();
            this.pnlRegAnalysis.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabRegional.ResumeLayout(false);
            this.layoutRegional.ResumeLayout(false);
            this.LayoutRegAnalTop.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.grpSelDepVar.ResumeLayout(false);
            this.grpRegGages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelRegGages)).EndInit();
            this.LayoutModelResults.ResumeLayout(false);
            this.splitModelResults.Panel1.ResumeLayout(false);
            this.splitModelResults.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitModelResults)).EndInit();
            this.splitModelResults.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvModel)).EndInit();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.grpEstimate.ResumeLayout(false);
            this.grpEstimate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEstimate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spatialHeaderControl1)).EndInit();
            this.spatialToolStripPanel1.ResumeLayout(false);
            this.spatialToolStripPanel1.PerformLayout();
            this.pnlTabCtl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.TabControl tabCtl;
        private System.Windows.Forms.TabPage tabGages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabPage tabSeries;
        private System.Windows.Forms.SplitContainer splitTab0;
        private System.Windows.Forms.DataGridView dgvGages;
        private System.Windows.Forms.SplitContainer splitTimeSeries;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.DataGridView dgvSeries;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.GroupBox grpGageCbo;
        private System.Windows.Forms.Label lblUSGS;
        private ZedGraph.ZedGraphControl zedGraph;
        private System.Windows.Forms.ToolStripMenuItem mnuCalc7Q10;
        private System.Windows.Forms.ToolStripMenuItem mnuGetSeries;
        private System.Windows.Forms.ComboBox cboSeries;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.RadioButton optDly;
        private System.Windows.Forms.RadioButton optAnn;
        private System.Windows.Forms.SplitContainer splitResults;
        private System.Windows.Forms.Panel pnlRegAnalysis;
        private System.Windows.Forms.GroupBox grpBoxRegAnal;
        private System.Windows.Forms.SplitContainer splitResultsUpper;
        private ZedGraph.ZedGraphControl zedGraphCDF;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutGages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtDly;
        private System.Windows.Forms.TextBox txtYr;
        private System.Windows.Forms.Label lblDly;
        private System.Windows.Forms.Button btnQueryNWIS;
        private System.Windows.Forms.Label lblYr;
        private System.Windows.Forms.Label lblSumd;
        private System.Windows.Forms.ComboBox cboDay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button btnRegion;
        private System.Windows.Forms.TabPage tabRegional;
        private System.Windows.Forms.TableLayoutPanel layoutRegional;
        private System.Windows.Forms.GroupBox grpRegGages;
        private System.Windows.Forms.DataGridView dgvSelRegGages;
        private System.Windows.Forms.GroupBox grpSelDepVar;
        private System.Windows.Forms.ListBox lstDepVar;
        private System.Windows.Forms.Button btnModel;
        private System.Windows.Forms.TableLayoutPanel LayoutRegAnalTop;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TextBox txtRegInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuTR;
        private System.Windows.Forms.ToolStripTextBox txtTR;
        private System.Windows.Forms.DataGridView dgvModel;
        private System.Windows.Forms.GroupBox grpInfo;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.TableLayoutPanel LayoutModelResults;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown yrTo;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.NumericUpDown yrFrom;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.ToolStripMenuItem mnuDoc;
        private System.Windows.Forms.ToolStripMenuItem mnuWhat;
        private System.Windows.Forms.ToolStripMenuItem emailFernandezGlennepagovForProblemsToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutCDFplot;
        private System.Windows.Forms.DataGridView dgvProb;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvStats;
        private System.Windows.Forms.ToolStripMenuItem mnuRegAnal;
        private System.Windows.Forms.SplitContainer splitModelResults;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox grpEstimate;
        private System.Windows.Forms.Label lblLon;
        private System.Windows.Forms.Label lblLat;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.DataGridView dgvEstimate;
        private System.Windows.Forms.Button btnEstMap;
        public System.Windows.Forms.TextBox txtLon;
        public System.Windows.Forms.TextBox txtLat;
        private System.Windows.Forms.TextBox txtWet;
        private System.Windows.Forms.TextBox txtUrb;
        private System.Windows.Forms.TextBox txtFor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFlow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRain;
        private System.Windows.Forms.TextBox txtArea;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Area;
        private System.Windows.Forms.TabPage tabMap;
        private DotSpatial.Controls.SpatialDockManager spatialDockManager1;
        private DotSpatial.Controls.Map uxMap;
        private DotSpatial.Controls.AppManager appManager;
        private DotSpatial.Controls.SpatialHeaderControl spatialHeaderControl1;
        private DotSpatial.Controls.SpatialToolStripPanel spatialToolStripPanel1;
        private System.Windows.Forms.Panel pnlTabCtl;
        private System.Windows.Forms.ToolStripMenuItem mnuEstimate;
        private System.Windows.Forms.SplitContainer splitContainerLegend;
        private System.Windows.Forms.TabControl tabCtlLegend;
        private System.Windows.Forms.TabPage tabPage1;
        private DotSpatial.Controls.Legend uxLegend;
        private System.Windows.Forms.Button btnPointOK;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem mnuReSelectGages;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.Label label7;
    }
}

