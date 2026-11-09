Imports System.Data
Imports System.Linq
Imports System.Collections
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Globalization
Imports atcData

Public Class clsWea
    Private WdmFile As String, AirFile As String
    Private lstDSN As List(Of Integer)
    Private BegYr As Integer, EndYr As Integer
    Private BegSim As DateTime, EndSim As DateTime
    Private lblStatus As ToolStripStatusLabel
    Private lblStrip As StatusStrip
    Private wrlog As StreamWriter
    Private lWdmDS As atcWDM.atcDataSourceWDM
    Private lBaseDSN As Integer, numDS As Integer, curDSN As Integer
    Private dbTable As DataTable
    Private dictAir As New SortedDictionary(Of DateTime, List(Of Double))

    Public Sub New(ByVal _wdm As String, ByVal _air As String,
                   ByVal _lstDSN As List(Of Integer),
                   ByVal _begsim As DateTime, ByVal _endsim As DateTime,
                   ByVal _strip As Object, ByVal _label As Object,
                   ByVal _wrlog As StreamWriter)
        WdmFile = _wdm
        AirFile = _air
        lstDSN = _lstDSN
        BegSim = _begsim
        EndSim = _endsim
        wrlog = _wrlog
        lblStatus = _label
        lblStrip = _strip

        If Not OpenWDM() Then Return
        Debug.WriteLine("Opened WDM File")
        Dim msg = "Writing " + AirFile.Trim() + " ..."
        WriteStatus(msg)
        ProcessAirFile()
        WriteAirFile()
        CloseWDM()
    End Sub

    Private Sub SetDataTable()
        dbTable = New DataTable
        dbTable.Columns.Add("DateTime", GetType(DateTime))
        dbTable.Columns.Add("PREC", GetType(String))
        dbTable.Columns.Add("PEVT", GetType(String))
        dbTable.Columns.Add("ATEM", GetType(String))
        dbTable.Columns.Add("SOLR", GetType(String))
        dbTable.Columns.Add("WIND", GetType(String))
        dbTable.Columns.Add("DEWP", GetType(String))
        dbTable.Columns.Add("CLOU", GetType(String))
    End Sub

    Private Sub WriteLog(ByVal msg As String)
        wrlog.WriteLine(msg)
    End Sub

    Private Sub WriteStatus(ByVal msg As String)
        lblStatus.Text = msg
        lblStrip.Refresh()
    End Sub

    Private Function OpenWDM() As Boolean
        lWdmDS = New atcWDM.atcDataSourceWDM
        Try
            lWdmDS.Open(WdmFile)
            Dim numds As Integer = lWdmDS.DataSets.Count
            lBaseDSN = GetMaxDSN(lWdmDS)
            'WriteLog("Number of Datasets in datasource is  " + numds.ToString())
            'WriteLog("Maximum DSN ID is  " + lBaseDSN.ToString())
        Catch ex As Exception
            Dim msg = "Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source
            WriteLog(msg)
            ErrorMessage(msg)
            msg = Nothing
            Return False
        End Try
        Return True
    End Function

    Public Sub CloseWDM()
        lWdmDS = Nothing
    End Sub

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

    Private Sub ProcessAirFile()

        Debug.WriteLine("Executing ProcessAirFile()...")
        Dim lseries As atcData.atcTimeseries
        Dim svar As String = String.Empty, msg As String

        Try
            lWdmDS.Open(WdmFile)
            'Debug.WriteLine("Num datasets = " + lWdmDS.DataSets.Count.ToString() + ", DSN Count=" + lstDSN.Count.ToString())
            numDS = lWdmDS.DataSets.Count

            'process series for all dsn (lstDSN) in an airfile
            For ivar As Integer = 0 To lstDSN.Count - 1
                lseries = New atcData.atcTimeseries(Nothing)
                curDSN = lstDSN(ivar)
                lseries = GetSeries(curDSN)
                svar = lseries.Attributes.GetValue("Constituent").Trim()
                msg = AirFile + ", " + curDSN.ToString() + ", " + svar + ", " + lseries.numValues.ToString()
                'Debug.WriteLine(msg)
                ReadWeatherSeries(ivar, lseries, curDSN)
                lseries = Nothing
            Next
        Catch ex As Exception
            Dim err = "Error processing series with DSN " + curDSN + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(err)
            err = Nothing
        End Try
    End Sub

    Private Sub ReadWeatherSeries(ByVal ivar As Integer, ByVal ts As atcTimeseries, ByVal curDSN As Integer)
        Debug.WriteLine("Executing ReadWeatherSeries() for DSN " + curDSN.ToString())
        Cursor.Current = Cursors.WaitCursor
        Dim lst As List(Of Double), idx As Integer = 0
        Try
            For j As Integer = 0 To ts.numValues - 1
                Dim ddate As Date = Date.FromOADate(ts.Dates.Value(j))
                'If ddate.Year >= BegYr And ddate.Year <= EndYr Then
                If ddate >= BegSim And ddate <= EndSim Then
                    idx += 1
                    Dim v As Double = ts.Value(j + 1) 'per email from Mark 12/6/07 need to use next index
                    'If (idx <= 24) Then
                    'Debug.WriteLine(ddate.ToString() + ", " + v.ToString())
                    'End If

                    lst = New List(Of Double)
                    If ivar = 0 Then
                        lst.Add(v)
                        dictAir.Add(ddate, lst)
                    Else
                        dictAir.TryGetValue(ddate, lst)
                        lst.Add(v)
                        dictAir(ddate) = lst
                    End If
                    lst = Nothing
                End If
            Next
        Catch ex As Exception
            Dim msg = "Error reading series " + curDSN + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(msg)
            msg = Nothing
        End Try
        Cursor.Current = Cursors.Default
    End Sub

    Private Function GetSeries(ByVal ldsn As Integer) As atcTimeseries
        Dim dsn As Integer, loc As String = String.Empty, svar As String
        Dim lseries As New atcData.atcTimeseries(Nothing)

        Debug.WriteLine("Executing GetSeries() for DSN " + ldsn.ToString())
        Try
            'lseries = lWdmDS.DataSets.FindData("ID", ldsn)(0)
            'If Not IsNothing(lseries) Then
            'Return lseries
            'End If
        Catch ex As Exception
            'Dim msg = "Error getting series " + loc + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            'ErrorMessage(msg)
            'msg = Nothing
            'Return Nothing
        End Try

        Try
            numDS = lWdmDS.DataSets.Count
            For i As Integer = 0 To numDS - 1
                dsn = lWdmDS.DataSets(i).Attributes.GetValue("ID")
                loc = lWdmDS.DataSets(i).Attributes.GetValue("Location").Trim()
                svar = lWdmDS.DataSets(i).Attributes.GetValue("Constituent").Trim()
                'Debug.WriteLine(dsn.ToString() + "," + loc.ToString())
                If ldsn = dsn Then
                    Debug.WriteLine("DataSetNum is " + i.ToString() + ", location=" + loc + ", dsn=" + dsn.ToString() + ", var=" + svar)
                    lseries = lWdmDS.DataSets(i)
                    Return lseries
                    Exit For
                End If
            Next
        Catch ex As Exception
            Dim msg = "Error getting series " + loc + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(msg)
            msg = Nothing
            Return Nothing
        End Try
        Return lseries
    End Function

    Private Sub WriteAirFile()
        Cursor.Current = Cursors.WaitCursor
        Dim sr As New IO.StreamWriter(AirFile)
        Dim sep() As String = {"/", ":", " "}
        Dim year As Integer, mon As Integer, day As Integer, hr As Integer
        Dim lst As List(Of Double)

        'write headers
        Try
            sr.WriteLine("1    LSPC METEOROLOGICAL DATA FILE FOR DRIVING WATERSHED MODEL")
            sr.WriteLine("1    Time interval:  60 mins          Last month in printout year: 12")
            sr.WriteLine("1    No. of curves plotted:  Point-valued:  0   Mean-valued:  7   Total  7")
            sr.WriteLine("1    Label flag:  0          Pivl:    1          Idelt:  60")
            sr.WriteLine("1    Plot title: " + AirFile)
            sr.WriteLine("1    Scale info:  Ymin:  0.00000               Threshold:-0.10000E+31")
            sr.WriteLine("1                 Ymax:   1000.0")
            sr.WriteLine("1                 Time:   20.000     intervals/inch")
            sr.WriteLine("1    Data for each curve (Point-valued first, then mean-valued):")
            sr.WriteLine("1    Label                   LINTYP     INTEQ    COLCOD      TRAN   TRANCOD")
            sr.WriteLine("1    PRECIPI IN/TIMESTEP          0         5         1      AVER         2")
            sr.WriteLine("1    POT ET, IN/TIMESTEP          0         5         1      AVER         2")
            sr.WriteLine("1    AIR TEMP, DEG F              0         5         1      AVER         2")
            sr.WriteLine("1    WIND SPEED,MI/TIMESTEP       0         5         1      AVER         2")
            sr.WriteLine("1    SOLAR RAD,LY/TIMESTEP        0         5         1      AVER         2")
            sr.WriteLine("1    DEW POINT, DEG F             0         5         1      AVER         2")
            sr.WriteLine("1    CLOUD COVER, TENTHS          0         5         1      AVER         2")
            sr.WriteLine("1   ")
            sr.WriteLine("1   ")
            sr.WriteLine("1    Time series (pt-valued, then mean-valued):")
            sr.WriteLine("1   ")
            sr.WriteLine("1    Date/time                      Values")
            sr.WriteLine("1   ")

            For Each kv As KeyValuePair(Of DateTime, List(Of Double)) In dictAir
                lst = New List(Of Double)
                dictAir.TryGetValue(kv.Key, lst)
                year = kv.Key.Year
                mon = kv.Key.Month
                day = kv.Key.Day
                hr = kv.Key.Hour + 1

                Dim strng As New StringBuilder
                strng.Append("1")
                strng.Append("    " + year.ToString("0000")) 'year
                strng.Append(" " + mon.ToString("00")) 'month
                strng.Append(" " + day.ToString("00")) 'day
                strng.Append(" " + hr.ToString("00")) 'hr
                strng.Append(" " + "0")
                strng.Append(" " + lst(0).ToString("0.00000")) 'PREC
                strng.Append(" " + lst(1).ToString("0.000000")) 'PEVT
                strng.Append(" " + lst(2).ToString("#00.00")) 'ATEM
                strng.Append(" " + lst(3).ToString("#0.00")) 'WIND
                strng.Append(" " + lst(4).ToString("#0.00000")) 'SOLR
                strng.Append(" " + lst(5).ToString("#00.0")) 'DEWP
                strng.Append(" " + lst(6).ToString("#.00")) 'CLOU
                sr.WriteLine(strng.ToString())
                lst = Nothing
            Next
        Catch ex As Exception
            Dim msg = "Error writing " + AirFile + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(msg)
            msg = Nothing
        Finally
            sr.Close()
            sr.Dispose()
        End Try
        Cursor.Current = Cursors.Default
    End Sub

    Private Sub WriteAirFile_UseDataTable()
        Dim sr As New IO.StreamWriter(AirFile)
        Dim sep() As String = {"/", ":", " "}
        'write headers
        Try
            sr.WriteLine("1    LSPC METEOROLOGICAL DATA FILE FOR DRIVING WATERSHED MODEL")
            sr.WriteLine("1    Time interval:  60 mins          Last month in printout year: 12")
            sr.WriteLine("1    No. of curves plotted:  Point-valued:  0   Mean-valued:  7   Total  7")
            sr.WriteLine("1    Label flag:  0          Pivl:    1          Idelt:  60")
            sr.WriteLine("1    Plot title: " + AirFile)
            sr.WriteLine("1    Scale info:  Ymin:  0.00000               Threshold:-0.10000E+31")
            sr.WriteLine("1                 Ymax:   1000.0")
            sr.WriteLine("1                 Time:   20.000     intervals/inch")
            sr.WriteLine("1    Data for each curve (Point-valued first, then mean-valued):")
            sr.WriteLine("1    Label                   LINTYP     INTEQ    COLCOD      TRAN   TRANCOD")
            sr.WriteLine("1    PRECIPI IN/TIMESTEP          0         5         1      AVER         2")
            sr.WriteLine("1    POT ET, IN/TIMESTEP          0         5         1      AVER         2")
            sr.WriteLine("1    AIR TEMP, DEG F              0         5         1      AVER         2")
            sr.WriteLine("1    WIND SPEED,MI/TIMESTEP       0         5         1      AVER         2")
            sr.WriteLine("1    SOLAR RAD,LY/TIMESTEP        0         5         1      AVER         2")
            sr.WriteLine("1    DEW POINT, DEG F             0         5         1      AVER         2")
            sr.WriteLine("1    CLOUD COVER, TENTHS          0         5         1      AVER         2")
            sr.WriteLine("1   ")
            sr.WriteLine("1   ")
            sr.WriteLine("1    Time series (pt-valued, then mean-valued):")
            sr.WriteLine("1   ")
            sr.WriteLine("1    Date/time                      Values")
            sr.WriteLine("1   ")

            For Each drow As DataRow In dbTable.Rows
                Dim dt() As String = drow.Item("DateTime").Split(sep, StringSplitOptions.RemoveEmptyEntries)

                Dim strng As New StringBuilder
                strng.Append("1")
                strng.Append(vbTab + dt(0)) 'year
                strng.Append(vbTab + dt(1)) 'month
                strng.Append(vbTab + dt(2)) 'day
                strng.Append(vbTab + dt(3)) 'hr
                strng.Append(vbTab + "0")
                strng.Append(vbTab + drow.Item("PREC"))
                strng.Append(vbTab + drow.Item("PEVT"))
                strng.Append(vbTab + drow.Item("ATEM"))
                strng.Append(vbTab + drow.Item("SOLR"))
                strng.Append(vbTab + drow.Item("WIND"))
                strng.Append(vbTab + drow.Item("DEWP"))
                strng.Append(vbTab + drow.Item("CLOU"))
                sr.WriteLine(strng.ToString())
            Next
        Catch ex As Exception
            Dim msg = "Error writing " + AirFile + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            ErrorMessage(msg)
            msg = Nothing
        Finally
            sr.Close()
            sr.Dispose()
        End Try
    End Sub
    Private Sub ErrorMessage(ByVal msg As String)
        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
End Class
