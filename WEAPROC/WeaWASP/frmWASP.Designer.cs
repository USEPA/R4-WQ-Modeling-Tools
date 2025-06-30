namespace WeaWASP
{
    partial class frmWASP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWASP));
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpPeriod = new System.Windows.Forms.GroupBox();
            this.simLayout = new System.Windows.Forms.TableLayoutPanel();
            this.btnAssign = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtBegDate = new System.Windows.Forms.DateTimePicker();
            this.dtEndDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.ToolStrip();
            this.statuslbl = new System.Windows.Forms.ToolStripLabel();
            this.grpLocation = new System.Windows.Forms.GroupBox();
            this.dgvAir = new System.Windows.Forms.DataGridView();
            this.grpCommon = new System.Windows.Forms.GroupBox();
            this.obsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblBegDate = new System.Windows.Forms.Label();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.mainLayout.SuspendLayout();
            this.grpPeriod.SuspendLayout();
            this.simLayout.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.grpLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAir)).BeginInit();
            this.grpCommon.SuspendLayout();
            this.obsLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 6;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 237F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayout.Controls.Add(this.grpPeriod, 0, 3);
            this.mainLayout.Controls.Add(this.statusStrip, 0, 9);
            this.mainLayout.Controls.Add(this.grpLocation, 0, 5);
            this.mainLayout.Controls.Add(this.grpCommon, 3, 3);
            this.mainLayout.Controls.Add(this.label1, 0, 0);
            this.mainLayout.Controls.Add(this.btnClose, 4, 9);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 10;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayout.Size = new System.Drawing.Size(844, 537);
            this.mainLayout.TabIndex = 7;
            // 
            // grpPeriod
            // 
            this.grpPeriod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainLayout.SetColumnSpan(this.grpPeriod, 3);
            this.grpPeriod.Controls.Add(this.simLayout);
            this.grpPeriod.Location = new System.Drawing.Point(3, 82);
            this.grpPeriod.Name = "grpPeriod";
            this.grpPeriod.Size = new System.Drawing.Size(514, 56);
            this.grpPeriod.TabIndex = 5;
            this.grpPeriod.TabStop = false;
            this.grpPeriod.Text = "Simulation Period";
            // 
            // simLayout
            // 
            this.simLayout.ColumnCount = 8;
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 172F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.simLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 443F));
            this.simLayout.Controls.Add(this.btnAssign, 0, 0);
            this.simLayout.Controls.Add(this.label2, 2, 0);
            this.simLayout.Controls.Add(this.dtBegDate, 3, 0);
            this.simLayout.Controls.Add(this.dtEndDate, 5, 0);
            this.simLayout.Controls.Add(this.label3, 4, 0);
            this.simLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simLayout.Location = new System.Drawing.Point(3, 16);
            this.simLayout.Name = "simLayout";
            this.simLayout.RowCount = 1;
            this.simLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.simLayout.Size = new System.Drawing.Size(508, 37);
            this.simLayout.TabIndex = 0;
            // 
            // btnAssign
            // 
            this.btnAssign.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAssign.Enabled = false;
            this.btnAssign.Location = new System.Drawing.Point(3, 8);
            this.btnAssign.Name = "btnAssign";
            this.btnAssign.Size = new System.Drawing.Size(166, 23);
            this.btnAssign.TabIndex = 4;
            this.btnAssign.Text = "Assign Nearest Station";
            this.btnAssign.UseVisualStyleBackColor = true;
            this.btnAssign.Click += new System.EventHandler(this.btnAssign_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Begin Date";
            // 
            // dtBegDate
            // 
            this.dtBegDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtBegDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtBegDate.Location = new System.Drawing.Point(254, 9);
            this.dtBegDate.Name = "dtBegDate";
            this.dtBegDate.Size = new System.Drawing.Size(80, 20);
            this.dtBegDate.TabIndex = 2;
            this.dtBegDate.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dtBegDate.ValueChanged += new System.EventHandler(this.dtBegDate_ValueChanged);
            // 
            // dtEndDate
            // 
            this.dtEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtEndDate.Location = new System.Drawing.Point(406, 9);
            this.dtEndDate.Name = "dtEndDate";
            this.dtEndDate.Size = new System.Drawing.Size(80, 20);
            this.dtEndDate.TabIndex = 3;
            this.dtEndDate.ValueChanged += new System.EventHandler(this.dtEndDate_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(342, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "End Date";
            // 
            // statusStrip
            // 
            this.statusStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainLayout.SetColumnSpan(this.statusStrip, 4);
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statuslbl});
            this.statusStrip.Location = new System.Drawing.Point(0, 506);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(757, 25);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "toolStrip1";
            // 
            // statuslbl
            // 
            this.statuslbl.Name = "statuslbl";
            this.statuslbl.Size = new System.Drawing.Size(51, 22);
            this.statuslbl.Text = "Ready ...";
            // 
            // grpLocation
            // 
            this.mainLayout.SetColumnSpan(this.grpLocation, 6);
            this.grpLocation.Controls.Add(this.dgvAir);
            this.grpLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLocation.Location = new System.Drawing.Point(3, 154);
            this.grpLocation.Name = "grpLocation";
            this.mainLayout.SetRowSpan(this.grpLocation, 3);
            this.grpLocation.Size = new System.Drawing.Size(838, 336);
            this.grpLocation.TabIndex = 7;
            this.grpLocation.TabStop = false;
            // 
            // dgvAir
            // 
            this.dgvAir.AllowUserToAddRows = false;
            this.dgvAir.AllowUserToDeleteRows = false;
            this.dgvAir.AllowUserToResizeColumns = false;
            this.dgvAir.AllowUserToResizeRows = false;
            this.dgvAir.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAir.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAir.Location = new System.Drawing.Point(3, 16);
            this.dgvAir.Name = "dgvAir";
            this.dgvAir.RowHeadersWidth = 5;
            this.dgvAir.Size = new System.Drawing.Size(832, 317);
            this.dgvAir.TabIndex = 0;
            // 
            // grpCommon
            // 
            this.grpCommon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainLayout.SetColumnSpan(this.grpCommon, 3);
            this.grpCommon.Controls.Add(this.obsLayout);
            this.grpCommon.Location = new System.Drawing.Point(523, 82);
            this.grpCommon.Name = "grpCommon";
            this.grpCommon.Size = new System.Drawing.Size(318, 56);
            this.grpCommon.TabIndex = 11;
            this.grpCommon.TabStop = false;
            this.grpCommon.Text = "Common Period";
            // 
            // obsLayout
            // 
            this.obsLayout.ColumnCount = 6;
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 11F));
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 341F));
            this.obsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.obsLayout.Controls.Add(this.label5, 0, 0);
            this.obsLayout.Controls.Add(this.label6, 3, 0);
            this.obsLayout.Controls.Add(this.lblBegDate, 1, 0);
            this.obsLayout.Controls.Add(this.lblEndDate, 4, 0);
            this.obsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.obsLayout.Location = new System.Drawing.Point(3, 16);
            this.obsLayout.Name = "obsLayout";
            this.obsLayout.RowCount = 1;
            this.obsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.obsLayout.Size = new System.Drawing.Size(312, 37);
            this.obsLayout.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 26);
            this.label5.TabIndex = 1;
            this.label5.Text = "Begin Date:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(166, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 26);
            this.label6.TabIndex = 2;
            this.label6.Text = "End Date:";
            // 
            // lblBegDate
            // 
            this.lblBegDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBegDate.AutoSize = true;
            this.lblBegDate.Location = new System.Drawing.Point(48, 12);
            this.lblBegDate.Name = "lblBegDate";
            this.lblBegDate.Size = new System.Drawing.Size(0, 13);
            this.lblBegDate.TabIndex = 3;
            // 
            // lblEndDate
            // 
            this.lblEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(211, 12);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(0, 13);
            this.lblEndDate.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.mainLayout.SetColumnSpan(this.label1, 6);
            this.label1.Location = new System.Drawing.Point(3, 19);
            this.label1.Name = "label1";
            this.mainLayout.SetRowSpan(this.label1, 3);
            this.label1.Size = new System.Drawing.Size(838, 39);
            this.label1.TabIndex = 13;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location = new System.Drawing.Point(762, 505);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 27);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmWASP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 537);
            this.Controls.Add(this.mainLayout);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmWASP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WASP Weather";
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.grpPeriod.ResumeLayout(false);
            this.simLayout.ResumeLayout(false);
            this.simLayout.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.grpLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAir)).EndInit();
            this.grpCommon.ResumeLayout(false);
            this.obsLayout.ResumeLayout(false);
            this.obsLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.GroupBox grpPeriod;
        private System.Windows.Forms.TableLayoutPanel simLayout;
        private System.Windows.Forms.Button btnAssign;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.DateTimePicker dtBegDate;
        public System.Windows.Forms.DateTimePicker dtEndDate;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ToolStrip statusStrip;
        public System.Windows.Forms.ToolStripLabel statuslbl;
        private System.Windows.Forms.GroupBox grpLocation;
        private System.Windows.Forms.DataGridView dgvAir;
        private System.Windows.Forms.GroupBox grpCommon;
        private System.Windows.Forms.TableLayoutPanel obsLayout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblBegDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
    }
}