''' <summary>
''' Project contains properties, layers, and timeseries data
''' </summary>
Public Class Project

    Public DesiredProjection As DotSpatial.Projections.ProjectionInfo
    Public CacheFolder As String
    Public ProjectFilename As String
    Public ProjectFolder As String
    Public Region As Region
    Public Clip As Boolean
    Public Merge As Boolean
    Public GetEvenIfCached As Boolean
    Public CacheOnly As Boolean

    Public Layers As New System.Collections.ObjectModel.ObservableCollection(Of Layer)

    Public TimeseriesSources As New Generic.List(Of atcData.atcTimeseriesSource)

    ''' <summary>Create a new Project and specify several of its properties</summary>
    ''' <param name="aDesiredProjection">PROJ.4 string describing desired projection</param>
    ''' <param name="aCacheFolder">Folder to cache raw downloaded data in</param>
    ''' <param name="aProjectFolder">Save unpacked data in this folder</param>
    ''' <param name="aRegion">Region to download</param>
    ''' <param name="aClip">True to clip downloaded data to aRegion</param>
    ''' <param name="aMerge">True to merge data downloaded from different HUC8, False to create separate HUC8 folders</param>
    ''' <param name="aGetEvenIfCached">True to retrieve new data even if it is already in the cache</param>
    ''' <param name="aCacheOnly">True to only download to cache</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal aDesiredProjection As DotSpatial.Projections.ProjectionInfo,
                   ByVal aCacheFolder As String,
                   ByVal aProjectFolder As String,
                   ByVal aRegion As Region,
                   ByVal aClip As Boolean,
                   ByVal aMerge As Boolean,
          Optional ByVal aGetEvenIfCached As Boolean = False,
          Optional ByVal aCacheOnly As Boolean = False)
        DesiredProjection = aDesiredProjection

        ProjectFolder = aProjectFolder
        If String.IsNullOrEmpty(aCacheFolder) Then
            CacheFolder = IO.Path.GetTempPath
        Else
            CacheFolder = aCacheFolder
        End If
        Region = aRegion
        Clip = aClip
        Merge = aMerge
        GetEvenIfCached = aGetEvenIfCached
        CacheOnly = aCacheOnly
    End Sub

    Public Sub New(ByVal aXML As String)
        Me.XML = aXML
        If String.IsNullOrEmpty(CacheFolder) Then
            CacheFolder = IO.Path.GetTempPath
        End If
    End Sub

    Public Function LayerFromFileName(ByVal aFileName As String) As Layer
        For Each lLayer As Layer In Layers
            If lLayer.FileName.ToLower.Equals(aFileName.ToLower) Then
                Return lLayer
            End If
        Next
        Return Nothing
    End Function

    Public Function LayerFromTag(ByVal aTag As String) As Layer
        For Each lLayer As Layer In Layers
            If lLayer.Specification.Tag.Equals(aTag) Then
                Return lLayer
            End If
        Next
        Return Nothing
    End Function

    Public Function LayerFromRole(ByVal aRole As LayerSpecification.Roles) As Layer
        For Each lLayer As Layer In Layers
            If lLayer.Specification.Role.Equals(aRole) Then
                Return lLayer
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Get a TimeseriesSource that is already open by its specification (file name)
    ''' </summary>
    ''' <param name="aSpecification">The file name for file-based data sources</param>
    ''' <returns>the data source if it is already open, Nothing if it is not open</returns>
    ''' <remarks>
    ''' Searching for a match is not case sensitive but it can be fooled if two different specifications refer to the same file
    ''' Always use consistent full path of files
    ''' Use OpenDataSource to open one that is not yet open
    ''' </remarks>
    Public Function TimeseriesSourceBySpecification(ByVal aSpecification As String) As atcData.atcTimeseriesSource
        Dim lSource As atcData.atcTimeseriesSource
        If TimeseriesSources IsNot Nothing Then
            aSpecification = aSpecification.ToLower
            For Each lSource In TimeseriesSources
                If lSource.Specification.ToLower.Equals(aSpecification) Then
                    Return lSource
                End If
            Next
        End If
        'If we did not find it in this project, also check atcDataManager for already-open data
        lSource = atcData.atcDataManager.DataSourceBySpecification(aSpecification)
        If lSource IsNot Nothing Then
            MapWinUtility.Logger.Dbg("Found timeseries source via atcDataManager.DataSourceBySpecification: " & lSource.Specification)
        End If
        Return lSource
    End Function

    'Public Function GetFeatureSet(ByVal aTag As String) As DotSpatial.Data.FeatureSet
    '    Dim lLayer As Layer = LayerFromTag(aTag)

    '    If lLayer Is Nothing Then
    '        Throw New ApplicationException("Layer " & aTag & " not found in project.")
    '    ElseIf TypeOf (lLayer.DataSet) Is DotSpatial.Data.FeatureSet Then  'NOT: lLayer.DataSet Is GetType(DotSpatial.Data.FeatureSet) Then
    '        Return lLayer.DataSet
    '    Else
    '        Throw New ApplicationException("Layer " & aTag & " is not a FeatureSet.")
    '    End If
    'End Function

    'Public Function GetImageData(ByVal aTag As String) As DotSpatial.Data.ImageData
    '    Dim lLayer As Layer = LayerFromTag(aTag)

    '    If lLayer Is Nothing Then
    '        Throw New ApplicationException("Layer " & aTag & " not found in project.")
    '    ElseIf TypeOf (lLayer.DataSet) Is DotSpatial.Data.ImageData Then
    '        Return lLayer.DataSet
    '    Else
    '        Throw New ApplicationException("Layer " & aTag & " is not ImageData.")
    '    End If
    'End Function

    'Public Function GetTimeSeriesSource(ByVal key As String) As atcData.atcTimeseriesSource
    'End Function

    Public Overridable Function GetKeys(ByVal aKeyType As LayerSpecification) As Generic.List(Of String)
        Dim lKeysOverlapping As New Generic.List(Of String) '2-letter abbreviations for the states overlapping our region
        Dim lProjectLayer As Layer = Nothing
        'First check for existing layer in the project
        For Each lProjectLayer In Layers
            If lProjectLayer.Specification.FilePattern = aKeyType.FilePattern OrElse (lProjectLayer.Specification.FilePattern = "cat.shp" AndAlso aKeyType = Region.RegionTypes.huc8) Then
                Dim lFilename As String = IO.Path.Combine(ProjectFolder, lProjectLayer.Specification.FilePattern)
                lFilename = IO.Path.ChangeExtension(lFilename, ".dbf")
                If IO.File.Exists(lFilename) Then
                    Dim lDBF As New atcUtility.atcTableDBF
                    If lDBF.OpenFile(lFilename) Then
                        Dim lFieldIndex As Integer = lDBF.FieldNumber(lProjectLayer.Specification.IdFieldName)
                        If lFieldIndex > 0 Then
                            For lRecord As Integer = 1 To lDBF.NumRecords
                                lDBF.CurrentRecord = lRecord
                                lKeysOverlapping.Add(lDBF.Value(lFieldIndex))
                            Next
                        End If
                    End If
                End If
            End If
        Next

        If lKeysOverlapping.Count > 0 Then
            Return lKeysOverlapping
        Else
            Return Region.GetKeys(aKeyType)
        End If
    End Function

    'Public Function AsMWPRJ() As String
    '    Dim lMWPRJ As String = atcUtility.GetEmbeddedFileAsString("mwprj.txt")
    '    Dim lLayersSB As New System.Text.StringBuilder
    '    For Each lLayer As D4EM.Data.Layer In Layers
    '        Dim lRenderString As String = ""
    '        Dim lRendererFilename As String = IO.Path.ChangeExtension(lLayer.FileName, ".mwsr")
    '        If IO.File.Exists(lRendererFilename) Then
    '            lRenderString = IO.File.ReadAllText(lRendererFilename).Replace("<SFRendering>", "").Replace("</SFRendering>", "")
    '            Dim lPathStart As Integer = lRenderString.IndexOf("Path=")
    '            Dim lPathEnd As Integer = lRenderString.IndexOf("""", lPathStart + 6)
    '            lRenderString = lRenderString.Substring(0, lPathStart + 6) & lLayer.FileName & lRenderString.Substring(lPathEnd)
    '            lLayersSB.Append(lRenderString)
    '        Else
    '            'TODO: lRendererFilename = IO.Path.ChangeExtension(lLayer.FileName, ".mwleg")
    '            Dim lTypeCode As String = 4 'default to raster layer type
    '            Dim lDataSet As DotSpatial.Data.IDataSet = lLayer.DataSet
    '            If TypeOf (lDataSet) Is DotSpatial.Data.FeatureSet Then
    '                Select Case lLayer.AsFeatureSet.FeatureType
    '                    Case DotSpatial.Topology.FeatureType.Line : lTypeCode = 2
    '                    Case DotSpatial.Topology.FeatureType.MultiPoint : lTypeCode = 1
    '                    Case DotSpatial.Topology.FeatureType.Point : lTypeCode = 1
    '                    Case DotSpatial.Topology.FeatureType.Polygon : lTypeCode = 3
    '                End Select
    '                lLayersSB.Append("<Layer Name=""" & lLayer.Specification.Name & """ GroupName=""Layers"" Type=""" & lTypeCode & """ Path=""" & lLayer.FileName & """ Tag="""" LegendPicture="""" Visible=""True"" LabelsVisible=""True"" Expanded=""False"">" & vbCrLf _
    '                               & "<Image Type="""">" & vbCrLf & "</Image>" & vbCrLf _
    '                               & "<ShapeFileProperties VerticesVisible=""False"" Color=""16777215"" DrawFill=""False"" TransparencyPercent=""1"" FillStipple=""0"" LineOrPointSize=""1"" LineStipple=""0"" OutLineColor=""13816530"" PointType=""0"" CustomLineStipple=""0"" UseTransparency=""False"" TransparencyColor=""0"" FillStippleTransparent=""True"" FillStippleLineColor=""-16777216"">" & vbCrLf _
    '                               & "<CustomPointType>" & vbCrLf _
    '                               & "<Image Type="""">" & vbCrLf _
    '                               & "</Image>" & vbCrLf _
    '                               & "</CustomPointType>" & vbCrLf _
    '                               & "</ShapeFileProperties>" & vbCrLf _
    '                               & "<DynamicVisibility UseDynamicVisibility=""False"" Scale=""0"" />" & vbCrLf _
    '                               & "</Layer>" & vbCrLf)
    '            Else
    '                lTypeCode = 4
    '                lLayersSB.Append("<Layer Name=""" & lLayer.Specification.Name & """ GroupName=""Layers"" Type=""" & lTypeCode & """ Path=""" & lLayer.FileName & """ Tag="""" LegendPicture="""" Visible=""True"" LabelsVisible=""True"" Expanded=""False"">" & vbCrLf _
    '                               & "<Image Type="""">" & vbCrLf & "</Image>" & vbCrLf _
    '                               & "<GridProperty ImageLayerFillTransparency=""0"" UseHistogram=""False"" AllowHillshade=""True"" SetToGrey=""False"" BufferSize=""100"" ImageColorScheme=""FallLeaves"" TransparentColor=""65536"" UseTransparency=""True"">" & vbCrLf _
    '                               & "<Legend Key="""" NoDataColor=""65536"">" & vbCrLf _
    '                               & "<ColorBreaks>" & vbCrLf _
    '                               & "<Break HighColor=""4031175"" LowColor=""680970"" HighValue=""27934"" LowValue=""17585"" GradientModel=""1"" ColoringType=""0"" Caption="""" />" & vbCrLf _
    '                               & "<Break HighColor=""8772849"" LowColor=""4031175"" HighValue=""38283"" LowValue=""27934"" GradientModel=""1"" ColoringType=""0"" Caption="""" />" & vbCrLf _
    '                               & "</ColorBreaks>" & vbCrLf _
    '                               & "</Legend>" & vbCrLf _
    '                               & "</GridProperty>" & vbCrLf _
    '                               & "<DynamicVisibility UseDynamicVisibility=""False"" Scale=""0"" />" & vbCrLf _
    '                               & "</Layer>" & vbCrLf)
    '            End If
    '        End If
    '    Next
    '    'TODO: add group label to LayerSpecification, then use them here
    '    Return lMWPRJ.Replace("<Groups>", "<Groups>" & vbCrLf _
    '                          & "<Group Name=""Layers"" Expanded=""True"" Position=""0"">" & vbCrLf _
    '                          & "<Image Type="""">" & vbCrLf _
    '                          & "</Image>" & vbCrLf _
    '                          & "" & vbCrLf _
    '                          & "<Layers>" & vbCrLf _
    '                          & lLayersSB.ToString & vbCrLf _
    '                          & "</Layers>" & vbCrLf _
    '                          & "</Group>")
    'End Function

    ''' <summary>
    ''' Return the contents of a MapWindow project file describing this project
    ''' </summary>
    Public Function AsMWPRJ() As String
        Dim lMWPRJ As String = atcUtility.GetEmbeddedFileAsString("mwprj.txt")
        lMWPRJ = lMWPRJ.Replace("ProjectName", IO.Path.GetFileNameWithoutExtension(Me.ProjectFilename))
        'TODO: replace ExtentsLeft="" ExtentsRight="" ExtentsBottom="" ExtentsTop=""
        Dim lFolder As String = ""
        Try
            lFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            If IO.Directory.Exists(lFolder) Then
                lMWPRJ = lMWPRJ.Replace("Application Data", lFolder)
            End If
        Catch
        End Try

        lMWPRJ = lMWPRJ.Replace("ProjectProjection=""""", "ProjectProjection=""" & DesiredProjection.ToProj4String() & """")
        'lMWPRJ = lMWPRJ.Replace("ProjectProjectionWKT=""""", "ProjectProjectionWKT=""" & DesiredProjection.ToEsriString() & """")

        'First <Layers> contains rendering information for MapWinGIS, remove layers not in me.Layers
        Dim lLayerNames As New Generic.List(Of String)
        Dim lLayerFileNames As New Generic.List(Of String)
        Dim lLayersStartTag As String = "<Layers>"
        Dim lLayersEndTag As String = "</Layers>"
        Dim lStartTagIndex As Integer = lMWPRJ.IndexOf(lLayersStartTag)
        Dim lEndTagIndex As Integer = lMWPRJ.IndexOf(lLayersEndTag)

        Dim lTemplate As String = lMWPRJ.Substring(lStartTagIndex, lEndTagIndex - lStartTagIndex + lLayersEndTag.Length)
        Dim lInProject As String = lLayersStartTag & vbCrLf

        Dim lXMLdoc As New Xml.XmlDocument()
        lXMLdoc.LoadXml(lTemplate)
        Dim lTopNode As Xml.XmlNode = lXMLdoc.ChildNodes(0)
        For Each lLayerXML As Xml.XmlNode In lTopNode.ChildNodes
            Dim lFileNameXML = lLayerXML.Attributes.GetNamedItem("Filename")
            If lFileNameXML IsNot Nothing Then
                If lFileNameXML.Value.EndsWith(".tif") Then
                    MapWinUtility.Logger.Dbg("Temporarily skip adding GeoTIFF layer to BASINS map: " & lFileNameXML.Value)
                Else
                    'LayerFromFileName(IO.Path.Combine(ProjectFolder, lFileNameXML.Value))
                    For Each lProjectLayer As Layer In Layers
                        If lProjectLayer.FileName.EndsWith(lFileNameXML.Value) Then
                            lLayerNames.Add(lLayerXML.Attributes.GetNamedItem("LayerName").Value)
                            lLayerFileNames.Add(lProjectLayer.FileName)
                            Dim lRelativeFilename As String = atcUtility.RelativeFilename(lProjectLayer.FileName, ProjectFolder)
                            If lFileNameXML.Value <> lRelativeFilename Then
                                lFileNameXML.Value = lRelativeFilename
                            End If
                            Dim lGridFilenameXML = lLayerXML.Attributes.GetNamedItem("GridFilename")
                            If lGridFilenameXML IsNot Nothing Then
                                lGridFilenameXML.Value = lProjectLayer.FileName
                            End If
                            lInProject &= "    " & lLayerXML.OuterXml & vbCrLf
                        End If
                    Next
                End If
            End If
        Next
        'For Each lLayer As D4EM.Data.Layer In Layers
        '    If Not lLayerFileNames.Contains(lLayer.FileName) Then
        '        MapWinUtility.Logger.Dbg("Layer not in MapWindow Project Template: " & lLayer.FileName)
        '        lInProject &= "" 'TODO
        '    End If
        'Next
        lMWPRJ = lMWPRJ.Replace(lTemplate, lInProject & lLayersEndTag)

        'Second <Layers> contains group information for legend, remove layers not in me.Layers
        Dim lGroupNames As New atcUtility.atcCollection
        lStartTagIndex = lMWPRJ.LastIndexOf(lLayersStartTag)
        lEndTagIndex = lMWPRJ.LastIndexOf(lLayersEndTag)

        lTemplate = lMWPRJ.Substring(lStartTagIndex, lEndTagIndex - lStartTagIndex + lLayersEndTag.Length)
        lInProject = lLayersStartTag & vbCrLf

        lXMLdoc.LoadXml(lTemplate)
        lTopNode = lXMLdoc.ChildNodes(0)
        For Each lLayerXML As Xml.XmlNode In lTopNode.ChildNodes
            Dim lLayerNameXML = lLayerXML.Attributes.GetNamedItem("Name")
            If lLayerNameXML IsNot Nothing Then
                If lLayerNames.Contains(lLayerNameXML.Value) Then
                    Dim lHandleXML = lLayerXML.Attributes.GetNamedItem("Handle")
                    If lHandleXML IsNot Nothing Then
                        lHandleXML.Value = lLayerNames.IndexOf(lLayerNameXML.Value)
                    End If

                    Dim lGroupNameXML = lLayerXML.Attributes.GetNamedItem("GroupName")
                    If lGroupNameXML IsNot Nothing Then
                        'Keep track of how many layers in each group
                        Dim lLayersInGroup As Integer = lGroupNames.Increment(lGroupNameXML.Value)

                        Dim lLayerPositionInGroup = lLayerXML.Attributes.GetNamedItem("PositionInGroup")
                        If lLayerPositionInGroup IsNot Nothing Then
                            lLayerPositionInGroup.Value = lLayersInGroup - 1
                        End If

                        Dim lLayerGroupIndex = lLayerXML.Attributes.GetNamedItem("GroupIndex")
                        If lLayerGroupIndex IsNot Nothing Then
                            lLayerGroupIndex.Value = lGroupNames.IndexFromKey(lGroupNameXML.Value)
                        End If
                    End If
                    lInProject &= "      " & lLayerXML.OuterXml & vbCrLf
                End If
            End If
        Next
        'Add layers in me that are not in the MWPRJ
        For Each lLayer As D4EM.Data.Layer In Layers
            If Not lLayerFileNames.Contains(lLayer.FileName) Then
                lInProject &= "" 'TODO
            End If
        Next
        lMWPRJ = lMWPRJ.Replace(lTemplate, lInProject & "    " & lLayersEndTag)

        'Keep only groups that have a layer in them
        Dim lGroupsStartTag As String = "<Groups>"
        Dim lGroupsEndTag As String = "</Groups>"
        lStartTagIndex = lMWPRJ.LastIndexOf(lGroupsStartTag)
        lEndTagIndex = lMWPRJ.LastIndexOf(lGroupsEndTag)

        lTemplate = lMWPRJ.Substring(lStartTagIndex, lEndTagIndex - lStartTagIndex + lGroupsEndTag.Length)
        lInProject = lGroupsStartTag & vbCrLf

        lXMLdoc.LoadXml(lTemplate)
        lTopNode = lXMLdoc.ChildNodes(0)
        For Each lGroupXML As Xml.XmlNode In lTopNode.ChildNodes
            Dim lGroupNameXML = lGroupXML.Attributes.GetNamedItem("Name")
            If lGroupNameXML IsNot Nothing Then
                If lGroupNames.Keys.Contains(lGroupNameXML.Value) Then
                    Dim lGroupPosition = lGroupXML.Attributes.GetNamedItem("Position")
                    If lGroupPosition IsNot Nothing Then
                        lGroupPosition.Value = lGroupNames.IndexFromKey(lGroupNameXML.Value)
                    End If
                    lInProject &= "      " & lGroupXML.OuterXml & vbCrLf
                End If
            End If
        Next
        lMWPRJ = lMWPRJ.Replace(lTemplate, lInProject & "    " & lGroupsEndTag)

        Dim lBasinsSettingsStringTemplate As String = "SettingsString="""" Key=""BASINS_atcBasinsPlugIn"
        If lMWPRJ.Contains(lBasinsSettingsStringTemplate) Then
            For Each lTimeseriesSource In TimeseriesSources
                If Not atcData.atcDataManager.DataSources.Contains(lTimeseriesSource) Then
                    atcData.atcDataManager.DataSources.Add(lTimeseriesSource)
                End If
            Next
            Try
                lMWPRJ = lMWPRJ.Replace(lBasinsSettingsStringTemplate, _
                         "SettingsString=""" & ("<BASINS>" & atcData.atcDataManager.XML & "</BASINS>").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;").Replace(vbCr, "&#xD;").Replace(vbLf, "&#xA;") & """ Key=""BASINS_atcBasinsPlugIn")
            Catch 'Ignore if we can't set data manager XML, it just means there are no datasets to save
            End Try
        End If
        Return lMWPRJ.Replace(vbCrLf, vbCr).Replace(vbLf, vbCr).Replace(vbCr, vbCrLf)
    End Function

    Public Function Summary() As String
        Dim lFeatureSetCount As Integer = 0
        Dim lPolygonShapeFileCount As Integer = 0
        Dim lPointShapeFileCount As Integer = 0

        For lLayerIndex As Integer = 0 To Layers.Count - 1
            If lLayerIndex = 20 Then
                Debug.Print("LayerIndex " & lLayerIndex)
            End If
            Dim lLayer As D4EM.Data.Layer = Layers(lLayerIndex)
            If TypeOf (lLayer.DataSet) Is DotSpatial.Data.FeatureSet Then
                lFeatureSetCount += 1
            End If
            If TypeOf (lLayer.DataSet) Is DotSpatial.Data.PolygonShapefile Then
                lPolygonShapeFileCount += 1
            End If
            If TypeOf (lLayer.DataSet) Is DotSpatial.Data.PointShapefile Then
                lPointShapeFileCount += 1
            End If
        Next
        Return "Layers: " & Layers.Count & " with " _
                          & lFeatureSetCount & " FeatureSets " _
                          & lPolygonShapeFileCount & " PolygonShapeFiles " _
                          & lPointShapeFileCount & " PointShapeFiles, " _
                          & TimeseriesSources.Count & " TimeseriesSources"
    End Function

    Public Property XML() As String
        Get
            Dim lIndent As String = "    "
            Dim lProjectionXML As String = ""
            If DesiredProjection IsNot Nothing Then
                lProjectionXML = lIndent & "<DesiredProjection>" & DesiredProjection.ToProj4String & "</DesiredProjection>" & vbCrLf
            End If

            Dim lLayersXML As String = ""
            For Each lLayer In Layers
                lLayersXML &= lIndent & "<Layer>" & lLayer.FileName & "</Layer>" & vbCrLf
            Next

            Dim lTimeseriesSourcesXML As String = ""
            For Each lTimeseriesSource In TimeseriesSources
                lTimeseriesSourcesXML &= lIndent & "<TimeseriesSource>" & lTimeseriesSource.Specification & "</TimeseriesSource>" & vbCrLf
            Next

            Return "<Project>" & vbCrLf _
                 & lIndent & "<ProjectFilename>" & ProjectFilename & "</ProjectFilename>" & vbCrLf _
                 & lIndent & "<ProjectFolder>" & ProjectFolder & "</ProjectFolder>" & vbCrLf _
                 & lIndent & "<CacheFolder>" & CacheFolder & "</CacheFolder>" & vbCrLf _
                 & lIndent & "<Clip>" & Clip & "</Clip>" & vbCrLf _
                 & lIndent & "<Merge>" & Merge & "</Merge>" & vbCrLf _
                 & lIndent & "<GetEvenIfCached>" & GetEvenIfCached & "</GetEvenIfCached>" & vbCrLf _
                 & lIndent & "<CacheOnly>" & CacheOnly & "</CacheOnly>" & vbCrLf _
                 & lProjectionXML _
                 & Region.XML _
                 & lLayersXML _
                 & lTimeseriesSourcesXML _
                 & "</Project>" & vbCrLf
            'If Not Double.IsNaN(PourPointLatitude) Then
            '    sb.AppendLine("  <PourPoint>")
            '    sb.AppendLine("    <PourPointLatitude>" & PourPointLatitude & "</PourPointLatitude>")
            '    sb.AppendLine("    <PourPointLongitude>" & PourPointLongitude & "</PourPointLongitude>")
            '    sb.AppendLine("    <PourPointMaxKm>" & PourPointMaxKm & "</PourPointMaxKm>")
            '    sb.AppendLine("  </PourPoint>")
            'End If
        End Get
        Set(ByVal value As String)
            Dim lDoc As New Xml.XmlDocument
            lDoc.LoadXml(value)
            SetXML(lDoc.FirstChild)
        End Set
    End Property

    Private Sub SetXML(ByVal aXML As Xml.XmlNode)
        Dim lArg As Xml.XmlNode = aXML.FirstChild
        While Not lArg Is Nothing
            Select Case lArg.Name.ToLower
                Case "project" : SetXML(lArg)
                Case "region"
                    Try
                        Region = New D4EM.Data.Region(lArg.OuterXml)
                    Catch ex As Exception
                        MapWinUtility.Logger.Dbg("Unable to set Region: " & ex.Message & " in XML:" & vbCrLf & lArg.OuterXml)
                    End Try
                Case Else : SetPrivate(lArg.Name, lArg.InnerText)
            End Select
            lArg = lArg.NextSibling
        End While
    End Sub

    Private Sub SetPrivate(ByVal aPart As String, ByVal aNewValue As String)
        Select Case aPart.ToLower
            Case "projectfilename" : ProjectFilename = aNewValue
            Case "projectfolder" : ProjectFolder = aNewValue
            Case "cachefolder" : CacheFolder = aNewValue
            Case "clip" : Boolean.TryParse(aNewValue, Clip)
            Case "merge" : Boolean.TryParse(aNewValue, Merge)
            Case "getevenifcached" : Boolean.TryParse(aNewValue, GetEvenIfCached)
            Case "cacheonly" : Boolean.TryParse(aNewValue, CacheOnly)
            Case "desiredprojection" : DesiredProjection = Globals.FromProj4(aNewValue)
            Case "layer" : Layers.Add(New D4EM.Data.Layer(aNewValue, D4EM.Data.LayerSpecification.FromFilename(aNewValue, GetType(D4EM.Data.Region.RegionTypes)), False))
            Case "timeseriessource" : atcData.atcDataManager.OpenDataSource(aNewValue)
        End Select
    End Sub

End Class