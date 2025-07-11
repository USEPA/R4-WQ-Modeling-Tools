<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmWeaComp
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmWeaComp))
        Me.mnuStrip = New System.Windows.Forms.MenuStrip()
        Me.mnuDB = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalculate = New System.Windows.Forms.ToolStripMenuItem()
        Me.CloudCoverToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcSolar = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcHPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcJPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcPPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcPriestly = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcPMPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.WindTravelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisaggregate = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagSolar = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagDew = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagRain = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagTemp = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisagWind = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuImportNCEI = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAddAttributes = New System.Windows.Forms.ToolStripMenuItem()
        Me.layoutMain = New System.Windows.Forms.TableLayoutPanel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.splitWDMTab = New System.Windows.Forms.SplitContainer()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.grpTable = New System.Windows.Forms.GroupBox()
        Me.dgvWDM = New System.Windows.Forms.DataGridView()
        Me.btnSelAllRows = New System.Windows.Forms.Button()
        Me.btnSelRows = New System.Windows.Forms.Button()
        Me.btnClearSelRows = New System.Windows.Forms.Button()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.grpSelSeries = New System.Windows.Forms.GroupBox()
        Me.dgvSelSeries = New System.Windows.Forms.DataGridView()
        Me.btnCalc = New System.Windows.Forms.Button()
        Me.btnClearSelSta = New System.Windows.Forms.Button()
        Me.statusStrip = New System.Windows.Forms.StatusStrip()
        Me.statuslbl = New System.Windows.Forms.ToolStripStatusLabel()
        Me.mnuStrip.SuspendLayout()
        Me.layoutMain.SuspendLayout()
        CType(Me.splitWDMTab, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitWDMTab.Panel1.SuspendLayout()
        Me.splitWDMTab.Panel2.SuspendLayout()
        Me.splitWDMTab.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.grpTable.SuspendLayout()
        CType(Me.dgvWDM, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.grpSelSeries.SuspendLayout()
        CType(Me.dgvSelSeries, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.statusStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'mnuStrip
        '
        Me.mnuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuCalculate, Me.mnuDB, Me.mnuDisaggregate, Me.mnuImportNCEI, Me.mnuAddAttributes})
        Me.mnuStrip.Location = New System.Drawing.Point(0, 0)
        Me.mnuStrip.Name = "mnuStrip"
        Me.mnuStrip.Size = New System.Drawing.Size(862, 24)
        Me.mnuStrip.TabIndex = 0
        Me.mnuStrip.Text = "MenuStrip1"
        '
        'mnuDB
        '
        Me.mnuDB.Name = "mnuDB"
        Me.mnuDB.Size = New System.Drawing.Size(83, 20)
        Me.mnuDB.Text = "Select WDM"
        Me.mnuDB.Visible = False
        '
        'mnuCalculate
        '
        Me.mnuCalculate.AutoToolTip = True
        Me.mnuCalculate.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CloudCoverToolStripMenuItem, Me.mnuCalcSolar, Me.mnuCalcHPET, Me.mnuCalcJPET, Me.mnuCalcPPET, Me.mnuCalcPriestly, Me.mnuCalcPMPET, Me.WindTravelToolStripMenuItem})
        Me.mnuCalculate.Name = "mnuCalculate"
        Me.mnuCalculate.Size = New System.Drawing.Size(102, 20)
        Me.mnuCalculate.Text = "Compute Series"
        Me.mnuCalculate.ToolTipText = "Compute PET and Solar"
        '
        'CloudCoverToolStripMenuItem
        '
        Me.CloudCoverToolStripMenuItem.Name = "CloudCoverToolStripMenuItem"
        Me.CloudCoverToolStripMenuItem.Size = New System.Drawing.Size(207, 22)
        Me.CloudCoverToolStripMenuItem.Text = "Cloud Cover "
        '
        'mnuCalcSolar
        '
        Me.mnuCalcSolar.Name = "mnuCalcSolar"
        Me.mnuCalcSolar.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcSolar.Text = "Solar Radiation"
        '
        'mnuCalcHPET
        '
        Me.mnuCalcHPET.Name = "mnuCalcHPET"
        Me.mnuCalcHPET.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcHPET.Text = "Hamon PET"
        '
        'mnuCalcJPET
        '
        Me.mnuCalcJPET.Name = "mnuCalcJPET"
        Me.mnuCalcJPET.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcJPET.Text = "Jensen PET"
        Me.mnuCalcJPET.Visible = False
        '
        'mnuCalcPPET
        '
        Me.mnuCalcPPET.Name = "mnuCalcPPET"
        Me.mnuCalcPPET.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcPPET.Text = "Penman Pan Evaporation"
        '
        'mnuCalcPriestly
        '
        Me.mnuCalcPriestly.Name = "mnuCalcPriestly"
        Me.mnuCalcPriestly.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcPriestly.Text = "Priestley-Taylor PET"
        Me.mnuCalcPriestly.Visible = False
        '
        'mnuCalcPMPET
        '
        Me.mnuCalcPMPET.Name = "mnuCalcPMPET"
        Me.mnuCalcPMPET.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcPMPET.Text = "Penman-Monteith PET"
        '
        'WindTravelToolStripMenuItem
        '
        Me.WindTravelToolStripMenuItem.Name = "WindTravelToolStripMenuItem"
        Me.WindTravelToolStripMenuItem.Size = New System.Drawing.Size(207, 22)
        Me.WindTravelToolStripMenuItem.Text = "Wind Travel"
        Me.WindTravelToolStripMenuItem.Visible = False
        '
        'mnuDisaggregate
        '
        Me.mnuDisaggregate.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuDisagSolar, Me.mnuDisagDew, Me.mnuDisagRain, Me.mnuDisagPET, Me.mnuDisagTemp, Me.mnuDisagWind})
        Me.mnuDisaggregate.Name = "mnuDisaggregate"
        Me.mnuDisaggregate.Size = New System.Drawing.Size(121, 20)
        Me.mnuDisaggregate.Text = "Disaggregate Series"
        Me.mnuDisaggregate.Visible = False
        '
        'mnuDisagSolar
        '
        Me.mnuDisagSolar.Name = "mnuDisagSolar"
        Me.mnuDisagSolar.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagSolar.Text = "Solar Radiation"
        '
        'mnuDisagDew
        '
        Me.mnuDisagDew.Name = "mnuDisagDew"
        Me.mnuDisagDew.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagDew.Text = "Dew Point"
        '
        'mnuDisagRain
        '
        Me.mnuDisagRain.Name = "mnuDisagRain"
        Me.mnuDisagRain.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagRain.Text = "Precipitation"
        '
        'mnuDisagPET
        '
        Me.mnuDisagPET.Name = "mnuDisagPET"
        Me.mnuDisagPET.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagPET.Text = "Evapotranspiration"
        '
        'mnuDisagTemp
        '
        Me.mnuDisagTemp.Name = "mnuDisagTemp"
        Me.mnuDisagTemp.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagTemp.Text = "Temperature"
        '
        'mnuDisagWind
        '
        Me.mnuDisagWind.Name = "mnuDisagWind"
        Me.mnuDisagWind.Size = New System.Drawing.Size(173, 22)
        Me.mnuDisagWind.Text = "Wind"
        '
        'mnuImportNCEI
        '
        Me.mnuImportNCEI.Enabled = False
        Me.mnuImportNCEI.Name = "mnuImportNCEI"
        Me.mnuImportNCEI.Size = New System.Drawing.Size(123, 20)
        Me.mnuImportNCEI.Text = "Import Hourly NCEI"
        Me.mnuImportNCEI.Visible = False
        '
        'mnuAddAttributes
        '
        Me.mnuAddAttributes.Name = "mnuAddAttributes"
        Me.mnuAddAttributes.Size = New System.Drawing.Size(96, 20)
        Me.mnuAddAttributes.Text = "Add Attributes"
        Me.mnuAddAttributes.Visible = False
        '
        'layoutMain
        '
        Me.layoutMain.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.layoutMain.ColumnCount = 5
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5.0!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        Me.layoutMain.Controls.Add(Me.Label2, 1, 1)
        Me.layoutMain.Controls.Add(Me.splitWDMTab, 1, 4)
        Me.layoutMain.Controls.Add(Me.statusStrip, 1, 5)
        Me.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.layoutMain.Location = New System.Drawing.Point(0, 24)
        Me.layoutMain.Name = "layoutMain"
        Me.layoutMain.RowCount = 6
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27.0!))
        Me.layoutMain.Size = New System.Drawing.Size(862, 567)
        Me.layoutMain.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.layoutMain.SetColumnSpan(Me.Label2, 3)
        Me.Label2.Location = New System.Drawing.Point(8, 12)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(845, 26)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Routine calculates PEVT using Hamon and Penman method from input timeseries in a " &
    "WDM." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Also calculates solar radiation from cloud cover. "
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'splitWDMTab
        '
        Me.layoutMain.SetColumnSpan(Me.splitWDMTab, 3)
        Me.splitWDMTab.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitWDMTab.Location = New System.Drawing.Point(8, 73)
        Me.splitWDMTab.Name = "splitWDMTab"
        '
        'splitWDMTab.Panel1
        '
        Me.splitWDMTab.Panel1.Controls.Add(Me.TableLayoutPanel1)
        '
        'splitWDMTab.Panel2
        '
        Me.splitWDMTab.Panel2.Controls.Add(Me.TableLayoutPanel2)
        Me.splitWDMTab.Size = New System.Drawing.Size(845, 464)
        Me.splitWDMTab.SplitterDistance = 500
        Me.splitWDMTab.TabIndex = 5
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 4
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.grpTable, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.btnSelAllRows, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.btnSelRows, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.btnClearSelRows, 3, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(500, 464)
        Me.TableLayoutPanel1.TabIndex = 5
        '
        'grpTable
        '
        Me.grpTable.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.SetColumnSpan(Me.grpTable, 4)
        Me.grpTable.Controls.Add(Me.dgvWDM)
        Me.grpTable.Location = New System.Drawing.Point(3, 3)
        Me.grpTable.Name = "grpTable"
        Me.grpTable.Size = New System.Drawing.Size(494, 427)
        Me.grpTable.TabIndex = 4
        Me.grpTable.TabStop = False
        '
        'dgvWDM
        '
        Me.dgvWDM.AllowUserToAddRows = False
        Me.dgvWDM.AllowUserToDeleteRows = False
        Me.dgvWDM.AllowUserToResizeRows = False
        Me.dgvWDM.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvWDM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvWDM.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvWDM.Enabled = False
        Me.dgvWDM.Location = New System.Drawing.Point(3, 16)
        Me.dgvWDM.Name = "dgvWDM"
        Me.dgvWDM.ReadOnly = True
        Me.dgvWDM.RowHeadersWidth = 5
        Me.dgvWDM.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvWDM.Size = New System.Drawing.Size(488, 408)
        Me.dgvWDM.TabIndex = 0
        '
        'btnSelAllRows
        '
        Me.btnSelAllRows.Enabled = False
        Me.btnSelAllRows.Location = New System.Drawing.Point(3, 436)
        Me.btnSelAllRows.Name = "btnSelAllRows"
        Me.btnSelAllRows.Size = New System.Drawing.Size(73, 24)
        Me.btnSelAllRows.TabIndex = 5
        Me.btnSelAllRows.Text = "Select All"
        Me.btnSelAllRows.UseVisualStyleBackColor = True
        '
        'btnSelRows
        '
        Me.btnSelRows.Enabled = False
        Me.btnSelRows.Location = New System.Drawing.Point(82, 436)
        Me.btnSelRows.Name = "btnSelRows"
        Me.btnSelRows.Size = New System.Drawing.Size(69, 23)
        Me.btnSelRows.TabIndex = 6
        Me.btnSelRows.Text = "Select"
        Me.btnSelRows.UseVisualStyleBackColor = True
        '
        'btnClearSelRows
        '
        Me.btnClearSelRows.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnClearSelRows.Enabled = False
        Me.btnClearSelRows.Location = New System.Drawing.Point(386, 437)
        Me.btnClearSelRows.Name = "btnClearSelRows"
        Me.btnClearSelRows.Size = New System.Drawing.Size(111, 23)
        Me.btnClearSelRows.TabIndex = 8
        Me.btnClearSelRows.Text = "Clear Selection"
        Me.btnClearSelRows.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 4
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 59.42857!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.14286!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.83333!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 189.0!))
        Me.TableLayoutPanel2.Controls.Add(Me.grpSelSeries, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCalc, 3, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.btnClearSelSta, 0, 1)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(341, 464)
        Me.TableLayoutPanel2.TabIndex = 1
        '
        'grpSelSeries
        '
        Me.TableLayoutPanel2.SetColumnSpan(Me.grpSelSeries, 4)
        Me.grpSelSeries.Controls.Add(Me.dgvSelSeries)
        Me.grpSelSeries.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpSelSeries.Location = New System.Drawing.Point(3, 3)
        Me.grpSelSeries.Name = "grpSelSeries"
        Me.grpSelSeries.Size = New System.Drawing.Size(335, 428)
        Me.grpSelSeries.TabIndex = 0
        Me.grpSelSeries.TabStop = False
        '
        'dgvSelSeries
        '
        Me.dgvSelSeries.AllowUserToAddRows = False
        Me.dgvSelSeries.AllowUserToDeleteRows = False
        Me.dgvSelSeries.AllowUserToResizeRows = False
        Me.dgvSelSeries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvSelSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSelSeries.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvSelSeries.Location = New System.Drawing.Point(3, 16)
        Me.dgvSelSeries.Name = "dgvSelSeries"
        Me.dgvSelSeries.ReadOnly = True
        Me.dgvSelSeries.RowHeadersWidth = 5
        Me.dgvSelSeries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvSelSeries.Size = New System.Drawing.Size(329, 409)
        Me.dgvSelSeries.TabIndex = 0
        '
        'btnCalc
        '
        Me.btnCalc.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnCalc.Enabled = False
        Me.btnCalc.Location = New System.Drawing.Point(180, 437)
        Me.btnCalc.Name = "btnCalc"
        Me.btnCalc.Size = New System.Drawing.Size(158, 23)
        Me.btnCalc.TabIndex = 1
        Me.btnCalc.Text = "Calculate "
        Me.btnCalc.UseVisualStyleBackColor = True
        Me.btnCalc.Visible = False
        '
        'btnClearSelSta
        '
        Me.btnClearSelSta.Location = New System.Drawing.Point(3, 437)
        Me.btnClearSelSta.Name = "btnClearSelSta"
        Me.btnClearSelSta.Size = New System.Drawing.Size(83, 23)
        Me.btnClearSelSta.TabIndex = 2
        Me.btnClearSelSta.Text = "Clear Station(s)"
        Me.btnClearSelSta.UseVisualStyleBackColor = True
        '
        'statusStrip
        '
        Me.layoutMain.SetColumnSpan(Me.statusStrip, 3)
        Me.statusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statuslbl})
        Me.statusStrip.Location = New System.Drawing.Point(5, 545)
        Me.statusStrip.Name = "statusStrip"
        Me.statusStrip.Size = New System.Drawing.Size(851, 22)
        Me.statusStrip.TabIndex = 6
        '
        'statuslbl
        '
        Me.statuslbl.Name = "statuslbl"
        Me.statuslbl.Size = New System.Drawing.Size(51, 17)
        Me.statuslbl.Text = "Ready ..."
        '
        'frmWeaComp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(862, 591)
        Me.Controls.Add(Me.layoutMain)
        Me.Controls.Add(Me.mnuStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.mnuStrip
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmWeaComp"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Weather Series Computation"
        Me.mnuStrip.ResumeLayout(False)
        Me.mnuStrip.PerformLayout()
        Me.layoutMain.ResumeLayout(False)
        Me.layoutMain.PerformLayout()
        Me.splitWDMTab.Panel1.ResumeLayout(False)
        Me.splitWDMTab.Panel2.ResumeLayout(False)
        CType(Me.splitWDMTab, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitWDMTab.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.grpTable.ResumeLayout(False)
        CType(Me.dgvWDM, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.grpSelSeries.ResumeLayout(False)
        CType(Me.dgvSelSeries, System.ComponentModel.ISupportInitialize).EndInit()
        Me.statusStrip.ResumeLayout(False)
        Me.statusStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents mnuStrip As MenuStrip
    Friend WithEvents mnuDB As ToolStripMenuItem
    Friend WithEvents mnuCalculate As ToolStripMenuItem
    Friend WithEvents mnuCalcHPET As ToolStripMenuItem
    Friend WithEvents mnuCalcPPET As ToolStripMenuItem
    Friend WithEvents layoutMain As TableLayoutPanel
    Friend WithEvents Label2 As Label
    Friend WithEvents grpTable As GroupBox
    Friend WithEvents dgvWDM As DataGridView
    Friend WithEvents mnuCalcPriestly As ToolStripMenuItem
    Friend WithEvents splitWDMTab As SplitContainer
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents btnSelAllRows As Button
    Friend WithEvents btnSelRows As Button
    Friend WithEvents btnClearSelRows As Button
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents grpSelSeries As GroupBox
    Friend WithEvents dgvSelSeries As DataGridView
    Friend WithEvents btnCalc As Button
    Friend WithEvents btnClearSelSta As Button
    Friend WithEvents mnuImportNCEI As ToolStripMenuItem
    Friend WithEvents mnuCalcSolar As ToolStripMenuItem
    Friend WithEvents mnuAddAttributes As ToolStripMenuItem
    Friend WithEvents statusStrip As StatusStrip
    Friend WithEvents statuslbl As ToolStripStatusLabel
    Friend WithEvents mnuCalcJPET As ToolStripMenuItem
    Friend WithEvents WindTravelToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CloudCoverToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents mnuDisaggregate As ToolStripMenuItem
    Friend WithEvents mnuDisagSolar As ToolStripMenuItem
    Friend WithEvents mnuDisagDew As ToolStripMenuItem
    Friend WithEvents mnuDisagRain As ToolStripMenuItem
    Friend WithEvents mnuDisagPET As ToolStripMenuItem
    Friend WithEvents mnuDisagTemp As ToolStripMenuItem
    Friend WithEvents mnuDisagWind As ToolStripMenuItem
    Friend WithEvents mnuCalcPMPET As ToolStripMenuItem
End Class
