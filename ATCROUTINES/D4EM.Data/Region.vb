Imports atcUtility
Imports MapWinUtility
Imports D4EM.Data.LayerSpecification
Imports DotSpatial.Data

Public Class Region
    Implements IEquatable(Of Region)

    Public Class RegionTypes
        Public Shared box As New LayerSpecification(Name:="Bounding Box", Tag:="box")
        Public Shared polygon As New LayerSpecification(Name:="Polygon", Tag:="polygon")
        Public Shared catchment As New LayerSpecification(Name:="NHDPlus Catchment Polygons", FilePattern:="catchment.shp", Tag:="catchment", Role:=Roles.SubBasin, IdFieldName:="COMID")
        Public Shared closest As New LayerSpecification(Name:="Closest Station", Tag:="closest")

        Public Shared huc8 As LayerSpecification = National.LayerSpecifications.huc8
        Public Shared huc12 As LayerSpecification = National.LayerSpecifications.huc12
        Public Shared county As LayerSpecification = National.LayerSpecifications.county
        Public Shared state As LayerSpecification = National.LayerSpecifications.state
    End Class

    Public Shared AllRegionTypes() As LayerSpecification = {RegionTypes.box, RegionTypes.polygon, RegionTypes.catchment, RegionTypes.closest,
                                                            RegionTypes.huc8, RegionTypes.huc12, RegionTypes.county, RegionTypes.state}

    Private Shared pInitialized As Boolean = Globals.Initialize

    ''' <summary>
    ''' Set by creating region from a shape or by calling ToShape
    ''' </summary>
    ''' <remarks></remarks>
    Private pPolygon As DotSpatial.Data.Shape
    Private pPolygonProjection As DotSpatial.Projections.ProjectionInfo

    Private pNorth As Double = GetNaN()
    Private pSouth As Double = pNorth
    Private pWest As Double = pNorth
    Private pEast As Double = pNorth
    Private pProjection As DotSpatial.Projections.ProjectionInfo = Globals.GeographicProjection

    ''' <summary>
    ''' Keys of shapes from any number of layers that overlap this region
    ''' </summary>
    ''' <remarks>
    ''' Stored here when either:
    ''' 1. this is how the region was specified (in this case, LayerSpecification = pRegionSpecification)
    ''' 2. overlapping shapes have been found and are stored here
    ''' </remarks>
    Private pKeys As New Generic.Dictionary(Of LayerSpecification, Generic.List(Of String))

    Private pRegionSpecification As LayerSpecification = RegionTypes.box
    Private pRegionLayer As Layer = Nothing

    Public Shared MatchAllKeys() As String = {"*"}

    Public Sub New(ByVal aNorth As Double, _
                   ByVal aSouth As Double, _
                   ByVal aWest As Double, _
                   ByVal aEast As Double, _
                   ByVal aProjection As DotSpatial.Projections.ProjectionInfo)
        pRegionSpecification = RegionTypes.box
        pNorth = aNorth
        pSouth = aSouth
        pWest = aWest
        pEast = aEast
        pProjection = aProjection
        Validate()
    End Sub

    Public Sub New(ByVal aXML As Xml.XmlNode)
        SetXML(aXML)
        SetBoundsIfMissing()
        Validate()
    End Sub

    Public Sub New(ByVal aXML As String)
        Me.XML = aXML
    End Sub

    Public Sub New(ByVal aLayerSpecification As LayerSpecification, ByVal aKey As String)
        pRegionSpecification = aLayerSpecification
        AddKey(pRegionSpecification, aKey)
        SetBoundsFromKeys(pRegionSpecification)
    End Sub

    Public Sub New(ByVal aLayerSpecification As LayerSpecification, ByVal aKeys As IEnumerable)
        pRegionSpecification = aLayerSpecification
        SetKeys(aLayerSpecification, aKeys)
    End Sub

    Public Sub New(ByVal aRegionLayer As Layer, ByVal aKey As String)
        pRegionLayer = aRegionLayer
        pRegionSpecification = pRegionLayer.Specification
        AddKey(pRegionSpecification, aKey)
        SetBoundsFromKeys(pRegionSpecification)
    End Sub

    Public Sub New(ByVal aRegionLayer As Layer, ByVal aKeys As IEnumerable)
        pRegionLayer = aRegionLayer
        pRegionSpecification = pRegionLayer.Specification
        SetKeys(pRegionSpecification, aKeys)
    End Sub

    Public Sub New(ByVal aPolygon As DotSpatial.Data.Shape, _
                   ByVal aProjection As DotSpatial.Projections.ProjectionInfo)
        pRegionSpecification = RegionTypes.polygon
        pPolygon = aPolygon
        pPolygonProjection = aProjection
        SetBoundsFromEnvelope(aPolygon.ToGeometry.Envelope, aProjection)
    End Sub

    Public Overloads Function Equals(ByVal aCompareTo As Region) As Boolean Implements IEquatable(Of D4EM.Data.Region).Equals
        If ReferenceEquals(aCompareTo, Me) Then Return True
        With aCompareTo
            If Math.Abs(pNorth - .pNorth) > 0.000001 Then Return False
            If Math.Abs(pSouth - .pSouth) > 0.000001 Then Return False
            If Math.Abs(pEast - .pEast) > 0.000001 Then Return False
            If Math.Abs(pWest - .pWest) > 0.000001 Then Return False
            If (pRegionSpecification <> .RegionSpecification) Then Return False
        End With
        Return True
    End Function

    Private Sub SetXML(ByVal aXML As Xml.XmlNode)
        Dim lArg As Xml.XmlNode = aXML.FirstChild
        While Not lArg Is Nothing
            If lArg.Name.ToLower.Equals("region") Then
                SetXML(lArg)
            Else
                SetPrivate(lArg.Name, lArg.InnerText)
            End If
            lArg = lArg.NextSibling
        End While
    End Sub

    Private Sub SetPrivate(ByVal aPart As String, ByVal aNewValue As String)
        Select Case aPart.ToLower
            Case "northbc", "top" : If IsNumeric(aNewValue) Then pNorth = CDbl(aNewValue)
            Case "southbc", "bottom" : If IsNumeric(aNewValue) Then pSouth = CDbl(aNewValue)
            Case "westbc", "left" : If IsNumeric(aNewValue) Then pWest = CDbl(aNewValue)
            Case "eastbc", "right" : If IsNumeric(aNewValue) Then pEast = CDbl(aNewValue)
            Case "regionspecification", "preferredformat"
                pRegionSpecification = RegionSpecificationFromString(aNewValue)
            Case "projection", "boxprojection"
                pProjection = Globals.FromProj4(aNewValue)
            Case "polygon"
                If aNewValue.StartsWith("<") Then
                    Try
                        Dim lDeserializer As New DotSpatial.Serialization.XmlDeserializer
                        pPolygon = lDeserializer.Deserialize(Of DotSpatial.Data.Shape)(aNewValue)
                    Catch ex As Exception
                        Logger.Dbg("Unable to deserialize polygon: " & aNewValue)
                    End Try
                ElseIf IsNumeric(aNewValue) Then
                    Logger.Dbg("Region polygon is numeric, setting polygon key = " & aNewValue)
                    SetKeys(D4EM.Data.Region.RegionTypes.polygon, {aNewValue})
                Else
                    Logger.Dbg("Region polygon not XML: " & aNewValue)
                End If
            Case "polygonprojection"
                pPolygonProjection = Globals.FromProj4(aNewValue)

            Case Else
                Dim lSpecification As LayerSpecification = RegionSpecificationFromString(aPart)
                If lSpecification IsNot Nothing Then
                    AddKey(lSpecification, aNewValue)
                    If lSpecification = RegionTypes.huc12 Then
                        AddKey(RegionTypes.huc8, (SafeSubstring(aNewValue, 0, 8)))
                    End If
                End If
                'Case "huc8"
                '        AddKey(RegionTypes.huc8, aNewValue)
                'Case "huc12"
                '        AddKey(RegionTypes.huc12, aNewValue)
                '        AddKey(RegionTypes.huc8, (SafeSubstring(aNewValue, 0, 8)))
        End Select
    End Sub

    Public Property XML() As String
        Get
            Dim lPerimeterXML As String = "<northbc>" & pNorth & "</northbc>" & vbCrLf _
                                        & "<southbc>" & pSouth & "</southbc>" & vbCrLf _
                                        & "<eastbc>" & pEast & "</eastbc>" & vbCrLf _
                                        & "<westbc>" & pWest & "</westbc>" & vbCrLf

            Dim lPolygonXML As String = ""

            If pRegionSpecification = RegionTypes.polygon Then
                'Dim lSerializer As New DotSpatial.Serialization.XmlSerializer
                'lPerimeterXML = "<polygon>" & lSerializer.Serialize(pPolygon).Replace("<?xml version=""1.0"" encoding=""utf-16""?>", "") & "</polygon>" & vbCrLf _
                '    & "<polygonprojection>" & pPolygonProjection.ToProj4String & "</polygonprojection>" & vbCrLf
                'TODO: serialize in a way that we can reliably deserialize. DotSpatial.Serialization.XmlSerializer was not working.
                lPerimeterXML = "<polygon>" & pPolygon.ToGeometry.ToString & "</polygon>" & vbCrLf _
                    & "<polygonprojection>" & pPolygonProjection.ToProj4String & "</polygonprojection>" & vbCrLf

            End If

            Dim lKeysXML As String = ""
            For Each lKeyValue As Collections.Generic.KeyValuePair(Of LayerSpecification, Generic.List(Of String)) In pKeys
                Dim lTag As String = lKeyValue.Key.Tag
                If String.IsNullOrEmpty(lTag) Then lTag = lKeyValue.Key.Name.Replace(" ", "").ToLower
                For Each lKey As String In lKeyValue.Value
                    lKeysXML &= "<" & lTag & ">" & lKey & "</" & lTag & ">" & vbCrLf
                Next
            Next

            Dim lProjectionXML As String = ""
            If pProjection IsNot Nothing Then
                lProjectionXML = "<projection>" & pProjection.ToProj4String & "</projection>" & vbCrLf
            End If

            Return "<region>" & vbCrLf _
                 & "<regionspecification>" & pRegionSpecification.Tag & "</regionspecification>" & vbCrLf _
                 & lPolygonXML _
                 & lPerimeterXML _
                 & lKeysXML _
                 & lProjectionXML _
                 & "</region>" & vbCrLf
        End Get
        Set(ByVal value As String)
            Dim lQuery As New Xml.XmlDocument
            lQuery.LoadXml(value)
            SetXML(lQuery.FirstChild)
            SetBoundsIfMissing()
            Validate()
        End Set
    End Property

    Private Shared Function RegionSpecificationFromString(ByVal aSpecificationName As String) As LayerSpecification
        Dim lSpecificationName As String = aSpecificationName.ToLower.Trim
        For Each lRegionType As LayerSpecification In AllRegionTypes
            If lSpecificationName.Equals(lRegionType.Tag) OrElse
               lSpecificationName.Equals(lRegionType.Name.ToLower) OrElse
               lSpecificationName.Equals(lRegionType.Name.Replace(" ", "").ToLower) Then
                Return lRegionType
            End If
        Next
        Logger.Dbg("Could not determine RegionSpecificationFromString, defaulting to box for '" & aSpecificationName & "'")
        Return RegionTypes.box
    End Function

    Public Overridable Function Validate() As Boolean
        If Double.IsNaN(pNorth) Then
            Throw New ApplicationException("North not specified")
        ElseIf Double.IsNaN(pSouth) Then
            Throw New ApplicationException("South not specified")
        ElseIf Double.IsNaN(pWest) Then
            Throw New ApplicationException("West not specified")
        ElseIf Double.IsNaN(pEast) Then
            Throw New ApplicationException("East not specified")
        ElseIf pProjection Is Nothing Then
            Throw New ApplicationException("Projection not specified")
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Overlay this region on a shape file and returns a value from the
    ''' shape table for each shape that overlaps this region.
    ''' Example: return the list of county FIPS codes that overlap the region.
    ''' </summary>
    ''' <param name="aSelectFromLayer">Select shapes from this layer</param>
    ''' <returns>List of values from aKeyField of shapes overlapping this Region</returns>
    Public Overridable Function GetKeysOfOverlappingShapes(ByVal aSelectFromLayer As D4EM.Data.Layer) As Generic.List(Of String)
        Return aSelectFromLayer.GetKeysOfOverlappingShapes(Me.ToShape(aSelectFromLayer.AsFeatureSet.Projection))
    End Function

    Public Property RegionSpecification() As LayerSpecification
        Get
            Return pRegionSpecification
        End Get
        Set(ByVal value As LayerSpecification)
            pRegionSpecification = value
        End Set
    End Property

    Private Sub AddKey(ByVal aLayerSpecification As LayerSpecification, ByVal aKey As String)
        If Not String.IsNullOrWhiteSpace(aKey) Then
            Dim lKeys As Generic.List(Of String) = CurrentKeys(aLayerSpecification)
            If Not lKeys.Contains(aKey) Then
                lKeys.Add(aKey)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Return the current list of keys of the given type.
    ''' If no list of keys of that type exists yet, create an empty list.
    ''' </summary>
    ''' <param name="aKeyType">Type of keys to return - Commonly used types are in Region.RegionTypes</param>
    Private Function CurrentKeys(ByVal aKeyType As LayerSpecification) As Generic.List(Of String)
        If Not pKeys.ContainsKey(aKeyType) Then pKeys.Add(aKeyType, New Generic.List(Of String))
        Return pKeys.Item(aKeyType)
    End Function

    ''' <summary>
    ''' Set a known list of keys. When keys are known, this makes GetKeys faster.
    ''' </summary>
    ''' <param name="aKeyType">Type of keys being set - Commonly used types are in Region.RegionTypes</param>
    ''' <param name="aKeys">List of keys to set. If items are not String then .ToString is used as key</param>
    Public Sub SetKeys(ByVal aKeyType As LayerSpecification, ByVal aKeys As IEnumerable)
        Dim lKeys As Generic.List(Of String) = CurrentKeys(aKeyType)
        lKeys.Clear()
        For Each lKey As Object In aKeys
            If Not String.IsNullOrWhiteSpace(lKey) Then
                Dim lKeyString As String = lKey.ToString
                If Not lKeys.Contains(lKeyString) Then
                    lKeys.Add(lKeyString)
                End If
            End If
        Next
        If aKeyType = pRegionSpecification Then
            SetBoundsFromKeys(aKeyType)
        End If
    End Sub

    ''' <summary>
    ''' Get the list of keys of the given type. If no keys already exist, try to compute by GIS overlay.
    ''' </summary>
    ''' <param name="aKeyType">Type of keys to return - Commonly used types are in Region.RegionTypes</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetKeys(ByVal aKeyType As LayerSpecification) As Generic.List(Of String)
        Dim lKeys As Generic.List(Of String) = Nothing
        Try
            lKeys = CurrentKeys(aKeyType)
            If lKeys.Count = 0 Then
                Dim lLayerFilenames As New Generic.List(Of String)
                Select Case aKeyType
                    Case RegionTypes.huc8
                        If CurrentKeys(RegionTypes.huc12).Count > 0 Then 'can get HUC-8s from HUC-12s
                            For Each lHuc12 As String In CurrentKeys(RegionTypes.huc12)
                                Dim lHuc8 As String = lHuc12.Substring(0, 8)
                                If Not lKeys.Contains(lHuc8) Then lKeys.Add(lHuc8)
                            Next
                        Else
                            lLayerFilenames.Add(National.ShapeFilename(aKeyType))
                        End If
                    Case RegionTypes.huc12
                        Dim lHuc8Folder As String = IO.Path.GetDirectoryName(National.ShapeFilename(RegionTypes.huc8))
                        For Each lHuc8 In GetKeys(RegionTypes.huc8)
                            lLayerFilenames.Add(IO.Path.Combine(lHuc8Folder, "huc12", lHuc8, "huc12.shp"))
                        Next
                    Case Else
                        lLayerFilenames.Add(National.ShapeFilename(aKeyType))
                End Select
                For Each lLayerFilename In lLayerFilenames
                    If IO.File.Exists(lLayerFilename) Then
                        lKeys.AddRange(GetKeysOfOverlappingShapes(New D4EM.Data.Layer(lLayerFilename, aKeyType)))
                        Logger.Dbg("Found " & lKeys.Count & " " & aKeyType.Name, True)
                    Else
                        Logger.Dbg(aKeyType.Name & " layer not found.")
                    End If
                Next
            End If
        Catch e As Exception
            Logger.Dbg("Exception getting keys of region: " & e.Message)
        End Try
        Return lKeys
    End Function

    ''' <summary>
    ''' Clip a grid file to this region, save clipped grid in given file
    ''' </summary>
    ''' <param name="aGridFilename">Original grid to clip</param>
    ''' <param name="aClippedFilename">Save clipped grid as</param>
    ''' <param name="aGridProjection">Optional: specify projection of original grid. If not specified, try to determine grid projection automatically.</param>
    ''' <remarks>aGridProjection is available for cases where grid projection is not available with the file
    ''' TODO: change to using Layer object for incoming grid and returning Layer as function
    ''' </remarks>
    Public Overridable Sub ClipGrid(ByVal aGridFilename As String, ByVal aClippedFilename As String, _
                           Optional ByVal aGridProjection As DotSpatial.Projections.ProjectionInfo = Nothing)
        Dim lGridToClip As DotSpatial.Data.IRaster = DotSpatial.Data.Raster.OpenFile(aGridFilename, False)
        If lGridToClip.Projection Is Nothing Then lGridToClip.Projection = aGridProjection

        'Dim lClipPolygon As New DotSpatial.Data.Feature
        'Dim lClipGeometry As DotSpatial.Topology.IBasicGeometry = Me.ToShape(aGridProjection).ToGeometry
        'lClipPolygon.BasicGeometry = lClipGeometry

        'Dim lGridAfterClip As DotSpatial.Data.IRaster = DotSpatial.Analysis.ClipRaster.ClipRasterWithPolygon(lClipPolygon, lGridToClip, aClippedFilename)

        Dim lGridAfterClip = ClipGrid(lGridToClip, aClippedFilename)

        If lGridAfterClip Is Nothing Then
            Throw New ApplicationException("ClipGrid Failed to clip " & aGridFilename)
        End If
        'If Not IO.Directory.Exists(aClippedFilename) Then lGridAfterClip.SaveAs(aClippedFilename)
        'Layer.CopyProcStepsFromCachedFile(aGridFilename, aClippedFilename)
        'Layer.AddProcessStepToFile("Clipped to region " & Me.ToString, aClippedFilename)
    End Sub

    ''' <summary>
    ''' Clip a grid file to this region, save clipped grid in given file
    ''' </summary>
    ''' <param name="aRaster">The input raster.</param>
    ''' <param name="aClippedFilename">Save clipped grid as</param>
    ''' <returns>clipped IRaster</returns>
    Public Function ClipGrid(ByVal aRaster As IRaster, ByVal aClippedFilename As String) As DotSpatial.Data.IRaster
        If aRaster Is Nothing Then
            Return Nothing
        End If
        Globals.RepairAlbers(aRaster.Projection)

        'Dim output As DotSpatial.Data.IRaster = DotSpatial.Analysis.ClipRaster.ClipRasterWithPolygon(Me.ToShape(aRaster.Projection), aRaster, aClippedFilename)

        Dim lClipPolygon As New DotSpatial.Data.Feature
        Dim lClipGeometry As DotSpatial.Topology.IBasicGeometry = Me.ToShape(aRaster.Projection).ToGeometry
        lClipPolygon.BasicGeometry = lClipGeometry

        Dim cellWidth As Double = aRaster.CellWidth
        Dim cellHeight As Double = aRaster.CellHeight
        Dim lPolyExtent = lClipPolygon.Envelope.ToExtent
        Dim SharedExtent As DotSpatial.Data.Extent = aRaster.Bounds.Extent.Intersection(lPolyExtent)

        Dim lNewMinRow As Integer, lNewMinColumn As Integer
        Dim lNewMaxRow As Integer, lNewMaxColumn As Integer
        D4EM.Data.Layer.CellBoundsRaster(aRaster, SharedExtent.MinX, SharedExtent.MaxY, SharedExtent.MaxX, SharedExtent.MinY,
                                         lNewMinRow, lNewMinColumn, lNewMaxRow, lNewMaxColumn)

        Dim lNewMinCoord = DotSpatial.Data.RasterBoundsExt.CellBottomLeft_ToProj(aRaster.Bounds, lNewMaxRow, lNewMinColumn)
        Dim lNewMaxCoord = DotSpatial.Data.RasterBoundsExt.CellTopRight_ToProj(aRaster.Bounds, lNewMinRow, lNewMaxColumn)

        Dim lNewNumCols As Integer = lNewMaxColumn - lNewMinColumn + 1
        Dim lNewNumRows As Integer = lNewMaxRow - lNewMinRow + 1

        'create output raster
        Dim output As DotSpatial.Data.IRaster
        output = DotSpatial.Data.Raster.Create(aClippedFilename, aRaster.DriverCode, lNewNumCols, lNewNumRows, 1, aRaster.DataType, New String() {""})
        output.NoDataValue = aRaster.NoDataValue
        output.Bounds = New DotSpatial.Data.RasterBounds(lNewNumRows, lNewNumCols, New DotSpatial.Data.Extent(lNewMinCoord.X, lNewMinCoord.Y, lNewMaxCoord.X, lNewMaxCoord.Y))
        output.Projection = aRaster.Projection
        output.NoDataValue = aRaster.NoDataValue

        Dim previous As Integer = 0

        'Dim lRange As New ShapeRange(Me.GetExtent(Me.pProjection))

        Dim lLastNewRow As Integer = (output.Bounds.NumRows - 1)
        Dim lLastNewColumn As Integer = (output.Bounds.NumColumns - 1)
        For lNewRow As Integer = 0 To lLastNewRow
            For lNewColumn As Integer = 0 To lLastNewColumn
                'Commented out slow and possibly buggy test for inside polygon so we are just clipping to extent
                'Dim cellCenter As DotSpatial.Topology.Coordinate = output.CellToProj(lNewRow, lNewColumn)
                'If lClipPolygon.Intersects(cellCenter) Then 'lRange.Intersects(cellCenter) Then '
                output.Value(lNewRow, lNewColumn) = aRaster.Value(lNewRow + lNewMinRow, lNewColumn + lNewMinColumn)
                'Else
                '    output.Value(lNewRow, lNewColumn) = output.NoDataValue
                'End If
            Next
            Logger.Progress(lNewRow, lLastNewRow)
        Next

        output.GetStatistics()
        output.Save()
        Layer.CopyProcStepsFromCachedFile(aRaster.Filename, aClippedFilename)
        Layer.AddProcessStepToFile("Clipped to region " & Me.ToString, aClippedFilename)
        Return output
    End Function

    ''' <summary>
    ''' Save the set of shapes from aFeatures which overlap the Region into aClipFilename
    ''' </summary>
    ''' <param name="aFeatures">features to search</param>
    ''' <param name="aClipFilename">save overlapping shapes here</param>
    ''' <returns>New Layer containing only the features from aFeatures that overlap this region</returns>
    Public Overridable Function SelectShapes(ByVal aFeatures As D4EM.Data.Layer, ByVal aClipFilename As String) As D4EM.Data.Layer
        TryCopyShapefile(aFeatures.FileName, aClipFilename)
        Dim lSelectedLayer As New D4EM.Data.Layer(aClipFilename, aFeatures.Specification)
        Dim lDestinationFeatureSet As DotSpatial.Data.FeatureSet = lSelectedLayer.AsFeatureSet
        If lDestinationFeatureSet Is Nothing OrElse lDestinationFeatureSet.Features.Count = 0 Then
            Logger.Dbg("Empty FeatureSet, not selecting to " & aClipFilename)
            Return Nothing
        Else
            Dim lRegionGeometry As DotSpatial.Topology.Geometry = Me.ToShape(lDestinationFeatureSet.Projection).ToGeometry
            Dim lFeature As DotSpatial.Data.IFeature
            For lIndex As Integer = lDestinationFeatureSet.Features.Count - 1 To 0 Step -1
                lFeature = lDestinationFeatureSet.Features(lIndex)
                Dim lGeometry As DotSpatial.Topology.IGeometry = lFeature.BasicGeometry
                If Not lGeometry.Intersects(lRegionGeometry) Then
                    lDestinationFeatureSet.Features.RemoveAt(lIndex)
                End If
            Next
            lDestinationFeatureSet.Save()
            Return lSelectedLayer
        End If
    End Function

    Private Function DictHuc8_12(ByVal aHuc12s As Generic.List(Of String)) As System.Collections.Generic.Dictionary(Of String, Generic.List(Of String))
        'Group HUC-12s by HUC8, then for each HUC8 expand the bounds to accomodate all the HUC-12 in that HUC-8
        Dim lDictHUC8s As New System.Collections.Generic.Dictionary(Of String, Generic.List(Of String))
        For Each lHuc12 In aHuc12s
            Dim lHUC8 As String = lHuc12.Substring(0, 8)
            If (Not lDictHUC8s.ContainsKey(lHUC8)) Then
                lDictHUC8s.Add(lHUC8, New Generic.List(Of String))
            End If
            lDictHUC8s(lHUC8).Add(lHuc12)
        Next
        Return lDictHUC8s
    End Function

    ''' <summary>Region as a polygon shape</summary>
    ''' <param name="aNewProjection">Projection for returned shape</param>
    ''' <returns>Polygon shape</returns>
    ''' <remarks>In some cases the shape will be a simple bounding box, in others it will be a more complex shape.</remarks>
    Public Overridable Function ToShape(ByVal aNewProjection As DotSpatial.Projections.ProjectionInfo) As DotSpatial.Data.Shape
        If pPolygon Is Nothing Then
            If pRegionSpecification <> RegionTypes.box Then
                Try
                    Dim lKeys As Generic.List(Of String) = CurrentKeys(RegionSpecification)
                    If lKeys.Count > 0 Then
                        'TODO: shapes in national layer are not very detailed, use more detailed version when available, e.g. cat.shp from BASINS huc-8 download

                        Dim lShapeFilename As String
                        Dim lSelectFromLayer As DotSpatial.Data.FeatureSet

                        If pRegionLayer IsNot Nothing Then
                            lSelectFromLayer = pRegionLayer.AsFeatureSet
                            If lSelectFromLayer IsNot Nothing Then
                                ExpandPolygon(lSelectFromLayer, lKeys)
                            End If
                        ElseIf pRegionSpecification = National.LayerSpecifications.huc12 Then
                            Dim lNationalFolder As String = IO.Path.GetDirectoryName(National.ShapeFilename(National.LayerSpecifications.huc8))
                            Dim lDictHUC8s As System.Collections.Generic.Dictionary(Of String, Generic.List(Of String)) = DictHuc8_12(lKeys)
                            For Each lHuc8 In lDictHUC8s
                                lShapeFilename = IO.Path.Combine(lNationalFolder, "huc12", lHuc8.Key, "huc12.shp")
                                lSelectFromLayer = DotSpatial.Data.FeatureSet.OpenFile(lShapeFilename)
                                If lSelectFromLayer IsNot Nothing Then
                                    ExpandPolygon(lSelectFromLayer, lHuc8.Value)

                                    If lDictHUC8s.Count = 1 Then
                                        pRegionLayer = New D4EM.Data.Layer(lSelectFromLayer, pRegionSpecification)
                                    Else
                                        lSelectFromLayer.Close()
                                    End If
                                End If
                            Next
                        Else
                            lShapeFilename = National.ShapeFilename(RegionSpecification)
                            If IO.File.Exists(lShapeFilename) Then
                                lSelectFromLayer = DotSpatial.Data.FeatureSet.OpenFile(lShapeFilename)
                                If lSelectFromLayer IsNot Nothing Then
                                    pRegionLayer = New D4EM.Data.Layer(lSelectFromLayer, pRegionSpecification)
                                End If
                                ExpandPolygon(lSelectFromLayer, lKeys)
                            End If
                        End If
                    End If
                Catch e As Exception
                    Logger.Dbg("Could not get exact shape from region, building from bounding box instead.")
                    pPolygon = Nothing
                End Try
            End If
            If pPolygon Is Nothing Then
                pPolygon = New DotSpatial.Data.Shape(GetExtent(aNewProjection))
                pPolygonProjection = aNewProjection
            End If
        End If

        If aNewProjection.Equals(pPolygonProjection) Then
            Return pPolygon
        Else
            Dim lProjectedPolygon As DotSpatial.Data.Shape = pPolygon.Clone
            DotSpatial.Projections.Reproject.ReprojectPoints(lProjectedPolygon.Vertices, Nothing, pPolygonProjection, aNewProjection, 0, pPolygon.Vertices.Length / 2)
            Return lProjectedPolygon
        End If
    End Function

    Private Sub ExpandPolygon(ByVal lSelectFromLayer As DotSpatial.Data.FeatureSet, ByVal lKeys As Generic.List(Of String))
        If lSelectFromLayer IsNot Nothing Then
            Dim lSearchDBF As New atcTableDBF
            If lSearchDBF.OpenFile(IO.Path.ChangeExtension(lSelectFromLayer.Filename, "dbf")) Then
                Dim lKeyField As Integer = lSearchDBF.FieldNumber(RegionSpecification.IdFieldName)
                If lKeyField > 0 Then
                    For Each lKey As String In lKeys
                        If lSearchDBF.FindFirst(lKeyField, lKey) Then
                            If pPolygon Is Nothing Then
                                pPolygon = lSelectFromLayer.Features(lSearchDBF.CurrentRecord - 1).ToShape
                            Else
                                pPolygon.AddPart(lSelectFromLayer.Features(lSearchDBF.CurrentRecord - 1).Coordinates, lSelectFromLayer.CoordinateType)
                            End If
                            pPolygonProjection = lSelectFromLayer.Projection
                            If pPolygonProjection Is Nothing Then Throw New ApplicationException("Projection not found for '" & lSelectFromLayer.Filename & "'")
                            While lSearchDBF.FindNext(lKeyField, lKey)
                                pPolygon.AddPart(lSelectFromLayer.Features(lSearchDBF.CurrentRecord - 1).Coordinates, lSelectFromLayer.CoordinateType)
                            End While
                        End If
                    Next
                    lSelectFromLayer.Close()
                End If
                lSearchDBF.Clear()
            End If
        End If

    End Sub

    ''' <summary>
    ''' Get the bounding box of the region as a DotSpatial Extent
    ''' </summary>
    ''' <param name="aNewProjection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetExtent(ByVal aNewProjection As DotSpatial.Projections.ProjectionInfo) As DotSpatial.Data.Extent
        Dim lNorth As Double
        Dim lSouth As Double
        Dim lWest As Double
        Dim lEast As Double
        Me.GetBounds(lNorth, lSouth, lWest, lEast, aNewProjection)

        Return New DotSpatial.Data.Extent(lWest, lSouth, lEast, lNorth)
    End Function

    ''' <summary>
    ''' Get the bounding box of the region (North, South, East, West edges)
    ''' </summary>
    ''' <param name="aNorth"></param>
    ''' <param name="aSouth"></param>
    ''' <param name="aWest"></param>
    ''' <param name="aEast"></param>
    ''' <param name="aNewProjection">If specified, use aNewProjection for the bounding box. If not specified, use the region's native projection</param>
    ''' <remarks>Reprojection is designed to always include at least the original area. A larger area may need to be returned to make it rectangular in aNewProjection.</remarks>
    Public Overridable Sub GetBounds(ByRef aNorth As Double, _
                                     ByRef aSouth As Double, _
                                     ByRef aWest As Double, _
                                     ByRef aEast As Double, _
                            Optional ByVal aNewProjection As DotSpatial.Projections.ProjectionInfo = Nothing)
        aNorth = pNorth
        aSouth = pSouth
        aWest = pWest
        aEast = pEast

        If aNewProjection IsNot Nothing AndAlso Not aNewProjection.Equals(pProjection) Then
            Dim lBounds As Double() = {pWest, pNorth, pEast, pNorth, pWest, pSouth, pEast, pSouth}

            DotSpatial.Projections.Reproject.ReprojectPoints(lBounds, Nothing, pProjection, aNewProjection, 0, 4)

            aNorth = Math.Max(lBounds(1), lBounds(3))
            aSouth = Math.Min(lBounds(5), lBounds(7))
            aEast = Math.Max(lBounds(2), lBounds(6))
            aWest = Math.Min(lBounds(0), lBounds(4))
        End If
    End Sub

    Private Sub SetBoundsIfMissing()
        If Double.IsNaN(pNorth) Then
            If pRegionSpecification = RegionTypes.polygon AndAlso pPolygon IsNot Nothing AndAlso pPolygonProjection IsNot Nothing Then
                SetBoundsFromEnvelope(pPolygon.ToGeometry.Envelope, pPolygonProjection)
            ElseIf pKeys.Count > 0 Then
                SetBoundsFromKeys(pRegionSpecification)
            End If
        End If
    End Sub

    Private Sub SetBoundsFromEnvelope(ByVal aEnvelope As DotSpatial.Topology.Envelope, _
                                      ByVal aProjection As DotSpatial.Projections.ProjectionInfo)
        With aEnvelope
            pNorth = .Maximum.Y
            pSouth = .Minimum.Y
            pWest = .Minimum.X
            pEast = .Maximum.X
        End With
        pProjection = aProjection
    End Sub

    ''' <summary>
    ''' Set the bounds (North, South, East, West) and projection from the shapes of the given keys of the specified layer
    ''' </summary>
    ''' <param name="aKeyType"></param>
    ''' <remarks>Will also set pPolygon and pPolygonProjection if one shape is found</remarks>
    Private Sub SetBoundsFromKeys(ByVal aKeyType As LayerSpecification)
        Dim lKeys As Generic.List(Of String) = GetKeys(aKeyType)
        If lKeys.Count > 0 Then
            If pRegionLayer IsNot Nothing AndAlso aKeyType = pRegionLayer.Specification Then
                SetBoundsFromKeys(pRegionLayer.AsFeatureSet, aKeyType.IdFieldName, lKeys)
            Else
                Dim lShapeFilename As String
                Dim lSelectFromLayer As DotSpatial.Data.FeatureSet
                If aKeyType = National.LayerSpecifications.huc12 Then
                    Dim lNationalFolder As String = IO.Path.GetDirectoryName(National.ShapeFilename(National.LayerSpecifications.huc8))
                    If IO.Directory.Exists(lNationalFolder) Then
                        'Group HUC-12s by HUC8, then for each HUC8 expand the bounds to accomodate all the HUC-12 in that HUC-8
                        Dim lDictHUC8s As System.Collections.Generic.Dictionary(Of String, Generic.List(Of String)) = DictHuc8_12(lKeys)

                        For Each lkvPair In lDictHUC8s
                            lShapeFilename = IO.Path.Combine(lNationalFolder, "huc12", lkvPair.Key, "huc12.shp")
                            lSelectFromLayer = DotSpatial.Data.FeatureSet.OpenFile(lShapeFilename)
                            SetBoundsFromKeys(lSelectFromLayer, aKeyType.IdFieldName, lkvPair.Value)
                            If aKeyType = pRegionSpecification AndAlso lDictHUC8s.Count = 1 Then
                                pRegionLayer = New D4EM.Data.Layer(lSelectFromLayer, aKeyType)
                            Else
                                lSelectFromLayer.Close()
                            End If
                        Next
                    End If
                Else
                    lShapeFilename = National.ShapeFilename(aKeyType)
                    If IO.File.Exists(lShapeFilename) Then
                        lSelectFromLayer = DotSpatial.Data.FeatureSet.OpenFile(lShapeFilename)
                        SetBoundsFromKeys(lSelectFromLayer, aKeyType.IdFieldName, lKeys)
                        lSelectFromLayer.Close()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub SetBoundsFromKeys(ByVal aSelectFromLayer As DotSpatial.Data.FeatureSet, ByVal aKeyFieldName As String, ByVal aKeys As Generic.List(Of String))
        Dim lMatchAll As Boolean = (aKeys.Count = 1 AndAlso aKeys(0) = MatchAllKeys(0))
        ClearBounds()
        pProjection = aSelectFromLayer.Projection
        If lMatchAll Then
            Dim lSetPolygon As Boolean = (aSelectFromLayer.Features.Count = 1)
            For Each lFeature In aSelectFromLayer.Features
                ExpandBounds(lFeature, lSetPolygon)
            Next
        Else
            Dim lSearchDBF As New atcTableDBF
            If lSearchDBF.OpenFile(IO.Path.ChangeExtension(aSelectFromLayer.Filename, "dbf")) Then
                Dim lKeyField As Integer = lSearchDBF.FieldNumber(aKeyFieldName)
                If lKeyField > 0 Then
                    ClearBounds()
                    pProjection = aSelectFromLayer.Projection
                    Dim lSetPolygon As Boolean = (aKeys.Count = 1)
                    For Each lKey As String In aKeys
                        If lSearchDBF.FindFirst(lKeyField, lKey) Then
                            ExpandBounds(aSelectFromLayer.Features(lSearchDBF.CurrentRecord - 1), lSetPolygon)
                        End If
                    Next
                End If
                lSearchDBF.Clear()
            End If
        End If
    End Sub

    Private Sub ClearBounds()
        pNorth = GetNaN()
        pSouth = pNorth
        pWest = pNorth
        pEast = pNorth
    End Sub

    Private Sub ExpandBounds(ByVal aFeature As DotSpatial.Data.Feature, ByVal aSetPolygon As Boolean)
        With aFeature.Envelope
            If Double.IsNaN(pNorth) OrElse .Y > pNorth Then pNorth = .Y
            If Double.IsNaN(pSouth) OrElse .Y - .Height < pSouth Then pSouth = .Y - .Height
            If Double.IsNaN(pWest) OrElse .X > pWest Then pWest = .X
            If Double.IsNaN(pEast) OrElse .X + .Width > pEast Then pEast = .X + .Width
            If aSetPolygon Then
                pPolygon = aFeature.ToShape
                pPolygonProjection = pProjection
            End If
        End With
    End Sub

    Public Overrides Function ToString() As String
        Select Case pRegionSpecification
            Case RegionTypes.closest
                Return "Closest To Y= " & DoubleToString((pNorth + pSouth) / 2) & " X= " & DoubleToString((pEast + pWest) / 2)

            Case RegionTypes.county, RegionTypes.huc12, RegionTypes.huc8, RegionTypes.state
                Return pRegionSpecification.Name & "=" & String.Join(" ", CurrentKeys(pRegionSpecification).ToArray())

            Case Else 'RegionTypes.box or other
                'If pRegionLayer Is Nothing OrElse String.IsNullOrEmpty(pRegionLayer.ToString) Then
                Return pRegionSpecification.Name & " N " & DoubleToString(pNorth) & " S " & DoubleToString(pSouth) & " W " & DoubleToString(pWest) & " E " & DoubleToString(pEast)
                'Else
                '    Dim lKeys As String = ""
                '    Dim lSpecKeys As Generic.List(Of String) = CurrentKeys(pRegionSpecification)
                '    If lSpecKeys IsNot Nothing AndAlso lSpecKeys.Count > 0 Then
                '        If (lSpecKeys.Count = 1 AndAlso lSpecKeys(0) = MatchAllKeys(0)) Then
                '            lKeys = "(all)"
                '        ElseIf lSpecKeys IsNot Nothing AndAlso lSpecKeys.Count > 0 Then
                '            lKeys = "(" & String.Join(",", lSpecKeys) & ")"
                '        End If
                '    End If
                '    Return pRegionSpecification.Name & " " & pRegionLayer.ToString() & lKeys
                'End If
        End Select
    End Function

    Public Overrides Function Equals(ByVal aRegionObject As Object) As Boolean
        Try
            Dim lRegion As Region = CType(aRegionObject, Region)
            Dim lEast As Double, lWest As Double, lNorth As Double, lSouth As Double
            lRegion.GetBounds(lNorth, lSouth, lWest, lEast, pProjection)
            If Math.Abs(lNorth - pNorth) > 0.0001 Then Return False
            If Math.Abs(lSouth - pSouth) > 0.0001 Then Return False
            If Math.Abs(lWest - pWest) > 0.0001 Then Return False
            If Math.Abs(lEast - pEast) > 0.0001 Then Return False
            Return True
        Catch
            Return False
        End Try
    End Function
End Class
