Imports MapWinUtility
Imports WorldWind.Net

''' <summary>Convenience functions supporting data download</summary>
''' <remarks></remarks>
Public Module Download

    Private pBackupURL As String = "http://hspf.com/cgi-bin/finddata.pl?url="
    Private pLastStatusSent As Double = 0

    ''' <summary>Download a URL, return results as a string</summary>
    ''' <param name="aURL">URL to download</param>
    ''' <returns>content of response from server</returns>
    Public Function DownloadURL(ByVal aURL As String) As String
        Dim lStream As IO.StreamReader = GetHTTPStreamReader(aURL, 60)
        Return lStream.ReadToEnd
    End Function

    ''' <summary></summary>
    ''' <param name="aURL"></param>
    ''' <param name="aSecondsToRespond"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHTTPStreamReader(ByVal aURL As String, Optional ByVal aSecondsToRespond As Long = 30) As IO.StreamReader
        Dim myWebRequest As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        myWebRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials
        myWebRequest.Timeout = aSecondsToRespond * 1000
        Return New IO.StreamReader(myWebRequest.GetResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"))
    End Function

    ''' <summary>Download from aURL to file aSaveAs</summary>
    ''' <param name="aURL">Uniform Resource Locator</param>
    ''' <param name="aSaveAs">Full path of file to save in</param>
    ''' <returns>True if download was successful, False if it was not.</returns>
    ''' <remarks>Creates directory if SaveAs includes a directory and it does not exist</remarks>
    Public Function DownloadURL(ByVal aURL As String, ByVal aSaveAs As String) As Boolean
        Try
            DownloadURLProgress(aURL, aSaveAs, AddressOf DefaultProgressHandler, AddressOf DefaultCompleteHandler)
            Layer.AddProcessStepToFile("Downloaded from " & aURL, aSaveAs)
            Logger.Dbg("Downloaded from primary server")
        Catch we As System.Net.WebException
            'TODO: catch proxy exception and prompt for proxy authentication
            Logger.Dbg("Caught WebException '" & we.Message & "' trying backup server")
            If aURL.StartsWith(pBackupURL) Then 'Already tried backup server
                Logger.Dbg("Unable to download from backup server")
                Return False
            End If
            Try
                DownloadURLProgress(pBackupURL & aURL, aSaveAs, Nothing, Nothing)
                If FileLen(aSaveAs) < 1000 AndAlso IO.File.ReadAllText(aSaveAs).Contains("404 Not Found") Then
                    Logger.Dbg("Not found using backup server")
                    TryDelete(aSaveAs)
                    Return False
                End If
                Layer.AddProcessStepToFile("Downloaded from " & pBackupURL & aURL, aSaveAs)
                Logger.Dbg("Downloaded using backup server")
            Catch be As Exception
                Logger.Dbg("Caught Backup Exception '" & be.ToString() & "'")
                Return False
            End Try
        Catch e As Exception
            Logger.Dbg("Caught Exception '" & e.ToString() & "'")
            Return False
        End Try
        Return True
    End Function

    Private Sub DefaultProgressHandler(ByVal aBytesRead As Integer, ByVal aTotalBytes As Integer)
        If aTotalBytes > 0 Then
            Logger.Progress(aBytesRead, aTotalBytes)
        Else
            If Now.ToOADate > pLastStatusSent + atcUtility.JulianSecond Then
                Logger.Status(Format(aBytesRead, "#,###") & " bytes downloaded")
                pLastStatusSent = Now.ToOADate
            End If
        End If
    End Sub

    Private Sub DefaultCompleteHandler(ByVal aDownloadInfo As WebDownload)
        Logger.Progress("", 0, 0)
        Logger.Dbg("Downloaded " & Format(aDownloadInfo.BytesProcessed, "#,##0") & " bytes from " & aDownloadInfo.Url)
    End Sub

    Private Sub DownloadURLProgress(ByVal URL As String, ByVal SaveAs As String, _
                                    ByVal progressHandler As DownloadProgressHandler, _
                                    ByVal completeHandler As DownloadCompleteHandler)
        Dim lURLserver As String = URL.Substring(URL.IndexOf("/") + 2)
        lURLserver = lURLserver.Substring(0, lURLserver.IndexOf("/"))

        Logger.Status("Downloading from " & lURLserver)
        Logger.Dbg("Download URL " + URL + " Save As " + SaveAs)

        Dim webDownload As New WebDownload
        webDownload.ProgressCallback = progressHandler
        webDownload.CompleteCallback = completeHandler
        webDownload.Url = URL
        Try
            webDownload.DownloadFile(SaveAs) 'Creates directory if SaveAs includes a directory and it does not exist
            'wait for download to complete before returning
            While (webDownload.IsDownloadInProgress)
                System.Threading.Thread.Sleep(50)
            End While
            If Not webDownload.IsComplete Then
                Logger.Dbg("Waiting another half second for completion")
                System.Threading.Thread.Sleep(500)
            End If
            If webDownload.Url <> URL Then Logger.Dbg("Redirected to " + webDownload.Url)
            webDownload.Dispose()
            If Not IO.File.Exists(SaveAs) Then
                Logger.Status("File not downloaded")
                Throw (New System.Net.WebException("File not downloaded"))
            End If
        Catch e As Exception
            Logger.Status("Error downloading: " & e.Message, True)
            Throw e
        End Try
        Logger.Status("")
    End Sub

    Private NullCertificateValidator As New System.Net.Security.RemoteCertificateValidationCallback(AddressOf ValidateRemoteCertificate)

    Public Sub DisableHttpsCertificateCheck()
        ' validate cert by calling a function
        System.Net.ServicePointManager.ServerCertificateValidationCallback = NullCertificateValidator
    End Sub

    ' callback used to validate the certificate in an SSL conversation
    Private Function ValidateRemoteCertificate(ByVal sender As Object, ByVal cert As Object, ByVal chain As Object, ByVal policyErrors As Object) As Boolean
        Return True
    End Function

    Public Sub SetSecurityProtocol()
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
    End Sub
End Module