Public Class clsMetGages
    Friend DSN As Integer, staID As String, Pcode As String, Scenario As String, Lat As String, Lon As String,
        MinDate As String, MaxDate As String
    Sub New(ByVal _DSN As Integer, ByVal _staID As String, ByVal _Pcode As String, ByVal _scen As String,
            ByVal _lat As String, ByVal _lon As String, ByVal _mndate As String, ByVal _mxdate As String)
        DSN = _DSN
        staID = _staID
        Pcode = _Pcode
        Scenario = _scen
        Lat = _lat
        Lon = _lon
        MinDate = _mndate
        MaxDate = _mxdate
    End Sub
End Class
