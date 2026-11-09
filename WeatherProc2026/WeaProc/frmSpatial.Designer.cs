
namespace NCEIData
{
    partial class frmSpatial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSpatial));
            this.grpTable = new System.Windows.Forms.GroupBox();
            this.dgvSeaWDM = new System.Windows.Forms.DataGridView();
            this.zedSeries = new ZedGraph.ZedGraphControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSpatial = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboVar = new System.Windows.Forms.ComboBox();
            this.cboPeriod = new System.Windows.Forms.ComboBox();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.splitTableGraph = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.grpGraph = new System.Windows.Forms.GroupBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.dgvStats = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.numBin = new System.Windows.Forms.NumericUpDown();
            this.chkShowGrpTable = new System.Windows.Forms.CheckBox();
            this.chkShowTable = new System.Windows.Forms.CheckBox();
            this.chkSelected = new System.Windows.Forms.CheckBox();
            this.grpTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeaWDM)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitTableGraph)).BeginInit();
            this.splitTableGraph.Panel1.SuspendLayout();
            this.splitTableGraph.Panel2.SuspendLayout();
            this.splitTableGraph.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.grpGraph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBin)).BeginInit();
            this.SuspendLayout();
            // 
            // grpTable
            // 
            this.grpTable.Controls.Add(this.dgvSeaWDM);
            this.grpTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTable.Location = new System.Drawing.Point(0, 0);
            this.grpTable.Name = "grpTable";
            this.grpTable.Size = new System.Drawing.Size(250, 1);
            this.grpTable.TabIndex = 0;
            this.grpTable.TabStop = false;
            // 
            // dgvSeaWDM
            // 
            this.dgvSeaWDM.AllowUserToAddRows = false;
            this.dgvSeaWDM.AllowUserToDeleteRows = false;
            this.dgvSeaWDM.AllowUserToResizeRows = false;
            this.dgvSeaWDM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSeaWDM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSeaWDM.Location = new System.Drawing.Point(3, 16);
            this.dgvSeaWDM.Name = "dgvSeaWDM";
            this.dgvSeaWDM.ReadOnly = true;
            this.dgvSeaWDM.RowHeadersWidth = 10;
            this.dgvSeaWDM.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSeaWDM.Size = new System.Drawing.Size(244, 0);
            this.dgvSeaWDM.TabIndex = 0;
            // 
            // zedSeries
            // 
            this.zedSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedSeries.Location = new System.Drawing.Point(3, 16);
            this.zedSeries.Name = "zedSeries";
            this.zedSeries.ScrollGrace = 0D;
            this.zedSeries.ScrollMaxX = 0D;
            this.zedSeries.ScrollMaxY = 0D;
            this.zedSeries.ScrollMaxY2 = 0D;
            this.zedSeries.ScrollMinX = 0D;
            this.zedSeries.ScrollMinY = 0D;
            this.zedSeries.ScrollMinY2 = 0D;
            this.zedSeries.Size = new System.Drawing.Size(297, 0);
            this.zedSeries.TabIndex = 21;
            this.zedSeries.UseExtendedPrintDialog = true;
            this.zedSeries.MouseClick += new System.Windows.Forms.MouseEventHandler(this.zedSeries_MouseClick);
            this.zedSeries.MouseEnter += new System.EventHandler(this.zedSeries_MouseEnter);
            this.zedSeries.MouseLeave += new System.EventHandler(this.zedSeries_MouseLeave);
            this.zedSeries.MouseHover += new System.EventHandler(this.zedSeries_MouseHover);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 9;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 111F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 127F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblSpatial, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cboVar, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cboPeriod, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPeriod, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.splitTableGraph, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.numBin, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkShowGrpTable, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkShowTable, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkSelected, 5, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(569, 139);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // lblSpatial
            // 
            this.lblSpatial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpatial.AutoSize = true;
            this.lblSpatial.BackColor = System.Drawing.Color.LemonChiffon;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSpatial, 9);
            this.lblSpatial.Location = new System.Drawing.Point(3, 14);
            this.lblSpatial.Name = "lblSpatial";
            this.lblSpatial.Size = new System.Drawing.Size(563, 26);
            this.lblSpatial.TabIndex = 3;
            this.lblSpatial.Text = "Spatial Mapping of Annual Weather Variables. Select variable and time period and " +
    "specify number of categories. Click on Show Table/Graph to enable datatable and " +
    "graph.";
            this.lblSpatial.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Parameter";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cboVar
            // 
            this.cboVar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboVar.FormattingEnabled = true;
            this.cboVar.Location = new System.Drawing.Point(96, 59);
            this.cboVar.Name = "cboVar";
            this.cboVar.Size = new System.Drawing.Size(114, 21);
            this.cboVar.TabIndex = 16;
            this.cboVar.SelectedIndexChanged += new System.EventHandler(this.cboVar_SelectedIndexChanged);
            // 
            // cboPeriod
            // 
            this.cboPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboPeriod.FormattingEnabled = true;
            this.cboPeriod.Location = new System.Drawing.Point(306, 59);
            this.cboPeriod.Name = "cboPeriod";
            this.cboPeriod.Size = new System.Drawing.Size(105, 21);
            this.cboPeriod.TabIndex = 17;
            this.cboPeriod.SelectedIndexChanged += new System.EventHandler(this.cboPeriod_SelectedIndexChanged);
            // 
            // lblPeriod
            // 
            this.lblPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPeriod.AutoSize = true;
            this.lblPeriod.Location = new System.Drawing.Point(224, 63);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(70, 13);
            this.lblPeriod.TabIndex = 18;
            this.lblPeriod.Text = "Select Period";
            // 
            // splitTableGraph
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.splitTableGraph, 9);
            this.splitTableGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTableGraph.Location = new System.Drawing.Point(3, 116);
            this.splitTableGraph.Name = "splitTableGraph";
            // 
            // splitTableGraph.Panel1
            // 
            this.splitTableGraph.Panel1.Controls.Add(this.grpTable);
            // 
            // splitTableGraph.Panel2
            // 
            this.splitTableGraph.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitTableGraph.Size = new System.Drawing.Size(563, 1);
            this.splitTableGraph.SplitterDistance = 250;
            this.splitTableGraph.TabIndex = 4;
            this.splitTableGraph.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitTableGraph_SplitterMoved);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 218F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.grpGraph, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblInfo, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.dgvStats, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(309, 1);
            this.tableLayoutPanel2.TabIndex = 23;
            // 
            // grpGraph
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.grpGraph, 2);
            this.grpGraph.Controls.Add(this.zedSeries);
            this.grpGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpGraph.Location = new System.Drawing.Point(3, 36);
            this.grpGraph.Name = "grpGraph";
            this.grpGraph.Size = new System.Drawing.Size(303, 1);
            this.grpGraph.TabIndex = 22;
            this.grpGraph.TabStop = false;
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.AutoSize = true;
            this.lblInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.tableLayoutPanel2.SetColumnSpan(this.lblInfo, 2);
            this.lblInfo.Location = new System.Drawing.Point(3, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(303, 33);
            this.lblInfo.TabIndex = 23;
            this.lblInfo.Text = "Hover on a point to display values. Left-click to highlight station in the datata" +
    "ble and scroll on the datatable to the highlighted station.";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dgvStats
            // 
            this.dgvStats.AllowUserToAddRows = false;
            this.dgvStats.AllowUserToDeleteRows = false;
            this.dgvStats.AllowUserToResizeRows = false;
            this.dgvStats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel2.SetColumnSpan(this.dgvStats, 2);
            this.dgvStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStats.Location = new System.Drawing.Point(3, -52);
            this.dgvStats.Name = "dgvStats";
            this.dgvStats.ReadOnly = true;
            this.dgvStats.RowHeadersWidth = 5;
            this.dgvStats.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStats.Size = new System.Drawing.Size(303, 50);
            this.dgvStats.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "No. Categories";
            // 
            // numBin
            // 
            this.numBin.Location = new System.Drawing.Point(96, 87);
            this.numBin.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numBin.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numBin.Name = "numBin";
            this.numBin.Size = new System.Drawing.Size(48, 20);
            this.numBin.TabIndex = 24;
            this.numBin.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // chkShowGrpTable
            // 
            this.chkShowGrpTable.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowGrpTable.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkShowGrpTable, 2);
            this.chkShowGrpTable.Location = new System.Drawing.Point(224, 90);
            this.chkShowGrpTable.Name = "chkShowGrpTable";
            this.chkShowGrpTable.Size = new System.Drawing.Size(117, 17);
            this.chkShowGrpTable.TabIndex = 25;
            this.chkShowGrpTable.Text = "Show Table/Graph";
            this.chkShowGrpTable.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.chkShowGrpTable.UseVisualStyleBackColor = true;
            this.chkShowGrpTable.CheckedChanged += new System.EventHandler(this.chkShowGrpTable_CheckedChanged);
            // 
            // chkShowTable
            // 
            this.chkShowTable.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowTable.AutoSize = true;
            this.chkShowTable.Location = new System.Drawing.Point(417, 61);
            this.chkShowTable.Name = "chkShowTable";
            this.chkShowTable.Size = new System.Drawing.Size(101, 17);
            this.chkShowTable.TabIndex = 19;
            this.chkShowTable.Text = "Hide DataTable";
            this.chkShowTable.UseVisualStyleBackColor = true;
            this.chkShowTable.Visible = false;
            this.chkShowTable.CheckedChanged += new System.EventHandler(this.chkShowTable_CheckedChanged);
            // 
            // chkSelected
            // 
            this.chkSelected.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSelected.AutoSize = true;
            this.chkSelected.Location = new System.Drawing.Point(417, 90);
            this.chkSelected.Name = "chkSelected";
            this.chkSelected.Size = new System.Drawing.Size(98, 17);
            this.chkSelected.TabIndex = 26;
            this.chkSelected.Text = "Show Selected";
            this.chkSelected.UseVisualStyleBackColor = true;
            this.chkSelected.Visible = false;
            this.chkSelected.CheckedChanged += new System.EventHandler(this.chkSelected_CheckedChanged);
            // 
            // frmSpatial
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 139);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSpatial";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spatial Analysis";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSpatial_FormClosed);
            this.Load += new System.EventHandler(this.frmSpatial_Load);
            this.grpTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeaWDM)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitTableGraph.Panel1.ResumeLayout(false);
            this.splitTableGraph.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTableGraph)).EndInit();
            this.splitTableGraph.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.grpGraph.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpTable;
        private System.Windows.Forms.DataGridView dgvSeaWDM;
        private ZedGraph.ZedGraphControl zedSeries;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.Label lblSpatial;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboVar;
        private System.Windows.Forms.ComboBox cboPeriod;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.SplitContainer splitTableGraph;
        private System.Windows.Forms.CheckBox chkShowTable;
        private System.Windows.Forms.GroupBox grpGraph;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.DataGridView dgvStats;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numBin;
        private System.Windows.Forms.CheckBox chkShowGrpTable;
        public System.Windows.Forms.CheckBox chkSelected;
    }
}