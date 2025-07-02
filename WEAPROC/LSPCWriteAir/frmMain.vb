Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms

Public Class frmMain
    Private DSNFile As String = String.Empty, StaFile As String = String.Empty
    Private AppFolder As String, LogFile As String, WdmFile As String = String.Empty
    Private wrlog As StreamWriter
    Private BegYr As Integer, EndYr As Integer
    Private dtbeg As DateTime, SimBeg As DateTime, SimEnd As DateTime
    Private tblAirfiles As DataTable
    Private outFile As String, outPath As String
    Public NumAir As Integer
    Public dictwea As New Dictionary(Of String, List(Of Integer))
    Private lWdmDS As atcWDM.atcDataSourceWDM
    Private nArgs As Integer
    Private isStandAlone As Boolean = False
    Private dlgStatus As frmStatus

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AppFolder = IO.Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly.Location)

        LogFile = Path.Combine(AppFolder, "LSPCWriteAir.log")
        wrlog = New StreamWriter(LogFile)
        wrlog.AutoFlush = True

        dtbeg = Date.Now
        WriteLog("Start Process: " + Date.Now.ToShortDateString() + "  " + Date.Now.ToLongTimeString())

        Dim arguments() As String = Environment.GetCommandLineArgs()
        nArgs = arguments.Count
        If (arguments.Count > 1) Then
            Debug.WriteLine("arg0 = " + arguments(0))
            Debug.WriteLine("arg1 = " + arguments(1))
            isStandAlone = False
            'txtWea.Text = arguments(1)
            DSNFile = arguments(1)
            Debug.WriteLine("dsnwea = " + DSNFile)
            Debug.WriteLine("Running not standalone...")
            dlgStatus = New frmStatus(Me)
            dlgStatus.Show()
            dlgStatus.ProcessWeather()
            dlgStatus.Dispose()
        Else
            isStandAlone = True
        End If
    End Sub

    Private Sub WriteLog(ByVal msg As String)
        wrlog.WriteLine(msg)
    End Sub

    Private Sub WriteStatus(ByVal msg As String)
        'lblForm.Text = msg
        lblStatus.Text = msg
        Me.Refresh()
        statusLabel.Text = msg
        statusStrip.Refresh()
    End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not isStandAlone Then
            Me.WindowState = FormWindowState.Minimized
            Me.Visible = False
        Else
            lblForm.Text = "This routine reads a *.wea file that contains list of airfiles to write."
            lblStatus.Text = "Ready ..."
        End If
    End Sub

    Public Function ReadDSNFile() As Boolean
        Dim sline As String, airfile As String
        Dim first As Boolean = True
        Dim delim = New Char() {","}
        Dim icnt As Integer = 0, slen As Integer = 0
        Dim lst As List(Of Integer)
        Dim head() As String

        WriteStatus("Reading input file " + DSNFile + "...")
        WriteLog("Reading input file " + DSNFile + "...")

        Try
            For Each sline In File.ReadLines(DSNFile)
                If icnt = 0 Then
                    head = sline.Split(delim, StringSplitOptions.RemoveEmptyEntries)
                    WdmFile = head(0).Trim()
                    NumAir = Convert.ToInt16(head(1))
                    'BegYr = Convert.ToInt16(head(2))
                    'EndYr = Convert.ToInt16(head(3))
                    SimBeg = Convert.ToDateTime(head(2))
                    SimEnd = Convert.ToDateTime(head(3))
                Else
                    head = sline.Split(delim, StringSplitOptions.RemoveEmptyEntries)
                    airfile = head(0).Trim()
                    lst = New List(Of Integer)
                    For i As Integer = 1 To 7
                        lst.Add(Convert.ToInt16(head(i)))
                    Next
                    If Not dictwea.ContainsKey(airfile) Then
                        dictwea.Add(airfile, lst)
                        lst = Nothing
                    End If
                End If
                icnt += 1
            Next

            'debug.
            For Each kv As KeyValuePair(Of String, List(Of Integer)) In dictwea
                Debug.WriteLine(kv.Key)
                lst = New List(Of Integer)
                dictwea.TryGetValue(kv.Key, lst)
                Dim air = lst(0).ToString() +
                                "," + lst(1).ToString() +
                                "," + lst(2).ToString() +
                                "," + lst(3).ToString() +
                                "," + lst(4).ToString() +
                                "," + lst(5).ToString() +
                                "," + lst(6).ToString()
                'Debug.WriteLine(air)
                WriteLog(kv.Key + " , " + air)
            Next

        Catch ex As Exception
            Dim msg = "Error reading " + DSNFile + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(msg)
            msg = Nothing
            Return False
        End Try
        WriteStatus("Ready ...")
        Return True
    End Function

    Private Sub ErrorMessage(ByVal msg As String)
        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub btnInput_Click(sender As Object, e As EventArgs) Handles btnInput.Click
        With New OpenFileDialog
            .Title = "Open DSN data file ..."
            .Filter = "DSN File (*.wea)|*.wea|All files (*.*)|*.*"
            .FilterIndex = 1
            .InitialDirectory = AppFolder
            .RestoreDirectory = vbTrue
            .ValidateNames = True
            .CheckFileExists = vbTrue
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                txtWea.Text = .FileName
                DSNFile = .FileName
                btnAir.Enabled = True
            Else 'user cancels
                Return
            End If
            .Dispose()
        End With

        ReadDSNFile()
        ProcessingAirFiles()

    End Sub

    Private Sub btnAir_Click(sender As Object, e As EventArgs) Handles btnAir.Click
        ProcessingAirFiles()
    End Sub

    Public Sub ProcessingAirFiles()

        WriteStatus("Writing AirFiles ...")
        Application.DoEvents()
        ProcessAirFiles()

        Dim td As TimeSpan = Date.Now - dtbeg
        WriteLog("End Process: " + Date.Now.ToShortDateString() + "  " + Date.Now.ToLongTimeString())
        WriteLog("Process Time: " + td.TotalMinutes.ToString("F4") + " minutes.")
        Dim msg As String = "Processed and written " + NumAir.ToString() + "  AirFiles"
        MessageBox.Show(msg, "Completed Writing AirFiles", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
        Me.Dispose()
        Environment.Exit(0)
    End Sub

    Private Sub ProcessAirFiles()

        Dim msg As String, airfile As String = String.Empty
        Dim iter As Integer = 0

        Try
            Dim cWea As clsWea
            For Each kv As KeyValuePair(Of String, List(Of Integer)) In dictwea
                iter += 1
                airfile = Path.GetFileName(kv.Key.ToString().Trim())
                msg = iter.ToString() + ": Writing " + kv.Key.ToString().Trim()
                WriteLog(msg)
                WriteStatus(msg)
                If Not isStandAlone Then
                    Debug.WriteLine(msg)
                    dlgStatus.SetStatus(iter.ToString() + ": Writing " + airfile)
                End If
                Application.DoEvents()

                Dim lstvar = New List(Of Integer)
                dictwea.TryGetValue(kv.Key, lstvar)
                cWea = New clsWea(WdmFile, kv.Key, lstvar, SimBeg, SimEnd, statusStrip, statusLabel, wrlog)
                cWea = Nothing
            Next
            WriteStatus("Ready ...")
        Catch ex As Exception
        End Try
    End Sub

End Class
