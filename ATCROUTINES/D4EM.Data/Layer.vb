Imports atcUtility
Imports MapWinUtility
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

''' <summary>D4EM vector or raster layer with specification</summary>
''' <remarks></remarks>
Public Class Layer
    Property Specification As LayerSpecification
    Property IsRequired As Boolean = False
    Property IdFieldIndex As Integer = -1

    Private Shared pRenderersPath As String = Nothing

    Private pDataSet As DotSpatial.Data.IDataSet
    Private pFileName As String = ""
    Private pIsFeatureSet As Boolean

    Public Sub New(ByVal aLayerFileName As String,
                   ByVal aLayerSpecification As LayerSpecification,
          Optional ByVal aOpenNow As Boolean = True)
        FileName = aLayerFileName
        Specification = aLayerSpecification
        GetDefaultRenderer(aLayerFileName)

        If aOpenNow Then
            Open()
            'TODO: only check for IDFIELD/IDNAME when needed
            'If TypeOf (DataSet) Is DotSpatial.Data.FeatureSet Then
            '    If Specification.IdFieldIndex < 0 Then
            '        Throw New ApplicationException("Layer " & aLayerFileName & " missing IDFIELD")
            '    ElseIf Specification.IdFieldName.Length = 0 Then
            '        Throw New ApplicationException("Layer " & aLayerFileName & " missing IDNAME")
            '        'ElseIf Specification.IdFieldName <> ShapeFile.Field(IdFieldIndex).Name Then
            '        '    Throw New Exception("Layer " & Tag & " FieldName " & IdFieldName & " and FieldIndex " & IdFieldIndex & " are incompatable")
            '    Else
            '        Logger.Dbg("ShapeLayer " & Specification.Tag & " opened, using ID field " & Specification.IdFieldName & " (" & Specification.IdFieldIndex & ")")
            '    End If
            'Else
            '    Logger.Dbg("GridLayer " & Specification.Tag & " opened")
            'End If
            If pDataSet IsNot Nothing Then
                Logger.Dbg("Opened " & aLayerFileName & " as " & DataSet.GetType.ToString)
            End If
        End If
    End Sub

    Public Sub New(ByVal aDataSet As DotSpatial.Data.IDataSet, ByVal aLayerSpecification As LayerSpecification)
        pDataSet = aDataSet
        FileName = CType(aDataSet, Object).Filename
        Specification = aLayerSpecification
        GetDefaultRenderer(FileName)
        If IsFeatureSet() AndAlso Not String.IsNullOrEmpty(Specification.IdFieldName) Then
            IdFieldIndex = FieldIndex(Specification.IdFieldName)
        End If
        Globals.RepairAlbers(pDataSet.Projection)
    End Sub

    Property FileName As String
        Get
            Return pFileName
        End Get
        Set(ByVal value As String)
            pFileName = value
            pIsFeatureSet = IO.Path.GetExtension(pFileName).ToLower.Equals(".shp")
        End Set
    End Property

    ''' <summary>
    ''' Folder containing default rendering information for layers (*.mwsr, *.mwsymb, *.mwleg)
    ''' </summary>
    ''' <remarks>Note that this is Shared, not set differently for each Layer</remarks>
    Public Shared Property RenderersPath() As String
        Get
            If String.IsNullOrEmpty(pRenderersPath) Then
                Dim lSearched As New Generic.List(Of String) 'Keep track of where we did not find renderers to report if never found
                Dim lStartPath As String = PathNameOnly(System.Reflection.Assembly.GetExecutingAssembly.Location)
                While IO.Directory.Exists(lStartPath)
                    pRenderersPath = lStartPath & g_PathChar & "renderers" & g_PathChar : If IO.Directory.Exists(pRenderersPath) Then Return pRenderersPath
                    lSearched.Add(pRenderersPath)
                    pRenderersPath = lStartPath & g_PathChar & "etc" & g_PathChar & "renderers" & g_PathChar : If IO.Directory.Exists(pRenderersPath) Then Return pRenderersPath
                    lSearched.Add(pRenderersPath)
                    lStartPath = PathNameOnly(lStartPath)
                End While

                lStartPath = PathNameOnly(Reflection.Assembly.GetEntryAssembly.Location)
                While IO.Directory.Exists(lStartPath)
                    pRenderersPath = lStartPath & g_PathChar & "renderers" & g_PathChar : If IO.Directory.Exists(pRenderersPath) Then Return pRenderersPath
                    lSearched.Add(pRenderersPath)
                    pRenderersPath = lStartPath & g_PathChar & "etc" & g_PathChar & "renderers" & g_PathChar : If IO.Directory.Exists(pRenderersPath) Then Return pRenderersPath
                    lSearched.Add(pRenderersPath)
                    lStartPath = PathNameOnly(lStartPath)
                End While
                pRenderersPath = ""
                Logger.Dbg("Could not find renderers folder, using random renderers. Searched:" & vbCrLf & String.Join(vbCrLf, lSearched.ToArray()))
            End If
            Return pRenderersPath
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) OrElse IO.Directory.Exists(value) Then
                pRenderersPath = value
            Else
                Logger.Dbg("Ignored attempt to set renderers path to '" & value & "' which is not a directory")
            End If
        End Set
    End Property

    ''' <summary>
    ''' If the named layer does not have a .mwsymb or .mwsr (for shape) or .mwleg (for grid) then look for the default file
    ''' and put a copy of the default renderer with the layer
    ''' </summary>
    ''' <returns>Filename of newly copied renderer file if default was copied or "" if not copied</returns>
    Public Shared Function GetDefaultRenderer(ByVal aLayerFilename As String) As String
        Dim lFilename As String = ""
        If Not String.IsNullOrEmpty(aLayerFilename) AndAlso IO.Directory.Exists(RenderersPath) Then
            For Each lRendererExt As String In {".shp.mwsymb", ".mwsr", ".mwleg"}
                If lRendererExt IsNot Nothing Then
                    Dim lRendererFilename As String = IO.Path.ChangeExtension(aLayerFilename, lRendererExt)
                    If IO.File.Exists(lRendererFilename) Then
                        Return lRendererFilename
                    End If
                    If lRendererFilename.Length > 0 Then
                        Dim lRendererFilenameNoPath As String = IO.Path.GetFileName(lRendererFilename)
                        If lRendererFilenameNoPath.StartsWith("NASS") Then
                            lRendererFilenameNoPath = "NASS" & lRendererExt
                        ElseIf aLayerFilename.Contains(g_PathChar & "landuse" & g_PathChar) Then
                            lRendererFilenameNoPath = "giras" & lRendererExt
                        End If
                        Dim lDefaultRendererFilename As String = FindFile("", RenderersPath() & lRendererFilenameNoPath)
                        If Not FileExists(lDefaultRendererFilename) Then
                            If lRendererFilenameNoPath.Contains("_") Then 'Some layers are named huc8_xxx.shp, renderer is named _xxx & lRendererExt
                                lDefaultRendererFilename = FindFile("", RenderersPath() & lRendererFilenameNoPath.Substring(lRendererFilenameNoPath.IndexOf("_")))
                            End If
                            If Not FileExists(lDefaultRendererFilename) Then 'Try trimming off numbers before extension in layername2.mwleg
                                lDefaultRendererFilename = FindFile("", RenderersPath() & IO.Path.GetFileNameWithoutExtension(aLayerFilename).TrimEnd("0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c) & lRendererExt)
                            End If
                            If Not FileExists(lDefaultRendererFilename) Then 'Try trimming off numbers before extension in layername2.mwleg
                                lDefaultRendererFilename = FindFile("", RenderersPath() & IO.Path.GetFileNameWithoutExtension(aLayerFilename).TrimEnd("0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c) & lRendererExt)
                            End If
                        End If
                        If FileExists(lDefaultRendererFilename) Then
                            If lRendererFilenameNoPath = "NASS.mwleg" Then
                                Dim lRendererContents As String = IO.File.ReadAllText(lDefaultRendererFilename)
                                IO.File.WriteAllText(lRendererFilename, lRendererContents.Replace("NASS", "NASS " & IO.Path.GetFileName(lRendererFilename).Substring(4, 4)))
                            Else
                                IO.File.Copy(lDefaultRendererFilename, lRendererFilename)
                            End If
                            lFilename = lRendererFilename
                        End If
                    End If
                End If
            Next
        End If
        Return lFilename
    End Function

    Property DataSet As DotSpatial.Data.IDataSet
        Get
            If pDataSet Is Nothing Then
                Open()
            End If
            Return pDataSet
        End Get
        Set(ByVal value As DotSpatial.Data.IDataSet)
            pDataSet = value
        End Set
    End Property

    Public Function Projection() As DotSpatial.Projections.ProjectionInfo
        If pDataSet Is Nothing Then
            Try 'See if we can find projection without opening layer
                Dim lProjectionFilename As String = IO.Path.ChangeExtension(FileName, ".prj")
                If IO.File.Exists(lProjectionFilename) Then Return DotSpatial.Projections.ProjectionInfo.FromEsriString(IO.File.ReadAllText(lProjectionFilename))
            Catch 'Ignore exception. If we can't read projection file, then open the layer and find it inside.
            End Try
        End If
        If IsFeatureSet() Then Return AsFeatureSet.Projection Else Return AsRaster.Projection
    End Function

    Function IsFeatureSet() As Boolean
        Return pIsFeatureSet
    End Function

    Function AsFeatureSet() As DotSpatial.Data.FeatureSet
        Return DataSet
    End Function

    Function AsRaster() As DotSpatial.Data.Raster
        Return DataSet
    End Function

    ''' <summary>
    ''' If the layer is a FeatureSet, get and set values in its DBF
    ''' </summary>
    ''' <param name="aFieldIndex">Zero-based index of column in DBF</param>
    ''' <param name="aFeatureIndex">Zero-based index of shape in the shape file</param>
    ''' <remarks>Raises an exception if this layer is not a FeatureSet or if an index is out of range</remarks>
    Public Property CellValue(ByVal aFieldIndex As Integer, ByVal aFeatureIndex As Integer) As Object
        Get
            Return AsFeatureSet.Features(aFeatureIndex).DataRow(aFieldIndex)
        End Get
        Set(ByVal value As Object)
            AsFeatureSet.Features(aFeatureIndex).DataRow(aFieldIndex) = value
        End Set
    End Property

    ''' <summary>
    ''' Get the range of cells that overlap the rectangle defined by the given projected coordinates
    ''' </summary>
    ''' <param name="aRaster">Grid containing cells</param>
    ''' <param name="aX1">first horizontal coordinate</param>
    ''' <param name="aY1">first vertical coordinate</param>
    ''' <param name="aX2">second horizontal coordinate</param>
    ''' <param name="aY2">second vertical coordinate</param>
    ''' <param name="aMinRow">returns the minumum row included in the rectangle</param>
    ''' <param name="aMinColumn">returns the minumum column included in the rectangle</param>
    ''' <param name="aMaxRow">returns the maximum row included in the rectangle</param>
    ''' <param name="aMaxColumn">returns the maximum column included in the rectangle</param>
    ''' <remarks>order of X1, X2 or of Y1, Y2 does not matter, either can be the minumum or maximum</remarks>
    Public Shared Sub CellBoundsRaster(ByVal aRaster As DotSpatial.Data.Raster,
                                       ByVal aX1 As Double, ByVal aY1 As Double,
                                       ByVal aX2 As Double, ByVal aY2 As Double,
                                       ByRef aMinRow As Integer, ByRef aMinColumn As Integer,
                                       ByRef aMaxRow As Integer, ByRef aMaxColumn As Integer)
        Dim lRcIndex As New DotSpatial.Data.RcIndex
        lRcIndex = DotSpatial.Data.RasterExt.ProjToCell(aRaster, Math.Min(aX1, aX2), Math.Max(aY1, aY2))
        If lRcIndex.IsEmpty() Then
            aMinRow = 0
            aMinColumn = 0
        Else
            aMinRow = lRcIndex.Row
            aMinColumn = lRcIndex.Column
        End If

        lRcIndex = DotSpatial.Data.RasterExt.ProjToCell(aRaster, Math.Max(aX1, aX2), Math.Min(aY1, aY2))
        If lRcIndex.IsEmpty() Then
            aMaxRow = aRaster.EndRow
            aMaxColumn = aRaster.EndColumn
        Else
            aMaxRow = lRcIndex.Row
            aMaxColumn = lRcIndex.Column
        End If
    End Sub

    ''' <summary>
    ''' Get the range of cells in this grid that overlap the rectangle defined by the given projected coordinates
    ''' </summary>
    ''' <param name="aX1">first horizontal coordinate</param>
    ''' <param name="aY1">first vertical coordinate</param>
    ''' <param name="aX2">second horizontal coordinate</param>
    ''' <param name="aY2">second vertical coordinate</param>
    ''' <param name="aMinRow">returns the minumum row included in the rectangle</param>
    ''' <param name="aMinColumn">returns the minumum column included in the rectangle</param>
    ''' <param name="aMaxRow">returns the maximum row included in the rectangle</param>
    ''' <param name="aMaxColumn">returns the maximum column included in the rectangle</param>
    ''' <remarks>order of X1, X2 or of Y1, Y2 does not matter, either can be the minumum or maximum</remarks>
    Public Sub CellBounds(ByVal aX1 As Double, ByVal aY1 As Double,
                          ByVal aX2 As Double, ByVal aY2 As Double,
                          ByRef aMinRow As Integer, ByRef aMinColumn As Integer,
                          ByRef aMaxRow As Integer, ByRef aMaxColumn As Integer)
        CellBoundsRaster(AsRaster, aX1, aY1, aX2, aY2, aMinRow, aMinColumn, aMaxRow, aMaxColumn)
    End Sub

    Public Function Feature(ByVal aFeatureIndex As Integer) As DotSpatial.Data.Feature
        Return AsFeatureSet.Features(aFeatureIndex)
    End Function

    Public Function PointInShape(ByVal aFeatureIndex As Integer, ByVal aX As Double, ByVal aY As Double) As Boolean
        Dim lGeometry As DotSpatial.Topology.Geometry = AsFeatureSet.Features(aFeatureIndex).BasicGeometry
        Return lGeometry.Intersects(aX, aY)
    End Function

    Private pLastCoordinateIndex As Integer = -1
    'Private pGeometries As Generic.List(Of DotSpatial.Topology.Geometry)

    ''' <summary>
    ''' Returns the index of the polygon that contains the given point, or -1 if no polygon contains the point
    ''' </summary>
    ''' <param name="aX"></param>
    ''' <param name="aY"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CoordinatesInShapefile(ByVal aX As Double, ByVal aY As Double) As Integer
        'Dim lCoordinate As New DotSpatial.Topology.Coordinate(aX, aY)
        'Dim lIndex As Integer = 0
        'For Each lFeature As DotSpatial.Data.Feature In AsFeatureSet.Features
        'With lFeature.Envelope
        'If DotSpatial.Topology.Envelope.Intersects(.Minimum, .Maximum, lCoordinate) Then
        'Dim lGeometry As DotSpatial.Topology.Geometry = lFeature.BasicGeometry
        'If lGeometry.Intersects(aX, aY) Then
        'Return lIndex
        'End If
        'End If
        'End With
        'lIndex += 1
        'Next
        With AsFeatureSet()
            If .Extent.Intersects(aX, aY) Then
                Dim lCoordinate As New DotSpatial.Topology.Coordinate(aX, aY)
                Dim lShapeIndices = .ShapeIndices

                If pLastCoordinateIndex > -1 AndAlso lShapeIndices(pLastCoordinateIndex).Intersects(lCoordinate) Then
                    Return pLastCoordinateIndex
                End If

                Dim lIndex As Integer = 0
                For Each lShapeRange As DotSpatial.Data.ShapeRange In .ShapeIndices
                    If lIndex <> pLastCoordinateIndex AndAlso lShapeRange.Intersects(lCoordinate) Then
                        pLastCoordinateIndex = lIndex
                        Return lIndex
                    End If
                    lIndex += 1
                Next
            End If
        End With
        'Dim lFeature As DotSpatial.Data.Feature
        'Dim lGeometry As DotSpatial.Topology.Geometry

        'If pGeometries Is Nothing Then
        '    pGeometries = New Generic.List(Of DotSpatial.Topology.Geometry)
        '    For Each lFeature In AsFeatureSet.Features
        '        pGeometries.Add(lFeature.BasicGeometry)
        '    Next
        'End If

        'If pLastCoordinateIndex > -1 AndAlso pGeometries(pLastCoordinateIndex).Intersects(aX, aY) Then
        '    Return pLastCoordinateIndex
        'End If

        'Dim lIndex As Integer = 0
        'For Each lGeometry In pGeometries
        '    If lIndex <> pLastCoordinateIndex AndAlso lGeometry.Intersects(aX, aY) Then
        '        pLastCoordinateIndex = lIndex
        '        Return lIndex
        '    End If
        '    lIndex += 1
        'Next
        Return -1
    End Function

    'TODO: move modSDM.ClipCatchments here?
    'Public Function Clip()

    'End Function

    ''' <summary>
    ''' Reproject the layer and save to disk, replacing original file
    ''' </summary>
    ''' <param name="aDesiredProjection"></param>
    ''' <returns></returns>
    ''' <remarks>Currently only implemented for FeatureSet layers</remarks>
    Public Function Reproject(ByVal aDesiredProjection As DotSpatial.Projections.ProjectionInfo) As Boolean
        Dim lSuccess As Boolean = False
        If TypeOf (DataSet) Is DotSpatial.Data.FeatureSet Then
            Dim lFeatureSet As DotSpatial.Data.FeatureSet = AsFeatureSet()
            If lFeatureSet.Projection.Equals(aDesiredProjection) Then
                Logger.Status("Reproject: already in desired projection: " & FileName)
                lSuccess = True
            Else
                Dim lOriginalProjectionString As String = lFeatureSet.Projection.ToProj4String
                lFeatureSet.Reproject(aDesiredProjection)
                lFeatureSet.Save()

                'TODO: move this metadata handling into FeatureSet.Reproject
                Dim lMetadataFilename As String = FileName & ".xml"
                Dim lMetadata As New Metadata(lMetadataFilename)
                lMetadata.AddProcessStep("Projected from '" & lOriginalProjectionString & "' to '" & aDesiredProjection.ToProj4String & "'")
                Dim lExtents As New Region(lFeatureSet.Extent.MaxY, lFeatureSet.Extent.MinY, lFeatureSet.Extent.MinX, lFeatureSet.Extent.MaxX, aDesiredProjection)
                Dim lNorth, lSouth, lEast, lWest As Double
                lExtents.GetBounds(lNorth, lSouth, lWest, lEast, D4EM.Data.Globals.GeographicProjection)
                lMetadata.SetBoundingBox(DoubleToString(lWest), DoubleToString(lEast), DoubleToString(lNorth), DoubleToString(lSouth))
                lMetadata.Save()

                lSuccess = True
            End If
        Else
            Throw New ApplicationException("Layer.Reproject currently only supports FeatureSets")
        End If
        Return lSuccess
    End Function

    ''' <summary>
    ''' Add to XML metadata for a layer
    ''' </summary>
    ''' <param name="aProcessStep">contents of new "procstep" entry</param>
    ''' <remarks>metadata is saved in Me.FileName.xml</remarks>
    Public Sub AddProcessStep(ByVal aProcessStep As String)
        AddProcessStepToFile(aProcessStep, FileName)
    End Sub

    ''' <summary>
    ''' Add to XML metadata for a layer
    ''' </summary>
    ''' <param name="aProcessStep">contents of new "procstep" entry</param>
    ''' <param name="aDataFilename">Full path of data file, metadata is saved in aLayerFilename.xml</param>
    Public Shared Sub AddProcessStepToFile(ByVal aProcessStep As String, _
                                           ByVal aDataFilename As String)
        Dim lMetadataFilename As String = aDataFilename & ".xml"
        Dim lMetadata As New Metadata(lMetadataFilename)
        lMetadata.AddProcessStep(aProcessStep)
        lMetadata.Save()
    End Sub


    '''' <summary>
    '''' Add to XML metadata for all layers matching any given pattern
    '''' </summary>
    '''' <param name="aProcessStep">contents of new "procstep" entry</param>
    '''' <param name="aFolder">Folder to search for matching files</param>
    '''' <param name="aPatterns">Patterns to search for, e.g. "*.shp", "*.tif"</param>
    'Public Shared Sub AddProcessStepToAllLayers(ByVal aProcessStep As String, _
    '                                            ByVal aFolder As String, _
    '                                            ByVal ParamArray aPatterns() As String)
    '    For Each lPattern As String In aPatterns
    '        Dim lMatchingFiles() As String = IO.Directory.GetFiles(aFolder, lPattern, IO.SearchOption.AllDirectories)
    '        For Each lFilename As String In lMatchingFiles
    '            AddProcessStepToLayer(aProcessStep, lFilename)
    '        Next
    '    Next
    'End Sub

    Public Sub CopyProcStepsFromCachedFile(ByVal aCacheFilename As String)
        CopyProcStepsFromCachedFile(aCacheFilename, FileName)
    End Sub

    Public Shared Sub CopyProcStepsFromCachedFile(ByVal aCacheFilename As String,
                                                  ByVal aDataFilename As String)
        Const lCachedPrefix As String = "Read from cache file created "
        If IO.File.Exists(aCacheFilename) Then
            Dim lMetadata As New Metadata(aDataFilename & ".xml")
            With lMetadata
                Dim lCacheMetadataFilename As String = aCacheFilename & ".xml"
                Dim lAlreadyCached As Boolean = False
                If IO.File.Exists(lCacheMetadataFilename) Then
                    Dim lCacheMetadata As New MapWinUtility.Metadata(lCacheMetadataFilename)
                    Dim lPreviousSteps = lCacheMetadata.GetProcessSteps()
                    .AddProcessSteps(lPreviousSteps)
                    For Each lStep In lPreviousSteps
                        If lStep.StartsWith(lCachedPrefix) Then
                            lAlreadyCached = True
                            Exit For
                        End If
                    Next
                End If
                If Not lAlreadyCached Then
                    .AddProcessStep(lCachedPrefix & Format(IO.File.GetCreationTime(aCacheFilename), "yyyy-MM-dd HH:mm") & " '" & aCacheFilename & "'")
                End If
                .Save()
            End With
        End If
    End Sub

    'Private pRowCachedKeys As Integer = -1
    'Private pCachedKeys() As String
    ' ''' <summary>
    ' ''' Retrieve the key value from this layer for the specified grid cell
    ' ''' </summary>
    ' ''' <param name="aGrid">Reference grid for locating cell</param>
    ' ''' <param name="aRow">Row of location in reference grid</param>
    ' ''' <param name="aCol">Column of location in reference grid</param>
    ' ''' <returns>Key value from this layer for the given location</returns>
    ' ''' <remarks>when layer is a shapefile, a row of keys is cached,
    ' ''' so access is more efficient when Key is called for all cells needed
    ' ''' in one row before requesting a Key from another row</remarks>
    'Public Function Key(ByVal aGrid As DotSpatial.Data.Raster, ByVal aRow As Integer, ByVal aCol As Integer) As String
    '    Dim lKey As String = ""
    '    If IsFeatureSet() Then
    '        Dim lProjCoord As DotSpatial.Topology.Coordinate = DotSpatial.Data.RasterExt.CellToProj(aGrid, aRow, aCol)
    '        Dim lFeatureIndex As Integer = CoordinatesInShapefile(lProjCoord.X, lProjCoord.Y)
    '        If lFeatureIndex >= 0 Then
    '            lKey = AsFeatureSet.Features(lFeatureIndex).DataRow(IdFieldIndex)
    '        End If

    '        'For Each lFeature As DotSpatial.Data.Feature In lMyFeatureSet.Features
    '        '    Dim lPolygonGeometry As DotSpatial.Topology.Geometry = lFeature.BasicGeometry
    '        '    If lPolygonGeometry.Intersects(lProjCoord.X, lProjCoord.Y) Then
    '        '        lKey = lFeature.DataRow.Item(pIdFieldIndex)
    '        '        Exit For
    '        '    End If
    '        'Next

    '        'If aRow <> Me.pRowCachedKeys Then 'Read a new row of key values into pCachedKeys
    '        '    Dim lLastShapeIndex As Integer = -1
    '        '    Dim lLastCol As Integer = aGrid.EndColumn
    '        '    ReDim pCachedKeys(lLastCol)
    '        '    For lCol As Integer = 0 To lLastCol
    '        '        Dim lProjCoord As DotSpatial.Topology.Coordinate = DotSpatial.Data.RasterExt.CellToProj(aGrid, aRow, lCol)
    '        '        Dim lShapeIndex As Integer = -1
    '        '        If lLastShapeIndex > -1 Then
    '        '            If lMyFeatureSet. .PointInShape(lLastShapeIndex, lProjCoord) Then
    '        '                lShapeIndex = lLastShapeIndex
    '        '            End If
    '        '        End If
    '        '        If lShapeIndex = -1 Then
    '        '            lShapeIndex = Me.ShapeFile.PointInShapefile(lProjCoord)
    '        '            lLastShapeIndex = lShapeIndex
    '        '        End If
    '        '        Dim lId As String
    '        '        If lShapeIndex = -1 Then
    '        '            lId = ""
    '        '        Else
    '        '            lId = Me.ShapeFile.CellValue(.IdFieldIndex, lShapeIndex)
    '        '        End If
    '        '        If lId Is Nothing Then
    '        '            lId = ""
    '        '        End If
    '        '        pCachedKeys(lCol) = lId
    '        '    Next
    '        '    Me.pRowCachedKeys = aRow
    '        'End If
    '        'Return pCachedKeys(aCol)
    '    Else
    '        Dim lMyRaster As DotSpatial.Data.Raster = AsRaster()
    '        If NeedsProjection Then
    '            Dim lProjCoord As DotSpatial.Topology.Coordinate = DotSpatial.Data.RasterExt.CellToProj(aGrid, aRow, aCol)
    '            With DotSpatial.Data.RasterExt.ProjToCell(lMyRaster, lProjCoord)
    '                aRow = .Row
    '                aCol = .Column
    '            End With
    '        End If
    '        If aRow < 0 OrElse aCol < 0 OrElse aRow > lMyRaster.EndRow OrElse aCol > lMyRaster.EndColumn Then
    '            lKey = ""
    '        Else
    '            lKey = lMyRaster.Value(aRow, aCol)
    '            If lKey = lMyRaster.NoDataValue Then
    '                lKey = ""
    '            End If
    '        End If
    '    End If
    '    Return lKey
    'End Function

    'Public Function MatchesGrid(ByVal aGrid As MapWinGIS.Grid) As Boolean
    '    If Not Me.IsShape Then
    '        Try
    '            Dim lLastCol As Integer = Me.Grid.Header.NumberCols
    '            Dim lLastRow As Integer = Me.Grid.Header.NumberRows
    '            If aGrid.Header.NumberCols = lLastCol AndAlso _
    '                aGrid.Header.NumberRows = lLastRow Then
    '                Dim lCol As Integer, lRow As Integer
    '                Dim lX As Double, lY As Double
    '                aGrid.CellToProj(0, 0, lX, lY)
    '                Me.Grid.ProjToCell(lX, lY, lCol, lRow)
    '                If lCol = 0 AndAlso lRow = 0 Then
    '                    aGrid.CellToProj(lLastCol, lLastRow, lX, lY)
    '                    Me.Grid.ProjToCell(lX, lY, lCol, lRow)
    '                    Return (lCol = lLastCol AndAlso lRow = lLastRow)
    '                End If
    '            End If
    '        Catch e As Exception
    '            Logger.Dbg("MatchesGrid Exception: " & e.Message)
    '        End Try
    '    End If
    '    Return False
    'End Function

    Private Sub Open()
        Dim lError As String = ""
        If Not IO.File.Exists(FileName) Then
            lError = "Cannot open layer, file does not exist: '" & FileName & "'"
        Else
            If IsFeatureSet() Then
                If FileLen(FileName) <= 100 Then
                    lError = "Empty shape file, deleting '" & FileName & "'"
                    TryDeleteShapefile(FileName)
                ElseIf Not FileExists(IO.Path.ChangeExtension(FileName, ".shx")) Then
                    lError = "Shape file missing shx, deleting '" & FileName & "'"
                    TryDeleteShapefile(FileName)
                ElseIf Not FileExists(IO.Path.ChangeExtension(FileName, ".dbf")) Then
                    lError = "Shape file missing DBF, deleting '" & FileName & "'"
                    TryDeleteShapefile(FileName)
                Else
                    ReplaceNonNumericDBF(IO.Path.ChangeExtension(FileName, ".dbf"), "")
                End If
            End If
            If lError.Length = 0 Then
                DataSet = DotSpatial.Data.DataManager.DefaultDataManager.OpenFile(FileName)
                If DataSet Is Nothing Then
                    Throw New ApplicationException("Failed to open layer: " & FileName)
                ElseIf DataSet.Projection Is Nothing Then
                    Throw New ApplicationException("No projection for layer: " & FileName)
                End If
                If IsFeatureSet() Then
                    IdFieldIndex = FieldIndex(Specification.IdFieldName)
                End If
                Globals.RepairAlbers(pDataSet.Projection)
            End If
        End If
        If lError.Length > 0 Then
            Logger.Dbg("Layer.Open: Error: " & lError)
            'Throw New ApplicationException(lError)
        End If
    End Sub

    Public Sub Close()
        If pDataSet IsNot Nothing Then
            pDataSet.Close()
            pDataSet = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Re-write any non-numeric values found in numeric fields
    ''' </summary>
    ''' <param name="aPathDBF">Full path to DBF file</param>
    ''' <param name="aSetNonNumeric">Value to replace non-numeric values with</param>
    ''' <remarks></remarks>
    Public Shared Sub ReplaceNonNumericDBF(ByVal aPathDBF As String, ByVal aSetNonNumeric As String)
        Dim lDBF As New atcTableDBF
        lDBF.OpenFile(aPathDBF)
        Dim lFieldIndex As Integer
        Dim lLastRecordIndex As Integer = lDBF.NumRecords

        Dim lNumericFields As New Generic.List(Of Integer)
        For lFieldIndex = 1 To lDBF.NumFields
            If lDBF.FieldType(lFieldIndex) = "N" Then
                lNumericFields.Add(lFieldIndex)
            End If
        Next

        If lNumericFields.Count > 0 Then
            Dim lChangedPerField(lDBF.NumFields) As Integer
            Dim lChanged As Integer = 0
            Dim lValue As String
            For lRecord As Integer = 1 To lLastRecordIndex
                For Each lFieldIndex In lNumericFields
                    lValue = lDBF.Value(lFieldIndex)
                    If Not IsNumeric(lValue) AndAlso lValue <> aSetNonNumeric Then
                        lDBF.Value(lFieldIndex) = aSetNonNumeric
                        lChangedPerField(lFieldIndex) += 1
                        lChanged += 1
                    End If
                Next
                If lRecord < lLastRecordIndex Then lDBF.MoveNext()
            Next
            If lChanged > 0 Then
                If lDBF.NumRecords <> lLastRecordIndex Then
                    Throw New ApplicationException("ReplaceNonNumericDBF changed number of records from " & lLastRecordIndex & " to " & lDBF.NumRecords)
                End If
                lDBF.WriteFile(aPathDBF)
                Logger.Dbg("Set " & lChanged & " non-numeric values out of " & lLastRecordIndex & " rows to '" & aSetNonNumeric & "' in " & aPathDBF)
                For lFieldIndex = 1 To lDBF.NumFields
                    If lChangedPerField(lFieldIndex) > 0 Then
                        Logger.Dbg("Field " & lFieldIndex & " '" & lDBF.FieldName(lFieldIndex) & "' set " & lChangedPerField(lFieldIndex))
                    End If
                Next
            End If
        End If
        lDBF.Clear()
    End Sub

    Public Function Reclassify(ByVal aReclassiflyScheme As atcCollection,
                               ByVal aReclassifyGridName As String,
                               Optional ByVal aNoKeyNoData As Boolean = False) As Boolean
        Dim lResult As Boolean = False
        Throw New NotImplementedException("ReclassifyRange not yet implemented in DotSpatial")
        'If Not TypeOf (DataSet) Is DotSpatial.Data.FeatureSet Then ' PolygonShapefile Then
        '    Try
        '        Dim lReclassifyGridHeader As New MapWinGIS.GridHeader
        '        lReclassifyGridHeader.CopyFrom(Me.Grid.Header)
        '        Dim lReclassifyGrid As New MapWinGIS.Grid
        '        lReclassifyGrid.CreateNew(aReclassifyGridName, lReclassifyGridHeader, Me.Grid.DataType, 0, True, MapWinGIS.GridFileType.GeoTiff)
        '        With Me.Grid
        '            For lRow As Integer = 0 To .Header.NumberRows - 1
        '                For lCol As Integer = 0 To .Header.NumberCols - 1
        '                    Dim lGridValue As Double = .Value(lRow, lCol)
        '                    Dim lKeyIndex As Integer = aReclassiflyScheme.IndexFromKey(lGridValue)
        '                    If lKeyIndex = -1 Then
        '                        If aNoKeyNoData Then
        '                            lReclassifyGrid.Value(lRow, lCol) = lReclassifyGrid.Header.NodataValue
        '                        Else
        '                            lReclassifyGrid.Value(lRow, lCol) = lGridValue
        '                        End If
        '                    Else
        '                        lReclassifyGrid.Value(lRow, lCol) = aReclassiflyScheme.ItemByIndex(lKeyIndex)
        '                    End If
        '                Next
        '            Next
        '        End With
        '        lResult = lReclassifyGrid.Save(aReclassifyGridName, MapWinGIS.GridFileType.GeoTiff, Nothing)
        '        lReclassifyGrid.Close()
        '    Catch lEx As ApplicationException
        '        Throw New Exception("ReclassifyRangeExcepton " & lEx.Message)
        '    End Try
        'End If
        Return lResult
    End Function

    Public Function ReclassifyRange(ByVal aReclassiflyScheme As Generic.List(Of Double),
                                    ByVal aReclassifyGridName As String,
                                    ByVal aNewSpecification As D4EM.Data.LayerSpecification) As Layer
        If DataSet Is Nothing Then
            Open()
        End If

        If DataSet IsNot Nothing AndAlso TypeOf (DataSet) Is DotSpatial.Data.Raster Then
            Try
                Dim lMyRaster As DotSpatial.Data.Raster = DataSet
                'TryCopyGroup(IO.Path.GetFileNameWithoutExtension(FileName), IO.Path.GetFileNameWithoutExtension(aReclassifyGridName), atcUtility.modFile.TifExtensions)
                'If Not IO.File.Exists(FileName) Then
                '    lMyRaster.SaveAs(FileName)
                'End If
                'IO.File.Copy(FileName, aReclassifyGridName)
                'Dim lReclassifyGrid As DotSpatial.Data.Raster = DotSpatial.Data.Raster.OpenFile(aReclassifyGridName)
                'Dim lReclassifyGrid As New DotSpatial.Data.Raster(Of Byte)(lMyRaster.NumRows, lMyRaster.NumColumns)

                'Dim lReclassifyGrid As DotSpatial.Data.IRaster = DotSpatial.Data.Raster.CreateRaster(aReclassifyGridName, lMyRaster.DriverCode, lMyRaster.NumColumns, lMyRaster.NumRows, 1, GetType(Integer), lGridOptions)
                'lReclassifyGrid.Projection = lMyRaster.Projection
                'lReclassifyGrid.Xllcenter = lMyRaster.Xllcenter
                'lReclassifyGrid.Yllcenter = lMyRaster.Yllcenter
                Dim lReclassifyGrid = CreateSimilarRaster(aReclassifyGridName, GetType(Integer))
                Dim lReclassValue As Integer
                'Dim lGridStatistics As New atcCollection

                For lRow As Integer = 0 To lReclassifyGrid.NumRows - 1
                    For lCol As Integer = 0 To lReclassifyGrid.NumColumns - 1
                        Dim lGridValue As Double = lMyRaster.Value(lRow, lCol)
                        lReclassValue = -1
                        For lIndex As Integer = 0 To aReclassiflyScheme.Count - 1
                            If lGridValue <= aReclassiflyScheme(lIndex) Then
                                lReclassValue = lIndex
                                'lGridStatistics.Increment(lIndex)
                                Exit For
                            End If
                        Next
                        lReclassifyGrid.Value(lRow, lCol) = lReclassValue
                    Next
                Next
                lReclassifyGrid.Save()

                Layer.CopyProcStepsFromCachedFile(FileName, aReclassifyGridName)
                Dim lReclassDescription As String = "Reclassified Values"
                For Each lReclassLimit As Double In aReclassiflyScheme
                    lReclassDescription &= " " & DoubleToString(lReclassLimit)
                Next
                Layer.AddProcessStepToFile(lReclassDescription, aReclassifyGridName)

                Return New D4EM.Data.Layer(lReclassifyGrid, aNewSpecification)

            Catch lEx As ApplicationException
                Throw New ApplicationException("ReclassifyRangeExcepton " & lEx.Message, lEx)
            End Try
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Overlay with another feature layer to create a lookup table.
    ''' Each shape in the given layer is mapped to a shape in this layer.
    ''' The center of each shape's extents in the given layer is tested for which shape contains it in this layer.
    ''' The resulting table allows looking up an index in this layer from a key value from the given layer.
    ''' </summary>
    ''' <param name="aShapeFileSmallAreas"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function OverlayFeatures(ByVal aShapeFileSmallAreas As D4EM.Data.Layer) As atcUtility.atcCollection
        Dim lLookup As New atcUtility.atcCollection
        Dim lBig As DotSpatial.Data.FeatureSet = Me.DataSet
        Dim lSmall As DotSpatial.Data.FeatureSet = aShapeFileSmallAreas.DataSet
        Dim lSmallKeyFieldIndex As Integer = aShapeFileSmallAreas.FieldIndex(aShapeFileSmallAreas.Specification.IdFieldName)
        For Each lSmallFeature As DotSpatial.Data.IFeature In lSmall.Features
            Dim lCentroid As DotSpatial.Topology.IPoint = lSmallFeature.ToShape.ToGeometry.Centroid
            Dim lBigShapeIndex As Integer = 0
            For Each lMyFeature As DotSpatial.Data.Shape In lBig.Features
                If lMyFeature.ToGeometry.Contains(lCentroid) Then
                    lLookup.Add(lSmallFeature.DataRow(lSmallKeyFieldIndex), lBigShapeIndex)
                    Exit For
                End If
                lBigShapeIndex += 1
            Next
            If lBigShapeIndex >= lBig.Features.Count Then
                Logger.Dbg("Small shape centroid not found in big shapes " & lSmallFeature.DataRow(lSmallKeyFieldIndex))
            End If
        Next
        Return lLookup
    End Function

    ''' <summary>
    ''' Find a field (aka column) by case-insensitive search for the field name
    ''' </summary>
    ''' <param name="aFieldName">Name of field to search for</param>
    ''' <returns>Zero-based index of field if found, -1 if not found</returns>
    Public Function FieldIndex(ByVal aFieldName As String) As Integer
        If Not String.IsNullOrEmpty(aFieldName) Then
            Dim lFeatureSet As DotSpatial.Data.IFeatureSet = DataSet
            Dim lFieldName As String = aFieldName.ToLower
            Dim lFieldIndex As Integer = 0
            For Each lColumn As System.Data.DataColumn In lFeatureSet.GetColumns
                If lColumn.ColumnName.ToLower.Equals(lFieldName) Then
                    Return lFieldIndex
                End If
                lFieldIndex += 1
            Next
            Logger.Dbg("Field " & aFieldName & " not found in " & lFeatureSet.Filename)
        End If
        Return -1
    End Function

    ''' <summary>
    ''' Assign integers starting with one and incrementing to all features in the featureset
    ''' Create a new field if the named field does not yet exist
    ''' </summary>
    ''' <param name="aIndexFieldName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AssignIndexes(ByVal aIndexFieldName As String) As Integer
        Dim lFeatureSet As DotSpatial.Data.IFeatureSet = DataSet
        Dim lFieldIndex As Integer = FieldIndex(aIndexFieldName)

        If lFieldIndex < 0 Then 'need to add it
            lFieldIndex = lFeatureSet.DataTable.Columns.Count
            lFeatureSet.DataTable.Columns.Add(New System.Data.DataColumn(aIndexFieldName, GetType(Integer)))
        End If

        For lFeatureIndex As Integer = 1 To lFeatureSet.NumRows
            lFeatureSet.Features(lFeatureIndex - 1).DataRow(lFieldIndex) = lFeatureIndex
        Next

        'We don't really need to check this, just here for debugging
        'For lFeatureIndex As Integer = 1 To lFeatureSet.NumRows
        '    If lFeatureSet.Features(lFeatureIndex - 1).DataRow(lFieldIndex) <> lFeatureIndex Then
        '        Logger.Dbg("Failed to set " & aIndexFieldName & " for feature " & lFeatureIndex & " (was set to " & lFeatureSet.Features(lFeatureIndex - 1).DataRow(lFieldIndex) & ")")
        '    End If
        'Next

        Return lFieldIndex
    End Function

    Public Overrides Function ToString() As String
        If Not String.IsNullOrEmpty(FileName) Then
            Return FileName
        ElseIf Specification IsNot Nothing Then
            Return Specification.ToString
        Else
            Return "Empty Layer"
        End If
    End Function

    Public Function CreateSimilarRaster(ByVal aNewFilename As String,
                               Optional ByVal aDataType As Type = Nothing,
                               Optional ByVal aNumColumns As Integer = 0,
                               Optional ByVal aNumRows As Integer = 0) As DotSpatial.Data.Raster
        With AsRaster()
            If aDataType Is Nothing Then aDataType = .DataType
            If aNumColumns < 1 Then aNumColumns = .NumColumns
            If aNumRows < 1 Then aNumRows = .NumRows
            Dim lGridOptions() As String = {String.Empty}
            Dim lNewGrid = DotSpatial.Data.Raster.CreateRaster(aNewFilename, .DriverCode, aNumColumns, aNumRows, 1, aDataType, lGridOptions)
            lNewGrid.Projection = .Projection
            If aNumColumns = .NumColumns AndAlso aNumRows = .NumRows Then
                lNewGrid.Bounds = AsRaster.Bounds.Copy
            Else
                'TODO
            End If
            Return lNewGrid
        End With
    End Function

    ''' <summary>
    ''' Overlay a shape on this feature layer and return the ID of each shape that overlaps.
    ''' Example: return the list of county FIPS codes that overlap the shape.
    ''' </summary>
    ''' <param name="aShape">Select shapes that overlap this shape</param>
    ''' <param name="aIgnoreBelowFraction">Ignore an overlap if it is less than this fraction of both aShape and the overlapping shape (0.01 = 1%)</param>
    ''' <returns>List of values from IdFieldIndex of shapes overlapping aShape</returns>
    Public Overridable Function GetKeysOfOverlappingShapes(ByVal aShape As DotSpatial.Data.Shape, Optional ByVal aIgnoreBelowFraction As Double = 0.02) As Generic.List(Of String)
        Dim lKeys As New Generic.List(Of String)
        Dim lRegionGeometry As DotSpatial.Topology.Geometry = aShape.ToGeometry
        Dim lRegionArea As Double = lRegionGeometry.Area

        For Each lFeature As DotSpatial.Data.IFeature In AsFeatureSet.Features
            Dim lShape As DotSpatial.Data.Shape = lFeature.ToShape
            Dim lGeometry As DotSpatial.Topology.IGeometry = lShape.ToGeometry
            'If lGeometry.Intersects(lRegionGeometry) Then
            ''TODO: different intersection, maybe: If lShape.Range.Intersects(lRegionShape) Then
            '    If lRegionArea > 0 Then
            '        Try
            '            Dim lIntersection As DotSpatial.Topology.Geometry = lGeometry.Intersection(lRegionGeometry)
            '            If lIntersection IsNot Nothing AndAlso lIntersection.NumPoints > 0 Then
            '                Dim lIntersectionArea As Double = lIntersection.Area
            '                'If intersection area is at all significant to either the region or the feature, include it.
            '                'If not, this is probably not a real overlap and is caused by slight variation in boundary lines.
            '                If lIntersectionArea / lRegionArea > 0.01 OrElse lIntersectionArea / lGeometry.Area > 0.01 Then
            '                    lKeys.Add(lShape.Attributes(aKeyField))
            '                Else
            '                    Logger.Dbg("Insignificant intersection skipped with " & lShape.Attributes(aKeyField))
            '                End If
            '            End If
            '        Catch ex As Exception
            '            lKeys.Add(lShape.Attributes(aKeyField))
            '        End Try
            '    Else
            '        'If area could not be computed, use simple Intersects test instead of testing area of intersection.
            '        lKeys.Add(lShape.Attributes(aKeyField))
            '    End If
            'End If

            If lRegionArea > 0 Then
                Try
                    Dim lIntersection As DotSpatial.Topology.Geometry = lGeometry.Intersection(lRegionGeometry)
                    If lIntersection IsNot Nothing AndAlso lIntersection.NumPoints > 0 Then
                        Dim lIntersectionArea As Double = lIntersection.Area
                        'If intersection area is at all significant to either the region or the feature, include it.
                        'If not, this is probably not a real overlap and is caused by slight variation in boundary lines.
                        If lIntersectionArea / lRegionArea > aIgnoreBelowFraction OrElse lIntersectionArea / lGeometry.Area > aIgnoreBelowFraction Then
                            lKeys.Add(lShape.Attributes(IdFieldIndex))
                        Else
                            Logger.Dbg("Insignificant intersection skipped with " & lShape.Attributes(IdFieldIndex))
                        End If
                    End If
                Catch ex As Exception
                    If lGeometry.Overlaps(lRegionGeometry) Then
                        lKeys.Add(lShape.Attributes(IdFieldIndex))
                    End If
                End Try
            ElseIf lGeometry.Overlaps(lRegionGeometry) Then
                lKeys.Add(lShape.Attributes(IdFieldIndex))
            End If
        Next
        Return lKeys
    End Function

End Class