Imports atcMwGisUtility
Imports atcUtility
Imports MapWinUtility

Module modMetSegment
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

End Module
