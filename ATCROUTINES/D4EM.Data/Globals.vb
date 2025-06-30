Imports MapWinUtility

Public Class Globals

    ''These all mean the same thing, but we want to check more than one way of writing it so we can avoid unnecessary projection
    'Private Shared GeographicProjections() As String = {"+proj=latlong +datum=NAD83", _
    '                                                   "+proj=longlat +ellps=sphere +lon_0=0 +lat_0=0 +h=0 +datum=NAD83"}

    ''These all mean the same thing, but we want to check more than one way of writing it so we can avoid unnecessary projection
    'Private Shared AlbersProjections() As String = {"+proj=aea +ellps=GRS80 +lon_0=-96 +lat_0=23.0 +lat_1=29.5 +lat_2=45.5 +x_0=0 +y_0=0 +datum=NAD83 +units=m", _
    '                                               "+proj=aea +lat_1=29.5 +lat_2=45.5 +lat_0=23 +lon_0=-96 +x_0=0 +y_0=0 +ellps=GRS80 +datum=NAD83 +units=m +no_defs", _
    '                                               "+proj=aea +datum=NAD83"}

    'Private Shared WebMercatorProjection = New DotSpatial.Projections.ProjectionInfo("+proj=merc +lon_0=0 +lat_ts=0 +x_0=0 +y_0=0 +a=6378137 +b=6378137 +units=m +no_defs")

    Private Shared pGeographicProjection As DotSpatial.Projections.ProjectionInfo

    Public Shared Function GeographicProjection() As DotSpatial.Projections.ProjectionInfo
        If pGeographicProjection Is Nothing Then
            pGeographicProjection = DotSpatial.Projections.ProjectionInfo.FromProj4String("+proj=latlong +datum=NAD83")
        End If
        Return pGeographicProjection 'DotSpatial.Projections.KnownCoordinateSystems.Geographic.NorthAmerica.NorthAmericanDatum1983
    End Function

    Public Shared Function WebMercatorProjection() As DotSpatial.Projections.ProjectionInfo
        Return DotSpatial.Projections.KnownCoordinateSystems.Projected.World.WebMercator
    End Function

    Public Shared Function AlbersProjection() As DotSpatial.Projections.ProjectionInfo
        Return DotSpatial.Projections.KnownCoordinateSystems.Projected.NorthAmerica.USAContiguousAlbersEqualAreaConicUSGS
    End Function

    Public Shared Sub RepairAlbers(ByRef aProjection As DotSpatial.Projections.ProjectionInfo)
        If aProjection IsNot DotSpatial.Projections.KnownCoordinateSystems.Projected.NorthAmerica.USAContiguousAlbersEqualAreaConicUSGS Then
            Dim lProj4 As String = aProjection.ToProj4String
            If MatchesAlbersUSGS(lProj4) Then
                Logger.Dbg("Repairing Albers Projection from " & lProj4)
                aProjection = DotSpatial.Projections.KnownCoordinateSystems.Projected.NorthAmerica.USAContiguousAlbersEqualAreaConicUSGS
            End If
        End If
    End Sub

    Private Shared Function MatchesAlbersUSGS(aProj4 As String) As Boolean
        Select Case aProj4.Trim().ToLowerInvariant()
            Case "+proj=aea +ellps=grs80 +lon_0=-96 +lat_0=23.0 +lat_1=29.5 +lat_2=45.5 +x_0=0 +y_0=0 +datum=nad83 +units=m",
                 "+proj=aea +lat_1=29.5 +lat_2=45.5 +lat_0=23 +lon_0=-96 +x_0=0 +y_0=0 +ellps=grs80 +datum=nad83 +units=m +no_defs",
                 "+proj=aea +lat_1=29.5 +lat_2=45.5 +lat_0=23 +lon_0=-96 +x_0=0 +y_0=0 +ellps=grs80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs",
                 "+x_0=0 +y_0=0 +lat_0=23 +lon_0=-96 +lat_1=29.5 +lat_2=45.5 +proj=aea +datum=nad83 +no_defs",
                 "+x_0=0 +y_0=0 +lat_0=23 +lon_0=-96 +lat_1=29.5 +lat_2=45.5 +proj=aea +towgs84=0,0,0 +ellps=grs80 +no_defs",
                 "+x_0=0 +y_0=0 +lat_0=23 +lat_1=29.5 +lat_2=45.5 +lonc=-96 +proj=aea +a=6378137 +b=6356752.31414036 +no_defs",
                 "+x_0=0 +y_0=0 +lat_0=0 +lat_1=29.5 +lat_2=45.5 +lonc=0 +proj=aea +datum=nad83 +no_defs",
                 "+proj=aea +datum=nad83"
                Return True
        End Select
        Return False
    End Function

    Public Shared Function FromProj4(ByVal aProj4String As String) As DotSpatial.Projections.ProjectionInfo
        If MatchesAlbersUSGS(aProj4String) Then
            Return AlbersProjection()
        End If
        Dim lProjection = DotSpatial.Projections.ProjectionInfo.FromProj4String(aProj4String)
        RepairAlbers(lProjection)
        Return lProjection
    End Function

    Public Shared Function Initialize() As Boolean
        If DotSpatial.Data.DataManager.DefaultDataManager.PreferredProviders.Count = 0 Then
            Logger.Dbg("Opening GDAL")
            Dim lGdalRasterProvider As New DotSpatial.Data.Rasters.GdalExtension.GdalRasterProvider
            'Creating the provider now automatically adds it as the preferred provider for these types
            'DotSpatial.Data.DataManager.DefaultDataManager.PreferredProviders.Add(".tif", lGdalRasterProvider)
            'DotSpatial.Data.DataManager.DefaultDataManager.PreferredProviders.Add(".adf", lGdalRasterProvider)
            Logger.Dbg("Opened " & lGdalRasterProvider.Name)
        End If
        Return True
    End Function

End Class
