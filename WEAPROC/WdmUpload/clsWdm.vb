Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports atcUtility.modDate
Imports atcData
Imports wdmuploader.WeaTimeSeries

Public Class clsWdm
    Private AppFolder As String, LogFile As String
    Private WdmFile As String = String.Empty
    Private wrlog As StreamWriter
    Private lWdmDS As atcWDM.atcDataSourceWDM
    Private lBaseDSN As Integer = 0
    Private dictSiteWea As New Dictionary(Of String, SortedDictionary(Of DateTime, String))
    Private WeaVars As New List(Of String) From {"WIND", "CLOU", "ATEM", "DEWP"}
    Private dictOtherSta As New Dictionary(Of String, List(Of String))
    Private dictSta As New Dictionary(Of String, List(Of String))
    Private station As String
    Private siteAttrib As List(Of String)
    Private optDataset As Int32
    Private tstep As String
    Private DSNcnt As Integer
    Enum IsDataSource
        NLDAS = 0
        ISD = 1
        HRAIN = 2
        GHCN = 3
        GLDAS = 4
        TRMM = 5
        CMIP6 = 6
        EDDE = 7
    End Enum

    Public Sub New(ByVal _wrlog As StreamWriter, ByVal _station As String, ByVal _siteAttrib As List(Of String),
                   ByVal _dictSiteWea As Dictionary(Of String, SortedDictionary(Of DateTime, String)),
                   ByVal _wdmFile As String, ByVal _optDataSrc As Int32, ByVal _tstep As String)

        Me.tstep = _tstep
        Me.station = _station
        Me.dictSiteWea = _dictSiteWea
        Me.siteAttrib = _siteAttrib
        Me.WdmFile = _wdmFile
        Me.wrlog = _wrlog
        Me.optDataset = _optDataSrc
        wrlog.AutoFlush = True
        lBaseDSN = GetMaxDSN(WdmFile)
    End Sub

    Public Sub New(ByVal _wdmFile As String, ByVal _optDataSrc As Integer)
        Me.WdmFile = _wdmFile
        Me.optDataset = _optDataSrc
    End Sub

    Private Sub OpenWDM(ByVal WdmFile As String)
        WriteLog("Opening file " + WdmFile)
        lWdmDS = New atcWDM.atcDataSourceWDM
        Try
            lWdmDS.Open(WdmFile)
            Dim numds As Integer = lWdmDS.DataSets.Count
            WriteLog("Number of Datasets in datasource is  " + numds.ToString())
            'lBaseDSN = GetMaxDSN(WdmFile)
            WriteLog("Maximum DSN ID is  " + lBaseDSN.ToString())
        Catch ex As Exception
            WriteLog("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            MessageBox.Show("Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source)
            Return
        End Try

    End Sub

    Private Function GetMaxDSN(ByVal wdmfile As String) As Integer
        Dim mxDSN As Integer = 0
        Dim id As Integer = 0
        Dim lds As New atcWDM.atcDataSourceWDM

        Try
            lds.Open(wdmfile)
            Dim numds As Integer = lds.DataSets.Count
            WriteLog("Number of Datasets in datasource is  " + numds.ToString())
        Catch ex As Exception
            Dim msg = "Error opening datasource!" + vbCrLf + ex.Message + ex.StackTrace + vbCrLf + ex.Source
            Debug.WriteLine(msg)
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        For i As Integer = 0 To lds.DataSets.Count - 1
            With lds.DataSets(i).Attributes
                id = .GetValue("ID")
                If (id > mxDSN) Then
                    mxDSN = id
                End If
            End With
        Next
        lds = Nothing
        Return mxDSN
    End Function

    Public Function UploadSeriesToWDM(ByVal site As String, ByVal WdmFile As String) As Boolean
        WriteLog("Entering UploadSeriesToWDM routine: " + site) 'ok
        Debug.WriteLine("Entering UploadSeriesToWDM routine: " + site) 'ok
        'Cursor.Current = Cursors.WaitCursor

        Dim svar As String
        Dim numVal As Integer = 0
        Dim jcnt As Integer = 0
        Dim dictSeries As SortedDictionary(Of Date, String)
        Dim adjustedSeries As SortedDictionary(Of Date, String)

        DSNcnt = lBaseDSN + 1
        Try
            OpenWDM(WdmFile)

            'iterate all variables in dictionary 

            For Each kv As KeyValuePair(Of String, SortedDictionary(Of Date, String)) In dictSiteWea
                svar = kv.Key
                dictSeries = New SortedDictionary(Of Date, String)
                adjustedSeries = New SortedDictionary(Of Date, String)
                dictSiteWea.TryGetValue(svar, dictSeries)

                'adjust dates if daily like in GHCN
                If (optDataset = IsDataSource.GHCN) Then
                    Dim ddate As List(Of Date) = dictSeries.Keys.ToList()
                    Dim vals As List(Of String) = dictSeries.Values.ToList()

                    For i As Integer = 0 To ddate.Count - 1
                        adjustedSeries.Add(ddate.ElementAt(i).AddDays(1), vals.ElementAt(i))
                    Next
                    ddate = Nothing
                    vals = Nothing
                Else
                    adjustedSeries = dictSeries
                End If

                'increment count of DSN
                DSNcnt += 1
                If UploadSeries(DSNcnt, svar, site, adjustedSeries) Then
                    WriteLog("Uploaded " + svar + " for station " + site + ", DSN = " + DSNcnt.ToString())
                End If
            Next kv
        Catch ex As Exception
            Return False
        End Try
        'Cursor.Current = Cursors.Default
        Return True
    End Function

    Public Property DSNCount() As Integer
        Get
            Return DSNcnt
        End Get
        Set(value As Integer)
            DSNcnt = value
        End Set
    End Property

    Public Function UploadSeriesToWDMold(ByVal site As String, ByVal WdmFile As String) As Boolean
        'WriteLog("Entering UploadSeriesToWDM routine: " + site) 'ok
        Cursor.Current = Cursors.WaitCursor

        Dim svar As String, DSNcnt As Integer = lBaseDSN + 1
        Dim numVal As Integer = 0
        Dim jcnt As Integer = 0
        Dim dictSeries As New SortedDictionary(Of Date, String)
        Try
            OpenWDM(WdmFile)

            'iterate all variables in dictionary 

            For Each kv As KeyValuePair(Of String, SortedDictionary(Of Date, String)) In dictSiteWea
                svar = kv.Key
                dictSiteWea.TryGetValue(svar, dictSeries)
                numVal = dictSeries.Keys.Count

                Dim Values(numVal) As Double
                Dim DateTimes(numVal) As DateTime
                Dim sval As String = String.Empty

                'first and last date
                Dim dtfirst As DateTime = dictSeries.Keys.First()
                Dim dtlast As DateTime = dictSeries.Keys.Last()

                jcnt = 0
                For Each kv1 As KeyValuePair(Of Date, String) In dictSeries
                    dictSeries.TryGetValue(kv1.Key, sval)
                    Values(jcnt) = sval
                    DateTimes(jcnt) = kv1.Key
                    jcnt += 1
                Next

                'increment count of DSN
                DSNcnt += 1
                If UploadSeries(DSNcnt, svar, site, DateTimes, Values) Then
                    'WriteLog("Uploaded " + svar + " for station " + site + ", DSN = " + DSNcnt.ToString())
                End If

                Values = Nothing
                DateTimes = Nothing
            Next kv
        Catch ex As Exception
            Return False
        End Try
        Cursor.Current = Cursors.Default
        Return True
    End Function

    Public Function GetListOfWDMDatasets() As Dictionary(Of String, List(Of String))
        Dim lstDS As New List(Of String)
        Dim dSta As New Dictionary(Of String, List(Of String))
        Dim lwdm As atcWDM.atcDataSourceWDM

        Try
            lwdm = New atcWDM.atcDataSourceWDM
            lwdm.Open(WdmFile)
            Dim numds As Integer = lwdm.DataSets.Count
            If (numds = 0) Then Return Nothing

            For i As Integer = 0 To numds - 1
                Dim sloc As String = Proper(lwdm.DataSets(i).Attributes.GetValue("Location")).Trim()
                Dim sdsn As Integer = lwdm.DataSets(i).Attributes.GetValue("ID")
                Dim svar As String = Proper(lwdm.DataSets(i).Attributes.GetValue("Constituent")).Trim()
                Dim sdec As String = Proper(lwdm.DataSets(i).Attributes.GetValue("Description")).Trim()

                'Debug.WriteLine(sloc + ", " + svar + ", " + sdsn.ToString())
                If Not dSta.Keys.Contains(sdec) Then
                    Dim lstv = New List(Of String)
                    lstv.Add(svar)
                    'lstv.Add(sdsn.ToString())
                    'use description for station, long station ID
                    dSta.Add(sdec, lstv)
                    lstv = Nothing
                Else 'same station
                    Dim lstv = New List(Of String)
                    dSta.TryGetValue(sdec, lstv)
                    lstv.Add(svar)
                    'lstv.Add(sdsn.ToString())
                    'use description for station, long station ID
                    dSta(sdec) = lstv
                    lstv = Nothing
                End If
            Next
            Return dSta
        Catch ex As Exception
            Dim msg = vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error getting list of wdm datasets", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return Nothing
        End Try
    End Function

    Private Function UploadSeries(ByVal dsn As Integer, ByVal sVar As String, ByVal station As String,
                                  ByVal Datetimes() As DateTime, ByVal Values() As Double) As Boolean

        WriteLog("Uploading " + sVar + " for station " + station + ", DSN = " + dsn.ToString())
        Debug.WriteLine("Entering UploadSeries routine")

        Dim Scenario As String = String.Empty, Location As String = String.Empty
        Dim Constituent As String = String.Empty, description As String = String.Empty
        Dim latitude As Double = 0.0, longitude As Double = 0.0, elevation As Double = 0.0
        Dim lst As New List(Of String)

        Dim site As String = String.Empty, wban As String, aws As String
        Dim stname As String, datasrc As String = String.Empty
        'need to change station id for limit of wdm
        Select Case optDataset
            Case IsDataSource.ISD
                'format from json is AWS:722280, WBAN: 13876, 72228013876
                'in dictionary 72228013876
                'in GIS file, USAF:722280, WBAN: 13876 
                aws = station.Substring(0, 6)
                wban = station.Substring(6, 5)
                If (Not aws.Contains("999999")) Then
                    site = aws + wban.Substring(0, 2) 'eg 72228099
                Else
                    site = aws.Substring(0, 2) + wban 'eg 9913876
                End If
                datasrc = "ISD"
            Case IsDataSource.GHCN
                'format from GIS is GHCND:US1GALR0001
                'in dictionary US1GALR0001, file is US1GALR0001.dly
                site = station.Substring(3)
                datasrc = "GHCN"

            Case IsDataSource.HRAIN
                'format from GIS service is COOP:010957
                'in dictionary ???00010957, file is ???00010957.csv
                site = station.Substring(5)
                datasrc = "COOP"

        End Select

        Try
            Scenario = "OBSERVED"
            'Location = station, cannot use location since wdm limits to 8 char for IDLOCN field
            Location = site
            Constituent = sVar
            latitude = Convert.ToDouble(siteAttrib(2))
            longitude = Convert.ToDouble(siteAttrib(3))
            elevation = Convert.ToDouble(siteAttrib(1))
            'stname = siteAttrib(0)
            'description = station
            'modified 8/20/20, use stname instead of description
            'location is revised station id

            'stname = station
            'description = siteAttrib(0) 'full station name
            'CHANGE 10/20/2020
            'station is full stationid e.g. USC00000000 for GHCN and COOP
            stname = station
            description = datasrc + ":" + siteAttrib(0) 'siteAttrib(0) is full station name

            'WDMWrite(dsn, Scenario, Location, Constituent, Datetimes, Values,
            'latitude, longitude, elevation, DataSrc)
            WDMWrite(dsn, Scenario, Location, Constituent, Datetimes, Values,
                     latitude, longitude, elevation, description, stname, datasrc)

        Catch ex As Exception
            ErrorMsg(, ex)
            Return False
        End Try
        Return True
    End Function

    Private Function UploadSeries(ByVal dsn As Integer, ByVal sVar As String, ByVal station As String,
                                  ByVal dictSeries As SortedDictionary(Of Date, String)) As Boolean

        WriteLog("Uploading " + sVar + " for station " + station + ", DSN = " + dsn.ToString())
        Debug.WriteLine("Entering UploadSeries routine")

        Dim Scenario As String = String.Empty, Location As String = String.Empty
        Dim Constituent As String = String.Empty, description As String = String.Empty
        Dim latitude As Double = 0.0, longitude As Double = 0.0, elevation As Double = 0.0
        Dim lst As New List(Of String)

        Dim site As String = String.Empty, wban As String, aws As String
        Dim stname As String, datasrc As String = String.Empty
        'need to change station id for limit of wdm

        Select Case optDataset
            Case IsDataSource.ISD
                'format from json is AWS:722280, WBAN: 13876, 72228013876
                'in dictionary 72228013876
                'in GIS file, USAF:722280, WBAN: 13876 
                aws = station.Substring(0, 6)
                wban = station.Substring(6, 5)
                If (Not aws.Contains("999999")) Then
                    site = aws + wban.Substring(0, 2) 'eg 72228099
                Else
                    site = aws.Substring(0, 2) + wban 'eg 9913876
                End If
                datasrc = "ISD"
            Case IsDataSource.GHCN
                'format from GIS is GHCND:US1GALR0001
                'in dictionary US1GALR0001, file is US1GALR0001.dly
                site = station.Substring(3)
                datasrc = "GHCN"

            Case IsDataSource.HRAIN
                'format from GIS service is COOP:010957
                'in dictionary ???00010957, file is ???00010957.csv
                site = station.Substring(5)
                datasrc = "COOP"

            Case IsDataSource.CMIP6
                'format from GIS service is CXXXXYYY
                site = station
                datasrc = "CMIP6"

            Case IsDataSource.EDDE
                'format from GIS service is CXXXXYYY
                site = station
                datasrc = "EDDE"
        End Select

        Try
            If optDataset = IsDataSource.CMIP6 Then
                Scenario = siteAttrib(7)
                Location = site
                Constituent = sVar
                latitude = Convert.ToDouble(siteAttrib(2))
                longitude = Convert.ToDouble(siteAttrib(3))
                elevation = Convert.ToDouble(siteAttrib(1))
                stname = station
                description = "CMIP6:" + siteAttrib(5) + "_" + siteAttrib(6) 'scenario & pathway
            ElseIf optDataset = IsDataSource.EDDE Then
                Scenario = siteAttrib(7)
                Location = site
                Constituent = sVar
                latitude = Convert.ToDouble(siteAttrib(2))
                longitude = Convert.ToDouble(siteAttrib(3))
                elevation = Convert.ToDouble(siteAttrib(1))
                stname = station
                description = "EDDE:" + siteAttrib(5) + "_" + siteAttrib(6) 'scenario & pathway
            Else
                Scenario = "OBSERVED"
                'Location = station, cannot use location since wdm limits to 8 char for IDLOCN field
                Location = site
                Constituent = sVar
                latitude = Convert.ToDouble(siteAttrib(2))
                longitude = Convert.ToDouble(siteAttrib(3))
                elevation = Convert.ToDouble(siteAttrib(1))
                stname = station
                description = datasrc + ":" + siteAttrib(0) 'siteAttrib(0) is full station name
            End If

            'stname = siteAttrib(0)
            'description = station
            'modified 8/20/20, use stname instead of description
            'location is revised station id

            'stname = station
            'description = siteAttrib(0) 'full station name
            'CHANGE 10/20/2020
            'station is full stationid e.g. USC00000000 for GHCN and COOP

            WDMWrite(dsn, Scenario, Location, Constituent, dictSeries,
                  latitude, longitude, elevation, description, stname, datasrc, tstep)

        Catch ex As Exception
            ErrorMsg(, ex)
            Return False
        End Try
        Return True
    End Function

    'WDMWrite using array of datetimes and values of timeseries
    Private Sub WDMWrite(ByVal dsn As Integer, ByVal Scenario As String, ByVal Location As String,
                     ByVal Constituen As String, ByVal DateTimes() As DateTime, ByVal Values() As Double,
                     ByVal lat As Double, ByVal lon As Double, ByVal elev As Double,
                     ByVal descript As String, ByVal stname As String, ByVal datasrc As String)

        'calling sub by
        'WDMWrite(dsn, Scenario, Location, Constituent, DateTimes, Values,
        'latitude, longitude, elevation, description, stname)

        WriteLog("Entering WDMWrite routine")
        Debug.WriteLine("Entering WDMWrite routine")

        Dim numvalues As Integer = 0
        Try
            Dim lTS As New atcData.atcTimeseries(Nothing)
            lTS.Dates = New atcData.atcTimeseries(Nothing)
            lTS.numValues = Values.Length
            Dim tu As atcUtility.atcTimeUnit = atcUtility.atcTimeUnit.TUHour
            With lTS.Attributes
                .SetValue("Elevation", elev)
                .SetValue("Scenario", Scenario)
                .SetValue("Location", Location)
                .SetValue("StaNam", stname)
                .SetValue("Description", descript) 'use the original long station name
                .SetValue("Constituent", Constituen)
                .SetValue("Latitude", lat)
                .SetValue("Longitude", lon)
                .SetValue("TSType", Constituen) 'needed by some legacy apps
                .SetValue("Time Step", 1)
                '.SetValue("STAID", datasrc + ":" + stname)
                .SetValue("STAID", stname)

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
                    ElseIf lDateSpan.TotalDays = 366 Then
                        tu = atcUtility.atcTimeUnit.TUYear
                    ElseIf lDateSpan.TotalDays = 3650 Then
                        tu = atcUtility.atcTimeUnit.TUCentury
                    Else
                        tu = atcUtility.atcTimeUnit.TUUnknown
                        Throw New InvalidConstraintException(String.Format("WDM files are generally expected to contain constant increment time series; specifically, the first two observations must have a standard time step (second, hour, day, month, or year); when this is not true, the dataset cannot be saved to the WDM file. Please examine your data for Location='{0}' and Constituent='{1}'", Location, Constituen))
                    End If
                End If
                .SetValue("tu", tu)
                .SetValue("SJDay", DateTimes(0).ToOADate)
                .SetValue("EJDay", DateTimes(lTS.numValues - 1).ToOADate)
            End With

            'lTS.Value(0) = atcUtility.GetNaN()
            'lTS.Value(0) = Values(0)
            'lTS.Dates.Value(0) = DateTimes(0).ToOADate

            numvalues = lTS.numValues - 2
            For i As Integer = 0 To numvalues - 1  'lTS.numValues - 2
                lTS.Dates.Value(i) = DateTimes(i).ToOADate
                lTS.Value(i) = Values(i)
            Next
            'lst two dates
            lTS.Dates.Value(numvalues) = DateTimes(numvalues - 1).ToOADate
            lTS.Value(numvalues) = Values(numvalues - 1)
            lTS.Dates.Value(numvalues + 1) = DateTimes(numvalues - 1).ToOADate
            lTS.Value(numvalues + 1) = Values(numvalues - 1)
            lTS.Attributes.SetValue("ID", dsn)

            Dim Wdm As New atcWDM.atcDataSourceWDM
            If Wdm.Open(WdmFile) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And
                       ds.Attributes.GetValue("Location", "") = Location And
                       ds.Attributes.GetValue("Constituent", "") = Constituen Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next

                'Dim ltseries As atcData.atcTimeseries = atcData.modTimeseriesMath.SubsetByDate(lTS, DateTimes(0).ToOADate(), DateTimes(lTS.numValues - 1).ToOADate(), Wdm)

                If Not Wdm.AddDataSet(lTS, atcData.atcDataSource.EnumExistAction.ExistReplace) Then
                    Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ='{1}' and Variable ='{2}'",
                           WdmFile.Replace("\", "\\"), Location, Constituen))
                End If
                'ltseries = Nothing
                lTS = Nothing
            End If

            Wdm = Nothing
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

    'WDMWrite using dictionary of time series
    Private Sub WDMWrite(ByVal dsn As Integer, ByVal Scenario As String, ByVal Location As String,
                     ByVal Constituen As String, ByVal dictSeries As SortedDictionary(Of Date, String),
                     ByVal lat As Double, ByVal lon As Double, ByVal elev As Double,
                     ByVal descript As String, ByVal stname As String, ByVal datasrc As String,
                     ByVal tstep As String)

        WriteLog("Entering WDMWrite routine ...")

        Try
            Dim wea As New WeaTimeSeries()
            'If optDataset = CInt(IsDataSource.GHCN) Or optDataset = CInt(IsDataSource.CMIP6) Then
            'Else
            'wea.OpenSeries("Hour", dictSeries)
            'End If
            If optDataset = CInt(IsDataSource.GHCN) Then
                wea.OpenSeries("Day", dictSeries)
            Else
                wea.OpenSeries(tstep, dictSeries)
            End If
            'openseries builds an atcTimeseries so only 1 dataset in wea
            Dim ltseries As atcData.atcTimeseries = wea.DataSets.Item(0)
            ltseries.Attributes.SetValue("ID", dsn)
            ltseries.Attributes.SetValue("Constituent", Constituen)
            ltseries.Attributes.SetValue("Location", Location)
            ltseries.Attributes.SetValue("Scenario", Scenario)
            ltseries.Attributes.SetValue("StaNam", stname)
            ltseries.Attributes.SetValue("Description", descript) 'use the original long station name
            ltseries.Attributes.SetValue("Constituent", Constituen)
            ltseries.Attributes.SetValue("Latitude", lat)
            ltseries.Attributes.SetValue("Longitude", lon)
            ltseries.Attributes.SetValue("Elevation", elev)
            ltseries.Attributes.SetValue("TSType", Constituen) 'needed by some legacy apps
            ltseries.Attributes.SetValue("STAID", stname)

            Dim Wdm As New atcWDM.atcDataSourceWDM
            If Wdm.Open(WdmFile) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And
                       ds.Attributes.GetValue("Location", "") = Location And
                       ds.Attributes.GetValue("Constituent", "") = Constituen Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next
                If Not Wdm.AddDataSet(ltseries, atcData.atcDataSource.EnumExistAction.ExistReplace) Then
                    Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ='{1}' and Variable ='{2}'",
                           WdmFile.Replace("\", "\\"), Location, Constituen))
                End If
                ltseries = Nothing
            End If
            Wdm = Nothing
            wea = Nothing
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

    Private Sub WDMWriteOLD_111320(ByVal dsn As Integer, ByVal Scenario As String, ByVal Location As String,
                     ByVal Constituen As String, ByVal DateTimes() As DateTime, ByVal Values() As Double,
                     ByVal lat As Double, ByVal lon As Double, ByVal elev As Double,
                     ByVal descript As String, ByVal stname As String, ByVal datasrc As String)

        'revise 11132020
        Debug.WriteLine("Entering WDMWrite routine")

        Dim numvalues As Integer = 0
        Try
            Dim lTS As New atcData.atcTimeseries(Nothing)
            lTS.Dates = New atcData.atcTimeseries(Nothing)
            lTS.numValues = Values.Length
            Dim tu As atcUtility.atcTimeUnit = atcUtility.atcTimeUnit.TUHour
            With lTS.Attributes
                .SetValue("Elevation", elev)
                .SetValue("Scenario", Scenario)
                .SetValue("Location", Location)
                .SetValue("StaNam", stname)
                .SetValue("Description", descript) 'use the original long station name
                .SetValue("Constituent", Constituen)
                .SetValue("Latitude", lat)
                .SetValue("Longitude", lon)
                .SetValue("TSType", Constituen) 'needed by some legacy apps
                .SetValue("Time Step", 1)
                '.SetValue("STAID", datasrc + ":" + stname)
                .SetValue("STAID", stname)

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
                        Throw New InvalidConstraintException(String.Format("WDM files are generally expected to contain constant increment time series; specifically, the first two observations must have a standard time step (second, hour, day, month, or year); when this is not true, the dataset cannot be saved to the WDM file. Please examine your data for Location='{0}' and Constituent='{1}'", Location, Constituen))
                    End If
                End If
                .SetValue("tu", tu)
                .SetValue("SJDay", DateTimes(0).ToOADate)
                .SetValue("EJDay", DateTimes(lTS.numValues - 1).ToOADate)
            End With

            'lTS.Value(0) = atcUtility.GetNaN()
            'lTS.Value(0) = Values(0)
            'lTS.Dates.Value(0) = DateTimes(0).ToOADate

            For i As Integer = 0 To lTS.numValues - 2
                lTS.Dates.Value(i) = DateTimes(i).ToOADate
                lTS.Value(i) = Values(i + 1)
            Next
            'lst two dates
            'lTS.Dates.Value(numvalues) = DateTimes(numvalues - 1).ToOADate
            'lTS.Value(numvalues) = Values(numvalues - 1)
            'lTS.Dates.Value(numvalues + 1) = DateTimes(numvalues - 1).ToOADate
            'lTS.Value(numvalues + 1) = Values(numvalues - 1)
            'lTS.Attributes.SetValue("ID", dsn)

            Dim Wdm As New atcWDM.atcDataSourceWDM
            'Try
            'Wdm.Open(WdmFile)
            'Catch ex As Exception
            'End Try

            If Wdm.Open(WdmFile) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And
                       ds.Attributes.GetValue("Location", "") = Location And
                       ds.Attributes.GetValue("Constituent", "") = Constituen Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next

                'Dim ltseries As atcData.atcTimeseries = atcData.modTimeseriesMath.SubsetByDate(lTS, DateTimes(0).ToOADate(), DateTimes(lTS.numValues - 1).ToOADate(), Wdm)

                If Not Wdm.AddDataSet(lTS, atcData.atcDataSource.EnumExistAction.ExistReplace) Then
                    Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ='{1}' and Variable ='{2}'",
                           WdmFile.Replace("\", "\\"), Location, Constituen))
                End If
                'ltseries = Nothing
                lTS = Nothing
            End If

            Wdm = Nothing
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

    Private Sub WriteLogFile(ByVal msg As String)
        Debug.WriteLine(msg)
        wrlog.WriteLine(msg)
        wrlog.AutoFlush = True
        wrlog.Flush()
    End Sub

    Friend Sub ErrorMsg(Optional ByVal ErrorText As String = "", Optional ByVal ex As Exception = Nothing)
        If ErrorText = "" Then ErrorText = "An error has occurred in writing to the wdm database. Please check series."
        If ex IsNot Nothing Then ErrorText &= String.Format("\n\n{0}\n\n\nThe detailed error message was:\n\n{1}", ex.Message, ex.ToString)
        MapWinUtility.Logger.Message(ErrorText.Replace("\n", vbCr), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK)
    End Sub
    Private Function Proper(ByVal s As String) As String
        Return My.Application.Culture.TextInfo.ToTitleCase(s)
    End Function
    Private Function GetOtherStationSeries(ByVal svar As String,
                                           ByVal OtherSta As String,
                                           ByVal BegYr As Integer,
                                           ByVal EndYr As Integer) As SortedDictionary(Of DateTime, String)
        Dim dOther As New SortedDictionary(Of DateTime, String)
        Dim lwdm As New atcWDM.atcDataSourceWDM
        Dim dsn As Integer, loc As String, cons As String
        Dim datanum As Integer = -1
        Dim lseries As atcData.atcTimeseries = Nothing

        Debug.WriteLine(vbCrLf + "Entering GetOtherStationSeries for " + OtherSta + ", " + svar)

        Try
            lwdm.Open(WdmFile)
            Debug.WriteLine("Num datasets = " + lwdm.DataSets.Count.ToString())

            For i As Integer = 0 To lwdm.DataSets.Count - 1
                loc = lwdm.DataSets(i).Attributes.GetValue("Location").Trim.ToString()
                cons = lwdm.DataSets(i).Attributes.GetValue("Constituent").Trim.ToString()
                Debug.WriteLine(loc + "," + cons)
                If svar.ToUpper.Contains(cons.ToUpper()) And OtherSta.ToUpper.Contains(loc.ToUpper()) Then
                    dsn = lwdm.DataSets(i).Attributes.GetValue("ID")
                    datanum = i
                    Debug.WriteLine("DataNum is " + i.ToString() + ", location=" + loc + ", Var=" + cons)
                    Exit For
                End If
            Next
        Catch ex As Exception
            Dim msg = "Error opening wdm file! " + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try

        If datanum < 0 Then Return Nothing

        Try
            lseries = lwdm.DataSets(datanum)
            Debug.WriteLine("Other Station data = " + lseries.numValues.ToString())

            Dim dt() As Double = lseries.Dates.Values()
            Dim values() As Double = lseries.Values()

            For j As Integer = 0 To dt.Count - 1
                Dim ddate = DateTime.FromOADate(dt(j))
                If ddate.Year >= BegYr And ddate.Year <= EndYr Then
                    dOther.Add(ddate, values(j + 1).ToString()) 'Mark Gray see atcwdmvb
                End If
                ddate = Nothing
            Next
            dt = Nothing
            values = Nothing
            lseries = Nothing
            lwdm = Nothing

            'debug
            'For Each kv As KeyValuePair(Of DateTime, String) In dOther
            'Dim val As String = String.Empty
            'dOther.TryGetValue(kv.Key, val)
            'Debug.WriteLine(kv.Key.ToString() + ", " + val)
            'Next

        Catch ex As Exception
            Dim msg = "Error getting other station series! " + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try

        Return dOther
    End Function
    ''' <summary>
    ''' Read timeseries from WDM file, use stanam as location identifier for datasets other than NLDAS
    ''' </summary>
    ''' <param name="site"></param>
    ''' <param name="svar"></param>
    ''' <returns></returns>

    Public Function ReadWeatherSeries(ByVal site As String, ByVal svar As String) As SortedDictionary(Of DateTime, String)
        Dim lwdm As New atcWDM.atcDataSourceWDM
        Dim dsn As Integer, loc As String, cons As String
        Dim datanum As Integer = -1
        Dim lseries As atcData.atcTimeseries = Nothing
        Dim dSeries As New SortedDictionary(Of DateTime, String)
        Dim tunit As String
        Debug.WriteLine("Entering ReadWeatherSeries ....")
        Try
            lwdm.Open(WdmFile)

            For i As Integer = 0 To lwdm.DataSets.Count - 1
                'loc = lwdm.DataSets(i).Attributes.GetValue("Location").Trim.ToString()
                'use description for location coz of the long IDs more than field lenght of WDM
                If optDataset = CInt(IsDataSource.NLDAS) Or optDataset = CInt(IsDataSource.GLDAS) Or
                      optDataset = CInt(IsDataSource.TRMM) Or optDataset = CInt(IsDataSource.CMIP6) Then
                    loc = lwdm.DataSets(i).Attributes.GetValue("Location").ToString()
                Else
                    'loc = lwdm.DataSets(i).Attributes.GetValue("Description").Trim.ToString()
                    loc = lwdm.DataSets(i).Attributes.GetValue("StaNam").ToString()
                End If
                tunit = lwdm.DataSets(i).Attributes.GetValue("Time Unit").ToString()
                cons = lwdm.DataSets(i).Attributes.GetValue("Constituent").ToString()
                If svar.ToUpper.Contains(cons.ToUpper()) And site.ToUpper.Contains(loc.ToUpper()) Then
                    dsn = lwdm.DataSets(i).Attributes.GetValue("ID")
                    datanum = i
                    'Debug.WriteLine("DataNum is " + i.ToString() + ", location=" + loc + ", Var=" + cons + ", DSN=" + dsn.ToString())
                    Exit For
                End If
            Next
        Catch ex As Exception
            Dim msg = "Error opening wdm file! " + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try

        'read dataseries
        If datanum < 0 Then Return Nothing
        Try
            lseries = lwdm.DataSets(datanum)
            tunit = lseries.Attributes.GetValue("Time Unit").ToString()
            For j As Integer = 0 To lseries.numValues - 1
                Dim ddate As Date = Date.FromOADate(lseries.Dates.Value(j))
                'Dim val As Double = lseries.Value(j + 1) 'per email from Mark 12/6/07 need to use next index
                'revise 07/28/2020
                Dim val As Double = lseries.Value(j)
                Dim svalue As String = String.Empty
                If Not (Double.IsNaN(val) Or Double.IsInfinity(val)) Then
                    If (svar.Contains("PREC") Or svar.Contains("PRCP")) Then
                        svalue = val.ToString("F4") 'Mark Gray see atcwdmvb
                    ElseIf (svar.Contains("SOLR") Or svar.Contains("LRAD")) Then
                        svalue = val.ToString("F4") 'Mark Gray see atcwdmvb
                    Else
                        svalue = val.ToString("F2") 'Mark Gray see atcwdmvb
                    End If
                    If (svalue.Contains("9999")) Then svalue = "9999"
                    If tunit.Contains("Day") Then
                        dSeries.Add(ddate.AddDays(-1), svalue)
                        'dSeries.Add(ddate, svalue)
                    Else
                        dSeries.Add(ddate, svalue)
                    End If


                End If
            Next
            lseries = Nothing
            lwdm.Clear()
            lwdm = Nothing
            Return dSeries

        Catch ex As Exception
            Dim msg = "Error reading station series from " + WdmFile + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function
    '
    Public Function ReadWeatherSeriesByDSN(ByVal dsn As Integer) As SortedDictionary(Of DateTime, String)
        Debug.WriteLine("Entering ReadWeatherSeriesByDSN ...")
        Dim lwdm As New atcWDM.atcDataSourceWDM
        Dim loc As String, svar As String
        Dim datanum As Integer = -1
        Dim lseries As atcData.atcTimeseries = Nothing
        Dim dSeries As New SortedDictionary(Of DateTime, String)

        Try
            lwdm.Open(WdmFile)
            lseries = lwdm.DataSets.FindData("ID", dsn)(0)

            loc = lseries.Attributes.GetValue("Location").ToString()
            svar = lseries.Attributes.GetValue("Constituent").ToString()
            Debug.WriteLine("Found series {0},{1}", loc, svar)
        Catch ex As Exception
            Dim msg = "Error opening wdm file! " + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try

        'read dataseries
        If lseries = Nothing Then Return Nothing
        Try
            For j As Integer = 0 To lseries.numValues - 1
                Dim ddate As Date = Date.FromOADate(lseries.Dates.Value(j))
                'Dim val As Double = lseries.Value(j + 1) 'per email from Mark 12/6/07 need to use next index
                'revise 07/28/2020
                Dim val As Double = lseries.Value(j)
                Dim svalue As String = String.Empty
                If Not (Double.IsNaN(val) Or Double.IsInfinity(val)) Then
                    If (svar.Contains("PREC") Or svar.Contains("PRCP")) Then
                        svalue = val.ToString("F4") 'Mark Gray see atcwdmvb
                    ElseIf (svar.Contains("SOLR") Or svar.Contains("LRAD")) Then
                        svalue = val.ToString("F4") 'Mark Gray see atcwdmvb
                    Else
                        svalue = val.ToString("F2") 'Mark Gray see atcwdmvb
                    End If
                    If (svalue.Contains("9999")) Then svalue = "9999"
                    dSeries.Add(ddate, svalue)
                End If
            Next
            lseries = Nothing
            lwdm.Clear()
            lwdm = Nothing
            Return dSeries

        Catch ex As Exception
            Dim msg = "Error reading station series from " + WdmFile + "!" + vbCrLf + vbCrLf + ex.Message + vbCrLf + ex.StackTrace
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    Private Sub WriteLog(ByVal msg As String)
        Debug.WriteLine(msg)
        wrlog.WriteLine(msg)
    End Sub

#Region "Old Routines"
    Private Sub WDMWriteOLD(ByVal dsn As Integer, ByVal Scenario As String, ByVal Location As String,
                     ByVal Constituen As String, ByVal DateTimes() As DateTime, ByVal Values() As Double,
                     ByVal lat As Double, ByVal lon As Double, ByVal elev As Double,
                     ByVal descript As String, ByVal stname As String)

        'WriteLog("Entering WDMWrite routine")
        Debug.WriteLine("Entering WDMWrite routine")

        Dim numvalues As Integer = 0
        Try
            Dim lTS As New atcData.atcTimeseries(Nothing)
            lTS.Dates = New atcData.atcTimeseries(Nothing)
            lTS.numValues = Values.Length
            Dim tu As atcUtility.atcTimeUnit = atcUtility.atcTimeUnit.TUHour
            With lTS.Attributes
                .SetValue("Elevation", elev)
                .SetValue("Scenario", Scenario)
                .SetValue("Location", Location)
                .SetValue("StaNam", stname)
                .SetValue("Description", descript) 'use the original station id
                .SetValue("Constituent", Constituen)
                .SetValue("Latitude", lat)
                .SetValue("Longitude", lon)
                .SetValue("TSType", Constituen) 'needed by some legacy apps
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
                        Throw New InvalidConstraintException(String.Format("WDM files are generally expected to contain constant increment time series; specifically, the first two observations must have a standard time step (second, hour, day, month, or year); when this is not true, the dataset cannot be saved to the WDM file. Please examine your data for Location='{0}' and Constituent='{1}'", Location, Constituen))
                    End If
                End If
                .SetValue("tu", tu)
                .SetValue("SJDay", DateTimes(0).ToOADate)
                .SetValue("EJDay", DateTimes(lTS.numValues - 1).ToOADate)
            End With

            lTS.Value(0) = atcUtility.GetNaN()

            For i As Integer = 0 To lTS.numValues
                'there is an apparent problem when subsequent identical dates are used; increment slightly
                'If i > 1 AndAlso DateTimes(i - 1) <= DateTimes(i - 2) Then DateTimes(i - 1) = DateTimes(i - 2).AddMinutes(1)

                lTS.Dates.Value(i - 1) = DateTimes(i - 1).ToOADate
                lTS.Value(i) = Values(i - 1)
            Next
            lTS.Dates.Value(lTS.numValues) = atcUtility.TimAddJ(lTS.Dates.Value(lTS.numValues - 1), tu, 1, 1)
            'lst two dates
            'lTS.Dates.Value(numvalues) = DateTimes(numvalues - 1).ToOADate
            'lTS.Value(numvalues) = Values(numvalues - 1)
            'lTS.Dates.Value(numvalues + 1) = DateTimes(numvalues - 1).ToOADate
            'lTS.Value(numvalues + 1) = Values(numvalues - 1)
            lTS.Attributes.SetValue("ID", dsn)

            Dim Wdm As New atcWDM.atcDataSourceWDM
            'Try
            'Wdm.Open(WdmFile)
            'Catch ex As Exception
            'End Try

            If Wdm.Open(WdmFile) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And
                       ds.Attributes.GetValue("Location", "") = Location And
                       ds.Attributes.GetValue("Constituent", "") = Constituen Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next

                If Not Wdm.AddDataSet(lTS, atcData.atcDataSource.EnumExistAction.ExistReplace) Then
                    Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ='{1}' and Variable ='{2}'",
                           WdmFile.Replace("\", "\\"), Location, Constituen))
                End If
            End If

            Wdm = Nothing
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

#End Region
End Class
