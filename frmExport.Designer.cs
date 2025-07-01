namespace NCEIData
{
    partial class frmExport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExport));
            this.layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblExport = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.splitWDMTab = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.grpTable = new System.Windows.Forms.GroupBox();
            this.dgvWDM = new System.Windows.Forms.DataGridView();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnClearSelection = new System.Windows.Forms.Button();
            this.TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.grpSelSeries = new System.Windows.Forms.GroupBox();
            this.dgvSelSeries = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statuslbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblOut = new System.Windows.Forms.Label();
            this.dblabel = new System.Windows.Forms.Label();
            this.lblSDB = new System.Windows.Forms.Label();
            this.btnOut = new System.Windows.Forms.Button();
            this.txtOut = new System.Windows.Forms.Label();
            this.layoutMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitWDMTab)).BeginInit();
            this.splitWDMTab.Panel1.SuspendLayout();
            this.splitWDMTab.Panel2.SuspendLayout();
            this.splitWDMTab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWDM)).BeginInit();
            this.TableLayoutPanel2.SuspendLayout();
            this.grpSelSeries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelSeries)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutMain
            // 
            this.layoutMain.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.layoutMain.ColumnCount = 5;
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.layoutMain.Controls.Add(this.lblExport, 2, 1);
            this.layoutMain.Controls.Add(this.btnExport, 1, 1);
            this.layoutMain.Controls.Add(this.splitWDMTab, 1, 4);
            this.layoutMain.Controls.Add(this.statusStrip, 1, 5);
            this.layoutMain.Controls.Add(this.lblOut, 1, 2);
            this.layoutMain.Controls.Add(this.dblabel, 1, 3);
            this.layoutMain.Controls.Add(this.lblSDB, 2, 3);
            this.layoutMain.Controls.Add(this.btnOut, 3, 2);
            this.layoutMain.Controls.Add(this.txtOut, 2, 2);
            this.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMain.Location = new System.Drawing.Point(0, 0);
            this.layoutMain.Name = "layoutMain";
            this.layoutMain.RowCount = 6;
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.layoutMain.Size = new System.Drawing.Size(669, 438);
            this.layoutMain.TabIndex = 2;
            // 
            // lblExport
            // 
            this.lblExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExport.AutoSize = true;
            this.lblExport.Location = new System.Drawing.Point(108, 17);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(507, 13);
            this.lblExport.TabIndex = 2;
            this.lblExport.Text = "Routine exports selected WDM TimeSeries to SQLite tables.";
            this.lblExport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnExport.Location = new System.Drawing.Point(8, 12);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(94, 23);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export Series";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // splitWDMTab
            // 
            this.layoutMain.SetColumnSpan(this.splitWDMTab, 3);
            this.splitWDMTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitWDMTab.Location = new System.Drawing.Point(8, 102);
            this.splitWDMTab.Name = "splitWDMTab";
            // 
            // splitWDMTab.Panel1
            // 
            this.splitWDMTab.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitWDMTab.Panel2
            // 
            this.splitWDMTab.Panel2.Controls.Add(this.TableLayoutPanel2);
            this.splitWDMTab.Panel2Collapsed = true;
            this.splitWDMTab.Size = new System.Drawing.Size(645, 308);
            this.splitWDMTab.SplitterDistance = 440;
            this.splitWDMTab.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
            this.tableLayoutPanel1.Controls.Add(this.grpTable, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectAll, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnClose, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnClearSelection, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(645, 308);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // grpTable
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpTable, 5);
            this.grpTable.Controls.Add(this.dgvWDM);
            this.grpTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTable.Location = new System.Drawing.Point(3, 3);
            this.grpTable.Name = "grpTable";
            this.grpTable.Size = new System.Drawing.Size(639, 272);
            this.grpTable.TabIndex = 0;
            this.grpTable.TabStop = false;
            // 
            // dgvWDM
            // 
            this.dgvWDM.AllowUserToAddRows = false;
            this.dgvWDM.AllowUserToDeleteRows = false;
            this.dgvWDM.AllowUserToResizeRows = false;
            this.dgvWDM.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvWDM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWDM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWDM.Location = new System.Drawing.Point(3, 16);
            this.dgvWDM.Name = "dgvWDM";
            this.dgvWDM.ReadOnly = true;
            this.dgvWDM.RowHeadersWidth = 10;
            this.dgvWDM.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWDM.Size = new System.Drawing.Size(633, 253);
            this.dgvWDM.TabIndex = 0;
            this.dgvWDM.Click += new System.EventHandler(this.dgvWDM_Click);
            this.dgvWDM.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvWDM_MouseClick);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(3, 281);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(563, 281);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnClearSelection
            // 
            this.btnClearSelection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnClearSelection.Location = new System.Drawing.Point(89, 281);
            this.btnClearSelection.Name = "btnClearSelection";
            this.btnClearSelection.Size = new System.Drawing.Size(93, 23);
            this.btnClearSelection.TabIndex = 3;
            this.btnClearSelection.Text = "Clear Selection";
            this.btnClearSelection.UseVisualStyleBackColor = true;
            this.btnClearSelection.Click += new System.EventHandler(this.btnClearSelection_Click);
            // 
            // TableLayoutPanel2
            // 
            this.TableLayoutPanel2.ColumnCount = 4;
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.82022F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.16779F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.87248F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.TableLayoutPanel2.Controls.Add(this.grpSelSeries, 0, 0);
            this.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel2.Name = "TableLayoutPanel2";
            this.TableLayoutPanel2.RowCount = 2;
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableLayoutPanel2.Size = new System.Drawing.Size(96, 100);
            this.TableLayoutPanel2.TabIndex = 1;
            // 
            // grpSelSeries
            // 
            this.TableLayoutPanel2.SetColumnSpan(this.grpSelSeries, 4);
            this.grpSelSeries.Controls.Add(this.dgvSelSeries);
            this.grpSelSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelSeries.Location = new System.Drawing.Point(3, 3);
            this.grpSelSeries.Name = "grpSelSeries";
            this.grpSelSeries.Size = new System.Drawing.Size(90, 64);
            this.grpSelSeries.TabIndex = 0;
            this.grpSelSeries.TabStop = false;
            // 
            // dgvSelSeries
            // 
            this.dgvSelSeries.AllowUserToAddRows = false;
            this.dgvSelSeries.AllowUserToDeleteRows = false;
            this.dgvSelSeries.AllowUserToResizeRows = false;
            this.dgvSelSeries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSelSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSelSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSelSeries.Location = new System.Drawing.Point(3, 16);
            this.dgvSelSeries.Name = "dgvSelSeries";
            this.dgvSelSeries.ReadOnly = true;
            this.dgvSelSeries.RowHeadersWidth = 5;
            this.dgvSelSeries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSelSeries.Size = new System.Drawing.Size(84, 45);
            this.dgvSelSeries.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.layoutMain.SetColumnSpan(this.statusStrip, 3);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statuslbl});
            this.statusStrip.Location = new System.Drawing.Point(5, 416);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(651, 22);
            this.statusStrip.TabIndex = 6;
            // 
            // statuslbl
            // 
            this.statuslbl.Name = "statuslbl";
            this.statuslbl.Size = new System.Drawing.Size(51, 17);
            this.statuslbl.Text = "Ready ...";
            // 
            // lblOut
            // 
            this.lblOut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOut.AutoSize = true;
            this.lblOut.Location = new System.Drawing.Point(8, 47);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(74, 13);
            this.lblOut.TabIndex = 7;
            this.lblOut.Text = "Output Folder:";
            // 
            // dblabel
            // 
            this.dblabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dblabel.AutoSize = true;
            this.dblabel.Location = new System.Drawing.Point(8, 77);
            this.dblabel.Name = "dblabel";
            this.dblabel.Size = new System.Drawing.Size(60, 13);
            this.dblabel.TabIndex = 8;
            this.dblabel.Text = "SQLite DB:";
            // 
            // lblSDB
            // 
            this.lblSDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSDB.AutoSize = true;
            this.lblSDB.Location = new System.Drawing.Point(108, 77);
            this.lblSDB.Name = "lblSDB";
            this.lblSDB.Size = new System.Drawing.Size(507, 13);
            this.lblSDB.TabIndex = 10;
            this.lblSDB.Click += new System.EventHandler(this.lblSDB_Click);
            // 
            // btnOut
            // 
            this.btnOut.Image = global::NCEIData.Properties.Resources.openfolderhs;
            this.btnOut.Location = new System.Drawing.Point(621, 42);
            this.btnOut.Name = "btnOut";
            this.btnOut.Size = new System.Drawing.Size(32, 23);
            this.btnOut.TabIndex = 11;
            this.btnOut.UseVisualStyleBackColor = true;
            this.btnOut.Click += new System.EventHandler(this.btnOut_Click);
            // 
            // txtOut
            // 
            this.txtOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOut.AutoSize = true;
            this.txtOut.BackColor = System.Drawing.SystemColors.Window;
            this.txtOut.Location = new System.Drawing.Point(108, 47);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(507, 13);
            this.txtOut.TabIndex = 13;
            // 
            // frmExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 438);
            this.Controls.Add(this.layoutMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export WDM to SQLite";
            this.Load += new System.EventHandler(this.frmExport_Load);
            this.layoutMain.ResumeLayout(false);
            this.layoutMain.PerformLayout();
            this.splitWDMTab.Panel1.ResumeLayout(false);
            this.splitWDMTab.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitWDMTab)).EndInit();
            this.splitWDMTab.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.grpTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWDM)).EndInit();
            this.TableLayoutPanel2.ResumeLayout(false);
            this.grpSelSeries.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelSeries)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel layoutMain;
        internal System.Windows.Forms.Label lblExport;
        internal System.Windows.Forms.SplitContainer splitWDMTab;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel2;
        internal System.Windows.Forms.GroupBox grpSelSeries;
        internal System.Windows.Forms.DataGridView dgvSelSeries;
        internal System.Windows.Forms.Button btnExport;
        internal System.Windows.Forms.StatusStrip statusStrip;
        internal System.Windows.Forms.ToolStripStatusLabel statuslbl;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.Label dblabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpTable;
        private System.Windows.Forms.DataGridView dgvWDM;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnClearSelection;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblSDB;
        private System.Windows.Forms.Button btnOut;
        private System.Windows.Forms.Label txtOut;
    }
}