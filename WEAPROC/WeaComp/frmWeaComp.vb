Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms

Imports atcUtility
Imports atcData
'Imports atcMetCmp
Imports atcWDM
Imports atcData.atcDataManager
Imports MapWinUtility

Public Class frmWeaComp
    Private NCEIFile As String
    Private WDMFile As String
    Private AppFolder As String, LogFile As String
    Private wrlog As StreamWriter
    Private lWdmDataSource As New atcDataSourceWDM
    Private GridLocations As String()
    Private dictSeries As New SortedDictionary(Of Integer, clsMetGages)
    Private dictSelSeries As New SortedDictionary(Of Integer, String)
    'Private wdm As clsWDM = Nothing
    Private wdmTable As DataTable, wdmSelSeries As DataTable, wdmAllSeries As DataTable

    Private lstSelectedSeries As List(Of String)
    Private dictSelectedSeries As SortedDictionary(Of String, String)
    Private lstStaForPET As New List(Of String)
    Private lMaxDSN As Integer = 0

    Private Enum enumCalculation
        Solar
        Hamon
        Jensen
        Priestly
        Penman
        Wind
        Cloud
    End Enum

    Private Calculation As Integer = 0

    Public Sub New(ByVal _WDMFile As String)

        ' This call is required by the designer.
        InitializeComponent()

        AppFolder = IO.Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly.Location)

        LogFile = Path.Combine(AppFolder, "WeatherComp.log")
        wrlog = New StreamWriter(LogFile, vbTrue)
        wrlog.AutoFlush = True

        WriteLog(vbCrLf + "Start Process: " + Date.Now.ToShortDateString() + "  " + Date.Now.ToShortTimeString())
        'WriteLog("AppFolder=" + AppFolder)
        'WriteLog("LogFile=" + LogFile)

        ' Add any initialization after the InitializeComponent() call.
        CreateDataTable()
        lstSelectedSeries = New List(Of String)
        dictSelectedSeries = New SortedDictionary(Of String, String)

        btnCalc.Visible = False
        btnClearSelSta.Enabled = False

        WDMFile = _WDMFile
        mnuCalculate.Enabled = True
        GetWDMAttributes(WDMFile)

    End Sub

    Private Sub WriteLog(ByVal msg As String)
        wrlog.WriteLine(msg)
        Debug.WriteLine(msg)
    End Sub

    Private Function CreateDataTable() As Boolean
        Try
            wdmTable = New DataTable
            wdmTable.Columns.Add("DSN", GetType(Integer))
            wdmTable.Columns.Add("Location", GetType(String))
            wdmTable.Columns.Add("Scenario", GetType(String))
            wdmTable.Columns.Add("Constituent", GetType(String))
            wdmTable.Columns.Add("Latitude", GetType(String))
            wdmTable.Columns.Add("Longitude", GetType(String))
            wdmTable.Columns.Add("StartDate", GetType(Date))
            wdmTable.Columns.Add("EndDate", GetType(Date))
            wdmTable.TableName = "dtwdm"

            wdmAllSeries = New DataTable
            wdmAllSeries.Columns.Add("DSN", GetType(Integer))
            wdmAllSeries.Columns.Add("Location", GetType(String))
            wdmAllSeries.Columns.Add("Scenario", GetType(String))
            wdmAllSeries.Columns.Add("Constituent", GetType(String))
            wdmAllSeries.Columns.Add("Latitude", GetType(String))
            wdmAllSeries.Columns.Add("Longitude", GetType(String))
            wdmAllSeries.Columns.Add("StartDate", GetType(Date))
            wdmAllSeries.Columns.Add("EndDate", GetType(Date))
            wdmAllSeries.TableName = "dtAllSeries"

            wdmSelSeries = New DataTable
            wdmSelSeries.Columns.Add("DSN", GetType(Integer))
            wdmSelSeries.Columns.Add("Location", GetType(String))
            wdmSelSeries.Columns.Add("Scenario", GetType(String))

            Return True

        Catch ex As Exception
            MessageBox.Show("Error creating wdm datatable!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace)
            Return False
        End Try
    End Function

    Private Sub CalcHamon()
        Cursor.Current = Cursors.WaitCursor

        Dim lCnt As Integer = 0
        Dim lWdmFileName As String = WDMFile
        Dim lBaseDSN As Integer = 0
        Dim id As Integer = 0, msg As String

        Dim lWdmDS As New atcWDM.atcDataSourceWDM
        'Dim lWdmDS As New atcData.atcDataSource
        Try
            lWdmDS.Open(lWdmFileName)
            Dim numds As Integer = lWdmDS.DataSets.Count
            WriteLog("Number of Datasets in datasource is  " + numds.ToString())
            lBaseDSN = GetMaxDSN(lWdmDS)
            WriteLog("Maximum DSN ID is  " + lBaseDSN.ToString())
        Catch ex As Exception
            WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

        Dim numCalc As Integer = 0
        Dim sta As String, arr As String()
        Dim dsn As Integer, scen As String

        Try
            'For Each sta As String In lstSelectedSeries
            For Each kv As KeyValuePair(Of String, String) In dictSelectedSeries
                lCnt += 1
                id = lCnt + lBaseDSN
                'Dim dsn As Integer = GetSelectedDSN(sta)
                arr = kv.Key.Split("_")
                sta = arr(0)
                dsn = arr(1)
                scen = kv.Value

                Try
                    Dim lPET As atcTimeseries = lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "PEVT")(0)
                    If Not lPET = Nothing Then
                        'WriteLog("WDM already contains PEVT for selected station " + sta.ToString())
                        lPET = Nothing
                    Else
                        numCalc += 1
                        WriteLog("Calculating PEVT for selected station = " + sta.ToString() + ", dsn " + dsn.ToString())
                        WriteStatus("Calculating PEVT for selected station = " + sta.ToString())
                        lPET = Nothing
                        If scen.ToUpper().Contains("NLDAS") Then
                            CalculateHamonPET_NLDAS(lWdmDS, sta, dsn, id)
                        Else
                            CalculateHamonPET_NCDC(lWdmDS, sta, dsn, id)
                        End If
                    End If
                Catch ex As Exception
                End Try
            Next

            lWdmDS = Nothing
            If numCalc > 0 Then
                msg = "Calculated " + numCalc.ToString() + " Hamon PET timeseries!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                msg = "No Hamon PEVT timeseries were calculated" + vbCrLf +
                                "WDM already contains PEVT for selected stations!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            WriteStatus("Ready ...")

        Catch ex As Exception
            msg = "Error in calculation of Hamon PET!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Cursor.Current = Cursors.Default
    End Sub
    Private Sub CalcJensen()
        Cursor.Current = Cursors.WaitCursor

        Dim lCnt As Integer = 0
        Dim lWdmFileName As String = WDMFile
        Dim lBaseDSN As Integer = 0
        Dim id As Integer = 0, msg As String

        Dim lWdmDS As New atcWDM.atcDataSourceWDM
        'Dim lWdmDS As New atcData.atcDataSource
        Try
            lWdmDS.Open(lWdmFileName)
            Dim numds As Integer = lWdmDS.DataSets.Count
            WriteLog("Number of Datasets in datasource is  " + numds.ToString())
            lBaseDSN = GetMaxDSN(lWdmDS)
            WriteLog("Maximum DSN ID is  " + lBaseDSN.ToString())
        Catch ex As Exception
            WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

        Dim numCalc As Integer = 0
        Dim sta As String, arr As String()
        Dim dsn As Integer, scen As String

        Try
            'For Each sta As String In lstSelectedSeries
            For Each kv As KeyValuePair(Of String, String) In dictSelectedSeries
                lCnt += 1
                id = lCnt + lBaseDSN
                'Dim dsn As Integer = GetSelectedDSN(sta)
                arr = kv.Key.Split("_")
                sta = arr(0)
                dsn = arr(1)
                scen = kv.Value

                Try
                    Dim lPET As atcTimeseries = lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "PEVT")(0)
                    If Not lPET = Nothing Then
                        lPET = Nothing
                    Else
                        numCalc += 1
                        WriteLog("Calculating PEVT for selected station = " + sta.ToString() + ", dsn " + dsn.ToString())
                        WriteStatus("Calculating PEVT for selected station = " + sta.ToString())
                        lPET = Nothing
                        If scen.ToUpper().Contains("NLDAS") Then
                            CalculateHamonPET_NLDAS(lWdmDS, sta, dsn, id)
                        Else
                            CalculateHamonPET_NCDC(lWdmDS, sta, dsn, id)
                        End If
                    End If
                Catch ex As Exception
                End Try
            Next

            lWdmDS = Nothing
            If numCalc > 0 Then
                msg = "Calculated " + numCalc.ToString() + " Hamon PET timeseries!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                msg = "No Hamon PEVT timeseries were calculated" + vbCrLf +
                                "WDM already contains PEVT for selected stations!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            WriteStatus("Ready ...")

        Catch ex As Exception
            msg = "Error in calculation of Hamon PET!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Cursor.Current = Cursors.Default
    End Sub

    Private Sub CalcCloud()

    End Sub

    Private Sub CalcWind()

    End Sub

    Public Function CalculateHamonPET_NLDAS(ByVal lWdmDS As atcWDM.atcDataSourceWDM, ByVal GridLocation As String,
                                       ByVal dsn As Integer, ByRef lCounter As Integer) As Boolean

        Try
            Dim lTemp As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "ATEM")(0)

            Dim staid As String, stnam As String
            'staid = lTemp.Attributes.GetDefinedValue("STAID").Value.ToString()
            'stnam = lTemp.Attributes.GetDefinedValue("StaNam").Value.ToString()
            staid = GridLocation
            stnam = staid

            Dim lTMinTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMin)
            WriteLog("Aggregated TMIN from ATEM for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())
            Dim lTMaxTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMax)
            WriteLog("Aggregated TMAX from ATEM for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())

            Dim lLatitude As Double = lTemp.Attributes.GetDefinedValue("Latitude").Value
            Dim lLongitude As Double = lTemp.Attributes.GetDefinedValue("Longitude").Value
            WriteLog("Latitide for selected station  " + GridLocation.ToString() + " is " + lLatitude.ToString("0.0000"))

            Dim lCTS() As Double = {0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055,
                                    0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055}

            'Dim lHamonCmTS As atcTimeseries = PanEvaporationTimeseriesComputedByHamon(lTMinTSer, lTMaxTSer, Nothing, True, lLatitude, lCTS)
            Dim lHamonCmTS As atcTimeseries = PanEvaporationTimeseriesComputedByHamon("NLDAS", lTMinTSer, lTMaxTSer, lWdmDS, True, lLatitude, lCTS)
            lTMaxTSer = Nothing
            lTMinTSer = Nothing
            lTemp = Nothing

            lHamonCmTS = DisSolPet(lHamonCmTS, Nothing, 2, lLatitude)
            lHamonCmTS.Attributes.SetValue("ID", lCounter)
            lHamonCmTS.Attributes.SetValue("Constituent", "PEVT")
            lHamonCmTS.Attributes.SetValue("TSTYPE", "PEVT")
            lHamonCmTS.Attributes.AddHistory("Calculated using Hamon")
            lHamonCmTS.Attributes.SetValue("Description", "Hamon PET")
            lHamonCmTS.Attributes.SetValue("Scenario", "NLDAS")
            lHamonCmTS.Attributes.SetValue("STAID", staid)
            lHamonCmTS.Attributes.SetValue("StaNam", stnam)

            'debug, get dates and values of timeseries
            'this works for ncei hourly imported

            Dim dt() As Double = lHamonCmTS.Dates.Values()
            Dim values() As Double = lHamonCmTS.Values()
            Dim ddate(dt.Count) As Date
            For j As Integer = 0 To dt.Count - 1
                ddate(j) = DateTime.FromOADate(dt(j))
            Next
            Dim desc As String = "Hamon PET calculated from NLDAS Temperature"
            Write("NLDAS", GridLocation, desc, "PEVT", ddate, values, lLatitude, lLongitude, staid, stnam)
            WriteLog("Computed Hamon PET for selected station " + GridLocation.ToString())

            dt = Nothing
            values = Nothing
            ddate = Nothing
            lHamonCmTS = Nothing
            Return True

            'Dim Wdm As New atcWDM.atcDataSourceWDM
            'Try
            'Wdm.Open(WDMFile)
            'Catch ex As Exception
            'End Try

            'If (Wdm.AddDataSet(lHamonCmTS, atcDataSource.EnumExistAction.ExistReplace)) Then
            'WriteLog("Computed Hamon PET for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())
            'lHamonCmTS = Nothing
            'Return True
            'Else
            'WriteLog("Error computing Hamon PET for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())
            'Return False
            'End If
        Catch ex As Exception
            Dim msg = "Error calculating Hamon PET!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Function CalculateHamonPET_NCDC(ByVal lWdmDS As atcWDM.atcDataSourceWDM, ByVal GridLocation As String,
                                       ByVal dsn As Integer, ByRef lCounter As Integer) As Boolean

        Try
            Dim lTemp As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "ATEM")(0)

            Dim staid As String, stnam As String
            staid = lTemp.Attributes.GetDefinedValue("STAID").Value.ToString()
            stnam = lTemp.Attributes.GetDefinedValue("StaNam").Value.ToString()

            Dim lTMinTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMin)
            WriteLog("Aggregated TMIN from ATEM for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())
            Dim lTMaxTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMax)
            WriteLog("Aggregated TMAX from ATEM for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())

            Dim lLatitude As Double = lTemp.Attributes.GetDefinedValue("Latitude").Value
            Dim lLongitude As Double = lTemp.Attributes.GetDefinedValue("Longitude").Value
            WriteLog("Latitide for selected station  " + GridLocation.ToString() + " is " + lLatitude.ToString("0.0000"))

            Dim lCTS() As Double = {0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055,
                                    0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055, 0.0055}

            Dim lHamonCmTS As atcTimeseries = PanEvaporationTimeseriesComputedByHamon("NCDC", lTMinTSer, lTMaxTSer, lWdmDS, True, lLatitude, lCTS)
            lTMaxTSer = Nothing
            lTMinTSer = Nothing
            lTemp = Nothing

            lHamonCmTS = DisSolPet(lHamonCmTS, Nothing, 2, lLatitude)
            lHamonCmTS.Attributes.SetValue("ID", lCounter)
            lHamonCmTS.Attributes.SetValue("Constituent", "PEVT")
            lHamonCmTS.Attributes.SetValue("TSTYPE", "PEVT")
            lHamonCmTS.Attributes.AddHistory("Calculated using Hamon")
            lHamonCmTS.Attributes.SetValue("Description", "Hamon PET")
            lHamonCmTS.Attributes.SetValue("Scenario", "COMPUTED")

            'lWdmDS.AddDataSet(lHamonCmTS, atcDataSource.EnumExistAction.ExistAskUser)
            'WriteLog("Computed Hamon PET for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())

            'debug, get dates and values of timeseries
            'this works for ncei hourly imported
            Dim dt() As Double = lHamonCmTS.Dates.Values()
            Dim values() As Double = lHamonCmTS.Values()
            Dim ddate(dt.Count) As Date
            For j As Integer = 0 To dt.Count - 1
                ddate(j) = DateTime.FromOADate(dt(j))
                'Debug.WriteLine(ddate(j).ToString() + ", " + values(j).ToString())
            Next
            Dim desc As String = "Hamon PET calculated from min and max temp"
            Write("COMPUTED", GridLocation, desc, "PEVT", ddate, values, lLatitude, lLongitude, staid, stnam)
            WriteLog("Computed Hamon PET for selected station " + GridLocation.ToString())

            dt = Nothing
            values = Nothing
            ddate = Nothing
            lHamonCmTS = Nothing
            Return True
        Catch ex As Exception
            Dim msg = "Error calculating Hamon PET!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
    Private Sub CalcPenman()
        Cursor.Current = Cursors.WaitCursor

        Dim lCnt As Integer = 0
        Dim lWdmFileName As String = WDMFile
        Dim lBaseDSN As Integer = 0
        Dim id As Integer = 0, msg As String

        Dim lWdmDS As New atcWDM.atcDataSourceWDM
        Try
            lWdmDS.Open(lWdmFileName)
            Dim numds As Integer = lWdmDS.DataSets.Count
            WriteLog("Number of Datasets in datasource is  " + numds.ToString())
            lBaseDSN = GetMaxDSN(lWdmDS)
            WriteLog("Maximum DSN ID is  " + lBaseDSN.ToString())
        Catch ex As Exception
            WriteLog("CalcPenman: Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            MessageBox.Show("CalcPenman: Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

        Dim numCalc As Integer = 0
        Try
            For Each sta As String In lstSelectedSeries
                lCnt += 1
                id = lCnt + lBaseDSN
                Dim dsn As Integer = GetSelectedDSN(sta)
                Try
                    Dim lPET As atcTimeseries = lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "PPAN")(0)
                    If Not lPET = Nothing Then
                        lPET = Nothing
                        WriteLog("WDM already contains PPAN for selected station " + sta.ToString())
                    Else
                        lPET = Nothing
                        numCalc += 1
                        WriteLog("Calculating Penman Pan Evaporation(PPAN) for selected station = " + sta.ToString() + ", dsn " + dsn.ToString())
                        WriteStatus("Calculating Penman Pan Evaporation(PPAN) for selected station = " + sta.ToString())
                        CalculatePenmanPET(lWdmDS, sta, dsn, id)
                    End If
                Catch ex As Exception
                End Try
            Next
            lWdmDS = Nothing

            If numCalc > 0 Then
                msg = "Calculated " + numCalc.ToString() + " Penman Pan Evaporation timeseries!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                msg = "No Pan Evopration timeseries were calculated" + vbCrLf +
                                "WDM already contains Penman Pan Evaporation (PPAN) for selected stations!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            WriteStatus("Ready...")
        Catch ex As Exception
            msg = "Error in calculation of Penman Pan Evaporation (PPAN)!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Cursor.Current = Cursors.Default
    End Sub

    Private Function CalculatePenmanPET(ByVal lWdmDS As atcWDM.atcDataSourceWDM,
                                    ByVal GridLocation As String,
                                    ByVal dsn As Integer, ByRef lCounter As Integer) As Boolean

        Try
            Dim lTemp As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "ATEM")(0)

            Dim staid As String, stnam As String
            staid = lTemp.Attributes.GetDefinedValue("STAID").Value.ToString()
            stnam = lTemp.Attributes.GetDefinedValue("StaNam").Value.ToString()

            Dim scen As String = lTemp.Attributes.GetDefinedValue("Scenario").Value

            Dim lTMinTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMin)
            Dim lTMaxTSer As atcTimeseries = Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranMax)
            Dim lsRadTSer As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "SOLR")(0)
            lsRadTSer = Aggregate(lsRadTSer, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv)
            Dim lDewPTSer As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "DEWP")(0)
            lDewPTSer = Aggregate(lDewPTSer, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
            Dim lWindTSer As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "WIND")(0)
            lWindTSer = Aggregate(lWindTSer, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv)
            Dim lMetCmpTS As atcTimeseries = PanEvaporationTimeseriesComputedByPenman(lTMinTSer, lTMaxTSer, lsRadTSer, lDewPTSer, lWindTSer, Nothing)
            Dim lLatitude As Double = lTemp.Attributes.GetDefinedValue("Latitude").Value
            Dim lLongitude As Double = lTemp.Attributes.GetDefinedValue("Longitude").Value

            lTemp = Nothing
            lTMinTSer = Nothing
            lTMaxTSer = Nothing
            lsRadTSer = Nothing
            lDewPTSer = Nothing
            lWindTSer = Nothing

            lMetCmpTS = DisSolPet(lMetCmpTS, Nothing, 2, lLatitude)
            lMetCmpTS.Attributes.SetValue("ID", lCounter)
            lMetCmpTS.Attributes.SetValue("Constituent", "PPAN")
            lMetCmpTS.Attributes.SetValue("TSTYPE", "PPAN")
            lMetCmpTS.Attributes.AddHistory("Calculated using Penman Pan")

            If scen.ToUpper().Contains("NLDAS") Then
                lMetCmpTS.Attributes.SetValue("Description", "Penman Evaporation from NLDAS grid")
                lMetCmpTS.Attributes.SetValue("Scenario", "NLDAS")
                lWdmDS.AddDataSet(lMetCmpTS, atcDataSource.EnumExistAction.ExistAskUser)
            Else
                lMetCmpTS.Attributes.SetValue("Description", "Penman Evaporation from NCDC Station")
                lMetCmpTS.Attributes.SetValue("Scenario", "COMPUTED")
                Dim dt() As Double = lMetCmpTS.Dates.Values()
                Dim values() As Double = lMetCmpTS.Values()
                Dim ddate(dt.Count) As Date
                For j As Integer = 0 To dt.Count - 1
                    ddate(j) = DateTime.FromOADate(dt(j))
                Next
                Dim desc As String = "Hourly Penman Pan Evaporation from NCDC Station"
                Write("COMPUTED", GridLocation, desc, "PPAN", ddate, values, lLatitude, lLongitude, staid, stnam)
                dt = Nothing
                values = Nothing
                ddate = Nothing
            End If

            WriteLog("CalculatePenmanPET: Computed hourly Penman Pan Evaporation for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())
            lMetCmpTS = Nothing

            Return True
        Catch ex As Exception
            Dim msg = "CalculatePenmanPET: Error calculating hourly Penman Pan Evaporation!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub CalcSolar()
        Cursor.Current = Cursors.WaitCursor

        Dim lCnt As Integer = 0
        Dim lWdmFileName As String = WDMFile
        Dim lBaseDSN As Integer = 0
        Dim id As Integer = 0
        Dim msg As String

        Dim lWdmDS As New atcWDM.atcDataSourceWDM
        Try
            lWdmDS.Open(lWdmFileName)
            Dim numds As Integer = lWdmDS.DataSets.Count
            lBaseDSN = GetMaxDSN(lWdmDS)
        Catch ex As Exception
            MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

        Dim numCalc As Integer = 0
        Dim sta As String, arr As String()
        Dim dsn As Integer, scen As String

        Try
            'For Each sta As String In lstSelectedSeries
            For Each kv As KeyValuePair(Of String, String) In dictSelectedSeries
                lCnt += 1
                id = lCnt + lBaseDSN
                'Dim dsn As Integer = GetSelectedDSN(sta)
                arr = kv.Key.Split("_")
                sta = arr(0)
                dsn = arr(1)
                scen = kv.Value

                Try
                    Dim lSol As atcTimeseries = lWdmDS.DataSets.FindData("Location", sta).FindData("Constituent", "SOLR")(0)
                    If Not lSol = Nothing Then
                        lSol = Nothing
                        WriteLog("WDM already contains SOLR for selected station " + sta.ToString())
                    Else
                        lSol = Nothing
                        numCalc += 1
                        WriteLog("Calculation of SOLR for selected station = " + sta.ToString() + ", dsn " + dsn.ToString())
                        WriteStatus("Calculating SOLR for selected station = " + sta.ToString())
                        'CalculateSolar(lWdmDS, sta, dsn, id)
                        If scen.ToUpper().Contains("NLDAS") Then
                            CalculateSolar_NLDAS(lWdmDS, sta, dsn, id)
                        Else
                            CalculateSolar_NCDC(lWdmDS, sta, dsn, id)
                        End If
                    End If
                Catch ex As Exception
                End Try
            Next
            lWdmDS = Nothing
            If numCalc > 0 Then
                msg = "Calculated " + numCalc.ToString() + " SOLR timeseries!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else

                msg = "No SOLR timeseries were calculated" + vbCrLf +
                                "WDM already contains SOLR for selected stations!"
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            WriteStatus("Ready...")

        Catch ex As Exception
            msg = "Error in calculation of SOLR!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Cursor.Current = Cursors.Default
    End Sub

    Private Function CalculateSolar_NLDAS(ByVal lWdmDS As atcWDM.atcDataSourceWDM,
                                    ByVal GridLocation As String,
                                    ByVal dsn As Integer, ByRef lCounter As Integer) As Boolean

        Try
            Dim lClou As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "CLOU")(0)
            Dim scen As String = lClou.Attributes.GetDefinedValue("Scenario").Value

            Dim lDClou As atcTimeseries = Aggregate(lClou, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
            WriteLog("Aggregated daily CLOU from hourly for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())

            Dim lLatitude As Double = lClou.Attributes.GetDefinedValue("Latitude").Value
            WriteLog("Latitide for selected station " + GridLocation.ToString() + " is " + lLatitude.ToString("0.0000"))

            Dim lSolarCmTS As atcTimeseries = SolarRadiationFromCloudCover(lDClou, lWdmDS, lLatitude)
            lClou = Nothing
            lDClou = Nothing

            lSolarCmTS = DisSolPet(lSolarCmTS, lWdmDS, 1, lLatitude)
            lSolarCmTS.Attributes.SetValue("ID", lCounter)
            lSolarCmTS.Attributes.SetValue("Constituent", "SOLR")
            lSolarCmTS.Attributes.SetValue("TSTYPE", "SOLR")
            lSolarCmTS.Attributes.AddHistory("Computed Daily Solar Radiation from cloud cover")
            lSolarCmTS.Attributes.SetValue("Description", "Hourly Solar Radiation (langleys) computed from Daily Cloud Cover")

            If scen.ToUpper().Contains("NLDAS") Then
                lSolarCmTS.Attributes.SetValue("Scenario", "NLDAS")
            Else
                lSolarCmTS.Attributes.SetValue("Scenario", "COMPUTED")
            End If

            lWdmDS.AddDataSet(lSolarCmTS, atcDataSource.EnumExistAction.ExistAskUser)
            WriteLog("Computed hourly SOLR for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())
            lSolarCmTS = Nothing

            Return True
        Catch ex As Exception
            Dim msg = "Error calculating hourly SOLR!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Function CalculateSolar_NCDC(ByVal lWdmDS As atcWDM.atcDataSourceWDM,
                                    ByVal GridLocation As String,
                                    ByVal dsn As Integer, ByRef lCounter As Integer) As Boolean

        Try
            Dim lClou As atcTimeseries = lWdmDS.DataSets.FindData("Location", GridLocation).FindData("Constituent", "CLOU")(0)

            Dim staid As String, stnam As String
            staid = lClou.Attributes.GetDefinedValue("STAID").Value.ToString()
            stnam = lClou.Attributes.GetDefinedValue("StaNam").Value.ToString()

            Dim lDClou As atcTimeseries = Aggregate(lClou, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
            WriteLog("Aggregated daily CLOU from hourly for selected station " + GridLocation.ToString() + ", dsn " + dsn.ToString())

            Dim lLatitude As Double = lClou.Attributes.GetDefinedValue("Latitude").Value
            Dim lLongitude As Double = lClou.Attributes.GetDefinedValue("Longitude").Value
            Dim lElevation As Double = lClou.Attributes.GetDefinedValue("Elevation").Value
            Dim lDescript As String = lClou.Attributes.GetDefinedValue("Description").Value
            WriteLog("Latitude for selected station " + GridLocation.ToString() + " is " + lLatitude.ToString("0.0000"))

            Dim lSolarCmTS As atcTimeseries = SolarRadiationFromCloudCover(lDClou, lWdmDS, lLatitude)
            lDClou = Nothing
            lClou = Nothing

            lSolarCmTS = DisSolPet(lSolarCmTS, lWdmDS, 1, lLatitude)
            lSolarCmTS.Attributes.SetValue("ID", lCounter)
            lSolarCmTS.Attributes.SetValue("Constituent", "SOLR")
            lSolarCmTS.Attributes.SetValue("TSTYPE", "SOLR")
            lSolarCmTS.Attributes.AddHistory("Computed Daily Solar Radiation from cloud cover")
            lSolarCmTS.Attributes.SetValue("Description", "Hourly Solar Radiation (langleys) computed from Daily Cloud Cover")
            lSolarCmTS.Attributes.SetValue("Latitude", lLatitude)
            lSolarCmTS.Attributes.SetValue("Longitude", lLongitude)
            lSolarCmTS.Attributes.SetValue("Elevation", lElevation)
            'lSolarCmTS.Attributes.SetValue("Description", lDescript)
            lSolarCmTS.Attributes.SetValue("StaNam", stnam)
            lSolarCmTS.Attributes.SetValue("STAID", staid)
            lSolarCmTS.Attributes.SetValue("Scenario", "COMPUTED")

            'lWdmDS.AddDataSet(lSolarCmTS, atcDataSource.EnumExistAction.ExistReplace)
            'WriteLog("Computed hourly SOLR for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())

            'use this alternative, if imported ncei hourly data 
            'debug,   get dates and values of timeseries
            'this works for ncei hourly imported
            'WriteLog("Writing NCEI data using alternative write routine...")

            Dim dt() As Double = lSolarCmTS.Dates.Values()
            Dim values() As Double = lSolarCmTS.Values()
            Dim ddate(dt.Count) As Date
            For j As Integer = 0 To dt.Count - 1
                ddate(j) = DateTime.FromOADate(dt(j))
                'Debug.WriteLine(ddate(j).ToString() + ", " + values(j).ToString())
            Next
            Dim desc As String = lDescript + " (Computed from Daily Cloud Cover)"
            Write("COMPUTED", GridLocation, desc, "SOLR", ddate, values, lLatitude, lLongitude, staid, stnam)
            WriteLog("Computed hourly SOLR for selected station " + GridLocation.ToString())

            lSolarCmTS = Nothing
            dt = Nothing
            values = Nothing
            ddate = Nothing

            Return True
        Catch ex As Exception
            Dim msg = "Error calculating hourly SOLR!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
    Private Function GetDictionaryOfDatasets() As Boolean
        'Dim dSeries As New SortedDictionary(Of Integer, clsMetGages)
        Dim lwdm As New atcWDM.atcDataSourceWDM
        If Not lwdm.Open(WDMFile) Then
            lwdm = Nothing
            Return False
        End If

        Try
            'lwdm.Open(WDMFile)
            Dim numds As Integer = lwdm.DataSets.Count
            Dim dbeg As Date
            Dim dend As Date
            Dim numval As Integer, dsn As Integer
            Dim sloc As String, svar As String, scen As String, lat As String, lon As String
            Dim mndate As String = String.Empty, mxdate As String = String.Empty

            For Each lDataSet As atcTimeseries In lwdm.DataSets
                sloc = lDataSet.Attributes.GetValue("Location").Trim()
                dsn = lDataSet.Attributes.GetValue("ID")
                svar = lDataSet.Attributes.GetValue("Constituent").Trim()
                scen = lDataSet.Attributes.GetValue("Scenario").Trim()
                lat = lDataSet.Attributes.GetValue("Latitude").ToString().Trim()
                lon = lDataSet.Attributes.GetValue("Longitude").ToString().Trim()

                numval = lDataSet.numValues
                'memory error here 8/11/20
                dbeg = Date.FromOADate(lDataSet.Dates.Value(0))
                dend = Date.FromOADate(lDataSet.Dates.Value(numval - 1))
                mndate = dbeg.ToLocalTime().ToString()
                mxdate = dend.ToLocalTime().ToString()
                lDataSet.Clear()

                'For i As Integer = 0 To numds - 1
                'Dim sloc As String = (lwdm.DataSets(i).Attributes.GetValue("Location")).Trim()
                'Dim dsn As Integer = lwdm.DataSets(i).Attributes.GetValue("ID")
                'Dim svar As String = (lwdm.DataSets(i).Attributes.GetValue("Constituent")).Trim()
                'Dim scen As String = (lwdm.DataSets(i).Attributes.GetValue("Scenario")).Trim()
                'Dim lat As String = (lwdm.DataSets(i).Attributes.GetValue("Latitude")).ToString().Trim()
                'Dim lon As String = (lwdm.DataSets(i).Attributes.GetValue("Longitude")).ToString().Trim()

                'Dim tseries As atcData.atcTimeseries = lwdm.DataSets(i)
                'numval = tseries.numValues
                'If i = 0 Then
                'dbeg = Date.FromOADate(tseries.Dates.Value(0))
                'dend = Date.FromOADate(tseries.Dates.Value(numval - 1))
                'End If

                'Dim mndate As String = dbeg.ToLocalTime().ToString()
                'Dim mxdate As String = dend.ToLocalTime().ToString()
                'tseries = Nothing

                'dbeg = Nothing
                'dend = Nothing

                Dim clsMet As New clsMetGages(dsn, sloc, svar, scen, lat, lon, mndate, mxdate)
                If Not dictSeries.ContainsKey(dsn) Then
                    dictSeries.Add(dsn, clsMet)
                End If
                clsMet = Nothing
            Next
            lwdm = Nothing
            Return True
        Catch ex As Exception
            lwdm = Nothing
            Dim msg = vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error getting attributes of wdm datasets", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return False
        End Try
        Return True
    End Function

    Private Sub GetWDMAttributes(ByVal aWDMFile As String)
        Cursor.Current = Cursors.WaitCursor

        WriteLog("Reading WDM timeseries location And attributes ...")

        'wdm.Initialize(aWDMFile)
        'dictSeries = wdm.GetWeaSeriesAttributes()
        GetDictionaryOfDatasets()
        Dim icnt As Integer = FillAllSeriesDataTable(dictSeries)

        'set group text
        grpTable.Text = icnt.ToString() + " series In " + WDMFile

        Cursor.Current = Cursors.Default
    End Sub

    Private Function FillAllSeriesDataTable(ByVal dictSeries As SortedDictionary(Of Integer, clsMetGages)) As Integer

        Cursor.Current = Cursors.WaitCursor
        wdmAllSeries.Clear()

        Try
            wdmAllSeries.Rows.Clear()
            For Each kv As KeyValuePair(Of Integer, clsMetGages) In dictSeries
                Dim dsn As Integer = kv.Key
                Dim sta As New clsMetGages(0, "", "", "", "", "", "", "")
                dictSeries.TryGetValue(dsn, sta)

                Dim dtrow As DataRow = wdmAllSeries.NewRow()
                dtrow("DSN") = dsn
                dtrow("Scenario") = sta.Scenario
                dtrow("Location") = sta.staID
                dtrow("Constituent") = sta.Pcode
                dtrow("Latitude") = sta.Lat
                dtrow("Longitude") = sta.Lon
                dtrow("StartDate") = CDate(sta.MinDate)
                dtrow("EndDate") = CDate(sta.MaxDate)
                'dtrow("StartDate") = DBNull.Value
                'dtrow("EndDate") = DBNull.Value

                'only includes NLDAS timeseries
                'If sta.Scenario.Contains("NLDAS") Then
                'wdmAllSeries.Rows.Add(dtrow)
                'End If

                'show all
                wdmAllSeries.Rows.Add(dtrow)

                dtrow = Nothing
            Next

            dgvWDM.Enabled = True
            dgvWDM.DataSource = Nothing
            dgvWDM.DataSource = wdmAllSeries
            dgvWDM.ClearSelection()
            dgvWDM.Columns.Item("Latitude").Visible = False
            dgvWDM.Columns.Item("Longitude").Visible = False
            'dgvWDM.Columns.Item("StartDate").Visible = False
            'dgvWDM.Columns.Item("EndDate").Visible = False

            Return wdmAllSeries.Rows.Count
        Catch ex As Exception
            MessageBox.Show("Error In generating WDM datatable!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace)
            Return 0
        End Try

        Cursor.Current = Cursors.Default
    End Function

    Private Function GetMaxDSN(ByVal lds As atcWDM.atcDataSourceWDM) As Integer
        Dim mxDSN As Integer = 0
        Dim id As Integer = 0

        For i As Integer = 0 To lds.DataSets.Count - 1
            With lds.DataSets(i).Attributes
                id = .GetValue("ID")
                If (id > mxDSN) Then
                    mxDSN = id
                End If
            End With
        Next
        Return mxDSN
    End Function

    Private Sub AddSelectedSeries()
        'enable clear selection button
        btnClearSelRows.Enabled = True

        Dim sta As String, scen As String, sta_dsn As String, dsn As String
        If dgvWDM.SelectedRows.Count > 0 Then
            For Each dr As DataGridViewRow In dgvWDM.SelectedRows
                sta = dr.Cells.Item("Location").Value
                scen = dr.Cells.Item("Scenario").Value
                dsn = dr.Cells.Item("DSN").Value.ToString()
                sta_dsn = sta + "_" + dsn

                If Not lstSelectedSeries.Contains(sta) Then
                    lstSelectedSeries.Add(sta)
                End If

                'alternative using dictionary
                If Not dictSelectedSeries.Keys.Contains(sta_dsn) Then
                    dictSelectedSeries.Add(sta_dsn, scen)
                End If

            Next

            wdmSelSeries.Rows.Clear()
            'For Each sta As String In lstSelectedSeries
            For Each kv As KeyValuePair(Of String, String) In dictSelectedSeries
                Dim dtrow As DataRow = wdmSelSeries.NewRow()
                Dim arr As String() = kv.Key.Split("_")
                scen = String.Empty
                'dtrow(0) = GetSelectedDSN(sta)
                dtrow("DSN") = CInt(arr(1))           'dsn
                dtrow("Location") = arr(0)            'location
                dictSelectedSeries.TryGetValue(kv.Key, scen)
                If scen.ToUpper().Contains("NLDAS") Then    'scenario
                    dtrow("Scenario") = "NLDAS"
                Else
                    dtrow("Scenario") = "NCDC"
                End If

                wdmSelSeries.Rows.Add(dtrow)
                dtrow = Nothing
            Next

            dgvSelSeries.DataSource = Nothing
            dgvSelSeries.DataSource = wdmSelSeries
            dgvSelSeries.ClearSelection()

            If dgvSelSeries.Rows.Count > 0 Then
                btnCalc.Enabled = True
            Else
                btnCalc.Enabled = False
            End If
        Else
            btnSelRows.Enabled = False
            Return
        End If
    End Sub

    Private Function GetLocationDSN(ByVal sta_dsn As String) As String()
        Dim ar As String() = sta_dsn.Split("_")
        Dim dsn As Integer = CInt(ar(1))
        Return ar
    End Function

    Private Function GetSelectedDSN(ByVal location As String) As Integer
        Dim dsn As Integer = 0
        Dim drows() As DataRow = wdmTable.Select("Location=" + "'" + location + "'")

        For Each dr As DataRow In drows
            dsn = dr("DSN")
        Next
        Return dsn
    End Function

    Private Function GetSelectedDSN_Scenario(ByVal location As String) As List(Of String)
        Dim dsn As Integer = 0, scen As String
        Dim lst As New List(Of String)
        Dim drows() As DataRow = wdmTable.Select("Location=" + "'" + location + "'")

        For Each dr As DataRow In drows
            dsn = dr("DSN")
            scen = dr("Scenario")
            lst.Add(dsn.ToString())
            lst.Add(scen)
        Next
        Return lst
    End Function

    Private Function ShowWDMDataGrid(ByVal aPcode As String) As Integer
        dgvWDM.Enabled = True

        wdmTable.Rows.Clear()
        wdmSelSeries.Rows.Clear()
        lstSelectedSeries.Clear()

        Dim qry As String = "Constituent = " + "'" + aPcode + "'"
        Debug.WriteLine("Qry=" + qry)
        Dim dtrows() As DataRow = wdmAllSeries.Select(qry)

        If dtrows.Count > 0 Then
            wdmTable.Rows.Clear()
            For Each arow As DataRow In dtrows

                If Calculation = CInt(enumCalculation.Penman) Then
                    Debug.WriteLine("Station=" + arow("Location"))
                    If Not lstStaForPET.Contains(arow("Location")) Then Continue For
                End If

                Dim dtrow As DataRow = wdmTable.NewRow()
                Dim sta = arow("Location")
                dtrow("DSN") = arow("DSN")
                dtrow("Scenario") = arow("Scenario")
                dtrow("Location") = arow("Location")
                dtrow("Constituent") = arow("Constituent")
                dtrow("Latitude") = arow("Latitude")
                dtrow("Longitude") = arow("Longitude")
                dtrow("StartDate") = arow("StartDate")
                dtrow("EndDate") = arow("EndDate")

                wdmTable.Rows.Add(dtrow)
                dtrow = Nothing
            Next

            dgvWDM.DataSource = Nothing
            dgvWDM.DataSource = wdmTable
            dgvWDM.ClearSelection()
            dgvWDM.Columns.Item("Latitude").Visible = False
            dgvWDM.Columns.Item("Longitude").Visible = False
            If Calculation = CInt(enumCalculation.Penman) Then
                dgvWDM.Columns.Item("Constituent").Visible = False
            Else
                dgvWDM.Columns.Item("Constituent").Visible = True
            End If

            Return wdmTable.Rows.Count
        Else
            MessageBox.Show("There are no datasets for constituent " + aPcode + "!")
            Return 0
        End If
    End Function

    Private Sub SetCalculationText()
        Select Case Calculation
            Case enumCalculation.Solar
                btnCalc.Text = "Calculate Solar"
            Case enumCalculation.Hamon
                btnCalc.Text = "Calculate Hamon PET"
            Case enumCalculation.Penman
                btnCalc.Text = "Calculate Penman Pan"
            Case enumCalculation.Priestly
                btnCalc.Text = "Calculate Priestly PET"
        End Select
    End Sub

    Private Function GetStationsForPenman() As Integer
        Dim lstSta As New List(Of String)
        Try

            Dim drows = From row In wdmAllSeries.AsEnumerable()
                        Select row.Field(Of String)("Location") Distinct

            For Each arow As String In drows
                lstSta.Add(arow)
                'Debug.WriteLine("unique station=" + arow)
            Next
            drows = Nothing

            'check all parameters
            Dim lds As New atcWDM.atcDataSourceWDM
            lds.Open(WDMFile)

            Dim ltmp As atcTimeseries = Nothing
            Dim lsol As atcTimeseries = Nothing
            Dim ldew As atcTimeseries = Nothing
            Dim lwnd As atcTimeseries = Nothing

            lstStaForPET.Clear()
            For Each sta In lstSta
                ltmp = lds.DataSets.FindData("Location", sta).FindData("Constituent", "ATEM")(0)
                lsol = lds.DataSets.FindData("Location", sta).FindData("Constituent", "SOLR")(0)
                ldew = lds.DataSets.FindData("Location", sta).FindData("Constituent", "DEWP")(0)
                lwnd = lds.DataSets.FindData("Location", sta).FindData("Constituent", "WIND")(0)

                If (Not IsNothing(ltmp) And Not IsNothing(lsol) And
                    Not IsNothing(ldew) And Not IsNothing(lwnd)) Then
                    lstStaForPET.Add(sta)
                End If
            Next
            lds = Nothing
            lstSta = Nothing

            'debug
            For Each st As String In lstStaForPET
                Debug.WriteLine("Available Penman Sta =" + st)
            Next
            Return lstStaForPET.Count
        Catch ex As Exception
            WriteLog("Error in finding stations for Penman PET!" + vbCrLf + ex.Message +
                            vbCrLf + ex.StackTrace)
            MessageBox.Show("Error in finding stations for Penman PET!" + vbCrLf + ex.Message +
                            vbCrLf + ex.StackTrace)

            Return 0
        End Try
    End Function

    Private Sub btnSelAllRows_Click(sender As Object, e As EventArgs) Handles btnSelAllRows.Click
        'enable clear selection button
        btnClearSelRows.Enabled = True
        'select all rows
        dgvWDM.SelectAll()

        lstSelectedSeries.Clear()
        dictSelectedSeries.Clear()
        wdmSelSeries.Rows.Clear()

        AddSelectedSeries()

        btnClearSelSta.Enabled = True
        btnCalc.Enabled = True
        btnCalc.Visible = True
    End Sub

    Private Sub btnSelRows_Click(sender As Object, e As EventArgs) Handles btnSelRows.Click
        AddSelectedSeries()
        btnClearSelSta.Enabled = True
        btnCalc.Enabled = True
        btnCalc.Visible = True
    End Sub

    Private Sub dgvWDM_Click(sender As Object, e As EventArgs) Handles dgvWDM.Click
        'btnSelRows_Click(sender, e)
        'AddSelectedSeries()
        If dgvWDM.SelectedRows.Count > 0 Then
            btnClearSelRows.Enabled = True
            btnSelRows.Enabled = True
        Else
            btnClearSelRows.Enabled = False
            btnSelRows.Enabled = False
        End If
    End Sub

    Private Sub btnCalc_Click(sender As Object, e As EventArgs) Handles btnCalc.Click
        Select Case Calculation
            Case enumCalculation.Solar
                CalcSolar()
            Case enumCalculation.Hamon
                CalcHamon()
            Case enumCalculation.Jensen
                CalcJensen()
            Case enumCalculation.Penman
                CalcPenman()
            Case enumCalculation.Wind
                CalcWind()
            Case enumCalculation.Cloud
                CalcCloud()
            Case enumCalculation.Priestly
        End Select
    End Sub

    Private Sub btnClearSelSta_Click(sender As Object, e As EventArgs) Handles btnClearSelSta.Click
        If lstSelectedSeries.Count > 0 Then
            lstSelectedSeries.Clear()
        End If
        If dictSelectedSeries.Count > 0 Then
            dictSelectedSeries.Clear()
        End If
        If wdmSelSeries.Rows.Count > 0 Then
            wdmSelSeries.Rows.Clear()
        End If
        dgvSelSeries.DataSource = Nothing
        btnClearSelSta.Enabled = False
        btnCalc.Enabled = False
        btnCalc.Visible = False
    End Sub

    Private Sub btnClearSelRows_Click(sender As Object, e As EventArgs) Handles btnClearSelRows.Click
        dgvWDM.ClearSelection()
        'If dgvSelSeries.Rows.Count > 0 Then dgvSelSeries.Rows.Clear()
        'If lstSelectedSeries.Count > 0 Then lstSelectedSeries.Clear()
        'If wdmSelSeries.Rows.Count > 0 Then wdmSelSeries.Rows.Clear()
        btnSelRows.Enabled = False
    End Sub

    Private Sub mnuCalcHPET_Click(sender As Object, e As EventArgs) Handles mnuCalcHPET.Click

        Dim icnt As Integer = ShowWDMDataGrid("ATEM")
        grpTable.Text = icnt.ToString() + " ATEM series in " + WDMFile

        btnSelAllRows.Enabled = True
        btnSelRows.Enabled = True
        Calculation = enumCalculation.Hamon
        SetCalculationText()

    End Sub

    Private Sub mnuCalcPPET_Click(sender As Object, e As EventArgs) Handles mnuCalcPPET.Click

        Calculation = CInt(enumCalculation.Penman)
        If GetStationsForPenman() <= 0 Then
            WriteLog("There are no stations available for Penman PET calculation!")
            MessageBox.Show("There are no stations available for Penman PET calculation!")
            Return
        End If
        Dim icnt As Integer = ShowWDMDataGrid("ATEM")
        grpTable.Text = icnt.ToString() + " stations for Penman PET in " + WDMFile

        btnSelAllRows.Enabled = True
        btnSelRows.Enabled = True
        SetCalculationText()

    End Sub

    Private Sub mnuCalcSolar_Click(sender As Object, e As EventArgs) Handles mnuCalcSolar.Click

        Calculation = enumCalculation.Solar

        Dim icnt As Integer = ShowWDMDataGrid("CLOU")
        grpTable.Text = icnt.ToString() + " CLOU series in " + WDMFile

        btnSelAllRows.Enabled = True
        btnSelRows.Enabled = True
        SetCalculationText()
    End Sub

    Private Sub mnuDB_Click(sender As Object, e As EventArgs) Handles mnuDB.Click

        Application.DoEvents()
        With New OpenFileDialog
            .Title = "Select WDM file ..."
            .Filter = "WDM File (*.wdm)|*.wdm|All files (*.*)|*.*"
            .FilterIndex = 1
            .InitialDirectory = AppFolder
            .RestoreDirectory = vbTrue
            .ValidateNames = True
            .CheckFileExists = vbTrue
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                WDMFile = .FileName
                mnuCalculate.Enabled = True
            Else 'user cancels
                Return
            End If
            .Dispose()
        End With

        'user change wdm
        'If IsNothing(wdm) Then
        'wdm = New clsWDM
        'Else
        'wdm = Nothing
        'wdm = New clsWDM
        'End If

        WriteStatus("Getting stations in " + WDMFile)
        GetWDMAttributes(WDMFile)
        Me.Text = "LSPC Weather Processing:" + WDMFile
        WriteStatus("Ready...")
    End Sub

    'Write("COMPUTED", GridLocation, desc, "SOLR", ddate, values, lLatitude, lLongitude, staid, stnam)
    'Write("NLDAS", GridLocation, desc, "PEVT", ddate, values, lLatitude, lLongitude)
    Private Sub Write(ByVal Scenario As String, ByVal StationID As String,
                     ByVal StationName As String, ByVal PCode As String,
                     ByVal DateTimes() As DateTime, ByVal Values() As Double,
                     ByVal lat As Double, ByVal lon As Double,
                     ByVal staid As String, ByVal stanam As String)
        Try
            Dim lTS As New atcData.atcTimeseries(Nothing)
            lTS.Dates = New atcData.atcTimeseries(Nothing)
            lTS.numValues = Values.Length
            Dim tu As atcUtility.atcTimeUnit = atcUtility.atcTimeUnit.TUHour
            With lTS.Attributes
                '.SetValue("Elevation", Elevation)
                .SetValue("Scenario", Scenario)
                .SetValue("Location", StationID)
                .SetValue("STAID", staid)
                .SetValue("StaNam", stanam)
                .SetValue("Description", StationName)
                .SetValue("Constituent", PCode)
                .SetValue("Latitude", lat)
                .SetValue("Longitude", lon)
                .SetValue("TSType", PCode) 'needed by some legacy apps
                .SetValue("Time Step", 1)
                If DateTimes.Length > 1 Then
                    Dim lDateSpan As TimeSpan = DateTimes(1).Subtract(DateTimes(0))
                    If lDateSpan.TotalSeconds = 1 Then
                        tu = atcUtility.atcTimeUnit.TUSecond
                    ElseIf lDateSpan.TotalMinutes = 1 Then
                        tu = atcUtility.atcTimeUnit.TUMinute
                    ElseIf lDateSpan.TotalHours = 1 Then
                        tu = atcUtility.atcTimeUnit.TUHour
                    ElseIf lDateSpan.TotalDays = 1 Then
                        tu = atcUtility.atcTimeUnit.TUDay
                    ElseIf lDateSpan.TotalDays = 30 Then
                        tu = atcUtility.atcTimeUnit.TUMonth
                    ElseIf lDateSpan.TotalDays = 365 Then
                        tu = atcUtility.atcTimeUnit.TUYear
                    ElseIf lDateSpan.TotalDays = 3650 Then
                        tu = atcUtility.atcTimeUnit.TUCentury
                    Else
                        tu = atcUtility.atcTimeUnit.TUUnknown
                        Throw New InvalidConstraintException(String.Format("WDM files are generally expected to contain constant increment time series; specifically, the first two observations must have a standard time step (second, hour, day, month, or year); when this is not true, the dataset cannot be saved to the WDM file. Please examine your data for Location='{0}' and Constituent='{1}'", StationID, PCode))
                    End If
                End If
                .SetValue("tu", tu)
                .SetValue("SJDay", DateTimes(0).ToOADate)
                .SetValue("EJDay", DateTimes(lTS.numValues - 1).ToOADate)
            End With

            lTS.Value(0) = atcUtility.GetNaN()
            For i As Integer = 1 To lTS.numValues
                'there is an apparent problem when subsequent identical dates are used; increment slightly
                If i > 1 AndAlso DateTimes(i - 1) <= DateTimes(i - 2) Then DateTimes(i - 1) = DateTimes(i - 2).AddMinutes(1)
                lTS.Dates.Value(i - 1) = DateTimes(i - 1).ToOADate
                lTS.Value(i) = Values(i - 1)
            Next

            lTS.Dates.Value(lTS.numValues) = atcUtility.TimAddJ(lTS.Dates.Value(lTS.numValues - 1), tu, 1, 1)

            Dim Wdm As New atcWDM.atcDataSourceWDM
            'for some reason, the first time a file is opened during a session, an error occurs
            Try
                Wdm.Open(WDMFile)
            Catch ex As Exception
            End Try

            If Wdm.Open(WDMFile) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And ds.Attributes.GetValue("Location", "") = StationID And ds.Attributes.GetValue("Constituent", "") = PCode Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next

                Dim dsn As Integer = 0
                dsn += 1
                lTS.Attributes.SetValue("ID", dsn)
                If Not Wdm.AddDataSet(lTS, atcData.atcDataSource.EnumExistAction.ExistRenumber) Then Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ID='{1}' and PCode='{2}'", WDMFile.Replace("\", "\\"), StationID, PCode))
            Else
                Throw New System.IO.IOException(String.Format("Unable to open WDM file: {0} while writing Station ID='{1}' and PCode='{2}'", WDMFile.Replace("\", "\\"), StationID, PCode))
            End If
            lTS = Nothing
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

    Private Sub PriestleyTaylorPETToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mnuCalcPriestly.Click
        MessageBox.Show("Priestly-Taylor routine not implemented yet!")
    End Sub

    Private Sub mnuCalculate_Click(sender As Object, e As EventArgs) Handles mnuCalculate.Click
        If dgvSelSeries.Rows.Count > 0 Then
            btnCalc.Enabled = True
            btnClearSelSta.Enabled = True
        Else
            btnCalc.Enabled = False
            btnClearSelSta.Enabled = False
        End If
    End Sub

    Private Sub mnuCalculate_MouseHover(sender As Object, e As EventArgs) Handles mnuCalculate.MouseHover
        If dgvSelSeries.Rows.Count > 0 Then
            btnCalc.Enabled = True
            btnClearSelSta.Enabled = True
        Else
            btnCalc.Enabled = False
            btnClearSelSta.Enabled = False
        End If
    End Sub

    Private Sub WriteStatus(msg As String)
        statuslbl.Text = msg
        statusStrip.Refresh()
    End Sub
#Region "Process Attributes"
    Private Sub mnuAddAttributes_Click(sender As Object, e As EventArgs) Handles mnuAddAttributes.Click

        Dim CoordFile As String = String.Empty

        With New OpenFileDialog
            .Filter = "Coordinates File (*.csv)|*.csv|All files (*.*)|*.*"
            .FilterIndex = 1
            .InitialDirectory = AppFolder
            .RestoreDirectory = vbTrue
            .ValidateNames = True
            .CheckFileExists = vbTrue
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                CoordFile = .FileName
            Else 'user cancels
                Return
            End If
            .Dispose()
        End With

        ProcessAttributesFile(CoordFile)

    End Sub

    Private Sub ProcessAttributesFile(ByVal CoordFile As String)
        Cursor.Current = Cursors.WaitCursor
        Dim lat As Double, lon As Double
        Dim aline() As String, sta As String
        Dim delim = New Char() {","}
        Dim lWdmDS As New atcWDM.atcDataSourceWDM
        Dim first As Boolean = True

        Try
            lWdmDS.Open(WDMFile)
        Catch ex As Exception
            WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

        Try
            For Each sline In File.ReadLines(CoordFile)
                If first Then 'skip line 1
                    first = False
                    Continue For
                End If
                aline = sline.Split(delim, StringSplitOptions.RemoveEmptyEntries)
                lon = CDbl(aline(0))
                lat = CDbl(aline(1))
                sta = aline(2)
                WriteStatus("Setting PREC attributes for station " + sta.ToString())
                AddAttributes(sta, lWdmDS, lon, lat)
            Next

        Catch ex As Exception
            MessageBox.Show("Error in reading station file!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace)
        End Try
        Cursor.Current = Cursors.Default
        WriteStatus("Ready ...")
        lWdmDS = Nothing
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs)
        wrlog.Flush()
        wrlog.Close()
        wrlog.Dispose()
    End Sub

    Private Sub frmWeaComp_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        wrlog.Flush()
        wrlog.Close()
        wrlog.Dispose()
    End Sub

    Private Sub frmWeaComp_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

    End Sub

    Private Sub ToolStripComboBox1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub mnuDisagSolar_Click(sender As Object, e As EventArgs) Handles mnuDisagSolar.Click

    End Sub

    Private Sub mnuDisagDew_Click(sender As Object, e As EventArgs) Handles mnuDisagDew.Click

    End Sub

    Private Sub mnuDisagRain_Click(sender As Object, e As EventArgs) Handles mnuDisagRain.Click

    End Sub

    Private Sub mnuDisagPET_Click(sender As Object, e As EventArgs) Handles mnuDisagPET.Click

    End Sub

    Private Sub mnuDisagTemp_Click(sender As Object, e As EventArgs) Handles mnuDisagTemp.Click

    End Sub

    Private Sub mnuDisagWind_Click(sender As Object, e As EventArgs) Handles mnuDisagWind.Click

    End Sub

    Private Function AddAttributes(ByVal loc As String, ByVal lWdmDS As atcWDM.atcDataSourceWDM,
                                   ByVal lon As Double, ByVal lat As Double) As Boolean
        Try
            Dim lseries As atcTimeseries = lWdmDS.DataSets.FindData("Location", loc).FindData("Constituent", "PREC")(0)
            If lseries = Nothing Then
                MessageBox.Show("WDM does not contains PREC for selected station " + loc.ToString())
                Return False
            Else
                lseries.Attributes.SetValue("Latitude", lat)
                lseries.Attributes.SetValue("Longitude", lon)
                lseries = Nothing
            End If
        Catch ex As Exception
            MessageBox.Show("Error in adding attributes to " + loc + vbCrLf + ex.Message + vbCrLf + ex.StackTrace)
            Return False
        End Try
        Return True
    End Function
#End Region

#Region "Old routines"
    Private Sub ShowWDMDataGridOld(ByVal aPcode As String)
        dgvWDM.Enabled = True

        wdmTable.Rows.Clear()
        wdmSelSeries.Rows.Clear()
        lstSelectedSeries.Clear()

        For Each kv As KeyValuePair(Of Integer, clsMetGages) In dictSeries
            Dim dsn As Integer = kv.Key
            Dim sta As New clsMetGages(0, "", "", "", "", "", "", "")
            dictSeries.TryGetValue(dsn, sta)

            Dim dtrow As DataRow = wdmTable.NewRow()
            dtrow("DSN") = dsn
            dtrow("Scenario") = sta.Scenario
            dtrow("Location") = sta.staID
            dtrow("Constituent") = sta.Pcode
            dtrow("Latitude") = sta.Lat
            dtrow("Longitude") = sta.Lon
            dtrow("StartDate") = Date.Now
            dtrow("EndDate") = Date.Now

            If sta.Pcode.Contains(aPcode) Then
                wdmTable.Rows.Add(dtrow)
            End If
            dtrow = Nothing
        Next
        'dictSeries = Nothing
        If wdmTable.Rows.Count > 0 Then
            dgvWDM.DataSource = Nothing
            dgvWDM.DataSource = wdmTable
            dgvWDM.ClearSelection()
        Else
            MessageBox.Show("There are no datasets for constituent " + aPcode + "!" + vbCrLf +
                            "Please import hourly NCEI data.")
        End If
    End Sub

    Private Function GetBaseDSN(ByVal ilower As Integer, iupper As Integer) As Integer
        ' Initialize the random-number generator.
        Randomize()
        ' Generate random value between 1 and 6.
        Dim bdsn As Integer = CInt(Math.Floor((iupper - ilower + 1) * Rnd())) + ilower
        Debug.WriteLine("Base dsn =" + bdsn.ToString())
        Return bdsn
    End Function

    'Dim lWdmDataSource As New atcWDM.atcDataSourceWDM
    'Try
    'If atcDataManager.OpenDataSource(lWdmFileName) Then
    'lWdmDataSource = atcDataManager.DataSourceBySpecification(lWdmFileName)
    'End If
    'lWdmDataSource.Open(lWdmFileName)
    'Catch ex As Exception
    'MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
    ''Return False
    'End Try
    'calculate PET if not there, else ignore, 
    'Try
    'Dim lPET As atcTimeseries = lWdmDataSource.DataSets.FindData("Location", GridLocation).FindData("Constituent", "HPET")(0)
    'If Not lPET = Nothing Then
    'WriteLog("WDM already contains PEVT for selected station " + GridLocation.ToString())
    'Return False
    'End If
    'Catch ex As Exception
    'End Try

#End Region

    'debug,   get dates and values of timeseries
    'this works for ncei hourly imported
    'Dim dt() As Double = lHamonCmTS.Dates.Values()
    'Dim values() As Double = lHamonCmTS.Values()
    'Dim ddate(dt.Count) As Date
    'For j As Integer = 0 To dt.Count - 1
    'ddate(j) = DateTime.FromOADate(dt(j))
    'Debug.WriteLine(ddate(j).ToString() + ", " + values(j).ToString())
    'Next
    'wdm.Write("COMPUTED", GridLocation, GridLocation, "PEVT", ddate, values)
    'WriteLog("Computed Hamon PET for selected station " + GridLocation.ToString() + ", dsn " + lCounter.ToString())

End Class
