namespace NCEIData
{
    partial class frmDownloadCMIP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDownloadCMIP));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cboScenario = new System.Windows.Forms.ComboBox();
            this.cboPathway = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.grpVars = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.optRH = new System.Windows.Forms.CheckBox();
            this.optTMP = new System.Windows.Forms.CheckBox();
            this.optLW = new System.Windows.Forms.CheckBox();
            this.optSW = new System.Windows.Forms.CheckBox();
            this.optWND = new System.Windows.Forms.CheckBox();
            this.optPCP = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.numPercentMiss = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numYearFrom = new System.Windows.Forms.NumericUpDown();
            this.numYearTo = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numMinYears = new System.Windows.Forms.NumericUpDown();
            this.grpModel = new System.Windows.Forms.GroupBox();
            this.lblGCM = new System.Windows.Forms.Label();
            this.grpSSP = new System.Windows.Forms.GroupBox();
            this.lblSSP = new System.Windows.Forms.Label();
            this.CMIP6link = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.grpVars.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPercentMiss)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYearFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYearTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinYears)).BeginInit();
            this.grpModel.SuspendLayout();
            this.grpSSP.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 11F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 113F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.51351F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 86.48649F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.grpVars, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.btnOK, 8, 9);
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 7, 9);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.numYearFrom, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.numYearTo, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.label6, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.numMinYears, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.grpModel, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.grpSSP, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.CMIP6link, 7, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(797, 474);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 8);
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(15, 175);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(765, 62);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 325F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 141F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 441F));
            this.tableLayoutPanel4.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label9, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.cboScenario, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.cboPathway, 3, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 19);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(757, 39);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 16);
            this.label8.TabIndex = 0;
            this.label8.Text = "GCM Model:";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(433, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(123, 16);
            this.label9.TabIndex = 1;
            this.label9.Text = "SSP/RCP Pathway:";
            // 
            // cboScenario
            // 
            this.cboScenario.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboScenario.FormattingEnabled = true;
            this.cboScenario.Items.AddRange(new object[] {
            "ACCESS-CM2",
            "ACCESS-ESM1-5",
            "BCC-CSM2-MR",
            "CanESM5",
            "CESM2",
            "CESM2-WACCM",
            "CMCC-CM2-SR5",
            "CMCC-ESM2",
            "CNRM-CM6-1",
            "CNRM-ESM2-1",
            "EC-Earth3",
            "EC-Earth3-Veg-LR",
            "FGOALS-g3",
            "GFDL-CM4",
            "GFDL-CM4_gr2",
            "GFDL-ESM4",
            "GISS-E2-1-G",
            "HadGEM3-GC31-LL",
            "HadGEM3-GC31-MM",
            "IITM-ESM",
            "INM-CM4-8",
            "INM-CM5-0",
            "IPSL-CM6A-LR",
            "KACE-1-0-G",
            "KIOST-ESM",
            "MIROC6",
            "MIROC-ES2L",
            "MPI-ESM1-2-HR",
            "MPI-ESM1-2-LR",
            "MRI-ESM2-0",
            "NESM3",
            "NorESM2-LM",
            "NorESM2-MM",
            "TaiESM1",
            "UKESM1-0-LL"});
            this.cboScenario.Location = new System.Drawing.Point(108, 7);
            this.cboScenario.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboScenario.Name = "cboScenario";
            this.cboScenario.Size = new System.Drawing.Size(319, 24);
            this.cboScenario.TabIndex = 2;
            this.cboScenario.SelectedIndexChanged += new System.EventHandler(this.cboScenario_SelectedIndexChanged);
            // 
            // cboPathway
            // 
            this.cboPathway.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboPathway.FormattingEnabled = true;
            this.cboPathway.Items.AddRange(new object[] {
            "SSP585",
            "SSP370",
            "SSP245",
            "SSP126",
            "Historical"});
            this.cboPathway.Location = new System.Drawing.Point(574, 7);
            this.cboPathway.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboPathway.Name = "cboPathway";
            this.cboPathway.Size = new System.Drawing.Size(95, 24);
            this.cboPathway.TabIndex = 3;
            this.cboPathway.SelectedIndexChanged += new System.EventHandler(this.cboPathway_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Info;
            this.tableLayoutPanel1.SetColumnSpan(this.label4, 10);
            this.label4.Location = new System.Drawing.Point(4, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(789, 53);
            this.label4.TabIndex = 5;
            this.label4.Text = "Specify range of dates for climate scenario, representative concentration pathway" +
    " series and variables to download.";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpVars
            // 
            this.grpVars.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.grpVars, 8);
            this.grpVars.Controls.Add(this.tableLayoutPanel3);
            this.grpVars.Location = new System.Drawing.Point(15, 109);
            this.grpVars.Margin = new System.Windows.Forms.Padding(4);
            this.grpVars.Name = "grpVars";
            this.grpVars.Padding = new System.Windows.Forms.Padding(4);
            this.grpVars.Size = new System.Drawing.Size(765, 58);
            this.grpVars.TabIndex = 7;
            this.grpVars.TabStop = false;
            this.grpVars.Text = "Variables";
            this.grpVars.UseCompatibleTextRendering = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 7;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 122F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 137F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 258F));
            this.tableLayoutPanel3.Controls.Add(this.optRH, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.optTMP, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.optLW, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.optSW, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.optWND, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.optPCP, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 19);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(757, 35);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // optRH
            // 
            this.optRH.AutoSize = true;
            this.optRH.Checked = true;
            this.optRH.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optRH.Location = new System.Drawing.Point(614, 4);
            this.optRH.Margin = new System.Windows.Forms.Padding(4);
            this.optRH.Name = "optRH";
            this.optRH.Size = new System.Drawing.Size(105, 20);
            this.optRH.TabIndex = 5;
            this.optRH.Text = "Rel.Humidity";
            this.optRH.UseVisualStyleBackColor = true;
            // 
            // optTMP
            // 
            this.optTMP.AutoSize = true;
            this.optTMP.Checked = true;
            this.optTMP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optTMP.Location = new System.Drawing.Point(128, 4);
            this.optTMP.Margin = new System.Windows.Forms.Padding(4);
            this.optTMP.Name = "optTMP";
            this.optTMP.Size = new System.Drawing.Size(107, 20);
            this.optTMP.TabIndex = 1;
            this.optTMP.Text = "Temperature";
            this.optTMP.UseVisualStyleBackColor = true;
            // 
            // optLW
            // 
            this.optLW.AutoSize = true;
            this.optLW.Checked = true;
            this.optLW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optLW.Location = new System.Drawing.Point(490, 4);
            this.optLW.Margin = new System.Windows.Forms.Padding(4);
            this.optLW.Name = "optLW";
            this.optLW.Size = new System.Drawing.Size(110, 20);
            this.optLW.TabIndex = 4;
            this.optLW.Text = "LW Radiation";
            this.optLW.UseVisualStyleBackColor = true;
            // 
            // optSW
            // 
            this.optSW.AutoSize = true;
            this.optSW.Checked = true;
            this.optSW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optSW.Location = new System.Drawing.Point(353, 4);
            this.optSW.Margin = new System.Windows.Forms.Padding(4);
            this.optSW.Name = "optSW";
            this.optSW.Size = new System.Drawing.Size(112, 20);
            this.optSW.TabIndex = 3;
            this.optSW.Text = "SW Radiation";
            this.optSW.UseVisualStyleBackColor = true;
            // 
            // optWND
            // 
            this.optWND.AutoSize = true;
            this.optWND.Checked = true;
            this.optWND.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optWND.Location = new System.Drawing.Point(250, 4);
            this.optWND.Margin = new System.Windows.Forms.Padding(4);
            this.optWND.Name = "optWND";
            this.optWND.Size = new System.Drawing.Size(60, 20);
            this.optWND.TabIndex = 2;
            this.optWND.Text = "Wind";
            this.optWND.UseVisualStyleBackColor = true;
            // 
            // optPCP
            // 
            this.optPCP.AutoSize = true;
            this.optPCP.Checked = true;
            this.optPCP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optPCP.Location = new System.Drawing.Point(4, 4);
            this.optPCP.Margin = new System.Windows.Forms.Padding(4);
            this.optPCP.Name = "optPCP";
            this.optPCP.Size = new System.Drawing.Size(103, 20);
            this.optPCP.TabIndex = 0;
            this.optPCP.Text = "Precipitation";
            this.optPCP.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 8);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(15, 245);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(765, 1);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 155F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 317F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 476F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.numPercentMiss, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 19);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(757, 0);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 1);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ignore timeseries with";
            this.label1.Visible = false;
            // 
            // numPercentMiss
            // 
            this.numPercentMiss.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numPercentMiss.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.numPercentMiss.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numPercentMiss.Location = new System.Drawing.Point(159, 4);
            this.numPercentMiss.Margin = new System.Windows.Forms.Padding(4);
            this.numPercentMiss.Maximum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numPercentMiss.Name = "numPercentMiss";
            this.numPercentMiss.Size = new System.Drawing.Size(53, 22);
            this.numPercentMiss.TabIndex = 1;
            this.numPercentMiss.ThousandsSeparator = true;
            this.numPercentMiss.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numPercentMiss.Visible = false;
            this.numPercentMiss.ValueChanged += new System.EventHandler(this.numPercentMiss_ValueChanged);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(224, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(203, 1);
            this.label5.TabIndex = 2;
            this.label5.Text = "percent of daily data are missing.";
            this.label5.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(695, 439);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(85, 30);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "Download";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(602, 439);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "From Year:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 66);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "To Year:";
            // 
            // numYearFrom
            // 
            this.numYearFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numYearFrom.Location = new System.Drawing.Point(127, 63);
            this.numYearFrom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numYearFrom.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.numYearFrom.Minimum = new decimal(new int[] {
            1950,
            0,
            0,
            0});
            this.numYearFrom.Name = "numYearFrom";
            this.numYearFrom.Size = new System.Drawing.Size(71, 22);
            this.numYearFrom.TabIndex = 13;
            this.numYearFrom.Value = new decimal(new int[] {
            1950,
            0,
            0,
            0});
            // 
            // numYearTo
            // 
            this.numYearTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numYearTo.Location = new System.Drawing.Point(323, 63);
            this.numYearTo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numYearTo.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.numYearTo.Minimum = new decimal(new int[] {
            1950,
            0,
            0,
            0});
            this.numYearTo.Name = "numYearTo";
            this.numYearTo.Size = new System.Drawing.Size(71, 22);
            this.numYearTo.TabIndex = 14;
            this.numYearTo.Value = new decimal(new int[] {
            1950,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 435);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 39);
            this.label7.TabIndex = 2;
            this.label7.Text = "Timeseries with less than the specified minimum years of record are ignored.";
            this.label7.Visible = false;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(128, 435);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 39);
            this.label6.TabIndex = 0;
            this.label6.Text = "Minimum Years of Record:";
            this.label6.Visible = false;
            // 
            // numMinYears
            // 
            this.numMinYears.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numMinYears.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.numMinYears.Location = new System.Drawing.Point(205, 443);
            this.numMinYears.Margin = new System.Windows.Forms.Padding(4);
            this.numMinYears.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMinYears.Name = "numMinYears";
            this.numMinYears.Size = new System.Drawing.Size(32, 22);
            this.numMinYears.TabIndex = 1;
            this.numMinYears.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMinYears.Visible = false;
            // 
            // grpModel
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpModel, 8);
            this.grpModel.Controls.Add(this.lblGCM);
            this.grpModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpModel.Location = new System.Drawing.Point(14, 252);
            this.grpModel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpModel.Name = "grpModel";
            this.grpModel.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpModel.Size = new System.Drawing.Size(767, 82);
            this.grpModel.TabIndex = 15;
            this.grpModel.TabStop = false;
            this.grpModel.Text = "GCM Model";
            // 
            // lblGCM
            // 
            this.lblGCM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGCM.Location = new System.Drawing.Point(3, 17);
            this.lblGCM.Name = "lblGCM";
            this.lblGCM.Size = new System.Drawing.Size(761, 63);
            this.lblGCM.TabIndex = 0;
            this.lblGCM.Text = "label10";
            // 
            // grpSSP
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpSSP, 8);
            this.grpSSP.Controls.Add(this.lblSSP);
            this.grpSSP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSSP.Location = new System.Drawing.Point(14, 338);
            this.grpSSP.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpSSP.Name = "grpSSP";
            this.grpSSP.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpSSP.Size = new System.Drawing.Size(767, 86);
            this.grpSSP.TabIndex = 16;
            this.grpSSP.TabStop = false;
            this.grpSSP.Text = "SSP/RCP Pathway";
            // 
            // lblSSP
            // 
            this.lblSSP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSSP.Location = new System.Drawing.Point(3, 17);
            this.lblSSP.Name = "lblSSP";
            this.lblSSP.Size = new System.Drawing.Size(761, 67);
            this.lblSSP.TabIndex = 0;
            this.lblSSP.Text = "label11";
            // 
            // CMIP6link
            // 
            this.CMIP6link.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.CMIP6link.AutoSize = true;
            this.CMIP6link.Location = new System.Drawing.Point(439, 66);
            this.CMIP6link.Name = "CMIP6link";
            this.CMIP6link.Size = new System.Drawing.Size(248, 16);
            this.CMIP6link.TabIndex = 17;
            this.CMIP6link.TabStop = true;
            this.CMIP6link.Text = "Readme NEX-GDDP-CMIP6";
            this.CMIP6link.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CMIP6link_LinkClicked);
            // 
            // frmDownloadCMIP
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(797, 474);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDownloadCMIP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Download Options";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.grpVars.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPercentMiss)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYearFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYearTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinYears)).EndInit();
            this.grpModel.ResumeLayout(false);
            this.grpSSP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grpVars;
        private System.Windows.Forms.CheckBox optTMP;
        private System.Windows.Forms.CheckBox optLW;
        private System.Windows.Forms.CheckBox optSW;
        private System.Windows.Forms.CheckBox optWND;
        private System.Windows.Forms.CheckBox optPCP;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown numPercentMiss;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown numMinYears;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox optRH;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cboScenario;
        private System.Windows.Forms.ComboBox cboPathway;
        private System.Windows.Forms.NumericUpDown numYearFrom;
        private System.Windows.Forms.NumericUpDown numYearTo;
        private System.Windows.Forms.GroupBox grpModel;
        private System.Windows.Forms.GroupBox grpSSP;
        private System.Windows.Forms.Label lblGCM;
        private System.Windows.Forms.Label lblSSP;
        private System.Windows.Forms.LinkLabel CMIP6link;
    }
}