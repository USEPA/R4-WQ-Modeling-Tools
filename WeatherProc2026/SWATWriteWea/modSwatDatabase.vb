Imports MapWinUtility
Imports atcUtility
Imports atcData
Imports D4EM.Geo
Imports SwatObject
Imports System.Data

Public Module modSwatDatabase
    Private SWATParamsXmlDoc As Xml.XmlDocument

    Public Enum WhichSoilIndex
        USGS
        SWATSTATSGO
        SSURGO
    End Enum

    Friend Sub BuildSwatDatabase(ByVal aCacheFolder As String, _
                                 ByVal aProjectFolder As String, _
                                 ByVal aFlowlinesFilename As String, _
                                 ByVal aScenario As String, _
                                 ByVal aSwatDatabaseName As String, _
                                 ByVal aParamTable As atcTable, _
                                 ByVal aSubBasinToParamIndex As atcCollection, _
                                 ByVal aHruTable As HRUTable, _
                                 ByVal aSoilProperties As List(Of D4EM.Data.Source.NRCS_Soil.SoilLocation.Soil), _
                                 ByVal aSimulationStartYear As Integer, _
                                 ByVal aSimulationEndYear As Integer, _
                                 ByVal aUseMgtCropFile As Boolean, _
                                 ByVal aCreateArcSWATFiles As Boolean, _
                                 ByVal aFields As D4EM.Geo.NetworkOperations.FieldIndexes,
                                 ByVal aReportFlowUnits As Short)

        If aHruTable Is Nothing Then
            Logger.Dbg("BuildSwatDatabase: HRU table not present, aborting")
        ElseIf aHruTable.Count = 0 Then
            Logger.Dbg("BuildSwatDatabase: HRU table is empty, aborting")
        Else
            Dim lProjectDataBaseName As String = IO.Path.Combine(aProjectFolder, aScenario & ".mdb")
            If IO.File.Exists(lProjectDataBaseName) Then
                IO.File.Delete(lProjectDataBaseName)
            End If

            Try
                Logger.Dbg("AboutToCreateSwatInputObject " & aSwatDatabaseName)
                Dim lSwatInput As New SwatInput(aSwatDatabaseName, lProjectDataBaseName, aProjectFolder, aScenario)
                With lSwatInput
                    Dim lTxtInOutFolder As String = IO.Path.Combine(aProjectFolder, "Scenarios\" & aScenario & "\TxtInOut")

                    Dim lComIdToBsnId As atcCollection = CreateSWATFig(aFlowlinesFilename, IO.Path.Combine(lTxtInOutFolder, "fig.fig"), aFields.FlowlinesComId, aFields.FlowlinesDownstreamComId)

                    Dim lFlowlineTable As New atcTableDelimited
                    lFlowlineTable.Delimiter = vbTab
                    lFlowlineTable.OpenFile(IO.Path.Combine(aProjectFolder, "Flowlines.txt"))

                    'Dim lHruTable As New atcTableDelimited
                    'lHruTable.Delimiter = vbTab
                    'lHruTable.OpenFile(IO.Path.Combine(aProjectFolder, "HRUsRev.txt"))
                    Dim lHruCount As Integer = 0

                    Dim lCatchmentTable As New atcTableDelimited
                    lCatchmentTable.Delimiter = vbTab
                    lCatchmentTable.OpenFile(IO.Path.Combine(aProjectFolder, "Catchments.txt"))

                    lSwatInput.CIO.Add(CioDefault(aSimulationStartYear, aSimulationEndYear, aReportFlowUnits))
                    'lSwatInput.CIO.Item.ILOG = aReportFlowUnits
                    lSwatInput.Wwq.Add(WwqDefault)
                    lSwatInput.Bsn.Add(BsnDefault)

                    Dim lSolItem As SwatInput.clsSolItem = Nothing
                    Dim lSoilClassOld As String = ""

                    Dim lWeatherGenAllDataTable As DataTable = lSwatInput.QueryGDB("SELECT * FROM weatherstations")
                    Logger.Dbg("WeatherStationCount " & lWeatherGenAllDataTable.Rows.Count)

                    Dim lLandUseMissing As New atcCollection

                    Dim lHruComIdIndex As Integer = aHruTable.Tags.IndexOf("catchment")
                    If lHruComIdIndex < 0 Then
                        Throw New ApplicationException("Could not find catchment HRU tag")
                    End If

                    Dim lHruLandUseIndex As Integer = aHruTable.Tags.IndexOf("NLCD2011.LandCover")
                    If lHruLandUseIndex < 0 Then
                        Throw New ApplicationException("Could not find NLCD2011.LandCover HRU tag")
                    End If

                    Dim lHruSoilIndex As Integer = -1
                    Dim lWhichSoilIndex As WhichSoilIndex = -1
                    For lTagIndex As Integer = 0 To aHruTable.Tags.Count - 1
                        Select Case aHruTable.Tags(lTagIndex)
                            Case "core31.statsgo"
                                lHruSoilIndex = lTagIndex
                                lWhichSoilIndex = WhichSoilIndex.SWATSTATSGO
                            Case "SSURGO"
                                lHruSoilIndex = lTagIndex
                                lWhichSoilIndex = WhichSoilIndex.SSURGO
                                'TODO: Figure out when to use WhichSoilIndex.USGS
                        End Select
                    Next

                    If lHruSoilIndex < 0 Then
                        Throw New ApplicationException("Could not find core31.statsgo HRU tag")
                    End If

                    Dim lHruSlopeCdIndex As Integer = aHruTable.Tags.IndexOf("SlopeReclass")
                    If lHruSlopeCdIndex < 0 Then
                        lHruSlopeCdIndex = aHruTable.Tags.IndexOf("Reclassified")
                        If lHruSlopeCdIndex < 0 Then
                            Throw New ApplicationException("Could not find Reclassified HRU tag")
                        End If
                    End If

                    For lBasinIndex As Integer = 0 To lComIdToBsnId.Count - 1
                        Dim lSubId As Integer = lComIdToBsnId.ItemByIndex(lBasinIndex)
                        Dim lComId As Integer = lComIdToBsnId.Keys(lBasinIndex)
                        Dim lHaveParams As Boolean = False
                        If aParamTable IsNot Nothing Then
                            Dim lSubBasinParamIndex As Integer = aSubBasinToParamIndex.IndexFromKey(CDbl(lComId))
                            If lSubBasinParamIndex >= 0 Then
                                lHaveParams = True
                                aParamTable.CurrentRecord = aSubBasinToParamIndex.ItemByIndex(lSubBasinParamIndex) + 1
                            End If
                        End If
                        Dim lHruTableStartSearchAt As Integer = 1
                        Dim lHruSubCount As Integer = 0
                        Dim lSubBasinAreaTotal As Double = 0
                        Logger.Dbg("Basin(" & lBasinIndex & ") SubId " & lSubId & " ComID " & lComId)
                        Dim lSubBasinHrus As New Generic.List(Of SwatInput.clsHruItem)

                        For Each lHru As HRU In aHruTable
                            If lHru.Ids(lHruComIdIndex) = lComId Then
                                Dim lSlopeId As Double = lHru.Ids(lHruSlopeCdIndex)
                                If Double.IsNaN(lSlopeId) Then
                                    Logger.Dbg("Skip " & lHru.Key & ":" & lHru.Area)
                                Else
                                    Dim lSoilId As String = lHru.Ids(lHruSoilIndex)
                                    Dim lSoilClass As String = SoilId2Str(lSwatInput, lSoilId, lWhichSoilIndex)
                                    If lSoilClass = "<unk>" Then
                                        'Throw New ApplicationException("CouldNotConvertSoilId " & lSoilId & " SubId " & lSubId & " HRU " & lHru.ToString)
                                        Logger.Dbg("CouldNotConvertSoilId " & lSoilId & " SubId " & lSubId & " HRU " & lHru.Key & ":" & lHru.Area)
                                    Else
                                        lHruSubCount += 1
                                        Dim lSlope_CdStr As String = SlopeId2Str((lSlopeId))
                                        Dim lLuIdStr As String = lHru.Ids(lHruLandUseIndex)
                                        Dim lLuId As Integer
                                        If Not (Integer.TryParse(lLuIdStr, lLuId)) Then
                                            lLuId = -1
                                        End If

                                        Dim lLandUseStr As String = LuId2Str(lLuId)
                                        'Check the new soil class with the old soil class
                                        If lSolItem Is Nothing OrElse lSoilClassOld <> lSoilClass Then
                                            Dim lUSSoil As DataTable
                                            Select Case lWhichSoilIndex
                                                Case WhichSoilIndex.SWATSTATSGO
                                                    Dim lState As String = lSoilClass.Substring(0, 1) & lSoilClass.Substring(1, 1).ToLower
                                                    lUSSoil = lSwatInput.QuerySoils("SELECT * FROM( " & lState & "MUIDs) WHERE MUID = '" & lSoilClass & "' ORDER BY CMPPCT DESC;")
                                                Case WhichSoilIndex.SSURGO
                                                    lUSSoil = SSURGOtable(aSoilProperties, lSoilClass, lSwatInput)
                                                Case WhichSoilIndex.USGS
                                                    'TODO: set soil from USGS soil index
                                            End Select
                                            If lUSSoil.Rows.Count = 0 Then
                                                Logger.Dbg("Could not find soil " & lSoilId & ", defaulting to MN009")
                                                lUSSoil = lSwatInput.QuerySoils("SELECT * FROM( " & "Mn" & "MUIDs) WHERE MUID = '" & "MN009" & "' ORDER BY CMPPCT DESC;")
                                            End If
                                            lSolItem = New SwatInput.clsSolItem(lUSSoil.Rows(0))
                                        End If

                                        With lSolItem
                                            .SUBBASIN = lSubId
                                            .HRU = lHruSubCount
                                            .LANDUSE = lLandUseStr
                                            .SOIL = lSoilClass
                                            .SLOPE_CD = lSlope_CdStr
                                        End With
                                        lSwatInput.Sol.Add(lSolItem)
                                        lSoilClassOld = lSoilClass

                                        Dim lHruItem As New SwatInput.clsHruItem(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                        With lHruItem
                                            .HRU_FR = lHru.Area
                                            .HRU_SLP = lHru.SlopeMean / 100
                                            .OV_N = 0.1
                                            'from OpenSWAT:createHRUGeneralTable
                                            'Select Case lHru.SlopeMean
                                            '    Case 0 To 0.1 : .SLSUBBSN = 121
                                            '    Case 0.1 To 2.0 : .SLSUBBSN = 90
                                            '    Case 2.0 To 3.0 : .SLSUBBSN = 75
                                            '    Case 3.0 To 4.0 : .SLSUBBSN = 60
                                            '    Case 4.0 To 5.0 : .SLSUBBSN = 45
                                            '    Case Else : .SLSUBBSN = 25
                                            'End Select
                                            'New slope values from Srini 12-1-2009: 
                                            'for slope length if the slope of the HRU is:
                                            '< 1% then use 120m
                                            ' 1-2% - 100m
                                            ' 2-3% - 90m 
                                            ' 3-5% - 60m
                                            ' 5-8% - 30m
                                            ' 8-15% - 15m
                                            ' > 15% - 9m
                                            Select Case lHru.SlopeMean
                                                Case Is < 1.0 : .SLSUBBSN = 120
                                                Case 1.0 To 2.0 : .SLSUBBSN = 100
                                                Case 2.0 To 3.0 : .SLSUBBSN = 90
                                                Case 3.0 To 5.0 : .SLSUBBSN = 60
                                                Case 5.0 To 8.0 : .SLSUBBSN = 30
                                                Case 8.0 To 15.0 : .SLSUBBSN = 15
                                                Case Else : .SLSUBBSN = 9
                                            End Select

                                            lSubBasinAreaTotal += lHru.Area
                                        End With
                                        lSubBasinHrus.Add(lHruItem)

                                        Dim lChmItem As New SwatInput.clsChmItem(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                        lSwatInput.Chm.Add(lChmItem)

                                        Dim lGwItem As SwatInput.clsGwItem = GwDefault(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                        If lHaveParams Then UpdateFromTable(lGwItem, aParamTable)
                                        lSwatInput.Gw.Add(lGwItem)

                                        Dim lMgtItem1 As New SwatInput.clsMgtItem1(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                        Dim lCrop As SwatInput.clsCropItem = lSwatInput.Crop.FindCrop(lLandUseStr)

                                        Dim lUrban As SwatInput.clsUrbanItem = lSwatInput.Urban.FindUrban(lLandUseStr)

                                        Dim lMgtItem2 As New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                        If aUseMgtCropFile Then
                                            CropInfoFromXml(aProjectFolder, lSwatInput, lMgtItem1, lCrop, lUrban, lMgtItem2, lSolItem)
                                        Else
                                            With lMgtItem1
                                                .BIOMIX = 0.2
                                                If Not lCrop Is Nothing Then
                                                    Select Case lSolItem.HYDGRP
                                                        Case "A" : .CN2 = lCrop.CN2A
                                                        Case "B" : .CN2 = lCrop.CN2B
                                                        Case "C" : .CN2 = lCrop.CN2C
                                                        Case "D" : .CN2 = lCrop.CN2D
                                                    End Select
                                                    'planting
                                                    lMgtItem2 = New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                                    With lMgtItem2
                                                        .CROP = lCrop.CPNM
                                                        .HUSC = 0.15
                                                        .MGT_OP = 1
                                                        .PLANT_ID = lCrop.ICNUM
                                                        'use the .SUBBASIN --> weather stn and get the x, y
                                                        'need to have a subbasin centroid x, y look up table
                                                        'lCatchmentTable.FindFirst(3, lLat)
                                                        'lCatchmentTable.FindFirst(4, lLong)
                                                        .HEATUNITS = HeatUnits(lCrop.CPNM)
                                                        '.HEATUNITS = HeatUnits(lCrop.CPNM, lLat, lLong, aProjectFolder)
                                                    End With
                                                    lSwatInput.Mgt.Add2(lMgtItem2)
                                                    If lCrop.CPNM = "AGRR" Then
                                                        'autofert
                                                        lMgtItem2 = New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                                        With lMgtItem2
                                                            .CROP = lCrop.CPNM
                                                            .MGT_OP = 11
                                                            .HUSC = 0.16
                                                        End With
                                                        lSwatInput.Mgt.Add2(lMgtItem2)
                                                    End If
                                                    'harvest
                                                    lMgtItem2 = New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                                    With lMgtItem2
                                                        .CROP = lCrop.CPNM
                                                        .MGT_OP = 5
                                                        .HUSC = 1.2
                                                    End With
                                                    lSwatInput.Mgt.Add2(lMgtItem2)
                                                    .NROT = 1
                                                ElseIf Not lUrban Is Nothing Then
                                                    Select Case lSolItem.HYDGRP
                                                        Case "A" : .CN2 = lUrban.CN2A
                                                        Case "B" : .CN2 = lUrban.CN2B
                                                        Case "C" : .CN2 = lUrban.CN2C
                                                        Case "D" : .CN2 = lUrban.CN2D
                                                    End Select
                                                    .IURBAN = 1
                                                    .URBLU = 4
                                                    'planting
                                                    lCrop = lSwatInput.Crop.FindCrop("BERM") 'TODO-this is grass-other types?
                                                    lMgtItem2 = New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                                    With lMgtItem2
                                                        .CROP = lCrop.CPNM
                                                        .HUSC = 0.15
                                                        .MGT_OP = 1
                                                        .PLANT_ID = lCrop.ICNUM
                                                        .HEATUNITS = HeatUnits(lCrop.CPNM)
                                                    End With
                                                    lSwatInput.Mgt.Add2(lMgtItem2)
                                                    'harvest
                                                    lMgtItem2 = New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                                                    With lMgtItem2
                                                        .CROP = lCrop.CPNM
                                                        .MGT_OP = 5
                                                        .HUSC = 1.2
                                                    End With
                                                    lSwatInput.Mgt.Add2(lMgtItem2)
                                                    .NROT = 1
                                                Else
                                                    .CN2 = 66
                                                    If lLandUseMissing.IndexFromKey(lLandUseStr) = -1 Then
                                                        Logger.Dbg("MissingLU:" & lLandUseStr)
                                                        lLandUseMissing.Add(lLandUseStr)
                                                        .NROT = 0
                                                    End If
                                                End If
                                                'TODO: more from crop??
                                                .USLE_P = 1
                                                .HUSC = 0
                                            End With
                                            lSwatInput.Mgt.Add1(lMgtItem1)
                                        End If
                                    End If
                                End If
                            End If
                        Next
                        For Each lHruItem As SwatInput.clsHruItem In lSubBasinHrus
                            lHruItem.HRU_FR /= lSubBasinAreaTotal
                            lSwatInput.Hru.Add(lHruItem)
                        Next

                        lHruCount += lHruSubCount

                        Dim lSubBasin As New SwatInput.clsSubBsnItem(lSubId)
                        With lSubBasin
                            .COMID = lComId
                            .SUB_KM = lSubBasinAreaTotal / 1000000.0 'm2 -> km2

                            lFlowlineTable.FindFirst(2, lComId)
                            'TODO: this seems to be how OpenSWAT does this!!!!!
                            .CH_L1 = lFlowlineTable.Value(6) 'TODO: need trib values here!!! - longest flowline length [km]
                            .CH_S1 = lFlowlineTable.Value(5) 'TODO: need trib values here!!! - average slope of trib [m/m]
                            '.CH_W1 = lFlowlineTable.Value(3) 'TODO: need trib values here!!! 
                            .CH_W1 = .SUB_KM ^ 0.6 * 1.29 '- average width of trib 1.29*(SA**0.6) [meters]
                            .HRUTOT = lHruSubCount
                            .FCST_REG = 1
                            .CH_K1 = 0.5
                            .CH_N1 = 0.014

                            lCatchmentTable.FindFirst(2, lComId)
                            .SUB_LAT = lCatchmentTable.Value(3)
                            .SUB_ELEV = lCatchmentTable.Value(5)
                            Dim lSubLng As Double = lCatchmentTable.Value(4)
                            Dim lMinDistance As Double = 1.0E+30
                            Dim lRowClose As DataRow = Nothing
                            Dim lFieldLat As Integer = lWeatherGenAllDataTable.Columns.IndexOf("WLATITUDE")
                            Dim lFieldLng As Integer = lWeatherGenAllDataTable.Columns.IndexOf("WLONGITUDE")
                            Dim lFieldStn As Integer = lWeatherGenAllDataTable.Columns.IndexOf("STATION")
                            For Each lWeatherRow As DataRow In lWeatherGenAllDataTable.Rows
                                Dim lDistance = Math.Sqrt((lWeatherRow(lFieldLat) - .SUB_LAT) ^ 2 + _
                                                          (lWeatherRow(lFieldLng) - lSubLng) ^ 2)
                                If lDistance < lMinDistance Then
                                    lRowClose = lWeatherRow
                                    lMinDistance = lDistance
                                End If
                            Next

                            If lRowClose IsNot Nothing Then
                                lSwatInput.Wgn.Add(New SwatInput.clsWgnItem(.SUBBASIN, lRowClose))
                            Else
                                Logger.Dbg("Could not find weather station for " & .SUBBASIN)
                            End If
                        End With
                        lSwatInput.SubBsn.Add(lSubBasin)

                        lSwatInput.Swq.Add(SwqDefault(lSubId))
                        Dim lWusItem As New SwatInput.clsWusItem(lSubId)
                        lSwatInput.Wus.Add(lWusItem)
                        lSwatInput.Pnd.Add(PndDefault(lSubId))
                        Dim lRteItem As New SwatInput.clsRteItem(lSubId)
                        With lRteItem
                            .CH_L2 = lFlowlineTable.Value(6)
                            .CH_S2 = lFlowlineTable.Value(5)
                            .CH_D = lFlowlineTable.Value(4)
                            .CH_W2 = lFlowlineTable.Value(3)
                            .CH_N2 = 0.014
                            If .CH_D > 0 Then
                                .CH_WDR = .CH_W2 / .CH_D
                            Else
                                .CH_WDR = 0
                            End If
                        End With
                        lSwatInput.Rte.Add(lRteItem)
                    Next
                    Logger.Dbg("SWATDatabaseBuilt S " & lComIdToBsnId.Count & " H " & lHruCount)

                    If lLandUseMissing.Count > 0 Then
                        Dim lMessage As String = "Error: Missing LandUse "
                        For Each lLandUseStr As String In lLandUseMissing
                            lMessage &= lLandUseStr & ", "
                        Next
                        Logger.Msg(lMessage & vbCrLf & "Update 'crop.dat' or reclassification in LuId2Str")
                    End If
                    Dim lCioItem As SwatInput.clsCIOItem = lSwatInput.CIO.Item
                    Dim lDate(5) As Integer
                    lDate(0) = lCioItem.IYR
                    lDate(1) = 1
                    lDate(2) = 1
                    Dim lDateStart As Double = Date2J(lDate)
                    lDate(0) += lCioItem.NBYR
                    Dim lDateEnd As Double = Date2J(lDate)
                    Logger.Dbg("Writing " & timdifJ(lDateStart, lDateEnd, 6, 1) & " years of data " & MemUsage())

                    WriteSwatMetInput(aCacheFolder, aProjectFolder, lTxtInOutFolder, lDateStart, lDateEnd, aCreateArcSWATFiles)

                    'TODO: allow multiple gages
                    For Each lSubBsn As SwatInput.clsSubBsnItem In lSwatInput.SubBsn.Items
                        With lSubBsn
                            .IRGAGE = 1
                            .ITGAGE = 1
                            .IWGAGE = 1
                            .ISGAGE = 1
                        End With
                        lSwatInput.SubBsn.Update(lSubBsn)
                    Next
                    Logger.Dbg("MetDataWritten " & MemUsage())

                    .SaveAllTextInput()
                    Logger.Dbg("SWATInputSaved")
                End With

                lSwatInput.Close()
            Catch lEx As Exception
                Logger.Dbg(lEx.ToString)
                Dim lTex As Exception = lEx
                Dim lCount As Integer = 1
                While lTex.InnerException IsNot Nothing AndAlso lCount < 3
                    lCount += 1
                    Logger.Dbg("Details " & lTex.InnerException.Message)
                    lTex = lTex.InnerException
                End While
            End Try
        End If
    End Sub

    Private Function SSURGOtable(ByVal aSoilProperties As List(Of D4EM.Data.Source.NRCS_Soil.SoilLocation.Soil), ByVal lSoilClass As String, ByVal lSwatInput As SwatInput) As DataTable
        Dim lUSSoil As DataTable
        Dim lSoil As D4EM.Data.Source.NRCS_Soil.SoilLocation.Soil = Nothing
        Dim lComp As D4EM.Data.Source.NRCS_Soil.SoilLocation.SoilComponent = Nothing
        Dim lState As String = ""
        For Each lSoil In aSoilProperties
            If lSoil.MuKey.ToUpper = lSoilClass.ToUpper Then
                lState = lSoil.AreaSymbol.Substring(0, 1) & lSoil.AreaSymbol.Substring(1, 1).ToLower
                For Each lComp In lSoil.Components
                    If lComp.IsDominant Then Exit For 'Assume on is already set to be predominant
                Next
                Exit For
            End If
        Next

        Dim lMatchingSoil As Boolean = True
        lUSSoil = lSwatInput.QuerySoils("SELECT TOP 1 * FROM( " & lState & "MUIDs) WHERE MUID = '" & lSoil.AreaSymbol & "';")
        If lUSSoil.Rows.Count = 0 Then
            lUSSoil = lSwatInput.QuerySoils("SELECT TOP 1 * FROM( " & lState & "MUIDs);")
            lMatchingSoil = False
        End If
        If lSoil.Components.Count = 0 OrElse lComp.chorizons.Count = 0 Then
            If lMatchingSoil Then
                Logger.Dbg("SSURGO missing soil information for " & lSoil.AreaSymbol & " MuKey " & lSoil.MuKey & " defaulting to STATSGO soil " & lUSSoil.Rows(0).Item("MUID"))
            Else
                Logger.Dbg("SSURGO and STATSGO are both missing soil information for " & lSoil.AreaSymbol & " MuKey " & lSoil.MuKey & " defaulting to STATSGO soil " & lUSSoil.Rows(0).Item("MUID"))
            End If
            Return lUSSoil
        Else
            'fill in the ssurgo attributes
            With lUSSoil.Rows(0)
                If Not lMatchingSoil Then
                    .Item("MUID") = lSoil.AreaSymbol
                End If
                .Item("SNAM") = lSoil.AreaSymbol & lSoil.MuSym & "-" & lComp.cokey
                .Item("S5ID") = ""
                .Item("CMPPCT") = lComp.comppct_r.ToString
                .Item("NLAYERS") = lComp.chorizons.Count.ToString
                .Item("HYDGRP") = SafeSubstring(lComp.HSG, 0, 1)
                .Item("SOL_ZMX") = lComp.chorizons(lComp.chorizons.Count - 1).hzdepb_r
                .Item("TEXTURE") = lComp.chorizons(0).texture
                For I As Integer = 1 To 10
                    If I - 1 < lComp.chorizons.Count Then
                        .Item("SOL_Z" & I) = lComp.chorizons(I - 1).hzdepb_r
                        .Item("SOL_BD" & I) = lComp.chorizons(I - 1).dbovendry_r
                        .Item("SOL_AWC" & I) = lComp.chorizons(I - 1).awc_r
                        .Item("SOL_K" & I) = lComp.chorizons(I - 1).ksat_r
                        .Item("SOL_CBN" & I) = lComp.chorizons(I - 1).om_r
                        .Item("CLAY" & I) = lComp.chorizons(I - 1).claytotal_r
                        .Item("SILT" & I) = lComp.chorizons(I - 1).silttotal_r
                        .Item("SAND" & I) = lComp.chorizons(I - 1).sandtotal_r
                        .Item("ROCK" & I) = lComp.chorizons(I - 1).rockpct
                        .Item("SOL_ALB" & I) = lComp.albedodry_r
                        .Item("USLE_K" & I) = lComp.chorizons(I - 1).kffact
                        .Item("SOL_EC" & I) = lComp.chorizons(I - 1).ec_r
                    Else
                        .Item("SOL_Z" & I) = 0
                        .Item("SOL_BD" & I) = 0
                        .Item("SOL_AWC" & I) = 0
                        .Item("SOL_K" & I) = 0
                        .Item("SOL_CBN" & I) = 0
                        .Item("CLAY" & I) = 0
                        .Item("SILT" & I) = 0
                        .Item("SAND" & I) = 0
                        .Item("ROCK" & I) = 0
                        .Item("SOL_ALB" & I) = 0
                        .Item("USLE_K" & I) = 0
                        .Item("SOL_EC" & I) = 0
                    End If
                Next
                'TODO? .AcceptChanges()
            End With 'lUSSoil.Rows(0)
            Return lUSSoil
        End If
    End Function

    ''' <summary>Generate a .fig file from NHDPlus flowlines for use in SWAT.</summary>
    ''' <param name="aFlowlinesFileName"></param>
    ''' <param name="aFigFilename"></param>
    ''' <returns>collection of ComId(key) to SubBasinId(value) mapping</returns>
    ''' <remarks></remarks>
    Friend Function CreateSWATFig(ByVal aFlowlinesFileName As String, ByVal aFigFilename As String, _
                                  ByVal aFlowlinesComIdFieldIndex As Integer, _
                                  ByVal aFlowlinesDownstreamComIdFieldIndex As Integer) As atcUtility.atcCollection
        Dim lSwatSubbasinFieldname As String = "SWATSUB"
        Logger.Status("Create SWAT .fig", True)
        CreateSWATFig = New atcUtility.atcCollection

        Dim lFlowlinesShapefile = DotSpatial.Data.Shapefile.Open(aFlowlinesFileName)
        If lFlowlinesShapefile Is Nothing Then
            Logger.Dbg("Unable to open '" & aFlowlinesFileName & "'")
            Exit Function
        End If

        If aFlowlinesComIdFieldIndex = -1 Then
            Logger.Dbg("COMID not found in '" & aFlowlinesFileName & "'")
            Logger.Status("")
            Exit Function
        ElseIf aFlowlinesDownstreamComIdFieldIndex = -1 Then
            Logger.Dbg("TOCOMID not found in '" & aFlowlinesFileName & "'")
            Logger.Status("")
            Exit Function
        End If

        Dim lFlowlinesSubBasinFieldIndex As Integer = lFlowlinesShapefile.DataTable.Columns.IndexOf(lSwatSubbasinFieldname)
        If lFlowlinesSubBasinFieldIndex = -1 Then 'field does not exist, add it
            lFlowlinesSubBasinFieldIndex = lFlowlinesShapefile.GetColumns.Count
            lFlowlinesShapefile.DataTable.Columns.Add(lSwatSubbasinFieldname, GetType(Integer))
            Logger.Dbg("Added field " & lSwatSubbasinFieldname & " to " & lFlowlinesShapefile.Filename)
        End If

        Dim fig As New IO.StreamWriter(aFigFilename)

        Dim substack As New Generic.Stack(Of Integer)
        Dim subIDs As New Generic.List(Of Integer)
        Dim Hyd_Stor_Num As Integer = 0
        Dim Res_Num As Integer = 0
        Dim InFlow_Num1 As Integer = 0
        Dim InFlow_Num2 As Integer = 0
        Dim InFlow_ID As Integer = 0
        Dim UpstreamCount, UpstreamFinishedCount As Integer

        'Write subbasins
        Dim lFlowlineIndex As Integer
        Dim lLastFlowline As Integer = lFlowlinesShapefile.NumRows - 1
        For lFlowlineIndex = 0 To lLastFlowline
            Dim lComId As Long = lFlowlinesShapefile.Features(lFlowlineIndex).DataRow(aFlowlinesComIdFieldIndex)
            If lComId = 0 Then
                'TODO: adding a subID=0 here is not enough to make the code below work, 
                'we currently depend on cosmetic flowlines being at end of shapefile so there are no skipped indexes
                subIDs.Add(0)
                Logger.Dbg("CreateSWATFig: Skipping cosmetic flowline at index " & lFlowlineIndex)
            Else
                Hyd_Stor_Num += 1
                fig.Write("subbasin       1{0,6:G6}{0,6:G6}                              Subbasin: {0:G} ComId: {1:G}" & ControlChars.Lf & _
                          "          {0,5:D5}0000.sub" & ControlChars.Lf, _
                          Hyd_Stor_Num, lComId)
                InFlow_ID = lFlowlinesShapefile.Features(lFlowlineIndex).DataRow(aFlowlinesDownstreamComIdFieldIndex)
                lFlowlinesShapefile.Features(lFlowlineIndex).DataRow(lFlowlinesSubBasinFieldIndex) = Hyd_Stor_Num
                If FindRecord(lFlowlinesShapefile, aFlowlinesComIdFieldIndex, InFlow_ID) < 0 Then
                    substack.Push(lFlowlineIndex)
                End If
                subIDs.Add(-1)
                CreateSWATFig.Add(lComId.ToString, Hyd_Stor_Num)
            End If
        Next

        Logger.Dbg("CreateSWATFig: Wrote Subbasins " & Hyd_Stor_Num)

        'Write the rest
        Dim curridx As Integer
        'Dim currUS1, currUS2 As String
        'Dim currUS1idx, currUS2idx, currUS1ID, currUS2ID As Integer
        While substack.Count > 0
            curridx = substack.Pop()
            Dim currComID As Long = lFlowlinesShapefile.Features(curridx).DataRow(aFlowlinesComIdFieldIndex)
            Dim lUpstreamIndexes As Generic.List(Of Integer) = D4EM.Geo.NetworkOperations.FindRecords(lFlowlinesShapefile, aFlowlinesDownstreamComIdFieldIndex, currComID)

            UpstreamCount = lUpstreamIndexes.Count
            If UpstreamCount = 0 Then 'then we're on an outer reach.
                If subIDs(curridx) = -1 Then 'then it hasn't been added yet. add a route
                    Hyd_Stor_Num += 1
                    InFlow_Num1 = curridx + 1
                    fig.Write("route          2{0,6:G6}{1,6:G6}{2,6:G6}" + ControlChars.Lf + "          {1,5:D5}0000.rte{1,5:D5}0000.swq" + ControlChars.Lf, Hyd_Stor_Num, curridx + 1, InFlow_Num1)
                    subIDs(curridx) = Hyd_Stor_Num

                    'TODO: handle reservoirs
                    'If lFlowlinesShapefile.CellValue(ReservoirFieldNum, curridx).ToString() = "1" Then 'it's a reservoir
                    '    Hyd_Stor_Num += 1
                    '    Res_Num += 1
                    '    InFlow_Num1 = Hyd_Stor_Num - 1
                    '    InFlow_ID = curridx + 1
                    '    fig.Write("routres        3{0,6:G6}{1,6:G6}{2,6:G6}{3,6:G6}" + ControlChars.Lf + "          {3,5:D5}0000.res{3,5:D5}0000.lwq" + ControlChars.Lf, Hyd_Stor_Num, Res_Num, InFlow_Num1, InFlow_ID)
                    '    subIDs(curridx) = Hyd_Stor_Num
                    'End If
                End If

            Else 'we're on a middle or final reach
                'UpstreamCount = 0
                UpstreamFinishedCount = 0

                For Each lUpstreamIndex As Integer In lUpstreamIndexes
                    Dim lUpstreamID As Integer = subIDs(lUpstreamIndex)
                    If lUpstreamID <> -1 Then
                        UpstreamFinishedCount += 1
                    End If
                Next

                If UpstreamCount = UpstreamFinishedCount Then 'all upstreams finished
                    InFlow_Num2 = curridx + 1
                    For Each lUpstreamIndex As Integer In lUpstreamIndexes
                        Dim lUpstreamID As Integer = subIDs(lUpstreamIndex)
                        Hyd_Stor_Num += 1
                        InFlow_Num1 = lUpstreamID
                        fig.Write("add            5{0,6:G6}{1,6:G6}{2,6:G6}" + ControlChars.Lf, Hyd_Stor_Num, lUpstreamID, InFlow_Num2)
                        InFlow_Num2 = Hyd_Stor_Num
                    Next

                    'After summing, create the route and possibly reservoir
                    Hyd_Stor_Num += 1
                    InFlow_Num1 = Hyd_Stor_Num - 1
                    fig.Write("route          2{0,6:G6}{1,6:G6}{2,6:G6}" + ControlChars.Lf + "          {1,5:D5}0000.rte{1,5:D5}0000.swq" + ControlChars.Lf, Hyd_Stor_Num, curridx + 1, InFlow_Num1)
                    subIDs(curridx) = Hyd_Stor_Num

                    'TODO: handle reservoirs
                    'If sf.CellValue(ReservoirFieldNum, curridx).ToString() = "1" Then
                    '    Hyd_Stor_Num += 1
                    '    Res_Num += 1
                    '    InFlow_Num1 = Hyd_Stor_Num - 1
                    '    InFlow_ID = curridx + 1
                    '    fig.Write("routres        3{0,6:G6}{1,6:G6}{2,6:G6}{3,6:G6}" + ControlChars.Lf + "          {3,5:D5}0000.res{3,5:D5}0000.lwq" + ControlChars.Lf, Hyd_Stor_Num, Res_Num, InFlow_Num1, InFlow_ID)
                    '    subIDs(curridx) = Hyd_Stor_Num
                    'End If

                Else 'There are upstream items that need to still be processed before this one
                    substack.Push(curridx)
                    For Each lUpstreamIndex As Integer In lUpstreamIndexes
                        substack.Push(lUpstreamIndex)
                    Next
                End If
            End If
        End While

        'Write out the saveconc and finish commands
        Dim SaveFile_Num As Integer = 1
        Dim Print_Freq As Integer = 0 '0 for daily, 1 for hourly
        fig.Write("saveconc      14{0,6:G6}{1,6:G6}{2,6:G6}" + ControlChars.Lf + "          watout.dat" + ControlChars.Lf, Hyd_Stor_Num, SaveFile_Num, Print_Freq)
        fig.Write("finish         0" + ControlChars.Lf)

        fig.Close()
        lFlowlinesShapefile.Save()
        lFlowlinesShapefile.Close()
        Logger.Dbg("CreateSWATFig: Finished " & Hyd_Stor_Num)
        Logger.Status("")
    End Function

    'Find first matching record by field value as Long
    Private Function FindRecord(ByVal aShapeFile As DotSpatial.Data.Shapefile, _
                                ByVal aFieldIndex As Integer, _
                                ByVal aValue As Long) As Integer
        Dim lFieldValue As Long = 0 'Initial value does not matter, gets set in UpdateValueIfNotNull before being checked
        Dim lLastRow As Integer = aShapeFile.NumRows() - 1
        For lRecordIndex As Integer = 0 To lLastRow
            Try
                If UpdateValueIfNotNull(lFieldValue, aShapeFile.Features(lRecordIndex).DataRow(aFieldIndex)) AndAlso lFieldValue = aValue Then 'this is the record we want
                    Return lRecordIndex
                End If
            Catch 'Ignore non-numeric values
            End Try
        Next
        Return -1
    End Function

    Private Function UpdateValueIfNotNull(ByRef aCurrentObject As Object, ByRef aNewObject As Object) As Boolean
        If aNewObject IsNot System.DBNull.Value Then
            aCurrentObject = aNewObject
            Return True
        End If
        Return False
    End Function

    Private Sub UpdateFromTable(ByVal aUpdateMe As Object, ByVal aTable As atcUtility.atcTable)
        For lFieldIndex As Integer = 1 To aTable.NumFields
            If atcUtility.SetSomething(aUpdateMe, aTable.FieldName(lFieldIndex), aTable.Value(lFieldIndex), False).Length = 0 Then
                MapWinUtility.Logger.Dbg("UpdateFromTable: " & aTable.FieldName(lFieldIndex) & " = " & aTable.Value(lFieldIndex))
            End If
        Next
    End Sub

    Private Function HeatUnits(ByVal aCropName As String) As Double
        'TODO: use following function from OpenSWAT when all arguments and datafiles are understood
        Dim lHeatUnits As Double = 0

        'Return calHeatUnits(aCropName, aX, aY, aDataDirectory)

        Select Case aCropName
            Case "AGRR" : lHeatUnits = 1841.061
            Case "FRSD" : lHeatUnits = 2234.698
            Case "FRSE" : lHeatUnits = 4572.529
            Case "HAY", "URLD" : lHeatUnits = 1454.7
            Case "WETF" : lHeatUnits = 2151.05
            Case Else
                lHeatUnits = 2000.0
        End Select
        Return lHeatUnits
    End Function

    Private Function HeatUnits(ByVal aCropName As String, ByVal aLat As Single, ByVal along As Single, ByVal aDataDirectory As String) As Double
        'TODO: use following function from OpenSWAT when all arguments and datafiles are understood
        Return Nothing 'calHeatUnits(aCropName, along, aLat, aDataDirectory)
    End Function

    Private Function SlopeId2Str(ByVal aSlopeId As Integer) As String
        Dim lSlopeId2Str As String = ""
        Select Case aSlopeId
            Case 0 : lSlopeId2Str = "<un>"
            Case 1 : lSlopeId2Str = "0-0.5"
            Case 2 : lSlopeId2Str = "0.5-2"
            Case 3 : lSlopeId2Str = "2-9999"
            Case Else : lSlopeId2Str = "<un>"
        End Select
        Return lSlopeId2Str
    End Function

    Friend Function SoilId2Str(ByVal aSwatInput As SwatInput, ByVal aSoilId As String, _
                               ByVal aWhichSoilIndex As WhichSoilIndex) As String
        aSoilId = aSoilId.Trim
        If Not IsNumeric(aSoilId) Then
            Return aSoilId 'Must already be the string version with state abbreviation
        End If
        Dim lNumericSTMUID As String
        Dim lStateString As String = ""

        Select Case aWhichSoilIndex
            Case WhichSoilIndex.USGS
                Select Case aSoilId.Substring(0, 2)
                    Case "27" : lStateString = "MI"
                    Case "28" : lStateString = "MN"
                    Case "58" : lStateString = "WI"
                End Select
                If lStateString.Length = 2 Then
                    Return lStateString & aSoilId.Substring(2)
                End If
            Case WhichSoilIndex.SWATSTATSGO
                Dim lFindMUID As DataTable = aSwatInput.QuerySoils("Select STMUID From Stats_grd_lu Where Value_=" & aSoilId & ";")
                If lFindMUID.Rows.Count > 0 Then
                    lNumericSTMUID = lFindMUID.Rows(0).Item(0)
                Else
                    lNumericSTMUID = aSoilId
                End If
                Dim lFindStateTxt As DataTable = aSwatInput.QuerySoils("Select StateTxt From tblStmuidLu Where StateNum='" & SafeSubstring(lNumericSTMUID, 0, 2) & "';")
                If lFindStateTxt.Rows.Count > 0 Then
                    Return lFindStateTxt.Rows(0).Item(0) & lNumericSTMUID.Substring(2)
                End If
            Case WhichSoilIndex.SSURGO
                Return aSoilId
        End Select

        Return "<unk>"
    End Function

    Friend Function LuId2Str(ByVal aLuId As Integer) As String
        Dim lLuId2Str As String = ""
        Select Case aLuId
            Case 11 : lLuId2Str = "WATR"
            Case 21 : lLuId2Str = "URLD"
            Case 22 : lLuId2Str = "URML"
            Case 23 : lLuId2Str = "URMD"
            Case 24 : lLuId2Str = "URHD"
            Case 31 : lLuId2Str = "RNGB" 'barren land -> range / brush
            Case 41 : lLuId2Str = "FRSD"
            Case 42 : lLuId2Str = "FRSE"
            Case 43 : lLuId2Str = "FRST"
            Case 52 : lLuId2Str = "RNGB" 'scrub -> range / brush
            Case 71 : lLuId2Str = "PAST"
            Case 81 : lLuId2Str = "HAY"
            Case 82 : lLuId2Str = "AGRR" 'generic row crop
            Case 90 : lLuId2Str = "WETL"
            Case 95 : lLuId2Str = "WETF"
                'ADDED for APES
                'KLW July 14, 2009
            Case 100 : lLuId2Str = "CORN" 'Corn, all
            Case 200 : lLuId2Str = "COTS" 'Cotton() - 2 types
            Case 400 : lLuId2Str = "SGHY" 'Sorghum() - sorghum hay
            Case 500 : lLuId2Str = "SOYB" 'Soybeans()
            Case 1000 : lLuId2Str = "PNUT" 'Peanuts()
            Case 1100 : lLuId2Str = "TOBC" 'Tobacco()
            Case 2100 : lLuId2Str = "BARL" 'Barley() - spring barley
            Case 2400 : lLuId2Str = "WWHT" 'Winter(Wheat)
            Case 2500 : lLuId2Str = "HAY" 'Other(Grains / Hay)
            Case 2600 : lLuId2Str = "CORN" 'Wheat/Soybeans Double Cropped
            Case 2700 : lLuId2Str = "RYE" 'Rye()
            Case 2800 : lLuId2Str = "OATS" 'Oats()
            Case 3600 : lLuId2Str = "ALFA" 'Alfalfa()
            Case 4300 : lLuId2Str = "POTA" 'Potatoes()
            Case 4400 : lLuId2Str = "AGRR" 'Other(Crops)
            Case 4600 : lLuId2Str = "SPOT" 'Sweet(Potatoes)
            Case 5000 : lLuId2Str = "AGRR" 'Cucumbers (NC); Other Crops (VA)
            Case 5100 : lLuId2Str = "AGGR" 'Processed(Vegetables(VA))
            Case 5800 : lLuId2Str = "POTA" 'Potatoes(VA)
            Case 7100 : lLuId2Str = "ORCD" 'State722, Cottonwood Tree, Orchard, Other Fruits & Nuts - orchard

            Case Else
                Dim lMissingLandUse As String = "RNGB"
                Logger.Dbg("MissingLandUseCode " & aLuId & " defaulting to " & lMissingLandUse)
                lLuId2Str = lMissingLandUse
        End Select
        Return lLuId2Str
    End Function

    Private Function CropInfoFromXml(ByVal aProjectFolder As String, _
                                     ByVal lSwatInput As SwatInput, _
                                     ByVal lMgtItem1 As SwatInput.clsMgtItem1, _
                                     ByVal lCrop As SwatInput.clsCropItem, _
                                     ByVal lUrban As SwatInput.clsUrbanItem, _
                                     ByVal lMgtItem2In As SwatInput.clsMgtItem2, _
                                     ByVal lSolItem As SwatInput.clsSolItem) As String

        Dim cropNode As Xml.XmlNode
        cropNode = Nothing
        Dim msgException As String = ""
        Dim heatUnit As Single
        Dim lMgtItem2 As SwatInput.clsMgtItem2

        Try

            If Not (lCrop Is Nothing) Then

                If (SWATParamsXmlDoc Is Nothing) Then
                    SWATParamsXmlDoc = New Xml.XmlDocument
                    Dim paramsFile = IO.Path.Combine(aProjectFolder, "CropLandcover\SWATParams.xml")
                    SWATParamsXmlDoc.Load(paramsFile)
                End If

                cropNode = SWATParamsXmlDoc.SelectSingleNode("/SWATData/Parameterset[@name='APES']/Crop[@name='" + lCrop.CPNM + "']")
                If (cropNode Is Nothing) Then
                    cropNode = SWATParamsXmlDoc.SelectSingleNode("/SWATData/Parameterset[@name='APES']/Crop[@name='DEFAULT']")
                End If

                Dim mgt1Node As Xml.XmlNode
                Dim nodeVal As Xml.XmlNode
                mgt1Node = SWATParamsXmlDoc.SelectSingleNode("/SWATData/Parameterset[@name='APES']/MGT1")

                With lMgtItem1
                    'Try

                    nodeVal = mgt1Node.SelectSingleNode("BIOMIX")
                    If Not (nodeVal Is Nothing) Then
                        .BIOMIX = Convert.ToSingle(nodeVal.InnerText)
                    End If

                    nodeVal = mgt1Node.SelectSingleNode("USLE_P")
                    If Not (nodeVal Is Nothing) Then
                        .USLE_P = Convert.ToSingle(nodeVal.InnerText)
                    End If

                    nodeVal = mgt1Node.SelectSingleNode("HUSC")
                    If Not (nodeVal Is Nothing) Then
                        .HUSC = Convert.ToSingle(nodeVal.InnerText)
                    End If

                    nodeVal = mgt1Node.SelectSingleNode("NROT")
                    If Not (nodeVal Is Nothing) Then
                        .NROT = Convert.ToSingle(nodeVal.InnerText)
                    End If

                    'Catch ex As Exception
                    '    Dim msg As String
                    '    msg = ex.Message
                    'End Try

                    'Try

                    Select Case lSolItem.HYDGRP
                        Case "A" : .CN2 = lCrop.CN2A
                        Case "B" : .CN2 = lCrop.CN2B
                        Case "C" : .CN2 = lCrop.CN2C
                        Case "D" : .CN2 = lCrop.CN2D
                    End Select

                    nodeVal = cropNode.SelectSingleNode("HeatUnit")
                    If Not (nodeVal Is Nothing) Then
                        heatUnit = Convert.ToSingle(nodeVal.InnerText)
                    End If

                    'Catch ex As Exception
                    '    Dim msg As String
                    '    msg = ex.Message
                    'End Try

                    Dim mgt2Nodes As Xml.XmlNodeList
                    Dim mgt2Node As Xml.XmlNode
                    Dim attr As Xml.XmlAttribute
                    Dim mgtType As String
                    mgt2Nodes = cropNode.SelectNodes("MGT2")
                    For Each mgt2Node In mgt2Nodes

                        'Try
                        lMgtItem2 = lMgtItem2In.Clone()
                        attr = mgt2Node.Attributes("type")

                        If Not (attr Is Nothing) Then
                            mgtType = attr.InnerText
                            If (String.Compare(mgtType, "planting", True) = 0) Then
                                lMgtItem2.HEATUNITS = heatUnit
                                lMgtItem2.PLANT_ID = lCrop.ICNUM
                            End If
                        End If

                        If Not (mgt2Node.SelectSingleNode("HUSC") Is Nothing) Then
                            lMgtItem2.HUSC = Convert.ToSingle(mgt2Node.SelectSingleNode("HUSC").InnerText)
                        End If
                        If Not (mgt2Node.SelectSingleNode("MGT_OP") Is Nothing) Then
                            lMgtItem2.MGT_OP = Convert.ToInt32(mgt2Node.SelectSingleNode("MGT_OP").InnerText)
                        End If
                        lMgtItem2.CROP = lCrop.CPNM

                        lSwatInput.Mgt.Add2(lMgtItem2)
                        'Catch ex As Exception
                        '    Dim msg As String
                        '    msg = ex.Message

                        'End Try
                    Next
                End With

            ElseIf Not (lUrban Is Nothing) Then
                'Try
                With lMgtItem1
                    Select Case lSolItem.HYDGRP
                        Case "A" : .CN2 = lUrban.CN2A
                        Case "B" : .CN2 = lUrban.CN2B
                        Case "C" : .CN2 = lUrban.CN2C
                        Case "D" : .CN2 = lUrban.CN2D
                    End Select
                    .IURBAN = 1
                    .URBLU = 4
                    'planting
                    lCrop = lSwatInput.Crop.FindCrop("BERM") 'TODO-this is grass-other types?
                    'Dim lMgtItem2 As New SwatInput.clsMgtItem2(lSubId, lHruSubCount, lLandUseStr, lSoilClass, lSlope_CdStr)
                    lMgtItem2 = lMgtItem2In.Clone()
                    With lMgtItem2
                        .CROP = lCrop.CPNM
                        .HUSC = 0.15
                        .MGT_OP = 1
                        .PLANT_ID = lCrop.ICNUM
                        .HEATUNITS = HeatUnits(lCrop.CPNM)
                    End With
                    lSwatInput.Mgt.Add2(lMgtItem2)
                    'harvest
                    lMgtItem2 = lMgtItem2In.Clone()
                    With lMgtItem2
                        .CROP = lCrop.CPNM
                        .MGT_OP = 5
                        .HUSC = 1.2
                    End With
                    lSwatInput.Mgt.Add2(lMgtItem2)
                    'Catch ex As Exception
                    '    Dim msg As String
                    '    msg = ex.Message
                    'End Try
                End With

            Else
                With lMgtItem1
                    .CN2 = 66
                End With

                'If lLandUseMissing.IndexFromKey(lLandUseStr) = -1 Then
                'Logger.Dbg("MissingLU:" & lLandUseStr)
                'lLandUseMissing.Add(lLandUseStr)
            End If

            lSwatInput.Mgt.Add1(lMgtItem1)

            Return ""

        Catch ex As Exception
            Dim src As String
            msgException = ex.Message
            src = ex.Source

        Finally
            If (msgException.Length > 0) Then
                Throw New Exception("Error in CropInfoFromXml: " & msgException)
            End If

        End Try
        Return ""
    End Function

End Module
