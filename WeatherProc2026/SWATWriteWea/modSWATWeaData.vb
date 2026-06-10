Imports atcData
Imports atcUtility
Imports atcWDM
Imports System.Diagnostics
Imports MapWinUtility
Public Module modSWATWeaData
    Private pModifiedData As atcTimeseriesGroup
    Private pListDSN As List(Of Integer)
    Public Sub WriteSWATMetFiles(ByVal lMetWDMFile As String,
                            ByVal aSaveInFolder As String,
                            ByVal pDateStart As DateTime,
                            ByVal pDateEnd As DateTime,
                            ByVal aListDSN As List(Of Integer),
                            ByVal aLocation As String)

        pListDSN = aListDSN
        Dim aDateStart As Double = Date2J(pDateStart.Year, pDateStart.Month, pDateStart.Day,
                                   pDateStart.Hour, pDateStart.Minute)
        Dim aDateEnd As Double = Date2J(pDateEnd.Year, pDateEnd.Month, pDateEnd.Day,
                                   pDateEnd.Hour, pDateEnd.Minute)

        Dim lMetWDM As New atcDataSourceWDM
        If lMetWDM.Open(lMetWDMFile) Then
            Debug.WriteLine("WDM file" + lMetWDMFile + " file opened" + lMetWDM.DataSets.Count.ToString() + " series")
            If lMetWDM.DataSets.Count > 0 Then
                WriteSwatMetInput(lMetWDM, Nothing, aSaveInFolder, aDateStart, aDateEnd, aLocation)
            Else
                MapWinUtility.Logger.Dbg("No data found in met wdm: " & lMetWDMFile)
            End If
            lMetWDM.Clear()
            atcDataManager.DataSources.Remove(lMetWDM)
        Else
            MapWinUtility.Logger.Dbg("Could not open met wdm: " & lMetWDMFile)
        End If
    End Sub

    Public Sub WriteSwatMetInput(ByVal aMetWDM As atcDataSource,
                                 ByVal aModifiedData As atcTimeseriesGroup,
                                 ByVal aSaveInFolder As String,
                                 ByVal aDateStart As Double,
                                 ByVal aDateEnd As Double,
                                 ByVal aLocation As String)

        pModifiedData = aModifiedData
        Dim aWeaFile As String = String.Empty
        Dim aDSN As Integer = 0

        'write PREC
        aWeaFile = IO.Path.Combine(aSaveInFolder, "pcp" + aLocation + ".pcp")
        aDSN = CInt(pListDSN.ElementAt(0))
        WritePCP(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write PEVT
        aWeaFile = IO.Path.Combine(aSaveInFolder, "pet" + aLocation + ".pet")
        aDSN = CInt(pListDSN.ElementAt(1))
        WritePET(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write ATEM
        aWeaFile = IO.Path.Combine(aSaveInFolder, "tmp" + aLocation + ".tmp")
        aDSN = CInt(pListDSN.ElementAt(2))
        WriteTMP(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write SOLR
        aWeaFile = IO.Path.Combine(aSaveInFolder, "slr" + aLocation + ".slr")
        aDSN = CInt(pListDSN.ElementAt(3))
        WriteSLR(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write WIND
        aWeaFile = IO.Path.Combine(aSaveInFolder, "wnd" + aLocation + ".wnd")
        aDSN = CInt(pListDSN.ElementAt(4))
        WriteWND(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write DEWP
        aWeaFile = IO.Path.Combine(aSaveInFolder, "dew" + aLocation + ".dew")
        aDSN = CInt(pListDSN.ElementAt(5))
        WriteDEW(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)

        'write CLOU
        aWeaFile = IO.Path.Combine(aSaveInFolder, "clo" + aLocation + ".clo")
        aDSN = CInt(pListDSN.ElementAt(6))
        WriteCLO(aMetWDM, aWeaFile, aDSN, aDateStart, aDateEnd)
    End Sub

    Private Function WritePCP(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean

        Dim lCons As String = "PREC"
        Dim lTS As atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Return WritePCPSeries(lTS, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WritePCPSeries(ByVal aTS As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Dim lWriteDaily As Boolean = True
        Dim lTS As atcTimeseries = Nothing
        Try
            If aTS IsNot Nothing Then
                lTS = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv)
                lTS = EnsureComplete(lTS, aDateStart, aDateEnd, Nothing)
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(lTS)) 'Not processed by model
                lWriter.WriteLine(LatLonElev(lTS))

                lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 25.4) 'in to mm
                lTS.Attributes.SetValue("Units", "mm/day")
                Dim lDate As Date
                Dim lValue As Double
                For lIndex As Integer = 1 To lTS.numValues
                    Try
                        lDate = Date.FromOADate(lTS.Dates.Value(lIndex - 1))
                        lValue = lTS.Value(lIndex)
                        If Double.IsNaN(lValue) Then lValue = -99
                        If lWriteDaily Then 'Write daily format including just year and day
                            lWriter.WriteLine(YYYYddd(lDate) & f51(lValue))
                        Else 'Write sub-daily format including year, day and time
                            lWriter.WriteLine(YYYYddd(lDate) & Format(lDate, "HH:mm") & f51(lValue))
                        End If
                    Catch e As Exception
                        lWriter.WriteLine(YYYYddd(lDate) & f51(-99))
                    End Try
                Next
                lWriter.Close()
            End If
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Private Function WriteTMP(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean
        Dim lCons As String = "ATEM"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Return WriteTMPSeries(lTS, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WriteTMPSeries(ByVal aTS As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Try
            If aTS IsNot Nothing Then
                aTS.EnsureValuesRead()
                'lTS = atcData.SubsetByDate(lTS, aDateStart, aDateEnd, Nothing)
                aTS = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", aTS)
                aTS.Attributes.SetValue("Units", "C")

                Dim lTsMax As atcTimeseries = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranMax)
                lTsMax = EnsureComplete(lTsMax, aDateStart, aDateEnd, Nothing)

                Dim lTsMin As atcTimeseries = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranMin)
                lTsMin = EnsureComplete(lTsMin, aDateStart, aDateEnd, Nothing)

                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(aTS)) 'Not processed by model
                lWriter.WriteLine(LatLonElev(aTS))

                'lTS.Attributes.SetValueIfMissing("Units", "F")

                For lIndex As Integer = 1 To lTsMax.numValues
                    WriteTMPline(lWriter,
                             Date.FromOADate(lTsMax.Dates.Value(lIndex - 1)),
                             lTsMax.Value(lIndex),
                             lTsMin.Value(lIndex))
                Next

                lWriter.Close()

                aTS = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
                aTS = EnsureComplete(aTS, aDateStart, aDateEnd, Nothing)

                'Keep min and max timeseries for later use next to average
                aTS.Attributes.SetValue("tmax", lTsMax)
                aTS.Attributes.SetValue("tmin", lTsMin)
            End If

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Private Sub WriteTMPline(ByVal aWriter As IO.StreamWriter, ByVal aDate As Date, ByVal aMax As Double, ByVal aMin As Double)
        If aMax < aMin Then
            aMax = -99
            aMin = -99
        Else
            If Double.IsNaN(aMax) Then aMax = -99
            If Double.IsNaN(aMin) Then aMin = -99
        End If
        aWriter.WriteLine(YYYYddd(aDate) & f51(aMax) & f51(aMin))
    End Sub

    Private Function WriteSLR(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean
        Dim lCons As String = "SOLR" '"DSOL"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Return WriteSLRSeries(lTS, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WriteSLRSeries(ByVal aTS As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Try
            If aTS IsNot Nothing Then
                aTS.EnsureValuesRead()
                aTS = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv)
                aTS = EnsureComplete(aTS, aDateStart, aDateEnd, Nothing)

                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(aTS)) 'Not processed by model

                Dim lDate As Date
                Dim lValue As Double
                'lTS.Attributes.SetValueIfMissing("Units", "Langley")
                aTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", aTS, 0.484 * 0.0864) 'convert Langley to MJ/m2
                aTS.Attributes.SetValue("Units", "MJ/m2/day")

                'If lDaily Then
                For lIndex As Integer = 1 To aTS.numValues
                    lDate = Date.FromOADate(aTS.Dates.Value(lIndex - 1)) 'begin of interval
                    lValue = aTS.Value(lIndex)
                    If Double.IsNaN(lValue) Then lValue = -99
                    lWriter.WriteLine(YYYYddd(lDate) & f83(lValue))
                Next

                lWriter.Close()
            End If

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Private Function WriteWND(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean
        Dim lCons As String = "WIND"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Return WriteWNDSeries(lTS, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WriteWNDSeries(ByVal aTS As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Try
            If aTS IsNot Nothing Then
                aTS.EnsureValuesRead()
                aTS = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)

                aTS = EnsureComplete(aTS, aDateStart, aDateEnd, Nothing)

                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(aTS)) 'Not processed by model

                'lTS.Attributes.SetValueIfMissing("Units", "mph")
                aTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", aTS, 0.44704) 'convert wind speed miles per hour to meters per second
                aTS.Attributes.SetValue("Units", "m/s")
                Dim lDate As Date
                Dim lValue As Double
                For lIndex As Integer = 1 To aTS.numValues
                    lDate = Date.FromOADate(aTS.Dates.Value(lIndex - 1))
                    lValue = aTS.Value(lIndex)
                    If Double.IsNaN(lValue) Then lValue = -99
                    lWriter.WriteLine(YYYYddd(lDate) & f83(lValue))
                Next
                lWriter.Close()
            End If

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    'Write the SWAT Potential EvapoTranspiration file and return the timeseries used to write the file
    Private Function WritePET(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean
        Dim lCons As String = "PEVT"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Return WritePETSeries(lTS, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WritePETSeries(ByVal aTS As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Try
            If aTS IsNot Nothing Then
                aTS.EnsureValuesRead()
                aTS = atcData.modTimeseriesMath.Aggregate(aTS, atcTimeUnit.TUDay, 1, atcTran.TranSumDiv)

                aTS = EnsureComplete(aTS, aDateStart, aDateEnd, Nothing)

                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(aTS)) 'Not processed by model
                lWriter.WriteLine(LatLonElev(aTS))

                'lTS.Attributes.SetValueIfMissing("Units", "in")
                aTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", aTS, 25.4)
                aTS.Attributes.SetValue("Units", "mm/day")
                Dim lDate As Date
                Dim lValue As Double
                For lIndex As Integer = 1 To aTS.numValues
                    lDate = Date.FromOADate(aTS.Dates.Value(lIndex - 1))
                    lValue = aTS.Value(lIndex)
                    If Double.IsNaN(lValue) Then lValue = -99
                    lWriter.WriteLine(YYYYddd(lDate) & f51(lValue))
                Next
                lWriter.Close()
            End If

        Catch ex As Exception

        End Try
        Return True
    End Function

    'Write the SWAT relative humidity file and return the timeseries used to write the file
    Private Function WriteHMD(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As Boolean
        Dim lCons As String = "DEWP"
        Dim lDSN As Integer = CInt(pListDSN.ElementAt(2))
        Dim lDew As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        Dim lTemp As atcData.atcTimeseries = GetWDMTimeseries(aWDM, lDSN, "ATEM")

        Return WriteHMDSeries(lDew, lTemp, aWeaFile, aDateStart, aDateEnd)
    End Function

    Private Function WriteHMDSeries(ByVal lDew As atcTimeseries,
                              ByVal lTemp As atcTimeseries,
                              ByVal aWeaFile As String,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As Boolean
        Dim lHMD As atcTimeseries = Nothing
        Try
            If lDew IsNot Nothing AndAlso lTemp IsNot Nothing Then
                lDew.EnsureValuesRead()
                lTemp.EnsureValuesRead()
                lTemp = atcData.modTimeseriesMath.Aggregate(lTemp, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
                lTemp = EnsureComplete(lTemp, aDateStart, aDateEnd, Nothing)
                lTemp = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lTemp)

                lDew = atcData.modTimeseriesMath.Aggregate(lDew, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)
                lDew = EnsureComplete(lDew, aDateStart, aDateEnd, Nothing)
                lDew = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lDew)

                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aWeaFile))
                Dim lWriter As New IO.StreamWriter(aWeaFile)
                lWriter.WriteLine(MetHeader(lDew)) 'Not processed by model
                lWriter.WriteLine(LatLonElev(lDew))

                lHMD = lDew.Clone
                With lHMD.Attributes
                    .SetValue("Constituent", "Relative Humidity")
                    .SetValue("Units", "fraction")
                End With

                Dim lDate As Date
                Dim lDewValue As Double
                Dim lTempValue As Double
                Dim lValue As Double
                For lIndex As Integer = 1 To lDew.numValues
                    lDate = Date.FromOADate(lDew.Dates.Value(lIndex - 1))

                    lDewValue = lDew.Value(lIndex)
                    lTempValue = lTemp.Value(lIndex)

                    If lDewValue > lTempValue Then
                        Debug.WriteLine("dew > temp, " & DoubleToString(lDewValue) & " > " & DoubleToString(lTempValue))
                    End If

                    lValue = Math.Exp((17.269 * lDewValue) / (273.3 + lDewValue)) _
                       / Math.Exp((17.269 * lTempValue) / (273.3 + lTempValue))
                    lHMD.Value(lIndex) = lValue
                    If Double.IsNaN(lValue) Then lValue = -99
                    lWriter.WriteLine(YYYYddd(lDate) & f83(lValue))
                Next
                lWriter.Close()
            End If

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    'returns the dewpoint timeseries in degrees C, does not write SWAT input
    Private Function WriteDEW(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double, ByVal aDateEnd As Double) As atcTimeseries
        Dim lCons As String = "DEWP"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        If lTS IsNot Nothing Then
            lTS.EnsureValuesRead()
            lTS = atcData.modTimeseriesMath.Aggregate(lTS, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)

            lTS = EnsureComplete(lTS, aDateStart, aDateEnd, Nothing)

            lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("F to Celsius", lTS)
            lTS.Attributes.SetValue("Units", "C")
        End If
        Return lTS
    End Function

    'return the filled cloud cover dataset, does not write SWAT input
    Private Function WriteCLO(ByVal aWDM As atcDataSourceWDM,
                              ByVal aWeaFile As String,
                              ByVal aDSN As Integer,
                              ByVal aDateStart As Double,
                              ByVal aDateEnd As Double) As atcTimeseries
        Dim lCons As String = "CLOU"
        Dim lTS As atcData.atcTimeseries = GetWDMTimeseries(aWDM, aDSN, lCons)
        If lTS IsNot Nothing Then
            lTS = atcData.modTimeseriesMath.Aggregate(lTS, atcTimeUnit.TUDay, 1, atcTran.TranAverSame)

            lTS = EnsureComplete(lTS, aDateStart, aDateEnd, Nothing)

            'lTS.Attributes.SetValueIfMissing("Units", "tenths")
            lTS = atcTimeseriesMath.atcTimeseriesMath.Compute("Multiply", lTS, 0.1)
            lTS.Attributes.SetValue("Units", "fraction")
        End If
        Return lTS
    End Function

    ''' <summary>
    ''' Return the modified data with this constituent if any, otherwise return the data from the given data source
    ''' </summary>
    ''' <param name="aWDM">Data source containing original data</param>
    ''' <param name="aWDMConstituentName">Name of constituent to search for</param>
    ''' <returns>uses module variable pModifiedData</returns>
    ''' <remarks>Does not yet support multiple met stations in one run</remarks>
    Private Function OLDGetWDMTimeseries(ByVal aWDM As atcDataSource, ByVal aWDMConstituentName As String) As atcTimeseries
        ' Search modified data for given constituent
        If pModifiedData IsNot Nothing Then
            For Each lTSsearch As atcData.atcTimeseries In pModifiedData
                Select Case lTSsearch.Attributes.GetValue("Constituent", "")
                    Case aWDMConstituentName : Return lTSsearch
                End Select
            Next
        End If
        ' Search for this data in original data source
        For Each lTSsearch As atcData.atcTimeseries In aWDM.DataSets
            Select Case lTSsearch.Attributes.GetValue("Constituent", "")
                Case aWDMConstituentName : Return lTSsearch
            End Select
        Next
        'Logger.Dbg("No " & aWDMConstituentName & " available in " & aWDM.Specification)
        Return Nothing
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
        Logger.Dbg("No " & aConstituent & ": DSN" & aDSN.ToString() + " available in " & aWDM.Specification)
        Return Nothing
    End Function

    Private Function MetHeader(ByVal aTimeseries As atcData.atcTimeseries) As String
        With aTimeseries.Attributes
            Return .GetValue("Constituent", "") & " at " _
                 & .GetValue("Location", "") & ", " _
                 & .GetValue("STANAM", "") _
                 & " data from BASINS via D4EM/EDDT"
        End With
    End Function

    Private Function LatLonElev(ByVal aTimeseries As atcData.atcTimeseries) As String
        With aTimeseries.Attributes
            'Lat and Lon not processed by model
            Return ("LatDeg " & DoubleToString(CDbl(.GetValue("Latitude", "0")), 5) & vbCrLf _
                  & "LonDeg " & DoubleToString(CDbl(.GetValue("Longitude", "0")), 5) & vbCrLf _
                  & "Elev(m)" & Format(CInt(.GetValue("Elevation", "0")), "00000"))
            'TODO: populate station list with elevation, add to WDM attributes, convert to meters
        End With
    End Function

    Private Function f51(ByVal aValue As Double) As String
        If aValue < 0 Then
            Return Format(aValue, "#0.0").PadLeft(5)
        Else
            Return Format(aValue, "##0.0").PadLeft(5)
        End If
    End Function

    Private Function f83(ByVal aValue As Double) As String
        If aValue < 0 Then
            Return Format(aValue, "##0.000").PadLeft(8)
        Else
            Return Format(aValue, "###0.000").PadLeft(8)
        End If
    End Function

    Private Function YYYYddd(ByVal aDate As Date) As String
        Return aDate.Year & Format(aDate.DayOfYear, "000")
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

    '    Private Sub EnsureComplete(ByVal aStartDate As Double, _
    '                               ByVal aEndDate As Double, _
    '                               ByVal aDataSource As atcDataSource, _
    '                               ByVal aTimeseriesOut As atcDataGroup, _
    '                               ByVal ParamArray aTimeseriesIn() As atcTimeseries)

    '        Dim lLastTsIndex As Integer = aTimeseriesIn.GetUpperBound(0)
    '        Dim lTsIndex As Integer
    '        Dim lOverlapTimeseries(lLastTsIndex) As atcTimeseries
    '        Dim lNewTimeseries As atcTimeseries
    '        Dim lReport As New Text.StringBuilder

    '        lReport.AppendLine("Complete date range from " & DoubleToString(aStartDate) & " to " & DoubleToString(aEndDate) & " for " & lLastTsIndex - 1 & " timeseries")

    '        For lTsIndex = 0 To lLastTsIndex
    '            lOverlapTimeseries(lTsIndex) = SubsetByDate(aTimeseriesIn(lTsIndex), aStartDate, aEndDate, aDataSource)
    '        Next


    '        If Math.Abs(lOverlapTimeseries.Dates.Value(0) - aStartDate) < JulianSecond AndAlso _
    '           Math.Abs(lOverlapTimeseries.Dates.Value(lOverlapTimeseries.numValues) - aEndDate) < JulianSecond Then
    '            'Requested time 
    '            lNewTimeseries = lOverlapTimeseries
    '        Else
    '            lNewTimeseries = NewTimeseries(aStartDate, aEndDate, atcTimeUnit.TUDay, 1, , GetNaN)

    '            'First copy overlapping values into new timeseries
    '            Dim lOverlapStart As Integer = FindDateAtOrAfter(lNewTimeseries.Dates.Values, lOverlapTimeseries.Dates.Value(1))
    '            System.Array.Copy(lOverlapTimeseries.Values, 1, lNewTimeseries.Values, lOverlapStart, lOverlapTimeseries.numValues)
    '        End If

    '        'Fill in missing values from same date in another year of input data
    '        Dim lOldIndex As Integer = 1
    '        Dim lOldDateArray(5) As Integer
    '        Dim lNewDateArray(5) As Integer

    '        For lNewIndex As Integer = 1 To lNewTimeseries.numValues
    '            If Double.IsNaN(lNewTimeseries.Value(lNewIndex)) Then
    '                J2Date(lNewTimeseries.Dates.Value(lNewIndex), lNewDateArray)
    '                Dim lSearchOldIndex As Integer = lOldIndex
    '                While lSearchOldIndex <= aTimeseries.numValues
    '                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
    '                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
    '                        If lOldDateArray(1) = lNewDateArray(1) AndAlso lOldDateArray(2) = lNewDateArray(2) Then
    '                            lReport.AppendLine("Missing value for " & lNewDateArray(0) & "/" & lNewDateArray(1) & "/" & lNewDateArray(2) & ", copied from " & lOldDateArray(0) & "/" & lOldDateArray(1) & "/" & lOldDateArray(2) & ", " & aTimeseries.Value(lSearchOldIndex))
    '                            GoTo FoundOldDate
    '                        End If
    '                    End If
    '                    lSearchOldIndex += 1
    '                End While
    '                lSearchOldIndex = 1
    '                While lSearchOldIndex < lOldIndex
    '                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
    '                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
    '                        If lOldDateArray(1) = lNewDateArray(1) AndAlso lOldDateArray(2) = lNewDateArray(2) Then
    '                            GoTo FoundOldDate
    '                        End If
    '                    End If
    '                    lSearchOldIndex += 1
    '                End While

    '                lSearchOldIndex = lOldIndex
    '                Dim lNearestIndex As Integer = lOldIndex
    '                Dim lNearestDays As Integer = 500
    '                Dim lDays As Integer
    '                While lSearchOldIndex <= aTimeseries.numValues
    '                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
    '                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
    '                        If lOldDateArray(1) = lNewDateArray(1) Then
    '                            lDays = Math.Abs(lOldDateArray(2) - lNewDateArray(2))
    '                            If lDays < lNearestDays Then lNearestIndex = lSearchOldIndex
    '                        End If
    '                    End If
    '                    lSearchOldIndex += 1
    '                End While
    '                lSearchOldIndex = 1
    '                While lSearchOldIndex < lOldIndex
    '                    If Not Double.IsNaN(aTimeseries.Value(lSearchOldIndex)) Then
    '                        J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
    '                        If lOldDateArray(1) = lNewDateArray(1) Then
    '                            lDays = Math.Abs(lOldDateArray(2) - lNewDateArray(2))
    '                            If lDays < lNearestDays Then lNearestIndex = lSearchOldIndex
    '                        End If
    '                    End If
    '                    lSearchOldIndex += 1
    '                End While

    '                lSearchOldIndex = lNearestIndex
    '                J2Date(aTimeseries.Dates.Value(lSearchOldIndex), lOldDateArray)
    '                lReport.AppendLine("Value not found for " & lNewDateArray(0) & "/" & lNewDateArray(1) & "/" & lNewDateArray(2) & " using value from " & lNearestDays & " days away: " & lOldDateArray(0) & "/" & lOldDateArray(1) & "/" & lOldDateArray(2) & ", " & aTimeseries.Value(lSearchOldIndex))
    'FoundOldDate:
    '                lNewTimeseries.Value(lNewIndex) = aTimeseries.Value(lSearchOldIndex)
    '                lNewTimeseries.ValueAttributes(lNewIndex).SetValue("Original Date", aTimeseries.Dates.Value(lSearchOldIndex))
    '            End If
    '        Next

    '        lNewTimeseries.Attributes.SetValue("Description", lReport.ToString)
    '        Return lNewTimeseries

    '    End Sub

End Module
