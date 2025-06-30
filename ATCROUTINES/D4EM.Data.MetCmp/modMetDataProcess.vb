Imports Microsoft.VisualBasic
Imports System.Collections
Imports System.Collections.Specialized
Imports System.IO
Imports System.Math
Imports System.Text
Imports MapWinUtility
Imports MapWinUtility.Strings
Imports atcUtility
Imports atcData
Imports atcWDM

Public Module modMetDataProcess

    Private Const pEpsilon As Double = 0.000000001
    Private pMaxValue As Double = GetMaxValue()

    Public Sub MissingSummary(ByVal aTsGroup As atcTimeseriesGroup, ByVal aPath As String, Optional ByVal aSummaryFilename As String = "")
        Logger.Dbg("MissingSummary:Start")
        ChDriveDir(aPath)
        Logger.Dbg(" CurDir:" & CurDir())

        Dim lts As atcTimeseries
        Dim lCons As String = ""

        Dim lStr As New StringBuilder
        Dim lFileStr As String = ""

        Dim lMVal As Double = -999.0 '<--Global parameter?
        Dim lMAcc As Double = -998.0 '<--Global parameter?
        Dim lFMin As Double = -100.0 '<--Global parameter?
        Dim lFMax As Double = 10000.0 '<--Global parameter?
        Dim lRepType As Integer = 1 'DBF parsing output format <--This could be changed
        Dim lDBF As atcTableDBF = Nothing
        Dim lStatePath As String = ""
        Dim lCurPath As String = ""
        Dim lOneLine As String = ""

        'Define where to put the summary dbf file
        'Dim pStationPath As String = "C:\BasinsMet\Stations\" '<--Set dynamically, such as the D4EM project folder?
        'Dim lFName As String = pStationPath & "MissingSummary.dbf"
        Dim lFName As String = Path.Combine(aPath, "MissingSummary.dbf")
        If aSummaryFilename <> "" Then
            lFName = Path.Combine(aPath, aSummaryFilename & ".dbf")
        End If

        For Each lts In aTsGroup
            lCons = lts.Attributes.GetValue("Constituent")
            Dim lLocation As String = lts.Attributes.GetValue("Location")
            Logger.Dbg("MissingSummary: processing dataset - " & lCons & " at " & lLocation)

            If Not lCons.Contains("-OBS") Then
                Logger.Dbg("MissSummary:Summarizing station DSN " & lts.Attributes.GetValue("ID") &
                           " - Constituent: " & lCons)
                Try
                    If lts.numValues = 0 OrElse
                       Double.IsNaN(lts.Dates.Value(0)) OrElse
                       Double.IsNaN(lts.Dates.Value(lts.numValues)) Then
                        Logger.Dbg("MissSummary:Summarizing station DSN " & lts.Attributes.GetValue("ID") &
                                   " - Constituent: " & lCons & " failed due to bad dates or no data")
                    Else
                        lOneLine = MissingDataSummary(lts, lMVal, lMAcc, lFMin, lFMax, lRepType)
                        lStr.Append(lOneLine)
                        'saving %missing as an attribute
                        lts.Attributes.SetValue("UBC200", CDbl(lOneLine.Substring(lOneLine.LastIndexOf(",") + 1)))
                    End If

                Catch ex As Exception
                    Logger.Dbg("MissSummary:Summarizing station DSN " & lts.Attributes.GetValue("ID") &
                               " - Constituent: " & lCons & " failed due to bad dates or no data")
                End Try
            End If
        Next

        If lRepType = 0 Then 'send output to text file
            lFileStr &= lStr.ToString()
        Else 'send output to DBF file
            If lDBF Is Nothing Then 'create DBF file
                lDBF = BuildDBFSummary(lStr.ToString, lFName)
                If lDBF Is Nothing Then
                    Logger.Dbg("MissingSummary: Couldn't create DBF file")
                End If
            End If
            Add2DBFSummary(lDBF, lStr.ToString)
        End If

        Logger.Dbg("MissingSummary:Completed Summaries")
        If lRepType = 0 Then
            SaveFileString(FilenameNoExt(lFName) & ".txt", lFileStr)
            Logger.Dbg("MissingSummary:Wrote Text output file")
        ElseIf lRepType = 1 Then
            lDBF.WriteFile(lFName)
            Logger.Dbg("MissingSummary:Wrote DBF File" & lFName)
        End If

    End Sub

    Private Function BuildDBFSummary(ByVal aSummStr As String, ByVal aFName As String) As atcTableDBF
        'This function is used by MissingSummary
        Dim i As Integer ', j As Long, TSCount As Long, 
        Dim lFldLen As Integer
        'Dim tmpv As Long, maxv As Long, 
        Dim lTmpStr As String
        Dim lStr As String
        Dim lFldNames As New ArrayList
        Dim lFldTypes As New ArrayList
        Dim lFirstVals As New ArrayList
        Dim lDBF As New atcTableDBF

        Logger.Dbg("BuildDBFSummary:Start")
        lFldLen = 6
        'build dbf file
        'extract field names, first from Station record
        lTmpStr = StrSplit(aSummStr, "STATION:HEADER", "")
        lTmpStr = StrSplit(aSummStr, vbCrLf, "")
        While Len(lTmpStr) > 0
            lFldNames.Add(StrSplit(lTmpStr, ",", ""))
        End While
        Logger.Dbg("BuildDBFSummary:Extracted field names from Station record")

        lTmpStr = StrSplit(aSummStr, "STATION:DATA", "")
        lTmpStr = StrSplit(aSummStr, vbCrLf, "")
        While Len(lTmpStr) > 0
            lStr = StrSplit(lTmpStr, ",", "")
            If lStr.Length > 1 And lStr.StartsWith("0") Then  'preceeding 0 usually means character field
                lFldTypes.Add("C")
            ElseIf Not IsNumeric(lStr) Then
                lFldTypes.Add("C")
            Else
                lFldTypes.Add("N")
            End If
            lFirstVals.Add(lStr)
        End While
        Logger.Dbg("BuildDBFSummary:Extracted field types from Station record")

        'now from Summary record
        lTmpStr = StrSplit(aSummStr, "SUMMARY:HEADER", "")
        lTmpStr = Trim(StrSplit(aSummStr, vbCrLf, ""))
        While Len(lTmpStr) > 0
            lFldNames.Add(StrSplit(lTmpStr, ",", ""))
        End While
        Logger.Dbg("BuildDBFSummary:Extracted field names from Summary record")

        lTmpStr = StrSplit(aSummStr, "SUMMARY:DATA", "")
        lTmpStr = StrSplit(aSummStr, vbCrLf, "")
        While Len(lTmpStr) > 0
            lStr = StrSplit(lTmpStr, ",", "")
            If lStr.IndexOf("/") > 0 Then 'date field
                lFldTypes.Add("D")
            ElseIf lStr.Length > 1 And lStr.StartsWith("0") Then  'preceeding 0 usually means character field
                lFldTypes.Add("C")
            ElseIf Not IsNumeric(lStr) Then
                lFldTypes.Add("C")
            Else
                lFldTypes.Add("N")
            End If
            lFirstVals.Add(lStr)
        End While
        Logger.Dbg("BuildDBFSummary:Extracted field types names from Summary record")
        lDBF.NumFields = lFldNames.Count
        Logger.Dbg("BuildDBFSummary:Set number of fields")

        For i = 0 To lDBF.NumFields - 1
            lDBF.FieldType(i + 1) = lFldTypes(i)
            lDBF.FieldName(i + 1) = lFldNames(i)
            If lFldTypes(i) = "N" Then
                lDBF.FieldLength(i + 1) = lFldLen 'use length of max number for numeric fields
            ElseIf lFldTypes(i) = "D" Then
                lDBF.FieldLength(i + 1) = 10
            Else
                If lFldNames(i) = "Description" Then
                    lDBF.FieldLength(i + 1) = 32
                Else 'all other character fields use width of 8
                    lDBF.FieldLength(i + 1) = 8
                End If
            End If
            If i < lDBF.NumFields Then
                lDBF.FieldDecimalCount(i + 1) = 0
            Else
                lDBF.FieldDecimalCount(i + 1) = 1 'last field (percent missing) needs a decimal
            End If
        Next i
        Logger.Dbg("BuildDBFSummary:Set DBF Field info")

        lDBF.InitData()
        lDBF.CurrentRecord = 1
        Logger.Dbg("BuildDBFSummary:Initialized data for DBF")
        lFldNames.Clear()
        lFldNames = Nothing
        lFldTypes.Clear()
        lFldTypes = Nothing
        lFirstVals.Clear()
        lFirstVals = Nothing
        Return lDBF
        'Catch ex As Exception
        '    Logger.Dbg("BuildDBFSummary: PROBLEM creating DBF file " & aFName & vbCrLf & ex.ToString)
        '    Return Nothing
        'End Try

    End Function

    Private Sub Add2DBFSummary(ByRef aDBF As atcTableDBF, ByVal aSummStr As String)
        'This function is used by MissingSummary
        Dim i As Integer
        Dim lTmpStr As String

        'For i = 0 To aDBF.NumFields - 1
        '    aDBF.Value(i + 1) = lFirstVals.ItemByIndex(i)
        'Next i
        'Logger.Dbg("BuildDBFSummary:Set first record values")

        While Len(aSummStr) > 0
            aDBF.CurrentRecord = aDBF.CurrentRecord + 1
            lTmpStr = StrSplit(aSummStr, "STATION:DATA", "")
            lTmpStr = StrSplit(aSummStr, vbCrLf, "")
            i = 0
            While Len(lTmpStr) > 0
                i += 1
                aDBF.Value(i) = StrSplit(lTmpStr, ",", "")
            End While
            lTmpStr = StrSplit(aSummStr, "SUMMARY:DATA", "")
            lTmpStr = StrSplit(aSummStr, vbCrLf, "")
            While Len(lTmpStr) > 0
                i += 1
                aDBF.Value(i) = StrSplit(lTmpStr, ",", "")
            End While
        End While
    End Sub

    Public Sub FirstFilter(ByVal aTsGroupInput As atcTimeseriesGroup, ByVal aTsGroupOutput As atcTimeseriesGroup, ByVal aInfoPath As String)
        'This routine will go through all data in input group and put into output group
        'write filled data in output path, so the original data are not changed

        Dim lts As atcTimeseries

        Dim lStr As String = ""
        Dim lFileStr As String = ""

        Dim lMVal As Double = -999.0
        Dim lMAcc As Double = -998.0
        Dim lFMin As Double = -100.0
        Dim lFMax As Double = 10000.0
        Dim lRepType As Integer = 1
        Dim lSkipped As Integer = 0

        Dim lMissDBF As New atcTableDBF
        'Dim lSODStaDBF As New atcTableDBF
        'Dim lHPDStaDBF As New atcTableDBF
        'Dim lISHStaDBF As New atcTableDBF
        Dim lStation As String = ""
        Dim lStatePath As String = ""
        Dim lCurPath As String = ""
        Dim lCons As String = ""
        Dim lAddMe As Boolean = True
        Dim lStaCount As Integer = 0

        If lMissDBF.OpenFile(Path.Combine(aInfoPath, "MissingSummary.dbf")) Then
            Logger.Dbg("FirstFilter: Opened Missing Summary file " & aInfoPath & "MissingSummary.dbf")
        Else
            Logger.Dbg("FirstFilter: PROBLEM Opening Summary file " & aInfoPath & "MissingSummary.dbf")
        End If

        'If lSODStaDBF.OpenFile(aInfoPath & "coop_Summ.dbf") Then
        '    Logger.Dbg("FirstFilter: Opened SOD Station Summary file " & aInfoPath & "coop_Summ.dbf")
        'Else
        '    Logger.Dbg("FirstFilter: PROBLEM Opening SOD Station Summary file " & aInfoPath & "coop_Summ.dbf")
        'End If

        'If lHPDStaDBF.OpenFile(aInfoPath & "HPD_Stations.dbf") Then
        '    Logger.Dbg("FirstFilter: Opened HPD Station Summary file " & aInfoPath & "HPD_Stations.dbf")
        'Else
        '    Logger.Dbg("FirstFilter: PROBLEM Opening HPD Station Summary file " & aInfoPath & "HPD_Stations.dbf")
        'End If

        'If lISHStaDBF.OpenFile(aInfoPath & "ISH_Stations.dbf") Then
        '    Logger.Dbg("FirstFilter: Opened ISH Station Summary file " & aInfoPath & "ISH_Stations.dbf")
        'Else
        '    Logger.Dbg("FirstFilter: PROBLEM Opening ISH Station Summary file " & aInfoPath & "ISH_Stations.dbf")
        'End If

        For Each lts In aTsGroupInput
            lStation = lts.Attributes.GetValue("STAID1") 'eg 720355
            lCons = lts.Attributes.GetValue("Constituent")
            Logger.Dbg("FirstFilter: For Station - " & lStation & " for constituent - " & lCons)

            lMissDBF.FindFirst(3, lStation)
            'lts.EnsureValuesRead()

            If Not lCons.EndsWith("-OBS") Then
                'find missing data summary record for this station/constituent
                While lCons <> lMissDBF.Value(4)
                    If Not lMissDBF.FindNext(3, lStation) Then
                        Logger.Dbg("FirstFilter: PROBLEM - Could not find constituent " & lCons & " for station " & lStation)
                        Exit While
                    End If
                End While
            End If
            Select Case lCons
                Case "TMIN", "TMAX", "PRCP", "EVAP", "WDMV", "HPCP",
                     "WIND", "ATEMP", "DPTEMP", "HPCP1", "HPCP1-TM",
                     "SKY-SST1", "SKY-SUM1", "SKYCOND"
                    lts.Attributes.SetValue("UBC200", lMissDBF.Value(19)) 'store %missing as attribute
                    lAddMe = True
                Case "TMIN-OBS", "TMAX-OBS", "PRCP-OBS", "EVAP-OBS", "WDMV-OBS" 'save Obs Time timeseries for daily data
                    lAddMe = True
                Case Else
                    Logger.Dbg("FirstFilter: Removed constituent " & lCons)
                    lAddMe = False
            End Select
            If lAddMe Then
                aTsGroupOutput.Add(lts)
            End If
        Next
        If aTsGroupOutput.Count > 0 Then
            lStaCount += 1
            Logger.Dbg("FirstFilter: Filtered " & aTsGroupOutput.Count & " datasets.")
        Else
            Logger.Dbg("FirstFilter: No Datasets are filtered.")
            lSkipped += 1
        End If
        Logger.Dbg("FirstFilter: Saved " & lStaCount & " datasets." & vbCrLf &
                   "             Removed " & lSkipped & " datasets.")
        Logger.Dbg("FirstFilter: Completed Filtering")
    End Sub

    Public Sub StorePRISM(ByVal aPRISMFilePath As String, ByRef aTsGroup As atcTimeseriesGroup)

        Logger.Dbg("StorePRISM:Start")
        'ChDriveDir(aPRISMFilePath)
        'Logger.Dbg(" CurDir:" & CurDir())

        Dim lStation As String = ""
        Dim lCons As String = ""
        Dim lVal As Double = 0.0
        Dim lStaCnt As Integer = 0

        Dim lPrismPrecip As New atcTableDBF
        Dim lPrismTMin As New atcTableDBF
        Dim lPrismTMax As New atcTableDBF

        If lPrismPrecip.OpenFile(IO.Path.Combine(aPRISMFilePath, "us_ppt_ann.dbf")) Then
            Logger.Dbg("StorePRISM: Opened PRISM Precip file " & lPrismPrecip.FileName)
        Else
            Throw New ApplicationException("StorePRISM: Failed to open PRISM Precip file " & IO.Path.Combine(aPRISMFilePath, "us_ppt_ann.dbf"))
        End If
        If lPrismTMin.OpenFile(IO.Path.Combine(aPRISMFilePath, "us_tmin_ann.dbf")) Then
            Logger.Dbg("StorePRISM: Opened PRISM TMin file " & lPrismTMin.FileName)
        Else
            Throw New ApplicationException("StorePRISM: Failed to open PRISM TMin file " & IO.Path.Combine(aPRISMFilePath, "us_tmin_ann.dbf"))
        End If
        If lPrismTMax.OpenFile(IO.Path.Combine(aPRISMFilePath, "us_tmax_ann.dbf")) Then
            Logger.Dbg("StorePRISM: Opened PRISM TMax file " & lPrismTMax.FileName)
        Else
            Throw New ApplicationException("StorePRISM: Failed to open PRISM TMax file " & IO.Path.Combine(aPRISMFilePath, "us_tmax_ann.dbf"))
        End If

        'Logger.Dbg("StorePRISM: Get all files in data directory " & aPRISMFilePath)
        For Each lDS As atcDataSet In aTsGroup
            'lStation = lDS.Attributes.GetValue("STAID1")
            lStation = lDS.Attributes.GetValue("Location").ToString
            If lStation.Length > 6 Then lStation = lStation.Substring(0, 6)
            lCons = lDS.Attributes.GetValue("Constituent")

            Select Case lCons
                Case "PRCP", "PREC", "HPCP", "HPCP1"
                    If lPrismPrecip.FindFirst(1, lStation) Then
                        lVal = CDbl(lPrismPrecip.Value(6))
                        If lVal > 0 AndAlso lVal < 200 Then
                            lDS.Attributes.SetValue("PRECIP", lVal)
                        Else
                            Logger.Dbg("StorePRISM: No valid value found on PRISM Precip DBF file - no attribute stored")
                        End If
                    Else
                        Logger.Dbg("StorePRISM: could not find station " & lStation & " in PRISM Precip DBF file")
                    End If
                Case "TMIN", "TMAX", "DPTP", "ATEMP", "DPTEMP", "ATEM", "DEWP"
                    If lPrismTMin.FindFirst(1, lStation) AndAlso lPrismTMax.FindFirst(1, lStation) Then
                        lVal = (CDbl(lPrismTMin.Value(6)) + CDbl(lPrismTMax.Value(6))) / 2
                        If lVal > 0 AndAlso lVal < 200 Then
                            lDS.Attributes.SetValue("UBC190", lVal)
                        Else
                            Logger.Dbg("StorePRISM: No valid value found on PRISM TMin/TMax DBF file - no attribute stored")
                        End If
                    Else
                        Logger.Dbg("StorePRISM: could not find station " & lStation & " on PRISM TMin/TMax DBF file")
                    End If
            End Select
        Next
        'lPrismPrecip.Clear()
        'lPrismTMin.Clear()
        'lPrismTMax.Clear()

        'lPrismPrecip = Nothing
        'lPrismTMin = Nothing
        'lPrismTMax = Nothing

        'Logger.Dbg("StorePRISM: Reviewed " & lStaCnt & " stations")
        Logger.Dbg("StorePRISM:Completed PRISM Storing")
    End Sub

    Public Sub ShiftISH(ByRef aTsGroup As atcTimeseriesGroup, ByVal aPath As String)
        Logger.Dbg("ShiftISH:Start")

        Dim lTS As atcTimeseries
        Dim lNewTS As atcDataSet = Nothing

        Dim lStation As String = ""
        Dim lState As String = ""
        Dim lCurPath As String = ""
        Dim lCons As String = ""
        Dim lAddMe As Boolean = True
        Dim lCnt As Integer = 0

        Dim lNewTsGroup As New atcTimeseriesGroup
        For Each lTS In aTsGroup
            lCnt += 1

            lCons = lTS.Attributes.GetValue("Constituent")
            lState = lTS.Attributes.GetValue("State") 'should be two-char state name abbrev, eg md <--Need to set this attribute right after download
            'lStation = lTS.Attributes.GetValue("STAID1") 'should be the station id, eg 720355 <--Need to set this attribute right after download
            lStation = lTS.Attributes.GetValue("Location").ToString
            If lStation.Length > 6 Then lStation = lStation.Substring(0, 6)

            Logger.Dbg("ShiftISH: For Station - " & lStation & " for constituent - " & lCons)
            Dim lOffset As Integer = 0
            If lOffset = 0 Then
                lOffset = DetermineTimeOffset(lTS)
                Logger.Dbg("ShiftISH: Time Offset set to " & lOffset)
            End If
            lNewTS = ShiftDates(lTS, atcTimeUnit.TUHour, -lOffset)

            'lNewTS.Attributes.SetValue("SJDay", CType(lNewTS, atcTimeseries).Dates.Value(0))
            'lNewTS.Attributes.SetValue("EJDay", CType(lNewTS, atcTimeseries).Dates.Value(CType(lNewTS, atcTimeseries).numValues))

            'End If
            lNewTsGroup.Add(lNewTS)
        Next

        aTsGroup.Clear()
        aTsGroup.AddRange(lNewTsGroup)

        Logger.Dbg("ShiftISH:Completed Shifting - processed " & lCnt & " ISH Stations")
    End Sub

    Private Function DetermineTimeOffset(ByVal ats As atcTimeseries) As Integer

        Dim lESTStates As String = "ME,VT,NH,MA,RI,CT,NY,NJ,PA,MD,DE,VA,WV,OH,NC,SC,GA"
        Dim lCSTStates As String = "MN,WI,IA,IL,MO,AR,OK,LA,MS,AL"
        Dim lMSTStates As String = "MT,WY,UT,CO,AZ,NM"
        Dim lPSTStates As String = "WA,NV,CA"

        Dim lState As String = ats.Attributes.GetValue("State", "KS")

        If lESTStates.Contains(lState.ToUpper) Then
            Logger.Dbg("Time Offset - in Eastern States")
            Return 5
        ElseIf lCSTStates.Contains(lState.ToUpper) Then
            Logger.Dbg("Time Offset - in Central States")
            Return 6
        ElseIf lMSTStates.Contains(lState.ToUpper) Then
            Logger.Dbg("Time Offset - in Mountain States")
            Return 7
        ElseIf lPSTStates.Contains(lState.ToUpper) Then
            Logger.Dbg("Time Offset - in Pacific States")
            Return 8
        ElseIf lState.ToUpper = "AK" Then
            Logger.Dbg("Time Offset - in Alaska")
            Return 9
        ElseIf lState.ToUpper = "HI" Then
            Logger.Dbg("Time Offset - in Hawaii")
            Return 10
        ElseIf lState.ToUpper = "PR" Then
            Logger.Dbg("Time Offset - in Puerto Rico")
            Return 4
        End If
        Logger.Dbg("Time Offset - used Lat/Lng rules")
        Dim lLng As Double = ats.Attributes.GetValue("LNGDEG")
        Dim lLat As Double = ats.Attributes.GetValue("LATDEG")
        Select Case lState.ToUpper
            Case "FL"
                If lLng < -85.0 Then
                    Return 6
                Else
                    Return 5
                End If
            Case "TN"
                If lLat > 36 Then 'line is further East in northern portion of state
                    If lLng < -84.75 Then
                        Return 6
                    Else
                        Return 5
                    End If
                Else
                    If lLng < -85.3 Then
                        Return 6
                    Else
                        Return 5
                    End If
                End If
            Case "KY"
                If lLat > 37.25 Then 'further West in Northern portion of state
                    If lLng < -86 Then
                        Return 6
                    Else
                        Return 5
                    End If
                Else
                    If lLng < -85 Then
                        Return 6
                    Else
                        Return 5
                    End If
                End If
            Case "IN"
                If lLat > 41 Then 'Northwest corner of state is central
                    If lLng < -86.75 Then
                        Return 6
                    Else
                        Return 5
                    End If
                ElseIf lLat < 38.25 Then
                    If lLng < -86.75 Then 'Southwest corner of state is central
                        Return 6
                    Else
                        Return 5
                    End If
                Else 'everything else is Eastern
                    Return 5
                End If
            Case "MI"
                If lLng < -87.5 AndAlso lLat < 46.4 Then 'SW part of upper penn. in Central
                    Return 6
                Else
                    Return 5
                End If
            Case "TX"
                If lLat < -104.85 Then 'far West corner is in Mountain
                    Return 7
                Else
                    Return 6
                End If
            Case "KS"
                '2 western sections in Mountain
                If lLat > 37.75 AndAlso lLat < 38.25 AndAlso lLng < -101.1 Then
                    Return 7
                ElseIf lLat >= 38.25 AndAlso lLat < 39.6 AndAlso lLng < -101.5 Then
                    Return 7
                Else 'rest is in central
                    Return 6
                End If
            Case "NE"
                If lLng < -101 Then
                    Return 7
                Else
                    Return 6
                End If
            Case "SD"
                If lLng < -100.7 Then
                    Return 7
                Else
                    Return 6
                End If
            Case "ND"
                If lLng < -101 AndAlso lLat < 47.5 Then 'Southwest corner is in Mountain
                    Return 7
                Else
                    Return 6
                End If
            Case "ID"
                If lLat > 45.5 Then 'northern portion in pacific
                    Return 8
                Else
                    Return 7
                End If
            Case "OR"
                If lLng > -118.2 AndAlso lLat > 42.45 AndAlso lLat < 44.5 Then
                    'small Eastern section in Mountain
                    Return 7
                Else 'rest in pacific
                    Return 8
                End If
        End Select
        Throw New ApplicationException("Could not determine time offset for " & ats.Attributes.GetValue("STAID", "station") & " in " & lState)
    End Function

    Public Function ShiftDates(ByVal aTSer As atcTimeseries, ByVal aTU As modDate.atcTimeUnit, ByVal aShift As Integer) As atcDataSet
        Dim lTSer As atcTimeseries = aTSer.Clone
        Dim lShiftInc As Double = 0
        Select Case aTU
            Case atcTimeUnit.TUSecond
                lShiftInc = aShift * modDate.JulianSecond
            Case atcTimeUnit.TUMinute
                lShiftInc = aShift * modDate.JulianMinute
            Case atcTimeUnit.TUHour
                lShiftInc = aShift * modDate.JulianHour
            Case atcTimeUnit.TUDay
                lShiftInc = aShift
            Case atcTimeUnit.TUMonth
                lShiftInc = aShift * modDate.JulianMonth
            Case atcTimeUnit.TUYear
                lShiftInc = aShift * modDate.JulianYear
        End Select
        For i As Integer = 0 To lTSer.numValues
            lTSer.Dates.Value(i) += lShiftInc
        Next
        Return lTSer
    End Function

    Public Function FillMissing(ByRef aTsGroup As atcTimeseriesGroup) As atcTimeseries
        'Some parameters
        Dim pMaxNearStas As Integer = 30
        Dim pMaxFillLength As Integer = 11 'any span < max time shift (10 hrs for HI)
        Dim pMinNumHrly As Integer = 43830 '5 years of hourly values
        Dim pMinNumDly As Integer = 1830 '5 years of daily
        Dim pMaxPctMiss As Integer = 35 '20

        Dim lAddMe As Boolean = True
        Dim lInterpolate As Boolean
        Dim i As Integer
        Dim lStr As String
        Dim lMVal As Double = -999.0
        Dim lMAcc As Double = -998.0
        Dim lFMin As Double = -100.0
        Dim lFMax As Double = 10000.0
        Dim lRepType As Integer = 1 'DBF parsing output format in MissingDataReport

        Dim lFillTS As atcTimeseries = Nothing
        Dim lFileName As String = ""
        Dim lFilledTS As atcTimeseries = Nothing
        Dim lFillers As atcCollection = Nothing
        Dim lFillerOTs As atcCollection = Nothing

        'Dim X1 As Double
        'Dim Y1 As Double
        Dim lStationCount As Integer = aTsGroup.Count
        Dim X2(lStationCount) As Double
        Dim Y2(lStationCount) As Double
        Dim lDist(lStationCount) As Double
        Dim lPos(lStationCount) As Integer
        Dim lRank(lStationCount) As Integer

        For Each lts As atcTimeseries In aTsGroup
            If Not lts.Attributes.GetValue("NeedToFill") Then Continue For
            lAddMe = False
            lInterpolate = False
            Dim lCons As String = lts.Attributes.GetValue("Constituent")
            Dim lPctMiss As Double = lts.Attributes.GetValue("UBC200")
            Select Case lCons 'look for wanted constituents and check % missing
                Case "HPCP", "HPCP1", "TMIN", "TMAX", "PRCP", "PREC"
                    If lPctMiss < pMaxPctMiss Then '% missing OK
                        'If (lts.Attributes.GetValue("tu") = atcTimeUnit.TUHour AndAlso lts.numValues > pMinNumHrly) OrElse _
                        '   (lts.Attributes.GetValue("tu") = atcTimeUnit.TUDay AndAlso lts.numValues > pMinNumDly) Then 'want a significant time span
                        '    ExtendISHTSer(lts)
                        '    Logger.Dbg("FillMissing:  Filling data for " & lts.ToString & ", " & lts.Attributes.GetValue("Description"))
                        '    lAddMe = True
                        'Else
                        '    Logger.Dbg("FillMissing:  Not enough values (" & lts.numValues & ") for " & lts.ToString & _
                        '               " - need at least " & pMinNumHrly)
                        'End If

                        ExtendISHTSer(lts)
                        Logger.Dbg("FillMissing:  Filling data for " & lts.ToString & ", " & lts.Attributes.GetValue("Description"))
                        lAddMe = True

                    ElseIf lPctMiss < pMaxPctMiss + 10 Then 'try to find recent subset with < max % missing
                        Logger.Dbg("FillMissing:  For " & lts.ToString & ", percent Missing (" & lPctMiss & ") > " & pMaxPctMiss & " - try subset of most recent data")
                        Dim lEJDay As Double = lts.Attributes.GetValue("EJDay")
                        Dim lSJDay As Double
                        Dim lSubTS As atcTimeseries
                        i = 0
                        Do 'back up in 5 year increments while %missing is low enough
                            i += 1
                            lSJDay = lEJDay - System.Math.Round(5 * i * JulianYear)
                            lSubTS = SubsetByDate(lts, lSJDay, lEJDay, Nothing)
                            lStr = MissingDataSummary(lSubTS, lMVal, lMAcc, lFMin, lFMax, lRepType)
                            lPctMiss = CDbl(lStr.Substring(lStr.LastIndexOf(",") + 1))
                            lSubTS = Nothing
                        Loop While lPctMiss < pMaxPctMiss
                        If i > 1 Then
                            lAddMe = True
                            Logger.Dbg("FillMissing:  Filling data for " & lts.ToString & ", " & lts.Attributes.GetValue("Description"))
                            lSJDay = lEJDay - System.Math.Round(5 * (i - 1) * JulianYear)
                            lSubTS = SubsetByDate(lts, lSJDay, lEJDay, Nothing)
                            lStr = MissingDataSummary(lSubTS, lMVal, lMAcc, lFMin, lFMax, lRepType)
                            lPctMiss = CDbl(lStr.Substring(lStr.LastIndexOf(",") + 1))
                            lts = Nothing
                            lts = lSubTS
                            lSubTS = Nothing
                            Logger.Dbg("FillMissing:  Using time subset of " & DumpDate(lSJDay) & " to " & DumpDate(lEJDay))
                        Else
                            Logger.Dbg("FillMissing:  Subset percent missing (" & lPctMiss & ") still too large")
                        End If
                    Else
                        Logger.Dbg("FillMissing:  For " & lts.ToString & ", percent Missing (" & lPctMiss & ") too large (> " & pMaxPctMiss & ")")
                    End If
                Case "ATEMP", "DPTEMP", "WIND", "CLOU", "ATEM", "DEWP"
                    If lPctMiss < 20 Then '% missing OK
                        'If lts.numValues > pMinNumHrly Then 'want a significant time span
                        'Else
                        '    Logger.Dbg("FillMissing:  Not enough values (" & lts.numValues & ") for " & lts.ToString & _
                        '               " - need at least " & pMinNumHrly)
                        'End If

                        Logger.Dbg("FillMissing:  Filling data for " & lts.ToString & ", " & lts.Attributes.GetValue("Description"))
                        lAddMe = True
                        'extend TSer to end of last day (from ISH data being time shifted)
                        ExtendISHTSer(lts)
                        Logger.Dbg("FillMissing:  Before Interpolation, % Missing:  " & lPctMiss)
                        Logger.Dbg("FillMissing:  Max span to interpolate is " & pMaxFillLength & " hours")
                        'try interpolation for these hourly constituents
                        Dim lInterpTS As atcTimeseries = FillMissingByInterpolation(lts, (CDbl(pMaxFillLength) + 0.001) / 24)
                        If Not lInterpTS Is Nothing Then
                            lts = lInterpTS
                            lInterpTS = Nothing
                            lStr = MissingDataSummary(lts, lMVal, lMAcc, lFMin, lFMax, lRepType)
                            lPctMiss = CDbl(lStr.Substring(lStr.LastIndexOf(",") + 1))
                            Logger.Dbg("FillMissing:  After Interpolation, % Missing:  " & lPctMiss)
                        Else
                            Logger.Dbg("FillMissing:  PROBLEM with Interpolation")
                        End If

                    Else
                        Logger.Dbg("FillMissing:  For " & lts.ToString & ", percent Missing (" & lPctMiss & ") too large (> " & pMaxPctMiss & ")")
                    End If
                Case Else
                    Logger.Dbg("FillMissing:  Not processing constituent for " & lts.ToString)
            End Select
            If lAddMe Then
                If lPctMiss > 0 Then
                    'If X1 < Double.Epsilon AndAlso Y1 < Double.Epsilon Then 'determine nearest geographic stations
                    '    X1 = lts.Attributes.GetValue("Longitude")
                    '    Y1 = lts.Attributes.GetValue("Latitude")
                    '    Logger.Dbg("FillMissing: For Station " & lts.Attributes.GetValue("ID") & ", " & lts.Attributes.GetValue("STANAM") & "  at Lat/Lng " & Y1 & " / " & X1)
                    '    Dim lDistList As atcCollection = FindClosestStation2(aTsGroup, lts.Attributes.GetValue("ID"))
                    '    For i = 1 To lStationCount
                    '        'lDist(i) = System.Math.Sqrt((X1 - X2(i)) ^ 2 + (Y1 - Y2(i)) ^ 2)
                    '        lDist(i) = lDistList.Keys.Item(i - 1)
                    '    Next
                    '    SortRealArray(0, lStationCount, lDist, lPos)
                    '    'SortIntegerArray(0, lStationDBF.NumRecords, lPos, lRank)
                    '    Logger.Dbg("FillMissing: Sorted stations by distance")
                    'End If
                    Logger.Dbg("FillMissing:    Nearby Stations:")
                    lFillers = New atcCollection
                    lFillerOTs = New atcCollection
                    i = 0 'not using the zero position
                    If lCons = "PRCP" Then 'see if HPCP exists on same WDM
                        For Each llts As atcTimeseries In aTsGroup
                            If llts.Attributes.GetValue("Constituent") = "HPCP" Then
                                lFillers.Add(0, llts)
                                Logger.Dbg("FillMissing:  Using " &
                                           llts.Attributes.GetValue("Constituent") & " from " &
                                           llts.Attributes.GetValue("Location") & " " &
                                           llts.Attributes.GetValue("STANAM") & " at Lat/Lng " &
                                           llts.Attributes.GetValue("LATDEG") & "/" & llts.Attributes.GetValue("LNGDEG"))

                            End If
                        Next
                    End If

                    Dim lDistList As atcCollection = FindClosestStation2(aTsGroup, lts.Attributes.GetValue("D4EMIDX"))
                    For i = 1 To lDistList.Count - 1 'zero position is itself
                        If i < pMaxNearStas Then
                            lFillTS = aTsGroup.FindData("D4EMIDX", lDistList(i))(0)
                            With lFillTS.Attributes
                                If .GetValue("Constituent") = lCons Then
                                    'contains data for time period being filled
                                    lFillers.Add(lDistList.Keys.Item(i), lFillTS)
                                    Logger.Dbg("FillMissing:  Using " &
                                               .GetValue("Constituent") & " from " &
                                               .GetValue("Location") & " " &
                                               .GetValue("STANAM") & " at Lat/Lng " &
                                               .GetValue("Latitude") & "/" & .GetValue("Longitude"))
                                End If
                            End With
                        End If
                    Next

                    If lFillers.Count > 0 Then
                        Logger.Dbg("FillMissing:  Found " & lFillers.Count & " nearby stations for filling")
                        Logger.Dbg("FillMissing:  Before Filling, % Missing:  " & lPctMiss)
                        If lts.Attributes.GetValue("TU") = atcTimeUnit.TUHour Then
                            If lPctMiss > 0 Then
                                FillHourlyTser(lts, lFillers, lMVal, lMAcc, 90)
                            Else
                                Logger.Dbg("FillMissing:  All Missing periods filled via interpolation")
                            End If
                        Else 'daily tser, locate obs time tsers
                            For Each lFiller As atcTimeseries In lFillers
                                lFillTS = FindObsTimeTS(aTsGroup, lFiller)
                                lFillerOTs.Add(lFillTS)
                            Next
                            lFillTS = FindObsTimeTS(aTsGroup, lts)
                            FillDailyTser(lts, lFillTS, lFillers, lFillerOTs, lMVal, lMAcc, 90)
                        End If
                        lStr = MissingDataSummary(lts, lMVal, lMAcc, lFMin, lFMax, lRepType)
                        lPctMiss = CDbl(lStr.Substring(lStr.LastIndexOf(",") + 1))
                        lts.Attributes.SetValue("UBC200", lPctMiss)
                        Logger.Dbg("FillMissing:  After Filling, % Missing:  " & lPctMiss)
                    Else
                        Logger.Dbg("FillMissing:  PROBLEM - Could not find any nearby stations for filling")
                    End If
                    If lPctMiss > 0 AndAlso (lCons = "ATEMP" OrElse lCons = "DPTEMP" OrElse lCons = "DEWP" OrElse
                                             lCons = "WIND" OrElse lCons = "CLOU" OrElse lCons = "ATEM") Then
                        'fill remaining missing by interpolation for these hourly constituents
                        Dim lFillInstances As New ArrayList
                        Logger.Dbg("FillMissing:  NOTE - Forcing Interpolation of all remaining missing periods")
                        Dim lInterpTS As atcTimeseries = FillMissingByInterpolation(lts, , lFillInstances)
                        If Not lInterpTS Is Nothing Then
                            lts = lInterpTS
                            lInterpTS = Nothing
                            lStr = MissingDataSummary(lts, lMVal, lMAcc, lFMin, lFMax, lRepType)
                            lPctMiss = CDbl(lStr.Substring(lStr.LastIndexOf(",") + 1))
                            lts.Attributes.SetValue("UBC200", lPctMiss)
                            Logger.Dbg("FillMissing:  After Interpolation, % Missing:  " & lPctMiss)
                            Dim lHours As Integer = 0
                            Dim lQtrDay As Integer = 0
                            Dim lHalfDay As Integer = 0
                            Dim lDay As Integer = 0
                            Dim lTwoDays As Integer = 0
                            Dim lWeek As Integer = 0
                            For Each lInstance As Double In lFillInstances
                                If lInstance > 7 Then
                                    lWeek += 1
                                ElseIf lInstance > 2 Then
                                    lTwoDays += 1
                                ElseIf lInstance > 1 Then
                                    lDay += 1
                                ElseIf lInstance > 0.5 Then
                                    lHalfDay += 1
                                ElseIf lInstance > 0.25 Then
                                    lQtrDay += 1
                                Else
                                    lHours += 1
                                End If
                            Next
                            Logger.Dbg("FillMissing:  Forced Interpolation Summary" & vbCrLf &
                                       "                " & lFillInstances.Count & " instances of interpolation" & vbCrLf &
                                       "                   " & lWeek & " longer than 1 week" & vbCrLf &
                                       "                   " & lTwoDays & " longer than 2 days" & vbCrLf &
                                       "                   " & lDay & " longer than 1 Day" & vbCrLf &
                                       "                   " & lHalfDay & " longer than 12 hours" & vbCrLf &
                                       "                   " & lQtrDay & " longer than 6 hours" & vbCrLf &
                                       "                   " & lHours & " less than 6 hours" & vbCrLf)
                        Else
                            Logger.Dbg("FillMissing:  PROBLEM with Interpolation")
                        End If
                    End If
                Else
                    Logger.Dbg("FillMissing:  No Missing Data for this dataset!")
                End If
            End If

            Return lts.Clone()
        Next
        Return Nothing
    End Function

    'Use for filling HOURLY OR LESS timeseries.
    'Fill missing values in timeseries aTS2Fill with values from nearby timeseries (aTSAvail).
    'Use Tolerance factor aTol to determine which nearby stations are acceptable
    'for distributing accumulated data (i.e. missing time distribution).
    Public Sub FillHourlyTser(ByVal aTS2Fill As atcTimeseries, ByVal aTSAvail As atcCollection, ByVal aMVal As Double, ByVal aMAcc As Double, ByVal aTol As Double)
        Dim j As Integer
        Dim k As Integer
        Dim lInd As Integer
        Dim lCurInd As Integer
        Dim lFPos As Integer
        Dim lSPos As Integer
        Dim lMLen As Integer
        Dim ld(5) As Integer
        Dim lMTyp As Integer
        Dim lNSta As Integer
        Dim s As String
        Dim lstr As String
        Dim lSJDay As Double
        Dim lMJDay As Double
        Dim lEJDay As Double
        Dim lSubSJDay As Double
        Dim lFVal As Double
        Dim lFillAdjust As Double
        Dim lStaAdjust As Double
        Dim lRatio As Double
        Dim lCarry As Double
        Dim lRndOff As Double = 0.001
        Dim lAccVal As Double
        Dim lMaxHrVal As Double
        Dim lMaxHrInd As Integer
        Dim lDist As atcCollection
        Dim lTSer As atcTimeseries
        Dim lAdjustAttribute As String = GetAdjustingAttribute(aTS2Fill)
        Const lValMin As Double = -90
        Const lValMax As Double = 200
        Dim lTol As Double
        Dim lFilledIt As Boolean
        Dim lTU As Integer = aTS2Fill.Attributes.GetValue("tu")
        Dim lTS As Integer = aTS2Fill.Attributes.GetValue("ts")
        Dim lIntsPerDay As Double
        Dim lTUStr As String = ""

        If lTol > 1 Then 'passed in as percentage
            lTol = lTol / 100
        End If
        lNSta = 10
        Select Case lTU
            Case atcTimeUnit.TUHour
                lIntsPerDay = 24 / lTS
                lTUStr = "Hour"
            Case atcTimeUnit.TUMinute
                lIntsPerDay = 1440 / lTS
                lTUStr = "Minute"
            Case atcTimeUnit.TUSecond
                lIntsPerDay = 86400 / lTS
                lTUStr = "Second"
            Case Else
                Logger.Dbg("  PROBLEM - Time Units of TSer being filled are not hours, minutes, or seconds")
        End Select
        Logger.Dbg("Filling " & lTS & "-" & lTUStr & " values for " & aTS2Fill.ToString & ", " & aTS2Fill.Attributes.GetValue("STANAM"))
        s = MissingDataSummary(aTS2Fill, aMVal, aMAcc, lValMin, lValMax, 2)
        lstr = StrSplit(s, "DETAIL:DATA", "")
        If Len(s) > 0 Then 'missing data found
            lDist = Nothing
            lDist = CalcMetDistances(aTS2Fill, aTSAvail, lAdjustAttribute)
            lFillAdjust = aTS2Fill.Attributes.GetValue(lAdjustAttribute, -999)
            If Math.Abs(lFillAdjust + 999) > pEpsilon Then
                Logger.Dbg("  (Historical average is " & lFillAdjust & ")")
            Else
                Logger.Dbg("  (Historical averages not in use)")
            End If
            lSJDay = aTS2Fill.Attributes.GetValue("SJDay")
            While Len(s) > 0
                lstr = StrSplit(s, ",", "")
                lMJDay = CDbl(StrSplit(s, ",", ""))
                lMLen = CLng(StrSplit(s, ",", ""))
                lMTyp = CLng(StrSplit(s, ",", ""))
                J2Date(lMJDay, ld)
                If lMTyp = 1 Then 'fill missing period
                    Logger.Dbg("  For Missing period starting " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & " lasting " & lMLen & " intervals:")
                    Dim lLoggedNoStation As Boolean = False
                    lCurInd = -1
                    For k = 1 To lMLen
                        lFPos = lIntsPerDay * (lMJDay - lSJDay) + k - 1
                        lFilledIt = False
                        j = 1
                        While j < aTSAvail.Count
                            lInd = lDist.IndexFromKey(CStr(j))
                            lTSer = aTSAvail.ItemByIndex(lInd)
                            lFVal = -1
                            If (lMJDay + (k - 1) / lIntsPerDay) + JulianMillisecond - lTSer.Dates.Values(1) > pEpsilon And
                               (lMJDay + (k - 1) / lIntsPerDay) - JulianMillisecond <= lTSer.Dates.Values(lTSer.numValues) Then 'check value
                                lSPos = lIntsPerDay * (lMJDay - lTSer.Attributes.GetValue("SJDay")) + k - 1
                                If lTSer.Value(lSPos) <> aMVal AndAlso lTSer.Value(lSPos) > lValMin AndAlso lTSer.Value(lSPos) < lValMax Then 'good value
                                    lFVal = lTSer.Value(lSPos)
                                    lStaAdjust = lTSer.Attributes.GetValue(lAdjustAttribute, -999)
                                    If Math.Abs(lFillAdjust + 999) > pEpsilon AndAlso
                                       Math.Abs(lStaAdjust + 999) > pEpsilon Then
                                        aTS2Fill.Value(lFPos) = lFillAdjust / lStaAdjust * lFVal
                                    Else
                                        aTS2Fill.Value(lFPos) = lFVal
                                    End If
                                    J2Date(lMJDay + (k - 1) / lIntsPerDay, ld)
                                    If lCurInd <> lInd Then 'changing station used to fill
                                        Logger.Dbg("    Filling from TS " & lTSer.ToString & ", " & lTSer.Attributes.GetValue("STANAM"))
                                        If Math.Abs(lFillAdjust + 999) > 0.0001 Then 'pEpsilon Then
                                            Logger.Dbg("      (Adjusting values using historical average of " & lStaAdjust & ")")
                                        End If
                                        lCurInd = lInd
                                    End If
                                    If lTU = atcTimeUnit.TUHour Then
                                        Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & " - " & aTS2Fill.Value(lFPos))
                                    ElseIf lTU = atcTimeUnit.TUMinute Then
                                        Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & " - " & aTS2Fill.Value(lFPos))
                                    ElseIf lTU = atcTimeUnit.TUSecond Then
                                        Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & ":" & ld(5) & " - " & aTS2Fill.Value(lFPos))
                                    End If
                                    j = aTSAvail.Count
                                    lFilledIt = True
                                End If
                            End If
                            j = j + 1
                        End While
                        If Not lFilledIt AndAlso Not lLoggedNoStation Then
                            Logger.Dbg("   No nearby station found for filling")
                            lLoggedNoStation = True
                        End If
                    Next k
                Else 'fill accumulated period
                    If lTU = atcTimeUnit.TUHour Then
                        Logger.Dbg("  For Accumulated period starting " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & " lasting " & lMLen & " intervals:")
                    ElseIf lTU = atcTimeUnit.TUMinute Then
                        Logger.Dbg("  For Accumulated period starting " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & " lasting " & lMLen & " intervals:")
                    ElseIf lTU = atcTimeUnit.TUSecond Then
                        Logger.Dbg("  For Accumulated period starting " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & ":" & ld(5) & " lasting " & lMLen & " intervals:")
                    End If
                    lAccVal = CDbl(StrSplit(s, vbCrLf, ""))
                    lSubSJDay = lMJDay - 1 / lIntsPerDay 'back up one interval for SubSetByDate in ClosestPrecip
                    lEJDay = lSubSJDay + lMLen / lIntsPerDay
                    lTSer = ClosestPrecip(aTS2Fill, aTSAvail, lAccVal, lSubSJDay, lEJDay, aTol)
                    If Not lTSer Is Nothing Then
                        Logger.Dbg("    Distributing " & lAccVal & " from TS# " & lTSer.ToString)
                        lRatio = lAccVal / lTSer.Attributes.GetValue("Sum")
                        For k = 1 To lMLen
                            lFPos = lIntsPerDay * (lMJDay - lSJDay) + k - 1
                            aTS2Fill.Value(lFPos) = lRatio * lTSer.Value(k) + lCarry
                            If aTS2Fill.Value(lFPos) > pEpsilon Then
                                lCarry = aTS2Fill.Value(lFPos) - (Math.Round(aTS2Fill.Value(lFPos) / lRndOff) * lRndOff)
                                aTS2Fill.Value(lFPos) = aTS2Fill.Value(lFPos) - lCarry
                            Else
                                aTS2Fill.Value(lFPos) = 0.0#
                            End If
                            If aTS2Fill.Value(lFPos) > lMaxHrVal Then
                                lMaxHrVal = aTS2Fill.Value(lFPos)
                                lMaxHrInd = lFPos
                            End If
                            J2Date(lMJDay + (k - 1) / lIntsPerDay, ld)
                            If lTU = atcTimeUnit.TUHour Then
                                Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & " - " & aTS2Fill.Value(lFPos))
                            ElseIf lTU = atcTimeUnit.TUMinute Then
                                Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & " - " & aTS2Fill.Value(lFPos))
                            ElseIf lTU = atcTimeUnit.TUSecond Then
                                Logger.Dbg("      " & ld(0) & "/" & ld(1) & "/" & ld(2) & " " & ld(3) & ":" & ld(4) & ":" & ld(5) & " - " & aTS2Fill.Value(lFPos))
                            End If
                        Next k
                        If lCarry > 0 Then 'add remainder to max hourly value
                            aTS2Fill.Value(lMaxHrInd) = aTS2Fill.Value(lMaxHrInd) + lCarry
                        End If
                    Else
                        J2Date(lEJDay, ld)
                        Logger.Dbg("   No nearby station found for distributing accumulated value of " & lAccVal & " on " & ld(0) & "/" & ld(1) & "/" & ld(2))
                        For k = 1 To lMLen - 1 'set missing dist period to 0 except for accum value at end
                            lFPos = lIntsPerDay * (lMJDay - lSJDay) + k - 1
                            aTS2Fill.Value(lFPos) = 0
                        Next k
                    End If
                End If
                lstr = StrSplit(s, "DETAIL:DATA", "")
            End While
        Else 'no missing data
            Logger.Dbg("  No missing data to fill")
        End If
    End Sub

    Public Function ClosestPrecip(ByVal aTSer As atcTimeseries, ByVal aStations As atcCollection, ByVal aDV As Double, ByVal aSDt As Double, ByVal aEDt As Double, ByVal aTol As Double, Optional ByVal aObsTime As Integer = 24) As atcTimeseries
        'find the precip station (from the collection aStations) that is closest to
        'a precip station being filled (aTSer) considering both geographic distance and
        'difference between totals for the time specified by sdt and edt
        '(aStation totals adjusted based on PRISM average annual values)
        'ratio of totals must be within specified tolerance (aTol)
        Dim i As Integer
        Dim lTSerAnn As Double = aTSer.Attributes.GetValue("PRECIP")
        Dim lStaAnn As Double
        Dim lCompareAnnuals As Boolean
        Dim lGDist As Double
        Dim lPrecDist As Double
        Dim lClosestDist As Double = pMaxValue
        Dim lSumTSer As atcTimeseries
        Dim lStaTSer As atcTimeseries
        Dim lPrecTSer As atcTimeseries
        Dim lSum As Double
        Dim lRatio As Double
        Dim lInd As Integer
        Dim lFillDaily As Boolean = aTSer.Attributes.GetValue("TU") = atcTimeUnit.TUDay
        Dim lLoc As String = aTSer.Attributes.GetValue("Location")
        Dim lCons As String = aTSer.Attributes.GetValue("Constituent")
        Dim lSDt As Double
        Dim lEDt As Double
        Static lDist As atcCollection
        Static lCurLoc As String
        Static lCurCons As String

        If lLoc <> lCurLoc OrElse lCons <> lCurCons Then 'calculate station distances
            lDist = New atcCollection
            Dim lDistance(aStations.Count) As Double
            Dim lOrder(aStations.Count) As Integer
            Dim lRank(aStations.Count) As Integer
            Dim lAnnAdj As Double
            If lTSerAnn = 0 Then
                lCompareAnnuals = False
            Else
                lCompareAnnuals = True
            End If
            'stations part of 0-based collection
            For i = 0 To aStations.Count - 1
                lAnnAdj = 1 'assume no adjustment for annual averages
                lStaTSer = aStations.ItemByIndex(i)
                If lCompareAnnuals Then
                    lStaAnn = lStaTSer.Attributes.GetValue("PRECIP")
                    If lStaAnn > 0 Then
                        lAnnAdj = lTSerAnn / lStaAnn
                        If lAnnAdj < 1 Then lAnnAdj = 1 / lAnnAdj
                    End If
                End If
                If IsNumeric(aStations.Keys.Item(i)) Then 'distance value already exists
                    lGDist = aStations.Keys.Item(i)
                Else 'try to calculate distance from attributes
                    lGDist = GeoDist(aTSer, lStaTSer) 'now calculate geographic distance
                End If
                'distance (and order and rank) array is 1-based
                lDistance(i + 1) = lAnnAdj * lGDist
            Next i
            'sort modules are 1-based
            SortRealArray(0, aStations.Count, lDistance, lOrder)
            SortIntegerArray(0, aStations.Count, lOrder, lRank)
            For i = 1 To aStations.Count
                lDist.Add(CStr(lRank(i)), lDistance(i))
            Next i
            lCurLoc = lLoc
            lCurCons = lCons
        End If

        lPrecTSer = Nothing
        For i = 1 To aStations.Count 'look through closest nsta stations for an acceptable daily total
            lInd = lDist.IndexFromKey(CStr(i))
            lStaTSer = aStations.ItemByIndex(lInd)
            If lFillDaily And lStaTSer.Attributes.GetValue("TU") < aTSer.Attributes.GetValue("TU") Then
                'filling daily with less than daily time step, use obs time to set date subset
                'calling routine (FillDailyTSer) backed up start date, so adjust forward with ObsTime
                lSDt = aSDt + aObsTime
                lEDt = aEDt + aObsTime
            Else
                lSDt = aSDt
                lEDt = aEDt
            End If
            If lStaTSer.Attributes.GetValue("SJDay") <= lSDt AndAlso
               lStaTSer.Attributes.GetValue("EJDay") >= lEDt Then
                'tser contains time span being filled
                lSumTSer = SubsetByDate(lStaTSer, lSDt, lEDt, Nothing)
                lSum = lSumTSer.Attributes.GetValue("Sum")
                If lSum > 0 Then
                    If lCompareAnnuals Then
                        lStaAnn = lSumTSer.Attributes.GetValue("PRECIP")
                        If lStaAnn > 0 Then
                            lSum = lSum * lTSerAnn / lStaAnn
                        End If
                    End If
                    lRatio = aDV / lSum
                    If lRatio > 1 Then lRatio = 1 / lRatio
                    If lRatio >= 1 - aTol Then 'acceptable total, include distance factor
                        'lGDist = GeoDist(aTSer, lSumTSer) 'now calculate geographic distance
                        'try 2 as a "weighting factor" for daily precip ratio
                        lPrecDist = (2 - lRatio) * lDist(lInd) 'lGDist
                        If lPrecDist < lClosestDist Then
                            lClosestDist = lPrecDist
                            lPrecTSer = lSumTSer
                        End If
                    End If
                End If
            End If
        Next i
        Return lPrecTSer
    End Function

    Public Function GetAdjustingAttribute(ByVal aTSer As atcTimeseries) As String
        'based on input TSer's constituent, try to find an adjustment
        'factor attribute based on a historical average for that constituent
        Dim lCons As String = aTSer.Attributes.GetValue("Constituent")

        Select Case lCons
            Case "PREC", "HPCP", "PRCP", "HPRC", "HPCP1"
                Return "PRECIP"
            Case "TEMP", "HTMP", "ATMP", "ATEM", "DTMP", "DPTP", "DEWP", "DWPT", "TMIN", "TMAX"
                Return "UBC190" 'user defined att for temperature adjustment
            Case "CLOU"
                'don't use adjusting attribute for cloud cover
                'otherwise, values greater than valid max (10) can occur
                Return ""
            Case Else 'just use average for timeseries
                Return "MEAN"
        End Select

    End Function

    Private Function FindObsTimeTS(ByVal aTsGroup As atcTimeseriesGroup, ByVal aTargetTSer As atcTimeseries) As atcTimeseries
        Dim lCons As String = aTargetTSer.Attributes.GetValue("Constituent")
        Dim lStation As String = aTargetTSer.Attributes.GetValue("Location")
        Dim lState As String = lStation.Substring(0, 2)
        Dim lTSer As New atcTimeseries(Nothing)

        If aTargetTSer.Attributes.GetValue("TU") = atcTimeUnit.TUDay AndAlso IsNumeric(lState) Then
            If lState < 68 OrElse lState = 91 Then
                'coop station, may find daily obs time dataset
                Dim lStaCons As String
                For Each lts As atcTimeseries In aTsGroup
                    lStaCons = lts.Attributes.GetValue("Constituent")
                    If lStaCons = lCons & "-OBS" Then
                        lTSer = lts
                        lTSer.EnsureValuesRead()
                        'Logger.Dbg("FillMissing:FindObsTimeTS: Found Obs Time TSer " & lTSer.ToString & " containing " & lTSer.numValues & " values.")
                        Exit For
                    End If
                Next
            End If
        End If
        Return lTSer
    End Function

    Private Sub ExtendISHTSer(ByRef aTSer As atcTimeseries)
        Dim lEJDay As Double = aTSer.Attributes.GetValue("EJDay")
        If lEJDay > 0 Then
            Dim lNewEJDay As Double = System.Math.Round(lEJDay)
            If lNewEJDay > lEJDay Then
                Dim lNumOldVals As Integer = aTSer.numValues
                Dim lNumNewVals As Integer = System.Math.Round((lNewEJDay - lEJDay) * 24)
                aTSer.numValues = lNumOldVals + lNumNewVals
                For i As Integer = 1 To lNumNewVals
                    aTSer.Dates.Values(lNumOldVals + i) = aTSer.Dates.Values(lNumOldVals) + i / 24
                    aTSer.Values(lNumOldVals + i) = Double.NaN 'aTSer.Attributes.GetValue("TSFILL")
                Next
                aTSer.Attributes.DiscardCalculated()
            End If
        End If
    End Sub

    Public Sub SkyCond2Tenths(ByRef aTsGroup As atcTimeseriesGroup)
        Logger.Dbg("SkyCond2Tenths:Start")

        Dim lCons As String = ""
        Dim lts As atcTimeseries
        Dim ltsCloud As atcTimeseries
        Dim lNSta As Integer = 0
        Dim lNSky As Integer = 0

        For I As Integer = 0 To aTsGroup.Count - 1
            lts = aTsGroup(I)
            lCons = lts.Attributes.GetValue("Constituent")
            If lCons = "SKYCOND" Then
                lNSky += 1
                Logger.Dbg("SkyCond2Tenths: Found timeseries " & lts.ToString)
                ltsCloud = SkyCondOktas2CloudTenths(lts)
                ltsCloud.Attributes.SetValue("ID", 8)
                ltsCloud.Attributes.SetValue("Constituent", "CLOU")
                lts.Clear()
                aTsGroup.RemoveAt(I)
                aTsGroup.Insert(I, ltsCloud)
            End If
        Next

        Logger.Dbg("SkyCond2Tenths: Found " & lNSta & " ISH Stations, of which " & lNSky & " contained SKYCOND/CLOU")
        Logger.Dbg("SkyCond2Tenths: Completed Converting SKYCOND to Cloud Cover")
    End Sub

    Private Function SkyCondOktas2CloudTenths(ByRef aHrlySky As atcTimeseries) As atcTimeseries
        'convert ISH hourly Sky Condition timeseries (recorded in Oktas)
        'to Cloud Cover timeseries in tenths

        Dim lts As atcTimeseries = aHrlySky.Clone
        Dim lVal As Double = 0

        For i As Integer = 1 To lts.numValues
            lVal = lts.Value(i)
            If lVal >= 0 AndAlso lVal <= 10 Then
                Select Case lVal
                    Case 0, 1 'same as recorded okta value
                        lts.Value(i) = lVal
                    Case 2 '2/10 - 3/10
                        lts.Value(i) = 2.5
                    Case 3, 4, 5 '1/10 higher than recorded okta value
                        lts.Value(i) = lVal + 1
                    Case 6 '7/10 - 8/10
                        lts.Value(i) = 7.5
                    Case 7, 8 '2/10 higher than recorded okta value
                        lts.Value(i) = lVal + 2
                    Case 9, 10 'assume fully covered
                        lts.Value(i) = 10
                    Case Else
                        lts.Value(i) = lVal
                End Select
            End If
        Next
        Return lts
    End Function

    Public Function GenSolar(ByVal aTsCloud As atcTimeseries) As atcTimeseries
        'aTsCloud is a hourly time series

        Logger.Dbg("GenSolar:Start")

        Dim lts As atcTimeseries = Nothing
        Dim lSJD As Double
        Dim lEJD As Double
        Dim lNSta As Integer = 0

        If aTsCloud.Attributes.GetValue("Constituent") = "CLOU" Then 'generate daily cloud cover TSER
            lSJD = aTsCloud.Attributes.GetValue("SJDay")
            lEJD = aTsCloud.Attributes.GetValue("EJDay")
            If lSJD - Fix(lSJD) > JulianSecond Then 'get on whole day boundaries
                'then subset by date on those boundaries for aggregating to daily
                lSJD = Fix(lSJD) + 1
                If lEJD - Fix(lEJD) > JulianSecond Then
                    lEJD = Fix(lEJD)
                End If
                lts = Aggregate(SubsetByDate(aTsCloud, lSJD, lEJD, Nothing), atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
            Else 'already on whole day boundary
                lts = Aggregate(aTsCloud, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
            End If
        End If

        If lts IsNot Nothing Then
            Logger.Dbg("GenSolar:  Generated Daily Cloud Cover from " & DumpDate(lSJD) & " to " & DumpDate(lEJD))
            Dim lLat As Double = aTsCloud.Attributes.GetValue("Latitude")
            If lLat > 50 Then 'can't use larger than latitude 51
                Logger.Dbg("GenSolar:  NOTE - latitude (" & lLat & ") is greater than allowable for solar generation, use Lat=50.")
                lLat = 50
            ElseIf lLat < 25 Then 'can't use lower than latitude 25
                Logger.Dbg("GenSolar:  NOTE - latitude (" & lLat & ") is smaller than allowable for solar generation, use Lat=25.")
                lLat = 25
            End If

            Dim ltsDSol As atcTimeseries = Nothing
            Try
                ltsDSol = SolarRadiationFromCloudCover(lts, Nothing, lLat)
                ltsDSol.Attributes.SetValue("ID", 102)
            Catch ex As Exception
                ltsDSol = Nothing
                Logger.Dbg("GenSolar:  PROBLEM - Could not write DSOL to DSN 102")
            End Try
            If ltsDSol IsNot Nothing Then
                Logger.Dbg("GenSolar:  Wrote DSOL to DSN 102")
                Try
                    lts = Nothing
                    lts = DisSolPet(ltsDSol, Nothing, 1, lLat)
                    lts.Attributes.SetValue("ID", 103)
                    lNSta += 1
                    Logger.Dbg("GenSolar:  DisSolPet SOLR to DSN 103")
                Catch ex As Exception
                    Logger.Dbg("GenSolar:  PROBLEM - Could not DisSolPet SOLR to DSN 103")
                End Try
            End If
            ltsDSol = Nothing
            Logger.Dbg("GenSolar: Completed Solar Radiation generation - " & lNSta & " datasets generated.")
        End If

        Return lts
    End Function

    ''' <summary>
    ''' Find the closest station in a timeseries group based on their lat/long
    ''' also sort the timeseries group based on distance to a target in the group in ascending order 
    ''' </summary>
    ''' <param name="aTsGroup">A Timeseries Group</param>
    ''' <param name="aID">The target timeseries's dataset id</param>
    ''' <returns>The timeseries that is cloest to the target timeseries</returns>
    ''' <remarks>Make sure each timeseries' latitude and longitude attributes are set
    ''' Also make sure the timeseries in the group are of the same constituent</remarks>
    ''' 

    Public Function FindClosestStation(ByVal aTsGroup As atcTimeseriesGroup, ByVal aID As Integer) As atcTimeseries
        Dim lTargetStationID As Integer = 0
        'Dim lClosestStations As New SortedList()

        Dim lTargetLat As Double = 0
        Dim lTargetLong As Double = 0
        For Each lTs As atcTimeseries In aTsGroup
            With lTs.Attributes
                If .GetValue("ID") = aID Then
                    lTargetLat = .GetValue("Latitude")
                    lTargetLong = .GetValue("Longitude")
                    Exit For
                End If
            End With
        Next
        If lTargetLat = 0 OrElse lTargetLong = 0 Then Return Nothing

        Dim lDistanceToTarget As Double
        For Each lTs As atcTimeseries In aTsGroup
            With lTs.Attributes
                If .GetValue("ID") = aID Then
                    lDistanceToTarget = 0
                Else
                    lDistanceToTarget = Spatial.GreatCircleDistance(lTargetLong, lTargetLat, .GetValue("Longitude"), .GetValue("Latitude"))
                End If
                .SetValue("Distance", lDistanceToTarget)
                'lClosestStations.Add(lDistanceToTarget, .GetValue("ID"))
            End With
        Next

        Dim lComparerOfDistance As New CompareDistance()
        aTsGroup.Sort(lComparerOfDistance)
        Return aTsGroup(1) 'return the closest (not itself)
    End Function

    Public Function FindClosestStation2(ByVal aTsGroup As atcTimeseriesGroup, ByVal aD4EMIndex As Integer) As atcCollection
        Dim lTargetStationID As Integer = 0
        'Dim lClosestStations As New SortedList()
        Dim lStations As New atcCollection()

        Dim lTargetLat As Double = 0
        Dim lTargetLong As Double = 0

        For Each lTs As atcTimeseries In aTsGroup
            With lTs.Attributes
                If .GetValue("D4EMIDX") = aD4EMIndex Then
                    lTargetLat = .GetValue("Latitude")
                    lTargetLong = .GetValue("Longitude")
                    Exit For
                End If
            End With
        Next
        If lTargetLat = 0 OrElse lTargetLong = 0 Then Return Nothing

        Dim lDistanceToTarget As Double
        For I As Integer = 0 To aTsGroup.Count - 1
            With aTsGroup(I).Attributes
                If .GetValue("D4EMIDX") = aD4EMIndex Then
                    lDistanceToTarget = 0
                Else
                    lDistanceToTarget = Spatial.GreatCircleDistance(lTargetLong, lTargetLat, .GetValue("Longitude"), .GetValue("Latitude"))
                End If
                For Each lDistInCollection As Double In lStations.Keys
                    If Math.Abs(lDistInCollection - lDistanceToTarget) < 0.00001 Then
                        lDistanceToTarget *= 1.0001
                    End If
                Next
                lStations.Add(lDistanceToTarget, .GetValue("D4EMIDX"))
            End With
        Next

        lStations.Sort()
        Return lStations 'return the closest (not itself)
    End Function

    Public Sub WriteTserToFile(ByVal aTs As atcTimeseries)
        Dim lFileDebug As String = "C:\BASINSMet\NCDCSample\NCDC\Debug.txt"
        Dim lSW As New StreamWriter(lFileDebug, False)
        lSW.WriteLine("Date" & vbTab & "Value")
        For Z As Integer = 0 To aTs.numValues - 1
            lSW.WriteLine(aTs.Dates.Value(Z) & vbTab & aTs.Value(Z + 1))
        Next
        lSW.Flush() : lSW.Close() : lSW = Nothing
    End Sub

    Public Class CompareDistance
        Implements IComparer(Of atcTimeseries)

        Public Function Compare(ByVal x As atcTimeseries, ByVal y As atcTimeseries) As Integer Implements System.Collections.Generic.IComparer(Of atcTimeseries).Compare
            If x.Attributes.GetValue("Distance") > y.Attributes.GetValue("Distance") Then
                Return 1
            ElseIf x.Attributes.GetValue("Distance") < y.Attributes.GetValue("Distance") Then
                Return -1
            Else
                Return 0
            End If
        End Function
    End Class

#Region "Sample Code"
    Public Sub TestMetCmp()
        'Create Swat PMET
        Dim lMetCmp As New D4EMMetCmp()
        Dim lSwatWDM As New atcWDM.atcDataSourceWDM()
        Dim lSwatWDMFile As String = "C:\BASINSMet\WDMFilledD4EM\SWAT.wdm"
        If Not lSwatWDM.Open(lSwatWDMFile) Then
            Exit Sub
        End If
        For Each lTs As atcTimeseries In lSwatWDM.DataSets
            lTs.EnsureValuesRead()
        Next
        Dim lDailyRain As atcTimeseries = lSwatWDM.DataSets.FindData("ID", 1)(0)
        Dim lDailyTmax As atcTimeseries = lSwatWDM.DataSets.FindData("ID", 2)(0)
        Dim lDailyTmin As atcTimeseries = lSwatWDM.DataSets.FindData("ID", 3)(0)

        Dim lArgs As New atcDataAttributes
        lArgs.SetValue("Elevation", lDailyRain.Attributes.GetValue("Elev"))
        lArgs.SetValue("Latitude", lDailyRain.Attributes.GetValue("Latitude"))
        lArgs.SetValue("Longitude", lDailyRain.Attributes.GetValue("Longitude"))
        lArgs.SetValue("DailyRain", lDailyRain)
        lArgs.SetValue("TMAX", lDailyTmax)
        lArgs.SetValue("TMIN", lDailyTmin)
        lArgs.SetValue("RetainDailyTimeStep", True)

        If Not lMetCmp.Open("SWATPMET", lArgs) Then
            Exit Sub
        End If

        'For Show-n-Tell
        Dim lSW As New StreamWriter(Path.Combine(Path.GetDirectoryName(lSwatWDMFile), "SWATPMET.txt"), False)
        lSW.WriteLine("Date" & vbTab & "PREC,mm" & vbTab & "TMAX,C" & vbTab & "TMIN,C" & vbTab & "PMET,mm")
        Dim lDateString As String
        Dim lRainString As String
        Dim lTmaxString As String
        Dim lTminString As String
        Dim lPMETString As String
        For I As Integer = 1 To lDailyRain.numValues
            lDateString = DumpDate(lDailyRain.Dates.Value(I - 1))
            lRainString = lDailyRain.Value(I).ToString
            lTmaxString = lDailyTmax.Value(I).ToString
            lTminString = lDailyTmin.Value(I).ToString
            lPMETString = lMetCmp.DataSets(0).Value(I).ToString

            lSW.WriteLine(lDateString & vbTab & lRainString & vbTab & lTmaxString & vbTab & lTminString & vbTab & lPMETString)
        Next
        lSW.Flush()
        lSW.Close()
        lSW = Nothing

        Logger.Dbg("SumAnnual PMET: " & lMetCmp.DataSets(0).Attributes.GetValue("SumAnnual"))

    End Sub

    ''' <summary>
    ''' Read CSV files in aPath from NCDC or WDM, translate constituent name and units for HSPF, and return group of converted timeseries
    ''' </summary>
    ''' <param name="aPath">Look in this folder for met data files to read</param>
    ''' <param name="aSource">Read from: 1 = NCDC .csv, 2 = WDM</param>
    ''' <returns></returns>
    Public Function ReadData(ByVal aPath As String, ByVal aSource As Integer) As atcTimeseriesGroup

        Dim lRawDataGroup As New atcTimeseriesGroup
        Dim lWDM As atcWDM.atcDataSourceWDM
        Dim lSTAID As String = ""

        If aSource = 1 Then 'from NCDC raw data file
            Dim lFiles As New NameValueCollection
            AddFilesInDir(lFiles, aPath, True, "*.csv")
            Logger.Dbg("D4EMReadData: Found " & lFiles.Count & " ISH NCDC data files")

            Dim lNCDCReader As atcTimeseriesNCDC.atcTimeseriesNCDC = Nothing
            Dim lSkipRptType As New List(Of String)
            lSkipRptType.Add("FM-16") 'skip the point measurements
            Dim lFileIndex As Integer = 0
            For Each lFile As String In lFiles
                Using lLevel As New ProgressLevel(False)
                    lNCDCReader = New atcTimeseriesNCDC.atcTimeseriesNCDC()
                    lNCDCReader.SkipRptType = lSkipRptType
                    If lNCDCReader.Open(lFile) Then
                        For Each lTs As atcTimeseries In lNCDCReader.DataSets
                            If lTs.Attributes.GetValue("Constituent") = "UNK" Then Exit For
                            lSTAID = lTs.Attributes.GetValue("STAID1")
                            If lTs.Attributes.GetValue("RptType") = "FM-15" Then
                                lTs.EnsureValuesRead()
                                lRawDataGroup.Add(lTs)
                            End If
                        Next 'dataset in lncdcreader
                    End If

                    For Each lTs As atcTimeseries In lNCDCReader.DataSets
                        If lTs.Attributes.GetValue("Constituent") = "UNK" OrElse
                           lTs.Attributes.GetValue("RptType") <> "FM-15" Then
                            lTs.Clear()
                        End If
                    Next
                    lNCDCReader.DataSets.Clear()
                    lNCDCReader = Nothing
                End Using
                lFileIndex += 1
                Logger.Progress(lFileIndex, lFiles.Count)
            Next

            'Dim lId As Integer = 2
            'Dim lNewWDM As New atcWDM.atcDataSourceWDM
            'lNewWDM.Open(aPath & "NCDCdata.wdm")
            'For I As Integer = 0 To lRawDataGroup.Count - 1
            '    lRawDataGroup(I).Attributes.SetValue("ID", lId)
            '    lNewWDM.AddDataSet(lRawDataGroup(I), atcDataSource.EnumExistAction.ExistReplace)
            '    lId += 1
            'Next
            'lNewWDM.Clear()
            'lNewWDM = Nothing

            ''HPCP data is already in NCDCData.wdm file at DSN 1
            'Dim lWDMFilename As String = Path.Combine(aPath, "NCDCData.wdm")
            'lWDM = New atcWDM.atcDataSourceWDM()
            'If lWDM.Open(lWDMFilename) Then
            '    For Each lTs As atcTimeseries In lWDM.DataSets
            '        If lTs.Attributes.GetValue("Constituent") = "HPCP" Then
            '            lTs.EnsureValuesRead()
            '            lSTAID = lTs.Attributes.GetValue("Location")
            '            lRawDataGroup.Add(lTs)
            '            Exit For
            '        End If
            '    Next
            'End If
            'lWDM = Nothing
        ElseIf aSource = 2 Then 'from saved wdm
            Dim lWDMFilename As String = Path.Combine(aPath, "NCDCData.wdm")
            lWDM = New atcWDM.atcDataSourceWDM()
            If lWDM.Open(lWDMFilename) Then
                lRawDataGroup = lWDM.DataSets()
            End If
            lWDM = Nothing
        End If

        Logger.Status("Converting units of NCDC data")
        'Adjust data unit first before going further as it might hit some ceiling values in the follow-on routines
        For Each lTs As atcTimeseries In lRawDataGroup
            lTs.EnsureValuesRead()
            Select Case lTs.Attributes.GetValue("Constituent")
                Case "WIND" 'convert m/s (scale factor 10) to mph
                    For I As Integer = 0 To lTs.numValues
                        lTs.Value(I) /= 4.47
                    Next
                Case "ATEM", "DEWP" 'convert Deg C (scale factor 10) to Deg F
                    For I As Integer = 0 To lTs.numValues
                        lTs.Value(I) = lTs.Value(I) / 10 * 9 / 5 + 32
                    Next
                Case "HPCP", "PREC"
                    For I As Integer = 0 To lTs.numValues
                        'convert mm (scale factor 10) to inches
                        lTs.Value(I) /= 254.0
                    Next
                Case Else
                    'do nothing
            End Select
        Next

        'Assign additional attributes
        'Dim lStationInfoFilename As String = Path.Combine(aPath, "StationList.txt") 'this is a tab-delimited file
        'Dim lTable As New atcTableDelimited
        'With lTable
        '    .Delimiter = vbTab
        '    .NumHeaderRows = 0
        '    If Not .OpenFile(lStationInfoFilename) Then
        '        Return Nothing
        '    End If
        'End With

        'For Each lTs As atcTimeseries In lRawDataGroup
        '    'lSTAID = lTs.Attributes.GetValue("STAID1")
        '    lSTAID = lTs.Attributes.GetValue("Location").ToString
        '    If lSTAID.Length > 6 Then lSTAID = lSTAID.Substring(0, 6)
        '    With lTable
        '        .CurrentRecord = 1
        '        While Not .EOF
        '            If lSTAID = .Value(1) Then
        '                Dim lat As Double = Double.Parse(.Value(2))
        '                Dim longi As Double = Double.Parse(.Value(3))
        '                Dim lelev As Double = Double.Parse(.Value(4))
        '                Dim lState As String = .Value(5)

        '                With lTs.Attributes
        '                    .SetValue("Latitude", lat)
        '                    .SetValue("Longitude", longi)
        '                    .SetValue("Elev", lelev)
        '                    .SetValue("State", lState)
        '                End With 'lTs.Attributes
        '                Exit While

        '            End If
        '            .MoveNext()
        '        End While
        '    End With 'lTable
        'Next

        'lTable.Clear()
        'lTable = Nothing
        Return lRawDataGroup
    End Function

    ''' <summary>
    ''' Several steps of met data processing from raw into a full set of filled, disaggregate, computed timeseries
    ''' </summary>
    ''' <param name="aTsGroup">Raw datasets as downloaded from NCDC</param>
    ''' <param name="aPath">Save reports here</param>
    ''' <param name="aPREC">On return, is set to processed precip dataset</param>
    ''' <param name="aATEM">On return, is set to processed air temp dataset</param>
    ''' <param name="aWIND">On return, is set to processed wind dataset</param>
    ''' <param name="aSOLR">On return, is set to processed solar radiation dataset</param>
    ''' <param name="aPEVT">On return, is set to processed potential evapotranspiration dataset</param>
    ''' <param name="aDEWP">On return, is set to processed dewpoint dataset</param>
    ''' <param name="aCLOU">On return, is set to processed cloud dataset</param>
    ''' <remarks></remarks>
    Public Sub MetDataProcess(ByRef aTsGroup As atcTimeseriesGroup,
                              ByVal aPath As String,
                              ByRef aPREC As atcTimeseries,
                              ByRef aATEM As atcTimeseries,
                              ByRef aWIND As atcTimeseries,
                              ByRef aSOLR As atcTimeseries,
                              ByRef aPEVT As atcTimeseries,
                              ByRef aDEWP As atcTimeseries,
                              ByRef aCLOU As atcTimeseries)

        'Step 1. Construct the missing dbf for later use
        MissingSummary(aTsGroup, aPath)

        'Step 2. First filtering
        'this routine basically just filter for needed constituents and set the missing value percent attribute (UBC200)
        'so could be combined with step 1 in missing summary
        'Dim lTsGroupFiltered As New atcTimeseriesGroup
        'FirstFilter(aTsGroup, lTsGroupFiltered, lFolder)

        'Step 3. convert SKYCOND --> CLOU
        SkyCond2Tenths(aTsGroup)

        'Step 3. If raw ISH data, needs to shift its dates
        '-Need to set 'State' (eg md) and 'STAID' (eg 720355) attributes for each ts in group
        '-Need to point to directory where the following 3 files are located
        ' MissingSummary.dbf (currently in C:\BASINSMet\Stations\)
        ' coop_Summ.dbf (currently in C:\BASINSMet\Stations\)
        ' ISH_Stations.dbf (currently in C:\BASINSMet\Stations\)
        ShiftISH(aTsGroup, aPath)

        'Step 4. Store PRISM long term stat for mean annual precip and temperature
FindPRISM:
        Dim lPRISM_ppt_ann As String = atcUtility.FindFile("Please locate PRISM annual DBF", "us_ppt_ann.dbf")
        If Not IO.File.Exists(lPRISM_ppt_ann) Then
            If Logger.Msg("Copy DBFs us_ppt_ann, us_tmax_ann, us_tmin_ann into " & vbCrLf _
                       & IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly.Location), "Cannot locate PRISM files") = vbRetry Then
                GoTo FindPRISM
            End If
            Exit Sub
        End If
        StorePRISM(IO.Path.GetDirectoryName(lPRISM_ppt_ann), aTsGroup)

        'Step 5. Fill Missing values
        For Each lTs As atcTimeseries In aTsGroup
            If lTs.Attributes.GetValue("Constituent") = "HPCP" Then
                lTs.Attributes.SetValue("Constituent", "PREC")
            End If
        Next
        Dim lTargetStationID As String = aTsGroup(0).Attributes.GetValue("Location").ToString.Substring(0, 6) 'TODO: specify closest station
        Dim lConsTargets() As String = {"PREC", "WIND", "ATEM", "CLOU", "DEWP"}
        Dim lCon As String = ""
        Dim lFilledTsGroup As New atcTimeseriesGroup
        For Each lCon In lConsTargets
            Dim lNewTsGroup As atcTimeseriesGroup = aTsGroup.FindData("Constituent", lCon)

            For I As Integer = 0 To lNewTsGroup.Count - 1
                With lNewTsGroup(I).Attributes
                    .SetValue("D4EMIDX", I) '<--this step is needed for sorting
                    If .GetValue("Location").ToString.Substring(0, 6) = lTargetStationID Then
                        .SetValue("NeedToFill", True)
                    Else
                        .SetValue("NeedToFill", False)
                    End If
                End With
            Next

            Dim lNewTs As atcTimeseries = FillMissing(lNewTsGroup)
            If lNewTs IsNot Nothing Then
                lFilledTsGroup.Add(lNewTs)
            Else
                Logger.Dbg("Nothing to fill or failed filling missing for " & lCon)
            End If
        Next

        'Step 6. Compute PEVT using Hamon method and generate SOLR
        Dim lTsSOLR As atcTimeseries = Nothing
        Dim lTsPEVT As atcTimeseries = Nothing
        Dim lLatitude As Double
        Dim lFinalTsGroup As New atcTimeseriesGroup
        For Each lTs As atcTimeseries In lFilledTsGroup

            If lTs.Attributes.GetValue("Location").ToString.Substring(0, 6) <> lTargetStationID Then Continue For

            lFinalTsGroup.Add(lTs)

            Select Case lTs.Attributes.GetValue("Constituent")
                Case "CLOU"
                    lTsSOLR = GenSolar(lTs)
                    lFinalTsGroup.Add(lTsSOLR)
                Case "ATEM"
                    Dim lCTS() As Double = {0, 0.0045, 0.01, 0.01, 0.01, 0.0085, 0.0085, 0.0085, 0.0085, 0.0085, 0.0095, 0.0095, 0.0095}
                    Logger.Dbg("Computing Hamon PET from Hourly temperature")
                    lLatitude = lTs.Attributes.GetValue("Latitude")
                    lTsPEVT = PanEvaporationTimeseriesComputedByHamonX(lTs, Nothing, True, lLatitude, lCTS)
                    If lTsPEVT.Attributes.GetValue("tu") = atcTimeUnit.TUDay Then
                        lTsPEVT = DisSolPet(lTsPEVT, Nothing, 2, lLatitude)
                    End If
                    lFinalTsGroup.Add(lTsPEVT)
            End Select
        Next

        aTsGroup.ChangeTo(lFinalTsGroup)
        MissingSummary(aTsGroup, aPath, "MissingDataSummary_PostProcessing")

        For Each lTs As atcTimeseries In lFinalTsGroup
            Select Case lTs.Attributes.GetValue("Constituent")
                Case "PREC" : aPREC = lTs
                Case "ATEM" : aATEM = lTs
                Case "CLOU" : aCLOU = lTs
                Case "WIND" : aWIND = lTs
                Case "DEWP" : aDEWP = lTs
                Case "SOLR" : aSOLR = lTs
                Case "EVAP" : aPEVT = lTs
            End Select
        Next

    End Sub
#End Region

End Module
