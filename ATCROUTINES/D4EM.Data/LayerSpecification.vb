''' <summary>Invariant properties of a layer provided by a source</summary>
''' <remarks></remarks>
Public Class LayerSpecification
    Enum Roles
        Unknown = 0
        Elevation
        Slope
        LandUse
        SubBasin
        Hydrography
        Soil
        County
        State
        ZIP
        OtherBoundary
        Station
        MetStation
        Road
        Railroad
    End Enum

    Property Tag As String = ""
    Property Name As String = ""
    Property FilePattern As String = ""
    Property Source As System.Type = GetType(D4EM.Data.SourceBase)
    Property Role As Roles = Roles.Unknown
    Property IdFieldName As String = ""
    Property NoData As String = ""

    Public Sub New(Optional ByVal Tag As String = "",
                   Optional ByVal Name As String = "",
                   Optional ByVal FilePattern As String = "",
                   Optional ByVal Source As System.Type = Nothing,
                   Optional ByVal Role As Roles = Roles.Unknown,
                   Optional ByVal IdFieldName As String = "",
                   Optional ByVal NoData As String = "")
        Me.Tag = Tag
        Me.Name = Name
        Me.FilePattern = FilePattern
        Me.IdFieldName = IdFieldName
        Me.Source = Source
        Me.Role = Role
        Me.Source = Source
        Me.NoData = NoData
    End Sub

    Public Shared Operator =(ByVal a As LayerSpecification, ByVal b As LayerSpecification) As Boolean
        Return a.Equals(b)
    End Operator
    Public Shared Operator <>(ByVal a As LayerSpecification, ByVal b As LayerSpecification) As Boolean
        Return Not a.Equals(b)
    End Operator

    Public Overrides Function ToString() As String
        Dim lString As String = ""
        If Not String.IsNullOrEmpty(Name) Then lString &= Name
        If Not String.IsNullOrEmpty(Tag) Then lString &= " (" & Tag & ")"
        If Not String.IsNullOrEmpty(FilePattern) Then lString &= " (" & FilePattern & ")"
        If Role <> Roles.Unknown Then lString &= " Role:" & Role.ToString()
        Return lString
    End Function

    ''' <summary>
    ''' Return the LayerSpecification with FilePattern matching aFileName, either as a field of the given type or as a field of a nested type
    ''' </summary>
    ''' <param name="aFileName">Filename of data file</param>
    ''' <param name="aType">Type to search for a LayerSpecification field</param>
    ''' <returns>LayerSpecification matching FilePattern or Nothing if no match found</returns>
    ''' <remarks>Recursively searches nested types within aType if match not found as a field of aType</remarks>
    Public Shared Function FromFilename(ByVal aFileName As String, ByVal aType As System.Type) As LayerSpecification
        Dim lFileName As String = IO.Path.GetFileName(aFileName).ToLower
        Dim lLayerSpecfication As LayerSpecification
        For Each lFieldInfo As System.Reflection.FieldInfo In aType.GetFields
            Try
                lLayerSpecfication = CType(lFieldInfo.GetValue(Nothing), LayerSpecification)
                ' If this layer specification has a file pattern, 
                ' And either the pattern matches the end of the one we are looking for, 
                ' Or it contains * and is a regular expression match, then this LayerSpecification matches the file name
                Dim lFilePattern As String = lLayerSpecfication.FilePattern
                If Not String.IsNullOrEmpty(lFilePattern) Then
                    If lFileName.EndsWith(lFilePattern.ToLower) Then
                        Return lLayerSpecfication
                    ElseIf lFilePattern.Contains("*") OrElse lFilePattern.Contains("+") Then
                        If lFilePattern.Contains("\\") Then
                            lFileName = aFileName.ToLower
                            If Not lFilePattern.StartsWith("*") OrElse lFilePattern.StartsWith(".*") Then
                                lFilePattern = ".*" & lFilePattern
                            End If
                        End If
                        'REGEX expects specification of what repeats, but we want to allow starting a pattern with *
                        If lFilePattern.StartsWith("*") Then lFilePattern = "." & lFilePattern
                        If Not lFilePattern.EndsWith("$") Then lFilePattern &= "$"
                        If System.Text.RegularExpressions.Regex.IsMatch(lFileName, lFilePattern) Then
                            Return lLayerSpecfication
                        End If
                    End If
                End If
            Catch
            End Try
        Next
        'Recursively check nested types too
        For Each lType As System.Type In aType.GetNestedTypes
            lLayerSpecfication = FromFilename(lFileName, lType)
            If lLayerSpecfication IsNot Nothing Then
                Return lLayerSpecfication
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Return the LayerSpecification with Tag matching aTag, either as a field of the given type or as a field of a nested type
    ''' </summary>
    ''' <param name="aTag">Tag of data file</param>
    ''' <param name="aType">Type to search for a LayerSpecification field</param>
    ''' <returns>LayerSpecification matching aTag or Nothing if no match found</returns>
    ''' <remarks>Recursively searches nested types within aType if match not found as a field of aType</remarks>
    Public Shared Function FromTag(ByVal aTag As String, ByVal aType As System.Type) As LayerSpecification
        Dim lTag As String = aTag.ToLower
        Dim lLayerSpecfication As LayerSpecification
        For Each lFieldInfo As System.Reflection.FieldInfo In aType.GetFields
            Try
                lLayerSpecfication = CType(lFieldInfo.GetValue(Nothing), LayerSpecification)
                ' If this layer specification has a file pattern, 
                ' And either the pattern matches the end of the one we are looking for, 
                ' Or it contains * and is a regular expression match, then this LayerSpecification matches the file name
                If lTag = lLayerSpecfication.Tag Then
                    Return lLayerSpecfication
                End If
            Catch
            End Try
        Next
        'Recursively check nested types too
        For Each lType As System.Type In aType.GetNestedTypes
            lLayerSpecfication = FromTag(lTag, lType)
            If lLayerSpecfication IsNot Nothing Then
                Return lLayerSpecfication
            End If
        Next
        Return Nothing
    End Function
End Class

