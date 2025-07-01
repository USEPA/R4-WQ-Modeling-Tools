
namespace NCEIData
{
    partial class frmDownloadEDDE
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cboScenario = new System.Windows.Forms.ComboBox();
            this.cboPathway = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.grpVars = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.optTMP = new System.Windows.Forms.CheckBox();
            this.optWND = new System.Windows.Forms.CheckBox();
            this.optPCP = new System.Windows.Forms.CheckBox();
            this.optDEW = new System.Windows.Forms.CheckBox();
            this.optWDIR = new System.Windows.Forms.CheckBox();
            this.optSolar = new System.Windows.Forms.CheckBox();
            this.optLWdown = new System.Windows.Forms.CheckBox();
            this.optLWout = new System.Windows.Forms.CheckBox();
            this.optSensible = new System.Windows.Forms.CheckBox();
            this.optLatent = new System.Windows.Forms.CheckBox();
            this.optRH = new System.Windows.Forms.CheckBox();
            this.optCLO = new System.Windows.Forms.CheckBox();
            this.optPres = new System.Windows.Forms.CheckBox();
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
            this.grpModel = new System.Windows.Forms.GroupBox();
            this.lblGCM = new System.Windows.Forms.Label();
            this.grpSSP = new System.Windows.Forms.GroupBox();
            this.lblSSP = new System.Windows.Forms.Label();
            this.EDDElink = new System.Windows.Forms.LinkLabel();
            this.btnUse = new System.Windows.Forms.Button();
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
            this.grpModel.SuspendLayout();
            this.grpSSP.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.96109F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.03891F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 6, 2);
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
            this.tableLayoutPanel1.Controls.Add(this.grpModel, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.grpSSP, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.EDDElink, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnUse, 7, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1027, 619);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(401, 100);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 1);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Pressure";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 8);
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(13, 224);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(1001, 58);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 349F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(993, 35);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 16);
            this.label8.TabIndex = 0;
            this.label8.Text = "GCM Model:";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(507, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 16);
            this.label9.TabIndex = 1;
            this.label9.Text = "Pathway:";
            // 
            // cboScenario
            // 
            this.cboScenario.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboScenario.FormattingEnabled = true;
            this.cboScenario.Items.AddRange(new object[] {
            "CESM",
            "GFDL-CM3"});
            this.cboScenario.Location = new System.Drawing.Point(108, 5);
            this.cboScenario.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboScenario.Name = "cboScenario";
            this.cboScenario.Size = new System.Drawing.Size(337, 24);
            this.cboScenario.TabIndex = 2;
            this.cboScenario.SelectedIndexChanged += new System.EventHandler(this.cboScenario_SelectedIndexChanged);
            // 
            // cboPathway
            // 
            this.cboPathway.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboPathway.FormattingEnabled = true;
            this.cboPathway.Items.AddRange(new object[] {
            "RCP8.5",
            "Historical"});
            this.cboPathway.Location = new System.Drawing.Point(574, 5);
            this.cboPathway.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboPathway.Name = "cboPathway";
            this.cboPathway.Size = new System.Drawing.Size(175, 24);
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
            this.label4.Size = new System.Drawing.Size(1019, 53);
            this.label4.TabIndex = 5;
            this.label4.Text = "Specify range of dates for climate scenario, pathway series and variables to down" +
    "load.";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpVars
            // 
            this.grpVars.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.grpVars, 8);
            this.grpVars.Controls.Add(this.tableLayoutPanel3);
            this.grpVars.Location = new System.Drawing.Point(13, 111);
            this.grpVars.Margin = new System.Windows.Forms.Padding(4);
            this.grpVars.Name = "grpVars";
            this.grpVars.Padding = new System.Windows.Forms.Padding(4);
            this.grpVars.Size = new System.Drawing.Size(1001, 102);
            this.grpVars.TabIndex = 7;
            this.grpVars.TabStop = false;
            this.grpVars.Text = "Variables";
            this.grpVars.UseCompatibleTextRendering = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 10;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 135F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.optTMP, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.optWND, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.optPCP, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.optDEW, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.optWDIR, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.optSolar, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.optLWdown, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.optLWout, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.optSensible, 3, 1);
            this.tableLayoutPanel3.Controls.Add(this.optLatent, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.optRH, 6, 0);
            this.tableLayoutPanel3.Controls.Add(this.optCLO, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.optPres, 5, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 19);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(993, 79);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // optTMP
            // 
            this.optTMP.AutoSize = true;
            this.optTMP.Location = new System.Drawing.Point(139, 4);
            this.optTMP.Margin = new System.Windows.Forms.Padding(4);
            this.optTMP.Name = "optTMP";
            this.optTMP.Size = new System.Drawing.Size(107, 20);
            this.optTMP.TabIndex = 1;
            this.optTMP.Text = "Temperature";
            this.optTMP.UseVisualStyleBackColor = true;
            // 
            // optWND
            // 
            this.optWND.AutoSize = true;
            this.optWND.Location = new System.Drawing.Point(277, 4);
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
            this.optPCP.Location = new System.Drawing.Point(4, 4);
            this.optPCP.Margin = new System.Windows.Forms.Padding(4);
            this.optPCP.Name = "optPCP";
            this.optPCP.Size = new System.Drawing.Size(103, 20);
            this.optPCP.TabIndex = 0;
            this.optPCP.Text = "Precipitation";
            this.optPCP.UseVisualStyleBackColor = true;
            // 
            // optDEW
            // 
            this.optDEW.AutoSize = true;
            this.optDEW.Location = new System.Drawing.Point(556, 4);
            this.optDEW.Margin = new System.Windows.Forms.Padding(4);
            this.optDEW.Name = "optDEW";
            this.optDEW.Size = new System.Drawing.Size(86, 20);
            this.optDEW.TabIndex = 5;
            this.optDEW.Text = "DewPoint";
            this.optDEW.UseVisualStyleBackColor = true;
            // 
            // optWDIR
            // 
            this.optWDIR.AutoSize = true;
            this.optWDIR.Location = new System.Drawing.Point(409, 2);
            this.optWDIR.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.optWDIR.Name = "optWDIR";
            this.optWDIR.Size = new System.Drawing.Size(80, 20);
            this.optWDIR.TabIndex = 9;
            this.optWDIR.Text = "Wind Dir";
            this.optWDIR.UseVisualStyleBackColor = true;
            // 
            // optSolar
            // 
            this.optSolar.AutoSize = true;
            this.optSolar.Location = new System.Drawing.Point(4, 44);
            this.optSolar.Margin = new System.Windows.Forms.Padding(4);
            this.optSolar.Name = "optSolar";
            this.optSolar.Size = new System.Drawing.Size(112, 20);
            this.optSolar.TabIndex = 3;
            this.optSolar.Text = "SW Radiation";
            this.optSolar.UseVisualStyleBackColor = true;
            // 
            // optLWdown
            // 
            this.optLWdown.AutoSize = true;
            this.optLWdown.Location = new System.Drawing.Point(139, 44);
            this.optLWdown.Margin = new System.Windows.Forms.Padding(4);
            this.optLWdown.Name = "optLWdown";
            this.optLWdown.Size = new System.Drawing.Size(110, 20);
            this.optLWdown.TabIndex = 10;
            this.optLWdown.Text = "LW Radiation";
            this.optLWdown.UseVisualStyleBackColor = true;
            // 
            // optLWout
            // 
            this.optLWout.AutoSize = true;
            this.optLWout.Location = new System.Drawing.Point(276, 43);
            this.optLWout.Name = "optLWout";
            this.optLWout.Size = new System.Drawing.Size(106, 20);
            this.optLWout.TabIndex = 11;
            this.optLWout.Text = "Outgoing LW";
            this.optLWout.UseVisualStyleBackColor = true;
            // 
            // optSensible
            // 
            this.optSensible.AutoSize = true;
            this.optSensible.Location = new System.Drawing.Point(409, 43);
            this.optSensible.Name = "optSensible";
            this.optSensible.Size = new System.Drawing.Size(114, 20);
            this.optSensible.TabIndex = 12;
            this.optSensible.Text = "Sensible Heat";
            this.optSensible.UseVisualStyleBackColor = true;
            // 
            // optLatent
            // 
            this.optLatent.AutoSize = true;
            this.optLatent.Location = new System.Drawing.Point(555, 43);
            this.optLatent.Name = "optLatent";
            this.optLatent.Size = new System.Drawing.Size(97, 20);
            this.optLatent.TabIndex = 13;
            this.optLatent.Text = "Latent Heat";
            this.optLatent.UseVisualStyleBackColor = true;
            // 
            // optRH
            // 
            this.optRH.AutoSize = true;
            this.optRH.Location = new System.Drawing.Point(811, 4);
            this.optRH.Margin = new System.Windows.Forms.Padding(4);
            this.optRH.Name = "optRH";
            this.optRH.Size = new System.Drawing.Size(55, 20);
            this.optRH.TabIndex = 6;
            this.optRH.Text = "R.H.";
            this.optRH.UseVisualStyleBackColor = true;
            // 
            // optCLO
            // 
            this.optCLO.AutoSize = true;
            this.optCLO.Location = new System.Drawing.Point(702, 2);
            this.optCLO.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.optCLO.Name = "optCLO";
            this.optCLO.Size = new System.Drawing.Size(64, 20);
            this.optCLO.TabIndex = 8;
            this.optCLO.Text = "Cloud";
            this.optCLO.UseVisualStyleBackColor = true;
            // 
            // optPres
            // 
            this.optPres.AutoSize = true;
            this.optPres.Location = new System.Drawing.Point(703, 44);
            this.optPres.Margin = new System.Windows.Forms.Padding(4);
            this.optPres.Name = "optPres";
            this.optPres.Size = new System.Drawing.Size(83, 20);
            this.optPres.TabIndex = 7;
            this.optPres.Text = "Pressure";
            this.optPres.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 8);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(13, 290);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(1001, 1);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(993, 0);
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
            this.btnOK.Location = new System.Drawing.Point(925, 581);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(89, 30);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "Download";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(815, 581);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 30);
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
            2025,
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
            2100,
            0,
            0,
            0});
            // 
            // grpModel
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpModel, 8);
            this.grpModel.Controls.Add(this.lblGCM);
            this.grpModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpModel.Location = new System.Drawing.Point(12, 297);
            this.grpModel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpModel.Name = "grpModel";
            this.grpModel.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpModel.Size = new System.Drawing.Size(1003, 139);
            this.grpModel.TabIndex = 15;
            this.grpModel.TabStop = false;
            this.grpModel.Text = "GCM Model";
            // 
            // lblGCM
            // 
            this.lblGCM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGCM.Location = new System.Drawing.Point(3, 17);
            this.lblGCM.Name = "lblGCM";
            this.lblGCM.Size = new System.Drawing.Size(997, 120);
            this.lblGCM.TabIndex = 0;
            this.lblGCM.Text = "label10";
            // 
            // grpSSP
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpSSP, 8);
            this.grpSSP.Controls.Add(this.lblSSP);
            this.grpSSP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSSP.Location = new System.Drawing.Point(12, 440);
            this.grpSSP.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpSSP.Name = "grpSSP";
            this.grpSSP.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpSSP.Size = new System.Drawing.Size(1003, 122);
            this.grpSSP.TabIndex = 16;
            this.grpSSP.TabStop = false;
            this.grpSSP.Text = "SSP/RCP Pathway";
            // 
            // lblSSP
            // 
            this.lblSSP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSSP.Location = new System.Drawing.Point(3, 17);
            this.lblSSP.Name = "lblSSP";
            this.lblSSP.Size = new System.Drawing.Size(997, 103);
            this.lblSSP.TabIndex = 0;
            this.lblSSP.Text = "label11";
            // 
            // EDDElink
            // 
            this.EDDElink.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.EDDElink.AutoSize = true;
            this.EDDElink.Location = new System.Drawing.Point(422, 66);
            this.EDDElink.Name = "EDDElink";
            this.EDDElink.Size = new System.Drawing.Size(361, 16);
            this.EDDElink.TabIndex = 18;
            this.EDDElink.TabStop = true;
            this.EDDElink.Text = "Readme: EPA Dynamically Downscaled Ensemble (EDDE)";
            this.EDDElink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EDDElink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.EDDElink_LinkClicked);
            // 
            // btnUse
            // 
            this.btnUse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.btnUse, 2);
            this.btnUse.Location = new System.Drawing.Point(811, 58);
            this.btnUse.Name = "btnUse";
            this.btnUse.Size = new System.Drawing.Size(204, 32);
            this.btnUse.TabIndex = 19;
            this.btnUse.Text = "Use Constraint";
            this.btnUse.UseVisualStyleBackColor = true;
            this.btnUse.Click += new System.EventHandler(this.btnUse_Click);
            // 
            // frmDownloadEDDE
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1027, 619);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmDownloadEDDE";
            this.Text = "EDDE Download Options";
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
            this.grpModel.ResumeLayout(false);
            this.grpSSP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cboScenario;
        private System.Windows.Forms.ComboBox cboPathway;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grpVars;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox optDEW;
        private System.Windows.Forms.CheckBox optTMP;
        private System.Windows.Forms.CheckBox optSolar;
        private System.Windows.Forms.CheckBox optWND;
        private System.Windows.Forms.CheckBox optPCP;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown numPercentMiss;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numYearFrom;
        private System.Windows.Forms.NumericUpDown numYearTo;
        private System.Windows.Forms.GroupBox grpModel;
        private System.Windows.Forms.Label lblGCM;
        private System.Windows.Forms.GroupBox grpSSP;
        private System.Windows.Forms.Label lblSSP;
        private System.Windows.Forms.CheckBox optRH;
        private System.Windows.Forms.LinkLabel EDDElink;
        private System.Windows.Forms.CheckBox optPres;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox optCLO;
        private System.Windows.Forms.CheckBox optWDIR;
        private System.Windows.Forms.CheckBox optLWdown;
        private System.Windows.Forms.CheckBox optLWout;
        private System.Windows.Forms.CheckBox optSensible;
        private System.Windows.Forms.CheckBox optLatent;
        private System.Windows.Forms.Button btnUse;
    }
}