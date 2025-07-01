namespace NCEIData
{
    partial class frmDataEDDE
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataEDDE));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tabData = new System.Windows.Forms.TabControl();
            this.tabPageData = new System.Windows.Forms.TabPage();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.lblGages = new System.Windows.Forms.Label();
            this.tabPageDaily = new System.Windows.Forms.TabPage();
            this.dgvDaily = new System.Windows.Forms.DataGridView();
            this.tabPageMiss = new System.Windows.Forms.TabPage();
            this.dgvMiss = new System.Windows.Forms.DataGridView();
            this.lblSite = new System.Windows.Forms.Label();
            this.grpMiss = new System.Windows.Forms.GroupBox();
            this.dgvMissCnt = new System.Windows.Forms.DataGridView();
            this.grpMissing = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnEstimate = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblnoSta = new System.Windows.Forms.Label();
            this.numMaxStations = new System.Windows.Forms.NumericUpDown();
            this.lbRadius = new System.Windows.Forms.Label();
            this.numRadius = new System.Windows.Forms.NumericUpDown();
            this.optInter = new System.Windows.Forms.RadioButton();
            this.optOther = new System.Windows.Forms.RadioButton();
            this.lblInterpolate = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.optAll = new System.Windows.Forms.RadioButton();
            this.optSite = new System.Windows.Forms.RadioButton();
            this.splitData = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.dataTree = new System.Windows.Forms.TreeView();
            this.chkShowData = new System.Windows.Forms.CheckBox();
            this.lblGrid = new System.Windows.Forms.Label();
            this.splitDataView = new System.Windows.Forms.SplitContainer();
            this.splitDataGrid = new System.Windows.Forms.SplitContainer();
            this.splitDataTable = new System.Windows.Forms.SplitContainer();
            this.zgvSeries = new ZedGraph.ZedGraphControl();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip = new System.Windows.Forms.ToolStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripLabel();
            this.btnClose = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabData.SuspendLayout();
            this.tabPageData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tabPageDaily.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDaily)).BeginInit();
            this.tabPageMiss.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMiss)).BeginInit();
            this.grpMiss.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMissCnt)).BeginInit();
            this.grpMissing.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxStations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitData)).BeginInit();
            this.splitData.Panel1.SuspendLayout();
            this.splitData.Panel2.SuspendLayout();
            this.splitData.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDataView)).BeginInit();
            this.splitDataView.Panel1.SuspendLayout();
            this.splitDataView.Panel2.SuspendLayout();
            this.splitDataView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDataGrid)).BeginInit();
            this.splitDataGrid.Panel1.SuspendLayout();
            this.splitDataGrid.Panel2.SuspendLayout();
            this.splitDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDataTable)).BeginInit();
            this.splitDataTable.Panel1.SuspendLayout();
            this.splitDataTable.Panel2.SuspendLayout();
            this.splitDataTable.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 375F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabData, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSite, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 209F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(341, 537);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Station";
            // 
            // tabData
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabData, 2);
            this.tabData.Controls.Add(this.tabPageData);
            this.tabData.Controls.Add(this.tabPageDaily);
            this.tabData.Controls.Add(this.tabPageMiss);
            this.tabData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabData.Location = new System.Drawing.Point(4, 40);
            this.tabData.Margin = new System.Windows.Forms.Padding(4);
            this.tabData.Name = "tabData";
            this.tableLayoutPanel1.SetRowSpan(this.tabData, 2);
            this.tabData.SelectedIndex = 0;
            this.tabData.Size = new System.Drawing.Size(430, 493);
            this.tabData.TabIndex = 4;
            this.tabData.SelectedIndexChanged += new System.EventHandler(this.tabData_SelectedIndexChanged);
            // 
            // tabPageData
            // 
            this.tabPageData.Controls.Add(this.dgvData);
            this.tabPageData.Controls.Add(this.lblGages);
            this.tabPageData.Location = new System.Drawing.Point(4, 25);
            this.tabPageData.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageData.Name = "tabPageData";
            this.tabPageData.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageData.Size = new System.Drawing.Size(422, 464);
            this.tabPageData.TabIndex = 0;
            this.tabPageData.Text = "Hourly Records";
            this.tabPageData.UseVisualStyleBackColor = true;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(4, 4);
            this.dgvData.Margin = new System.Windows.Forms.Padding(4);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersWidth = 10;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(414, 456);
            this.dgvData.TabIndex = 0;
            // 
            // lblGages
            // 
            this.lblGages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGages.AutoSize = true;
            this.lblGages.Location = new System.Drawing.Point(320, 79);
            this.lblGages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGages.Name = "lblGages";
            this.lblGages.Size = new System.Drawing.Size(0, 17);
            this.lblGages.TabIndex = 1;
            this.lblGages.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabPageDaily
            // 
            this.tabPageDaily.Controls.Add(this.dgvDaily);
            this.tabPageDaily.Location = new System.Drawing.Point(4, 25);
            this.tabPageDaily.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageDaily.Name = "tabPageDaily";
            this.tabPageDaily.Size = new System.Drawing.Size(422, 464);
            this.tabPageDaily.TabIndex = 2;
            this.tabPageDaily.Text = "Daily Records";
            this.tabPageDaily.UseVisualStyleBackColor = true;
            // 
            // dgvDaily
            // 
            this.dgvDaily.AllowUserToAddRows = false;
            this.dgvDaily.AllowUserToDeleteRows = false;
            this.dgvDaily.AllowUserToResizeRows = false;
            this.dgvDaily.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDaily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDaily.Location = new System.Drawing.Point(0, 0);
            this.dgvDaily.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDaily.Name = "dgvDaily";
            this.dgvDaily.RowHeadersWidth = 10;
            this.dgvDaily.Size = new System.Drawing.Size(422, 464);
            this.dgvDaily.TabIndex = 0;
            // 
            // tabPageMiss
            // 
            this.tabPageMiss.Controls.Add(this.dgvMiss);
            this.tabPageMiss.Location = new System.Drawing.Point(4, 25);
            this.tabPageMiss.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageMiss.Name = "tabPageMiss";
            this.tabPageMiss.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageMiss.Size = new System.Drawing.Size(422, 407);
            this.tabPageMiss.TabIndex = 1;
            this.tabPageMiss.Text = "Missing Records";
            this.tabPageMiss.UseVisualStyleBackColor = true;
            // 
            // dgvMiss
            // 
            this.dgvMiss.AllowUserToAddRows = false;
            this.dgvMiss.AllowUserToDeleteRows = false;
            this.dgvMiss.AllowUserToResizeRows = false;
            this.dgvMiss.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMiss.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMiss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMiss.Location = new System.Drawing.Point(4, 4);
            this.dgvMiss.Margin = new System.Windows.Forms.Padding(4);
            this.dgvMiss.Name = "dgvMiss";
            this.dgvMiss.RowHeadersWidth = 10;
            this.dgvMiss.Size = new System.Drawing.Size(414, 399);
            this.dgvMiss.TabIndex = 0;
            // 
            // lblSite
            // 
            this.lblSite.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSite.AutoSize = true;
            this.lblSite.Location = new System.Drawing.Point(67, 9);
            this.lblSite.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSite.Name = "lblSite";
            this.lblSite.Size = new System.Drawing.Size(105, 17);
            this.lblSite.TabIndex = 10;
            this.lblSite.Text = "<StationName>";
            // 
            // grpMiss
            // 
            this.grpMiss.Controls.Add(this.dgvMissCnt);
            this.grpMiss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMiss.Location = new System.Drawing.Point(0, 0);
            this.grpMiss.Margin = new System.Windows.Forms.Padding(4);
            this.grpMiss.Name = "grpMiss";
            this.grpMiss.Padding = new System.Windows.Forms.Padding(4);
            this.grpMiss.Size = new System.Drawing.Size(341, 115);
            this.grpMiss.TabIndex = 9;
            this.grpMiss.TabStop = false;
            // 
            // dgvMissCnt
            // 
            this.dgvMissCnt.AllowUserToAddRows = false;
            this.dgvMissCnt.AllowUserToDeleteRows = false;
            this.dgvMissCnt.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMissCnt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMissCnt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMissCnt.Location = new System.Drawing.Point(4, 19);
            this.dgvMissCnt.Margin = new System.Windows.Forms.Padding(4);
            this.dgvMissCnt.MultiSelect = false;
            this.dgvMissCnt.Name = "dgvMissCnt";
            this.dgvMissCnt.ReadOnly = true;
            this.dgvMissCnt.RowHeadersWidth = 10;
            this.dgvMissCnt.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMissCnt.Size = new System.Drawing.Size(333, 92);
            this.dgvMissCnt.TabIndex = 0;
            // 
            // grpMissing
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.grpMissing, 2);
            this.grpMissing.Controls.Add(this.tableLayoutPanel3);
            this.grpMissing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMissing.Location = new System.Drawing.Point(4, 4);
            this.grpMissing.Margin = new System.Windows.Forms.Padding(4);
            this.grpMissing.Name = "grpMissing";
            this.grpMissing.Padding = new System.Windows.Forms.Padding(4);
            this.grpMissing.Size = new System.Drawing.Size(1460, 171);
            this.grpMissing.TabIndex = 7;
            this.grpMissing.TabStop = false;
            this.grpMissing.Text = "Missing Records";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 9;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 167F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 229F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.Controls.Add(this.btnEstimate, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.lblInterpolate, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 20);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1196, 167);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // btnEstimate
            // 
            this.btnEstimate.Location = new System.Drawing.Point(4, 4);
            this.btnEstimate.Margin = new System.Windows.Forms.Padding(4);
            this.btnEstimate.Name = "btnEstimate";
            this.btnEstimate.Size = new System.Drawing.Size(152, 28);
            this.btnEstimate.TabIndex = 4;
            this.btnEstimate.Text = "Estimate Missing";
            this.btnEstimate.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.groupBox3, 4);
            this.groupBox3.Controls.Add(this.lblnoSta);
            this.groupBox3.Controls.Add(this.numMaxStations);
            this.groupBox3.Controls.Add(this.lbRadius);
            this.groupBox3.Controls.Add(this.numRadius);
            this.groupBox3.Controls.Add(this.optInter);
            this.groupBox3.Controls.Add(this.optOther);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(4, 41);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(505, 122);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Estimation Methods:";
            // 
            // lblnoSta
            // 
            this.lblnoSta.AutoSize = true;
            this.lblnoSta.Location = new System.Drawing.Point(281, 53);
            this.lblnoSta.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblnoSta.Name = "lblnoSta";
            this.lblnoSta.Size = new System.Drawing.Size(97, 17);
            this.lblnoSta.TabIndex = 21;
            this.lblnoSta.Text = "No of Stations";
            // 
            // numMaxStations
            // 
            this.numMaxStations.Location = new System.Drawing.Point(223, 50);
            this.numMaxStations.Margin = new System.Windows.Forms.Padding(4);
            this.numMaxStations.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMaxStations.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxStations.Name = "numMaxStations";
            this.numMaxStations.Size = new System.Drawing.Size(51, 22);
            this.numMaxStations.TabIndex = 20;
            this.numMaxStations.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMaxStations.ValueChanged += new System.EventHandler(this.numMaxStations_ValueChanged);
            // 
            // lbRadius
            // 
            this.lbRadius.AutoSize = true;
            this.lbRadius.Location = new System.Drawing.Point(281, 85);
            this.lbRadius.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbRadius.Name = "lbRadius";
            this.lbRadius.Size = new System.Drawing.Size(129, 17);
            this.lbRadius.TabIndex = 19;
            this.lbRadius.Text = "Search Radius (mi)";
            this.lbRadius.Visible = false;
            // 
            // numRadius
            // 
            this.numRadius.Location = new System.Drawing.Point(219, 82);
            this.numRadius.Margin = new System.Windows.Forms.Padding(4);
            this.numRadius.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numRadius.Name = "numRadius";
            this.numRadius.Size = new System.Drawing.Size(51, 22);
            this.numRadius.TabIndex = 18;
            this.numRadius.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numRadius.Visible = false;
            this.numRadius.ValueChanged += new System.EventHandler(this.numRadius_ValueChanged);
            // 
            // optInter
            // 
            this.optInter.AutoSize = true;
            this.optInter.Checked = true;
            this.optInter.Location = new System.Drawing.Point(8, 23);
            this.optInter.Margin = new System.Windows.Forms.Padding(4);
            this.optInter.Name = "optInter";
            this.optInter.Size = new System.Drawing.Size(136, 21);
            this.optInter.TabIndex = 16;
            this.optInter.TabStop = true;
            this.optInter.Text = "Stochastic Model";
            this.optInter.UseVisualStyleBackColor = true;
            this.optInter.CheckedChanged += new System.EventHandler(this.optInter_CheckedChanged);
            // 
            // optOther
            // 
            this.optOther.AutoSize = true;
            this.optOther.Location = new System.Drawing.Point(223, 23);
            this.optOther.Margin = new System.Windows.Forms.Padding(4);
            this.optOther.Name = "optOther";
            this.optOther.Size = new System.Drawing.Size(154, 21);
            this.optOther.TabIndex = 17;
            this.optOther.Text = "Spatial Interpolation";
            this.optOther.UseVisualStyleBackColor = true;
            this.optOther.CheckedChanged += new System.EventHandler(this.optOther_CheckedChanged);
            // 
            // lblInterpolate
            // 
            this.lblInterpolate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInterpolate.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.lblInterpolate, 4);
            this.lblInterpolate.Location = new System.Drawing.Point(517, 41);
            this.lblInterpolate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInterpolate.Name = "lblInterpolate";
            this.tableLayoutPanel3.SetRowSpan(this.lblInterpolate, 2);
            this.lblInterpolate.Size = new System.Drawing.Size(660, 85);
            this.lblInterpolate.TabIndex = 20;
            this.lblInterpolate.Text = resources.GetString("lblInterpolate.Text");
            this.lblInterpolate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel3.SetColumnSpan(this.tableLayoutPanel4, 2);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.optAll, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.optSite, 1, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(164, 4);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(264, 28);
            this.tableLayoutPanel4.TabIndex = 13;
            // 
            // optAll
            // 
            this.optAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.optAll.AutoSize = true;
            this.optAll.Checked = true;
            this.optAll.Location = new System.Drawing.Point(4, 4);
            this.optAll.Margin = new System.Windows.Forms.Padding(4);
            this.optAll.Name = "optAll";
            this.optAll.Size = new System.Drawing.Size(79, 20);
            this.optAll.TabIndex = 11;
            this.optAll.TabStop = true;
            this.optAll.Text = "All Sites";
            this.optAll.UseVisualStyleBackColor = true;
            this.optAll.CheckedChanged += new System.EventHandler(this.optAll_CheckedChanged);
            // 
            // optSite
            // 
            this.optSite.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.optSite.AutoSize = true;
            this.optSite.Location = new System.Drawing.Point(136, 4);
            this.optSite.Margin = new System.Windows.Forms.Padding(4);
            this.optSite.Name = "optSite";
            this.optSite.Size = new System.Drawing.Size(73, 20);
            this.optSite.TabIndex = 12;
            this.optSite.Text = "By Site";
            this.optSite.UseVisualStyleBackColor = true;
            this.optSite.CheckedChanged += new System.EventHandler(this.optSite_CheckedChanged);
            // 
            // splitData
            // 
            this.splitData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel6.SetColumnSpan(this.splitData, 2);
            this.splitData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitData.Location = new System.Drawing.Point(4, 4);
            this.splitData.Margin = new System.Windows.Forms.Padding(4);
            this.splitData.Name = "splitData";
            // 
            // splitData.Panel1
            // 
            this.splitData.Panel1.Controls.Add(this.tableLayoutPanel5);
            // 
            // splitData.Panel2
            // 
            this.splitData.Panel2.Controls.Add(this.splitDataView);
            this.splitData.Size = new System.Drawing.Size(1664, 845);
            this.splitData.SplitterDistance = 189;
            this.splitData.SplitterWidth = 5;
            this.splitData.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.dataTree, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.chkShowData, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.lblGrid, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.744958F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 95.25504F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(187, 843);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // dataTree
            // 
            this.tableLayoutPanel5.SetColumnSpan(this.dataTree, 2);
            this.dataTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataTree.HideSelection = false;
            this.dataTree.Location = new System.Drawing.Point(4, 43);
            this.dataTree.Margin = new System.Windows.Forms.Padding(4);
            this.dataTree.Name = "dataTree";
            this.dataTree.Size = new System.Drawing.Size(179, 796);
            this.dataTree.TabIndex = 0;
            this.dataTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dataTree_AfterSelect);
            this.dataTree.MouseEnter += new System.EventHandler(this.dataTree_MouseEnter);
            // 
            // chkShowData
            // 
            this.chkShowData.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkShowData.AutoSize = true;
            this.chkShowData.Checked = true;
            this.chkShowData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowData.Location = new System.Drawing.Point(87, 9);
            this.chkShowData.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowData.Name = "chkShowData";
            this.chkShowData.Size = new System.Drawing.Size(96, 21);
            this.chkShowData.TabIndex = 2;
            this.chkShowData.Text = "DataTable";
            this.chkShowData.UseVisualStyleBackColor = true;
            this.chkShowData.CheckedChanged += new System.EventHandler(this.chkShowData_CheckedChanged);
            // 
            // lblGrid
            // 
            this.lblGrid.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGrid.AutoSize = true;
            this.lblGrid.Location = new System.Drawing.Point(3, 11);
            this.lblGrid.Name = "lblGrid";
            this.lblGrid.Size = new System.Drawing.Size(35, 17);
            this.lblGrid.TabIndex = 3;
            this.lblGrid.Text = "Grid";
            // 
            // splitDataView
            // 
            this.splitDataView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitDataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDataView.Location = new System.Drawing.Point(0, 0);
            this.splitDataView.Margin = new System.Windows.Forms.Padding(4);
            this.splitDataView.Name = "splitDataView";
            this.splitDataView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitDataView.Panel1
            // 
            this.splitDataView.Panel1.Controls.Add(this.splitDataGrid);
            // 
            // splitDataView.Panel2
            // 
            this.splitDataView.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitDataView.Size = new System.Drawing.Size(1470, 845);
            this.splitDataView.SplitterDistance = 659;
            this.splitDataView.SplitterWidth = 5;
            this.splitDataView.TabIndex = 1;
            // 
            // splitDataGrid
            // 
            this.splitDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDataGrid.Location = new System.Drawing.Point(0, 0);
            this.splitDataGrid.Margin = new System.Windows.Forms.Padding(4);
            this.splitDataGrid.Name = "splitDataGrid";
            // 
            // splitDataGrid.Panel1
            // 
            this.splitDataGrid.Panel1.Controls.Add(this.splitDataTable);
            // 
            // splitDataGrid.Panel2
            // 
            this.splitDataGrid.Panel2.Controls.Add(this.zgvSeries);
            this.splitDataGrid.Size = new System.Drawing.Size(1468, 657);
            this.splitDataGrid.SplitterDistance = 341;
            this.splitDataGrid.SplitterWidth = 5;
            this.splitDataGrid.TabIndex = 1;
            // 
            // splitDataTable
            // 
            this.splitDataTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDataTable.Location = new System.Drawing.Point(0, 0);
            this.splitDataTable.Margin = new System.Windows.Forms.Padding(4);
            this.splitDataTable.Name = "splitDataTable";
            this.splitDataTable.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitDataTable.Panel1
            // 
            this.splitDataTable.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitDataTable.Panel2
            // 
            this.splitDataTable.Panel2.Controls.Add(this.grpMiss);
            this.splitDataTable.Size = new System.Drawing.Size(341, 657);
            this.splitDataTable.SplitterDistance = 537;
            this.splitDataTable.SplitterWidth = 5;
            this.splitDataTable.TabIndex = 1;
            // 
            // zgvSeries
            // 
            this.zgvSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgvSeries.Location = new System.Drawing.Point(0, 0);
            this.zgvSeries.Margin = new System.Windows.Forms.Padding(5);
            this.zgvSeries.Name = "zgvSeries";
            this.zgvSeries.ScrollGrace = 0D;
            this.zgvSeries.ScrollMaxX = 0D;
            this.zgvSeries.ScrollMaxY = 0D;
            this.zgvSeries.ScrollMaxY2 = 0D;
            this.zgvSeries.ScrollMinX = 0D;
            this.zgvSeries.ScrollMinY = 0D;
            this.zgvSeries.ScrollMinY2 = 0D;
            this.zgvSeries.Size = new System.Drawing.Size(1122, 657);
            this.zgvSeries.TabIndex = 0;
            this.zgvSeries.UseExtendedPrintDialog = true;
            this.zgvSeries.Load += new System.EventHandler(this.zgvSeries_Load);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 92.40088F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.599119F));
            this.tableLayoutPanel2.Controls.Add(this.grpMissing, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 179F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1468, 179);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 113F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel6.Controls.Add(this.splitData, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.statusStrip, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.btnClose, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1672, 892);
            this.tableLayoutPanel6.TabIndex = 2;
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 853);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1559, 39);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "toolStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(63, 36);
            this.lblStatus.Text = "Ready ...";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1563, 857);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmDataEDDE
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1672, 892);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel6);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDataEDDE";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EDDE Data Series";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmData_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabData.ResumeLayout(false);
            this.tabPageData.ResumeLayout(false);
            this.tabPageData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tabPageDaily.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDaily)).EndInit();
            this.tabPageMiss.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMiss)).EndInit();
            this.grpMiss.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMissCnt)).EndInit();
            this.grpMissing.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxStations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.splitData.Panel1.ResumeLayout(false);
            this.splitData.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitData)).EndInit();
            this.splitData.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.splitDataView.Panel1.ResumeLayout(false);
            this.splitDataView.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDataView)).EndInit();
            this.splitDataView.ResumeLayout(false);
            this.splitDataGrid.Panel1.ResumeLayout(false);
            this.splitDataGrid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDataGrid)).EndInit();
            this.splitDataGrid.ResumeLayout(false);
            this.splitDataTable.Panel1.ResumeLayout(false);
            this.splitDataTable.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDataTable)).EndInit();
            this.splitDataTable.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnEstimate;
        private System.Windows.Forms.GroupBox grpMissing;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TabControl tabData;
        private System.Windows.Forms.TabPage tabPageData;
        private System.Windows.Forms.TabPage tabPageMiss;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpMiss;
        private System.Windows.Forms.DataGridView dgvMissCnt;
        private System.Windows.Forms.Label lblSite;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitData;
        private System.Windows.Forms.TreeView dataTree;
        private System.Windows.Forms.SplitContainer splitDataView;
        private System.Windows.Forms.SplitContainer splitDataGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label lblGages;
        private System.Windows.Forms.CheckBox chkShowData;
        public ZedGraph.ZedGraphControl zgvSeries;
        public System.Windows.Forms.RadioButton optAll;
        public System.Windows.Forms.RadioButton optSite;
        private System.Windows.Forms.TabPage tabPageDaily;
        private System.Windows.Forms.DataGridView dgvDaily;
        public System.Windows.Forms.DataGridView dgvMiss;
        private System.Windows.Forms.RadioButton optInter;
        private System.Windows.Forms.RadioButton optOther;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblInterpolate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.SplitContainer splitDataTable;
        public System.Windows.Forms.ToolStrip statusStrip;
        public System.Windows.Forms.ToolStripLabel lblStatus;
        private System.Windows.Forms.Label lbRadius;
        private System.Windows.Forms.NumericUpDown numRadius;
        private System.Windows.Forms.Label lblnoSta;
        private System.Windows.Forms.NumericUpDown numMaxStations;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblGrid;
    }
}