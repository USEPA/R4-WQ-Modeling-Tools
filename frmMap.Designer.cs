namespace FREQANAL
{
    partial class frmMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMap));
            this.appManager = new DotSpatial.Controls.AppManager();
            this.spatialDockManager1 = new DotSpatial.Controls.SpatialDockManager();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabLegend = new System.Windows.Forms.TabPage();
            this.mapLegend = new DotSpatial.Controls.Legend();
            this.map = new DotSpatial.Controls.Map();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spatialDockManager1)).BeginInit();
            this.spatialDockManager1.Panel1.SuspendLayout();
            this.spatialDockManager1.Panel2.SuspendLayout();
            this.spatialDockManager1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabLegend.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // appManager
            // 
            this.appManager.Directories = ((System.Collections.Generic.List<string>)(resources.GetObject("appManager.Directories")));
            this.appManager.DockManager = this.spatialDockManager1;
            this.appManager.HeaderControl = null;
            this.appManager.Legend = this.mapLegend;
            this.appManager.Map = this.map;
            this.appManager.ProgressHandler = null;
            this.appManager.ShowExtensionsDialogMode = DotSpatial.Controls.ShowExtensionsDialogMode.Default;
            // 
            // spatialDockManager1
            // 
            this.spatialDockManager1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spatialDockManager1.Location = new System.Drawing.Point(3, 3);
            this.spatialDockManager1.Name = "spatialDockManager1";
            // 
            // spatialDockManager1.Panel1
            // 
            this.spatialDockManager1.Panel1.Controls.Add(this.tabControl);
            // 
            // spatialDockManager1.Panel2
            // 
            this.spatialDockManager1.Panel2.Controls.Add(this.map);
            this.spatialDockManager1.Size = new System.Drawing.Size(661, 470);
            this.spatialDockManager1.SplitterDistance = 191;
            this.spatialDockManager1.TabControl1 = this.tabControl;
            this.spatialDockManager1.TabControl2 = null;
            this.spatialDockManager1.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabLegend);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(191, 470);
            this.tabControl.TabIndex = 0;
            // 
            // tabLegend
            // 
            this.tabLegend.Controls.Add(this.mapLegend);
            this.tabLegend.Location = new System.Drawing.Point(4, 22);
            this.tabLegend.Name = "tabLegend";
            this.tabLegend.Padding = new System.Windows.Forms.Padding(3);
            this.tabLegend.Size = new System.Drawing.Size(183, 444);
            this.tabLegend.TabIndex = 0;
            this.tabLegend.Text = "Legend";
            this.tabLegend.UseVisualStyleBackColor = true;
            // 
            // mapLegend
            // 
            this.mapLegend.BackColor = System.Drawing.Color.White;
            this.mapLegend.ControlRectangle = new System.Drawing.Rectangle(0, 0, 177, 438);
            this.mapLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapLegend.DocumentRectangle = new System.Drawing.Rectangle(0, 0, 187, 428);
            this.mapLegend.HorizontalScrollEnabled = true;
            this.mapLegend.Indentation = 30;
            this.mapLegend.IsInitialized = false;
            this.mapLegend.Location = new System.Drawing.Point(3, 3);
            this.mapLegend.MinimumSize = new System.Drawing.Size(5, 5);
            this.mapLegend.Name = "mapLegend";
            this.mapLegend.ProgressHandler = null;
            this.mapLegend.ResetOnResize = false;
            this.mapLegend.SelectionFontColor = System.Drawing.Color.Black;
            this.mapLegend.SelectionHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(238)))), ((int)(((byte)(252)))));
            this.mapLegend.Size = new System.Drawing.Size(177, 438);
            this.mapLegend.TabIndex = 0;
            this.mapLegend.Text = "legend1";
            this.mapLegend.VerticalScrollEnabled = true;
            // 
            // map
            // 
            this.map.AllowDrop = true;
            this.map.BackColor = System.Drawing.Color.White;
            this.map.CollectAfterDraw = false;
            this.map.CollisionDetection = false;
            this.map.Dock = System.Windows.Forms.DockStyle.Fill;
            this.map.ExtendBuffer = false;
            this.map.FunctionMode = DotSpatial.Controls.FunctionMode.None;
            this.map.IsBusy = false;
            this.map.IsZoomedToMaxExtent = false;
            this.map.Legend = this.mapLegend;
            this.map.Location = new System.Drawing.Point(0, 0);
            this.map.Name = "map";
            this.map.ProgressHandler = null;
            this.map.ProjectionModeDefine = DotSpatial.Controls.ActionMode.Prompt;
            this.map.ProjectionModeReproject = DotSpatial.Controls.ActionMode.Prompt;
            this.map.RedrawLayersWhileResizing = false;
            this.map.SelectionEnabled = true;
            this.map.Size = new System.Drawing.Size(466, 470);
            this.map.TabIndex = 0;
            this.map.ZoomOutFartherThanMaxExtent = false;
            this.map.SelectionChanged += new System.EventHandler(this.map_SelectionChanged);
            this.map.MouseClick += new System.Windows.Forms.MouseEventHandler(this.map_MouseClick);
            this.map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.map_MouseUp);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.spatialDockManager1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(667, 506);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnSelect);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 479);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(661, 24);
            this.panel1.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Location = new System.Drawing.Point(602, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(59, 24);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSelect.Location = new System.Drawing.Point(0, 0);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(661, 24);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select Gages";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // frmMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 506);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmMap";
            this.Text = "GIS";
            this.spatialDockManager1.Panel1.ResumeLayout(false);
            this.spatialDockManager1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spatialDockManager1)).EndInit();
            this.spatialDockManager1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabLegend.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DotSpatial.Controls.AppManager appManager;
        private DotSpatial.Controls.SpatialDockManager spatialDockManager1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabLegend;
        private DotSpatial.Controls.Legend mapLegend;
        private DotSpatial.Controls.Map map;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSelect;
    }
}