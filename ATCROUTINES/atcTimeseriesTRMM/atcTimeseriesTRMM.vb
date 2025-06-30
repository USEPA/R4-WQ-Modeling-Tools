Option Strict Off
Option Explicit On

Imports atcData
Imports atcUtility
Imports MapWinUtility
Imports System.Collections
Imports System.IO

''' <summary>
''' Reads GrADS Data Server ASCII or TRMM timeseries.cgi files containing time-series values
''' </summary>
''' <remarks>
''' http://hydro1.sci.gsfc.nasa.gov/daac-bin/access/timeseries.cgi
''' http://www.iges.org/grads/gds/
''' http://disc.sci.gsfc.nasa.gov/additional/faq/hydrology_disc_faq.shtml#GDS_retrieve
''' </remarks>
Public Class atcTimeseriesTRMM
    Inherits atcTimeseriesSource

    Private Shared pFilter As String = "NASA TRMM (*.TRMM.txt)|*.TRMM.txt|NASA GDS (*.gds)|*.gds|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
    Private Shared pASC2FirstLine As String = "Metadata for Requested Time Series:"
    Private Shared pASC2DataHeader As String = "Date&Time               Data"
    Private pErrorDescription As String
    Private pJulianOffset As Double = New Date(1900, 1, 1).Subtract(New Date(1, 1, 1)).TotalDays

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "NASA TRMM File"
        End Get
    End Property

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "Timeseries::NASA TRMM"
        End Get
    End Property

    Public Overrides ReadOnly Property Category() As String
        Get
            Return "File"
        End Get
    End Property

    Public Overrides ReadOnly Property CanOpen() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides Function Open(ByVal aFileName As String,
                          Optional ByVal aAttributes As atcData.atcDataAttributes = Nothing) As Boolean
        Dim lOpened As Boolean = False
        If aFileName Is Nothing OrElse aFileName.Length = 0 OrElse Not FileExists(aFileName) Then
            aFileName = FindFile("Select " & Name & " file to open", , , Filter, True, , 1)
        End If

        If Not FileExists(aFileName) Then
            pErrorDescription = "File '" & aFileName & "' not found"
        Else
            Me.Specification = aFileName

            Try
                Using lInputStream As New FileStream(aFileName, FileMode.Open, FileAccess.Read)
                    Dim lInputBuffer As New BufferedStream(lInputStream)
                    Dim lInputReader As New BinaryReader(lInputBuffer)
                    Dim lFirstLine As String = NextLine(lInputReader)
                    If lFirstLine.StartsWith("ERROR") Then
                        While lFirstLine.StartsWith("ERROR")
                            Logger.Dbg(lFirstLine)
                            lFirstLine = NextLine(lInputReader)
                        End While
                        Return False
                    ElseIf lFirstLine.Equals(pASC2FirstLine) Then
                        lOpened = ReadASC2(lInputReader)
                    Else
                        'lOpened = ReadGDS(lFirstLine, lInputReader)
                    End If
                End Using
            Catch e As Exception
                Logger.Dbg("Exception reading '" & aFileName & "': " & e.Message, e.StackTrace)
                Return False
            End Try
        End If
        Return lOpened
    End Function

    Private Function ReadASC2(ByVal lInputReader As BinaryReader) As Boolean
        Dim lNaN As Double = GetNaN()
        Dim lUndef As String = ""
        Dim lBuilder As New atcTimeseriesBuilder(Me)

        Dim lCurLineString As String = NextLine(lInputReader)
        Dim lCurLine() As String
        Dim first As Boolean = vbTrue
        Dim lCons As String = String.Empty

        While lCurLineString <> pASC2DataHeader
            lCurLine = lCurLineString.Split("=")
            If lCurLine.Length = 2 Then
                Dim lAttName As String = lCurLine(0).Trim
                Dim lAttValue As String = lCurLine(1).Trim
                Select Case lAttName
                    Case "time_interval(hour)", "tot_record", "start_lat", "start_lon", "dlat", "dlon"
                        'Skip these
                    Case "param_short_name"
                        lBuilder.Attributes.SetValue("Constituent", lAttValue)
                        lCons = lAttValue
                    Case "param_name"
                        lBuilder.Attributes.SetValue("Description", lAttValue)
                    Case "undef"
                        lUndef = lAttValue
                    Case "unit"
                        lBuilder.Attributes.SetValue("Units", lAttValue)
                    Case "lat"
                        lBuilder.Attributes.SetValue("Latitude", lAttValue)
                    Case "lon"
                        lBuilder.Attributes.SetValue("Longitude", lAttValue)
                        'GPF added 11/23/20, TRMM dose not have elevation
                        lBuilder.Attributes.SetValue("Elevation", "-999")
                    Case Else
                        lBuilder.Attributes.SetValue(lAttName, lAttValue)
                End Select
            End If
            lCurLineString = NextLine(lInputReader)
        End While

        Dim irec As Integer = 0
        Dim lDate As Date, lValue As Double
        Dim dictTS As SortedDictionary(Of Date, Double) = New SortedDictionary(Of Date, Double)()

        Do
            Try
                lCurLineString = NextLine(lInputReader)
                'If Date.TryParse(SafeSubstring(lCurLineString, 0, 20).Trim.Replace("Z", ":00"), lDate) AndAlso
                'GPF 11/23/2020
                If Date.TryParse(SafeSubstring(lCurLineString, 0, 19).Trim, lDate) AndAlso
                    Double.TryParse(SafeSubstring(lCurLineString, 20), lValue) Then
                    irec += 1
                    Dim NewDate As New System.DateTime(lDate.Year, lDate.Month,
                                                       lDate.Day, lDate.Hour, 0, 0)
                    'zero out missing 11/23/20
                    If (lValue < -9990) Then
                        lValue = 9999
                    Else
                        lValue = lValue / 25.4
                    End If

                    If (Not dictTS.ContainsKey(NewDate)) Then
                            dictTS.Add(NewDate, lValue)
                        End If
                    End If
            Catch ex As EndOfStreamException
                Exit Do
            End Try
        Loop

        'interpolate and add to lBuilder
        Dim lsDate As Date() = dictTS.Keys.ToArray()
        Dim lsValue As Double() = dictTS.Values.ToArray()

        'Debug.WriteLine("Constituent =" + lCons)
        dictTS = InterpolateHourly(lCons, lsDate, lsValue)
        For Each kv As KeyValuePair(Of Date, Double) In dictTS
            lBuilder.AddValue(kv.Key, kv.Value)
        Next
        Debug.WriteLine("lBuilder Num = " + lBuilder.NumValues.ToString())
        Debug.WriteLine("dictTS NumRecs = " + dictTS.Keys.Count.ToString())
        dictTS = Nothing

        If lBuilder.NumValues > 0 Then
            Dim lTimeseries As atcTimeseries = lBuilder.CreateTimeseries()
            With lTimeseries.Attributes
                .AddHistory("Read from " & Specification)
                .SetValue("Scenario", "TRMM")
                .SetValue("Location", "X" & .GetValue("grid_x", "").ToString.PadLeft(3, "0"c) _
                                    & "Y" & .GetValue("grid_y", "").ToString.PadLeft(3, "0"c))
            End With
            lTimeseries.SetInterval(atcTimeUnit.TUHour, 1)
            DataSets.Add(lTimeseries)
            Return True
        Else
            Return False
        End If
    End Function

    Private Function InterpolateHourly(lParam As String, lDate As Date(), lValue As Double()) As SortedDictionary(Of Date, Double)

        Dim dictTS As SortedDictionary(Of Date, Double) = New SortedDictionary(Of Date, Double)()
        Dim ldt As Date
        Dim lval As Double
        Dim dtdiff As Integer

        'Debug.WriteLine("In interpolate: Numvalues =" + lDate.Count.ToString())
        Dim prelDate As Date = lDate.ElementAt(0)
        Dim prelValue As Double = lValue.ElementAt(0)
        Debug.WriteLine("{0},{1}", prelDate.ToString(), prelValue.ToString("0.000000"))

        dictTS.Add(prelDate, prelValue)
        If lParam.ToUpper().Contains("PRECIPITATION") Then
            For idx As Integer = 1 To lDate.Length - 1
                dtdiff = CInt(DateDiff("h", prelDate, lDate(idx)))
                'use contant
                For ihr As Integer = 1 To dtdiff - 1
                    ldt = prelDate.AddHours(ihr)
                    lval = lValue.ElementAt(idx)
                    dictTS.Add(ldt, lval)
                    Debug.WriteLine("{0},{1},{2}", ihr.ToString(), ldt.ToString(), lval.ToString("0.000000"))
                Next
                dictTS.Add(lDate.ElementAt(idx), lValue.ElementAt(idx))
                Debug.WriteLine("{0},{1},{2}", dtdiff.ToString(), lDate.ElementAt(idx).ToString(), lValue.ElementAt(idx).ToString("0.000000"))
                prelDate = lDate.ElementAt(idx)
            Next
        End If
        Return dictTS
    End Function

    Public Sub New()
        Filter = pFilter
    End Sub
End Class
