Imports System
Imports System.Collections
Imports System.Data.SQLite
Imports System.Diagnostics
Imports System.IO
Imports System.Data
Imports System.Windows.Forms
Imports System.Text

Public Class clsDataBase
    Private sDBFile As String
    Private conn As SQLiteConnection
    Private dbTables As New DataTable
    Private lstTables As New List(Of String)
    Private lstPcodes As New List(Of String)
    Private dicTblPcodes As New Dictionary(Of String, List(Of String))
    Private dicTblSta As New Dictionary(Of String, List(Of String))
    Private dicSeries As New Generic.SortedDictionary(Of Date, String)

    Public Sub New(ByVal _dbFile As String)
        Me.sDBFile = _dbFile
        If Not OpenDatabase() Then Return
    End Sub

    Public Function GetTablesAndPcodes() As Boolean
        If Not GetDBTables() Then Return False

        Try
            For Each tbl As String In lstTables
                'get list of pcodes in each table
                Dim lstVar As List(Of String) = GetPCODES(tbl)
                If Not IsNothing(lstVar) Then
                    dicTblPcodes.Add(tbl, lstVar)
                End If
                'get list of stations in each table
                'For Each pcod As String In lstVar
                Dim lstSta As List(Of String) = GetSTATIONS(tbl, String.Empty)
                If Not IsNothing(lstSta) Then
                    dicTblSta.Add(tbl, lstSta)
                End If
                'Next
            Next
            Return True
        Catch ex As Exception
            MessageBox.Show("Error querying database for PCODES and Stations!\r\n" + ex.Message)
            Return False
        End Try
    End Function

    Private Function OpenDatabase() As Boolean
        Try
            Dim connStr As String = "Data Source=" + sDBFile
            'open the database
            conn = New SQLiteConnection(connStr)
            conn.Open()
        Catch ex As Exception
            MessageBox.Show("Error connecting to database " + sDBFile + "\r\n" + ex.Message)
            Return False
        End Try
        Return True
    End Function

    Public Function ReadSeries(ByVal tbl As String,
                               ByVal pcode As String,
                               ByVal sta As String, ByVal arearatio As Double) As Integer

        Dim numrec As Integer = 0
        If String.IsNullOrEmpty(tbl) Or String.IsNullOrEmpty(pcode) Or
                String.IsNullOrEmpty(sta) Then Return 0

        Try
            Dim tblSeries As New DataTable
            Dim qry As New StringBuilder()

            qry.Append("SELECT DATE_TIME, AVG(RESULT) AS Value FROM ")
            qry.Append(tbl.ToString().Trim())
            qry.Append(" WHERE STATION_ID = '")
            qry.Append(sta)
            qry.Append("' AND PCODE = '")
            qry.Append(pcode)
            'qry.Append("' AND (CAST(STRFTIME('%Y', DATE_TIME) >= "+byr)
            'qry.Append(" AND CAST(STRFTIME('%Y', DATE_TIME) <= " + eyr+")")
            qry.Append("' GROUP BY DATE_TIME")
            qry.Append(" ORDER BY DATE_TIME")

            Dim adapter As New SQLiteDataAdapter(qry.ToString(), conn)
            adapter.Fill(tblSeries)
            qry = Nothing
            adapter = Nothing

            'set DataSeries table/dictionary
            numrec = tblSeries.Rows.Count
            Dim val As Double = 0.0
            If numrec > 0 Then
                dicSeries.Clear()
                For Each dr As DataRow In tblSeries.Rows
                    If Not String.IsNullOrEmpty(dr(1)) Then
                        val = Convert.ToDouble(dr(1)) / arearatio
                        dicSeries.Add(CDate(dr(0)), val.ToString("#.00"))
                    Else
                        dicSeries.Add(CDate(dr(0)), dr(1))
                    End If
                    'Debug.WriteLine(CDate(dr(0)).ToShortDateString() + ", " + dr(1).ToString())
                Next
            End If
            tblSeries = Nothing
        Catch ex As Exception
            MessageBox.Show("Error querying database for flow series at " + sta + "!\r\n" + ex.Message)
            Return 0
        End Try
        Return numrec
    End Function

    Private Function GetDBTables() As Boolean
        Dim dbTables As New DataTable
        Try
            'List of Tables in the database
            Dim qry As New StringBuilder()
            qry.Append("SELECT NAME FROM SQLITE_MASTER")
            qry.Append(" WHERE TYPE='table' ")
            qry.Append(" AND NAME NOT IN ")
            qry.Append(" ('Master','Settings','Stations','PCodes','CCodes','RCodes','QCodes',")
            qry.Append("'Branches','StaGrp','PCodeGrp','ConvGrp','Agencies','FIPSCodes',")
            qry.Append("'WQXMethods','Validation','Criteria','Journal','Tracking')")
            Dim adapter As New SQLiteDataAdapter(qry.ToString(), conn)
            adapter.Fill(dbTables)
            qry = Nothing
            adapter = Nothing

            'fill list of tables
            Dim ltab As String
            For Each drow In dbTables.Rows
                ltab = drow(0)
                If Not ltab.Contains("TEMP_") AndAlso Not ltab.Contains("DATA_") Then
                    lstTables.Add(drow(0))
                End If
                ' Debug.WriteLine("Table = " + drow(0).ToString())
            Next
        Catch ex As Exception
            MessageBox.Show("Error querying database " + sDBFile)
            Return False
        End Try
        Return True
    End Function

    Private Function GetPCODES(ByVal selTable As String) As List(Of String)
        Dim dbPcodes As New DataTable
        Dim lstVars As New List(Of String)

        Try
            Dim qry As New StringBuilder()
            qry.Append("SELECT DISTINCT PCODE FROM ")
            qry.Append(selTable.ToString().Trim())
            qry.Append(" ORDER BY PCODE ")

            Dim adapter As New SQLiteDataAdapter(qry.ToString(), conn)
            adapter.Fill(dbPcodes)
            qry = Nothing
            adapter = Nothing

            'fill list of pcodes
            For Each drow In dbPcodes.Rows
                lstVars.Add(drow(0))
                'Debug.WriteLine("Table = " + selTable + ", PCODE = " + drow(0).ToString())
            Next
            dbPcodes = Nothing
            Return lstVars
        Catch ex As Exception
            Return Nothing
        End Try
        Return lstVars
    End Function

    Private Function GetSTATIONS(ByVal selTable As String, ByVal pcode As String) As List(Of String)
        Dim dbSta As New DataTable
        Dim lstSta As New List(Of String)

        Try
            Dim qry As New StringBuilder()
            qry.Append("SELECT DISTINCT STATION_ID FROM ")
            qry.Append(selTable.ToString().Trim())
            'qry.Append(" WHERE PCODE = '")
            'qry.Append(pcode & "'")
            qry.Append(" ORDER BY STATION_ID ")

            Dim adapter As New SQLiteDataAdapter(qry.ToString(), conn)
            adapter.Fill(dbSta)
            qry = Nothing
            adapter = Nothing

            'fill list of pcodes
            'lstSta.Add(pcode)
            'Debug.WriteLine("Table = " + selTable + ", PCODE = " + pcode.ToString())
            For Each drow In dbSta.Rows
                lstSta.Add(drow(0))
                'Debug.WriteLine("Table = " + selTable + ", sta = " + drow(0).ToString())
            Next
            dbSta = Nothing
            Return lstSta
        Catch ex As Exception
            Return Nothing
        End Try
        Return lstSta
    End Function

    Public ReadOnly Property ListOfTables() As List(Of String)
        Get
            Return lstTables
        End Get
    End Property

    Public ReadOnly Property ListOfPcodes() As Dictionary(Of String, List(Of String))
        Get
            Return dicTblPcodes
        End Get
    End Property

    Public ReadOnly Property ListOfStations() As Dictionary(Of String, List(Of String))
        Get
            Return dicTblSta
        End Get
    End Property

    Public ReadOnly Property USGSSeries() As Generic.SortedDictionary(Of Date, String)
        Get
            Return dicSeries
        End Get
    End Property

    Public Function CloseDatabase() As Boolean
        Try
            conn.Close()
            conn = Nothing
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

End Class
