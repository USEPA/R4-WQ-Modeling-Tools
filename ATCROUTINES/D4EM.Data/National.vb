Imports atcUtility
Imports MapWinUtility
Imports D4EM.Data.LayerSpecification

''' <summary>
''' Methods for accessing national layers included with D4EM
''' </summary>
Public Class National

    Public Class LayerSpecifications
        'Public Shared huc8 As New LayerSpecification(FilePattern:="huc250d3.shp", Tag:="huc8", Role:=Roles.SubBasin, IdFieldName:="CU", Name:="HUC-8")
        Public Shared huc8 As New LayerSpecification(FilePattern:="national_huc250d3.shp", Tag:="huc8", Role:=Roles.SubBasin, IdFieldName:="CU", Name:="HUC-8")
        Public Shared huc12 As New LayerSpecification(FilePattern:="huc12.shp", Tag:="huc12", Role:=Roles.SubBasin, IdFieldName:="HUC_12", Name:="HUC-12")
        'Public Shared county As New LayerSpecification(FilePattern:="cnty.shp", Tag:="core31.cnty", Role:=Roles.County, IdFieldName:="FIPS", Name:="County")
        Public Shared county As New LayerSpecification(FilePattern:="national_cnty.shp", Tag:="core31.cnty", Role:=Roles.County, IdFieldName:="FIPS", Name:="County")
        'Public Shared state As New LayerSpecification(FilePattern:="st.shp", Tag:="core31.st", Role:=Roles.State, IdFieldName:="ST", Name:="State")
        Public Shared state As New LayerSpecification(FilePattern:="national_st.shp", Tag:="core31.st", Role:=Roles.State, IdFieldName:="ST", Name:="State")
    End Class

    'TODO: make All a method of a new base class of LayerSpecifications that does introspection to find all LayerSpecification fields including class children?
    Public Shared LayerSpecificationsAll() As LayerSpecification = {LayerSpecifications.huc8,
                                                                    LayerSpecifications.huc12,
                                                                    LayerSpecifications.county,
                                                                    LayerSpecifications.state}
    Private Shared pNationalShapeFilename As New Generic.Dictionary(Of LayerSpecification, String)

    ''' <summary>
    ''' Find the file path of a national layer
    ''' </summary>
    ''' <param name="aLayer">One of the members of National.LayerSpecifications</param>
    ''' <returns>Full path of shape file, or Nothing if not found</returns>
    Public Shared Property ShapeFilename(ByVal aLayer As LayerSpecification) As String
        Get
            Dim lFilename As String = Nothing
            If pNationalShapeFilename.ContainsKey(aLayer) AndAlso IO.File.Exists(pNationalShapeFilename(aLayer)) Then
                lFilename = pNationalShapeFilename(aLayer)
            ElseIf Not String.IsNullOrEmpty(aLayer.FilePattern) Then
                'TODO: Where will D4EM store national/world layers?
                lFilename = atcUtility.FindFile(Nothing, IO.Path.Combine("\data\national\", aLayer.FilePattern))
                If Not IO.File.Exists(lFilename) Then
                    Dim lPath As String = PathNameOnly(Reflection.Assembly.GetEntryAssembly.Location)
                    While IO.Directory.Exists(lPath)
                        lFilename = IO.Path.Combine(lPath, "data", "national", aLayer.FilePattern)
                        If IO.File.Exists(lFilename) Then Exit While
                        lPath = PathNameOnly(lPath)
                    End While
                End If
                If IO.File.Exists(lFilename) Then
                    ShapeFilename(aLayer) = lFilename                
                Else
                    lFilename = Nothing
                End If
            End If
            Return lFilename
        End Get
        Set(ByVal value As String)
            pNationalShapeFilename(aLayer) = value
            'Try to set the other national layer file names too
            Dim lDirectory As String = IO.Path.GetDirectoryName(value)
            Dim lHuc12pos As Integer = lDirectory.IndexOf("huc12")
            If lHuc12pos > 0 Then lDirectory = lDirectory.Substring(0, lHuc12pos)
            For Each lLayerSpecification In LayerSpecificationsAll
                If lLayerSpecification IsNot aLayer AndAlso Not pNationalShapeFilename.ContainsKey(lLayerSpecification) Then
                    Dim lFilename As String = IO.Path.Combine(lDirectory, lLayerSpecification.FilePattern)
                    If IO.File.Exists(lFilename) Then
                        pNationalShapeFilename(lLayerSpecification) = lFilename
                    End If
                End If
            Next
        End Set
    End Property

    ''' <summary>
    ''' Return county name given a county FIPS code
    ''' </summary>
    ''' <param name="aFIPScode">5-character FIPS code</param>
    ''' <returns>name of county</returns>
    ''' <remarks>Throws exception if county shape DBF, FIPS or CNTYNAME field, or FIPS code not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function CountyNameFromFIPS(ByVal aFIPScode As String) As String
        Dim lFIPS As String = aFIPScode
        Select Case aFIPScode.Length
            Case 4
        End Select
        Return FindDBFFieldValue(ShapeFilename(LayerSpecifications.county), "FIPS", aFIPScode, "CNTYNAME")
    End Function

    ''' <summary>
    ''' Return state's two-letter abbreviation given a state (or county) FIPS code
    ''' </summary>
    ''' <returns>abbreviation of state name</returns>
    ''' <remarks>Throws exception if state shape DBF, FIPS or ST field, or FIPS code not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function StateAbbreviationFromFIPS(ByVal aFIPScode As String) As String
        Return FindDBFFieldValue(ShapeFilename(LayerSpecifications.state), "FIPS", aFIPScode.Substring(0, 2), "ST")
    End Function

    ''' <summary>
    ''' Return state's two-letter abbreviation given the state's name
    ''' </summary>
    ''' <returns>state's two-letter abbreviation</returns>
    ''' <remarks>Throws exception if state shape DBF, NAME or ST field, or name not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function StateAbbreviationFromName(ByVal aName As String) As String
        Return FindDBFFieldValueCaseInsensitive(ShapeFilename(LayerSpecifications.state), "FIPS", aName.Substring(0, 2), "ST")
    End Function

    ''' <summary>
    ''' Return state FIPS code given the state's two-letter abbreviation
    ''' </summary>
    ''' <returns>abbreviation of state name</returns>
    ''' <remarks>Throws exception if state shape DBF, FIPS or ST field, or FIPS code not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function StateFIPSFromAbbreviation(ByVal aStateAbbreviation As String) As String
        Return FindDBFFieldValue(ShapeFilename(LayerSpecifications.state), "ST", aStateAbbreviation, "FIPS")
    End Function

    ''' <summary>
    ''' Return state name given a state two-letter abbreviation
    ''' </summary>
    ''' <returns>abbreviation of state name</returns>
    ''' <remarks>Throws exception if state shape DBF, FIPS or ST field, or FIPS code not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function StateNameFromAbbreviation(ByVal aStateAbbreviation As String) As String
        Return FindDBFFieldValue(ShapeFilename(LayerSpecifications.state), "ST", aStateAbbreviation.ToUpper, "NAME")
    End Function

    ''' <summary>
    ''' Return state name given a state (or county) FIPS code
    ''' </summary>
    ''' <param name="aFIPScode"></param>
    ''' <returns>name of state</returns>
    ''' <remarks>Throws exception if state shape DBF, FIPS or NAME field, or FIPS code not found.
    ''' Not efficient for multiple queries because DBF is reopened for each call</remarks>
    Public Shared Function StateNameFromFIPS(ByVal aFIPScode As String) As String
        Return FindDBFFieldValue(ShapeFilename(LayerSpecifications.state), "FIPS", aFIPScode.Substring(0, 2), "NAME")
    End Function

    Private Shared Function FindDBFFieldValue(ByVal aShapeFileName As String, ByVal aSearchFieldName As String, ByVal aSearchValue As String, ByVal aReturnFieldName As String) As String
        Dim lDBF As New atcTableDBF
        If lDBF.OpenFile(IO.Path.ChangeExtension(aShapeFileName, ".dbf")) Then
            If lDBF.FindFirst(lDBF.FieldNumber(aSearchFieldName), aSearchValue) Then
                Return lDBF.Value(lDBF.FieldNumber(aReturnFieldName))
            End If
        End If
        Throw New ApplicationException("Did not find " & aSearchFieldName & " = " & aSearchValue)
    End Function

    Private Shared Function FindDBFFieldValueCaseInsensitive(ByVal aShapeFileName As String, ByVal aSearchFieldName As String, ByVal aSearchValue As String, ByVal aReturnFieldName As String) As String
        Dim lValue As String = aSearchValue.ToLower
        Dim lDBF As New atcTableDBF
        If lDBF.OpenFile(lDBF.OpenFile(IO.Path.ChangeExtension(aShapeFileName, ".dbf"))) Then
            Dim lSearchField As Integer = lDBF.FieldNumber(aSearchFieldName)
            If lSearchField > 0 Then
                For lRecordIndex As Integer = 1 To lDBF.NumRecords
                    lDBF.CurrentRecord = lRecordIndex
                    If lDBF.Value(lSearchField).ToLower.Equals(lValue) Then
                        Return lDBF.Value(lDBF.FieldNumber(aReturnFieldName))
                    End If
                Next
            End If
        End If
        Throw New ApplicationException("Did not find " & aSearchFieldName & " = " & aSearchValue)
    End Function

End Class
