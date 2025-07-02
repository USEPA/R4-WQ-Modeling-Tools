Imports System
Imports System.Collections
Imports System.Data.SQLite
Imports System.Diagnostics
Imports System.IO
Imports System.Data
Imports System.Windows.Forms
Imports System.Text
Imports atcMwGisUtility
Imports atcUtility
Imports MapWinUtility


Public Class clsAir

    Public Sub New()
        AssignClimateStations()
    End Sub

    Private Function AssignClimateStations() As List(Of String)
        Debug.WriteLine("Entering AssignClimateStations...")

        Dim optPcp As String = String.Empty, optWea As String = String.Empty
        Try
            Dim pmetmsg As String = "Select Met Stations Layer Shapefile"
            Dim pTitle As String = "LSPC Model Segmentation"

            'revised 5/2018
            'Dim lSubbasinLayerName As String = cboLayer.Items(cboLayer.SelectedIndex)
            Dim lSubbasinLayerName As String = "LSPCSubbasins"
            Dim lSubbasinLayerIndex As Integer = GisUtil.LayerIndex(lSubbasinLayerName)
            Dim dsMetSta As DataTable = LSPCDataSet.Tables.Item(enumTable.WeatherTable).Clone()

            Dim lNumSubbasins As Integer = GisUtil.NumFeatures(lSubbasinLayerIndex)
            Dim lMetLayerName As String = "LSPCMetLayer"
            Dim lMetLayerIndex As Integer = GisUtil.LayerIndex(lMetLayerName)

            'assign gages
            dictWDM.TryGetValue("PREC", optPcp)
            AssignMetStationsByProximity(optPcp, lSubbasinLayerName, lMetLayerName, "PREC", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("ATEM", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "ATEM", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("PEVT", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "PEVT", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("SOLR", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "SOLR", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("WIND", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "WIND", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("DEWP", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "DEWP", PeriodFrom, PeriodTo)
            dictWDM.TryGetValue("CLOU", optWea)
            AssignMetStationsByProximity(optWea, lSubbasinLayerName, lMetLayerName, "CLOU", PeriodFrom, PeriodTo)

            Dim lFieldIndex As Integer = GisUtil.FieldIndex(lSubbasinLayerIndex, "SUBBASIN")
            Dim sBasin = ""
            Dim sPrec = ""
            Dim sTemp, sPevt, sSolr, sAir, sDewp, sClou, sWind As String
            'create list of weather stations by subbasin, will be filled up after assignment below

            For lIndex As Integer = 1 To GisUtil.NumFeatures(lSubbasinLayerIndex)
                sBasin = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, lFieldIndex)
                'If optNLDAS.Checked Then
                'If GisUtil.IsField(lSubbasinLayerIndex, "PREC_NLDAS") Then
                sPrec = ""
                sPrec = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "PREC"))
                sTemp = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "ATEM"))
                sPevt = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "PEVT"))
                sSolr = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "SOLR"))
                sDewp = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "DEWP"))
                sClou = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "CLOU"))
                sWind = GisUtil.FieldValue(lSubbasinLayerIndex, lIndex - 1, GisUtil.FieldIndex(lSubbasinLayerIndex, "WIND"))

                'added other weather params 4/2/2018
                sAir = sPrec + "_" + sPevt + "_" + sTemp + "_" + sWind + "_" + sSolr + "_" + sDewp + "_" + sClou
                dsMetSta.Rows.Add(CInt(sBasin), sPrec, sPevt, sTemp, sWind, sSolr, sDewp, sClou, sAir + ".air", 1)
            Next

            'get unique station combination and fill table of the weatherfile number
            'and debug
            Dim lstUnique As New List(Of String)
            Dim lst = (From dr As DataRow In dsMetSta.Rows
                       Select dr!AIRFILE Distinct)

            For Each item In lst
                'Debug.WriteLine("Unique Sta = " + item.ToString())
                lstUnique.Add(item.ToString())
            Next

            Dim sta As String, staIndex As Integer
            For Each dr As DataRow In dsMetSta.Rows
                sta = dr.Item("AIRFILE").ToString()
                staIndex = lstUnique.IndexOf(sta)
                dr.Item("WEAFILENUM") = staIndex + 1
                'Debug.WriteLine("Index Airfile = " + (staIndex + 1).ToString())
            Next

            dgvMet.DataSource = Nothing
            dgvMet.DataSource = dsMetSta

            'fill weather mapping tables
            Dim dt As DataTable = LSPCDataSet.Tables.Item(enumTable.WeatherMapping)
            dt.Clear()

            'Dim tempsta As String
            'Dim lVarIndex As Integer = GisUtil.FieldIndex(GisUtil.LayerIndex(lMetLayerName), "CONSTITUEN")
            'Dim lLocIndex As Integer = GisUtil.FieldIndex(GisUtil.LayerIndex(lMetLayerName), "LOCATION")

            For Each dr As DataRow In dsMetSta.Rows
                sta = dr.Item("AIRFILE").ToString()
                Dim elev As Single = 0.0
                For lbasin As Integer = 0 To numBasins - 1
                    Dim subbasin As Integer = GisUtil.FieldValue(lSubbasinLayerIndex, lbasin, GisUtil.FieldIndex(lSubbasinLayerIndex, "SUBBASIN"))
                    If subbasin = CInt(dr.Item("SUBBASIN")) Then
                        elev = GisUtil.FieldValue(lSubbasinLayerIndex, lbasin, GisUtil.FieldIndex(lSubbasinLayerIndex, "MEANELEV"))
                    End If
                Next
                'elev already in ft, add 2 meters for station elevation, generally
                dt.Rows.Add(CInt(dr.Item("SUBBASIN")), dr.Item("WEAFILENUM"), 1, 1, (elev + 6.56), sta)
            Next

            'fill weather filenames table
            Dim lstUniqueNum As New List(Of Integer)
            dt = LSPCDataSet.Tables.Item(enumTable.WeatherFileNames)
            dt.Rows.Clear()
            For Each item In lstUnique
                sta = item.ToString()
                staIndex = lstUnique.IndexOf(sta)
                dt.Rows.Add(staIndex + 1, sta, 1)
                lstUniqueNum.Add(staIndex + 1)
            Next
            dt = Nothing

            'fill weather combination table
            dt = LSPCDataSet.Tables.Item(enumTable.WeatherCombinations)
            dt.Rows.Clear()
            For ista As Integer = 0 To lstUnique.Count - 1
                dt.Rows.Add(ista + 1, 1, ista + 1)
            Next

            'fill weather structure table
            dt = LSPCDataSet.Tables.Item(enumTable.WeatherStructure)
            dt.Rows.Clear()
            For i As Integer = 1 To 7
                dt.Rows.Add(1, i, i)
            Next

            'fill weather elevation i, select unique stations 
            dt = LSPCDataSet.Tables.Item(enumTable.WeatherElevations)
            dt.Rows.Clear()
            Dim aTable As DataTable = LSPCDataSet.Tables.Item(enumTable.WeatherMapping)

            '6/7/2019 changes
            For Each wnum In lstUniqueNum
                Dim lststa = (From dr As DataRow In aTable.Rows
                              Where dr!WSTNUM = wnum
                              Select dr!ELEVFT)
                'Select Case dr!ELEVFT, dr!WSTNUM, dr!SUBBASIN Distinct)
                'For Each item In lststa
                'Debug.WriteLine("wstnum = " + item.WSTNUM.ToString() + ", " + item.ELEVFT.ToString())
                'dt.Rows.Add(item.WSTNUM, item.ELEVFT)
                'Next
                Dim imax As Double = lststa.Max
                'Debug.WriteLine("wstnum = " + wnum.ToString() + ", " + imax.ToString())
                dt.Rows.Add(wnum, imax)
                lststa = Nothing
            Next

            Return lstUnique
        Catch ex As Exception
            MessageBox.Show("Error in assigning weather stations!" + vbCrLf + ex.Message + vbCrLf + vbCrLf + ex.StackTrace)
            Return Nothing
        End Try

    End Function

    Public Sub AssignMetStationsByProximity(ByVal optScen As String, ByVal aSubbasinLayerName As String, ByVal aMetLayerName As String,
                                            ByVal MetVar As String, ByVal PeriodFrom As Integer, ByVal PeriodTo As Integer)
        Debug.WriteLine(".....       ")
        Debug.WriteLine("Calling AssignStationsby Proximity.....        ")
        Debug.WriteLine("MetLayer =" + aMetLayerName + " MetVar = " + MetVar)
        Debug.WriteLine(".....       ")

        Dim lSubbasinLayerIndex As Integer = GisUtil.LayerIndex(aSubbasinLayerName)
        Dim lMetLayerIndex As Integer = GisUtil.LayerIndex(aMetLayerName)
        Dim lModelSegFieldIndex, lMetFieldIndex, lCount As Integer
        Dim lMetStationsSelected As New atcCollection

        Select Case MetVar
            Case "PREC"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, MetVar)
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "PREC", PeriodFrom, PeriodTo)
            Case "ATEM"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "ATEM")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "ATEM", PeriodFrom, PeriodTo)
            Case "PEVT"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "PEVT")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "PEVT", PeriodFrom, PeriodTo)
            Case "SOLR"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "SOLR")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "SOLR", PeriodFrom, PeriodTo)
            Case "WIND"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "WIND")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "WIND", PeriodFrom, PeriodTo)
            Case "DEWP"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "DEWP")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "DEWP", PeriodFrom, PeriodTo)
            Case "CLOU"
                lMetStationsSelected.Clear()
                lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, "CLOU")
                lCount = SetSelectedFeatures(optScen, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "CLOU", PeriodFrom, PeriodTo)
        End Select

        'Debug.WriteLine("MetVar = " + MetVar + ", " +
        '                "MetfieldIndex=" + lMetFieldIndex.ToString() + ", " +
        '                "ModelSegFieldIndex=" + lModelSegFieldIndex.ToString() + ", " +
        '                "Count = " + lCount.ToString())

        'loop through each subbasin and assign to nearest met station
        Dim lSubBasinX As Double = 0.0, lSubBasinY As Double = 0.0
        Dim lMetSegX As Double = 0, lMetSegY As Double = 0
        GisUtil.StartSetFeatureValue(lSubbasinLayerIndex)

        Dim msg As String = "Assigning Nearest " + MetVar + " Station"
        'Debug.WriteLine("Assigning Nearest " + MetVar + " Station")
        Dim lsub As String = String.Empty

        For lSubBasinIndex As Integer = 1 To GisUtil.NumFeatures(lSubbasinLayerIndex)
            Logger.Progress(msg, lSubBasinIndex, GisUtil.NumFeatures(lSubbasinLayerIndex))

            'get subbasin
            lsub = GisUtil.FieldValue(lSubbasinLayerIndex, lSubBasinIndex - 1,
                                             GisUtil.FieldIndex(lSubbasinLayerIndex, "SUBBASIN"))
            'Debug.WriteLine("Subbasin=" + lsub.ToString() + ", Index = " + (lSubBasinIndex - 1).ToString())

            'get centroid of this subbasin
            GisUtil.ShapeCentroid(lSubbasinLayerIndex, lSubBasinIndex - 1, lSubBasinX, lSubBasinY)
            'now find the closest met station
            Dim lShortestDistance As Double = Double.MaxValue
            Dim lClosestMetStationIndex As Integer = -1
            Dim lDistance As Double = 0.0
            Dim lMetSta As String = String.Empty

            For lMetStationIndex As Integer = 0 To lMetStationsSelected.Count - 1
                GisUtil.PointXY(lMetLayerIndex, lMetStationsSelected(lMetStationIndex), lMetSegX, lMetSegY)
                'calculate the distance
                'lDistance = Math.Sqrt(((Math.Abs(lSubBasinX) - Math.Abs(lMetSegX)) ^ 2) + _
                '                      ((Math.Abs(lSubBasinY) - Math.Abs(lMetSegY)) ^ 2))
                lDistance = Math.Sqrt(((lSubBasinX - lMetSegX) ^ 2) +
                                      ((lSubBasinY - lMetSegY) ^ 2))
                If lDistance < lShortestDistance Then
                    lShortestDistance = lDistance
                    lClosestMetStationIndex = lMetStationsSelected(lMetStationIndex)
                End If

                'get station at the given met index
                lMetSta = GisUtil.FieldValue(lMetLayerIndex, lMetStationsSelected(lMetStationIndex),
                                             GisUtil.FieldIndex(lMetLayerIndex, "Location"))
                'Debug.WriteLine("Index =" + lMetStationIndex.ToString() + ", " +
                '                "Station =" + lMetSta.ToString() + ", " +
                '                "BasinX=" + lSubBasinX.ToString() + ", BasinY=" + lSubBasinY.ToString() + ", " +
                '                "MetX=" + lMetSegX.ToString() + ", MetY=" + lMetSegY.ToString() + ", " +
                '                "Distance=" + lDistance.ToString())
            Next

            If lClosestMetStationIndex > -1 Then 'set ModelSeg attribute
                Dim lModelSegText As String = ""
                Dim lLocationFieldIndex As Integer = -1
                If GisUtil.IsField(lMetLayerIndex, "Location") Then
                    lLocationFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "Location")
                    lModelSegText = GisUtil.FieldValue(lMetLayerIndex, lClosestMetStationIndex, lLocationFieldIndex)
                End If
                'Debug.WriteLine("Metvar = " + MetVar + " ModelSegText=" + lModelSegText)
                GisUtil.SetFeatureValueNoStartStop(lSubbasinLayerIndex, lModelSegFieldIndex, lSubBasinIndex - 1, lModelSegText)
            End If
        Next
        GisUtil.StopSetFeatureValue(lSubbasinLayerIndex)
        Logger.Progress("", 0, 0)

    End Sub

    Public Sub AssignMetStationsByProximityRev(ByVal optionRain As String, ByVal optionWea As String, ByVal aSubbasinLayerName As String, ByVal aMetLayerName As String,
                                            ByVal MetVar As String, ByVal PeriodFrom As Integer, ByVal PeriodTo As Integer)
        Debug.WriteLine(".....       ")
        Debug.WriteLine("Calling AssignStationsby Proximity.....        ")
        Debug.WriteLine("MetLayer =" + aMetLayerName + " MetVar = " + MetVar)
        Debug.WriteLine(".....       ")

        Dim lSubbasinLayerIndex As Integer = GisUtil.LayerIndex(aSubbasinLayerName)
        Dim lMetLayerIndex As Integer = GisUtil.LayerIndex(aMetLayerName)
        Dim lModelSegFieldIndex, lMetFieldIndex, lCount As Integer
        Dim lMetStationsSelected As New atcCollection

        Select Case MetVar
            Case "PREC"
                If optionRain = "NLDAS" Then
                    lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, MetVar)
                    lMetStationsSelected.Clear()
                    For lIndex As Integer = 1 To GisUtil.NumFeatures(lMetLayerIndex)
                        lMetStationsSelected.Add(lIndex - 1)
                    Next
                Else
                    lMetStationsSelected.Clear()
                    lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                    lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, MetVar)
                    lCount = SetSelectedFeatures(optionRain, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, "PREC", PeriodFrom, PeriodTo)
                End If

            Case "ATEM", "PEVT", "SOLR", "WIND", "DEWP", "CLOU"
                If optionWea = "NLDAS" Then
                    lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, MetVar)
                    lMetStationsSelected.Clear()
                    For lIndex As Integer = 1 To GisUtil.NumFeatures(lMetLayerIndex)
                        lMetStationsSelected.Add(lIndex - 1)
                    Next
                Else
                    lMetStationsSelected.Clear()
                    lMetFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "CONSTITUEN")
                    lModelSegFieldIndex = MetSegFieldIndex(lSubbasinLayerIndex, MetVar)
                    lCount = SetSelectedFeatures(optionRain, lMetLayerIndex, lMetFieldIndex, lMetStationsSelected, MetVar, PeriodFrom, PeriodTo)
                End If
        End Select

        'loop through each subbasin and assign to nearest met station
        Dim lSubBasinX As Double, lSubBasinY As Double
        Dim lMetSegX As Double, lMetSegY As Double
        GisUtil.StartSetFeatureValue(lSubbasinLayerIndex)

        Dim msg As String = "Assigning Nearest " + MetVar + " Station"

        For lSubBasinIndex As Integer = 1 To GisUtil.NumFeatures(lSubbasinLayerIndex)
            'Debug.WriteLine("subbasin=" + lSubBasinIndex.ToString())
            'do progress
            Logger.Progress(msg, lSubBasinIndex, GisUtil.NumFeatures(lSubbasinLayerIndex))

            'System.Windows.Forms.Application.DoEvents()

            'get centroid of this subbasin
            GisUtil.ShapeCentroid(lSubbasinLayerIndex, lSubBasinIndex - 1, lSubBasinX, lSubBasinY)
            'now find the closest met station
            Dim lShortestDistance As Double = Double.MaxValue
            Dim lClosestMetStationIndex As Integer = -1
            Dim lDistance As Double = 0.0
            For lMetStationIndex As Integer = 0 To lMetStationsSelected.Count - 1
                'Debug.WriteLine("met station selected =" + lMetStationIndex.ToString())
                GisUtil.PointXY(lMetLayerIndex, lMetStationsSelected(lMetStationIndex), lMetSegX, lMetSegY)
                'calculate the distance
                'lDistance = Math.Sqrt(((Math.Abs(lSubBasinX) - Math.Abs(lMetSegX)) ^ 2) + _
                '                      ((Math.Abs(lSubBasinY) - Math.Abs(lMetSegY)) ^ 2))
                lDistance = Math.Sqrt(((lSubBasinX - lMetSegX) ^ 2) +
                                      ((lSubBasinY - lMetSegY) ^ 2))
                If lDistance < lShortestDistance Then
                    lShortestDistance = lDistance
                    lClosestMetStationIndex = lMetStationsSelected(lMetStationIndex)
                End If
            Next

            If lClosestMetStationIndex > -1 Then 'set ModelSeg attribute
                Dim lModelSegText As String = ""
                Dim lLocationFieldIndex As Integer = -1
                If GisUtil.IsField(lMetLayerIndex, "Location") Then
                    lLocationFieldIndex = GisUtil.FieldIndex(lMetLayerIndex, "Location")
                    lModelSegText = GisUtil.FieldValue(lMetLayerIndex, lClosestMetStationIndex, lLocationFieldIndex)
                End If
                'Debug.WriteLine("Metvar = " + MetVar + " ModelSegText=" + lModelSegText)
                GisUtil.SetFeatureValueNoStartStop(lSubbasinLayerIndex, lModelSegFieldIndex, lSubBasinIndex - 1, lModelSegText)
            End If
        Next
        GisUtil.StopSetFeatureValue(lSubbasinLayerIndex)
        Logger.Progress("", 0, 0)

    End Sub

    Public Function MetSegFieldIndex(ByVal aSubbasinLayerIndex As Integer, ByVal lColName As String) As Integer
        'check to see if modelseg field is on subbasins layer, add if not 

        Dim lModelSegFieldIndex As Integer = -1
        If GisUtil.IsField(aSubbasinLayerIndex, lColName) Then
            lModelSegFieldIndex = GisUtil.FieldIndex(aSubbasinLayerIndex, lColName)
        Else  'need to add it
            lModelSegFieldIndex = GisUtil.FieldIndexAddIfMissing(aSubbasinLayerIndex, lColName, 0, 40)
            'lModelSegFieldIndex = GisUtil.AddField(aSubbasinLayerIndex, lColName, 0, 40)
        End If
        'Debug.WriteLine("In Metsegfieldindex: column =" + lColName + " Field Index=" + lModelSegFieldIndex.ToString())
        Return lModelSegFieldIndex
    End Function

    Public Function SetSelectedFeatures(ByVal optScen As String, ByVal lMetLayerIndex As Integer, ByVal lMetFieldIndex As Integer, ByRef lMetStationsSelected As atcCollection,
                                        ByVal MetVar As String, ByVal SimPeriodFrom As Integer, ByVal SimPeriodTo As Integer) As Integer
        'lMetFieldIndex - index of col CONSTITUEN
        'lScenIndex -index of col SCENARIO

        Dim lScenarioIndex As Integer = GisUtil.FieldIndex(lMetLayerIndex, "SCENARIO")

        lMetStationsSelected.Clear()
        Dim lConstituent As String
        Dim lScenario As String
        Dim startDate As String = ""
        Dim endDate As String = ""
        Dim sYear As Integer = 1900
        Dim eYear As Integer = 2006
        Dim defaultEnd As Integer = 2005

        'Debug.WriteLine("sim from = " + SimPeriodFrom.ToString() + ", sim to=" + SimPeriodTo.ToString())
        For lIndex As Integer = 1 To GisUtil.NumFeatures(lMetLayerIndex)
            lConstituent = GisUtil.FieldValue(lMetLayerIndex, lIndex - 1, lMetFieldIndex)
            lScenario = GisUtil.FieldValue(lMetLayerIndex, lIndex - 1, lScenarioIndex)

            'select only stations that are within the sim period, since wdm is extended only to 2006,2009, set 2006 at cutoff
            'dont check for dates 1/29/19

            'startDate = GisUtil.FieldValue(lMetLayerIndex, lIndex - 1, GisUtil.FieldIndex(lMetLayerIndex, "STARTDATE"))
            'endDate = GisUtil.FieldValue(lMetLayerIndex, lIndex - 1, GisUtil.FieldIndex(lMetLayerIndex, "ENDDATE"))
            'sYear = CInt(startDate.Substring(0, 4))
            'eYear = CInt(endDate.Substring(0, 4))

            If lConstituent.ToUpper.Contains(MetVar.ToUpper().Trim()) Then 'And
                'do not use scenario, assumes wdm contains only stations to be used
                'lScenario.ToUpper().Contains(optScen.ToUpper()) Then
                'If (sYear <= SimPeriodFrom AndAlso eYear >= defaultEnd) Then
                lMetStationsSelected.Add(lIndex - 1)
                'End If
                'Debug.WriteLine("In SetSelectedStation: MetFieldIndex =" + lMetFieldIndex.ToString() + " lConstituent=" + lConstituent + " MetVar =" + MetVar +
                '                "Feature Index = " + lIndex.ToString() + ", startdate=" + startDate +
                '                ", enddate=" + endDate + ", syear=" + sYear.ToString() + ", eyear=" + eYear.ToString() + ", defaultend=" + defaultEnd.ToString())
                'Debug.WriteLine("In SetSelectedStation: MetFieldIndex =" + lMetFieldIndex.ToString() + " lConstituent=" + lConstituent + " MetVar =" + MetVar +
                '                " Feature Index = " + lIndex.ToString())
            End If
        Next
        Return lMetStationsSelected.Count()
    End Function

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

    Public Sub Initialize(ByVal _Filename As String)
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

End Class
