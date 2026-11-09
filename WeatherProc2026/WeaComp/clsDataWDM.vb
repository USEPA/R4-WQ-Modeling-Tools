Imports System.Data

''' <summary>
''' Class for all classes to read WDM data
''' </summary>
''' <remarks>
''' atcWDM library should be able to read/write WDM files, however there are problems in the Fortran
''' libaries that are called that causes terrible crashes; therefore, for reading, use the fully managed
''' atcWDMVb library which does not cause any errors. Writing will still be problematic, however see
''' Mark Gray's email of 10/3/11 for hints about how to work around problems.
''' 
''' Starting in October 2011, this library was changed so that all datasets are returned (no separate tables)
''' Scenario, if present, is prepended to PCode
''' </remarks>
Public Class clsDataWDM
    Inherits clsData

    Private mNumRecords As Integer
    Private mDatasetNum As Integer = 0
    Private mRecordNumDS As Integer = -1
    Private mWDMRead As atcWdmVb.atcWDMfile
    Private hasMultipleScenarios As Boolean = False 'if true will prepend scenario to pcode
    Private dictStations As New Generic.SortedDictionary(Of Integer, String)

    ''' <summary>
    ''' Instantiate clsData, assign filename, open file, set filelength and fieldnames
    ''' </summary>
    ''' <param name="_Filename">Name of WDM file to read</param>
    Public Overrides Sub Initialize(ByVal _Filename As String)
        MyBase.Initialize(_Filename)
        mDataType = enumDataType.WDM
        mFieldNames.Clear()
        mTableNames.Clear()
        mTableNames.Add(IO.Path.GetFileName(_Filename))
        mStationIDs.Clear()
        mPCodes.Clear()
        mMinDate = Date.MaxValue
        mMaxDate = Date.MinValue
        mNumRecords = 0
        mWDMRead = New atcWdmVb.atcWDMfile
        mWDMRead.Open(mFilename)

        mFieldNames.AddRange(New String() {"Station_ID", "PCode", "Date_Time", "Result"})

        Dim DSN As Integer = 0
        For i As Integer = 0 To mWDMRead.DataSets.Count - 1
            If Not mStationIDs.Contains(StationID_DSN(i, DSN)) Then mStationIDs.Add(StationID_DSN(i, DSN))
            If Not mPCodes.Contains(PCode_DS(i)) Then mPCodes.Add(PCode_DS(i))
            Dim staid As String = StationID_DSN(i, DSN)
            Dim pcode As String = PCode_DS(i)
            Dim Scenario As String = Proper(mWDMRead.DataSets(i).Attributes.GetValue("Scenario")).Trim
            Dim lat As Single = mWDMRead.DataSets(i).Attributes.GetValue("Latitude")
            Dim lon As Single = mWDMRead.DataSets(i).Attributes.GetValue("Longitude")

            'concatenate DSN, location, constituent, scenario, latitude, longitude

            Dim sta As String = DSN.ToString() & "_" & staid & "_" & pcode.ToString() & "_" & Scenario
            ' add long and lat
            sta = sta & "_" & lat.ToString() & "_" & lon.ToString()

            Debug.WriteLine(sta)

            'does not work
            'Dim tseries As atcData.atcTimeseries = Nothing
            'tseries = New atcData.atcTimeseries
            'Dim dtbeg As Date = Date.FromOADate(tseries.Dates.Value(0))
            'Dim dtend As Date = Date.FromOADate(tseries.Dates.Value(tseries.numValues - 2))
            'tseries = Nothing
            'Debug.WriteLine("date beg =" + dtbeg.ToString() + ", date end = " + dtend.ToString())

            dictStations.Add(DSN, sta)
        Next
        'if any station IDs are associated with multiple DSNs add that as an additional item: ID (All DSNs)
        'also determine if the file contains multiple scenarios (so know whether to prepend scenario for PCode)

        Dim dictStationIDs As New Generic.Dictionary(Of String, Integer)
        Dim lstScenarios As New Generic.List(Of String)
        For i As Integer = 0 To mWDMRead.DataSets.Count - 1
            'Dim ts As atcData.atcTimeseries = mWDMRead.DataSets(i)
            'With ts
            'If .numValues > 0 Then

            Dim Scenario As String = Proper(mWDMRead.DataSets(i).Attributes.GetValue("Scenario")).Trim
            If Not lstScenarios.Contains(Scenario) Then lstScenarios.Add(Scenario)

            Dim StaID As String = StationID(i)

            If Not dictStationIDs.ContainsKey(StaID) Then dictStationIDs.Add(StaID, 0)
            dictStationIDs(StaID) += 1

            'Debug.WriteLine("Scenario=" + Scenario + ", Station=" + StaID)
        Next

        For Each kv As KeyValuePair(Of String, Integer) In dictStationIDs
            If kv.Value > 1 Then
                'following will be used to let users import data from multiple DSNs for a given Location ID
                Dim StaID As String = kv.Key & " (All DSNs)"
                If Not mStationIDs.Contains(StaID) Then mStationIDs.Add(StaID)
            End If
            'Debug.WriteLine("kv key=" + kv.Key + ", value=" + kv.Value.ToString())
        Next

        hasMultipleScenarios = lstScenarios.Count > 1
        mDatasetNum = 0
        'mWDMRead = Nothing

    End Sub

    Public ReadOnly Property GetGagesAttributeTable() As Generic.SortedDictionary(Of Integer, String)
        Get
            Return dictStations
        End Get
    End Property
    ''' <summary>
    ''' Instantiate clsData, assign filename, open file, set filelength and fieldnames
    ''' </summary>
    ''' <param name="_Filename">Name of text file to read</param>
    Public Sub InitializeWrite(ByVal _Filename As String)
        MyBase.Initialize(_Filename)
        mDataType = enumDataType.WDM
    End Sub

    Public Function ReadGageData(ByVal dsn As Integer) As atcData.atcTimeseries
        Dim tseries As atcData.atcTimeseries = Nothing
        For i As Integer = 0 To mWDMRead.DataSets.Count - 1
            Dim IDdsn As Integer = 0
            Dim sta As String = StationID_DSN(i, IDdsn)

            If (IDdsn = dsn) Then
                'Dim ts As atcData.atcTimeseries = mWDMRead.DataSets(i)
                tseries = mWDMRead.DataSets(i)
                'For j As Integer = 0 To tseries.numValues - 1
                'Dim d As Date = Date.FromOADate(tseries.Dates.Value(j))
                'Dim v As Double = tseries.Value(j + 1) 'per email from Mark 12/6/07 need to use next index
                'If Not (Double.IsNaN(v) Or Double.IsInfinity(v)) Then
                'End If
                'Next
                'tseries = ts
                Exit For
            End If
        Next
        Return tseries
    End Function

    Public Sub Write(ByVal Scenario As String, ByVal StationID As String, ByVal StationName As String, ByVal PCode As String, ByVal DateTimes() As DateTime, ByVal Values() As Double)
        Try
            Dim lTS As New atcData.atcTimeseries(Nothing)
            lTS.Dates = New atcData.atcTimeseries(Nothing)
            lTS.numValues = Values.Length
            Dim tu As atcUtility.atcTimeUnit = atcUtility.atcTimeUnit.TUHour
            With lTS.Attributes
                .SetValue("Scenario", Scenario)
                .SetValue("Location", StationID)
                .SetValue("Description", StationName)
                .SetValue("Constituent", PCode)
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
                        Throw New InvalidConstraintException(String.Format("WDM files are generally expected to contain constant increment time series; specifically, the first two observations must have a standard time step (second, hour, day, month, or year); when this is not true, the dataset cannot be saved to the WDM file. Please examine your WRDB data for Station ID='{0}' and PCode='{1}'", StationID, PCode))
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
                Wdm.Open(mFilename)
            Catch ex As Exception
            End Try

            If Wdm.Open(mFilename) Then
                For Each ds As atcData.atcDataSet In Wdm.DataSets
                    If ds.Attributes.GetValue("Scenario", "") = Scenario And ds.Attributes.GetValue("Location", "") = StationID And ds.Attributes.GetValue("Constituent", "") = PCode Then
                        Wdm.RemoveDataset(ds)
                        Exit For
                    End If
                Next

                Dim dsn As Integer = 0
                dsn += 1
                lTS.Attributes.SetValue("ID", dsn)
                If Not Wdm.AddDataSet(lTS, atcData.atcDataSource.EnumExistAction.ExistRenumber) Then Throw New InvalidOperationException(String.Format("Unable to write WDM file: {0} while writing Station ID='{1}' and PCode='{2}'", mFilename.Replace("\", "\\"), StationID, PCode))
            Else
                Throw New System.IO.IOException(String.Format("Unable to open WDM file: {0} while writing Station ID='{1}' and PCode='{2}'", mFilename.Replace("\", "\\"), StationID, PCode))
            End If
        Catch ex As Exception
            ErrorMsg(, ex)
        End Try
    End Sub

    Public Overrides Sub Rewind()
        MyBase.Rewind()
        mDatasetNum = 0
        mRecordNumDS = -1
    End Sub

    ''' <summary>
    ''' Currently active station ID (may allow some sources to avoid long sequential reads)
    ''' </summary>
    Public Overrides Property ActiveStationID() As String
        Get
            Return MyBase.ActiveStationID
        End Get
        Set(ByVal value As String)
            MyBase.ActiveStationID = value
            For i As Integer = 0 To mWDMRead.DataSets.Count - 1
                If StationID_DS(i) = value OrElse StationID_DSAll(i) = value Then
                    mDatasetNum = i
                    mRecordNumDS = -1
                    Exit For
                End If
            Next
        End Set
    End Property

    ''' <summary>
    ''' Read next line from file into memory, storing into mItems array
    ''' Also save mLastLineRead for error reporting
    ''' Automatically advance through all datasets
    ''' </summary>
    ''' <returns>True if successful, false if eof reached and no more data</returns>
    Public Overrides Function Read() As Boolean
        Try
            If Not MyBase.Read() Then Return False
            If mRecordNum > mNumRecords - 1 Then Return False
            mItems.Clear()
            Dim ts As atcData.atcTimeseries = mWDMRead.DataSets(mDatasetNum)
            mRecordNumDS += 1
            If mRecordNumDS > ts.numValues - 1 Then
                mDatasetNum += 1
                If mActiveStationIDSet Then                    'search for next dataset having this station id
                    While mDatasetNum <= mWDMRead.DataSets.Count - 1 AndAlso StationID_DS(mDatasetNum) <> mActiveStationID AndAlso StationID_DSAll(mDatasetNum) <> mActiveStationID
                        mDatasetNum += 1
                    End While
                End If

                If mDatasetNum > mWDMRead.DataSets.Count - 1 Then Return False
                mRecordNumDS = 0
                ts = mWDMRead.DataSets(mDatasetNum)
            End If

            If ts.numValues > 0 Then
                mItems.Add(StationID(mDatasetNum))
                mItems.Add(PCode_DS(mDatasetNum))
                mItems.Add(RoundDateTime(Date.FromOADate(ts.Dates.Value(mRecordNumDS))).ToString(MyDateTimeFormat))
                Dim v As Double = ts.Value(mRecordNumDS + 1) 'per email from Mark 12/6/07 need to use next index
                If (Double.IsNaN(v) Or Double.IsInfinity(v)) Then mItems.Add("") Else mItems.Add(Math.Round(v, 5))
            End If
            Return True
        Catch ex As Exception
            ErrorMsg(, ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Return string identifying specific DSN (for those who pay more attention to DSN than anything else)
    ''' </summary>
    ''' <remarks>Cache results to improve performance</remarks>
    Private Function StationID(DatasetNum As Integer) As String
        Static LastKey As Integer = -1
        Static CachedValue As String = ""
        If DatasetNum = LastKey Then Return CachedValue
        With mWDMRead.DataSets(DatasetNum).Attributes
            Dim StaID As String = .GetValue("Location")
            If StaID = "" Then
                Dim DsnID As String = .GetValue("ID")
                StaID = String.Format("_DSN_{0}", DsnID)
            End If
            CachedValue = StaID
            LastKey = DatasetNum
            Return StaID
        End With
    End Function

    ''' <summary>
    ''' Return string identifying specific DSN (for those who pay more attention to DSN than anything else)
    ''' </summary>
    ''' <remarks>Cache results to improve performance</remarks>
    Private Function StationID_DSN(DatasetNum As Integer, ByRef DSN As Integer) As String
        Static LastKey As Integer = -1
        Static CachedValue As String = ""
        If DatasetNum = LastKey Then Return CachedValue
        With mWDMRead.DataSets(DatasetNum).Attributes
            Dim StaID As String = .GetValue("Location")
            'If StaID <> "" Then StaID &= ""
            Dim DsnID As Integer = .GetValue("ID")
            DSN = DsnID
            'StaID &= String.Format("_DSN_{0}", DsnID)
            CachedValue = StaID
            LastKey = DatasetNum
            Return StaID
        End With
    End Function
    ''' <summary>
    ''' Return string identifying specific DSN (for those who pay more attention to DSN than anything else)
    ''' </summary>
    ''' <remarks>Cache results to improve performance</remarks>
    Private Function StationID_DS(DatasetNum As Integer) As String
        Static LastKey As Integer = -1
        Static CachedValue As String = ""
        If DatasetNum = LastKey Then Return CachedValue
        With mWDMRead.DataSets(DatasetNum).Attributes
            Dim StaID As String = .GetValue("Location")
            If StaID <> "" Then StaID &= ""
            Dim DsnID As String = .GetValue("ID")
            StaID &= String.Format("_DSN_{0}", DsnID)
            CachedValue = StaID
            LastKey = DatasetNum
            Return StaID
        End With
    End Function

    ''' <summary>
    ''' Return string that can be used to select a given location in multiple DSNs (e.g., multiple constituents)
    ''' </summary>
    ''' <remarks>Cache results to improve performance</remarks>
    Private Function StationID_DSAll(DatasetNum As Integer) As String
        Static LastKey As Integer = -1
        Static CachedValue As String = ""
        If DatasetNum = LastKey Then Return CachedValue
        With mWDMRead.DataSets(DatasetNum).Attributes
            Dim StaID As String = .GetValue("Location")
            If StaID <> "" Then StaID &= " "
            Dim DsnID As String = .GetValue("ID")
            StaID &= "(All DSNs)"
            CachedValue = StaID
            LastKey = DatasetNum
            Return StaID
        End With
    End Function

    Private Function PCode_DS(DatasetNum As Integer) As String
        Static LastKey As Integer = -1
        Static CachedValue As String = ""
        If DatasetNum = LastKey Then Return CachedValue
        With mWDMRead.DataSets(DatasetNum).Attributes
            Dim Scenario As String = Proper(.GetValue("Scenario")).Trim
            If Scenario <> "" Then Scenario &= " "
            Dim PCode As String = ""
            If hasMultipleScenarios Then
                PCode = Scenario & .GetValue("Constituent")
            Else
                PCode = .GetValue("Constituent")
            End If
            CachedValue = PCode
            LastKey = DatasetNum
            Return PCode
        End With
    End Function

    ''' <summary>
    ''' Get data table containing time series data for a specified station id and pcode
    ''' Table will contain date-time field and result field
    ''' </summary>

    Public Overrides Function GetDataTable(ByVal StationID As String, ByVal PCode As String) As DataTable
        If Not mStationIDs.Contains(StationID) Then
            Throw New ApplicationException("WDM file does not contain the station: " & StationID)
            Return Nothing
        End If
        Dim itemNum As Integer = -1
        If Not mPCodes.Contains(PCode) Then
            Throw New ApplicationException("WDM file does not contain the column: " & PCode)
            Return Nothing
        End If
        Dim dt As DataTable = MyBase.SetupDataTable(enumDataTableColumns.Date_Result)
        'find the dataset that references that particular station & pcode
        For i As Integer = 0 To mWDMRead.DataSets.Count - 1
            If (StationID_DS(i) = StationID OrElse StationID_DSAll(i) = StationID) AndAlso PCode_DS(i) = PCode Then
                Dim ts As atcData.atcTimeseries = mWDMRead.DataSets(i)
                For j As Integer = 0 To ts.numValues - 1
                    Dim d As Date = Date.FromOADate(ts.Dates.Value(j))
                    Dim v As Double = ts.Value(j + 1) 'per email from Mark 12/6/07 need to use next index
                    If Not (Double.IsNaN(v) Or Double.IsInfinity(v)) Then dt.Rows.Add(d, Math.Round(v, 5))
                Next
                Exit For
            End If
        Next
        Return dt
    End Function

    ''' <summary>
    ''' Determine percent of file that has been read
    ''' </summary>
    ''' <returns>Percent of file read</returns>
    Public Overrides Function PercentComplete() As Double
        Return (mRecordNum + 1) * 100.0 / mNumRecords
    End Function

    Private Function Proper(ByVal s As String) As String
        Return My.Application.Culture.TextInfo.ToTitleCase(s)
    End Function

End Class
