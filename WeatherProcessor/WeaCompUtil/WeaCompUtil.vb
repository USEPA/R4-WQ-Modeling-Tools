Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports atcUtility
Imports atcData
Imports atcData.atcDataManager
Imports MapWinUtility


Public Class WeaCompUtil
    Private WDMdataSource As atcTimeseriesSource

    Sub New(ByVal aDataSource As atcTimeseriesSource)
        WDMdataSource = aDataSource
    End Sub
End Class
