'Revision
'09.29.20 conversion of solar from ly/hr to W/m2
'
Imports atcData
Imports atcUtility
Imports atcWDM
Imports System.Diagnostics
Imports MapWinUtility
Imports System
Imports System.Windows.Forms
Imports System.Text

Public Class WriteEFDC
    Private pModifiedData As atcTimeseriesGroup
    'dsn list has 9 elements, first seven for aser file, next two for wser
    Private pListDSN As List(Of Integer)
    Private errmsg
    Private statusLbl As ToolStripLabel
    Private statusStrip As ToolStrip
    Private pDateStart As DateTime, pDateEnd As DateTime
    Private EFDCVars As New List(Of String) From {"ATMP", "ATEM", "DEWP", "PREC",
                               "PEVT", "SOLR", "CLOU", "WIND", "WNDD"}
    Private dictWea As New SortedDictionary(Of DateTime, List(Of Double))
    Private aserFile As String = String.Empty
    Private wserFile As String = String.Empty
    Private lWriter As IO.StreamWriter
    Private aLocation As String

    Public Sub New(ByRef _statuslbl As ToolStripLabel,
                   ByRef _statusStrip As ToolStrip,
                   ByVal lMetWDMFile As String,
                   ByVal aSaveInFolder As String,
                   ByVal _pDateStart As DateTime,
                   ByVal _pDateEnd As DateTime,
                   ByVal _aListDSN As List(Of Integer),
                   ByVal _aLocation As String)

        statusLbl = _statuslbl
        statusStrip = _statusStrip
        pListDSN = _aListDSN
        pDateStart = _pDateStart
        pDateEnd = _pDateEnd
        aLocation = _aLocation

        'Dim aDateStart As Double = Date2J(pDateStart.Year, pDateStart.Month, pDateStart.Day,
        'pDateStart.Hour, pDateStart.Minute)
        'Dim aDateEnd As Double = Date2J(pDateEnd.Year, pDateEnd.Month, pDateEnd.Day,
        'pDateEnd.Hour, pDateEnd.Minute)

        Dim aDateStart As Double = Date2J(pDateStart.Year, pDateStart.Month, pDateStart.Day,
                                   0, 0)
        Dim aDateEnd As Double = Date2J(pDateEnd.Year, pDateEnd.Month, pDateEnd.Day,
                                   23, 59)

        Debug.WriteLine("PDate Start = " + pDateStart.ToString())
        Debug.WriteLine("PDate  End = " + pDateEnd.ToString())
        Debug.WriteLine("Date Start = " + aDateStart.ToString())
        Debug.WriteLine("Date End = " + (aDateEnd + 1).ToString())
        Debug.WriteLine("Date Diff = " + (aDateEnd - aDateStart).ToString())

        ReadEFDCMetFiles(lMetWDMFile, aSaveInFolder, aDateStart, aDateEnd, pListDSN, aLocation)

        'set .aser file
        aserFile = IO.Path.Combine(aSaveInFolder, "aser" + aLocation + ".inp")
        lWriter = New IO.StreamWriter(aserFile)
        lWriter.AutoFlush = True

        'Write aser.inp
        WriteAserHeader()
        WriteAserFile()
        lWriter.Close()
        lWriter = Nothing

        'set .aser file
        wserFile = IO.Path.Combine(aSaveInFolder, "wser" + aLocation + ".inp")
        lWriter = New IO.StreamWriter(wserFile)
        lWriter.AutoFlush = True
        WriteWserHeader()
        WriteWserFile()
        lWriter.Close()
        lWriter = Nothing

        'Write wser.inp
    End Sub

    Private Sub WriteAserHeader()
        Dim dtStart As String = pDateStart.ToShortDateString()
        Dim dtEnd As String = pDateEnd.ToShortDateString()
        Dim nVals As Integer = dictWea.Count + 2
        Debug.WriteLine("Num Hours = " + nVals.ToString())

        'lWriter.WriteLine("# ASER.INP: " + dtStart + " To " + dtEnd)
        lWriter.WriteLine("# ASER.INP: " + pDateStart.ToString() + " To " + pDateEnd.ToString())
        lWriter.WriteLine("C Title")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  ATMOSPHERIC FORCING FILE, USE With 28 JULY 96 And LATER VERSIONS Of EFDC")
        lWriter.WriteLine("C  MASER     = NUMBER Of TIME DATA POINTS")
        lWriter.WriteLine("C  TCASER    = DATA TIME UNIT CONVERSION To SECONDS")
        lWriter.WriteLine("C  TAASER    = ADDITIVE ADJUSTMENT Of TIME VALUES SAME UNITS As INPUT TIMES")
        lWriter.WriteLine("C  IRELH     = 0 VALUE TWET COLUMN VALUE Is TWET, =1 VALUE Is RELATIVE HUMIDITY")
        lWriter.WriteLine("C  RAINCVT   = CONVERTS RAIN To UNITS Of M/SEC.  In/hr * 0.0254 m / 1 In * 1 hr / 3600 s = 7.05556E-06 (m/s)")
        lWriter.WriteLine("C  EVAPCVT   = CONVERTS EVAP To UNITS Of M/SEC, If EVAPCVT<0 EVAP Is INTERNALLY COMPUTED")
        lWriter.WriteLine("C  SOLRCVT   = CONVERTS SOLAR SW RADIATION To JOULES/SQ METER")
        lWriter.WriteLine("C  CLDCVT    = MULTIPLIER For ADJUSTING CLOU)D COVER")
        lWriter.WriteLine("C  IASWRAD   = O DISTRIBUTE SW SOL RAD OVER WATER COL And INTO BED, =1 ALL To SURF LAYER")
        lWriter.WriteLine("C  REVC      = 1000*EVAPORATIVE TRANSFER COEF, REVC<0 USE WIND SPD DEPD DRAG COEF")
        lWriter.WriteLine("C  RCHC      = 1000*CONVECTIVE HEAT TRANSFER COEF, REVC<0 USE WIND SPD DEPD DRAG COEF")
        lWriter.WriteLine("C  SWRATNF   = FAST SCALE SOLAR SW RADIATION ATTENUATION COEFFCIENT 1./METERS")
        lWriter.WriteLine("C  SWRATNS   = SLOW SCALE SOLAR SW RADIATION ATTENUATION COEFFCIENT 1./METERS")
        lWriter.WriteLine("C  FSWRATF   = FRACTION Of SOLSR SW RADIATION ATTENUATED FAST  0<FSWRATF<1")
        lWriter.WriteLine("C  DABEDT    = DEPTH Or THICKNESS Of ACTIVE BED TEMPERATURE LAYER, METERS")
        lWriter.WriteLine("C  TBEDIT    = INITIAL BED TEMPERATURE")
        lWriter.WriteLine("C  HTBED1    = CONVECTIVE HT COEFFCIENT BETWEEN BED And BOTTOM WATER LAYER  NO Dim")
        lWriter.WriteLine("C  HTBED2    = HEAT TRANS COEFFCIENT BETWEEN BED And BOTTOM WATER LAYER  M/SEC")
        lWriter.WriteLine("C  PATM      = ATM PRESS MILLIBAR")
        lWriter.WriteLine("C  TDRY/TEQ  = DRY ATM TEMP ISOPT(2)=1 Or EQUIL TEMP ISOPT(2)=2")
        lWriter.WriteLine("C  TWET/RELH = WET BULB ATM TEMP IRELH=0, RELATIVE HUMIDITY IRELH=1")
        lWriter.WriteLine("C  RAIN      = RAIN FALL RATE LENGTH/TIME")
        lWriter.WriteLine("C  EVAP      = EVAPORATION RATE If EVAPCVT>0.")
        lWriter.WriteLine("C  SOLSWR    = SOLAR Short WAVE RADIATION AT WATER SURFACE  ENERGY FLUX/UNIT AREA")
        lWriter.WriteLine("C  CLOUD     = FRATIONAL CLOUD COVER")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  MASER    TCASER  TAASER  IRELH    RAINCVT  EVAPCVT  SOLRCVT   CLDCVT")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  IASWRAD  REVC    RCHC    SWRATNF  SWRATNS  FSWRATF  DABEDT    TBEDIT    HTBED1    HTBED2")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  TASER(M) PATM(M) TDRY(M) TWET(M)  RAIN(M)  EVAP(M)  SOLSWR(M) CLOUD(M)")
        lWriter.WriteLine("C                    /TEQ   /RELH                      /HTCOEF")
        ' solar is read as langley, EFDC requires J/m2 conversion is 41840 J/m2 = 1 ly
        lWriter.WriteLine(F7(nVals) + "      86400.  0.       1     7.05556E-06   -1. 11.629935       1.00")
        lWriter.WriteLine("   1        1.5     1.5     1.0      0.0      1.0      10000.0   2.0       0.000     2.0E-6")
        lWriter.Flush()
    End Sub
    Private Sub WriteWserHeader()
        Dim dtStart As String = pDateStart.ToShortDateString()
        Dim dtEnd As String = pDateEnd.ToShortDateString()
        Dim nVals As Integer = dictWea.Count + 2
        Debug.WriteLine("Num Hours = " + nVals.ToString())

        'lWriter.WriteLine("C WSER.INP: " + dtStart + " To " + dtEnd)
        lWriter.WriteLine("C WSER.INP: " + pDateStart.ToString() + " To " + pDateEnd.ToString())
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  WIND FORCING FILE, USE WITH 28 MARCH 96 AND LATER VERSIONS OF EFDC")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  MASER(NW)   =NUMBER OF TIME DATA POINTS")
        lWriter.WriteLine("C  TCASER(NW)  =DATA TIME UNIT CONVERSION TO SECONDS")
        lWriter.WriteLine("C  TAASER(NW)  =ADDITIVE ADJUSTMENT OF TIME VALUES SAME UNITS AS INPUT TIMES")
        lWriter.WriteLine("C  WINDSCT(NW) =WIND SPEED CONVERSION TO M/SEC")
        lWriter.WriteLine("C  ISWDINT(NW) =DIRECTION CONVENTION")
        lWriter.WriteLine("C               0 DIRECTION TO")
        lWriter.WriteLine("C               1 DIRECTION FROM")
        lWriter.WriteLine("C               2 WINDS IS EAST VELOCITY, WINDD IS NORTH VELOCITY")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C")
        lWriter.WriteLine("C  MASER  TCASER   TAASER  WINDSCT  ISWDINT")
        lWriter.WriteLine("C  TASER(M)   WINDS(M)   WINDD(M)")
        lWriter.WriteLine("C")
        lWriter.WriteLine(F11(nVals) + "   86400.     0.     1.00    1")
        lWriter.Flush()
    End Sub
    Private Sub WriteAserFile()
        Cursor.Current = Cursors.WaitCursor
        Dim ihr As Double = 0.0
        Dim curyr As Integer, lastyr As Integer
        Dim lstVal As List(Of Double) = Nothing
        Dim sline As String = String.Empty

        Try
            'get first list of values
            lstVal = dictWea.First().Value
            lastyr = dictWea.First().Key.Year
            ihr = Convert.ToDouble(dictWea.First.Key.Hour) / 24
            WriteStatus("Writing timeseries for site " + aLocation + ": " + lastyr.ToString())
            WriteAserline(0, lstVal)

            For Each kv As KeyValuePair(Of Date, List(Of Double)) In dictWea
                curyr = kv.Key.Year
                dictWea.TryGetValue(kv.Key, lstVal)

                If curyr <> lastyr Then
                    WriteStatus("Writing timeseries for site " + aLocation + ": " + curyr.ToString())
                End If
                WriteAserline(ihr, lstVal)

                ihr += (1 / 24.0)
                lastyr = curyr
            Next
            lWriter.Flush()
            Cursor.Current = Cursors.Default
            WriteStatus("Ready ...")

        Catch ex As Exception
            errmsg = "Error writing EFDC met file" & vbCrLf & ex.Message & ex.StackTrace
            MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub WriteAserline(hr As Double, lstVal As List(Of Double))
        Dim st As New StringBuilder
        st.Append(hr.ToString("F4").PadLeft(12))
        st.Append(lstVal.ElementAt(0).ToString("0.00").PadLeft(11)) 'ATMP
        st.Append(lstVal.ElementAt(1).ToString("0.00").PadLeft(8)) 'ATEM
        st.Append(lstVal.ElementAt(2).ToString("0.00").PadLeft(8)) 'RHUM
        st.Append(lstVal.ElementAt(3).ToString("0.000").PadLeft(6)) 'PREC
        st.Append(lstVal.ElementAt(4).ToString("0.000").PadLeft(15)) 'PEVT
        st.Append(lstVal.ElementAt(5).ToString("0.00").PadLeft(8)) 'SOLR
        st.Append(lstVal.ElementAt(6).ToString("0.00").PadLeft(8)) 'CLOU
        lWriter.WriteLine(st.ToString())
        lWriter.Flush()
    End Sub
    Private Sub WriteWserFile()
        Cursor.Current = Cursors.WaitCursor
        Dim ihr As Double = 0.0
        Dim curyr As Integer, lastyr As Integer
        Dim lstVal As List(Of Double) = Nothing
        Dim sline As String = String.Empty

        Try
            'get first list of values
            lstVal = dictWea.First().Value
            lastyr = dictWea.First().Key.Year
            ihr = Convert.ToDouble(dictWea.First.Key.Hour) / 24
            WriteStatus("Writing timeseries for site " + aLocation + ": " + lastyr.ToString())
            WriteWserline(0, lstVal)

            For Each kv As KeyValuePair(Of Date, List(Of Double)) In dictWea
                curyr = kv.Key.Year
                dictWea.TryGetValue(kv.Key, lstVal)

                If curyr <> lastyr Then
                    WriteStatus("Writing timeseries for site " + aLocation + ": " + curyr.ToString())
                End If
                WriteWserline(ihr, lstVal)

                ihr += (1 / 24.0)
                lastyr = curyr
            Next
            lWriter.Flush()
            Cursor.Current = Cursors.Default
            WriteStatus("Ready ...")

        Catch ex As Exception
            errmsg = "Error writing EFDC met file" & vbCrLf & ex.Message & ex.StackTrace
            MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub WriteWserline(hr As Double, lstVal As List(Of Double))
        Dim st As New StringBuilder
        st.Append(hr.ToString("F4").PadLeft(12))
        Dim wdir As Integer = CInt(lstVal.ElementAt(8))
        st.Append(lstVal.ElementAt(7).ToString("0.00").PadLeft(11)) 'WIND
        st.Append(wdir.ToString().PadLeft(11)) 'WINDD
        lWriter.WriteLine(st.ToString())
        lWriter.Flush()
    End Sub
    Private Sub ReadEFDCMetFiles(ByVal lMetWDMFile As String,
                            ByVal aSaveInFolder As String,
                            ByVal aDateStart As Double,
                            ByVal aDateEnd As Double,
                            ByVal aListDSN As List(Of Integer),
                            ByVal aLocation As String)

        Debug.WriteLine("Executing ReadEFDCMetFiles ...")
        Dim lMetWDM As New atcDataSourceWDM
        If lMetWDM.Open(lMetWDMFile) Then
            Debug.WriteLine("WDM file" + lMetWDMFile + " file opened" + lMetWDM.DataSets.Count.ToString() + " series")
            If lMetWDM.DataSets.Count > 0 Then
                ReadEFDCMetInput(lMetWDM, aSaveInFolder, aDateStart, aDateEnd, aLocation)
            End If
            lMetWDM.Clear()
        Else
        End If
    End Sub
    Private Sub ReadEFDCMetInput(ByVal aMetWDM As atcDataSource,
                                 ByVal aSaveInFolder As String,
                                 ByVal aDateStart As Double,
                                 ByVal aDateEnd As Double,
                                 ByVal aLocation As String)

        Debug.WriteLine("Executing ReadEFDCMetInput ...")
        Dim aDSN As Integer = 0
        Dim sVar As String

        For ivar As Integer = 0 To EFDCVars.Count - 1
            WriteStatus("Reading " + EFDCVars(ivar) + " for location " + aLocation)
            Debug.WriteLine("Reading " + EFDCVars(ivar) + " for location " + aLocation)
            aDSN = CInt(pListDSN.ElementAt(ivar))
            sVar = EFDCVars(ivar)
            ReadSeries(aMetWDM, sVar, aDSN, aDateStart, aDateEnd)
        Next
        WriteStatus("Ready ...")
    End Sub
    Private Function ReadSeries(ByVal aWDM As atcDataSourceWDM,
                                ByVal sVar As String,
                                ByVal aDSN As Integer,
                                ByVal aDateStart As Double,
                                ByVal aDateEnd As Double) As Boolean

        Debug.WriteLine("Executing ReadSeries for " + sVar)
        Dim lCons As String = sVar
        Dim lTS As atcTimeseries = Nothing

        Try
            lTS = GetWDMTimeseries(aWDM, aDSN, lCons)
            Debug.WriteLine("NumValues  = " + lTS.numValues.ToString())
            '{"ATMP", "ATEM", "RHUM", "PREC", "PEVT", "SOLR", "CLOU"}

            If lTS IsNot Nothing Then
                'unit changes
                Select Case sVar
                    Case "ATMP" '0
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 1.33322) 'mmhg to mbar
                    Case "ATEM" '1
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lTS)
                    Case "DEWP" '2
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lTS)
                        lTS = ComputeRHUM(aWDM, lTS, aDSN, aDateStart, aDateEnd) 'lts is dewp in deg C
                    Case "PREC" '3
                        'lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 25.4) 'in to mm
                    Case "PEVT" '4
                        'lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 25.4)
                    Case "SOLR" '5 W/m2
                        'lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 11.6299354) 'convert Langley to W/m2
                    Case "CLOU" '6
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 0.1)
                    Case "WIND" '7 m/s
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 0.447027)
                    Case "WNDD" '8
                        lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 1)
                End Select

                Dim lDate As Date, kDate As Date
                Dim lValue As Double
                For lIndex As Integer = 1 To lTS.numValues
                    lDate = Date.FromOADate(lTS.Dates.Value(lIndex - 1))
                    kDate = lDate.AddHours(1)
                    If Date.Compare(lDate, pDateStart) >= 0 And
                            Date.Compare(lDate, pDateEnd) <= 0 Then

                        lValue = lTS.Value(lIndex)
                        If Double.IsNaN(lValue) Then lValue = -99

                        'debug
                        If (sVar.Contains("SOLR") And lIndex < 25) Then
                            Debug.WriteLine(lDate.ToString() + ", " + kDate.ToString() + ", " + lValue.ToString())
                        End If

                        If (sVar.Contains("ATMP")) Then
                            Dim lstval As New List(Of Double)
                            lstval.Add(lValue)
                            'dictWea.Add(lDate, lstval)
                            dictWea.Add(kDate, lstval)
                            lstval = Nothing
                        Else
                            Dim lstval As New List(Of Double)
                            'dictWea.TryGetValue(lDate, lstval)
                            dictWea.TryGetValue(kDate, lstval)
                            lstval.Add(lValue)
                            lstval = Nothing
                        End If
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
        'Catch ex As Exception
        'errmsg = "Error reading PREC timeseries" & vbCrLf & ex.Message & ex.StackTrace
        'MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
        'Return False
        'End Try

    End Function
    Private Function ComputeRHUM(ByVal aWDM As atcDataSourceWDM,
                              ByVal lDew As atcData.atcTimeseries,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As atcTimeseries
        Dim lDSN As Integer = CInt(pListDSN.ElementAt(1)) 'temperature
        Dim lTemp As atcData.atcTimeseries = GetWDMTimeseries(aWDM, lDSN, "ATEM")
        lTemp = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lTemp)

        Dim lHMD As atcTimeseries = Nothing
        Try
            If lDew IsNot Nothing AndAlso lTemp IsNot Nothing Then
                lHMD = lDew.Clone

                Dim lDate As Date
                Dim lDewValue As Double
                Dim lTempValue As Double
                Dim lValue As Double

                For lIndex As Integer = 1 To lDew.numValues
                    lDate = Date.FromOADate(lDew.Dates.Value(lIndex - 1))

                    lDewValue = lDew.Value(lIndex)
                    lTempValue = lTemp.Value(lIndex)

                    If lDewValue < lTempValue Then

                        lValue = Math.Exp((17.269 * lDewValue) / (273.3 + lDewValue)) _
                       / Math.Exp((17.269 * lTempValue) / (273.3 + lTempValue))
                        If Double.IsNaN(lValue) Then lValue = -99
                        lHMD.Value(lIndex) = lValue
                    Else
                        'Debug.WriteLine("dew > temp, " & DoubleToString(lDewValue) & " > " & DoubleToString(lTempValue))
                        lHMD.Value(lIndex) = 1.0
                    End If

                Next
            End If
            Return lHMD
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Private Function GetWDMTimeseries(ByVal aWDM As atcDataSource,
                                      ByVal aDSN As Integer,
                                      ByVal aConstituent As String) As atcTimeseries
        ' Search for this data in original data source
        For Each lTSsearch As atcData.atcTimeseries In aWDM.DataSets
            If lTSsearch.Attributes.GetValue("ID") = aDSN Then
                Return lTSsearch
            End If
        Next
        'Logger.Dbg("No " & aConstituent & ": DSN" & aDSN.ToString() + " available in " & aWDM.Specification)
        Return Nothing
    End Function
    Private Function F7(ByVal aValue As Integer) As String
        Return Format(aValue, "0").PadLeft(7)
    End Function
    Private Function F11(ByVal aValue As Integer) As String
        Return Format(aValue, "0").PadLeft(11)
    End Function
    Private Function F74(ByVal aValue As Double) As String
        Return Format(aValue, "0.0000").PadLeft(7)
    End Function
    Private Function F124(ByVal aValue As Double) As String
        Return Format(aValue, "0.0000").PadLeft(12)
    End Function
    Private Function EnsureComplete(ByVal aTimeseries As atcTimeseries,
                                    ByVal aStartDate As Double,
                                    ByVal aEndDate As Double,
                                    ByVal aDataSource As atcDataSource) As atcTimeseries

        Dim lNewTimeseries As atcTimeseries
        Dim lReport As New System.Text.StringBuilder

        lReport.AppendLine("Complete date range from " & Date.FromOADate(aStartDate) & " to " & Date.FromOADate(aEndDate))

        Dim lOverlapTimeseries As atcTimeseries = SubsetByDate(aTimeseries, aStartDate, aEndDate, aDataSource)
        If lOverlapTimeseries.numValues > 0 Then
            If Math.Abs(lOverlapTimeseries.Dates.Value(0) - aStartDate) < JulianSecond AndAlso
               Math.Abs(lOverlapTimeseries.Dates.Value(lOverlapTimeseries.numValues) - aEndDate) < JulianSecond Then
                'Requested time 
                lNewTimeseries = lOverlapTimeseries
            Else
                lNewTimeseries = NewTimeseries(aStartDate, aEndDate, atcTimeUnit.TUDay, 1, , GetNaN)
                'First copy overlapping values into new timeseries
                Dim lOverlapStart As Integer = FindDateAtOrAfter(lNewTimeseries.Dates.Values, lOverlapTimeseries.Dates.Value(1))
                System.Array.Copy(lOverlapTimeseries.Values, 1, lNewTimeseries.Values, lOverlapStart, lOverlapTimeseries.numValues)
            End If
        Else
            lNewTimeseries = NewTimeseries(aStartDate, aEndDate, atcTimeUnit.TUDay, 1, , GetNaN)
        End If

        'Fill in missing values from same date in another year of input data
        Dim lOldIndex As Integer = 1
        Dim lOldDateArray(5) As Integer
        Dim lNewDateArray(5) As Integer

        For lNewIndex As Integer = 1 To lNewTimeseries.numValues
            If Double.IsNaN(lNewTimeseries.Value(lNewIndex)) Then
                J2Date(lNewTimeseries.Dates.Value(lNewIndex), lNewDateArray)
                Dim lSearchOldIndex As Integer = lOldIndex
                While lSearchOldIndex <= aTimeseries.numValues
                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
                        If lOldDateArray(1) = lNewDateArray(1) AndAlso lOldDateArray(2) = lNewDateArray(2) Then
                            lReport.AppendLine("Missing value for " & lNewDateArray(0) & "/" & lNewDateArray(1) & "/" & lNewDateArray(2) & ", copied from " & lOldDateArray(0) & "/" & lOldDateArray(1) & "/" & lOldDateArray(2) & ", " & aTimeseries.Value(lSearchOldIndex))
                            GoTo FoundOldDate
                        End If
                    End If
                    lSearchOldIndex += 1
                End While
                lSearchOldIndex = 1
                While lSearchOldIndex < lOldIndex
                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
                        If lOldDateArray(1) = lNewDateArray(1) AndAlso lOldDateArray(2) = lNewDateArray(2) Then
                            GoTo FoundOldDate
                        End If
                    End If
                    lSearchOldIndex += 1
                End While

                lSearchOldIndex = lOldIndex
                Dim lNearestIndex As Integer = lOldIndex
                Dim lNearestDays As Integer = 500
                Dim lDays As Integer
                While lSearchOldIndex <= aTimeseries.numValues
                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
                        If lOldDateArray(1) = lNewDateArray(1) Then
                            lDays = Math.Abs(lOldDateArray(2) - lNewDateArray(2))
                            If lDays < lNearestDays Then lNearestIndex = lSearchOldIndex
                        End If
                    End If
                    lSearchOldIndex += 1
                End While
                lSearchOldIndex = 1
                While lSearchOldIndex < lOldIndex
                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
                        If lOldDateArray(1) = lNewDateArray(1) Then
                            lDays = Math.Abs(lOldDateArray(2) - lNewDateArray(2))
                            If lDays < lNearestDays Then lNearestIndex = lSearchOldIndex
                        End If
                    End If
                    lSearchOldIndex += 1
                End While

                lSearchOldIndex = lNearestIndex
                J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
                lReport.AppendLine("Value not found for " & lNewDateArray(0) & "/" & lNewDateArray(1) & "/" & lNewDateArray(2) & " using value from " & lNearestDays & " days away: " & lOldDateArray(0) & "/" & lOldDateArray(1) & "/" & lOldDateArray(2) & ", " & aTimeseries.Value(lSearchOldIndex))
FoundOldDate:
                lNewTimeseries.Value(lNewIndex) = aTimeseries.Value(lSearchOldIndex)
                lNewTimeseries.ValueAttributes(lNewIndex).SetValue("Original Date", aTimeseries.Dates.Value(lSearchOldIndex))
            End If
        Next

        lNewTimeseries.Attributes.SetValue("Description", lReport.ToString)
        Return lNewTimeseries

    End Function
    Private Sub WriteStatus(msg As String)
        statusLbl.Text = msg
        statusStrip.Refresh()
    End Sub

End Class
