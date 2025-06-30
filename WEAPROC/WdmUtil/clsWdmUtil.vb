Imports atcData
Imports atcUtility
Imports MapWinUtility
Public Class clsWdmUtil
    WithEvents SelectedData As New atcTimeseriesGroup
    Private pStatusMonitor As MonitorProgressStatus
    Friend Const g_AppNameShort As String = "WDMUtil"
    Friend Const g_AppNameLong As String = "WDM Utility"

    Private WdmFile As String
    Friend g_ProgramDir As String = String.Empty

    Public Sub New(ByVal _wdmfile As String)
        Me.WdmFile = _wdmfile

        g_ProgramDir = PathNameOnly(Reflection.Assembly.GetEntryAssembly.Location)
        Dim lLogFolder As String = g_ProgramDir & "cache"
        If IO.Directory.Exists(lLogFolder) Then
            lLogFolder = lLogFolder & g_PathChar & "log" & g_PathChar
        Else
            lLogFolder = IO.Path.Combine(My.Computer.FileSystem.SpecialDirectories.MyDocuments, "log") & g_PathChar
        End If

        Logger.StartToFile(lLogFolder & Format(Now, "yyyy-MM-dd") & "at" & Format(Now, "HH-mm") & "-" & g_AppNameShort & ".log")
        'Logger.Icon = Me.Icon
        If Logger.ProgressStatus Is Nothing OrElse Not (TypeOf (Logger.ProgressStatus) Is MonitorProgressStatus) Then
            'Start running status monitor to give better progress and status indication during long-running processes
            pStatusMonitor = New MonitorProgressStatus
            If pStatusMonitor.StartMonitor(FindFile("Find Status Monitor", "StatusMonitor.exe"),
                                            g_ProgramDir,
                                            System.Diagnostics.Process.GetCurrentProcess.Id) Then
                pStatusMonitor.InnerProgressStatus = Logger.ProgressStatus
                Logger.ProgressStatus = pStatusMonitor
                Logger.Status("LABEL TITLE " & g_AppNameLong & " Status")
                Logger.Status("PROGRESS TIME ON") 'Enable time-to-completion estimation
                Logger.Status("")
            Else
                pStatusMonitor.StopMonitor()
                pStatusMonitor = Nothing
            End If
        End If

        atcDataManager.Clear()
        With atcData.atcDataManager.DataPlugins
            .Add(New atcWDM.atcDataSourceWDM)
            .Add(New atcTimeseriesNCDC.atcTimeseriesNCDC)
            .Add(New atcTimeseriesScript.atcTimeseriesScriptPlugin)
            .Add(New atcList.atcListPlugin)
            .Add(New atcGraph.atcGraphPlugin)
            .Add(New atcDataTree.atcDataTreePlugin)
        End With

        atcTimeseriesStatistics.atcTimeseriesStatistics.InitializeShared()
        AddHandler(atcDataManager.OpenedData), (AddressOf FileOpenedOrClosed)
        AddHandler(atcDataManager.ClosedData), (AddressOf FileOpenedOrClosed)

        Try
            If IO.File.Exists(WdmFile) Then
                atcDataManager.OpenDataSource(WdmFile)
            End If
            atcDataManager.UserManage("Manage Files", -1, My.Resources.met)
        Catch
        End Try
    End Sub

    Private Sub FileOpenedOrClosed(ByVal aDataSource As atcTimeseriesSource)
        Select Case atcDataManager.DataSources.Count
            'Case Is < 1 : lblFile.Text = "No files are open" : SelectedData.Clear()
            'Case 1 : lblFile.Text = atcDataManager.DataSources(0).Specification
            'Case Else : lblFile.Text = atcDataManager.DataSources.Count & " files are open"
        End Select

        Dim lSelectedStillOpen As New atcTimeseriesGroup
        For Each lSelectedTS As atcTimeseries In SelectedData
            For Each lDataSource As atcTimeseriesSource In atcDataManager.DataSources
                If lDataSource.DataSets.Contains(lSelectedTS) Then
                    lSelectedStillOpen.Add(lSelectedTS)
                End If
            Next
        Next
        If lSelectedStillOpen.Count <> SelectedData.Count Then
            SelectedData = lSelectedStillOpen
        End If
        'UpdateSelectedDataLabel()
    End Sub

End Class
