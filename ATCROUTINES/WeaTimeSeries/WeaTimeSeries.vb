Option Strict Off
Option Explicit On

Imports atcData
Imports atcUtility
Imports MapWinUtility
Imports System.Collections
Imports System.IO

Friend Class WeaTimeSeries
    Inherits atcTimeseriesSource
    Public Function OpenSeries(ByVal aDictSeries As SortedDictionary(Of Date, String),
                          Optional ByVal aAttributes As atcData.atcDataAttributes = Nothing) As Boolean
        Dim lOpened As Boolean = False
        Try
            lOpened = ReadDictSeries(aDictSeries)
        Catch e As Exception
            Logger.Dbg("Exception reading input series " & e.Message, e.StackTrace)
            Return False
        End Try
        Return lOpened
    End Function

    Private Function ReadDictSeries(ByVal aDictTSeries As SortedDictionary(Of Date, String)) As Boolean
        Dim lNaN As Double = GetNaN()
        Dim lBuilder As New atcTimeseriesBuilder(Me)
        Dim lCons As String = String.Empty

        For Each kv As KeyValuePair(Of Date, String) In aDictTSeries
            Dim dValue As Double = Convert.ToDouble(kv.Value)
            lBuilder.AddValue(kv.Key, dValue)
        Next

        If lBuilder.NumValues > 0 Then
            Dim lTimeseries As atcTimeseries = lBuilder.CreateTimeseries()

            lTimeseries.SetInterval(atcTimeUnit.TUHour, 1)
            DataSets.Add(lTimeseries)
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub New()
    End Sub
End Class
