namespace WeaGen
{
    partial class frmWeaGen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWeaGen));
            this.layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.dateEnd = new System.Windows.Forms.DateTimePicker();
            this.dateStart = new System.Windows.Forms.DateTimePicker();
            this.splitWDMTab = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.grpTable = new System.Windows.Forms.GroupBox();
            this.dgvSDB = new System.Windows.Forms.DataGridView();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClearSelection = new System.Windows.Forms.Button();
            this.chkSave = new System.Windows.Forms.CheckBox();
            this.TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statuslbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSDB = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOut = new System.Windows.Forms.Button();
            this.lblOut = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.layoutMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitWDMTab)).BeginInit();
            this.splitWDMTab.Panel1.SuspendLayout();
            this.splitWDMTab.Panel2.SuspendLayout();
            this.splitWDMTab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSDB)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutMain
            // 
            this.layoutMain.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.layoutMain.ColumnCount = 10;
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 89F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 143F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.layoutMain.Controls.Add(this.btnGenerate, 1, 1);
            this.layoutMain.Controls.Add(this.dateEnd, 5, 1);
            this.layoutMain.Controls.Add(this.dateStart, 3, 1);
            this.layoutMain.Controls.Add(this.splitWDMTab, 1, 4);
            this.layoutMain.Controls.Add(this.statusStrip, 1, 5);
            this.layoutMain.Controls.Add(this.label3, 1, 2);
            this.layoutMain.Controls.Add(this.lblSDB, 2, 2);
            this.layoutMain.Controls.Add(this.label1, 1, 3);
            this.layoutMain.Controls.Add(this.btnOut, 8, 3);
            this.layoutMain.Controls.Add(this.lblOut, 2, 3);
            this.layoutMain.Controls.Add(this.label2, 2, 1);
            this.layoutMain.Controls.Add(this.label4, 4, 1);
            this.layoutMain.Controls.Add(this.label5, 1, 0);
            this.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMain.Location = new System.Drawing.Point(0, 0);
            this.layoutMain.Name = "layoutMain";
            this.layoutMain.RowCount = 6;
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.layoutMain.Size = new System.Drawing.Size(775, 513);
            this.layoutMain.TabIndex = 3;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnGenerate.Location = new System.Drawing.Point(8, 42);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(111, 32);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate Series";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // dateEnd
            // 
            this.dateEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateEnd.Location = new System.Drawing.Point(322, 48);
            this.dateEnd.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateEnd.Name = "dateEnd";
            this.dateEnd.Size = new System.Drawing.Size(79, 20);
            this.dateEnd.TabIndex = 3;
            this.dateEnd.ValueChanged += new System.EventHandler(this.dateEnd_ValueChanged);
            // 
            // dateStart
            // 
            this.dateStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateStart.Location = new System.Drawing.Point(200, 48);
            this.dateStart.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateStart.Name = "dateStart";
            this.dateStart.Size = new System.Drawing.Size(82, 20);
            this.dateStart.TabIndex = 1;
            this.dateStart.ValueChanged += new System.EventHandler(this.dateStart_ValueChanged);
            // 
            // splitWDMTab
            // 
            this.layoutMain.SetColumnSpan(this.splitWDMTab, 8);
            this.splitWDMTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitWDMTab.Location = new System.Drawing.Point(8, 152);
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
            this.splitWDMTab.Size = new System.Drawing.Size(755, 331);
            this.splitWDMTab.SplitterDistance = 448;
            this.splitWDMTab.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
            this.tableLayoutPanel1.Controls.Add(this.grpTable, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectAll, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnClearSelection, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkSave, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(755, 331);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // grpTable
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpTable, 5);
            this.grpTable.Controls.Add(this.dgvSDB);
            this.grpTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTable.Location = new System.Drawing.Point(3, 3);
            this.grpTable.Name = "grpTable";
            this.grpTable.Size = new System.Drawing.Size(749, 234);
            this.grpTable.TabIndex = 0;
            this.grpTable.TabStop = false;
            // 
            // dgvSDB
            // 
            this.dgvSDB.AllowUserToAddRows = false;
            this.dgvSDB.AllowUserToDeleteRows = false;
            this.dgvSDB.AllowUserToResizeRows = false;
            this.dgvSDB.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSDB.Location = new System.Drawing.Point(3, 16);
            this.dgvSDB.Name = "dgvSDB";
            this.dgvSDB.ReadOnly = true;
            this.dgvSDB.RowHeadersWidth = 15;
            this.dgvSDB.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSDB.Size = new System.Drawing.Size(743, 215);
            this.dgvSDB.TabIndex = 0;
            this.dgvSDB.Click += new System.EventHandler(this.dgvSDB_Click);
            this.dgvSDB.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvSDB_MouseClick);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(3, 243);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnClearSelection
            // 
            this.btnClearSelection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnClearSelection.Location = new System.Drawing.Point(89, 243);
            this.btnClearSelection.Name = "btnClearSelection";
            this.btnClearSelection.Size = new System.Drawing.Size(93, 23);
            this.btnClearSelection.TabIndex = 3;
            this.btnClearSelection.Text = "Clear Selection";
            this.btnClearSelection.UseVisualStyleBackColor = true;
            this.btnClearSelection.Click += new System.EventHandler(this.btnClearSelection_Click);
            // 
            // chkSave
            // 
            this.chkSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSave.AutoSize = true;
            this.chkSave.Checked = true;
            this.chkSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkSave, 3);
            this.chkSave.Location = new System.Drawing.Point(3, 296);
            this.chkSave.Name = "chkSave";
            this.chkSave.Size = new System.Drawing.Size(273, 17);
            this.chkSave.TabIndex = 4;
            this.chkSave.Text = "Export Generated Stochastic TimeSeries to csv files.";
            this.chkSave.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel2
            // 
            this.TableLayoutPanel2.ColumnCount = 2;
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel2.Name = "TableLayoutPanel2";
            this.TableLayoutPanel2.RowCount = 8;
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TableLayoutPanel2.Size = new System.Drawing.Size(96, 100);
            this.TableLayoutPanel2.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.layoutMain.SetColumnSpan(this.statusStrip, 8);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statuslbl});
            this.statusStrip.Location = new System.Drawing.Point(5, 491);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(761, 22);
            this.statusStrip.TabIndex = 6;
            // 
            // statuslbl
            // 
            this.statuslbl.Name = "statuslbl";
            this.statuslbl.Size = new System.Drawing.Size(51, 17);
            this.statuslbl.Text = "Ready ...";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Parameter Database:";
            // 
            // lblSDB
            // 
            this.lblSDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSDB.AutoSize = true;
            this.layoutMain.SetColumnSpan(this.lblSDB, 7);
            this.lblSDB.Location = new System.Drawing.Point(133, 92);
            this.lblSDB.Name = "lblSDB";
            this.lblSDB.Size = new System.Drawing.Size(630, 13);
            this.lblSDB.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Output Database:";
            // 
            // btnOut
            // 
            this.btnOut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnOut.Image = ((System.Drawing.Image)(resources.GetObject("btnOut.Image")));
            this.btnOut.Location = new System.Drawing.Point(730, 121);
            this.btnOut.Name = "btnOut";
            this.btnOut.Size = new System.Drawing.Size(30, 23);
            this.btnOut.TabIndex = 12;
            this.btnOut.UseVisualStyleBackColor = true;
            this.btnOut.Click += new System.EventHandler(this.btnOut_Click);
            // 
            // lblOut
            // 
            this.lblOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOut.AutoSize = true;
            this.lblOut.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.layoutMain.SetColumnSpan(this.lblOut, 6);
            this.lblOut.Location = new System.Drawing.Point(133, 126);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(591, 13);
            this.lblOut.TabIndex = 9;
            this.lblOut.TextChanged += new System.EventHandler(this.lblOut_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(133, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "From:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(289, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "To:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.layoutMain.SetColumnSpan(this.label5, 8);
            this.label5.Location = new System.Drawing.Point(8, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(755, 34);
            this.label5.TabIndex = 15;
            this.label5.Text = "Please select from list of station-parameter to generate stochastic series at the" +
    " selected site. Need to specify output SQLite database.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmWeaGen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 513);
            this.Controls.Add(this.layoutMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmWeaGen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stochastic Weather Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmWeaGen_FormClosing);
            this.Load += new System.EventHandler(this.frmWeaGen_Load);
            this.layoutMain.ResumeLayout(false);
            this.layoutMain.PerformLayout();
            this.splitWDMTab.Panel1.ResumeLayout(false);
            this.splitWDMTab.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitWDMTab)).EndInit();
            this.splitWDMTab.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.grpTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSDB)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel layoutMain;
        internal System.Windows.Forms.Button btnGenerate;
        internal System.Windows.Forms.SplitContainer splitWDMTab;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpTable;
        private System.Windows.Forms.DataGridView dgvSDB;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnClearSelection;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel2;
        internal System.Windows.Forms.StatusStrip statusStrip;
        internal System.Windows.Forms.ToolStripStatusLabel statuslbl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.Label lblSDB;
        private System.Windows.Forms.DateTimePicker dateStart;
        private System.Windows.Forms.DateTimePicker dateEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOut;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkSave;
    }
}