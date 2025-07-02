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
        Me.mnuCalculate = New System.Windows.Forms.ToolStripMenuItem()
        Me.CloudCoverToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcSolar = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcHPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcJPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcPPET = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalcPriestly = New System.Windows.Forms.ToolStripMenuItem()
        Me.WindTravelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDB = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.mnuCalcPMPET = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.mnuStrip.Padding = New System.Windows.Forms.Padding(16, 5, 0, 5)
        Me.mnuStrip.Size = New System.Drawing.Size(2299, 29)
        Me.mnuStrip.TabIndex = 0
        Me.mnuStrip.Text = "MenuStrip1"
        '
        'mnuCalculate
        '
        Me.mnuCalculate.AutoToolTip = True
        Me.mnuCalculate.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CloudCoverToolStripMenuItem, Me.mnuCalcSolar, Me.mnuCalcHPET, Me.mnuCalcJPET, Me.mnuCalcPPET, Me.mnuCalcPriestly, Me.mnuCalcPMPET, Me.WindTravelToolStripMenuItem})
        Me.mnuCalculate.Name = "mnuCalculate"
        Me.mnuCalculate.Size = New System.Drawing.Size(102, 19)
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
        'WindTravelToolStripMenuItem
        '
        Me.WindTravelToolStripMenuItem.Name = "WindTravelToolStripMenuItem"
        Me.WindTravelToolStripMenuItem.Size = New System.Drawing.Size(207, 22)
        Me.WindTravelToolStripMenuItem.Text = "Wind Travel"
        Me.WindTravelToolStripMenuItem.Visible = False
        '
        'mnuDB
        '
        Me.mnuDB.Name = "mnuDB"
        Me.mnuDB.Size = New System.Drawing.Size(83, 19)
        Me.mnuDB.Text = "Select WDM"
        Me.mnuDB.Visible = False
        '
        'mnuDisaggregate
        '
        Me.mnuDisaggregate.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuDisagSolar, Me.mnuDisagDew, Me.mnuDisagRain, Me.mnuDisagPET, Me.mnuDisagTemp, Me.mnuDisagWind})
        Me.mnuDisaggregate.Name = "mnuDisaggregate"
        Me.mnuDisaggregate.Size = New System.Drawing.Size(121, 19)
        Me.mnuDisaggregate.Text = "Disaggregate Series"
        Me.mnuDisaggregate.Visible = False
        '
        'mnuDisagSolar
        '
        Me.mnuDisagSolar.Name = "mnuDisagSolar"
        Me.mnuDisagSolar.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagSolar.Text = "Solar Radiation"
        '
        'mnuDisagDew
        '
        Me.mnuDisagDew.Name = "mnuDisagDew"
        Me.mnuDisagDew.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagDew.Text = "Dew Point"
        '
        'mnuDisagRain
        '
        Me.mnuDisagRain.Name = "mnuDisagRain"
        Me.mnuDisagRain.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagRain.Text = "Precipitation"
        '
        'mnuDisagPET
        '
        Me.mnuDisagPET.Name = "mnuDisagPET"
        Me.mnuDisagPET.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagPET.Text = "Evapotranspiration"
        '
        'mnuDisagTemp
        '
        Me.mnuDisagTemp.Name = "mnuDisagTemp"
        Me.mnuDisagTemp.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagTemp.Text = "Temperature"
        '
        'mnuDisagWind
        '
        Me.mnuDisagWind.Name = "mnuDisagWind"
        Me.mnuDisagWind.Size = New System.Drawing.Size(180, 22)
        Me.mnuDisagWind.Text = "Wind"
        '
        'mnuImportNCEI
        '
        Me.mnuImportNCEI.Enabled = False
        Me.mnuImportNCEI.Name = "mnuImportNCEI"
        Me.mnuImportNCEI.Size = New System.Drawing.Size(123, 19)
        Me.mnuImportNCEI.Text = "Import Hourly NCEI"
        Me.mnuImportNCEI.Visible = False
        '
        'mnuAddAttributes
        '
        Me.mnuAddAttributes.Name = "mnuAddAttributes"
        Me.mnuAddAttributes.Size = New System.Drawing.Size(96, 19)
        Me.mnuAddAttributes.Text = "Add Attributes"
        Me.mnuAddAttributes.Visible = False
        '
        'layoutMain
        '
        Me.layoutMain.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.layoutMain.ColumnCount = 5
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 13.0!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133.0!))
        Me.layoutMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16.0!))
        Me.layoutMain.Controls.Add(Me.Label2, 1, 1)
        Me.layoutMain.Controls.Add(Me.splitWDMTab, 1, 4)
        Me.layoutMain.Controls.Add(Me.statusStrip, 1, 5)
        Me.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.layoutMain.Location = New System.Drawing.Point(0, 29)
        Me.layoutMain.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.layoutMain.Name = "layoutMain"
        Me.layoutMain.RowCount = 6
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 81.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.layoutMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64.0!))
        Me.layoutMain.Size = New System.Drawing.Size(2299, 1380)
        Me.layoutMain.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.layoutMain.SetColumnSpan(Me.Label2, 3)
        Me.Label2.Location = New System.Drawing.Point(21, 28)
        Me.Label2.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(2253, 62)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Routine calculates PEVT using Hamon and Penman method from input timeseries in a " &
    "WDM." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Also calculates solar radiation from cloud cover. "
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'splitWDMTab
        '
        Me.layoutMain.SetColumnSpan(Me.splitWDMTab, 3)
        Me.splitWDMTab.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitWDMTab.Location = New System.Drawing.Point(21, 174)
        Me.splitWDMTab.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.splitWDMTab.Name = "splitWDMTab"
        '
        'splitWDMTab.Panel1
        '
        Me.splitWDMTab.Panel1.Controls.Add(Me.TableLayoutPanel1)
        '
        'splitWDMTab.Panel2
        '
        Me.splitWDMTab.Panel2.Controls.Add(Me.TableLayoutPanel2)
        Me.splitWDMTab.Size = New System.Drawing.Size(2253, 1135)
        Me.splitWDMTab.SplitterDistance = 1333
        Me.splitWDMTab.SplitterWidth = 11
        Me.splitWDMTab.TabIndex = 5
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 4
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 211.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 208.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 312.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.grpTable, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.btnSelAllRows, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.btnSelRows, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.btnClearSelRows, 3, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1333, 1135)
        Me.TableLayoutPanel1.TabIndex = 5
        '
        'grpTable
        '
        Me.grpTable.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.SetColumnSpan(Me.grpTable, 4)
        Me.grpTable.Controls.Add(Me.dgvWDM)
        Me.grpTable.Location = New System.Drawing.Point(8, 7)
        Me.grpTable.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.grpTable.Name = "grpTable"
        Me.grpTable.Padding = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.grpTable.Size = New System.Drawing.Size(1317, 1047)
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
        Me.dgvWDM.Location = New System.Drawing.Point(8, 38)
        Me.dgvWDM.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.dgvWDM.Name = "dgvWDM"
        Me.dgvWDM.ReadOnly = True
        Me.dgvWDM.RowHeadersWidth = 5
        Me.dgvWDM.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvWDM.Size = New System.Drawing.Size(1301, 1002)
        Me.dgvWDM.TabIndex = 0
        '
        'btnSelAllRows
        '
        Me.btnSelAllRows.Enabled = False
        Me.btnSelAllRows.Location = New System.Drawing.Point(8, 1068)
        Me.btnSelAllRows.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.btnSelAllRows.Name = "btnSelAllRows"
        Me.btnSelAllRows.Size = New System.Drawing.Size(195, 57)
        Me.btnSelAllRows.TabIndex = 5
        Me.btnSelAllRows.Text = "Select All"
        Me.btnSelAllRows.UseVisualStyleBackColor = True
        '
        'btnSelRows
        '
        Me.btnSelRows.Enabled = False
        Me.btnSelRows.Location = New System.Drawing.Point(219, 1068)
        Me.btnSelRows.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.btnSelRows.Name = "btnSelRows"
        Me.btnSelRows.Size = New System.Drawing.Size(184, 55)
        Me.btnSelRows.TabIndex = 6
        Me.btnSelRows.Text = "Select"
        Me.btnSelRows.UseVisualStyleBackColor = True
        '
        'btnClearSelRows
        '
        Me.btnClearSelRows.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnClearSelRows.Enabled = False
        Me.btnClearSelRows.Location = New System.Drawing.Point(1029, 1070)
        Me.btnClearSelRows.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.btnClearSelRows.Name = "btnClearSelRows"
        Me.btnClearSelRows.Size = New System.Drawing.Size(296, 55)
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
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 509.0!))
        Me.TableLayoutPanel2.Controls.Add(Me.grpSelSeries, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCalc, 3, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.btnClearSelSta, 0, 1)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel2.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(909, 1135)
        Me.TableLayoutPanel2.TabIndex = 1
        '
        'grpSelSeries
        '
        Me.TableLayoutPanel2.SetColumnSpan(Me.grpSelSeries, 4)
        Me.grpSelSeries.Controls.Add(Me.dgvSelSeries)
        Me.grpSelSeries.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpSelSeries.Location = New System.Drawing.Point(8, 7)
        Me.grpSelSeries.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.grpSelSeries.Name = "grpSelSeries"
        Me.grpSelSeries.Padding = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.grpSelSeries.Size = New System.Drawing.Size(893, 1049)
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
        Me.dgvSelSeries.Location = New System.Drawing.Point(8, 38)
        Me.dgvSelSeries.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.dgvSelSeries.Name = "dgvSelSeries"
        Me.dgvSelSeries.ReadOnly = True
        Me.dgvSelSeries.RowHeadersWidth = 5
        Me.dgvSelSeries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvSelSeries.Size = New System.Drawing.Size(877, 1004)
        Me.dgvSelSeries.TabIndex = 0
        '
        'btnCalc
        '
        Me.btnCalc.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnCalc.Enabled = False
        Me.btnCalc.Location = New System.Drawing.Point(480, 1071)
        Me.btnCalc.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.btnCalc.Name = "btnCalc"
        Me.btnCalc.Size = New System.Drawing.Size(421, 55)
        Me.btnCalc.TabIndex = 1
        Me.btnCalc.Text = "Calculate "
        Me.btnCalc.UseVisualStyleBackColor = True
        Me.btnCalc.Visible = False
        '
        'btnClearSelSta
        '
        Me.btnClearSelSta.Location = New System.Drawing.Point(8, 1070)
        Me.btnClearSelSta.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
        Me.btnClearSelSta.Name = "btnClearSelSta"
        Me.btnClearSelSta.Size = New System.Drawing.Size(220, 55)
        Me.btnClearSelSta.TabIndex = 2
        Me.btnClearSelSta.Text = "Clear Station(s)"
        Me.btnClearSelSta.UseVisualStyleBackColor = True
        '
        'statusStrip
        '
        Me.layoutMain.SetColumnSpan(Me.statusStrip, 3)
        Me.statusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statuslbl})
        Me.statusStrip.Location = New System.Drawing.Point(13, 1358)
        Me.statusStrip.Name = "statusStrip"
        Me.statusStrip.Padding = New System.Windows.Forms.Padding(3, 0, 37, 0)
        Me.statusStrip.Size = New System.Drawing.Size(2269, 22)
        Me.statusStrip.TabIndex = 6
        '
        'statuslbl
        '
        Me.statuslbl.Name = "statuslbl"
        Me.statuslbl.Size = New System.Drawing.Size(51, 17)
        Me.statuslbl.Text = "Ready ..."
        '
        'mnuCalcPMPET
        '
        Me.mnuCalcPMPET.Name = "mnuCalcPMPET"
        Me.mnuCalcPMPET.Size = New System.Drawing.Size(207, 22)
        Me.mnuCalcPMPET.Text = "Penman-Monteith PET"
        '
        'frmWeaComp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(16.0!, 31.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(2299, 1409)
        Me.Controls.Add(Me.layoutMain)
        Me.Controls.Add(Me.mnuStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.mnuStrip
        Me.Margin = New System.Windows.Forms.Padding(8, 7, 8, 7)
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
