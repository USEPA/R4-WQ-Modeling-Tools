Imports atcUtility
Imports MapWinUtility
Imports D4EM.Geo.NetworkOperations
Imports D4EM.Data.LayerSpecification.Roles
Imports DotSpatial.Data.RasterExt

Public Class SWATmodel

    Public Shared Sub BuildSWATInput(ByVal aProject As D4EM.Data.Project,
                                     ByVal aScenarioName As String,
                                     ByVal aCatchmentsLayer As D4EM.Data.Layer,
                                     ByVal aFlowlinesLayer As D4EM.Data.Layer,
                                     ByVal aLandUseLayer As D4EM.Data.Layer,
                                     ByVal aDemGridLayer As D4EM.Data.Layer,
                                     ByVal aSoilsLayer As D4EM.Data.Layer,
                                     ByVal aSoilProperties As List(Of D4EM.Data.Source.NRCS_Soil.SoilLocation.Soil),
                                     ByVal aCreateArcSWATFiles As Boolean,
                                     ByVal aBuildDatabase As Boolean,
                                     ByVal aSWATDatabaseName As String,
                                     ByVal aGeoProcess As Boolean,
                                     ByVal aResume As Boolean,
                                     ByVal aAreaIgnoreBelowFraction As Double,
                                     ByVal aAreaIgnoreBelowAbsolute As Double,
                                     ByVal aLandUseIgnoreBelowFraction As Double,
                                     ByVal aLandUseIgnoreBelowAbsolute As Double,
                                     ByVal aParameterShapefileName As String,
                                     ByVal aSimulationStartYear As Integer,
                                     ByVal aSimulationEndYear As Integer,
                                     ByVal aUseMgtCropFile As Boolean,
                                     ByVal aOutputSummarize As Boolean,
                                     ByVal aFields As D4EM.Geo.NetworkOperations.FieldIndexes,
                                     ByVal aReportFlowUnits As Short)
        Try
            'KLW 12/21/2009 Generate files for running ArcSWAT
            If (aCreateArcSWATFiles) Then
                Logger.Dbg("CreateArcSWATFiles")
                modArcSWAT.CopyShapeFiles(aFlowlinesLayer.FileName, aProject.ProjectFolder & "\ArcSwat\")
                modArcSWAT.CopyShapeFiles(aCatchmentsLayer.FileName, aProject.ProjectFolder & "\ArcSwat\")
                Dim sfile As String = IO.Path.GetFileNameWithoutExtension(aFlowlinesLayer.FileName)
                Dim numRecs As Integer
                numRecs = modArcSWAT.ConvertFlowLineFile(aProject.ProjectFolder & "\ArcSwat\" & sfile & ".dbf",
                                                         aProject.ProjectFolder + "\ArcSwat\" & sfile & ".dbf")
                sfile = IO.Path.GetFileNameWithoutExtension(aCatchmentsLayer.FileName)
                modArcSWAT.FillCatchmentFile(aProject.ProjectFolder & "\ArcSwat\" & sfile & ".dbf", numRecs)
            End If
            ' End Generate files for running ArcSWAT

            If aBuildDatabase AndAlso Not FileExists(aSWATDatabaseName) Then
                aSWATDatabaseName = FindFile("Please locate SWAT2005.mdb", "SWAT2005.mdb").Replace("swat", "SWAT")
                If Not FileExists(aSWATDatabaseName) Then
                    Logger.Msg("SWAT Database not found: '" & aSWATDatabaseName & "'", "SWAT2005.mdb Required")
                    Exit Sub
                End If
            End If

            Dim lHRUGridFileName As String = IO.Path.Combine(aProject.ProjectFolder, "HRUs.tif")
            Dim lHruTableFilename As String = IO.Path.ChangeExtension(lHRUGridFileName, ".table.txt")
            Dim lHruTable As D4EM.Geo.HRUTable = Nothing
            My.Computer.FileSystem.CurrentDirectory = aProject.ProjectFolder

            'Dim lDisplayTags As New Generic.List(Of String)
            'lDisplayTags.Add("SubBasin")
            'lDisplayTags.Add("Soil")
            'lDisplayTags.Add("SlopeReclass")
            'lDisplayTags.Add("LandUse")

            If aGeoProcess Then
                'Slope - Use existing or compute from elevation
                Dim lSlopeGridFileName = IO.Path.Combine(PathNameOnly(aDemGridLayer.FileName), "slope_" & SafeFilename(aProject.Region.ToString.Replace(" ", ""), "") & ".tif")
                Dim lSlopeLayer = D4EM.Geo.SpatialOperations.GetSlopeGrid(aDemGridLayer, lSlopeGridFileName, aProject.Region)

                Dim lSlopeReclassifyGridFileName = IO.Path.ChangeExtension(lSlopeGridFileName, "Reclassify.tif")
                Dim lSlopeReclassified As D4EM.Data.Layer
                Dim lReclassSpec As New D4EM.Data.LayerSpecification(Name:="Reclassified Slope", Tag:="SlopeReclass", Role:=D4EM.Data.LayerSpecification.Roles.Slope)

                If IO.File.Exists(lSlopeReclassifyGridFileName) Then
                    Logger.Status("UsingExisting " & lSlopeReclassifyGridFileName)
                    lSlopeReclassified = New D4EM.Data.Layer(lSlopeReclassifyGridFileName, lReclassSpec)
                Else
                    Dim lReclassiflyScheme As New Generic.List(Of Double)
                    With lReclassiflyScheme
                        .Add(-0.00001) 'no slope data
                        .Add(0.5)
                        .Add(2)
                        .Add(GetMaxValue) 'All values above previous category. Note: may include some "bad" slope values
                    End With
                    lSlopeReclassified = lSlopeLayer.ReclassifyRange(lReclassiflyScheme, lSlopeReclassifyGridFileName, lReclassSpec)
                    Logger.Status("Slopes Reclassified")
                    Logger.Dbg("Slopes Reclassified " & MemUsage())
                End If

                'subbasin gis properties
                Logger.Status("CalculateCatchmentProperty " & MemUsage(), True)
                CalculateCatchmentProperty(aCatchmentsLayer, aDemGridLayer, IO.Path.Combine(aProject.ProjectFolder, "Catchments.txt"))
                Logger.Status("CalculateCatchmentPropertyDone " & MemUsage(), True)
                'flowline gis properties 
                CalculateFlowlineProperty(aFlowlinesLayer, aDemGridLayer, IO.Path.Combine(aProject.ProjectFolder, "Flowlines.txt"))
                Logger.Status("CalculateFlowlinePropertyDone " & MemUsage(), True)

                If aResume AndAlso IO.File.Exists(lHruTableFilename) Then
                    lHruTable = New D4EM.Geo.HRUTable(lHruTableFilename)
                End If
                If lHruTable Is Nothing OrElse lHruTable.Count = 0 Then

                    Dim lOverlayLayers As New Generic.List(Of D4EM.Data.Layer)
                    lOverlayLayers.Add(aLandUseLayer)
                    lOverlayLayers.Add(aSoilsLayer)
                    lOverlayLayers.Add(lSlopeReclassified)

                    Dim lCatchmentsRasterFilename As String = IO.Path.ChangeExtension(aCatchmentsLayer.FileName, ".raster.tif")
                    If IO.File.Exists(lCatchmentsRasterFilename) Then
                        Dim lCatchmentsRasterLayer As New D4EM.Data.Layer(lCatchmentsRasterFilename, aCatchmentsLayer.Specification)
                        Dim lNumGoodPixels As Integer = 0
                        For lRow As Integer = 1 To lCatchmentsRasterLayer.AsRaster.EndRow
                            For lCol As Integer = 1 To lCatchmentsRasterLayer.AsRaster.EndColumn
                                If lCatchmentsRasterLayer.AsRaster.Value(lRow, lCol) > 0 Then
                                    lNumGoodPixels += 1 'Debug.WriteLine(lRow & ", " & lCol)
                                End If
                            Next
                        Next
                        lCatchmentsRasterLayer.IsRequired = True
                        lOverlayLayers.Add(lCatchmentsRasterLayer)
                    Else
                        'aCatchmentsLayer.AssignIndexes("SUBBASIN")
                        'Dim lCatchmentsRasterLayer As New D4EM.Data.Layer(lCatchmentsRasterFilename, aCatchmentsLayer.Specification, False)
                        'lCatchmentsRasterLayer.DataSet = DotSpatial.Analysis.VectorToRaster.ToRaster(aCatchmentsLayer.AsFeatureSet,
                        '                                                                            lSlopeReclassified.AsRaster.Extent,
                        '                                                                            lSlopeReclassified.AsRaster.Extent.Width / lSlopeReclassified.AsRaster.NumColumns,
                        '                                                                            "SUBBASIN",
                        '                                                                            lCatchmentsRasterFilename,
                        '                                                                            lSlopeLayer.AsRaster.DriverCode, {}, Nothing)
                        aCatchmentsLayer.IsRequired = True
                        lOverlayLayers.Add(aCatchmentsLayer)
                    End If

                    'For lRow As Integer = 1 To lCatchmentsRasterLayer.AsRaster.EndRow
                    '    For lCol As Integer = 1 To lCatchmentsRasterLayer.AsRaster.EndColumn
                    '        If lCatchmentsRasterLayer.AsRaster.Value(lRow, lCol) > 0 Then
                    '            Debug.WriteLine(lRow & ", " & lCol)
                    '        End If
                    '    Next
                    'Next

                    'If g_GeoProcess AndAlso lLayers Is Nothing OrElse lLayers.Length < 1 Then
                    '    Logger.Msg("No layers specified for overlay", "Geoprocessing Overlay Requires Layers")
                    '    Exit Sub
                    'End If

                    'If aGeoProcess Then 'Check for layer that does not exist (e.g. soils)
                    '    For Each lLayer As D4EM.Data.Layer In lOverlayLayers
                    '        If Not IO.File.Exists(lLayer.FileName) Then
                    '            Logger.Msg(lLayer.FileName, "Layer for overlay does not exist")
                    '            Exit Sub
                    '        End If
                    '    Next
                    'End If

                    Logger.Status("Doing Overlay")
                    lHruTable = D4EM.Geo.OverlayReclassify.Overlay(lHRUGridFileName,
                                                              lSlopeLayer,
                                                              lOverlayLayers,
                                                              aResume)
                    If lHruTable IsNot Nothing Then Logger.Status("Overlay Successful")
                End If

                If lHruTable Is Nothing Then
                    Logger.Status("HRU table not produced by overlay")
                ElseIf lHruTable.Count = 0 Then
                    Logger.Status("HRU table produced by overlay is empty")
                Else
                    Dim lHruReportBuilder As New System.Text.StringBuilder
                    lHruTable = lHruTable.Sort(True)
                    D4EM.Geo.OverlayReclassify.ReportByTag(lHruReportBuilder, New atcCollection(lHruTable), lHruTable.Tags) 'lDisplayTags)
                    IO.File.WriteAllText(IO.Path.GetDirectoryName(lHRUGridFileName) & "\Hrus.txt", lHruReportBuilder.ToString)
                    IO.File.WriteAllText(IO.Path.GetDirectoryName(lHRUGridFileName) & "\UniqueValuesSummary.txt", D4EM.Geo.OverlayReclassify.UniqueValuesSummary(lHruTable))
                    IO.File.WriteAllText(IO.Path.GetDirectoryName(lHRUGridFileName) & "\LuCounts.txt", D4EM.Geo.OverlayReclassify.UniqueValuesSummary(lHruTable, "NLCD2011.LandCover")) '"LandUse"))
                End If
                Logger.Status("DoneGeoProcessing " & MemUsage())
            Else
                lHruTable = New D4EM.Geo.HRUTable(lHruTableFilename)
            End If

            If lHruTable.Count > 0 Then
                Logger.Status("CountOfRawHrus " & lHruTable.Count)
                Dim lOriginalIDs As Generic.List(Of String) = Nothing
                Dim lNewIds As Generic.List(Of String) = Nothing

                'Look for any files like g_BaseFolder\ReclassifyLandUse.csv, follow the table if such files exist
                For Each lTag As String In lHruTable.Tags 'lDisplayTags
                    Dim lReclassifyFileName As String = IO.Path.Combine(aProject.ProjectFolder, "Reclassify") & lTag & ".csv"
                    If IO.File.Exists(lReclassifyFileName) Then
                        lHruTable.ReadReclassifyCSV(lReclassifyFileName, lOriginalIDs, lNewIds, ":")
                        lHruTable.Reclassify(lTag, lOriginalIDs, lNewIds)
                        'TODO: lHruTable.Tags(lHruTable.Tags.IndexOf("SubBasin") = "MetSeg"
                        Logger.Status("CountOfHrusAfterReclassify" & lTag & " " & lHruTable.Count)
                    End If
                Next

                If aAreaIgnoreBelowFraction > 0 OrElse aAreaIgnoreBelowAbsolute > 0 Then
                    lHruTable = D4EM.Geo.OverlayReclassify.Simplify(lHruTable.Tags, lHruTable.SplitByTag("SubBasin"),
                                "Area", aAreaIgnoreBelowFraction, aAreaIgnoreBelowAbsolute, lHRUGridFileName)
                    Logger.Status("CountOfHrusAfterSimplifyArea " & lHruTable.Count)
                End If

                If aLandUseIgnoreBelowFraction > 0 OrElse aLandUseIgnoreBelowAbsolute > 0 Then
                    'lHruTable = D4EM.Geo.OverlayReclassify.Simplify(lHruTable.Tags, lHruTable.SplitByTag("SubBasin"),
                    '            "LandUse", aLandUseIgnoreBelowFraction, aLandUseIgnoreBelowAbsolute, lHRUGridFileName)
                    lHruTable = D4EM.Geo.OverlayReclassify.Simplify(lHruTable.Tags, lHruTable.SplitByTag("catchment"),
                                "NLCD2011.LandCover", aLandUseIgnoreBelowFraction, aLandUseIgnoreBelowAbsolute, lHRUGridFileName)
                    Logger.Status("CountOfHrusAfterSimplifyLandUse " & lHruTable.Count)
                End If

                If aBuildDatabase Then
                    Dim lParamTable As atcTable = Nothing
                    Dim lSubBasinToParamIndex As atcCollection = Nothing
                    If IO.File.Exists(aParameterShapefileName) Then
                        Dim lParameterLayer As New D4EM.Data.Layer(aParameterShapefileName, New D4EM.Data.LayerSpecification(Name:="Parameter"))

                        lSubBasinToParamIndex = lParameterLayer.OverlayFeatures(aCatchmentsLayer)
                        lParamTable = New atcTableDBF
                        lParamTable.OpenFile(IO.Path.ChangeExtension(aParameterShapefileName, ".dbf"))
                    End If
                    Logger.Status("BuildSWATDatabase")
                    BuildSwatDatabase(aCacheFolder:=aProject.CacheFolder,
                                      aProjectFolder:=aProject.ProjectFolder,
                                      aFlowlinesFilename:=aFlowlinesLayer.FileName,
                                      aScenario:=aScenarioName,
                                      aSwatDatabaseName:=aSWATDatabaseName,
                                      aParamTable:=lParamTable,
                                      aSubBasinToParamIndex:=lSubBasinToParamIndex,
                                      aHruTable:=lHruTable,
                                      aSoilProperties:=aSoilProperties,
                                      aSimulationStartYear:=aSimulationStartYear,
                                      aSimulationEndYear:=aSimulationEndYear,
                                      aUseMgtCropFile:=aUseMgtCropFile,
                                      aCreateArcSWATFiles:=aCreateArcSWATFiles,
                                      aFields:=aFields,
                                      aReportFlowUnits:=aReportFlowUnits
                                      )
                    Logger.Status("After BuildSwatDatabase " & MemUsage())
                    If (aCreateArcSWATFiles) Then

                    End If
                End If
                Logger.Status("Finished building SWAT scenario " & aScenarioName, True)
            End If
        Catch lEx As Exception
            Logger.Dbg("Exception in BuildSwatInput: " & lEx.ToString)
        End Try
    End Sub

    Public Shared Function RunModel(ByVal aTxtInOutFolder As String) As Integer
        Dim lExitCode As Integer = 0
        If Not IO.Directory.Exists(aTxtInOutFolder) Then
            Logger.Status("Cannot start model, input folder does not exist: " & aTxtInOutFolder)
            lExitCode = -1
        Else
            Dim lExePath As String = FindFile("SWAT Model", "Swat2005.exe")
            If IO.File.Exists(lExePath) Then
                Logger.Status("StartModel " & lExePath)
                lExitCode = MapWinUtility.LaunchProgram(lExePath, aTxtInOutFolder)
                Logger.Status("DoneModelRunExitCode " & lExitCode & " " & MemUsage())
            Else
                Logger.Msg("Could not launch SWAT model", "Swat2005.exe not found")
                lExitCode = -1
            End If
        End If
        Return lExitCode
    End Function

    Friend Shared Sub CalculateCatchmentProperty(ByVal aCatchments As D4EM.Data.Layer,
                                                 ByVal aDemGrid As D4EM.Data.Layer,
                                                 ByVal aSaveAs As String)
        Dim lSB As New System.Text.StringBuilder 'Collect properties here to be saved to aSaveAs at the end
        Dim lCatchments As DotSpatial.Data.IFeatureSet = aCatchments.DataSet
        Dim lLastCatchment As Integer = lCatchments.Features.Count - 1
        Dim lCatchmentComIdFieldIndex As Integer = aCatchments.FieldIndex("COMID")
        If lCatchmentComIdFieldIndex < 0 Then
            'This is not NHDPlus, so probably it is an ArcSWAT ShapeFile
            lCatchmentComIdFieldIndex = aCatchments.FieldIndex("Subbasin")
            Dim lElevFieldIndex As Integer = aCatchments.FieldIndex("Elev")
            Dim lLatFieldIndex As Integer = aCatchments.FieldIndex("Lat")
            Dim lLngFieldIndex As Integer = aCatchments.FieldIndex("Long_")
            If lCatchmentComIdFieldIndex >= 0 AndAlso lElevFieldIndex >= 0 AndAlso lLatFieldIndex >= 0 AndAlso lLngFieldIndex >= 0 Then
                lSB.AppendLine("Index" & vbTab & "SubBasin" & vbTab & "Lat" & vbTab & "Long" & vbTab & "Elev")
                For lShapeIndex As Integer = 0 To lLastCatchment
                    lSB.AppendLine(lShapeIndex & vbTab & _
                                       CStr(lCatchments.Features(lShapeIndex).DataRow(lCatchmentComIdFieldIndex)) & vbTab & _
                                       CStr(lCatchments.Features(lShapeIndex).DataRow(lLatFieldIndex)) & vbTab & _
                                       CStr(lCatchments.Features(lShapeIndex).DataRow(lLngFieldIndex)) & vbTab & _
                                       CStr(lCatchments.Features(lShapeIndex).DataRow(lElevFieldIndex)))
                Next
            Else
                Throw New ApplicationException("CalculateCatchmentProperty: Catchments layer does not contain NHDPlus or ArcSwat fields: " & aCatchments.FileName)
            End If
        Else 'NHDPlus provided this file instead of ArcSWAT, so need to calculate some values instead of just looking them up in ArcSWAT shapefile
            Dim lDemGrid As DotSpatial.Data.Raster = aDemGrid.DataSet ' DotSpatial.Data.Raster.Open(aDemGridFilename)
            Dim lSameProjection As Boolean = lCatchments.Projection.Equals(lDemGrid.Projection)
            lSB.AppendLine("Index" & vbTab & "ComID" & vbTab & "Lat" & vbTab & "Long" & vbTab & "Elev")
            Dim lElevation As Double = 0
            For lShapeIndex As Integer = 0 To lLastCatchment
                Dim lX As Double
                Dim lY As Double
                Dim lCatchmentGeometry As DotSpatial.Topology.Geometry = lCatchments.Features(lShapeIndex).BasicGeometry
                If lCatchmentGeometry.NumPoints < 1 Then
                    Logger.Dbg("CalculateCatchmentProperty: Catchment index " & lShapeIndex & " could not be converted into a BasicGeometry, not adding it to Catchments.txt")
                Else
                    With lCatchmentGeometry.Envelope
                        'Default to using elevation at middle of bounding box in case centroid calculation does not work
                        lX = (.Maximum.X + .Minimum.X) / 2
                        lY = (.Maximum.Y + .Minimum.Y) / 2
                        Try
                            Dim lCentroidCalculator As New DotSpatial.Topology.Algorithm.CentroidArea
                            lCentroidCalculator.Add(lCatchmentGeometry)
                            Dim lCentroid As DotSpatial.Topology.Coordinate = lCentroidCalculator.Centroid

                            If lCentroid.X >= .Minimum.X AndAlso lCentroid.X <= .Maximum.X AndAlso _
                               lCentroid.Y >= .Minimum.Y AndAlso lCentroid.Y <= .Maximum.Y Then
                                lX = lCentroid.X
                                lY = lCentroid.Y
                            Else
                                'DotSpatial.Topology.Algorithm.CentroidArea failed, probably a multi-polygon
                            End If
                        Catch
                            'DotSpatial.Topology.Algorithm.CentroidArea failed, use default location
                        End Try
                    End With
                    If Not lSameProjection Then
                        D4EM.Geo.SpatialOperations.ProjectPoint(lX, lY, lCatchments.Projection, lDemGrid.Projection)
                    End If
                    'TODO - update to allow use of average elevation for whole shape
                    Dim lRowCol As DotSpatial.Data.RcIndex = lDemGrid.ProjToCell(lX, lY)
                    If lRowCol.IsEmpty Then
                        Logger.Dbg("Elevation grid does not cover " & DoubleToString(lX) & ", " & DoubleToString(lY) & " defaulting to " & DoubleToString(lElevation))
                    Else
                        lElevation = D4EM.Geo.SpatialOperations.ValidGridValueNear(lDemGrid, lRowCol, -100000, 1000000)
                        lElevation /= 100.0 'cm to m
                    End If
                    D4EM.Geo.SpatialOperations.ProjectPoint(lX, lY, lDemGrid.Projection, D4EM.Data.Globals.GeographicProjection)
                    lSB.AppendLine(lShapeIndex & vbTab & CStr(lCatchments.Features(lShapeIndex).DataRow(lCatchmentComIdFieldIndex)) & vbTab _
                                 & lY & vbTab & lX & vbTab & lElevation)
                End If
            Next
        End If
        IO.File.WriteAllText(aSaveAs, lSB.ToString)
    End Sub

    Friend Shared Sub CalculateFlowlineProperty(ByVal aFlowLines As D4EM.Data.Layer, ByVal aDemGrid As D4EM.Data.Layer, ByVal aSaveAs As String)
        TryDelete(aSaveAs)

        Dim pFlowlinesComIdFieldIndex As Integer
        Dim pFlowlinesCumDrainAreaIndex As Integer

        Dim lDemGrid As DotSpatial.Data.Raster = aDemGrid.DataSet ' .Open(aDemGridFilename)
        Dim lFlowlines As DotSpatial.Data.FeatureSet = aFlowLines.DataSet
        Dim lFlowLineFileName As String = lFlowlines.Filename

        Dim lSB As New System.Text.StringBuilder
        Dim lArcSwatShapeFile As Boolean = False
        Dim lQMeanFieldIndex As Integer = aFlowLines.FieldIndex("MAFlowU")
        Dim lSlopeFieldIndex As Integer = aFlowLines.FieldIndex("Slope")
        Dim lLengthFieldIndex As Integer = aFlowLines.FieldIndex("LengthKM")
        Dim lWidthFieldIndex As Integer
        Dim lDepthFieldIndex As Integer
        If pFlowlinesComIdFieldIndex = -1 Then
            pFlowlinesComIdFieldIndex = aFlowLines.FieldIndex("Subbasin")
            pFlowlinesCumDrainAreaIndex = aFlowLines.FieldIndex("AreaC")
            lQMeanFieldIndex = aFlowLines.FieldIndex("MAFlowU")
            lSlopeFieldIndex = aFlowLines.FieldIndex("Slo2")
            lLengthFieldIndex = aFlowLines.FieldIndex("Len2")
            lWidthFieldIndex = aFlowLines.FieldIndex("Wid2")
            lDepthFieldIndex = aFlowLines.FieldIndex("Dep2")

            lArcSwatShapeFile = True
            lSB.AppendLine("Index" & vbTab & "SubBasin" & vbTab _
                            & "Width" & vbTab & "Depth" & vbTab & "Slope" & vbTab & "Length")
        Else
            lSB.AppendLine("Index" & vbTab & "ComID" & vbTab _
                            & "Width" & vbTab & "Depth" & vbTab & "Slope" & vbTab & "Length")
            pFlowlinesCumDrainAreaIndex = aFlowLines.FieldIndex("CumDrainag")
            lQMeanFieldIndex = aFlowLines.FieldIndex("MAFlowU")
            lSlopeFieldIndex = aFlowLines.FieldIndex("Slope")
            lLengthFieldIndex = aFlowLines.FieldIndex("LengthKM")
        End If
        Dim lElevation As Double
        For lShapeIndex As Integer = 0 To lFlowlines.NumRows - 1
            Dim lFlowline As DotSpatial.Data.Feature = lFlowlines.Features(lShapeIndex)
            Dim lComID As String
            Dim lWidth As Double = 0
            Dim lDepth As Double = 0
            Dim lSlope As Double = 0
            Dim lLength As Double = 0
            If lFlowline IsNot Nothing Then
                lComID = lFlowlines.Features(lShapeIndex).DataRow(pFlowlinesComIdFieldIndex)
                If lComID <> "0" Then
                    If lArcSwatShapeFile Then
                        Try
                            lWidth = lFlowlines.Features(lShapeIndex).DataRow(lWidthFieldIndex)
                            Logger.Dbg("Missing Width in flowline record " & lShapeIndex & " field " & lWidthFieldIndex & " of " & lFlowLineFileName)
                        Catch
                        End Try
                        Try
                            lDepth = lFlowlines.Features(lShapeIndex).DataRow(lDepthFieldIndex)
                        Catch
                            Logger.Dbg("Missing Depth in flowline record " & lShapeIndex & " field " & lDepthFieldIndex & " of " & lFlowLineFileName)
                        End Try
                        Try
                            lSlope = lFlowlines.Features(lShapeIndex).DataRow(lSlopeFieldIndex)
                            Logger.Dbg("Missing Slope in flowline record " & lShapeIndex & " field " & lSlopeFieldIndex & " of " & lFlowLineFileName)
                        Catch
                        End Try
                        Try
                            lLength = lFlowlines.Features(lShapeIndex).DataRow(lLengthFieldIndex)
                            Logger.Dbg("Missing Length in flowline record " & lShapeIndex & " field " & lLengthFieldIndex & " of " & lFlowLineFileName)
                        Catch
                        End Try
                    Else
                        Dim lFlowlineGeometry As DotSpatial.Topology.Geometry = lFlowline.ToShape.ToGeometry
                        Dim lCentroidCalculator As New DotSpatial.Topology.Algorithm.CentroidLine
                        lCentroidCalculator.Add(lFlowlineGeometry)
                        Dim lCentroid As DotSpatial.Topology.Coordinate = lCentroidCalculator.Centroid
                        Dim lX As Double = lCentroid.X
                        Dim lY As Double = lCentroid.Y
                        With lFlowlineGeometry.Envelope
                            If lX < .Minimum.X OrElse lX > .Maximum.X OrElse _
                               lY < .Minimum.Y OrElse lY > .Maximum.Y Then
                                'MapWinGeoProc.Statistics.Centroid failed, probably a multi-polygon
                                'Use elevation at middle of bounding box
                                lX = (.Maximum.X + .Minimum.X) / 2
                                lY = (.Maximum.Y + .Minimum.Y) / 2
                            End If
                        End With

                        D4EM.Geo.SpatialOperations.ProjectPoint(lX, lY, lFlowlines.Projection, D4EM.Data.Globals.GeographicProjection)
                        Dim lRowCol As DotSpatial.Data.RcIndex = lDemGrid.ProjToCell(lCentroid.X, lCentroid.Y)
                        If lRowCol.IsEmpty Then
                            Logger.Dbg("Elevation grid does not cover " & DoubleToString(lX) & ", " & DoubleToString(lY) & " defaulting to " & DoubleToString(lElevation))
                        Else
                            lElevation = D4EM.Geo.SpatialOperations.ValidGridValueNear(lDemGrid, lRowCol, -100000, 1000000)
                            lElevation /= 100.0 'cm to m
                        End If

                        Dim lAreaContrib As Double
                        If Not ObjectToDouble(lFlowlines.Features(lShapeIndex).DataRow(pFlowlinesCumDrainAreaIndex), lAreaContrib) Then 'km
                            Logger.Dbg("Missing contributing area in flowline record " & lShapeIndex & " field " & pFlowlinesCumDrainAreaIndex & " of " & lFlowLineFileName)
                        End If
                        'Dim lQMean As Double = lFlowlines.Features(lShapeIndex).DataRow(lQMeanFieldIndex) 'cfs
                        'Dim lQPk2 As Double = lQMean * 5 'TODO: WAG UGH - get BASINS neural network code !!!!!!!!!
                        'lWidth = 1.22 * (lQPk2 ^ 0.557) 'from BASINS FAQ
                        'lDepth = 0.34 * (lQPk2 ^ 0.341)
                        lWidth = 1.29 * (lAreaContrib ^ 0.6) 'from BASINS avenue script
                        lDepth = 0.13 * (lAreaContrib ^ 0.4)
                        If Not ObjectToDouble(lFlowlines.Features(lShapeIndex).DataRow(lSlopeFieldIndex), lSlope) Then
                            Logger.Dbg("Missing slope in flowline record " & lShapeIndex & " field " & lSlopeFieldIndex & " of " & lFlowLineFileName)
                        End If
                        If Not ObjectToDouble(lFlowlines.Features(lShapeIndex).DataRow(lLengthFieldIndex), lLength) Then
                            Logger.Dbg("Missing length in flowline record " & lShapeIndex & " field " & lLengthFieldIndex & " of " & lFlowLineFileName)
                        End If
                    End If
                End If
                If lSB.Length > 1000000 Then
                    IO.File.AppendAllText(aSaveAs, lSB.ToString)
                    lSB = New System.Text.StringBuilder
                End If
                lSB.AppendLine(lShapeIndex & vbTab & lComID & _
                                                vbTab & lWidth & _
                                                vbTab & lDepth & _
                                                vbTab & lSlope & _
                                                vbTab & lLength)
            End If
        Next
        IO.File.AppendAllText(aSaveAs, lSB.ToString)
    End Sub

    ''' <summary>
    ''' Read the output of a model run, rewrite as binary files, and write a summary
    ''' </summary>
    ''' <param name="aProjectFolder"></param>
    ''' <param name="aInputFilePath"></param>
    ''' <param name="aScenarioName">Name of folder in aProjectFolder\Scenarios</param>
    ''' <remarks></remarks>
    Public Shared Sub SummarizeOutput(ByVal aProjectFolder As String, ByVal aInputFilePath As String, ByVal aScenarioName As String)
        Dim lReportFilePath As String = IO.Path.Combine(aProjectFolder, "Scenarios" & g_PathChar & aScenarioName & g_PathChar & "TablesOut")

        Dim lOutputHruFileName As String = IO.Path.Combine(aInputFilePath, "output.hru")
        If FileExists(lOutputHruFileName) Then
            Dim lOutputHru As New atcTimeseriesSWAT.atcTimeseriesSWAT
            With lOutputHru
                Dim lOutputFields As New atcData.atcDataAttributes
                lOutputFields.SetValue("FieldName", "AREAkm2;YLDt/ha")
                '.Open(pOutputFolder & "\tab.hru", lOutputFields)
                If .Open(lOutputHruFileName, lOutputFields) Then
                    Logger.Dbg("OutputHruTimserCount " & .DataSets.Count)
                    WriteDatasets(IO.Path.Combine(lReportFilePath, "hru"), .DataSets)
                End If
            End With
        Else
            Logger.Dbg("MissingHruOutput " & lOutputHruFileName)
        End If

        Dim lOutputRchFileName As String = IO.Path.Combine(aInputFilePath, "output.rch")
        If FileExists(lOutputRchFileName) Then
            Dim lOutputRch As New atcTimeseriesSWAT.atcTimeseriesSWAT
            With lOutputRch
                If .Open(lOutputRchFileName) Then
                    Logger.Dbg("OutputRchTimserCount " & .DataSets.Count)
                    WriteDatasets(IO.Path.Combine(lReportFilePath, "rch"), .DataSets)
                End If
            End With
        Else
            Logger.Dbg("MissingRchOutput " & lOutputRchFileName)
        End If

        Dim lOutputSubFileName As String = IO.Path.Combine(aInputFilePath, "output.sub")
        If FileExists(lOutputSubFileName) Then
            Dim lOutputSub As New atcTimeseriesSWAT.atcTimeseriesSWAT
            With lOutputSub
                If .Open(lOutputSubFileName) Then
                    Logger.Dbg("OutputSubTimserCount " & .DataSets.Count)
                    WriteDatasets(IO.Path.Combine(lReportFilePath, "sub"), .DataSets)
                End If
            End With
        Else
            Logger.Dbg("MissingSubOutput " & lOutputSubFileName)
        End If
    End Sub

    Private Shared Sub WriteDatasets(ByVal aFileName As String, ByVal aDatasets As atcData.atcDataGroup)
        Dim lDataTarget As New atcTimeseriesBinary.atcDataSourceTimeseriesBinary
        Dim lFileName As String = aFileName & ".tsbin" 'lDataTarget.Filter.?) Then
        TryDelete(lFileName)
        If lDataTarget.Open(lFileName) Then
            lDataTarget.AddDatasets(aDatasets)
        End If
    End Sub
End Class
